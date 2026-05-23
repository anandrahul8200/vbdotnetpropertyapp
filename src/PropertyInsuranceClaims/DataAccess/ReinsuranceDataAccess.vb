Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Reinsurance operations — treaties, cessions, bordereaux.
''' </summary>
Public Class ReinsuranceDataAccess

    ' --- Treaty Management ---
    Public Shared Function GetActiveTreaties() As DataTable
        Return DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Treaty_GetActive", Nothing)
    End Function

    Public Shared Function GetTreatyDetails(treatyID As Integer) As DataSet
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@TreatyID", treatyID)}
        Return DatabaseHelper.ExecuteDataSet("Reinsurance.usp_Treaty_GetDetails", params)
    End Function

    Public Shared Function CreateTreaty(treatyName As String, treatyType As String, reinsurerID As Integer,
                                         effectiveDate As Date, expiryDate As Date, retentionAmount As Decimal,
                                         cessionPercent As Decimal) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@TreatyName", treatyName),
            DatabaseHelper.CreateParam("@TreatyType", treatyType),
            DatabaseHelper.CreateParam("@ReinsurerID", reinsurerID),
            DatabaseHelper.CreateParam("@EffectiveDate", effectiveDate),
            DatabaseHelper.CreateParam("@ExpiryDate", expiryDate),
            DatabaseHelper.CreateParam("@RetentionAmount", retentionAmount),
            DatabaseHelper.CreateParam("@CessionPercent", cessionPercent),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TreatyID", SqlDbType.Int)
        Dim numParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TreatyNumber", SqlDbType.VarChar)
        params.AddRange({idParam, numParam})
        DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Treaty_Create", params.ToArray())
        Return CInt(idParam.Value)
    End Function

    ' --- Cessions ---
    Public Shared Function CalculateCession(treatyID As Integer, policyID As Integer, grossAmount As Decimal) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@TreatyID", treatyID),
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@GrossAmount", grossAmount),
            DatabaseHelper.CreateParam("@CessionType", "PREMIUM"),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Cession_Calculate", params)
    End Function

    Public Shared Function GetCessionsByTreaty(treatyID As Integer, Optional accountingPeriod As String = Nothing) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@TreatyID", treatyID),
            DatabaseHelper.CreateParam("@AccountingPeriod", accountingPeriod)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Cession_GetByTreaty", params)
    End Function

    ' --- Bordereaux ---
    Public Shared Function GenerateBordereaux(treatyID As Integer, reportingPeriod As String, reportType As String) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@TreatyID", treatyID),
            DatabaseHelper.CreateParam("@ReportingPeriod", reportingPeriod),
            DatabaseHelper.CreateParam("@ReportType", reportType),
            DatabaseHelper.CreateParam("@GeneratedBy", GlobalState.CurrentUser)
        }
        Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@BordereauxID", SqlDbType.Int)
        params.Add(idParam)
        DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Bordereaux_Generate", params.ToArray())
        Return CInt(idParam.Value)
    End Function

    Public Shared Function GetBordereaux(treatyID As Integer) As DataTable
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@TreatyID", treatyID)}
        Return DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Bordereaux_GetByTreaty", params)
    End Function

    ' --- Reinsurers ---
    Public Shared Function GetReinsurers() As DataTable
        Return DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Reinsurer_List", Nothing)
    End Function

End Class
