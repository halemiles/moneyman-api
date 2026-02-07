import { test, expect } from "@playwright/test";

const BASE_URL = "http://localhost:5000"; // Adjust if needed


  test("Update transaction and verify persistence", async ({ request }) => {
    const transactionId = await setup(request);
    console.log('test 2 transactionid', transactionId);
    
    const updateResponse = await request.put(`${BASE_URL}/transaction`, {
      data: {
        id: transactionId,
        name: "updated Transaction",
        amount: 200.75,
        active: false,
          startDate: "2023-10-10T01:30:45Z",
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
    expect(updatedData.startDate).toBe("2023-10-10T01:30:45");
  });

  // test("Update transaction with null values", async ({ request }) => {
  //
  //   const updateResponse = await request.put(`${BASE_URL}/transaction`, {
  //     data: {
  //       id: transactionId,
  //     },
  //   });
  //
  //   expect(updateResponse.ok()).toBeTruthy();
  //
  //   // Fetch updated transaction
  //   const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
  //   //expect(getResponse.ok()).toBeTruthy();
  //   const updatedData = await getResponse.json();
  //
  //   expect(updatedData.name).toBe("updated Transaction");
  //   expect(updatedData.amount).toBe(200.75);
  //   expect(updatedData.active).toBe(false);
  // });

  // test("Delete transaction and confirm it's gone", async ({ request }) => {

  //   const deleteResponse = await request.delete(`${BASE_URL}/transaction/${transactionId}`);
  //   expect(deleteResponse.ok()).toBeTruthy();

  //   // Verify it's deleted
  //   const getResponse = await request.get(`${BASE_URL}/transaction/${transactionId}`);
  //   expect(getResponse.status()).toBe(404);
  // });



async function setup(request) {
    let transactionIdResponse = 0;
    const response = await request.post(`${BASE_URL}/transaction`, {
        data: {
            name: "test transaction",
            amount: 100.5,
            startdate: "2026-05-17T00:00:00Z",
            frequency: 1,
            active: true,
            isanticipated: false,
            bankaccountid: 123,
        },
    });

    expect(response.ok()).toBeTruthy();
    const body = await response.json();
    transactionIdResponse = body.id;
    expect(transactionIdResponse).toBeDefined();

    // Fetch the transaction and verify its data
    const getResponse = await request.get(`${BASE_URL}/transaction/${transactionIdResponse}`);
    expect(getResponse.ok()).toBeTruthy();
    const fetchedData = await getResponse.json();

    expect(fetchedData.name).toBe("test transaction");
    expect(fetchedData.amount).toBe(100.5);
    expect(fetchedData.active).toBe(true);
    expect(fetchedData.startDate).toBe("2026-05-17T00:00:00");
    return transactionIdResponse;
}
