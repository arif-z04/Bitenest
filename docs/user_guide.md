# BiteNest — User Manual & Operational Workflows

This guide explains how each of the four system actors uses BiteNest.

---

## 1. Customer Workflow

1. **Discovery & Smart Queue Monitoring**:
   - Open the web application.
   - Restaurants display live queue badges:
     - 🟢 **Free**: Kitchen is free; instant preparation.
     - 🟡 **Normal**: Moderate volume; standard prep duration.
     - 🔴 **Busy**: High order volume; longer wait time.
2. **Placing a Smart Multi-Restaurant Combined Delivery Order**:
   - Add a dish from Restaurant A (e.g. *SpiceCraft Kitchen*).
   - Add a dish or drink from nearby Restaurant B (e.g. *Burger Barn & Grill*, 0.46 km away).
   - Open cart: The system automatically detects eligible proximity and applies **Smart Combined Delivery** (Total delivery fee = BDT 60, saving BDT 20).
   - Select optional **Customer Preferred Delivery Time**.
   - Select Payment method (`bKash`, `Nagad`, `Card`, or `Cash on Delivery`).
   - Submit order.
3. **Tracking & Review**:
   - Navigate to **My Orders & Tracker**.
   - Follow the visual timeline: `Placed` $\to$ `Accepted` $\to$ `Preparing` $\to$ `Ready For Pickup` $\to$ `Out For Delivery` $\to$ `Delivered`.
4. **Using the Advanced Meal Planner**:
   - Click **Meal Planner** tab.
   - Pick **Calorie Target** (e.g. 650 kcal) or **Budget Target** (e.g. BDT 350).
   - Click *Generate Smart Meal Recommendations*.
   - Click *Add Combo to Cart* to order directly.
5. **Browsing Leftover Flash Deals**:
   - Click **Leftover Deals** tab.
   - View surplus food discounted up to 50% with live countdown clocks.
   - Click *Claim Surplus Deal* before expiration.

---

## 2. Restaurant Owner Workflow

1. **Active Kitchen Management**:
   - Select role `RestaurantOwner` (e.g. *SpiceCraft Manager*).
   - The dashboard displays kitchen load, queue status, active orders count, and daily revenue.
2. **Order Lifecycle Execution**:
   - Incoming orders arrive in real time.
   - Click `Accept Order` $\to$ `Start Preparing` $\to$ `Mark Ready For Pickup`.
   - The status change automatically notifies the customer and makes the order visible to riders.
3. **Publishing a Leftover Flash Deal**:
   - Click `Post Leftover Deal`.
   - Select surplus menu item, discount percentage (25%, 35%, 50%), portions available, and expiry hours.
   - Deal is published immediately to the customer deals marketplace.

---

## 3. Delivery Rider Workflow

1. **Availability Toggle**:
   - Select role `DeliveryRider` (e.g. *Tanvir Hasan*).
   - Toggle status to `Available`.
2. **Order Pickup & Batch Delivery**:
   - Review assigned orders. Combined orders display multi-stop pickup markers:
     - Stop 1: Restaurant A $\to$ Stop 2: Restaurant B $\to$ Dropoff: Customer.
   - Click `Confirm Pick Up & Start Delivery` $\to$ Click `Mark Order Delivered`.
   - Delivery history and earnings are updated instantaneously.

---

## 4. Administrator Workflow

1. **Platform Oversight**:
   - Switch role to `Administrator`.
   - Review total platform Gross Merchandise Value (GMV), orders count, active restaurants, and users.
2. **Restaurant Verification**:
   - Inspect registered restaurants and toggle verification status (`Verify` / `Unverify`).
3. **User Management**:
   - Audit registered user profiles across all roles.
