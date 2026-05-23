# Claims Aging Report Batch Job
# Scheduled: Weekly (Monday) at 7:00 AM
param([string]$Server = "172.31.87.12", [string]$OutputPath = "C:\PropertyInsurance\Reports")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting ClaimsAgingReport..."
if (-not (Test-Path $OutputPath)) { New-Item $OutputPath -ItemType Directory -Force | Out-Null }
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT CASE WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 30 THEN '0-30 Days' WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 60 THEN '31-60 Days' WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 90 THEN '61-90 Days' ELSE '90+ Days' END AS AgingBucket, COUNT(*) AS ClaimCount, SUM(NetIncurred) AS TotalIncurred FROM Claims.Claims WHERE ClaimStatus NOT IN ('CLOSED','DENIED') GROUP BY CASE WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 30 THEN '0-30 Days' WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 60 THEN '31-60 Days' WHEN DATEDIFF(DAY, ReportedDate, GETDATE()) <= 90 THEN '61-90 Days' ELSE '90+ Days' END;" -s "," -W -o "$OutputPath\ClaimsAging_$(Get-Date -Format 'yyyyMMdd').csv" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ClaimsAgingReport completed."
