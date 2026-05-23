Imports System.Configuration

''' <summary>
''' Centralized application settings access.
''' Reads from app.config/web.config AppSettings section.
''' </summary>
Public Class AppSettings

    ' --- Connection ---
    Public Shared ReadOnly Property ConnectionString As String
        Get
            Return ConfigurationManager.ConnectionStrings("PropertyInsuranceDB").ConnectionString
        End Get
    End Property

    ' --- Application Info ---
    Public Shared ReadOnly Property ApplicationName As String
        Get
            Return GetSetting("ApplicationName", "Property Insurance Claims Management")
        End Get
    End Property

    Public Shared ReadOnly Property ApplicationVersion As String
        Get
            Return GetSetting("ApplicationVersion", "1.0.0")
        End Get
    End Property

    Public Shared ReadOnly Property CompanyName As String
        Get
            Return GetSetting("CompanyName", "Insurance Company")
        End Get
    End Property

    ' --- Security ---
    Public Shared ReadOnly Property MaxLoginAttempts As Integer
        Get
            Return GetSettingInt("MaxLoginAttempts", 5)
        End Get
    End Property

    Public Shared ReadOnly Property SessionTimeoutMinutes As Integer
        Get
            Return GetSettingInt("SessionTimeoutMinutes", 30)
        End Get
    End Property

    Public Shared ReadOnly Property PasswordMinLength As Integer
        Get
            Return GetSettingInt("PasswordMinLength", 8)
        End Get
    End Property

    ' --- File Paths ---
    Public Shared ReadOnly Property ReportTemplatePath As String
        Get
            Return GetSetting("ReportTemplatePath", "C:\Reports\Templates\")
        End Get
    End Property

    Public Shared ReadOnly Property DocumentStoragePath As String
        Get
            Return GetSetting("DocumentStoragePath", "C:\Documents\Insurance\")
        End Get
    End Property

    Public Shared ReadOnly Property ExportPath As String
        Get
            Return GetSetting("ExportPath", "C:\Exports\")
        End Get
    End Property

    ' --- Business Rules ---
    Public Shared ReadOnly Property DefaultPageSize As Integer
        Get
            Return GetSettingInt("DefaultPageSize", 50)
        End Get
    End Property

    Public Shared ReadOnly Property ReserveApprovalThreshold As Decimal
        Get
            Return GetSettingDecimal("ReserveApprovalThreshold", 50000D)
        End Get
    End Property

    Public Shared ReadOnly Property PaymentApprovalThreshold As Decimal
        Get
            Return GetSettingDecimal("PaymentApprovalThreshold", 10000D)
        End Get
    End Property

    Public Shared ReadOnly Property FraudScoreThreshold As Decimal
        Get
            Return GetSettingDecimal("FraudScoreThreshold", 70D)
        End Get
    End Property

    ' --- Helper Methods ---
    Private Shared Function GetSetting(key As String, defaultValue As String) As String
        Dim value As String = ConfigurationManager.AppSettings(key)
        Return If(String.IsNullOrEmpty(value), defaultValue, value)
    End Function

    Private Shared Function GetSettingInt(key As String, defaultValue As Integer) As Integer
        Dim value As String = ConfigurationManager.AppSettings(key)
        Dim result As Integer
        If Integer.TryParse(value, result) Then Return result
        Return defaultValue
    End Function

    Private Shared Function GetSettingDecimal(key As String, defaultValue As Decimal) As Decimal
        Dim value As String = ConfigurationManager.AppSettings(key)
        Dim result As Decimal
        If Decimal.TryParse(value, result) Then Return result
        Return defaultValue
    End Function

End Class
