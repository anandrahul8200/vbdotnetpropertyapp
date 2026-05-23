Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Policy dashboard — KPIs, production summary, expiring policies, and quick actions.
''' Main landing page after login for policy/underwriting staff.
''' </summary>
Public Class frmPolicyDashboard
    Inherits Form

    ' KPI Cards
    Private pnlActivePolicies As New Panel()
    Private pnlNewBusiness As New Panel()
    Private pnlRenewals As New Panel()
    Private pnlCancellations As New Panel()
    Private pnlWrittenPremium As New Panel()
    Private pnlLossRatio As New Panel()

    Private lblActivePoliciesCount As New Label()
    Private lblActivePoliciesSub As New Label()
    Private lblNewBusinessCount As New Label()
    Private lblNewBusinessSub As New Label()
    Private lblRenewalsCount As New Label()
    Private lblRenewalsSub As New Label()
    Private lblCancellationsCount As New Label()
    Private lblCancellationsSub As New Label()
    Private lblWrittenPremiumAmount As New Label()
    Private lblWrittenPremiumSub As New Label()
    Private lblLossRatioValue As New Label()
    Private lblLossRatioSub As New Label()

    ' Grids
    Private dgvExpiringPolicies As New DataGridView()
    Private dgvRecentActivity As New DataGridView()
    Private dgvPendingReferrals As New DataGridView()

    ' Quick action buttons
    Private WithEvents btnNewQuote As New Button()
    Private WithEvents btnCustomerSearch As New Button()
    Private WithEvents btnPolicySearch As New Button()
    Private WithEvents btnReferralQueue As New Button()
    Private WithEvents btnRefresh As New Button()

    ' Period filter
    Private cboPeriod As New ComboBox()
    Private WithEvents btnApplyPeriod As New Button()

    Private Sub frmPolicyDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Policy Dashboard"
            Me.Size = New Drawing.Size(1100, 750)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadDashboard()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPolicyDashboard_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Top toolbar
        Dim pnlToolbar As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        btnRefresh.Location = New Point(10, 8) : btnRefresh.Size = New Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlToolbar.Controls.Add(btnRefresh)
        pnlToolbar.Controls.Add(New Label() With {.Text = "Period:", .Location = New Point(120, 12), .AutoSize = True})
        cboPeriod.Location = New Point(170, 9) : cboPeriod.Size = New Size(120, 20) : cboPeriod.DropDownStyle = ComboBoxStyle.DropDownList
        cboPeriod.Items.AddRange({"This Month", "Last Month", "This Quarter", "This Year", "Last 12 Months"})
        cboPeriod.SelectedIndex = 0 : pnlToolbar.Controls.Add(cboPeriod)
        btnApplyPeriod.Location = New Point(300, 8) : btnApplyPeriod.Size = New Size(60, 28) : btnApplyPeriod.Text = "Apply" : pnlToolbar.Controls.Add(btnApplyPeriod)

        ' Quick actions
        btnNewQuote.Location = New Point(500, 8) : btnNewQuote.Size = New Size(100, 28) : btnNewQuote.Text = "New &Quote" : pnlToolbar.Controls.Add(btnNewQuote)
        btnCustomerSearch.Location = New Point(610, 8) : btnCustomerSearch.Size = New Size(120, 28) : btnCustomerSearch.Text = "&Customer Search" : pnlToolbar.Controls.Add(btnCustomerSearch)
        btnPolicySearch.Location = New Point(740, 8) : btnPolicySearch.Size = New Size(110, 28) : btnPolicySearch.Text = "&Policy Search" : pnlToolbar.Controls.Add(btnPolicySearch)
        btnReferralQueue.Location = New Point(860, 8) : btnReferralQueue.Size = New Size(120, 28) : btnReferralQueue.Text = "Re&ferral Queue" : pnlToolbar.Controls.Add(btnReferralQueue)
        Me.Controls.Add(pnlToolbar)

        ' KPI Cards row
        Dim pnlKPIs As New Panel() With {.Dock = DockStyle.Top, .Height = 95, .BackColor = Color.WhiteSmoke, .Padding = New Padding(5)}
        CreateKPICard(pnlActivePolicies, "Active Policies", lblActivePoliciesCount, lblActivePoliciesSub, 10, Color.SteelBlue)
        CreateKPICard(pnlNewBusiness, "New Business", lblNewBusinessCount, lblNewBusinessSub, 185, Color.ForestGreen)
        CreateKPICard(pnlRenewals, "Renewals", lblRenewalsCount, lblRenewalsSub, 360, Color.DarkOrange)
        CreateKPICard(pnlCancellations, "Cancellations", lblCancellationsCount, lblCancellationsSub, 535, Color.Crimson)
        CreateKPICard(pnlWrittenPremium, "Written Premium", lblWrittenPremiumAmount, lblWrittenPremiumSub, 710, Color.Purple)
        CreateKPICard(pnlLossRatio, "Loss Ratio", lblLossRatioValue, lblLossRatioSub, 885, Color.Teal)
        pnlKPIs.Controls.AddRange({pnlActivePolicies, pnlNewBusiness, pnlRenewals, pnlCancellations, pnlWrittenPremium, pnlLossRatio})
        Me.Controls.Add(pnlKPIs)

        ' Middle section: Expiring policies and pending referrals side by side
        Dim pnlMiddle As New Panel() With {.Dock = DockStyle.Top, .Height = 250}

        Dim grpExpiring As New GroupBox() With {.Text = "Policies Expiring (Next 30 Days)", .Location = New Point(5, 5), .Size = New Size(540, 240)}
        dgvExpiringPolicies.Dock = DockStyle.Fill : dgvExpiringPolicies.ReadOnly = True : dgvExpiringPolicies.AllowUserToAddRows = False
        dgvExpiringPolicies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill : dgvExpiringPolicies.RowHeadersVisible = False
        grpExpiring.Controls.Add(dgvExpiringPolicies)
        pnlMiddle.Controls.Add(grpExpiring)

        Dim grpReferrals As New GroupBox() With {.Text = "Pending Referrals", .Location = New Point(555, 5), .Size = New Size(520, 240)}
        dgvPendingReferrals.Dock = DockStyle.Fill : dgvPendingReferrals.ReadOnly = True : dgvPendingReferrals.AllowUserToAddRows = False
        dgvPendingReferrals.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill : dgvPendingReferrals.RowHeadersVisible = False
        grpReferrals.Controls.Add(dgvPendingReferrals)
        pnlMiddle.Controls.Add(grpReferrals)
        Me.Controls.Add(pnlMiddle)

        ' Bottom: Recent activity
        Dim grpRecent As New GroupBox() With {.Text = "Recent Activity", .Dock = DockStyle.Fill}
        dgvRecentActivity.Dock = DockStyle.Fill : dgvRecentActivity.ReadOnly = True : dgvRecentActivity.AllowUserToAddRows = False
        dgvRecentActivity.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill : dgvRecentActivity.RowHeadersVisible = False
        grpRecent.Controls.Add(dgvRecentActivity)
        Me.Controls.Add(grpRecent)
        grpRecent.BringToFront()
    End Sub

    Private Sub CreateKPICard(pnl As Panel, title As String, lblMain As Label, lblSub As Label, x As Integer, accent As Color)
        pnl.Location = New Point(x, 8) : pnl.Size = New Size(165, 75) : pnl.BorderStyle = BorderStyle.FixedSingle : pnl.BackColor = Color.White
        Dim bar As New Panel() With {.Location = New Point(0, 0), .Size = New Size(4, 75), .BackColor = accent}
        pnl.Controls.Add(bar)
        Dim lblTitle As New Label() With {.Text = title, .Location = New Point(10, 4), .AutoSize = True, .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 7.5F)}
        pnl.Controls.Add(lblTitle)
        lblMain.Location = New Point(10, 20) : lblMain.AutoSize = True : lblMain.Font = New Font("Segoe UI", 14, FontStyle.Bold) : lblMain.ForeColor = accent : lblMain.Text = "--"
        pnl.Controls.Add(lblMain)
        If lblSub IsNot Nothing Then
            lblSub.Location = New Point(10, 52) : lblSub.AutoSize = True : lblSub.ForeColor = Color.DimGray : lblSub.Font = New Font("Segoe UI", 7.5F) : lblSub.Text = ""
            pnl.Controls.Add(lblSub)
        End If
    End Sub

    Private Sub LoadDashboard()
        Try
            Me.Cursor = Cursors.WaitCursor

            ' Load KPIs
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@AsOfDate", DateTime.Today)
            }
            Dim ds As DataSet = DatabaseHelper.ExecuteDataSet("Reporting.usp_Dashboard_PolicyKPIs", params)

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                Dim row As DataRow = ds.Tables(0).Rows(0)
                lblActivePoliciesCount.Text = CInt(If(IsDBNull(row("ActivePolicies")), 0, row("ActivePolicies"))).ToString("N0")
                lblNewBusinessCount.Text = CInt(If(IsDBNull(row("NewBusiness")), 0, row("NewBusiness"))).ToString("N0")
                lblRenewalsCount.Text = CInt(If(IsDBNull(row("Renewals")), 0, row("Renewals"))).ToString("N0")
                lblCancellationsCount.Text = CInt(If(IsDBNull(row("Cancellations")), 0, row("Cancellations"))).ToString("N0")
                Dim writtenPrem As Decimal = CDec(If(IsDBNull(row("WrittenPremium")), 0, row("WrittenPremium")))
                lblWrittenPremiumAmount.Text = If(writtenPrem >= 1000000, $"{writtenPrem / 1000000:N1}M", writtenPrem.ToString("C0"))
                Dim lossRatio As Decimal = CDec(If(IsDBNull(row("LossRatio")), 0, row("LossRatio")))
                lblLossRatioValue.Text = $"{lossRatio:N1}%"
                lblLossRatioValue.ForeColor = If(lossRatio > 70, Color.Red, If(lossRatio > 55, Color.DarkOrange, Color.ForestGreen))
            End If

            ' Load expiring policies
            If ds.Tables.Count > 1 Then dgvExpiringPolicies.DataSource = ds.Tables(1)

            ' Load pending referrals
            Dim refDt As DataTable = UnderwritingDataAccess.GetPendingReferrals()
            dgvPendingReferrals.DataSource = refDt

            ' Load recent activity
            If ds.Tables.Count > 2 Then dgvRecentActivity.DataSource = ds.Tables(2)

        Catch ex As Exception
            ' Dashboard should not crash on data errors
            ErrorLogger.LogError(ex, "LoadDashboard")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadDashboard()
    End Sub

    Private Sub btnApplyPeriod_Click(sender As Object, e As EventArgs) Handles btnApplyPeriod.Click
        LoadDashboard()
    End Sub

    Private Sub btnNewQuote_Click(sender As Object, e As EventArgs) Handles btnNewQuote.Click
        Dim frm As New frmPolicyEntry(0)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnCustomerSearch_Click(sender As Object, e As EventArgs) Handles btnCustomerSearch.Click
        Dim frm As New frmCustomerSearch()
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnPolicySearch_Click(sender As Object, e As EventArgs) Handles btnPolicySearch.Click
        Dim frm As New frmPolicySearch()
        frm.MdiParent = Me.MdiParent
        frm.Show()
    End Sub

    Private Sub btnReferralQueue_Click(sender As Object, e As EventArgs) Handles btnReferralQueue.Click
        Dim frm As New frmReferralQueue()
        frm.MdiParent = Me.MdiParent
        frm.Show()
    End Sub
End Class
