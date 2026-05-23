Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Evaluates fraud indicators on open claims.
''' Runs nightly to score new/updated claims and auto-refer to SIU.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "FraudScoring"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim processed As Integer = 0
        Dim failed As Integer = 0
        Dim referred As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")

            ' Parse arguments
            Dim daysBack As Integer = 7
            If args.Length > 0 AndAlso Integer.TryParse(args(0), daysBack) Then
                Console.WriteLine($"  Scoring claims reported in last {daysBack} days")
            End If

            jobLogID = LogJobStart(JOB_NAME, $"DaysBack={daysBack}")

            ' Get claims needing fraud evaluation
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@DaysBack", daysBack)
            }
            Dim dtClaims As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Fraud_GetClaimsToScore", params)

            Console.WriteLine($"  Found {dtClaims.Rows.Count} claims to evaluate")

            For Each row As DataRow In dtClaims.Rows
                Try
                    Dim claimID As Integer = CInt(row("ClaimID"))
                    Dim claimNumber As String = row("ClaimNumber").ToString()

                    Console.Write($"  Scoring {claimNumber}...")

                    ' Call fraud evaluation SP
                    Dim evalParams As New List(Of SqlParameter) From {
                        DatabaseHelper.CreateParam("@ClaimID", claimID),
                        DatabaseHelper.CreateParam("@EvaluatedBy", "BATCH_FRAUD")
                    }
                    Dim scoreParam As SqlParameter = DatabaseHelper.CreateOutputParam("@FraudScore", SqlDbType.Decimal)
                    evalParams.Add(scoreParam)

                    DatabaseHelper.ExecuteNonQuery("Claims.usp_Fraud_EvaluateClaim", evalParams.ToArray())

                    Dim score As Decimal = CDec(If(scoreParam.Value, 0))
                    processed += 1

                    If score >= 70 Then
                        referred += 1
                        Console.WriteLine($" Score: {score:N1} ** SIU REFERRED **")
                    Else
                        Console.WriteLine($" Score: {score:N1}")
                    End If

                Catch exInner As Exception
                    failed += 1
                    Console.WriteLine($" FAILED: {exInner.Message}")
                    ErrorLogger.LogError(exInner, $"{JOB_NAME}.ScoreClaim", $"ClaimID={row("ClaimID")}")
                End Try
            Next

            LogJobComplete(jobLogID, processed, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed. Scored: {processed}, Referred: {referred}, Failed: {failed}")

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
