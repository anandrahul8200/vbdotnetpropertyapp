Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Agent commission statement viewer.
''' </summary>
Public Class frmCommissionStatement
    Inherits Form

    Private cboAgent As New ComboBox()
    Private dtpPeriodFrom As New DateTimePicker()
    Private dtpPeriodTo As New DateTimePicker()
    Private WithEvents btnGenerate As New Button()
    Private WithEvents btnExport As New Button()

    ' Summary labels
    Private lblAgentName As New Label()
    Private lblTotalEarned As New Label()
    Private lblTotalReversals As New Label()
    Private lblNetCommission As New Label()

    Private dgvDetail As New DataGridView()

    Private Sub frmCommissionStatement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Commission Statement"
        Me.Size = New Drawing.Size(900, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Filter panel
        Dim pnlFilter As New Panel() With {.Dock = DockStyle.Top, .Height = 50}
        pnlFilter.Controls.Add(New Label() With {.Text = "Agent:", .Location = New Drawing.Point(10, 15), .AutoSize = True})
        cboAgent.Location = New Drawing.Point(55, 12) : cboAgent.Size = New Drawing.Size(200, 20) : cboAgent.DropDownStyle = ComboBoxStyle.DropDownList
        cboAgent.Items.Add("(Select Agent)") : cboAgent.SelectedIndex = 0 : pnlFilter.Controls.Add(cboAgent)
        pnlFilter.Controls.Add(New Label() With {.Text = "From:", .Location = New Drawing.Point(280, 15), .AutoSize = True})
        dtpPeriodFrom.Location = New Drawing.Point(320, 12) : dtpPeriodFrom.Size = New Drawing.Size(110, 20) : dtpPeriodFrom.Format = DateTimePickerFormat.Short
        dtpPeriodFrom.Value = New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1) : pnlFilter.Controls.Add(dtpPeriodFrom)
        pnlFilter.Controls.Add(New Label() With {.Text = "To:", .Location = New Drawing.Point(440, 15), .AutoSize = True})
        dtpPeriodTo.Location = New Drawing.Point(465, 12) : dtpPeriodTo.Size = New Drawing.Size(110, 20) : dtpPeriodTo.Format = DateTimePickerFormat.Short : pnlFilter.Controls.Add(dtpPeriodTo)
        btnGenerate.Location = New Drawing.Point(600, 8) : btnGenerate.Size = New Drawing.Size(90, 30) : btnGenerate.Text = "&Generate" : pnlFilter.Controls.Add(btnGenerate)
        btnExport.Location = New Drawing.Point(700, 8) : btnExport.Size = New Drawing.Size(80, 30) : btnExport.Text = "&Export" : btnExport.Enabled = False : pnlFilter.Controls.Add(btnExport)
        Me.Controls.Add(pnlFilter)

        ' Summary panel
        Dim pnlSummary As New Panel() With {.Dock = DockStyle.Top, .Height = 60, .BackColor = Drawing.Color.WhiteSmoke}
        lblAgentName.Location = New Drawing.Point(10, 8) : lblAgentName.AutoSize = True : lblAgentName.Font = New Drawing.Font("Segoe UI", 11, Drawing.FontStyle.Bold) : pnlSummary.Controls.Add(lblAgentName)
        lblTotalEarned.Location = New Drawing.Point(10, 35) : lblTotalEarned.AutoSize = True : pnlSummary.Controls.Add(lblTotalEarned)
        lblTotalReversals.Location = New Drawing.Point(250, 35) : lblTotalReversals.AutoSize = True : pnlSummary.Controls.Add(lblTotalReversals)
        lblNetCommission.Location = New Drawing.Point(500, 35) : lblNetCommission.AutoSize = True : lblNetCommission.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlSummary.Controls.Add(lblNetCommission)
        Me.Controls.Add(pnlSummary)

        ' Detail grid
        dgvDetail.Dock = DockStyle.Fill : dgvDetail.ReadOnly = True : dgvDetail.AllowUserToAddRows = False
        dgvDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvDetail)
        dgvDetail.BringToFront()
    End Sub

    Private Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            ' In production, would get AgentID from combo selection
            Dim agentID As Integer = 1
            Dim ds As DataSet = BillingDataAccess.GetCommissionStatement(agentID, dtpPeriodFrom.Value.Date, dtpPeriodTo.Value.Date)

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                Dim summary As DataRow = ds.Tables(0).Rows(0)
                lblAgentName.Text = summary("AgentName").ToString()
                lblTotalEarned.Text = $"Earned: {CDec(If(IsDBNull(summary("TotalEarned")), 0, summary("TotalEarned"))):C}"
                lblTotalReversals.Text = $"Reversals: {CDec(If(IsDBNull(summary("TotalReversals")), 0, summary("TotalReversals"))):C}"
                lblNetCommission.Text = $"Net: {CDec(If(IsDBNull(summary("NetCommission")), 0, summary("NetCommission"))):C}"
            End If

            If ds.Tables.Count > 1 Then dgvDetail.DataSource = ds.Tables(1)
            btnExport.Enabled = True
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmCommissionStatement.btnGenerate_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        MessageBox.Show("Export to CSV/Excel would happen here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
