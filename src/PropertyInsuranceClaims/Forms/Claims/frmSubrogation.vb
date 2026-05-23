Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Subrogation management form — create, track, and record recoveries.
''' </summary>
Public Class frmSubrogation
    Inherits Form

    Private _claimID As Integer
    Private dgvSubrogation As New DataGridView()
    Private WithEvents btnCreate As New Button()
    Private WithEvents btnUpdateStatus As New Button()
    Private WithEvents btnRecordRecovery As New Button()
    Private WithEvents btnClose As New Button()

    ' Create panel
    Private grpCreate As New GroupBox()
    Private txtResponsibleParty As New TextBox()
    Private txtInsurer As New TextBox()
    Private txtPolicyNumber As New TextBox()
    Private txtDemandAmount As New TextBox()
    Private txtNotes As New TextBox()
    Private WithEvents btnSaveNew As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmSubrogation_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Subrogation Management"
        Me.Size = New Drawing.Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadSubrogation()
    End Sub

    Private Sub InitializeControls()
        ' Toolbar
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        btnCreate.Location = New Drawing.Point(10, 8) : btnCreate.Size = New Drawing.Size(120, 30) : btnCreate.Text = "&New Subrogation" : pnlTop.Controls.Add(btnCreate)
        btnUpdateStatus.Location = New Drawing.Point(140, 8) : btnUpdateStatus.Size = New Drawing.Size(110, 30) : btnUpdateStatus.Text = "Update &Status" : pnlTop.Controls.Add(btnUpdateStatus)
        btnRecordRecovery.Location = New Drawing.Point(260, 8) : btnRecordRecovery.Size = New Drawing.Size(120, 30) : btnRecordRecovery.Text = "Record &Recovery" : pnlTop.Controls.Add(btnRecordRecovery)
        btnClose.Location = New Drawing.Point(660, 8) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlTop.Controls.Add(btnClose)
        Me.Controls.Add(pnlTop)

        ' Grid
        dgvSubrogation.Dock = DockStyle.Top : dgvSubrogation.Height = 200
        dgvSubrogation.ReadOnly = True : dgvSubrogation.AllowUserToAddRows = False
        dgvSubrogation.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvSubrogation.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvSubrogation)

        ' Create panel
        grpCreate.Text = "New Subrogation" : grpCreate.Dock = DockStyle.Fill
        Dim y As Integer = 25
        AddLabelTo(grpCreate, "Responsible Party:", 10, y)
        txtResponsibleParty.Location = New Drawing.Point(150, y - 3) : txtResponsibleParty.Size = New Drawing.Size(250, 20) : grpCreate.Controls.Add(txtResponsibleParty)
        y += 30
        AddLabelTo(grpCreate, "Their Insurer:", 10, y)
        txtInsurer.Location = New Drawing.Point(150, y - 3) : txtInsurer.Size = New Drawing.Size(250, 20) : grpCreate.Controls.Add(txtInsurer)
        y += 30
        AddLabelTo(grpCreate, "Their Policy #:", 10, y)
        txtPolicyNumber.Location = New Drawing.Point(150, y - 3) : txtPolicyNumber.Size = New Drawing.Size(150, 20) : grpCreate.Controls.Add(txtPolicyNumber)
        y += 30
        AddLabelTo(grpCreate, "Demand Amount:", 10, y)
        txtDemandAmount.Location = New Drawing.Point(150, y - 3) : txtDemandAmount.Size = New Drawing.Size(120, 20) : grpCreate.Controls.Add(txtDemandAmount)
        y += 30
        AddLabelTo(grpCreate, "Notes:", 10, y)
        txtNotes.Location = New Drawing.Point(150, y - 3) : txtNotes.Size = New Drawing.Size(400, 60) : txtNotes.Multiline = True : grpCreate.Controls.Add(txtNotes)
        y += 70
        btnSaveNew.Location = New Drawing.Point(150, y) : btnSaveNew.Size = New Drawing.Size(100, 30) : btnSaveNew.Text = "&Save" : grpCreate.Controls.Add(btnSaveNew)
        Me.Controls.Add(grpCreate)
        grpCreate.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadSubrogation()
        Try
            ' Would call a GetSubrogation SP
            Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@ClaimID", _claimID)}
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Claims.usp_Subrogation_GetByClaim", params)
            dgvSubrogation.DataSource = dt
        Catch ex As Exception
            ErrorLogger.LogError(ex, "LoadSubrogation")
        End Try
    End Sub

    Private Sub btnSaveNew_Click(sender As Object, e As EventArgs) Handles btnSaveNew.Click
        Try
            If String.IsNullOrWhiteSpace(txtResponsibleParty.Text) Then
                MessageBox.Show("Responsible party is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@ClaimID", _claimID),
                DatabaseHelper.CreateParam("@ResponsibleParty", txtResponsibleParty.Text.Trim()),
                DatabaseHelper.CreateParam("@ResponsiblePartyInsurer", If(String.IsNullOrWhiteSpace(txtInsurer.Text), Nothing, txtInsurer.Text.Trim())),
                DatabaseHelper.CreateParam("@ResponsiblePartyPolicy", If(String.IsNullOrWhiteSpace(txtPolicyNumber.Text), Nothing, txtPolicyNumber.Text.Trim())),
                DatabaseHelper.CreateParam("@DemandAmount", If(String.IsNullOrWhiteSpace(txtDemandAmount.Text), Nothing, CDec(txtDemandAmount.Text))),
                DatabaseHelper.CreateParam("@Notes", If(String.IsNullOrWhiteSpace(txtNotes.Text), Nothing, txtNotes.Text.Trim())),
                DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
            }
            Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@SubrogationID", SqlDbType.Int)
            params.Add(idParam)

            DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_Create", params.ToArray())
            MessageBox.Show("Subrogation record created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtResponsibleParty.Clear() : txtInsurer.Clear() : txtPolicyNumber.Clear() : txtDemandAmount.Clear() : txtNotes.Clear()
            LoadSubrogation()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmSubrogation.btnSaveNew_Click")
        End Try
    End Sub

    Private Sub btnCreate_Click(sender As Object, e As EventArgs) Handles btnCreate.Click
        grpCreate.Visible = True : txtResponsibleParty.Focus()
    End Sub

    Private Sub btnUpdateStatus_Click(sender As Object, e As EventArgs) Handles btnUpdateStatus.Click
        If dgvSubrogation.CurrentRow Is Nothing Then Return
        Dim status As String = InputBox("Enter new status (DEMAND_SENT, NEGOTIATING, ARBITRATION, SETTLED, CLOSED, ABANDONED):", "Update Status")
        If String.IsNullOrWhiteSpace(status) Then Return

        Try
            Dim subID As Integer = CInt(dgvSubrogation.CurrentRow.Cells("SubrogationID").Value)
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@SubrogationID", subID),
                DatabaseHelper.CreateParam("@NewStatus", status.Trim().ToUpper()),
                DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_UpdateStatus", params)
            LoadSubrogation()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnRecordRecovery_Click(sender As Object, e As EventArgs) Handles btnRecordRecovery.Click
        If dgvSubrogation.CurrentRow Is Nothing Then Return
        Dim amountStr As String = InputBox("Enter recovery amount:", "Record Recovery")
        If String.IsNullOrWhiteSpace(amountStr) Then Return

        Dim amount As Decimal
        If Not Decimal.TryParse(amountStr, amount) OrElse amount <= 0 Then
            MessageBox.Show("Invalid amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If

        Try
            Dim subID As Integer = CInt(dgvSubrogation.CurrentRow.Cells("SubrogationID").Value)
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@SubrogationID", subID),
                DatabaseHelper.CreateParam("@RecoveryAmount", amount),
                DatabaseHelper.CreateParam("@RecordedBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Claims.usp_Subrogation_RecordRecovery", params)
            MessageBox.Show($"Recovery of {amount:C} recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadSubrogation()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
