# Reinsurance Module - UI Tests

## Module: RNS (Reinsurance)
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

## Forms/Screens Covered:
1. frmReinsuranceView (Treaties tab, Cessions tab, Bordereaux tab)

---

### Test Case ID: UI-RNS-001
**Form/Screen**: frmReinsuranceView
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens without error | Form loads with title "Reinsurance Management", size 1000x650, centered on parent |
| 2 | Tab control visible with 3 tabs | tabControl contains Treaties, Cessions, Bordereaux tabs |
| 3 | Treaties tab is default active tab | Treaties tab displayed first on form load |
| 4 | Treaties grid populated on load | dgvTreaties bound to data from ReinsuranceDataAccess.GetActiveTreaties() |
| 5 | Treaty filter dropdowns populated | cboTreatyFilter and cboTreatyForBord contain treaty entries (format: "TreatyNumber - TreatyName") |
| 6 | TreatyID column hidden | dgvTreaties.Columns("TreatyID").Visible = False |
| 7 | Default period set | txtAccountingPeriod.Text = current date in "yyyy-MM" format |
| 8 | Bordereaux period default set | txtBordPeriod.Text = current date in "yyyy-MM" format |
| 9 | Report type dropdown populated | cboReportType contains: PREMIUM, LOSS, OUTSTANDING (SelectedIndex=0) |
| 10 | Close button visible | btnClose positioned at bottom-right panel |

#### Form Load Error Handling
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database connection fails on load | MessageBox: "Error: [exception message]", title "Error", Icon=Error |
| 2 | Error logged | ErrorLogger.LogError called with exception and "frmReinsuranceView_Load" |
| 3 | LoadTreaties fails | MessageBox: "Error loading treaties: [message]", form remains open |

---

### Test Case ID: UI-RNS-002
**Form/Screen**: frmReinsuranceView - Treaties Tab
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Control Layout Tests
| # | Control | Type | Properties |
|---|---------|------|------------|
| 1 | btnNewTreaty | Button | Text="&New Treaty", Location=(10,6), Size=(100,28) |
| 2 | btnViewTreaty | Button | Text="&View Details", Location=(120,6), Size=(100,28) |
| 3 | btnRefreshTreaties | Button | Text="Refresh", Location=(850,6), Size=(80,28) |
| 4 | dgvTreaties | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |

#### Button Click Tests
| # | Button | Action | Expected Result |
|---|--------|--------|-----------------|
| 1 | btnNewTreaty | Click | MessageBox: "New treaty creation dialog would open here.", title "Info", Icon=Information |
| 2 | btnViewTreaty (row selected) | Click | MessageBox: "Treaty details view for ID {treatyID} would open here." |
| 3 | btnViewTreaty (no row selected) | Click | Nothing happens (CurrentRow Is Nothing check) |
| 4 | btnRefreshTreaties | Click | LoadTreaties() called, grid refreshed |

#### Grid Interaction Tests
| # | Test | Expected Behavior |
|---|------|-------------------|
| 1 | Select row in dgvTreaties | Full row highlighted (FullRowSelect mode) |
| 2 | Double-click row | [ASSUMPTION] No double-click handler defined |
| 3 | Attempt to edit cell | Editing blocked (ReadOnly=True) |
| 4 | Attempt to add row | Not possible (AllowUserToAddRows=False) |

---

### Test Case ID: UI-RNS-003
**Form/Screen**: frmReinsuranceView - Cessions Tab
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Control Layout Tests
| # | Control | Type | Properties |
|---|---------|------|------------|
| 1 | cboTreatyFilter | ComboBox | DropDownStyle=DropDownList, Location=(60,11), Size=(250,20) |
| 2 | txtAccountingPeriod | TextBox | Location=(380,11), Size=(80,20), default=current "yyyy-MM" |
| 3 | btnLoadCessions | Button | Text="&Load", Location=(480,8), Size=(80,28) |
| 4 | btnCalculateCession | Button | Text="Calculate &New", Location=(570,8), Size=(120,28) |
| 5 | lblCessionSummary | Label | AutoSize=True, Location=(710,14) |
| 6 | dgvCessions | DataGridView | ReadOnly=True, AllowUserToAddRows=False, AutoSizeColumnsMode=Fill |

#### Button Click Tests
| # | Button | Pre-condition | Expected Result |
|---|--------|---------------|-----------------|
| 1 | btnLoadCessions (treaty selected) | cboTreatyFilter.SelectedIndex >= 0 | Cursor=WaitCursor, grid populated, summary label updated, Cursor=Default |
| 2 | btnLoadCessions (no treaty) | cboTreatyFilter.SelectedIndex < 0 | Nothing happens (early return) |
| 3 | btnCalculateCession | Click | MessageBox: "Cession calculation dialog would open here." |

#### Summary Label Tests
| # | Scenario | Expected lblCessionSummary.Text |
|---|----------|---------------------------------|
| 1 | Cessions loaded with data | "Gross: $X | Ceded: $Y" (currency format, no decimals) |
| 2 | No cessions for period | "Gross: $0 | Ceded: $0" |
| 3 | DBNull values in amounts | DBNull handled (IsDBNull check), treated as 0 |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database error during Load | MessageBox: "Error: [message]", Cursor reset to Default |
| 2 | Cursor always restored | Even on exception | Cursor=Default (Finally block) |

---

### Test Case ID: UI-RNS-004
**Form/Screen**: frmReinsuranceView - Bordereaux Tab
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Control Layout Tests
| # | Control | Type | Properties |
|---|---------|------|------------|
| 1 | cboTreatyForBord | ComboBox | DropDownStyle=DropDownList, Location=(60,11), Size=(250,20) |
| 2 | txtBordPeriod | TextBox | Location=(380,11), Size=(80,20), default=current "yyyy-MM" |
| 3 | cboReportType | ComboBox | DropDownStyle=DropDownList, Items={"PREMIUM","LOSS","OUTSTANDING"}, SelectedIndex=0 |
| 4 | btnGenerateBord | Button | Text="&Generate", Location=(10,44), Size=(100,28) |
| 5 | btnSubmitBord | Button | Text="&Submit", Location=(120,44), Size=(100,28) |
| 6 | btnExportBord | Button | Text="E&xport", Location=(230,44), Size=(80,28) |
| 7 | btnRefreshBord | Button | Text="Refresh", Location=(850,44), Size=(80,28) |
| 8 | dgvBordereaux | DataGridView | ReadOnly=True, AllowUserToAddRows=False, AutoSizeColumnsMode=Fill |

#### Button Click Tests - Generate
| # | Pre-condition | Action | Expected Result |
|---|---------------|--------|-----------------|
| 1 | Treaty selected, period entered | Click btnGenerateBord | Cursor=WaitCursor, bordereaux generated, MessageBox: "Bordereaux generated (ID: N).", Refresh triggered |
| 2 | No treaty selected | Click btnGenerateBord | Nothing happens (SelectedIndex < 0 check) |
| 3 | Error during generation | Click btnGenerateBord | MessageBox: "Error: [message]", Cursor=Default |

#### Button Click Tests - Submit
| # | Pre-condition | Action | Expected Result |
|---|---------------|--------|-----------------|
| 1 | Row selected in dgvBordereaux | Click btnSubmitBord | MessageBox: "Bordereaux submitted to reinsurer." |
| 2 | No row selected | Click btnSubmitBord | Nothing happens (CurrentRow Is Nothing check) |

#### Button Click Tests - Export
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click btnExportBord | MessageBox: "Export to Excel/CSV would happen here." |

#### Button Click Tests - Refresh
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click btnRefreshBord | dgvBordereaux reloaded from ReinsuranceDataAccess.GetBordereaux(treatyID) |
| 2 | Error during refresh | MessageBox: "Error: [message]" |

---

### Test Case ID: UI-RNS-005
**Form/Screen**: frmReinsuranceView - Navigation and General
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Switch to Treaties tab | Treaties grid and buttons visible |
| 2 | Switch to Cessions tab | Treaty filter, period field, Load/Calculate buttons visible |
| 3 | Switch to Bordereaux tab | Treaty/period/type filters, Generate/Submit/Export buttons visible |
| 4 | Click Close button | Form closes (Me.Close()) |

#### Form Properties Tests
| # | Property | Expected Value |
|---|----------|----------------|
| 1 | Form title | "Reinsurance Management" |
| 2 | Form size | 1000 x 650 pixels |
| 3 | Start position | CenterParent |
| 4 | Tab control dock | DockStyle.Fill |
| 5 | Bottom panel height | 40 pixels |

#### Keyboard Accelerator Tests
| # | Key Combination | Expected Action |
|---|-----------------|-----------------|
| 1 | Alt+N | Activates btnNewTreaty ("&New Treaty") |
| 2 | Alt+V | Activates btnViewTreaty ("&View Details") |
| 3 | Alt+L | Activates btnLoadCessions ("&Load") |
| 4 | Alt+N (Cessions tab) | Activates btnCalculateCession ("Calculate &New") |
| 5 | Alt+G | Activates btnGenerateBord ("&Generate") |
| 6 | Alt+S | Activates btnSubmitBord ("&Submit") |
| 7 | Alt+X | Activates btnExportBord ("E&xport") |
| 8 | Alt+C | Activates btnClose ("&Close") |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database unavailable on form load | Error message displayed, ErrorLogger.LogError called |
| 2 | Exception in any button handler | MessageBox with error details, form remains functional |
| 3 | DBNull values in grid data | Handled gracefully (IsDBNull checks in summary calculation) |
