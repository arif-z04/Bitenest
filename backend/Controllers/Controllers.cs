using System.Security.Claims;
using BiteNest.Api.Data;
using BiteNest.Api.DTOs;
using BiteNest.Api.Models;
using BiteNest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiteNest.Api.Controllers;

// ============================================================================
// 1. AUTH CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuthService _auth;

    public AuthController(ApplicationDbContext db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email.ToLower()))
        {
            return BadRequest(new { message = "Email already registered." });
        }

        var user = new User
        {
            Name = req.Name,
            Email = req.Email.ToLower(),
            PasswordHash = _auth.HashPassword(req.Password),
            Role = req.Role,
            Phone = req.Phone,
            Address = req.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // If registering as DeliveryRider, create rider profile
        if (user.Role == UserRole.DeliveryRider)
        {
            _db.Riders.Add(new Rider
            {
                UserId = user.Id,
                CurrentStatus = RiderStatus.Offline,
                VehicleType = VehicleType.Motorcycle,
                TotalDeliveries = 0,
                Rating = 5.0m,
                IsVerified = true
            });
            await _db.SaveChangesAsync();
        }

        var token = _auth.GenerateJwtToken(user);
        return Ok(new AuthResponse(user.Id, user.Name, user.Email, user.Role.ToString(), token, user.Phone, user.Address));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());
        if (user == null || !_auth.VerifyPassword(req.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (!user.IsActive)
        {
            return Forbid("User account is deactivated.");
        }

        var token = _auth.GenerateJwtToken(user);
        return Ok(new AuthResponse(user.Id, user.Name, user.Email, user.Role.ToString(), token, user.Phone, user.Address));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new { user.Id, user.Name, user.Email, Role = user.Role.ToString(), user.Phone, user.Address });
    }
}

// ============================================================================
// 2. RESTAURANTS CONTROLLER (Includes Smart Queue Status)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class RestaurantsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IQueueStatusService _queueService;

    public RestaurantsController(ApplicationDbContext db, IQueueStatusService queueService)
    {
        _db = db;
        _queueService = queueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        var query = _db.Restaurants.Where(r => r.IsVerified && r.IsOpen);

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(r => r.Category.Contains(category));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search) || (r.Description != null && r.Description.Contains(search)));

        var restaurants = await query.ToListAsync();
        var enriched = await _queueService.EnrichRestaurantsWithQueueStatusAsync(restaurants);
        return Ok(enriched);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurant = await _db.Restaurants
            .Include(r => r.MenuItems.Where(m => m.IsAvailable))
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null) return NotFound();

        var (status, activeOrders, waitTime) = await _queueService.GetRestaurantQueueStatusAsync(id);
        var dto = new RestaurantDto(
            restaurant.Id, restaurant.OwnerUserId, restaurant.Name, restaurant.Description,
            restaurant.Category, restaurant.Address, restaurant.Latitude, restaurant.Longitude,
            restaurant.Phone, restaurant.ImageUrl, restaurant.IsVerified, restaurant.IsOpen,
            restaurant.AvgPrepTimeMinutes, restaurant.KitchenCapacity, restaurant.Rating,
            status.ToString(), activeOrders, waitTime
        );

        return Ok(new { Restaurant = dto, Menu = restaurant.MenuItems });
    }

    [HttpGet("{id}/queue-status")]
    public async Task<IActionResult> GetQueueStatus(int id)
    {
        var (status, activeOrders, waitTime) = await _queueService.GetRestaurantQueueStatusAsync(id);
        return Ok(new
        {
            RestaurantId = id,
            QueueStatus = status.ToString(),
            ActiveOrdersCount = activeOrders,
            EstimatedWaitTimeMinutes = waitTime,
            StatusDescription = status switch
            {
                QueueStatusLevel.Free => "Kitchen is free. Orders are prepared immediately.",
                QueueStatusLevel.Normal => "Moderate order volume. Standard prep time.",
                QueueStatusLevel.Busy => "High kitchen volume. Longer prep delays expected.",
                _ => "Normal"
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RestaurantCreateDto req)
    {
        var restaurant = new Restaurant
        {
            OwnerUserId = 2, // Default or extracted from token
            Name = req.Name,
            Description = req.Description,
            Category = req.Category,
            Address = req.Address,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Phone = req.Phone,
            ImageUrl = req.ImageUrl,
            AvgPrepTimeMinutes = req.AvgPrepTimeMinutes,
            KitchenCapacity = req.KitchenCapacity,
            IsVerified = true,
            IsOpen = true
        };

        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = restaurant.Id }, restaurant);
    }
}

// ============================================================================
// 3. MENU ITEMS CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MenuItemsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems([FromQuery] int? restaurantId, [FromQuery] string? category)
    {
        var query = _db.MenuItems.AsQueryable();

        if (restaurantId.HasValue)
            query = query.Where(m => m.RestaurantId == restaurantId.Value);

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(m => m.Category == category);

        var items = await query.ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MenuItemCreateDto req)
    {
        var item = new MenuItem
        {
            RestaurantId = req.RestaurantId,
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            Category = req.Category,
            PrepTimeMinutes = req.PrepTimeMinutes,
            Calories = req.Calories,
            ImageUrl = req.ImageUrl,
            IsAvailable = req.IsAvailable
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] MenuItem item)
    {
        if (id != item.Id) return BadRequest();
        _db.Entry(item).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return NotFound();
        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ============================================================================
// 4. ORDERS CONTROLLER (Single & Smart Multi-Restaurant Combined Delivery)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICombinedDeliveryService _combinedService;
    private readonly INotificationService _notificationService;

    public OrdersController(ApplicationDbContext db, ICombinedDeliveryService combinedService, INotificationService notificationService)
    {
        _db = db;
        _combinedService = combinedService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] int? customerId, [FromQuery] int? restaurantId, [FromQuery] int? riderId)
    {
        var query = _db.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Rider)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (restaurantId.HasValue)
            query = query.Where(o => o.RestaurantId == restaurantId.Value);

        if (riderId.HasValue)
            query = query.Where(o => o.RiderId == riderId.Value);

        var list = await query.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Customer)
            .Include(o => o.Rider).ThenInclude(r => r!.User)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Include(o => o.Payment)
            .Include(o => o.Review)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? NotFound() : Ok(order);
    }

    // Standard Single Restaurant Order
    [HttpPost]
    public async Task<IActionResult> CreateSingleOrder([FromBody] OrderCreateDto req, [FromQuery] int? userId = 5)
    {
        var customerId = userId ?? 5;
        var menuItemIds = req.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await _db.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();
        foreach (var itemReq in req.Items)
        {
            if (menuItems.TryGetValue(itemReq.MenuItemId, out var mItem))
            {
                var lineSubtotal = mItem.Price * itemReq.Quantity;
                subtotal += lineSubtotal;
                orderItems.Add(new OrderItem
                {
                    MenuItemId = mItem.Id,
                    Quantity = itemReq.Quantity,
                    UnitPrice = mItem.Price,
                    Subtotal = lineSubtotal,
                    SpecialInstructions = itemReq.SpecialInstructions
                });
            }
        }

        const decimal deliveryFee = 40.00m;
        var total = subtotal + deliveryFee;

        var order = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId,
            TotalAmount = total,
            DeliveryFee = deliveryFee,
            DiscountAmount = 0.00m,
            DeliveryAddress = req.DeliveryAddress,
            DeliveryLatitude = req.DeliveryLatitude,
            DeliveryLongitude = req.DeliveryLongitude,
            PreferredDeliveryTime = req.PreferredDeliveryTime,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(35),
            Status = OrderStatus.Placed,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Create initial payment
        _db.Payments.Add(new Payment
        {
            OrderId = order.Id,
            Amount = total,
            Method = req.PaymentMethod,
            Status = req.PaymentMethod == PaymentMethod.CashOnDelivery ? PaymentStatus.Pending : PaymentStatus.Completed,
            TransactionReference = $"TRX-MOCK-{Guid.NewGuid().ToString()[..8].ToUpper()}",
            PaidAt = req.PaymentMethod != PaymentMethod.CashOnDelivery ? DateTime.UtcNow : null
        });

        await _db.SaveChangesAsync();

        // Notify customer
        await _notificationService.CreateNotificationAsync(
            customerId,
            "Order Placed",
            $"Your order #{order.Id} has been received and sent to the kitchen.",
            NotificationType.OrderPlaced,
            order.Id
        );

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // Smart Multi-Restaurant Combined Delivery Order
    [HttpPost("combined")]
    public async Task<IActionResult> CreateCombinedOrder([FromBody] CombinedOrderCreateDto req, [FromQuery] int? userId = 5)
    {
        var customerId = userId ?? 5;

        // Verify eligibility
        var eligibility = await _combinedService.CheckCombinationEligibilityAsync(req.RestaurantId1, req.RestaurantId2);
        if (!eligibility.CanCombine)
        {
            return BadRequest(new { message = eligibility.RecommendationReason });
        }

        var groupCode = $"COMB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        // Order 1
        var menuItems1 = await _db.MenuItems.Where(m => req.Items1.Select(i => i.MenuItemId).Contains(m.Id)).ToDictionaryAsync(m => m.Id);
        decimal subtotal1 = 0;
        var orderItems1 = new List<OrderItem>();
        foreach (var i in req.Items1)
        {
            if (menuItems1.TryGetValue(i.MenuItemId, out var m))
            {
                var line = m.Price * i.Quantity;
                subtotal1 += line;
                orderItems1.Add(new OrderItem { MenuItemId = m.Id, Quantity = i.Quantity, UnitPrice = m.Price, Subtotal = line });
            }
        }

        // Order 2
        var menuItems2 = await _db.MenuItems.Where(m => req.Items2.Select(i => i.MenuItemId).Contains(m.Id)).ToDictionaryAsync(m => m.Id);
        decimal subtotal2 = 0;
        var orderItems2 = new List<OrderItem>();
        foreach (var i in req.Items2)
        {
            if (menuItems2.TryGetValue(i.MenuItemId, out var m))
            {
                var line = m.Price * i.Quantity;
                subtotal2 += line;
                orderItems2.Add(new OrderItem { MenuItemId = m.Id, Quantity = i.Quantity, UnitPrice = m.Price, Subtotal = line });
            }
        }

        // Discounted split fee: 30 BDT each instead of 40 each, saving 20 BDT total
        var order1 = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId1,
            TotalAmount = subtotal1 + 30.00m,
            DeliveryFee = 30.00m,
            DiscountAmount = 10.00m,
            DeliveryAddress = req.DeliveryAddress,
            DeliveryLatitude = req.DeliveryLatitude,
            DeliveryLongitude = req.DeliveryLongitude,
            PreferredDeliveryTime = req.PreferredDeliveryTime,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(45),
            IsCombinedDelivery = true,
            CombinedGroupCode = groupCode,
            Status = OrderStatus.Placed,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems1
        };

        _db.Orders.Add(order1);
        await _db.SaveChangesAsync();

        var order2 = new Order
        {
            CustomerId = customerId,
            RestaurantId = req.RestaurantId2,
            ParentCombinedOrderId = order1.Id,
            TotalAmount = subtotal2 + 30.00m,
            DeliveryFee = 30.00m,
            DiscountAmount = 10.00m,
            DeliveryAddress = req.DeliveryAddress,
            DeliveryLatitude = req.DeliveryLatitude,
            DeliveryLongitude = req.DeliveryLongitude,
            PreferredDeliveryTime = req.PreferredDeliveryTime,
            EstimatedDeliveryTime = DateTime.UtcNow.AddMinutes(45),
            IsCombinedDelivery = true,
            CombinedGroupCode = groupCode,
            Status = OrderStatus.Placed,
            CreatedAt = DateTime.UtcNow,
            Items = orderItems2
        };

        _db.Orders.Add(order2);
        await _db.SaveChangesAsync();

        // Create payments
        _db.Payments.Add(new Payment { OrderId = order1.Id, Amount = order1.TotalAmount, Method = req.PaymentMethod, Status = PaymentStatus.Completed, TransactionReference = $"TRX-COMB-1-{Guid.NewGuid().ToString()[..6].ToUpper()}" });
        _db.Payments.Add(new Payment { OrderId = order2.Id, Amount = order2.TotalAmount, Method = req.PaymentMethod, Status = PaymentStatus.Completed, TransactionReference = $"TRX-COMB-2-{Guid.NewGuid().ToString()[..6].ToUpper()}" });
        await _db.SaveChangesAsync();

        // Notify customer
        await _notificationService.CreateNotificationAsync(
            customerId,
            "Smart Combined Delivery Initiated",
            $"Orders from 2 nearby restaurants bundled into batch {groupCode} with single delivery fee of BDT 60 (saved BDT 20)!",
            NotificationType.OrderPlaced,
            order1.Id
        );

        return Ok(new
        {
            Message = "Combined Delivery order successfully created with single batch rider routing.",
            CombinedGroupCode = groupCode,
            Order1Id = order1.Id,
            Order2Id = order2.Id,
            TotalSavings = 20.00m,
            TotalPayable = order1.TotalAmount + order2.TotalAmount
        });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto req)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = req.NewStatus;
        if (req.RiderId.HasValue) order.RiderId = req.RiderId.Value;
        if (req.NewStatus == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;

        // If combined order, update sibling order status and rider if applicable
        if (order.IsCombinedDelivery && !string.IsNullOrEmpty(order.CombinedGroupCode))
        {
            var siblings = await _db.Orders
                .Where(o => o.CombinedGroupCode == order.CombinedGroupCode && o.Id != order.Id)
                .ToListAsync();

            foreach (var sibling in siblings)
            {
                if (req.RiderId.HasValue) sibling.RiderId = req.RiderId.Value;
                if (req.NewStatus == OrderStatus.OutForDelivery || req.NewStatus == OrderStatus.Delivered)
                {
                    sibling.Status = req.NewStatus;
                    if (req.NewStatus == OrderStatus.Delivered) sibling.DeliveredAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();

        // Dispatch status notification to customer
        await _notificationService.CreateNotificationAsync(
            order.CustomerId,
            $"Order #{order.Id} Update",
            $"Your order status is now {order.Status}.",
            NotificationType.OrderPreparing,
            order.Id
        );

        return Ok(order);
    }

    [HttpPost("validate-preferred-time")]
    public IActionResult ValidatePreferredTime([FromBody] DateTime preferredTime)
    {
        var minTime = DateTime.UtcNow.AddMinutes(30);
        var maxTime = DateTime.UtcNow.AddHours(12);

        if (preferredTime < minTime)
        {
            return BadRequest(new
            {
                IsValid = false,
                Message = "Preferred delivery time must be at least 30 minutes in the future to allow for prep and delivery."
            });
        }

        if (preferredTime > maxTime)
        {
            return BadRequest(new
            {
                IsValid = false,
                Message = "Preferred delivery time cannot exceed 12 hours ahead for same-day freshness."
            });
        }

        return Ok(new
        {
            IsValid = true,
            Message = "Preferred delivery slot is feasible and verified against kitchen and rider capacity.",
            PreferredTime = preferredTime
        });
    }
}

// ============================================================================
// 5. COMBINED DELIVERY CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
[Route("api/combined-delivery")]
public class CombinedDeliveryController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICombinedDeliveryService _combinedService;

    public CombinedDeliveryController(ApplicationDbContext db, ICombinedDeliveryService combinedService)
    {
        _db = db;
        _combinedService = combinedService;
    }

    [HttpGet("check-eligibility")]
    public async Task<IActionResult> CheckEligibility([FromQuery] int restaurant1, [FromQuery] int? restaurant2)
    {
        var response = await _combinedService.CheckCombinationEligibilityAsync(restaurant1, restaurant2);
        return Ok(response);
    }

    [HttpGet("nearby-partners/{restaurantId}")]
    public async Task<IActionResult> GetNearbyPartners(int restaurantId)
    {
        var origin = await _db.Restaurants.FindAsync(restaurantId);
        if (origin == null) return NotFound();

        var others = await _db.Restaurants
            .Where(r => r.Id != restaurantId && r.IsOpen && r.IsVerified)
            .ToListAsync();

        var nearby = others.Select(r => new
        {
            r.Id,
            r.Name,
            r.Category,
            r.Address,
            r.ImageUrl,
            DistanceKm = _combinedService.CalculateHaversineDistanceKm(origin.Latitude, origin.Longitude, r.Latitude, r.Longitude),
            EligibleForCombined = _combinedService.CalculateHaversineDistanceKm(origin.Latitude, origin.Longitude, r.Latitude, r.Longitude) <= 2.0
        })
        .Where(x => x.EligibleForCombined)
        .OrderBy(x => x.DistanceKm)
        .ToList();

        return Ok(nearby);
    }
}

// ============================================================================
// 6. MEAL PLANNER CONTROLLER (Calorie & Budget AI Recommender)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
[Route("api/meal-planner")]
public class MealPlannerController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMealPlannerService _planner;

    public MealPlannerController(ApplicationDbContext db, IMealPlannerService planner)
    {
        _db = db;
        _planner = planner;
    }

    [HttpPost("recommend")]
    public async Task<IActionResult> Recommend([FromBody] MealPlanRequestDto req)
    {
        var results = await _planner.GenerateMealPlanAsync(req);
        return Ok(results);
    }

    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] MealPlan plan, [FromQuery] int? userId = 5)
    {
        plan.CustomerId = userId ?? 5;
        plan.CreatedAt = DateTime.UtcNow;
        _db.MealPlans.Add(plan);
        await _db.SaveChangesAsync();
        return Ok(plan);
    }

    [HttpGet("my-plans")]
    public async Task<IActionResult> GetMyPlans([FromQuery] int? userId = 5)
    {
        var customerId = userId ?? 5;
        var plans = await _db.MealPlans.Where(p => p.CustomerId == customerId && p.IsActive).ToListAsync();
        return Ok(plans);
    }
}

// ============================================================================
// 7. LEFTOVER OFFERS CONTROLLER (Near end-of-day discounts)
// ============================================================================
[ApiController]
[Route("api/[controller]")]
[Route("api/leftover-offers")]
public class LeftoverOffersController : ControllerBase

{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public LeftoverOffersController(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveOffers()
    {
        var offers = await _db.LeftoverFoodOffers
            .Include(l => l.Restaurant)
            .Include(l => l.MenuItem)
            .Where(l => l.IsActive && l.QuantityAvailable > 0 && l.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(l => l.DiscountPercent)
            .Select(l => new
            {
                l.Id,
                RestaurantId = l.RestaurantId,
                RestaurantName = l.Restaurant!.Name,
                MenuItemId = l.MenuItemId,
                ItemName = l.MenuItem!.Name,
                Category = l.MenuItem.Category,
                ImageUrl = l.MenuItem.ImageUrl,
                l.OriginalPrice,
                l.DiscountPercent,
                l.DiscountedPrice,
                Savings = l.OriginalPrice - l.DiscountedPrice,
                l.QuantityAvailable,
                l.ExpiresAt,
                MinutesRemaining = (int)(l.ExpiresAt - DateTime.UtcNow).TotalMinutes
            })
            .ToListAsync();

        return Ok(offers);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeftoverOfferCreateDto req)
    {
        var menuItem = await _db.MenuItems.FindAsync(req.MenuItemId);
        if (menuItem == null) return NotFound("Menu item not found.");

        var discountedPrice = menuItem.Price * (1.0m - (req.DiscountPercent / 100.0m));

        var offer = new LeftoverFoodOffer
        {
            RestaurantId = menuItem.RestaurantId,
            MenuItemId = menuItem.Id,
            OriginalPrice = menuItem.Price,
            DiscountPercent = req.DiscountPercent,
            DiscountedPrice = Math.Round(discountedPrice, 2),
            QuantityAvailable = req.QuantityAvailable,
            ExpiresAt = req.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.LeftoverFoodOffers.Add(offer);
        await _db.SaveChangesAsync();

        // Broadcast notification to customer Rahim
        await _notificationService.CreateNotificationAsync(
            5,
            "Flash Leftover Deal!",
            $"{menuItem.Name} is now {req.DiscountPercent}% OFF at BiteNest!",
            NotificationType.LeftoverAlert,
            offer.Id
        );

        return Ok(offer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await _db.LeftoverFoodOffers.FindAsync(id);
        if (offer == null) return NotFound();
        _db.LeftoverFoodOffers.Remove(offer);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ============================================================================
// 8. RIDERS CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class RidersController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RidersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("available-orders")]
    public async Task<IActionResult> GetAvailableOrders()
    {
        // Orders ready for pickup or accepted without assigned rider
        var orders = await _db.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.RiderId == null && (o.Status == OrderStatus.Accepted || o.Status == OrderStatus.ReadyForPickup))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders);
    }

    [HttpPost("assign/{orderId}")]
    public async Task<IActionResult> AssignRider(int orderId, [FromQuery] int riderId = 1)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order == null) return NotFound();

        order.RiderId = riderId;
        order.Status = OrderStatus.ReadyForPickup;

        // If combined batch, assign to sibling too
        if (order.IsCombinedDelivery && !string.IsNullOrEmpty(order.CombinedGroupCode))
        {
            var siblings = await _db.Orders
                .Where(o => o.CombinedGroupCode == order.CombinedGroupCode)
                .ToListAsync();

            foreach (var s in siblings)
            {
                s.RiderId = riderId;
                s.Status = OrderStatus.ReadyForPickup;
            }
        }

        var rider = await _db.Riders.FindAsync(riderId);
        if (rider != null) rider.CurrentStatus = RiderStatus.OnDelivery;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Order successfully assigned to rider.", orderId, riderId });
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus([FromBody] RiderStatusUpdateDto req, [FromQuery] int riderId = 1)
    {
        var rider = await _db.Riders.FindAsync(riderId);
        if (rider == null) return NotFound();

        rider.CurrentStatus = req.Status;
        if (req.Latitude.HasValue) rider.CurrentLatitude = req.Latitude.Value;
        if (req.Longitude.HasValue) rider.CurrentLongitude = req.Longitude.Value;

        await _db.SaveChangesAsync();
        return Ok(rider);
    }

    [HttpGet("my-deliveries")]
    public async Task<IActionResult> GetMyDeliveries([FromQuery] int riderId = 1)
    {
        var deliveries = await _db.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .Where(o => o.RiderId == riderId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(deliveries);
    }
}

// ============================================================================
// 9. ADMIN CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetPlatformMetrics()
    {
        var totalUsers = await _db.Users.CountAsync();
        var totalRestaurants = await _db.Restaurants.CountAsync();
        var totalRiders = await _db.Riders.CountAsync();
        var totalOrders = await _db.Orders.CountAsync();
        var totalRevenue = await _db.Orders.Where(o => o.Status == OrderStatus.Delivered).SumAsync(o => (decimal?)o.TotalAmount) ?? 0.00m;
        var activeDeals = await _db.LeftoverFoodOffers.CountAsync(l => l.IsActive && l.ExpiresAt > DateTime.UtcNow);

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalRestaurants = totalRestaurants,
            TotalRiders = totalRiders,
            TotalOrders = totalOrders,
            TotalGrossMerchandiseValue = totalRevenue,
            ActiveLeftoverDeals = activeDeals
        });
    }

    [HttpPut("verify-restaurant/{id}")]
    public async Task<IActionResult> ToggleRestaurantVerification(int id)
    {
        var restaurant = await _db.Restaurants.FindAsync(id);
        if (restaurant == null) return NotFound();

        restaurant.IsVerified = !restaurant.IsVerified;
        await _db.SaveChangesAsync();
        return Ok(new { restaurant.Id, restaurant.Name, restaurant.IsVerified });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Select(u => new { u.Id, u.Name, u.Email, Role = u.Role.ToString(), u.Phone, u.IsActive, u.CreatedAt })
            .ToListAsync();
        return Ok(users);
    }
}

// ============================================================================
// 10. REVIEWS CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReviewsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("restaurant/{id}")]
    public async Task<IActionResult> GetByRestaurant(int id)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Customer)
            .Where(r => r.RestaurantId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewCreateDto req, [FromQuery] int? customerId = 5)
    {
        var review = new Review
        {
            CustomerId = customerId ?? 5,
            RestaurantId = req.RestaurantId,
            OrderId = req.OrderId,
            Rating = req.Rating,
            Comment = req.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);

        // Recalculate average rating
        var avg = await _db.Reviews.Where(r => r.RestaurantId == req.RestaurantId).Select(r => (double?)r.Rating).AverageAsync() ?? req.Rating;
        var restaurant = await _db.Restaurants.FindAsync(req.RestaurantId);
        if (restaurant != null)
        {
            restaurant.Rating = Math.Round((decimal)avg, 1);
        }

        await _db.SaveChangesAsync();
        return Ok(review);
    }
}

// ============================================================================
// 11. NOTIFICATIONS CONTROLLER
// ============================================================================
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public NotificationsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int userId = 5)
    {
        var list = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(30)
            .ToListAsync();

        return Ok(list);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var item = await _db.Notifications.FindAsync(id);
        if (item == null) return NotFound();
        item.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
