Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Policy search form with filters and paginated grid.
''' </summary>
Public Class frmPolicySearch
    Inherits Form

    Private txtPolicyNumber As New TextBox()
    Private txtCustomerName As New TextBox()
    Private cboPolicyType As New ComboBox()
    Private cboPolicyStatus As New ComboBox()
    Private cboState As New ComboBox()
    Private dtpEffectiveFrom As New DateTimePicker()
    Private dtpEffectiveTo As New DateTimePicker()
    Private WithEvents chkDateFilter As New CheckBox()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnNewQuote As New Button()
    Private WithEvents btnOpen As New Button()
    Private WithEvents dgvResults As New DataGridView()
    Private lblRecordCount As New Label()
    Private WithEvents btnPrevPage As New Button()
    Private WithEvents btnNextPage As New Button()
    Private lblPage As New Label()

    Private _criteria As New PolicySearchCriteria()
    Private _selectedPolicyID As Integer = 0

    ''' <summary>When True, the form acts as a picker — Open/double-click selects and closes.</summary>
    Public Property IsPickerMode As Boolean = False

    ''' <summary>Gets the selected policy ID (for caller forms).</summary>
    Public ReadOnly Property SelectedPolicyID As Integer
        Get
            Return _selectedPolicyID
        End Get
    End Property

    Private Sub frmPolicySearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Policy Search"
            Me.Size = New Drawing.Size(1100, 600)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadLookups()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPolicySearch_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim pnlSearch As New Panel() With {.Dock = DockStyle.Top, .Height = 85}

        AddLabelTo(pnlSearch, "Policy #:", 10, 12)
        txtPolicyNumber.Location = New Drawing.Point(80, 9) : txtPolicyNumber.Size = New Drawing.Size(120, 20) : pnlSearch.Controls.Add(txtPolicyNumber)
        AddLabelTo(pnlSearch, "Customer:", 215, 12)
        txtCustomerName.Location = New Drawing.Point(290, 9) : txtCustomerName.Size = New Drawing.Size(150, 20) : pnlSearch.Controls.Add(txtCustomerName)
        AddLabelTo(pnlSearch, "Type:", 460, 12)
        cboPolicyType.Location = New Drawing.Point(500, 9) : cboPolicyType.Size = New Drawing.Size(100, 20) : cboPolicyType.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboPolicyType)
        AddLabelTo(pnlSearch, "Status:", 620, 12)
        cboPolicyStatus.Location = New Drawing.Point(670, 9) : cboPolicyStatus.Size = New Drawing.Size(120, 20) : cboPolicyStatus.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboPolicyStatus)

        chkDateFilter.Location = New Drawing.Point(10, 42) : chkDateFilter.Text = "Effective:" : chkDateFilter.AutoSize = True : pnlSearch.Controls.Add(chkDateFilter)
        dtpEffectiveFrom.Location = New Drawing.Point(100, 39) : dtpEffectiveFrom.Size = New Drawing.Size(110, 20) : dtpEffectiveFrom.Format = DateTimePickerFormat.Short : dtpEffectiveFrom.Enabled = False : pnlSearch.Controls.Add(dtpEffectiveFrom)
        AddLabelTo(pnlSearch, "to", 215, 42)
        dtpEffectiveTo.Location = New Drawing.Point(235, 39) : dtpEffectiveTo.Size = New Drawing.Size(110, 20) : dtpEffectiveTo.Format = DateTimePickerFormat.Short : dtpEffectiveTo.Enabled = False : pnlSearch.Controls.Add(dtpEffectiveTo)
        AddLabelTo(pnlSearch, "State:", 370, 42)
        cboState.Location = New Drawing.Point(410, 39) : cboState.Size = New Drawing.Size(60, 20) : cboState.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboState)

        btnSearch.Location = New Drawing.Point(850, 10) : btnSearch.Size = New Drawing.Size(80, 30) : btnSearch.Text = "&Search" : pnlSearch.Controls.Add(btnSearch)
        btnClear.Location = New Drawing.Point(940, 10) : btnClear.Size = New Drawing.Size(80, 30) : btnClear.Text = "&Clear" : pnlSearch.Controls.Add(btnClear)
        Me.Controls.Add(pnlSearch)

        dgvResults.Dock = DockStyle.Fill
        dgvResults.ReadOnly = True : dgvResults.AllowUserToAddRows = False
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResults.MultiSelect = False : dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvResults)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnNewQuote.Location = New Drawing.Point(10, 8) : btnNewQuote.Size = New Drawing.Size(100, 30) : btnNewQuote.Text = "New &Quote" : pnlBottom.Controls.Add(btnNewQuote)
        btnOpen.Location = New Drawing.Point(120, 8) : btnOpen.Size = New Drawing.Size(80, 30) : btnOpen.Text = "&Open" : btnOpen.Enabled = False : pnlBottom.Controls.Add(btnOpen)
        lblRecordCount.Location = New Drawing.Point(350, 14) : lblRecordCount.AutoSize = True : pnlBottom.Controls.Add(lblRecordCount)
        btnPrevPage.Location = New Drawing.Point(700, 8) : btnPrevPage.Size = New Drawing.Size(30, 30) : btnPrevPage.Text = "<" : pnlBottom.Controls.Add(btnPrevPage)
        lblPage.Location = New Drawing.Point(740, 14) : lblPage.AutoSize = True : lblPage.Text = "Page 1" : pnlBottom.Controls.Add(lblPage)
        btnNextPage.Location = New Drawing.Point(810, 8) : btnNextPage.Size = New Drawing.Size(30, 30) : btnNextPage.Text = ">" : pnlBottom.Controls.Add(btnNextPage)
        Me.Controls.Add(pnlBottom)
        dgvResults.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadLookups()
        cboPolicyType.Items.AddRange({"(All)", "HO3", "HO4", "HO6", "DP3", "BOP"})
        cboPolicyType.SelectedIndex = 0
        cboPolicyStatus.Items.AddRange({"(All)", "QUOTE", "REFERRED", "BOUND", "ACTIVE", "PENDING_CANCEL", "CANCELLED", "EXPIRED"})
        cboPolicyStatus.SelectedIndex = 0
        cboState.Items.Add("")
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            _criteria.PolicyNumber = If(String.IsNullOrWhiteSpace(txtPolicyNumber.Text), Nothing, txtPolicyNumber.Text.Trim())
            _criteria.CustomerName = If(String.IsNullOrWhiteSpace(txtCustomerName.Text), Nothing, txtCustomerName.Text.Trim())
            _criteria.PolicyType = If(cboPolicyType.SelectedIndex > 0, cboPolicyType.SelectedItem.ToString(), Nothing)
            _criteria.PolicyStatus = If(cboPolicyStatus.SelectedIndex > 0, cboPolicyStatus.SelectedItem.ToString(), Nothing)
            _criteria.EffectiveDateFrom = If(chkDateFilter.Checked, dtpEffectiveFrom.Value.Date, CType(Nothing, Date?))
            _criteria.EffectiveDateTo = If(chkDateFilter.Checked, dtpEffectiveTo.Value.Date, CType(Nothing, Date?))
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
        Dim dt As DataTable = PolicyDataAccess.Search(_criteria)
        dgvResults.DataSource = dt
        If dgvResults.Columns.Contains("PolicyID") Then dgvResults.Columns("PolicyID").Visible = False
        lblRecordCount.Text = $"{_criteria.TotalRecords} record(s)"
        lblPage.Text = $"Page {_criteria.PageNumber}"
        btnPrevPage.Enabled = _criteria.PageNumber > 1
        btnNextPage.Enabled = (_criteria.PageNumber * _criteria.PageSize) < _criteria.TotalRecords
        btnOpen.Enabled = dt.Rows.Count > 0
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtPolicyNumber.Clear() : txtCustomerName.Clear()
        cboPolicyType.SelectedIndex = 0 : cboPolicyStatus.SelectedIndex = 0
        chkDateFilter.Checked = False : dgvResults.DataSource = Nothing
        lblRecordCount.Text = "" : btnOpen.Enabled = False
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        If dgvResults.CurrentRow Is Nothing Then Return
        Dim policyID As Integer = CInt(dgvResults.CurrentRow.Cells("PolicyID").Value)

        If IsPickerMode OrElse Me.Modal Then
            _selectedPolicyID = policyID
            Me.Close()
            Return
        End If

        Dim frm As New frmPolicyView(policyID)
        frm.MdiParent = Me.MdiParent
        frm.Show()
    End Sub

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        If e.RowIndex >= 0 Then btnOpen.PerformClick()
    End Sub

    Private Sub btnNewQuote_Click(sender As Object, e As EventArgs) Handles btnNewQuote.Click
        Dim frm As New frmPolicyEntry(0)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnNextPage_Click(sender As Object, e As EventArgs) Handles btnNextPage.Click
        _criteria.PageNumber += 1 : ExecuteSearch()
    End Sub

    Private Sub btnPrevPage_Click(sender As Object, e As EventArgs) Handles btnPrevPage.Click
        If _criteria.PageNumber > 1 Then _criteria.PageNumber -= 1
        ExecuteSearch()
    End Sub

    Private Sub chkDateFilter_CheckedChanged(sender As Object, e As EventArgs) Handles chkDateFilter.CheckedChanged
        dtpEffectiveFrom.Enabled = chkDateFilter.Checked : dtpEffectiveTo.Enabled = chkDateFilter.Checked
    End Sub
End Class
