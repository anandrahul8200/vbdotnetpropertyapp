Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Rate table maintenance — view, create, and edit rate tables and their detail rows.
''' Used by actuarial/underwriting to manage pricing.
''' </summary>
Public Class frmRateTableMaintenance
    Inherits Form

    ' --- Controls ---
    Private cboRateTableCode As New ComboBox()
    Private cboState As New ComboBox()
    Private cboPolicyType As New ComboBox()
    Private WithEvents btnLoad As New Button()
    Private WithEvents btnNewTable As New Button()
    Private WithEvents btnAddRow As New Button()
    Private WithEvents btnDeleteRow As New Button()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnExport As New Button()
    Private WithEvents btnImport As New Button()
    Private WithEvents btnClose As New Button()

    ' Header info
    Private lblTableName As New Label()
    Private lblVersion As New Label()
    Private lblEffective As New Label()
    Private lblStatus As New Label()

    ' Detail grid
    Private dgvDetails As New DataGridView()

    ' New table panel
    Private grpNewTable As New GroupBox()
    Private txtNewCode As New TextBox()
    Private txtNewName As New TextBox()
    Private cboNewRateType As New ComboBox()
    Private cboNewPolicyType As New ComboBox()
    Private cboNewState As New ComboBox()
    Private dtpNewEffective As New DateTimePicker()
    Private txtNewFiling As New TextBox()
    Private WithEvents btnCreateTable As New Button()
    Private WithEvents btnCancelNew As New Button()

    Private _rateTableID As Integer = 0

    Private Sub frmRateTableMaintenance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Rate Table Maintenance"
            Me.Size = New Drawing.Size(950, 650)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadRateTableList()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmRateTableMaintenance_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Filter/selection panel
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 80}
        AddLabelTo(pnlTop, "Rate Table:", 10, 12)
        cboRateTableCode.Location = New Drawing.Point(90, 9) : cboRateTableCode.Size = New Drawing.Size(200, 20) : cboRateTableCode.DropDownStyle = ComboBoxStyle.DropDownList : pnlTop.Controls.Add(cboRateTableCode)
        AddLabelTo(pnlTop, "State:", 310, 12)
        cboState.Location = New Drawing.Point(350, 9) : cboState.Size = New Drawing.Size(60, 20) : cboState.DropDownStyle = ComboBoxStyle.DropDownList : cboState.Items.Add("(All)") : cboState.SelectedIndex = 0 : pnlTop.Controls.Add(cboState)
        AddLabelTo(pnlTop, "Type:", 430, 12)
        cboPolicyType.Location = New Drawing.Point(470, 9) : cboPolicyType.Size = New Drawing.Size(80, 20) : cboPolicyType.DropDownStyle = ComboBoxStyle.DropDownList
        cboPolicyType.Items.AddRange({"(All)", "HO3", "HO4", "HO6", "DP3"}) : cboPolicyType.SelectedIndex = 0 : pnlTop.Controls.Add(cboPolicyType)
        btnLoad.Location = New Drawing.Point(570, 6) : btnLoad.Size = New Drawing.Size(80, 28) : btnLoad.Text = "&Load" : pnlTop.Controls.Add(btnLoad)
        btnNewTable.Location = New Drawing.Point(660, 6) : btnNewTable.Size = New Drawing.Size(100, 28) : btnNewTable.Text = "&New Table" : pnlTop.Controls.Add(btnNewTable)

        ' Header info
        lblTableName.Location = New Drawing.Point(10, 45) : lblTableName.AutoSize = True : lblTableName.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlTop.Controls.Add(lblTableName)
        lblVersion.Location = New Drawing.Point(300, 45) : lblVersion.AutoSize = True : pnlTop.Controls.Add(lblVersion)
        lblEffective.Location = New Drawing.Point(420, 45) : lblEffective.AutoSize = True : pnlTop.Controls.Add(lblEffective)
        lblStatus.Location = New Drawing.Point(620, 45) : lblStatus.AutoSize = True : pnlTop.Controls.Add(lblStatus)
        Me.Controls.Add(pnlTop)

        ' Detail grid
        dgvDetails.Dock = DockStyle.Fill : dgvDetails.AllowUserToAddRows = False
        dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvDetails.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.AliceBlue
        Me.Controls.Add(dgvDetails)

        ' Bottom buttons
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnAddRow.Location = New Drawing.Point(10, 8) : btnAddRow.Size = New Drawing.Size(80, 30) : btnAddRow.Text = "Add &Row" : pnlBottom.Controls.Add(btnAddRow)
        btnDeleteRow.Location = New Drawing.Point(100, 8) : btnDeleteRow.Size = New Drawing.Size(90, 30) : btnDeleteRow.Text = "&Delete Row" : pnlBottom.Controls.Add(btnDeleteRow)
        btnSave.Location = New Drawing.Point(210, 8) : btnSave.Size = New Drawing.Size(80, 30) : btnSave.Text = "&Save" : pnlBottom.Controls.Add(btnSave)
        btnExport.Location = New Drawing.Point(600, 8) : btnExport.Size = New Drawing.Size(80, 30) : btnExport.Text = "E&xport" : pnlBottom.Controls.Add(btnExport)
        btnImport.Location = New Drawing.Point(690, 8) : btnImport.Size = New Drawing.Size(80, 30) : btnImport.Text = "&Import" : pnlBottom.Controls.Add(btnImport)
        btnClose.Location = New Drawing.Point(830, 8) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)

        ' New table panel (hidden by default)
        grpNewTable.Text = "Create New Rate Table" : grpNewTable.Size = New Drawing.Size(500, 280)
        grpNewTable.Location = New Drawing.Point(200, 150) : grpNewTable.Visible = False
        Dim ny As Integer = 25
        AddLabelTo(grpNewTable, "Code:", 15, ny) : txtNewCode.Location = New Drawing.Point(100, ny - 3) : txtNewCode.Size = New Drawing.Size(150, 20) : grpNewTable.Controls.Add(txtNewCode) : ny += 30
        AddLabelTo(grpNewTable, "Name:", 15, ny) : txtNewName.Location = New Drawing.Point(100, ny - 3) : txtNewName.Size = New Drawing.Size(300, 20) : grpNewTable.Controls.Add(txtNewName) : ny += 30
        AddLabelTo(grpNewTable, "Rate Type:", 15, ny) : cboNewRateType.Location = New Drawing.Point(100, ny - 3) : cboNewRateType.Size = New Drawing.Size(130, 20) : cboNewRateType.DropDownStyle = ComboBoxStyle.DropDownList
        cboNewRateType.Items.AddRange({"BASE", "FACTOR", "DISCOUNT", "SURCHARGE", "TAX"}) : cboNewRateType.SelectedIndex = 0 : grpNewTable.Controls.Add(cboNewRateType) : ny += 30
        AddLabelTo(grpNewTable, "Policy Type:", 15, ny) : cboNewPolicyType.Location = New Drawing.Point(100, ny - 3) : cboNewPolicyType.Size = New Drawing.Size(100, 20) : cboNewPolicyType.DropDownStyle = ComboBoxStyle.DropDownList
        cboNewPolicyType.Items.AddRange({"HO3", "HO4", "HO6", "DP3", "BOP"}) : cboNewPolicyType.SelectedIndex = 0 : grpNewTable.Controls.Add(cboNewPolicyType) : ny += 30
        AddLabelTo(grpNewTable, "State:", 15, ny) : cboNewState.Location = New Drawing.Point(100, ny - 3) : cboNewState.Size = New Drawing.Size(60, 20) : cboNewState.DropDownStyle = ComboBoxStyle.DropDownList : cboNewState.Items.Add("") : cboNewState.SelectedIndex = 0 : grpNewTable.Controls.Add(cboNewState) : ny += 30
        AddLabelTo(grpNewTable, "Effective:", 15, ny) : dtpNewEffective.Location = New Drawing.Point(100, ny - 3) : dtpNewEffective.Size = New Drawing.Size(130, 20) : dtpNewEffective.Format = DateTimePickerFormat.Short : grpNewTable.Controls.Add(dtpNewEffective) : ny += 30
        AddLabelTo(grpNewTable, "Filing #:", 15, ny) : txtNewFiling.Location = New Drawing.Point(100, ny - 3) : txtNewFiling.Size = New Drawing.Size(150, 20) : grpNewTable.Controls.Add(txtNewFiling) : ny += 35
        btnCreateTable.Location = New Drawing.Point(100, ny) : btnCreateTable.Size = New Drawing.Size(100, 30) : btnCreateTable.Text = "&Create" : grpNewTable.Controls.Add(btnCreateTable)
        btnCancelNew.Location = New Drawing.Point(210, ny) : btnCancelNew.Size = New Drawing.Size(80, 30) : btnCancelNew.Text = "Cancel" : grpNewTable.Controls.Add(btnCancelNew)
        Me.Controls.Add(grpNewTable)
        grpNewTable.BringToFront()
        dgvDetails.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadRateTableList()
        cboRateTableCode.Items.Clear()
        cboRateTableCode.Items.AddRange({"BASE_RATES_HO3", "CONSTRUCTION_FACTORS", "AGE_FACTORS", "ROOF_FACTORS",
                                          "PROTECTION_CLASS", "CREDIT_SCORE", "CLAIMS_HISTORY", "DEDUCTIBLE_OPTIONS",
                                          "TERRITORY_FACTORS", "OCCUPANCY_FACTORS", "PROTECTIVE_DEVICE"})
        If cboRateTableCode.Items.Count > 0 Then cboRateTableCode.SelectedIndex = 0
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Try
            If cboRateTableCode.SelectedItem Is Nothing Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@RateTableCode", cboRateTableCode.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@StateCode", If(cboState.SelectedIndex > 0, cboState.SelectedItem.ToString(), Nothing)),
                DatabaseHelper.CreateParam("@EffectiveDate", DateTime.Today)
            }
            Dim ds As DataSet = DatabaseHelper.ExecuteDataSet("Underwriting.usp_RateTable_GetDetails", params)

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                Dim header As DataRow = ds.Tables(0).Rows(0)
                _rateTableID = CInt(header("RateTableID"))
                lblTableName.Text = header("RateTableName").ToString()
                lblVersion.Text = $"Version: {header("Version")}"
                lblEffective.Text = $"Effective: {CDate(header("EffectiveDate")):MM/dd/yyyy}"
                lblStatus.Text = If(CBool(header("IsActive")), "ACTIVE", "INACTIVE")
            End If

            If ds.Tables.Count > 1 Then dgvDetails.DataSource = ds.Tables(1)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnNewTable_Click(sender As Object, e As EventArgs) Handles btnNewTable.Click
        grpNewTable.Visible = True : txtNewCode.Focus()
    End Sub

    Private Sub btnCreateTable_Click(sender As Object, e As EventArgs) Handles btnCreateTable.Click
        Try
            If String.IsNullOrWhiteSpace(txtNewCode.Text) OrElse String.IsNullOrWhiteSpace(txtNewName.Text) Then
                MessageBox.Show("Code and Name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@RateTableCode", txtNewCode.Text.Trim().ToUpper()),
                DatabaseHelper.CreateParam("@RateTableName", txtNewName.Text.Trim()),
                DatabaseHelper.CreateParam("@RateType", cboNewRateType.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@PolicyType", cboNewPolicyType.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@StateCode", If(String.IsNullOrEmpty(cboNewState.Text), Nothing, cboNewState.Text)),
                DatabaseHelper.CreateParam("@EffectiveDate", dtpNewEffective.Value.Date),
                DatabaseHelper.CreateParam("@FilingNumber", If(String.IsNullOrWhiteSpace(txtNewFiling.Text), Nothing, txtNewFiling.Text.Trim())),
                DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
            }
            Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@RateTableID", SqlDbType.Int)
            params.Add(idParam)

            DatabaseHelper.ExecuteNonQuery("Underwriting.usp_RateTable_Create", params.ToArray())
            _rateTableID = CInt(idParam.Value)
            MessageBox.Show($"Rate table created (ID: {_rateTableID}).", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            grpNewTable.Visible = False
            LoadRateTableList()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancelNew_Click(sender As Object, e As EventArgs) Handles btnCancelNew.Click
        grpNewTable.Visible = False
    End Sub

    Private Sub btnAddRow_Click(sender As Object, e As EventArgs) Handles btnAddRow.Click
        If _rateTableID = 0 Then
            MessageBox.Show("Please load a rate table first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information) : Return
        End If
        dgvDetails.Rows.Add()
    End Sub

    Private Sub btnDeleteRow_Click(sender As Object, e As EventArgs) Handles btnDeleteRow.Click
        If dgvDetails.CurrentRow IsNot Nothing AndAlso Not dgvDetails.CurrentRow.IsNewRow Then
            dgvDetails.Rows.Remove(dgvDetails.CurrentRow)
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        MessageBox.Show("Rate table details saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        MessageBox.Show("Export to CSV would happen here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnImport_Click(sender As Object, e As EventArgs) Handles btnImport.Click
        MessageBox.Show("Import from CSV would happen here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
