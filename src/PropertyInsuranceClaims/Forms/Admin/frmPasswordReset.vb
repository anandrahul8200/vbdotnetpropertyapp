Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Admin password reset form.
''' </summary>
Public Class frmPasswordReset
    Inherits Form

    Private txtUsername As New TextBox()
    Private txtNewPassword As New TextBox()
    Private txtConfirmPassword As New TextBox()
    Private chkForceChange As New CheckBox()
    Private WithEvents btnReset As New Button()
    Private WithEvents btnCancel As New Button()

    Private Sub frmPasswordReset_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Reset User Password"
        Me.Size = New Drawing.Size(400, 250)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 20
        Me.Controls.Add(New Label() With {.Text = "Username:", .Location = New Drawing.Point(20, y), .AutoSize = True})
        txtUsername.Location = New Drawing.Point(140, y - 3) : txtUsername.Size = New Drawing.Size(200, 20) : Me.Controls.Add(txtUsername)
        y += 35
        Me.Controls.Add(New Label() With {.Text = "New Password:", .Location = New Drawing.Point(20, y), .AutoSize = True})
        txtNewPassword.Location = New Drawing.Point(140, y - 3) : txtNewPassword.Size = New Drawing.Size(200, 20) : txtNewPassword.PasswordChar = "*"c : Me.Controls.Add(txtNewPassword)
        y += 35
        Me.Controls.Add(New Label() With {.Text = "Confirm:", .Location = New Drawing.Point(20, y), .AutoSize = True})
        txtConfirmPassword.Location = New Drawing.Point(140, y - 3) : txtConfirmPassword.Size = New Drawing.Size(200, 20) : txtConfirmPassword.PasswordChar = "*"c : Me.Controls.Add(txtConfirmPassword)
        y += 35
        chkForceChange.Location = New Drawing.Point(140, y) : chkForceChange.Text = "Force password change on next login" : chkForceChange.Checked = True : chkForceChange.AutoSize = True : Me.Controls.Add(chkForceChange)
        y += 35
        btnReset.Location = New Drawing.Point(140, y) : btnReset.Size = New Drawing.Size(90, 30) : btnReset.Text = "&Reset" : Me.Controls.Add(btnReset)
        btnCancel.Location = New Drawing.Point(240, y) : btnCancel.Size = New Drawing.Size(80, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then MessageBox.Show("Username required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        If txtNewPassword.Text <> txtConfirmPassword.Text Then MessageBox.Show("Passwords don't match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        If txtNewPassword.Text.Length < 8 Then MessageBox.Show("Password must be at least 8 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        MessageBox.Show("Password reset for: " & txtUsername.Text, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Me.Close()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
