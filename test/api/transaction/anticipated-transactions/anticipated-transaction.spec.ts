import { test, expect } from "../../fixtures";
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

// A date a couple of days from now, so an anticipated bill falls inside the
// current payday period (today .. next payday) and is visible in /dtp/current.
function inCurrentPeriodDate(): string {
  const d = new Date();
  d.setDate(d.getDate() + 2);
  return d.toISOString().split("T")[0];
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

  const dtpFullResult = await request.get("/dtp/all?startingValue=100");
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
      StartDate: inCurrentPeriodDate(),
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

test("anticipated transactions returned from /transaction?anticipated=true", async ({ request }) => {
  const name1 = `Anticipated-${generateUuid()}`;
  const name2 = `Anticipated-${generateUuid()}`;
  const name3 = `NotAnticipated-${generateUuid()}`;

  // Create sequentially, not via Promise.all: SQLite is single-writer, so
  // concurrent POSTs can hit "database is locked". Concurrency isn't under test
  // here — we only need the three transactions to exist before we query them.
  const res1 = await request.post("/transaction", {
    data: {
      Name: name1,
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: Frequency.Anticipated,
      Active: true,
    },
  });
  const res2 = await request.post("/transaction", {
    data: {
      Name: name2,
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: Frequency.Anticipated,
      Active: true,
    },
  });
  const res3 = await request.post("/transaction", {
    data: {
      Name: name3,
      Amount: 34,
      StartDate: "2026-06-17",
      Frequency: Frequency.Monthly,
      Active: true,
    },
  });

  await expect(res1.ok()).toBeTruthy();
  await expect(res2.ok()).toBeTruthy();
  await expect(res3.ok()).toBeTruthy();

  const body1 = await res1.json();
  const body2 = await res2.json();
  const body3 = await res3.json();

  const transactionResponse = await request.get("/transaction?anticipated=true");
  const transactions = await transactionResponse.json();

  expect(transactions.some((t) => t.name === name1)).toBeTruthy();
  expect(transactions.some((t) => t.name === name2)).toBeTruthy();
  expect(transactions.some((t) => t.name === name3)).toBeFalsy();

  await request.delete(`/transaction/${body1.id}`);
  await request.delete(`/transaction/${body2.id}`);
  await request.delete(`/transaction/${body3.id}`);
});
