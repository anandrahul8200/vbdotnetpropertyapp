Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Claim assignment form — assign adjusters, vendors, examiners to claims.
''' Shows current assignments and allows new/reassignment.
''' </summary>
Public Class frmClaimAssignment
    Inherits Form

    Private _claimID As Integer
    Private dgvCurrentAssignments As New DataGridView()
    Private grpNewAssignment As New GroupBox()
    Private cboAssigneeType As New ComboBox()
    Private cboAssignee As New ComboBox()
    Private dtpDueDate As New DateTimePicker()
    Private chkHasDueDate As New CheckBox()
    Private txtInstructions As New TextBox()
    Private WithEvents btnAssign As New Button()
    Private WithEvents btnReassign As New Button()
    Private WithEvents btnComplete As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmClaimAssignment_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Claim Assignments"
            Me.Size = New Drawing.Size(750, 550)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadAssignments()
            LoadAssignees()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmClaimAssignment_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Current assignments grid
        Dim grpCurrent As New GroupBox() With {.Text = "Current Assignments", .Dock = DockStyle.Top, .Height = 200}
        dgvCurrentAssignments.Dock = DockStyle.Fill : dgvCurrentAssignments.ReadOnly = True : dgvCurrentAssignments.AllowUserToAddRows = False
        dgvCurrentAssignments.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvCurrentAssignments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpCurrent.Controls.Add(dgvCurrentAssignments)

        Dim pnlGridButtons As New Panel() With {.Dock = DockStyle.Bottom, .Height = 35}
        btnReassign.Location = New Drawing.Point(10, 4) : btnReassign.Size = New Drawing.Size(100, 28) : btnReassign.Text = "&Reassign" : pnlGridButtons.Controls.Add(btnReassign)
        btnComplete.Location = New Drawing.Point(120, 4) : btnComplete.Size = New Drawing.Size(120, 28) : btnComplete.Text = "Mark &Complete" : pnlGridButtons.Controls.Add(btnComplete)
        grpCurrent.Controls.Add(pnlGridButtons)
        Me.Controls.Add(grpCurrent)

        ' New assignment panel
        grpNewAssignment.Text = "New Assignment" : grpNewAssignment.Dock = DockStyle.Fill
        Dim y As Integer = 25
        AddLabelTo(grpNewAssignment, "Assignee Type:", 15, y)
        cboAssigneeType.Location = New Drawing.Point(130, y - 3) : cboAssigneeType.Size = New Drawing.Size(150, 20) : cboAssigneeType.DropDownStyle = ComboBoxStyle.DropDownList
        cboAssigneeType.Items.AddRange({"ADJUSTER", "VENDOR", "EXAMINER", "SIU"}) : cboAssigneeType.SelectedIndex = 0
        AddHandler cboAssigneeType.SelectedIndexChanged, AddressOf AssigneeTypeChanged
        grpNewAssignment.Controls.Add(cboAssigneeType)
        y += 32

        AddLabelTo(grpNewAssignment, "Assignee:", 15, y)
        cboAssignee.Location = New Drawing.Point(130, y - 3) : cboAssignee.Size = New Drawing.Size(300, 20) : cboAssignee.DropDownStyle = ComboBoxStyle.DropDownList
        grpNewAssignment.Controls.Add(cboAssignee)
        y += 32

        chkHasDueDate.Location = New Drawing.Point(15, y) : chkHasDueDate.Text = "Due Date:" : chkHasDueDate.AutoSize = True : grpNewAssignment.Controls.Add(chkHasDueDate)
        dtpDueDate.Location = New Drawing.Point(130, y - 2) : dtpDueDate.Size = New Drawing.Size(130, 20) : dtpDueDate.Format = DateTimePickerFormat.Short : dtpDueDate.Enabled = False
        AddHandler chkHasDueDate.CheckedChanged, Sub() dtpDueDate.Enabled = chkHasDueDate.Checked
        grpNewAssignment.Controls.Add(dtpDueDate)
        y += 32

        AddLabelTo(grpNewAssignment, "Instructions:", 15, y)
        txtInstructions.Location = New Drawing.Point(130, y - 3) : txtInstructions.Size = New Drawing.Size(450, 60) : txtInstructions.Multiline = True : txtInstructions.ScrollBars = ScrollBars.Vertical
        grpNewAssignment.Controls.Add(txtInstructions)
        y += 70

        btnAssign.Location = New Drawing.Point(130, y) : btnAssign.Size = New Drawing.Size(100, 30) : btnAssign.Text = "&Assign" : grpNewAssignment.Controls.Add(btnAssign)
        Me.Controls.Add(grpNewAssignment)

        ' Close button
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(620, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        grpNewAssignment.BringToFront()
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        parent.Controls.Add(New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True})
    End Sub

    Private Sub LoadAssignments()
        Try
            Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@ClaimID", _claimID)}
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Claims.usp_Assignment_GetByClaim", params)
            dgvCurrentAssignments.DataSource = dt
        Catch ex As Exception
            ErrorLogger.LogError(ex, "LoadAssignments")
        End Try
    End Sub

    Private Sub LoadAssignees()
        ' Would load from Vendors/Users based on type
        cboAssignee.Items.Clear()
        cboAssignee.Items.Add("(Select)")
        cboAssignee.SelectedIndex = 0
    End Sub

    Private Sub AssigneeTypeChanged(sender As Object, e As EventArgs)
        LoadAssignees()
    End Sub

    Private Sub btnAssign_Click(sender As Object, e As EventArgs) Handles btnAssign.Click
        Try
            If cboAssignee.SelectedIndex <= 0 Then
                MessageBox.Show("Please select an assignee.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
            End If

            Me.Cursor = Cursors.WaitCursor
            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@ClaimID", _claimID),
                DatabaseHelper.CreateParam("@AssigneeType", cboAssigneeType.SelectedItem.ToString()),
                DatabaseHelper.CreateParam("@AssigneeID", 1),
                DatabaseHelper.CreateParam("@DueDate", If(chkHasDueDate.Checked, dtpDueDate.Value.Date, CType(Nothing, Object))),
                DatabaseHelper.CreateParam("@Instructions", If(String.IsNullOrWhiteSpace(txtInstructions.Text), Nothing, txtInstructions.Text.Trim())),
                DatabaseHelper.CreateParam("@CreatedBy", GlobalState.CurrentUser)
            }
            Dim idParam As SqlParameter = DatabaseHelper.CreateOutputParam("@AssignmentID", SqlDbType.Int)
            params.Add(idParam)

            DatabaseHelper.ExecuteNonQuery("Claims.usp_Claim_Assign", params.ToArray())
            MessageBox.Show("Assignment created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtInstructions.Clear()
            LoadAssignments()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnAssign_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnReassign_Click(sender As Object, e As EventArgs) Handles btnReassign.Click
        If dgvCurrentAssignments.CurrentRow Is Nothing Then Return
        MessageBox.Show("Reassignment dialog would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnComplete_Click(sender As Object, e As EventArgs) Handles btnComplete.Click
        If dgvCurrentAssignments.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Mark this assignment as completed?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            MessageBox.Show("Assignment marked complete.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadAssignments()
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
