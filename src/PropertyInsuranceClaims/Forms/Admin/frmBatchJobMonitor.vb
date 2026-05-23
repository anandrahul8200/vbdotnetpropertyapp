Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Batch job monitor — view run history, status, and errors for scheduled jobs.
''' </summary>
Public Class frmBatchJobMonitor
    Inherits Form

    Private dgvJobHistory As New DataGridView()
    Private cboJobName As New ComboBox()
    Private cboStatus As New ComboBox()
    Private dtpDateFrom As New DateTimePicker()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnViewLog As New Button()
    Private WithEvents btnClose As New Button()
    Private lblCount As New Label()

    Private Sub frmBatchJobMonitor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Batch Job Monitor"
        Me.Size = New Drawing.Size(900, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Job:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        cboJobName.Location = New Drawing.Point(40, 11) : cboJobName.Size = New Drawing.Size(180, 20) : cboJobName.DropDownStyle = ComboBoxStyle.DropDownList
        cboJobName.Items.AddRange({"(All)", "RenewalProcessor", "ExpirationProcessor", "FraudScoring", "PaymentBatch", "ReserveRecalculator", "ReinsuranceAllocator"})
        cboJobName.SelectedIndex = 0 : pnlTop.Controls.Add(cboJobName)
        pnlTop.Controls.Add(New Label() With {.Text = "Status:", .Location = New Drawing.Point(240, 14), .AutoSize = True})
        cboStatus.Location = New Drawing.Point(290, 11) : cboStatus.Size = New Drawing.Size(100, 20) : cboStatus.DropDownStyle = ComboBoxStyle.DropDownList
        cboStatus.Items.AddRange({"(All)", "COMPLETED", "FAILED", "RUNNING"}) : cboStatus.SelectedIndex = 0 : pnlTop.Controls.Add(cboStatus)
        pnlTop.Controls.Add(New Label() With {.Text = "Since:", .Location = New Drawing.Point(410, 14), .AutoSize = True})
        dtpDateFrom.Location = New Drawing.Point(450, 11) : dtpDateFrom.Size = New Drawing.Size(110, 20) : dtpDateFrom.Format = DateTimePickerFormat.Short : dtpDateFrom.Value = DateTime.Today.AddDays(-7) : pnlTop.Controls.Add(dtpDateFrom)
        btnSearch.Location = New Drawing.Point(580, 8) : btnSearch.Size = New Drawing.Size(80, 28) : btnSearch.Text = "&Search" : pnlTop.Controls.Add(btnSearch)
        lblCount.Location = New Drawing.Point(680, 14) : lblCount.AutoSize = True : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        dgvJobHistory.Dock = DockStyle.Fill : dgvJobHistory.ReadOnly = True : dgvJobHistory.AllowUserToAddRows = False
        dgvJobHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvJobHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvJobHistory)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnViewLog.Location = New Drawing.Point(10, 5) : btnViewLog.Size = New Drawing.Size(100, 30) : btnViewLog.Text = "&View Log" : pnlBottom.Controls.Add(btnViewLog)
        btnClose.Location = New Drawing.Point(780, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvJobHistory.BringToFront()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        lblCount.Text = "0 job runs"
    End Sub

    Private Sub btnViewLog_Click(sender As Object, e As EventArgs) Handles btnViewLog.Click
        If dgvJobHistory.CurrentRow Is Nothing Then Return
        MessageBox.Show("Job execution log would display here.", "Log", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
