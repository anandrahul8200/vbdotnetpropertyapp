Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Catastrophe event manager — declare, manage, and view CAT events.
''' </summary>
Public Class frmCatastropheManager
    Inherits Form

    Private dgvCatastrophes As New DataGridView()
    Private WithEvents btnDeclare As New Button()
    Private WithEvents btnViewClaims As New Button()
    Private WithEvents btnClose As New Button()
    Private WithEvents btnRefresh As New Button()

    Private Sub frmCatastropheManager_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Catastrophe Manager"
        Me.Size = New Drawing.Size(850, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        btnDeclare.Location = New Drawing.Point(10, 6) : btnDeclare.Size = New Drawing.Size(120, 28) : btnDeclare.Text = "&Declare New CAT" : pnlTop.Controls.Add(btnDeclare)
        btnViewClaims.Location = New Drawing.Point(140, 6) : btnViewClaims.Size = New Drawing.Size(100, 28) : btnViewClaims.Text = "&View Claims" : pnlTop.Controls.Add(btnViewClaims)
        btnRefresh.Location = New Drawing.Point(720, 6) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "Refresh" : pnlTop.Controls.Add(btnRefresh)
        Me.Controls.Add(pnlTop)

        dgvCatastrophes.Dock = DockStyle.Fill : dgvCatastrophes.ReadOnly = True : dgvCatastrophes.AllowUserToAddRows = False
        dgvCatastrophes.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvCatastrophes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvCatastrophes)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(730, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvCatastrophes.BringToFront()
    End Sub

    Private Sub btnDeclare_Click(sender As Object, e As EventArgs) Handles btnDeclare.Click
        MessageBox.Show("Declare catastrophe dialog would open.", "New CAT", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnViewClaims_Click(sender As Object, e As EventArgs) Handles btnViewClaims.Click
        If dgvCatastrophes.CurrentRow Is Nothing Then Return
        MessageBox.Show("CAT claims view would open.", "View", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
