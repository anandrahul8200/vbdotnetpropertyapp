Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Generic report viewer with parameter selection and grid/export display.
''' </summary>
Public Class frmReportViewer
    Inherits Form

    Private cboReport As New ComboBox()
    Private pnlParameters As New Panel()
    Private WithEvents btnRun As New Button()
    Private WithEvents btnExport As New Button()
    Private WithEvents btnPrint As New Button()
    Private dgvReport As New DataGridView()
    Private lblRecordCount As New Label()

    ' Dynamic parameter controls
    Private dtpDateFrom As New DateTimePicker()
    Private dtpDateTo As New DateTimePicker()
    Private cboState As New ComboBox()
    Private cboPolicyType As New ComboBox()
    Private cboClaimStatus As New ComboBox()

    Private Sub frmReportViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Report Viewer"
        Me.Size = New Drawing.Size(1100, 700)
        Me.StartPosition = FormStartPosition.CenterParent
        InitializeControls()
        LoadReportList()
    End Sub

    Private Sub InitializeControls()
        ' Report selection
        Dim pnlTop As New Panel() With {.Dock = DockStyle.Top, .Height = 45}
        pnlTop.Controls.Add(New Label() With {.Text = "Report:", .Location = New Drawing.Point(10, 14), .AutoSize = True})
        cboReport.Location = New Drawing.Point(65, 11) : cboReport.Size = New Drawing.Size(300, 20) : cboReport.DropDownStyle = ComboBoxStyle.DropDownList
        AddHandler cboReport.SelectedIndexChanged, AddressOf ReportChanged
        pnlTop.Controls.Add(cboReport)
        btnRun.Location = New Drawing.Point(700, 8) : btnRun.Size = New Drawing.Size(80, 30) : btnRun.Text = "&Run" : pnlTop.Controls.Add(btnRun)
        btnExport.Location = New Drawing.Point(790, 8) : btnExport.Size = New Drawing.Size(80, 30) : btnExport.Text = "&Export" : btnExport.Enabled = False : pnlTop.Controls.Add(btnExport)
        btnPrint.Location = New Drawing.Point(880, 8) : btnPrint.Size = New Drawing.Size(80, 30) : btnPrint.Text = "&Print" : btnPrint.Enabled = False : pnlTop.Controls.Add(btnPrint)
        Me.Controls.Add(pnlTop)

        ' Parameters panel
        pnlParameters.Dock = DockStyle.Top : pnlParameters.Height = 50
        pnlParameters.Controls.Add(New Label() With {.Text = "From:", .Location = New Drawing.Point(10, 15), .AutoSize = True})
        dtpDateFrom.Location = New Drawing.Point(50, 12) : dtpDateFrom.Size = New Drawing.Size(110, 20) : dtpDateFrom.Format = DateTimePickerFormat.Short
        dtpDateFrom.Value = New DateTime(DateTime.Now.Year, 1, 1) : pnlParameters.Controls.Add(dtpDateFrom)
        pnlParameters.Controls.Add(New Label() With {.Text = "To:", .Location = New Drawing.Point(170, 15), .AutoSize = True})
        dtpDateTo.Location = New Drawing.Point(195, 12) : dtpDateTo.Size = New Drawing.Size(110, 20) : dtpDateTo.Format = DateTimePickerFormat.Short : pnlParameters.Controls.Add(dtpDateTo)
        pnlParameters.Controls.Add(New Label() With {.Text = "State:", .Location = New Drawing.Point(320, 15), .AutoSize = True})
        cboState.Location = New Drawing.Point(365, 12) : cboState.Size = New Drawing.Size(60, 20) : cboState.DropDownStyle = ComboBoxStyle.DropDownList : cboState.Items.Add("(All)") : cboState.SelectedIndex = 0 : pnlParameters.Controls.Add(cboState)
        pnlParameters.Controls.Add(New Label() With {.Text = "Type:", .Location = New Drawing.Point(440, 15), .AutoSize = True})
        cboPolicyType.Location = New Drawing.Point(480, 12) : cboPolicyType.Size = New Drawing.Size(80, 20) : cboPolicyType.DropDownStyle = ComboBoxStyle.DropDownList : cboPolicyType.Items.AddRange({"(All)", "HO3", "HO4", "HO6", "DP3"}) : cboPolicyType.SelectedIndex = 0 : pnlParameters.Controls.Add(cboPolicyType)
        Me.Controls.Add(pnlParameters)

        ' Grid
        dgvReport.Dock = DockStyle.Fill : dgvReport.ReadOnly = True : dgvReport.AllowUserToAddRows = False
        dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvReport.AlternatingRowsDefaultCellStyle.BackColor = Drawing.Color.AliceBlue
        Me.Controls.Add(dgvReport)

        ' Bottom
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 30}
        lblRecordCount.Location = New Drawing.Point(10, 8) : lblRecordCount.AutoSize = True : pnlBottom.Controls.Add(lblRecordCount)
        Me.Controls.Add(pnlBottom)
        dgvReport.BringToFront()
    End Sub

    Private Sub LoadReportList()
        cboReport.Items.Clear()
        cboReport.Items.Add(New ReportDefinition("Loss Run Report", "Reporting.usp_Report_LossRun"))
        cboReport.Items.Add(New ReportDefinition("Production Report", "Reporting.usp_Report_Production"))
        cboReport.Items.Add(New ReportDefinition("Claims Aging Report", "Reporting.usp_Report_ClaimsAging"))
        cboReport.Items.Add(New ReportDefinition("Financial Summary", "Reporting.usp_Report_FinancialSummary"))
        cboReport.Items.Add(New ReportDefinition("Open Claims by Adjuster", "Reporting.usp_Report_OpenClaimsByAdjuster"))
        cboReport.Items.Add(New ReportDefinition("Premium by State", "Reporting.usp_Report_PremiumByState"))
        cboReport.Items.Add(New ReportDefinition("Catastrophe Summary", "Reporting.usp_Report_CatastropheSummary"))
        cboReport.Items.Add(New ReportDefinition("Commission Summary", "Reporting.usp_Report_CommissionSummary"))
        cboReport.DisplayMember = "DisplayName"
        If cboReport.Items.Count > 0 Then cboReport.SelectedIndex = 0
    End Sub

    Private Sub ReportChanged(sender As Object, e As EventArgs)
        ' Could show/hide parameters based on report type
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        Try
            If cboReport.SelectedItem Is Nothing Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim report As ReportDefinition = DirectCast(cboReport.SelectedItem, ReportDefinition)
            Dim params As New List(Of SqlParameter) From {
                DatabaseHelper.CreateParam("@DateFrom", dtpDateFrom.Value.Date),
                DatabaseHelper.CreateParam("@DateTo", dtpDateTo.Value.Date),
                DatabaseHelper.CreateParam("@StateCode", If(cboState.SelectedIndex > 0, cboState.SelectedItem.ToString(), Nothing)),
                DatabaseHelper.CreateParam("@PolicyType", If(cboPolicyType.SelectedIndex > 0, cboPolicyType.SelectedItem.ToString(), Nothing))
            }

            Dim dt As DataTable = DatabaseHelper.ExecuteStoredProcedure(report.StoredProcedure, params.ToArray())
            dgvReport.DataSource = dt
            lblRecordCount.Text = $"{dt.Rows.Count} row(s)"
            btnExport.Enabled = dt.Rows.Count > 0
            btnPrint.Enabled = dt.Rows.Count > 0
        Catch ex As Exception
            MessageBox.Show("Error running report: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmReportViewer.btnRun_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Try
            Dim sfd As New SaveFileDialog() With {.Filter = "CSV Files|*.csv", .Title = "Export Report"}
            If sfd.ShowDialog() = DialogResult.OK Then
                ExportToCsv(dgvReport, sfd.FileName)
                MessageBox.Show("Report exported.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Export error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        MessageBox.Show("Print preview would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ExportToCsv(dgv As DataGridView, filePath As String)
        Dim sb As New Text.StringBuilder()

        ' Headers
        For i As Integer = 0 To dgv.Columns.Count - 1
            If i > 0 Then sb.Append(",")
            sb.Append(""""c & dgv.Columns(i).HeaderText & """"c)
        Next
        sb.AppendLine()

        ' Rows
        For Each row As DataGridViewRow In dgv.Rows
            For i As Integer = 0 To dgv.Columns.Count - 1
                If i > 0 Then sb.Append(",")
                Dim value As String = If(row.Cells(i).Value IsNot Nothing, row.Cells(i).Value.ToString(), "")
                sb.Append(""""c & value.Replace("""", """""") & """"c)
            Next
            sb.AppendLine()
        Next

        IO.File.WriteAllText(filePath, sb.ToString())
    End Sub
End Class

''' <summary>
''' Simple report definition for the combo box.
''' </summary>
Public Class ReportDefinition
    Public Property DisplayName As String
    Public Property StoredProcedure As String

    Public Sub New(name As String, sp As String)
        DisplayName = name
        StoredProcedure = sp
    End Sub

    Public Overrides Function ToString() As String
        Return DisplayName
    End Function
End Class
