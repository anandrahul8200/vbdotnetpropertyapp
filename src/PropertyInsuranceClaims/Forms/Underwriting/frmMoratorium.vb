Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Moratorium management — view active moratoriums, create new, lift existing.
''' </summary>
Public Class frmMoratorium
    Inherits Form

    Private dgvMoratoriums As New DataGridView()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnLift As New Button()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnCheck As New Button()

    Private Sub frmMoratorium_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Moratorium Management"
        Me.Size = New Drawing.Size(900, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadMoratoriums()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        btnNew.Location = New Drawing.Point(10, 8) : btnNew.Size = New Drawing.Size(120, 30) : btnNew.Text = "&New Moratorium" : pnlTop.Controls.Add(btnNew)
        btnLift.Location = New Drawing.Point(140, 8) : btnLift.Size = New Drawing.Size(100, 30) : btnLift.Text = "&Lift Selected" : pnlTop.Controls.Add(btnLift)
        btnCheck.Location = New Drawing.Point(250, 8) : btnCheck.Size = New Drawing.Size(120, 30) : btnCheck.Text = "&Check Location" : pnlTop.Controls.Add(btnCheck)
        btnRefresh.Location = New Drawing.Point(750, 8) : btnRefresh.Size = New Drawing.Size(80, 30) : btnRefresh.Text = "Re&fresh" : pnlTop.Controls.Add(btnRefresh)
        Me.Controls.Add(pnlTop)

        dgvMoratoriums.Dock = DockStyle.Fill : dgvMoratoriums.ReadOnly = True : dgvMoratoriums.AllowUserToAddRows = False
        dgvMoratoriums.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvMoratoriums.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvMoratoriums)
        dgvMoratoriums.BringToFront()
    End Sub

    Private Sub LoadMoratoriums()
        Try
            Dim dt As DataTable = UnderwritingDataAccess.GetActiveMoratoriums()
            dgvMoratoriums.DataSource = dt
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        Dim frm As New frmMoratoriumEntry()
        If frm.ShowDialog(Me) = DialogResult.OK Then LoadMoratoriums()
    End Sub

    Private Sub btnLift_Click(sender As Object, e As EventArgs) Handles btnLift.Click
        If dgvMoratoriums.CurrentRow Is Nothing Then Return
        Dim moratoriumID As Integer = CInt(dgvMoratoriums.CurrentRow.Cells("MoratoriumID").Value)
        Dim name As String = dgvMoratoriums.CurrentRow.Cells("MoratoriumName").Value.ToString()

        If MessageBox.Show($"Lift moratorium: {name}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Dim params() As SqlParameter = {
                    DatabaseHelper.CreateParam("@MoratoriumID", moratoriumID),
                    DatabaseHelper.CreateParam("@LiftedBy", GlobalState.CurrentUser)
                }
                DatabaseHelper.ExecuteNonQuery("Underwriting.usp_Moratorium_Lift", params)
                MessageBox.Show("Moratorium lifted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                LoadMoratoriums()
            Catch ex As Exception
                MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Sub btnCheck_Click(sender As Object, e As EventArgs) Handles btnCheck.Click
        Dim state As String = InputBox("Enter state code (e.g., FL):", "Check Moratorium")
        If String.IsNullOrWhiteSpace(state) Then Return
        Dim zip As String = InputBox("Enter zip code:", "Check Moratorium")
        If String.IsNullOrWhiteSpace(zip) Then Return

        Dim isMoratorium As Boolean = UnderwritingDataAccess.CheckMoratorium("HO3", state.Trim().ToUpper(), zip.Trim())
        If isMoratorium Then
            MessageBox.Show($"MORATORIUM IN EFFECT for {state} {zip}. New business is restricted.", "Moratorium Active", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Else
            MessageBox.Show($"No moratorium for {state} {zip}. Writing is allowed.", "Clear", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadMoratoriums()
    End Sub
End Class

''' <summary>
''' Create new moratorium dialog.
''' </summary>
Public Class frmMoratoriumEntry
    Inherits Form

    Private txtName As New TextBox()
    Private cboType As New ComboBox()
    Private txtReason As New TextBox()
    Private txtAffectedStates As New TextBox()
    Private txtAffectedPolicyTypes As New TextBox()
    Private dtpStartDate As New DateTimePicker()
    Private dtpEndDate As New DateTimePicker()
    Private chkHasEndDate As New CheckBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Private Sub frmMoratoriumEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "New Moratorium"
        Me.Size = New Drawing.Size(500, 380)
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False

        Dim y As Integer = 15
        AddLabel("Name:", 15, y)
        txtName.Location = New Drawing.Point(130, y - 3) : txtName.Size = New Drawing.Size(320, 20) : Me.Controls.Add(txtName)
        y += 32

        AddLabel("Type:", 15, y)
        cboType.Location = New Drawing.Point(130, y - 3) : cboType.Size = New Drawing.Size(150, 20) : cboType.DropDownStyle = ComboBoxStyle.DropDownList
        cboType.Items.AddRange({"NEW_BUSINESS", "RENEWAL", "ALL"}) : cboType.SelectedIndex = 0 : Me.Controls.Add(cboType)
        y += 32

        AddLabel("Reason:", 15, y)
        txtReason.Location = New Drawing.Point(130, y - 3) : txtReason.Size = New Drawing.Size(320, 40) : txtReason.Multiline = True : Me.Controls.Add(txtReason)
        y += 50

        AddLabel("Affected States:", 15, y)
        txtAffectedStates.Location = New Drawing.Point(130, y - 3) : txtAffectedStates.Size = New Drawing.Size(200, 20) : Me.Controls.Add(txtAffectedStates)
        AddLabel("(comma-separated)", 340, y) : y += 32

        AddLabel("Policy Types:", 15, y)
        txtAffectedPolicyTypes.Location = New Drawing.Point(130, y - 3) : txtAffectedPolicyTypes.Size = New Drawing.Size(200, 20) : Me.Controls.Add(txtAffectedPolicyTypes)
        y += 32

        AddLabel("Start Date:", 15, y)
        dtpStartDate.Location = New Drawing.Point(130, y - 3) : dtpStartDate.Size = New Drawing.Size(130, 20) : dtpStartDate.Format = DateTimePickerFormat.Short : Me.Controls.Add(dtpStartDate)
        y += 32

        chkHasEndDate.Location = New Drawing.Point(15, y) : chkHasEndDate.Text = "End Date:" : chkHasEndDate.AutoSize = True : Me.Controls.Add(chkHasEndDate)
        dtpEndDate.Location = New Drawing.Point(130, y - 2) : dtpEndDate.Size = New Drawing.Size(130, 20) : dtpEndDate.Format = DateTimePickerFormat.Short : dtpEndDate.Enabled = False : Me.Controls.Add(dtpEndDate)
        AddHandler chkHasEndDate.CheckedChanged, Sub() dtpEndDate.Enabled = chkHasEndDate.Checked
        y += 40

        btnSave.Location = New Drawing.Point(130, y) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Create" : Me.Controls.Add(btnSave)
        btnCancel.Location = New Drawing.Point(240, y) : btnCancel.Size = New Drawing.Size(100, 30) : btnCancel.Text = "&Cancel" : Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Me.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If String.IsNullOrWhiteSpace(txtName.Text) Then
                MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@MoratoriumName", txtName.Text.Trim()),
                DatabaseHelper.CreateParam("@MoratoriumType", cboType.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@Reason", txtReason.Text.Trim()),
                DatabaseHelper.CreateParam("@AffectedStates", If(String.IsNullOrWhiteSpace(txtAffectedStates.Text), Nothing, txtAffectedStates.Text.Trim())),
                DatabaseHelper.CreateParam("@AffectedPolicyTypes", If(String.IsNullOrWhiteSpace(txtAffectedPolicyTypes.Text), Nothing, txtAffectedPolicyTypes.Text.Trim())),
                DatabaseHelper.CreateParam("@StartDate", dtpStartDate.Value.Date),
                DatabaseHelper.CreateParam("@EndDate", If(chkHasEndDate.Checked, dtpEndDate.Value.Date, CType(Nothing, Object))),
                DatabaseHelper.CreateParam("@DeclaredBy", GlobalState.CurrentUser)
            }
            Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@MoratoriumID", SqlDbType.Int)
            params.Add(idParam)

            DatabaseHelper.ExecuteNonQuery("Underwriting.usp_Moratorium_Create", params.ToArray())
            MessageBox.Show("Moratorium created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK : Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel : Me.Close()
    End Sub
End Class
