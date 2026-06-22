<#
.SYNOPSIS
  Runs the Playwright API suite from the test/ folder with sensible defaults.

.DESCRIPTION
  Wraps `npx playwright test` so it always runs from test/ (where the config and
  node_modules live) without a Push-Location/Pop-Location compound that triggers
  an approval each time. Defaults to a single worker, matching CI
  (playwright.config sets workers=1 on CI), which keeps the shared-SQLite suite
  deterministic -- multiple workers collide on global operations like
  POST /dtp/generate (it wipes and regenerates all plan dates).

.PARAMETER Filter
  Optional path or grep passed to Playwright (e.g. api/transaction/ or a title).

.PARAMETER Workers
  Number of workers. Defaults to 1 (serial). Raise only to reproduce parallel
  isolation issues.

.PARAMETER RepeatEach
  Run each selected test this many times (flake hunting). Defaults to 1.

.PARAMETER Reporter
  Playwright reporter. Defaults to "line".

.EXAMPLE
  ./run-tests.ps1
  ./run-tests.ps1 -Filter api/transaction/anticipated-transactions/ -RepeatEach 3
  ./run-tests.ps1 -Workers 7        # reproduce the parallel isolation failures
#>
[CmdletBinding()]
param(
    [string]$Filter = "",
    [int]$Workers = 1,
    [int]$RepeatEach = 1,
    [string]$Reporter = "line"
)

$testDir = "C:\Users\Miles\dev\moneyman-api\test"

Push-Location $testDir
try {
    $pwArgs = @("playwright", "test")
    if ($Filter) { $pwArgs += $Filter }
    $pwArgs += "--workers=$Workers"
    if ($RepeatEach -gt 1) { $pwArgs += "--repeat-each=$RepeatEach" }
    $pwArgs += "--reporter=$Reporter"

    Write-Host "npx $($pwArgs -join ' ')" -ForegroundColor Cyan
    npx @pwArgs
    $code = $LASTEXITCODE
}
finally {
    Pop-Location
}
exit $code
