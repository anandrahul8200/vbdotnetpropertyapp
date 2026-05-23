Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Recalculates reserves for open claims.
''' Applies IBNR factors, validates case reserves against thresholds,
''' and flags claims needing reserve review.
''' Scheduled to run weekly.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "ReserveRecalculator"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim processed As Integer = 0
        Dim flagged As Integer = 0
        Dim failed As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")
            jobLogID = LogJobStart(JOB_NAME, String.Join(",", args))

            ' Step 1: Get open claims with reserves
            Console.WriteLine("  Step 1: Loading open claims with reserves...")
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_RESERVE")
            }
            Dim dtClaims As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Reserve_GetClaimsForReview", params)
            Console.WriteLine($"    Found {dtClaims.Rows.Count} claims to review")

            ' Step 2: Process each claim
            Console.WriteLine("  Step 2: Reviewing reserves...")
            For Each row As DataRow In dtClaims.Rows
                Try
                    Dim claimID As Integer = CInt(row("ClaimID"))
                    Dim claimNumber As String = row("ClaimNumber").ToString()
                    Dim currentReserve As Decimal = CDec(If(IsDBNull(row("TotalReserve")), 0, row("TotalReserve")))
                    Dim totalPaid As Decimal = CDec(If(IsDBNull(row("TotalPaid")), 0, row("TotalPaid")))
                    Dim claimAge As Integer = CInt(If(IsDBNull(row("ClaimAgeDays")), 0, row("ClaimAgeDays")))

                    ' Check if reserve is adequate
                    Dim needsReview As Boolean = False
                    Dim reviewReason As String = ""

                    ' Rule 1: Reserve less than paid (negative IBNR)
                    If currentReserve < totalPaid * 0.1D AndAlso totalPaid > 0 Then
                        needsReview = True
                        reviewReason = "Reserve less than 10% of paid amount"
                    End If

                    ' Rule 2: Claim open > 180 days with no reserve change in 90 days
                    If claimAge > 180 Then
                        Dim lastChangeDate As DateTime = CDate(If(IsDBNull(row("LastReserveChange")), DateTime.MinValue, row("LastReserveChange")))
                        If (DateTime.Now - lastChangeDate).TotalDays > 90 Then
                            needsReview = True
                            reviewReason = "No reserve change in 90+ days on aged claim"
                        End If
                    End If

                    ' Rule 3: Reserve exceeds policy limit
                    Dim policyLimit As Decimal = CDec(If(IsDBNull(row("PolicyLimit")), 0, row("PolicyLimit")))
                    If currentReserve > policyLimit AndAlso policyLimit > 0 Then
                        needsReview = True
                        reviewReason = "Reserve exceeds policy limit"
                    End If

                    If needsReview Then
                        ' Flag for review
                        Dim flagParams() As SqlParameter = {
                            DatabaseHelper.CreateParam("@ClaimID", claimID),
                            DatabaseHelper.CreateParam("@ReviewReason", reviewReason),
                            DatabaseHelper.CreateParam("@FlaggedBy", "BATCH_RESERVE")
                        }
                        DatabaseHelper.ExecuteNonQuery("Batch.usp_Reserve_FlagForReview", flagParams)
                        flagged += 1
                        Console.WriteLine($"    {claimNumber}: FLAGGED - {reviewReason}")
                    End If

                    processed += 1
                Catch exInner As Exception
                    failed += 1
                    Console.WriteLine($"    FAILED: {exInner.Message}")
                    ErrorLogger.LogError(exInner, $"{JOB_NAME}.ProcessClaim", $"ClaimID={row("ClaimID")}")
                End Try
            Next

            ' Step 3: Calculate IBNR reserves
            Console.WriteLine("  Step 3: Calculating IBNR...")
            Dim ibnrParams() As SqlParameter = {
                DatabaseHelper.CreateParam("@AsOfDate", DateTime.Today),
                DatabaseHelper.CreateParam("@CalculatedBy", "BATCH_RESERVE")
            }
            DatabaseHelper.ExecuteNonQuery("Batch.usp_Reserve_CalculateIBNR", ibnrParams)
            Console.WriteLine("    IBNR calculation complete")

            ' Step 4: Update claim net incurred totals
            Console.WriteLine("  Step 4: Updating net incurred totals...")
            DatabaseHelper.ExecuteNonQuery("Batch.usp_Reserve_UpdateNetIncurred", Nothing)
            Console.WriteLine("    Net incurred updated")

            LogJobComplete(jobLogID, processed, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed. Reviewed: {processed}, Flagged: {flagged}, Failed: {failed}")

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
