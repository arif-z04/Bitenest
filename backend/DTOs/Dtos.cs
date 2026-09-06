using System.ComponentModel.DataAnnotations;
using BiteNest.Api.Models;

namespace BiteNest.Api.DTOs;

// --- Auth DTOs ---
public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record RegisterRequest(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] UserRole Role,
    [Required] string Phone,
    string? Address
);

public record AuthResponse(
    int Id,
    string Name,
    string Email,
    string Role,
    string Token,
    string Phone,
    string? Address
);

// --- Restaurant DTOs ---
public record RestaurantCreateDto(
    [Required] string Name,
    string? Description,
    [Required] string Category,
    [Required] string Address,
    [Required] decimal Latitude,
    [Required] decimal Longitude,
    [Required] string Phone,
    string? ImageUrl,
    int AvgPrepTimeMinutes = 20,
    int KitchenCapacity = 15
);

public record RestaurantDto(
    int Id,
    int OwnerUserId,
    string Name,
    string? Description,
    string Category,
    string Address,
    decimal Latitude,
    decimal Longitude,
    string Phone,
    string? ImageUrl,
    bool IsVerified,
    bool IsOpen,
    int AvgPrepTimeMinutes,
    int KitchenCapacity,
    decimal Rating,
    string QueueStatus,
    int ActiveOrdersCount,
    int EstimatedWaitTimeMinutes
);

// --- Menu Item DTOs ---
public record MenuItemCreateDto(
    [Required] int RestaurantId,
    [Required] string Name,
    string? Description,
    [Required] decimal Price,
    [Required] string Category,
    int PrepTimeMinutes = 15,
    int? Calories = null,
    string? ImageUrl = null,
    bool IsAvailable = true
);

// --- Order DTOs ---
public record OrderItemCreateDto(
    [Required] int MenuItemId,
    [Required, Range(1, 50)] int Quantity,
    string? SpecialInstructions
);

public record OrderCreateDto(
    [Required] int RestaurantId,
    [Required] List<OrderItemCreateDto> Items,
    [Required] string DeliveryAddress,
    decimal? DeliveryLatitude,
    decimal? DeliveryLongitude,
    DateTime? PreferredDeliveryTime,
    PaymentMethod PaymentMethod = PaymentMethod.CashOnDelivery
);

public record CombinedOrderCreateDto(
    [Required] int RestaurantId1,
    [Required] List<OrderItemCreateDto> Items1,
    [Required] int RestaurantId2,
    [Required] List<OrderItemCreateDto> Items2,
    [Required] string DeliveryAddress,
    decimal? DeliveryLatitude,
    decimal? DeliveryLongitude,
    DateTime? PreferredDeliveryTime,
    PaymentMethod PaymentMethod = PaymentMethod.CashOnDelivery
);

public record OrderStatusUpdateDto(
    [Required] OrderStatus NewStatus,
    int? RiderId
);

// --- Meal Planner DTOs ---
public record MealPlanRequestDto(
    TargetType TargetType,
    decimal TargetValue,
    int? PreferredRestaurantId
);

public record MealPlanOptionDto(
    int MenuItemId,
    string ItemName,
    string RestaurantName,
    int RestaurantId,
    decimal Price,
    int Calories,
    string Category,
    string? ImageUrl
);

public record MealPlanResultDto(
    string PlanSummary,
    decimal TotalPrice,
    int TotalCalories,
    List<MealPlanOptionDto> Items
);

// --- Leftover Offer DTOs ---
public record LeftoverOfferCreateDto(
    [Required] int MenuItemId,
    [Required, Range(5, 90)] int DiscountPercent,
    [Required, Range(1, 100)] int QuantityAvailable,
    [Required] DateTime ExpiresAt
);

// --- Rider DTOs ---
public record RiderStatusUpdateDto(
    [Required] RiderStatus Status,
    decimal? Latitude,
    decimal? Longitude
);

// --- Review DTOs ---
public record ReviewCreateDto(
    [Required] int RestaurantId,
    [Required] int OrderId,
    [Required, Range(1, 5)] int Rating,
    string? Comment
);

// --- Delivery Recommendation DTOs ---
public record DeliveryRecommendationRequest(
    decimal CustomerLat,
    decimal CustomerLng,
    int RestaurantId1,
    int? RestaurantId2
);

public record DeliveryRecommendationResponse(
    bool CanCombine,
    double DistanceBetweenRestaurantsKm,
    string RecommendationReason,
    decimal CombinedDeliveryFee,
    decimal NormalDeliveryFee,
    decimal CustomerSavings
);
