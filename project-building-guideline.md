# BiteNest — Project Building Guideline

A practical, step-by-step guide for turning the BiteNest proposal into a working system. This assumes the team is starting from an empty repository and follows the stack already committed to in the proposal: **ASP.NET Core Web API (.NET 8) + C#**, **Entity Framework Core**, **MySQL**, and **HTML5 + Tailwind CSS + JavaScript** on the frontend.

---

## 0. Before You Write Any Code

- [ ] Install .NET 8 SDK, MySQL Server (or MySQL Workbench + a local instance), Visual Studio 2022 or VS Code, Git, and Postman.
- [ ] Create the GitHub repository and agree on a branching model (recommended: `main` = stable, `dev` = integration, feature branches per module — e.g. `feature/auth`, `feature/order-flow`).
- [ ] Agree on naming conventions up front (PascalCase for C# classes/methods, camelCase for JS, snake_case or PascalCase for DB columns — pick one and document it) so four people's code doesn't look like four different projects.
- [ ] Write down the non-functional targets mentioned in the recommendations doc (expected concurrent users, response time target) — even rough numbers — so later decisions (caching, indexing) have something to be judged against.

---

## 1. Build Order — Why This Sequence

Build **bottom-up**: database → backend core → business logic → frontend. Each layer depends on the one below it, so building top-down means mocking data you'll throw away later. The six phases below map directly onto the proposal's own module list.

```
Phase 1: Database schema
Phase 2: Auth & user roles
Phase 3: Core CRUD (Restaurants, Menu, Orders)
Phase 4: Innovative features (Queue Status, Combined Delivery, Meal Planner, Leftover Discount)
Phase 5: Notifications & Rider workflow
Phase 6: Frontend integration + Admin dashboard
```

---

## Phase 1 — Database Schema

Start here, not with controllers. Every table below comes from the System Architecture diagram in the proposal.

| Table | Key columns to include | Notes |
|---|---|---|
| `Users` | Id, Name, Email, PasswordHash, Role (enum: Customer/Restaurant/Rider/Admin), Phone, CreatedAt | One table with a `Role` discriminator is simpler than four separate tables for a project this size. |
| `Restaurants` | Id, OwnerUserId (FK → Users), Name, Location/Lat/Lng, Category, IsVerified | `IsVerified` supports the Admin "User & Restaurant Management" feature. |
| `MenuItems` | Id, RestaurantId (FK), Name, Price, Category, IsAvailable | |
| `Orders` | Id, CustomerId (FK), RestaurantId (FK), RiderId (FK, nullable), Status, PreferredDeliveryTime, IsCombinedDelivery, CreatedAt | `Status` should be an enum with a defined state machine (see Phase 3). |
| `OrderItems` | Id, OrderId (FK), MenuItemId (FK), Quantity, UnitPrice | |
| `Payments` | Id, OrderId (FK), Amount, Method, Status, PaidAt | Decide early: real gateway or mock (see Recommendation #6 from the project explanation doc). |
| `Riders` | Id, UserId (FK), CurrentStatus (Available/OnDelivery/Offline), VehicleInfo | |
| `Reviews` | Id, CustomerId (FK), RestaurantId (FK), Rating, Comment, CreatedAt | |
| `MealPlans` | Id, CustomerId (FK), Goal (calorie/budget), SuggestedItems (JSON or join table) | Unique to BiteNest — give this real design attention. |
| `LeftoverFoodOffers` | Id, RestaurantId (FK), MenuItemId (FK), DiscountPercent, ExpiresAt | Unique to BiteNest — `ExpiresAt` matters for hiding stale offers. |

**Steps:**
1. Model these as C# classes in a `Models/` folder.
2. Set up `ApplicationDbContext : DbContext` with a `DbSet<T>` per table.
3. Configure the MySQL connection string in `appsettings.json` (use `Pomelo.EntityFrameworkCore.MySql` as the EF Core provider).
4. Run your first migration: `dotnet ef migrations add InitialCreate`, then `dotnet ef database update`.
5. Seed a handful of test rows (a few users of each role, 2–3 restaurants, some menu items) so you're not testing against an empty database.

---

## Phase 2 — Authentication & Authorization

This unblocks everything else, since every other endpoint needs to know *who* is calling it.

1. Implement registration/login endpoints (`POST /api/auth/register`, `POST /api/auth/login`).
2. Hash passwords (use `BCrypt.Net` or ASP.NET Core Identity's built-in hasher — don't write your own).
3. Issue JWT tokens on login, containing the user's Id and Role.
4. Add `[Authorize(Roles = "Restaurant")]`-style attributes to lock down endpoints by role — this is what physically enforces "a Rider can't see a Restaurant's dashboard."
5. Test with Postman: confirm a Customer token gets a 403 on a Restaurant-only endpoint before writing any more business logic.

---

## Phase 3 — Core CRUD: Restaurants, Menus, Orders

This is standard e-commerce plumbing — get it solid before touching the innovative features, since they all build on top of it.

1. **Restaurant & Menu endpoints:** create/read/update restaurants and menu items (Restaurant role), plus public search/browse endpoints (Customer role) with filtering by category and location.
2. **Order state machine** — define this explicitly before coding it:
   ```
   Placed → Accepted → Preparing → ReadyForPickup → OutForDelivery → Delivered
                                                              ↘ Cancelled
   ```
   Store this as an enum, and only allow valid transitions (e.g., don't let an order jump from `Placed` to `Delivered`).
3. **Cart & Checkout:** build this client-side first (cart in browser state/localStorage-equivalent) and only hit the API on checkout to create the `Order` + `OrderItems` rows.
4. **Payment:** implement whichever approach you decided on in Phase 0 — mock is fine for a course project, just be explicit about it in your documentation.

Test each endpoint in Postman/Swagger as you build it — don't wait until the frontend exists to discover a broken endpoint.

---

## Phase 4 — The Innovative Features (the hard part)

Budget the most time here — this is what the benchmark analysis says differentiates BiteNest, and it's also the most algorithmically involved part of the system.

### Smart Queue Status
- Define the formula concretely, e.g.: `Free` if active orders < 5, `Normal` if 5–15, `Busy` if >15 (adjust thresholds per restaurant capacity if you want it more realistic).
- Compute this on-demand from the `Orders` table (count of orders in `Preparing`/`Accepted` status for that restaurant) rather than storing it as a separate field that can go stale.

### Smart Multi-Restaurant Combined Delivery
- This is the most complex feature. Break it into sub-steps:
  1. When a customer adds items from a second nearby restaurant, check distance between the two restaurants (a simple straight-line/Haversine distance check is enough for a course project — you don't need real routing APIs unless you want to).
  2. If within a threshold (e.g., 2km), offer "combine into one delivery."
  3. On order creation, link both restaurant orders to a single `Order` record (or a `DeliveryGroup` table if you want cleaner separation) with one `RiderId`.
- Write this as its own service class (`ICombinedDeliveryService`) so it's testable independent of the controller.

### Smart Delivery Recommendation
- This can start as a simple rule-based decision (distance + combined prep time + rider availability) rather than anything ML-based — "smart" doesn't have to mean machine learning for this to satisfy the proposal.

### Advanced Meal Planner
- Start simple: filter menu items by calorie/price tags and suggest combinations that fit a stated budget or calorie target. This can be a straightforward filtering/sorting query — resist the urge to over-engineer this into a recommendation engine unless you have time left over.

### Leftover Discount
- Restaurant-side endpoint to mark a `MenuItem` as a `LeftoverFoodOffer` with a discount and expiry.
- Customer-side: surface these prominently (a dedicated "Leftover Deals" section), and make sure expired offers are filtered out of every query rather than just hidden in the UI.

---

## Phase 5 — Notifications & Rider Workflow

1. Rider endpoints: view assigned orders, update delivery status, view delivery history.
2. Notification service: start with in-app notifications (a `Notifications` table polled by the frontend) before adding email/SMS — real SMS/email integration (Twilio, SendGrid) can be added later without changing the rest of the architecture.
3. Wire status changes to notifications: when an `Order` status changes, insert a notification row for the relevant Customer and/or Rider.

---

## Phase 6 — Frontend & Admin Dashboard

1. Build the Customer-facing pages first (search, cart, checkout, order tracking) — this is what a grader/user will interact with most.
2. Build the Restaurant dashboard (menu management, incoming orders, sales report).
3. Build the Rider view (assigned orders, status update buttons).
4. Build the Admin dashboard last — it's the least complex functionally (mostly tables + approve/reject buttons) and least urgent to demo.
5. Use the Site Map from the proposal as your page checklist — walk through it and confirm every listed page actually exists and links correctly.

---

## Testing Checklist (do this continuously, not at the end)

- [ ] Unit test the Smart Queue Status calculation and the Combined Delivery distance logic specifically — these are the parts most likely to have edge-case bugs (e.g., what happens at exactly the threshold distance?).
- [ ] Test role-based access control by attempting cross-role requests (Rider hitting a Restaurant endpoint, etc.) and confirming rejection.
- [ ] Test the order state machine against invalid transitions.
- [ ] Manually test the full "combined delivery" flow end-to-end at least once a week during Phase 4 — it's the feature most likely to silently break as other code changes.

---

## Suggested Timeline (adjust to your semester length)

| Weeks | Focus |
|---|---|
| 1 | Setup, schema design, migrations, seed data |
| 2 | Auth & role-based access |
| 3–4 | Core CRUD: restaurants, menus, orders, cart/checkout |
| 5–6 | Innovative features (Queue Status, Combined Delivery, Meal Planner, Leftover Discount) |
| 7 | Notifications, Rider workflow |
| 8 | Frontend build-out across all four roles |
| 9 | Integration testing, bug fixing |
| 10 | Polish, documentation, demo prep |

---

## Definition of Done (per feature)

Before marking any feature complete, confirm:
- [ ] Endpoint(s) tested in Postman with valid and invalid input.
- [ ] Role restrictions enforced and tested.
- [ ] Frontend calls the real endpoint (not mock data).
- [ ] Edge cases considered (empty cart, restaurant with zero menu items, expired leftover offer, etc.).
- [ ] Committed with a clear message and merged via pull request, not pushed directly to `main`.