''' <summary>
''' Data transfer object for Property entity.
''' </summary>
Public Class PropertyDTO
    Public Property PropertyID As Integer
    Public Property PropertyNumber As String
    Public Property CustomerID As Integer
    Public Property PropertyType As String
    Public Property ConstructionType As String
    Public Property OccupancyType As String
    Public Property YearBuilt As Integer
    Public Property SquareFootage As Integer
    Public Property NumberOfStories As Integer = 1
    Public Property RoofType As String
    Public Property RoofAge As Integer?
    Public Property HasBasement As Boolean = False
    Public Property HasPool As Boolean = False
    Public Property HasFireAlarm As Boolean = False
    Public Property HasBurglarAlarm As Boolean = False
    Public Property HasSprinklerSystem As Boolean = False
    Public Property DistanceToFireStation As Decimal?
    Public Property DistanceToHydrant As Decimal?
    Public Property FireProtectionClass As Integer?
    Public Property FloodZone As String
    Public Property MarketValue As Decimal?
    Public Property ReplacementCost As Decimal?
    Public Property AddressLine1 As String
    Public Property AddressLine2 As String
    Public Property City As String
    Public Property StateCode As String
    Public Property ZipCode As String
    Public Property County As String
    Public Property TerritoryID As Integer?
    Public Property IsActive As Boolean = True

    Public ReadOnly Property FullAddress As String
        Get
            Dim addr As String = AddressLine1
            If Not String.IsNullOrEmpty(AddressLine2) Then addr &= ", " & AddressLine2
            addr &= $", {City}, {StateCode} {ZipCode}"
            Return addr
        End Get
    End Property

    Public ReadOnly Property PropertyAge As Integer
        Get
            Return DateTime.Now.Year - YearBuilt
        End Get
    End Property
End Class
