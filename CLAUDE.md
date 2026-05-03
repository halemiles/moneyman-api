# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

MoneyMan API is an ASP.NET Core 9.0 REST API for personal finance management. It tracks transactions, bank accounts, plan dates, and paydays, with date prediction (DTP) logic for payday scheduling around UK bank holidays.

## Build & Run

```bash
# From /src directory
dotnet restore
dotnet build
dotnet run --project Moneyman.Api
```

```bash
# Docker (port 8600)
docker-compose up
```

## Testing

**Unit tests** (MSTest + FluentAssertions + Moq + AutoFixture):
```bash
cd src
dotnet test
dotnet test --filter "FullyQualifiedName~ServiceTests"   # run a specific category
dotnet test --filter "FullyQualifiedName~MyTestName"     # run a single test
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
./Scripts/generate-coverage.sh                           # HTML coverage report
```

**E2E/Playwright tests** (against a running API):
```bash
cd test
npm install
npx playwright install
npm test
npm run test:ui      # interactive UI mode
```

## Architecture

The solution (`src/Moneyman.Api.sln`) is structured as 6 projects with clear layer separation:

| Project | Role |
|---|---|
| `Moneyman.Api` | ASP.NET Core host, controllers, startup, DI composition root |
| `Moneyman.Domain` | EF Core `MoneymanContext`, entity models, DTOs |
| `Moneyman.Services` | Business logic, FluentValidation validators |
| `Moneyman.Persistence` | Generic + typed repository implementations |
| `Moneyman.Extensions` | Shared extension methods |
| `Moneyman.Tests` | Unit tests mirroring the above layers |

**Data flow**: Controller → Service → Repository → EF Core (SQLite)

**DI registration** is split into extension methods called from `Startup.cs`:
- `AddRepositories()`, `AddServices()`, `AddMappers()`, `SetupContexts()`, `SetupHolidays()`, `SetupPayday()`

## Key Patterns

**Repository pattern**: Generic `IRepository<T>` with scoped typed repositories per entity (`ITransactionRepository`, `IBankAccountRepository`, etc.).

**Mapping**: [Riok.Mapperly](https://mapperly.riok.app/) for compile-time code-generated mappers (`BankAccountMapper`, `TransactionMapper`, `PlanDateMapper`). Add new mappings in the mapper classes, not manually.

**Configuration via Options pattern**:
- `PaydayOptions` — day of month payday falls on
- `HolidayOptions` — list of UK bank holiday dates used by DTP logic

**Startup initialisation**: `PaydayInitializerHostedService` seeds paydays on application start.

**Database**: SQLite (`LocalDatabase.db`) via EF Core. Migration assembly is `Moneyman.Api`. The DB file is volume-mounted in docker-compose.

## Test Conventions

- `Moneyman.Tests` uses EF Core InMemory provider for repository tests (not the real SQLite DB).
- AutoFixture generates test data; Moq mocks service/repository dependencies.
- Snapper is used for snapshot assertions — update snapshots with `Snapper.UpdateSnapshots = true` when intentionally changing output shapes.
- Tests are organized by layer: `RepositoryTests/`, `ServiceTests/`, `DomainTests/`, `MapperTests/`, `ValidatorTests/`, `StrategyTests/`.
