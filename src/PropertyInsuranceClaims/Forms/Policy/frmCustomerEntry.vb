Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Customer create/edit form.
''' </summary>
Public Class frmCustomerEntry
    Inherits Form

    ' --- Private fields ---
    Private _customerID As Integer = 0
    Private _isEditMode As Boolean = False
    Private _isDirty As Boolean = False

    ' --- Controls ---
    Private cboCustomerType As New ComboBox()
    Private txtTitle As New TextBox()
    Private txtFirstName As New TextBox()
    Private txtLastName As New TextBox()
    Private txtCompanyName As New TextBox()
    Private txtTaxID As New TextBox()
    Private dtpDateOfBirth As New DateTimePicker()
    Private txtEmail As New TextBox()
    Private txtPhone As New TextBox()
    Private txtMobilePhone As New TextBox()
    Private txtAddressLine1 As New TextBox()
    Private txtAddressLine2 As New TextBox()
    Private txtCity As New TextBox()
    Private cboState As New ComboBox()
    Private txtZipCode As New TextBox()
    Private txtCounty As New TextBox()
    Private txtOccupation As New TextBox()
    Private txtAnnualIncome As New TextBox()
    Private txtCreditScore As New TextBox()
    Private lblCustomerNumber As New Label()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(customerID As Integer)
        _customerID = customerID
    End Sub

    Private Sub frmCustomerEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = If(_customerID > 0, "Edit Customer", "New Customer")
            Me.Size = New Drawing.Size(600, 650)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False

            InitializeControls()
            LoadLookups()

            If _customerID > 0 Then
                LoadData()
                _isEditMode = True
            End If

            _isDirty = False
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmCustomerEntry_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        Dim lblWidth As Integer = 110
        Dim ctrlLeft As Integer = 125
        Dim ctrlWidth As Integer = 200

        ' Customer Number (display only)
        AddLabel("Customer #:", 10, y)
        lblCustomerNumber.Location = New Drawing.Point(ctrlLeft, y)
        lblCustomerNumber.AutoSize = True
        lblCustomerNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)
        Me.Controls.Add(lblCustomerNumber)
        y += 30

        ' Customer Type
        AddLabel("Type:", 10, y)
        cboCustomerType.Location = New Drawing.Point(ctrlLeft, y - 3)
        cboCustomerType.Size = New Drawing.Size(150, 20)
        cboCustomerType.DropDownStyle = ComboBoxStyle.DropDownList
        Me.Controls.Add(cboCustomerType)
        y += 30

        ' Name fields
        AddLabel("First Name:", 10, y)
        txtFirstName.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtFirstName.Size = New Drawing.Size(ctrlWidth, 20)
        Me.Controls.Add(txtFirstName)
        y += 30

        AddLabel("Last Name:", 10, y)
        txtLastName.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtLastName.Size = New Drawing.Size(ctrlWidth, 20)
        Me.Controls.Add(txtLastName)
        y += 30

        AddLabel("Company:", 10, y)
        txtCompanyName.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtCompanyName.Size = New Drawing.Size(300, 20)
        Me.Controls.Add(txtCompanyName)
        y += 30

        ' Contact
        AddLabel("Email:", 10, y)
        txtEmail.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtEmail.Size = New Drawing.Size(250, 20)
        Me.Controls.Add(txtEmail)
        y += 30

        AddLabel("Phone:", 10, y)
        txtPhone.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtPhone.Size = New Drawing.Size(150, 20)
        Me.Controls.Add(txtPhone)
        y += 30

        AddLabel("Mobile:", 10, y)
        txtMobilePhone.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtMobilePhone.Size = New Drawing.Size(150, 20)
        Me.Controls.Add(txtMobilePhone)
        y += 30

        ' Address
        AddLabel("Address:", 10, y)
        txtAddressLine1.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtAddressLine1.Size = New Drawing.Size(350, 20)
        Me.Controls.Add(txtAddressLine1)
        y += 30

        AddLabel("Address 2:", 10, y)
        txtAddressLine2.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtAddressLine2.Size = New Drawing.Size(350, 20)
        Me.Controls.Add(txtAddressLine2)
        y += 30

        AddLabel("City:", 10, y)
        txtCity.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtCity.Size = New Drawing.Size(180, 20)
        Me.Controls.Add(txtCity)

        AddLabel("State:", 320, y)
        cboState.Location = New Drawing.Point(370, y - 3)
        cboState.Size = New Drawing.Size(60, 20)
        cboState.DropDownStyle = ComboBoxStyle.DropDownList
        Me.Controls.Add(cboState)

        AddLabel("Zip:", 440, y)
        txtZipCode.Location = New Drawing.Point(470, y - 3)
        txtZipCode.Size = New Drawing.Size(80, 20)
        Me.Controls.Add(txtZipCode)
        y += 30

        ' Financial
        AddLabel("Credit Score:", 10, y)
        txtCreditScore.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtCreditScore.Size = New Drawing.Size(80, 20)
        Me.Controls.Add(txtCreditScore)
        y += 30

        AddLabel("Annual Income:", 10, y)
        txtAnnualIncome.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtAnnualIncome.Size = New Drawing.Size(120, 20)
        Me.Controls.Add(txtAnnualIncome)
        y += 30

        AddLabel("Occupation:", 10, y)
        txtOccupation.Location = New Drawing.Point(ctrlLeft, y - 3)
        txtOccupation.Size = New Drawing.Size(200, 20)
        Me.Controls.Add(txtOccupation)
        y += 45

        ' Buttons
        btnSave.Location = New Drawing.Point(ctrlLeft, y)
        btnSave.Size = New Drawing.Size(100, 35)
        btnSave.Text = "&Save"
        Me.Controls.Add(btnSave)

        btnCancel.Location = New Drawing.Point(ctrlLeft + 110, y)
        btnCancel.Size = New Drawing.Size(100, 35)
        btnCancel.Text = "&Cancel"
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True}
        Me.Controls.Add(lbl)
    End Sub

    Private Sub LoadLookups()
        cboCustomerType.Items.Clear()
        cboCustomerType.Items.Add(New With {.Text = "Individual", .Value = "I"})
        cboCustomerType.Items.Add(New With {.Text = "Commercial", .Value = "C"})
        cboCustomerType.DisplayMember = "Text"
        cboCustomerType.ValueMember = "Value"
        cboCustomerType.SelectedIndex = 0
    End Sub

    Private Sub LoadData()
        Dim dt As DataTable = CustomerDataAccess.GetByID(_customerID)
        If dt.Rows.Count = 0 Then
            MessageBox.Show("Customer not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close()
            Return
        End If

        Dim row As DataRow = dt.Rows(0)
        lblCustomerNumber.Text = row("CustomerNumber").ToString()
        txtFirstName.Text = row("FirstName").ToString()
        txtLastName.Text = row("LastName").ToString()
        txtCompanyName.Text = row("CompanyName").ToString()
        txtEmail.Text = row("Email").ToString()
        txtPhone.Text = row("Phone").ToString()
        txtMobilePhone.Text = row("MobilePhone").ToString()
        txtAddressLine1.Text = row("AddressLine1").ToString()
        txtAddressLine2.Text = row("AddressLine2").ToString()
        txtCity.Text = row("City").ToString()
        txtZipCode.Text = row("ZipCode").ToString()
        txtOccupation.Text = row("Occupation").ToString()

        If Not IsDBNull(row("CreditScore")) Then txtCreditScore.Text = row("CreditScore").ToString()
        If Not IsDBNull(row("AnnualIncome")) Then txtAnnualIncome.Text = CDec(row("AnnualIncome")).ToString("N2")
    End Sub

    ' --- Save ---
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If Not ValidateForm() Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim dto As New CustomerDTO() With {
                .CustomerID = _customerID,
                .CustomerType = "I",
                .FirstName = txtFirstName.Text.Trim(),
                .LastName = txtLastName.Text.Trim(),
                .CompanyName = txtCompanyName.Text.Trim(),
                .Email = txtEmail.Text.Trim(),
                .Phone = txtPhone.Text.Trim(),
                .MobilePhone = txtMobilePhone.Text.Trim(),
                .AddressLine1 = txtAddressLine1.Text.Trim(),
                .AddressLine2 = txtAddressLine2.Text.Trim(),
                .City = txtCity.Text.Trim(),
                .StateCode = If(cboState.SelectedItem IsNot Nothing, cboState.SelectedItem.ToString(), ""),
                .ZipCode = txtZipCode.Text.Trim(),
                .Occupation = txtOccupation.Text.Trim()
            }

            If Not String.IsNullOrEmpty(txtCreditScore.Text) Then dto.CreditScore = CInt(txtCreditScore.Text)
            If Not String.IsNullOrEmpty(txtAnnualIncome.Text) Then dto.AnnualIncome = CDec(txtAnnualIncome.Text)

            If _isEditMode Then
                CustomerDataAccess.Update(dto)
            Else
                CustomerDataAccess.Create(dto)
                _customerID = dto.CustomerID
                lblCustomerNumber.Text = dto.CustomerNumber
                _isEditMode = True
            End If

            _isDirty = False
            MessageBox.Show("Customer saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error saving: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Function ValidateForm() As Boolean
        If String.IsNullOrWhiteSpace(txtFirstName.Text) AndAlso String.IsNullOrWhiteSpace(txtCompanyName.Text) Then
            MessageBox.Show("First Name or Company Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtFirstName.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtAddressLine1.Text) Then
            MessageBox.Show("Address is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtAddressLine1.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtCity.Text) Then
            MessageBox.Show("City is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtCity.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtZipCode.Text) Then
            MessageBox.Show("Zip Code is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtZipCode.Focus()
            Return False
        End If

        Return True
    End Function

    ' --- Cancel ---
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    ' --- Dirty check on close ---
    Private Sub frmCustomerEntry_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _isDirty Then
            Dim result = MessageBox.Show("You have unsaved changes. Discard?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result = DialogResult.No Then e.Cancel = True
        End If
    End Sub

End Class
