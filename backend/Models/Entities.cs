using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BiteNest.Api.Models;

[Table("users")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Address { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? Longitude { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public List<Restaurant> Restaurants { get; set; } = new();

    [JsonIgnore]
    public Rider? RiderProfile { get; set; }

    [JsonIgnore]
    public List<Order> CustomerOrders { get; set; } = new();

    [JsonIgnore]
    public List<Review> Reviews { get; set; } = new();

    [JsonIgnore]
    public List<MealPlan> MealPlans { get; set; } = new();

    [JsonIgnore]
    public List<Notification> Notifications { get; set; } = new();
}

[Table("restaurants")]
public class Restaurant
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OwnerUserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public decimal Latitude { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public decimal Longitude { get; set; }

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsVerified { get; set; } = false;

    public bool IsOpen { get; set; } = true;

    public int AvgPrepTimeMinutes { get; set; } = 20;

    public int KitchenCapacity { get; set; } = 15;

    [Column(TypeName = "decimal(2, 1)")]
    public decimal Rating { get; set; } = 4.5m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Dynamic queue status calculation helper
    [NotMapped]
    public QueueStatusLevel CurrentQueueStatus { get; set; } = QueueStatusLevel.Free;

    [NotMapped]
    public int ActiveOrdersCount { get; set; } = 0;

    // Navigation properties
    public User? Owner { get; set; }

    public List<MenuItem> MenuItems { get; set; } = new();

    [JsonIgnore]
    public List<Order> Orders { get; set; } = new();

    [JsonIgnore]
    public List<Review> Reviews { get; set; } = new();

    [JsonIgnore]
    public List<LeftoverFoodOffer> LeftoverOffers { get; set; } = new();
}

[Table("menu_items")]
public class MenuItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public int PrepTimeMinutes { get; set; } = 15;

    public int? Calories { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }

    [JsonIgnore]
    public List<OrderItem> OrderItems { get; set; } = new();

    [JsonIgnore]
    public List<LeftoverFoodOffer> LeftoverOffers { get; set; } = new();
}

[Table("riders")]
public class Rider
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public VehicleType VehicleType { get; set; } = VehicleType.Motorcycle;

    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    public RiderStatus CurrentStatus { get; set; } = RiderStatus.Offline;

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? CurrentLatitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? CurrentLongitude { get; set; }

    public int TotalDeliveries { get; set; } = 0;

    [Column(TypeName = "decimal(2, 1)")]
    public decimal Rating { get; set; } = 5.0m;

    public bool IsVerified { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }

    [JsonIgnore]
    public List<Order> Deliveries { get; set; } = new();
}

[Table("orders")]
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    public int? RiderId { get; set; }

    public int? ParentCombinedOrderId { get; set; }

    public bool IsCombinedDelivery { get; set; } = false;

    [MaxLength(50)]
    public string? CombinedGroupCode { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Placed;

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal DeliveryFee { get; set; } = 40.00m;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal DiscountAmount { get; set; } = 0.00m;

    [Required]
    [MaxLength(255)]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? DeliveryLatitude { get; set; }

    [Column(TypeName = "decimal(10, 7)")]
    public decimal? DeliveryLongitude { get; set; }

    public DateTime? PreferredDeliveryTime { get; set; }

    public DateTime? EstimatedDeliveryTime { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Customer { get; set; }

    public Restaurant? Restaurant { get; set; }

    public Rider? Rider { get; set; }

    [JsonIgnore]
    public Order? ParentCombinedOrder { get; set; }

    [JsonIgnore]
    public List<Order> ChildCombinedOrders { get; set; } = new();

    public List<OrderItem> Items { get; set; } = new();

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }
}

[Table("order_items")]
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Subtotal { get; set; }

    [MaxLength(255)]
    public string? SpecialInstructions { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Order? Order { get; set; }

    public MenuItem? MenuItem { get; set; }
}

[Table("payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.CashOnDelivery;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public Order? Order { get; set; }
}

[Table("reviews")]
public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Customer { get; set; }

    [JsonIgnore]
    public Restaurant? Restaurant { get; set; }

    [JsonIgnore]
    public Order? Order { get; set; }
}

[Table("meal_plans")]
public class MealPlan
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;

    public TargetType TargetType { get; set; } = TargetType.Calorie;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal TargetValue { get; set; }

    [Column(TypeName = "json")]
    public string SuggestedItemsJson { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public User? Customer { get; set; }
}

[Table("leftover_food_offers")]
public class LeftoverFoodOffer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RestaurantId { get; set; }

    [Required]
    public int MenuItemId { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal OriginalPrice { get; set; }

    [Range(1, 90)]
    public int DiscountPercent { get; set; } = 30;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal DiscountedPrice { get; set; }

    public int QuantityAvailable { get; set; } = 1;

    public DateTime ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Restaurant? Restaurant { get; set; }

    public MenuItem? MenuItem { get; set; }
}

[Table("notifications")]
public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.System;

    public int? ReferenceId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonIgnore]
    public User? User { get; set; }
}
