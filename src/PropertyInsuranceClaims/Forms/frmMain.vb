Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Main MDI parent form with menu bar and status strip.
''' </summary>
Public Class frmMain
    Inherits Form

    Private mnuMain As New MenuStrip()
    Private statusStrip As New StatusStrip()
    Private lblStatusUser As New ToolStripStatusLabel()
    Private lblStatusRole As New ToolStripStatusLabel()
    Private lblStatusDate As New ToolStripStatusLabel()

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = $"{AppSettings.ApplicationName} - {GlobalState.CurrentUserFullName}"
        Me.Size = New Drawing.Size(1280, 800)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.IsMdiContainer = True
        Me.WindowState = FormWindowState.Maximized

        BuildMenu()
        BuildStatusBar()

        ' Auto-open dashboard on login
        Dim dashboard As New frmDashboardMain()
        dashboard.MdiParent = Me
        dashboard.WindowState = FormWindowState.Maximized
        dashboard.Show()
    End Sub

    Private Sub BuildMenu()
        ' --- Policy Menu ---
        Dim mnuPolicy As New ToolStripMenuItem("&Policy")
        mnuPolicy.DropDownItems.Add("&Customer Search", Nothing, AddressOf mnuCustomerSearch_Click)
        mnuPolicy.DropDownItems.Add("&New Customer", Nothing, AddressOf mnuNewCustomer_Click)
        mnuPolicy.DropDownItems.Add(New ToolStripSeparator())
        mnuPolicy.DropDownItems.Add("&Policy Search", Nothing, AddressOf mnuPolicySearch_Click)
        mnuPolicy.DropDownItems.Add("New &Quote", Nothing, AddressOf mnuNewQuote_Click)

        ' --- Claims Menu ---
        Dim mnuClaims As New ToolStripMenuItem("&Claims")
        mnuClaims.DropDownItems.Add("Claim &Search", Nothing, AddressOf mnuClaimSearch_Click)
        mnuClaims.DropDownItems.Add("New &FNOL", Nothing, AddressOf mnuNewFNOL_Click)

        ' --- Underwriting Menu ---
        Dim mnuUnderwriting As New ToolStripMenuItem("&Underwriting")
        mnuUnderwriting.DropDownItems.Add("&Referral Queue", Nothing, AddressOf mnuReferralQueue_Click)
        mnuUnderwriting.DropDownItems.Add("&Moratoriums", Nothing, AddressOf mnuMoratoriums_Click)
        mnuUnderwriting.DropDownItems.Add("&Rate Tables", Nothing, AddressOf mnuRateTables_Click)

        ' --- Billing Menu ---
        Dim mnuBilling As New ToolStripMenuItem("&Billing")
        mnuBilling.DropDownItems.Add("Billing &Inquiry", Nothing, AddressOf mnuBillingInquiry_Click)
        mnuBilling.DropDownItems.Add("Record &Payment", Nothing, AddressOf mnuRecordPayment_Click)
        mnuBilling.DropDownItems.Add("&Commission Statement", Nothing, AddressOf mnuCommissionStatement_Click)

        ' --- Admin Menu ---
        Dim mnuAdmin As New ToolStripMenuItem("&Admin")
        mnuAdmin.DropDownItems.Add("&User Management", Nothing, AddressOf mnuUserManagement_Click)
        mnuAdmin.DropDownItems.Add("&System Config", Nothing, AddressOf mnuSystemConfig_Click)
        mnuAdmin.DropDownItems.Add("&Audit Viewer", Nothing, AddressOf mnuAuditViewer_Click)
        mnuAdmin.DropDownItems.Add("&Lookup Maintenance", Nothing, AddressOf mnuLookupMaintenance_Click)

        ' --- Help Menu ---
        Dim mnuHelp As New ToolStripMenuItem("&Help")
        mnuHelp.DropDownItems.Add("&About", Nothing, AddressOf mnuAbout_Click)

        mnuMain.Items.AddRange({mnuPolicy, mnuClaims, mnuUnderwriting, mnuBilling, mnuAdmin, mnuHelp})
        Me.MainMenuStrip = mnuMain
        Me.Controls.Add(mnuMain)
    End Sub

    Private Sub BuildStatusBar()
        lblStatusUser.Text = $"User: {GlobalState.CurrentUser}"
        lblStatusRole.Text = $"Role: {GlobalState.CurrentRole}"
        lblStatusDate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        lblStatusUser.BorderSides = ToolStripStatusLabelBorderSides.Right
        lblStatusRole.BorderSides = ToolStripStatusLabelBorderSides.Right

        statusStrip.Items.AddRange({lblStatusUser, lblStatusRole, lblStatusDate})
        Me.Controls.Add(statusStrip)
    End Sub

    ' --- Menu Handlers ---
    Private Sub mnuCustomerSearch_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmCustomerSearch)()
    End Sub

    Private Sub mnuNewCustomer_Click(sender As Object, e As EventArgs)
        Dim frm As New frmCustomerEntry(0)
        frm.ShowDialog(Me)
    End Sub

    Private Sub mnuPolicySearch_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmPolicySearch)()
    End Sub

    Private Sub mnuNewQuote_Click(sender As Object, e As EventArgs)
        Dim frm As New frmPolicyEntry(0)
        frm.ShowDialog(Me)
    End Sub

    Private Sub mnuClaimSearch_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmClaimSearch)()
    End Sub

    Private Sub mnuNewFNOL_Click(sender As Object, e As EventArgs)
        Dim frm As New frmClaimFNOL()
        frm.ShowDialog(Me)
    End Sub

    Private Sub mnuReferralQueue_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmReferralQueue)()
    End Sub

    Private Sub mnuMoratoriums_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmMoratorium)()
    End Sub

    Private Sub mnuRateTables_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmRateTableMaintenance)()
    End Sub

    Private Sub mnuBillingInquiry_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmBillingInquiry)()
    End Sub

    Private Sub mnuRecordPayment_Click(sender As Object, e As EventArgs)
        Dim frm As New frmPaymentEntry(0)
        frm.ShowDialog(Me)
    End Sub

    Private Sub mnuCommissionStatement_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmCommissionStatement)()
    End Sub

    Private Sub mnuUserManagement_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmUserManagement)()
    End Sub

    Private Sub mnuSystemConfig_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmSystemConfig)()
    End Sub

    Private Sub mnuAuditViewer_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmAuditViewer)()
    End Sub

    Private Sub mnuLookupMaintenance_Click(sender As Object, e As EventArgs)
        OpenChildForm(Of frmLookupMaintenance)()
    End Sub

    Private Sub mnuAbout_Click(sender As Object, e As EventArgs)
        MessageBox.Show($"{AppSettings.ApplicationName}{Environment.NewLine}Version {AppSettings.ApplicationVersion}{Environment.NewLine}{AppSettings.CompanyName}",
                       "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ''' <summary>
    ''' Opens an MDI child form (singleton — reuses if already open).
    ''' </summary>
    Private Sub OpenChildForm(Of T As {Form, New})()
        ' Check if already open
        For Each child As Form In Me.MdiChildren
            If TypeOf child Is T Then
                child.Activate()
                Return
            End If
        Next

        Dim frm As New T()
        frm.MdiParent = Me
        frm.Show()
    End Sub

    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Dim result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        If result = DialogResult.No Then e.Cancel = True
    End Sub

End Class
