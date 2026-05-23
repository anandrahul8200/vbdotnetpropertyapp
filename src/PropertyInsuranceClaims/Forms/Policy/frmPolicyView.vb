Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Read-only policy view with tabbed display: Summary, Coverages, Claims, Billing, Notes.
''' </summary>
Public Class frmPolicyView
    Inherits Form

    Private _policyID As Integer
    Private tabControl As New TabControl()
    Private tabSummary As New TabPage("Summary")
    Private tabCoverages As New TabPage("Coverages")
    Private tabClaims As New TabPage("Claims")
    Private tabBilling As New TabPage("Billing")
    Private tabNotes As New TabPage("Notes")

    ' Summary controls
    Private lblPolicyNumber As New Label()
    Private lblStatus As New Label()
    Private lblType As New Label()
    Private lblCustomer As New Label()
    Private lblProperty As New Label()
    Private lblAgent As New Label()
    Private lblEffective As New Label()
    Private lblExpiry As New Label()
    Private lblPremium As New Label()
    Private lblTIV As New Label()

    ' Grids
    Private dgvCoverages As New DataGridView()
    Private dgvClaims As New DataGridView()
    Private dgvBilling As New DataGridView()
    Private dgvNotes As New DataGridView()

    ' Action buttons
    Private WithEvents btnEndorsement As New Button()
    Private WithEvents btnNewClaim As New Button()
    Private WithEvents btnRenew As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(policyID As Integer)
        _policyID = policyID
    End Sub

    Private Sub frmPolicyView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = "Policy View"
            Me.Size = New Drawing.Size(900, 650)
            Me.StartPosition = FormStartPosition.CenterParent

            InitializeControls()
            LoadData()
        Catch ex As Exception
            MessageBox.Show("Error loading policy: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPolicyView_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        ' Header panel
        Dim pnlHeader As New Panel() With {.Dock = DockStyle.Top, .Height = 100}
        lblPolicyNumber.Location = New Drawing.Point(10, 10) : lblPolicyNumber.AutoSize = True : lblPolicyNumber.Font = New Drawing.Font("Segoe UI", 14, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblPolicyNumber)
        lblStatus.Location = New Drawing.Point(250, 15) : lblStatus.AutoSize = True : pnlHeader.Controls.Add(lblStatus)
        lblType.Location = New Drawing.Point(350, 15) : lblType.AutoSize = True : pnlHeader.Controls.Add(lblType)
        lblCustomer.Location = New Drawing.Point(10, 40) : lblCustomer.AutoSize = True : pnlHeader.Controls.Add(lblCustomer)
        lblProperty.Location = New Drawing.Point(10, 58) : lblProperty.AutoSize = True : pnlHeader.Controls.Add(lblProperty)
        lblAgent.Location = New Drawing.Point(450, 40) : lblAgent.AutoSize = True : pnlHeader.Controls.Add(lblAgent)
        lblEffective.Location = New Drawing.Point(450, 58) : lblEffective.AutoSize = True : pnlHeader.Controls.Add(lblEffective)
        lblPremium.Location = New Drawing.Point(10, 78) : lblPremium.AutoSize = True : lblPremium.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold) : pnlHeader.Controls.Add(lblPremium)
        lblTIV.Location = New Drawing.Point(250, 78) : lblTIV.AutoSize = True : pnlHeader.Controls.Add(lblTIV)
        Me.Controls.Add(pnlHeader)

        ' Tab control
        tabControl.Dock = DockStyle.Fill
        ' Summary tab content
        Dim lblSummaryInfo As New Label() With {
            .AutoSize = False, .Dock = DockStyle.Fill,
            .Font = New Drawing.Font("Segoe UI", 10),
            .Padding = New Padding(10)
        }
        lblSummaryInfo.Name = "lblSummaryInfo"
        tabSummary.Controls.Add(lblSummaryInfo)
        ConfigureGrid(dgvCoverages) : tabCoverages.Controls.Add(dgvCoverages)
        ConfigureGrid(dgvClaims) : tabClaims.Controls.Add(dgvClaims)
        ConfigureGrid(dgvBilling) : tabBilling.Controls.Add(dgvBilling)
        ConfigureGrid(dgvNotes) : tabNotes.Controls.Add(dgvNotes)
        tabControl.TabPages.AddRange({tabSummary, tabCoverages, tabClaims, tabBilling, tabNotes})
        Me.Controls.Add(tabControl)

        ' Bottom buttons
        Dim pnlBottom As New Panel() With {.Dock = DockStyle.Bottom, .Height = 45}
        btnEndorsement.Location = New Drawing.Point(10, 8) : btnEndorsement.Size = New Drawing.Size(110, 30) : btnEndorsement.Text = "&Endorsement" : pnlBottom.Controls.Add(btnEndorsement)
        btnNewClaim.Location = New Drawing.Point(130, 8) : btnNewClaim.Size = New Drawing.Size(100, 30) : btnNewClaim.Text = "New C&laim" : pnlBottom.Controls.Add(btnNewClaim)
        btnRenew.Location = New Drawing.Point(240, 8) : btnRenew.Size = New Drawing.Size(80, 30) : btnRenew.Text = "&Renew" : pnlBottom.Controls.Add(btnRenew)
        btnCancel.Location = New Drawing.Point(750, 8) : btnCancel.Size = New Drawing.Size(80, 30) : btnCancel.Text = "&Close" : pnlBottom.Controls.Add(btnCancel)
        Me.Controls.Add(pnlBottom)

        tabControl.BringToFront()
    End Sub

    Private Sub ConfigureGrid(dgv As DataGridView)
        dgv.Dock = DockStyle.Fill : dgv.ReadOnly = True : dgv.AllowUserToAddRows = False
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect : dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    Private Sub LoadData()
        Dim ds As DataSet = PolicyDataAccess.GetDetails(_policyID)
        If ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
            MessageBox.Show("Policy not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Me.Close() : Return
        End If

        ' Header
        Dim row As DataRow = ds.Tables(0).Rows(0)
        lblPolicyNumber.Text = row("PolicyNumber").ToString()
        lblStatus.Text = $"Status: {row("PolicyStatus")}"
        lblType.Text = $"Type: {row("PolicyType")}"
        lblCustomer.Text = $"Customer: {row("FirstName")} {row("LastName")} ({row("CustomerNumber")})"
        lblProperty.Text = $"Property: {row("PropertyAddress")}, {row("PropertyCity")} {row("PropertyState")} {row("PropertyZip")}"
        lblAgent.Text = $"Agent: {row("AgentFirstName")} {row("AgentLastName")}"
        lblEffective.Text = $"Effective: {CDate(row("EffectiveDate")):MM/dd/yyyy} - {CDate(row("ExpiryDate")):MM/dd/yyyy}"

        If Not IsDBNull(row("GrossPremium")) Then lblPremium.Text = $"Premium: {CDec(row("GrossPremium")):C}"
        If Not IsDBNull(row("TotalInsuredValue")) Then lblTIV.Text = $"TIV: {CDec(row("TotalInsuredValue")):C}"

        ' Populate Summary tab
        Dim lblSummaryInfo As Label = DirectCast(tabSummary.Controls("lblSummaryInfo"), Label)
        If lblSummaryInfo IsNot Nothing Then
            Dim premium As String = If(Not IsDBNull(row("GrossPremium")), CDec(row("GrossPremium")).ToString("C"), "N/A")
            Dim annualPrem As String = If(Not IsDBNull(row("AnnualPremium")), CDec(row("AnnualPremium")).ToString("C"), "N/A")
            Dim tiv As String = If(Not IsDBNull(row("TotalInsuredValue")), CDec(row("TotalInsuredValue")).ToString("C"), "N/A")
            Dim priorCarrier As String = If(Not IsDBNull(row("PriorCarrier")), row("PriorCarrier").ToString(), "None")
            Dim claimFree As String = If(Not IsDBNull(row("ClaimFreeYears")), row("ClaimFreeYears").ToString(), "0")
            Dim payPlan As String = If(Not IsDBNull(row("PaymentPlan")), row("PaymentPlan").ToString(), "N/A")

            lblSummaryInfo.Text = $"POLICY SUMMARY" & vbCrLf & vbCrLf &
                $"Policy Number:     {row("PolicyNumber")}" & vbCrLf &
                $"Status:            {row("PolicyStatus")}" & vbCrLf &
                $"Type:              {row("PolicyType")}" & vbCrLf & vbCrLf &
                $"Customer:          {row("FirstName")} {row("LastName")}" & vbCrLf &
                $"Property:          {row("PropertyAddress")}, {row("PropertyCity")} {row("PropertyState")}" & vbCrLf &
                $"Agent:             {row("AgentFirstName")} {row("AgentLastName")}" & vbCrLf & vbCrLf &
                $"Effective:         {CDate(row("EffectiveDate")):MM/dd/yyyy}" & vbCrLf &
                $"Expiry:            {CDate(row("ExpiryDate")):MM/dd/yyyy}" & vbCrLf &
                $"Term:              {row("TermMonths")} months" & vbCrLf & vbCrLf &
                $"Annual Premium:    {annualPrem}" & vbCrLf &
                $"Gross Premium:     {premium}" & vbCrLf &
                $"Total Insured:     {tiv}" & vbCrLf & vbCrLf &
                $"Payment Plan:      {payPlan}" & vbCrLf &
                $"Prior Carrier:     {priorCarrier}" & vbCrLf &
                $"Claim-Free Years:  {claimFree}"
        End If

        ' Coverages (table index 1)
        If ds.Tables.Count > 1 Then dgvCoverages.DataSource = ds.Tables(1)
        ' Claims (table index 3)
        If ds.Tables.Count > 3 Then dgvClaims.DataSource = ds.Tables(3)
        ' Billing (table index 4)
        If ds.Tables.Count > 4 Then dgvBilling.DataSource = ds.Tables(4)
        ' Notes (table index 5)
        If ds.Tables.Count > 5 Then dgvNotes.DataSource = ds.Tables(5)
    End Sub

    Private Sub btnEndorsement_Click(sender As Object, e As EventArgs) Handles btnEndorsement.Click
        MessageBox.Show("Endorsement form would open here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnNewClaim_Click(sender As Object, e As EventArgs) Handles btnNewClaim.Click
        Dim frm As New frmClaimFNOL(_policyID)
        frm.ShowDialog(Me)
    End Sub

    Private Sub btnRenew_Click(sender As Object, e As EventArgs) Handles btnRenew.Click
        MessageBox.Show("Renewal processing would start here.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub
End Class
