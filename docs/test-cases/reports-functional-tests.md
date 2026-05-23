# Reports Module - Functional Tests

## Module: RPT (Reports)
## Test Type: End-to-End Functional Tests
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`
- `database/02-stored-procedures/009-reporting-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

---

### Test Case ID: FT-RPT-001
**Feature**: Report Selection and Execution
**Priority**: High

#### Preconditions
- User is logged in with report viewing permissions
- frmReportViewer is open
- Database contains policy, claims, and billing data

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmReportViewer | Form loads with Report ComboBox populated with 8 report definitions |
| 2 | Select "Loss Run Report" from combo | cboReport.SelectedItem = ReportDefinition("Loss Run Report", "Reporting.usp_Report_LossRun") |
| 3 | Set Date From and Date To parameters | DateTimePicker controls accept date values |
| 4 | Click Run button | Cursor changes to WaitCursor, DatabaseHelper.ExecuteStoredProcedure called |
| 5 | Report data returns | DataGridView populated with results, lblRecordCount shows row count |
| 6 | Export and Print buttons enabled | btnExport.Enabled=True, btnPrint.Enabled=True (when rows > 0) |

#### Validation Rules
- Report combo must have a selected item before Run executes
- Parameters @DateFrom, @DateTo, @StateCode, @PolicyType passed to SP
- @StateCode is NULL if "(All)" selected (cboState.SelectedIndex = 0)
- @PolicyType is NULL if "(All)" selected (cboPolicyType.SelectedIndex = 0)
- Errors caught and shown via MessageBox with "Error running report: " prefix

---

### Test Case ID: FT-RPT-002
**Feature**: Report Export to CSV
**Priority**: High

#### Preconditions
- frmReportViewer is open
- A report has been successfully executed with data in the grid

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Export button | SaveFileDialog opens with filter "CSV Files|*.csv" |
| 2 | Select save location and confirm | ExportToCsv method called with grid and file path |
| 3 | CSV file written | File contains headers (quoted) and data rows (quoted, comma-separated) |
| 4 | Success message | MessageBox shows "Report exported." |

#### Validation Rules
- Export button disabled until report has rows (btnExport.Enabled = dt.Rows.Count > 0)
- CSV headers use DataGridView column HeaderText values
- Cell values with quotes are escaped with double quotes
- Each row ends with newline

---

### Test Case ID: FT-RPT-003
**Feature**: Production Report Workflow
**Priority**: High

#### Preconditions
- Policies exist with agents assigned
- Policies have mix of IsRenewal=0 (new business) and IsRenewal=1 (renewals)

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "Production Report" from combo | Report selected |
| 2 | Set date range for fiscal year | @DateFrom and @DateTo set |
| 3 | Click Run | usp_Report_Production (or mapped SP) executed |
| 4 | Results show agent breakdown | AgentNumber, AgentName, AgencyName, NewBusinessCount, NewBusinessPremium, RenewalCount, RenewalPremium, TotalPolicies, TotalPremium, TotalCommission |
| 5 | Results ordered by TotalPremium | Highest producing agents listed first |

---

### Test Case ID: FT-RPT-004
**Feature**: Claims Aging Report Workflow
**Priority**: High

#### Preconditions
- Open claims exist with various ReportedDate values
- Claims span multiple aging buckets

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "Claims Aging Report" from combo | Report selected |
| 2 | Click Run | Admin.usp_Report_ClaimsAging executed |
| 3 | Results show aging buckets | AgingBucket, ClaimCount, TotalReserve, TotalPaid, TotalIncurred, AvgIncurred |
| 4 | Only open claims shown | Claims with status CLOSED or DENIED excluded |
| 5 | Buckets ordered by age | 0-30 Days first, 365+ Days last |

---

### Test Case ID: FT-RPT-005
**Feature**: Financial Summary Report Workflow
**Priority**: High

#### Preconditions
- Policies, claims, payments, commissions, and reinsurance data exist

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select "Financial Summary" from combo | Report selected |
| 2 | Set date range parameters | @PeriodFrom and @PeriodTo populated |
| 3 | Click Run | Admin.usp_Report_FinancialSummary executed |
| 4 | Results show 7 financial categories | Written Premium, Earned Premium, Incurred Losses, Paid Losses, Outstanding Reserves, Commissions, Ceded Premium (Reinsurance) |

---

### Test Case ID: FT-RPT-006
**Feature**: Executive Dashboard Report
**Priority**: High

#### Preconditions
- Active policies exist
- Open claims exist
- Overdue billing exists

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Execute Admin.usp_Report_ExecutiveDashboard | 5 result sets returned |
| 2 | Policy counts by status | PolicyStatus grouped with counts and premium totals |
| 3 | Written premium trend | Monthly breakdown for last 12 months |
| 4 | Claims summary | Total open claims, reserves, paid, and incurred amounts |
| 5 | Loss ratio | Earned premium vs incurred losses ratio calculation |
| 6 | Outstanding billing | Count and total of overdue/partial invoices |

---

### Test Case ID: FT-RPT-007
**Feature**: Report Parameter Filtering
**Priority**: Medium

#### Preconditions
- frmReportViewer open with parameters panel visible
- Data exists across multiple states and policy types

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Set State filter to specific state code | cboState selection passed as @StateCode |
| 2 | Set Policy Type to "HO3" | cboPolicyType selection passed as @PolicyType |
| 3 | Run report | Results filtered by both state and policy type |
| 4 | Set State back to "(All)" | @StateCode passed as Nothing (NULL) |
| 5 | Run report again | Results not filtered by state |

---

### Test Case ID: FT-RPT-008
**Feature**: Report Error Handling
**Priority**: Medium

#### Preconditions
- frmReportViewer open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Simulate database connection failure | Exception thrown during ExecuteStoredProcedure |
| 2 | Error caught in Try/Catch | MessageBox shows "Error running report: " + ex.Message |
| 3 | Error logged | ErrorLogger.LogError called with exception and "frmReportViewer.btnRun_Click" |
| 4 | Cursor restored | Me.Cursor = Cursors.Default in Finally block |
