Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Fraud indicator review form — shows fraud score breakdown and allows SIU referral.
''' </summary>
Public Class frmFraudReview
    Inherits Form

    Private _claimID As Integer
    Private lblClaimNumber As New Label()
    Private lblFraudScore As New Label()
    Private lblSIUStatus As New Label()
    Private dgvIndicators As New DataGridView()
    Private WithEvents btnEvaluate As New Button()
    Private WithEvents btnReferSIU As New Button()
    Private WithEvents btnClose As New Button()
    Private txtReferralReason As New TextBox()
    Private pnlScoreGauge As New Panel()

    Public Sub New(claimID As Integer)
        _claimID = claimID
    End Sub

    Private Sub frmFraudReview_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Text = "Fraud Review"
            Me.Size = New Drawing.Size(800, 550)
            Me.StartPosition = FormStartPosition.CenterParent
            InitializeControls()
            LoadFraudData()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmFraudReview_Load")
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Header
        Dim pnlHeader As New Panel() With {.Dock = DockStyle.Top, .Height = 80}
        lblClaimNumber.Location = New Drawing.Point(10, 10) : lblClaimNumber.AutoSize = True : lblClaimNumber.Font = New Drawing.Font("Segoe UI", 12, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblClaimNumber)
        lblFraudScore.Location = New Drawing.Point(10, 40) : lblFraudScore.AutoSize = True : lblFraudScore.Font = New Drawing.Font("Segoe UI", 14, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblFraudScore)
        lblSIUStatus.Location = New Drawing.Point(250, 40) : lblSIUStatus.AutoSize = True : pnlHeader.Controls.Add(lblSIUStatus)

        pnlScoreGauge.Location = New Drawing.Point(500, 10) : pnlScoreGauge.Size = New Drawing.Size(250, 60) : pnlScoreGauge.BorderStyle = BorderStyle.FixedSingle : pnlHeader.Controls.Add(pnlScoreGauge)
        AddHandler pnlScoreGauge.Paint, AddressOf PaintScoreGauge
        Me.Controls.Add(pnlHeader)

        ' Indicators grid
        dgvIndicators.Dock = DockStyle.Fill : dgvIndicators.ReadOnly = True : dgvIndicators.AllowUserToAddRows = False
        dgvIndicators.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvIndicators.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvIndicators)

        ' Bottom panel
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 100}
        pnlBottom.Controls.Add(New Label() With {.Text = "Referral Reason:", .Location = New Drawing.Point(10, 10), .AutoSize = True})
        txtReferralReason.Location = New Drawing.Point(120, 7) : txtReferralReason.Size = New Drawing.Size(400, 40) : txtReferralReason.Multiline = True : pnlBottom.Controls.Add(txtReferralReason)
        btnEvaluate.Location = New Drawing.Point(10, 60) : btnEvaluate.Size = New Drawing.Size(110, 30) : btnEvaluate.Text = "Re-&Evaluate" : pnlBottom.Controls.Add(btnEvaluate)
        btnReferSIU.Location = New Drawing.Point(130, 60) : btnReferSIU.Size = New Drawing.Size(110, 30) : btnReferSIU.Text = "Refer to &SIU" : btnReferSIU.BackColor = Drawing.Color.LightCoral : pnlBottom.Controls.Add(btnReferSIU)
        btnClose.Location = New Drawing.Point(660, 60) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvIndicators.BringToFront()
    End Sub

    Private Sub LoadFraudData()
        Try
            Dim params() As SqlParameter = {DatabaseHelper.CreateParam("@ClaimID", _claimID)}
            Dim ds As DataSet = DatabaseHelper.ExecuteDataSet("Claims.usp_Fraud_GetEvaluation", params)

            If ds.Tables.Count > 0 AndAlso ds.Tables(0).Rows.Count > 0 Then
                Dim row As DataRow = ds.Tables(0).Rows(0)
                lblClaimNumber.Text = $"Claim: {row("ClaimNumber")}"
                Dim score As Decimal = CDec(If(IsDBNull(row("FraudScore")), 0, row("FraudScore")))
                lblFraudScore.Text = $"Fraud Score: {score:N1}/100"
                lblFraudScore.ForeColor = If(score >= 70, Drawing.Color.Red, If(score >= 40, Drawing.Color.DarkOrange, Drawing.Color.Green))

                Dim isSIU As Boolean = CBool(If(IsDBNull(row("IsSIUReferred")), False, row("IsSIUReferred")))
                lblSIUStatus.Text = If(isSIU, "⚠ REFERRED TO SIU", "Not referred")
                lblSIUStatus.ForeColor = If(isSIU, Drawing.Color.Red, Drawing.Color.Gray)
                btnReferSIU.Enabled = Not isSIU
            End If

            If ds.Tables.Count > 1 Then dgvIndicators.DataSource = ds.Tables(1)

            pnlScoreGauge.Invalidate()
        Catch ex As Exception
            MessageBox.Show("Error loading fraud data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub PaintScoreGauge(sender As Object, e As PaintEventArgs)
        ' Simple score gauge visualization
        Dim g As Drawing.Graphics = e.Graphics
        Dim width As Integer = pnlScoreGauge.Width - 20
        Dim height As Integer = 20
        Dim y As Integer = 20

        ' Background
        g.FillRectangle(Drawing.Brushes.LightGray, 10, y, width, height)

        ' Score bar
        Dim scoreText As String = lblFraudScore.Text
        Dim score As Decimal = 0
        If scoreText.Contains(":") Then
            Decimal.TryParse(scoreText.Split(":"c)(1).Replace("/100", "").Trim(), score)
        End If

        Dim barWidth As Integer = CInt((score / 100) * width)
        Dim barColor As Drawing.Brush = If(score >= 70, Drawing.Brushes.Red, If(score >= 40, Drawing.Brushes.Orange, Drawing.Brushes.Green))
        g.FillRectangle(barColor, 10, y, barWidth, height)

        ' Labels
        g.DrawString("0", Me.Font, Drawing.Brushes.Black, 10, y + height + 2)
        g.DrawString("70", Me.Font, Drawing.Brushes.Red, CInt(0.7 * width), y + height + 2)
        g.DrawString("100", Me.Font, Drawing.Brushes.Black, width - 10, y + height + 2)
    End Sub

    Private Sub btnEvaluate_Click(sender As Object, e As EventArgs) Handles btnEvaluate.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@ClaimID", _claimID),
                DatabaseHelper.CreateParam("@EvaluatedBy", GlobalState.CurrentUser)
            }
            Dim scoreParam As SqlParameter = DatabaseHelper.CreateOutputParam("@FraudScore", SqlDbType.Decimal)
            params.Add(scoreParam)
            DatabaseHelper.ExecuteNonQuery("Claims.usp_Fraud_EvaluateClaim", params.ToArray())
            LoadFraudData()
            MessageBox.Show($"Re-evaluation complete. Score: {CDec(scoreParam.Value):N1}/100", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnReferSIU_Click(sender As Object, e As EventArgs) Handles btnReferSIU.Click
        If String.IsNullOrWhiteSpace(txtReferralReason.Text) Then
            MessageBox.Show("Please enter a referral reason.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If

        Try
            Dim params() As SqlParameter = {
                DatabaseHelper.CreateParam("@ClaimID", _claimID),
                DatabaseHelper.CreateParam("@ReferralReason", txtReferralReason.Text.Trim()),
                DatabaseHelper.CreateParam("@ReferredBy", GlobalState.CurrentUser)
            }
            DatabaseHelper.ExecuteNonQuery("Claims.usp_Fraud_ReferToSIU", params)
            MessageBox.Show("Claim referred to SIU.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadFraudData()
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
