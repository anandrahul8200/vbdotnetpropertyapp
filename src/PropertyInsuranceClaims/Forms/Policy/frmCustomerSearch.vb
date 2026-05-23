Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Customer search form with grid results and pagination.
''' </summary>
Public Class frmCustomerSearch
    Inherits Form

    ' --- Controls ---
    Private WithEvents txtSearch As New TextBox()
    Private WithEvents cboCustomerType As New ComboBox()
    Private WithEvents cboState As New ComboBox()
    Private WithEvents txtCity As New TextBox()
    Private WithEvents txtZipCode As New TextBox()
    Private WithEvents chkActiveOnly As New CheckBox()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnOpen As New Button()
    Private WithEvents dgvResults As New DataGridView()
    Private lblRecordCount As New Label()
    Private WithEvents btnPrevPage As New Button()
    Private WithEvents btnNextPage As New Button()
    Private lblPage As New Label()

    ' --- State ---
    Private _criteria As New CustomerSearchCriteria()
    Private _selectedCustomerID As Integer = 0

    ''' <summary>
    ''' Gets the selected customer ID (for caller forms).
    ''' </summary>
    Public ReadOnly Property SelectedCustomerID As Integer
        Get
            Return _selectedCustomerID
        End Get
    End Property

    ' --- Form Load ---
    Private Sub frmCustomerSearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Customer Search"
            Me.Size = New Drawing.Size(1000, 600)
            Me.StartPosition = FormStartPosition.CenterParent

            InitializeControls()
            LoadLookups()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmCustomerSearch_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Search panel
        Dim pnlSearch As New Panel() With {.Dock = DockStyle.Top, .Height = 100}

        txtSearch.Location = New Drawing.Point(80, 15)
        txtSearch.Size = New Drawing.Size(200, 20)
        Dim lblSearch As New Label() With {.Text = "Search:", .Location = New Drawing.Point(10, 18), .AutoSize = True}

        cboCustomerType.Location = New Drawing.Point(370, 15)
        cboCustomerType.Size = New Drawing.Size(120, 20)
        cboCustomerType.DropDownStyle = ComboBoxStyle.DropDownList
        Dim lblType As New Label() With {.Text = "Type:", .Location = New Drawing.Point(320, 18), .AutoSize = True}

        cboState.Location = New Drawing.Point(570, 15)
        cboState.Size = New Drawing.Size(80, 20)
        cboState.DropDownStyle = ComboBoxStyle.DropDownList
        Dim lblState As New Label() With {.Text = "State:", .Location = New Drawing.Point(520, 18), .AutoSize = True}

        txtCity.Location = New Drawing.Point(80, 50)
        txtCity.Size = New Drawing.Size(150, 20)
        Dim lblCity As New Label() With {.Text = "City:", .Location = New Drawing.Point(10, 53), .AutoSize = True}

        txtZipCode.Location = New Drawing.Point(320, 50)
        txtZipCode.Size = New Drawing.Size(80, 20)
        Dim lblZip As New Label() With {.Text = "Zip:", .Location = New Drawing.Point(280, 53), .AutoSize = True}

        chkActiveOnly.Location = New Drawing.Point(450, 50)
        chkActiveOnly.Text = "Active Only"
        chkActiveOnly.Checked = True

        btnSearch.Location = New Drawing.Point(600, 45)
        btnSearch.Size = New Drawing.Size(80, 30)
        btnSearch.Text = "&Search"

        btnClear.Location = New Drawing.Point(690, 45)
        btnClear.Size = New Drawing.Size(80, 30)
        btnClear.Text = "&Clear"

        pnlSearch.Controls.AddRange({lblSearch, txtSearch, lblType, cboCustomerType, lblState, cboState,
                                     lblCity, txtCity, lblZip, txtZipCode, chkActiveOnly, btnSearch, btnClear})
        Me.Controls.Add(pnlSearch)

        ' Grid
        dgvResults.Dock = DockStyle.Fill
        dgvResults.ReadOnly = True
        dgvResults.AllowUserToAddRows = False
        dgvResults.AllowUserToDeleteRows = False
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResults.MultiSelect = False
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvResults)

        ' Bottom panel
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnNew.Location = New Drawing.Point(10, 8)
        btnNew.Size = New Drawing.Size(100, 30)
        btnNew.Text = "&New Customer"

        btnOpen.Location = New Drawing.Point(120, 8)
        btnOpen.Size = New Drawing.Size(100, 30)
        btnOpen.Text = "&Open"
        btnOpen.Enabled = False

        lblRecordCount.Location = New Drawing.Point(350, 14)
        lblRecordCount.AutoSize = True

        btnPrevPage.Location = New Drawing.Point(600, 8)
        btnPrevPage.Size = New Drawing.Size(30, 30)
        btnPrevPage.Text = "<"
        btnPrevPage.Enabled = False

        lblPage.Location = New Drawing.Point(640, 14)
        lblPage.AutoSize = True
        lblPage.Text = "Page 1"

        btnNextPage.Location = New Drawing.Point(710, 8)
        btnNextPage.Size = New Drawing.Size(30, 30)
        btnNextPage.Text = ">"
        btnNextPage.Enabled = False

        pnlBottom.Controls.AddRange({btnNew, btnOpen, lblRecordCount, btnPrevPage, lblPage, btnNextPage})
        Me.Controls.Add(pnlBottom)

        dgvResults.BringToFront()
    End Sub

    Private Sub LoadLookups()
        cboCustomerType.Items.Clear()
        cboCustomerType.Items.Add(New With {.Text = "(All)", .Value = ""})
        cboCustomerType.Items.Add(New With {.Text = "Individual", .Value = "I"})
        cboCustomerType.Items.Add(New With {.Text = "Commercial", .Value = "C"})
        cboCustomerType.DisplayMember = "Text"
        cboCustomerType.ValueMember = "Value"
        cboCustomerType.SelectedIndex = 0

        ' States would be loaded from DB in production
        cboState.Items.Add("")
    End Sub

    ' --- Search ---
    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            _criteria.SearchTerm = If(String.IsNullOrWhiteSpace(txtSearch.Text), Nothing, txtSearch.Text.Trim())
            _criteria.City = If(String.IsNullOrWhiteSpace(txtCity.Text), Nothing, txtCity.Text.Trim())
            _criteria.ZipCode = If(String.IsNullOrWhiteSpace(txtZipCode.Text), Nothing, txtZipCode.Text.Trim())
            _criteria.IsActive = If(chkActiveOnly.Checked, True, Nothing)
            _criteria.PageNumber = 1

            ExecuteSearch()
        Catch ex As Exception
            MessageBox.Show("Search error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSearch_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub ExecuteSearch()
        Dim dt As DataTable = CustomerDataAccess.Search(_criteria)
        dgvResults.DataSource = dt

        ' Hide ID column
        If dgvResults.Columns.Contains("CustomerID") Then dgvResults.Columns("CustomerID").Visible = False

        lblRecordCount.Text = $"{_criteria.TotalRecords} record(s) found"
        lblPage.Text = $"Page {_criteria.PageNumber}"
        btnPrevPage.Enabled = _criteria.PageNumber > 1
        btnNextPage.Enabled = (_criteria.PageNumber * _criteria.PageSize) < _criteria.TotalRecords
        btnOpen.Enabled = dt.Rows.Count > 0
    End Sub

    ' --- Pagination ---
    Private Sub btnNextPage_Click(sender As Object, e As EventArgs) Handles btnNextPage.Click
        _criteria.PageNumber += 1
        ExecuteSearch()
    End Sub

    Private Sub btnPrevPage_Click(sender As Object, e As EventArgs) Handles btnPrevPage.Click
        If _criteria.PageNumber > 1 Then _criteria.PageNumber -= 1
        ExecuteSearch()
    End Sub

    ' --- Clear ---
    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtSearch.Clear()
        txtCity.Clear()
        txtZipCode.Clear()
        cboCustomerType.SelectedIndex = 0
        cboState.SelectedIndex = 0
        chkActiveOnly.Checked = True
        dgvResults.DataSource = Nothing
        lblRecordCount.Text = ""
        btnOpen.Enabled = False
    End Sub

    ' --- Open Customer ---
    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        OpenSelectedCustomer()
    End Sub

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        If e.RowIndex >= 0 Then OpenSelectedCustomer()
    End Sub

    Private Sub OpenSelectedCustomer()
        If dgvResults.CurrentRow Is Nothing Then Return
        _selectedCustomerID = CInt(dgvResults.CurrentRow.Cells("CustomerID").Value)

        ' If opened as a dialog (picker mode), just set the ID and close
        If Me.Modal Then
            Me.Close()
            Return
        End If

        ' Otherwise open the edit form
        Dim frm As New frmCustomerEntry(_selectedCustomerID)
        frm.ShowDialog(Me)

        ' Refresh after edit
        If _criteria.TotalRecords > 0 Then ExecuteSearch()
    End Sub

    ' --- New Customer ---
    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        Dim frm As New frmCustomerEntry(0)
        frm.ShowDialog(Me)
    End Sub

    ' --- Enter key triggers search ---
    Private Sub txtSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then btnSearch.PerformClick()
    End Sub

End Class
