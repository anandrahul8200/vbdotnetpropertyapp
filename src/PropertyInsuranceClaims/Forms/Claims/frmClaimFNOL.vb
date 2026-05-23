Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' First Notice of Loss entry form.
''' Creates a new claim against an active policy.
''' </summary>
Public Class frmClaimFNOL
    Inherits Form

    ' --- Private fields ---
    Private _policyID As Integer = 0
    Private _claimID As Integer = 0
    Private _isDirty As Boolean = False

    ' --- Controls ---
    Private lblPolicyNumber As New Label()
    Private lblCustomerName As New Label()
    Private lblPropertyAddress As New Label()
    Private lblPolicyStatus As New Label()
    Private WithEvents btnSelectPolicy As New Button()
    Private cboClaimType As New ComboBox()
    Private dtpLossDate As New DateTimePicker()
    Private dtpLossTime As New DateTimePicker()
    Private txtLossDescription As New TextBox()
    Private txtLossLocation As New TextBox()
    Private txtEstimatedLoss As New TextBox()
    Private txtPoliceReport As New TextBox()
    Private txtFireReport As New TextBox()
    Private cboWeatherCondition As New ComboBox()
    Private txtPointOfOrigin As New TextBox()
    Private cboPriority As New ComboBox()
    Private cboCatastrophe As New ComboBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()
    Private lblClaimNumber As New Label()

    Public Sub New(Optional policyID As Integer = 0)
        _policyID = policyID
    End Sub

    Private Sub frmClaimFNOL_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "First Notice of Loss (FNOL)"
            Me.Size = New Drawing.Size(700, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False

            InitializeControls()
            LoadLookups()

            If _policyID > 0 Then LoadPolicyInfo()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimFNOL_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        Dim ctrlLeft As Integer = 140

        ' Policy section
        Dim grpPolicy As New GroupBox() With {.Text = "Policy Information", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(660, 100)}

        btnSelectPolicy.Location = New Drawing.Point(15, 25)
        btnSelectPolicy.Size = New Drawing.Size(100, 25)
        btnSelectPolicy.Text = "Select Policy..."

        lblPolicyNumber.Location = New Drawing.Point(130, 28)
        lblPolicyNumber.AutoSize = True
        lblPolicyNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)

        lblCustomerName.Location = New Drawing.Point(130, 50)
        lblCustomerName.AutoSize = True

        lblPropertyAddress.Location = New Drawing.Point(130, 70)
        lblPropertyAddress.AutoSize = True

        lblPolicyStatus.Location = New Drawing.Point(450, 28)
        lblPolicyStatus.AutoSize = True

        grpPolicy.Controls.AddRange({btnSelectPolicy, lblPolicyNumber, lblCustomerName, lblPropertyAddress, lblPolicyStatus})
        Me.Controls.Add(grpPolicy)
        y += 115

        ' Loss details
        Dim grpLoss As New GroupBox() With {.Text = "Loss Details", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(660, 350)}
        Dim ly As Integer = 25

        AddLabelTo(grpLoss, "Claim Type:", 15, ly)
        cboClaimType.Location = New Drawing.Point(ctrlLeft, ly - 3)
        cboClaimType.Size = New Drawing.Size(200, 20)
        cboClaimType.DropDownStyle = ComboBoxStyle.DropDownList
        grpLoss.Controls.Add(cboClaimType)
        ly += 30

        AddLabelTo(grpLoss, "Loss Date:", 15, ly)
        dtpLossDate.Location = New Drawing.Point(ctrlLeft, ly - 3)
        dtpLossDate.Size = New Drawing.Size(150, 20)
        dtpLossDate.Format = DateTimePickerFormat.Short
        grpLoss.Controls.Add(dtpLossDate)

        dtpLossTime.Location = New Drawing.Point(310, ly - 3)
        dtpLossTime.Size = New Drawing.Size(100, 20)
        dtpLossTime.Format = DateTimePickerFormat.Time
        dtpLossTime.ShowUpDown = True
        grpLoss.Controls.Add(dtpLossTime)
        ly += 30

        AddLabelTo(grpLoss, "Description:", 15, ly)
        txtLossDescription.Location = New Drawing.Point(ctrlLeft, ly - 3)
        txtLossDescription.Size = New Drawing.Size(490, 80)
        txtLossDescription.Multiline = True
        txtLossDescription.ScrollBars = ScrollBars.Vertical
        grpLoss.Controls.Add(txtLossDescription)
        ly += 90

        AddLabelTo(grpLoss, "Loss Location:", 15, ly)
        txtLossLocation.Location = New Drawing.Point(ctrlLeft, ly - 3)
        txtLossLocation.Size = New Drawing.Size(400, 20)
        grpLoss.Controls.Add(txtLossLocation)
        ly += 30

        AddLabelTo(grpLoss, "Estimated Loss:", 15, ly)
        txtEstimatedLoss.Location = New Drawing.Point(ctrlLeft, ly - 3)
        txtEstimatedLoss.Size = New Drawing.Size(120, 20)
        grpLoss.Controls.Add(txtEstimatedLoss)

        AddLabelTo(grpLoss, "Priority:", 310, ly)
        cboPriority.Location = New Drawing.Point(370, ly - 3)
        cboPriority.Size = New Drawing.Size(100, 20)
        cboPriority.DropDownStyle = ComboBoxStyle.DropDownList
        grpLoss.Controls.Add(cboPriority)
        ly += 30

        AddLabelTo(grpLoss, "Police Report #:", 15, ly)
        txtPoliceReport.Location = New Drawing.Point(ctrlLeft, ly - 3)
        txtPoliceReport.Size = New Drawing.Size(150, 20)
        grpLoss.Controls.Add(txtPoliceReport)

        AddLabelTo(grpLoss, "Fire Report #:", 310, ly)
        txtFireReport.Location = New Drawing.Point(400, ly - 3)
        txtFireReport.Size = New Drawing.Size(150, 20)
        grpLoss.Controls.Add(txtFireReport)
        ly += 30

        AddLabelTo(grpLoss, "Weather:", 15, ly)
        cboWeatherCondition.Location = New Drawing.Point(ctrlLeft, ly - 3)
        cboWeatherCondition.Size = New Drawing.Size(150, 20)
        cboWeatherCondition.DropDownStyle = ComboBoxStyle.DropDownList
        grpLoss.Controls.Add(cboWeatherCondition)

        AddLabelTo(grpLoss, "Catastrophe:", 310, ly)
        cboCatastrophe.Location = New Drawing.Point(400, ly - 3)
        cboCatastrophe.Size = New Drawing.Size(200, 20)
        cboCatastrophe.DropDownStyle = ComboBoxStyle.DropDownList
        grpLoss.Controls.Add(cboCatastrophe)

        Me.Controls.Add(grpLoss)
        y += 365

        ' Claim number display
        lblClaimNumber.Location = New Drawing.Point(15, y + 5)
        lblClaimNumber.AutoSize = True
        lblClaimNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)
        Me.Controls.Add(lblClaimNumber)

        ' Buttons
        btnSave.Location = New Drawing.Point(400, y)
        btnSave.Size = New Drawing.Size(120, 35)
        btnSave.Text = "&Submit FNOL"
        Me.Controls.Add(btnSave)

        btnCancel.Location = New Drawing.Point(530, y)
        btnCancel.Size = New Drawing.Size(100, 35)
        btnCancel.Text = "&Cancel"
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        Dim lbl As New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True}
        parent.Controls.Add(lbl)
    End Sub

    Private Sub LoadLookups()
        ' Claim types
        cboClaimType.Items.AddRange({"PROPERTY_DAMAGE", "THEFT", "LIABILITY", "WATER_DAMAGE", "FIRE", "WIND", "HAIL", "OTHER"})
        cboClaimType.SelectedIndex = 0

        ' Priority
        cboPriority.Items.AddRange({"LOW", "NORMAL", "HIGH", "CRITICAL"})
        cboPriority.SelectedIndex = 1

        ' Weather
        cboWeatherCondition.Items.AddRange({"", "CLEAR", "RAIN", "STORM", "SNOW", "WIND", "HAIL", "TORNADO", "HURRICANE"})
        cboWeatherCondition.SelectedIndex = 0

        ' Catastrophe (would load from DB)
        cboCatastrophe.Items.Add(New With {.Text = "(None)", .Value = 0})
        cboCatastrophe.DisplayMember = "Text"
        cboCatastrophe.SelectedIndex = 0
    End Sub

    Private Sub LoadPolicyInfo()
        Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
        If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then Return

        Dim row As DataRow = ds.Tables(0).Rows(0)
        lblPolicyNumber.Text = row("PolicyNumber").ToString()
        lblCustomerName.Text = $"{row("FirstName")} {row("LastName")}"
        lblPropertyAddress.Text = $"{row("PropertyAddress")}, {row("PropertyCity")} {row("PropertyState")}"
        lblPolicyStatus.Text = $"Status: {row("PolicyStatus")}"
    End Sub

    ' --- Select Policy ---
    Private Sub btnSelectPolicy_Click(sender As Object, e As EventArgs) Handles btnSelectPolicy.Click
        Dim frm As New frmPolicySearch()
        frm.IsPickerMode = True
        frm.ShowDialog(Me)
        If frm.SelectedPolicyID > 0 Then
            _policyID = frm.SelectedPolicyID
            LoadPolicyInfo()
        End If
    End Sub

    ' --- Save FNOL ---
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If Not ValidateForm() Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim lossDateTime As DateTime = dtpLossDate.Value.Date.Add(dtpLossTime.Value.TimeOfDay)

            Dim dto As New ClaimDTO() With {
                .PolicyID = _policyID,
                .ClaimType = cboClaimType.SelectedItem.ToString(),
                .LossDate = lossDateTime,
                .LossDescription = txtLossDescription.Text.Trim(),
                .LossLocation = If(String.IsNullOrWhiteSpace(txtLossLocation.Text), Nothing, txtLossLocation.Text.Trim()),
                .PoliceReportNumber = If(String.IsNullOrWhiteSpace(txtPoliceReport.Text), Nothing, txtPoliceReport.Text.Trim()),
                .FireReportNumber = If(String.IsNullOrWhiteSpace(txtFireReport.Text), Nothing, txtFireReport.Text.Trim()),
                .WeatherCondition = If(cboWeatherCondition.SelectedIndex > 0, cboWeatherCondition.SelectedItem.ToString(), Nothing),
                .PointOfOrigin = If(String.IsNullOrWhiteSpace(txtPointOfOrigin.Text), Nothing, txtPointOfOrigin.Text.Trim()),
                .Priority = cboPriority.SelectedItem.ToString()
            }

            If Not String.IsNullOrWhiteSpace(txtEstimatedLoss.Text) Then
                dto.EstimatedLoss = CDec(txtEstimatedLoss.Text)
            End If

            _claimID = ClaimDataAccess.Create(dto)
            lblClaimNumber.Text = $"Claim Created: {dto.ClaimNumber}"

            _isDirty = False
            MessageBox.Show($"FNOL submitted successfully.{Environment.NewLine}Claim Number: {dto.ClaimNumber}",
                           "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Disable form after successful submission
            btnSave.Enabled = False
        Catch ex As Exception
            MessageBox.Show("Error submitting FNOL: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Function ValidateForm() As Boolean
        If _policyID = 0 Then
            MessageBox.Show("Please select a policy.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If cboClaimType.SelectedIndex < 0 Then
            MessageBox.Show("Please select a claim type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If dtpLossDate.Value > DateTime.Now Then
            MessageBox.Show("Loss date cannot be in the future.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtLossDescription.Text) Then
            MessageBox.Show("Loss description is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtLossDescription.Focus()
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(txtEstimatedLoss.Text) Then
            Dim amount As Decimal
            If Not Decimal.TryParse(txtEstimatedLoss.Text, amount) OrElse amount < 0 Then
                MessageBox.Show("Estimated loss must be a valid positive number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtEstimatedLoss.Focus()
                Return False
            End If
        End If

        Return True
    End Function

    ' --- Cancel ---
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub frmClaimFNOL_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _isDirty Then
            Dim result = MessageBox.Show("You have unsaved changes. Discard?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result = DialogResult.No Then e.Cancel = True
        End If
    End Sub

End Class
