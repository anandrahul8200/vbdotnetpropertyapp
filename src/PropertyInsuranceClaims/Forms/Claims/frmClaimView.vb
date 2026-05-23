Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Claim detail view with tabbed display matching the 7 result sets from usp_Claim_GetDetails.
''' </summary>
Public Class frmClaimView
    Inherits Form

    Private _claimID As Integer
    Private tabControl As New TabControl()
    Private tabSummary As New TabPage("Summary")
    Private tabCoverages As New TabPage("Coverages")
    Private tabReserves As New TabPage("Reserves")
    Private tabPayments As New TabPage("Payments")
    Private tabActivities As New TabPage("Activities")
    Private tabHistory As New TabPage("Status History")
    Private tabAssignments As New TabPage("Assignments")

    ' Header
    Private lblClaimNumber As New Label()
    Private lblStatus As New Label()
    Private lblType As New Label()
    Private lblCustomer As New Label()
    Private lblPolicy As New Label()
    Private lblLossDate As New Label()
    Private lblPriority As New Label()
    Private lblIncurred As New Label()
    Private lblFraudScore As New Label()

    ' Grids
    Private dgvCoverages As New DataGridView()
    Private dgvReserves As New DataGridView()
    Private dgvPayments As New DataGridView()
    Private dgvActivities As New DataGridView()
    Private dgvHistory As New DataGridView()
    Private dgvAssignments As New DataGridView()

    ' Action buttons
    Private WithEvents btnSetReserve As New Button()
    Private WithEvents btnMakePayment As New Button()
    Private WithEvents btnAddActivity As New Button()
    Private WithEvents btnChangeStatus As New Button()
    Private WithEvents btnAssign As New Button()
    Private WithEvents btnFraudReview As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmClaimView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Claim View"
            Me.Size = New Drawing.Size(1000, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadData()
        Catch ex As Exception
            MessageBox.Show("Error loading claim: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimView_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Header
        Dim pnlHeader As New Panel() With {.Dock = DockStyle.Top, .Height = 95}
        lblClaimNumber.Location = New Drawing.Point(10, 8) : lblClaimNumber.AutoSize = True : lblClaimNumber.Font = New Drawing.Font("Segoe UI", 14, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblClaimNumber)
        lblStatus.Location = New Drawing.Point(220, 12) : lblStatus.AutoSize = True : pnlHeader.Controls.Add(lblStatus)
        lblType.Location = New Drawing.Point(350, 12) : lblType.AutoSize = True : pnlHeader.Controls.Add(lblType)
        lblPriority.Location = New Drawing.Point(520, 12) : lblPriority.AutoSize = True : pnlHeader.Controls.Add(lblPriority)
        lblCustomer.Location = New Drawing.Point(10, 35) : lblCustomer.AutoSize = True : pnlHeader.Controls.Add(lblCustomer)
        lblPolicy.Location = New Drawing.Point(350, 35) : lblPolicy.AutoSize = True : pnlHeader.Controls.Add(lblPolicy)
        lblLossDate.Location = New Drawing.Point(10, 55) : lblLossDate.AutoSize = True : pnlHeader.Controls.Add(lblLossDate)
        lblIncurred.Location = New Drawing.Point(250, 55) : lblIncurred.AutoSize = True : lblIncurred.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblIncurred)
        lblFraudScore.Location = New Drawing.Point(500, 55) : lblFraudScore.AutoSize = True : pnlHeader.Controls.Add(lblFraudScore)
        Me.Controls.Add(pnlHeader)

        ' Tabs
        tabControl.Dock = DockStyle.Fill
        ConfigureGrid(dgvCoverages) : tabCoverages.Controls.Add(dgvCoverages)
        ConfigureGrid(dgvReserves) : tabReserves.Controls.Add(dgvReserves)
        ConfigureGrid(dgvPayments) : tabPayments.Controls.Add(dgvPayments)
        ConfigureGrid(dgvActivities) : tabActivities.Controls.Add(dgvActivities)
        ConfigureGrid(dgvHistory) : tabHistory.Controls.Add(dgvHistory)
        ConfigureGrid(dgvAssignments) : tabAssignments.Controls.Add(dgvAssignments)
        tabControl.TabPages.AddRange({tabSummary, tabCoverages, tabReserves, tabPayments, tabActivities, tabHistory, tabAssignments})
        Me.Controls.Add(tabControl)

        ' Bottom buttons
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnChangeStatus.Location = New Drawing.Point(10, 8) : btnChangeStatus.Size = New Drawing.Size(110, 30) : btnChangeStatus.Text = "Change &Status" : pnlBottom.Controls.Add(btnChangeStatus)
        btnSetReserve.Location = New Drawing.Point(130, 8) : btnSetReserve.Size = New Drawing.Size(100, 30) : btnSetReserve.Text = "Set &Reserve" : pnlBottom.Controls.Add(btnSetReserve)
        btnMakePayment.Location = New Drawing.Point(240, 8) : btnMakePayment.Size = New Drawing.Size(100, 30) : btnMakePayment.Text = "&Payment" : pnlBottom.Controls.Add(btnMakePayment)
        btnAddActivity.Location = New Drawing.Point(350, 8) : btnAddActivity.Size = New Drawing.Size(100, 30) : btnAddActivity.Text = "&Activity" : pnlBottom.Controls.Add(btnAddActivity)
        btnAssign.Location = New Drawing.Point(460, 8) : btnAssign.Size = New Drawing.Size(80, 30) : btnAssign.Text = "Assi&gn" : pnlBottom.Controls.Add(btnAssign)
        btnFraudReview.Location = New Drawing.Point(550, 8) : btnFraudReview.Size = New Drawing.Size(100, 30) : btnFraudReview.Text = "&Fraud Review" : pnlBottom.Controls.Add(btnFraudReview)
        btnClose.Location = New Drawing.Point(860, 8) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "C&lose" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        tabControl.BringToFront()
    End Sub

    Private Sub ConfigureGrid(dgv As DataGridView)
        dgv.Dock = DockStyle.Fill : dgv.ReadOnly = True : dgv.AllowUserToAddRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    Private Sub LoadData()
        Dim ds As DataSet = ClaimDataAccess.GetDetails(_claimID)
        If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
            MessageBox.Show("Claim not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close() : Return
        End If

        Dim row As DataRow = ds.Tables(0).Rows(0)
        lblClaimNumber.Text = row("ClaimNumber").ToString()
        lblStatus.Text = $"Status: {row("ClaimStatus")}"
        lblType.Text = $"Type: {row("ClaimType")}"
        lblPriority.Text = $"Priority: {row("Priority")}"
        lblCustomer.Text = $"Customer: {row("FirstName")} {row("LastName")}"
        lblPolicy.Text = $"Policy: {row("PolicyNumber")}"
        lblLossDate.Text = $"Loss Date: {CDate(row("LossDate")):MM/dd/yyyy}"
        If Not IsDBNull(row("NetIncurred")) Then lblIncurred.Text = $"Net Incurred: {CDec(row("NetIncurred")):C}"
        If Not IsDBNull(row("FraudScore")) Then lblFraudScore.Text = $"Fraud Score: {CDec(row("FraudScore")):N1}/100"

        ' Bind grids to result sets
        If ds.Tables.Count > 1 Then dgvCoverages.DataSource = ds.Tables(1)
        If ds.Tables.Count > 2 Then dgvReserves.DataSource = ds.Tables(2)
        If ds.Tables.Count > 3 Then dgvPayments.DataSource = ds.Tables(3)
        If ds.Tables.Count > 4 Then dgvActivities.DataSource = ds.Tables(4)
        If ds.Tables.Count > 5 Then dgvHistory.DataSource = ds.Tables(5)
        If ds.Tables.Count > 6 Then dgvAssignments.DataSource = ds.Tables(6)
    End Sub

    Private Sub btnChangeStatus_Click(sender As Object, e As EventArgs) Handles btnChangeStatus.Click
        Dim frm As New frmClaimStatusChange(_claimID, lblStatus.Text.Replace("Status: ", ""))
        If frm.ShowDialog(Me) = DialogResult.OK Then LoadData()
    End Sub

    Private Sub btnSetReserve_Click(sender As Object, e As EventArgs) Handles btnSetReserve.Click
        Dim frm As New frmClaimReserve(_claimID)
        If frm.ShowDialog(Me) = DialogResult.OK Then LoadData()
    End Sub

    Private Sub btnMakePayment_Click(sender As Object, e As EventArgs) Handles btnMakePayment.Click
        Dim frm As New frmClaimPayment(_claimID)
        If frm.ShowDialog(Me) = DialogResult.OK Then LoadData()
    End Sub

    Private Sub btnAddActivity_Click(sender As Object, e As EventArgs) Handles btnAddActivity.Click
        Dim frm As New frmClaimActivity(_claimID)
        If frm.ShowDialog(Me) = DialogResult.OK Then LoadData()
    End Sub

    Private Sub btnAssign_Click(sender As Object, e As EventArgs) Handles btnAssign.Click
        MessageBox.Show("Assignment form would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnFraudReview_Click(sender As Object, e As EventArgs) Handles btnFraudReview.Click
        MessageBox.Show("Fraud review form would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
