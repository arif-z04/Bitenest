# BiteNest — RESTful API Specification

The BiteNest backend exposes a standard REST API documented via OpenAPI / Swagger.

Base URL: `http://localhost:5000/api`

---

## 1. Authentication Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register new user (Customer, RestaurantOwner, DeliveryRider, Administrator) |
| `POST` | `/api/auth/login` | Public | Authenticate user and receive JWT Bearer token |
| `GET` | `/api/auth/me` | Authenticated | Retrieve current user profile and claims |

---

## 2. Restaurant Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `GET` | `/api/restaurants` | Public | List verified restaurants enriched with live Queue Status (`Free`, `Normal`, `Busy`) |
| `GET` | `/api/restaurants/{id}` | Public | Get restaurant details, full menu catalog, and active wait estimate |
| `GET` | `/api/restaurants/{id}/queue-status` | Public | Real-time queue metrics and estimated wait duration |
| `POST` | `/api/restaurants` | RestaurantOwner/Admin | Create and register a new restaurant |

---

## 3. Orders & Combined Delivery Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `GET` | `/api/orders` | Authenticated | List orders filtered by customer, restaurant, or rider |
| `GET` | `/api/orders/{id}` | Authenticated | Get full order breakdown, items, payment, and delivery status |
| `POST` | `/api/orders` | Customer | Place a standard single-restaurant order |
| `POST` | `/api/orders/combined` | Customer | Place a Smart Multi-Restaurant Combined Delivery order (2 nearby restaurants, 1 rider, BDT 20 savings) |
| `PUT` | `/api/orders/{id}/status` | Restaurant/Rider | Transition order status (`Accepted`, `Preparing`, `ReadyForPickup`, `OutForDelivery`, `Delivered`) |
| `POST` | `/api/orders/validate-preferred-time`| Customer | Validate feasibility of customer preferred delivery time |

---

## 4. Innovative Feature Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `GET` | `/api/combined-delivery/check-eligibility` | Public | Check if two restaurants are within $\le 2.0\text{ km}$ threshold via Haversine formula |
| `GET` | `/api/combined-delivery/nearby-partners/{id}` | Public | List nearby restaurants eligible for combined delivery |
| `POST` | `/api/meal-planner/recommend` | Public | Generate calorie or budget-optimized single dishes and combos |
| `POST` | `/api/meal-planner/save` | Customer | Save a personalized meal plan |
| `GET` | `/api/leftover-offers/active` | Public | List active, non-expired surplus food flash deals with remaining countdown |
| `POST` | `/api/leftover-offers` | RestaurantOwner | Publish a new surplus leftover discount deal |

---

## 5. Rider & Operations Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `GET` | `/api/riders/available-orders` | DeliveryRider | View orders ready for pickup across restaurants |
| `POST` | `/api/riders/assign/{orderId}` | DeliveryRider | Claim order / batch assignment for delivery |
| `PUT` | `/api/riders/status` | DeliveryRider | Toggle availability (`Available`, `OnDelivery`, `Offline`) |
| `GET` | `/api/riders/my-deliveries` | DeliveryRider | View assigned delivery route and earnings history |

---

## 6. Admin Endpoints

| Method | Path | Role | Description |
|---|---|---|---|
| `GET` | `/api/admin/metrics` | Administrator | Platform KPIs: GMV, total orders, active restaurants, riders |
| `PUT` | `/api/admin/verify-restaurant/{id}`| Administrator | Approve or unverify a restaurant |
| `GET` | `/api/admin/users` | Administrator | List all registered platform accounts |
