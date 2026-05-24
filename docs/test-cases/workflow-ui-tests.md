# Workflow Module - UI Tests

## Module: WFL (Workflow)
## Test Type: Screen-Level UI Tests
## Forms Covered:
- `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

---

### Test Case ID: UI-WFL-001
**Form**: frmTaskQueue
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "My Task Queue - {GlobalState.CurrentUserFullName}" |
| Size | 900 x 550 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboFilter | ComboBox | DropDownStyle=DropDownList, Items={(All), POLICY, CLAIMS, UNDERWRITING, BILLING}, SelectedIndex=0 |
| cboPriority | ComboBox | DropDownStyle=DropDownList, Items={(All), HIGH, NORMAL, LOW}, SelectedIndex=0 |
| btnRefresh | Button | Text="&Refresh", Size=80x28 |
| lblTaskCount | Label | AutoSize=True, shows pending task count |
| dgvTasks | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| btnOpen | Button | Text="&Open", Size=80x30 |
| btnComplete | Button | Text="&Complete", Size=100x30 |
| btnReassign | Button | Text="Re&assign", Size=100x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads with user name in title | Open form | Title includes GlobalState.CurrentUserFullName |
| 2 | Module filter defaults to (All) | Form loads | cboFilter.SelectedIndex = 0 |
| 3 | Priority filter defaults to (All) | Form loads | cboPriority.SelectedIndex = 0 |
| 4 | Task count shown on load | Form loads | lblTaskCount.Text = "0 pending tasks" |
| 5 | Tasks loaded on form load | Form loads | LoadTasks() called |
| 6 | Grid is full-row select | Click any cell | Entire row selected |
| 7 | Grid is read-only | Try editing | No cell editing allowed |
| 8 | Refresh reloads tasks | Click Refresh | LoadTasks() called |
| 9 | Open requires selection | No row selected, click Open | Returns immediately |
| 10 | Complete requires selection | No row selected, click Complete | Returns immediately |
| 11 | Reassign requires selection | No row selected, click Reassign | Returns immediately |
| 12 | Open shows entity message | Row selected, click Open | "Would open the related entity." |
| 13 | Complete shows confirmation | Row selected, click Complete | "Task marked complete." then reload |
| 14 | Reassign shows dialog message | Row selected, click Reassign | "Reassignment dialog would open." |

---

### Test Case ID: UI-WFL-002
**Form**: frmNotificationCenter
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Notification Center" |
| Size | 700 x 450 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| lblUnreadCount | Label | AutoSize=True, Font=Bold, Text="0 unread notifications" |
| btnRefresh | Button | Text="&Refresh", Size=80x28 |
| btnMarkAllRead | Button | Text="Mark All Read", Size=90x28 |
| dgvNotifications | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| btnMarkRead | Button | Text="Mark &Read", Size=100x30 |
| btnDelete | Button | Text="&Delete", Size=80x30 |
| btnClose | Button | Text="&Close", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads correctly | Open form | All controls visible |
| 2 | Unread count in bold | Form loads | lblUnreadCount.Font = Bold |
| 3 | Unread count initialized | Form loads | "0 unread notifications" |
| 4 | Refresh updates count | Click Refresh | lblUnreadCount.Text updated |
| 5 | Grid is full-row select | Click any cell | Entire row selected |
| 6 | Grid is read-only | Try editing | No cell editing |
| 7 | Mark Read requires selection | No selection, click Mark Read | No action (if check) |
| 8 | Mark Read shows confirmation | Row selected, click Mark Read | "Marked as read." |
| 9 | Mark All Read no selection needed | Click Mark All Read | "All notifications marked as read." |
| 10 | Delete requires selection | No selection, click Delete | No action (if check) |
| 11 | Delete shows confirmation | Row selected, click Delete | "Notification deleted." |
| 12 | Close button works | Click Close | Form closes |

---

### Test Case ID: UI-WFL-003
**Form**: frmDiaryManager
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Diary Manager" |
| Size | 850 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| dtpDateFrom | DateTimePicker | Format=Short, Value=DateTime.Today.AddDays(-7) |
| dtpDateTo | DateTimePicker | Format=Short, Value=today |
| cboStatus | ComboBox | DropDownStyle=DropDownList, Items={(All), PENDING, OVERDUE, COMPLETED}, SelectedIndex=0 |
| btnSearch | Button | Text="&Search", Size=80x28 |
| lblCount | Label | AutoSize=True |
| dgvDiary | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| btnComplete | Button | Text="&Complete", Size=100x30 |
| btnReschedule | Button | Text="&Reschedule", Size=100x30 |
| btnClose | Button | Text="C&lose", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads correctly | Open form | All controls visible |
| 2 | Date From defaults to 7 days ago | Form loads | dtpDateFrom.Value = DateTime.Today.AddDays(-7) |
| 3 | Date To defaults to today | Form loads | dtpDateTo.Value = today |
| 4 | Status defaults to (All) | Form loads | cboStatus.SelectedIndex = 0 |
| 5 | Status options | Click dropdown | (All), PENDING, OVERDUE, COMPLETED |
| 6 | Search updates count | Click Search | lblCount.Text = "0 diary entries" |
| 7 | Grid is full-row select | Click any cell | Entire row selected |
| 8 | Grid is read-only | Try editing | No cell editing |
| 9 | Complete requires selection | No row selected | Returns immediately |
| 10 | Complete shows confirmation | Row selected, click Complete | "Diary entry completed." |
| 11 | Reschedule requires selection | No row selected | Returns immediately |
| 12 | Reschedule shows dialog message | Row selected, click Reschedule | "Reschedule dialog would open." |
| 13 | Close button works | Click Close | Form closes |
| 14 | Grid brought to front | Form loads | dgvDiary.BringToFront() called for Dock layout |
