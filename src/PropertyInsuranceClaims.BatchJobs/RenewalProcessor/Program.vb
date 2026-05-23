Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Processes policy renewals.
''' Finds policies expiring within the configured window and generates renewal quotes.
''' Scheduled to run daily via Windows Task Scheduler.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "RenewalProcessor"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim processed As Integer = 0
        Dim failed As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")

            ' Parse arguments
            Dim daysAhead As Integer = 30
            If args.Length > 0 AndAlso Integer.TryParse(args(0), daysAhead) Then
                Console.WriteLine($"  Processing renewals due within {daysAhead} days")
            End If

            ' Log job start
            jobLogID = LogJobStart(JOB_NAME, $"DaysAhead={daysAhead}")

            ' Get policies due for renewal
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@DaysAhead", daysAhead),
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_RENEWAL")
            }
            Dim dtPolicies As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Renewal_GetDuePolicies", params)

            Console.WriteLine($"  Found {dtPolicies.Rows.Count} policies due for renewal")

            ' Process each policy
            For Each row As DataRow In dtPolicies.Rows
                Try
                    Dim policyID As Integer = CInt(row("PolicyID"))
                    Dim policyNumber As String = row("PolicyNumber").ToString()

                    Console.Write($"  Processing {policyNumber}...")

                    ' Call renewal SP
                    Dim renewParams() As SqlParameter = {
                        DatabaseHelper.CreateParam("@PolicyID", policyID),
                        DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_RENEWAL")
                    }
                    DatabaseHelper.ExecuteNonQuery("Batch.usp_Renewal_ProcessPolicy", renewParams)

                    processed += 1
                    Console.WriteLine(" OK")

                Catch exInner As Exception
                    failed += 1
                    Console.WriteLine($" FAILED: {exInner.Message}")
                    ErrorLogger.LogError(exInner, $"{JOB_NAME}.ProcessPolicy", $"PolicyID={row("PolicyID")}")
                End Try
            Next

            ' Log job completion
            LogJobComplete(jobLogID, processed, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed. Processed: {processed}, Failed: {failed}")

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
            Dim result As Object = DatabaseHelper.ExecuteScalar("Batch.usp_JobLog_Start", params)
            Return CLng(If(result, 0))
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
            ' Swallow
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
            ' Swallow
        End Try
    End Sub

End Module
