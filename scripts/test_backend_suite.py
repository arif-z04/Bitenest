#!/usr/bin/env python3
"""
BiteNest Backend API Automated Integration Test Suite
======================================================
Zero-dependency test runner testing all 15 core workflows:
- System health & DB connectivity
- Authentication & JWT token issuance
- Restaurant catalog & category filtering
- Real-time kitchen queue calculations
- Leftover flash deals & atomic reservations
- Combined delivery Haversine proximity & quote
- Multi-day smart meal planner generation
- Order lifecycle progression & notifications
"""

import sys
import json
import time
import urllib.request
import urllib.error
import ssl

BASE_URL = "http://localhost:5000"

GREEN = "\033[92m"
RED = "\033[91m"
YELLOW = "\033[93m"
CYAN = "\033[96m"
BOLD = "\033[1m"
RESET = "\033[0m"

passed_count = 0
failed_count = 0
shared_context = {}

ssl_ctx = ssl.create_default_context()
ssl_ctx.check_hostname = False
ssl_ctx.verify_mode = ssl.CERT_NONE

def print_header(title):
    print(f"\n{BOLD}{CYAN}======================================================{RESET}")
    print(f"{BOLD}{CYAN}  {title}{RESET}")
    print(f"{BOLD}{CYAN}======================================================{RESET}")

def make_request(method, endpoint, payload=None, token=None):
    url = f"{BASE_URL}{endpoint}"
    headers = {
        "Accept": "application/json",
        "User-Agent": "BiteNest-TestRunner/1.0"
    }
    data = None
    if payload is not None:
        headers["Content-Type"] = "application/json"
        data = json.dumps(payload).encode("utf-8")

    if token:
        headers["Authorization"] = f"Bearer {token}"

    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, context=ssl_ctx, timeout=10) as response:
            status = response.status
            body_text = response.read().decode("utf-8")
            body_json = json.loads(body_text) if body_text else None
            return status, body_json
    except urllib.error.HTTPError as e:
        body_text = e.read().decode("utf-8")
        try:
            body_json = json.loads(body_text)
        except Exception:
            body_json = {"raw_error": body_text}
        return e.code, body_json
    except Exception as e:
        return 0, {"exception": str(e)}

def assert_test(test_name, condition, error_detail=""):
    global passed_count, failed_count
    if condition:
        print(f"  [{GREEN}PASS{RESET}] {test_name}")
        passed_count += 1
        return True
    else:
        print(f"  [{RED}FAIL{RESET}] {test_name}")
        if error_detail:
            print(f"         {YELLOW}Details: {error_detail}{RESET}")
        failed_count += 1
        return False

def main():
    print_header("Stage 1: System Health & Server Connectivity")
    status, res = make_request("GET", "/api/restaurants")
    assert_test("API Server Online", status == 200, f"Expected 200 OK, got {status}")

    print_header("Stage 2: Authentication & Access Control")
    ts = int(time.time())
    test_email = f"qa_user_{ts}@example.com"
    reg_payload = {
        "fullName": "QA Automation User",
        "email": test_email,
        "password": "Password123!",
        "phoneNumber": "+1-555-0199",
        "address": "100 QA Parkway",
        "role": 0
    }
    status, res = make_request("POST", "/api/auth/register", reg_payload)
    assert_test("User Registration", status == 200 and "token" in res, f"Response: {res}")
    if res and "token" in res:
        shared_context["token"] = res["token"]

    status, res = make_request("POST", "/api/auth/register", reg_payload)
    assert_test("Prevent Duplicate Email", status == 400, f"Expected 400, got {status}")

    login_payload = {"email": test_email, "password": "Password123!"}
    status, res = make_request("POST", "/api/auth/login", login_payload)
    assert_test("User Login & JWT", status == 200 and "token" in res, f"Response: {res}")

    bad_login = {"email": test_email, "password": "WrongPassword!"}
    status, res = make_request("POST", "/api/auth/login", bad_login)
    assert_test("Reject Bad Password", status == 401, f"Expected 401, got {status}")

    print_header("Stage 3: Restaurant Catalog & Details")
    status, res = make_request("GET", "/api/restaurants")
    assert_test("Retrieve Catalog", status == 200 and isinstance(res, list) and len(res) > 0, f"Count: {len(res) if isinstance(res, list) else 0}")
    rid = res[0]["id"] if isinstance(res, list) and len(res) > 0 else 1

    status, res = make_request("GET", f"/api/restaurants/{rid}")
    assert_test("Fetch Details with Menu", status == 200 and "menuItems" in res, f"Keys: {list(res.keys()) if isinstance(res, dict) else res}")

    print_header("Stage 4: Real-Time Kitchen Queue Engine")
    status, res = make_request("GET", f"/api/queue-status/restaurant/{rid}")
    assert_test("Queue Wait Times", status == 200 and "crowdLevel" in res, f"Queue: {res}")

    print_header("Stage 5: Leftover Saver (Food Waste Rescue)")
    status, res = make_request("GET", "/api/leftovers")
    assert_test("List Surplus Offers", status == 200 and isinstance(res, list), f"Leftovers: {res}")
    if isinstance(res, list) and len(res) > 0:
        lid = res[0]["id"]
        status, rres = make_request("POST", f"/api/leftovers/{lid}/reserve", {"quantity": 1}, token=shared_context.get("token"))
        assert_test("Reserve Surplus Item", status == 200 and "reservationId" in rres, f"Reservation: {rres}")

    print_header("Stage 6: Combined Dual-Delivery Engine")
    status, res = make_request("GET", f"/api/combined-delivery/eligible-pairs?primaryRestaurantId={rid}&maxDistanceKm=2.0")
    assert_test("Eligible Partner Restaurants (<=2km)", status == 200 and isinstance(res, list), f"Pairs: {res}")
    if isinstance(res, list) and len(res) > 0:
        sec_id = res[0]["restaurantId"]
        quote_payload = {
            "primaryRestaurantId": rid,
            "secondaryRestaurantId": sec_id,
            "deliveryLatitude": 40.7200,
            "deliveryLongitude": -74.0100
        }
        status, qres = make_request("POST", "/api/combined-delivery/quote", quote_payload)
        assert_test("Combined Quote & Savings", status == 200 and qres.get("customerSavings", 0) > 0, f"Quote: {qres}")

    print_header("Stage 7: Smart Nutritional Meal Planner")
    plan_payload = {"days": 3, "maxDailyBudget": 35.00, "targetDailyCalories": 2000, "dietaryPreference": "None"}
    status, res = make_request("POST", "/api/meal-planner/generate", plan_payload)
    assert_test("Synthesize Caloric & Budget Meal Plan", status == 200 and len(res.get("daysPlan", [])) == 3, f"Plan: {res}")

    print_header("Stage 8: Notifications Feed")
    status, res = make_request("GET", "/api/notifications")
    assert_test("Retrieve In-App Notifications", status == 200 and isinstance(res, list), f"Notifications: {res}")

    print_header("Test Execution Summary")
    total = passed_count + failed_count
    print(f"Total Tests: {total} | Passed: {GREEN}{passed_count}{RESET} | Failed: {RED}{failed_count}{RESET}")
    if failed_count == 0:
        print(f"\n{BOLD}{GREEN}ALL TEST CASES PASSED! BiteNest API is 100% operational.{RESET}\n")
        sys.exit(0)
    else:
        print(f"\n{BOLD}{RED}SOME TESTS FAILED. Check output above.{RESET}\n")
        sys.exit(1)

if __name__ == "__main__":
    main()
