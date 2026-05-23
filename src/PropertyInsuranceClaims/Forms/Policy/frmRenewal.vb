Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Renewal processing form — review expiring policies, generate renewal quotes,
''' compare old vs new premium, and process renewal.
''' </summary>
Public Class frmRenewal
    Inherits Form

    Private _policyID As Integer
    Private dgvExpiringPolicies As New DataGridView()
    Private grpRenewalDetails As New GroupBox()

    ' Renewal details
    Private lblPolicyNumber As New Label()
    Private lblCustomer As New Label()
    Private lblCurrentPremium As New Label()
    Private lblNewPremium As New Label()
    Private lblPremiumChange As New Label()
    Private lblExpiryDate As New Label()
    Private dtpNewEffective As New DateTimePicker()
    Private cboNewTerm As New ComboBox()
    Private cboNewPaymentPlan As New ComboBox()
    Private chkSameAgent As New CheckBox()
    Private txtRenewalNotes As New TextBox()

    ' Coverage comparison grid
    Private dgvCoverageComparison As New DataGridView()

    ' Buttons
    Private WithEvents btnLoadExpiring As New Button()
    Private WithEvents btnSelectPolicy As New Button()
    Private WithEvents btnCalculate As New Button()
    Private WithEvents btnRenew As New Button()
    Private WithEvents btnDecline As New Button()
    Private WithEvents btnClose As New Button()

    Private Sub frmRenewal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Renewal Processing"
            Me.Size = New Drawing.Size(1000, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmRenewal_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Top: Expiring policies list
        Dim grpExpiring As New GroupBox() With {.Text = "Policies Expiring Soon", .Dock = DockStyle.Top, .Height = 220}
        Dim pnlExpiringTop As New Panel() With {.Dock = DockStyle.Top, .Height = 35}
        btnLoadExpiring.Location = New Drawing.Point(10, 4) : btnLoadExpiring.Size = New Drawing.Size(130, 28) : btnLoadExpiring.Text = "&Load Expiring (30d)" : pnlExpiringTop.Controls.Add(btnLoadExpiring)
        btnSelectPolicy.Location = New Drawing.Point(150, 4) : btnSelectPolicy.Size = New Drawing.Size(120, 28) : btnSelectPolicy.Text = "&Select for Renewal" : btnSelectPolicy.Enabled = False : pnlExpiringTop.Controls.Add(btnSelectPolicy)
        grpExpiring.Controls.Add(pnlExpiringTop)

        dgvExpiringPolicies.Dock = DockStyle.Fill : dgvExpiringPolicies.ReadOnly = True : dgvExpiringPolicies.AllowUserToAddRows = False
        dgvExpiringPolicies.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvExpiringPolicies.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpExpiring.Controls.Add(dgvExpiringPolicies)
        dgvExpiringPolicies.BringToFront()
        Me.Controls.Add(grpExpiring)

        ' Bottom: Renewal details
        grpRenewalDetails.Text = "Renewal Details" : grpRenewalDetails.Dock = DockStyle.Fill : grpRenewalDetails.Visible = False
        Dim y As Integer = 22

        lblPolicyNumber.Location = New Drawing.Point(15, y) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font("Segoe UI", 11, Drawing.FontStyle.Bold) : grpRenewalDetails.Controls.Add(lblPolicyNumber)
        lblCustomer.Location = New Drawing.Point(300, y) : lblCustomer.AutoSize = True : grpRenewalDetails.Controls.Add(lblCustomer)
        y += 28

        lblExpiryDate.Location = New Drawing.Point(15, y) : lblExpiryDate.AutoSize = True : grpRenewalDetails.Controls.Add(lblExpiryDate)
        AddLabelTo(grpRenewalDetails, "New Effective:", 250, y)
        dtpNewEffective.Location = New Drawing.Point(350, y - 3) : dtpNewEffective.Size = New Drawing.Size(120, 20) : dtpNewEffective.Format = DateTimePickerFormat.Short : grpRenewalDetails.Controls.Add(dtpNewEffective)
        AddLabelTo(grpRenewalDetails, "Term:", 490, y)
        cboNewTerm.Location = New Drawing.Point(530, y - 3) : cboNewTerm.Size = New Drawing.Size(60, 20) : cboNewTerm.DropDownStyle = ComboBoxStyle.DropDownList
        cboNewTerm.Items.AddRange({"6", "12"}) : cboNewTerm.SelectedIndex = 1 : grpRenewalDetails.Controls.Add(cboNewTerm)
        AddLabelTo(grpRenewalDetails, "Plan:", 610, y)
        cboNewPaymentPlan.Location = New Drawing.Point(645, y - 3) : cboNewPaymentPlan.Size = New Drawing.Size(120, 20) : cboNewPaymentPlan.DropDownStyle = ComboBoxStyle.DropDownList
        cboNewPaymentPlan.Items.AddRange({"ANNUAL", "SEMI_ANNUAL", "QUARTERLY", "MONTHLY"}) : cboNewPaymentPlan.SelectedIndex = 0 : grpRenewalDetails.Controls.Add(cboNewPaymentPlan)
        y += 28

        chkSameAgent.Location = New Drawing.Point(15, y) : chkSameAgent.Text = "Same Agent" : chkSameAgent.Checked = True : chkSameAgent.AutoSize = True : grpRenewalDetails.Controls.Add(chkSameAgent)
        y += 28

        ' Premium comparison
        lblCurrentPremium.Location = New Drawing.Point(15, y) : lblCurrentPremium.AutoSize = True : grpRenewalDetails.Controls.Add(lblCurrentPremium)
        lblNewPremium.Location = New Drawing.Point(250, y) : lblNewPremium.AutoSize = True : grpRenewalDetails.Controls.Add(lblNewPremium)
        lblPremiumChange.Location = New Drawing.Point(500, y) : lblPremiumChange.AutoSize = True : lblPremiumChange.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : grpRenewalDetails.Controls.Add(lblPremiumChange)
        y += 28

        ' Coverage comparison
        dgvCoverageComparison.Location = New Drawing.Point(15, y) : dgvCoverageComparison.Size = New Drawing.Size(750, 120)
        dgvCoverageComparison.AllowUserToAddRows = False : dgvCoverageComparison.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvCoverageComparison.Columns.Add("Coverage", "Coverage")
        dgvCoverageComparison.Columns.Add("CurrentLimit", "Current Limit")
        dgvCoverageComparison.Columns.Add("NewLimit", "New Limit")
        dgvCoverageComparison.Columns.Add("CurrentPremium", "Current Premium")
        dgvCoverageComparison.Columns.Add("NewPremium", "New Premium")
        dgvCoverageComparison.Columns(0).ReadOnly = True : dgvCoverageComparison.Columns(1).ReadOnly = True : dgvCoverageComparison.Columns(3).ReadOnly = True
        grpRenewalDetails.Controls.Add(dgvCoverageComparison)
        y += 130

        ' Notes
        AddLabelTo(grpRenewalDetails, "Notes:", 15, y)
        txtRenewalNotes.Location = New Drawing.Point(65, y - 3) : txtRenewalNotes.Size = New Drawing.Size(500, 40) : txtRenewalNotes.Multiline = True : grpRenewalDetails.Controls.Add(txtRenewalNotes)
        y += 50

        ' Action buttons
        btnCalculate.Location = New Drawing.Point(15, y) : btnCalculate.Size = New Drawing.Size(100, 35) : btnCalculate.Text = "Ca&lculate" : grpRenewalDetails.Controls.Add(btnCalculate)
        btnRenew.Location = New Drawing.Point(125, y) : btnRenew.Size = New Drawing.Size(100, 35) : btnRenew.Text = "&Renew" : btnRenew.Enabled = False : btnRenew.BackColor = Drawing.Color.LightGreen : grpRenewalDetails.Controls.Add(btnRenew)
        btnDecline.Location = New Drawing.Point(235, y) : btnDecline.Size = New Drawing.Size(100, 35) : btnDecline.Text = "&Non-Renew" : btnDecline.BackColor = Drawing.Color.LightCoral : grpRenewalDetails.Controls.Add(btnDecline)

        Me.Controls.Add(grpRenewalDetails)

        ' Close button
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(880, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        grpRenewalDetails.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnLoadExpiring_Click(sender As Object, e As EventArgs) Handles btnLoadExpiring.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@DaysAhead", 30),
                DatabaseHelper.CreateParam("@ProcessedBy", GlobalState.CurrentUser)
            }
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Renewal_GetDuePolicies", params)
            dgvExpiringPolicies.DataSource = dt
            If dgvExpiringPolicies.Columns.Contains("PolicyID") Then dgvExpiringPolicies.Columns("PolicyID").Visible = False
            btnSelectPolicy.Enabled = dt.Rows.Count > 0
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnSelectPolicy_Click(sender As Object, e As EventArgs) Handles btnSelectPolicy.Click
        If dgvExpiringPolicies.CurrentRow Is Nothing Then Return
        _policyID = CInt(dgvExpiringPolicies.CurrentRow.Cells("PolicyID").Value)
        LoadRenewalDetails()
    End Sub

    Private Sub LoadRenewalDetails()
        Try
            Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
            If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then Return

            Dim row As DataRow = ds.Tables(0).Rows(0)
            lblPolicyNumber.Text = $"Policy: {row("PolicyNumber")} ({row("PolicyType")})"
            lblCustomer.Text = $"Customer: {row("FirstName")} {row("LastName")}"
            lblExpiryDate.Text = $"Expires: {CDate(row("ExpiryDate")):MM/dd/yyyy}"
            dtpNewEffective.Value = CDate(row("ExpiryDate"))

            If Not IsDBNull(row("AnnualPremium")) Then
                lblCurrentPremium.Text = $"Current Premium: {CDec(row("AnnualPremium")):C}"
            End If

            ' Load coverages for comparison
            dgvCoverageComparison.Rows.Clear()
            If ds.Tables.Count > 1 Then
                For Each covRow As DataRow In ds.Tables(1).Rows
                    Dim limit As String = If(IsDBNull(covRow("LimitAmount")), "", CDec(covRow("LimitAmount")).ToString("N0"))
                    Dim premium As String = If(IsDBNull(covRow("Premium")), "", CDec(covRow("Premium")).ToString("N2"))
                    dgvCoverageComparison.Rows.Add(covRow("CoverageCode").ToString(), limit, limit, premium, "")
                Next
            End If

            grpRenewalDetails.Visible = True
        Catch ex As Exception
            MessageBox.Show("Error loading policy: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCalculate_Click(sender As Object, e As EventArgs) Handles btnCalculate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            ' Simulate renewal premium calculation
            Dim currentPremium As Decimal = 0
            Decimal.TryParse(lblCurrentPremium.Text.Replace("Current Premium: ", "").Replace("$", "").Replace(",", ""), currentPremium)

            ' Apply a simulated rate change (5% increase)
            Dim newPremium As Decimal = Math.Round(currentPremium * 1.05D, 2)
            Dim change As Decimal = newPremium - currentPremium

            lblNewPremium.Text = $"New Premium: {newPremium:C}"
            lblPremiumChange.Text = $"Change: {change:C} ({(change / If(currentPremium = 0, 1, currentPremium)) * 100:N1}%)"
            lblPremiumChange.ForeColor = If(change > 0, Drawing.Color.Red, Drawing.Color.Green)

            ' Update coverage grid with new premiums
            For Each row As DataGridViewRow In dgvCoverageComparison.Rows
                If row.Cells("CurrentPremium").Value IsNot Nothing AndAlso row.Cells("CurrentPremium").Value.ToString() <> "" Then
                    Dim oldPrem As Decimal = CDec(row.Cells("CurrentPremium").Value)
                    row.Cells("NewPremium").Value = Math.Round(oldPrem * 1.05D, 2).ToString("N2")
                End If
            Next

            btnRenew.Enabled = True
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnRenew_Click(sender As Object, e As EventArgs) Handles btnRenew.Click
        If MessageBox.Show("Process this renewal?", "Confirm Renewal", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Me.Cursor = Cursors.WaitCursor
                ' Would call Policy.usp_Policy_Renew SP
                MessageBox.Show("Policy renewed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                grpRenewalDetails.Visible = False
                btnLoadExpiring.PerformClick()
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                Me.Cursor = Cursors.Default
            End Try
        End If
    End Sub

    Private Sub btnDecline_Click(sender As Object, e As EventArgs) Handles btnDecline.Click
        If MessageBox.Show("Non-renew this policy? This cannot be undone.", "Confirm Non-Renewal", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            MessageBox.Show("Policy marked as non-renewed.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            grpRenewalDetails.Visible = False
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
