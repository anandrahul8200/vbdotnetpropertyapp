Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Policy reinstatement — reinstate a cancelled policy with conditions.
''' </summary>
Public Class frmPolicyReinstatement
    Inherits Form

    Private _policyID As Integer
    Private lblPolicyInfo As New Label()
    Private lblCancelDate As New Label()
    Private lblCancelReason As New Label()
    Private dtpReinstatementDate As New DateTimePicker()
    Private chkBackdateEffective As New CheckBox()
    Private chkRequirePayment As New CheckBox()
    Private txtPaymentAmount As New TextBox()
    Private txtConditions As New TextBox()
    Private WithEvents btnReinstate As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmPolicyReinstatement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Reinstate Policy"
        Me.Size = New Drawing.Size(500, 380)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        lblPolicyInfo.Location = New Drawing.Point(15, y) : lblPolicyInfo.AutoSize = True : lblPolicyInfo.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblPolicyInfo.Text = "Policy ID: " & _policyID.ToString() : Me.Controls.Add(lblPolicyInfo)
        y += 22
        lblCancelDate.Location = New Drawing.Point(15, y) : lblCancelDate.AutoSize = True : lblCancelDate.Text = "Cancelled: 01/15/2026" : Me.Controls.Add(lblCancelDate)
        lblCancelReason.Location = New Drawing.Point(200, y) : lblCancelReason.AutoSize = True : lblCancelReason.Text = "Reason: NON_PAYMENT" : Me.Controls.Add(lblCancelReason)
        y += 30
        Me.Controls.Add(New Label() With {.Text = "Reinstatement Date:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        dtpReinstatementDate.Location = New Drawing.Point(150, y - 3) : dtpReinstatementDate.Size = New Drawing.Size(130, 20) : dtpReinstatementDate.Format = DateTimePickerFormat.Short : Me.Controls.Add(dtpReinstatementDate)
        y += 32
        chkBackdateEffective.Location = New Drawing.Point(15, y) : chkBackdateEffective.Text = "Backdate to original cancellation date (no lapse)" : chkBackdateEffective.AutoSize = True : Me.Controls.Add(chkBackdateEffective)
        y += 28
        chkRequirePayment.Location = New Drawing.Point(15, y) : chkRequirePayment.Text = "Require payment before reinstatement" : chkRequirePayment.Checked = True : chkRequirePayment.AutoSize = True : Me.Controls.Add(chkRequirePayment)
        y += 28
        Me.Controls.Add(New Label() With {.Text = "Amount Due:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtPaymentAmount.Location = New Drawing.Point(150, y - 3) : txtPaymentAmount.Size = New Drawing.Size(100, 20) : txtPaymentAmount.Text = "450.00" : Me.Controls.Add(txtPaymentAmount)
        y += 32
        Me.Controls.Add(New Label() With {.Text = "Conditions:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtConditions.Location = New Drawing.Point(150, y - 3) : txtConditions.Size = New Drawing.Size(300, 60) : txtConditions.Multiline = True : Me.Controls.Add(txtConditions)
        y += 75
        btnReinstate.Location = New Drawing.Point(150, y) : btnReinstate.Size = New Drawing.Size(100, 30) : btnReinstate.Text = "&Reinstate" : btnReinstate.BackColor = Drawing.Color.LightGreen : Me.Controls.Add(btnReinstate)
        btnCancel.Location = New Drawing.Point(260, y) : btnCancel.Size = New Drawing.Size(80, 30) : btnCancel.Text = "Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub btnReinstate_Click(sender As Object, e As EventArgs) Handles btnReinstate.Click
        If MessageBox.Show("Reinstate this policy?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Policy reinstated successfully.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
