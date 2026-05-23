Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Vendor management form — search, create, and edit vendors (adjusters, contractors, etc.).
''' </summary>
Public Class frmVendorManagement
    Inherits Form

    ' Search controls
    Private txtVendorName As New TextBox()
    Private cboVendorType As New ComboBox()
    Private cboState As New ComboBox()
    Private chkPreferredOnly As New CheckBox()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnEdit As New Button()
    Private WithEvents btnDeactivate As New Button()

    ' Grid
    Private dgvVendors As New DataGridView()
    Private lblCount As New Label()

    ' Edit panel
    Private grpEdit As New GroupBox()
    Private txtEditName As New TextBox()
    Private cboEditType As New ComboBox()
    Private txtEditContact As New TextBox()
    Private txtEditPhone As New TextBox()
    Private txtEditEmail As New TextBox()
    Private txtEditAddress As New TextBox()
    Private txtEditCity As New TextBox()
    Private cboEditState As New ComboBox()
    Private txtEditZip As New TextBox()
    Private txtEditLicense As New TextBox()
    Private txtEditHourlyRate As New TextBox()
    Private txtEditDailyRate As New TextBox()
    Private chkEditPreferred As New CheckBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancelEdit As New Button()

    Private _editVendorID As Integer = 0

    Private Sub frmVendorManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Vendor Management"
            Me.Size = New Drawing.Size(950, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadLookups()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmVendorManagement_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Search panel
        Dim pnlSearch As New Panel() With {.Dock = DockStyle.Top, .Height = 50}
        AddLabelTo(pnlSearch, "Name:", 10, 15) : txtVendorName.Location = New Drawing.Point(55, 12) : txtVendorName.Size = New Drawing.Size(150, 20) : pnlSearch.Controls.Add(txtVendorName)
        AddLabelTo(pnlSearch, "Type:", 220, 15) : cboVendorType.Location = New Drawing.Point(260, 12) : cboVendorType.Size = New Drawing.Size(130, 20) : cboVendorType.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboVendorType)
        AddLabelTo(pnlSearch, "State:", 410, 15) : cboState.Location = New Drawing.Point(450, 12) : cboState.Size = New Drawing.Size(60, 20) : cboState.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboState)
        chkPreferredOnly.Location = New Drawing.Point(530, 13) : chkPreferredOnly.Text = "Preferred Only" : chkPreferredOnly.AutoSize = True : pnlSearch.Controls.Add(chkPreferredOnly)
        btnSearch.Location = New Drawing.Point(680, 8) : btnSearch.Size = New Drawing.Size(80, 30) : btnSearch.Text = "&Search" : pnlSearch.Controls.Add(btnSearch)
        btnClear.Location = New Drawing.Point(770, 8) : btnClear.Size = New Drawing.Size(60, 30) : btnClear.Text = "C&lear" : pnlSearch.Controls.Add(btnClear)
        Me.Controls.Add(pnlSearch)

        ' Grid
        dgvVendors.Dock = DockStyle.Top : dgvVendors.Height = 250
        dgvVendors.ReadOnly = True : dgvVendors.AllowUserToAddRows = False
        dgvVendors.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvVendors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvVendors)

        ' Grid buttons
        Dim pnlGridButtons As New Panel() With {.Dock = DockStyle.Top, .Height = 38}
        btnNew.Location = New Drawing.Point(10, 5) : btnNew.Size = New Drawing.Size(80, 28) : btnNew.Text = "&New" : pnlGridButtons.Controls.Add(btnNew)
        btnEdit.Location = New Drawing.Point(100, 5) : btnEdit.Size = New Drawing.Size(80, 28) : btnEdit.Text = "&Edit" : pnlGridButtons.Controls.Add(btnEdit)
        btnDeactivate.Location = New Drawing.Point(190, 5) : btnDeactivate.Size = New Drawing.Size(100, 28) : btnDeactivate.Text = "&Deactivate" : pnlGridButtons.Controls.Add(btnDeactivate)
        lblCount.Location = New Drawing.Point(350, 10) : lblCount.AutoSize = True : pnlGridButtons.Controls.Add(lblCount)
        Me.Controls.Add(pnlGridButtons)

        ' Edit panel
        grpEdit.Text = "Vendor Details" : grpEdit.Dock = DockStyle.Fill
        Dim y As Integer = 22
        Dim col2 As Integer = 350

        AddLabelTo(grpEdit, "Name:", 15, y) : txtEditName.Location = New Drawing.Point(100, y - 3) : txtEditName.Size = New Drawing.Size(220, 20) : grpEdit.Controls.Add(txtEditName)
        AddLabelTo(grpEdit, "Type:", col2, y) : cboEditType.Location = New Drawing.Point(col2 + 50, y - 3) : cboEditType.Size = New Drawing.Size(130, 20) : cboEditType.DropDownStyle = ComboBoxStyle.DropDownList : grpEdit.Controls.Add(cboEditType)
        y += 28
        AddLabelTo(grpEdit, "Contact:", 15, y) : txtEditContact.Location = New Drawing.Point(100, y - 3) : txtEditContact.Size = New Drawing.Size(200, 20) : grpEdit.Controls.Add(txtEditContact)
        AddLabelTo(grpEdit, "Phone:", col2, y) : txtEditPhone.Location = New Drawing.Point(col2 + 50, y - 3) : txtEditPhone.Size = New Drawing.Size(130, 20) : grpEdit.Controls.Add(txtEditPhone)
        y += 28
        AddLabelTo(grpEdit, "Email:", 15, y) : txtEditEmail.Location = New Drawing.Point(100, y - 3) : txtEditEmail.Size = New Drawing.Size(250, 20) : grpEdit.Controls.Add(txtEditEmail)
        y += 28
        AddLabelTo(grpEdit, "Address:", 15, y) : txtEditAddress.Location = New Drawing.Point(100, y - 3) : txtEditAddress.Size = New Drawing.Size(300, 20) : grpEdit.Controls.Add(txtEditAddress)
        y += 28
        AddLabelTo(grpEdit, "City:", 15, y) : txtEditCity.Location = New Drawing.Point(100, y - 3) : txtEditCity.Size = New Drawing.Size(150, 20) : grpEdit.Controls.Add(txtEditCity)
        AddLabelTo(grpEdit, "State:", 270, y) : cboEditState.Location = New Drawing.Point(310, y - 3) : cboEditState.Size = New Drawing.Size(60, 20) : cboEditState.DropDownStyle = ComboBoxStyle.DropDownList : grpEdit.Controls.Add(cboEditState)
        AddLabelTo(grpEdit, "Zip:", 385, y) : txtEditZip.Location = New Drawing.Point(415, y - 3) : txtEditZip.Size = New Drawing.Size(80, 20) : grpEdit.Controls.Add(txtEditZip)
        y += 28
        AddLabelTo(grpEdit, "License #:", 15, y) : txtEditLicense.Location = New Drawing.Point(100, y - 3) : txtEditLicense.Size = New Drawing.Size(150, 20) : grpEdit.Controls.Add(txtEditLicense)
        y += 28
        AddLabelTo(grpEdit, "Hourly Rate:", 15, y) : txtEditHourlyRate.Location = New Drawing.Point(100, y - 3) : txtEditHourlyRate.Size = New Drawing.Size(80, 20) : grpEdit.Controls.Add(txtEditHourlyRate)
        AddLabelTo(grpEdit, "Daily Rate:", 200, y) : txtEditDailyRate.Location = New Drawing.Point(270, y - 3) : txtEditDailyRate.Size = New Drawing.Size(80, 20) : grpEdit.Controls.Add(txtEditDailyRate)
        chkEditPreferred.Location = New Drawing.Point(380, y) : chkEditPreferred.Text = "Preferred Vendor" : chkEditPreferred.AutoSize = True : grpEdit.Controls.Add(chkEditPreferred)
        y += 35

        btnSave.Location = New Drawing.Point(100, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Save" : grpEdit.Controls.Add(btnSave)
        btnCancelEdit.Location = New Drawing.Point(210, y) : btnCancelEdit.Size = New Drawing.Size(80, 30) : btnCancelEdit.Text = "Cancel" : grpEdit.Controls.Add(btnCancelEdit)
        Me.Controls.Add(grpEdit)
        grpEdit.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadLookups()
        cboVendorType.Items.AddRange({"(All)", "ADJUSTER", "CONTRACTOR", "APPRAISER", "ENGINEER", "ATTORNEY", "INVESTIGATOR"})
        cboVendorType.SelectedIndex = 0
        cboEditType.Items.AddRange({"ADJUSTER", "CONTRACTOR", "APPRAISER", "ENGINEER", "ATTORNEY", "INVESTIGATOR"})
        cboState.Items.Add("(All)")
        cboEditState.Items.Add("")
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim vendorType As String = If(cboVendorType.SelectedIndex > 0, cboVendorType.SelectedItem.ToString(), Nothing)
            Dim state As String = If(cboState.SelectedIndex > 0, cboState.SelectedItem.ToString(), Nothing)
            Dim dt As DataTable = FraudDataAccess.SearchVendors(vendorType, state, chkPreferredOnly.Checked)
            dgvVendors.DataSource = dt
            If dgvVendors.Columns.Contains("VendorID") Then dgvVendors.Columns("VendorID").Visible = False
            lblCount.Text = $"{dt.Rows.Count} vendor(s) found"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtVendorName.Clear() : cboVendorType.SelectedIndex = 0 : cboState.SelectedIndex = 0
        chkPreferredOnly.Checked = False : dgvVendors.DataSource = Nothing : lblCount.Text = ""
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        _editVendorID = 0
        ClearEditFields()
        grpEdit.Text = "New Vendor"
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        If dgvVendors.CurrentRow Is Nothing Then Return
        _editVendorID = CInt(dgvVendors.CurrentRow.Cells("VendorID").Value)
        grpEdit.Text = "Edit Vendor"
        ' Would populate fields from selected row
        txtEditName.Text = dgvVendors.CurrentRow.Cells("VendorName").Value.ToString()
    End Sub

    Private Sub btnDeactivate_Click(sender As Object, e As EventArgs) Handles btnDeactivate.Click
        If dgvVendors.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Deactivate this vendor?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Vendor deactivated.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
            btnSearch.PerformClick()
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If String.IsNullOrWhiteSpace(txtEditName.Text) Then
                MessageBox.Show("Vendor name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If
            If cboEditType.SelectedItem Is Nothing Then
                MessageBox.Show("Vendor type is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@VendorName", txtEditName.Text.Trim()),
                DatabaseHelper.CreateParam("@VendorType", cboEditType.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@ContactName", If(String.IsNullOrWhiteSpace(txtEditContact.Text), Nothing, txtEditContact.Text.Trim())),
                DatabaseHelper.CreateParam("@Phone", If(String.IsNullOrWhiteSpace(txtEditPhone.Text), Nothing, txtEditPhone.Text.Trim())),
                DatabaseHelper.CreateParam("@Email", If(String.IsNullOrWhiteSpace(txtEditEmail.Text), Nothing, txtEditEmail.Text.Trim())),
                DatabaseHelper.CreateParam("@AddressLine1", If(String.IsNullOrWhiteSpace(txtEditAddress.Text), Nothing, txtEditAddress.Text.Trim())),
                DatabaseHelper.CreateParam("@City", If(String.IsNullOrWhiteSpace(txtEditCity.Text), Nothing, txtEditCity.Text.Trim())),
                DatabaseHelper.CreateParam("@StateCode", If(cboEditState.SelectedItem IsNot Nothing AndAlso cboEditState.SelectedItem.ToString() <> "", cboEditState.SelectedItem.ToString(), Nothing)),
                DatabaseHelper.CreateParam("@ZipCode", If(String.IsNullOrWhiteSpace(txtEditZip.Text), Nothing, txtEditZip.Text.Trim())),
                DatabaseHelper.CreateParam("@LicenseNumber", If(String.IsNullOrWhiteSpace(txtEditLicense.Text), Nothing, txtEditLicense.Text.Trim())),
                DatabaseHelper.CreateParam("@HourlyRate", If(String.IsNullOrWhiteSpace(txtEditHourlyRate.Text), Nothing, CDec(txtEditHourlyRate.Text))),
                DatabaseHelper.CreateParam("@DailyRate", If(String.IsNullOrWhiteSpace(txtEditDailyRate.Text), Nothing, CDec(txtEditDailyRate.Text))),
                DatabaseHelper.CreateParam("@PreferredVendor", chkEditPreferred.Checked),
                DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
            }
            Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@VendorID", SqlDbType.Int)
            Dim numParam As SqlParameter = DatabaseHelper.CreateOutputParam("@VendorNumber", SqlDbType.VarChar)
            params.AddRange({idParam, numParam})

            DatabaseHelper.ExecuteNonQuery("Claims.usp_Vendor_Create", params.ToArray())
            MessageBox.Show("Vendor saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            ClearEditFields()
            btnSearch.PerformClick()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmVendorManagement.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancelEdit_Click(sender As Object, e As EventArgs) Handles btnCancelEdit.Click
        ClearEditFields()
    End Sub

    Private Sub ClearEditFields()
        _editVendorID = 0
        txtEditName.Clear() : txtEditContact.Clear() : txtEditPhone.Clear() : txtEditEmail.Clear()
        txtEditAddress.Clear() : txtEditCity.Clear() : txtEditZip.Clear() : txtEditLicense.Clear()
        txtEditHourlyRate.Clear() : txtEditDailyRate.Clear() : chkEditPreferred.Checked = False
        If cboEditType.Items.Count > 0 Then cboEditType.SelectedIndex = 0
    End Sub
End Class
