# ============================================================
# Database Deployment Script
# Deploys all SQL scripts in order to the target SQL Server
# ============================================================
# Usage: .\deploy-database.ps1 -ServerName "10.0.1.50" -Username "sa" -Password "YourPassword"
# Or with Windows Auth: .\deploy-database.ps1 -ServerName "10.0.1.50" -UseWindowsAuth

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [string]$DatabaseName = "PropertyInsuranceDB",
    
    [string]$Username = "sa",
    
    [string]$Password,
    
    [switch]$UseWindowsAuth,
    
    [string]$ScriptRoot = "..\database",
    
    [switch]$SkipSchema,
    
    [switch]$SkipSPs,
    
    [switch]$SkipSeedData
)

$ErrorActionPreference = "Stop"

# Build connection string
if ($UseWindowsAuth) {
    $connectionString = "Server=$ServerName;Database=master;Integrated Security=True;TrustServerCertificate=True;"
} else {
    if (-not $Password) {
        $Password = Read-Host "Enter SQL Server password" -AsSecureString | ConvertFrom-SecureString -AsPlainText
    }
    $connectionString = "Server=$ServerName;Database=master;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
}

function Execute-SqlFile {
    param(
        [string]$FilePath,
        [string]$ConnString
    )
    
    $fileName = Split-Path $FilePath -Leaf
    Write-Host "  Executing: $fileName" -ForegroundColor Cyan
    
    $bytes = [System.IO.File]::ReadAllBytes((Resolve-Path $FilePath).Path)
    $sql = [System.Text.Encoding]::UTF8.GetString($bytes)
    # Remove BOM if present
    $sql = $sql.TrimStart([char]0xFEFF)
    
    # Split on GO statements (batch separator) - must be on its own line
    $batches = [regex]::Split($sql, '(?mi)^\s*GO\s*$')
    
    $conn = New-Object System.Data.SqlClient.SqlConnection($ConnString)
    $conn.Open()
    
    foreach ($batch in $batches) {
        $batch = $batch.Trim()
        if ($batch -eq "" -or $batch -eq $null) { continue }
        
        try {
            $cmd = New-Object System.Data.SqlClient.SqlCommand($batch, $conn)
            $cmd.CommandTimeout = 300
            $cmd.ExecuteNonQuery() | Out-Null
        }
        catch {
            Write-Host "    ERROR in batch: $($_.Exception.Message)" -ForegroundColor Red
            throw
        }
    }
    
    $conn.Close()
    Write-Host "    OK" -ForegroundColor Green
}

# ============================================================
# Main Deployment
# ============================================================
Write-Host "============================================" -ForegroundColor Yellow
Write-Host "Property Insurance DB Deployment" -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow

$startTime = Get-Date

# Step 1: Schema
if (-not $SkipSchema) {
    Write-Host "`n[Step 1/3] Deploying Schema..." -ForegroundColor White
    $schemaPath = Join-Path $ScriptRoot "01-schema"
    $schemaFiles = Get-ChildItem $schemaPath -Filter "*.sql" | Sort-Object Name
    
    foreach ($file in $schemaFiles) {
        Execute-SqlFile -FilePath $file.FullName -ConnString $connectionString
    }
    
    # Switch connection to the new database for subsequent scripts
    if ($UseWindowsAuth) {
        $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"
    } else {
        $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    }
    
    Write-Host "  Schema deployment complete." -ForegroundColor Green
} else {
    Write-Host "`n[Step 1/3] Schema SKIPPED" -ForegroundColor DarkGray
    # Still need to point at the right DB
    if ($UseWindowsAuth) {
        $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"
    } else {
        $connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
    }
}

# Step 2: Stored Procedures
if (-not $SkipSPs) {
    Write-Host "`n[Step 2/3] Deploying Stored Procedures..." -ForegroundColor White
    $spPath = Join-Path $ScriptRoot "02-stored-procedures"
    $spFiles = Get-ChildItem $spPath -Filter "*.sql" | Sort-Object Name
    
    foreach ($file in $spFiles) {
        Execute-SqlFile -FilePath $file.FullName -ConnString $connectionString
    }
    
    Write-Host "  Stored procedures deployment complete." -ForegroundColor Green
} else {
    Write-Host "`n[Step 2/3] Stored Procedures SKIPPED" -ForegroundColor DarkGray
}

# Step 3: Seed Data
if (-not $SkipSeedData) {
    Write-Host "`n[Step 3/3] Deploying Seed Data..." -ForegroundColor White
    $seedPath = Join-Path $ScriptRoot "03-seed-data"
    if (Test-Path $seedPath) {
        $seedFiles = Get-ChildItem $seedPath -Filter "*.sql" | Sort-Object Name
        
        foreach ($file in $seedFiles) {
            Execute-SqlFile -FilePath $file.FullName -ConnString $connectionString
        }
    }
    
    Write-Host "  Seed data deployment complete." -ForegroundColor Green
} else {
    Write-Host "`n[Step 3/3] Seed Data SKIPPED" -ForegroundColor DarkGray
}

# Summary
$elapsed = (Get-Date) - $startTime
Write-Host "`n============================================" -ForegroundColor Yellow
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "Duration: $($elapsed.ToString('mm\:ss'))" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
