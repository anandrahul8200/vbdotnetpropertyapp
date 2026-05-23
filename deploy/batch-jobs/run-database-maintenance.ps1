# Database Maintenance Batch Job
# Scheduled: Weekly (Sunday) at 11:00 PM
param([string]$Server = "172.31.87.12")
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting DatabaseMaintenance..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "EXEC sp_updatestats; PRINT 'Statistics updated'; SELECT name AS TableName, rows AS RowCount FROM sys.sysindexes WHERE indid < 2 AND rows > 0 ORDER BY rows DESC;" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] DatabaseMaintenance completed."
