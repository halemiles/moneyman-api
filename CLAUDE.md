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

Endpoint smoke check (requires the API already running):
```powershell
./Scripts/check-endpoints.ps1              # Probes read endpoints on :5000, exits non-zero on failure
./Scripts/check-endpoints.ps1 -BaseUrl http://localhost:8600 -IncludeGenerate
```
Inspects the response body (not just HTTP status) so it catches business failures the API
returns as `200` with `success:false` (e.g. "Could not find any paydays"). Fast pre-test
sanity check; not a substitute for the Playwright suite. Run against a seeded DB — `/dtp/*`
can legitimately report `success:false` on an empty database.

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

### Error Monitoring (Sentry)

`Sentry.AspNetCore` is wired via `webBuilder.UseSentry()` in `Program.cs` and configured through the `Sentry` section in `appsettings.json` (prod project `moneyman-dotnet`) and `appsettings.Development.json` (dev project `moneyman-dotnet-dev`). The DSN must be pasted into each file's `Sentry:Dsn`; an empty DSN disables Sentry with no error. The environment tag is taken from `ASPNETCORE_ENVIRONMENT`.

`SentryTestContextMiddleware` reads the `X-Test-Name` and `X-Test-Run-Id` request headers and sets them as the `test.name` / `test.run_id` Sentry tags. The Playwright suite (`test/api/fixtures.ts`) injects these headers on every request, so a failing API test can be traced to its server-side error by searching Sentry for `test.name`. Note: Sentry only captures server-side errors (unhandled exceptions / 500s), not Playwright assertion failures on successful responses.

## Test Structure

Unit tests (`Moneyman.Tests`) mirror the service/persistence layer. Playwright API tests (`test/api/`) are organized by controller with a `Base/Pages/` page object model and shared `models/` types.

Playwright tests expect the API running locally at `http://localhost:5000`. On CI, retries are set to 2.
