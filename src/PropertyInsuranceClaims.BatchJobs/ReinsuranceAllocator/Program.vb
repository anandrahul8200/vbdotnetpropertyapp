Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job: Allocates premiums and losses to reinsurance treaties.
''' Calculates cessions for new/renewed policies and paid claims,
''' generates monthly bordereaux reports.
''' Scheduled to run monthly.
''' </summary>
Module Program

    Private Const JOB_NAME As String = "ReinsuranceAllocator"

    Sub Main(args As String())
        Dim jobLogID As Long = 0
        Dim premiumCessions As Integer = 0
        Dim lossCessions As Integer = 0
        Dim bordereaux As Integer = 0
        Dim failed As Integer = 0

        Try
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting {JOB_NAME}...")

            ' Determine accounting period
            Dim accountingPeriod As String = DateTime.Now.AddMonths(-1).ToString("yyyy-MM")
            If args.Length > 0 Then accountingPeriod = args(0)
            Console.WriteLine($"  Accounting Period: {accountingPeriod}")

            jobLogID = LogJobStart(JOB_NAME, $"Period={accountingPeriod}")

            ' Step 1: Get active treaties
            Console.WriteLine("  Step 1: Loading active treaties...")
            Dim dtTreaties As DataTable = DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Treaty_GetActive", Nothing)
            Console.WriteLine($"    Active treaties: {dtTreaties.Rows.Count}")

            ' Step 2: Allocate premium cessions
            Console.WriteLine("  Step 2: Allocating premium cessions...")
            For Each treatyRow As DataRow In dtTreaties.Rows
                Try
                    Dim treatyID As Integer = CInt(treatyRow("TreatyID"))
                    Dim treatyNumber As String = treatyRow("TreatyNumber").ToString()
                    Dim treatyType As String = treatyRow("TreatyType").ToString()

                    Console.Write($"    Treaty {treatyNumber} ({treatyType})...")

                    ' Get unallocated premiums for this treaty
                    Dim premParams() As SqlParameter = {
                        DatabaseHelper.CreateParam("@TreatyID", treatyID),
                        DatabaseHelper.CreateParam("@AccountingPeriod", accountingPeriod),
                        DatabaseHelper.CreateParam("@CessionType", "PREMIUM"),
                        DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_REINSURANCE")
                    }
                    Dim dtPremiums As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Reinsurance_GetUnallocated", premParams)

                    Dim treatyPremCessions As Integer = 0
                    For Each premRow As DataRow In dtPremiums.Rows
                        Try
                            Dim policyID As Integer = CInt(premRow("PolicyID"))
                            Dim grossAmount As Decimal = CDec(premRow("GrossAmount"))

                            ' Calculate cession
                            Dim cessionParams() As SqlParameter = {
                                DatabaseHelper.CreateParam("@TreatyID", treatyID),
                                DatabaseHelper.CreateParam("@PolicyID", policyID),
                                DatabaseHelper.CreateParam("@GrossAmount", grossAmount),
                                DatabaseHelper.CreateParam("@CessionType", "PREMIUM"),
                                DatabaseHelper.CreateParam("@AccountingPeriod", accountingPeriod),
                                DatabaseHelper.CreateParam("@CreatedBy", "BATCH_REINSURANCE")
                            }
                            DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Cession_Create", cessionParams)
                            treatyPremCessions += 1
                            premiumCessions += 1
                        Catch exPrem As Exception
                            failed += 1
                            ErrorLogger.LogError(exPrem, $"{JOB_NAME}.PremiumCession", $"TreatyID={treatyID}, PolicyID={premRow("PolicyID")}")
                        End Try
                    Next

                    Console.WriteLine($" {treatyPremCessions} premium cessions")
                Catch exTreaty As Exception
                    failed += 1
                    Console.WriteLine($" FAILED: {exTreaty.Message}")
                    ErrorLogger.LogError(exTreaty, $"{JOB_NAME}.ProcessTreaty", $"TreatyID={treatyRow("TreatyID")}")
                End Try
            Next

            ' Step 3: Allocate loss cessions
            Console.WriteLine("  Step 3: Allocating loss cessions...")
            For Each treatyRow As DataRow In dtTreaties.Rows
                Try
                    Dim treatyID As Integer = CInt(treatyRow("TreatyID"))
                    Dim treatyNumber As String = treatyRow("TreatyNumber").ToString()

                    ' Get unallocated losses
                    Dim lossParams() As SqlParameter = {
                        DatabaseHelper.CreateParam("@TreatyID", treatyID),
                        DatabaseHelper.CreateParam("@AccountingPeriod", accountingPeriod),
                        DatabaseHelper.CreateParam("@CessionType", "LOSS"),
                        DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_REINSURANCE")
                    }
                    Dim dtLosses As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Reinsurance_GetUnallocated", lossParams)

                    For Each lossRow As DataRow In dtLosses.Rows
                        Try
                            Dim claimID As Integer = CInt(lossRow("ClaimID"))
                            Dim grossAmount As Decimal = CDec(lossRow("GrossAmount"))

                            Dim cessionParams() As SqlParameter = {
                                DatabaseHelper.CreateParam("@TreatyID", treatyID),
                                DatabaseHelper.CreateParam("@ClaimID", claimID),
                                DatabaseHelper.CreateParam("@GrossAmount", grossAmount),
                                DatabaseHelper.CreateParam("@CessionType", "LOSS"),
                                DatabaseHelper.CreateParam("@AccountingPeriod", accountingPeriod),
                                DatabaseHelper.CreateParam("@CreatedBy", "BATCH_REINSURANCE")
                            }
                            DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Cession_Create", cessionParams)
                            lossCessions += 1
                        Catch exLoss As Exception
                            failed += 1
                            ErrorLogger.LogError(exLoss, $"{JOB_NAME}.LossCession", $"TreatyID={treatyID}, ClaimID={lossRow("ClaimID")}")
                        End Try
                    Next
                Catch exTreaty As Exception
                    failed += 1
                    ErrorLogger.LogError(exTreaty, $"{JOB_NAME}.LossTreaty", $"TreatyID={treatyRow("TreatyID")}")
                End Try
            Next
            Console.WriteLine($"    Loss cessions created: {lossCessions}")

            ' Step 4: Generate bordereaux
            Console.WriteLine("  Step 4: Generating bordereaux reports...")
            For Each treatyRow As DataRow In dtTreaties.Rows
                Try
                    Dim treatyID As Integer = CInt(treatyRow("TreatyID"))
                    Dim treatyNumber As String = treatyRow("TreatyNumber").ToString()

                    ' Generate premium bordereaux
                    Dim bordParams As New List(Of SqlParameter) From {
                        DatabaseHelper.CreateParam("@TreatyID", treatyID),
                        DatabaseHelper.CreateParam("@ReportingPeriod", accountingPeriod),
                        DatabaseHelper.CreateParam("@ReportType", "PREMIUM"),
                        DatabaseHelper.CreateParam("@GeneratedBy", "BATCH_REINSURANCE")
                    }
                    Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@BordereauxID", SqlDbType.Int)
                    bordParams.Add(idParam)
                    DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Bordereaux_Generate", bordParams.ToArray())
                    bordereaux += 1

                    ' Generate loss bordereaux
                    bordParams = New List(Of SqlParameter) From {
                        DatabaseHelper.CreateParam("@TreatyID", treatyID),
                        DatabaseHelper.CreateParam("@ReportingPeriod", accountingPeriod),
                        DatabaseHelper.CreateParam("@ReportType", "LOSS"),
                        DatabaseHelper.CreateParam("@GeneratedBy", "BATCH_REINSURANCE")
                    }
                    idParam = DatabaseHelper.CreateOutputParam("@BordereauxID", SqlDbType.Int)
                    bordParams.Add(idParam)
                    DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Bordereaux_Generate", bordParams.ToArray())
                    bordereaux += 1

                    Console.WriteLine($"    {treatyNumber}: Bordereaux generated (Premium + Loss)")
                Catch exBord As Exception
                    failed += 1
                    ErrorLogger.LogError(exBord, $"{JOB_NAME}.Bordereaux", $"TreatyID={treatyRow("TreatyID")}")
                End Try
            Next

            LogJobComplete(jobLogID, premiumCessions + lossCessions + bordereaux, failed)
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Completed.")
            Console.WriteLine($"  Premium Cessions: {premiumCessions}")
            Console.WriteLine($"  Loss Cessions: {lossCessions}")
            Console.WriteLine($"  Bordereaux: {bordereaux}")
            Console.WriteLine($"  Failed: {failed}")

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
