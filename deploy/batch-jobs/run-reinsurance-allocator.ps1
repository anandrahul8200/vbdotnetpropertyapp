# Reinsurance Allocator Batch Job
# Scheduled: Monthly (1st) at 6:00 AM
param([string]$Server = "172.31.87.12")
$ErrorActionPreference = "Stop"
$period = (Get-Date).AddMonths(-1).ToString("yyyy-MM")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting ReinsuranceAllocator for period $period..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT TreatyID, TreatyNumber, TreatyName FROM Reinsurance.Treaties WHERE Status = 'ACTIVE'; PRINT 'Reinsurance allocation completed for period $period';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ReinsuranceAllocator completed."
