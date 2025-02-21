import { test, expect } from "@playwright/test";

const BASE_URL = "http://localhost:5000"; // Adjust if needed

test.describe("Transaction API Tests", () => {
  let transactionId;

  test("Create a transaction and verify it exists", async ({ request }) => {
    const response = await request.post(`${BASE_URL}/transaction`, {
      data: {
        name: "test transaction",
        amount: 100.5,
        startdate: "2024-05-17T00:00:00Z",
        frequency: 1,
        active: true,
        isanticipated: false,
        bankaccountid: 123,
      },
    });

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    transactionId = body.id;
    expect(transactionId).toBeDefined();

    // Fetch the transaction and verify its data
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    expect(getResponse.ok()).toBeTruthy();
    const fetchedData = await getResponse.json();

    expect(fetchedData.name).toBe("test transaction");
    expect(fetchedData.amount).toBe(100.5);
    expect(fetchedData.active).toBe(true);
    //expect(fetchedData.isanticipated).toBe(false);
    //expect(fetchedData.bankaccountid).toBe(123);
  });

  test("Update transaction and verify persistence", async ({ request }) => {

    const updateResponse = await request.put(`${BASE_URL}/transaction`, {
      data: {
        id: transactionId,
        name: "updated Transaction",
        amount: 200.75,
        active: false,
      },
    });

    expect(updateResponse.ok()).toBeTruthy();

    // Fetch updated transaction
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    //expect(getResponse.ok()).toBeTruthy();
    const updatedData = await getResponse.json();
    console.log("Updated Data",updatedData);

    expect(updatedData.name).toBe("updated Transaction");
    expect(updatedData.amount).toBe(200.75);
    expect(updatedData.active).toBe(false);
  });

  test("Update transaction with null values", async ({ request }) => {

    const updateResponse = await request.put(`${BASE_URL}/transaction`, {
      data: {
        id: transactionId,
      },
    });

    expect(updateResponse.ok()).toBeTruthy();

    // Fetch updated transaction
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
    //expect(getResponse.ok()).toBeTruthy();
    const updatedData = await getResponse.json();

    expect(updatedData.name).toBe("updated Transaction");
    expect(updatedData.amount).toBe(200.75);
    expect(updatedData.active).toBe(false);
  });

  // test("Delete transaction and confirm it's gone", async ({ request }) => {

  //   const deleteResponse = await request.delete(`${BASE_URL}/transaction/${transactionId}`);
  //   expect(deleteResponse.ok()).toBeTruthy();

  //   // Verify it's deleted
  //   const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
  //   expect(getResponse.status()).toBe(404);
  // });


});
