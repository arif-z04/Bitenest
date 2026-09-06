# BiteNest — Project Breakdown & Technical Explanation

**System:** Smart Food Delivery & Restaurant Management System
**Course:** CIT-222
**Purpose of this document:** a plain-language walkthrough of every component in the BiteNest proposal — what it does, why it's there, and how it connects to the rest of the system — followed by recommendations for moving from proposal to implementation.

---

## 1. Problem Statement — *what BiteNest is solving*

Existing food delivery platforms (Foodpanda, Pathao Food, Uber Eats) handle basic ordering and delivery well, but they fall short in three specific ways:

- **No workload visibility** — customers can't tell if a restaurant is swamped or free, so delivery-time estimates are unreliable.
- **No multi-restaurant efficiency** — ordering from two nearby restaurants means two separate deliveries and two delivery fees.
- **No waste/cost tools** — no meal planning support, and no mechanism for restaurants to discount food that would otherwise go to waste.

BiteNest's core pitch is to fix these three gaps specifically, rather than being "another delivery app."

## 2. Objectives — *the measurable goals*

The objectives translate the problem statement into concrete deliverables: a secure ordering platform, location-based restaurant search, a restaurant-side operations dashboard, an organized rider workflow, and the three signature features (Smart Queue Status, Smart Multi-Restaurant Combined Delivery, Meal Planner/Leftover Discount). Everything else in the system exists in service of these objectives.

## 3. Feasibility Study — *can this actually be built and run?*

| Dimension | What it tells you |
|---|---|
| **Technical** | The proposed stack (ASP.NET Core Web API on .NET 8, C#, Entity Framework Core, MySQL, HTML5/CSS3/Bootstrap/JavaScript) is mature, well-documented, and appropriate for a system of this scope. Nothing here is exotic or unproven. |
| **Economic** | Every tool listed (Visual Studio Community, VS Code, MySQL, GitHub, Postman, Swagger) is free. Realistic cost during development is effectively $0 in licensing; the only future cost is hosting. |
| **Operational** | Four separate dashboards (Customer, Restaurant, Rider, Admin) keep each user type focused on only what's relevant to them, which reduces training/onboarding friction. |

**Note:** section 4, "Information Gathering," is listed in the proposal's structure but has no content — this is a gap in the source document, not something resolved here.

## 5. Benchmark Analysis — *how BiteNest differentiates itself*

The comparison against Foodpanda, Pathao Food, and Uber Eats isn't just marketing — it maps directly to the feature list in Section 6. Each competitor gap becomes a named feature:

- Foodpanda's separate-delivery problem → **Smart Multi-Restaurant Combined Delivery**
- Pathao's lack of workload visibility → **Smart Queue Status**
- Uber Eats' limited delivery-time control → **Customer Preferred Delivery Time**

This is a good sign for the proposal: the features aren't arbitrary, they're each answering a specific, named shortcoming of an existing competitor.

## 6. Proposed Features — *the functional modules*

### Customer Module
Registration/login, location-based restaurant search, category browsing, cart & checkout, order tracking/history, and ratings & reviews. This is the standard customer-facing feature set any delivery app needs.

### Restaurant Module
A dashboard for menu management, incoming order management, inventory tracking, and sales reporting. This is where a restaurant owner spends their day-to-day time.

### Delivery Rider Module
Order assignment, delivery status updates, and delivery history — deliberately minimal, since riders are mostly on mobile and need speed over feature depth.

### Administrator Module
User/restaurant management, category management, rider management, and reporting/analytics — the oversight layer that keeps the other three modules honest (verifying restaurants, resolving disputes, monitoring platform health).

### Innovative Features (the differentiators)
- **Smart Queue Status** — restaurant availability shown as Free / Normal / Busy, computed from active orders and prep time.
- **Customer Preferred Delivery Time** — customers pick a delivery window; the system validates feasibility against restaurant workload and rider availability.
- **Smart Multi-Restaurant Combined Delivery** — up to two nearby restaurants combined into a single delivery run.
- **Smart Delivery Recommendation** — the system decides whether combining orders makes sense based on distance, prep time, and rider availability.
- **Advanced Meal Planner** — suggests meals based on preferences like calorie count or budget.
- **Leftover Discount** — restaurants can discount near-end-of-day surplus food.

These six features are the actual intellectual contribution of the project — the rest of the system is the necessary scaffolding to support them.

## 7. Site Map — *how a user moves through the app*

The site map shows the navigation hierarchy: entry point → authentication → role-based routing into Customer / Restaurant / Rider dashboards, each with their own sub-pages (Profile, Restaurants, Cart, Deliveries, etc.). This is a UX-level document, not a technical one — it exists to confirm no user role has a broken or dead-end path through the app before development starts.

## 8. System Requirements

**Hardware:** an ordinary development machine (Core i5, 8GB RAM, 256GB SSD) — nothing GPU-bound or resource-heavy, which tracks with a CRUD-style web application.

**Software:** ASP.NET Core (.NET 8) + C# backend, HTML5/Tailwind CSS/JavaScript frontend, MySQL database, with Visual Studio, Git/GitHub, and Postman/Swagger for development and API testing.

## 9. System Architecture — *how the pieces fit together*

This is the most important diagram in the proposal because it's the only place all the layers are shown together. Reading it top to bottom:

1. **Actors** — Customer, Restaurant Owner, Delivery Rider, Administrator. Four distinct entry points into the same system.
2. **Web Browser layer** — HTML5 + Tailwind CSS + JavaScript. This is a traditional server-rendered or API-consuming frontend, not a separate SPA framework (no React/Vue/Angular mentioned) — worth confirming that's intentional.
3. **ASP.NET Core Web API (.NET 8) layer** — the application's brain, split into three concerns:
   - **Authentication & Authorization** — login/register, role management, access control. This is what keeps a Rider from seeing a Restaurant's dashboard, for example.
   - **Business Logic** — order management, the Smart Multi-Restaurant Delivery logic, Meal Planner, Queue Status calculation, and payment processing. This is where the six "innovative features" actually get implemented.
   - **Notification Service** — email/SMS, order updates, push notifications. This is what keeps a Customer or Rider informed without needing to refresh the app.
4. **Entity Framework Core (ORM)** — the translation layer between C# objects and MySQL rows. This is what lets the business logic layer work with objects instead of writing raw SQL everywhere.
5. **MySQL Database** — the tables listed are Users, Restaurants, Menu Items, Orders, Order Items, Payments, Riders, Reviews, Meal Plans, and Leftover Food Offers. This is a fairly standard normalized e-commerce/marketplace schema: Users/Restaurants/Riders as core entities, Orders/Order Items as the transactional core, and Meal Plans/Leftover Food Offers as the tables specific to BiteNest's differentiating features.

## Conclusion (from the original proposal)

BiteNest's stated conclusion is that the combination of Smart Multi-Restaurant Delivery, Meal Planner, Queue Status, Preferred Delivery Scheduling, and Leftover Discounts — built on a secure, scalable, open-source stack — makes it a practical, cost-efficient solution for all four user types.

---

## Recommendations

These are my own observations on strengthening the project going from proposal to build:

### Fill the gaps in the proposal itself
1. **Write the "Information Gathering" section.** It's currently a heading with no content. Even a short description of how requirements were validated (user interviews, competitor research, a survey) would round out the document and is normally expected in a System Analysis and Design deliverable.
2. **Add a data flow diagram (DFD) or ER diagram.** The System Architecture diagram shows *components*; it doesn't show how data actually moves through a request (e.g., what happens, step by step, when a customer places a combined multi-restaurant order). A Level-1 DFD for the "place order" and "combined delivery" flows would make the innovative features easier to evaluate and implement correctly.

### Technical considerations worth deciding early
3. **Clarify the frontend approach.** Plain HTML5/CSS/JavaScript is fine for a course project, but if the Customer Module needs live queue-status updates or real-time order tracking, consider whether you want polling, WebSockets/SignalR, or a lightweight frontend framework — this decision affects the API design, so it's better made before backend work starts than retrofitted later.
4. **Design the "combined delivery" logic carefully — it's the hardest part of the system.** Smart Multi-Restaurant Combined Delivery and Smart Delivery Recommendation both require geographic proximity calculations, prep-time estimation, and rider routing. This is meaningfully harder than standard CRUD order management — budget disproportionate design and testing time here, since it's also the project's main differentiator and likely where a grader/reviewer will focus.
5. **Define Smart Queue Status precisely.** "Free / Normal / Busy" needs an actual formula (e.g., active-order count vs. average prep time vs. kitchen capacity) documented somewhere — right now it's described only at the feature level, not the algorithm level.
6. **Plan the payment flow.** "Payment Processing" appears in the architecture diagram but isn't detailed anywhere — decide early whether this is a real payment gateway integration (bKash/Nagad/Stripe) or a simulated/mock flow for the course scope, since that affects both the database schema and security requirements (PCI-adjacent handling if real).
7. **Add basic non-functional requirements.** Expected number of concurrent users, response-time targets, and uptime expectations aren't mentioned anywhere in the proposal. Even rough targets ("support ~200 concurrent users," "API responses under 500ms") strengthen the feasibility study and give you something to test against later.

### Database-specific notes
8. **Confirm the Meal Plans and Leftover Food Offers tables have clear foreign-key relationships** to Restaurants and Menu Items — these are the two tables unique to BiteNest, so their schema design deserves more attention than the standard Users/Orders/Payments tables, which are largely boilerplate.
9. **Consider a status/history table for Orders** (or a status field with an audit trail) since the Rider module needs "Delivery Status Update" and the Customer module needs "Order Tracking" — both depend on order state transitions being recorded, not just the final state.

