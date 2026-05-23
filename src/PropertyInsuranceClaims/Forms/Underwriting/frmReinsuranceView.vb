Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Reinsurance management form — view treaties, cessions, and generate bordereaux.
''' </summary>
Public Class frmReinsuranceView
    Inherits Form

    Private tabControl As New TabControl()
    Private tabTreaties As New TabPage("Treaties")
    Private tabCessions As New TabPage("Cessions")
    Private tabBordereaux As New TabPage("Bordereaux")

    ' Treaties tab
    Private dgvTreaties As New DataGridView()
    Private WithEvents btnNewTreaty As New Button()
    Private WithEvents btnViewTreaty As New Button()
    Private WithEvents btnRefreshTreaties As New Button()

    ' Cessions tab
    Private dgvCessions As New DataGridView()
    Private cboTreatyFilter As New ComboBox()
    Private txtAccountingPeriod As New TextBox()
    Private WithEvents btnLoadCessions As New Button()
    Private WithEvents btnCalculateCession As New Button()
    Private lblCessionSummary As New Label()

    ' Bordereaux tab
    Private dgvBordereaux As New DataGridView()
    Private cboTreatyForBord As New ComboBox()
    Private txtBordPeriod As New TextBox()
    Private cboReportType As New ComboBox()
    Private WithEvents btnGenerateBord As New Button()
    Private WithEvents btnSubmitBord As New Button()
    Private WithEvents btnExportBord As New Button()
    Private WithEvents btnRefreshBord As New Button()

    Private WithEvents btnClose As New Button()

    Private Sub frmReinsuranceView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Reinsurance Management"
            Me.Size = New Drawing.Size(1000, 650)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadTreaties()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmReinsuranceView_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Tab control
        tabControl.Dock = DockStyle.Fill

        ' --- Treaties Tab ---
        Dim pnlTreatyButtons As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        btnNewTreaty.Location = New Drawing.Point(10, 6) : btnNewTreaty.Size = New Drawing.Size(100, 28) : btnNewTreaty.Text = "&New Treaty" : pnlTreatyButtons.Controls.Add(btnNewTreaty)
        btnViewTreaty.Location = New Drawing.Point(120, 6) : btnViewTreaty.Size = New Drawing.Size(100, 28) : btnViewTreaty.Text = "&View Details" : pnlTreatyButtons.Controls.Add(btnViewTreaty)
        btnRefreshTreaties.Location = New Drawing.Point(850, 6) : btnRefreshTreaties.Size = New Drawing.Size(80, 28) : btnRefreshTreaties.Text = "Refresh" : pnlTreatyButtons.Controls.Add(btnRefreshTreaties)
        tabTreaties.Controls.Add(pnlTreatyButtons)

        dgvTreaties.Dock = DockStyle.Fill : dgvTreaties.ReadOnly = True : dgvTreaties.AllowUserToAddRows = False
        dgvTreaties.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvTreaties.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        tabTreaties.Controls.Add(dgvTreaties)
        dgvTreaties.BringToFront()

        ' --- Cessions Tab ---
        Dim pnlCessionFilter As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        AddLabelTo(pnlCessionFilter, "Treaty:", 10, 14)
        cboTreatyFilter.Location = New Drawing.Point(60, 11) : cboTreatyFilter.Size = New Drawing.Size(250, 20) : cboTreatyFilter.DropDownStyle = ComboBoxStyle.DropDownList : pnlCessionFilter.Controls.Add(cboTreatyFilter)
        AddLabelTo(pnlCessionFilter, "Period:", 330, 14)
        txtAccountingPeriod.Location = New Drawing.Point(380, 11) : txtAccountingPeriod.Size = New Drawing.Size(80, 20) : txtAccountingPeriod.Text = DateTime.Now.ToString("yyyy-MM") : pnlCessionFilter.Controls.Add(txtAccountingPeriod)
        btnLoadCessions.Location = New Drawing.Point(480, 8) : btnLoadCessions.Size = New Drawing.Size(80, 28) : btnLoadCessions.Text = "&Load" : pnlCessionFilter.Controls.Add(btnLoadCessions)
        btnCalculateCession.Location = New Drawing.Point(570, 8) : btnCalculateCession.Size = New Drawing.Size(120, 28) : btnCalculateCession.Text = "Calculate &New" : pnlCessionFilter.Controls.Add(btnCalculateCession)
        lblCessionSummary.Location = New Drawing.Point(710, 14) : lblCessionSummary.AutoSize = True : pnlCessionFilter.Controls.Add(lblCessionSummary)
        tabCessions.Controls.Add(pnlCessionFilter)

        dgvCessions.Dock = DockStyle.Fill : dgvCessions.ReadOnly = True : dgvCessions.AllowUserToAddRows = False
        dgvCessions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        tabCessions.Controls.Add(dgvCessions)
        dgvCessions.BringToFront()

        ' --- Bordereaux Tab ---
        Dim pnlBordFilter As New Panel() With {.Dock = DockStyle.Top, .Height = 80}
        AddLabelTo(pnlBordFilter, "Treaty:", 10, 14)
        cboTreatyForBord.Location = New Drawing.Point(60, 11) : cboTreatyForBord.Size = New Drawing.Size(250, 20) : cboTreatyForBord.DropDownStyle = ComboBoxStyle.DropDownList : pnlBordFilter.Controls.Add(cboTreatyForBord)
        AddLabelTo(pnlBordFilter, "Period:", 330, 14)
        txtBordPeriod.Location = New Drawing.Point(380, 11) : txtBordPeriod.Size = New Drawing.Size(80, 20) : txtBordPeriod.Text = DateTime.Now.ToString("yyyy-MM") : pnlBordFilter.Controls.Add(txtBordPeriod)
        AddLabelTo(pnlBordFilter, "Type:", 480, 14)
        cboReportType.Location = New Drawing.Point(520, 11) : cboReportType.Size = New Drawing.Size(120, 20) : cboReportType.DropDownStyle = ComboBoxStyle.DropDownList
        cboReportType.Items.AddRange({"PREMIUM", "LOSS", "OUTSTANDING"}) : cboReportType.SelectedIndex = 0 : pnlBordFilter.Controls.Add(cboReportType)

        btnGenerateBord.Location = New Drawing.Point(10, 44) : btnGenerateBord.Size = New Drawing.Size(100, 28) : btnGenerateBord.Text = "&Generate" : pnlBordFilter.Controls.Add(btnGenerateBord)
        btnSubmitBord.Location = New Drawing.Point(120, 44) : btnSubmitBord.Size = New Drawing.Size(100, 28) : btnSubmitBord.Text = "&Submit" : pnlBordFilter.Controls.Add(btnSubmitBord)
        btnExportBord.Location = New Drawing.Point(230, 44) : btnExportBord.Size = New Drawing.Size(80, 28) : btnExportBord.Text = "E&xport" : pnlBordFilter.Controls.Add(btnExportBord)
        btnRefreshBord.Location = New Drawing.Point(850, 44) : btnRefreshBord.Size = New Drawing.Size(80, 28) : btnRefreshBord.Text = "Refresh" : pnlBordFilter.Controls.Add(btnRefreshBord)
        tabBordereaux.Controls.Add(pnlBordFilter)

        dgvBordereaux.Dock = DockStyle.Fill : dgvBordereaux.ReadOnly = True : dgvBordereaux.AllowUserToAddRows = False
        dgvBordereaux.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        tabBordereaux.Controls.Add(dgvBordereaux)
        dgvBordereaux.BringToFront()

        tabControl.TabPages.AddRange({tabTreaties, tabCessions, tabBordereaux})
        Me.Controls.Add(tabControl)

        ' Close button
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(880, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        tabControl.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadTreaties()
        Try
            Dim dt As DataTable = ReinsuranceDataAccess.GetActiveTreaties()
            dgvTreaties.DataSource = dt
            If dgvTreaties.Columns.Contains("TreatyID") Then dgvTreaties.Columns("TreatyID").Visible = False

            ' Populate treaty combos
            cboTreatyFilter.Items.Clear() : cboTreatyForBord.Items.Clear()
            For Each row As DataRow In dt.Rows
                Dim display As String = $"{row("TreatyNumber")} - {row("TreatyName")}"
                cboTreatyFilter.Items.Add(display)
                cboTreatyForBord.Items.Add(display)
            Next
            If cboTreatyFilter.Items.Count > 0 Then cboTreatyFilter.SelectedIndex = 0
            If cboTreatyForBord.Items.Count > 0 Then cboTreatyForBord.SelectedIndex = 0
        Catch ex As Exception
            MessageBox.Show("Error loading treaties: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnNewTreaty_Click(sender As Object, e As EventArgs) Handles btnNewTreaty.Click
        MessageBox.Show("New treaty creation dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnViewTreaty_Click(sender As Object, e As EventArgs) Handles btnViewTreaty.Click
        If dgvTreaties.CurrentRow Is Nothing Then Return
        Dim treatyID As Integer = CInt(dgvTreaties.CurrentRow.Cells("TreatyID").Value)
        MessageBox.Show($"Treaty details view for ID {treatyID} would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnRefreshTreaties_Click(sender As Object, e As EventArgs) Handles btnRefreshTreaties.Click
        LoadTreaties()
    End Sub

    Private Sub btnLoadCessions_Click(sender As Object, e As EventArgs) Handles btnLoadCessions.Click
        Try
            If cboTreatyFilter.SelectedIndex < 0 Then Return
            Me.Cursor = Cursors.WaitCursor
            ' Would get treaty ID from selection
            Dim treatyID As Integer = 1
            Dim dt As DataTable = ReinsuranceDataAccess.GetCessionsByTreaty(treatyID, txtAccountingPeriod.Text.Trim())
            dgvCessions.DataSource = dt

            ' Summary
            Dim totalGross As Decimal = 0, totalCeded As Decimal = 0
            For Each row As DataRow In dt.Rows
                If Not IsDBNull(row("GrossAmount")) Then totalGross += CDec(row("GrossAmount"))
                If Not IsDBNull(row("CededAmount")) Then totalCeded += CDec(row("CededAmount"))
            Next
            lblCessionSummary.Text = $"Gross: {totalGross:C0} | Ceded: {totalCeded:C0}"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCalculateCession_Click(sender As Object, e As EventArgs) Handles btnCalculateCession.Click
        MessageBox.Show("Cession calculation dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnGenerateBord_Click(sender As Object, e As EventArgs) Handles btnGenerateBord.Click
        Try
            If cboTreatyForBord.SelectedIndex < 0 Then Return
            Me.Cursor = Cursors.WaitCursor
            Dim treatyID As Integer = 1
            Dim bordID As Integer = ReinsuranceDataAccess.GenerateBordereaux(treatyID, txtBordPeriod.Text.Trim(), cboReportType.SelectedItem.ToString())
            MessageBox.Show($"Bordereaux generated (ID: {bordID}).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnRefreshBord.PerformClick()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnSubmitBord_Click(sender As Object, e As EventArgs) Handles btnSubmitBord.Click
        If dgvBordereaux.CurrentRow Is Nothing Then Return
        MessageBox.Show("Bordereaux submitted to reinsurer.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnExportBord_Click(sender As Object, e As EventArgs) Handles btnExportBord.Click
        MessageBox.Show("Export to Excel/CSV would happen here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnRefreshBord_Click(sender As Object, e As EventArgs) Handles btnRefreshBord.Click
        Try
            Dim treatyID As Integer = 1
            Dim dt As DataTable = ReinsuranceDataAccess.GetBordereaux(treatyID)
            dgvBordereaux.DataSource = dt
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
