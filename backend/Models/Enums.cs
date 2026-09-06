namespace BiteNest.Api.Models;

public enum UserRole
{
    Customer,
    RestaurantOwner,
    DeliveryRider,
    Administrator
}

public enum OrderStatus
{
    Placed,
    Accepted,
    Preparing,
    ReadyForPickup,
    OutForDelivery,
    Delivered,
    Cancelled
}

public enum QueueStatusLevel
{
    Free,
    Normal,
    Busy
}

public enum RiderStatus
{
    Available,
    OnDelivery,
    Offline
}

public enum VehicleType
{
    Bicycle,
    Motorcycle,
    Scooter
}

public enum PaymentMethod
{
    CashOnDelivery,
    bKash,
    Nagad,
    Card,
    MockDigitalPayment
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public enum TargetType
{
    Calorie,
    Budget,
    Balanced
}

public enum NotificationType
{
    OrderPlaced,
    OrderAccepted,
    OrderPreparing,
    OrderReady,
    OrderOutForDelivery,
    OrderDelivered,
    LeftoverAlert,
    System
}
