''' <summary>
''' Data transfer object for Policy entity.
''' </summary>
Public Class PolicyDTO
    Public Property PolicyID As Integer
    Public Property PolicyNumber As String
    Public Property PolicyVersion As Integer
    Public Property PolicyType As String
    Public Property PolicyStatus As String
    Public Property CustomerID As Integer
    Public Property PropertyID As Integer
    Public Property AgentID As Integer
    Public Property EffectiveDate As Date
    Public Property ExpiryDate As Date
    Public Property TermMonths As Integer = 12
    Public Property PaymentPlan As String = "ANNUAL"
    Public Property BillingMethod As String = "DIRECT"
    Public Property CommissionRate As Decimal
    Public Property AnnualPremium As Decimal
    Public Property WrittenPremium As Decimal
    Public Property TotalTaxes As Decimal
    Public Property GrossPremium As Decimal
    Public Property TotalInsuredValue As Decimal
    Public Property CommissionAmount As Decimal
    Public Property PriorCarrier As String
    Public Property PriorPolicyNumber As String
    Public Property PriorExpiryDate As Date?
    Public Property YearsWithPriorCarrier As Integer?
    Public Property ClaimFreeYears As Integer = 0
    Public Property IsRenewal As Boolean = False
    Public Property RenewalCount As Integer = 0

    ' Display fields (from joins)
    Public Property CustomerName As String
    Public Property CustomerNumber As String
    Public Property PropertyAddress As String
    Public Property AgentName As String
End Class

''' <summary>
''' Search criteria for policy search.
''' </summary>
Public Class PolicySearchCriteria
    Public Property PolicyNumber As String
    Public Property CustomerName As String
    Public Property PolicyType As String
    Public Property PolicyStatus As String
    Public Property StateCode As String
    Public Property EffectiveDateFrom As Date?
    Public Property EffectiveDateTo As Date?
    Public Property AgentID As Integer?
    Public Property PageNumber As Integer = 1
    Public Property PageSize As Integer = 50
    Public Property SortColumn As String = "PolicyNumber"
    Public Property SortDirection As String = "ASC"
    Public Property TotalRecords As Integer
End Class
