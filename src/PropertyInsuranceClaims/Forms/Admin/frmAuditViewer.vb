Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Audit trail viewer with search filters.
''' </summary>
Public Class frmAuditViewer
    Inherits Form

    Private txtTableName As New TextBox()
    Private txtUsername As New TextBox()
    Private cboAction As New ComboBox()
    Private dtpDateFrom As New DateTimePicker()
    Private dtpDateTo As New DateTimePicker()
    Private WithEvents btnSearch As New Button()
    Private dgvAudit As New DataGridView()
    Private lblRecordCount As New Label()

    Private Sub frmAuditViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Audit Trail Viewer"
        Me.Size = New Drawing.Size(1000, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlSearch As New Panel() With {.Dock = DockStyle.Top, .Height = 50}
        AddLabelTo(pnlSearch, "Table:", 10, 15) : txtTableName.Location = New Drawing.Point(55, 12) : txtTableName.Size = New Drawing.Size(130, 20) : pnlSearch.Controls.Add(txtTableName)
        AddLabelTo(pnlSearch, "User:", 200, 15) : txtUsername.Location = New Drawing.Point(240, 12) : txtUsername.Size = New Drawing.Size(100, 20) : pnlSearch.Controls.Add(txtUsername)
        AddLabelTo(pnlSearch, "Action:", 355, 15) : cboAction.Location = New Drawing.Point(405, 12) : cboAction.Size = New Drawing.Size(90, 20) : cboAction.DropDownStyle = ComboBoxStyle.DropDownList
        cboAction.Items.AddRange({"(All)", "INSERT", "UPDATE", "DELETE", "LOGIN"}) : cboAction.SelectedIndex = 0 : pnlSearch.Controls.Add(cboAction)
        AddLabelTo(pnlSearch, "From:", 510, 15) : dtpDateFrom.Location = New Drawing.Point(550, 12) : dtpDateFrom.Size = New Drawing.Size(110, 20) : dtpDateFrom.Format = DateTimePickerFormat.Short : dtpDateFrom.Value = DateTime.Now.AddDays(-7) : pnlSearch.Controls.Add(dtpDateFrom)
        AddLabelTo(pnlSearch, "To:", 670, 15) : dtpDateTo.Location = New Drawing.Point(695, 12) : dtpDateTo.Size = New Drawing.Size(110, 20) : dtpDateTo.Format = DateTimePickerFormat.Short : pnlSearch.Controls.Add(dtpDateTo)
        btnSearch.Location = New Drawing.Point(830, 8) : btnSearch.Size = New Drawing.Size(80, 30) : btnSearch.Text = "&Search" : pnlSearch.Controls.Add(btnSearch)
        Me.Controls.Add(pnlSearch)

        dgvAudit.Dock = DockStyle.Fill : dgvAudit.ReadOnly = True : dgvAudit.AllowUserToAddRows = False
        dgvAudit.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvAudit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvAudit)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 30}
        lblRecordCount.Location = New Drawing.Point(10, 8) : lblRecordCount.AutoSize = True : pnlBottom.Controls.Add(lblRecordCount)
        Me.Controls.Add(pnlBottom)
        dgvAudit.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@TableName", If(String.IsNullOrWhiteSpace(txtTableName.Text), Nothing, txtTableName.Text.Trim())),
                DatabaseHelper.CreateParam("@Username", If(String.IsNullOrWhiteSpace(txtUsername.Text), Nothing, txtUsername.Text.Trim())),
                DatabaseHelper.CreateParam("@Action", If(cboAction.SelectedIndex > 0, cboAction.SelectedItem.ToString(), Nothing)),
                DatabaseHelper.CreateParam("@DateFrom", dtpDateFrom.Value.Date),
                DatabaseHelper.CreateParam("@DateTo", dtpDateTo.Value.Date.AddDays(1)),
                DatabaseHelper.CreateParam("@PageNumber", 1),
                DatabaseHelper.CreateParam("@PageSize", 500)
            }
            Dim totalParam As SqlParameter = DatabaseHelper.CreateOutputParam("@TotalRecords", SqlDbType.Int)
            params.Add(totalParam)

            Dim dt As DataTable = DatabaseHelper.ExecuteWithOutput("Admin.usp_Audit_Search", params.ToArray())
            dgvAudit.DataSource = dt
            lblRecordCount.Text = $"{CInt(If(totalParam.Value, 0))} record(s)"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmAuditViewer.btnSearch_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub
End Class
