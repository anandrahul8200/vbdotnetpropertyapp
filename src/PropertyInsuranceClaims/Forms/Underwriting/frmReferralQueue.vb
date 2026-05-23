Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Underwriting referral workqueue — review and process pending referrals.
''' </summary>
Public Class frmReferralQueue
    Inherits Form

    Private dgvReferrals As New DataGridView()
    Private WithEvents btnApprove As New Button()
    Private WithEvents btnDecline As New Button()
    Private WithEvents btnConditional As New Button()
    Private WithEvents btnRefresh As New Button()
    Private WithEvents btnViewPolicy As New Button()
    Private lblCount As New Label()
    Private txtNotes As New TextBox()
    Private txtConditions As New TextBox()
    Private grpDecision As New GroupBox()

    Private Sub frmReferralQueue_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Underwriting Referral Queue"
            Me.Size = New Drawing.Size(1000, 650)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadReferrals()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmReferralQueue_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Top toolbar
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 40}
        btnRefresh.Location = New Drawing.Point(10, 6) : btnRefresh.Size = New Drawing.Size(80, 28) : btnRefresh.Text = "&Refresh" : pnlTop.Controls.Add(btnRefresh)
        btnViewPolicy.Location = New Drawing.Point(100, 6) : btnViewPolicy.Size = New Drawing.Size(100, 28) : btnViewPolicy.Text = "&View Policy" : pnlTop.Controls.Add(btnViewPolicy)
        lblCount.Location = New Drawing.Point(250, 12) : lblCount.AutoSize = True : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        ' Grid (top half)
        dgvReferrals.Dock = DockStyle.Top : dgvReferrals.Height = 300
        dgvReferrals.ReadOnly = True : dgvReferrals.AllowUserToAddRows = False
        dgvReferrals.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvReferrals.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvReferrals)

        ' Decision panel (bottom half)
        grpDecision.Text = "Decision" : grpDecision.Dock = DockStyle.Fill
        grpDecision.Controls.Add(New Label() With {.Text = "Notes:", .Location = New Drawing.Point(10, 25), .AutoSize = True})
        txtNotes.Location = New Drawing.Point(80, 22) : txtNotes.Size = New Drawing.Size(500, 60) : txtNotes.Multiline = True : grpDecision.Controls.Add(txtNotes)
        grpDecision.Controls.Add(New Label() With {.Text = "Conditions:", .Location = New Drawing.Point(10, 95), .AutoSize = True})
        txtConditions.Location = New Drawing.Point(80, 92) : txtConditions.Size = New Drawing.Size(500, 40) : txtConditions.Multiline = True : grpDecision.Controls.Add(txtConditions)

        btnApprove.Location = New Drawing.Point(80, 145) : btnApprove.Size = New Drawing.Size(100, 35) : btnApprove.Text = "&Approve" : btnApprove.BackColor = Drawing.Color.LightGreen : grpDecision.Controls.Add(btnApprove)
        btnConditional.Location = New Drawing.Point(190, 145) : btnConditional.Size = New Drawing.Size(100, 35) : btnConditional.Text = "C&onditional" : btnConditional.BackColor = Drawing.Color.LightYellow : grpDecision.Controls.Add(btnConditional)
        btnDecline.Location = New Drawing.Point(300, 145) : btnDecline.Size = New Drawing.Size(100, 35) : btnDecline.Text = "&Decline" : btnDecline.BackColor = Drawing.Color.LightCoral : grpDecision.Controls.Add(btnDecline)
        Me.Controls.Add(grpDecision)
        grpDecision.BringToFront()
    End Sub

    Private Sub LoadReferrals()
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim dt As DataTable = UnderwritingDataAccess.GetPendingReferrals()
            dgvReferrals.DataSource = dt
            If dgvReferrals.Columns.Contains("ReferralID") Then dgvReferrals.Columns("ReferralID").Visible = False
            If dgvReferrals.Columns.Contains("PolicyID") Then dgvReferrals.Columns("PolicyID").Visible = False
            lblCount.Text = $"{dt.Rows.Count} pending referral(s)"
        Catch ex As Exception
            MessageBox.Show("Error loading referrals: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Function GetSelectedReferralID() As Integer
        If dgvReferrals.CurrentRow Is Nothing Then Return 0
        Return CInt(dgvReferrals.CurrentRow.Cells("ReferralID").Value)
    End Function

    Private Sub ProcessDecision(decision As String)
        Dim referralID As Integer = GetSelectedReferralID()
        If referralID = 0 Then
            MessageBox.Show("Please select a referral.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information) : Return
        End If

        Try
            Me.Cursor = Cursors.WaitCursor
            UnderwritingDataAccess.ProcessReferral(referralID, decision, txtNotes.Text.Trim(), txtConditions.Text.Trim())
            MessageBox.Show($"Referral {decision}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtNotes.Clear() : txtConditions.Clear()
            LoadReferrals()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "ProcessDecision")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnApprove_Click(sender As Object, e As EventArgs) Handles btnApprove.Click
        ProcessDecision("APPROVED")
    End Sub

    Private Sub btnDecline_Click(sender As Object, e As EventArgs) Handles btnDecline.Click
        If MessageBox.Show("Are you sure you want to decline this referral?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            ProcessDecision("DECLINED")
        End If
    End Sub

    Private Sub btnConditional_Click(sender As Object, e As EventArgs) Handles btnConditional.Click
        If String.IsNullOrWhiteSpace(txtConditions.Text) Then
            MessageBox.Show("Please enter conditions for conditional approval.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        ProcessDecision("CONDITIONAL")
    End Sub

    Private Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        LoadReferrals()
    End Sub

    Private Sub btnViewPolicy_Click(sender As Object, e As EventArgs) Handles btnViewPolicy.Click
        If dgvReferrals.CurrentRow Is Nothing Then Return
        Dim policyID As Integer = CInt(dgvReferrals.CurrentRow.Cells("PolicyID").Value)
        Dim frm As New frmPolicyView(policyID)
        frm.ShowDialog(Me)
    End Sub
End Class
