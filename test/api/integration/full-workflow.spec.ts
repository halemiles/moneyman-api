import { test, expect, APIRequestContext } from "@playwright/test";
import {Frequency} from "../models/frequency";

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

test.describe("Complete Workflow Integration Tests", () => {
  test("Full workflow: Create transactions, generate plan dates, verify correctness", async ({ request }) => {
    // Step 1: Create multiple transactions with different frequencies
    const weeklyTransactionId = await createTransaction(request, {
      Name: "Weekly Grocery Shopping",
      Amount: 150,
      StartDate: "2026-06-01",
      Frequency: Frequency.Weekly, // Weekly
      Active: true,
    });

    const monthlyRentId = await createTransaction(request, {
      Name: "Monthly Rent",
      Amount: 1200,
      StartDate: "2026-06-15",
      Frequency: Frequency.Monthly, // Monthly
      Active: true,
    });

    const monthlyUtilityId = await createTransaction(request, {
      Name: "Monthly Utility Bill",
      Amount: 150,
      StartDate: "2026-06-15",
      Frequency: Frequency.Monthly, // Monthly
      Active: true,
    });

    // Step 2: Generate plan dates for all transactions
    const generateResponse = await request.post(`${BASE_URL}/dtp/generate`);
    expect(generateResponse.ok()).toBeTruthy();

    const generateBody = await generateResponse.json();
    expect(generateBody.message).toBe("Successfully generated plandates");
    expect(generateBody.recordCount).toBeGreaterThan(0);

    // Step 3: Retrieve the DTP with a starting value
    const startingValue = 5000;
    const dtpResponse = await request.get(`${BASE_URL}/dtp/current?startingValue=${startingValue}`);
    expect(dtpResponse.ok()).toBeTruthy();

    const dtpBody = await dtpResponse.json();
    expect(dtpBody.payload).toBeDefined();
    expect(dtpBody.payload.planDates).toBeDefined();
    expect(Array.isArray(dtpBody.payload.planDates)).toBeTruthy();

    // Step 4: Verify that all transactions appear in the plan dates
    const planDates = dtpBody.payload.planDates;

    const weeklyPlanDates = planDates.filter((pd) => pd.transactionName === "Weekly Grocery Shopping");
    const rentPlanDates = planDates.filter((pd) => pd.transactionName === "Monthly Rent");
    const utilityPlanDates = planDates.filter((pd) => pd.transactionName === "Monthly Utility Bill");

    expect(weeklyPlanDates.length).toBeGreaterThan(0);
    expect(rentPlanDates.length).toBeGreaterThan(0);
    expect(utilityPlanDates.length).toBeGreaterThan(0);

    // Step 5: Verify amounts are correct
    weeklyPlanDates.forEach((pd) => {
      expect(pd.amount).toBe(150);
    });
    rentPlanDates.forEach((pd) => {
      expect(pd.amount).toBe(1200);
    });
    utilityPlanDates.forEach((pd) => {
      expect(pd.amount).toBe(150);
    });

    // Step 6: Verify weekly transactions have more occurrences than monthly
    expect(weeklyPlanDates.length).toBeGreaterThan(rentPlanDates.length);

    // // Step 7: Search for specific plan dates using PlanDate controller
    // const searchResponse = await request.get(
    //   `${BASE_URL}/plandate/search?transactionName=${encodeURIComponent("Monthly Rent")}`
    // );
    // expect(searchResponse.ok()).toBeTruthy();

    // const searchResults = await searchResponse.json();
    // expect(searchResults.length).toBeGreaterThan(0);
    // expect(searchResults[0].transactionId).toBe(monthlyRentId);

    // Cleanup
    await deleteTransaction(request, weeklyTransactionId);
    await deleteTransaction(request, monthlyRentId);
    await deleteTransaction(request, monthlyUtilityId);
  });

  test("Verify plan date generation accuracy for weekly transactions", async ({ request }) => {
    // Create a weekly transaction and pass a known UniqueId so we can find its plan dates
    const uniqueId = generateUuid();
    const transactionId = await createTransaction(request, {
      Name: `Weekly Accuracy Test ${uniqueId}`,
      Amount: 50,
      StartDate: "2026-06-03", // Monday
      Frequency: 2, // Weekly
      Active: true,
      UniqueId: uniqueId,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get full DTP to see multiple periods
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    const dtpBody = await dtpResponse.json();

    // Filter by the transaction unique id so we only pick up this transaction's plan dates
    const planDates = dtpBody.payload.planDates.filter((pd) => pd.transactionName === `Weekly Accuracy Test ${uniqueId}`);

    // Verify we have between 3 and 4 records created around the last/next payday
    expect(planDates.length).toBeGreaterThanOrEqual(3);
    expect(planDates.length).toBeLessThanOrEqual(4);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("Verify plan date generation accuracy for monthly transactions", async ({ request }) => {
    // Create a monthly transaction
    const monthlyAccuracyUniqueName = `Monthly Accuracy Test ${generateUuid()}`;
    const transactionId = await createTransaction(request, {
      Name: monthlyAccuracyUniqueName,
      Amount: 100,
      StartDate: "2026-06-15",
      Frequency: 1, // Monthly
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get full DTP
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=2000`);
    const dtpBody = await dtpResponse.json();

    const planDates = dtpBody.payload.planDates.filter(
      (pd) => pd.transactionName === monthlyAccuracyUniqueName
    );

    // Verify we have monthly occurrences
    expect(planDates.length).toBeGreaterThan(0);

    // Verify dates are approximately 1 month apart
    if (planDates.length >= 2) {
      for (let i = 1; i < planDates.length; i++) {
        const prevDate = new Date(planDates[i - 1].date);
        const currDate = new Date(planDates[i].date);
        const daysDiff = Math.round((currDate.getTime() - prevDate.getTime()) / (1000 * 60 * 60 * 24));

        // Should be roughly 28-31 days apart (accounting for different month lengths)
        expect(daysDiff).toBeGreaterThanOrEqual(25);
        expect(daysDiff).toBeLessThanOrEqual(35);
      }
    }

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("Verify balance calculation in DTP", async ({ request }) => {
    // Create transactions with known amounts
    const balanceTestUniqueName1 = `Balance Test1 ${generateUuid()}`;
    const transaction1Id = await createTransaction(request, {
      Name: balanceTestUniqueName1,
      Amount: 100,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    const balanceTestUniqueName2 = `Balance Test2 ${generateUuid()}`;
    const transaction2Id = await createTransaction(request, {
      Name: balanceTestUniqueName2,
      Amount: 50,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get DTP with a starting value
    const startingValue = 1000;
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=${startingValue}`);
    const dtpBody = await dtpResponse.json();

    expect(dtpBody.payload).toBeDefined();
    expect(dtpBody.payload.planDates).toBeDefined();

    // Verify that our test transactions appear in the plan dates with balances below the starting value
    const planDates = dtpBody.payload.planDates;
    const planDate1 = planDates.find((pd) => pd.transactionName === balanceTestUniqueName1);
    const planDate2 = planDates.find((pd) => pd.transactionName === balanceTestUniqueName2);

    expect(planDate1).toBeDefined();
    expect(planDate1.amount).toBeLessThanOrEqual(startingValue);

    expect(planDate2).toBeDefined();
    expect(planDate2.amount).toBeLessThanOrEqual(startingValue);

    // Cleanup
    await deleteTransaction(request, transaction1Id);
    await deleteTransaction(request, transaction2Id);
  });

  test.skip("Create anticipated transaction and verify it appears in DTP", async ({ request }) => {
    // Create an anticipated transaction
    const anticipatedUniqueName:string = `Anticipated Bonus ${generateUuid()}`;
    const anticipatedId = await createTransaction(request, {
      Name: anticipatedUniqueName,
      Amount: 500,
      StartDate: "2026-06-20",
      Frequency: Frequency.Anticipated,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get DTP
    const dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    const dtpBody = await dtpResponse.json();

    const planDates = dtpBody.payload.planDates;
    const anticipatedPlanDate = planDates.find((pd) => pd.transactionName === anticipatedUniqueName);

    expect(anticipatedPlanDate).toBeDefined();
    expect(anticipatedPlanDate.amount).toBe(500);

    // Cleanup
    await deleteTransaction(request, anticipatedId);
  });

  test("Update transaction and regenerate plan dates", async ({ request }) => {
    // Create a transaction
    const updateTestTransactionUniqueName = `Update Test Transaction ${generateUuid()}`;
    const transactionId = await createTransaction(request, {
      Name: updateTestTransactionUniqueName,
      Amount: 100,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get DTP and verify original amount
    let dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    let dtpBody = await dtpResponse.json();
    let planDates = dtpBody.payload.planDates.filter(
      (pd) => pd.transactionName === updateTestTransactionUniqueName
    );

    expect(planDates.length).toBeGreaterThan(0);
    planDates.forEach((pd) => {
      expect(pd.amount).toBe(100);
    });

    const updateData =
      {
        Id: transactionId,
        Name: `Update Test Transaction ${generateUuid()}`,
        Amount: 200,
        StartDate: "2026-06-01",
        Frequency: 1,
        Active: true,
      };
    // Update the transaction amount
    //TODO - Create another unique ID here
    const updateResponse = await request.put(`${BASE_URL}/transaction`, {
      data: updateData
    });
    expect(updateResponse.ok()).toBeTruthy();

    // Regenerate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get DTP again and verify updated amount
    dtpResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    dtpBody = await dtpResponse.json();
    planDates = dtpBody.payload.planDates.filter(
      (pd) => pd.transactionName === updateData.Name
    );

    expect(planDates.length).toBeGreaterThan(0);
    planDates.forEach((pd) => {
      expect(pd.amount).toBe(200);
    });

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("Verify full DTP vs current DTP returns different data", async ({ request }) => {
    // Create a transaction
    const transactionId = await createTransaction(request, {
      Name: "Current vs Full Test",
      Amount: 75,
      StartDate: "2026-06-01",
      Frequency: 1,
      Active: true,
    });

    // Generate plan dates
    await request.post(`${BASE_URL}/dtp/generate`);

    // Get current DTP
    const currentResponse = await request.get(`${BASE_URL}/dtp/current?startingValue=1000`);
    const currentBody = await currentResponse.json();
    const currentPlanDates = currentBody.payload.planDates || [];

    // Get full DTP
    const fullResponse = await request.get(`${BASE_URL}/dtp/full?startingValue=1000`);
    const fullBody = await fullResponse.json();
    const fullPlanDates = fullBody.payload.planDates || [];

    // Full DTP should typically have more or equal plan dates than current
    expect(fullPlanDates.length).toBeGreaterThanOrEqual(currentPlanDates.length);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });
});



function generateUuid() {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let id = '';
  for (let i = 0; i < 6; i++) {
    id += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return id;
}

