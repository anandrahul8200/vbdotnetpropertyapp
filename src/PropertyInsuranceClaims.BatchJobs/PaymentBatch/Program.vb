Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Processes payment batches.
''' Applies late fees, generates overdue notices, processes auto-pay EFT transactions.
''' Scheduled to run daily.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "PaymentBatch"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim lateFeeCount As Integer = 0
        Dim autoPayCount As Integer = 0
        Dim failed As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")
            jobLogID = LogJobStart(JOB_NAME, String.Join(",", args))

            ' Step 1: Apply late fees to overdue invoices
            Console.WriteLine("  Step 1: Applying late fees...")
            Dim lateFeeParams As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@AsOfDate", DateTime.Today),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_PAYMENT")
            }
            Dim processedParam As SqlParameter = DatabaseHelper.CreateOutputParam("@InvoicesProcessed", SqlDbType.Int)
            lateFeeParams.Add(processedParam)

            DatabaseHelper.ExecuteNonQuery("Billing.usp_Invoice_ApplyLateFees", lateFeeParams.ToArray())
            lateFeeCount = CInt(If(processedParam.Value, 0))
            Console.WriteLine($"    Late fees applied to {lateFeeCount} invoices")

            ' Step 2: Process auto-pay EFT transactions
            Console.WriteLine("  Step 2: Processing auto-pay EFT...")
            Dim eftParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@ProcessDate", DateTime.Today),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_PAYMENT")
            }
            Dim dtEFT As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Payment_ProcessAutoEFT", eftParams)
            autoPayCount = dtEFT.Rows.Count

            For Each row As DataRow In dtEFT.Rows
                Try
                    Dim policyID As Integer = CInt(row("PolicyID"))
                    Dim amount As Decimal = CDec(row("Amount"))
                    Dim invoiceID As Integer = CInt(row("InvoiceID"))

                    Console.Write($"    EFT: Policy {row("PolicyNumber")} - ${amount:N2}...")

                    ' Record the payment
                    Dim payParams As New List(Of SqlParameter) From {
                        DatabaseHelper.CreateParam("@InvoiceID", invoiceID),
                        DatabaseHelper.CreateParam("@PolicyID", policyID),
                        DatabaseHelper.CreateParam("@Amount", amount),
                        DatabaseHelper.CreateParam("@PaymentMethod", "EFT"),
                        DatabaseHelper.CreateParam("@ReferenceNumber", $"AUTO-{DateTime.Today:yyyyMMdd}-{invoiceID}"),
                        DatabaseHelper.CreateParam("@CreatedBy", "BATCH_PAYMENT")
                    }
                    Dim payIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PremiumPaymentID", SqlDbType.Int)
                    Dim payNumParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PaymentNumber", SqlDbType.VarChar)
                    payParams.AddRange({payIDParam, payNumParam})

                    DatabaseHelper.ExecuteNonQuery("Billing.usp_Payment_Record", payParams.ToArray())
                    Console.WriteLine(" OK")

                Catch exInner As Exception
                    failed += 1
                    Console.WriteLine($" FAILED: {exInner.Message}")
                    ErrorLogger.LogError(exInner, $"{JOB_NAME}.ProcessEFT", $"PolicyID={row("PolicyID")}")
                End Try
            Next

            Console.WriteLine($"    Auto-pay processed: {autoPayCount - failed} successful, {failed} failed")

            ' Step 3: Generate overdue notices
            Console.WriteLine("  Step 3: Generating overdue notices...")
            Dim noticeParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@DaysOverdue", 10),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_PAYMENT")
            }
            Dim dtNotices As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Payment_GenerateOverdueNotices", noticeParams)
            Console.WriteLine($"    Overdue notices: {dtNotices.Rows.Count}")

            ' Step 4: Process commission payments
            Console.WriteLine("  Step 4: Processing commission payments...")
            Dim commParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@PaymentDate", DateTime.Today),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_PAYMENT")
            }
            Dim dtComm As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Commission_ProcessPayments", commParams)
            Console.WriteLine($"    Commission payments: {dtComm.Rows.Count}")

            LogJobComplete(jobLogID, lateFeeCount + autoPayCount, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed. Late fees: {lateFeeCount}, Auto-pay: {autoPayCount}, Failed: {failed}")

        Catch ex As Exception
            LogJobFailed(jobLogID, ex.Message)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] FATAL ERROR: {ex.Message}")
            ErrorLogger.LogError(ex, JOB_NAME)
            Environment.ExitCode = 1
        End Try
    End Sub

    Private Function LogJobStart(jobName As String, parameters As String) As Long
        Try
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@JobName", jobName),
                DatabaseHelper.CreateParam("@Parameters", parameters),
                DatabaseHelper.CreateParam("@StartTime", DateTime.Now)
            }
            Return CLng(If(DatabaseHelper.ExecuteScalar("Batch.usp_JobLog_Start", params), 0))
        Catch
            Return 0
        End Try
    End Function

    Private Sub LogJobComplete(jobLogID As Long, processed As Integer, failed As Integer)
        Try
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@JobLogID", jobLogID),
                DatabaseHelper.CreateParam("@RecordsProcessed", processed),
                DatabaseHelper.CreateParam("@RecordsFailed", failed),
                DatabaseHelper.CreateParam("@EndTime", DateTime.Now),
                DatabaseHelper.CreateParam("@Status", "COMPLETED")
            }
            DatabaseHelper.ExecuteNonQuery("Batch.usp_JobLog_Complete", params)
        Catch
        End Try
    End Sub

    Private Sub LogJobFailed(jobLogID As Long, errorMessage As String)
        Try
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@JobLogID", jobLogID),
                DatabaseHelper.CreateParam("@EndTime", DateTime.Now),
                DatabaseHelper.CreateParam("@Status", "FAILED"),
                DatabaseHelper.CreateParam("@ErrorMessage", errorMessage)
            }
            DatabaseHelper.ExecuteNonQuery("Batch.usp_JobLog_Complete", params)
        Catch
        End Try
    End Sub

End Module
