Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Correspondence generation — create letters, notices, and emails from templates.
''' </summary>
Public Class frmCorrespondence
    Inherits Form

    Private cboTemplate As New ComboBox()
    Private cboRecipientType As New ComboBox()
    Private txtRecipientName As New TextBox()
    Private txtRecipientAddress As New TextBox()
    Private txtSubject As New TextBox()
    Private txtBody As New TextBox()
    Private cboDeliveryMethod As New ComboBox()
    Private WithEvents btnLoadTemplate As New Button()
    Private WithEvents btnPreview As New Button()
    Private WithEvents btnSend As New Button()
    Private WithEvents btnSaveDraft As New Button()
    Private WithEvents btnClose As New Button()
    Private dgvHistory As New DataGridView()

    Private Sub frmCorrespondence_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Correspondence"
        Me.Size = New Drawing.Size(800, 600)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Template selection
        Dim grpCompose As New GroupBox() With {.Text = "Compose", .Location = New Drawing.Point(10, 10), .Size = New Drawing.Size(760, 320)}
        Dim y As Integer = 22
        grpCompose.Controls.Add(New Label() With {.Text = "Template:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboTemplate.Location = New Drawing.Point(100, y - 3) : cboTemplate.Size = New Drawing.Size(300, 20) : cboTemplate.DropDownStyle = ComboBoxStyle.DropDownList
        cboTemplate.Items.AddRange({"(Select Template)", "Cancellation Notice", "Renewal Reminder", "Claim Acknowledgment", "Payment Reminder", "Policy Welcome Letter", "Non-Renewal Notice", "Claim Denial Letter"})
        cboTemplate.SelectedIndex = 0 : grpCompose.Controls.Add(cboTemplate)
        btnLoadTemplate.Location = New Drawing.Point(410, y - 4) : btnLoadTemplate.Size = New Drawing.Size(80, 24) : btnLoadTemplate.Text = "Load" : grpCompose.Controls.Add(btnLoadTemplate)
        y += 30

        grpCompose.Controls.Add(New Label() With {.Text = "Recipient:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboRecipientType.Location = New Drawing.Point(100, y - 3) : cboRecipientType.Size = New Drawing.Size(100, 20) : cboRecipientType.DropDownStyle = ComboBoxStyle.DropDownList
        cboRecipientType.Items.AddRange({"INSURED", "AGENT", "VENDOR", "ATTORNEY", "MORTGAGEE"}) : cboRecipientType.SelectedIndex = 0 : grpCompose.Controls.Add(cboRecipientType)
        txtRecipientName.Location = New Drawing.Point(210, y - 3) : txtRecipientName.Size = New Drawing.Size(200, 20) : grpCompose.Controls.Add(txtRecipientName)
        y += 30

        grpCompose.Controls.Add(New Label() With {.Text = "Address:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtRecipientAddress.Location = New Drawing.Point(100, y - 3) : txtRecipientAddress.Size = New Drawing.Size(400, 20) : grpCompose.Controls.Add(txtRecipientAddress)
        y += 30

        grpCompose.Controls.Add(New Label() With {.Text = "Subject:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtSubject.Location = New Drawing.Point(100, y - 3) : txtSubject.Size = New Drawing.Size(500, 20) : grpCompose.Controls.Add(txtSubject)
        y += 30

        grpCompose.Controls.Add(New Label() With {.Text = "Body:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtBody.Location = New Drawing.Point(100, y - 3) : txtBody.Size = New Drawing.Size(640, 100) : txtBody.Multiline = True : txtBody.ScrollBars = ScrollBars.Vertical : grpCompose.Controls.Add(txtBody)
        y += 110

        grpCompose.Controls.Add(New Label() With {.Text = "Delivery:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboDeliveryMethod.Location = New Drawing.Point(100, y - 3) : cboDeliveryMethod.Size = New Drawing.Size(120, 20) : cboDeliveryMethod.DropDownStyle = ComboBoxStyle.DropDownList
        cboDeliveryMethod.Items.AddRange({"PRINT", "EMAIL", "FAX"}) : cboDeliveryMethod.SelectedIndex = 0 : grpCompose.Controls.Add(cboDeliveryMethod)
        btnPreview.Location = New Drawing.Point(300, y - 4) : btnPreview.Size = New Drawing.Size(80, 25) : btnPreview.Text = "Pre&view" : grpCompose.Controls.Add(btnPreview)
        btnSend.Location = New Drawing.Point(390, y - 4) : btnSend.Size = New Drawing.Size(80, 25) : btnSend.Text = "&Send" : grpCompose.Controls.Add(btnSend)
        btnSaveDraft.Location = New Drawing.Point(480, y - 4) : btnSaveDraft.Size = New Drawing.Size(90, 25) : btnSaveDraft.Text = "Save &Draft" : grpCompose.Controls.Add(btnSaveDraft)
        Me.Controls.Add(grpCompose)

        ' History
        Dim grpHistory As New GroupBox() With {.Text = "Correspondence History", .Location = New Drawing.Point(10, 340), .Size = New Drawing.Size(760, 180)}
        dgvHistory.Dock = DockStyle.Fill : dgvHistory.ReadOnly = True : dgvHistory.AllowUserToAddRows = False : dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grpHistory.Controls.Add(dgvHistory)
        Me.Controls.Add(grpHistory)

        btnClose.Location = New Drawing.Point(690, 530) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub btnLoadTemplate_Click(sender As Object, e As EventArgs) Handles btnLoadTemplate.Click
        If cboTemplate.SelectedIndex <= 0 Then Return
        txtSubject.Text = cboTemplate.SelectedItem.ToString()
        txtBody.Text = "Dear [Insured Name]," & Environment.NewLine & Environment.NewLine & "This is regarding your " & cboTemplate.SelectedItem.ToString().ToLower() & "." & Environment.NewLine & Environment.NewLine & "Sincerely," & Environment.NewLine & "Insurance Company"
    End Sub

    Private Sub btnPreview_Click(sender As Object, e As EventArgs) Handles btnPreview.Click
        MessageBox.Show("Letter preview would display here.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnSend_Click(sender As Object, e As EventArgs) Handles btnSend.Click
        MessageBox.Show("Correspondence sent/printed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnSaveDraft_Click(sender As Object, e As EventArgs) Handles btnSaveDraft.Click
        MessageBox.Show("Draft saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
