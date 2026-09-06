# BiteNest — Setup & Deployment Guide

This document provides instructions for running and deploying the BiteNest system.

---

## 1. Prerequisites

- **.NET SDK**: .NET 8.0 / .NET 10.0 (`dotnet --version`)
- **Database**: MySQL 8.0+ / MariaDB 10.5+ (Optional: in-memory provider runs automatically without MySQL setup for development)
- **Web Browser**: Google Chrome, Mozilla Firefox, or Microsoft Edge
- **Git**

---

## 2. Database Setup (MySQL)

If running with MySQL Server:
1. Start your local MySQL service (or run via Docker):
   ```bash
   docker run -d --name bitenest-mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=root mysql:8.0
   ```
2. Execute the database initialization scripts in sequence:
   ```bash
   mysql -u root -p < sql/01_create_database_and_tables.sql
   mysql -u root -p < sql/02_seed_dummy_data.sql
   ```
3. Test sample queries:
   ```bash
   mysql -u root -p < sql/03_sample_queries.sql
   ```

---

## 3. Backend API Execution

1. Open a terminal in the `backend/` directory:
   ```bash
   cd backend
   dotnet restore
   dotnet build
   dotnet run
   ```
2. The API will start on `http://localhost:5000` (or `https://localhost:5001`).
3. View interactive Swagger UI documentation at:
   `http://localhost:5000/swagger`

---

## 4. Frontend Web App Execution

The frontend is an optimized responsive web application. You can run it with any lightweight HTTP server or open directly:

1. Using Python:
   ```bash
   cd frontend
   python3 -m http.server 3000
   ```
   Open `http://localhost:3000` in your browser.
2. Or using Node.js `npx serve`:
   ```bash
   npx serve frontend -p 3000
   ```
3. Or directly double-click `frontend/index.html` in your file browser.

---

## 5. Seed Accounts for Testing

| Role | Email | Password | Pre-configured Context |
|---|---|---|---|
| **Customer** | `customer.rahim@bitenest.com` | `Password@123` | Rahim Ahmed (Dhanmondi, Dhaka) |
| **Customer** | `customer.fatima@bitenest.com` | `Password@123` | Fatima Jahan (Has active combined delivery) |
| **Restaurant Owner** | `owner.spicecraft@bitenest.com` | `Password@123` | SpiceCraft Kitchen |
| **Restaurant Owner** | `owner.burgerbarn@bitenest.com` | `Password@123` | Burger Barn & Grill |
| **Restaurant Owner** | `owner.greenbowls@bitenest.com` | `Password@123` | Green Bowls & Juices |
| **Delivery Rider** | `rider.tanvir@bitenest.com` | `Password@123` | Tanvir Hasan (Motorcycle, 142 deliveries) |
| **Delivery Rider** | `rider.sumon@bitenest.com` | `Password@123` | Sumon Barua (Motorcycle) |
| **Administrator** | `admin@bitenest.com` | `Password@123` | System Administrator |
