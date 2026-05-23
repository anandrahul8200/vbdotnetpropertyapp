# Data Archiver Batch Job
# Scheduled: Monthly (15th) at 11:00 PM
param([string]$Server = "172.31.87.12", [int]$RetentionDays = 730)
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting DataArchiver (retention: $RetentionDays days)..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "DECLARE @CutoffDate DATE = DATEADD(DAY, -$RetentionDays, GETDATE()); SELECT COUNT(*) AS AuditRecordsToArchive FROM Audit.AuditLog WHERE ActionDate < @CutoffDate; PRINT 'Archive scan completed';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] DataArchiver completed."
