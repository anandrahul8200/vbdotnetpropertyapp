Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' System configuration viewer/editor.
''' </summary>
Public Class frmSystemConfig
    Inherits Form

    Private cboCategory As New ComboBox()
    Private dgvConfig As New DataGridView()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnRefresh As New Button()
    Private lblStatus As New Label()

    Private Sub frmSystemConfig_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "System Configuration"
        Me.Size = New Drawing.Size(800, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadConfig()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Category:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        cboCategory.Location = New Drawing.Point(80, 11) : cboCategory.Size = New Drawing.Size(180, 20) : cboCategory.DropDownStyle = ComboBoxStyle.DropDownList
        cboCategory.Items.AddRange({"(All)", "GENERAL", "SECURITY", "BILLING", "CLAIMS", "UNDERWRITING", "BATCH"})
        cboCategory.SelectedIndex = 0
        AddHandler cboCategory.SelectedIndexChanged, Sub() LoadConfig()
        pnlTop.Controls.Add(cboCategory)
        btnRefresh.Location = New Drawing.Point(280, 8) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        btnSave.Location = New Drawing.Point(370, 8) : btnSave.Size = New Drawing.Size(80, 28) : btnSave.Text = "&Save" : pnlTop.Controls.Add(btnSave)
        lblStatus.Location = New Drawing.Point(470, 14) : lblStatus.AutoSize = True : lblStatus.ForeColor = Drawing.Color.Green : pnlTop.Controls.Add(lblStatus)
        Me.Controls.Add(pnlTop)

        dgvConfig.Dock = DockStyle.Fill : dgvConfig.AllowUserToAddRows = False : dgvConfig.AllowUserToDeleteRows = False
        dgvConfig.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvConfig.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvConfig)
        dgvConfig.BringToFront()
    End Sub

    Private Sub LoadConfig()
        Try
            Dim category As String = If(cboCategory.SelectedIndex > 0, cboCategory.SelectedItem.ToString(), Nothing)
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@Category", category)
            }
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Admin.usp_Config_Get", params)
            dgvConfig.DataSource = dt

            ' Make only ConfigValue editable
            For Each col As DataGridViewColumn In dgvConfig.Columns
                col.ReadOnly = (col.Name <> "ConfigValue")
            Next
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim savedCount As Integer = 0

            For Each row As DataGridViewRow In dgvConfig.Rows
                If row.Cells("ConfigKey").Value Is Nothing Then Continue For
                Dim key As String = row.Cells("ConfigKey").Value.ToString()
                Dim value As String = If(row.Cells("ConfigValue").Value IsNot Nothing, row.Cells("ConfigValue").Value.ToString(), "")

                Dim params() As SqlParameter = {
                    DatabaseHelper.CreateParam("@ConfigKey", key),
                    DatabaseHelper.CreateParam("@ConfigValue", value),
                    DatabaseHelper.CreateParam("@ModifiedBy", GlobalState.CurrentUser)
                }
                DatabaseHelper.ExecuteNonQuery("Admin.usp_Config_Set", params)
                savedCount += 1
            Next

            lblStatus.Text = $"Saved {savedCount} settings at {DateTime.Now:HH:mm:ss}"
        Catch ex As Exception
            MessageBox.Show("Error saving: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmSystemConfig.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadConfig()
    End Sub
End Class
