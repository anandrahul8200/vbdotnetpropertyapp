Imports System.Windows.Forms
Imports System.Data
Imports System.Drawing
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Claims dashboard — summary statistics, open claims by status, overdue activities,
''' pending payments, and high-priority claims list.
''' </summary>
Public Class frmClaimDashboard
    Inherits Form

    ' Summary panels
    Private pnlOpenClaims As New Panel()
    Private pnlNewLast30 As New Panel()
    Private pnlOverdueActivities As New Panel()
    Private pnlPendingPayments As New Panel()

    Private lblOpenClaimsCount As New Label()
    Private lblOpenClaimsReserve As New Label()
    Private lblNewClaimsCount As New Label()
    Private lblNewClaimsLoss As New Label()
    Private lblOverdueCount As New Label()
    Private lblPendingPayCount As New Label()
    Private lblPendingPayAmount As New Label()

    ' Status breakdown grid
    Private dgvStatusBreakdown As New DataGridView()

    ' High priority claims
    Private dgvHighPriority As New DataGridView()

    ' Buttons
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnOpenClaim As New Button()
    Private WithEvents btnMyWorkqueue As New Button()

    Private Sub frmClaimDashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Claims Dashboard"
            Me.Size = New Drawing.Size(1050, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadDashboard()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimDashboard_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Top toolbar
        Dim pnlToolbar As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        btnRefresh.Location = New Point(10, 6) : btnRefresh.Size = New Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlToolbar.Controls.Add(btnRefresh)
        btnMyWorkqueue.Location = New Point(100, 6) : btnMyWorkqueue.Size = New Size(120, 28) : btnMyWorkqueue.Text = "&My Workqueue" : pnlToolbar.Controls.Add(btnMyWorkqueue)
        btnOpenClaim.Location = New Point(230, 6) : btnOpenClaim.Size = New Size(100, 28) : btnOpenClaim.Text = "&Open Claim" : btnOpenClaim.Enabled = False : pnlToolbar.Controls.Add(btnOpenClaim)
        Me.Controls.Add(pnlToolbar)

        ' Summary cards row
        Dim pnlSummary As New Panel() With {.Dock = DockStyle.Top, .Height = 90, .BackColor = Color.WhiteSmoke}
        CreateSummaryCard(pnlOpenClaims, "Open Claims", lblOpenClaimsCount, lblOpenClaimsReserve, 10, Color.SteelBlue)
        CreateSummaryCard(pnlNewLast30, "New (30 days)", lblNewClaimsCount, lblNewClaimsLoss, 260, Color.DarkOrange)
        CreateSummaryCard(pnlOverdueActivities, "Overdue Activities", lblOverdueCount, Nothing, 510, Color.Crimson)
        CreateSummaryCard(pnlPendingPayments, "Pending Payments", lblPendingPayCount, lblPendingPayAmount, 710, Color.DarkGreen)
        pnlSummary.Controls.AddRange({pnlOpenClaims, pnlNewLast30, pnlOverdueActivities, pnlPendingPayments})
        Me.Controls.Add(pnlSummary)

        ' Middle: Status breakdown
        Dim grpStatus As New GroupBox() With {.Text = "Open Claims by Status", .Dock = DockStyle.Top, .Height = 180}
        dgvStatusBreakdown.Dock = DockStyle.Fill : dgvStatusBreakdown.ReadOnly = True : dgvStatusBreakdown.AllowUserToAddRows = False
        dgvStatusBreakdown.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvStatusBreakdown.RowHeadersVisible = False
        grpStatus.Controls.Add(dgvStatusBreakdown)
        Me.Controls.Add(grpStatus)

        ' Bottom: High priority claims
        Dim grpHighPriority As New GroupBox() With {.Text = "High Priority Claims", .Dock = DockStyle.Fill}
        dgvHighPriority.Dock = DockStyle.Fill : dgvHighPriority.ReadOnly = True : dgvHighPriority.AllowUserToAddRows = False
        dgvHighPriority.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvHighPriority.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        AddHandler dgvHighPriority.SelectionChanged, Sub() btnOpenClaim.Enabled = dgvHighPriority.CurrentRow IsNot Nothing
        grpHighPriority.Controls.Add(dgvHighPriority)
        Me.Controls.Add(grpHighPriority)
        grpHighPriority.BringToFront()
    End Sub

    Private Sub CreateSummaryCard(pnl As Panel, title As String, lblMain As Label, lblSub As Label, x As Integer, accentColor As Color)
        pnl.Location = New Point(x, 10)
        pnl.Size = New Size(235, 70)
        pnl.BorderStyle = BorderStyle.FixedSingle
        pnl.BackColor = Color.White

        ' Accent bar
        Dim pnlAccent As New Panel() With {.Location = New Point(0, 0), .Size = New Size(5, 70), .BackColor = accentColor}
        pnl.Controls.Add(pnlAccent)

        ' Title
        Dim lblTitle As New Label() With {.Text = title, .Location = New Point(12, 5), .AutoSize = True, .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 8)}
        pnl.Controls.Add(lblTitle)

        ' Main value
        lblMain.Location = New Point(12, 22) : lblMain.AutoSize = True : lblMain.Font = New Font("Segoe UI", 16, FontStyle.Bold) : lblMain.ForeColor = accentColor : lblMain.Text = "0"
        pnl.Controls.Add(lblMain)

        ' Sub value
        If lblSub IsNot Nothing Then
            lblSub.Location = New Point(12, 50) : lblSub.AutoSize = True : lblSub.ForeColor = Color.DimGray : lblSub.Font = New Font("Segoe UI", 8) : lblSub.Text = ""
            pnl.Controls.Add(lblSub)
        End If
    End Sub

    Private Sub LoadDashboard()
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim ds As DataSet = ClaimDataAccess.GetDashboard()

            ' Result Set 0: Open claims by status
            If ds.Tables.Count > 0 Then
                dgvStatusBreakdown.DataSource = ds.Tables(0)
                Dim totalOpen As Integer = 0
                Dim totalReserve As Decimal = 0
                For Each row As DataRow In ds.Tables(0).Rows
                    totalOpen += CInt(row("ClaimCount"))
                    If Not IsDBNull(row("TotalReserve")) Then totalReserve += CDec(row("TotalReserve"))
                Next
                lblOpenClaimsCount.Text = totalOpen.ToString()
                lblOpenClaimsReserve.Text = $"Reserve: {totalReserve:C0}"
            End If

            ' Result Set 1: New claims last 30 days
            If ds.Tables.Count > 1 AndAlso ds.Tables(1).Rows.Count > 0 Then
                Dim row As DataRow = ds.Tables(1).Rows(0)
                lblNewClaimsCount.Text = CInt(If(IsDBNull(row("NewClaimsLast30Days")), 0, row("NewClaimsLast30Days"))).ToString()
                Dim estLoss As Decimal = CDec(If(IsDBNull(row("EstimatedLossLast30Days")), 0, row("EstimatedLossLast30Days")))
                lblNewClaimsLoss.Text = $"Est. Loss: {estLoss:C0}"
            End If

            ' Result Set 2: Overdue activities
            If ds.Tables.Count > 2 AndAlso ds.Tables(2).Rows.Count > 0 Then
                lblOverdueCount.Text = CInt(If(IsDBNull(ds.Tables(2).Rows(0)("OverdueActivities")), 0, ds.Tables(2).Rows(0)("OverdueActivities"))).ToString()
            End If

            ' Result Set 3: Pending payments
            If ds.Tables.Count > 3 AndAlso ds.Tables(3).Rows.Count > 0 Then
                Dim row As DataRow = ds.Tables(3).Rows(0)
                lblPendingPayCount.Text = CInt(If(IsDBNull(row("PendingPayments")), 0, row("PendingPayments"))).ToString()
                Dim pendingAmt As Decimal = CDec(If(IsDBNull(row("PendingPaymentTotal")), 0, row("PendingPaymentTotal")))
                lblPendingPayAmount.Text = $"Total: {pendingAmt:C0}"
            End If

            ' Result Set 4: High priority claims
            If ds.Tables.Count > 4 Then
                dgvHighPriority.DataSource = ds.Tables(4)
                If dgvHighPriority.Columns.Contains("ClaimID") Then dgvHighPriority.Columns("ClaimID").Visible = False
            End If

        Catch ex As Exception
            MessageBox.Show("Error loading dashboard: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "LoadDashboard")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadDashboard()
    End Sub

    Private Sub btnOpenClaim_Click(sender As Object, e As EventArgs) Handles btnOpenClaim.Click
        If dgvHighPriority.CurrentRow Is Nothing Then Return
        Dim claimID As Integer = CInt(dgvHighPriority.CurrentRow.Cells("ClaimID").Value)
        Dim frm As New frmClaimView(claimID)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnMyWorkqueue_Click(sender As Object, e As EventArgs) Handles btnMyWorkqueue.Click
        ' Would filter to current user's assigned claims
        MessageBox.Show("Filtering to your assigned claims...", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
