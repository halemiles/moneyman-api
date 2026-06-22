<#
.SYNOPSIS
  Resets LocalDatabase.db to a clean state; paydays reseed on API startup.

.DESCRIPTION
  Stops the API (it holds the DB file open), deletes the database plus any
  WAL/SHM sidecars, recreates the schema from src/Scripts/files/schema.sql via
  Python's sqlite3 (the sqlite3 CLI isn't installed on this box), then -- unless
  -NoRestart is given -- restarts the API. Paydays are seeded automatically:
  PaydayInitializerHostedService calls PaydayService.Generate(PaydayOptions.DayOfMonth)
  on every startup, so restarting against the empty DB repopulates the Paydays
  table. This matters because most specs assume paydays already exist (only the
  anticipated spec tries to make them, via a /payday/generate route that doesn't
  actually exist). seed-data.sql is intentionally skipped (it's SQL Server syntax).

.PARAMETER Port
  API port. Defaults to 5000.

.PARAMETER NoRestart
  Recreate the schema only; leave the API stopped (caller restarts when ready).

.EXAMPLE
  ./reset-db.ps1
  ./reset-db.ps1 -NoRestart
#>
[CmdletBinding()]
param(
    [int]$Port = 5000,
    [switch]$NoRestart
)

$root   = "C:\Users\Miles\dev\moneyman-api"
$db     = "$root\src\Moneyman.Api\LocalDatabase.db"
$schema = "$root\src\Scripts\files\schema.sql"

# 1. Stop the API so the DB file isn't locked.
$pids = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique
foreach ($processId in $pids) {
    try { Stop-Process -Id $processId -Force -ErrorAction Stop; Write-Host "Stopped PID $processId" -ForegroundColor Yellow } catch { }
}
Start-Sleep -Milliseconds 500

# 2. Remove the database and its WAL/SHM sidecars.
foreach ($f in @($db, "$db-wal", "$db-shm")) {
    if (Test-Path $f) { Remove-Item $f -Force }
}
Write-Host "Removed old database files" -ForegroundColor Yellow

# 3. Recreate the schema with Python's sqlite3.
$py = @"
import sqlite3
con = sqlite3.connect(r'$db')
with open(r'$schema', 'r', encoding='utf-8') as f:
    con.executescript(f.read())
con.commit()
con.close()
print('Schema applied')
"@
$py | python -
if ($LASTEXITCODE -ne 0) {
    Write-Host "Schema load failed." -ForegroundColor Red
    exit 1
}
Write-Host "Recreated schema from schema.sql" -ForegroundColor Green

# 4. Restart the API; the hosted service reseeds paydays on startup.
if (-not $NoRestart) {
    & "$root\src\Scripts\restart-api.ps1" -SkipBuild
    exit $LASTEXITCODE
}

Write-Host "Database reset complete (API not restarted)." -ForegroundColor Green
exit 0
