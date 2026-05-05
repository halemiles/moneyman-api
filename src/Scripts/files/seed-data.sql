PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;

INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (85, '2024-01-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (86, '2024-02-27 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (87, '2024-03-27 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (88, '2024-04-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (89, '2024-05-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (90, '2024-06-26 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (91, '2024-07-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (92, '2024-08-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (93, '2024-09-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (94, '2024-10-25 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (95, '2024-11-27 00:00:00');
INSERT OR IGNORE INTO "Paydays" ("Id", "Date") VALUES (96, '2024-12-27 00:00:00');

INSERT OR IGNORE INTO "Transactions" ("Id", "Frequency", "Name", "Amount", "Active", "StartDate", "CategoryType", "PaymentType", "PriorityType", "BankAccountID") VALUES (1, 1, 'Electricity', '100', 1, '2024-01-25 00:00:00', 2, 1, 0, NULL);
INSERT OR IGNORE INTO "Transactions" ("Id", "Frequency", "Name", "Amount", "Active", "StartDate", "CategoryType", "PaymentType", "PriorityType", "BankAccountID") VALUES (2, 1, 'Internet',     '50',  1, '2024-01-01 00:00:00', 0, 0, 0, NULL);
INSERT OR IGNORE INTO "Transactions" ("Id", "Frequency", "Name", "Amount", "Active", "StartDate", "CategoryType", "PaymentType", "PriorityType", "BankAccountID") VALUES (3, 1, 'Fuel',          '150', 1, '2024-01-29 00:00:00', 0, 0, 0, NULL);

COMMIT;
