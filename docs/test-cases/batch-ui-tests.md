# Batch Jobs Module - UI Tests

## Module: BAT (Batch Jobs)
## Test Type: Screen-Level UI Tests
## Forms Covered:
- `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

---

### Test Case ID: UI-BAT-001
**Form**: frmBatchJobMonitor
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Batch Job Monitor" |
| Size | 900 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboJobName | ComboBox | DropDownStyle=DropDownList, Items: (All), RenewalProcessor, ExpirationProcessor, FraudScoring, PaymentBatch, ReserveRecalculator, ReinsuranceAllocator |
| cboStatus | ComboBox | DropDownStyle=DropDownList, Items: (All), COMPLETED, FAILED, RUNNING |
| dtpDateFrom | DateTimePicker | Format=Short, Value=Today.AddDays(-7) |
| btnSearch | Button | Text="&Search" |
| btnViewLog | Button | Text="&View Log" |
| btnClose | Button | Text="&Close" |
| dgvJobHistory | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| lblCount | Label | AutoSize=True |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads correctly | Open Batch Job Monitor | All controls visible, proper layout, title = "Batch Job Monitor" |
| 2 | Job name dropdown populated | Form loads | 7 items: (All), RenewalProcessor, ExpirationProcessor, FraudScoring, PaymentBatch, ReserveRecalculator, ReinsuranceAllocator |
| 3 | Job name defaults to (All) | Form loads | cboJobName.SelectedIndex = 0 |
| 4 | Status dropdown populated | Form loads | 4 items: (All), COMPLETED, FAILED, RUNNING |
| 5 | Status defaults to (All) | Form loads | cboStatus.SelectedIndex = 0 |
| 6 | Date from defaults to 7 days ago | Form loads | dtpDateFrom.Value = DateTime.Today.AddDays(-7) |
| 7 | Search button click | Click &Search | lblCount updated with "0 job runs" |
| 8 | DataGridView read-only | Try to edit cell | No editing allowed |
| 9 | Full row selection | Click a cell | Entire row selected |
| 10 | View Log with no selection | Click &View Log with no row | No action (dgvJobHistory.CurrentRow Is Nothing check) |
| 11 | View Log with selection | Select row, click &View Log | MessageBox: "Job execution log would display here." |
| 12 | Close button | Click &Close | Form closes (Me.Close()) |

---

### Test Case ID: UI-BAT-002
**Form**: frmBatchJobMonitor - Filter Combinations
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

#### Filter Tests
| # | Test | Job Filter | Status Filter | Date | Expected |
|---|------|-----------|---------------|------|----------|
| 1 | All jobs, all statuses | (All) | (All) | 7 days ago | All job runs in last 7 days |
| 2 | Specific job | RenewalProcessor | (All) | 7 days ago | Only RenewalProcessor runs |
| 3 | Failed only | (All) | FAILED | 7 days ago | Only failed job runs |
| 4 | Specific job + status | FraudScoring | COMPLETED | 7 days ago | Only completed FraudScoring runs |
| 5 | Custom date range | (All) | (All) | 30 days ago | Broader date range results |

---

### Test Case ID: UI-BAT-003
**Form**: frmBatchJobMonitor - Panel Layout
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

#### Layout Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Top panel docked to top | pnlTop.Dock = DockStyle.Top, Height = 45 |
| 2 | Bottom panel docked to bottom | pnlBottom.Dock = DockStyle.Bottom, Height = 40 |
| 3 | DataGridView fills remaining space | dgvJobHistory.Dock = DockStyle.Fill |
| 4 | DataGridView brought to front | dgvJobHistory.BringToFront() ensures proper Z-order |
| 5 | Filter controls positioned correctly | Labels and controls at specified coordinates |

---
