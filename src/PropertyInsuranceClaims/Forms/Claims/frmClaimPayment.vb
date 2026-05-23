Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Create claim payment dialog.
''' </summary>
Public Class frmClaimPayment
    Inherits Form

    Private _claimID As Integer

    Private cboPaymentType As New ComboBox()
    Private cboPaymentMethod As New ComboBox()
    Private cboPayeeType As New ComboBox()
    Private txtPayeeName As New TextBox()
    Private txtPayeeAddress As New TextBox()
    Private txtAmount As New TextBox()
    Private txtDescription As New TextBox()
    Private txtInvoiceNumber As New TextBox()
    Private chkTaxReportable As New CheckBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmClaimPayment_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Create Claim Payment"
        Me.Size = New Drawing.Size(500, 420)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 15
        AddLabel("Payment Type:", 15, y)
        cboPaymentType.Location = New Drawing.Point(130, y - 3) : cboPaymentType.Size = New Drawing.Size(150, 20) : cboPaymentType.DropDownStyle = ComboBoxStyle.DropDownList
        cboPaymentType.Items.AddRange({"INDEMNITY", "EXPENSE", "PARTIAL", "FINAL", "SUPPLEMENT"}) : cboPaymentType.SelectedIndex = 0
        Me.Controls.Add(cboPaymentType)
        y += 32

        AddLabel("Method:", 15, y)
        cboPaymentMethod.Location = New Drawing.Point(130, y - 3) : cboPaymentMethod.Size = New Drawing.Size(120, 20) : cboPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboPaymentMethod.Items.AddRange({"CHECK", "EFT", "WIRE", "DRAFT"}) : cboPaymentMethod.SelectedIndex = 0
        Me.Controls.Add(cboPaymentMethod)
        y += 32

        AddLabel("Payee Type:", 15, y)
        cboPayeeType.Location = New Drawing.Point(130, y - 3) : cboPayeeType.Size = New Drawing.Size(150, 20) : cboPayeeType.DropDownStyle = ComboBoxStyle.DropDownList
        cboPayeeType.Items.AddRange({"INSURED", "VENDOR", "ATTORNEY", "MORTGAGEE", "LIENHOLDER"}) : cboPayeeType.SelectedIndex = 0
        Me.Controls.Add(cboPayeeType)
        y += 32

        AddLabel("Payee Name:", 15, y)
        txtPayeeName.Location = New Drawing.Point(130, y - 3) : txtPayeeName.Size = New Drawing.Size(300, 20)
        Me.Controls.Add(txtPayeeName)
        y += 32

        AddLabel("Address:", 15, y)
        txtPayeeAddress.Location = New Drawing.Point(130, y - 3) : txtPayeeAddress.Size = New Drawing.Size(300, 20)
        Me.Controls.Add(txtPayeeAddress)
        y += 32

        AddLabel("Amount ($):", 15, y)
        txtAmount.Location = New Drawing.Point(130, y - 3) : txtAmount.Size = New Drawing.Size(120, 20)
        Me.Controls.Add(txtAmount)
        y += 32

        AddLabel("Description:", 15, y)
        txtDescription.Location = New Drawing.Point(130, y - 3) : txtDescription.Size = New Drawing.Size(300, 40) : txtDescription.Multiline = True
        Me.Controls.Add(txtDescription)
        y += 50

        AddLabel("Invoice #:", 15, y)
        txtInvoiceNumber.Location = New Drawing.Point(130, y - 3) : txtInvoiceNumber.Size = New Drawing.Size(120, 20)
        Me.Controls.Add(txtInvoiceNumber)
        chkTaxReportable.Location = New Drawing.Point(280, y - 2) : chkTaxReportable.Text = "Tax Reportable (1099)" : chkTaxReportable.AutoSize = True
        Me.Controls.Add(chkTaxReportable)
        y += 40

        btnSave.Location = New Drawing.Point(130, y) : btnSave.Size = New Drawing.Size(120, 35) : btnSave.Text = "&Create Payment" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(260, y) : btnCancel.Size = New Drawing.Size(100, 35) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If String.IsNullOrWhiteSpace(txtPayeeName.Text) Then
                MessageBox.Show("Payee name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            Dim amount As Decimal
            If Not Decimal.TryParse(txtAmount.Text, amount) OrElse amount <= 0 Then
                MessageBox.Show("Please enter a valid positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            Dim dto As New ClaimPaymentDTO() With {
                .ClaimID = _claimID,
                .PaymentType = cboPaymentType.SelectedItem.ToString(),
                .PaymentMethod = cboPaymentMethod.SelectedItem.ToString(),
                .PayeeType = cboPayeeType.SelectedItem.ToString(),
                .PayeeName = txtPayeeName.Text.Trim(),
                .PayeeAddress = txtPayeeAddress.Text.Trim(),
                .Amount = amount,
                .Description = txtDescription.Text.Trim(),
                .InvoiceNumber = txtInvoiceNumber.Text.Trim(),
                .TaxReportable = chkTaxReportable.Checked
            }

            ClaimDataAccess.CreatePayment(dto)
            MessageBox.Show($"Payment {dto.PaymentNumber} created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error creating payment: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimPayment.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
