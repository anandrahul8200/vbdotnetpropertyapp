Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Log claim activity/diary entry dialog.
''' </summary>
Public Class frmClaimActivity
    Inherits Form

    Private _claimID As Integer

    Private cboActivityType As New ComboBox()
    Private txtSubject As New TextBox()
    Private txtDescription As New TextBox()
    Private dtpDueDate As New DateTimePicker()
    Private chkHasDueDate As New CheckBox()
    Private txtContactName As New TextBox()
    Private txtContactPhone As New TextBox()
    Private txtDuration As New TextBox()
    Private cboAssignedTo As New ComboBox()
    Private cboPriority As New ComboBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmClaimActivity_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Log Activity"
        Me.Size = New Drawing.Size(500, 430)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 15
        AddLabel("Type:", 15, y)
        cboActivityType.Location = New Drawing.Point(120, y - 3) : cboActivityType.Size = New Drawing.Size(150, 20) : cboActivityType.DropDownStyle = ComboBoxStyle.DropDownList
        cboActivityType.Items.AddRange({"NOTE", "PHONE_CALL", "EMAIL", "INSPECTION", "DOCUMENT"}) : cboActivityType.SelectedIndex = 0
        Me.Controls.Add(cboActivityType)
        y += 32

        AddLabel("Subject:", 15, y)
        txtSubject.Location = New Drawing.Point(120, y - 3) : txtSubject.Size = New Drawing.Size(330, 20)
        Me.Controls.Add(txtSubject)
        y += 32

        AddLabel("Description:", 15, y)
        txtDescription.Location = New Drawing.Point(120, y - 3) : txtDescription.Size = New Drawing.Size(330, 80) : txtDescription.Multiline = True : txtDescription.ScrollBars = ScrollBars.Vertical
        Me.Controls.Add(txtDescription)
        y += 90

        chkHasDueDate.Location = New Drawing.Point(15, y) : chkHasDueDate.Text = "Due Date:" : chkHasDueDate.AutoSize = True : Me.Controls.Add(chkHasDueDate)
        dtpDueDate.Location = New Drawing.Point(120, y - 2) : dtpDueDate.Size = New Drawing.Size(130, 20) : dtpDueDate.Format = DateTimePickerFormat.Short : dtpDueDate.Enabled = False
        Me.Controls.Add(dtpDueDate)
        AddHandler chkHasDueDate.CheckedChanged, Sub() dtpDueDate.Enabled = chkHasDueDate.Checked
        y += 32

        AddLabel("Contact:", 15, y)
        txtContactName.Location = New Drawing.Point(120, y - 3) : txtContactName.Size = New Drawing.Size(150, 20) : Me.Controls.Add(txtContactName)
        AddLabel("Phone:", 285, y)
        txtContactPhone.Location = New Drawing.Point(330, y - 3) : txtContactPhone.Size = New Drawing.Size(120, 20) : Me.Controls.Add(txtContactPhone)
        y += 32

        AddLabel("Duration (min):", 15, y)
        txtDuration.Location = New Drawing.Point(120, y - 3) : txtDuration.Size = New Drawing.Size(50, 20) : Me.Controls.Add(txtDuration)
        AddLabel("Priority:", 200, y)
        cboPriority.Location = New Drawing.Point(260, y - 3) : cboPriority.Size = New Drawing.Size(90, 20) : cboPriority.DropDownStyle = ComboBoxStyle.DropDownList
        cboPriority.Items.AddRange({"LOW", "NORMAL", "HIGH"}) : cboPriority.SelectedIndex = 1 : Me.Controls.Add(cboPriority)
        y += 40

        btnSave.Location = New Drawing.Point(120, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Save" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(230, y) : btnCancel.Size = New Drawing.Size(100, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If String.IsNullOrWhiteSpace(txtSubject.Text) Then
                MessageBox.Show("Subject is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            Dim dueDate As Date? = If(chkHasDueDate.Checked, dtpDueDate.Value.Date, CType(Nothing, Date?))
            ClaimDataAccess.CreateActivity(_claimID, cboActivityType.SelectedItem.ToString(), txtSubject.Text.Trim(), txtDescription.Text.Trim(), dueDate)
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimActivity.btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
