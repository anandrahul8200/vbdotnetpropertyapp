Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Fraud and Subrogation operations.
''' </summary>
Public Class FraudDataAccess

    ' --- Fraud Evaluation ---
    Public Shared Function EvaluateClaim(claimID As Integer) As Decimal
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@EvaluatedBy", GlobalState.CurrentUser)
        }
        Dim scoreParam As SqlParameter = DatabaseHelper.CreateOutputParam("@FraudScore", SqlDbType.Decimal)
        params.Add(scoreParam)
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Fraud_EvaluateClaim", params.ToArray())
        Return CDec(If(scoreParam.Value, 0))
    End Function

    Public Shared Sub ReferToSIU(claimID As Integer, reason As String)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@ReferralReason", reason),
            DatabaseHelper.CreateParam("@ReferredBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Fraud_ReferToSIU", params)
    End Sub

    Public Shared Function GetFraudEvaluation(claimID As Integer) As DataSet
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@ClaimID", claimID)}
        Return DatabaseHelper.ExecuteDataSet("Claims.usp_Fraud_GetEvaluation", params)
    End Function

    ' --- Subrogation ---
    Public Shared Function CreateSubrogation(claimID As Integer, responsibleParty As String,
                                              Optional insurer As String = Nothing, Optional policyNumber As String = Nothing,
                                              Optional demandAmount As Decimal? = Nothing) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@ResponsibleParty", responsibleParty),
            DatabaseHelper.CreateParam("@ResponsiblePartyInsurer", insurer),
            DatabaseHelper.CreateParam("@ResponsiblePartyPolicy", policyNumber),
            DatabaseHelper.CreateParam("@DemandAmount", demandAmount),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@SubrogationID", SqlDbType.Int)
        params.Add(idParam)
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_Create", params.ToArray())
        Return CInt(idParam.Value)
    End Function

    Public Shared Sub UpdateSubrogationStatus(subrogationID As Integer, newStatus As String,
                                              Optional settlementAmount As Decimal? = Nothing,
                                              Optional notes As String = Nothing)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@SubrogationID", subrogationID),
            DatabaseHelper.CreateParam("@NewStatus", newStatus),
            DatabaseHelper.CreateParam("@SettlementAmount", settlementAmount),
            DatabaseHelper.CreateParam("@Notes", notes),
            DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_UpdateStatus", params)
    End Sub

    Public Shared Sub RecordRecovery(subrogationID As Integer, amount As Decimal)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@SubrogationID", subrogationID),
            DatabaseHelper.CreateParam("@RecoveryAmount", amount),
            DatabaseHelper.CreateParam("@RecordedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_RecordRecovery", params)
    End Sub

    ' --- Catastrophe ---
    Public Shared Function CreateCatastrophe(name As String, catType As String, eventDate As Date,
                                              Optional affectedStates As String = Nothing) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@CatastropheName", name),
            DatabaseHelper.CreateParam("@CatastropheType", catType),
            DatabaseHelper.CreateParam("@EventDate", eventDate),
            DatabaseHelper.CreateParam("@AffectedStates", affectedStates),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@CatastropheID", SqlDbType.Int)
        Dim numParam As SqlParameter = DatabaseHelper.CreateOutputParam("@CatastropheNumber", SqlDbType.VarChar)
        params.AddRange({idParam, numParam})
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Catastrophe_Create", params.ToArray())
        Return CInt(idParam.Value)
    End Function

    Public Shared Sub LinkClaimToCatastrophe(claimID As Integer, catastropheID As Integer)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@CatastropheID", catastropheID),
            DatabaseHelper.CreateParam("@LinkedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Catastrophe_LinkClaim", params)
    End Sub

    Public Shared Function GetCatastropheSummary(catastropheID As Integer) As DataSet
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@CatastropheID", catastropheID)}
        Return DatabaseHelper.ExecuteDataSet("Claims.usp_Catastrophe_GetSummary", params)
    End Function

    ' --- Vendor ---
    Public Shared Function SearchVendors(Optional vendorType As String = Nothing, Optional stateCode As String = Nothing,
                                          Optional preferredOnly As Boolean = False) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@VendorType", vendorType),
            DatabaseHelper.CreateParam("@StateCode", stateCode),
            DatabaseHelper.CreateParam("@PreferredOnly", preferredOnly),
            DatabaseHelper.CreateParam("@IsActive", True)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Claims.usp_Vendor_Search", params)
    End Function

End Class
