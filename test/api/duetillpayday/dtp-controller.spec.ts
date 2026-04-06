import { test, expect, APIRequestContext } from "@playwright/test";

const BASE_URL = "http://localhost:5000";

// Helper to produce an ISO date string for a day offset from today
function isoDate(days = 1) {
  const d = new Date();
  d.setDate(d.getDate() + days);
  return d.toISOString().split("T")[0];
}

// Helper function to create a transaction
async function createTransaction(request: APIRequestContext, data: any): Promise<number> {
  const response = await request.post(`${BASE_URL}/transaction`, { data });

  // expect(response.ok()).toBeTruthy();
  const body = await response.json();
  return body.id;
}

// Helper function to delete a transaction
async function deleteTransaction(request: APIRequestContext, id: number): Promise<void> {
  await request.delete(`${BASE_URL}/transaction/${id}`);
}

test.describe("DtpController - Plan Date Generation", () => {
  test("POST /dtp/generate - Generate plan dates for all transactions", async ({ request }) => {
    // Create a test transaction
    const transactionId = await createTransaction(request, {
      Name: "Test DTP Transaction",
      Amount: 50,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    const response = await request.post(`${BASE_URL}/dtp/generate`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body.message).toBeDefined();
    expect(body.recordCount).toBeDefined();
    expect(typeof body.recordCount).toBe("number");

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("POST /dtp/generate?transactionId={id} - Generate plan dates for specific transaction", async ({ request }) => {
    // Create a specific transaction
    const transactionId = await createTransaction(request, {
      Name: "Specific Transaction for DTP",
      Amount: 75,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    const response = await request.post(`${BASE_URL}/dtp/generate?transactionId=${transactionId}`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body.message).toBeDefined();
    expect(body.recordCount).toBeDefined();

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("GET /dtp/current - Get current period DTP", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/dtp/current`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body).toBeDefined();
    expect(body.payload).toBeDefined();
  });

  test("GET /dtp/current?startingValue=1000 - Get current period with starting value", async ({ request }) => {
    const startingValue = 1000;
    const response = await request.get(`${BASE_URL}/dtp/current?startingValue=${startingValue}`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body).toBeDefined();
    expect(body.payload).toBeDefined();
  });

  test("GET /dtp/current?bankAccountId=1 - Get current period for specific bank account", async ({ request }) => {
    const bankAccountId = 1;
    const response = await request.get(`${BASE_URL}/dtp/current?bankAccountId=${bankAccountId}`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body).toBeDefined();
  });

  test("GET /dtp/full - Get full DTP period", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/dtp/full`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body).toBeDefined();
    expect(body.payload).toBeDefined();
  });

  test("GET /dtp/full?startingValue=2000 - Get full DTP with starting value", async ({ request }) => {
    const startingValue = 2000;
    const response = await request.get(`${BASE_URL}/dtp/full?startingValue=${startingValue}`);
    expect(response.ok()).toBeTruthy();

    const body = await response.json();
    expect(body).toBeDefined();
  });
});

test.describe("DtpController - Integration Tests with Transactions", () => {
  test("Create transaction, generate plan dates, verify plan dates exist", async ({ request }) => {
    // Create a weekly transaction
    const weeklyTransactionId = await createTransaction(request, {
      Name: "Weekly Test Transaction",
      Amount: 25,
      StartDate: isoDate(),
      Frequency: 2, // Weekly
      Active: true,
    });

    // Create a monthly transaction
    const monthlyTransactionId = await createTransaction(request, {
      Name: "Monthly Test Transaction",
      Amount: 100,
      StartDate: isoDate(),
      Frequency: 1, // Monthly
      Active: true,
    });

    // Generate plan dates
    const generateResponse = await request.post(`${BASE_URL}/dtp/generate`);
    expect(generateResponse.ok()).toBeTruthy();
    const generateBody = await generateResponse.json();
    expect(generateBody.message).toBeDefined();
    expect(generateBody.recordCount).toBeGreaterThan(0);

    // Get current DTP to verify plan dates were created
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    expect(dtpResponse.ok()).toBeTruthy();

    const dtpBody = await dtpResponse.json();
    expect(dtpBody.payload).toBeDefined();
    expect(dtpBody.payload.planDates).toBeDefined();

    // Verify our transactions appear in the plan dates
    const planDates = dtpBody.payload.planDates;
    const weeklyPlanDate = planDates.find((pd) => pd.transactionName === "Weekly Test Transaction");
    const monthlyPlanDate = planDates.find((pd) => pd.transactionName === "Monthly Test Transaction");

    expect(weeklyPlanDate).toBeDefined();
    expect(monthlyPlanDate).toBeDefined();

    // Cleanup
    await deleteTransaction(request, weeklyTransactionId);
    await deleteTransaction(request, monthlyTransactionId);
  });

  test("Verify plan dates are generated with correct amounts", async ({ request }) => {
    const expectedAmount = 123.45;

    const transactionId = await createTransaction(request, {
      Name: "Amount Verification Transaction",
      Amount: expectedAmount,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get full DTP
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    const dtpBody = await dtpResponse.json();

    // Find our transaction in the plan dates
    const planDate = dtpBody.payload.planDates.find(
      (pd) => pd.transactionName === "Amount Verification Transaction"
    );

    expect(planDate).toBeDefined();
    expect(planDate.amount).toBe(expectedAmount);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("Verify multiple transactions generate multiple plan dates", async ({ request }) => {
    // Create multiple transactions
    const transactionIds = [];

    for (let i = 1; i <= 3; i++) {
      const id = await createTransaction(request, {
        Name: `Multi Transaction ${i}`,
        Amount: i * 10,
        StartDate: isoDate(),
        Frequency: 1,
        Active: true,
      });
      transactionIds.push(id);
    }

    // Generate plan dates
    const generateResponse = await request.post(`${BASE_URL}/dtp/generate`);
    const generateBody = await generateResponse.json();
    expect(generateBody.recordCount).toBeGreaterThan(0);

    // Get DTP and verify all transactions appear
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=500`);
    const dtpBody = await dtpResponse.json();

    const planDates = dtpBody.payload.planDates;

    for (let i = 1; i <= 3; i++) {
      const planDate = planDates.find((pd) => pd.transactionName === `Multi Transaction ${i}`);
      expect(planDate).toBeDefined();
    }

    // Cleanup
    for (const id of transactionIds) {
      await deleteTransaction(request, id);
    }
  });

  test("Verify inactive transactions do not generate plan dates", async ({ request }) => {
    const activeId = await createTransaction(request, {
      Name: "Active Transaction",
      Amount: 50,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    const inactiveId = await createTransaction(request, {
      Name: "Inactive Transaction",
      Amount: 75,
      StartDate: isoDate(),
      Frequency: 1,
      Active: false,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get DTP
    const dtpResponse = await request.get(`${BASE_URL}/dtp/current?startingValue=500`);
    const dtpBody = await dtpResponse.json();

    const planDates = dtpBody.payload.planDates;

    const activePlanDate = planDates.find((pd) => pd.transactionName === "Active Transaction");
    const inactivePlanDate = planDates.find((pd) => pd.transactionName === "Inactive Transaction");

    expect(activePlanDate).toBeDefined();
    expect(inactivePlanDate).toBeUndefined();

    // Cleanup
    await deleteTransaction(request, activeId);
    await deleteTransaction(request, inactiveId);
  });

  test("Generate plan dates for specific transaction only", async ({ request }) => {
    const transaction1Id = await createTransaction(request, {
      Name: "Transaction One",
      Amount: 100,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    const transaction2Id = await createTransaction(request, {
      Name: "Transaction Two",
      Amount: 200,
      StartDate: isoDate(),
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates for only transaction2
    const generateResponse = await request.post(
      `${BASE_URL}/dtp/generate?transactionId=${transaction2Id}`
    );

    expect(generateResponse.ok()).toBeTruthy();
    const generateBody = await generateResponse.json();
    expect(generateBody.message).toBeDefined();
    expect(generateBody.recordCount).toBeGreaterThan(0);

    // Cleanup
    await deleteTransaction(request, transaction1Id);
    await deleteTransaction(request, transaction2Id);
  });
});
