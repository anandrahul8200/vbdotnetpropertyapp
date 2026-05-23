Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Role-permission management — assign permissions to roles.
''' </summary>
Public Class frmRolePermissions
    Inherits Form

    Private cboRole As New ComboBox()
    Private dgvPermissions As New DataGridView()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnClose As New Button()

    Private Sub frmRolePermissions_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Role Permissions"
        Me.Size = New Drawing.Size(600, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Role:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        cboRole.Location = New Drawing.Point(50, 11) : cboRole.Size = New Drawing.Size(200, 20) : cboRole.DropDownStyle = ComboBoxStyle.DropDownList
        cboRole.Items.AddRange({"Administrator", "Underwriter", "Claims Adjuster", "Billing Clerk", "Agent", "Read Only"})
        cboRole.SelectedIndex = 0 : pnlTop.Controls.Add(cboRole)
        Me.Controls.Add(pnlTop)

        dgvPermissions.Dock = DockStyle.Fill : dgvPermissions.AllowUserToAddRows = False
        dgvPermissions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvPermissions.Columns.Add(New DataGridViewCheckBoxColumn() With {.Name = "Granted", .HeaderText = "Grant", .Width = 50})
        dgvPermissions.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Module", .HeaderText = "Module", .ReadOnly = True})
        dgvPermissions.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Permission", .HeaderText = "Permission", .ReadOnly = True})
        dgvPermissions.Rows.Add(True, "POLICY", "Create Policy") : dgvPermissions.Rows.Add(True, "POLICY", "Edit Policy")
        dgvPermissions.Rows.Add(True, "POLICY", "Cancel Policy") : dgvPermissions.Rows.Add(True, "CLAIMS", "Create Claim")
        dgvPermissions.Rows.Add(True, "CLAIMS", "Approve Payment") : dgvPermissions.Rows.Add(True, "CLAIMS", "Close Claim")
        dgvPermissions.Rows.Add(True, "UNDERWRITING", "Approve Referral") : dgvPermissions.Rows.Add(True, "BILLING", "Record Payment")
        dgvPermissions.Rows.Add(True, "ADMIN", "Manage Users") : dgvPermissions.Rows.Add(True, "ADMIN", "View Audit")
        Me.Controls.Add(dgvPermissions)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnSave.Location = New Drawing.Point(10, 5) : btnSave.Size = New Drawing.Size(80, 30) : btnSave.Text = "&Save" : pnlBottom.Controls.Add(btnSave)
        btnClose.Location = New Drawing.Point(480, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvPermissions.BringToFront()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        MessageBox.Show("Permissions saved for role: " & cboRole.SelectedItem.ToString(), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
