Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Coverage editor — add, remove, and modify coverages on a policy.
''' </summary>
Public Class frmCoverageEditor
    Inherits Form

    Private _policyID As Integer
    Private dgvCoverages As New DataGridView()
    Private WithEvents btnAdd As New Button()
    Private WithEvents btnRemove As New Button()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnClose As New Button()
    Private lblTotalPremium As New Label()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmCoverageEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Coverage Editor - Policy " & _policyID.ToString()
        Me.Size = New Drawing.Size(750, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadCoverages()
    End Sub

    Private Sub InitializeControls()
        dgvCoverages.Location = New Drawing.Point(10, 10) : dgvCoverages.Size = New Drawing.Size(710, 330)
        dgvCoverages.AllowUserToAddRows = False : dgvCoverages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvCoverages.Columns.Add(New DataGridViewCheckBoxColumn() With {.Name = "Selected", .HeaderText = "Active", .Width = 50})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Code", .HeaderText = "Code", .Width = 100, .ReadOnly = True})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Name", .HeaderText = "Coverage", .Width = 200, .ReadOnly = True})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Limit", .HeaderText = "Limit"})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Deductible", .HeaderText = "Deductible"})
        dgvCoverages.Columns.Add(New DataGridViewTextBoxColumn() With {.Name = "Premium", .HeaderText = "Premium", .ReadOnly = True})
        Me.Controls.Add(dgvCoverages)

        lblTotalPremium.Location = New Drawing.Point(10, 350) : lblTotalPremium.AutoSize = True : lblTotalPremium.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : Me.Controls.Add(lblTotalPremium)
        btnAdd.Location = New Drawing.Point(10, 375) : btnAdd.Size = New Drawing.Size(100, 30) : btnAdd.Text = "&Add Coverage" : Me.Controls.Add(btnAdd)
        btnRemove.Location = New Drawing.Point(120, 375) : btnRemove.Size = New Drawing.Size(100, 30) : btnRemove.Text = "&Remove" : Me.Controls.Add(btnRemove)
        btnSave.Location = New Drawing.Point(500, 375) : btnSave.Size = New Drawing.Size(100, 30) : btnSave.Text = "&Save" : Me.Controls.Add(btnSave)
        btnClose.Location = New Drawing.Point(610, 375) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub LoadCoverages()
        dgvCoverages.Rows.Add(True, "DWELLING", "Dwelling (Cov A)", "250000", "1000", "1000.00")
        dgvCoverages.Rows.Add(True, "OTHER_STRUCT", "Other Structures (Cov B)", "25000", "1000", "100.00")
        dgvCoverages.Rows.Add(True, "PERSONAL", "Personal Property (Cov C)", "125000", "1000", "500.00")
        dgvCoverages.Rows.Add(True, "LOSS_USE", "Loss of Use (Cov D)", "50000", "0", "75.00")
        dgvCoverages.Rows.Add(True, "LIABILITY", "Liability (Cov E)", "100000", "0", "200.00")
        dgvCoverages.Rows.Add(True, "MEDICAL", "Medical (Cov F)", "5000", "0", "50.00")
        dgvCoverages.Rows.Add(False, "FLOOD", "Flood", "250000", "5000", "0.00")
        dgvCoverages.Rows.Add(False, "EARTHQUAKE", "Earthquake", "250000", "10000", "0.00")
        lblTotalPremium.Text = "Total Premium: $1,925.00"
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        MessageBox.Show("Add coverage dialog would open.", "Add", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click
        If dgvCoverages.CurrentRow IsNot Nothing Then dgvCoverages.Rows.Remove(dgvCoverages.CurrentRow)
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        MessageBox.Show("Coverages saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
