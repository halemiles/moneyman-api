import { test, expect } from "@playwright/test";
import { dtp, dtpResponse } from "../models/dtp";

test.skip("anticipated transaction shows in dtp", async ({ request }) => {
  const newAnticipatedTransaction = await request.post("/transaction", {
    data: {
      Name: "TestAnticipatedTransaction",
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: 1,
      IsAnticipated: true,
      Active: true,
    },
  });

  await expect(newAnticipatedTransaction.ok()).toBeTruthy();
  const body = await newAnticipatedTransaction.json();

  await request.post("/payday/generate", {
    data: { DayOfMonth: 25 },
  });

  var generateResponse = await request.post("/dtp/generate");
  expect(await generateResponse.ok()).toBeTruthy();
  console.log(await generateResponse.json());

  const dtpCurrentResult = await request.get("/dtp/full", {
    data: {
      startingValue: 100,
    },
  });

  const dtpResultJson = (await dtpCurrentResult.json()) as dtpResponse;

  await expect(await dtpResultJson.payload.planDates).not.toBeNull();
  await expect(
    await dtpResultJson.payload.planDates.some(
      (pd) => pd.transactionName == "TestAnticipatedTransaction"
    )
  ).toBeTruthy();

  await request.delete(`/transaction/${body.id}`);
  const deletedTransaction = await request.get(`/transaction/${body.id}`);
  const deletedTransactionJson = await deletedTransaction.json();
  await expect(await deletedTransactionJson.status).toBe(404);
});

test("anticipated transactions returned", async ({ request }) => {
  const newAnticipatedTransaction1 = await request.post("/transaction", {
    data: {
      Name: "TestAnticipatedTransaction1",
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: 1,
      IsAnticipated: true,
      Active: true,
    },
  });

  const newAnticipatedTransaction2 = await request.post("/transaction", {
    data: {
      Name: "TestAnticipatedTransaction2",
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: 1,
      IsAnticipated: true,
      Active: true,
    },
  });

  const newAnticipatedTransaction3 = await request.post("/transaction", {
    data: {
      Name: "TestAnticipatedTransaction3",
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: 1,
      IsAnticipated: false,
      Active: true,
    },
  });

  await expect(newAnticipatedTransaction1.ok()).toBeTruthy();
  await expect(newAnticipatedTransaction2.ok()).toBeTruthy();
  await expect(newAnticipatedTransaction3.ok()).toBeTruthy();

  var transactionResponse = await request.get("/transaction/anticipated");
  var response = await transactionResponse.json();

  await expect(
    response.some(
      (transaction) => transaction.name === "TestAnticipatedTransaction1"
    )
  ).toBeTruthy();
  await expect(
    response.some(
      (transaction) => transaction.name === "TestAnticipatedTransaction2"
    )
  ).toBeTruthy();
  await expect(
    response.some(
      (transaction) => transaction.name === "TestAnticipatedTransaction3"
    )
  ).toBeFalsy();
});
