import test, { expect } from "@playwright/test";
import {describe} from "node:test";

describe('basic transaction endpoint tests', async () => {
  test('transaction returns OK', async ({ request, baseURL }) => {
    const createdTransaction = await request.get(`${baseURL}/transaction`);
    console.log(createdTransaction);
    await expect(await createdTransaction.status()).toBe(200);
  });
});


describe('anticipated transactions', async () => {
  test.skip('anticipated transaction shows in dtp', async ({ request, baseURL }) => {
    const createdTransaction = await request.post(`${baseURL}/transaction`, {
      data: {
        "Name": "Testransaction",
        "Amount": 34,
        "StartDate": "2024-05-17",
        "Frequency": 1,
        "Active": true
      }
    });

    await expect(createdTransaction.ok()).toBeTruthy();
    const body = await createdTransaction.json();

    await request.delete(`${baseURL}/transaction/${body.id}`);
    const deletedTransaction = await request.get(`${baseURL}/transaction/${body.id}`);
    const deletedTransactionJson = await deletedTransaction.json();
    await expect(await deletedTransactionJson.status()).toBe(404);
  });
})