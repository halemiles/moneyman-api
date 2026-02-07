import { test, expect } from "@playwright/test";

const BASE_URL = "http://localhost:5000";

// Helper function to create a transaction
async function createTransaction(request, data) {
  const response = await request.post(`${BASE_URL}/transaction`, { data });
  expect(response.ok()).toBeTruthy();
  const body = await response.json();
  return body.id;
}

// Helper function to delete a transaction
async function deleteTransaction(request, id) {
  await request.delete(`${BASE_URL}/transaction/${id}`);
}

test.describe("TransactionController - CRUD Operations", () => {
  test("POST /transaction - Create a single transaction", async ({ request }) => {
    const transactionData = {
      Name: "Test Transaction",
      Amount: 50.75,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
      IsAnticipated: false,
    };

    const response = await request.post(`${BASE_URL}/transaction`, {
      data: transactionData,
    });

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    expect(body.id).toBeDefined();
    expect(typeof body.id).toBe("number");

    // Cleanup
    await deleteTransaction(request, body.id);
  });

  test("POST /transaction/multiple - Create multiple transactions", async ({ request }) => {
    const transactions = [
      {
        Name: "Transaction 1",
        Amount: 10,
        StartDate: "2024-06-01",
        Frequency: 1,
        Active: true,
      },
      {
        Name: "Transaction 2",
        Amount: 20,
        StartDate: "2024-06-01",
        Frequency: 2,
        Active: true,
      },
    ];

    const response = await request.post(`${BASE_URL}/transaction/multiple`, {
      data: transactions,
    });

    expect(response.ok()).toBeTruthy();
  });

  test("GET /transaction/{id} - Get transaction by ID", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Fetch Test Transaction",
      Amount: 100,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
    });

    const response = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    expect(response.ok()).toBeTruthy();
    
    const body = await response.json();
    expect(body.id).toBe(transactionId);
    expect(body.name).toBe("Fetch Test Transaction");
    expect(body.amount).toBe(100);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("GET /transaction/{id} - Returns 404 for non-existent transaction", async ({ request }) => {
    const response = await request.get(`${BASE_URL}/transaction/999999`);
    expect(response.status()).toBe(404);
  });

  test("GET /transaction - Get all transactions (non-anticipated)", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Non-Anticipated Transaction",
      Amount: 50,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
      IsAnticipated: false,
    });

    const response = await request.get(`${BASE_URL}/transaction`);
    expect(response.ok()).toBeTruthy();
    
    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
    
    const transaction = body.find((t) => t.id === transactionId);
    expect(transaction).toBeDefined();
    expect(transaction.isAnticipated).toBe(false);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("GET /transaction?anticipated=true - Get all anticipated transactions", async ({ request }) => {
    const anticipatedId = await createTransaction(request, {
      Name: "Anticipated Transaction",
      Amount: 75,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
      IsAnticipated: true,
    });

    const response = await request.get(`${BASE_URL}/transaction?anticipated=true`);
    expect(response.ok()).toBeTruthy();
    
    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
    
    // All returned transactions should be anticipated
    body.forEach((t) => {
      expect(t.isAnticipated).toBe(true);
    });

    const transaction = body.find((t) => t.id === anticipatedId);
    expect(transaction).toBeDefined();

    // Cleanup
    await deleteTransaction(request, anticipatedId);
  });

  test("GET /transaction/anticipated - Get anticipated transactions", async ({ request }) => {
    const anticipatedId = await createTransaction(request, {
      Name: "Another Anticipated Transaction",
      Amount: 125,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
      IsAnticipated: true,
    });

    const response = await request.get(`${BASE_URL}/transaction/anticipated`);
    expect(response.ok()).toBeTruthy();
    
    const body = await response.json();
    expect(Array.isArray(body)).toBeTruthy();
    
    const transaction = body.find((t) => t.id === anticipatedId);
    expect(transaction).toBeDefined();
    expect(transaction.isAnticipated).toBe(true);

    // Cleanup
    await deleteTransaction(request, anticipatedId);
  });

  test("PUT /transaction - Update transaction", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Original Transaction",
      Amount: 100,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
    });

    const updateResponse = await request.put(`${BASE_URL}/transaction`, {
      data: {
        Id: transactionId,
        Name: "Updated Transaction",
        Amount: 200.50,
        StartDate: "2024-07-01",
        Frequency: 2,
        Active: false,
      },
    });

    expect(updateResponse.ok()).toBeTruthy();

    // Verify the update
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    const updatedBody = await getResponse.json();
    
    expect(updatedBody.name).toBe("Updated Transaction");
    expect(updatedBody.amount).toBe(200.50);
    expect(updatedBody.active).toBe(false);

    // Cleanup
    await deleteTransaction(request, transactionId);
  });

  test("DELETE /transaction/{id} - Delete transaction", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Transaction to Delete",
      Amount: 50,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: true,
    });

    const deleteResponse = await request.delete(`${BASE_URL}/transaction/${transactionId}`);
    expect(deleteResponse.ok()).toBeTruthy();

    // Verify deletion
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    expect(getResponse.status()).toBe(404);
  });
});

test.describe("TransactionController - Different Transaction Types", () => {
  test("Create weekly transaction", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Weekly Transaction",
      Amount: 25,
      StartDate: "2024-06-01",
      Frequency: 0, // Weekly
      Active: true,
    });

    const response = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    const body = await response.json();
    
    expect(body.frequency).toBe(0);
    expect(body.name).toBe("Weekly Transaction");

    await deleteTransaction(request, transactionId);
  });

  test("Create monthly transaction", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Monthly Transaction",
      Amount: 100,
      StartDate: "2024-06-01",
      Frequency: 1, // Monthly
      Active: true,
    });

    const response = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    const body = await response.json();
    
    expect(body.frequency).toBe(1);
    expect(body.name).toBe("Monthly Transaction");

    await deleteTransaction(request, transactionId);
  });

  test("Create inactive transaction", async ({ request }) => {
    const transactionId = await createTransaction(request, {
      Name: "Inactive Transaction",
      Amount: 50,
      StartDate: "2024-06-01",
      Frequency: 1,
      Active: false,
    });

    const response = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    const body = await response.json();
    
    expect(body.active).toBe(false);

    await deleteTransaction(request, transactionId);
  });
});
