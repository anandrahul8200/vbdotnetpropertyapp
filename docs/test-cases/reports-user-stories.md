# Reports Module - User Stories

## Module: RPT (Reports)
## Test Type: User Story Acceptance Criteria

---

### User Story ID: US-RPT-001
**Title**: Run and View Reports
**As a** claims manager or underwriter
**I want to** select and run various reports with date and filter parameters
**So that** I can analyze business performance and make data-driven decisions

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Report Viewer form displays a list of available reports | cboReport populated with 8 report definitions |
| 2 | I can select date range parameters (From and To) | dtpDateFrom and dtpDateTo accept date input |
| 3 | I can filter by State and Policy Type | cboState and cboPolicyType provide dropdown selections |
| 4 | Clicking Run executes the selected report | DatabaseHelper.ExecuteStoredProcedure called with SP and parameters |
| 5 | Results display in a grid with row count | dgvReport shows data, lblRecordCount shows "N row(s)" |
| 6 | Wait cursor shown during execution | Me.Cursor = WaitCursor then restored to Default |
| 7 | Errors are displayed in a user-friendly message | "Error running report: " + error text |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-001 | Report Selection and Execution |
| UI-RPT-001 | frmReportViewer controls |
| UT-RPT-001 | btnRun_Click unit tests |

---

### User Story ID: US-RPT-002
**Title**: Export Report to CSV
**As a** business analyst
**I want to** export report results to a CSV file
**So that** I can perform further analysis in Excel or other tools

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Export button is enabled only when report has data | btnExport.Enabled = dt.Rows.Count > 0 |
| 2 | Clicking Export opens a Save dialog with CSV filter | SaveFileDialog with "CSV Files|*.csv" |
| 3 | CSV file contains column headers | First row has quoted column names |
| 4 | CSV file contains all data rows | All grid rows written |
| 5 | Special characters in data are properly escaped | Quotes doubled, commas within quotes |
| 6 | Success confirmation shown after export | MessageBox "Report exported." |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-002 | Report Export to CSV |
| UT-RPT-002 | ExportToCsv method tests |
| UI-RPT-003 | Export Dialog Interaction |

---

### User Story ID: US-RPT-003
**Title**: View Executive Dashboard
**As an** executive or senior manager
**I want to** see a high-level summary of policy, claims, and billing metrics
**So that** I can quickly assess overall company performance

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Dashboard shows policy counts by status | Result set 1: PolicyStatus, PolicyCount, TotalPremium |
| 2 | Dashboard shows 12-month premium trend | Result set 2: Monthly WrittenPremium for last 12 months |
| 3 | Dashboard shows open claims summary | Result set 3: TotalOpenClaims, TotalOutstandingReserve, TotalPaidYTD, TotalIncurred |
| 4 | Dashboard shows loss ratio | Result set 4: EarnedPremium, IncurredLosses, LossRatio percentage |
| 5 | Dashboard shows overdue billing | Result set 5: OverdueInvoices count, TotalOverdue amount |
| 6 | Data can be generated as of any date | @AsOfDate parameter accepted |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-006 | Executive Dashboard Report |
| SP-RPT-001 | usp_Report_ExecutiveDashboard |
| NFR-RPT-001 | Dashboard performance |

---

### User Story ID: US-RPT-004
**Title**: View Loss Ratio Analysis
**As an** underwriting manager
**I want to** view loss ratio data grouped by policy type, state, or agent
**So that** I can identify unprofitable segments and adjust pricing

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Loss ratio can be grouped by Policy Type | @GroupBy='POLICY_TYPE' returns per-type results |
| 2 | Loss ratio can be grouped by State | @GroupBy='STATE' returns per-state results ordered by ratio |
| 3 | Loss ratio can be grouped by Agent | @GroupBy='AGENT' returns per-agent results ordered by ratio |
| 4 | Date range is configurable | @PeriodFrom and @PeriodTo parameters |
| 5 | Loss ratio calculated as IncurredLosses / EarnedPremium * 100 | Rounded to 2 decimal places |
| 6 | Zero premium segments show 0% ratio | CASE WHEN prevents divide-by-zero |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-002 | usp_Report_LossRatio |
| BR-RPT-001 | Loss Ratio Calculation Rule |
| DV-RPT-002 | Loss Ratio data validation |

---

### User Story ID: US-RPT-005
**Title**: View Claims Aging Report
**As a** claims manager
**I want to** see open claims grouped by how long they have been open
**So that** I can prioritize older claims and manage reserves effectively

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Claims grouped into aging buckets | 0-30, 31-60, 61-90, 91-180, 181-365, 365+ Days |
| 2 | Each bucket shows claim count | COUNT(*) per bucket |
| 3 | Each bucket shows total reserve and paid amounts | SUM(TotalReserve), SUM(TotalPaid) |
| 4 | Average incurred shown per bucket | AVG(NetIncurred) |
| 5 | Only open claims included | Excludes CLOSED and DENIED |
| 6 | Results ordered by age (youngest first) | ORDER BY MIN(DATEDIFF) ascending |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-004 | Claims Aging Report Workflow |
| SP-RPT-003 | usp_Report_ClaimsAging |
| BR-RPT-002 | Claims Aging Bucket Rules |

---

### User Story ID: US-RPT-006
**Title**: View Agent Production Report
**As a** sales manager
**I want to** see production metrics for each agent (new business and renewals)
**So that** I can evaluate agent performance and commission payouts

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Report shows each active agent | Grouped by agent with AgentNumber, Name, Agency |
| 2 | New business and renewal counts shown separately | IsRenewal=0 vs IsRenewal=1 split |
| 3 | Premium totals shown for new and renewal | Separate premium columns |
| 4 | Total commission displayed | SUM(CommissionAmount) |
| 5 | Can filter to a specific agent | @AgentID parameter |
| 6 | Results ordered by total premium | Highest producer first |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-003 | Production Report Workflow |
| SP-RPT-004 | usp_Report_Production |
| SP-RPT-008 | usp_Report_AgentProduction |

---

### User Story ID: US-RPT-007
**Title**: View Financial Summary
**As a** finance director
**I want to** see a consolidated view of premiums, losses, reserves, commissions, and reinsurance
**So that** I can assess the company's financial position for a given period

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Written Premium total shown | SUM for ACTIVE/EXPIRED policies in period |
| 2 | Earned Premium calculated pro-rata | Days-in-period / Total-term formula |
| 3 | Incurred Losses shown | Claims with LossDate in period |
| 4 | Paid Losses shown | Approved/Issued/Cleared payments in period |
| 5 | Outstanding Reserves shown | All open claims (not date-filtered) |
| 6 | Commission total shown | EARNED commission transactions in period |
| 7 | Ceded Reinsurance Premium shown | PREMIUM type cessions in period |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-005 | Financial Summary Report Workflow |
| SP-RPT-005 | usp_Report_FinancialSummary |
| DV-RPT-005 | Financial Summary data validation |

---

### User Story ID: US-RPT-008
**Title**: View Policy KPIs Dashboard
**As a** branch manager
**I want to** see key policy indicators at a glance including expiring policies
**So that** I can manage renewals and track growth metrics

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Active policy count displayed | COUNT where PolicyStatus='ACTIVE' |
| 2 | New business count for last month | Policies created in last 30 days |
| 3 | Renewal count for last month | IsRenewal=1 created in last 30 days |
| 4 | Cancellation count for last month | CANCELLED policies modified in last 30 days |
| 5 | Policies expiring within 30 days listed | TOP 20 ordered by ExpiryDate |
| 6 | Recent activity shown | TOP 20 from last 7 days |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-006 | usp_Dashboard_PolicyKPIs |
| DV-RPT-006 | Policy KPIs data validation |
