import { expect, test } from "@playwright/test";

const NEWSLETTER_INPUT_PLACEHOLDER = "Subscribe to newsletter";
const SUBSCRIBE_BUTTON_NAME = "Subscribe";
const CONFIRMATION_TEXT = "You are subscribed. Please check your inbox to confirm.";

test("newsletter footer validates email format", async ({ page }) => {
  await page.goto("/");

  const newsletterInput = page.getByPlaceholder(NEWSLETTER_INPUT_PLACEHOLDER);
  await newsletterInput.fill("invalid-email");
  await page.getByRole("button", { name: SUBSCRIBE_BUTTON_NAME }).click();

  await expect(page.getByText("Use a valid email format.")).toBeVisible();
});

test("newsletter footer shows confirmation after successful submit", async ({ page }) => {
  await page.route("**/api/newsletter/subscriptions", async (route) => {
    await route.fulfill({
      status: 202,
      contentType: "application/json",
      body: JSON.stringify({ accepted: true }),
    });
  });

  await page.goto("/");

  const newsletterInput = page.getByPlaceholder(NEWSLETTER_INPUT_PLACEHOLDER);
  await newsletterInput.fill("person@example.com");
  await page.getByRole("button", { name: SUBSCRIBE_BUTTON_NAME }).click();

  await expect(page.getByText(CONFIRMATION_TEXT)).toBeVisible();
});
