using BiteNest.Api.Models;

namespace BiteNest.Api.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return; // DB already seeded
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");

        // 1. Seed Users
        var admin = new User
        {
            Name = "System Administrator",
            Email = "admin@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.Administrator,
            Phone = "+8801711000001",
            Address = "BiteNest HQ, Road 11, Dhanmondi, Dhaka",
            Latitude = 23.7510m,
            Longitude = 90.3750m,
            IsActive = true
        };

        var owner1 = new User
        {
            Name = "SpiceCraft Manager",
            Email = "owner.spicecraft@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.RestaurantOwner,
            Phone = "+8801711000002",
            Address = "House 42, Road 7A, Dhanmondi, Dhaka",
            Latitude = 23.7508m,
            Longitude = 90.3752m,
            IsActive = true
        };

        var owner2 = new User
        {
            Name = "Burger Barn Owner",
            Email = "owner.burgerbarn@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.RestaurantOwner,
            Phone = "+8801711000003",
            Address = "Plot 15, Satmasjid Road, Dhanmondi, Dhaka",
            Latitude = 23.7540m,
            Longitude = 90.3780m,
            IsActive = true
        };

        var owner3 = new User
        {
            Name = "Green Bowls Director",
            Email = "owner.greenbowls@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.RestaurantOwner,
            Phone = "+8801711000004",
            Address = "Block C, Road 27, Dhanmondi, Dhaka",
            Latitude = 23.7580m,
            Longitude = 90.3810m,
            IsActive = true
        };

        var customer1 = new User
        {
            Name = "Rahim Ahmed",
            Email = "customer.rahim@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            Phone = "+8801711000005",
            Address = "Apartment 4B, Road 12, Dhanmondi, Dhaka",
            Latitude = 23.7525m,
            Longitude = 90.3765m,
            IsActive = true
        };

        var customer2 = new User
        {
            Name = "Fatima Jahan",
            Email = "customer.fatima@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            Phone = "+8801711000006",
            Address = "House 88, Road 8, Dhanmondi, Dhaka",
            Latitude = 23.7495m,
            Longitude = 90.3740m,
            IsActive = true
        };

        var riderUser1 = new User
        {
            Name = "Tanvir Hasan (Rider 1)",
            Email = "rider.tanvir@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.DeliveryRider,
            Phone = "+8801711000007",
            Address = "Shankar Stand, Dhanmondi, Dhaka",
            Latitude = 23.7515m,
            Longitude = 90.3760m,
            IsActive = true
        };

        var riderUser2 = new User
        {
            Name = "Sumon Barua (Rider 2)",
            Email = "rider.sumon@bitenest.com",
            PasswordHash = passwordHash,
            Role = UserRole.DeliveryRider,
            Phone = "+8801711000008",
            Address = "Zigatola Bus Stand, Dhanmondi, Dhaka",
            Latitude = 23.7480m,
            Longitude = 90.3720m,
            IsActive = true
        };

        context.Users.AddRange(admin, owner1, owner2, owner3, customer1, customer2, riderUser1, riderUser2);
        context.SaveChanges();

        // 2. Seed Restaurants
        var rest1 = new Restaurant
        {
            OwnerUserId = owner1.Id,
            Name = "SpiceCraft Kitchen",
            Description = "Authentic slow-cooked biryani, aromatic curries, and freshly baked tandoori naans.",
            Category = "Bengali & Indian",
            Address = "House 42, Road 7A, Dhanmondi, Dhaka",
            Latitude = 23.7508m,
            Longitude = 90.3752m,
            Phone = "+8801711000002",
            ImageUrl = "https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=600&auto=format&fit=crop&q=80",
            IsVerified = true,
            IsOpen = true,
            AvgPrepTimeMinutes = 25,
            KitchenCapacity = 12,
            Rating = 4.8m
        };

        var rest2 = new Restaurant
        {
            OwnerUserId = owner2.Id,
            Name = "Burger Barn & Grill",
            Description = "Gourmet smashed beef burgers, crispy buttermilk chicken tenders, and loaded cheese fries.",
            Category = "Fast Food & Burgers",
            Address = "Plot 15, Satmasjid Road, Dhanmondi, Dhaka",
            Latitude = 23.7540m,
            Longitude = 90.3780m,
            Phone = "+8801711000003",
            ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80",
            IsVerified = true,
            IsOpen = true,
            AvgPrepTimeMinutes = 15,
            KitchenCapacity = 18,
            Rating = 4.6m
        };

        var rest3 = new Restaurant
        {
            OwnerUserId = owner3.Id,
            Name = "Green Bowls & Juices",
            Description = "Nutrient-rich protein grain bowls, detox fresh cold-pressed juices, and organic wraps.",
            Category = "Healthy & Salads",
            Address = "Block C, Road 27, Dhanmondi, Dhaka",
            Latitude = 23.7580m,
            Longitude = 90.3810m,
            Phone = "+8801711000004",
            ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80",
            IsVerified = true,
            IsOpen = true,
            AvgPrepTimeMinutes = 12,
            KitchenCapacity = 10,
            Rating = 4.7m
        };

        context.Restaurants.AddRange(rest1, rest2, rest3);
        context.SaveChanges();

        // 3. Seed Menu Items
        var items = new List<MenuItem>
        {
            new() { RestaurantId = rest1.Id, Name = "Kacchi Biryani Special", Description = "Fragrant basmati rice layered with tender mutton chunks and spiced saffron potato.", Price = 380.00m, Category = "Main Course", PrepTimeMinutes = 25, Calories = 820, ImageUrl = "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest1.Id, Name = "Butter Chicken Masala", Description = "Charcoal-grilled chicken simmered in velvety tomato, cashew, and cream gravy.", Price = 320.00m, Category = "Main Course", PrepTimeMinutes = 20, Calories = 580, ImageUrl = "https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest1.Id, Name = "Garlic Butter Naan", Description = "Tandoor baked flatbread brushed with roasted garlic butter and cilantro.", Price = 60.00m, Category = "Bread", PrepTimeMinutes = 10, Calories = 210, ImageUrl = "https://images.unsplash.com/photo-1601050690597-df0568f70950?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest1.Id, Name = "Borhani Pitcher (500ml)", Description = "Traditional spiced yogurt digestive drink with mint, coriander, and black rock salt.", Price = 90.00m, Category = "Beverages", PrepTimeMinutes = 5, Calories = 140, ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80", IsAvailable = true },

            new() { RestaurantId = rest2.Id, Name = "Classic Smokehouse Beef Burger", Description = "150g grilled beef patty, melted cheddar, caramelized onions, and house smoky barbecue mayo.", Price = 290.00m, Category = "Burgers", PrepTimeMinutes = 15, Calories = 690, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest2.Id, Name = "Crispy Hot Buttermilk Chicken Burger", Description = "Double fried crunchy chicken thigh tossed in spicy cayenne butter with crisp dill pickles.", Price = 270.00m, Category = "Burgers", PrepTimeMinutes = 15, Calories = 640, ImageUrl = "https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest2.Id, Name = "Truffle Parmesan Loaded Fries", Description = "Hand-cut golden fries tossed with white truffle oil, grated parmesan, and chives.", Price = 160.00m, Category = "Sides", PrepTimeMinutes = 10, Calories = 420, ImageUrl = "https://images.unsplash.com/photo-1576107232684-1279f3908594?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest2.Id, Name = "Belgian Chocolate Milkshake", Description = "Thick blended artisanal dark chocolate ice cream topped with chocolate drizzle.", Price = 180.00m, Category = "Beverages", PrepTimeMinutes = 8, Calories = 380, ImageUrl = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&auto=format&fit=crop&q=80", IsAvailable = true },

            new() { RestaurantId = rest3.Id, Name = "Mediterranean Grilled Chicken Salad", Description = "Herb marinated chicken strips over mixed greens, kalamata olives, feta cheese, and lemon vinaigrette.", Price = 310.00m, Category = "Salads", PrepTimeMinutes = 12, Calories = 390, ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest3.Id, Name = "Quinoa Avocado Power Bowl", Description = "Organic red quinoa, fresh Hass avocado, edamame, roasted chickpeas, and tahini drizzle.", Price = 340.00m, Category = "Grain Bowls", PrepTimeMinutes = 12, Calories = 460, ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80", IsAvailable = true },
            new() { RestaurantId = rest3.Id, Name = "Cold Pressed Green Detox Juice", Description = "Pure blend of baby spinach, celery, green apple, cucumber, and ginger.", Price = 140.00m, Category = "Beverages", PrepTimeMinutes = 5, Calories = 95, ImageUrl = "https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80", IsAvailable = true }
        };

        context.MenuItems.AddRange(items);
        context.SaveChanges();

        // 4. Seed Riders
        var rider1 = new Rider
        {
            UserId = riderUser1.Id,
            VehicleType = VehicleType.Motorcycle,
            LicenseNumber = "DHAKA-METRO-HA-123456",
            CurrentStatus = RiderStatus.Available,
            CurrentLatitude = 23.7515m,
            CurrentLongitude = 90.3760m,
            TotalDeliveries = 142,
            Rating = 4.9m,
            IsVerified = true
        };

        var rider2 = new Rider
        {
            UserId = riderUser2.Id,
            VehicleType = VehicleType.Motorcycle,
            LicenseNumber = "DHAKA-METRO-LA-654321",
            CurrentStatus = RiderStatus.OnDelivery,
            CurrentLatitude = 23.7480m,
            CurrentLongitude = 90.3720m,
            TotalDeliveries = 89,
            Rating = 4.8m,
            IsVerified = true
        };

        context.Riders.AddRange(rider1, rider2);
        context.SaveChanges();

        // 5. Seed Leftover Deals
        var leftover1 = new LeftoverFoodOffer
        {
            RestaurantId = rest1.Id,
            MenuItemId = items[0].Id, // Kacchi Biryani
            OriginalPrice = 380.00m,
            DiscountPercent = 35,
            DiscountedPrice = 247.00m,
            QuantityAvailable = 4,
            ExpiresAt = DateTime.UtcNow.AddHours(4),
            IsActive = true
        };

        var leftover2 = new LeftoverFoodOffer
        {
            RestaurantId = rest2.Id,
            MenuItemId = items[5].Id, // Crispy Chicken Burger
            OriginalPrice = 270.00m,
            DiscountPercent = 30,
            DiscountedPrice = 189.00m,
            QuantityAvailable = 3,
            ExpiresAt = DateTime.UtcNow.AddHours(3),
            IsActive = true
        };

        context.LeftoverFoodOffers.AddRange(leftover1, leftover2);
        context.SaveChanges();

        // 6. Seed Orders
        var order1 = new Order
        {
            CustomerId = customer1.Id,
            RestaurantId = rest1.Id,
            RiderId = rider1.Id,
            Status = OrderStatus.Delivered,
            TotalAmount = 420.00m,
            DeliveryFee = 40.00m,
            DiscountAmount = 0.00m,
            DeliveryAddress = customer1.Address!,
            DeliveryLatitude = customer1.Latitude,
            DeliveryLongitude = customer1.Longitude,
            DeliveredAt = DateTime.UtcNow.AddMinutes(-90),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var order2 = new Order
        {
            CustomerId = customer2.Id,
            RestaurantId = rest1.Id,
            Status = OrderStatus.Preparing,
            TotalAmount = 470.00m,
            DeliveryFee = 40.00m,
            DiscountAmount = 0.00m,
            DeliveryAddress = customer2.Address!,
            DeliveryLatitude = customer2.Latitude,
            DeliveryLongitude = customer2.Longitude,
            PreferredDeliveryTime = DateTime.UtcNow.AddMinutes(45),
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        context.Orders.AddRange(order1, order2);
        context.SaveChanges();

        // Order Items
        context.OrderItems.AddRange(
            new OrderItem { OrderId = order1.Id, MenuItemId = items[0].Id, Quantity = 1, UnitPrice = 380.00m, Subtotal = 380.00m },
            new OrderItem { OrderId = order2.Id, MenuItemId = items[1].Id, Quantity = 1, UnitPrice = 320.00m, Subtotal = 320.00m },
            new OrderItem { OrderId = order2.Id, MenuItemId = items[3].Id, Quantity = 1, UnitPrice = 90.00m, Subtotal = 90.00m }
        );

        // Payments
        context.Payments.AddRange(
            new Payment { OrderId = order1.Id, Amount = 420.00m, Method = PaymentMethod.bKash, Status = PaymentStatus.Completed, TransactionReference = "TRX-BKASH-991823", PaidAt = DateTime.UtcNow.AddHours(-2) },
            new Payment { OrderId = order2.Id, Amount = 470.00m, Method = PaymentMethod.CashOnDelivery, Status = PaymentStatus.Pending }
        );

        // Reviews
        context.Reviews.Add(new Review
        {
            CustomerId = customer1.Id,
            RestaurantId = rest1.Id,
            OrderId = order1.Id,
            Rating = 5,
            Comment = "Exceptional Kacchi Biryani! Meat was tender and falling off the bone. Fast delivery."
        });

        // Meal Plans
        context.MealPlans.Add(new MealPlan
        {
            CustomerId = customer1.Id,
            PlanName = "High Protein Lunch Target",
            TargetType = TargetType.Calorie,
            TargetValue = 650.00m,
            SuggestedItemsJson = "[{\"menuItemId\": 9, \"name\": \"Mediterranean Grilled Chicken Salad\", \"calories\": 390, \"price\": 310.00}]"
        });

        // Notifications
        context.Notifications.Add(new Notification
        {
            UserId = customer1.Id,
            Title = "Order Delivered Successfully",
            Message = "Your order from SpiceCraft Kitchen has arrived. Enjoy your meal!",
            Type = NotificationType.OrderDelivered,
            ReferenceId = order1.Id,
            IsRead = true
        });

        context.SaveChanges();
    }
}
