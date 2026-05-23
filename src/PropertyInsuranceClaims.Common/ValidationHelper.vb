Imports System
Imports System.Text.RegularExpressions
Imports System.Linq
Imports System.Windows.Forms

''' <summary>
''' Common validation utilities used across all forms.
''' </summary>
Public Class ValidationHelper

    ''' <summary>
    ''' Validates an email address format.
    ''' </summary>
    Public Shared Function IsValidEmail(email As String) As Boolean
        If String.IsNullOrWhiteSpace(email) Then Return False
        Dim pattern As String = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"
        Return Regex.IsMatch(email, pattern)
    End Function

    ''' <summary>
    ''' Validates a US phone number (10 digits, various formats accepted).
    ''' </summary>
    Public Shared Function IsValidPhone(phone As String) As Boolean
        If String.IsNullOrWhiteSpace(phone) Then Return False
        Dim digitsOnly As String = Regex.Replace(phone, "[^0-9]", "")
        Return digitsOnly.Length = 10 OrElse digitsOnly.Length = 11
    End Function

    ''' <summary>
    ''' Validates a US zip code (5 digits or 5+4 format).
    ''' </summary>
    Public Shared Function IsValidZipCode(zip As String) As Boolean
        If String.IsNullOrWhiteSpace(zip) Then Return False
        Return Regex.IsMatch(zip.Trim(), "^\d{5}(-\d{4})?$")
    End Function

    ''' <summary>
    ''' Validates a US state code (2 uppercase letters).
    ''' </summary>
    Public Shared Function IsValidStateCode(state As String) As Boolean
        If String.IsNullOrWhiteSpace(state) Then Return False
        Dim validStates() As String = {"AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                                        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                                        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                                        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                                        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"}
        Return Array.IndexOf(validStates, state.Trim().ToUpper()) >= 0
    End Function

    ''' <summary>
    ''' Validates a decimal amount is positive and within range.
    ''' </summary>
    Public Shared Function IsValidAmount(text As String, ByRef amount As Decimal, Optional minValue As Decimal = 0, Optional maxValue As Decimal = Decimal.MaxValue) As Boolean
        amount = 0
        If String.IsNullOrWhiteSpace(text) Then Return False
        If Not Decimal.TryParse(text.Replace("$", "").Replace(",", ""), amount) Then Return False
        Return amount >= minValue AndAlso amount <= maxValue
    End Function

    ''' <summary>
    ''' Validates a date is within an acceptable range.
    ''' </summary>
    Public Shared Function IsValidDate(dateValue As Date, Optional minDate As Date? = Nothing, Optional maxDate As Date? = Nothing) As Boolean
        If Not minDate.HasValue Then minDate = New Date(1900, 1, 1)
        If Not maxDate.HasValue Then maxDate = Date.Today.AddYears(5)
        Return dateValue >= minDate.Value AndAlso dateValue <= maxDate.Value
    End Function

    ''' <summary>
    ''' Validates year built is reasonable.
    ''' </summary>
    Public Shared Function IsValidYearBuilt(text As String, ByRef year As Integer) As Boolean
        year = 0
        If Not Integer.TryParse(text, year) Then Return False
        Return year >= 1800 AndAlso year <= DateTime.Now.Year
    End Function

    ''' <summary>
    ''' Validates a policy number format (POL + 7 digits).
    ''' </summary>
    Public Shared Function IsValidPolicyNumber(policyNumber As String) As Boolean
        If String.IsNullOrWhiteSpace(policyNumber) Then Return False
        Return Regex.IsMatch(policyNumber.Trim(), "^POL\d{7}$")
    End Function

    ''' <summary>
    ''' Validates a claim number format (CLM + 7 digits).
    ''' </summary>
    Public Shared Function IsValidClaimNumber(claimNumber As String) As Boolean
        If String.IsNullOrWhiteSpace(claimNumber) Then Return False
        Return Regex.IsMatch(claimNumber.Trim(), "^CLM\d{7}$")
    End Function

    ''' <summary>
    ''' Formats a phone number for display.
    ''' </summary>
    Public Shared Function FormatPhone(phone As String) As String
        If String.IsNullOrWhiteSpace(phone) Then Return ""
        Dim digits As String = Regex.Replace(phone, "[^0-9]", "")
        If digits.Length = 10 Then Return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}"
        If digits.Length = 11 AndAlso digits.StartsWith("1") Then Return $"+1 ({digits.Substring(1, 3)}) {digits.Substring(4, 3)}-{digits.Substring(7, 4)}"
        Return phone
    End Function

    ''' <summary>
    ''' Formats a currency amount for display.
    ''' </summary>
    Public Shared Function FormatCurrency(amount As Decimal) As String
        Return amount.ToString("C2")
    End Function

    ''' <summary>
    ''' Formats a currency amount abbreviated (e.g., $1.2M, $500K).
    ''' </summary>
    Public Shared Function FormatCurrencyAbbreviated(amount As Decimal) As String
        If Math.Abs(amount) >= 1000000 Then Return $"${amount / 1000000:N1}M"
        If Math.Abs(amount) >= 1000 Then Return $"${amount / 1000:N0}K"
        Return amount.ToString("C0")
    End Function

    ''' <summary>
    ''' Validates that a required text field is not empty.
    ''' Shows a message box and returns False if empty.
    ''' </summary>
    Public Shared Function RequireField(control As TextBox, fieldName As String) As Boolean
        If String.IsNullOrWhiteSpace(control.Text) Then
            MessageBox.Show($"{fieldName} is required.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            control.Focus()
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Validates that a combo box has a selection.
    ''' </summary>
    Public Shared Function RequireSelection(control As ComboBox, fieldName As String) As Boolean
        If control.SelectedIndex < 0 OrElse (control.SelectedIndex = 0 AndAlso control.Items(0).ToString().StartsWith("(")) Then
            MessageBox.Show($"Please select a {fieldName}.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            control.Focus()
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Validates a numeric text field and shows error if invalid.
    ''' </summary>
    Public Shared Function RequireNumeric(control As TextBox, fieldName As String, ByRef value As Decimal, Optional minValue As Decimal = 0) As Boolean
        If Not IsValidAmount(control.Text, value, minValue) Then
            MessageBox.Show($"{fieldName} must be a valid number" & If(minValue > 0, $" greater than {minValue:N2}", "") & ".",
                "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            control.Focus()
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' Validates a date field is not in the future.
    ''' </summary>
    Public Shared Function RequirePastDate(dateValue As Date, fieldName As String) As Boolean
        If dateValue > DateTime.Now Then
            MessageBox.Show($"{fieldName} cannot be in the future.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If
        Return True
    End Function

End Class
