# IBNR Calculator Batch Job
# Scheduled: Monthly (1st) at 8:00 AM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting IBNRCalculator..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "EXEC Batch.usp_Reserve_CalculateIBNR @AsOfDate='$(Get-Date -Format 'yyyy-MM-dd')', @CalculatedBy='BATCH_IBNR'; PRINT 'IBNR calculation completed';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] IBNRCalculator completed."
