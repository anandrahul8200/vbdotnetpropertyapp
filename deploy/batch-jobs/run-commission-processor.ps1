# Commission Processor Batch Job
# Scheduled: Monthly (5th) at 9:00 AM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting CommissionProcessor..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT a.AgentNumber, a.FirstName + ' ' + a.LastName AS AgentName, SUM(p.CommissionAmount) AS TotalCommission FROM Policy.Policies p INNER JOIN Policy.Agents a ON p.AgentID = a.AgentID WHERE p.PolicyStatus = 'ACTIVE' GROUP BY a.AgentNumber, a.FirstName, a.LastName ORDER BY TotalCommission DESC; PRINT 'Commission processing completed';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] CommissionProcessor completed."
