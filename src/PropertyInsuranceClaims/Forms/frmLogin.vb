Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Login form — authenticates user and sets global session.
''' </summary>
Public Class frmLogin
    Inherits Form

    Private txtUsername As New TextBox()
    Private txtPassword As New TextBox()
    Private WithEvents btnLogin As New Button()
    Private WithEvents btnCancel As New Button()
    Private lblError As New Label()
    Private lblVersion As New Label()

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = AppSettings.ApplicationName & " - Login"
        Me.Size = New Drawing.Size(400, 250)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

        InitializeControls()
        txtUsername.Focus()
    End Sub

    Private Sub InitializeControls()
        Dim lblTitle As New Label() With {
            .Text = AppSettings.ApplicationName,
            .Font = New Drawing.Font("Segoe UI", 12, Drawing.FontStyle.Bold),
            .Location = New Drawing.Point(20, 15),
            .AutoSize = True
        }
        Me.Controls.Add(lblTitle)

        Dim lblUser As New Label() With {.Text = "Username:", .Location = New Drawing.Point(30, 60), .AutoSize = True}
        Me.Controls.Add(lblUser)
        txtUsername.Location = New Drawing.Point(120, 57)
        txtUsername.Size = New Drawing.Size(220, 20)
        Me.Controls.Add(txtUsername)

        Dim lblPass As New Label() With {.Text = "Password:", .Location = New Drawing.Point(30, 95), .AutoSize = True}
        Me.Controls.Add(lblPass)
        txtPassword.Location = New Drawing.Point(120, 92)
        txtPassword.Size = New Drawing.Size(220, 20)
        txtPassword.PasswordChar = "●"c
        Me.Controls.Add(txtPassword)

        lblError.Location = New Drawing.Point(120, 120)
        lblError.AutoSize = True
        lblError.ForeColor = Drawing.Color.Red
        lblError.Visible = False
        Me.Controls.Add(lblError)

        btnLogin.Location = New Drawing.Point(120, 150)
        btnLogin.Size = New Drawing.Size(100, 35)
        btnLogin.Text = "&Login"
        Me.Controls.Add(btnLogin)
        Me.AcceptButton = btnLogin

        btnCancel.Location = New Drawing.Point(230, 150)
        btnCancel.Size = New Drawing.Size(100, 35)
        btnCancel.Text = "&Exit"
        Me.Controls.Add(btnCancel)
        Me.CancelButton = btnCancel

        lblVersion.Location = New Drawing.Point(30, 195)
        lblVersion.AutoSize = True
        lblVersion.ForeColor = Drawing.Color.Gray
        lblVersion.Text = $"Version {AppSettings.ApplicationVersion}"
        Me.Controls.Add(lblVersion)
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Try
            lblError.Visible = False

            If String.IsNullOrWhiteSpace(txtUsername.Text) OrElse String.IsNullOrWhiteSpace(txtPassword.Text) Then
                ShowError("Please enter username and password.")
                Return
            End If

            Me.Cursor = Cursors.WaitCursor

            Dim passwordHash As String = SecurityHelper.HashPassword(txtPassword.Text)

            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@Username", txtUsername.Text.Trim()),
                DatabaseHelper.CreateParam("@PasswordHash", passwordHash),
                DatabaseHelper.CreateParam("@IPAddress", Environment.MachineName)
            }

            Dim isAuthParam As SqlParameter = DatabaseHelper.CreateOutputParam("@IsAuthenticated", SqlDbType.Bit)
            Dim userIDParam As SqlParameter = DatabaseHelper.CreateOutputParam("@UserID", SqlDbType.Int)
            Dim fullNameParam As SqlParameter = DatabaseHelper.CreateOutputParam("@FullName", SqlDbType.VarChar)
            fullNameParam.Size = 200
            Dim roleParam As SqlParameter = DatabaseHelper.CreateOutputParam("@RoleName", SqlDbType.VarChar)
            roleParam.Size = 50

            params.AddRange({isAuthParam, userIDParam, fullNameParam, roleParam})

            DatabaseHelper.ExecuteNonQuery("Admin.usp_User_Authenticate", params.ToArray())

            If CBool(isAuthParam.Value) Then
                ' Load permissions
                Dim userID As Integer = CInt(userIDParam.Value)
                Dim permParams() As SqlParameter = {DatabaseHelper.CreateParam("@UserID", userID)}
                Dim dtPerms As DataTable = DatabaseHelper.ExecuteStoredProcedure("Admin.usp_User_GetPermissions", permParams)

                Dim permissions As New List(Of String)
                For Each row As DataRow In dtPerms.Rows
                    permissions.Add(row("PermissionCode").ToString().ToUpper())
                Next

                ' Set global session
                GlobalState.SetSession(userID, txtUsername.Text.Trim(), CStr(fullNameParam.Value), CStr(roleParam.Value), permissions)

                Me.DialogResult = DialogResult.OK
                Me.Close()
            Else
                ShowError("Invalid username or password.")
                txtPassword.Clear()
                txtPassword.Focus()
            End If
        Catch ex As Exception
            ShowError("Login failed. Please try again.")
            ErrorLogger.LogError(ex, "btnLogin_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub ShowError(message As String)
        lblError.Text = message
        lblError.Visible = True
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

End Class
