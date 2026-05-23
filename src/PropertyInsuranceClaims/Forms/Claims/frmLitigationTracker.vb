Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Litigation tracker — manage claims in litigation with attorney info and court dates.
''' </summary>
Public Class frmLitigationTracker
    Inherits Form

    Private dgvLitigation As New DataGridView()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnUpdateStatus As New Button()
    Private WithEvents btnClose As New Button()
    Private lblCount As New Label()

    Private Sub frmLitigationTracker_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Litigation Tracker"
        Me.Size = New Drawing.Size(900, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        btnRefresh.Location = New Drawing.Point(10, 6) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        btnUpdateStatus.Location = New Drawing.Point(100, 6) : btnUpdateStatus.Size = New Drawing.Size(110, 28) : btnUpdateStatus.Text = "&Update Status" : pnlTop.Controls.Add(btnUpdateStatus)
        lblCount.Location = New Drawing.Point(250, 12) : lblCount.AutoSize = True : lblCount.Text = "Claims in litigation: 0" : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        dgvLitigation.Dock = DockStyle.Fill : dgvLitigation.ReadOnly = True : dgvLitigation.AllowUserToAddRows = False
        dgvLitigation.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvLitigation.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvLitigation)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(780, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvLitigation.BringToFront()
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
    End Sub

    Private Sub btnUpdateStatus_Click(sender As Object, e As EventArgs) Handles btnUpdateStatus.Click
        If dgvLitigation.CurrentRow Is Nothing Then Return
        MessageBox.Show("Update litigation status dialog.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
