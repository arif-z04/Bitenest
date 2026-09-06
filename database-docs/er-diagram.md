# BiteNest — Database Design & Entity-Relationship (ER) Documentation

This document provides the formal relational database architecture, entity relationships, normal forms analysis, and a comprehensive Mermaid ER diagram for the **BiteNest Smart Food Delivery & Restaurant Management System**.

---

## 1. Entity-Relationship (ER) Diagram

```mermaid
erDiagram
    USERS ||--o{ RESTAURANTS : "owns/operates"
    USERS ||--o| RIDERS : "profile for delivery"
    USERS ||--o{ ORDERS : "places as customer"
    USERS ||--o{ REVIEWS : "writes"
    USERS ||--o{ MEAL_PLANS : "creates"
    USERS ||--o{ NOTIFICATIONS : "receives"

    RESTAURANTS ||--o{ MENU_ITEMS : "offers"
    RESTAURANTS ||--o{ ORDERS : "fulfills"
    RESTAURANTS ||--o{ REVIEWS : "receives"
    RESTAURANTS ||--o{ LEFTOVER_FOOD_OFFERS : "publishes"

    RIDERS ||--o{ ORDERS : "delivers"

    ORDERS ||--o{ ORDER_ITEMS : "contains"
    ORDERS ||--o| PAYMENTS : "billed through"
    ORDERS ||--o| REVIEWS : "reviewed via"
    ORDERS ||--o{ ORDERS : "bundled into combined delivery"

    MENU_ITEMS ||--o{ ORDER_ITEMS : "item ordered"
    MENU_ITEMS ||--o{ LEFTOVER_FOOD_OFFERS : "discounted as surplus"

    USERS {
        int id PK "Auto Increment"
        string name "Full Name"
        string email "Unique Email Address"
        string password_hash "BCrypt hashed password"
        enum role "Customer, RestaurantOwner, DeliveryRider, Administrator"
        string phone "Contact Number"
        string address "Default Street Address"
        decimal latitude "GPS Latitude"
        decimal longitude "GPS Longitude"
        boolean is_active "Account status flag"
        timestamp created_at "Registration timestamp"
    }

    RESTAURANTS {
        int id PK "Auto Increment"
        int owner_user_id FK "Owner User ID"
        string name "Restaurant Name"
        string description "Culinary summary"
        string category "Cuisine Category"
        string address "Physical Address"
        decimal latitude "GPS Latitude"
        decimal longitude "GPS Longitude"
        string phone "Business Contact"
        string image_url "Storefront Image Path"
        boolean is_verified "Admin Verification Flag"
        boolean is_open "Operating Status"
        int avg_prep_time_minutes "Base preparation duration"
        int kitchen_capacity "Max active orders threshold"
        decimal rating "Average Customer Rating"
        timestamp created_at "Created timestamp"
    }

    MENU_ITEMS {
        int id PK "Auto Increment"
        int restaurant_id FK "Parent Restaurant ID"
        string name "Item Name"
        string description "Item Description"
        decimal price "Standard Price"
        string category "Food Category"
        int prep_time_minutes "Item Cooking Time"
        int calories "Caloric Value (kcal)"
        string image_url "Item Image Path"
        boolean is_available "Stock Status"
        timestamp created_at "Created timestamp"
    }

    ORDERS {
        int id PK "Auto Increment"
        int customer_id FK "Ordering Customer ID"
        int restaurant_id FK "Fulfilling Restaurant ID"
        int rider_id FK "Assigned Rider ID (nullable)"
        int parent_combined_order_id FK "Combined Delivery Parent ID (nullable)"
        boolean is_combined_delivery "Part of multi-restaurant batch"
        string combined_group_code "Shared Batch Tracking Code"
        enum status "Placed, Accepted, Preparing, ReadyForPickup, OutForDelivery, Delivered, Cancelled"
        decimal total_amount "Order Subtotal + Delivery Fee - Discount"
        decimal delivery_fee "Assigned Delivery Fee"
        decimal discount_amount "Applied Discount Amount"
        string delivery_address "Dropoff Address"
        decimal delivery_latitude "Dropoff GPS Lat"
        decimal delivery_longitude "Dropoff GPS Lng"
        datetime preferred_delivery_time "Customer Selected Delivery Time"
        datetime estimated_delivery_time "Calculated Arrival Time"
        datetime delivered_at "Actual Delivery Timestamp"
        timestamp created_at "Order Placement Timestamp"
        timestamp updated_at "Status Modification Timestamp"
    }

    ORDER_ITEMS {
        int id PK "Auto Increment"
        int order_id FK "Parent Order ID"
        int menu_item_id FK "Ordered Menu Item ID"
        int quantity "Item Quantity"
        decimal unit_price "Price at Time of Order"
        decimal subtotal "Quantity * Unit Price"
        string special_instructions "Cooking Customization Note"
    }

    PAYMENTS {
        int id PK "Auto Increment"
        int order_id FK "Associated Order ID"
        decimal amount "Payment Amount"
        enum method "CashOnDelivery, bKash, Nagad, Card, MockDigitalPayment"
        enum status "Pending, Completed, Failed, Refunded"
        string transaction_reference "Payment Reference ID"
        datetime paid_at "Completion Timestamp"
        timestamp created_at "Created Timestamp"
    }

    RIDERS {
        int id PK "Auto Increment"
        int user_id FK "User Account ID"
        enum vehicle_type "Bicycle, Motorcycle, Scooter"
        string license_number "Driver/Vehicle License ID"
        enum current_status "Available, OnDelivery, Offline"
        decimal current_latitude "Rider GPS Lat"
        decimal current_longitude "Rider GPS Lng"
        int total_deliveries "Lifetime Deliveries Counter"
        decimal rating "Rider Service Rating"
        boolean is_verified "Admin Verification Flag"
        timestamp created_at "Created Timestamp"
    }

    REVIEWS {
        int id PK "Auto Increment"
        int customer_id FK "Reviewing Customer ID"
        int restaurant_id FK "Target Restaurant ID"
        int order_id FK "Verified Order ID"
        int rating "Score 1 to 5 Stars"
        string comment "Written Feedback"
        timestamp created_at "Review Timestamp"
    }

    MEAL_PLANS {
        int id PK "Auto Increment"
        int customer_id FK "Owner Customer ID"
        string plan_name "Plan Label"
        enum target_type "Calorie, Budget, Balanced"
        decimal target_value "Target Number (kcal or BDT)"
        json suggested_items_json "Recommended Menu Item References"
        boolean is_active "Plan Active Status"
        timestamp created_at "Created Timestamp"
    }

    LEFTOVER_FOOD_OFFERS {
        int id PK "Auto Increment"
        int restaurant_id FK "Offering Restaurant ID"
        int menu_item_id FK "Surplus Menu Item ID"
        decimal original_price "Standard Menu Price"
        int discount_percent "Percentage Discount (e.g. 20-50%)"
        decimal discounted_price "Calculated Sale Price"
        int quantity_available "Remaining Surplus Count"
        datetime expires_at "Offer Expiration Time"
        boolean is_active "Listing Active Flag"
        timestamp created_at "Created Timestamp"
    }

    NOTIFICATIONS {
        int id PK "Auto Increment"
        int user_id FK "Recipient User ID"
        string title "Notification Title"
        string message "Notification Content"
        enum type "OrderPlaced, OrderAccepted, OrderPreparing, OrderReady, OrderOutForDelivery, OrderDelivered, LeftoverAlert, System"
        int reference_id "Related Entity ID (e.g. OrderId)"
        boolean is_read "Read Status Flag"
        timestamp created_at "Timestamp"
    }
```

---

## 2. Relational Mapping & Cardinality Rules

| Relationship | Cardinality | Business Constraint & Lifecycle Rule |
|---|---|---|
| **Users $\to$ Restaurants** | $1 : N$ (0..*) | A user with role `RestaurantOwner` owns one or more restaurants. Cascade delete is restricted to preserve financial records. |
| **Users $\to$ Riders** | $1 : 1$ (0..1) | A user with role `DeliveryRider` has exactly one rider record containing vehicle credentials, current GPS location, and dispatch status. |
| **Restaurants $\to$ MenuItems** | $1 : N$ (1..*) | Menu items belong to exactly one restaurant. Deleting a restaurant cascades to its menu items. |
| **Users (Customer) $\to$ Orders** | $1 : N$ (0..*) | A customer can place multiple orders over time. |
| **Restaurants $\to$ Orders** | $1 : N$ (0..*) | A restaurant processes multiple customer orders. |
| **Riders $\to$ Orders** | $0..1 : N$ | An order may have no rider assigned initially (`Placed`, `Preparing`), but is assigned to a rider prior to `ReadyForPickup` / `OutForDelivery`. In **Smart Multi-Restaurant Combined Delivery**, one rider is assigned to multiple sibling orders sharing a `combined_group_code`. |
| **Orders $\to$ OrderItems** | $1 : N$ (1..*) | Each order contains one or more line items. If an order is deleted, line items cascade delete. |
| **Orders $\to$ Payments** | $1 : 1$ | Every order has a corresponding payment ledger tracking transaction method, status, and payment timestamp. |
| **Restaurants & MenuItems $\to$ LeftoverFoodOffers** | $1 : N$ | Surplus items are linked directly to their parent restaurant and menu item. Expired offers are filtered automatically by time check (`expires_at > NOW()`). |
| **Users $\to$ MealPlans** | $1 : N$ | Customers save personalized dietary and budget goal templates. |
| **Users $\to$ Notifications** | $1 : N$ | Real-time and persistent notification logs dispatched on order lifecycle events and leftover flash deals. |

---

## 3. Database Normalization (3NF)

1. **First Normal Form (1NF)**:
   - All attributes contain atomic values.
   - Primary keys uniquely identify every row across all tables.
   - No repeating groups or comma-delimited columns (e.g., `OrderItems` are stored as distinct relational rows rather than concatenated strings in `Orders`).
2. **Second Normal Form (2NF)**:
   - All non-key attributes are fully functionally dependent on the complete primary key.
   - In `OrderItems`, columns like `unit_price` and `quantity` depend on `(order_id, menu_item_id)`.
3. **Third Normal Form (3NF)**:
   - No transitive dependencies exist.
   - Restaurant address, coordinates, and contact info belong exclusively in `Restaurants`, not duplicated inside `Orders`.
   - Customer profile details remain strictly in `Users`.
