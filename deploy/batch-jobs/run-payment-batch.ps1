# Payment Batch Job
# Scheduled: Daily at 5:00 AM
param([string]$Server = "172.31.87.12")
$ErrorActionPreference = "Stop"
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Starting PaymentBatch..."
sqlcmd -S $Server -U sa -P "YourNewPassword123!" -d PropertyInsuranceDB -Q "DECLARE @Processed INT; EXEC Billing.usp_Invoice_ApplyLateFees @AsOfDate='$(Get-Date -Format 'yyyy-MM-dd')', @ProcessedBy='BATCH_PAYMENT', @InvoicesProcessed=@Processed OUTPUT; PRINT 'Late fees applied to ' + CAST(@Processed AS VARCHAR) + ' invoices';" -b
Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] PaymentBatch completed."
