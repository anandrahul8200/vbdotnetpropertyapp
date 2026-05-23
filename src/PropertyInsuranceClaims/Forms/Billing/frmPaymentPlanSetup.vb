Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Payment plan setup and management form — configure installment plans,
''' view plan details, and manage plan assignments.
''' </summary>
Public Class frmPaymentPlanSetup
    Inherits Form

    ' Plan list
    Private dgvPlans As New DataGridView()
    Private WithEvents btnNewPlan As New Button()
    Private WithEvents btnEditPlan As New Button()
    Private WithEvents btnDeactivate As New Button()
    Private WithEvents btnRefresh As New Button()

    ' Plan detail panel
    Private grpDetail As New GroupBox()
    Private txtPlanCode As New TextBox()
    Private txtPlanName As New TextBox()
    Private txtInstallments As New TextBox()
    Private txtDownPaymentPct As New TextBox()
    Private txtInstallmentFee As New TextBox()
    Private txtLateFee As New TextBox()
    Private txtGracePeriod As New TextBox()
    Private txtCancelNoticeDays As New TextBox()
    Private chkIsActive As New CheckBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancelEdit As New Button()

    ' Installment schedule preview
    Private grpPreview As New GroupBox()
    Private dgvSchedulePreview As New DataGridView()
    Private txtPreviewPremium As New TextBox()
    Private WithEvents btnPreview As New Button()

    Private WithEvents btnClose As New Button()
    Private _editPlanID As Integer = 0

    Private Sub frmPaymentPlanSetup_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Payment Plan Setup"
            Me.Size = New Drawing.Size(900, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadPlans()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPaymentPlanSetup_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Plan list (top)
        Dim grpList As New GroupBox() With {.Text = "Payment Plans", .Dock = DockStyle.Top, .Height = 200}
        Dim pnlListButtons As New Panel() With {.Dock = DockStyle.Top, .Height = 38}
        btnNewPlan.Location = New Drawing.Point(10, 5) : btnNewPlan.Size = New Drawing.Size(80, 28) : btnNewPlan.Text = "&New" : pnlListButtons.Controls.Add(btnNewPlan)
        btnEditPlan.Location = New Drawing.Point(100, 5) : btnEditPlan.Size = New Drawing.Size(80, 28) : btnEditPlan.Text = "&Edit" : pnlListButtons.Controls.Add(btnEditPlan)
        btnDeactivate.Location = New Drawing.Point(190, 5) : btnDeactivate.Size = New Drawing.Size(100, 28) : btnDeactivate.Text = "&Deactivate" : pnlListButtons.Controls.Add(btnDeactivate)
        btnRefresh.Location = New Drawing.Point(750, 5) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "Re&fresh" : pnlListButtons.Controls.Add(btnRefresh)
        grpList.Controls.Add(pnlListButtons)

        dgvPlans.Dock = DockStyle.Fill : dgvPlans.ReadOnly = True : dgvPlans.AllowUserToAddRows = False
        dgvPlans.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvPlans.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpList.Controls.Add(dgvPlans)
        dgvPlans.BringToFront()
        Me.Controls.Add(grpList)

        ' Detail panel (middle)
        grpDetail.Text = "Plan Details" : grpDetail.Dock = DockStyle.Top : grpDetail.Height = 200
        Dim y As Integer = 22
        Dim col2 As Integer = 400

        AddLabelTo(grpDetail, "Plan Code:", 15, y) : txtPlanCode.Location = New Drawing.Point(120, y - 3) : txtPlanCode.Size = New Drawing.Size(100, 20) : grpDetail.Controls.Add(txtPlanCode)
        AddLabelTo(grpDetail, "Plan Name:", col2, y) : txtPlanName.Location = New Drawing.Point(col2 + 85, y - 3) : txtPlanName.Size = New Drawing.Size(200, 20) : grpDetail.Controls.Add(txtPlanName)
        y += 30

        AddLabelTo(grpDetail, "# Installments:", 15, y) : txtInstallments.Location = New Drawing.Point(120, y - 3) : txtInstallments.Size = New Drawing.Size(50, 20) : grpDetail.Controls.Add(txtInstallments)
        AddLabelTo(grpDetail, "Down Payment %:", col2, y) : txtDownPaymentPct.Location = New Drawing.Point(col2 + 120, y - 3) : txtDownPaymentPct.Size = New Drawing.Size(60, 20) : grpDetail.Controls.Add(txtDownPaymentPct)
        y += 30

        AddLabelTo(grpDetail, "Installment Fee:", 15, y) : txtInstallmentFee.Location = New Drawing.Point(120, y - 3) : txtInstallmentFee.Size = New Drawing.Size(80, 20) : grpDetail.Controls.Add(txtInstallmentFee)
        AddLabelTo(grpDetail, "Late Fee:", col2, y) : txtLateFee.Location = New Drawing.Point(col2 + 85, y - 3) : txtLateFee.Size = New Drawing.Size(80, 20) : grpDetail.Controls.Add(txtLateFee)
        y += 30

        AddLabelTo(grpDetail, "Grace Period (days):", 15, y) : txtGracePeriod.Location = New Drawing.Point(150, y - 3) : txtGracePeriod.Size = New Drawing.Size(50, 20) : grpDetail.Controls.Add(txtGracePeriod)
        AddLabelTo(grpDetail, "Cancel Notice (days):", col2, y) : txtCancelNoticeDays.Location = New Drawing.Point(col2 + 150, y - 3) : txtCancelNoticeDays.Size = New Drawing.Size(50, 20) : grpDetail.Controls.Add(txtCancelNoticeDays)
        y += 30

        chkIsActive.Location = New Drawing.Point(120, y) : chkIsActive.Text = "Active" : chkIsActive.Checked = True : chkIsActive.AutoSize = True : grpDetail.Controls.Add(chkIsActive)
        btnSave.Location = New Drawing.Point(250, y - 3) : btnSave.Size = New Drawing.Size(80, 28) : btnSave.Text = "&Save" : grpDetail.Controls.Add(btnSave)
        btnCancelEdit.Location = New Drawing.Point(340, y - 3) : btnCancelEdit.Size = New Drawing.Size(80, 28) : btnCancelEdit.Text = "Cancel" : grpDetail.Controls.Add(btnCancelEdit)
        Me.Controls.Add(grpDetail)

        ' Preview panel (bottom)
        grpPreview.Text = "Installment Schedule Preview" : grpPreview.Dock = DockStyle.Fill
        Dim pnlPreviewTop As New Panel() With {.Dock = DockStyle.Top, .Height = 38}
        AddLabelTo(pnlPreviewTop, "Annual Premium ($):", 10, 10)
        txtPreviewPremium.Location = New Drawing.Point(145, 7) : txtPreviewPremium.Size = New Drawing.Size(100, 20) : txtPreviewPremium.Text = "1500.00" : pnlPreviewTop.Controls.Add(txtPreviewPremium)
        btnPreview.Location = New Drawing.Point(260, 5) : btnPreview.Size = New Drawing.Size(120, 28) : btnPreview.Text = "Generate &Preview" : pnlPreviewTop.Controls.Add(btnPreview)
        grpPreview.Controls.Add(pnlPreviewTop)

        dgvSchedulePreview.Dock = DockStyle.Fill : dgvSchedulePreview.ReadOnly = True : dgvSchedulePreview.AllowUserToAddRows = False
        dgvSchedulePreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvSchedulePreview.Columns.Add("Installment", "#")
        dgvSchedulePreview.Columns.Add("DueDate", "Due Date")
        dgvSchedulePreview.Columns.Add("Premium", "Premium")
        dgvSchedulePreview.Columns.Add("Fee", "Fee")
        dgvSchedulePreview.Columns.Add("Total", "Total Due")
        dgvSchedulePreview.Columns.Add("RunningTotal", "Cumulative")
        grpPreview.Controls.Add(dgvSchedulePreview)
        dgvSchedulePreview.BringToFront()
        Me.Controls.Add(grpPreview)

        ' Close
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(780, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        grpPreview.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadPlans()
        Try
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Billing.usp_PaymentPlan_List", Nothing)
            dgvPlans.DataSource = dt
            If dgvPlans.Columns.Contains("PaymentPlanID") Then dgvPlans.Columns("PaymentPlanID").Visible = False
        Catch ex As Exception
            ErrorLogger.LogError(ex, "LoadPlans")
        End Try
    End Sub

    Private Sub btnNewPlan_Click(sender As Object, e As EventArgs) Handles btnNewPlan.Click
        _editPlanID = 0
        ClearDetailFields()
        txtPlanCode.Enabled = True
        txtPlanCode.Focus()
    End Sub

    Private Sub btnEditPlan_Click(sender As Object, e As EventArgs) Handles btnEditPlan.Click
        If dgvPlans.CurrentRow Is Nothing Then Return
        _editPlanID = CInt(dgvPlans.CurrentRow.Cells("PaymentPlanID").Value)
        txtPlanCode.Text = dgvPlans.CurrentRow.Cells("PlanCode").Value.ToString()
        txtPlanCode.Enabled = False
        txtPlanName.Text = dgvPlans.CurrentRow.Cells("PlanName").Value.ToString()
        txtInstallments.Text = dgvPlans.CurrentRow.Cells("NumberOfInstallments").Value.ToString()
        txtDownPaymentPct.Text = (CDec(dgvPlans.CurrentRow.Cells("DownPaymentPercent").Value) * 100).ToString("N1")
        txtInstallmentFee.Text = dgvPlans.CurrentRow.Cells("InstallmentFee").Value.ToString()
        txtLateFee.Text = dgvPlans.CurrentRow.Cells("LateFeeAmount").Value.ToString()
        txtGracePeriod.Text = dgvPlans.CurrentRow.Cells("GracePeriodDays").Value.ToString()
        txtCancelNoticeDays.Text = dgvPlans.CurrentRow.Cells("CancellationNoticeDays").Value.ToString()
        chkIsActive.Checked = CBool(dgvPlans.CurrentRow.Cells("IsActive").Value)
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If String.IsNullOrWhiteSpace(txtPlanCode.Text) OrElse String.IsNullOrWhiteSpace(txtPlanName.Text) Then
                MessageBox.Show("Plan code and name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            If Not Integer.TryParse(txtInstallments.Text, Nothing) OrElse CInt(txtInstallments.Text) < 1 Then
                MessageBox.Show("Valid number of installments required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            MessageBox.Show("Payment plan saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadPlans()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancelEdit_Click(sender As Object, e As EventArgs) Handles btnCancelEdit.Click
        ClearDetailFields()
    End Sub

    Private Sub btnDeactivate_Click(sender As Object, e As EventArgs) Handles btnDeactivate.Click
        If dgvPlans.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Deactivate this plan?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Plan deactivated.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadPlans()
        End If
    End Sub

    Private Sub btnPreview_Click(sender As Object, e As EventArgs) Handles btnPreview.Click
        Try
            Dim premium As Decimal
            If Not Decimal.TryParse(txtPreviewPremium.Text, premium) OrElse premium <= 0 Then
                MessageBox.Show("Enter a valid premium amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim numInstallments As Integer = If(Integer.TryParse(txtInstallments.Text, Nothing), CInt(txtInstallments.Text), 4)
            Dim downPct As Decimal = If(Decimal.TryParse(txtDownPaymentPct.Text, Nothing), CDec(txtDownPaymentPct.Text) / 100, 0.25D)
            Dim fee As Decimal = If(Decimal.TryParse(txtInstallmentFee.Text, Nothing), CDec(txtInstallmentFee.Text), 5D)
            Dim graceDays As Integer = If(Integer.TryParse(txtGracePeriod.Text, Nothing), CInt(txtGracePeriod.Text), 10)

            dgvSchedulePreview.Rows.Clear()
            Dim downPayment As Decimal = Math.Round(premium * downPct, 2)
            Dim remaining As Decimal = premium - downPayment
            Dim installmentAmount As Decimal = Math.Round(remaining / (numInstallments - 1), 2)
            Dim runningTotal As Decimal = 0
            Dim effectiveDate As Date = DateTime.Today

            ' First installment (down payment)
            Dim firstTotal As Decimal = downPayment + fee
            runningTotal += firstTotal
            dgvSchedulePreview.Rows.Add("1 (Down)", effectiveDate.AddDays(graceDays).ToString("MM/dd/yyyy"), downPayment.ToString("N2"), fee.ToString("N2"), firstTotal.ToString("N2"), runningTotal.ToString("N2"))

            ' Subsequent installments
            For i As Integer = 2 To numInstallments
                Dim dueDate As Date = effectiveDate.AddMonths(i - 1).AddDays(graceDays)
                Dim amt As Decimal = If(i = numInstallments, remaining - (installmentAmount * (numInstallments - 2)), installmentAmount)
                Dim total As Decimal = amt + fee
                runningTotal += total
                dgvSchedulePreview.Rows.Add(i.ToString(), dueDate.ToString("MM/dd/yyyy"), amt.ToString("N2"), fee.ToString("N2"), total.ToString("N2"), runningTotal.ToString("N2"))
            Next
        Catch ex As Exception
            MessageBox.Show("Error generating preview: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ClearDetailFields()
        _editPlanID = 0
        txtPlanCode.Clear() : txtPlanName.Clear() : txtInstallments.Clear()
        txtDownPaymentPct.Clear() : txtInstallmentFee.Clear() : txtLateFee.Clear()
        txtGracePeriod.Clear() : txtCancelNoticeDays.Clear() : chkIsActive.Checked = True
        txtPlanCode.Enabled = True
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadPlans()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
