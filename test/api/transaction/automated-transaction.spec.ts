import { test, expect } from '@playwright/test';
import {dtp, dtpResponse} from '../models/dtp';


test('anticipated transaction shows in dtp', async ({ request }) => {
  const newAnticipatedTransaction = await request.post('/transaction', {
    data: {
      "Name":"TestAnticipatedTransaction",
      "Amount":34,
      "StartDate":"2024-06-17",
      "Frequency":1,
      "IsAnticipated":true,
      "Active": true
  }});

  await expect(newAnticipatedTransaction.ok()).toBeTruthy();
  const  body = await newAnticipatedTransaction.json();

  await request.post('/payday/generate', {
    data: {DayOfMonth: 25}
  });

  var generateResponse = await request.get('/dtp/generate');
  //await expect(generateResponse.ok()).toBeTruthy();
  console.log(await generateResponse.json());

  const dtpCurrentResult = await request.post('/dtp/full',{
    data:{
      startingValue:100
    }
  });

  const dtpResultJson = await dtpCurrentResult.json() as dtpResponse;

  await expect(await dtpResultJson.payload.planDates).not.toBeNull();
  await expect(await dtpResultJson.payload.planDates.some(pd => pd.transactionName == "TestAnticipatedTransaction")).toBeTruthy();

 await request.delete(`/transaction/${body.id}`);
 const deletedTransaction = await request.get(`/transaction/${body.id}`);
 const deletedTransactionJson = await deletedTransaction.json();
 await expect(await deletedTransactionJson.status).toBe(404);
});

