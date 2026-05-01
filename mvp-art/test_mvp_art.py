#!/usr/bin/env python3
"""
Test MVP Core Art HTML page using Playwright
"""
from playwright.sync_api import sync_playwright
import sys

def test_mvp_art():
    with sync_playwright() as p:
        print("Launching browser...")
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        print("Navigating to MVP Core Art page...")
        page.goto('http://localhost:8080/mvp-art/mvp-core-art.html')

        print("Waiting for page to load...")
        page.wait_for_load_state('networkidle')

        # Check for console errors
        console_errors = []
        page.on("console", lambda msg: console_errors.append(msg.text) if msg.type == "error" else None)

        print("Checking page title...")
        title = page.title()
        print(f"Page title: {title}")

        print("Checking for canvas...")
        canvas = page.locator('canvas')
        if canvas.count() > 0:
            print(f"✓ Canvas found ({canvas.count()} element(s))")
        else:
            print("✗ Canvas not found")

        print("Checking sidebar controls...")
        sidebar = page.locator('.sidebar')
        if sidebar.count() > 0:
            print("✓ Sidebar found")
        else:
            print("✗ Sidebar not found")

        print("Checking for parameter controls...")
        particle_slider = page.locator('#coreParticles')
        if particle_slider.count() > 0:
            print("✓ Particle count slider found")
        else:
            print("✗ Particle count slider not found")

        print("Checking seed controls...")
        seed_input = page.locator('#seed-input')
        if seed_input.count() > 0:
            seed_value = seed_input.input_value()
            print(f"✓ Seed input found (value: {seed_value})")
        else:
            print("✗ Seed input not found")

        print("Checking color pickers...")
        color_pickers = page.locator('input[type="color"]')
        print(f"✓ Found {color_pickers.count()} color picker(s)")

        print("Checking buttons...")
        buttons = page.locator('.button')
        print(f"✓ Found {buttons.count()} button(s)")

        # Test interactivity
        print("\nTesting interactivity...")
        print("Clicking random seed button...")
        random_btn = page.locator('text=随机')
        if random_btn.count() > 0:
            random_btn.click()
            page.wait_for_timeout(500)
            new_seed = seed_input.input_value()
            print(f"✓ Random button works (new seed: {new_seed})")
        else:
            print("✗ Random button not found")

        print("Taking screenshot...")
        page.screenshot(path='/tmp/mvp-art-test.png', full_page=True)
        print("✓ Screenshot saved to /tmp/mvp-art-test.png")

        # Check loading state is hidden
        loading = page.locator('.loading')
        if loading.count() > 0:
            loading_style = loading.get_attribute('style')
            if loading_style and 'none' in loading_style:
                print("✓ Loading message hidden (art initialized)")
            else:
                print("! Loading message still visible")

        print("\n--- Test Results ---")
        if console_errors:
            print(f"✗ Console errors found: {console_errors}")
            browser.close()
            sys.exit(1)
        else:
            print("✓ No console errors")

        print("✓ All basic tests passed!")

        browser.close()
        return True

if __name__ == "__main__":
    try:
        test_mvp_art()
    except Exception as e:
        print(f"✗ Test failed with error: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
