import { expect, test } from "@playwright/test";

const THANK_YOU_TEXT = "Thank you for subscribing to our newsletter.";

test("newsletter confirm page posts token and redirects to landing page", async ({ page }) => {
  let capturedToken: string | null = null;

  await page.route("**/api/newsletter/confirmations", async (route) => {
    const body = route.request().postDataJSON() as { token?: string };
    capturedToken = body.token ?? null;

    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ accepted: true }),
    });
  });

  await page.goto("/newsletter/confirm?token=abc-token");

  await expect(page.getByText(THANK_YOU_TEXT)).toBeVisible();
  await page.waitForURL("**/");
  expect(capturedToken).toBe("abc-token");
});
