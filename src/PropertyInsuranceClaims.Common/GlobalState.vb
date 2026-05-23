Imports System
Imports System.Collections.Generic

''' <summary>
''' Application-wide state container for the current session.
''' Set at login, accessible throughout the application.
''' </summary>
Public Class GlobalState

    ' --- Current User ---
    Public Shared Property CurrentUser As String = "SYSTEM"
    Public Shared Property CurrentUserID As Integer = 0
    Public Shared Property CurrentUserFullName As String = ""
    Public Shared Property CurrentRole As String = ""

    ' --- Session ---
    Public Shared Property LoginTime As DateTime = DateTime.MinValue
    Public Shared Property SessionID As String = ""

    ' --- Permissions (loaded at login) ---
    Public Shared Property Permissions As New List(Of String)

    ''' <summary>
    ''' Checks if the current user has a specific permission.
    ''' </summary>
    Public Shared Function HasPermission(permissionCode As String) As Boolean
        Return Permissions.Contains(permissionCode.ToUpper())
    End Function

    ''' <summary>
    ''' Sets the current user session after successful login.
    ''' </summary>
    Public Shared Sub SetSession(userID As Integer, username As String, fullName As String, role As String, userPermissions As List(Of String))
        CurrentUserID = userID
        CurrentUser = username
        CurrentUserFullName = fullName
        CurrentRole = role
        Permissions = userPermissions
        LoginTime = DateTime.Now
        SessionID = Guid.NewGuid().ToString("N")
    End Sub

    ''' <summary>
    ''' Clears the session (logout).
    ''' </summary>
    Public Shared Sub ClearSession()
        CurrentUserID = 0
        CurrentUser = "SYSTEM"
        CurrentUserFullName = ""
        CurrentRole = ""
        Permissions.Clear()
        LoginTime = DateTime.MinValue
        SessionID = ""
    End Sub

    ''' <summary>
    ''' Checks if the session has timed out.
    ''' </summary>
    Public Shared Function IsSessionExpired() As Boolean
        If LoginTime = DateTime.MinValue Then Return True
        Return (DateTime.Now - LoginTime).TotalMinutes > AppSettings.SessionTimeoutMinutes
    End Function

End Class
