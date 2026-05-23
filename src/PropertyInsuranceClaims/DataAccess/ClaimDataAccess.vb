Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Claims operations.
''' </summary>
Public Class ClaimDataAccess

    Public Shared Function Create(dto As ClaimDTO) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@PolicyID", dto.PolicyID),
            DatabaseHelper.CreateParam("@ClaimType", dto.ClaimType),
            DatabaseHelper.CreateParam("@LossDate", dto.LossDate),
            DatabaseHelper.CreateParam("@LossDescription", dto.LossDescription),
            DatabaseHelper.CreateParam("@LossLocation", dto.LossLocation),
            DatabaseHelper.CreateParam("@EstimatedLoss", dto.EstimatedLoss),
            DatabaseHelper.CreateParam("@PoliceReportNumber", dto.PoliceReportNumber),
            DatabaseHelper.CreateParam("@FireReportNumber", dto.FireReportNumber),
            DatabaseHelper.CreateParam("@WeatherCondition", dto.WeatherCondition),
            DatabaseHelper.CreateParam("@PointOfOrigin", dto.PointOfOrigin),
            DatabaseHelper.CreateParam("@CatastropheID", dto.CatastropheID),
            DatabaseHelper.CreateParam("@Priority", dto.Priority),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim claimIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@ClaimID", SqlDbType.Int)
        Dim claimNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@ClaimNumber", SqlDbType.VarChar)
        params.Add(claimIDParam)
        params.Add(claimNumberParam)

        DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_Create", params.ToArray())

        dto.ClaimID = CInt(claimIDParam.Value)
        dto.ClaimNumber = CStr(claimNumberParam.Value)
        Return dto.ClaimID
    End Function

    Public Shared Sub UpdateStatus(claimID As Integer, newStatus As String, reason As String, Optional denialReason As String = Nothing)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@NewStatus", newStatus),
            DatabaseHelper.CreateParam("@Reason", reason),
            DatabaseHelper.CreateParam("@DenialReason", denialReason),
            DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_UpdateStatus", params)
    End Sub

    Public Shared Function GetDetails(claimID As Integer) As DataSet
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ClaimID", claimID)
        }
        Return DatabaseHelper.ExecuteDataSet("Claims.usp_Claim_GetDetails", params)
    End Function

    Public Shared Function Search(criteria As ClaimSearchCriteria) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimNumber", criteria.ClaimNumber),
            DatabaseHelper.CreateParam("@PolicyNumber", criteria.PolicyNumber),
            DatabaseHelper.CreateParam("@CustomerName", criteria.CustomerName),
            DatabaseHelper.CreateParam("@ClaimStatus", criteria.ClaimStatus),
            DatabaseHelper.CreateParam("@ClaimType", criteria.ClaimType),
            DatabaseHelper.CreateParam("@LossDateFrom", criteria.LossDateFrom),
            DatabaseHelper.CreateParam("@LossDateTo", criteria.LossDateTo),
            DatabaseHelper.CreateParam("@AdjusterID", criteria.AdjusterID),
            DatabaseHelper.CreateParam("@CatastropheID", criteria.CatastropheID),
            DatabaseHelper.CreateParam("@Priority", criteria.Priority),
            DatabaseHelper.CreateParam("@MinAmount", criteria.MinAmount),
            DatabaseHelper.CreateParam("@MaxAmount", criteria.MaxAmount),
            DatabaseHelper.CreateParam("@PageNumber", criteria.PageNumber),
            DatabaseHelper.CreateParam("@PageSize", criteria.PageSize),
            DatabaseHelper.CreateParam("@SortColumn", criteria.SortColumn),
            DatabaseHelper.CreateParam("@SortDirection", criteria.SortDirection)
        }

        Dim totalRecordsParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
        params.Add(totalRecordsParam)

        Dim dt As DataTable = DatabaseHelper.ExecuteWithOutput("Claims.usp_Claim_Search", params.ToArray())
        criteria.TotalRecords = CInt(If(totalRecordsParam.Value, 0))
        Return dt
    End Function

    Public Shared Function SetReserve(claimID As Integer, reserveType As String, category As String, amount As Decimal, reason As String) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@ReserveType", reserveType),
            DatabaseHelper.CreateParam("@ReserveCategory", category),
            DatabaseHelper.CreateParam("@Amount", amount),
            DatabaseHelper.CreateParam("@ChangeReason", reason),
            DatabaseHelper.CreateParam("@SetBy", GlobalState.CurrentUser)
        }

        Dim reserveIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@ReserveID", SqlDbType.Int)
        params.Add(reserveIDParam)

        DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_SetReserve", params.ToArray())
        Return CInt(reserveIDParam.Value)
    End Function

    Public Shared Function CreatePayment(dto As ClaimPaymentDTO) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimID", dto.ClaimID),
            DatabaseHelper.CreateParam("@PaymentType", dto.PaymentType),
            DatabaseHelper.CreateParam("@PaymentMethod", dto.PaymentMethod),
            DatabaseHelper.CreateParam("@PayeeType", dto.PayeeType),
            DatabaseHelper.CreateParam("@PayeeName", dto.PayeeName),
            DatabaseHelper.CreateParam("@PayeeAddress", dto.PayeeAddress),
            DatabaseHelper.CreateParam("@Amount", dto.Amount),
            DatabaseHelper.CreateParam("@CoverageCode", dto.CoverageCode),
            DatabaseHelper.CreateParam("@InvoiceNumber", dto.InvoiceNumber),
            DatabaseHelper.CreateParam("@Description", dto.Description),
            DatabaseHelper.CreateParam("@TaxReportable", dto.TaxReportable),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim paymentIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PaymentID", SqlDbType.Int)
        Dim paymentNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PaymentNumber", SqlDbType.VarChar)
        params.Add(paymentIDParam)
        params.Add(paymentNumberParam)

        DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_CreatePayment", params.ToArray())

        dto.PaymentID = CInt(paymentIDParam.Value)
        dto.PaymentNumber = CStr(paymentNumberParam.Value)
        Return dto.PaymentID
    End Function

    Public Shared Function CreateActivity(claimID As Integer, activityType As String, subject As String, description As String,
                                          Optional dueDate As Date? = Nothing, Optional assignedTo As String = Nothing) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@ClaimID", claimID),
            DatabaseHelper.CreateParam("@ActivityType", activityType),
            DatabaseHelper.CreateParam("@Subject", subject),
            DatabaseHelper.CreateParam("@Description", description),
            DatabaseHelper.CreateParam("@DueDate", dueDate),
            DatabaseHelper.CreateParam("@AssignedTo", assignedTo),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim activityIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@ActivityID", SqlDbType.Int)
        params.Add(activityIDParam)

        DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_CreateActivity", params.ToArray())
        Return CInt(activityIDParam.Value)
    End Function

    Public Shared Function GetDashboard(Optional adjusterID As Integer? = Nothing) As DataSet
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@AdjusterID", adjusterID)
        }
        Return DatabaseHelper.ExecuteDataSet("Claims.usp_Claim_GetDashboard", params)
    End Function

End Class
