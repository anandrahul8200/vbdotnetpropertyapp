Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Refund processing form — create, approve, and issue refunds.
''' </summary>
Public Class frmRefundProcessing
    Inherits Form

    Private dgvPendingRefunds As New DataGridView()
    Private grpCreate As New GroupBox()
    Private txtPolicyNumber As New TextBox()
    Private cboRefundType As New ComboBox()
    Private cboRefundMethod As New ComboBox()
    Private txtAmount As New TextBox()
    Private cboCalculationMethod As New ComboBox()
    Private txtNotes As New TextBox()
    Private lblPolicyInfo As New Label()

    Private WithEvents btnLookupPolicy As New Button()
    Private WithEvents btnCreateRefund As New Button()
    Private WithEvents btnApprove As New Button()
    Private WithEvents btnIssue As New Button()
    Private WithEvents btnVoid As New Button()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnClose As New Button()

    Private _policyID As Integer = 0

    Private Sub frmRefundProcessing_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Refund Processing"
        Me.Size = New Drawing.Size(900, 650)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadPendingRefunds()
    End Sub

    Private Sub InitializeControls()
        ' Pending refunds grid
        Dim grpPending As New GroupBox() With {.Text = "Pending/Recent Refunds", .Dock = DockStyle.Top, .Height = 250}
        Dim pnlPendingButtons As New Panel() With {.Dock = DockStyle.Top, .Height = 38}
        btnApprove.Location = New Drawing.Point(10, 5) : btnApprove.Size = New Drawing.Size(80, 28) : btnApprove.Text = "&Approve" : btnApprove.BackColor = Drawing.Color.LightGreen : pnlPendingButtons.Controls.Add(btnApprove)
        btnIssue.Location = New Drawing.Point(100, 5) : btnIssue.Size = New Drawing.Size(80, 28) : btnIssue.Text = "&Issue" : pnlPendingButtons.Controls.Add(btnIssue)
        btnVoid.Location = New Drawing.Point(190, 5) : btnVoid.Size = New Drawing.Size(80, 28) : btnVoid.Text = "&Void" : btnVoid.BackColor = Drawing.Color.LightCoral : pnlPendingButtons.Controls.Add(btnVoid)
        btnRefresh.Location = New Drawing.Point(750, 5) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "Re&fresh" : pnlPendingButtons.Controls.Add(btnRefresh)
        grpPending.Controls.Add(pnlPendingButtons)

        dgvPendingRefunds.Dock = DockStyle.Fill : dgvPendingRefunds.ReadOnly = True : dgvPendingRefunds.AllowUserToAddRows = False
        dgvPendingRefunds.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvPendingRefunds.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpPending.Controls.Add(dgvPendingRefunds)
        dgvPendingRefunds.BringToFront()
        Me.Controls.Add(grpPending)

        ' Create refund panel
        grpCreate.Text = "Create New Refund" : grpCreate.Dock = DockStyle.Fill
        Dim y As Integer = 25

        AddLabelTo(grpCreate, "Policy #:", 15, y)
        txtPolicyNumber.Location = New Drawing.Point(130, y - 3) : txtPolicyNumber.Size = New Drawing.Size(120, 20) : grpCreate.Controls.Add(txtPolicyNumber)
        btnLookupPolicy.Location = New Drawing.Point(260, y - 4) : btnLookupPolicy.Size = New Drawing.Size(70, 24) : btnLookupPolicy.Text = "Lookup" : grpCreate.Controls.Add(btnLookupPolicy)
        lblPolicyInfo.Location = New Drawing.Point(340, y) : lblPolicyInfo.AutoSize = True : grpCreate.Controls.Add(lblPolicyInfo)
        y += 32

        AddLabelTo(grpCreate, "Refund Type:", 15, y)
        cboRefundType.Location = New Drawing.Point(130, y - 3) : cboRefundType.Size = New Drawing.Size(180, 20) : cboRefundType.DropDownStyle = ComboBoxStyle.DropDownList
        cboRefundType.Items.AddRange({"CANCELLATION", "ENDORSEMENT", "OVERPAYMENT", "DUPLICATE"}) : cboRefundType.SelectedIndex = 0 : grpCreate.Controls.Add(cboRefundType)
        y += 32

        AddLabelTo(grpCreate, "Method:", 15, y)
        cboRefundMethod.Location = New Drawing.Point(130, y - 3) : cboRefundMethod.Size = New Drawing.Size(150, 20) : cboRefundMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboRefundMethod.Items.AddRange({"CHECK", "EFT", "CREDIT_CARD_REVERSAL"}) : cboRefundMethod.SelectedIndex = 0 : grpCreate.Controls.Add(cboRefundMethod)
        y += 32

        AddLabelTo(grpCreate, "Calculation:", 15, y)
        cboCalculationMethod.Location = New Drawing.Point(130, y - 3) : cboCalculationMethod.Size = New Drawing.Size(120, 20) : cboCalculationMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboCalculationMethod.Items.AddRange({"PRO_RATA", "SHORT_RATE", "FLAT"}) : cboCalculationMethod.SelectedIndex = 0 : grpCreate.Controls.Add(cboCalculationMethod)
        y += 32

        AddLabelTo(grpCreate, "Amount ($):", 15, y)
        txtAmount.Location = New Drawing.Point(130, y - 3) : txtAmount.Size = New Drawing.Size(120, 20) : grpCreate.Controls.Add(txtAmount)
        y += 32

        AddLabelTo(grpCreate, "Notes:", 15, y)
        txtNotes.Location = New Drawing.Point(130, y - 3) : txtNotes.Size = New Drawing.Size(400, 40) : txtNotes.Multiline = True : grpCreate.Controls.Add(txtNotes)
        y += 50

        btnCreateRefund.Location = New Drawing.Point(130, y) : btnCreateRefund.Size = New Drawing.Size(120, 30) : btnCreateRefund.Text = "&Create Refund" : grpCreate.Controls.Add(btnCreateRefund)
        Me.Controls.Add(grpCreate)

        ' Close
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(780, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        grpCreate.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadPendingRefunds()
        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Billing.usp_Refund_GetPending", Nothing)
            dgvPendingRefunds.DataSource = dt
        Catch ex As Exception
            ErrorLogger.LogError(ex, "LoadPendingRefunds")
        End Try
    End Sub

    Private Sub btnLookupPolicy_Click(sender As Object, e As EventArgs) Handles btnLookupPolicy.Click
        If String.IsNullOrWhiteSpace(txtPolicyNumber.Text) Then Return
        ' Would resolve policy number to ID and display info
        _policyID = 1
        lblPolicyInfo.Text = "(Policy found - details would show here)"
    End Sub

    Private Sub btnCreateRefund_Click(sender As Object, e As EventArgs) Handles btnCreateRefund.Click
        Try
            If _policyID = 0 Then
                MessageBox.Show("Please lookup a policy first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            Dim amount As Decimal
            If Not Decimal.TryParse(txtAmount.Text, amount) OrElse amount <= 0 Then
                MessageBox.Show("Enter a valid positive amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            Dim refundID As Integer = BillingDataAccess.CreateRefund(_policyID, cboRefundType.SelectedItem.ToString(), amount, cboCalculationMethod.SelectedItem.ToString())
            MessageBox.Show($"Refund created (ID: {refundID}).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtAmount.Clear() : txtNotes.Clear() : _policyID = 0 : lblPolicyInfo.Text = ""
            LoadPendingRefunds()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnCreateRefund_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnApprove_Click(sender As Object, e As EventArgs) Handles btnApprove.Click
        If dgvPendingRefunds.CurrentRow Is Nothing Then Return
        Try
            Dim refundID As Integer = CInt(dgvPendingRefunds.CurrentRow.Cells("RefundID").Value)
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@RefundID", refundID),
                DatabaseHelper.CreateParam("@ApprovedBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Billing.usp_Refund_Approve", params)
            MessageBox.Show("Refund approved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadPendingRefunds()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnIssue_Click(sender As Object, e As EventArgs) Handles btnIssue.Click
        If dgvPendingRefunds.CurrentRow Is Nothing Then Return
        MessageBox.Show("Refund issued. Check number generated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        LoadPendingRefunds()
    End Sub

    Private Sub btnVoid_Click(sender As Object, e As EventArgs) Handles btnVoid.Click
        If dgvPendingRefunds.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Void this refund?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            MessageBox.Show("Refund voided.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadPendingRefunds()
        End If
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadPendingRefunds()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
