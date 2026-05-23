Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Policy operations.
''' </summary>
Public Class PolicyDataAccess

    Public Shared Function CreateQuote(dto As PolicyDTO) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyType", dto.PolicyType),
            DatabaseHelper.CreateParam("@CustomerID", dto.CustomerID),
            DatabaseHelper.CreateParam("@PropertyID", dto.PropertyID),
            DatabaseHelper.CreateParam("@AgentID", dto.AgentID),
            DatabaseHelper.CreateParam("@EffectiveDate", dto.EffectiveDate),
            DatabaseHelper.CreateParam("@TermMonths", dto.TermMonths),
            DatabaseHelper.CreateParam("@PaymentPlan", dto.PaymentPlan),
            DatabaseHelper.CreateParam("@BillingMethod", dto.BillingMethod),
            DatabaseHelper.CreateParam("@PriorCarrier", dto.PriorCarrier),
            DatabaseHelper.CreateParam("@PriorPolicyNumber", dto.PriorPolicyNumber),
            DatabaseHelper.CreateParam("@PriorExpiryDate", dto.PriorExpiryDate),
            DatabaseHelper.CreateParam("@YearsWithPriorCarrier", dto.YearsWithPriorCarrier),
            DatabaseHelper.CreateParam("@ClaimFreeYears", dto.ClaimFreeYears),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim policyIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PolicyID", SqlDbType.Int)
        Dim policyNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PolicyNumber", SqlDbType.VarChar)
        params.Add(policyIDParam)
        params.Add(policyNumberParam)

        DatabaseHelper.ExecuteNonQuery("Policy.usp_Policy_CreateQuote", params.ToArray())

        dto.PolicyID = CInt(policyIDParam.Value)
        dto.PolicyNumber = CStr(policyNumberParam.Value)
        Return dto.PolicyID
    End Function

    Public Shared Function GetDetails(policyID As Integer) As DataSet
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@PolicyID", policyID)
        }
        Return DatabaseHelper.ExecuteDataSet("Policy.usp_Policy_GetDetails", params)
    End Function

    Public Shared Function Search(criteria As PolicySearchCriteria) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyNumber", criteria.PolicyNumber),
            DatabaseHelper.CreateParam("@CustomerName", criteria.CustomerName),
            DatabaseHelper.CreateParam("@PolicyType", criteria.PolicyType),
            DatabaseHelper.CreateParam("@PolicyStatus", criteria.PolicyStatus),
            DatabaseHelper.CreateParam("@StateCode", criteria.StateCode),
            DatabaseHelper.CreateParam("@EffectiveDateFrom", criteria.EffectiveDateFrom),
            DatabaseHelper.CreateParam("@EffectiveDateTo", criteria.EffectiveDateTo),
            DatabaseHelper.CreateParam("@AgentID", criteria.AgentID),
            DatabaseHelper.CreateParam("@PageNumber", criteria.PageNumber),
            DatabaseHelper.CreateParam("@PageSize", criteria.PageSize),
            DatabaseHelper.CreateParam("@SortColumn", criteria.SortColumn),
            DatabaseHelper.CreateParam("@SortDirection", criteria.SortDirection)
        }

        Dim totalRecordsParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
        params.Add(totalRecordsParam)

        Dim dt As DataTable = DatabaseHelper.ExecuteWithOutput("Policy.usp_Policy_Search", params.ToArray())
        criteria.TotalRecords = CInt(If(totalRecordsParam.Value, 0))
        Return dt
    End Function

End Class
