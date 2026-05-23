''' <summary>
''' Data transfer object for Customer entity.
''' </summary>
Public Class CustomerDTO
    Public Property CustomerID As Integer
    Public Property CustomerNumber As String
    Public Property CustomerType As String ' I=Individual, C=Commercial
    Public Property Title As String
    Public Property FirstName As String
    Public Property LastName As String
    Public Property CompanyName As String
    Public Property TaxID As String
    Public Property DateOfBirth As Date?
    Public Property Gender As String
    Public Property Email As String
    Public Property Phone As String
    Public Property MobilePhone As String
    Public Property AddressLine1 As String
    Public Property AddressLine2 As String
    Public Property City As String
    Public Property StateCode As String
    Public Property ZipCode As String
    Public Property County As String
    Public Property Occupation As String
    Public Property AnnualIncome As Decimal?
    Public Property CreditScore As Integer?
    Public Property RiskTier As String
    Public Property CustomerSince As Date?
    Public Property IsActive As Boolean = True

    ' Computed
    Public Property ActivePolicyCount As Integer
    Public Property OpenClaimCount As Integer

    Public ReadOnly Property DisplayName As String
        Get
            If CustomerType = "C" Then Return CompanyName
            Return $"{FirstName} {LastName}".Trim()
        End Get
    End Property
End Class

''' <summary>
''' Search criteria for customer search.
''' </summary>
Public Class CustomerSearchCriteria
    Public Property SearchTerm As String
    Public Property CustomerNumber As String
    Public Property CustomerType As String
    Public Property StateCode As String
    Public Property City As String
    Public Property ZipCode As String
    Public Property IsActive As Boolean? = True
    Public Property PageNumber As Integer = 1
    Public Property PageSize As Integer = 50
    Public Property SortColumn As String = "CustomerNumber"
    Public Property SortDirection As String = "ASC"
    Public Property TotalRecords As Integer
End Class
