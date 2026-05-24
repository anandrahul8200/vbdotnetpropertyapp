# Reports Module - Unit Tests

## Module: RPT (Reports)
## Test Type: Unit Tests for Report Data Retrieval
## Classes Covered:
- `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`
- `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

---

### Test Case ID: UT-RPT-001
**Class**: frmReportViewer
**Method**: btnRun_Click
**File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

#### Method Behavior
- Gets selected ReportDefinition from cboReport
- Builds SqlParameter list: @DateFrom, @DateTo, @StateCode, @PolicyType
- Calls DatabaseHelper.ExecuteStoredProcedure(report.StoredProcedure, params.ToArray())
- Binds returned DataTable to dgvReport.DataSource
- Updates lblRecordCount with row count
- Enables Export/Print buttons if rows > 0

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct stored procedure | Select "Production Report" | SP = "Reporting.usp_Report_Production" |
| 2 | Passes 4 parameters | All filters set | @DateFrom, @DateTo, @StateCode, @PolicyType SqlParameters |
| 3 | @StateCode is NULL when "(All)" | cboState.SelectedIndex=0 | Parameter value = Nothing |
| 4 | @PolicyType is NULL when "(All)" | cboPolicyType.SelectedIndex=0 | Parameter value = Nothing |
| 5 | @StateCode has value when selected | cboState.SelectedIndex=1 | Parameter value = state code string |
| 6 | @PolicyType has value when selected | cboPolicyType="HO3" | Parameter value = "HO3" |
| 7 | DataTable bound to grid | SP returns DataTable | dgvReport.DataSource = returned DataTable |
| 8 | Row count label updated | DataTable with 25 rows | lblRecordCount.Text = "25 row(s)" |
| 9 | Export enabled when rows exist | dt.Rows.Count > 0 | btnExport.Enabled = True |
| 10 | Export disabled when no rows | dt.Rows.Count = 0 | btnExport.Enabled = False |
| 11 | Exception caught and shown | SP throws SqlException | MessageBox shows "Error running report: " + message |
| 12 | Error logged on exception | Exception thrown | ErrorLogger.LogError(ex, "frmReportViewer.btnRun_Click") called |
| 13 | Cursor restored on exception | Exception in Finally | Me.Cursor = Cursors.Default |
| 14 | Returns early if no selection | cboReport.SelectedItem = Nothing | No SP call made |

---

### Test Case ID: UT-RPT-002
**Class**: frmReportViewer
**Method**: ExportToCsv(dgv As DataGridView, filePath As String)
**File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

#### Method Behavior
- Builds CSV string with headers and rows
- Headers are quoted column HeaderText values, comma-separated
- Row cells are quoted values with escaped internal quotes
- Writes to file via IO.File.WriteAllText

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Headers written first | Grid with 3 columns | First line = "Col1","Col2","Col3" |
| 2 | Values quoted | Cell value = "Hello" | Written as "Hello" in CSV |
| 3 | Internal quotes escaped | Cell value contains " | Doubled to "" |
| 4 | NULL cells handled | Cell value = Nothing | Written as empty string "" |
| 5 | Rows separated by newline | Multiple rows | Each row on new line (AppendLine) |
| 6 | File written to path | Valid path | IO.File.WriteAllText called with path and content |
| 7 | Comma separation | Multiple columns | Columns separated by comma |

---

### Test Case ID: UT-RPT-003
**Class**: ReportDefinition
**File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Constructor sets properties | New("Test Report", "usp_Test") | DisplayName="Test Report", StoredProcedure="usp_Test" |
| 2 | ToString returns DisplayName | DisplayName="Loss Run Report" | ToString() = "Loss Run Report" |
| 3 | DisplayName property readable | Set via constructor | Get returns same value |
| 4 | StoredProcedure property readable | Set via constructor | Get returns same value |

---

### Test Case ID: UT-RPT-004
**Class**: frmReportViewer
**Method**: LoadReportList
**File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Clears existing items | Items previously exist | cboReport.Items.Clear() called first |
| 2 | Adds 8 report definitions | Method called | cboReport.Items.Count = 8 |
| 3 | DisplayMember set | Method called | cboReport.DisplayMember = "DisplayName" |
| 4 | First item selected | Items added | cboReport.SelectedIndex = 0 |
| 5 | Report names correct | Items loaded | Includes "Loss Run Report", "Production Report", "Claims Aging Report", "Financial Summary", etc. |

---

### Test Case ID: UT-RPT-005
**Class**: DatabaseHelper
**Method**: ExecuteStoredProcedure (report context)
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Returns DataTable for report SPs | Call with valid SP name | DataTable populated with result set |
| 2 | CreateParam builds SqlParameter | CreateParam("@DateFrom", dateValue) | SqlParameter with correct name and value |
| 3 | NULL value handling | CreateParam("@StateCode", Nothing) | SqlParameter with DBNull.Value |
| 4 | Connection opened and closed | Any SP call | Connection properly managed |
