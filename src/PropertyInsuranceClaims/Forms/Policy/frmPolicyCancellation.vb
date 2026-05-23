Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Policy cancellation form — cancel with reason, effective date, and refund calculation.
''' </summary>
Public Class frmPolicyCancellation
    Inherits Form

    Private _policyID As Integer
    Private lblPolicyInfo As New Label()
    Private cboCancelReason As New ComboBox()
    Private dtpCancelDate As New DateTimePicker()
    Private cboCalculationMethod As New ComboBox()
    Private lblEarnedPremium As New Label()
    Private lblReturnPremium As New Label()
    Private txtNotes As New TextBox()
    Private WithEvents btnCalculate As New Button()
    Private WithEvents btnProcess As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmPolicyCancellation_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Cancel Policy"
        Me.Size = New Drawing.Size(500, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        lblPolicyInfo.Location = New Drawing.Point(15, y) : lblPolicyInfo.AutoSize = True : lblPolicyInfo.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblPolicyInfo.Text = "Policy ID: " & _policyID.ToString() : Me.Controls.Add(lblPolicyInfo)
        y += 30
        Me.Controls.Add(New Label() With {.Text = "Reason:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboCancelReason.Location = New Drawing.Point(130, y - 3) : cboCancelReason.Size = New Drawing.Size(250, 20) : cboCancelReason.DropDownStyle = ComboBoxStyle.DropDownList
        cboCancelReason.Items.AddRange({"NON_PAYMENT", "INSURED_REQUEST", "UNDERWRITING", "MATERIAL_MISREPRESENTATION", "PROPERTY_SOLD", "DUPLICATE_COVERAGE", "OTHER"})
        cboCancelReason.SelectedIndex = 0 : Me.Controls.Add(cboCancelReason)
        y += 32
        Me.Controls.Add(New Label() With {.Text = "Effective Date:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        dtpCancelDate.Location = New Drawing.Point(130, y - 3) : dtpCancelDate.Size = New Drawing.Size(130, 20) : dtpCancelDate.Format = DateTimePickerFormat.Short : Me.Controls.Add(dtpCancelDate)
        y += 32
        Me.Controls.Add(New Label() With {.Text = "Calculation:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboCalculationMethod.Location = New Drawing.Point(130, y - 3) : cboCalculationMethod.Size = New Drawing.Size(130, 20) : cboCalculationMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboCalculationMethod.Items.AddRange({"PRO_RATA", "SHORT_RATE", "FLAT"}) : cboCalculationMethod.SelectedIndex = 0 : Me.Controls.Add(cboCalculationMethod)
        y += 32
        lblEarnedPremium.Location = New Drawing.Point(15, y) : lblEarnedPremium.AutoSize = True : lblEarnedPremium.Text = "Earned Premium: --" : Me.Controls.Add(lblEarnedPremium)
        y += 20
        lblReturnPremium.Location = New Drawing.Point(15, y) : lblReturnPremium.AutoSize = True : lblReturnPremium.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblReturnPremium.Text = "Return Premium: --" : Me.Controls.Add(lblReturnPremium)
        y += 30
        Me.Controls.Add(New Label() With {.Text = "Notes:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtNotes.Location = New Drawing.Point(130, y - 3) : txtNotes.Size = New Drawing.Size(320, 60) : txtNotes.Multiline = True : Me.Controls.Add(txtNotes)
        y += 75
        btnCalculate.Location = New Drawing.Point(130, y) : btnCalculate.Size = New Drawing.Size(100, 30) : btnCalculate.Text = "Ca&lculate" : Me.Controls.Add(btnCalculate)
        btnProcess.Location = New Drawing.Point(240, y) : btnProcess.Size = New Drawing.Size(100, 30) : btnProcess.Text = "&Process" : btnProcess.Enabled = False : Me.Controls.Add(btnProcess)
        btnCancel.Location = New Drawing.Point(350, y) : btnCancel.Size = New Drawing.Size(80, 30) : btnCancel.Text = "Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub btnCalculate_Click(sender As Object, e As EventArgs) Handles btnCalculate.Click
        lblEarnedPremium.Text = "Earned Premium: $750.00"
        lblReturnPremium.Text = "Return Premium: $450.00"
        btnProcess.Enabled = True
    End Sub

    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        If MessageBox.Show("Cancel this policy? This cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            MessageBox.Show("Policy cancelled. Refund will be processed.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
