<#
.SYNOPSIS
    Applies EF Core migrations to the target SQL Server database.

.DESCRIPTION
    This script applies all pending Entity Framework Core migrations to the configured database.
    It uses the connection string from user-secrets or appsettings, or you can override it via parameter.

.PARAMETER ConnectionString
    Optional. Override the connection string (SQL Auth format).
    Example: "Server=sql-sro-dev-ne.database.windows.net;Database=sro-db;User Id=sroadmin;Password=MyP@ss;TrustServerCertificate=True"

.EXAMPLE
    .\scripts\deploy-db.ps1
    # Uses connection string from user-secrets/appsettings

.EXAMPLE
    .\scripts\deploy-db.ps1 -ConnectionString "Server=myserver.database.windows.net;Database=mydb;User Id=admin;Password=Secret123!;TrustServerCertificate=True"
    # Overrides the connection string for a different environment
#>
param(
    [string]$ConnectionString
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

Push-Location $repoRoot
try {
    # Verify dotnet-ef tool is available
    if (-not (dotnet tool list --global | Select-String "dotnet-ef")) {
        Write-Host "Installing dotnet-ef tool globally..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef
    }

    $efArgs = @(
        "ef", "database", "update",
        "--project", "src/SRO.Infrastructure",
        "--startup-project", "src/SRO.Api"
    )

    if ($ConnectionString) {
        # Temporarily set the connection string as an environment variable
        $env:ConnectionStrings__SroDatabase = $ConnectionString
        Write-Host "Using provided connection string override." -ForegroundColor Cyan
    }

    Write-Host "Applying EF Core migrations..." -ForegroundColor Green
    dotnet @efArgs

    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed with exit code $LASTEXITCODE"
    }

    Write-Host "Migrations applied successfully!" -ForegroundColor Green
}
finally {
    if ($ConnectionString) {
        Remove-Item Env:\ConnectionStrings__SroDatabase -ErrorAction SilentlyContinue
    }
    Pop-Location
}
