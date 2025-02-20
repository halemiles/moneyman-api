import test, {expect} from "@playwright/test";
import {dtp} from "../models/dtp";

test('anticipated transaction shows in dtp', async ({ request }) => {
  const createdTransaction = await request.post('/transaction', {
    data: {
      "Name":"Testransaction",
      "Amount":34,
      "StartDate":"2024-05-17",
      "Frequency":1,
      "Active": true
  }});

  await expect(createdTransaction.ok()).toBeTruthy();
  const  body = await createdTransaction.json();


  await request.delete(`/transaction/${body.id}`);
  const deletedTransaction = await request.get(`/transaction/${body.id}`);
  const deletedTransactionJson = await deletedTransaction.json();
  await expect(await deletedTransactionJson.status).toBe(404);
});