# ============================================================
# Register Batch Jobs as Windows Scheduled Tasks
# Run this ON the App Server as Administrator
# ============================================================
param([string]$BatchJobsPath = "C:\PropertyInsurance\deploy\batch-jobs")

Write-Host "Registering batch jobs as Scheduled Tasks..." -ForegroundColor Yellow

$jobs = @(
    @{Name="PI-RenewalProcessor"; Script="run-renewal-processor.ps1"; Schedule="Daily"; Time="02:00"; Desc="Process policy renewals"},
    @{Name="PI-ExpirationProcessor"; Script="run-expiration-processor.ps1"; Schedule="Daily"; Time="03:00"; Desc="Expire/cancel policies"},
    @{Name="PI-FraudScoring"; Script="run-fraud-scoring.ps1"; Schedule="Daily"; Time="04:00"; Desc="Score claims for fraud"},
    @{Name="PI-PaymentBatch"; Script="run-payment-batch.ps1"; Schedule="Daily"; Time="05:00"; Desc="Apply late fees, process payments"},
    @{Name="PI-ReserveRecalculator"; Script="run-reserve-recalculator.ps1"; Schedule="Weekly"; Time="01:00"; Desc="Review reserves, calculate IBNR"},
    @{Name="PI-ReinsuranceAllocator"; Script="run-reinsurance-allocator.ps1"; Schedule="Monthly"; Time="06:00"; Desc="Allocate to treaties"},
    @{Name="PI-NotificationSender"; Script="run-notification-sender.ps1"; Schedule="Daily"; Time="06:30"; Desc="Send pending notifications"},
    @{Name="PI-ReportGenerator"; Script="run-report-generator.ps1"; Schedule="Daily"; Time="07:00"; Desc="Generate daily reports to CSV"},
    @{Name="PI-CancellationNotice"; Script="run-cancellation-notice.ps1"; Schedule="Daily"; Time="08:00"; Desc="Generate cancellation notices"},
    @{Name="PI-IBNRCalculator"; Script="run-ibnr-calculator.ps1"; Schedule="Monthly"; Time="08:00"; Desc="Calculate IBNR reserves"},
    @{Name="PI-CommissionProcessor"; Script="run-commission-processor.ps1"; Schedule="Monthly"; Time="09:00"; Desc="Process agent commissions"},
    @{Name="PI-ClaimsAging"; Script="run-claims-aging.ps1"; Schedule="Weekly"; Time="07:00"; Desc="Generate claims aging report"},
    @{Name="PI-PolicyAudit"; Script="run-policy-audit.ps1"; Schedule="Weekly"; Time="22:00"; Desc="Audit policy data integrity"},
    @{Name="PI-DataArchiver"; Script="run-data-archiver.ps1"; Schedule="Monthly"; Time="23:00"; Desc="Archive old audit/log data"},
    @{Name="PI-DatabaseMaintenance"; Script="run-database-maintenance.ps1"; Schedule="Weekly"; Time="23:00"; Desc="Update statistics, maintenance"}
)

foreach ($job in $jobs) {
    Write-Host "  Registering: $($job.Name) ($($job.Schedule) at $($job.Time))" -ForegroundColor Cyan
    
    $existing = Get-ScheduledTask -TaskName $job.Name -ErrorAction SilentlyContinue
    if ($existing) { Unregister-ScheduledTask -TaskName $job.Name -Confirm:$false }
    
    $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -File `"$BatchJobsPath\$($job.Script)`"" -WorkingDirectory $BatchJobsPath
    $trigger = New-ScheduledTaskTrigger -Daily -At $job.Time
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable
    
    Register-ScheduledTask -TaskName $job.Name -Action $action -Trigger $trigger -Settings $settings -User "NT AUTHORITY\SYSTEM" -RunLevel Highest -Description $job.Desc | Out-Null
    Write-Host "    OK" -ForegroundColor Green
}

Write-Host "`nAll 15 batch jobs registered!" -ForegroundColor Green
Write-Host "Verify with: Get-ScheduledTask -TaskName 'PI-*'" -ForegroundColor Yellow
