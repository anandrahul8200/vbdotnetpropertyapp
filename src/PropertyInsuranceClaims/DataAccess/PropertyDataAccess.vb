Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Property operations.
''' </summary>
Public Class PropertyDataAccess

    Public Shared Function Create(dto As PropertyDTO) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@CustomerID", dto.CustomerID),
            DatabaseHelper.CreateParam("@PropertyType", dto.PropertyType),
            DatabaseHelper.CreateParam("@ConstructionType", dto.ConstructionType),
            DatabaseHelper.CreateParam("@OccupancyType", dto.OccupancyType),
            DatabaseHelper.CreateParam("@YearBuilt", dto.YearBuilt),
            DatabaseHelper.CreateParam("@SquareFootage", dto.SquareFootage),
            DatabaseHelper.CreateParam("@NumberOfStories", dto.NumberOfStories),
            DatabaseHelper.CreateParam("@RoofType", dto.RoofType),
            DatabaseHelper.CreateParam("@RoofAge", dto.RoofAge),
            DatabaseHelper.CreateParam("@HasBasement", dto.HasBasement),
            DatabaseHelper.CreateParam("@HasPool", dto.HasPool),
            DatabaseHelper.CreateParam("@HasFireAlarm", dto.HasFireAlarm),
            DatabaseHelper.CreateParam("@HasBurglarAlarm", dto.HasBurglarAlarm),
            DatabaseHelper.CreateParam("@HasSprinklerSystem", dto.HasSprinklerSystem),
            DatabaseHelper.CreateParam("@DistanceToFireStation", dto.DistanceToFireStation),
            DatabaseHelper.CreateParam("@DistanceToHydrant", dto.DistanceToHydrant),
            DatabaseHelper.CreateParam("@FireProtectionClass", dto.FireProtectionClass),
            DatabaseHelper.CreateParam("@FloodZone", dto.FloodZone),
            DatabaseHelper.CreateParam("@MarketValue", dto.MarketValue),
            DatabaseHelper.CreateParam("@ReplacementCost", dto.ReplacementCost),
            DatabaseHelper.CreateParam("@AddressLine1", dto.AddressLine1),
            DatabaseHelper.CreateParam("@AddressLine2", dto.AddressLine2),
            DatabaseHelper.CreateParam("@City", dto.City),
            DatabaseHelper.CreateParam("@StateCode", dto.StateCode),
            DatabaseHelper.CreateParam("@ZipCode", dto.ZipCode),
            DatabaseHelper.CreateParam("@County", dto.County),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }

        Dim propertyIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PropertyID", SqlDbType.Int)
        Dim propertyNumberParam As SqlParameter = DatabaseHelper.CreateOutputParam("@PropertyNumber", SqlDbType.VarChar)
        params.Add(propertyIDParam)
        params.Add(propertyNumberParam)

        DatabaseHelper.ExecuteNonQuery("Policy.usp_Property_Create", params.ToArray())

        dto.PropertyID = CInt(propertyIDParam.Value)
        dto.PropertyNumber = CStr(propertyNumberParam.Value)
        Return dto.PropertyID
    End Function

    Public Shared Function GetByCustomer(customerID As Integer) As DataTable
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@CustomerID", customerID)}
        Return DatabaseHelper.ExecuteStoredProcedure("Policy.usp_Property_GetByCustomer", params)
    End Function

    Public Shared Function GetByID(propertyID As Integer) As DataTable
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@PropertyID", propertyID)}
        Return DatabaseHelper.ExecuteStoredProcedure("Policy.usp_Property_GetByID", params)
    End Function
End Class
