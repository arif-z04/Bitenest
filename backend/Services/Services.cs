using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BiteNest.Api.Data;
using BiteNest.Api.DTOs;
using BiteNest.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BiteNest.Api.Services;

// ============================================================================
// 1. AUTH SERVICE
// ============================================================================
public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateJwtToken(User user);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);

    public string GenerateJwtToken(User user)
    {
        var secret = _config["Jwt:SecretKey"] ?? "BiteNest_Super_Secret_Production_Key_2026_Secure_JWT_Key_Min256Bits";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "BiteNestApi",
            audience: _config["Jwt:Audience"] ?? "BiteNestUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// ============================================================================
// 2. QUEUE STATUS SERVICE (Dynamic Free / Normal / Busy calculation)
// ============================================================================
public interface IQueueStatusService
{
    Task<(QueueStatusLevel Status, int ActiveOrders, int EstimatedWaitMinutes)> GetRestaurantQueueStatusAsync(int restaurantId);
    Task<List<RestaurantDto>> EnrichRestaurantsWithQueueStatusAsync(List<Restaurant> restaurants);
}

public class QueueStatusService : IQueueStatusService
{
    private readonly ApplicationDbContext _db;

    public QueueStatusService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(QueueStatusLevel Status, int ActiveOrders, int EstimatedWaitMinutes)> GetRestaurantQueueStatusAsync(int restaurantId)
    {
        var restaurant = await _db.Restaurants.FindAsync(restaurantId);
        if (restaurant == null)
            return (QueueStatusLevel.Free, 0, 15);

        // Active orders count (Placed, Accepted, Preparing)
        var activeOrders = await _db.Orders
            .Where(o => o.RestaurantId == restaurantId &&
                        (o.Status == OrderStatus.Placed ||
                         o.Status == OrderStatus.Accepted ||
                         o.Status == OrderStatus.Preparing))
            .CountAsync();

        var capacity = restaurant.KitchenCapacity <= 0 ? 15 : restaurant.KitchenCapacity;
        QueueStatusLevel status;

        if (activeOrders < (capacity / 3.0))
            status = QueueStatusLevel.Free;
        else if (activeOrders < capacity)
            status = QueueStatusLevel.Normal;
        else
            status = QueueStatusLevel.Busy;

        // Estimated prep time = base prep time + 3 minutes per queued order
        var estimatedWait = restaurant.AvgPrepTimeMinutes + (activeOrders * 3);

        return (status, activeOrders, estimatedWait);
    }

    public async Task<List<RestaurantDto>> EnrichRestaurantsWithQueueStatusAsync(List<Restaurant> restaurants)
    {
        var restaurantIds = restaurants.Select(r => r.Id).ToList();
        var activeOrderCounts = await _db.Orders
            .Where(o => restaurantIds.Contains(o.RestaurantId) &&
                        (o.Status == OrderStatus.Placed ||
                         o.Status == OrderStatus.Accepted ||
                         o.Status == OrderStatus.Preparing))
            .GroupBy(o => o.RestaurantId)
            .Select(g => new { RestaurantId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RestaurantId, x => x.Count);

        var result = new List<RestaurantDto>();
        foreach (var r in restaurants)
        {
            var count = activeOrderCounts.TryGetValue(r.Id, out var c) ? c : 0;
            var capacity = r.KitchenCapacity <= 0 ? 15 : r.KitchenCapacity;
            var queueLevel = count < (capacity / 3.0)
                ? QueueStatusLevel.Free
                : (count < capacity ? QueueStatusLevel.Normal : QueueStatusLevel.Busy);

            var estimatedWait = r.AvgPrepTimeMinutes + (count * 3);

            result.Add(new RestaurantDto(
                r.Id,
                r.OwnerUserId,
                r.Name,
                r.Description,
                r.Category,
                r.Address,
                r.Latitude,
                r.Longitude,
                r.Phone,
                r.ImageUrl,
                r.IsVerified,
                r.IsOpen,
                r.AvgPrepTimeMinutes,
                r.KitchenCapacity,
                r.Rating,
                queueLevel.ToString(),
                count,
                estimatedWait
            ));
        }

        return result;
    }
}

// ============================================================================
// 3. SMART COMBINED DELIVERY SERVICE (Haversine distance & combination rules)
// ============================================================================
public interface ICombinedDeliveryService
{
    double CalculateHaversineDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2);
    Task<DeliveryRecommendationResponse> CheckCombinationEligibilityAsync(int restaurantId1, int? restaurantId2);
}

public class CombinedDeliveryService : ICombinedDeliveryService
{
    private readonly ApplicationDbContext _db;
    private const double CombinedDeliveryDistanceThresholdKm = 2.0;
    private const decimal StandardSingleDeliveryFee = 40.00m;
    private const decimal CombinedDeliveryFeeTotal = 60.00m; // 30 per restaurant order

    public CombinedDeliveryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public double CalculateHaversineDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = (double)(lat2 - lat1) * (Math.PI / 180.0);
        var dLon = (double)(lon2 - lon1) * (Math.PI / 180.0);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos((double)lat1 * (Math.PI / 180.0)) *
                Math.Cos((double)lat2 * (Math.PI / 180.0)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(earthRadiusKm * c, 2);
    }

    public async Task<DeliveryRecommendationResponse> CheckCombinationEligibilityAsync(int restaurantId1, int? restaurantId2)
    {
        if (!restaurantId2.HasValue || restaurantId1 == restaurantId2.Value)
        {
            return new DeliveryRecommendationResponse(
                CanCombine: false,
                DistanceBetweenRestaurantsKm: 0,
                RecommendationReason: "Single restaurant delivery. Standard delivery fee applies.",
                CombinedDeliveryFee: StandardSingleDeliveryFee,
                NormalDeliveryFee: StandardSingleDeliveryFee,
                CustomerSavings: 0.00m
            );
        }

        var r1 = await _db.Restaurants.FindAsync(restaurantId1);
        var r2 = await _db.Restaurants.FindAsync(restaurantId2.Value);

        if (r1 == null || r2 == null || !r1.IsOpen || !r2.IsOpen)
        {
            return new DeliveryRecommendationResponse(
                CanCombine: false,
                DistanceBetweenRestaurantsKm: 0,
                RecommendationReason: "One or both selected restaurants are currently unavailable.",
                CombinedDeliveryFee: StandardSingleDeliveryFee * 2,
                NormalDeliveryFee: StandardSingleDeliveryFee * 2,
                CustomerSavings: 0.00m
            );
        }

        var distanceKm = CalculateHaversineDistanceKm(r1.Latitude, r1.Longitude, r2.Latitude, r2.Longitude);
        var separateFee = StandardSingleDeliveryFee * 2; // 80 BDT

        if (distanceKm <= CombinedDeliveryDistanceThresholdKm)
        {
            var savings = separateFee - CombinedDeliveryFeeTotal; // 80 - 60 = 20 BDT savings
            return new DeliveryRecommendationResponse(
                CanCombine: true,
                DistanceBetweenRestaurantsKm: distanceKm,
                RecommendationReason: $"Restaurants are within {distanceKm:F2} km (under 2.0 km limit). Smart Multi-Restaurant Combined Delivery approved with 1 assigned rider.",
                CombinedDeliveryFee: CombinedDeliveryFeeTotal,
                NormalDeliveryFee: separateFee,
                CustomerSavings: savings
            );
        }

        return new DeliveryRecommendationResponse(
            CanCombine: false,
            DistanceBetweenRestaurantsKm: distanceKm,
            RecommendationReason: $"Distance between restaurants is {distanceKm:F2} km, exceeding the 2.0 km combined delivery threshold. Separate deliveries recommended for optimal food freshness.",
            CombinedDeliveryFee: separateFee,
            NormalDeliveryFee: separateFee,
            CustomerSavings: 0.00m
        );
    }
}

// ============================================================================
// 4. ADVANCED MEAL PLANNER SERVICE (Calorie & Budget target matching)
// ============================================================================
public interface IMealPlannerService
{
    Task<List<MealPlanResultDto>> GenerateMealPlanAsync(MealPlanRequestDto request);
}

public class MealPlannerService : IMealPlannerService
{
    private readonly ApplicationDbContext _db;

    public MealPlannerService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<MealPlanResultDto>> GenerateMealPlanAsync(MealPlanRequestDto request)
    {
        var query = _db.MenuItems.Include(m => m.Restaurant)
            .Where(m => m.IsAvailable && m.Restaurant != null && m.Restaurant.IsOpen);

        if (request.PreferredRestaurantId.HasValue)
        {
            query = query.Where(m => m.RestaurantId == request.PreferredRestaurantId.Value);
        }

        var availableItems = await query.ToListAsync();
        var results = new List<MealPlanResultDto>();

        if (request.TargetType == TargetType.Calorie)
        {
            var targetCal = (int)request.TargetValue;
            var tolerance = (int)(targetCal * 0.15); // +/- 15% tolerance

            // Plan Option 1: Single balanced meal
            var singleMatch = availableItems
                .Where(m => m.Calories.HasValue && Math.Abs(m.Calories.Value - targetCal) <= tolerance)
                .OrderBy(m => Math.Abs(m.Calories!.Value - targetCal))
                .Take(2);

            foreach (var item in singleMatch)
            {
                results.Add(new MealPlanResultDto(
                    $"Precision Single Meal ({item.Calories} kcal)",
                    item.Price,
                    item.Calories ?? 0,
                    new List<MealPlanOptionDto>
                    {
                        new(item.Id, item.Name, item.Restaurant?.Name ?? "", item.RestaurantId, item.Price, item.Calories ?? 0, item.Category, item.ImageUrl)
                    }
                ));
            }

            // Plan Option 2: Meal + Beverage/Side combo
            var mains = availableItems.Where(m => m.Category is "Main Course" or "Burgers" or "Salads" or "Grain Bowls").ToList();
            var sides = availableItems.Where(m => m.Category is "Beverages" or "Sides" or "Bread" or "Dessert").ToList();

            foreach (var main in mains)
            {
                foreach (var side in sides)
                {
                    var totalCal = (main.Calories ?? 0) + (side.Calories ?? 0);
                    if (Math.Abs(totalCal - targetCal) <= tolerance)
                    {
                        results.Add(new MealPlanResultDto(
                            $"Nutrient Combo: {main.Name} + {side.Name} ({totalCal} kcal)",
                            main.Price + side.Price,
                            totalCal,
                            new List<MealPlanOptionDto>
                            {
                                new(main.Id, main.Name, main.Restaurant?.Name ?? "", main.RestaurantId, main.Price, main.Calories ?? 0, main.Category, main.ImageUrl),
                                new(side.Id, side.Name, side.Restaurant?.Name ?? "", side.RestaurantId, side.Price, side.Calories ?? 0, side.Category, side.ImageUrl)
                            }
                        ));

                        if (results.Count >= 4) break;
                    }
                }
                if (results.Count >= 4) break;
            }
        }
        else // Budget Target
        {
            var budget = request.TargetValue;
            var affordableMains = availableItems
                .Where(m => m.Price <= budget)
                .OrderByDescending(m => m.Price)
                .Take(2);

            foreach (var item in affordableMains)
            {
                results.Add(new MealPlanResultDto(
                    $"Budget Value Pick: {item.Name} (BDT {item.Price:F2})",
                    item.Price,
                    item.Calories ?? 0,
                    new List<MealPlanOptionDto>
                    {
                        new(item.Id, item.Name, item.Restaurant?.Name ?? "", item.RestaurantId, item.Price, item.Calories ?? 0, item.Category, item.ImageUrl)
                    }
                ));
            }

            var mains = availableItems.Where(m => m.Category is "Main Course" or "Burgers" or "Salads" or "Grain Bowls").ToList();
            var sides = availableItems.Where(m => m.Category is "Beverages" or "Sides" or "Bread" or "Dessert").ToList();

            foreach (var m in mains)
            {
                foreach (var s in sides)
                {
                    if (m.Price + s.Price <= budget)
                    {
                        results.Add(new MealPlanResultDto(
                            $"Budget Combo: {m.Name} + {s.Name} (BDT {m.Price + s.Price:F2})",
                            m.Price + s.Price,
                            (m.Calories ?? 0) + (s.Calories ?? 0),
                            new List<MealPlanOptionDto>
                            {
                                new(m.Id, m.Name, m.Restaurant?.Name ?? "", m.RestaurantId, m.Price, m.Calories ?? 0, m.Category, m.ImageUrl),
                                new(s.Id, s.Name, s.Restaurant?.Name ?? "", s.RestaurantId, s.Price, s.Calories ?? 0, s.Category, s.ImageUrl)
                            }
                        ));

                        if (results.Count >= 4) break;
                    }
                }
                if (results.Count >= 4) break;
            }
        }

        return results;
    }
}

// ============================================================================
// 5. NOTIFICATION SERVICE
// ============================================================================
public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? referenceId = null);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, int? referenceId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
}
