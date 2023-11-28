PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Paydays" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Paydays" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Transactions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY AUTOINCREMENT,
    "Frequency" INTEGER NOT NULL,
    "IsAnticipated" INTEGER NOT NULL,
    "Name" TEXT NULL,
    "Amount" TEXT NOT NULL,
    "Active" INTEGER NOT NULL,
    "StartDate" TEXT NOT NULL
, "CategoryType" INTEGER NOT NULL DEFAULT 0, "PaymentType" INTEGER NOT NULL DEFAULT 0, "PriorityType" INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS "PlanDates" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_PlanDates" PRIMARY KEY AUTOINCREMENT,
    "TransactionId" INTEGER NULL,
    "Date" TEXT NOT NULL,
    "Active" INTEGER NOT NULL,
    "OriginalDate" TEXT NOT NULL,
    "YearGroup" INTEGER NOT NULL,
    "MonthGroup" INTEGER NOT NULL,
    "IsAnticipated" INTEGER NOT NULL,
    "OrderId" INTEGER NOT NULL,
    CONSTRAINT "FK_PlanDates_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id")
);

DELETE FROM sqlite_sequence;
INSERT INTO sqlite_sequence VALUES('Transactions',17);
INSERT INTO sqlite_sequence VALUES('Paydays',96);
INSERT INTO sqlite_sequence VALUES('PlanDates',3736);
CREATE INDEX "IX_PlanDates_TransactionId" ON "PlanDates" ("TransactionId");
COMMIT;
