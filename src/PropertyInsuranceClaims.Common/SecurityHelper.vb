Imports System
Imports System.Security.Cryptography
Imports System.Text
Imports System.Linq

''' <summary>
''' Security utilities for password hashing, validation, and encryption.
''' </summary>
Public Class SecurityHelper

    ''' <summary>
    ''' Hashes a password using SHA-256 with salt.
    ''' </summary>
    Public Shared Function HashPassword(password As String, Optional salt As String = Nothing) As String
        If String.IsNullOrEmpty(salt) Then
            salt = GenerateSalt()
        End If

        Using sha256 As SHA256 = SHA256.Create()
            Dim combined As String = salt & password
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(combined)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Return salt & ":" & Convert.ToBase64String(hash)
        End Using
    End Function

    ''' <summary>
    ''' Verifies a password against a stored hash.
    ''' </summary>
    Public Shared Function VerifyPassword(password As String, storedHash As String) As Boolean
        If String.IsNullOrEmpty(storedHash) OrElse Not storedHash.Contains(":") Then
            Return False
        End If

        Dim parts As String() = storedHash.Split(":"c)
        If parts.Length <> 2 Then Return False

        Dim salt As String = parts(0)
        Dim newHash As String = HashPassword(password, salt)
        Return String.Equals(newHash, storedHash, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Generates a random salt for password hashing.
    ''' </summary>
    Public Shared Function GenerateSalt() As String
        Dim saltBytes(15) As Byte
        Using rng As New RNGCryptoServiceProvider()
            rng.GetBytes(saltBytes)
        End Using
        Return Convert.ToBase64String(saltBytes)
    End Function

    ''' <summary>
    ''' Validates password meets complexity requirements.
    ''' </summary>
    Public Shared Function ValidatePasswordComplexity(password As String, ByRef errorMessage As String) As Boolean
        errorMessage = String.Empty

        If String.IsNullOrEmpty(password) Then
            errorMessage = "Password is required."
            Return False
        End If

        If password.Length < AppSettings.PasswordMinLength Then
            errorMessage = $"Password must be at least {AppSettings.PasswordMinLength} characters."
            Return False
        End If

        If Not password.Any(Function(c) Char.IsUpper(c)) Then
            errorMessage = "Password must contain at least one uppercase letter."
            Return False
        End If

        If Not password.Any(Function(c) Char.IsLower(c)) Then
            errorMessage = "Password must contain at least one lowercase letter."
            Return False
        End If

        If Not password.Any(Function(c) Char.IsDigit(c)) Then
            errorMessage = "Password must contain at least one digit."
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Sanitizes input to prevent SQL injection (defense in depth — SPs use parameters).
    ''' </summary>
    Public Shared Function SanitizeInput(input As String) As String
        If String.IsNullOrEmpty(input) Then Return input
        Return input.Replace("'", "''").Replace("--", "").Replace(";", "").Trim()
    End Function

End Class
