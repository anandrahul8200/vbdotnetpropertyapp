Imports System.Windows.Forms
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Letter/notice template manager — create and edit correspondence templates.
''' </summary>
Public Class frmTemplateManager
    Inherits Form

    Private dgvTemplates As New DataGridView()
    Private txtTemplateName As New TextBox()
    Private cboCategory As New ComboBox()
    Private txtSubjectTemplate As New TextBox()
    Private txtBodyTemplate As New TextBox()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnDelete As New Button()
    Private WithEvents btnClose As New Button()

    Private Sub frmTemplateManager_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Template Manager"
        Me.Size = New Drawing.Size(800, 550)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        ' Template list
        dgvTemplates.Location = New Drawing.Point(10, 10) : dgvTemplates.Size = New Drawing.Size(250, 440)
        dgvTemplates.ReadOnly = True : dgvTemplates.AllowUserToAddRows = False : dgvTemplates.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvTemplates.Columns.Add("Name", "Template Name")
        dgvTemplates.Columns.Add("Category", "Category")
        dgvTemplates.Rows.Add("Cancellation Notice", "POLICY")
        dgvTemplates.Rows.Add("Renewal Reminder", "POLICY")
        dgvTemplates.Rows.Add("Claim Acknowledgment", "CLAIMS")
        dgvTemplates.Rows.Add("Payment Reminder", "BILLING")
        dgvTemplates.Rows.Add("Claim Denial", "CLAIMS")
        dgvTemplates.Rows.Add("Welcome Letter", "POLICY")
        dgvTemplates.Rows.Add("Non-Renewal Notice", "POLICY")
        Me.Controls.Add(dgvTemplates)

        ' Edit panel
        Dim grpEdit As New GroupBox() With {.Text = "Template Editor", .Location = New Drawing.Point(270, 10), .Size = New Drawing.Size(500, 440)}
        Dim y As Integer = 22
        grpEdit.Controls.Add(New Label() With {.Text = "Name:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtTemplateName.Location = New Drawing.Point(100, y - 3) : txtTemplateName.Size = New Drawing.Size(300, 20) : grpEdit.Controls.Add(txtTemplateName)
        y += 30
        grpEdit.Controls.Add(New Label() With {.Text = "Category:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        cboCategory.Location = New Drawing.Point(100, y - 3) : cboCategory.Size = New Drawing.Size(150, 20) : cboCategory.DropDownStyle = ComboBoxStyle.DropDownList
        cboCategory.Items.AddRange({"POLICY", "CLAIMS", "BILLING", "UNDERWRITING", "GENERAL"}) : cboCategory.SelectedIndex = 0 : grpEdit.Controls.Add(cboCategory)
        y += 30
        grpEdit.Controls.Add(New Label() With {.Text = "Subject:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtSubjectTemplate.Location = New Drawing.Point(100, y - 3) : txtSubjectTemplate.Size = New Drawing.Size(380, 20) : grpEdit.Controls.Add(txtSubjectTemplate)
        y += 30
        grpEdit.Controls.Add(New Label() With {.Text = "Body:", .Location = New Drawing.Point(15, y), .AutoSize = True})
        txtBodyTemplate.Location = New Drawing.Point(100, y - 3) : txtBodyTemplate.Size = New Drawing.Size(380, 250) : txtBodyTemplate.Multiline = True : txtBodyTemplate.ScrollBars = ScrollBars.Vertical : grpEdit.Controls.Add(txtBodyTemplate)
        y += 260
        grpEdit.Controls.Add(New Label() With {.Text = "Variables: [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount]", .Location = New Drawing.Point(15, y), .AutoSize = True, .ForeColor = Drawing.Color.Gray})
        Me.Controls.Add(grpEdit)

        ' Buttons
        btnNew.Location = New Drawing.Point(10, 460) : btnNew.Size = New Drawing.Size(80, 30) : btnNew.Text = "&New" : Me.Controls.Add(btnNew)
        btnSave.Location = New Drawing.Point(270, 460) : btnSave.Size = New Drawing.Size(80, 30) : btnSave.Text = "&Save" : Me.Controls.Add(btnSave)
        btnDelete.Location = New Drawing.Point(100, 460) : btnDelete.Size = New Drawing.Size(80, 30) : btnDelete.Text = "&Delete" : Me.Controls.Add(btnDelete)
        btnClose.Location = New Drawing.Point(690, 460) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : Me.Controls.Add(btnClose)
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        txtTemplateName.Clear() : txtSubjectTemplate.Clear() : txtBodyTemplate.Clear() : txtTemplateName.Focus()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If String.IsNullOrWhiteSpace(txtTemplateName.Text) Then
            MessageBox.Show("Template name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
        End If
        MessageBox.Show("Template saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If MessageBox.Show("Delete this template?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
            MessageBox.Show("Template deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class
