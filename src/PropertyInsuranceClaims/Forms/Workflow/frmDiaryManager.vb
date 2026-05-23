Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Diary/follow-up manager — view and manage diary entries across claims.
''' </summary>
Public Class frmDiaryManager
    Inherits Form

    Private dgvDiary As New DataGridView()
    Private dtpDateFrom As New DateTimePicker()
    Private dtpDateTo As New DateTimePicker()
    Private cboStatus As New ComboBox()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnComplete As New Button()
    Private WithEvents btnReschedule As New Button()
    Private WithEvents btnClose As New Button()
    Private lblCount As New Label()

    Private Sub frmDiaryManager_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Diary Manager"
        Me.Size = New Drawing.Size(850, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "From:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        dtpDateFrom.Location = New Drawing.Point(50, 11) : dtpDateFrom.Size = New Drawing.Size(110, 20) : dtpDateFrom.Format = DateTimePickerFormat.Short : dtpDateFrom.Value = DateTime.Today.AddDays(-7) : pnlTop.Controls.Add(dtpDateFrom)
        pnlTop.Controls.Add(New Label() With {.Text = "To:", .Location = New Drawing.Point(170, 14), .AutoSize = True})
        dtpDateTo.Location = New Drawing.Point(195, 11) : dtpDateTo.Size = New Drawing.Size(110, 20) : dtpDateTo.Format = DateTimePickerFormat.Short : pnlTop.Controls.Add(dtpDateTo)
        pnlTop.Controls.Add(New Label() With {.Text = "Status:", .Location = New Drawing.Point(320, 14), .AutoSize = True})
        cboStatus.Location = New Drawing.Point(370, 11) : cboStatus.Size = New Drawing.Size(100, 20) : cboStatus.DropDownStyle = ComboBoxStyle.DropDownList
        cboStatus.Items.AddRange({"(All)", "PENDING", "OVERDUE", "COMPLETED"}) : cboStatus.SelectedIndex = 0 : pnlTop.Controls.Add(cboStatus)
        btnSearch.Location = New Drawing.Point(490, 8) : btnSearch.Size = New Drawing.Size(80, 28) : btnSearch.Text = "&Search" : pnlTop.Controls.Add(btnSearch)
        lblCount.Location = New Drawing.Point(590, 14) : lblCount.AutoSize = True : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        dgvDiary.Dock = DockStyle.Fill : dgvDiary.ReadOnly = True : dgvDiary.AllowUserToAddRows = False
        dgvDiary.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvDiary.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvDiary)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnComplete.Location = New Drawing.Point(10, 5) : btnComplete.Size = New Drawing.Size(100, 30) : btnComplete.Text = "&Complete" : pnlBottom.Controls.Add(btnComplete)
        btnReschedule.Location = New Drawing.Point(120, 5) : btnReschedule.Size = New Drawing.Size(100, 30) : btnReschedule.Text = "&Reschedule" : pnlBottom.Controls.Add(btnReschedule)
        btnClose.Location = New Drawing.Point(730, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "C&lose" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvDiary.BringToFront()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        lblCount.Text = "0 diary entries"
    End Sub

    Private Sub btnComplete_Click(sender As Object, e As EventArgs) Handles btnComplete.Click
        If dgvDiary.CurrentRow Is Nothing Then Return
        MessageBox.Show("Diary entry completed.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnReschedule_Click(sender As Object, e As EventArgs) Handles btnReschedule.Click
        If dgvDiary.CurrentRow Is Nothing Then Return
        MessageBox.Show("Reschedule dialog would open.", "Reschedule", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
