Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Billing inquiry — view invoices, payments, and refunds for a policy.
''' </summary>
Public Class frmBillingInquiry
    Inherits Form

    Private txtPolicyNumber As New TextBox()
    Private WithEvents btnLookup As New Button()
    Private lblPolicySummary As New Label()
    Private lblTotalBilled As New Label()
    Private lblTotalPaid As New Label()
    Private lblOutstanding As New Label()

    Private tabControl As New TabControl()
    Private tabInvoices As New TabPage("Invoices")
    Private tabPayments As New TabPage("Payments")
    Private tabRefunds As New TabPage("Refunds")
    Private dgvInvoices As New DataGridView()
    Private dgvPayments As New DataGridView()
    Private dgvRefunds As New DataGridView()

    Private WithEvents btnRecordPayment As New Button()
    Private WithEvents btnCreateRefund As New Button()

    Private _policyID As Integer = 0

    Private Sub frmBillingInquiry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Billing Inquiry"
        Me.Size = New Drawing.Size(900, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Search
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 80}
        pnlTop.Controls.Add(New Label() With {.Text = "Policy #:", .Location = New Drawing.Point(10, 15), .AutoSize = True})
        txtPolicyNumber.Location = New Drawing.Point(80, 12) : txtPolicyNumber.Size = New Drawing.Size(130, 20) : pnlTop.Controls.Add(txtPolicyNumber)
        btnLookup.Location = New Drawing.Point(220, 9) : btnLookup.Size = New Drawing.Size(80, 26) : btnLookup.Text = "&Lookup" : pnlTop.Controls.Add(btnLookup)
        lblPolicySummary.Location = New Drawing.Point(320, 15) : lblPolicySummary.AutoSize = True : pnlTop.Controls.Add(lblPolicySummary)
        lblTotalBilled.Location = New Drawing.Point(10, 50) : lblTotalBilled.AutoSize = True : pnlTop.Controls.Add(lblTotalBilled)
        lblTotalPaid.Location = New Drawing.Point(200, 50) : lblTotalPaid.AutoSize = True : pnlTop.Controls.Add(lblTotalPaid)
        lblOutstanding.Location = New Drawing.Point(400, 50) : lblOutstanding.AutoSize = True : lblOutstanding.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlTop.Controls.Add(lblOutstanding)
        Me.Controls.Add(pnlTop)

        ' Tabs
        tabControl.Dock = DockStyle.Fill
        ConfigureGrid(dgvInvoices) : tabInvoices.Controls.Add(dgvInvoices)
        ConfigureGrid(dgvPayments) : tabPayments.Controls.Add(dgvPayments)
        ConfigureGrid(dgvRefunds) : tabRefunds.Controls.Add(dgvRefunds)
        tabControl.TabPages.AddRange({tabInvoices, tabPayments, tabRefunds})
        Me.Controls.Add(tabControl)

        ' Bottom
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnRecordPayment.Location = New Drawing.Point(10, 8) : btnRecordPayment.Size = New Drawing.Size(120, 30) : btnRecordPayment.Text = "Record &Payment" : btnRecordPayment.Enabled = False : pnlBottom.Controls.Add(btnRecordPayment)
        btnCreateRefund.Location = New Drawing.Point(140, 8) : btnCreateRefund.Size = New Drawing.Size(110, 30) : btnCreateRefund.Text = "Create &Refund" : btnCreateRefund.Enabled = False : pnlBottom.Controls.Add(btnCreateRefund)
        Me.Controls.Add(pnlBottom)
        tabControl.BringToFront()
    End Sub

    Private Sub ConfigureGrid(dgv As DataGridView)
        dgv.Dock = DockStyle.Fill : dgv.ReadOnly = True : dgv.AllowUserToAddRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    Private Sub btnLookup_Click(sender As Object, e As EventArgs) Handles btnLookup.Click
        Try
            If String.IsNullOrWhiteSpace(txtPolicyNumber.Text) Then Return
            Me.Cursor = Cursors.WaitCursor

            ' Would resolve policy number to ID first
            _policyID = 1 ' Placeholder
            Dim ds As DataSet = BillingDataAccess.GetBillingByPolicy(_policyID)

            If ds.Tables.Count > 0 Then
                Dim summaryRow As DataRow = ds.Tables(0).Rows(0)
                lblPolicySummary.Text = $"Policy: {summaryRow("PolicyNumber")} | Premium: {CDec(summaryRow("GrossPremium")):C}"
                lblTotalBilled.Text = $"Billed: {CDec(summaryRow("TotalBilled")):C}"
                lblTotalPaid.Text = $"Paid: {CDec(summaryRow("TotalPaid")):C}"
                lblOutstanding.Text = $"Outstanding: {CDec(summaryRow("TotalOutstanding")):C}"
            End If
            If ds.Tables.Count > 1 Then dgvInvoices.DataSource = ds.Tables(1)
            If ds.Tables.Count > 2 Then dgvPayments.DataSource = ds.Tables(2)
            If ds.Tables.Count > 3 Then dgvRefunds.DataSource = ds.Tables(3)

            btnRecordPayment.Enabled = True : btnCreateRefund.Enabled = True
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmBillingInquiry.btnLookup_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnRecordPayment_Click(sender As Object, e As EventArgs) Handles btnRecordPayment.Click
        Dim frm As New frmPaymentEntry(_policyID)
        If frm.ShowDialog(Me) = DialogResult.OK Then btnLookup.PerformClick()
    End Sub

    Private Sub btnCreateRefund_Click(sender As Object, e As EventArgs) Handles btnCreateRefund.Click
        MessageBox.Show("Refund creation dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
