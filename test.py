from playwright.sync_api import sync_playwright

URL = "https://www.accuweather.com/en/my/puchong-new-village/230459/air-quality-index/230459"

def main():
    with sync_playwright() as p:
        # Use Firefox instead of Chromium to avoid HTTP/2 protocol issues
        browser = p.firefox.launch(headless=True)

        page = browser.new_page(
            user_agent=(
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) "
                "AppleWebKit/537.36 (KHTML, like Gecko) "
                "Chrome/123.0.0.0 Safari/537.36"
            )
        )

        # Less strict wait condition; just wait for DOM to be ready
        page.goto(URL, wait_until="domcontentloaded", timeout=60000)

        # Give the page a bit of time for scripts/charts to render
        page.wait_for_timeout(5000)

        # Wait for the air-quality bars to appear
        page.wait_for_selector("rect.air-quality-chart-bar", timeout=30000)

        rects = page.query_selector_all("rect.air-quality-chart-bar")

        rows = []
        for r in rects:
            x = r.get_attribute("data-xvalue")
            y = r.get_attribute("data-yvalue")
            if x is not None and y is not None:
                rows.append((x, y))

        browser.close()

    # Print pairs (xvalue, yvalue)
    for x, y in rows:
        print(f"{x}\t{y}")

if __name__ == "__main__":
    main()