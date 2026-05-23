Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Individual underwriting referral review form — shows policy details, rule triggers,
''' and allows detailed decision with conditions.
''' </summary>
Public Class frmUnderwritingReferral
    Inherits Form

    Private _referralID As Integer
    Private _policyID As Integer

    ' Header
    Private lblReferralReason As New Label()
    Private lblReferralDate As New Label()
    Private lblSeverity As New Label()

    ' Policy summary
    Private grpPolicy As New GroupBox()
    Private lblPolicyNumber As New Label()
    Private lblPolicyType As New Label()
    Private lblCustomer As New Label()
    Private lblProperty As New Label()
    Private lblPremium As New Label()
    Private lblTIV As New Label()

    ' Risk factors
    Private dgvRiskFactors As New DataGridView()

    ' Decision
    Private grpDecision As New GroupBox()
    Private cboDecision As New ComboBox()
    Private txtDecisionNotes As New TextBox()
    Private txtConditions As New TextBox()
    Private dtpExpiryDate As New DateTimePicker()
    Private chkHasExpiry As New CheckBox()

    ' Buttons
    Private WithEvents btnViewPolicy As New Button()
    Private WithEvents btnViewWorksheet As New Button()
    Private WithEvents btnSubmitDecision As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(referralID As Integer, policyID As Integer)
        _referralID = referralID
        _policyID = policyID
    End Sub

    Private Sub frmUnderwritingReferral_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Underwriting Referral Review"
            Me.Size = New Drawing.Size(850, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadReferralData()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmUnderwritingReferral_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 10

        ' Referral header
        lblReferralReason.Location = New Drawing.Point(10, y) : lblReferralReason.AutoSize = True : lblReferralReason.Font = New Drawing.Font("Segoe UI", 11, Drawing.FontStyle.Bold) : Me.Controls.Add(lblReferralReason)
        y += 25
        lblReferralDate.Location = New Drawing.Point(10, y) : lblReferralDate.AutoSize = True : lblReferralDate.ForeColor = Drawing.Color.Gray : Me.Controls.Add(lblReferralDate)
        lblSeverity.Location = New Drawing.Point(250, y) : lblSeverity.AutoSize = True : Me.Controls.Add(lblSeverity)
        y += 25

        ' Policy summary
        grpPolicy.Text = "Policy Summary" : grpPolicy.Location = New Drawing.Point(10, y) : grpPolicy.Size = New Drawing.Size(810, 100)
        lblPolicyNumber.Location = New Drawing.Point(15, 20) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : grpPolicy.Controls.Add(lblPolicyNumber)
        lblPolicyType.Location = New Drawing.Point(200, 20) : lblPolicyType.AutoSize = True : grpPolicy.Controls.Add(lblPolicyType)
        lblCustomer.Location = New Drawing.Point(15, 42) : lblCustomer.AutoSize = True : grpPolicy.Controls.Add(lblCustomer)
        lblProperty.Location = New Drawing.Point(15, 62) : lblProperty.AutoSize = True : grpPolicy.Controls.Add(lblProperty)
        lblPremium.Location = New Drawing.Point(500, 20) : lblPremium.AutoSize = True : grpPolicy.Controls.Add(lblPremium)
        lblTIV.Location = New Drawing.Point(500, 42) : lblTIV.AutoSize = True : grpPolicy.Controls.Add(lblTIV)
        btnViewPolicy.Location = New Drawing.Point(650, 60) : btnViewPolicy.Size = New Drawing.Size(100, 25) : btnViewPolicy.Text = "View Policy" : grpPolicy.Controls.Add(btnViewPolicy)
        btnViewWorksheet.Location = New Drawing.Point(500, 60) : btnViewWorksheet.Size = New Drawing.Size(130, 25) : btnViewWorksheet.Text = "Rating Worksheet" : grpPolicy.Controls.Add(btnViewWorksheet)
        Me.Controls.Add(grpPolicy)
        y += 110

        ' Risk factors grid
        Dim grpRisk As New GroupBox() With {.Text = "Triggered Rules / Risk Factors", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(810, 180)}
        dgvRiskFactors.Dock = DockStyle.Fill : dgvRiskFactors.ReadOnly = True : dgvRiskFactors.AllowUserToAddRows = False
        dgvRiskFactors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpRisk.Controls.Add(dgvRiskFactors)
        Me.Controls.Add(grpRisk)
        y += 190

        ' Decision panel
        grpDecision.Text = "Decision" : grpDecision.Location = New Drawing.Point(10, y) : grpDecision.Size = New Drawing.Size(810, 220)
        Dim dy As Integer = 22
        AddLabelTo(grpDecision, "Decision:", 15, dy)
        cboDecision.Location = New Drawing.Point(100, dy - 3) : cboDecision.Size = New Drawing.Size(150, 20) : cboDecision.DropDownStyle = ComboBoxStyle.DropDownList
        cboDecision.Items.AddRange({"APPROVED", "DECLINED", "CONDITIONAL"}) : cboDecision.SelectedIndex = 0
        AddHandler cboDecision.SelectedIndexChanged, AddressOf DecisionChanged
        grpDecision.Controls.Add(cboDecision)
        dy += 30

        AddLabelTo(grpDecision, "Notes:", 15, dy)
        txtDecisionNotes.Location = New Drawing.Point(100, dy - 3) : txtDecisionNotes.Size = New Drawing.Size(600, 50) : txtDecisionNotes.Multiline = True : txtDecisionNotes.ScrollBars = ScrollBars.Vertical
        grpDecision.Controls.Add(txtDecisionNotes)
        dy += 58

        AddLabelTo(grpDecision, "Conditions:", 15, dy)
        txtConditions.Location = New Drawing.Point(100, dy - 3) : txtConditions.Size = New Drawing.Size(600, 40) : txtConditions.Multiline = True : txtConditions.Enabled = False
        grpDecision.Controls.Add(txtConditions)
        dy += 48

        chkHasExpiry.Location = New Drawing.Point(15, dy) : chkHasExpiry.Text = "Approval Expiry:" : chkHasExpiry.AutoSize = True : grpDecision.Controls.Add(chkHasExpiry)
        dtpExpiryDate.Location = New Drawing.Point(150, dy - 2) : dtpExpiryDate.Size = New Drawing.Size(130, 20) : dtpExpiryDate.Format = DateTimePickerFormat.Short : dtpExpiryDate.Enabled = False
        dtpExpiryDate.Value = DateTime.Today.AddDays(30)
        AddHandler chkHasExpiry.CheckedChanged, Sub() dtpExpiryDate.Enabled = chkHasExpiry.Checked
        grpDecision.Controls.Add(dtpExpiryDate)
        dy += 30

        btnSubmitDecision.Location = New Drawing.Point(100, dy) : btnSubmitDecision.Size = New Drawing.Size(130, 35) : btnSubmitDecision.Text = "&Submit Decision" : grpDecision.Controls.Add(btnSubmitDecision)
        Me.Controls.Add(grpDecision)
        y += 230

        ' Close button
        btnClose.Location = New Drawing.Point(740, y) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub DecisionChanged(sender As Object, e As EventArgs)
        txtConditions.Enabled = (cboDecision.SelectedItem IsNot Nothing AndAlso cboDecision.SelectedItem.ToString() = "CONDITIONAL")
    End Sub

    Private Sub LoadReferralData()
        Try
            ' Load referral details
            Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@PolicyID", _policyID)}
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Underwriting.usp_Referral_GetByPolicy", params)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                lblReferralReason.Text = row("ReferralReason").ToString()
                lblReferralDate.Text = $"Referred: {CDate(row("ReferralDate")):MM/dd/yyyy HH:mm}"
                If dt.Columns.Contains("Severity") AndAlso Not IsDBNull(row("Severity")) Then
                    lblSeverity.Text = $"Severity: {row("Severity")}"
                    lblSeverity.ForeColor = If(row("Severity").ToString() = "CRITICAL", Drawing.Color.Red, If(row("Severity").ToString() = "HIGH", Drawing.Color.DarkOrange, Drawing.Color.Black))
                End If
            End If

            ' Load policy details
            Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                Dim pRow As DataRow = ds.Tables(0).Rows(0)
                lblPolicyNumber.Text = pRow("PolicyNumber").ToString()
                lblPolicyType.Text = $"Type: {pRow("PolicyType")}"
                lblCustomer.Text = $"Customer: {pRow("FirstName")} {pRow("LastName")}"
                lblProperty.Text = $"Property: {pRow("PropertyAddress")}, {pRow("PropertyCity")} {pRow("PropertyState")}"
                If Not IsDBNull(pRow("AnnualPremium")) Then lblPremium.Text = $"Premium: {CDec(pRow("AnnualPremium")):C}"
                If Not IsDBNull(pRow("TotalInsuredValue")) Then lblTIV.Text = $"TIV: {CDec(pRow("TotalInsuredValue")):C}"
            End If

            ' Load triggered rules
            Dim rulesDt As DataTable = UnderwritingDataAccess.EvaluateRules(_policyID)
            dgvRiskFactors.DataSource = rulesDt
        Catch ex As Exception
            MessageBox.Show("Error loading data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnViewPolicy_Click(sender As Object, e As EventArgs) Handles btnViewPolicy.Click
        Dim frm As New frmPolicyView(_policyID)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnViewWorksheet_Click(sender As Object, e As EventArgs) Handles btnViewWorksheet.Click
        Dim frm As New frmQuoteWorksheet(_policyID)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnSubmitDecision_Click(sender As Object, e As EventArgs) Handles btnSubmitDecision.Click
        Try
            Dim decision As String = cboDecision.SelectedItem.ToString()

            If decision = "DECLINED" Then
                If MessageBox.Show("Are you sure you want to DECLINE this referral? The policy will be declined.", "Confirm Decline", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then Return
            End If

            If decision = "CONDITIONAL" AndAlso String.IsNullOrWhiteSpace(txtConditions.Text) Then
                MessageBox.Show("Please enter conditions for conditional approval.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            UnderwritingDataAccess.ProcessReferral(_referralID, decision, txtDecisionNotes.Text.Trim(), txtConditions.Text.Trim())
            MessageBox.Show($"Referral {decision}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSubmitDecision_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
