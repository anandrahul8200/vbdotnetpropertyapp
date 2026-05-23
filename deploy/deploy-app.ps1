# ============================================================
# Application Deployment Script
# Builds the VB.NET solution and deploys to target EC2 instance
# ============================================================
# Usage: .\deploy-app.ps1 -TargetServer "10.0.1.100" -TargetUser "Administrator" -DBServer "10.0.1.50"

param(
    [Parameter(Mandatory=$true)]
    [string]$TargetServer,
    
    [string]$TargetUser = "Administrator",
    
    [string]$TargetPassword,
    
    [Parameter(Mandatory=$true)]
    [string]$DBServer,
    
    [string]$DBName = "PropertyInsuranceDB",
    
    [string]$DBUsername = "AppUser",
    
    [string]$DBPassword,
    
    [string]$InstallPath = "C:\PropertyInsurance",
    
    [string]$SolutionPath = "..\src",
    
    [switch]$SkipBuild,
    
    [switch]$SkipDeploy
)

$ErrorActionPreference = "Stop"

# ============================================================
# Step 1: Build Solution
# ============================================================
if (-not $SkipBuild) {
    Write-Host "[Step 1] Building solution..." -ForegroundColor White
    
    $msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    if (-not (Test-Path $msbuildPath)) {
        $msbuildPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
    }
    
    if (-not (Test-Path $msbuildPath)) {
        Write-Host "  MSBuild not found. Please install .NET Framework SDK or Visual Studio Build Tools." -ForegroundColor Red
        exit 1
    }
    
    $solutionFile = Get-ChildItem $SolutionPath -Filter "*.sln" -Recurse | Select-Object -First 1
    if (-not $solutionFile) {
        Write-Host "  No .sln file found in $SolutionPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  Building: $($solutionFile.Name)" -ForegroundColor Cyan
    $buildOutput = Join-Path $PSScriptRoot "build-output"
    
    & $msbuildPath $solutionFile.FullName /p:Configuration=Release /p:OutputPath=$buildOutput /t:Rebuild /v:minimal
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  Build FAILED!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  Build successful." -ForegroundColor Green
} else {
    Write-Host "[Step 1] Build SKIPPED" -ForegroundColor DarkGray
}

# ============================================================
# Step 2: Prepare deployment package
# ============================================================
Write-Host "`n[Step 2] Preparing deployment package..." -ForegroundColor White

$packagePath = Join-Path $PSScriptRoot "package"
if (Test-Path $packagePath) { Remove-Item $packagePath -Recurse -Force }
New-Item $packagePath -ItemType Directory | Out-Null

# Copy build output
$buildOutput = Join-Path $PSScriptRoot "build-output"
if (Test-Path $buildOutput) {
    Copy-Item "$buildOutput\*" $packagePath -Recurse -Force
}

# Generate app.config with connection string
$appConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="PropertyInsuranceDB" 
         connectionString="Server=$DBServer;Database=$DBName;User Id=$DBUsername;Password=$DBPassword;TrustServerCertificate=True;Application Name=PropertyInsurance;"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  <appSettings>
    <add key="ApplicationName" value="Property Insurance Claims Management" />
    <add key="ApplicationVersion" value="1.0.0" />
    <add key="CompanyName" value="Insurance Company" />
    <add key="SessionTimeoutMinutes" value="30" />
    <add key="DefaultPageSize" value="50" />
    <add key="ReportTemplatePath" value="$InstallPath\Reports\" />
    <add key="DocumentStoragePath" value="$InstallPath\Documents\" />
    <add key="ExportPath" value="$InstallPath\Exports\" />
  </appSettings>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
</configuration>
"@

$appConfig | Out-File (Join-Path $packagePath "PropertyInsuranceClaims.exe.config") -Encoding UTF8
Write-Host "  Package prepared with connection string for $DBServer" -ForegroundColor Green

# ============================================================
# Step 3: Deploy to target server
# ============================================================
if (-not $SkipDeploy) {
    Write-Host "`n[Step 3] Deploying to $TargetServer..." -ForegroundColor White
    
    # Create credential
    if ($TargetPassword) {
        $secPassword = ConvertTo-SecureString $TargetPassword -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential($TargetUser, $secPassword)
    } else {
        $credential = Get-Credential -UserName $TargetUser -Message "Enter credentials for $TargetServer"
    }
    
    # Create remote session
    $session = New-PSSession -ComputerName $TargetServer -Credential $credential
    
    # Create install directory on target
    Invoke-Command -Session $session -ScriptBlock {
        param($path)
        if (-not (Test-Path $path)) { New-Item $path -ItemType Directory -Force | Out-Null }
        if (-not (Test-Path "$path\Logs")) { New-Item "$path\Logs" -ItemType Directory -Force | Out-Null }
        if (-not (Test-Path "$path\Reports")) { New-Item "$path\Reports" -ItemType Directory -Force | Out-Null }
        if (-not (Test-Path "$path\Documents")) { New-Item "$path\Documents" -ItemType Directory -Force | Out-Null }
        if (-not (Test-Path "$path\Exports")) { New-Item "$path\Exports" -ItemType Directory -Force | Out-Null }
        if (-not (Test-Path "$path\BatchJobs")) { New-Item "$path\BatchJobs" -ItemType Directory -Force | Out-Null }
    } -ArgumentList $InstallPath
    
    # Copy files
    Write-Host "  Copying files to $InstallPath..." -ForegroundColor Cyan
    Copy-Item "$packagePath\*" -Destination $InstallPath -ToSession $session -Recurse -Force
    
    # Create desktop shortcut for all users
    Invoke-Command -Session $session -ScriptBlock {
        param($path)
        $shell = New-Object -ComObject WScript.Shell
        $shortcut = $shell.CreateShortcut("C:\Users\Public\Desktop\Property Insurance.lnk")
        $shortcut.TargetPath = "$path\PropertyInsuranceClaims.exe"
        $shortcut.WorkingDirectory = $path
        $shortcut.IconLocation = "$path\PropertyInsuranceClaims.exe,0"
        $shortcut.Save()
    } -ArgumentList $InstallPath
    
    Remove-PSSession $session
    Write-Host "  Deployment complete." -ForegroundColor Green
} else {
    Write-Host "`n[Step 3] Deploy SKIPPED" -ForegroundColor DarkGray
}

Write-Host "`n============================================" -ForegroundColor Yellow
Write-Host "Application Deployment Complete!" -ForegroundColor Green
Write-Host "Install Path: $InstallPath" -ForegroundColor Yellow
Write-Host "DB Server: $DBServer" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
