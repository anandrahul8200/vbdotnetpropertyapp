Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Application entry point.
''' Shows login form, then opens main MDI form on success.
''' </summary>
Module Program

    <STAThread()>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' Show login
        Dim loginForm As New frmLogin()
        If loginForm.ShowDialog() = DialogResult.OK Then
            ' Login successful — open main form
            Application.Run(New frmMain())
        End If

        ' Cleanup
        GlobalState.ClearSession()
    End Sub

End Module
