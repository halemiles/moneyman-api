# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MoneyMan API is a .NET 9 ASP.NET Core REST API for managing monthly bills and predicting due dates relative to payday. It uses a SQLite database and has both unit tests (MSTest) and API integration tests (Playwright).

## Commands

### .NET (run from `src/`)

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project Moneyman.Api          # Start API on http://localhost:5000
dotnet test Moneyman.Tests --no-build      # Run all unit tests
dotnet test Moneyman.Tests --filter "TestMethodName"  # Run a single test
dotnet test Moneyman.Tests /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput='cobertura.xml'
```

Coverage HTML report:
```bash
./Scripts/generate-coverage.sh             # Run from src/
```

Database initialization:
```bash
./Scripts/intitialise-db.sh                # Creates LocalDatabase.db from schema + seed data
```

### Playwright API Tests (run from `test/`)

```bash
npm install
npx playwright test                        # Run all tests (requires API running on :5000)
npx playwright test --ui                   # Interactive UI mode
npx playwright test api/transaction/       # Run a specific test folder
npx playwright test --headed               # Headed browser mode
```

### Docker

```bash
cd src && docker-compose up --build        # Runs on port 8600
```

## Architecture

The solution (`src/Moneyman.Api.sln`) follows a layered architecture:

- **Moneyman.Api** — Controllers, middleware, DI registration, Swagger. Controllers: `BankAccount`, `Dtp`, `PlanDate`, `Transaction`.
- **Moneyman.Domain** — Entities, DTOs, enums (`Frequency`, `CategoryType`, `PaymentType`, `PriorityType`, `GenerationStrategy`), EF Core `MoneymanContext`, and Mapperly mapper profiles.
- **Moneyman.Services** — Business logic. Key service: `DtpService` (Due Till Payday — calculates bills relative to payday). Also: `TransactionService`, `PlanDateService`, `PaydayService`, `HolidayService`, `OffsetCalculationService`, `WeekdayService`. FluentValidation validators live here.
- **Moneyman.Persistence** — `GenericRepository<T>` base class, then typed repos for each entity (BankAccount, Payday, PlanDate, Transaction).
- **Moneyman.Extensions** — Shared extension methods.
- **Moneyman.Tests** — MSTest unit tests using Moq and AutoFixture.

### Key Domain Concepts

- **Transaction** — A recurring bill with a frequency (Weekly, Monthly, etc.), start date, category, payment type, and priority.
- **PlanDate** — A predicted transaction date adjusted for weekends and UK bank holidays (configured in `appsettings.json`).
- **DTP (Due Till Payday)** — Core calculation: groups transactions into "before payday" and "after payday" buckets for a given month.
- **Payday** — Configured as day 25 of the month (`PaydayOptions:DayOfMonth`), pre-calculated and stored in the `Paydays` table.

### Object Mapping

Uses [Riok.Mapperly](https://mapperly.riok.app/) (source-generated mappers) — mapper profiles are in `Moneyman.Domain/MapperProfiles/`. Do not use AutoMapper.

### Database

SQLite via EF Core. Connection string: `Data Source=./LocalDatabase.db`. Schema and seed scripts are in `src/Scripts/files/`.

## Test Structure

Unit tests (`Moneyman.Tests`) mirror the service/persistence layer. Playwright API tests (`test/api/`) are organized by controller with a `Base/Pages/` page object model and shared `models/` types.

Playwright tests expect the API running locally at `http://localhost:5000`. On CI, retries are set to 2.
