import { test, expect } from "@playwright/test";
import { dtpResponse } from "../../models/dtp";
import { Frequency } from "../../models/frequency";

function generateUuid(): string {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let id = "";
  for (let i = 0; i < 8; i++) {
    id += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return id;
}

test("anticipated transaction shows in dtp/full", async ({ request }) => {
  const uniqueName = `AnticipatedDTP-${generateUuid()}`;

  const createResponse = await request.post("/transaction", {
    data: {
      Name: uniqueName,
      Amount: 34,
      StartDate: "2026-04-15",
      Frequency: Frequency.Anticipated,
      Active: true,
    },
  });

  await expect(createResponse.ok()).toBeTruthy();
  const body = await createResponse.json();

  await request.post("/payday/generate", {
    data: { DayOfMonth: 25 },
  });

  const generateResponse = await request.post("/dtp/generate");
  expect(generateResponse.ok()).toBeTruthy();

  const dtpFullResult = await request.get("/dtp/full?startingValue=100");
  const dtpResultJson = (await dtpFullResult.json()) as dtpResponse;

  expect(dtpResultJson.payload.planDates).not.toBeNull();
  expect(
    dtpResultJson.payload.planDates.some((pd) => pd.transactionName === uniqueName)
  ).toBeTruthy();

  await request.delete(`/transaction/${body.id}`);
});

test("anticipated transaction shows in dtp/current", async ({ request }) => {
  const uniqueName = `AnticipatedDTP-${generateUuid()}`;

  const createResponse = await request.post("/transaction", {
    data: {
      Name: uniqueName,
      Amount: 34,
      StartDate: "2026-04-15",
      Frequency: Frequency.Anticipated,
      Active: true,
    },
  });

  await expect(createResponse.ok()).toBeTruthy();
  const body = await createResponse.json();

  await request.post("/payday/generate", {
    data: { DayOfMonth: 25 },
  });

  const generateResponse = await request.post("/dtp/generate");
  expect(generateResponse.ok()).toBeTruthy();

  const dtpCurrentResult = await request.get("/dtp/current?startingValue=100");
  const dtpResultJson = (await dtpCurrentResult.json()) as dtpResponse;

  expect(dtpResultJson.payload.planDates).not.toBeNull();
  expect(
    dtpResultJson.payload.planDates.some((pd) => pd.transactionName === uniqueName)
  ).toBeTruthy();

  await request.delete(`/transaction/${body.id}`);
});

test("anticipated transactions returned from /transaction/anticipated", async ({ request }) => {
  const name1 = `Anticipated-${generateUuid()}`;
  const name2 = `Anticipated-${generateUuid()}`;
  const name3 = `NotAnticipated-${generateUuid()}`;

  const [res1, res2, res3] = await Promise.all([
    request.post("/transaction", {
      data: {
        Name: name1,
        Amount: 34,
        StartDate: "2026-06-17",
        Frequency: Frequency.Anticipated,
        Active: true,
      },
    }),
    request.post("/transaction", {
      data: {
        Name: name2,
        Amount: 34,
        StartDate: "2026-06-17",
        Frequency: Frequency.Anticipated,
        Active: true,
      },
    }),
    request.post("/transaction", {
      data: {
        Name: name3,
        Amount: 34,
        StartDate: "2026-06-17",
        Frequency: Frequency.Monthly,
        Active: true,
      },
    }),
  ]);

  await expect(res1.ok()).toBeTruthy();
  await expect(res2.ok()).toBeTruthy();
  await expect(res3.ok()).toBeTruthy();

  const [body1, body2, body3] = await Promise.all([res1.json(), res2.json(), res3.json()]);

  const transactionResponse = await request.get("/transaction/anticipated");
  const transactions = await transactionResponse.json();

  expect(transactions.some((t) => t.name === name1)).toBeTruthy();
  expect(transactions.some((t) => t.name === name2)).toBeTruthy();
  expect(transactions.some((t) => t.name === name3)).toBeFalsy();

  await Promise.all([
    request.delete(`/transaction/${body1.id}`),
    request.delete(`/transaction/${body2.id}`),
    request.delete(`/transaction/${body3.id}`),
  ]);
});
