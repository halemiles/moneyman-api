import { test, expect } from "@playwright/test";

const BASE_URL = "http://localhost:5000";

test.describe("BankAccountController - Tests", () => {
  test("GET /bankaccount - Get all bank accounts", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/bankaccount`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
  });

  test("GET /bankaccount?bankAccountId={id} - Get bank accounts with specific ID filter", async ({ request }) => {
    const bankAccountId = 1;
    const response = await request.get(`${BASE_URL}/bankaccount?bankAccountId=${bankAccountId}`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
  });

  test("GET /bankaccount - Verify response structure", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/bankaccount`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();

    // If there are bank accounts, verify the structure
    if (body.length > 0) {
      const bankAccount = body[0];
      expect(bankAccount).toBeDefined();
      // Verify common properties exist
      expect(bankAccount.id).toBeDefined();
    }
  });
});
