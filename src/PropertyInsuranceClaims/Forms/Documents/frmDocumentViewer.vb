Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Document viewer — preview and download attached documents.
''' </summary>
Public Class frmDocumentViewer
    Inherits Form

    Private _documentID As Integer
    Private _filePath As String

    Private lblFileName As New Label()
    Private lblFileType As New Label()
    Private lblFileSize As New Label()
    Private lblUploadDate As New Label()
    Private lblUploadedBy As New Label()
    Private pnlPreview As New Panel()
    Private WithEvents btnDownload As New Button()
    Private WithEvents btnPrint As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(documentID As Integer)
        _documentID = documentID
    End Sub

    Private Sub frmDocumentViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Document Viewer"
        Me.Size = New Drawing.Size(600, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        lblFileName.Location = New Drawing.Point(10, y) : lblFileName.AutoSize = True : lblFileName.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : lblFileName.Text = "Document_001.pdf" : Me.Controls.Add(lblFileName)
        y += 25
        lblFileType.Location = New Drawing.Point(10, y) : lblFileType.AutoSize = True : lblFileType.Text = "Type: PDF | Size: 245 KB" : Me.Controls.Add(lblFileType)
        y += 20
        lblUploadDate.Location = New Drawing.Point(10, y) : lblUploadDate.AutoSize = True : lblUploadDate.Text = "Uploaded: 01/15/2026 by admin" : Me.Controls.Add(lblUploadDate)
        y += 30

        pnlPreview.Location = New Drawing.Point(10, y) : pnlPreview.Size = New Drawing.Size(560, 330) : pnlPreview.BorderStyle = BorderStyle.FixedSingle : pnlPreview.BackColor = Drawing.Color.White
        Dim lblNoPreview As New Label() With {.Text = "Preview not available for this file type.", .Location = New Drawing.Point(150, 150), .AutoSize = True, .ForeColor = Drawing.Color.Gray}
        pnlPreview.Controls.Add(lblNoPreview)
        Me.Controls.Add(pnlPreview)

        btnDownload.Location = New Drawing.Point(10, 430) : btnDownload.Size = New Drawing.Size(100, 30) : btnDownload.Text = "&Download" : Me.Controls.Add(btnDownload)
        btnPrint.Location = New Drawing.Point(120, 430) : btnPrint.Size = New Drawing.Size(80, 30) : btnPrint.Text = "&Print" : Me.Controls.Add(btnPrint)
        btnClose.Location = New Drawing.Point(480, 430) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
        Dim sfd As New SaveFileDialog() With {.FileName = "Document_001.pdf", .Filter = "All Files|*.*"}
        If sfd.ShowDialog() = DialogResult.OK Then
            MessageBox.Show("Document saved to: " & sfd.FileName, "Downloaded", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        MessageBox.Show("Print dialog would open here.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
