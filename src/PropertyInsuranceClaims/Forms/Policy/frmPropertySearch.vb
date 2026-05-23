Imports System.Windows.Forms
Imports System.Data
Imports System.Data.SqlClient
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Property search/picker form — lists properties for a customer and allows selection.
''' When opened as a dialog, double-click or Open sets SelectedPropertyID and closes.
''' </summary>
Public Class frmPropertySearch
    Inherits Form

    ' --- Controls ---
    Private WithEvents dgvResults As New DataGridView()
    Private WithEvents btnOpen As New Button()
    Private WithEvents btnNew As New Button()
    Private WithEvents btnCancel As New Button()
    Private lblInfo As New Label()

    ' --- State ---
    Private _customerID As Integer = 0
    Private _selectedPropertyID As Integer = 0
    Private _selectedPropertyAddress As String = ""

    ''' <summary>
    ''' Gets the selected property ID (for caller forms).
    ''' </summary>
    Public ReadOnly Property SelectedPropertyID As Integer
        Get
            Return _selectedPropertyID
        End Get
    End Property

    ''' <summary>
    ''' Gets the selected property address display string.
    ''' </summary>
    Public ReadOnly Property SelectedPropertyAddress As String
        Get
            Return _selectedPropertyAddress
        End Get
    End Property

    Public Sub New(customerID As Integer)
        _customerID = customerID
    End Sub

    Private Sub frmPropertySearch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Select Property"
            Me.Size = New Drawing.Size(750, 400)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False

            InitializeControls()
            LoadProperties()
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPropertySearch_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Info label
        lblInfo.Location = New Drawing.Point(10, 10)
        lblInfo.AutoSize = True
        lblInfo.Text = "Select a property or create a new one:"
        Me.Controls.Add(lblInfo)

        ' Grid
        dgvResults.Location = New Drawing.Point(10, 35)
        dgvResults.Size = New Drawing.Size(710, 260)
        dgvResults.ReadOnly = True
        dgvResults.AllowUserToAddRows = False
        dgvResults.AllowUserToDeleteRows = False
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResults.MultiSelect = False
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        Me.Controls.Add(dgvResults)

        ' Buttons
        btnOpen.Location = New Drawing.Point(10, 305)
        btnOpen.Size = New Drawing.Size(100, 30)
        btnOpen.Text = "&Select"
        btnOpen.Enabled = False
        Me.Controls.Add(btnOpen)

        btnNew.Location = New Drawing.Point(120, 305)
        btnNew.Size = New Drawing.Size(120, 30)
        btnNew.Text = "&New Property"
        Me.Controls.Add(btnNew)

        btnCancel.Location = New Drawing.Point(620, 305)
        btnCancel.Size = New Drawing.Size(100, 30)
        btnCancel.Text = "&Cancel"
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub LoadProperties()
        Try
            Dim dt As DataTable = PropertyDataAccess.GetByCustomer(_customerID)
            dgvResults.DataSource = dt

            ' Hide ID column if present
            If dgvResults.Columns.Contains("PropertyID") Then dgvResults.Columns("PropertyID").Visible = False

            btnOpen.Enabled = (dt IsNot Nothing AndAlso dt.Rows.Count > 0)
        Catch ex As Exception
            MessageBox.Show("Error loading properties: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPropertySearch.LoadProperties")
        End Try
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        SelectProperty()
    End Sub

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        If e.RowIndex >= 0 Then SelectProperty()
    End Sub

    Private Sub SelectProperty()
        If dgvResults.CurrentRow Is Nothing Then Return
        _selectedPropertyID = CInt(dgvResults.CurrentRow.Cells("PropertyID").Value)

        ' Build address display string from available columns
        Dim addr As String = ""
        If dgvResults.Columns.Contains("AddressLine1") Then addr = dgvResults.CurrentRow.Cells("AddressLine1").Value.ToString()
        If dgvResults.Columns.Contains("City") Then addr &= ", " & dgvResults.CurrentRow.Cells("City").Value.ToString()
        If dgvResults.Columns.Contains("StateCode") Then addr &= " " & dgvResults.CurrentRow.Cells("StateCode").Value.ToString()
        _selectedPropertyAddress = addr

        Me.Close()
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        Dim frm As New frmPropertyEntry(_customerID)
        frm.ShowDialog(Me)
        ' Refresh list after adding new property
        LoadProperties()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        _selectedPropertyID = 0
        Me.Close()
    End Sub
End Class
