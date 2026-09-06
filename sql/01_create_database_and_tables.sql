-- ============================================================================
-- BiteNest: Smart Food Delivery & Restaurant Management System
-- Script: 01_create_database_and_tables.sql
-- Description: DDL script creating database and all normalized tables
-- Engine: MySQL 8.0+ / MariaDB
-- ============================================================================

CREATE DATABASE IF NOT EXISTS `bitenest_db`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `bitenest_db`;

-- Disable foreign key checks during schema re-creation
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `notifications`;
DROP TABLE IF EXISTS `leftover_food_offers`;
DROP TABLE IF EXISTS `meal_plans`;
DROP TABLE IF EXISTS `reviews`;
DROP TABLE IF EXISTS `payments`;
DROP TABLE IF EXISTS `order_items`;
DROP TABLE IF EXISTS `orders`;
DROP TABLE IF EXISTS `riders`;
DROP TABLE IF EXISTS `menu_items`;
DROP TABLE IF EXISTS `restaurants`;
DROP TABLE IF EXISTS `users`;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- 1. USERS TABLE
-- ============================================================================
CREATE TABLE `users` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `password_hash` VARCHAR(255) NOT NULL,
    `role` ENUM('Customer', 'RestaurantOwner', 'DeliveryRider', 'Administrator') NOT NULL DEFAULT 'Customer',
    `phone` VARCHAR(20) NOT NULL,
    `address` VARCHAR(255) NULL,
    `latitude` DECIMAL(10, 7) NULL,
    `longitude` DECIMAL(10, 7) NULL,
    `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_users_role` (`role`),
    INDEX `idx_users_email` (`email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 2. RESTAURANTS TABLE
-- ============================================================================
CREATE TABLE `restaurants` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `owner_user_id` INT NOT NULL,
    `name` VARCHAR(150) NOT NULL,
    `description` TEXT NULL,
    `category` VARCHAR(50) NOT NULL,
    `address` VARCHAR(255) NOT NULL,
    `latitude` DECIMAL(10, 7) NOT NULL,
    `longitude` DECIMAL(10, 7) NOT NULL,
    `phone` VARCHAR(20) NOT NULL,
    `image_url` VARCHAR(255) NULL,
    `is_verified` BOOLEAN NOT NULL DEFAULT FALSE,
    `is_open` BOOLEAN NOT NULL DEFAULT TRUE,
    `avg_prep_time_minutes` INT NOT NULL DEFAULT 20,
    `kitchen_capacity` INT NOT NULL DEFAULT 15,
    `rating` DECIMAL(2, 1) NOT NULL DEFAULT 4.5,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_restaurants_owner` FOREIGN KEY (`owner_user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT,
    INDEX `idx_restaurants_location` (`latitude`, `longitude`),
    INDEX `idx_restaurants_category` (`category`),
    INDEX `idx_restaurants_verified_open` (`is_verified`, `is_open`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 3. MENU_ITEMS TABLE
-- ============================================================================
CREATE TABLE `menu_items` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `restaurant_id` INT NOT NULL,
    `name` VARCHAR(150) NOT NULL,
    `description` TEXT NULL,
    `price` DECIMAL(10, 2) NOT NULL,
    `category` VARCHAR(50) NOT NULL,
    `prep_time_minutes` INT NOT NULL DEFAULT 15,
    `calories` INT NULL,
    `image_url` VARCHAR(255) NULL,
    `is_available` BOOLEAN NOT NULL DEFAULT TRUE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_menu_items_restaurant` FOREIGN KEY (`restaurant_id`) REFERENCES `restaurants` (`id`) ON DELETE CASCADE,
    INDEX `idx_menu_items_restaurant_available` (`restaurant_id`, `is_available`),
    INDEX `idx_menu_items_category` (`category`),
    INDEX `idx_menu_items_calories` (`calories`),
    INDEX `idx_menu_items_price` (`price`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 4. RIDERS TABLE
-- ============================================================================
CREATE TABLE `riders` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `user_id` INT NOT NULL UNIQUE,
    `vehicle_type` ENUM('Bicycle', 'Motorcycle', 'Scooter') NOT NULL DEFAULT 'Motorcycle',
    `license_number` VARCHAR(50) NULL,
    `current_status` ENUM('Available', 'OnDelivery', 'Offline') NOT NULL DEFAULT 'Offline',
    `current_latitude` DECIMAL(10, 7) NULL,
    `current_longitude` DECIMAL(10, 7) NULL,
    `total_deliveries` INT NOT NULL DEFAULT 0,
    `rating` DECIMAL(2, 1) NOT NULL DEFAULT 5.0,
    `is_verified` BOOLEAN NOT NULL DEFAULT FALSE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_riders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    INDEX `idx_riders_status` (`current_status`, `is_verified`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 5. ORDERS TABLE
-- ============================================================================
CREATE TABLE `orders` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `customer_id` INT NOT NULL,
    `restaurant_id` INT NOT NULL,
    `rider_id` INT NULL,
    `parent_combined_order_id` INT NULL,
    `is_combined_delivery` BOOLEAN NOT NULL DEFAULT FALSE,
    `combined_group_code` VARCHAR(50) NULL,
    `status` ENUM('Placed', 'Accepted', 'Preparing', 'ReadyForPickup', 'OutForDelivery', 'Delivered', 'Cancelled') NOT NULL DEFAULT 'Placed',
    `total_amount` DECIMAL(10, 2) NOT NULL,
    `delivery_fee` DECIMAL(10, 2) NOT NULL DEFAULT 40.00,
    `discount_amount` DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    `delivery_address` VARCHAR(255) NOT NULL,
    `delivery_latitude` DECIMAL(10, 7) NULL,
    `delivery_longitude` DECIMAL(10, 7) NULL,
    `preferred_delivery_time` DATETIME NULL,
    `estimated_delivery_time` DATETIME NULL,
    `delivered_at` DATETIME NULL,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT `fk_orders_customer` FOREIGN KEY (`customer_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_orders_restaurant` FOREIGN KEY (`restaurant_id`) REFERENCES `restaurants` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `fk_orders_rider` FOREIGN KEY (`rider_id`) REFERENCES `riders` (`id`) ON DELETE SET NULL,
    CONSTRAINT `fk_orders_parent` FOREIGN KEY (`parent_combined_order_id`) REFERENCES `orders` (`id`) ON DELETE SET NULL,
    INDEX `idx_orders_customer` (`customer_id`, `created_at`),
    INDEX `idx_orders_restaurant_status` (`restaurant_id`, `status`),
    INDEX `idx_orders_rider_status` (`rider_id`, `status`),
    INDEX `idx_orders_combined_group` (`combined_group_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 6. ORDER_ITEMS TABLE
-- ============================================================================
CREATE TABLE `order_items` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `order_id` INT NOT NULL,
    `menu_item_id` INT NOT NULL,
    `quantity` INT NOT NULL DEFAULT 1,
    `unit_price` DECIMAL(10, 2) NOT NULL,
    `subtotal` DECIMAL(10, 2) NOT NULL,
    `special_instructions` VARCHAR(255) NULL,
    CONSTRAINT `fk_order_items_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_order_items_item` FOREIGN KEY (`menu_item_id`) REFERENCES `menu_items` (`id`) ON DELETE RESTRICT,
    INDEX `idx_order_items_order` (`order_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 7. PAYMENTS TABLE
-- ============================================================================
CREATE TABLE `payments` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `order_id` INT NOT NULL,
    `amount` DECIMAL(10, 2) NOT NULL,
    `method` ENUM('CashOnDelivery', 'bKash', 'Nagad', 'Card', 'MockDigitalPayment') NOT NULL DEFAULT 'CashOnDelivery',
    `status` ENUM('Pending', 'Completed', 'Failed', 'Refunded') NOT NULL DEFAULT 'Pending',
    `transaction_reference` VARCHAR(100) NULL,
    `paid_at` DATETIME NULL,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_payments_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE,
    INDEX `idx_payments_order` (`order_id`),
    INDEX `idx_payments_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 8. REVIEWS TABLE
-- ============================================================================
CREATE TABLE `reviews` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `customer_id` INT NOT NULL,
    `restaurant_id` INT NOT NULL,
    `order_id` INT NOT NULL,
    `rating` INT NOT NULL CHECK (`rating` BETWEEN 1 AND 5),
    `comment` TEXT NULL,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_reviews_customer` FOREIGN KEY (`customer_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_reviews_restaurant` FOREIGN KEY (`restaurant_id`) REFERENCES `restaurants` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_reviews_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE,
    INDEX `idx_reviews_restaurant` (`restaurant_id`, `rating`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 9. MEAL_PLANS TABLE
-- ============================================================================
CREATE TABLE `meal_plans` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `customer_id` INT NOT NULL,
    `plan_name` VARCHAR(100) NOT NULL,
    `target_type` ENUM('Calorie', 'Budget', 'Balanced') NOT NULL,
    `target_value` DECIMAL(10, 2) NOT NULL,
    `suggested_items_json` JSON NOT NULL,
    `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_meal_plans_customer` FOREIGN KEY (`customer_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    INDEX `idx_meal_plans_customer` (`customer_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 10. LEFTOVER_FOOD_OFFERS TABLE
-- ============================================================================
CREATE TABLE `leftover_food_offers` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `restaurant_id` INT NOT NULL,
    `menu_item_id` INT NOT NULL,
    `original_price` DECIMAL(10, 2) NOT NULL,
    `discount_percent` INT NOT NULL CHECK (`discount_percent` BETWEEN 1 AND 90),
    `discounted_price` DECIMAL(10, 2) NOT NULL,
    `quantity_available` INT NOT NULL DEFAULT 1,
    `expires_at` DATETIME NOT NULL,
    `is_active` BOOLEAN NOT NULL DEFAULT TRUE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_leftover_restaurant` FOREIGN KEY (`restaurant_id`) REFERENCES `restaurants` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_leftover_menu_item` FOREIGN KEY (`menu_item_id`) REFERENCES `menu_items` (`id`) ON DELETE CASCADE,
    INDEX `idx_leftover_active_expiry` (`is_active`, `expires_at`),
    INDEX `idx_leftover_restaurant` (`restaurant_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 11. NOTIFICATIONS TABLE
-- ============================================================================
CREATE TABLE `notifications` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `user_id` INT NOT NULL,
    `title` VARCHAR(150) NOT NULL,
    `message` TEXT NOT NULL,
    `type` ENUM('OrderPlaced', 'OrderAccepted', 'OrderPreparing', 'OrderReady', 'OrderOutForDelivery', 'OrderDelivered', 'LeftoverAlert', 'System') NOT NULL DEFAULT 'System',
    `reference_id` INT NULL,
    `is_read` BOOLEAN NOT NULL DEFAULT FALSE,
    `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_notifications_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
    INDEX `idx_notifications_user_unread` (`user_id`, `is_read`, `created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
