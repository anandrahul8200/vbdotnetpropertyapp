Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Policy entry form — create/edit policy with coverages, prior carrier info, and premium calculation.
''' </summary>
Public Class frmPolicyEntry
    Inherits Form

    Private _policyID As Integer = 0
    Private _isEditMode As Boolean = False
    Private _isDirty As Boolean = False

    ' --- Header Controls ---
    Private lblPolicyNumber As New Label()
    Private lblPolicyStatus As New Label()
    Private cboPolicyType As New ComboBox()
    Private WithEvents btnSelectCustomer As New Button()
    Private lblCustomerName As New Label()
    Private _customerID As Integer = 0
    Private WithEvents btnSelectProperty As New Button()
    Private lblPropertyAddress As New Label()
    Private _propertyID As Integer = 0
    Private cboAgent As New ComboBox()
    Private dtpEffectiveDate As New DateTimePicker()
    Private cboTermMonths As New ComboBox()
    Private cboPaymentPlan As New ComboBox()

    ' --- Prior Carrier ---
    Private txtPriorCarrier As New TextBox()
    Private txtPriorPolicyNumber As New TextBox()
    Private txtClaimFreeYears As New TextBox()

    ' --- Coverages Grid ---
    Private dgvCoverages As New DataGridView()

    ' --- Totals ---
    Private lblTotalPremium As New Label()
    Private lblTotalTaxes As New Label()
    Private lblGrossPremium As New Label()

    ' --- Buttons ---
    Private WithEvents btnCalculate As New Button()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnBind As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmPolicyEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = If(_policyID > 0, "Edit Policy", "New Policy Quote")
            Me.Size = New Drawing.Size(800, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False

            InitializeControls()
            LoadLookups()

            If _policyID > 0 Then
                LoadData()
                _isEditMode = True
            End If
            _isDirty = False
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPolicyEntry_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15

        ' Policy header
        Dim grpHeader As New GroupBox() With {.Text = "Policy Information", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 160)}
        Dim hy As Integer = 22

        AddLabelTo(grpHeader, "Policy #:", 10, hy)
        lblPolicyNumber.Location = New Drawing.Point(90, hy) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : grpHeader.Controls.Add(lblPolicyNumber)
        lblPolicyStatus.Location = New Drawing.Point(250, hy) : lblPolicyStatus.AutoSize = True : grpHeader.Controls.Add(lblPolicyStatus)
        hy += 28

        AddLabelTo(grpHeader, "Type:", 10, hy)
        cboPolicyType.Location = New Drawing.Point(90, hy - 3) : cboPolicyType.Size = New Drawing.Size(100, 20) : cboPolicyType.DropDownStyle = ComboBoxStyle.DropDownList : grpHeader.Controls.Add(cboPolicyType)
        AddLabelTo(grpHeader, "Effective:", 220, hy)
        dtpEffectiveDate.Location = New Drawing.Point(290, hy - 3) : dtpEffectiveDate.Size = New Drawing.Size(120, 20) : dtpEffectiveDate.Format = DateTimePickerFormat.Short : grpHeader.Controls.Add(dtpEffectiveDate)
        AddLabelTo(grpHeader, "Term:", 430, hy)
        cboTermMonths.Location = New Drawing.Point(475, hy - 3) : cboTermMonths.Size = New Drawing.Size(60, 20) : cboTermMonths.DropDownStyle = ComboBoxStyle.DropDownList : grpHeader.Controls.Add(cboTermMonths)
        AddLabelTo(grpHeader, "Payment:", 560, hy)
        cboPaymentPlan.Location = New Drawing.Point(625, hy - 3) : cboPaymentPlan.Size = New Drawing.Size(110, 20) : cboPaymentPlan.DropDownStyle = ComboBoxStyle.DropDownList : grpHeader.Controls.Add(cboPaymentPlan)
        hy += 28

        AddLabelTo(grpHeader, "Customer:", 10, hy)
        btnSelectCustomer.Location = New Drawing.Point(90, hy - 4) : btnSelectCustomer.Size = New Drawing.Size(80, 23) : btnSelectCustomer.Text = "Select..." : grpHeader.Controls.Add(btnSelectCustomer)
        lblCustomerName.Location = New Drawing.Point(180, hy) : lblCustomerName.AutoSize = True : grpHeader.Controls.Add(lblCustomerName)
        hy += 28

        AddLabelTo(grpHeader, "Property:", 10, hy)
        btnSelectProperty.Location = New Drawing.Point(90, hy - 4) : btnSelectProperty.Size = New Drawing.Size(80, 23) : btnSelectProperty.Text = "Select..." : grpHeader.Controls.Add(btnSelectProperty)
        lblPropertyAddress.Location = New Drawing.Point(180, hy) : lblPropertyAddress.AutoSize = True : grpHeader.Controls.Add(lblPropertyAddress)
        hy += 28

        AddLabelTo(grpHeader, "Agent:", 10, hy)
        cboAgent.Location = New Drawing.Point(90, hy - 3) : cboAgent.Size = New Drawing.Size(250, 20) : cboAgent.DropDownStyle = ComboBoxStyle.DropDownList : grpHeader.Controls.Add(cboAgent)

        Me.Controls.Add(grpHeader)
        y += 170

        ' Prior carrier
        Dim grpPrior As New GroupBox() With {.Text = "Prior Insurance", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 55)}
        AddLabelTo(grpPrior, "Carrier:", 10, 22)
        txtPriorCarrier.Location = New Drawing.Point(65, 19) : txtPriorCarrier.Size = New Drawing.Size(180, 20) : grpPrior.Controls.Add(txtPriorCarrier)
        AddLabelTo(grpPrior, "Policy #:", 260, 22)
        txtPriorPolicyNumber.Location = New Drawing.Point(315, 19) : txtPriorPolicyNumber.Size = New Drawing.Size(130, 20) : grpPrior.Controls.Add(txtPriorPolicyNumber)
        AddLabelTo(grpPrior, "Claim-Free Yrs:", 470, 22)
        txtClaimFreeYears.Location = New Drawing.Point(570, 19) : txtClaimFreeYears.Size = New Drawing.Size(40, 20) : grpPrior.Controls.Add(txtClaimFreeYears)
        Me.Controls.Add(grpPrior)
        y += 65

        ' Coverages grid
        Dim grpCoverages As New GroupBox() With {.Text = "Coverages", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 220)}
        dgvCoverages.Location = New Drawing.Point(10, 20) : dgvCoverages.Size = New Drawing.Size(740, 190)
        dgvCoverages.AllowUserToAddRows = False : dgvCoverages.AllowUserToDeleteRows = False
        dgvCoverages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill

        ' Add columns
        dgvCoverages.Columns.Add(New DataGridViewCheckBoxColumn() With {.Name = "Selected", .HeaderText = "Sel", .Width = 40})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CoverageCode", .HeaderText = "Code", .Width = 80, .ReadOnly = True})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "CoverageName", .HeaderText = "Coverage", .Width = 200, .ReadOnly = True})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "LimitAmount", .HeaderText = "Limit", .Width = 100})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "DeductibleAmount", .HeaderText = "Deductible", .Width = 80})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Premium", .HeaderText = "Premium", .Width = 80, .ReadOnly = True})

        ' Add default coverages
        dgvCoverages.Rows.Add(True, "DWELLING", "Dwelling (Coverage A)", "250000", "1000", "")
        dgvCoverages.Rows.Add(True, "OTHER_STRUCTURES", "Other Structures (Cov B)", "25000", "1000", "")
        dgvCoverages.Rows.Add(True, "PERSONAL_PROPERTY", "Personal Property (Cov C)", "125000", "1000", "")
        dgvCoverages.Rows.Add(True, "LOSS_OF_USE", "Loss of Use (Cov D)", "50000", "0", "")
        dgvCoverages.Rows.Add(True, "LIABILITY", "Personal Liability (Cov E)", "100000", "0", "")
        dgvCoverages.Rows.Add(True, "MEDICAL", "Medical Payments (Cov F)", "5000", "0", "")
        dgvCoverages.Rows.Add(False, "FLOOD", "Flood Coverage", "250000", "5000", "")
        dgvCoverages.Rows.Add(False, "EARTHQUAKE", "Earthquake Coverage", "250000", "10000", "")
        dgvCoverages.Rows.Add(False, "SEWER_BACKUP", "Sewer/Drain Backup", "10000", "500", "")
        dgvCoverages.Rows.Add(False, "JEWELRY", "Scheduled Jewelry", "10000", "0", "")

        grpCoverages.Controls.Add(dgvCoverages)
        Me.Controls.Add(grpCoverages)
        y += 230

        ' Totals and buttons
        Dim grpTotals As New GroupBox() With {.Text = "Premium Summary", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(400, 80)}
        AddLabelTo(grpTotals, "Premium:", 10, 22) : lblTotalPremium.Location = New Drawing.Point(100, 22) : lblTotalPremium.AutoSize = True : lblTotalPremium.Text = "$0.00" : grpTotals.Controls.Add(lblTotalPremium)
        AddLabelTo(grpTotals, "Taxes/Fees:", 10, 44) : lblTotalTaxes.Location = New Drawing.Point(100, 44) : lblTotalTaxes.AutoSize = True : lblTotalTaxes.Text = "$0.00" : grpTotals.Controls.Add(lblTotalTaxes)
        AddLabelTo(grpTotals, "Total:", 200, 22) : lblGrossPremium.Location = New Drawing.Point(260, 22) : lblGrossPremium.AutoSize = True : lblGrossPremium.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblGrossPremium.Text = "$0.00" : grpTotals.Controls.Add(lblGrossPremium)
        Me.Controls.Add(grpTotals)

        btnCalculate.Location = New Drawing.Point(430, y + 10) : btnCalculate.Size = New Drawing.Size(100, 35) : btnCalculate.Text = "Ca&lculate" : Me.Controls.Add(btnCalculate)
        btnSave.Location = New Drawing.Point(540, y + 10) : btnSave.Size = New Drawing.Size(80, 35) : btnSave.Text = "&Save" : Me.Controls.Add(btnSave)
        btnBind.Location = New Drawing.Point(630, y + 10) : btnBind.Size = New Drawing.Size(80, 35) : btnBind.Text = "&Bind" : btnBind.Enabled = False : Me.Controls.Add(btnBind)
        btnCancel.Location = New Drawing.Point(430, y + 50) : btnCancel.Size = New Drawing.Size(80, 30) : btnCancel.Text = "Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadLookups()
        cboPolicyType.Items.AddRange({"HO3", "HO4", "HO6", "DP3", "BOP"}) : cboPolicyType.SelectedIndex = 0
        cboTermMonths.Items.AddRange({"6", "12"}) : cboTermMonths.SelectedIndex = 1
        cboPaymentPlan.Items.AddRange({"ANNUAL", "SEMI_ANNUAL", "QUARTERLY", "MONTHLY"}) : cboPaymentPlan.SelectedIndex = 0

        ' Load agents from database
        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Policy.usp_Agent_Search", Nothing)
            cboAgent.Items.Clear()
            cboAgent.Items.Add("(Select Agent)")
            For Each row As DataRow In dt.Rows
                cboAgent.Items.Add(row("FirstName").ToString() & " " & row("LastName").ToString())
            Next
            cboAgent.SelectedIndex = 0
        Catch ex As Exception
            cboAgent.Items.Clear()
            cboAgent.Items.Add("(Select Agent)")
            cboAgent.SelectedIndex = 0
        End Try
    End Sub

    Private Sub LoadData()
        Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
        If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then Return
        Dim row As DataRow = ds.Tables(0).Rows(0)
        lblPolicyNumber.Text = row("PolicyNumber").ToString()
        lblPolicyStatus.Text = $"[{row("PolicyStatus")}]"
        _customerID = CInt(row("CustomerID"))
        _propertyID = CInt(row("PropertyID"))
        lblCustomerName.Text = $"{row("FirstName")} {row("LastName")}"
        lblPropertyAddress.Text = $"{row("PropertyAddress")}, {row("PropertyCity")} {row("PropertyState")}"
    End Sub

    Private Sub btnSelectCustomer_Click(sender As Object, e As EventArgs) Handles btnSelectCustomer.Click
        Dim frm As New frmCustomerSearch()
        frm.ShowDialog(Me)
        If frm.SelectedCustomerID > 0 Then
            _customerID = frm.SelectedCustomerID
            lblCustomerName.Text = "(Customer ID: " & _customerID.ToString() & " selected)"
            _isDirty = True
        End If
    End Sub

    Private Sub btnSelectProperty_Click(sender As Object, e As EventArgs) Handles btnSelectProperty.Click
        If _customerID = 0 Then
            MessageBox.Show("Please select a customer first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim frm As New frmPropertySearch(_customerID)
        frm.ShowDialog(Me)
        If frm.SelectedPropertyID > 0 Then
            _propertyID = frm.SelectedPropertyID
            lblPropertyAddress.Text = frm.SelectedPropertyAddress
            _isDirty = True
        End If
    End Sub

    Private Sub btnCalculate_Click(sender As Object, e As EventArgs) Handles btnCalculate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            ' In production, this would call Underwriting.usp_Premium_Calculate
            ' For now, simulate
            Dim totalPremium As Decimal = 0
            For Each row As DataGridViewRow In dgvCoverages.Rows
                If CBool(row.Cells("Selected").Value) Then
                    Dim limit As Decimal = CDec(row.Cells("LimitAmount").Value)
                    Dim simPremium As Decimal = Math.Round(limit * 0.004D, 2) ' Simplified rate
                    row.Cells("Premium").Value = simPremium.ToString("N2")
                    totalPremium += simPremium
                End If
            Next
            Dim taxes As Decimal = Math.Round(totalPremium * 0.035D, 2)
            lblTotalPremium.Text = totalPremium.ToString("C")
            lblTotalTaxes.Text = taxes.ToString("C")
            lblGrossPremium.Text = (totalPremium + taxes).ToString("C")
            btnBind.Enabled = True
        Catch ex As Exception
            MessageBox.Show("Calculation error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnCalculate_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If Not ValidateForm() Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim dto As New PolicyDTO() With {
                .PolicyID = _policyID,
                .PolicyType = cboPolicyType.SelectedItem.ToString(),
                .CustomerID = _customerID,
                .PropertyID = _propertyID,
                .AgentID = 1,
                .EffectiveDate = dtpEffectiveDate.Value.Date,
                .TermMonths = CInt(cboTermMonths.SelectedItem.ToString()),
                .PaymentPlan = cboPaymentPlan.SelectedItem.ToString(),
                .PriorCarrier = txtPriorCarrier.Text.Trim(),
                .PriorPolicyNumber = txtPriorPolicyNumber.Text.Trim(),
                .ClaimFreeYears = If(String.IsNullOrEmpty(txtClaimFreeYears.Text), 0, CInt(txtClaimFreeYears.Text))
            }

            If Not _isEditMode Then
                PolicyDataAccess.CreateQuote(dto)
                _policyID = dto.PolicyID
                lblPolicyNumber.Text = dto.PolicyNumber
                lblPolicyStatus.Text = "[QUOTE]"
                _isEditMode = True
            End If

            ' Save coverages to database
            For Each row As DataGridViewRow In dgvCoverages.Rows
                Dim isSelected As Boolean = CBool(row.Cells("Selected").Value)
                Dim code As String = row.Cells("CoverageCode").Value.ToString()
                Dim name As String = row.Cells("CoverageName").Value.ToString()
                Dim limit As Decimal = CDec(row.Cells("LimitAmount").Value)
                Dim deductible As Decimal = CDec(row.Cells("DeductibleAmount").Value)
                Dim premium As Decimal = 0
                If Not String.IsNullOrEmpty(row.Cells("Premium").Value?.ToString()) Then
                    premium = CDec(row.Cells("Premium").Value)
                End If

                Dim params() As SqlParameter = {
                    New SqlParameter("@PolicyID", _policyID),
                    New SqlParameter("@CoverageCode", code),
                    New SqlParameter("@CoverageName", name),
                    New SqlParameter("@LimitAmount", limit),
                    New SqlParameter("@DeductibleAmount", deductible),
                    New SqlParameter("@Premium", premium),
                    New SqlParameter("@IsSelected", isSelected),
                    New SqlParameter("@CreatedBy", GlobalState.CurrentUser)
                }
                DatabaseHelper.ExecuteNonQuery("Policy.usp_Policy_SaveCoverage", params)
            Next

            ' Save premium totals
            Dim annualPremium As Decimal = 0
            Dim totalFees As Decimal = 0
            Decimal.TryParse(lblTotalPremium.Text.Replace("$", "").Replace(",", ""), annualPremium)
            Decimal.TryParse(lblTotalTaxes.Text.Replace("$", "").Replace(",", ""), totalFees)
            Dim grossPremium As Decimal = annualPremium + totalFees

            Dim premParams() As SqlParameter = {
                New SqlParameter("@PolicyID", _policyID),
                New SqlParameter("@AnnualPremium", annualPremium),
                New SqlParameter("@TotalFees", totalFees),
                New SqlParameter("@GrossPremium", grossPremium),
                New SqlParameter("@ModifiedBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Policy.usp_Policy_UpdatePremium", premParams)

            _isDirty = False
            MessageBox.Show("Policy saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error saving: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnBind_Click(sender As Object, e As EventArgs) Handles btnBind.Click
        If MessageBox.Show("Bind this policy? This will make it active.", "Confirm Bind", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Dim params() As SqlParameter = {
                    New SqlParameter("@PolicyID", _policyID),
                    New SqlParameter("@ModifiedBy", GlobalState.CurrentUser)
                }
                DatabaseHelper.ExecuteNonQuery("Policy.usp_Policy_Bind", params)
                lblPolicyStatus.Text = "[ACTIVE]"
                MessageBox.Show("Policy bound successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show("Error binding policy: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                ErrorLogger.LogError(ex, "btnBind_Click")
            End Try
        End If
    End Sub

    Private Function ValidateForm() As Boolean
        If _customerID = 0 Then
            MessageBox.Show("Please select a customer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return False
        End If
        If _propertyID = 0 Then
            MessageBox.Show("Please select a property.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return False
        End If
        Return True
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub frmPolicyEntry_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _isDirty Then
            If MessageBox.Show("Discard unsaved changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then e.Cancel = True
        End If
    End Sub
End Class
