Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Notification center — system alerts and notifications for the current user.
''' </summary>
Public Class frmNotificationCenter
    Inherits Form

    Private dgvNotifications As New DataGridView()
    Private WithEvents btnMarkRead As New Button()
    Private WithEvents btnMarkAllRead As New Button()
    Private WithEvents btnDelete As New Button()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnClose As New Button()
    Private lblUnreadCount As New Label()

    Private Sub frmNotificationCenter_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Notification Center"
        Me.Size = New Drawing.Size(700, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        lblUnreadCount.Location = New Drawing.Point(10, 12) : lblUnreadCount.AutoSize = True : lblUnreadCount.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblUnreadCount.Text = "0 unread notifications" : pnlTop.Controls.Add(lblUnreadCount)
        btnRefresh.Location = New Drawing.Point(500, 5) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        btnMarkAllRead.Location = New Drawing.Point(590, 5) : btnMarkAllRead.Size = New Drawing.Size(90, 28) : btnMarkAllRead.Text = "Mark All Read" : pnlTop.Controls.Add(btnMarkAllRead)
        Me.Controls.Add(pnlTop)

        dgvNotifications.Dock = DockStyle.Fill : dgvNotifications.ReadOnly = True : dgvNotifications.AllowUserToAddRows = False
        dgvNotifications.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvNotifications.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvNotifications)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnMarkRead.Location = New Drawing.Point(10, 5) : btnMarkRead.Size = New Drawing.Size(100, 30) : btnMarkRead.Text = "Mark &Read" : pnlBottom.Controls.Add(btnMarkRead)
        btnDelete.Location = New Drawing.Point(120, 5) : btnDelete.Size = New Drawing.Size(80, 30) : btnDelete.Text = "&Delete" : pnlBottom.Controls.Add(btnDelete)
        btnClose.Location = New Drawing.Point(580, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvNotifications.BringToFront()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        lblUnreadCount.Text = "0 unread notifications"
    End Sub

    Private Sub btnMarkRead_Click(sender As Object, e As EventArgs) Handles btnMarkRead.Click
        If dgvNotifications.CurrentRow IsNot Nothing Then MessageBox.Show("Marked as read.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnMarkAllRead_Click(sender As Object, e As EventArgs) Handles btnMarkAllRead.Click
        MessageBox.Show("All notifications marked as read.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If dgvNotifications.CurrentRow IsNot Nothing Then MessageBox.Show("Notification deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
