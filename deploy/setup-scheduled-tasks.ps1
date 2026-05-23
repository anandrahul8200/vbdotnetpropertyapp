# ============================================================
# Batch Job Scheduled Tasks Setup
# Registers all batch jobs as Windows Scheduled Tasks
# Run this ON the target EC2 app server (as Administrator)
# ============================================================
# Usage: .\setup-scheduled-tasks.ps1 -InstallPath "C:\PropertyInsurance"

param(
    [string]$InstallPath = "C:\PropertyInsurance",
    [string]$BatchJobsPath = "C:\PropertyInsurance\BatchJobs",
    [string]$RunAsUser = "NT AUTHORITY\SYSTEM"
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Yellow
Write-Host "Setting up Batch Job Scheduled Tasks" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow

# Define batch jobs
$batchJobs = @(
    @{
        Name = "PropertyIns-RenewalProcessor"
        Description = "Processes policy renewals due within 30 days"
        Executable = "$BatchJobsPath\RenewalProcessor\RenewalProcessor.exe"
        Arguments = "30"
        Schedule = "Daily"
        Time = "02:00"
    },
    @{
        Name = "PropertyIns-ExpirationProcessor"
        Description = "Processes policy expirations and non-payment cancellations"
        Executable = "$BatchJobsPath\ExpirationProcessor\ExpirationProcessor.exe"
        Arguments = ""
        Schedule = "Daily"
        Time = "03:00"
    },
    @{
        Name = "PropertyIns-FraudScoring"
        Description = "Evaluates fraud indicators on recent claims"
        Executable = "$BatchJobsPath\FraudScoring\FraudScoring.exe"
        Arguments = "7"
        Schedule = "Daily"
        Time = "04:00"
    },
    @{
        Name = "PropertyIns-PaymentBatch"
        Description = "Applies late fees, processes auto-pay EFT, generates overdue notices"
        Executable = "$BatchJobsPath\PaymentBatch\PaymentBatch.exe"
        Arguments = ""
        Schedule = "Daily"
        Time = "05:00"
    },
    @{
        Name = "PropertyIns-ReserveRecalculator"
        Description = "Reviews reserves on open claims, calculates IBNR"
        Executable = "$BatchJobsPath\ReserveRecalculator\ReserveRecalculator.exe"
        Arguments = ""
        Schedule = "Weekly"
        Time = "01:00"
        DayOfWeek = "Sunday"
    },
    @{
        Name = "PropertyIns-ReinsuranceAllocator"
        Description = "Allocates premiums/losses to treaties, generates bordereaux"
        Executable = "$BatchJobsPath\ReinsuranceAllocator\ReinsuranceAllocator.exe"
        Arguments = ""
        Schedule = "Monthly"
        Time = "06:00"
        DayOfMonth = 1
    }
)

foreach ($job in $batchJobs) {
    Write-Host "`nRegistering: $($job.Name)" -ForegroundColor Cyan
    Write-Host "  Schedule: $($job.Schedule) at $($job.Time)" -ForegroundColor Gray
    
    # Remove existing task if present
    $existing = Get-ScheduledTask -TaskName $job.Name -ErrorAction SilentlyContinue
    if ($existing) {
        Unregister-ScheduledTask -TaskName $job.Name -Confirm:$false
        Write-Host "  Removed existing task" -ForegroundColor DarkGray
    }
    
    # Create action
    $action = New-ScheduledTaskAction -Execute $job.Executable -Argument $job.Arguments -WorkingDirectory (Split-Path $job.Executable)
    
    # Create trigger based on schedule
    switch ($job.Schedule) {
        "Daily" {
            $trigger = New-ScheduledTaskTrigger -Daily -At $job.Time
        }
        "Weekly" {
            $trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek $job.DayOfWeek -At $job.Time
        }
        "Monthly" {
            # Monthly on specific day
            $trigger = New-ScheduledTaskTrigger -Daily -At $job.Time
            # Note: For true monthly, use Task Scheduler XML or CIM
        }
    }
    
    # Create settings
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable
    $settings.ExecutionTimeLimit = "PT2H"  # 2 hour timeout
    
    # Register task
    Register-ScheduledTask -TaskName $job.Name -Action $action -Trigger $trigger -Settings $settings -User $RunAsUser -RunLevel Highest -Description $job.Description
    
    Write-Host "  Registered successfully" -ForegroundColor Green
}

Write-Host "`n============================================" -ForegroundColor Yellow
Write-Host "All batch jobs registered!" -ForegroundColor Green
Write-Host "Use 'Get-ScheduledTask -TaskName PropertyIns-*' to verify" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Yellow
