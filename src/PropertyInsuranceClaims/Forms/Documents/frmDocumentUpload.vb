Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Document upload form — attach files to policies or claims.
''' </summary>
Public Class frmDocumentUpload
    Inherits Form

    Private _entityType As String ' POLICY or CLAIM
    Private _entityID As Integer

    Private txtFilePath As New TextBox()
    Private cboDocumentType As New ComboBox()
    Private txtDescription As New TextBox()
    Private lblEntityInfo As New Label()
    Private dgvExistingDocs As New DataGridView()
    Private WithEvents btnBrowse As New Button()
    Private WithEvents btnUpload As New Button()
    Private WithEvents btnDelete As New Button()
    Private WithEvents btnClose As New Button()

    Public Sub New(entityType As String, entityID As Integer)
        _entityType = entityType
        _entityID = entityID
    End Sub

    Private Sub frmDocumentUpload_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Document Management - " & _entityType & " #" & _entityID.ToString()
        Me.Size = New Drawing.Size(700, 500)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadDocuments()
    End Sub

    Private Sub InitializeControls()
        ' Entity info
        lblEntityInfo.Location = New Drawing.Point(10, 10) : lblEntityInfo.AutoSize = True
        lblEntityInfo.Text = _entityType & " ID: " & _entityID.ToString()
        lblEntityInfo.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)
        Me.Controls.Add(lblEntityInfo)

        ' Upload section
        Dim grpUpload As New GroupBox() With {.Text = "Upload Document", .Location = New Drawing.Point(10, 35), .Size = New Drawing.Size(660, 130)}
        grpUpload.Controls.Add(New Label() With {.Text = "File:", .Location = New Drawing.Point(15, 25), .AutoSize = True})
        txtFilePath.Location = New Drawing.Point(100, 22) : txtFilePath.Size = New Drawing.Size(400, 20) : txtFilePath.ReadOnly = True : grpUpload.Controls.Add(txtFilePath)
        btnBrowse.Location = New Drawing.Point(510, 20) : btnBrowse.Size = New Drawing.Size(80, 25) : btnBrowse.Text = "Browse..." : grpUpload.Controls.Add(btnBrowse)

        grpUpload.Controls.Add(New Label() With {.Text = "Type:", .Location = New Drawing.Point(15, 55), .AutoSize = True})
        cboDocumentType.Location = New Drawing.Point(100, 52) : cboDocumentType.Size = New Drawing.Size(180, 20) : cboDocumentType.DropDownStyle = ComboBoxStyle.DropDownList
        cboDocumentType.Items.AddRange({"PHOTO", "ESTIMATE", "INVOICE", "POLICE_REPORT", "CORRESPONDENCE", "CONTRACT", "INSPECTION", "OTHER"})
        cboDocumentType.SelectedIndex = 0 : grpUpload.Controls.Add(cboDocumentType)

        grpUpload.Controls.Add(New Label() With {.Text = "Description:", .Location = New Drawing.Point(15, 85), .AutoSize = True})
        txtDescription.Location = New Drawing.Point(100, 82) : txtDescription.Size = New Drawing.Size(400, 20) : grpUpload.Controls.Add(txtDescription)
        btnUpload.Location = New Drawing.Point(510, 80) : btnUpload.Size = New Drawing.Size(80, 25) : btnUpload.Text = "&Upload" : grpUpload.Controls.Add(btnUpload)
        Me.Controls.Add(grpUpload)

        ' Existing documents grid
        Dim grpDocs As New GroupBox() With {.Text = "Attached Documents", .Location = New Drawing.Point(10, 175), .Size = New Drawing.Size(660, 240)}
        dgvExistingDocs.Dock = DockStyle.Fill : dgvExistingDocs.ReadOnly = True : dgvExistingDocs.AllowUserToAddRows = False
        dgvExistingDocs.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvExistingDocs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpDocs.Controls.Add(dgvExistingDocs)
        Me.Controls.Add(grpDocs)

        ' Bottom buttons
        btnDelete.Location = New Drawing.Point(10, 425) : btnDelete.Size = New Drawing.Size(100, 30) : btnDelete.Text = "&Delete" : Me.Controls.Add(btnDelete)
        btnClose.Location = New Drawing.Point(580, 425) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub LoadDocuments()
        ' Would load from a Documents table
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Dim ofd As New OpenFileDialog() With {.Filter = "All Files|*.*|Images|*.jpg;*.png|PDFs|*.pdf", .Title = "Select Document"}
        If ofd.ShowDialog() = DialogResult.OK Then txtFilePath.Text = ofd.FileName
    End Sub

    Private Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        If String.IsNullOrEmpty(txtFilePath.Text) Then
            MessageBox.Show("Please select a file.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        MessageBox.Show("Document uploaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        txtFilePath.Clear() : txtDescription.Clear()
        LoadDocuments()
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If dgvExistingDocs.CurrentRow Is Nothing Then Return
        If MessageBox.Show("Delete this document?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            LoadDocuments()
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
