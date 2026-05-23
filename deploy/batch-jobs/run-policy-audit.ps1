# Policy Audit Batch Job
# Scheduled: Weekly (Wednesday) at 10:00 PM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting PolicyAudit..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT 'Policies without agent' AS Issue, COUNT(*) AS Count FROM Policy.Policies WHERE AgentID IS NULL AND PolicyStatus = 'ACTIVE' UNION ALL SELECT 'Policies with zero premium', COUNT(*) FROM Policy.Policies WHERE AnnualPremium = 0 AND PolicyStatus = 'ACTIVE' UNION ALL SELECT 'Claims without adjuster', COUNT(*) FROM Claims.Claims WHERE AdjusterID IS NULL AND ClaimStatus NOT IN ('FNOL','CLOSED','DENIED'); PRINT 'Policy audit completed';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] PolicyAudit completed."
