<#
.SYNOPSIS
  Stops any API on the port, rebuilds, restarts it detached, and waits until up.

.DESCRIPTION
  One-shot "give me a fresh running API" command. Frees the port (so the build
  isn't blocked by a locked apphost), builds, launches `dotnet run` detached so
  it survives this script, then polls api-up.ps1 until the API answers. Bundling
  these steps into a single script means one approval instead of one per step.

  Server output is redirected to api.log / api.err.log next to the project.

.PARAMETER Port
  Port the API listens on. Defaults to 5000.

.PARAMETER SkipBuild
  Restart without rebuilding (use when the build is already current).

.PARAMETER Configuration
  Build configuration. Defaults to Debug.

.EXAMPLE
  ./restart-api.ps1
  ./restart-api.ps1 -SkipBuild
#>
[CmdletBinding()]
param(
    [int]$Port = 5000,
    [switch]$SkipBuild,
    [string]$Configuration = "Debug"
)

$root    = "C:\Users\Miles\dev\moneyman-api"
$proj    = "$root\src\Moneyman.Api\Moneyman.Api.csproj"
$workDir = "$root\src\Moneyman.Api"
$log     = "$workDir\api.log"
$errLog  = "$workDir\api.err.log"

# 1. Free the port so the build can overwrite the apphost/DLLs.
$pids = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique
foreach ($processId in $pids) {
    try {
        Stop-Process -Id $processId -Force -ErrorAction Stop
        Write-Host "Stopped PID $processId on port $Port" -ForegroundColor Yellow
    } catch { }
}

# 2. Build.
if (-not $SkipBuild) {
    Write-Host "Building ($Configuration)..." -ForegroundColor Cyan
    dotnet build $proj -c $Configuration --nologo -v q
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit 1
    }
}

# 3. Start detached so the server keeps running after this script returns.
Write-Host "Starting API..." -ForegroundColor Cyan
Start-Process -FilePath "dotnet" `
    -ArgumentList @("run", "--project", $proj, "--no-build", "-c", $Configuration) `
    -WorkingDirectory $workDir `
    -RedirectStandardOutput $log `
    -RedirectStandardError $errLog `
    -WindowStyle Hidden | Out-Null

# 4. Wait for it to answer.
& "$root\src\Scripts\api-up.ps1" -Wait -TimeoutSec 90 -BaseUrl "http://localhost:$Port"
exit $LASTEXITCODE
