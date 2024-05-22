import { test, expect } from '@playwright/test';
import {dtp} from '../models/dtp';


test('anticipated transaction shows in dtp', async ({ request }) => {
  const newAnticipatedTransaction = await request.post('/transaction', {
    data: {
      "Name":"TestAnticipatedTransaction",
      "Amount":34,
      "StartDate":"2024-05-17",
      "Frequency":1,
      "IsAnticipated":true,
      "Active": true
  }});

  await expect(newAnticipatedTransaction.ok()).toBeTruthy();
  const  body = await newAnticipatedTransaction.json();

  await request.post('/payday/generate', {
    data: {DayOfMonth: 25}
  });
  //console.log(await paydayResult.json());

  const dtpResult = await request.get('/dtp/generate') ;
  //console.log(await dtpResult.json());

  const dtpCurrentResult = await request.post('/dtp/full',{
    data:{
      startingValue:100
    }
  });

  const dtpResultJson = await dtpCurrentResult.json() as dtp;
  //console.log(await dtpResultJson);

  await expect(dtpResultJson.planDates).not.toBeNull();
  await expect(dtpResultJson.planDates.some(pd => pd.transactionName == "TestAnticipatedTransaction")).toBeTruthy();

  // Clean up transaction and check transaction deleted
  console.log(body.id);
 await request.delete(`/transaction/${body.id}`);
 const deletedTransaction = await request.get(`/transaction/${body.id}`);
 const deletedTransactionJson = await deletedTransaction.json();
 await expect(await deletedTransactionJson.status).toBe(404);
});
