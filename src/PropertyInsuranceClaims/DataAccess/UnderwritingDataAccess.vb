Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Underwriting operations.
''' </summary>
Public Class UnderwritingDataAccess

    Public Shared Function CalculatePremium(policyID As Integer) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@CalculatedBy", GlobalState.CurrentUser),
            DatabaseHelper.CreateParam("@RecalculateAll", True)
        }
        Dim totalPremiumParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalPremium", SqlDbType.Decimal)
        Dim totalTaxesParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalTaxes", SqlDbType.Decimal)
        Dim grossPremiumParam As SqlParameter = DatabaseHelper.CreateOutputParam("@GrossPremium", SqlDbType.Decimal)
        params.AddRange({totalPremiumParam, totalTaxesParam, grossPremiumParam})

        DatabaseHelper.ExecuteNonQuery("Underwriting.usp_Premium_Calculate", params.ToArray())

        Dim dt As New DataTable()
        dt.Columns.Add("TotalPremium", GetType(Decimal))
        dt.Columns.Add("TotalTaxes", GetType(Decimal))
        dt.Columns.Add("GrossPremium", GetType(Decimal))
        dt.Rows.Add(CDec(totalPremiumParam.Value), CDec(totalTaxesParam.Value), CDec(grossPremiumParam.Value))
        Return dt
    End Function

    Public Shared Function EvaluateRules(policyID As Integer) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@PolicyID", policyID),
            DatabaseHelper.CreateParam("@EvaluatedBy", GlobalState.CurrentUser)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Underwriting.usp_Rules_Evaluate", params)
    End Function

    Public Shared Sub ProcessReferral(referralID As Integer, decision As String, notes As String, conditions As String)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ReferralID", referralID),
            DatabaseHelper.CreateParam("@Decision", decision),
            DatabaseHelper.CreateParam("@DecisionNotes", notes),
            DatabaseHelper.CreateParam("@Conditions", conditions),
            DatabaseHelper.CreateParam("@ReviewedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Underwriting.usp_Referral_Process", params)
    End Sub

    Public Shared Function GetPendingReferrals(Optional assignedTo As String = Nothing, Optional pageNumber As Integer = 1) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@AssignedTo", assignedTo),
            DatabaseHelper.CreateParam("@PageNumber", pageNumber),
            DatabaseHelper.CreateParam("@PageSize", 50)
        }
        Dim totalParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
        params.Add(totalParam)
        Return DatabaseHelper.ExecuteWithOutput("Underwriting.usp_Referral_SearchPending", params.ToArray())
    End Function

    Public Shared Function GetRatingWorksheet(policyID As Integer) As DataTable
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@PolicyID", policyID)}
        Return DatabaseHelper.ExecuteStoredProcedure("Underwriting.usp_Worksheet_GetByPolicy", params)
    End Function

    Public Shared Function CheckMoratorium(policyType As String, stateCode As String, zipCode As String) As Boolean
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyType", policyType),
            DatabaseHelper.CreateParam("@StateCode", stateCode),
            DatabaseHelper.CreateParam("@ZipCode", zipCode)
        }
        Dim isMoratoriumParam As SqlParameter = DatabaseHelper.CreateOutputParam("@IsMoratorium", SqlDbType.Bit)
        Dim nameParam As SqlParameter = DatabaseHelper.CreateOutputParam("@MoratoriumName", SqlDbType.VarChar)
        nameParam.Size = 200
        params.AddRange({isMoratoriumParam, nameParam})

        DatabaseHelper.ExecuteNonQuery("Underwriting.usp_Moratorium_Check", params.ToArray())
        Return CBool(isMoratoriumParam.Value)
    End Function

    Public Shared Function GetActiveMoratoriums() As DataTable
        Return DatabaseHelper.ExecuteStoredProcedure("Underwriting.usp_Moratorium_GetActive", Nothing)
    End Function
End Class
