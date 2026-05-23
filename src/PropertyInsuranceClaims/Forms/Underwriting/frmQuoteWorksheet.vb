Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Rating worksheet display — shows the full factor breakdown for a policy's premium calculation.
''' </summary>
Public Class frmQuoteWorksheet
    Inherits Form

    Private _policyID As Integer
    Private dgvWorksheet As New DataGridView()
    Private lblPolicyNumber As New Label()
    Private lblTotalPremium As New Label()
    Private WithEvents btnRecalculate As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmQuoteWorksheet_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Rating Worksheet"
            Me.Size = New Drawing.Size(900, 500)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadWorksheet()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmQuoteWorksheet_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 50}
        lblPolicyNumber.Location = New Drawing.Point(10, 12) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font("Segoe UI", 11, Drawing.FontStyle.Bold) : pnlTop.Controls.Add(lblPolicyNumber)
        lblTotalPremium.Location = New Drawing.Point(300, 12) : lblTotalPremium.AutoSize = True : lblTotalPremium.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlTop.Controls.Add(lblTotalPremium)
        btnRecalculate.Location = New Drawing.Point(600, 8) : btnRecalculate.Size = New Drawing.Size(100, 30) : btnRecalculate.Text = "&Recalculate" : pnlTop.Controls.Add(btnRecalculate)
        btnClose.Location = New Drawing.Point(710, 8) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlTop.Controls.Add(btnClose)
        Me.Controls.Add(pnlTop)

        dgvWorksheet.Dock = DockStyle.Fill : dgvWorksheet.ReadOnly = True : dgvWorksheet.AllowUserToAddRows = False
        dgvWorksheet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvWorksheet.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.AliceBlue
        Me.Controls.Add(dgvWorksheet)
        dgvWorksheet.BringToFront()
    End Sub

    Private Sub LoadWorksheet()
        Dim dt As DataTable = UnderwritingDataAccess.GetRatingWorksheet(_policyID)
        dgvWorksheet.DataSource = dt

        ' Calculate total
        Dim total As Decimal = 0
        For Each row As DataRow In dt.Rows
            If Not IsDBNull(row("FinalPremium")) Then total += CDec(row("FinalPremium"))
        Next
        lblTotalPremium.Text = $"Total Premium: {total:C}"
    End Sub

    Private Sub btnRecalculate_Click(sender As Object, e As EventArgs) Handles btnRecalculate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            UnderwritingDataAccess.CalculatePremium(_policyID)
            LoadWorksheet()
            MessageBox.Show("Premium recalculated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
