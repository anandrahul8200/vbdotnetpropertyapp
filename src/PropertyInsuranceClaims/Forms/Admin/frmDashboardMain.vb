Imports System.Windows.Forms
Imports System.Drawing
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Main landing dashboard — shows KPIs and quick links after login.
''' </summary>
Public Class frmDashboardMain
    Inherits Form

    Private lblWelcome As New Label()
    Private lblLastLogin As New Label()

    ' Quick action buttons
    Private WithEvents btnNewClaim As New Button()
    Private WithEvents btnNewQuote As New Button()
    Private WithEvents btnCustomerSearch As New Button()
    Private WithEvents btnClaimSearch As New Button()
    Private WithEvents btnMyTasks As New Button()
    Private WithEvents btnReports As New Button()

    ' KPI labels
    Private lblOpenClaims As New Label()
    Private lblActivePolicies As New Label()
    Private lblPendingTasks As New Label()
    Private lblOverdueItems As New Label()

    Private Sub frmDashboardMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Dashboard"
        Me.Size = New Drawing.Size(800, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Banner image
        Dim bannerPath As String = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "property_insurance_banner.png")
        If IO.File.Exists(bannerPath) Then
            Dim pnlBanner As New PictureBox()
            pnlBanner.Dock = DockStyle.Top
            pnlBanner.Height = 120
            pnlBanner.SizeMode = PictureBoxSizeMode.StretchImage
            Try
                pnlBanner.Image = Drawing.Image.FromFile(bannerPath)
            Catch
            End Try
            Me.Controls.Add(pnlBanner)
        End If

        ' Welcome
        lblWelcome.Location = New Point(20, 135) : lblWelcome.AutoSize = True
        lblWelcome.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        lblWelcome.Text = "Welcome, " & GlobalState.CurrentUserFullName
        Me.Controls.Add(lblWelcome)

        lblLastLogin.Location = New Point(20, 170) : lblLastLogin.AutoSize = True : lblLastLogin.ForeColor = Color.Gray
        lblLastLogin.Text = "Last login: " & DateTime.Now.AddHours(-8).ToString("MM/dd/yyyy hh:mm tt")
        Me.Controls.Add(lblLastLogin)

        ' KPI Cards
        Dim y As Integer = 200
        CreateKPICard(lblOpenClaims, "Open Claims", "47", 20, y, Color.SteelBlue)
        CreateKPICard(lblActivePolicies, "Active Policies", "400", 210, y, Color.ForestGreen)
        CreateKPICard(lblPendingTasks, "My Tasks", "12", 400, y, Color.DarkOrange)
        CreateKPICard(lblOverdueItems, "Overdue", "3", 590, y, Color.Crimson)

        ' Quick Actions
        y = 310
        Me.Controls.Add(New Label() With {.Text = "Quick Actions", .Location = New Point(20, y), .AutoSize = True, .Font = New Font("Segoe UI", 11, FontStyle.Bold)})
        y += 30
        btnNewQuote.Location = New Point(20, y) : btnNewQuote.Size = New Size(150, 40) : btnNewQuote.Text = "New Quote" : Me.Controls.Add(btnNewQuote)
        btnNewClaim.Location = New Point(180, y) : btnNewClaim.Size = New Size(150, 40) : btnNewClaim.Text = "New FNOL" : Me.Controls.Add(btnNewClaim)
        btnCustomerSearch.Location = New Point(340, y) : btnCustomerSearch.Size = New Size(150, 40) : btnCustomerSearch.Text = "Customer Search" : Me.Controls.Add(btnCustomerSearch)
        y += 50
        btnClaimSearch.Location = New Point(20, y) : btnClaimSearch.Size = New Size(150, 40) : btnClaimSearch.Text = "Claim Search" : Me.Controls.Add(btnClaimSearch)
        btnMyTasks.Location = New Point(180, y) : btnMyTasks.Size = New Size(150, 40) : btnMyTasks.Text = "My Tasks" : Me.Controls.Add(btnMyTasks)
        btnReports.Location = New Point(340, y) : btnReports.Size = New Size(150, 40) : btnReports.Text = "Reports" : Me.Controls.Add(btnReports)
    End Sub

    Private Sub CreateKPICard(lbl As Label, title As String, value As String, x As Integer, y As Integer, accent As Color)
        Dim pnl As New Panel() With {.Location = New Point(x, y), .Size = New Size(170, 80), .BorderStyle = BorderStyle.FixedSingle, .BackColor = Color.White}
        pnl.Controls.Add(New Panel() With {.Location = New Point(0, 0), .Size = New Size(4, 80), .BackColor = accent})
        pnl.Controls.Add(New Label() With {.Text = title, .Location = New Point(12, 5), .AutoSize = True, .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 8)})
        lbl.Location = New Point(12, 25) : lbl.AutoSize = True : lbl.Font = New Font("Segoe UI", 18, FontStyle.Bold) : lbl.ForeColor = accent : lbl.Text = value
        pnl.Controls.Add(lbl)
        Me.Controls.Add(pnl)
    End Sub

    Private Sub btnNewQuote_Click(sender As Object, e As EventArgs) Handles btnNewQuote.Click
        Dim frm As New frmPolicyEntry(0) : frm.ShowDialog(Me)
    End Sub

    Private Sub btnNewClaim_Click(sender As Object, e As EventArgs) Handles btnNewClaim.Click
        Dim frm As New frmClaimFNOL() : frm.ShowDialog(Me)
    End Sub

    Private Sub btnCustomerSearch_Click(sender As Object, e As EventArgs) Handles btnCustomerSearch.Click
        Dim frm As New frmCustomerSearch() : frm.ShowDialog(Me)
    End Sub

    Private Sub btnClaimSearch_Click(sender As Object, e As EventArgs) Handles btnClaimSearch.Click
        Dim frm As New frmClaimSearch() : frm.ShowDialog(Me)
    End Sub

    Private Sub btnMyTasks_Click(sender As Object, e As EventArgs) Handles btnMyTasks.Click
        Dim frm As New frmTaskQueue() : frm.ShowDialog(Me)
    End Sub

    Private Sub btnReports_Click(sender As Object, e As EventArgs) Handles btnReports.Click
        Dim frm As New frmReportViewer() : frm.ShowDialog(Me)
    End Sub
End Class
