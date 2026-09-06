# BiteNest — System Architecture & Technical Specifications

**Project:** Smart Food Delivery & Restaurant Management System  
**Course:** CIT-222  
**Tech Stack:** ASP.NET Core Web API (.NET 8/10), C#, Entity Framework Core, MySQL, HTML5, CSS3, Tailwind CSS, JavaScript, Material Design 3.

---

## 1. Architectural Overview

BiteNest follows a modern, scalable multi-tier web application architecture designed to overcome key operational shortcomings of legacy platforms like Foodpanda, Pathao Food, and Uber Eats.

```
+-------------------------------------------------------------------------+
|                               ACTORS                                    |
|   [Customer]    [Restaurant Owner]    [Delivery Rider]   [Administrator]|
+------------------------------------+------------------------------------+
                                     |
                                     v
+-------------------------------------------------------------------------+
|                           WEB FRONTEND (SPA)                            |
|             HTML5 • Tailwind CSS • JavaScript • Material UI             |
|  - Real-time Queue Badges    - Multi-Restaurant Cart   - Meal Planner   |
|  - Live Status Stepper       - Kitchen Order Stream    - Rider Dispatch |
+------------------------------------+------------------------------------+
                                     |  RESTful JSON / HTTPS / JWT
                                     v
+-------------------------------------------------------------------------+
|                  ASP.NET CORE WEB API (.NET 8 / 10)                     |
|                                                                         |
|  [Authentication & AuthZ]      [Business Logic Engine]                  |
|  - JWT Bearer Tokens           - Smart Queue Status (Free/Normal/Busy)  |
|  - Role Claims Enforcement     - Multi-Restaurant Combined Delivery     |
|  - BCrypt Password Hashes      - Advanced Meal Planner (Calorie/Budget) |
|                                - Customer Preferred Time Validator      |
|  [Notification Service]        - Leftover Flash Deals Engine            |
|  - Real-time In-App Dispatch   - Order Lifecycle State Machine          |
+------------------------------------+------------------------------------+
                                     |
                                     v
+-------------------------------------------------------------------------+
|                      ENTITY FRAMEWORK CORE (ORM)                        |
|  - ApplicationDbContext with Pomelo MySQL Provider                      |
|  - Relational Mapping, Fluent API Configurations, Cascade Controls      |
+------------------------------------+------------------------------------+
                                     |
                                     v
+-------------------------------------------------------------------------+
|                            MYSQL DATABASE                               |
|  - Users             - Restaurants         - MenuItems                  |
|  - Riders            - Orders              - OrderItems                 |
|  - Payments          - Reviews             - MealPlans                  |
|  - LeftoverOffers    - Notifications                                    |
+-------------------------------------------------------------------------+
```

---

## 2. Innovative Features & Mathematical Models

### 2.1 Smart Queue Status
Legacy delivery systems present static estimated delivery times regardless of kitchen congestion. BiteNest calculates active kitchen load dynamically:

$$\text{Active Orders} = \sum [\text{Status} \in \{\text{Placed}, \text{Accepted}, \text{Preparing}\}]$$

$$\text{Queue Status} = 
\begin{cases} 
\text{Free} & \text{if } \text{Active Orders} < \frac{\text{Kitchen Capacity}}{3} \\
\text{Normal} & \text{if } \frac{\text{Kitchen Capacity}}{3} \le \text{Active Orders} < \text{Kitchen Capacity} \\
\text{Busy} & \text{if } \text{Active Orders} \ge \text{Kitchen Capacity}
\end{cases}$$

$$\text{Estimated Wait Time (mins)} = \text{AvgPrepTime} + (\text{Active Orders} \times 3)$$

### 2.2 Smart Multi-Restaurant Combined Delivery
Allows customers to order from up to two nearby restaurants in a single checkout, serviced by a single rider.
1. **Haversine Distance Check**:
   $$d = 2R \arcsin\left(\sqrt{\sin^2\left(\frac{\Delta \text{lat}}{2}\right) + \cos(\text{lat}_1)\cos(\text{lat}_2)\sin^2\left(\frac{\Delta \text{lon}}{2}\right)}\right)$$
2. **Eligibility Threshold**: If $d \le 2.0\text{ km}$, combined delivery is authorized.
3. **Optimized Delivery Fee**:
   $$\text{Separate Deliveries} = 2 \times 40 = 80\text{ BDT}$$
   $$\text{Combined Delivery} = 60\text{ BDT (30 BDT per order component)}$$
   $$\text{Customer Savings} = 20\text{ BDT}$$

### 2.3 Customer Preferred Delivery Time
Customers can schedule deliveries within a window of $[30\text{ mins}, 12\text{ hours}]$ in advance. The system validates the slot against estimated preparation duration and rider availability.

### 2.4 Advanced Meal Planner
A recommendation engine matching customer caloric or budget requirements:
- **Calorie Mode**: Matches dishes or combo pairs (Main + Beverage/Side) satisfying $| \text{Calories} - \text{Target} | \le 0.15 \times \text{Target}$.
- **Budget Mode**: Suggests optimal meal combos such that $\sum \text{Price} \le \text{Budget Target}$.

### 2.5 Leftover Food Flash Discount
Surplus meals near the end of shift are discounted (25% - 50% OFF) with real-time countdown clocks, encouraging zero food waste and lowering meal costs. Expired offers ($\text{ExpiresAt} \le \text{NOW()}$) are automatically hidden from queries.

---

## 3. Order State Machine Transitions

```
[Placed] ---> [Accepted] ---> [Preparing] ---> [ReadyForPickup] ---> [OutForDelivery] ---> [Delivered]
    |               |
    v               v
[Cancelled]    [Cancelled]
```

Every state transition triggers an automated in-app notification to the customer and updates rider route dispatching.
