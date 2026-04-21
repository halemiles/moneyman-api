PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "BankAccounts" (
    "Id"    INTEGER,
    "Name"  TEXT,
    "Active" INTEGER,
    PRIMARY KEY("Id")
);

CREATE TABLE IF NOT EXISTS "Paydays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Paydays" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" INTEGER NOT NULL,
    "Frequency" INTEGER NOT NULL,
    "Name" TEXT,
    "Amount" TEXT NOT NULL,
    "Active" INTEGER NOT NULL,
    "StartDate" TEXT NOT NULL,
    "CategoryType" INTEGER NOT NULL DEFAULT 0,
    "PaymentType" INTEGER NOT NULL DEFAULT 0,
    "PriorityType" INTEGER NOT NULL DEFAULT 0,
    "BankAccountID" INTEGER,
    CONSTRAINT "PK_Transactions" PRIMARY KEY("Id" AUTOINCREMENT)
);

CREATE TABLE IF NOT EXISTS "PlanDates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PlanDates" PRIMARY KEY AUTOINCREMENT,
    "TransactionId" INTEGER NULL,
    "Date" TEXT NOT NULL,
    "Active" INTEGER NOT NULL,
    "OriginalDate" TEXT NOT NULL,
    "Paid" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_PlanDates_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id")
);

CREATE TABLE IF NOT EXISTS "BankHolidays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_BankHolidays" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL,
    "Title" TEXT NOT NULL
);

CREATE INDEX "IX_PlanDates_TransactionId" ON "PlanDates" ("TransactionId");

COMMIT;
