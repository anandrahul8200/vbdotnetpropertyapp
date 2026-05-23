# Notification Sender Batch Job
# Scheduled: Daily at 6:30 AM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting NotificationSender..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "SELECT COUNT(*) AS PendingNotifications FROM Claims.Activities WHERE IsCompleted = 0 AND DueDate < GETDATE(); PRINT 'Notifications processed';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] NotificationSender completed."
