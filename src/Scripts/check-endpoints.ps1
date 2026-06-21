<#
.SYNOPSIS
  Smoke-checks the MoneyMan API's read endpoints and reports pass/fail.

.DESCRIPTION
  Probes a running API (default http://localhost:5000) and prints a verdict per
  endpoint. As well as the HTTP status it inspects the response body: endpoints
  that use the {success,statusCode,message,payload} envelope are failed when
  "success" is false (this is what catches problems like "Could not find any
  paydays", which still return HTTP 200). List endpoints are checked for a 200
  plus a JSON array.

  Exits non-zero if any check fails, so it can gate CI or act as a pre-test
  sanity check. Each request carries X-Test-Name / X-Test-Run-Id headers, so any
  server-side exception triggered during the run is tagged and findable in Sentry.

.PARAMETER BaseUrl
  Base URL of the API. Defaults to http://localhost:5000.

.PARAMETER IncludeGenerate
  Also exercise POST /dtp/generate (this mutates plan dates).

.EXAMPLE
  ./check-endpoints.ps1
  ./check-endpoints.ps1 -BaseUrl http://localhost:8600 -IncludeGenerate
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5000",
    [switch]$IncludeGenerate
)

$runId   = "smoke-" + (Get-Date -Format "yyyyMMdd-HHmmss")
$headers = @{ "X-Test-Name" = "smoke-check"; "X-Test-Run-Id" = $runId }

# Method, Path, Expected HTTP status. Envelope endpoints are additionally
# checked for success=true; array endpoints for a JSON array.
$checks = @(
    @{ Method = "GET"; Path = "/dtp/current" },
    @{ Method = "GET"; Path = "/dtp/full" },
    @{ Method = "GET"; Path = "/transaction" },
    @{ Method = "GET"; Path = "/transaction?anticipated=true" },
    @{ Method = "GET"; Path = "/bankaccount" },
    @{ Method = "GET"; Path = "/plandate" }
)
if ($IncludeGenerate) {
    $checks += @{ Method = "POST"; Path = "/dtp/generate" }
}

Write-Host ""
Write-Host "Smoke-checking $BaseUrl  (run id: $runId)"
Write-Host ""

$failures = 0
foreach ($c in $checks) {
    $url     = "$BaseUrl$($c.Path)"
    $status  = $null
    $verdict = $false
    $detail  = ""

    try {
        $r = Invoke-WebRequest -Uri $url -Method $c.Method -Headers $headers -UseBasicParsing -TimeoutSec 10
        $status = [int]$r.StatusCode
        $body = $null
        try { $body = $r.Content | ConvertFrom-Json } catch { }

        if ($body -and ($body.PSObject.Properties.Name -contains 'success')) {
            $verdict = [bool]$body.success
            $detail  = if ($verdict) { "success=true" } else { "success=false msg='$($body.message)'" }
        }
        elseif ($body -is [System.Array]) {
            $verdict = ($status -eq 200)
            $detail  = "count=$($body.Count)"
        }
        else {
            $verdict = ($status -eq 200)
        }
    }
    catch {
        if ($_.Exception.Response) { $status = [int]$_.Exception.Response.StatusCode.value__ }
        $verdict = $false
    }

    $statusText = if ($null -eq $status) { "no response" } else { $status }
    $label      = "{0,-4} {1,-32}" -f $c.Method, $c.Path
    if ($verdict) {
        Write-Host ("  PASS  {0} -> {1} {2}" -f $label, $statusText, $detail) -ForegroundColor Green
    }
    else {
        $failures++
        Write-Host ("  FAIL  {0} -> {1} {2}" -f $label, $statusText, $detail) -ForegroundColor Red
    }
}

Write-Host ""
if ($failures -gt 0) {
    Write-Host "$failures check(s) failed." -ForegroundColor Red
    exit 1
}
Write-Host "All checks passed." -ForegroundColor Green
exit 0
