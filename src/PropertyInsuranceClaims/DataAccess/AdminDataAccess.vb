Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Data access layer for Admin operations — users, config, lookups, audit.
''' </summary>
Public Class AdminDataAccess

    ' --- User Management ---
    Public Shared Function CreateUser(username As String, passwordHash As String, firstName As String, lastName As String,
                                      email As String, roleID As Integer, Optional department As String = Nothing) As Integer
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@Username", username),
            DatabaseHelper.CreateParam("@PasswordHash", passwordHash),
            DatabaseHelper.CreateParam("@FirstName", firstName),
            DatabaseHelper.CreateParam("@LastName", lastName),
            DatabaseHelper.CreateParam("@Email", email),
            DatabaseHelper.CreateParam("@RoleID", roleID),
            DatabaseHelper.CreateParam("@Department", department),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        Dim userIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@UserID", SqlDbType.Int)
        params.Add(userIDParam)
        DatabaseHelper.ExecuteNonQuery("Admin.usp_User_Create", params.ToArray())
        Return CInt(userIDParam.Value)
    End Function

    Public Shared Function GetUserPermissions(userID As Integer) As List(Of String)
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@UserID", userID)}
        Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Admin.usp_User_GetPermissions", params)
        Dim permissions As New List(Of String)
        For Each row As DataRow In dt.Rows
            permissions.Add(row("PermissionCode").ToString().ToUpper())
        Next
        Return permissions
    End Function

    ' --- System Config ---
    Public Shared Function GetConfig(Optional key As String = Nothing, Optional category As String = Nothing) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ConfigKey", key),
            DatabaseHelper.CreateParam("@Category", category)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Admin.usp_Config_Get", params)
    End Function

    Public Shared Sub SetConfig(key As String, value As String)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ConfigKey", key),
            DatabaseHelper.CreateParam("@ConfigValue", value),
            DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Admin.usp_Config_Set", params)
    End Sub

    Public Shared Function GetConfigValue(key As String, Optional defaultValue As String = "") As String
        Dim dt As DataTable = GetConfig(key)
        If dt.Rows.Count > 0 Then Return dt.Rows(0)("ConfigValue").ToString()
        Return defaultValue
    End Function

    ' --- Lookups ---
    Public Shared Function GetLookupValues(category As String) As DataTable
        Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@Category", category)}
        Return DatabaseHelper.ExecuteStoredProcedure("Admin.usp_Lookup_GetByCategory", params)
    End Function

    Public Shared Sub CreateLookup(category As String, code As String, value As String, Optional description As String = Nothing)
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@Category", category),
            DatabaseHelper.CreateParam("@LookupCode", code),
            DatabaseHelper.CreateParam("@LookupValue", value),
            DatabaseHelper.CreateParam("@Description", description),
            DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
        }
        DatabaseHelper.ExecuteNonQuery("Admin.usp_Lookup_Create", params)
    End Sub

    ' --- Audit ---
    Public Shared Function SearchAuditLog(Optional tableName As String = Nothing, Optional username As String = Nothing,
                                           Optional action As String = Nothing, Optional dateFrom As Date? = Nothing,
                                           Optional dateTo As Date? = Nothing, Optional pageNumber As Integer = 1) As DataTable
        Dim params As New List(Of SqlParameter) From {
            DatabaseHelper.CreateParam("@TableName", tableName),
            DatabaseHelper.CreateParam("@Username", username),
            DatabaseHelper.CreateParam("@Action", action),
            DatabaseHelper.CreateParam("@DateFrom", dateFrom),
            DatabaseHelper.CreateParam("@DateTo", dateTo),
            DatabaseHelper.CreateParam("@PageNumber", pageNumber),
            DatabaseHelper.CreateParam("@PageSize", 100)
        }
        Dim totalParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
        params.Add(totalParam)
        Return DatabaseHelper.ExecuteWithOutput("Admin.usp_Audit_Search", params.ToArray())
    End Function

    ' --- Error Log ---
    Public Shared Function SearchErrorLog(Optional procedureName As String = Nothing, Optional dateFrom As Date? = Nothing) As DataTable
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@ProcedureName", procedureName),
            DatabaseHelper.CreateParam("@DateFrom", dateFrom)
        }
        Return DatabaseHelper.ExecuteStoredProcedure("Admin.usp_ErrorLog_Search", params)
    End Function

End Class
