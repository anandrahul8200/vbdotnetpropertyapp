Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Set/change claim reserve dialog.
''' </summary>
Public Class frmClaimReserve
    Inherits Form

    Private _claimID As Integer

    Private cboReserveType As New ComboBox()
    Private cboCategory As New ComboBox()
    Private txtAmount As New TextBox()
    Private txtReason As New TextBox()
    Private lblCurrentReserve As New Label()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmClaimReserve_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Set Reserve"
        Me.Size = New Drawing.Size(450, 320)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 20
        AddLabel("Reserve Type:", 15, y)
        cboReserveType.Location = New Drawing.Point(130, y - 3) : cboReserveType.Size = New Drawing.Size(150, 20) : cboReserveType.DropDownStyle = ComboBoxStyle.DropDownList
        cboReserveType.Items.AddRange({"CASE", "EXPENSE", "IBNR", "BULK"}) : cboReserveType.SelectedIndex = 0
        Me.Controls.Add(cboReserveType)
        y += 35

        AddLabel("Category:", 15, y)
        cboCategory.Location = New Drawing.Point(130, y - 3) : cboCategory.Size = New Drawing.Size(180, 20) : cboCategory.DropDownStyle = ComboBoxStyle.DropDownList
        cboCategory.Items.AddRange({"INDEMNITY", "DEFENSE", "ADJUSTMENT_EXPENSE", "MEDICAL"}) : cboCategory.SelectedIndex = 0
        Me.Controls.Add(cboCategory)
        y += 35

        AddLabel("Amount ($):", 15, y)
        txtAmount.Location = New Drawing.Point(130, y - 3) : txtAmount.Size = New Drawing.Size(120, 20)
        Me.Controls.Add(txtAmount)
        y += 35

        AddLabel("Reason:", 15, y)
        txtReason.Location = New Drawing.Point(130, y - 3) : txtReason.Size = New Drawing.Size(280, 60) : txtReason.Multiline = True
        Me.Controls.Add(txtReason)
        y += 75

        lblCurrentReserve.Location = New Drawing.Point(15, y) : lblCurrentReserve.AutoSize = True : lblCurrentReserve.ForeColor = Drawing.Color.Gray
        Me.Controls.Add(lblCurrentReserve)
        y += 30

        btnSave.Location = New Drawing.Point(130, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Save" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(240, y) : btnCancel.Size = New Drawing.Size(100, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Dim amount As Decimal
            If Not Decimal.TryParse(txtAmount.Text, amount) OrElse amount <= 0 Then
                MessageBox.Show("Please enter a valid positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            ClaimDataAccess.SetReserve(_claimID, cboReserveType.SelectedItem.ToString(), cboCategory.SelectedItem.ToString(), amount, txtReason.Text.Trim())
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error setting reserve: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimReserve.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
