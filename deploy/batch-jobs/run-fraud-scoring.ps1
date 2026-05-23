# Fraud Scoring Batch Job
# Scheduled: Daily at 4:00 AM
param([string]$Server = "172.31.87.12", [int]$DaysBack = 7)
$ErrorActionPreference = "Stop"
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting FraudScoring..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "EXEC Batch.usp_Fraud_GetClaimsToScore @DaysBack=$DaysBack" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] FraudScoring completed."
