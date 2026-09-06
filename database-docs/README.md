# BiteNest — Database Design & Data Dictionary

This directory contains the detailed database architecture, table definitions, constraints, indexes, and design considerations for the **BiteNest** system.

## Table of Contents
1. [Overview & Schema Strategy](#overview--schema-strategy)
2. [Data Dictionary](#data-dictionary)
   - [Users Table](#1-users-table)
   - [Restaurants Table](#2-restaurants-table)
   - [MenuItems Table](#3-menuitems-table)
   - [Orders Table](#4-orders-table)
   - [OrderItems Table](#5-orderitems-table)
   - [Payments Table](#6-payments-table)
   - [Riders Table](#7-riders-table)
   - [Reviews Table](#8-reviews-table)
   - [MealPlans Table](#9-mealplans-table)
   - [LeftoverFoodOffers Table](#10-leftoverfoodoffers-table)
   - [Notifications Table](#11-notifications-table)
3. [Special Architectural Features in Schema](#special-architectural-features-in-schema)
   - [Smart Queue Status Implementation](#smart-queue-status-implementation)
   - [Smart Multi-Restaurant Combined Delivery](#smart-multi-restaurant-combined-delivery)
   - [Leftover Deals Expiry Engine](#leftover-deals-expiry-engine)
   - [Customer Preferred Delivery Time Support](#customer-preferred-delivery-time-support)
4. [Indexing & Query Optimization](#indexing--query-optimization)

---

## Overview & Schema Strategy

The BiteNest database (`bitenest_db`) is designed for **MySQL 8.0+** using the `InnoDB` storage engine with `utf8mb4` character encoding. 
It supports high write-throughput for transactional order flows while providing indexes for fast spatial queries (Haversine distance calculation for combined deliveries) and real-time aggregation queries (smart queue status calculation).

---

## Data Dictionary

### 1. `users` Table
Stores authentication credentials, contact information, and role discriminators for all four system actors.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique user identifier |
| `name` | VARCHAR(100) | No | - | Full name of the user |
| `email` | VARCHAR(150) | No | UNIQUE | User login email address |
| `password_hash` | VARCHAR(255) | No | - | BCrypt salted password hash |
| `role` | ENUM | No | 'Customer' | 'Customer', 'RestaurantOwner', 'DeliveryRider', 'Administrator' |
| `phone` | VARCHAR(20) | No | - | Mobile contact number |
| `address` | VARCHAR(255) | Yes | NULL | Default delivery/business street address |
| `latitude` | DECIMAL(10, 7) | Yes | NULL | Geographic coordinate latitude |
| `longitude` | DECIMAL(10, 7) | Yes | NULL | Geographic coordinate longitude |
| `is_active` | BOOLEAN | No | TRUE | Account status flag |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Registration timestamp |
| `updated_at` | TIMESTAMP | No | CURRENT_TIMESTAMP ON UPDATE | Last profile update timestamp |

---

### 2. `restaurants` Table
Represents physical dining establishments registered on the platform.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique restaurant identifier |
| `owner_user_id` | INT | No | FK -> users(id) | Associated RestaurantOwner account |
| `name` | VARCHAR(150) | No | - | Trade name of the restaurant |
| `description` | TEXT | Yes | NULL | Culinary description and specialty |
| `category` | VARCHAR(50) | No | - | Food genre (Fast Food, Traditional, Healthy, Bakery, etc.) |
| `address` | VARCHAR(255) | No | - | Street address of kitchen |
| `latitude` | DECIMAL(10, 7) | No | - | GPS latitude coordinate |
| `longitude` | DECIMAL(10, 7) | No | - | GPS longitude coordinate |
| `phone` | VARCHAR(20) | No | - | Restaurant business phone |
| `image_url` | VARCHAR(255) | Yes | NULL | Storefront banner image URI |
| `is_verified` | BOOLEAN | No | FALSE | Admin approval status flag |
| `is_open` | BOOLEAN | No | TRUE | Operating status toggle |
| `avg_prep_time_minutes` | INT | No | 20 | Baseline preparation time per order in minutes |
| `kitchen_capacity` | INT | No | 15 | Maximum concurrent orders before queue is marked Busy |
| `rating` | DECIMAL(2, 1) | No | 4.5 | Customer review average (1.0 - 5.0) |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Onboarding date |

---

### 3. `menu_items` Table
Catalog of dishes and beverages offered by restaurants.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique dish identifier |
| `restaurant_id` | INT | No | FK -> restaurants(id) | Fulfilling restaurant |
| `name` | VARCHAR(150) | No | - | Food dish name |
| `description` | TEXT | Yes | NULL | Ingredients and description |
| `price` | DECIMAL(10, 2) | No | - | Regular selling price (BDT) |
| `category` | VARCHAR(50) | No | - | Dish category (Burgers, Rice, Pasta, Drinks, Dessert) |
| `prep_time_minutes` | INT | No | 15 | Preparation time for this specific dish |
| `calories` | INT | Yes | NULL | Caloric content (kcal) used by Meal Planner |
| `image_url` | VARCHAR(255) | Yes | NULL | Dish photo URI |
| `is_available` | BOOLEAN | No | TRUE | Availability/in-stock status |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Creation timestamp |

---

### 4. `orders` Table
Transactional centerpiece storing customer orders, status state-machine transitions, and combined delivery relationships.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique order identifier |
| `customer_id` | INT | No | FK -> users(id) | Customer who placed the order |
| `restaurant_id` | INT | No | FK -> restaurants(id) | Restaurant fulfilling the order |
| `rider_id` | INT | Yes | FK -> riders(id) | Assigned delivery rider (NULL until assigned) |
| `parent_combined_order_id` | INT | Yes | FK -> orders(id) | Points to master order if combined |
| `is_combined_delivery` | BOOLEAN | No | FALSE | True if part of a multi-restaurant batch |
| `combined_group_code` | VARCHAR(50) | Yes | NULL | Shared alphanumeric code linking bundled orders |
| `status` | ENUM | No | 'Placed' | 'Placed', 'Accepted', 'Preparing', 'ReadyForPickup', 'OutForDelivery', 'Delivered', 'Cancelled' |
| `total_amount` | DECIMAL(10, 2) | No | - | Total payable amount |
| `delivery_fee` | DECIMAL(10, 2) | No | 40.00 | Applied delivery fee |
| `discount_amount` | DECIMAL(10, 2) | No | 0.00 | Applied discount (leftover or combined savings) |
| `delivery_address` | VARCHAR(255) | No | - | Dropoff location |
| `delivery_latitude` | DECIMAL(10, 7) | Yes | NULL | Dropoff GPS latitude |
| `delivery_longitude` | DECIMAL(10, 7) | Yes | NULL | Dropoff GPS longitude |
| `preferred_delivery_time` | DATETIME | Yes | NULL | Customer-specified scheduled delivery window |
| `estimated_delivery_time` | DATETIME | Yes | NULL | Calculated expected arrival |
| `delivered_at` | DATETIME | Yes | NULL | Timestamp when rider completed dropoff |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Order submission time |
| `updated_at` | TIMESTAMP | No | CURRENT_TIMESTAMP ON UPDATE | State transition time |

---

### 5. `order_items` Table
Line items associated with each order.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique line item identifier |
| `order_id` | INT | No | FK -> orders(id) ON DELETE CASCADE | Associated order |
| `menu_item_id` | INT | No | FK -> menu_items(id) | Reference to dish |
| `quantity` | INT | No | 1 | Ordered count |
| `unit_price` | DECIMAL(10, 2) | No | - | Price per unit at purchase |
| `subtotal` | DECIMAL(10, 2) | No | - | Calculated `quantity * unit_price` |
| `special_instructions` | VARCHAR(255) | Yes | NULL | Customization (e.g. "no onions", "extra spicy") |

---

### 6. `payments` Table
Ledger of financial transactions for orders.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Transaction identifier |
| `order_id` | INT | No | FK -> orders(id) | Associated order |
| `amount` | DECIMAL(10, 2) | No | - | Paid amount |
| `method` | ENUM | No | 'CashOnDelivery' | 'CashOnDelivery', 'bKash', 'Nagad', 'Card', 'MockDigitalPayment' |
| `status` | ENUM | No | 'Pending' | 'Pending', 'Completed', 'Failed', 'Refunded' |
| `transaction_reference` | VARCHAR(100) | Yes | NULL | Gateway or simulated transaction code |
| `paid_at` | DATETIME | Yes | NULL | Completion timestamp |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Creation timestamp |

---

### 7. `riders` Table
Profile, vehicle credentials, and dispatch state for delivery personnel.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique rider identifier |
| `user_id` | INT | No | FK -> users(id) UNIQUE | Rider user account |
| `vehicle_type` | ENUM | No | 'Motorcycle' | 'Bicycle', 'Motorcycle', 'Scooter' |
| `license_number` | VARCHAR(50) | Yes | NULL | Driving / vehicle license number |
| `current_status` | ENUM | No | 'Offline' | 'Available', 'OnDelivery', 'Offline' |
| `current_latitude` | DECIMAL(10, 7) | Yes | NULL | Live GPS latitude |
| `current_longitude` | DECIMAL(10, 7) | Yes | NULL | Live GPS longitude |
| `total_deliveries` | INT | No | 0 | Completed orders counter |
| `rating` | DECIMAL(2, 1) | No | 5.0 | Average rider rating |
| `is_verified` | BOOLEAN | No | FALSE | Admin verification status |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Registration date |

---

### 8. `reviews` Table
Customer feedback and ratings.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Unique review identifier |
| `customer_id` | INT | No | FK -> users(id) | Author customer |
| `restaurant_id` | INT | No | FK -> restaurants(id) | Target restaurant |
| `order_id` | INT | No | FK -> orders(id) | Verified order reference |
| `rating` | INT | No | 5 | Score from 1 to 5 |
| `comment` | TEXT | Yes | NULL | Written customer review |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Submission timestamp |

---

### 9. `meal_plans` Table
Customer dietary or budget goals with recommended menu combinations.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Plan identifier |
| `customer_id` | INT | No | FK -> users(id) | Owner customer |
| `plan_name` | VARCHAR(100) | No | - | Plan title |
| `target_type` | ENUM | No | 'Calorie' | 'Calorie', 'Budget', 'Balanced' |
| `target_value` | DECIMAL(10, 2) | No | - | Target threshold (e.g. 700 kcal or 250 BDT) |
| `suggested_items_json` | JSON | No | - | JSON array of suggested menu item IDs and breakdown |
| `is_active` | BOOLEAN | No | TRUE | Active status |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Creation timestamp |

---

### 10. `leftover_food_offers` Table
Surplus food discount deals published by restaurants to minimize food waste.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Offer identifier |
| `restaurant_id` | INT | No | FK -> restaurants(id) | Offering restaurant |
| `menu_item_id` | INT | No | FK -> menu_items(id) | Surplus menu item |
| `original_price` | DECIMAL(10, 2) | No | - | Standard menu price |
| `discount_percent` | INT | No | 30 | Discount percentage (1 - 90%) |
| `discounted_price` | DECIMAL(10, 2) | No | - | Selling price after discount |
| `quantity_available` | INT | No | 1 | Remaining available portion count |
| `expires_at` | DATETIME | No | - | Cutoff timestamp after which deal is inactive |
| `is_active` | BOOLEAN | No | TRUE | Status toggle |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Publishing timestamp |

---

### 11. `notifications` Table
In-app notification ledger for order lifecycle events and promotional deals.

| Column | Type | Nullable | Default | Description |
|---|---|---|---|---|
| `id` | INT AUTO_INCREMENT | No | Primary Key | Notification identifier |
| `user_id` | INT | No | FK -> users(id) | Target recipient |
| `title` | VARCHAR(150) | No | - | Short title |
| `message` | TEXT | No | - | Full body text |
| `type` | ENUM | No | 'System' | Notification category enum |
| `reference_id` | INT | Yes | NULL | Related entity ID (e.g. order_id) |
| `is_read` | BOOLEAN | No | FALSE | Read status flag |
| `created_at` | TIMESTAMP | No | CURRENT_TIMESTAMP | Dispatch timestamp |

---

## Special Architectural Features in Schema

### Smart Queue Status Implementation
Rather than relying on manual status toggles by kitchen staff that often become stale, BiteNest calculates restaurant queue load dynamically:
$$\text{Active Orders} = \text{COUNT}(\text{Orders with status } \in \{\text{'Placed'}, \text{'Accepted'}, \text{'Preparing'}\})$$
- **Free**: $\text{Active Orders} < \frac{\text{kitchen\_capacity}}{3}$ (typically $< 5$ orders).
- **Normal**: $\frac{\text{kitchen\_capacity}}{3} \le \text{Active Orders} < \text{kitchen\_capacity}$ (typically $5 - 14$ orders).
- **Busy**: $\text{Active Orders} \ge \text{kitchen\_capacity}$ (typically $\ge 15$ orders).

### Smart Multi-Restaurant Combined Delivery
To enable combined ordering from up to 2 restaurants:
1. Coordinates $(lat_1, lon_1)$ and $(lat_2, lon_2)$ are evaluated via Haversine spherical distance:
   $$d = 2R \arcsin\left(\sqrt{\sin^2\left(\frac{\Delta \phi}{2}\right) + \cos(\phi_1)\cos(\phi_2)\sin^2\left(\frac{\Delta \lambda}{2}\right)}\right)$$
2. If $d \le 2.0\text{ km}$, a combined order batch is permitted.
3. Two order records share a unique `combined_group_code` (e.g. `COMB-98214`), are assigned the **same** `rider_id`, and receive a discounted combined delivery fee ($60\text{ BDT}$ total instead of $2 \times 40 = 80\text{ BDT}$, saving $20\text{ BDT}$).

---

## Indexing & Query Optimization

The database schema applies targeted B-Tree indexes:
1. `idx_restaurants_location` on `(latitude, longitude)` for nearby discovery.
2. `idx_orders_restaurant_status` on `(restaurant_id, status)` to make queue calculation sub-millisecond.
3. `idx_orders_customer` on `(customer_id, created_at DESC)` for instantaneous order history display.
4. `idx_orders_combined` on `(combined_group_code)` for atomic multi-restaurant batch queries.
5. `idx_leftover_active` on `(is_active, expires_at)` to eliminate expired deals without full table scans.
