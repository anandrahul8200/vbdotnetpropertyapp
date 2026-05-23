Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Error log viewer — view application and database errors.
''' </summary>
Public Class frmErrorLogViewer
    Inherits Form

    Private dgvErrors As New DataGridView()
    Private txtProcedure As New TextBox()
    Private dtpDateFrom As New DateTimePicker()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClear As New Button()
    Private WithEvents btnClose As New Button()
    Private lblCount As New Label()

    Private Sub frmErrorLogViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Error Log Viewer"
        Me.Size = New Drawing.Size(900, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Procedure:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        txtProcedure.Location = New Drawing.Point(80, 11) : txtProcedure.Size = New Drawing.Size(150, 20) : pnlTop.Controls.Add(txtProcedure)
        pnlTop.Controls.Add(New Label() With {.Text = "Since:", .Location = New Drawing.Point(250, 14), .AutoSize = True})
        dtpDateFrom.Location = New Drawing.Point(290, 11) : dtpDateFrom.Size = New Drawing.Size(110, 20) : dtpDateFrom.Format = DateTimePickerFormat.Short : dtpDateFrom.Value = DateTime.Today.AddDays(-7) : pnlTop.Controls.Add(dtpDateFrom)
        btnSearch.Location = New Drawing.Point(420, 8) : btnSearch.Size = New Drawing.Size(80, 28) : btnSearch.Text = "&Search" : pnlTop.Controls.Add(btnSearch)
        btnClear.Location = New Drawing.Point(510, 8) : btnClear.Size = New Drawing.Size(80, 28) : btnClear.Text = "&Clear" : pnlTop.Controls.Add(btnClear)
        lblCount.Location = New Drawing.Point(610, 14) : lblCount.AutoSize = True : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        dgvErrors.Dock = DockStyle.Fill : dgvErrors.ReadOnly = True : dgvErrors.AllowUserToAddRows = False
        dgvErrors.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvErrors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvErrors)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(780, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvErrors.BringToFront()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim dt As DataTable = AdminDataAccess.SearchErrorLog(
                If(String.IsNullOrWhiteSpace(txtProcedure.Text), Nothing, txtProcedure.Text.Trim()),
                dtpDateFrom.Value.Date)
            dgvErrors.DataSource = dt
            lblCount.Text = dt.Rows.Count.ToString() & " error(s)"
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        txtProcedure.Clear() : dgvErrors.DataSource = Nothing : lblCount.Text = ""
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
