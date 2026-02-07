import { test, expect, APIRequestContext } from "@playwright/test";

const BASE_URL = "http://localhost:5000";

// Helper function to create a transaction
async function createTransaction(request: APIRequestContext, data: any): Promise<number> {
  const response = await request.post(`${BASE_URL}/transaction`, { data });
  expect(response.ok()).toBeTruthy();
  const body = await response.json();
  return body.id;
}

// Helper function to delete a transaction
async function deleteTransaction(request: APIRequestContext, id: number): Promise<void> {
  await request.delete(`${BASE_URL}/transaction/${id}`);
}

test.describe("PlanDateController - Tests", () => {
  test("GET /plandate - Get all plan dates", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/plandate`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
  });

  test("GET /plandate/search?transactionName={name} - Search plan dates by transaction name", async ({ request }) => {
    // Create a transaction with a unique name
    const uniqueName = `Search Test Transaction ${Date.now()}`;
    const transactionId = await createTransaction(request, {
      Name: uniqueName,
      Amount: 50,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Search for the plan date
    const response = await request.get(
      `${BASE_URL}/plandate/search?transactionName=${encodeURIComponent(uniqueName)}`
    );
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("GET /plandate/search?transactionName=NonExistent - Search returns empty for non-existent transaction", async ({ request }) => {
    const response = await request.get(
      `${BASE_URL}/plandate/search?transactionName=NonExistentTransaction123456789`
    );
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
    expect(body.length).toBe(0);
  });

  test("GET /plandate - Verify response structure", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/plandate`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();

    // If there are plan dates, verify the structure
    if (body.length > 0) {
      const planDate = body[0];
      expect(planDate).toBeDefined();
      expect(planDate.id).toBeDefined();
    }
  });
});

test.describe("PlanDateController - Integration Tests", () => {
  test("Create transaction, generate plan dates, verify plan dates can be retrieved", async ({ request }) => {
    const uniqueName = `Integration Test Transaction ${Date.now()}`;
    
    const transactionId = await createTransaction(request, {
      Name: uniqueName,
      Amount: 100,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    const generateResponse = await request.post(`${BASE_URL}/dtp/generate`);
    expect(generateResponse.ok()).toBeTruthy();

    // Get all plan dates
    const allPlanDatesResponse = await request.get(`${BASE_URL}/plandate`);
    const allPlanDates = await allPlanDatesResponse.json();
    
    // Should have plan dates
    expect(allPlanDates.length).toBeGreaterThan(0);

    // Search for our specific plan date
    const searchResponse = await request.get(
      `${BASE_URL}/plandate/search?transactionName=${encodeURIComponent(uniqueName)}`
    );
    const searchResults = await searchResponse.json();
    
    expect(searchResults.length).toBeGreaterThan(0);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("Create multiple transactions, verify plan dates for each", async ({ request }) => {
    const transactionNames = [
      `Multi Test 1 ${Date.now()}`,
      `Multi Test 2 ${Date.now()}`,
      `Multi Test 3 ${Date.now()}`,
    ];
    
    const transactionIds = [];

    // Create multiple transactions
    for (const name of transactionNames) {
      const id = await createTransaction(request, {
        Name: name,
        Amount: 50,
        StartDate: "2026-06-01",
        Frequency: 1,
        Active: true,
      });
      transactionIds.push(id);
    }

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Verify each transaction has plan dates
    for (const name of transactionNames) {
      const searchResponse = await request.get(
        `${BASE_URL}/plandate/search?transactionName=${encodeURIComponent(name)}`
      );
      const searchResults = await searchResponse.json();
      expect(searchResults.length).toBeGreaterThan(0);
    }

    // Cleanup
    for (const id of transactionIds) {
      await deleteTransaction(request, id);
    }
  });

  test("Verify plan dates have correct transaction reference", async ({ request }) => {
    const uniqueName = `Reference Test ${Date.now()}`;
    
    const transactionId = await createTransaction(request, {
      Name: uniqueName,
      Amount: 75,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Search for plan dates
    const searchResponse = await request.get(
      `${BASE_URL}/plandate/search?transactionName=${encodeURIComponent(uniqueName)}`
    );
    const planDates = await searchResponse.json();
    
    expect(planDates.length).toBeGreaterThan(0);

    // Verify each plan date references the correct transaction
    for (const planDate of planDates) {
      expect(planDate.transactionId).toBe(transactionId);
    }

    // Cleanup
    await deleteTransaction(request, transactionId);
  });
});
