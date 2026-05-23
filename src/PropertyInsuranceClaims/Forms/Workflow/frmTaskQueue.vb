Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Task queue — shows user's pending work items across all modules.
''' </summary>
Public Class frmTaskQueue
    Inherits Form

    Private dgvTasks As New DataGridView()
    Private cboFilter As New ComboBox()
    Private cboPriority As New ComboBox()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnComplete As New Button()
    Private WithEvents btnReassign As New Button()
    Private WithEvents btnOpen As New Button()
    Private lblTaskCount As New Label()

    Private Sub frmTaskQueue_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "My Task Queue - " & GlobalState.CurrentUserFullName
        Me.Size = New Drawing.Size(900, 550)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadTasks()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Module:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        cboFilter.Location = New Drawing.Point(65, 11) : cboFilter.Size = New Drawing.Size(120, 20) : cboFilter.DropDownStyle = ComboBoxStyle.DropDownList
        cboFilter.Items.AddRange({"(All)", "POLICY", "CLAIMS", "UNDERWRITING", "BILLING"}) : cboFilter.SelectedIndex = 0 : pnlTop.Controls.Add(cboFilter)
        pnlTop.Controls.Add(New Label() With {.Text = "Priority:", .Location = New Drawing.Point(200, 14), .AutoSize = True})
        cboPriority.Location = New Drawing.Point(255, 11) : cboPriority.Size = New Drawing.Size(90, 20) : cboPriority.DropDownStyle = ComboBoxStyle.DropDownList
        cboPriority.Items.AddRange({"(All)", "HIGH", "NORMAL", "LOW"}) : cboPriority.SelectedIndex = 0 : pnlTop.Controls.Add(cboPriority)
        btnRefresh.Location = New Drawing.Point(370, 8) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        lblTaskCount.Location = New Drawing.Point(470, 14) : lblTaskCount.AutoSize = True : pnlTop.Controls.Add(lblTaskCount)
        Me.Controls.Add(pnlTop)

        dgvTasks.Dock = DockStyle.Fill : dgvTasks.ReadOnly = True : dgvTasks.AllowUserToAddRows = False
        dgvTasks.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvTasks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvTasks)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnOpen.Location = New Drawing.Point(10, 5) : btnOpen.Size = New Drawing.Size(80, 30) : btnOpen.Text = "&Open" : pnlBottom.Controls.Add(btnOpen)
        btnComplete.Location = New Drawing.Point(100, 5) : btnComplete.Size = New Drawing.Size(100, 30) : btnComplete.Text = "&Complete" : pnlBottom.Controls.Add(btnComplete)
        btnReassign.Location = New Drawing.Point(210, 5) : btnReassign.Size = New Drawing.Size(100, 30) : btnReassign.Text = "Re&assign" : pnlBottom.Controls.Add(btnReassign)
        Me.Controls.Add(pnlBottom)
        dgvTasks.BringToFront()
    End Sub

    Private Sub LoadTasks()
        ' Would query pending activities/tasks assigned to current user
        lblTaskCount.Text = "0 pending tasks"
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadTasks()
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        If dgvTasks.CurrentRow Is Nothing Then Return
        MessageBox.Show("Would open the related entity.", "Open", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnComplete_Click(sender As Object, e As EventArgs) Handles btnComplete.Click
        If dgvTasks.CurrentRow Is Nothing Then Return
        MessageBox.Show("Task marked complete.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
        LoadTasks()
    End Sub

    Private Sub btnReassign_Click(sender As Object, e As EventArgs) Handles btnReassign.Click
        If dgvTasks.CurrentRow Is Nothing Then Return
        MessageBox.Show("Reassignment dialog would open.", "Reassign", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class
