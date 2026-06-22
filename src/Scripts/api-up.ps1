<#
.SYNOPSIS
  Reports whether the MoneyMan API is up, optionally waiting until it is.

.DESCRIPTION
  Lightweight liveness check: issues a single GET against a cheap endpoint
  (default /transaction) and reports UP/DOWN. Unlike check-endpoints.ps1 it does
  not validate business responses -- it only answers "is the API accepting
  requests?". Exits 0 when up, 1 when down (or the wait times out), so it can
  gate a test run or be polled after starting the API.

.PARAMETER BaseUrl
  Base URL of the API. Defaults to http://localhost:5000.

.PARAMETER Path
  Endpoint to probe. Defaults to /transaction.

.PARAMETER Wait
  Poll until the API responds or TimeoutSec elapses, instead of checking once.

.PARAMETER TimeoutSec
  Maximum seconds to wait when -Wait is set. Defaults to 30.

.PARAMETER IntervalSec
  Seconds between polls when -Wait is set. Defaults to 1.

.EXAMPLE
  ./api-up.ps1                      # one-shot check
  ./api-up.ps1 -Wait               # wait up to 30s for the API to come up
  ./api-up.ps1 -BaseUrl http://localhost:8600 -Wait -TimeoutSec 60
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$Path = "/transaction",
    [switch]$Wait,
    [int]$TimeoutSec = 30,
    [int]$IntervalSec = 1
)

$url = "$BaseUrl$Path"

function Test-ApiUp {
    try {
        $r = Invoke-WebRequest -Uri $url -Method GET -UseBasicParsing -TimeoutSec 5
        return [int]$r.StatusCode -lt 500
    }
    catch {
        # A 4xx still means the server is up and answering.
        if ($_.Exception.Response) { return [int]$_.Exception.Response.StatusCode.value__ -lt 500 }
        return $false
    }
}

if ($Wait) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        if (Test-ApiUp) {
            Write-Host "API is UP at $BaseUrl" -ForegroundColor Green
            exit 0
        }
        Start-Sleep -Seconds $IntervalSec
    } while ((Get-Date) -lt $deadline)

    Write-Host "API is DOWN at $BaseUrl (waited ${TimeoutSec}s)" -ForegroundColor Red
    exit 1
}

if (Test-ApiUp) {
    Write-Host "API is UP at $BaseUrl" -ForegroundColor Green
    exit 0
}
Write-Host "API is DOWN at $BaseUrl" -ForegroundColor Red
exit 1
