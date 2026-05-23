# Reports Module - UI Tests

## Module: RPT (Reports)
## Test Type: Screen-Level UI Tests
## Forms Covered:
- `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

---

### Test Case ID: UI-RPT-001
**Form**: frmReportViewer
**File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Report Viewer" |
| Size | 1100 x 700 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboReport | ComboBox | DropDownStyle=DropDownList, Size=300x20, DisplayMember="DisplayName" |
| btnRun | Button | Text="&Run", Size=80x30 |
| btnExport | Button | Text="&Export", Size=80x30, Enabled=False initially |
| btnPrint | Button | Text="&Print", Size=80x30, Enabled=False initially |
| dtpDateFrom | DateTimePicker | Format=Short, Value=Jan 1 of current year |
| dtpDateTo | DateTimePicker | Format=Short, Value=today |
| cboState | ComboBox | DropDownStyle=DropDownList, Items include "(All)", SelectedIndex=0 |
| cboPolicyType | ComboBox | DropDownStyle=DropDownList, Items={"(All)", "HO3", "HO4", "HO6", "DP3"}, SelectedIndex=0 |
| dgvReport | DataGridView | ReadOnly=True, AllowUserToAddRows=False, AutoSizeColumnsMode=Fill, AlternatingRowsDefaultCellStyle.BackColor=AliceBlue |
| lblRecordCount | Label | AutoSize=True, shows row count after report execution |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads correctly | Open frmReportViewer | All controls visible, proper layout |
| 2 | Report combo populated | Form loads | 8 report items loaded via LoadReportList() |
| 3 | First report selected by default | Form loads | cboReport.SelectedIndex = 0 |
| 4 | Date From defaults to Jan 1 | Form loads | dtpDateFrom.Value = New DateTime(DateTime.Now.Year, 1, 1) |
| 5 | Date To defaults to today | Form loads | dtpDateTo.Value = today |
| 6 | Export disabled initially | Form loads | btnExport.Enabled = False |
| 7 | Print disabled initially | Form loads | btnPrint.Enabled = False |
| 8 | Grid has alternating row colors | Report run | AlternatingRows BackColor = AliceBlue |
| 9 | Record count displayed | Report returns 50 rows | lblRecordCount.Text = "50 row(s)" |
| 10 | Export enabled after results | Report returns > 0 rows | btnExport.Enabled = True |
| 11 | Print enabled after results | Report returns > 0 rows | btnPrint.Enabled = True |
| 12 | WaitCursor during execution | Click Run | Cursor = WaitCursor then Default |
| 13 | State combo has "(All)" first | Form loads | cboState.Items(0) = "(All)", SelectedIndex = 0 |
| 14 | Policy type combo options | Form loads | Items: "(All)", "HO3", "HO4", "HO6", "DP3" |

---

### Test Case ID: UI-RPT-002
**Form**: frmReportViewer
**Feature**: Report Selection Interaction

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Report selection triggers event | Change cboReport selection | ReportChanged event handler fires |
| 2 | Parameters panel visible | Form loads | pnlParameters visible with Date From, Date To, State, Type labels and controls |
| 3 | Parameters panel height | Form loads | pnlParameters.Height = 50 |
| 4 | Run with no selection | cboReport.SelectedItem = Nothing, click Run | Returns immediately (If ... Is Nothing Then Return) |

---

### Test Case ID: UI-RPT-003
**Form**: frmReportViewer
**Feature**: Export Dialog Interaction

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Export opens SaveFileDialog | Click Export | SaveFileDialog displayed with filter "CSV Files|*.csv" |
| 2 | Cancel export | Click Cancel on dialog | No file written, no success message |
| 3 | Successful export message | Save file successfully | MessageBox shows "Report exported." |
| 4 | Export error handling | File write fails | MessageBox shows "Export error: " + error message |

---

### Test Case ID: UI-RPT-004
**Form**: frmReportViewer
**Feature**: Print Interaction

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Print button click | Click Print | MessageBox shows "Print preview would open here." |
| 2 | Print disabled without data | No report run | btnPrint.Enabled = False |

---

### Test Case ID: UI-RPT-005
**Form**: frmReportViewer
**Feature**: Report Definitions in ComboBox

#### Available Reports
| # | DisplayName | StoredProcedure |
|---|-------------|-----------------|
| 1 | Loss Run Report | Reporting.usp_Report_LossRun |
| 2 | Production Report | Reporting.usp_Report_Production |
| 3 | Claims Aging Report | Reporting.usp_Report_ClaimsAging |
| 4 | Financial Summary | Reporting.usp_Report_FinancialSummary |
| 5 | Open Claims by Adjuster | Reporting.usp_Report_OpenClaimsByAdjuster |
| 6 | Premium by State | Reporting.usp_Report_PremiumByState |
| 7 | Catastrophe Summary | Reporting.usp_Report_CatastropheSummary |
| 8 | Commission Summary | Reporting.usp_Report_CommissionSummary |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | ComboBox displays names | Form loads | DisplayMember shows DisplayName property |
| 2 | ToString returns name | Inspect item | ReportDefinition.ToString() returns DisplayName |
| 3 | Selected item is ReportDefinition | Select item | DirectCast to ReportDefinition succeeds |
