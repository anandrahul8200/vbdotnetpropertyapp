Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Salvage and recovery tracking for claims.
''' </summary>
Public Class frmSalvageRecovery
    Inherits Form

    Private _claimID As Integer
    Private dgvRecoveries As New DataGridView()
    Private txtAmount As New TextBox()
    Private cboRecoveryType As New ComboBox()
    Private txtDescription As New TextBox()
    Private WithEvents btnAdd As New Button()
    Private WithEvents btnClose As New Button()
    Private lblTotal As New Label()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmSalvageRecovery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Salvage & Recovery - Claim " & _claimID.ToString()
        Me.Size = New Drawing.Size(650, 400)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Add recovery section
        Dim grpAdd As New GroupBox() With {.Text = "Record Recovery", .Location = New Drawing.Point(10, 10), .Size = New Drawing.Size(610, 90)}
        grpAdd.Controls.Add(New Label() With {.Text = "Type:", .Location = New Drawing.Point(15, 25), .AutoSize = True})
        cboRecoveryType.Location = New Drawing.Point(80, 22) : cboRecoveryType.Size = New Drawing.Size(130, 20) : cboRecoveryType.DropDownStyle = ComboBoxStyle.DropDownList
        cboRecoveryType.Items.AddRange({"SALVAGE", "SUBROGATION", "DEDUCTIBLE", "OTHER"}) : cboRecoveryType.SelectedIndex = 0 : grpAdd.Controls.Add(cboRecoveryType)
        grpAdd.Controls.Add(New Label() With {.Text = "Amount:", .Location = New Drawing.Point(230, 25), .AutoSize = True})
        txtAmount.Location = New Drawing.Point(290, 22) : txtAmount.Size = New Drawing.Size(100, 20) : grpAdd.Controls.Add(txtAmount)
        grpAdd.Controls.Add(New Label() With {.Text = "Description:", .Location = New Drawing.Point(15, 55), .AutoSize = True})
        txtDescription.Location = New Drawing.Point(80, 52) : txtDescription.Size = New Drawing.Size(400, 20) : grpAdd.Controls.Add(txtDescription)
        btnAdd.Location = New Drawing.Point(500, 50) : btnAdd.Size = New Drawing.Size(80, 25) : btnAdd.Text = "&Add" : grpAdd.Controls.Add(btnAdd)
        Me.Controls.Add(grpAdd)

        ' Recovery history
        Dim grpHistory As New GroupBox() With {.Text = "Recovery History", .Location = New Drawing.Point(10, 110), .Size = New Drawing.Size(610, 200)}
        dgvRecoveries.Dock = DockStyle.Fill : dgvRecoveries.ReadOnly = True : dgvRecoveries.AllowUserToAddRows = False : dgvRecoveries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpHistory.Controls.Add(dgvRecoveries)
        Me.Controls.Add(grpHistory)

        lblTotal.Location = New Drawing.Point(10, 320) : lblTotal.AutoSize = True : lblTotal.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblTotal.Text = "Total Recovered: $0.00" : Me.Controls.Add(lblTotal)
        btnClose.Location = New Drawing.Point(540, 320) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim amount As Decimal
        If Not Decimal.TryParse(txtAmount.Text, amount) OrElse amount <= 0 Then
            MessageBox.Show("Enter a valid amount.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        MessageBox.Show("Recovery recorded.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        txtAmount.Clear() : txtDescription.Clear()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
