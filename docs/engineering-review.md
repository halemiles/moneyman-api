# MoneyMan — Senior Engineering Review

**Date:** 2026-07-19
**Scope:** `halemiles/moneyman-api` (full solution: API, Domain, Services, Persistence, Tests) and `halemiles/moneyman-react` (full app). Both codebases were read end-to-end; findings reference `file:line` in each repo.

---

## Executive summary

The API has good bones: clean project layering, source-generated Mapperly mappers, FluentValidation, a testable `IDateTimeProvider`, a real unit-test suite with snapshot tests, a Playwright API suite, CI with coverage, and a pragmatic SQLite WAL interceptor. That puts it ahead of most hobby-scale projects.

The biggest risks are not the tech stack — they are **correctness and data-lifecycle issues in the core domain**:

1. **Regenerating plan dates destroys user state.** `POST /dtp/generate` deletes every `PlanDate` (including `Paid = true` rows) and rebuilds from scratch, so marking bills as paid is undone by the next regeneration.
2. **The generation algorithm ignores the transaction's start month and weekday.** Yearly and weekly bills are planned on the wrong dates.
3. **Several `Save()` calls are un-awaited fire-and-forget tasks**, which is both a correctness bug and the real "concurrency problem" in the codebase — more so than SQLite itself.
4. **The API↔UI contract has drifted**: the React create form posts a field the API doesn't have (`isAnticipated`), so the "Is Anticipated" switch silently does nothing and new transactions default to `Yearly` frequency.

Direct answers to the questions asked:

| Question | Short answer |
|---|---|
| SQL Server vs SQLite? | Keep SQLite for now; the concurrency pain is caused by app design (delete-all + regenerate, fire-and-forget saves), not the engine. If/when it's hosted multi-user, move to PostgreSQL or SQL Server via the EF provider swap. Details in §5.1. |
| Async on all endpoints? | Yes, but as an end-to-end change (repos → services → controllers, with `CancellationToken`). Making controllers `async` while repositories stay sync buys nothing. Fixing the un-awaited saves matters far more than adding `async` keywords. Details in §5.2. |
| .NET 10 features? | Worth adopting: keyed DI services for the strategy pattern (the "in-built strategy pattern"), `TimeProvider`, `IExceptionHandler` + `ProblemDetails`, built-in OpenAPI, `DateOnly`, EF Core 10, C# 14 extension members. Details in §6. |
| One repo or two? | Merge into a monorepo — or at minimum generate the TypeScript client from the API's OpenAPI spec in CI. The `isAnticipated` drift bug is exactly what contract-first tooling prevents. Details in §7. |

A prioritized backlog (large / medium / small) is at the end (§9).

---

## 1. Architecture review (API)

### 1.1 What's working

- **Layering** (`Api → Services → Persistence → Domain`) is consistent and dependencies point the right way. Controllers are thin and delegate to services.
- **Mapperly** over AutoMapper is the right call: compile-time, fast, no runtime config drift.
- **`IDateTimeProvider`** exists and is injected — date-based logic is testable (though not used everywhere, see §2.2).
- **`SqlitePragmaInterceptor`** (`src/Moneyman.Api/Data/SqlitePragmaInterceptor.cs`) is a genuinely good pragmatic fix: WAL + `busy_timeout` on every connection.
- **Static analysis** is centralized in `src/Directory.Build.props` (Sonar + .NET analyzers) — good hygiene.
- **Test estate** is real: unit tests with builders/snapshots, plus a Playwright API suite with Sentry test-tag correlation. That correlation middleware is a clever, unusual touch.

### 1.2 The repository pattern is leaky (Medium)

`GenericRepository<T>` (`src/Moneyman.Persistence/GenericRepository.cs`) has three structural problems:

1. **`GetAll()` returns `IEnumerable<T>` via `AsEnumerable()`** (line 33; also `PlanDateRepository.cs:19`). Every caller — `DtpService`, `PaydayService`, `PlanDateService.Search`, `TransactionController.GetAll` — loads the *entire table into memory* and filters with LINQ-to-objects. Today the tables are small; but `PlanDates` holds ~2 years × every transaction (a daily bill alone is 730 rows), and `DtpService.GetCurrent/GetOffset/GetAll` each pull the whole table per request. Either expose `IQueryable<T>`, or (better) add intent-revealing methods (`GetUnpaidBetween(start, end, bankAccountId)`) that translate to SQL.
2. **Inconsistent save semantics.** `Update()` calls `SaveChanges()` internally; `Add()`/`Remove()` don't. So `TransactionService.Update` saves twice (`TransactionService.cs:73–74`), while `Create` must remember to call `Save()`. Pick one unit-of-work convention: repositories mutate, one `SaveChangesAsync` commits.
3. **`TransactionRepository.Update` uses `_context.Update(newObject)`** (attach-and-mark-everything-modified) while the base class uses `SetValues` on a tracked entity. Two different update semantics behind one interface; the attach version also silently *inserts* when `Id == 0`, which is what accidentally makes `POST /transaction/multiple` work (see §2.6).

Honest alternative worth considering at this codebase's size: drop the generic repository entirely and inject `MoneymanContext` into services. EF Core's `DbSet` already *is* a repository/unit-of-work; the current abstraction hides the queryable and adds bugs without adding testability (the tests already use an in-memory/SQLite context for repo tests).

### 1.3 The strategy pattern is a strategy pattern in name only (Medium)

`IPlanDateGenerationStrategy` has exactly one implementation, `DefaultPlanDateGenerationStrategy`, which internally `switch`es on `Frequency` (`Strategies/DefaultPlanDateGenerationStrategy.cs:55–63`). That's the worst of both worlds: the indirection cost of a pattern with none of its payoff, plus special-case branches for `Anticipated` in *two* places (the `totalCount` ternary and the `switch`).

Recommended shape — one strategy per frequency, selected via **keyed DI** (built into `Microsoft.Extensions.DependencyInjection` since .NET 8; this is the "in-built strategy pattern" support):

```csharp
services.AddKeyedScoped<IPlanDateGenerationStrategy, MonthlyGenerationStrategy>(Frequency.Monthly);
services.AddKeyedScoped<IPlanDateGenerationStrategy, WeeklyGenerationStrategy>(Frequency.Weekly);
services.AddKeyedScoped<IPlanDateGenerationStrategy, OneOffGenerationStrategy>(Frequency.Anticipated);
// consumer:
public DtpService([FromKeyedServices] IServiceProvider sp) { ... }
var strategy = sp.GetRequiredKeyedService<IPlanDateGenerationStrategy>(transaction.Frequency);
```

Each strategy then owns its own occurrence math (weekly alignment, month clamping, one-off expiry), which fixes the correctness bugs in §2.1 as a side effect and makes the `Frequency.Anticipated` special-casing disappear. Alternatively, if separate classes feel heavy, at least extract per-frequency date sequences (`IEnumerable<DateTime> Occurrences(Transaction t, int horizonYears)`) so the switch is the *only* frequency-aware code.

Note: the `GenerationStrategy` enum mentioned in `CLAUDE.md` does not exist in `Moneyman.Domain/Enums/`, and neither does the documented `WeekdayService` — the docs have drifted (Small, §8.4).

### 1.4 `ApiResponse<T>` + HTTP 200-for-everything (Medium)

The API wraps results in `ApiResponse<T>` with an internal `StatusCode` but then mostly returns `Ok(...)` regardless — `DtpController.Generate` even catches all exceptions and returns 200 with a message (`DtpController.cs`, `catch` → `Ok(new DtpHttpResponse{...})`). Your own `check-endpoints.ps1` exists specifically to compensate ("catches business failures the API returns as 200 with success:false") — that script is a symptom.

Recommendation: return real status codes and use ASP.NET's `ProblemDetails` (RFC 7807) for errors, with a global `IExceptionHandler` (built-in since .NET 8) instead of per-controller try/catch. Keep `ApiResponse<T>` as a success envelope if the UI depends on it, but stop encoding failures as 200s. Note `ApiResponse.Success` treats `NoContent` as success while `GetCurrent` uses `NoContent` for "no plan dates — please regenerate", which the UI can't distinguish from a genuinely empty period.

### 1.5 Three observability stacks, one configured (Small)

`Moneyman.Api.csproj` references **Application Insights**, **Sentry**, and **Serilog + Seq + Console sinks**. Only Sentry is actually wired (`UseSentry()`); Serilog is never `UseSerilog()`'d anywhere, and the AppInsights connection string is empty. Pick one (Sentry for errors + built-in `ILogger` console logging is plenty) and delete the other packages — the Serilog 2.x/6.x pins are also years old.

### 1.6 Old-style hosting (Small)

`Program.cs`/`Startup.cs` use the .NET Core 3.1-era `Host.CreateDefaultBuilder` + `Startup` convention. Migrate to minimal hosting (`WebApplication.CreateBuilder`), which removes the `#pragma warning disable S2325` workaround, simplifies Sentry/middleware ordering, and is a prerequisite for some newer platform features. Mechanical, low-risk change.

---

## 2. Domain logic — correctness findings

These are the highest-value findings in the review.

### 2.1 Plan-date generation ignores the transaction's month and weekday (Large — correctness bug)

`DefaultPlanDateGenerationStrategy.cs:47–49`:

```csharp
int year = DateTime.Now.Year;
int day = transaction.StartDate.Day;
DateTime seedDate = new DateTime(year, 1, day);   // always January!
```

Only the *day-of-month* of `StartDate` survives; the month is discarded and every schedule seeds from January of the current year:

- **Yearly**: a bill genuinely due every June 15th gets plan dates on **January 15th** of each year (`seedDate.AddYears(i)`).
- **Weekly**: a bill paid every Friday gets `Jan {day}, +7, +14, …` — the weekday of the actual start date is irrelevant, so the whole series lands on whatever weekday `Jan {day}` happens to be.
- **Monthly**: works by luck (starting from January and generating 24 months covers the right days), but generates dates *before* the transaction's start date, and a start day of 29–31 silently clamps in short months with no "last day of month" intent captured.
- **Daily** generates 730 rows starting Jan 1 regardless of when the bill starts.

Fix: seed each series from the actual `StartDate` (advanced into the generation window), per-frequency:

```csharp
// monthly: startDate, startDate.AddMonths(1)…; weekly: startDate.AddDays(7*i); yearly: startDate.AddYears(i)
```

and skip occurrences `< StartDate`. This pairs naturally with the per-frequency strategies from §1.3. Also: this method uses `DateTime.Now` directly despite `IDateTimeProvider` existing and being injected two doors down — inject it here too, otherwise the unit tests for this class are date-sensitive (Small).

### 2.2 Regeneration wipes `Paid` state (Large — data-loss design flaw)

`DtpService.GenerateAll` (`DtpService.cs:59`) calls `planDateRepository.RemoveAll()` — an immediate `ExecuteDelete` — then regenerates and saves. Consequences:

1. **Every `Paid = true` flag set via `PATCH /plandate/{id}/paid` is destroyed** on the next generate. The mark-as-paid feature and the generate feature are mutually corrupting.
2. `ExecuteDelete` commits **outside** the change-tracker transaction, so if generation or `Save()` then throws (the catch at `DtpService.cs:77–80` swallows it and *still returns Success*), you're left with an empty `PlanDates` table and a 200 response saying "Successfully generated plandates".
3. Passing `transactionId` to regenerate *one* transaction still deletes **all** transactions' plan dates, then regenerates only that one. `POST /dtp/generate?transactionId=5` silently deletes every other bill's schedule.

Recommended redesign — make generation an **idempotent upsert**:

- Give `PlanDate` a real `TransactionId` FK + a unique index on `(TransactionId, OriginalScheduledDate)`.
- Generation computes the desired set, then inserts missing rows, updates changed dates, deletes rows whose occurrence no longer exists — *preserving `Paid` on surviving rows*.
- Wrap the whole thing in an explicit transaction (`context.Database.BeginTransactionAsync()`), which also fixes (2).
- Scope deletion to `transactionId` when one is supplied, fixing (3).

This one change removes the biggest data-integrity risk in the system and, incidentally, most of the write-concurrency pressure that motivates the SQL Server question (§5.1).

### 2.3 Holiday horizon is shorter than the plan-date horizon (Medium — correctness)

`BankHolidayInitializerHostedService.cs:52` only stores bank holidays up to **18 months** ahead (`DateTime.UtcNow.AddMonths(18)`), but plan dates and paydays are generated **24 months** ahead (`TotalPlanDateYears = 2`, `TotalPaydayMonths = 24`). Months 19–24 of every schedule get weekend-shifting but **no holiday-shifting**, silently. Align the horizons (single shared constant, e.g. `GenerationOptions.HorizonMonths`) or generate only as far as you have holiday data.

### 2.4 Offset checks (`OffsetCalculationService`) — asked about specifically

The core roll-forward-to-next-working-day logic is correct for UK direct debits. Issues, in order of importance:

1. **Callers ignore `IsValid`.** After 10 failed iterations the loop bails and returns `IsValid = false` with a *known-invalid* date — and both `PaydayService.Generate` and `DefaultPlanDateGenerationStrategy` use `.PlanDate` unconditionally. Ten consecutive non-working days can't happen in the UK calendar, so it's latent — but either throw/propagate the failure or log it; silently persisting an invalid date is the worst option. (Medium)
2. **Holiday lookup is `List<string>.Contains` on `"dd-MM-yyyy"` strings** (`Moneyman.Extensions/IsMonday.cs` — note the file is named `IsMonday.cs` but contains `DateVerificationExtensions`). Each generation run re-fetches the list (`GenerateHolidays()` per `CalculateOffset` call) and does an O(n) string scan per candidate day. Use a `HashSet<DateOnly>` populated once — faster and no format-string coupling between `HolidayService` and the extension. (Small)
3. **Direction is hardcoded forward.** Some obligations (payday itself in many orgs, standing orders configured "before weekend") roll *backward*. Making direction a parameter (`RollForward`/`RollBackward`) is cheap now and unlocks per-transaction behaviour later. (Small)
4. The mutable `CalculatedPlanDate` accumulating state through the loop (`Reason`, `OffsetBy` overwritten each pass) would be clearer as a simple loop returning a record at the end. (Small)

### 2.5 Anticipated transactions — asked about specifically

Currently `Anticipated` is a value of the `Frequency` enum, which is a category error: it's not a frequency, it's a *recurrence kind* (one-off vs recurring). The cost of that modelling shows up everywhere:

- `FrequencyExtensions.ToFrequencyCount` returns `0` for it (dead default branch), so the strategy special-cases `totalCount = 1` *and* special-cases the date in the `switch`.
- `TransactionController.GetAll` needs an `anticipated` query flag to filter them in/out of the main list.
- The current-year guard in `GenerateAll` (`DtpService.cs:52–56`) refuses to generate **anything** unless *some* transaction starts in the current year — an anticipated bill added in December for next January contributes nothing to satisfying the guard, and conversely the guard's error message doesn't match its actual condition (it checks "any transaction in current year", not "this transaction"). Consider scoping the guard per-transaction or dropping it in favour of "generated 0 rows" reporting.
- Nothing expires them: an anticipated bill whose date has passed but was never marked paid stays in `/dtp/all` forever.

Recommended model: keep `Frequency` for genuine recurrences and add `bool IsOneOff` (or an `OccurrenceKind` enum) to `Transaction` — the React form's "Is Anticipated" switch maps to it directly (fixing the §4.1 contract bug at the same time). Give one-offs an explicit lifecycle: after `Date + grace`, either auto-mark `Missed` or surface them in a "needs attention" bucket instead of silently rolling forward in every view.

### 2.6 Assorted service-layer bugs (all fixable in an afternoon)

- **`TransactionService.Update` can't update most fields** (`TransactionService.cs:39–77`): it copies `Name`, `Amount`, `StartDate`, `Frequency`, `Active` — but **not** `PaymentType`, `CategoryType`, `PriorityType`, or `BankAccountId`. `PUT /transaction` silently ignores those. Also `Amount` can never be corrected downward to a value ≤ 0 and `StartDate` can't be cleared, by design of the guards — fine, but undocumented.
- **`POST /transaction/multiple` is an update path, not a create path** (`TransactionController.CreateMultiple` → `transactionService.Update(list)`). It only works for creation because `_context.Update` on an `Id == 0` entity happens to insert. No validation runs on this path either — `TransactionDtoValidator` is bypassed entirely.
- **`PaydayService.GetNext/GetPrevious` return null and `DtpService.GetCurrent` handles it by catching `NullReferenceException`** (`DtpService.cs:88–96`). `GetOffset` null-checks properly; `GetCurrent` uses exception-driven control flow. Return `Payday?` and check, or introduce a `TryGetNext`.
- **Boundary semantics of `IsDueBetween`** (`DtpService.cs:127–135`): strictly `>` start and `<` end means a bill due *today* is excluded from "due till payday", and a bill due *on payday itself* belongs to neither period (both windows exclude their endpoints, and adjacent periods share the payday endpoint). Decide the convention (typically `>= start`, `< end`) and test the boundaries explicitly.
- **`WeeksRemaining` is integer division** (`(end - start).Days / 7`): 6 days until payday → 0 weeks → clamped to 1 → "spend per week" equals the whole remaining balance. Consider `Math.Ceiling(days / 7.0)` or exposing days.
- **`PlanDate` has no `Amount` snapshot**: DTP amounts always read through to the *current* `Transaction.Amount`, so editing a bill's amount retroactively changes what past periods say was due. Copy `Amount` onto `PlanDate` at generation if historical accuracy matters.
- **`PlanDateService.Search`** does exact-match `x.Transaction.Name == transactionName` in memory — probably intended `Contains`, and belongs in SQL.
- **`Entity`/`BaseTransaction` in `Interfaces/ITransaction.cs`**: a class named like an interface file, in an `Interfaces` folder. Move/rename (Small).

---

## 3. Startup behaviour (hosted services)

- `PaydayInitializerHostedService` calls `paydayService.Generate(...)` on **every startup**, which does `RemoveAll()` + re-insert. Two problems: (a) any manual payday adjustment is wiped on restart; (b) if two instances ever run (or a restart races the Playwright suite), they race on delete/insert. Make it idempotent — regenerate only when the table is empty or the horizon has shrunk below N months.
- `PaydayService.Generate` **doesn't await `Save()`** (`PaydayService.cs:51` — `_paydayRepository.Save();` returns a `Task` that is dropped). The scope created in the hosted service can dispose the `DbContext` while `SaveChangesAsync` is mid-flight — this is a genuine race, not a style nit. Same pattern in `TransactionService` (`TransactionService.cs:74, 86, 93, 102` — only line 130 awaits). Make `Save()` awaited everywhere or you will eventually see `ObjectDisposedException`/lost writes under load. **This is the single most impactful "concurrency" fix available.**
- `BankHolidayInitializerHostedService` blocks startup on an external HTTP call to gov.uk (with good error handling, to be fair). Consider `BackgroundService` + retry, so the API is up even if gov.uk is slow; the DB-backed cache fallback already makes this safe.

---

## 4. React app review

### 4.1 API contract drift — the "Is Anticipated" switch does nothing (Large — bug)

`TransactionCreate.js:17` posts `data.isAnticipated = ...`, but `TransactionDto` has no such property — it has `Frequency`. The flag is silently dropped by model binding, and since the form never sends `frequency`, Mapperly maps the null through and **every transaction created from the UI gets `Frequency = Yearly` (enum value 0)**. Related: `amount` comes out of `FormData` as a *string* (`"12.50"`), and System.Text.Json will not bind a JSON string to `decimal?` by default — verify this path actually succeeds at all; it may be returning 400s that the `.then((res) => res.json())` chain swallows. This whole class of bug is the monorepo/contract argument in §7.

### 4.2 Structural issues

- **CRA (`react-scripts 5.0.0`) is deprecated and unmaintained.** Migrate to **Vite** (mechanical for an app this size) — faster dev server, maintained toolchain, first-class TS. This is the highest-value frontend infrastructure change.
- **No API layer.** `fetch` calls with interpolated `process.env.REACT_APP_MONEYMAN_SERVER_URL` are scattered through components; `Controls.js:52` hardcodes `http://localhost:5000` outright (breaks any non-local deployment of that dropdown). Centralize in one generated or hand-written client module.
- **`data/DutTillPayday.ts`** (typo'd filename) mixes `await` with `.then`, takes a `currentBalance` param it never uses, is named `handlePostRefresh` but performs GETs, and returns `any`. It's the de-facto API client — make it a real one.
- **TypeScript adoption is one stray file.** Either commit to TS (recommended, especially with a generated API client) or stay JS; the half-state gives no type safety and complicates the build.
- **Hardcoded bank accounts** in `Controls.js` (`Natwest`/`Starling`, ids 1/2) — the API has `GET /bankaccount`; fetch it.
- **React keys**: `Transactions.js:53` uses `key={uuidv4}` — that's the *function reference*, identical for every row (React will warn about duplicate keys). `DueTillPaydayGrid.js` calls `uuidv4()`, which "works" but defeats reconciliation — every render remounts every row. Use `transaction.id` / `planDate.id`.
- **`window.location.reload()` after delete** (`Transactions.js`) instead of updating state; full page reload on every delete.
- **No error handling on any fetch** — a failed API call leaves the UI silently empty; add error states (and remove the `console.log`s).
- **Dependency hygiene** (`package.json`): `winston` + `@datalust/winston-seq` are Node-side loggers that do nothing in a CRA browser bundle; `playwright` and `npm-check-updates` belong in `devDependencies`; `axios` is present but never used (everything is `fetch`); `http-proxy-middleware` powers a `SetupProxy.js` whose `target` includes a path+query string (not how `target` works — it's ignored beyond the origin) and which nothing appears to use.
- **Dockerfile ships the dev server** (`npm start` in the container). Production containers should `npm run build` and serve static files (nginx or `serve`) — smaller, faster, no dev-server security surface.
- **Money math in floats** (`Summary.js` `toFixed(2)` on JS numbers): fine at this scale, but keep all derived-total logic on the API (it already computes `AmountDue`/`Remaining`/`SpendPerWeek`) instead of re-summing client-side in floating point — the two currently disagree in edge cases (the UI re-sums `planDates` while the API's `startingValue` is hardcoded to `1` in every call, making the API's `Remaining`/`SpendPerWeek` fields meaningless as consumed today).
- **Routing uses `<a href>` and `Button href`** for internal navigation (full page reloads) despite `react-router-dom` being installed — use `<Link>`/`useNavigate` consistently.
- `burndown-chart.js` computes a running total assuming `/dtp/full` returns dates sorted ascending — the API doesn't guarantee ordering (`GetOffset` returns repository order). Sort client-side or order in the API.

---

## 5. Concurrency, database engine, and async — asked about specifically

### 5.1 SQLite vs SQL Server

**Recommendation: keep SQLite; fix the design issues first; if you outgrow it, choose PostgreSQL over SQL Server unless you specifically want the MS stack.**

Reasoning:

- Your actual observed concurrency failures ("parallel Playwright workers collide on the single writer") were already mitigated correctly by WAL + `busy_timeout`. What remains is *application-level*: `RemoveAll()`-then-rebuild patterns (§2.2, §3) that hold long write bursts and destroy state, and fire-and-forget saves (§3) that race scope disposal. **Moving to SQL Server would not fix either** — you'd just get deadlocks instead of `SQLITE_BUSY`.
- SQLite's real limits for this app: single-writer (fine for a household app), no server = no independent scaling, and `ExecuteDelete`/date functions behave slightly differently across providers. None are pressing.
- Triggers to actually migrate: hosting the API for multiple concurrent users, wanting managed backups/PITR, needing multiple app instances against one DB. At that point the EF Core migration is mostly `UseSqlite` → `UseNpgsql`/`UseSqlServer` plus: regenerate migrations, drop the pragma interceptor, re-verify `ExecuteDelete`, and check `DateTime` column mappings (move to `DateOnly`/`timestamptz` deliberately — see §6).
- Practical middle step regardless of engine: **adopt EF Core migrations as the schema source of truth** (currently the schema lives in `src/Scripts/files/` shell-applied SQL, and `MigrationsAssembly("Moneyman.Api")` is configured but no migrations exist). One source of truth, and `context.Database.Migrate()` on startup replaces `intitialise-db.sh`.

### 5.2 "Do we need async on all endpoints?"

Async endpoints improve **throughput under concurrent load** (threads aren't parked on I/O), not single-request latency. For a self-hosted household app the measurable win is small — but the codebase should still go async, for correctness reasons more than performance ones:

1. **Today it's the worst of both worlds**: the stack is *sync* (`SaveChanges`, `Find`, in-memory `GetAll`) except for `Save()` which is async and then **not awaited** in four call sites (§3). That's not "sync but simple", it's "async but broken".
2. Going async properly is an end-to-end change: `GetAllAsync/FindAsync/ToListAsync/SaveChangesAsync` in repositories → `Task`-returning service methods → `async` controller actions **with `CancellationToken` threaded through** (ASP.NET injects it; EF Core honours it). Doing only the controller layer is cosmetic.
3. Sequencing: fix the un-awaited saves *first* (bug fix), then convert layer by layer, persistence up. The repository interface (`IRepository<T>`) is small; this is a bounded, mechanical refactor and a good forcing function for the unit-of-work cleanup in §1.2.

One caveat: SQLite's ADO.NET provider executes "async" operations synchronously under the hood, so with SQLite the throughput gain is ~zero — the payoff arrives when the provider changes (§5.1) and the code is already shaped correctly.

### 5.3 Other concurrency notes

- `HolidayService`'s double-checked lock is correct; on .NET 9 use the dedicated `System.Threading.Lock` type instead of `object` (analyzer-guided, trivial).
- `BankHolidayCache` swaps an immutable list reference — fine. Consider replacing the hand-rolled cache with `HybridCache`/`IMemoryCache` (§6) only if you add expiry; otherwise leave it.
- `GenerateAll` has a read-modify-write race with concurrent `MarkAsPaid` (paid flag set between delete and re-insert is lost even after the §2.2 upsert fix unless the upsert runs in a transaction with appropriate conflict handling). The §2.2 transaction covers this.

---

## 6. .NET 10 / modern platform features worth adopting

The solution targets `net9.0`. .NET 10 is the current LTS — retarget when convenient (the upgrade is trivial for this codebase). Features that would concretely improve *this* project:

| Feature | Since | Applies to |
|---|---|---|
| **Keyed DI services** (`AddKeyedScoped`, `[FromKeyedServices]`) | .NET 8 | The frequency-strategy selection in §1.3 — this is the framework's "in-built strategy pattern" and the single best-fit feature for this codebase. |
| **`TimeProvider`** + `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`) | .NET 8 | Replaces the custom `IDateTimeProvider` with the standard abstraction; tests get a supported fake with time-travel. Also removes the `DateTime.Now` stragglers (§2.1). |
| **`IExceptionHandler` + `ProblemDetails` service** | .NET 8 | Replaces per-controller try/catch and the 200-for-errors pattern (§1.4). |
| **Built-in OpenAPI (`Microsoft.AspNetCore.OpenApi`) + Scalar/Swagger UI** | .NET 9/10 | Replaces Swashbuckle 6.5 (old pin, community-maintenance limbo). Also the source for the generated TS client in §7. |
| **`DateOnly`** | EF Core 8+ maps it on SQLite | `Payday.Date`, `PlanDate.Date`, `BankHoliday.Date`, and the whole offset service are date-only concepts currently modelled as `DateTime` — midnight/TZ bugs waiting to happen (note `GetNext()` compares against `GetNow()`, i.e. a time-of-day-sensitive comparison against date-only values). |
| **EF Core 10** | .NET 10 | Named query filters (e.g. a global `Active`/`!Paid` filter you can disable per-query), `ExecuteUpdateAsync` improvements for the upsert in §2.2, LeftJoin operator. |
| **C# 14 extension members** | .NET 10 | `FrequencyExtensions`/`DateVerificationExtensions` become extension blocks; also lets you hang `Occurrences(...)` on `Frequency` cleanly. |
| **`System.Threading.Lock`** | .NET 9 | `HolidayService._populateCacheLock` (§5.3). |
| **`HybridCache`** | .NET 9 | Bank-holiday cache *if* you add expiry/refresh; otherwise skip. |
| **Output caching middleware** | .NET 7 | `GET /dtp/*` responses are pure functions of DB state; `AddOutputCache` with tag-based eviction on generate/update would remove most read load. Optional at this scale. |
| **Rate limiting middleware** | .NET 7 | Cheap protection for the destructive endpoints if the API is ever exposed (§8.1). |

Not recommended: Native AOT (EF Core-based apps aren't good candidates yet); minimal APIs conversion (controllers are fine here — convert only if you're rewriting anyway).

---

## 7. One repo or two? — asked about specifically

**Recommendation: merge into a monorepo.** For a solo-maintainer project where the API and UI ship together and change together, two repos is mostly friction:

- **The §4.1 contract bug is the case study**: `isAnticipated` was added to the UI, the API never learned about it, and nothing failed. In a monorepo a single PR changes DTO + form + tests together, and cross-repo drift becomes diff-visible.
- Layout: `/api` (current `src` + `test`), `/web` (React app), one root README, one issue tracker, CI with path filters (`paths: api/**` / `paths: web/**`) so pipelines stay independent. The existing Playwright *API* suite and the React repo's Playwright *E2E* suite (which currently duplicates transaction API coverage) can be rationalized into one `/e2e` project that spins up both containers via docker-compose.
- **Whether or not you merge, adopt contract-first API typing**: publish `swagger.json` as a CI artifact and generate the TypeScript client (`openapi-typescript` + `openapi-fetch`, or NSwag) into the web app. That gives compile-time failure on drift even across two repos, and it's the natural replacement for `DutTillPayday.ts`.

Reasons you'd keep two repos: independent release cadences, different access control, or multiple consumers of the API. None apply today. (Note the deploy workflows already both push versioned Docker images — a monorepo keeps that, just with two Dockerfiles.)

---

## 8. Security & operations

### 8.1 No authentication on destructive endpoints (Large if ever exposed)

`DELETE /transaction/all`, `DELETE /transaction/{id}`, and `POST /dtp/generate` (which deletes all plan dates, §2.2) are unauthenticated. CORS restriction (`RestrictedOriginsPolicy`) is **not** access control — it only constrains browsers, not `curl`. Also note: when `Cors:AllowedOrigins` is empty in production, the policy is registered with *no* origins configured, which silently blocks the UI rather than failing loudly at startup — validate config and fail fast. If this API is reachable beyond localhost, add at minimum an API key middleware or reverse-proxy auth (Authelia/Caddy basic auth in front is fine for homelab use), and consider removing `DELETE /transaction/all` outside Development.

### 8.2 Sentry `SendDefaultPii: true` + `MaxRequestBodySize: Always`

Full request bodies (transaction names, amounts — personal finance data) are shipped to Sentry on errors. Deliberate trade-off perhaps, but worth an explicit decision; consider `SendDefaultPii: false` and scrubbing.

### 8.3 CI/CD (Small items)

- `actions/checkout@v3` / `setup-dotnet@v2` are outdated majors — bump to v4/v5.
- The API deploy workflow pushes only `:latest` — add the version/SHA tag (the react repo's release workflow already does this properly).
- The `Debug` `ls` step in `main.yml` is leftover scaffolding — delete.
- Playwright API tests don't run in API CI (no workflow boots the API + runs `test/`); the smoke script exists but isn't in CI either. A `docker-compose up` + `check-endpoints.ps1` + Playwright job would catch the §4.1-class regressions.

### 8.4 Documentation drift (Small)

`CLAUDE.md` references `WeekdayService` and a `GenerationStrategy` enum that don't exist, and describes DTP as bucketing "before/after payday" while the code models it as a single window between paydays. Fix the doc — it steers future automated/human contributors wrong.

---

## 9. Prioritized backlog

### Large (design-level, highest payoff)

| # | Item | Ref |
|---|---|---|
| L1 | Make plan-date generation an idempotent, transactional upsert that preserves `Paid`; scope by `transactionId`; stop returning success when save fails | §2.2 |
| L2 | Fix generation seeding so yearly/weekly/monthly schedules honour the transaction's actual start month/weekday and don't emit pre-start occurrences | §2.1 |
| L3 | Remodel "anticipated" as a one-off occurrence kind (not a `Frequency`), with an explicit expiry/missed lifecycle; fix the UI's `isAnticipated` contract drift in the same change | §2.5, §4.1 |
| L4 | Await every `Save()`; convert persistence→services→controllers to true async with `CancellationToken` | §3, §5.2 |
| L5 | Adopt contract-first typing (OpenAPI-generated TS client) and merge the repos into a monorepo | §7 |
| L6 | Add authentication (or reverse-proxy auth) before any non-localhost exposure; gate `DELETE /transaction/all` | §8.1 |
| L7 | Migrate the React app off deprecated CRA to Vite (+ commit to TypeScript) | §4.2 |

### Medium

| # | Item | Ref |
|---|---|---|
| M1 | Replace the leaky generic repository with intent-revealing query methods (or direct `DbContext` use); unify update/save semantics | §1.2 |
| M2 | Real strategy-per-frequency via keyed DI; delete the `Anticipated` special cases | §1.3, §6 |
| M3 | Real HTTP status codes + `ProblemDetails` + global `IExceptionHandler`; stop encoding failures as 200 | §1.4 |
| M4 | Align holiday horizon (18 mo) with generation horizon (24 mo) via one shared constant | §2.3 |
| M5 | Adopt EF Core migrations + `Database.Migrate()` on startup; retire the shell-script schema | §5.1 |
| M6 | `TransactionService.Update`: map `PaymentType`/`CategoryType`/`PriorityType`/`BankAccountId`; route `POST /transaction/multiple` through validation and a real create path | §2.6 |
| M7 | Make payday initialization idempotent (don't wipe/regenerate on every boot); return `Payday?` and remove exception-driven null handling in `GetCurrent` | §3, §2.6 |
| M8 | Frontend API layer: one client module, no hardcoded URLs, error states, API-computed totals instead of client-side float sums | §4.2 |
| M9 | Fix DTP boundary semantics (`>=` start, `<` end) and `WeeksRemaining` integer division; add boundary tests | §2.6 |
| M10 | Run the Playwright API suite (or at least the smoke script) in CI against a composed API | §8.3 |

### Small

| # | Item | Ref |
|---|---|---|
| S1 | Propagate/`log` `IsValid = false` from `OffsetCalculationService` instead of persisting invalid dates; `HashSet<DateOnly>` holiday lookup; parameterize roll direction | §2.4 |
| S2 | Inject `TimeProvider` (replacing `IDateTimeProvider` and the stray `DateTime.Now` in the strategy) | §2.1, §6 |
| S3 | Migrate to minimal hosting (`WebApplication.CreateBuilder`); drop `Startup` + `#pragma S2325` | §1.6 |
| S4 | Remove unused Serilog/Seq + Application Insights packages (keep one observability stack) | §1.5 |
| S5 | Replace Swashbuckle with built-in OpenAPI + Scalar; retarget `net10.0` | §6 |
| S6 | `DateOnly` for `Payday`/`PlanDate`/`BankHoliday` dates | §6 |
| S7 | Snapshot `Amount` onto `PlanDate` at generation | §2.6 |
| S8 | `PlanDateService.Search`: `Contains` + translate to SQL | §2.6 |
| S9 | Rename `IsMonday.cs` → `DateVerificationExtensions.cs`; move `BaseTransaction` out of `Interfaces/ITransaction.cs`; fix `Extentions` namespace typo | §2.4, §2.6 |
| S10 | React: stable keys (`id`, not `uuidv4`), `<Link>` navigation, state update instead of `location.reload()`, fetch bank accounts from API, remove `console.log`s | §4.2 |
| S11 | React: dependency cleanup (drop winston/axios/n-c-u from deps; move playwright to devDeps); fix or remove `SetupProxy.js`; production Dockerfile (build + static serve) | §4.2 |
| S12 | Sort `/dtp` results by date server-side (burndown chart assumes ordering) | §4.2 |
| S13 | CI: bump action versions, tag Docker images with version/SHA, remove debug step | §8.3 |
| S14 | Update `CLAUDE.md`/docs (no `WeekdayService`, no `GenerationStrategy` enum, DTP description) | §8.4 |
| S15 | Revisit Sentry `SendDefaultPii`/`MaxRequestBodySize` for finance data | §8.2 |
| S16 | `System.Threading.Lock` in `HolidayService`; validate CORS config at startup (fail fast on empty prod origins) | §5.3, §8.1 |

---

## Suggested sequencing

1. **Correctness first (1–2 sessions):** L4 (awaited saves) → L1 + L2 with tests (the existing snapshot-test infrastructure makes locking in the new generation behaviour easy) → M4, M9.
2. **Contract (1 session):** L3 + the UI form fix, then L5's generated client so it can't regress.
3. **Platform (as convenient):** M1–M3, M5, then the Small list opportunistically — most are sub-hour changes.
4. **Frontend (parallel track):** L7 (Vite) first, then M8, then S10/S11.

The unit-test suite is the project's best asset for this work: nearly every Large/Medium item above can be characterization-tested before refactoring using the existing builders and snapshot helpers.
