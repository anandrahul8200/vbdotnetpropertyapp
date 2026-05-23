Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Claim search form with filters and paginated grid.
''' </summary>
Public Class frmClaimSearch
    Inherits Form

    ' --- Controls ---
    Private txtClaimNumber As New TextBox()
    Private txtPolicyNumber As New TextBox()
    Private txtCustomerName As New TextBox()
    Private cboClaimStatus As New ComboBox()
    Private cboClaimType As New ComboBox()
    Private cboPriority As New ComboBox()
    Private dtpLossDateFrom As New DateTimePicker()
    Private dtpLossDateTo As New DateTimePicker()
    Private WithEvents chkDateFilter As New CheckBox()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnNewClaim As New Button()
    Private WithEvents btnOpenClaim As New Button()
    Private WithEvents dgvResults As New DataGridView()
    Private lblRecordCount As New Label()
    Private WithEvents btnPrevPage As New Button()
    Private WithEvents btnNextPage As New Button()
    Private lblPage As New Label()

    Private _criteria As New ClaimSearchCriteria()

    Private Sub frmClaimSearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Claim Search"
            Me.Size = New Drawing.Size(1100, 650)
            Me.StartPosition = FormStartPosition.CenterParent

            InitializeControls()
            LoadLookups()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimSearch_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Search panel
        Dim pnlSearch As New Panel() With {.Dock = DockStyle.Top, .Height = 90}

        Dim y1 As Integer = 12
        AddLabelTo(pnlSearch, "Claim #:", 10, y1) : txtClaimNumber.Location = New Drawing.Point(75, y1 - 3) : txtClaimNumber.Size = New Drawing.Size(120, 20) : pnlSearch.Controls.Add(txtClaimNumber)
        AddLabelTo(pnlSearch, "Policy #:", 210, y1) : txtPolicyNumber.Location = New Drawing.Point(275, y1 - 3) : txtPolicyNumber.Size = New Drawing.Size(120, 20) : pnlSearch.Controls.Add(txtPolicyNumber)
        AddLabelTo(pnlSearch, "Customer:", 410, y1) : txtCustomerName.Location = New Drawing.Point(480, y1 - 3) : txtCustomerName.Size = New Drawing.Size(150, 20) : pnlSearch.Controls.Add(txtCustomerName)
        AddLabelTo(pnlSearch, "Status:", 650, y1) : cboClaimStatus.Location = New Drawing.Point(700, y1 - 3) : cboClaimStatus.Size = New Drawing.Size(130, 20) : cboClaimStatus.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboClaimStatus)

        Dim y2 As Integer = 42
        AddLabelTo(pnlSearch, "Type:", 10, y2) : cboClaimType.Location = New Drawing.Point(75, y2 - 3) : cboClaimType.Size = New Drawing.Size(150, 20) : cboClaimType.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboClaimType)
        AddLabelTo(pnlSearch, "Priority:", 240, y2) : cboPriority.Location = New Drawing.Point(300, y2 - 3) : cboPriority.Size = New Drawing.Size(100, 20) : cboPriority.DropDownStyle = ComboBoxStyle.DropDownList : pnlSearch.Controls.Add(cboPriority)

        chkDateFilter.Location = New Drawing.Point(420, y2) : chkDateFilter.Text = "Loss Date:" : chkDateFilter.AutoSize = True : pnlSearch.Controls.Add(chkDateFilter)
        dtpLossDateFrom.Location = New Drawing.Point(520, y2 - 3) : dtpLossDateFrom.Size = New Drawing.Size(110, 20) : dtpLossDateFrom.Format = DateTimePickerFormat.Short : dtpLossDateFrom.Enabled = False : pnlSearch.Controls.Add(dtpLossDateFrom)
        AddLabelTo(pnlSearch, "to", 635, y2)
        dtpLossDateTo.Location = New Drawing.Point(655, y2 - 3) : dtpLossDateTo.Size = New Drawing.Size(110, 20) : dtpLossDateTo.Format = DateTimePickerFormat.Short : dtpLossDateTo.Enabled = False : pnlSearch.Controls.Add(dtpLossDateTo)

        btnSearch.Location = New Drawing.Point(850, 15) : btnSearch.Size = New Drawing.Size(80, 30) : btnSearch.Text = "&Search" : pnlSearch.Controls.Add(btnSearch)
        btnClear.Location = New Drawing.Point(940, 15) : btnClear.Size = New Drawing.Size(80, 30) : btnClear.Text = "&Clear" : pnlSearch.Controls.Add(btnClear)

        Me.Controls.Add(pnlSearch)

        ' Grid
        dgvResults.Dock = DockStyle.Fill
        dgvResults.ReadOnly = True
        dgvResults.AllowUserToAddRows = False
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResults.MultiSelect = False
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvResults)

        ' Bottom panel
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnNewClaim.Location = New Drawing.Point(10, 8) : btnNewClaim.Size = New Drawing.Size(100, 30) : btnNewClaim.Text = "&New FNOL" : pnlBottom.Controls.Add(btnNewClaim)
        btnOpenClaim.Location = New Drawing.Point(120, 8) : btnOpenClaim.Size = New Drawing.Size(100, 30) : btnOpenClaim.Text = "&Open" : btnOpenClaim.Enabled = False : pnlBottom.Controls.Add(btnOpenClaim)
        lblRecordCount.Location = New Drawing.Point(350, 14) : lblRecordCount.AutoSize = True : pnlBottom.Controls.Add(lblRecordCount)
        btnPrevPage.Location = New Drawing.Point(700, 8) : btnPrevPage.Size = New Drawing.Size(30, 30) : btnPrevPage.Text = "<" : pnlBottom.Controls.Add(btnPrevPage)
        lblPage.Location = New Drawing.Point(740, 14) : lblPage.AutoSize = True : lblPage.Text = "Page 1" : pnlBottom.Controls.Add(lblPage)
        btnNextPage.Location = New Drawing.Point(810, 8) : btnNextPage.Size = New Drawing.Size(30, 30) : btnNextPage.Text = ">" : pnlBottom.Controls.Add(btnNextPage)
        Me.Controls.Add(pnlBottom)

        dgvResults.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        Dim lbl As New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True}
        parent.Controls.Add(lbl)
    End Sub

    Private Sub LoadLookups()
        cboClaimStatus.Items.AddRange({"(All)", "FNOL", "ASSIGNED", "INVESTIGATING", "ASSESSED", "APPROVED", "DENIED", "SETTLED", "CLOSED", "REOPENED", "LITIGATION"})
        cboClaimStatus.SelectedIndex = 0

        cboClaimType.Items.AddRange({"(All)", "PROPERTY_DAMAGE", "THEFT", "LIABILITY", "WATER_DAMAGE", "FIRE", "WIND", "HAIL", "OTHER"})
        cboClaimType.SelectedIndex = 0

        cboPriority.Items.AddRange({"(All)", "LOW", "NORMAL", "HIGH", "CRITICAL"})
        cboPriority.SelectedIndex = 0
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            _criteria.ClaimNumber = If(String.IsNullOrWhiteSpace(txtClaimNumber.Text), Nothing, txtClaimNumber.Text.Trim())
            _criteria.PolicyNumber = If(String.IsNullOrWhiteSpace(txtPolicyNumber.Text), Nothing, txtPolicyNumber.Text.Trim())
            _criteria.CustomerName = If(String.IsNullOrWhiteSpace(txtCustomerName.Text), Nothing, txtCustomerName.Text.Trim())
            _criteria.ClaimStatus = If(cboClaimStatus.SelectedIndex > 0, cboClaimStatus.SelectedItem.ToString(), Nothing)
            _criteria.ClaimType = If(cboClaimType.SelectedIndex > 0, cboClaimType.SelectedItem.ToString(), Nothing)
            _criteria.Priority = If(cboPriority.SelectedIndex > 0, cboPriority.SelectedItem.ToString(), Nothing)
            _criteria.LossDateFrom = If(chkDateFilter.Checked, dtpLossDateFrom.Value.Date, CType(Nothing, Date?))
            _criteria.LossDateTo = If(chkDateFilter.Checked, dtpLossDateTo.Value.Date, CType(Nothing, Date?))
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
        Dim dt As DataTable = ClaimDataAccess.Search(_criteria)
        dgvResults.DataSource = dt
        If dgvResults.Columns.Contains("ClaimID") Then dgvResults.Columns("ClaimID").Visible = False

        lblRecordCount.Text = $"{_criteria.TotalRecords} record(s) found"
        lblPage.Text = $"Page {_criteria.PageNumber}"
        btnPrevPage.Enabled = _criteria.PageNumber > 1
        btnNextPage.Enabled = (_criteria.PageNumber * _criteria.PageSize) < _criteria.TotalRecords
        btnOpenClaim.Enabled = dt.Rows.Count > 0
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtClaimNumber.Clear() : txtPolicyNumber.Clear() : txtCustomerName.Clear()
        cboClaimStatus.SelectedIndex = 0 : cboClaimType.SelectedIndex = 0 : cboPriority.SelectedIndex = 0
        chkDateFilter.Checked = False
        dgvResults.DataSource = Nothing : lblRecordCount.Text = "" : btnOpenClaim.Enabled = False
    End Sub

    Private Sub btnNewClaim_Click(sender As Object, e As EventArgs) Handles btnNewClaim.Click
        Dim frm As New frmClaimFNOL()
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnOpenClaim_Click(sender As Object, e As EventArgs) Handles btnOpenClaim.Click
        If dgvResults.CurrentRow Is Nothing Then Return
        Dim claimID As Integer = CInt(dgvResults.CurrentRow.Cells("ClaimID").Value)
        Dim frm As New frmClaimView(claimID)
        frm.ShowDialog(Me)
    End Sub

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        If e.RowIndex >= 0 Then btnOpenClaim.PerformClick()
    End Sub

    Private Sub btnNextPage_Click(sender As Object, e As EventArgs) Handles btnNextPage.Click
        _criteria.PageNumber += 1 : ExecuteSearch()
    End Sub

    Private Sub btnPrevPage_Click(sender As Object, e As EventArgs) Handles btnPrevPage.Click
        If _criteria.PageNumber > 1 Then _criteria.PageNumber -= 1
        ExecuteSearch()
    End Sub

    Private Sub chkDateFilter_CheckedChanged(sender As Object, e As EventArgs) Handles chkDateFilter.CheckedChanged
        dtpLossDateFrom.Enabled = chkDateFilter.Checked
        dtpLossDateTo.Enabled = chkDateFilter.Checked
    End Sub

End Class
