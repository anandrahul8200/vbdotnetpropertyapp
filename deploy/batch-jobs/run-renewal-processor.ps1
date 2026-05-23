# Renewal Processor Batch Job
# Scheduled: Daily at 2:00 AM
param([string]$Server = "172.31.87.12", [int]$DaysAhead = 30)
$ErrorActionPreference = "Stop"
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting RenewalProcessor..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "EXEC Batch.usp_Renewal_GetDuePolicies @DaysAhead=$DaysAhead, @ProcessedBy='BATCH_RENEWAL'" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] RenewalProcessor completed."
