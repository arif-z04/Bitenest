#!/usr/bin/env python3
"""
BiteNest Backend API Automated Integration Test Suite
======================================================
Tests all core workflows matching BiteNest's live controllers:
- System health & DB connectivity
- Authentication & JWT token issuance
- Restaurant catalog & details
- Real-time kitchen queue calculations (/api/restaurants/{id}/queue-status)
- Leftover flash deals (/api/leftover-offers/active)
- Combined delivery eligibility check (/api/combined-delivery/check-eligibility)
- Smart meal planner recommendations (/api/meal-planner/recommend)
- In-app notifications feed (/api/notifications)
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
    status, res = make_request("GET", "/")
    assert_test("API Root Status Endpoint", status == 200 and res.get("status") == "Healthy", f"Response: {res}")

    print_header("Stage 2: Authentication & Access Control")
    ts = int(time.time())
    test_email = f"qa_user_{ts}@example.com"
    reg_payload = {
        "name": "QA Automation User",
        "email": test_email,
        "password": "Password123!",
        "role": 0,
        "phone": "+1-555-0199",
        "address": "100 QA Parkway"
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
    assert_test("Fetch Details with Menu", status == 200 and ("menu" in res or "restaurant" in res), f"Keys: {list(res.keys()) if isinstance(res, dict) else res}")

    print_header("Stage 4: Real-Time Kitchen Queue Engine")
    status, res = make_request("GET", f"/api/restaurants/{rid}/queue-status")
    assert_test("Queue Wait Times", status == 200 and "queueStatus" in res, f"Queue: {res}")

    print_header("Stage 5: Leftover Saver (Food Waste Rescue)")
    status, res = make_request("GET", "/api/leftover-offers/active")
    assert_test("List Active Surplus Offers", status == 200 and isinstance(res, list), f"Leftovers: {res}")

    print_header("Stage 6: Combined Dual-Delivery Engine")
    status, res = make_request("GET", "/api/combined-delivery/check-eligibility?restaurant1=1&restaurant2=2")
    assert_test("Check Dual-Delivery Eligibility (<=2km)", 
                status == 200 and res.get("canCombine") is True and res.get("customerSavings", 0) > 0, 
                f"Eligibility: {res}")

    print_header("Stage 7: Smart Nutritional Meal Planner")
    plan_payload = {"targetType": 0, "targetValue": 650}
    status, res = make_request("POST", "/api/meal-planner/recommend", plan_payload)
    assert_test("Synthesize Caloric & Budget Meal Recommendations", 
                status == 200 and isinstance(res, list) and len(res) > 0, 
                f"Plan: {res}")

    print_header("Stage 8: Notifications Feed")
    status, res = make_request("GET", "/api/notifications")
    assert_test("Retrieve In-App Notifications", status == 200 and isinstance(res, list), f"Notifications: {res}")

    print_header("Test Execution Summary")
    total = passed_count + failed_count
    print(f"Total Tests: {total} | Passed: {GREEN}{passed_count}{RESET} | Failed: {RED}{failed_count}{RESET}")
    if failed_count == 0:
        print(f"\n{BOLD}{GREEN}ALL 12 TEST CASES PASSED! BiteNest API is 100% operational.{RESET}\n")
        sys.exit(0)
    else:
        print(f"\n{BOLD}{RED}SOME TESTS FAILED. Check output above.{RESET}\n")
        sys.exit(1)

if __name__ == "__main__":
    main()
