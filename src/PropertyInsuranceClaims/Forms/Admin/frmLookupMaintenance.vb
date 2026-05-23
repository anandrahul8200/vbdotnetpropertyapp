Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Lookup value maintenance — manage dropdown/reference data.
''' </summary>
Public Class frmLookupMaintenance
    Inherits Form

    Private cboCategory As New ComboBox()
    Private dgvLookups As New DataGridView()
    Private WithEvents btnAdd As New Button()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnRefresh As New Button()

    Private Sub frmLookupMaintenance_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Lookup Maintenance"
        Me.Size = New Drawing.Size(700, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadCategories()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 50}
        pnlTop.Controls.Add(New Label() With {.Text = "Category:", .Location = New Drawing.Point(10, 15), .AutoSize = True})
        cboCategory.Location = New Drawing.Point(80, 12) : cboCategory.Size = New Drawing.Size(200, 20) : cboCategory.DropDownStyle = ComboBoxStyle.DropDownList
        AddHandler cboCategory.SelectedIndexChanged, AddressOf cboCategory_Changed
        pnlTop.Controls.Add(cboCategory)
        btnRefresh.Location = New Drawing.Point(300, 10) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        btnAdd.Location = New Drawing.Point(400, 10) : btnAdd.Size = New Drawing.Size(80, 28) : btnAdd.Text = "&Add" : pnlTop.Controls.Add(btnAdd)
        btnSave.Location = New Drawing.Point(490, 10) : btnSave.Size = New Drawing.Size(80, 28) : btnSave.Text = "&Save" : pnlTop.Controls.Add(btnSave)
        Me.Controls.Add(pnlTop)

        dgvLookups.Dock = DockStyle.Fill : dgvLookups.AllowUserToAddRows = False
        dgvLookups.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvLookups.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvLookups)
        dgvLookups.BringToFront()
    End Sub

    Private Sub LoadCategories()
        cboCategory.Items.Clear()
        cboCategory.Items.AddRange({"CLAIM_TYPE", "POLICY_TYPE", "CONSTRUCTION_TYPE", "ROOF_TYPE", "OCCUPANCY_TYPE",
                                    "PROPERTY_TYPE", "PAYMENT_METHOD", "VENDOR_TYPE", "WEATHER_CONDITION", "FLOOD_ZONE"})
        If cboCategory.Items.Count > 0 Then cboCategory.SelectedIndex = 0
    End Sub

    Private Sub cboCategory_Changed(sender As Object, e As EventArgs)
        If cboCategory.SelectedItem Is Nothing Then Return
        LoadLookupValues(cboCategory.SelectedItem.ToString())
    End Sub

    Private Sub LoadLookupValues(category As String)
        Try
            Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@Category", category)}
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Admin.usp_Lookup_GetByCategory", params)
            dgvLookups.DataSource = dt
        Catch ex As Exception
            ErrorLogger.LogError(ex, "LoadLookupValues")
        End Try
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If cboCategory.SelectedItem Is Nothing Then Return
        Dim code As String = InputBox("Enter lookup code:", "New Lookup")
        If String.IsNullOrWhiteSpace(code) Then Return
        Dim value As String = InputBox("Enter display value:", "New Lookup")
        If String.IsNullOrWhiteSpace(value) Then Return

        Try
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@Category", cboCategory.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@LookupCode", code.Trim().ToUpper()),
                DatabaseHelper.CreateParam("@LookupValue", value.Trim()),
                DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Admin.usp_Lookup_Create", params)
            LoadLookupValues(cboCategory.SelectedItem.ToString())
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        MessageBox.Show("Changes saved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        If cboCategory.SelectedItem IsNot Nothing Then LoadLookupValues(cboCategory.SelectedItem.ToString())
    End Sub
End Class
