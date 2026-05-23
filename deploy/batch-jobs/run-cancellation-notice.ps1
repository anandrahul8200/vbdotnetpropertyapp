# Cancellation Notice Generator Batch Job
# Scheduled: Daily at 8:00 AM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting CancellationNoticeGenerator..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT p.PolicyNumber, c.FirstName + ' ' + c.LastName AS CustomerName, i.DueDate, i.BalanceDue FROM Billing.Invoices i INNER JOIN Policy.Policies p ON i.PolicyID = p.PolicyID INNER JOIN Policy.Customers c ON i.CustomerID = c.CustomerID WHERE i.Status = 'OVERDUE' AND DATEDIFF(DAY, i.DueDate, GETDATE()) >= 20 AND p.PolicyStatus = 'ACTIVE'; PRINT 'Cancellation notices generated';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] CancellationNoticeGenerator completed."
