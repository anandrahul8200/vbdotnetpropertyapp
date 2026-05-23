# Reserve Recalculator Batch Job
# Scheduled: Weekly (Sunday) at 1:00 AM
param([string]$Server = "172.31.87.12")
$ErrorActionPreference = "Stop"
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting ReserveRecalculator..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "EXEC Batch.usp_Reserve_GetClaimsForReview @ProcessedBy='BATCH_RESERVE'; EXEC Batch.usp_Reserve_CalculateIBNR @AsOfDate='$(Get-Date -Format 'yyyy-MM-dd')', @CalculatedBy='BATCH_RESERVE'; EXEC Batch.usp_Reserve_UpdateNetIncurred;" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ReserveRecalculator completed."
