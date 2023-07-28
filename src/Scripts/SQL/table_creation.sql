DROP TABLE Paydays;
CREATE TABLE IF NOT EXISTS Paydays (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    `Date` DATE NOT NULL
);

DROP TABLE Transactions;
CREATE TABLE IF NOT EXISTS Transactions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Frequency INTEGER NOT NULL,
    IsAnticipated INTEGER NOT NULL,
    `Name` VARCHAR(255) NULL,
    Amount DECIMAL(4,2) NOT NULL,
    Active INTEGER NOT NULL,
    StartDate Date NOT NULL
, CategoryType INTEGER NOT NULL DEFAULT 0, PaymentType INTEGER NOT NULL DEFAULT 0, PriorityType INTEGER NOT NULL DEFAULT 0);

CREATE TABLE IF NOT EXISTS PlanDates (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TransactionId INTEGER NULL,
    `Date` Date NOT NULL,
    Active INTEGER NOT NULL,
    OriginalDate VARCHAR(255) NOT NULL,
    YearGroup INTEGER NOT NULL,
    MonthGroup INTEGER NOT NULL,
    IsAnticipated INTEGER NOT NULL,
    OrderId INTEGER NOT NULL,
    CONSTRAINT FK_PlanDates_Transactions_TransactionId FOREIGN KEY (TransactionId) REFERENCES Transactions (Id)
);

CREATE INDEX IX_PlanDates_TransactionId ON PlanDates (TransactionId);

