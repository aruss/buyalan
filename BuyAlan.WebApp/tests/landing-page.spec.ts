import { expect, test } from "@playwright/test";

const LANDING_TITLE = /^BuyAlan$/i;
const HERO_HEADING = "Conversational Sales for your Square.";
const MOBILE_VIEWPORT = { width: 390, height: 844 };
const MOBILE_MENU_LABEL = "Open navigation menu";

test("landing page renders the current hero and CTA targets", async ({ page }) => {
  await page.goto("/");

  await expect(page).toHaveTitle(LANDING_TITLE);
  await expect(page.getByRole("main")).toBeVisible();
  await expect(page.getByRole("heading", { level: 1, name: HERO_HEADING })).toBeVisible();

  await expect(page.getByRole("link", { name: "Connect Square" })).toHaveAttribute("href", "/admin");
  await expect(page.getByRole("link", { name: "View Live Demo" })).toHaveAttribute("href", "/admin");
  await expect(page.getByText("Supported Channels")).toBeVisible();

  const pricingSection = page.locator("#pricing");
  await expect(pricingSection.getByRole("link", { name: "Deploy Free" })).toHaveAttribute("href", "/admin");
  await expect(pricingSection.getByRole("link", { name: "Initialize Trial" })).toHaveAttribute("href", "/admin");
  await expect(pricingSection.getByRole("link", { name: "Contact Integrations" })).toHaveAttribute("href", "/admin");
});

test("desktop navigation keeps section links and desktop CTA placement", async ({ page }) => {
  await page.goto("/");

  const navigation = page.locator("nav");
  await expect(navigation.getByRole("link", { name: "Features" })).toHaveAttribute("href", "#features");
  await expect(navigation.getByRole("link", { name: "Merchant Dashboard" })).toHaveAttribute("href", "#dashboard");
  await expect(navigation.getByRole("link", { name: "Pricing" })).toHaveAttribute("href", "#pricing");
  await expect(navigation.getByRole("link", { name: "Trust" })).toHaveAttribute("href", "#compliance");
  await expect(navigation.getByRole("link", { name: "Start Free Trial" })).toHaveAttribute("href", "/admin");
  await expect(navigation.getByRole("button", { name: MOBILE_MENU_LABEL })).toBeHidden();
});

test("mobile menu exposes landing navigation and closes after section selection", async ({ page }) => {
  await page.setViewportSize(MOBILE_VIEWPORT);
  await page.goto("/");

  const menuButton = page.locator("nav button").first();
  await expect(menuButton).toBeVisible();
  await expect(menuButton).toHaveAttribute("aria-label", MOBILE_MENU_LABEL);
  await expect(menuButton).toHaveAttribute("aria-expanded", "false");
  await expect(page.getByRole("link", { name: "Start Free Trial" }).nth(0)).not.toBeVisible();

  await menuButton.click();

  await expect(menuButton).toHaveAttribute("aria-expanded", "true");
  const menu = page.getByRole("menu");
  await expect(menu.getByRole("menuitem", { name: "Features" })).toBeVisible();
  await expect(menu.getByRole("menuitem", { name: "Merchant Dashboard" })).toBeVisible();
  await expect(menu.getByRole("menuitem", { name: "Pricing" })).toBeVisible();
  await expect(menu.getByRole("menuitem", { name: "Trust" })).toBeVisible();
  await expect(menu.getByRole("menuitem", { name: "Start Free Trial" })).toBeVisible();

  await menu.getByRole("menuitem", { name: "Pricing" }).click();

  await expect(page).toHaveURL(/#pricing$/);
  await expect(menuButton).toHaveAttribute("aria-expanded", "false");
});

test("mobile menu supports keyboard open and escape close", async ({ page }) => {
  await page.setViewportSize(MOBILE_VIEWPORT);
  await page.goto("/");

  const menuButton = page.locator("nav button").first();
  await expect(menuButton).toHaveAttribute("aria-label", MOBILE_MENU_LABEL);
  await menuButton.focus();
  await page.keyboard.press("Enter");

  await expect(menuButton).toHaveAttribute("aria-expanded", "true");
  await expect(page.getByRole("menuitem", { name: "Features" })).toBeVisible();

  await page.keyboard.press("Escape");

  await expect(menuButton).toHaveAttribute("aria-expanded", "false");
  await expect(menuButton).toBeFocused();
});
