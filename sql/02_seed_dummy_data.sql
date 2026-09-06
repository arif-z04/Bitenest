-- ============================================================================
-- BiteNest: Smart Food Delivery & Restaurant Management System
-- Script: 02_seed_dummy_data.sql
-- Description: Comprehensive seed data for testing all system capabilities
-- Engine: MySQL 8.0+ / MariaDB
-- ============================================================================

USE `bitenest_db`;

SET FOREIGN_KEY_CHECKS = 0;

TRUNCATE TABLE `notifications`;
TRUNCATE TABLE `leftover_food_offers`;
TRUNCATE TABLE `meal_plans`;
TRUNCATE TABLE `reviews`;
TRUNCATE TABLE `payments`;
TRUNCATE TABLE `order_items`;
TRUNCATE TABLE `orders`;
TRUNCATE TABLE `riders`;
TRUNCATE TABLE `menu_items`;
TRUNCATE TABLE `restaurants`;
TRUNCATE TABLE `users`;

SET FOREIGN_KEY_CHECKS = 1;

-- ============================================================================
-- 1. SEED USERS (Passwords hashed with BCrypt for 'Password@123')
-- Hash: $2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia
-- ============================================================================
INSERT INTO `users` (`id`, `name`, `email`, `password_hash`, `role`, `phone`, `address`, `latitude`, `longitude`, `is_active`) VALUES
(1, 'System Administrator', 'admin@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'Administrator', '+8801711000001', 'BiteNest HQ, Road 11, Dhanmondi, Dhaka', 23.7510000, 90.3750000, 1),
(2, 'SpiceCraft Manager', 'owner.spicecraft@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'RestaurantOwner', '+8801711000002', 'House 42, Road 7A, Dhanmondi, Dhaka', 23.7508000, 90.3752000, 1),
(3, 'Burger Barn Owner', 'owner.burgerbarn@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'RestaurantOwner', '+8801711000003', 'Plot 15, Satmasjid Road, Dhanmondi, Dhaka', 23.7540000, 90.3780000, 1),
(4, 'Green Bowls Director', 'owner.greenbowls@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'RestaurantOwner', '+8801711000004', 'Block C, Road 27, Dhanmondi, Dhaka', 23.7580000, 90.3810000, 1),
(5, 'Rahim Ahmed', 'customer.rahim@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'Customer', '+8801711000005', 'Apartment 4B, Road 12, Dhanmondi, Dhaka', 23.7525000, 90.3765000, 1),
(6, 'Fatima Jahan', 'customer.fatima@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'Customer', '+8801711000006', 'House 88, Road 8, Dhanmondi, Dhaka', 23.7495000, 90.3740000, 1),
(7, 'Tanvir Hasan (Rider 1)', 'rider.tanvir@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'DeliveryRider', '+8801711000007', 'Shankar Stand, Dhanmondi, Dhaka', 23.7515000, 90.3760000, 1),
(8, 'Sumon Barua (Rider 2)', 'rider.sumon@bitenest.com', '$2a$11$eE1LzK7oUuJg2pB0vHjNfeIqOQoYj6f3gX.8c4D6m0E7U9yY/qUia', 'DeliveryRider', '+8801711000008', 'Zigatola Bus Stand, Dhanmondi, Dhaka', 23.7480000, 90.3720000, 1);

-- ============================================================================
-- 2. SEED RESTAURANTS
-- (SpiceCraft and Burger Barn are 0.46 km apart; Green Bowls is 0.95 km away)
-- ============================================================================
INSERT INTO `restaurants` (`id`, `owner_user_id`, `name`, `description`, `category`, `address`, `latitude`, `longitude`, `phone`, `image_url`, `is_verified`, `is_open`, `avg_prep_time_minutes`, `kitchen_capacity`, `rating`) VALUES
(1, 2, 'SpiceCraft Kitchen', 'Authentic slow-cooked biryani, aromatic curries, and freshly baked tandoori naans.', 'Bengali & Indian', 'House 42, Road 7A, Dhanmondi, Dhaka', 23.7508000, 90.3752000, '+8801711000002', 'https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=600&auto=format&fit=crop&q=80', 1, 1, 25, 12, 4.8),
(2, 3, 'Burger Barn & Grill', 'Gourmet smashed beef burgers, crispy buttermilk chicken tenders, and loaded cheese fries.', 'Fast Food & Burgers', 'Plot 15, Satmasjid Road, Dhanmondi, Dhaka', 23.7540000, 90.3780000, '+8801711000003', 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80', 1, 1, 15, 18, 4.6),
(3, 4, 'Green Bowls & Juices', 'Nutrient-rich protein grain bowls, detox fresh cold-pressed juices, and organic wraps.', 'Healthy & Salads', 'Block C, Road 27, Dhanmondi, Dhaka', 23.7580000, 90.3810000, '+8801711000004', 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80', 1, 1, 12, 10, 4.7);

-- ============================================================================
-- 3. SEED MENU ITEMS
-- ============================================================================
INSERT INTO `menu_items` (`id`, `restaurant_id`, `name`, `description`, `price`, `category`, `prep_time_minutes`, `calories`, `image_url`, `is_available`) VALUES
-- SpiceCraft Kitchen
(1, 1, 'Kacchi Biryani Special', 'Fragrant basmati rice layered with tender mutton chunks and spiced saffron potato.', 380.00, 'Main Course', 25, 820, 'https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?w=600&auto=format&fit=crop&q=80', 1),
(2, 1, 'Butter Chicken Masala', 'Charcoal-grilled chicken simmered in a velvety tomato, cashew, and cream gravy.', 320.00, 'Main Course', 20, 580, 'https://images.unsplash.com/photo-1603894584373-5ac82b2ae398?w=600&auto=format&fit=crop&q=80', 1),
(3, 1, 'Garlic Butter Naan', 'Tandoor baked flatbread brushed with roasted garlic butter and cilantro.', 60.00, 'Bread', 10, 210, 'https://images.unsplash.com/photo-1601050690597-df0568f70950?w=600&auto=format&fit=crop&q=80', 1),
(4, 1, 'Shahi Morog Polao', 'Classic Dhaka-style festive chicken pilaf served with boiled egg and sweet plum chutney.', 340.00, 'Main Course', 22, 720, 'https://images.unsplash.com/photo-1633945274405-b6c8069047b0?w=600&auto=format&fit=crop&q=80', 1),
(5, 1, 'Borhani Pitcher (500ml)', 'Traditional spiced yogurt digestive drink with mint, coriander, and black rock salt.', 90.00, 'Beverages', 5, 140, 'https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80', 1),
(6, 1, 'Firni Dessert Bowl', 'Creamy ground rice pudding slow-infused with saffron, cardamom, and sliced almonds.', 85.00, 'Dessert', 5, 260, 'https://images.unsplash.com/photo-1551024601-bec78aea704b?w=600&auto=format&fit=crop&q=80', 1),

-- Burger Barn & Grill
(7, 2, 'Classic Smokehouse Beef Burger', '150g grilled beef patty, melted cheddar, caramelized onions, and house smoky barbecue mayo.', 290.00, 'Burgers', 15, 690, 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80', 1),
(8, 2, 'Crispy Hot Buttermilk Chicken Burger', 'Double fried crunchy chicken thigh tossed in spicy cayenne butter with crisp dill pickles.', 270.00, 'Burgers', 15, 640, 'https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=600&auto=format&fit=crop&q=80', 1),
(9, 2, 'Truffle Parmesan Loaded Fries', 'Hand-cut golden fries tossed with white truffle oil, grated parmesan, and chives.', 160.00, 'Sides', 10, 420, 'https://images.unsplash.com/photo-1576107232684-1279f3908594?w=600&auto=format&fit=crop&q=80', 1),
(10, 2, 'BBQ Glazed Chicken Wings (6 pcs)', 'Crispy wings smothered in tangy hickory smoked BBQ glaze with ranch dip.', 240.00, 'Appetizers', 15, 510, 'https://images.unsplash.com/photo-1567620832903-9fc6debc209f?w=600&auto=format&fit=crop&q=80', 1),
(11, 2, 'Belgian Chocolate Milkshake', 'Thick blended artisanal dark chocolate ice cream topped with chocolate drizzle.', 180.00, 'Beverages', 8, 380, 'https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&auto=format&fit=crop&q=80', 1),
(12, 2, 'Cheddar Bacon Melt Single', 'Smashed beef patty, crispy beef bacon, and double melted American cheese.', 330.00, 'Burgers', 14, 750, 'https://images.unsplash.com/photo-1586190848861-99aa4a171e90?w=600&auto=format&fit=crop&q=80', 1),

-- Green Bowls & Juices
(13, 3, 'Mediterranean Grilled Chicken Salad', 'Herb marinated chicken strips over mixed greens, kalamata olives, feta cheese, and lemon vinaigrette.', 310.00, 'Salads', 12, 390, 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80', 1),
(14, 3, 'Quinoa Avocado Power Bowl', 'Organic red quinoa, fresh Hass avocado, edamame, roasted chickpeas, and tahini drizzle.', 340.00, 'Grain Bowls', 12, 460, 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80', 1),
(15, 3, 'Chipotle Paneer Protein Wrap', 'Grilled cottage cheese cubes, roasted bell peppers, brown rice, and yogurt chipotle dressing.', 260.00, 'Wraps', 10, 410, 'https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=600&auto=format&fit=crop&q=80', 1),
(16, 3, 'Cold Pressed Green Detox Juice', 'Pure blend of baby spinach, celery, green apple, cucumber, and ginger.', 140.00, 'Beverages', 5, 95, 'https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80', 1),
(17, 3, 'Berry Acai Antioxidant Smoothie', 'Wild acai pulp blended with blueberries, almond milk, and organic chia seeds.', 220.00, 'Beverages', 7, 210, 'https://images.unsplash.com/photo-1553530666-ba11a7da3888?w=600&auto=format&fit=crop&q=80', 1),
(18, 3, 'Vegan Chocolate Chia Pudding', 'Coconut cream chia pudding layered with raw cocoa nibs and strawberries.', 150.00, 'Dessert', 5, 240, 'https://images.unsplash.com/photo-1541658016709-82535e94bc69?w=600&auto=format&fit=crop&q=80', 1);

-- ============================================================================
-- 4. SEED RIDERS
-- ============================================================================
INSERT INTO `riders` (`id`, `user_id`, `vehicle_type`, `license_number`, `current_status`, `current_latitude`, `current_longitude`, `total_deliveries`, `rating`, `is_verified`) VALUES
(1, 7, 'Motorcycle', 'DHAKA-METRO-HA-123456', 'Available', 23.7515000, 90.3760000, 142, 4.9, 1),
(2, 8, 'Motorcycle', 'DHAKA-METRO-LA-654321', 'OnDelivery', 23.7480000, 90.3720000, 89, 4.8, 1);

-- ============================================================================
-- 5. SEED LEFTOVER FOOD OFFERS (Near end-of-day discounts)
-- ============================================================================
INSERT INTO `leftover_food_offers` (`id`, `restaurant_id`, `menu_item_id`, `original_price`, `discount_percent`, `discounted_price`, `quantity_available`, `expires_at`, `is_active`) VALUES
(1, 1, 1, 380.00, 35, 247.00, 4, DATE_ADD(NOW(), INTERVAL 4 HOUR), 1),
(2, 2, 8, 270.00, 30, 189.00, 3, DATE_ADD(NOW(), INTERVAL 3 HOUR), 1),
(3, 3, 14, 340.00, 40, 204.00, 5, DATE_ADD(NOW(), INTERVAL 5 HOUR), 1),
(4, 1, 4, 340.00, 50, 170.00, 0, DATE_SUB(NOW(), INTERVAL 1 HOUR), 0); -- Expired offer for filter testing

-- ============================================================================
-- 6. SEED ORDERS
-- (Includes delivered history, active orders demonstrating Smart Queue,
-- and a Smart Multi-Restaurant Combined Delivery order batch)
-- ============================================================================
INSERT INTO `orders` (`id`, `customer_id`, `restaurant_id`, `rider_id`, `parent_combined_order_id`, `is_combined_delivery`, `combined_group_code`, `status`, `total_amount`, `delivery_fee`, `discount_amount`, `delivery_address`, `delivery_latitude`, `delivery_longitude`, `preferred_delivery_time`, `estimated_delivery_time`, `delivered_at`, `created_at`) VALUES
-- 1. Delivered historical order
(1, 5, 1, 1, NULL, 0, NULL, 'Delivered', 420.00, 40.00, 0.00, 'Apartment 4B, Road 12, Dhanmondi, Dhaka', 23.7525000, 90.3765000, NULL, DATE_SUB(NOW(), INTERVAL 2 HOUR), DATE_SUB(NOW(), INTERVAL 90 MINUTE), DATE_SUB(NOW(), INTERVAL 2 HOUR)),

-- 2. Active order: Preparing at SpiceCraft
(2, 6, 1, NULL, NULL, 0, NULL, 'Preparing', 470.00, 40.00, 0.00, 'House 88, Road 8, Dhanmondi, Dhaka', 23.7495000, 90.3740000, DATE_ADD(NOW(), INTERVAL 45 MINUTE), DATE_ADD(NOW(), INTERVAL 30 MINUTE), NULL, DATE_SUB(NOW(), INTERVAL 15 MINUTE)),

-- 3. Active order: ReadyForPickup at Burger Barn (Assigned to Rider 2)
(3, 5, 2, 2, NULL, 0, NULL, 'OutForDelivery', 490.00, 40.00, 0.00, 'Apartment 4B, Road 12, Dhanmondi, Dhaka', 23.7525000, 90.3765000, NULL, DATE_ADD(NOW(), INTERVAL 15 MINUTE), NULL, DATE_SUB(NOW(), INTERVAL 25 MINUTE)),

-- 4 & 5. Smart Multi-Restaurant Combined Delivery Order Pair!
-- Customer ordered Biryani from SpiceCraft (ID 1) AND Milkshake from Burger Barn (ID 2)
-- Combined group code links both; delivery fee discounted to 30.00 each (60.00 total instead of 80.00, saving 20.00!)
(4, 6, 1, 1, NULL, 1, 'COMB-20260906-01', 'Accepted', 410.00, 30.00, 20.00, 'House 88, Road 8, Dhanmondi, Dhaka', 23.7495000, 90.3740000, DATE_ADD(NOW(), INTERVAL 60 MINUTE), DATE_ADD(NOW(), INTERVAL 45 MINUTE), NULL, DATE_SUB(NOW(), INTERVAL 5 MINUTE)),
(5, 6, 2, 1, 4, 1, 'COMB-20260906-01', 'Accepted', 210.00, 30.00, 0.00, 'House 88, Road 8, Dhanmondi, Dhaka', 23.7495000, 90.3740000, DATE_ADD(NOW(), INTERVAL 60 MINUTE), DATE_ADD(NOW(), INTERVAL 45 MINUTE), NULL, DATE_SUB(NOW(), INTERVAL 5 MINUTE));

-- ============================================================================
-- 7. SEED ORDER ITEMS
-- ============================================================================
INSERT INTO `order_items` (`id`, `order_id`, `menu_item_id`, `quantity`, `unit_price`, `subtotal`, `special_instructions`) VALUES
-- Order 1 items
(1, 1, 1, 1, 380.00, 380.00, 'Less spice on meat'),
-- Order 2 items
(2, 2, 2, 1, 320.00, 320.00, 'Extra gravy'),
(3, 2, 5, 1, 90.00, 90.00, 'Serve cold'),
(4, 2, 3, 1, 60.00, 60.00, NULL),
-- Order 3 items
(5, 3, 7, 1, 290.00, 290.00, 'Well done patty'),
(6, 3, 9, 1, 160.00, 160.00, 'Extra parmesan'),
-- Combined Order 4 (SpiceCraft component)
(7, 4, 1, 1, 380.00, 380.00, 'Pack with extra green chili'),
-- Combined Order 5 (Burger Barn component)
(8, 5, 11, 1, 180.00, 180.00, 'Chilled insulated pack');

-- ============================================================================
-- 8. SEED PAYMENTS
-- ============================================================================
INSERT INTO `payments` (`id`, `order_id`, `amount`, `method`, `status`, `transaction_reference`, `paid_at`) VALUES
(1, 1, 420.00, 'bKash', 'Completed', 'TRX-BKASH-991823', DATE_SUB(NOW(), INTERVAL 2 HOUR)),
(2, 2, 470.00, 'CashOnDelivery', 'Pending', NULL, NULL),
(3, 3, 490.00, 'Card', 'Completed', 'TRX-VISA-551239', DATE_SUB(NOW(), INTERVAL 25 MINUTE)),
(4, 4, 410.00, 'bKash', 'Completed', 'TRX-BKASH-881900', DATE_SUB(NOW(), INTERVAL 5 MINUTE)),
(5, 5, 210.00, 'bKash', 'Completed', 'TRX-BKASH-881901', DATE_SUB(NOW(), INTERVAL 5 MINUTE));

-- ============================================================================
-- 9. SEED REVIEWS
-- ============================================================================
INSERT INTO `reviews` (`id`, `customer_id`, `restaurant_id`, `order_id`, `rating`, `comment`, `created_at`) VALUES
(1, 5, 1, 1, 5, 'Exceptional Kacchi Biryani! Meat was tender and falling off the bone. Fast delivery by Tanvir.', DATE_SUB(NOW(), INTERVAL 1 HOUR)),
(2, 6, 2, 3, 4, 'Burger patty was succulent and fries stayed crispy during transit. Loved the packaging.', DATE_SUB(NOW(), INTERVAL 2 HOUR));

-- ============================================================================
-- 10. SEED MEAL PLANS
-- ============================================================================
INSERT INTO `meal_plans` (`id`, `customer_id`, `plan_name`, `target_type`, `target_value`, `suggested_items_json`, `is_active`) VALUES
(1, 5, 'High Protein Lunch Target', 'Calorie', 650.00, '[{"menu_item_id": 13, "name": "Mediterranean Grilled Chicken Salad", "calories": 390, "price": 310.00}, {"menu_item_id": 17, "name": "Berry Acai Antioxidant Smoothie", "calories": 210, "price": 220.00}]', 1),
(2, 6, 'Student Budget Dinner', 'Budget', 300.00, '[{"menu_item_id": 8, "name": "Crispy Hot Buttermilk Chicken Burger", "price": 270.00, "calories": 640}]', 1);

-- ============================================================================
-- 11. SEED NOTIFICATIONS
-- ============================================================================
INSERT INTO `notifications` (`id`, `user_id`, `title`, `message`, `type`, `reference_id`, `is_read`) VALUES
(1, 5, 'Order Delivered Successfully', 'Your order #1 from SpiceCraft Kitchen has arrived. Enjoy your meal!', 'OrderDelivered', 1, 1),
(2, 6, 'Combined Delivery Confirmed', 'Your combined delivery #COMB-20260906-01 from SpiceCraft & Burger Barn is accepted and assigned to Rider Tanvir.', 'OrderAccepted', 4, 0),
(3, 7, 'New Combined Delivery Assignment', 'You have been assigned combined pickup batch #COMB-20260906-01 (2 stops: SpiceCraft -> Burger Barn).', 'OrderAccepted', 4, 0),
(4, 5, 'Leftover Flash Deal Alert', 'SpiceCraft Kitchen just posted Kacchi Biryani at 35% discount for the next 4 hours!', 'LeftoverAlert', 1, 0);
