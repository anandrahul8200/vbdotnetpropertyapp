Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Agent search and management form.
''' </summary>
Public Class frmAgentSearch
    Inherits Form

    Private txtAgentName As New TextBox()
    Private cboAgentType As New ComboBox()
    Private dgvAgents As New DataGridView()
    Private WithEvents btnSearch As New Button()
    Private WithEvents btnClose As New Button()
    Private lblCount As New Label()

    Private Sub frmAgentSearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Agent Search"
        Me.Size = New Drawing.Size(800, 450)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
    End Sub

    Private Sub InitializeControls()
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Name:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        txtAgentName.Location = New Drawing.Point(55, 11) : txtAgentName.Size = New Drawing.Size(150, 20) : pnlTop.Controls.Add(txtAgentName)
        pnlTop.Controls.Add(New Label() With {.Text = "Type:", .Location = New Drawing.Point(220, 14), .AutoSize = True})
        cboAgentType.Location = New Drawing.Point(260, 11) : cboAgentType.Size = New Drawing.Size(120, 20) : cboAgentType.DropDownStyle = ComboBoxStyle.DropDownList
        cboAgentType.Items.AddRange({"(All)", "CAPTIVE", "INDEPENDENT", "BROKER"}) : cboAgentType.SelectedIndex = 0 : pnlTop.Controls.Add(cboAgentType)
        btnSearch.Location = New Drawing.Point(400, 8) : btnSearch.Size = New Drawing.Size(80, 28) : btnSearch.Text = "&Search" : pnlTop.Controls.Add(btnSearch)
        lblCount.Location = New Drawing.Point(500, 14) : lblCount.AutoSize = True : pnlTop.Controls.Add(lblCount)
        Me.Controls.Add(pnlTop)

        dgvAgents.Dock = DockStyle.Fill : dgvAgents.ReadOnly = True : dgvAgents.AllowUserToAddRows = False
        dgvAgents.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgvAgents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvAgents)

        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 40}
        btnClose.Location = New Drawing.Point(680, 5) : btnClose.Size = New Drawing.Size(80, 30) : btnClose.Text = "&Close" : pnlBottom.Controls.Add(btnClose)
        Me.Controls.Add(pnlBottom)
        dgvAgents.BringToFront()
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Try
            Me.Cursor = Cursors.WaitCursor
            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure("Policy.usp_Agent_Search", Nothing)
            dgvAgents.DataSource = dt
            lblCount.Text = dt.Rows.Count.ToString() & " agent(s)"
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
