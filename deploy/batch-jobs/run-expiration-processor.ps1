# Expiration Processor Batch Job
# Scheduled: Daily at 3:00 AM
param([string]$Server = "172.31.87.12")
$ErrorActionPreference = "Stop"
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting ExpirationProcessor..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "UPDATE Policy.Policies SET PolicyStatus = 'EXPIRED' WHERE PolicyStatus = 'ACTIVE' AND ExpiryDate < GETDATE(); PRINT 'Expired ' + CAST(@@ROWCOUNT AS VARCHAR) + ' policies';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ExpirationProcessor completed."
