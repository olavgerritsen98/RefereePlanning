<#
.SYNOPSIS
    Full environment setup script for SRO application.

.DESCRIPTION
    Sets up a new Azure environment for SRO:
    1. Creates Azure SQL Server + Database
    2. Configures firewall rules
    3. Sets user secrets for local development
    4. Applies EF Core migrations

.PARAMETER ResourceGroup
    Azure resource group name.

.PARAMETER Location
    Azure region (e.g., northeurope).

.PARAMETER SqlServerName
    Name for the Azure SQL Server resource.

.PARAMETER DatabaseName
    Name for the database (default: sro-db).

.PARAMETER AdminUser
    SQL admin username (default: sroadmin).

.PARAMETER AdminPassword
    SQL admin password. Will prompt if not provided.

.EXAMPLE
    .\scripts\setup-environment.ps1 -ResourceGroup "rg-sro-test" -Location "northeurope" -SqlServerName "sql-sro-test-ne" -AdminPassword "MyStr0ngP@ss!"
#>
param(
    [Parameter(Mandatory)][string]$ResourceGroup,
    [Parameter(Mandatory)][string]$Location,
    [Parameter(Mandatory)][string]$SqlServerName,
    [string]$DatabaseName = "sro-db",
    [string]$AdminUser = "sroadmin",
    [string]$AdminPassword
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

if (-not $AdminPassword) {
    $securePass = Read-Host "Enter SQL admin password" -AsSecureString
    $AdminPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass))
}

Write-Host "=== SRO Environment Setup ===" -ForegroundColor Cyan

# 1. Create Resource Group
Write-Host "`n[1/6] Creating resource group '$ResourceGroup'..." -ForegroundColor Green
az group create --name $ResourceGroup --location $Location --output none

# 2. Create SQL Server
Write-Host "[2/6] Creating SQL Server '$SqlServerName'..." -ForegroundColor Green
az sql server create `
    --resource-group $ResourceGroup `
    --name $SqlServerName `
    --location $Location `
    --admin-user $AdminUser `
    --admin-password $AdminPassword `
    --output none

# 3. Create Database (Basic tier for dev, scale up for prod)
Write-Host "[3/6] Creating database '$DatabaseName'..." -ForegroundColor Green
az sql db create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name $DatabaseName `
    --edition Basic `
    --output none

# 4. Add firewall rule for current IP
Write-Host "[4/6] Adding firewall rule for current IP..." -ForegroundColor Green
$myIp = (Invoke-RestMethod -Uri "https://api.ipify.org")
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $SqlServerName `
    --name "LocalDev-$env:COMPUTERNAME" `
    --start-ip-address $myIp `
    --end-ip-address $myIp `
    --output none
Write-Host "  Added firewall rule for IP: $myIp" -ForegroundColor Gray

# 5. Configure user secrets
Write-Host "[5/6] Setting user secrets for SRO.Api..." -ForegroundColor Green
$connectionString = "Server=$SqlServerName.database.windows.net;Database=$DatabaseName;User Id=$AdminUser;Password=$AdminPassword;TrustServerCertificate=True"

Push-Location "$repoRoot/src/SRO.Api"
dotnet user-secrets set "ConnectionStrings:SroDatabase" $connectionString
Pop-Location

# 6. Apply migrations
Write-Host "[6/6] Applying EF Core migrations..." -ForegroundColor Green
& "$repoRoot/scripts/deploy-db.ps1" -ConnectionString $connectionString

Write-Host "`n=== Setup Complete ===" -ForegroundColor Cyan
Write-Host "Connection string: $connectionString" -ForegroundColor Gray
Write-Host "`nRun the API with: dotnet run --project src/SRO.Api" -ForegroundColor Yellow
