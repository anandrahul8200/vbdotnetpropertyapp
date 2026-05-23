Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Processes policy expirations and cancellations.
''' Marks expired policies, sends cancellation notices for non-payment.
''' Scheduled to run daily.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "ExpirationProcessor"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim expired As Integer = 0
        Dim cancelled As Integer = 0
        Dim failed As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")
            jobLogID = LogJobStart(JOB_NAME, String.Join(",", args))

            ' Step 1: Expire policies past their expiry date
            Console.WriteLine("  Step 1: Processing expirations...")
            Dim expireParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_EXPIRATION"),
                DatabaseHelper.CreateParam("@AsOfDate", DateTime.Today)
            }
            Dim dtExpired As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Policy_ProcessExpirations", expireParams)
            expired = dtExpired.Rows.Count
            Console.WriteLine($"    Expired: {expired} policies")

            ' Step 2: Process cancellations for non-payment
            Console.WriteLine("  Step 2: Processing non-payment cancellations...")
            Dim cancelParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_EXPIRATION"),
                DatabaseHelper.CreateParam("@GracePeriodDays", 30)
            }
            Dim dtCancelled As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Policy_ProcessCancellations", cancelParams)
            cancelled = dtCancelled.Rows.Count
            Console.WriteLine($"    Cancelled for non-payment: {cancelled} policies")

            ' Step 3: Send cancellation notices (policies approaching cancellation)
            Console.WriteLine("  Step 3: Generating cancellation notices...")
            Dim noticeParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@NoticeDays", 20),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_EXPIRATION")
            }
            Dim dtNotices As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Policy_GenerateCancelNotices", noticeParams)
            Console.WriteLine($"    Notices generated: {dtNotices.Rows.Count}")

            LogJobComplete(jobLogID, expired + cancelled, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed. Expired: {expired}, Cancelled: {cancelled}, Failed: {failed}")

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
