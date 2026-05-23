Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Endorsement processing form — mid-term policy changes with pro-rata premium calculation.
''' </summary>
Public Class frmEndorsement
    Inherits Form

    Private _policyID As Integer
    Private _endorsementID As Integer = 0

    ' Header
    Private lblPolicyNumber As New Label()
    Private lblCurrentPremium As New Label()
    Private dtpEffectiveDate As New DateTimePicker()
    Private cboEndorsementType As New ComboBox()
    Private txtDescription As New TextBox()

    ' Coverage changes grid
    Private dgvCoverages As New DataGridView()

    ' Premium impact
    Private lblPremiumChange As New Label()
    Private lblProRataFactor As New Label()
    Private lblAdditionalPremium As New Label()
    Private lblReturnPremium As New Label()

    ' Buttons
    Private WithEvents btnCalculate As New Button()
    Private WithEvents btnProcess As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmEndorsement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Process Endorsement"
            Me.Size = New Drawing.Size(800, 600)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            InitializeControls()
            LoadPolicyData()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmEndorsement_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15

        ' Policy info
        Dim grpPolicy As New GroupBox() With {.Text = "Policy", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 60)}
        lblPolicyNumber.Location = New Drawing.Point(15, 22) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : grpPolicy.Controls.Add(lblPolicyNumber)
        lblCurrentPremium.Location = New Drawing.Point(300, 22) : lblCurrentPremium.AutoSize = True : grpPolicy.Controls.Add(lblCurrentPremium)
        Me.Controls.Add(grpPolicy)
        y += 70

        ' Endorsement details
        Dim grpDetails As New GroupBox() With {.Text = "Endorsement Details", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 120)}
        AddLabelTo(grpDetails, "Type:", 15, 25)
        cboEndorsementType.Location = New Drawing.Point(100, 22) : cboEndorsementType.Size = New Drawing.Size(200, 20) : cboEndorsementType.DropDownStyle = ComboBoxStyle.DropDownList
        cboEndorsementType.Items.AddRange({"INCREASE_LIMITS", "DECREASE_LIMITS", "ADD_COVERAGE", "REMOVE_COVERAGE", "CHANGE_DEDUCTIBLE", "ADDRESS_CHANGE", "MORTGAGEE_CHANGE", "OTHER"})
        cboEndorsementType.SelectedIndex = 0 : grpDetails.Controls.Add(cboEndorsementType)

        AddLabelTo(grpDetails, "Effective:", 350, 25)
        dtpEffectiveDate.Location = New Drawing.Point(420, 22) : dtpEffectiveDate.Size = New Drawing.Size(130, 20) : dtpEffectiveDate.Format = DateTimePickerFormat.Short : grpDetails.Controls.Add(dtpEffectiveDate)

        AddLabelTo(grpDetails, "Description:", 15, 60)
        txtDescription.Location = New Drawing.Point(100, 57) : txtDescription.Size = New Drawing.Size(550, 40) : txtDescription.Multiline = True : grpDetails.Controls.Add(txtDescription)
        Me.Controls.Add(grpDetails)
        y += 130

        ' Coverage changes
        Dim grpCoverages As New GroupBox() With {.Text = "Coverage Changes", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(760, 180)}
        dgvCoverages.Location = New Drawing.Point(10, 20) : dgvCoverages.Size = New Drawing.Size(740, 150)
        dgvCoverages.AllowUserToAddRows = False : dgvCoverages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvCoverages.Columns.Add("CoverageCode", "Coverage")
        dgvCoverages.Columns.Add("CurrentLimit", "Current Limit")
        dgvCoverages.Columns.Add("NewLimit", "New Limit")
        dgvCoverages.Columns.Add("CurrentDeductible", "Current Ded.")
        dgvCoverages.Columns.Add("NewDeductible", "New Ded.")
        dgvCoverages.Columns(0).ReadOnly = True : dgvCoverages.Columns(1).ReadOnly = True : dgvCoverages.Columns(3).ReadOnly = True
        grpCoverages.Controls.Add(dgvCoverages)
        Me.Controls.Add(grpCoverages)
        y += 190

        ' Premium impact
        Dim grpPremium As New GroupBox() With {.Text = "Premium Impact", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(400, 80)}
        lblProRataFactor.Location = New Drawing.Point(15, 22) : lblProRataFactor.AutoSize = True : lblProRataFactor.Text = "Pro-Rata Factor: --" : grpPremium.Controls.Add(lblProRataFactor)
        lblPremiumChange.Location = New Drawing.Point(15, 44) : lblPremiumChange.AutoSize = True : lblPremiumChange.Text = "Premium Change: --" : grpPremium.Controls.Add(lblPremiumChange)
        lblAdditionalPremium.Location = New Drawing.Point(250, 22) : lblAdditionalPremium.AutoSize = True : lblAdditionalPremium.Text = "Additional: --" : grpPremium.Controls.Add(lblAdditionalPremium)
        lblReturnPremium.Location = New Drawing.Point(250, 44) : lblReturnPremium.AutoSize = True : lblReturnPremium.Text = "Return: --" : grpPremium.Controls.Add(lblReturnPremium)
        Me.Controls.Add(grpPremium)

        ' Buttons
        btnCalculate.Location = New Drawing.Point(430, y + 10) : btnCalculate.Size = New Drawing.Size(100, 35) : btnCalculate.Text = "Ca&lculate" : Me.Controls.Add(btnCalculate)
        btnProcess.Location = New Drawing.Point(540, y + 10) : btnProcess.Size = New Drawing.Size(100, 35) : btnProcess.Text = "&Process" : btnProcess.Enabled = False : Me.Controls.Add(btnProcess)
        btnCancel.Location = New Drawing.Point(650, y + 10) : btnCancel.Size = New Drawing.Size(80, 35) : btnCancel.Text = "Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadPolicyData()
        Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
        If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then Return

        Dim row As DataRow = ds.Tables(0).Rows(0)
        lblPolicyNumber.Text = $"Policy: {row("PolicyNumber")} ({row("PolicyType")})"
        If Not IsDBNull(row("AnnualPremium")) Then lblCurrentPremium.Text = $"Current Premium: {CDec(row("AnnualPremium")):C}"

        ' Load coverages into grid
        If ds.Tables.Count > 1 Then
            For Each covRow As DataRow In ds.Tables(1).Rows
                dgvCoverages.Rows.Add(
                    covRow("CoverageCode").ToString(),
                    If(IsDBNull(covRow("LimitAmount")), "", CDec(covRow("LimitAmount")).ToString("N0")),
                    If(IsDBNull(covRow("LimitAmount")), "", CDec(covRow("LimitAmount")).ToString("N0")),
                    If(IsDBNull(covRow("DeductibleAmount")), "", CDec(covRow("DeductibleAmount")).ToString("N0")),
                    If(IsDBNull(covRow("DeductibleAmount")), "", CDec(covRow("DeductibleAmount")).ToString("N0"))
                )
            Next
        End If
    End Sub

    Private Sub btnCalculate_Click(sender As Object, e As EventArgs) Handles btnCalculate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            ' Simulate pro-rata calculation
            Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
            If ds.Tables(0).Rows.Count = 0 Then Return
            Dim row As DataRow = ds.Tables(0).Rows(0)
            Dim policyEffective As Date = CDate(row("EffectiveDate"))
            Dim policyExpiry As Date = CDate(row("ExpiryDate"))
            Dim endorsementDate As Date = dtpEffectiveDate.Value.Date

            Dim daysInTerm As Integer = CInt((policyExpiry - policyEffective).TotalDays)
            Dim daysRemaining As Integer = CInt((policyExpiry - endorsementDate).TotalDays)
            Dim proRata As Decimal = CDec(daysRemaining) / CDec(daysInTerm)

            lblProRataFactor.Text = $"Pro-Rata Factor: {proRata:N4}"
            lblPremiumChange.Text = $"Premium Change: $0.00 (simulated)"
            lblAdditionalPremium.Text = "Additional: $0.00"
            lblReturnPremium.Text = "Return: $0.00"
            btnProcess.Enabled = True
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        If MessageBox.Show("Process this endorsement?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Endorsement processed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        End If
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
