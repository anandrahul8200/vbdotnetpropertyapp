Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Customer operations.
''' All methods call stored procedures via DatabaseHelper.
''' </summary>
Public Class CustomerDataAccess

    Public Shared Function Create(dto As CustomerDTO) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@CustomerType", dto.CustomerType),
            DatabaseHelper.CreateParam("@Title", dto.Title),
            DatabaseHelper.CreateParam("@FirstName", dto.FirstName),
            DatabaseHelper.CreateParam("@LastName", dto.LastName),
            DatabaseHelper.CreateParam("@CompanyName", dto.CompanyName),
            DatabaseHelper.CreateParam("@TaxID", dto.TaxID),
            DatabaseHelper.CreateParam("@DateOfBirth", dto.DateOfBirth),
            DatabaseHelper.CreateParam("@Gender", dto.Gender),
            DatabaseHelper.CreateParam("@Email", dto.Email),
            DatabaseHelper.CreateParam("@Phone", dto.Phone),
            DatabaseHelper.CreateParam("@MobilePhone", dto.MobilePhone),
            DatabaseHelper.CreateParam("@AddressLine1", dto.AddressLine1),
            DatabaseHelper.CreateParam("@AddressLine2", dto.AddressLine2),
            DatabaseHelper.CreateParam("@City", dto.City),
            DatabaseHelper.CreateParam("@StateCode", dto.StateCode),
            DatabaseHelper.CreateParam("@ZipCode", dto.ZipCode),
            DatabaseHelper.CreateParam("@County", dto.County),
            DatabaseHelper.CreateParam("@Occupation", dto.Occupation),
            DatabaseHelper.CreateParam("@AnnualIncome", dto.AnnualIncome),
            DatabaseHelper.CreateParam("@CreditScore", dto.CreditScore),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim customerIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@CustomerID", SqlDbType.Int)
        Dim customerNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@CustomerNumber", SqlDbType.VarChar)
        params.Add(customerIDParam)
        params.Add(customerNumberParam)

        DatabaseHelper.ExecuteNonQuery("Policy.usp_Customer_Create", params.ToArray())

        dto.CustomerID = CInt(customerIDParam.Value)
        dto.CustomerNumber = CStr(customerNumberParam.Value)
        Return dto.CustomerID
    End Function

    Public Shared Sub Update(dto As CustomerDTO)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@CustomerID", dto.CustomerID),
            DatabaseHelper.CreateParam("@Title", dto.Title),
            DatabaseHelper.CreateParam("@FirstName", dto.FirstName),
            DatabaseHelper.CreateParam("@LastName", dto.LastName),
            DatabaseHelper.CreateParam("@CompanyName", dto.CompanyName),
            DatabaseHelper.CreateParam("@Email", dto.Email),
            DatabaseHelper.CreateParam("@Phone", dto.Phone),
            DatabaseHelper.CreateParam("@MobilePhone", dto.MobilePhone),
            DatabaseHelper.CreateParam("@AddressLine1", dto.AddressLine1),
            DatabaseHelper.CreateParam("@AddressLine2", dto.AddressLine2),
            DatabaseHelper.CreateParam("@City", dto.City),
            DatabaseHelper.CreateParam("@StateCode", dto.StateCode),
            DatabaseHelper.CreateParam("@ZipCode", dto.ZipCode),
            DatabaseHelper.CreateParam("@County", dto.County),
            DatabaseHelper.CreateParam("@Occupation", dto.Occupation),
            DatabaseHelper.CreateParam("@AnnualIncome", dto.AnnualIncome),
            DatabaseHelper.CreateParam("@CreditScore", dto.CreditScore),
            DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Policy.usp_Customer_Update", params)
    End Sub

    Public Shared Function GetByID(customerID As Integer) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@CustomerID", customerID)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Policy.usp_Customer_GetByID", params)
    End Function

    Public Shared Function Search(criteria As CustomerSearchCriteria) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@SearchTerm", criteria.SearchTerm),
            DatabaseHelper.CreateParam("@CustomerNumber", criteria.CustomerNumber),
            DatabaseHelper.CreateParam("@CustomerType", criteria.CustomerType),
            DatabaseHelper.CreateParam("@StateCode", criteria.StateCode),
            DatabaseHelper.CreateParam("@City", criteria.City),
            DatabaseHelper.CreateParam("@ZipCode", criteria.ZipCode),
            DatabaseHelper.CreateParam("@IsActive", criteria.IsActive),
            DatabaseHelper.CreateParam("@PageNumber", criteria.PageNumber),
            DatabaseHelper.CreateParam("@PageSize", criteria.PageSize),
            DatabaseHelper.CreateParam("@SortColumn", criteria.SortColumn),
            DatabaseHelper.CreateParam("@SortDirection", criteria.SortDirection)
        }

        Dim totalRecordsParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
        params.Add(totalRecordsParam)

        Dim dt As DataTable = DatabaseHelper.ExecuteWithOutput("Policy.usp_Customer_Search", params.ToArray())
        criteria.TotalRecords = CInt(If(totalRecordsParam.Value, 0))
        Return dt
    End Function

End Class
