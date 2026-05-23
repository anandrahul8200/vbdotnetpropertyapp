Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Record premium payment dialog.
''' </summary>
Public Class frmPaymentEntry
    Inherits Form

    Private _policyID As Integer

    Private cboPaymentMethod As New ComboBox()
    Private txtAmount As New TextBox()
    Private dtpPaymentDate As New DateTimePicker()
    Private txtReferenceNumber As New TextBox()
    Private txtCheckNumber As New TextBox()
    Private txtBankName As New TextBox()
    Private txtCreditCardLast4 As New TextBox()
    Private lblMethodFields As New Label()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmPaymentEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Record Payment"
        Me.Size = New Drawing.Size(450, 350)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 15
        AddLabel("Amount ($):", 15, y)
        txtAmount.Location = New Drawing.Point(130, y - 3) : txtAmount.Size = New Drawing.Size(120, 20) : Me.Controls.Add(txtAmount)
        y += 32

        AddLabel("Payment Date:", 15, y)
        dtpPaymentDate.Location = New Drawing.Point(130, y - 3) : dtpPaymentDate.Size = New Drawing.Size(130, 20) : dtpPaymentDate.Format = DateTimePickerFormat.Short : Me.Controls.Add(dtpPaymentDate)
        y += 32

        AddLabel("Method:", 15, y)
        cboPaymentMethod.Location = New Drawing.Point(130, y - 3) : cboPaymentMethod.Size = New Drawing.Size(130, 20) : cboPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboPaymentMethod.Items.AddRange({"CHECK", "CREDIT_CARD", "EFT", "CASH", "WIRE"}) : cboPaymentMethod.SelectedIndex = 0
        AddHandler cboPaymentMethod.SelectedIndexChanged, AddressOf MethodChanged
        Me.Controls.Add(cboPaymentMethod)
        y += 32

        AddLabel("Reference #:", 15, y)
        txtReferenceNumber.Location = New Drawing.Point(130, y - 3) : txtReferenceNumber.Size = New Drawing.Size(150, 20) : Me.Controls.Add(txtReferenceNumber)
        y += 32

        AddLabel("Check #:", 15, y)
        txtCheckNumber.Location = New Drawing.Point(130, y - 3) : txtCheckNumber.Size = New Drawing.Size(100, 20) : Me.Controls.Add(txtCheckNumber)
        y += 32

        AddLabel("Bank:", 15, y)
        txtBankName.Location = New Drawing.Point(130, y - 3) : txtBankName.Size = New Drawing.Size(200, 20) : Me.Controls.Add(txtBankName)
        y += 32

        AddLabel("Card Last 4:", 15, y)
        txtCreditCardLast4.Location = New Drawing.Point(130, y - 3) : txtCreditCardLast4.Size = New Drawing.Size(50, 20) : txtCreditCardLast4.MaxLength = 4 : txtCreditCardLast4.Visible = False : Me.Controls.Add(txtCreditCardLast4)
        y += 40

        btnSave.Location = New Drawing.Point(130, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Record" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(240, y) : btnCancel.Size = New Drawing.Size(100, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub MethodChanged(sender As Object, e As EventArgs)
        Dim method As String = If(cboPaymentMethod.SelectedItem IsNot Nothing, cboPaymentMethod.SelectedItem.ToString(), "")
        txtCheckNumber.Visible = (method = "CHECK")
        txtBankName.Visible = (method = "CHECK" OrElse method = "EFT")
        txtCreditCardLast4.Visible = (method = "CREDIT_CARD")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Dim amount As Decimal
            If Not Decimal.TryParse(txtAmount.Text, amount) OrElse amount <= 0 Then
                MessageBox.Show("Enter a valid positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            BillingDataAccess.RecordPayment(_policyID, amount, cboPaymentMethod.SelectedItem.ToString(),
                                           Nothing, txtReferenceNumber.Text.Trim(), txtCheckNumber.Text.Trim())
            MessageBox.Show("Payment recorded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPaymentEntry.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
