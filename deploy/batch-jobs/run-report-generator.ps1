# Report Generator Batch Job
# Scheduled: Daily at 7:00 AM
param([string]$Server = "172.31.87.12", [string]$OutputPath = "C:\PropertyInsurance\Reports")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting ReportGenerator..."
if (-not (Test-Path $OutputPath)) { New-Item $OutputPath -ItemType Directory -Force | Out-Null }
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT ClaimStatus, COUNT(*) AS Count, SUM(NetIncurred) AS TotalIncurred FROM Claims.Claims WHERE ClaimStatus NOT IN ('CLOSED','DENIED') GROUP BY ClaimStatus;" -s "," -W -o "$OutputPath\DailyClaimsSummary_$(Get-Date -Format 'yyyyMMdd').csv" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] ReportGenerator completed. Output: $OutputPath"
