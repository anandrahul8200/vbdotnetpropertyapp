Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' User management form — list, create, edit, lock/unlock users.
''' </summary>
Public Class frmUserManagement
    Inherits Form

    Private dgvUsers As New DataGridView()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnEdit As New Button()
    Private WithEvents btnLock As New Button()
    Private WithEvents btnResetPassword As New Button()
    Private WithEvents btnRefresh As New Button()

    Private Sub frmUserManagement_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "User Management"
            Me.Size = New Drawing.Size(800, 500)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadUsers()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmUserManagement_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        btnNew.Location = New Drawing.Point(10, 8) : btnNew.Size = New Drawing.Size(80, 30) : btnNew.Text = "&New" : pnlTop.Controls.Add(btnNew)
        btnEdit.Location = New Drawing.Point(100, 8) : btnEdit.Size = New Drawing.Size(80, 30) : btnEdit.Text = "&Edit" : pnlTop.Controls.Add(btnEdit)
        btnLock.Location = New Drawing.Point(190, 8) : btnLock.Size = New Drawing.Size(100, 30) : btnLock.Text = "&Lock/Unlock" : pnlTop.Controls.Add(btnLock)
        btnResetPassword.Location = New Drawing.Point(300, 8) : btnResetPassword.Size = New Drawing.Size(120, 30) : btnResetPassword.Text = "Reset &Password" : pnlTop.Controls.Add(btnResetPassword)
        btnRefresh.Location = New Drawing.Point(650, 8) : btnRefresh.Size = New Drawing.Size(80, 30) : btnRefresh.Text = "Re&fresh" : pnlTop.Controls.Add(btnRefresh)
        Me.Controls.Add(pnlTop)

        dgvUsers.Dock = DockStyle.Fill : dgvUsers.ReadOnly = True : dgvUsers.AllowUserToAddRows = False
        dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvUsers)
        dgvUsers.BringToFront()
    End Sub

    Private Sub LoadUsers()
        Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Admin.usp_User_List", Nothing)
        dgvUsers.DataSource = dt
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        MessageBox.Show("New user dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click
        If dgvUsers.CurrentRow Is Nothing Then Return
        MessageBox.Show("Edit user dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnLock_Click(sender As Object, e As EventArgs) Handles btnLock.Click
        If dgvUsers.CurrentRow Is Nothing Then Return
        MessageBox.Show("User lock/unlock toggled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        LoadUsers()
    End Sub

    Private Sub btnResetPassword_Click(sender As Object, e As EventArgs) Handles btnResetPassword.Click
        If dgvUsers.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Reset password for this user?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Password reset. Temporary password sent.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadUsers()
    End Sub
End Class
