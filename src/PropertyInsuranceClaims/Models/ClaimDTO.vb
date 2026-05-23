''' <summary>
''' Data transfer object for Claim entity.
''' </summary>
Public Class ClaimDTO
    Public Property ClaimID As Integer
    Public Property ClaimNumber As String
    Public Property PolicyID As Integer
    Public Property CustomerID As Integer
    Public Property PropertyID As Integer
    Public Property ClaimStatus As String
    Public Property ClaimType As String
    Public Property CatastropheID As Integer?
    Public Property LossDate As DateTime
    Public Property ReportedDate As DateTime
    Public Property LossDescription As String
    Public Property LossLocation As String
    Public Property PoliceReportNumber As String
    Public Property FireReportNumber As String
    Public Property WeatherCondition As String
    Public Property PointOfOrigin As String
    Public Property EstimatedLoss As Decimal?
    Public Property ActualLoss As Decimal?
    Public Property DeductibleAmount As Decimal
    Public Property PolicyLimit As Decimal
    Public Property TotalPaid As Decimal
    Public Property TotalReserve As Decimal
    Public Property TotalRecovery As Decimal
    Public Property NetIncurred As Decimal
    Public Property AdjusterID As Integer?
    Public Property Priority As String = "NORMAL"
    Public Property Complexity As String = "SIMPLE"
    Public Property FraudScore As Decimal
    Public Property IsSIUReferred As Boolean
    Public Property IsLitigation As Boolean
    Public Property IsSubrogation As Boolean

    ' Display fields
    Public Property PolicyNumber As String
    Public Property CustomerName As String
    Public Property PropertyAddress As String
    Public Property AdjusterName As String
End Class

''' <summary>
''' Search criteria for claim search.
''' </summary>
Public Class ClaimSearchCriteria
    Public Property ClaimNumber As String
    Public Property PolicyNumber As String
    Public Property CustomerName As String
    Public Property ClaimStatus As String
    Public Property ClaimType As String
    Public Property LossDateFrom As Date?
    Public Property LossDateTo As Date?
    Public Property AdjusterID As Integer?
    Public Property CatastropheID As Integer?
    Public Property Priority As String
    Public Property MinAmount As Decimal?
    Public Property MaxAmount As Decimal?
    Public Property PageNumber As Integer = 1
    Public Property PageSize As Integer = 50
    Public Property SortColumn As String = "ReportedDate"
    Public Property SortDirection As String = "DESC"
    Public Property TotalRecords As Integer
End Class

''' <summary>
''' DTO for claim payment creation.
''' </summary>
Public Class ClaimPaymentDTO
    Public Property PaymentID As Integer
    Public Property PaymentNumber As String
    Public Property ClaimID As Integer
    Public Property PaymentType As String
    Public Property PaymentMethod As String = "CHECK"
    Public Property PayeeType As String
    Public Property PayeeName As String
    Public Property PayeeAddress As String
    Public Property Amount As Decimal
    Public Property CoverageCode As String
    Public Property InvoiceNumber As String
    Public Property Description As String
    Public Property TaxReportable As Boolean = False
End Class
