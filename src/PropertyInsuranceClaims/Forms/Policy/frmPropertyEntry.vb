Imports System.Windows.Forms
Imports System.Data
Imports PropertyInsuranceClaims.Common

''' <summary>
''' Property create/edit form with construction details, protective devices, and valuation.
''' </summary>
Public Class frmPropertyEntry
    Inherits Form

    Private _propertyID As Integer = 0
    Private _customerID As Integer = 0
    Private _isEditMode As Boolean = False
    Private _isDirty As Boolean = False

    ' --- Controls ---
    Private lblPropertyNumber As New Label()
    Private cboPropertyType As New ComboBox()
    Private cboConstructionType As New ComboBox()
    Private cboOccupancyType As New ComboBox()
    Private txtYearBuilt As New TextBox()
    Private txtSquareFootage As New TextBox()
    Private txtNumberOfStories As New TextBox()
    Private cboRoofType As New ComboBox()
    Private txtRoofAge As New TextBox()
    Private chkHasBasement As New CheckBox()
    Private chkHasPool As New CheckBox()
    Private chkHasFireAlarm As New CheckBox()
    Private chkHasBurglarAlarm As New CheckBox()
    Private chkHasSprinklerSystem As New CheckBox()
    Private txtDistanceToFireStation As New TextBox()
    Private txtDistanceToHydrant As New TextBox()
    Private txtFireProtectionClass As New TextBox()
    Private cboFloodZone As New ComboBox()
    Private txtMarketValue As New TextBox()
    Private txtReplacementCost As New TextBox()
    Private txtAddressLine1 As New TextBox()
    Private txtAddressLine2 As New TextBox()
    Private txtCity As New TextBox()
    Private cboState As New ComboBox()
    Private txtZipCode As New TextBox()
    Private txtCounty As New TextBox()
    Private WithEvents btnSave As New Button()
    Private WithEvents btnCancel As New Button()

    Public Sub New(customerID As Integer, Optional propertyID As Integer = 0)
        _customerID = customerID
        _propertyID = propertyID
    End Sub

    Private Sub frmPropertyEntry_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Me.Cursor = Cursors.WaitCursor
            Me.Text = If(_propertyID > 0, "Edit Property", "New Property")
            Me.Size = New Drawing.Size(650, 750)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False

            InitializeControls()
            LoadLookups()

            If _propertyID > 0 Then
                LoadData()
                _isEditMode = True
            End If
            _isDirty = False
        Catch ex As Exception
            MessageBox.Show("Error loading form: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "frmPropertyEntry_Load")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Sub InitializeControls()
        Dim y As Integer = 15
        Dim ctrlLeft As Integer = 160
        Dim ctrlWidth As Integer = 180

        ' Property Number
        AddLabel("Property #:", 10, y)
        lblPropertyNumber.Location = New Drawing.Point(ctrlLeft, y)
        lblPropertyNumber.AutoSize = True
        lblPropertyNumber.Font = New Drawing.Font(Me.Font, Drawing.FontStyle.Bold)
        Me.Controls.Add(lblPropertyNumber)
        y += 30

        ' Address section
        Dim grpAddress As New GroupBox() With {.Text = "Location", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(610, 130)}
        Dim ay As Integer = 22
        AddLabelTo(grpAddress, "Address:", 10, ay)
        txtAddressLine1.Location = New Drawing.Point(100, ay - 3) : txtAddressLine1.Size = New Drawing.Size(380, 20) : grpAddress.Controls.Add(txtAddressLine1)
        ay += 28
        AddLabelTo(grpAddress, "Address 2:", 10, ay)
        txtAddressLine2.Location = New Drawing.Point(100, ay - 3) : txtAddressLine2.Size = New Drawing.Size(380, 20) : grpAddress.Controls.Add(txtAddressLine2)
        ay += 28
        AddLabelTo(grpAddress, "City:", 10, ay)
        txtCity.Location = New Drawing.Point(100, ay - 3) : txtCity.Size = New Drawing.Size(160, 20) : grpAddress.Controls.Add(txtCity)
        AddLabelTo(grpAddress, "State:", 280, ay)
        cboState.Location = New Drawing.Point(325, ay - 3) : cboState.Size = New Drawing.Size(60, 20) : cboState.DropDownStyle = ComboBoxStyle.DropDownList : grpAddress.Controls.Add(cboState)
        AddLabelTo(grpAddress, "Zip:", 400, ay)
        txtZipCode.Location = New Drawing.Point(430, ay - 3) : txtZipCode.Size = New Drawing.Size(80, 20) : grpAddress.Controls.Add(txtZipCode)
        ay += 28
        AddLabelTo(grpAddress, "County:", 10, ay)
        txtCounty.Location = New Drawing.Point(100, ay - 3) : txtCounty.Size = New Drawing.Size(160, 20) : grpAddress.Controls.Add(txtCounty)
        Me.Controls.Add(grpAddress)
        y += 140

        ' Construction section
        Dim grpConstruction As New GroupBox() With {.Text = "Construction Details", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(610, 180)}
        Dim cy As Integer = 22
        AddLabelTo(grpConstruction, "Property Type:", 10, cy)
        cboPropertyType.Location = New Drawing.Point(130, cy - 3) : cboPropertyType.Size = New Drawing.Size(150, 20) : cboPropertyType.DropDownStyle = ComboBoxStyle.DropDownList : grpConstruction.Controls.Add(cboPropertyType)
        AddLabelTo(grpConstruction, "Occupancy:", 310, cy)
        cboOccupancyType.Location = New Drawing.Point(390, cy - 3) : cboOccupancyType.Size = New Drawing.Size(150, 20) : cboOccupancyType.DropDownStyle = ComboBoxStyle.DropDownList : grpConstruction.Controls.Add(cboOccupancyType)
        cy += 28
        AddLabelTo(grpConstruction, "Construction:", 10, cy)
        cboConstructionType.Location = New Drawing.Point(130, cy - 3) : cboConstructionType.Size = New Drawing.Size(150, 20) : cboConstructionType.DropDownStyle = ComboBoxStyle.DropDownList : grpConstruction.Controls.Add(cboConstructionType)
        AddLabelTo(grpConstruction, "Roof Type:", 310, cy)
        cboRoofType.Location = New Drawing.Point(390, cy - 3) : cboRoofType.Size = New Drawing.Size(150, 20) : cboRoofType.DropDownStyle = ComboBoxStyle.DropDownList : grpConstruction.Controls.Add(cboRoofType)
        cy += 28
        AddLabelTo(grpConstruction, "Year Built:", 10, cy)
        txtYearBuilt.Location = New Drawing.Point(130, cy - 3) : txtYearBuilt.Size = New Drawing.Size(60, 20) : grpConstruction.Controls.Add(txtYearBuilt)
        AddLabelTo(grpConstruction, "Roof Age:", 310, cy)
        txtRoofAge.Location = New Drawing.Point(390, cy - 3) : txtRoofAge.Size = New Drawing.Size(60, 20) : grpConstruction.Controls.Add(txtRoofAge)
        cy += 28
        AddLabelTo(grpConstruction, "Sq Footage:", 10, cy)
        txtSquareFootage.Location = New Drawing.Point(130, cy - 3) : txtSquareFootage.Size = New Drawing.Size(80, 20) : grpConstruction.Controls.Add(txtSquareFootage)
        AddLabelTo(grpConstruction, "Stories:", 310, cy)
        txtNumberOfStories.Location = New Drawing.Point(390, cy - 3) : txtNumberOfStories.Size = New Drawing.Size(40, 20) : grpConstruction.Controls.Add(txtNumberOfStories)
        cy += 28
        AddLabelTo(grpConstruction, "Flood Zone:", 10, cy)
        cboFloodZone.Location = New Drawing.Point(130, cy - 3) : cboFloodZone.Size = New Drawing.Size(80, 20) : cboFloodZone.DropDownStyle = ComboBoxStyle.DropDownList : grpConstruction.Controls.Add(cboFloodZone)
        Me.Controls.Add(grpConstruction)
        y += 190

        ' Protection section
        Dim grpProtection As New GroupBox() With {.Text = "Protection & Features", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(610, 120)}
        Dim py As Integer = 22
        chkHasFireAlarm.Location = New Drawing.Point(15, py) : chkHasFireAlarm.Text = "Fire Alarm" : chkHasFireAlarm.AutoSize = True : grpProtection.Controls.Add(chkHasFireAlarm)
        chkHasBurglarAlarm.Location = New Drawing.Point(150, py) : chkHasBurglarAlarm.Text = "Burglar Alarm" : chkHasBurglarAlarm.AutoSize = True : grpProtection.Controls.Add(chkHasBurglarAlarm)
        chkHasSprinklerSystem.Location = New Drawing.Point(300, py) : chkHasSprinklerSystem.Text = "Sprinkler System" : chkHasSprinklerSystem.AutoSize = True : grpProtection.Controls.Add(chkHasSprinklerSystem)
        py += 28
        chkHasBasement.Location = New Drawing.Point(15, py) : chkHasBasement.Text = "Basement" : chkHasBasement.AutoSize = True : grpProtection.Controls.Add(chkHasBasement)
        chkHasPool.Location = New Drawing.Point(150, py) : chkHasPool.Text = "Pool" : chkHasPool.AutoSize = True : grpProtection.Controls.Add(chkHasPool)
        py += 28
        AddLabelTo(grpProtection, "Dist. Fire Station (mi):", 10, py)
        txtDistanceToFireStation.Location = New Drawing.Point(170, py - 3) : txtDistanceToFireStation.Size = New Drawing.Size(60, 20) : grpProtection.Controls.Add(txtDistanceToFireStation)
        AddLabelTo(grpProtection, "Dist. Hydrant (mi):", 260, py)
        txtDistanceToHydrant.Location = New Drawing.Point(400, py - 3) : txtDistanceToHydrant.Size = New Drawing.Size(60, 20) : grpProtection.Controls.Add(txtDistanceToHydrant)
        AddLabelTo(grpProtection, "Prot. Class:", 480, py)
        txtFireProtectionClass.Location = New Drawing.Point(555, py - 3) : txtFireProtectionClass.Size = New Drawing.Size(35, 20) : grpProtection.Controls.Add(txtFireProtectionClass)
        Me.Controls.Add(grpProtection)
        y += 130

        ' Valuation
        Dim grpValue As New GroupBox() With {.Text = "Valuation", .Location = New Drawing.Point(10, y), .Size = New Drawing.Size(610, 55)}
        AddLabelTo(grpValue, "Market Value:", 10, 22)
        txtMarketValue.Location = New Drawing.Point(110, 19) : txtMarketValue.Size = New Drawing.Size(120, 20) : grpValue.Controls.Add(txtMarketValue)
        AddLabelTo(grpValue, "Replacement Cost:", 260, 22)
        txtReplacementCost.Location = New Drawing.Point(390, 19) : txtReplacementCost.Size = New Drawing.Size(120, 20) : grpValue.Controls.Add(txtReplacementCost)
        Me.Controls.Add(grpValue)
        y += 65

        ' Buttons
        btnSave.Location = New Drawing.Point(380, y)
        btnSave.Size = New Drawing.Size(110, 35)
        btnSave.Text = "&Save"
        Me.Controls.Add(btnSave)

        btnCancel.Location = New Drawing.Point(500, y)
        btnCancel.Size = New Drawing.Size(110, 35)
        btnCancel.Text = "&Cancel"
        Me.Controls.Add(btnCancel)
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True}
        Me.Controls.Add(lbl)
    End Sub

    Private Sub AddLabelTo(parent As Control, text As String, x As Integer, y As Integer)
        Dim lbl As New Label() With {.Text = text, .Location = New Drawing.Point(x, y), .AutoSize = True}
        parent.Controls.Add(lbl)
    End Sub

    Private Sub LoadLookups()
        cboPropertyType.Items.AddRange({"SINGLE_FAMILY", "MULTI_FAMILY", "CONDO", "TOWNHOUSE", "MOBILE_HOME", "COMMERCIAL"})
        cboConstructionType.Items.AddRange({"FRAME", "MASONRY", "MASONRY_VENEER", "FIRE_RESISTIVE", "SUPERIOR"})
        cboOccupancyType.Items.AddRange({"OWNER_OCCUPIED", "TENANT_OCCUPIED", "VACANT", "SEASONAL", "COMMERCIAL"})
        cboRoofType.Items.AddRange({"ASPHALT_SHINGLE", "WOOD_SHAKE", "TILE", "METAL", "SLATE", "FLAT"})
        cboFloodZone.Items.AddRange({"", "A", "AE", "AH", "AO", "B", "C", "D", "V", "VE", "X"})
        cboState.Items.Add("") ' Would load from DB
        cboPropertyType.SelectedIndex = 0
        cboConstructionType.SelectedIndex = 0
        cboOccupancyType.SelectedIndex = 0
        cboRoofType.SelectedIndex = 0
        cboFloodZone.SelectedIndex = 0
    End Sub

    Private Sub LoadData()
        ' Would call PropertyDataAccess.GetByID
        ' Populate controls from DataTable
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            If Not ValidateForm() Then Return
            Me.Cursor = Cursors.WaitCursor

            Dim dto As New PropertyDTO() With {
                .PropertyID = _propertyID,
                .CustomerID = _customerID,
                .PropertyType = cboPropertyType.SelectedItem.ToString(),
                .ConstructionType = cboConstructionType.SelectedItem.ToString(),
                .OccupancyType = cboOccupancyType.SelectedItem.ToString(),
                .YearBuilt = CInt(txtYearBuilt.Text),
                .SquareFootage = CInt(txtSquareFootage.Text),
                .NumberOfStories = If(String.IsNullOrEmpty(txtNumberOfStories.Text), 1, CInt(txtNumberOfStories.Text)),
                .RoofType = If(cboRoofType.SelectedIndex >= 0, cboRoofType.SelectedItem.ToString(), Nothing),
                .HasBasement = chkHasBasement.Checked,
                .HasPool = chkHasPool.Checked,
                .HasFireAlarm = chkHasFireAlarm.Checked,
                .HasBurglarAlarm = chkHasBurglarAlarm.Checked,
                .HasSprinklerSystem = chkHasSprinklerSystem.Checked,
                .AddressLine1 = txtAddressLine1.Text.Trim(),
                .AddressLine2 = txtAddressLine2.Text.Trim(),
                .City = txtCity.Text.Trim(),
                .StateCode = If(cboState.SelectedItem IsNot Nothing, cboState.SelectedItem.ToString(), ""),
                .ZipCode = txtZipCode.Text.Trim(),
                .County = txtCounty.Text.Trim()
            }

            If Not String.IsNullOrEmpty(txtRoofAge.Text) Then dto.RoofAge = CInt(txtRoofAge.Text)
            If Not String.IsNullOrEmpty(txtDistanceToFireStation.Text) Then dto.DistanceToFireStation = CDec(txtDistanceToFireStation.Text)
            If Not String.IsNullOrEmpty(txtDistanceToHydrant.Text) Then dto.DistanceToHydrant = CDec(txtDistanceToHydrant.Text)
            If Not String.IsNullOrEmpty(txtFireProtectionClass.Text) Then dto.FireProtectionClass = CInt(txtFireProtectionClass.Text)
            If Not String.IsNullOrEmpty(txtMarketValue.Text) Then dto.MarketValue = CDec(txtMarketValue.Text)
            If Not String.IsNullOrEmpty(txtReplacementCost.Text) Then dto.ReplacementCost = CDec(txtReplacementCost.Text)

            If _propertyID = 0 Then
                _propertyID = PropertyDataAccess.Create(dto)
            End If

            _isDirty = False
            MessageBox.Show("Property saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Close()
        Catch ex As Exception
            MessageBox.Show("Error saving: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ErrorLogger.LogError(ex, "btnSave_Click")
        Finally
            Me.Cursor = Cursors.Default
        End Try
    End Sub

    Private Function ValidateForm() As Boolean
        If String.IsNullOrWhiteSpace(txtAddressLine1.Text) Then
            MessageBox.Show("Address is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return False
        End If
        If String.IsNullOrWhiteSpace(txtYearBuilt.Text) OrElse Not Integer.TryParse(txtYearBuilt.Text, Nothing) Then
            MessageBox.Show("Valid year built is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return False
        End If
        If String.IsNullOrWhiteSpace(txtSquareFootage.Text) OrElse Not Integer.TryParse(txtSquareFootage.Text, Nothing) Then
            MessageBox.Show("Valid square footage is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return False
        End If
        Return True
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.Close()
    End Sub

    Private Sub frmPropertyEntry_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If _isDirty Then
            If MessageBox.Show("Discard unsaved changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.No Then e.Cancel = True
        End If
    End Sub
End Class
