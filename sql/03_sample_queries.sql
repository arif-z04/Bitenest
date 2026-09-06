-- ============================================================================
-- BiteNest: Smart Food Delivery & Restaurant Management System
-- Script: 03_sample_queries.sql
-- Description: Core analytical, operational, and innovative feature queries
-- Engine: MySQL 8.0+ / MariaDB
-- ============================================================================

USE `bitenest_db`;

-- ============================================================================
-- QUERY 1: SMART QUEUE STATUS CALCULATION
-- Computes real-time workload level (Free / Normal / Busy) dynamically based
-- on active orders (Placed, Accepted, Preparing) vs kitchen capacity.
-- ============================================================================
SELECT 
    r.id AS restaurant_id,
    r.name AS restaurant_name,
    r.category,
    r.kitchen_capacity,
    r.avg_prep_time_minutes,
    COUNT(o.id) AS active_orders_count,
    CASE 
        WHEN COUNT(o.id) < (r.kitchen_capacity / 3) THEN 'Free'
        WHEN COUNT(o.id) < r.kitchen_capacity THEN 'Normal'
        ELSE 'Busy'
    END AS queue_status,
    -- Estimated wait time in minutes = avg_prep_time + (active_orders * 3 min queue delay)
    (r.avg_prep_time_minutes + (COUNT(o.id) * 3)) AS estimated_prep_time_minutes
FROM `restaurants` r
LEFT JOIN `orders` o 
    ON r.id = o.restaurant_id 
    AND o.status IN ('Placed', 'Accepted', 'Preparing')
WHERE r.is_open = 1 AND r.is_verified = 1
GROUP BY r.id, r.name, r.category, r.kitchen_capacity, r.avg_prep_time_minutes
ORDER BY active_orders_count ASC;

-- ============================================================================
-- QUERY 2: SMART MULTI-RESTAURANT COMBINED DELIVERY PROXIMITY CHECK
-- Finds nearby restaurants within 2.0 km of a given restaurant (e.g. SpiceCraft ID 1)
-- using the Haversine spherical distance formula in kilometers.
-- ============================================================================
SELECT 
    r2.id AS partner_restaurant_id,
    r2.name AS partner_restaurant_name,
    r2.category AS partner_category,
    r2.address,
    ROUND(
        6371 * 2 * ASIN(
            SQRT(
                POWER(SIN(RADIANS(r2.latitude - r1.latitude) / 2), 2) +
                COS(RADIANS(r1.latitude)) * COS(RADIANS(r2.latitude)) *
                POWER(SIN(RADIANS(r2.longitude - r1.longitude) / 2), 2)
            )
        ), 2
    ) AS distance_km,
    CASE 
        WHEN (6371 * 2 * ASIN(
            SQRT(
                POWER(SIN(RADIANS(r2.latitude - r1.latitude) / 2), 2) +
                COS(RADIANS(r1.latitude)) * COS(RADIANS(r2.latitude)) *
                POWER(SIN(RADIANS(r2.longitude - r1.longitude) / 2), 2)
            )
        )) <= 2.0 THEN 'ELIGIBLE_FOR_COMBINED_DELIVERY'
        ELSE 'TOO_FAR_FOR_COMBINED_DELIVERY'
    END AS combined_delivery_status
FROM `restaurants` r1
CROSS JOIN `restaurants` r2
WHERE r1.id = 1 -- SpiceCraft Kitchen
  AND r2.id != 1
  AND r2.is_open = 1
  AND r2.is_verified = 1
HAVING distance_km <= 2.0
ORDER BY distance_km ASC;

-- ============================================================================
-- QUERY 3: ACTIVE LEFTOVER FOOD DISCOUNT OFFERS (Filtering Expired Deals)
-- Retrieves non-expired surplus food offers with remaining time in minutes
-- and calculates customer savings.
-- ============================================================================
SELECT 
    l.id AS offer_id,
    r.name AS restaurant_name,
    m.name AS food_item_name,
    m.category,
    l.original_price,
    l.discount_percent,
    l.discounted_price,
    (l.original_price - l.discounted_price) AS customer_savings_bdt,
    l.quantity_available,
    l.expires_at,
    TIMESTAMPDIFF(MINUTE, NOW(), l.expires_at) AS minutes_remaining
FROM `leftover_food_offers` l
JOIN `restaurants` r ON l.restaurant_id = r.id
JOIN `menu_items` m ON l.menu_item_id = m.id
WHERE l.is_active = 1
  AND l.quantity_available > 0
  AND l.expires_at > NOW()
ORDER BY l.discount_percent DESC;

-- ============================================================================
-- QUERY 4: ADVANCED MEAL PLANNER — SUGGESTIONS BY CALORIE OR BUDGET
-- Finds single dishes or combos matching a user calorie target (e.g. <= 700 kcal)
-- or budget target (e.g. <= 300 BDT) sorted for optimal nutritional value.
-- ============================================================================
-- Sub-query 4A: Single dish matching under 700 calories
SELECT 
    m.id,
    r.name AS restaurant_name,
    m.name AS item_name,
    m.category,
    m.price,
    m.calories,
    m.prep_time_minutes
FROM `menu_items` m
JOIN `restaurants` r ON m.restaurant_id = r.id
WHERE m.is_available = 1
  AND m.calories IS NOT NULL
  AND m.calories <= 700
ORDER BY m.calories ASC, m.price ASC;

-- Sub-query 4B: Combo pair matching a budget of <= 350 BDT
SELECT 
    r.name AS restaurant_name,
    m1.name AS dish_name,
    m1.price AS dish_price,
    m1.calories AS dish_calories,
    m2.name AS beverage_or_side,
    m2.price AS side_price,
    m2.calories AS side_calories,
    (m1.price + m2.price) AS combo_total_price,
    (COALESCE(m1.calories, 0) + COALESCE(m2.calories, 0)) AS combo_total_calories
FROM `menu_items` m1
JOIN `menu_items` m2 ON m1.restaurant_id = m2.restaurant_id AND m1.id < m2.id
JOIN `restaurants` r ON m1.restaurant_id = r.id
WHERE m1.category IN ('Burgers', 'Main Course', 'Salads', 'Grain Bowls')
  AND m2.category IN ('Beverages', 'Sides', 'Bread')
  AND (m1.price + m2.price) <= 350.00
ORDER BY combo_total_price ASC;

-- ============================================================================
-- QUERY 5: COMBINED DELIVERY BATCH TRACKING
-- Retrieves all order components grouped under a combined delivery batch,
-- showing assigned rider, individual restaurants, and consolidated savings.
-- ============================================================================
SELECT 
    o.combined_group_code,
    o.id AS order_id,
    u_cust.name AS customer_name,
    r.name AS restaurant_name,
    r.address AS restaurant_address,
    u_rider.name AS assigned_rider_name,
    o.status AS order_status,
    o.total_amount,
    o.delivery_fee,
    o.discount_amount,
    o.preferred_delivery_time,
    o.created_at
FROM `orders` o
JOIN `users` u_cust ON o.customer_id = u_cust.id
JOIN `restaurants` r ON o.restaurant_id = r.id
LEFT JOIN `riders` rd ON o.rider_id = rd.id
LEFT JOIN `users` u_rider ON rd.user_id = u_rider.id
WHERE o.is_combined_delivery = 1
ORDER BY o.combined_group_code, o.id ASC;

-- ============================================================================
-- QUERY 6: RESTAURANT DAILY SALES & PERFORMANCE REPORT
-- Summary of total revenue, order count, average order value, and average rating.
-- ============================================================================
SELECT 
    r.id AS restaurant_id,
    r.name AS restaurant_name,
    COUNT(o.id) AS total_orders_fulfilled,
    COALESCE(SUM(o.total_amount), 0.00) AS gross_revenue_bdt,
    COALESCE(AVG(o.total_amount), 0.00) AS avg_order_value_bdt,
    r.rating AS customer_rating
FROM `restaurants` r
LEFT JOIN `orders` o ON r.id = o.restaurant_id AND o.status = 'Delivered'
GROUP BY r.id, r.name, r.rating
ORDER BY gross_revenue_bdt DESC;

-- ============================================================================
-- QUERY 7: RIDER PERFORMANCE & ACTIVE DISPATCH METRICS
-- Lists riders, vehicle info, current delivery assignments, and rating.
-- ============================================================================
SELECT 
    rd.id AS rider_id,
    u.name AS rider_name,
    u.phone AS rider_phone,
    rd.vehicle_type,
    rd.current_status,
    rd.total_deliveries,
    rd.rating,
    COUNT(o.id) AS active_deliveries_in_progress
FROM `riders` rd
JOIN `users` u ON rd.user_id = u.id
LEFT JOIN `orders` o ON rd.id = o.rider_id AND o.status IN ('Accepted', 'Preparing', 'ReadyForPickup', 'OutForDelivery')
GROUP BY rd.id, u.name, u.phone, rd.vehicle_type, rd.current_status, rd.total_deliveries, rd.rating
ORDER BY rd.current_status ASC, rd.rating DESC;
