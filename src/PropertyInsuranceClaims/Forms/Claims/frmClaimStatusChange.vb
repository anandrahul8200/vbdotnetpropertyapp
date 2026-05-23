Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Change claim status dialog with transition validation.
''' </summary>
Public Class frmClaimStatusChange
    Inherits Form

    Private _claimID As Integer
    Private _currentStatus As String

    Private lblCurrentStatus As New Label()
    Private cboNewStatus As New ComboBox()
    Private txtReason As New TextBox()
    Private txtDenialReason As New TextBox()
    Private lblDenialReason As New Label()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(claimID As Integer, currentStatus As String)
        _claimID = claimID
        _currentStatus = currentStatus
    End Sub

    Private Sub frmClaimStatusChange_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Change Claim Status"
        Me.Size = New Drawing.Size(450, 320)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 15
        AddLabel("Current Status:", 15, y)
        lblCurrentStatus.Location = New Drawing.Point(130, y) : lblCurrentStatus.AutoSize = True
        lblCurrentStatus.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)
        lblCurrentStatus.Text = _currentStatus
        Me.Controls.Add(lblCurrentStatus)
        y += 35

        AddLabel("New Status:", 15, y)
        cboNewStatus.Location = New Drawing.Point(130, y - 3) : cboNewStatus.Size = New Drawing.Size(180, 20) : cboNewStatus.DropDownStyle = ComboBoxStyle.DropDownList
        LoadAllowedTransitions()
        Me.Controls.Add(cboNewStatus)
        y += 35

        AddLabel("Reason:", 15, y)
        txtReason.Location = New Drawing.Point(130, y - 3) : txtReason.Size = New Drawing.Size(280, 60) : txtReason.Multiline = True
        Me.Controls.Add(txtReason)
        y += 70

        lblDenialReason.Text = "Denial Reason:" : lblDenialReason.Location = New Drawing.Point(15, y) : lblDenialReason.AutoSize = True : lblDenialReason.Visible = False
        Me.Controls.Add(lblDenialReason)
        txtDenialReason.Location = New Drawing.Point(130, y - 3) : txtDenialReason.Size = New Drawing.Size(280, 40) : txtDenialReason.Multiline = True : txtDenialReason.Visible = False
        Me.Controls.Add(txtDenialReason)
        y += 55

        btnSave.Location = New Drawing.Point(130, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Apply" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(240, y) : btnCancel.Size = New Drawing.Size(100, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)

        AddHandler cboNewStatus.SelectedIndexChanged, Sub()
                                                          Dim showDenial As Boolean = cboNewStatus.SelectedItem IsNot Nothing AndAlso cboNewStatus.SelectedItem.ToString() = "DENIED"
                                                          lblDenialReason.Visible = showDenial : txtDenialReason.Visible = showDenial
                                                      End Sub
    End Sub

    Private Sub LoadAllowedTransitions()
        Dim allowed As String() = {}
        Select Case _currentStatus
            Case "FNOL" : allowed = {"ASSIGNED", "DENIED", "CLOSED"}
            Case "ASSIGNED" : allowed = {"INVESTIGATING", "DENIED", "CLOSED"}
            Case "INVESTIGATING" : allowed = {"ASSESSED", "DENIED", "CLOSED", "LITIGATION"}
            Case "ASSESSED" : allowed = {"APPROVED", "DENIED", "CLOSED"}
            Case "APPROVED" : allowed = {"SETTLED", "CLOSED"}
            Case "DENIED" : allowed = {"REOPENED", "CLOSED"}
            Case "SETTLED" : allowed = {"CLOSED", "REOPENED"}
            Case "CLOSED" : allowed = {"REOPENED"}
            Case "REOPENED" : allowed = {"INVESTIGATING", "ASSIGNED"}
            Case "LITIGATION" : allowed = {"SETTLED", "CLOSED", "DENIED"}
        End Select
        cboNewStatus.Items.AddRange(allowed)
        If cboNewStatus.Items.Count > 0 Then cboNewStatus.SelectedIndex = 0
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If cboNewStatus.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a new status.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim newStatus As String = cboNewStatus.SelectedItem.ToString()
            If newStatus = "DENIED" AndAlso String.IsNullOrWhiteSpace(txtDenialReason.Text) Then
                MessageBox.Show("Denial reason is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            ClaimDataAccess.UpdateStatus(_claimID, newStatus, txtReason.Text.Trim(), If(newStatus = "DENIED", txtDenialReason.Text.Trim(), Nothing))
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimStatusChange.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
