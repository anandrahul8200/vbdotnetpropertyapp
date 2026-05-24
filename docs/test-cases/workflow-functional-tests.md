# Workflow Module - Functional Tests

## Module: WFL (Workflow)
## Test Type: End-to-End Functional Tests
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

---

### Test Case ID: FT-WFL-001
**Feature**: Task Queue - View and Filter Tasks
**Priority**: High

#### Preconditions
- User is logged in with tasks assigned
- frmTaskQueue is open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmTaskQueue | Form loads with title "My Task Queue - {CurrentUserFullName}" |
| 2 | Tasks loaded automatically | LoadTasks() called on form load, lblTaskCount shows count |
| 3 | Filter by Module | Select "CLAIMS" from cboFilter | Only claims module tasks shown |
| 4 | Filter by Priority | Select "HIGH" from cboPriority | Only high priority tasks shown |
| 5 | Click Refresh | LoadTasks() called again, grid updated |
| 6 | Task count updated | After filter/refresh | lblTaskCount shows "N pending tasks" |

#### Validation Rules
- Module filter options: (All), POLICY, CLAIMS, UNDERWRITING, BILLING
- Priority filter options: (All), HIGH, NORMAL, LOW
- Default selection for both is "(All)" (SelectedIndex = 0)

---

### Test Case ID: FT-WFL-002
**Feature**: Task Completion
**Priority**: High

#### Preconditions
- frmTaskQueue is open with tasks displayed
- A task row is selected in dgvTasks

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a task in the grid | Row highlighted (FullRowSelect mode) |
| 2 | Click Complete button | MessageBox shows "Task marked complete." |
| 3 | Task list refreshed | LoadTasks() called to update grid |

#### Validation Rules
- Complete does nothing if no row selected (dgvTasks.CurrentRow Is Nothing)
- Task status updated to COMPLETED in database [ASSUMPTION]

---

### Test Case ID: FT-WFL-003
**Feature**: Task Reassignment
**Priority**: High

#### Preconditions
- frmTaskQueue is open with tasks displayed
- A task row is selected

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a task in the grid | Row highlighted |
| 2 | Click Reassign button | MessageBox shows "Reassignment dialog would open." |

#### Validation Rules
- Reassign does nothing if no row selected (dgvTasks.CurrentRow Is Nothing)
- Reassignment requires new assignee and optional reason [ASSUMPTION]

---

### Test Case ID: FT-WFL-004
**Feature**: Open Task Entity
**Priority**: Medium

#### Preconditions
- frmTaskQueue open with tasks displayed
- A task row is selected

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a task in the grid | Row highlighted |
| 2 | Click Open button | MessageBox shows "Would open the related entity." |

#### Validation Rules
- Open does nothing if no row selected (dgvTasks.CurrentRow Is Nothing)
- Should navigate to the linked entity (policy, claim, etc.) [ASSUMPTION]

---

### Test Case ID: FT-WFL-005
**Feature**: Notification Center - View Notifications
**Priority**: High

#### Preconditions
- User is logged in
- frmNotificationCenter is open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmNotificationCenter | Form loads with title "Notification Center" |
| 2 | Unread count displayed | lblUnreadCount shows "N unread notifications" in bold |
| 3 | Notifications displayed in grid | dgvNotifications populated with user's notifications |
| 4 | Click Refresh | Notifications reloaded, lblUnreadCount updated |

---

### Test Case ID: FT-WFL-006
**Feature**: Mark Notification as Read
**Priority**: High

#### Preconditions
- frmNotificationCenter open with unread notifications

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select an unread notification | Row highlighted |
| 2 | Click Mark Read button | MessageBox shows "Marked as read." |
| 3 | Notification visually updated | Unread count decremented [ASSUMPTION] |

#### Validation Rules
- Mark Read requires a row selection (CurrentRow IsNot Nothing)
- Calls Admin.usp_Notification_MarkRead with notification ID [ASSUMPTION]

---

### Test Case ID: FT-WFL-007
**Feature**: Mark All Notifications as Read
**Priority**: Medium

#### Preconditions
- frmNotificationCenter open with multiple unread notifications

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Mark All Read button | MessageBox shows "All notifications marked as read." |
| 2 | All notifications marked | All entries updated to read status [ASSUMPTION] |
| 3 | Unread count zeroed | lblUnreadCount shows "0 unread notifications" [ASSUMPTION] |

---

### Test Case ID: FT-WFL-008
**Feature**: Delete Notification
**Priority**: Medium

#### Preconditions
- frmNotificationCenter open with notifications

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a notification | Row highlighted |
| 2 | Click Delete button | MessageBox shows "Notification deleted." |

#### Validation Rules
- Delete requires a row selection (CurrentRow IsNot Nothing)

---

### Test Case ID: FT-WFL-009
**Feature**: Diary Manager - Search and Filter
**Priority**: High

#### Preconditions
- User is logged in
- frmDiaryManager is open
- Diary entries exist for the user's claims

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmDiaryManager | Form loads with title "Diary Manager" |
| 2 | Set date range | dtpDateFrom defaults to 7 days ago, dtpDateTo defaults to today |
| 3 | Select status filter | cboStatus options: (All), PENDING, OVERDUE, COMPLETED |
| 4 | Click Search | Grid populated, lblCount shows "N diary entries" |

#### Validation Rules
- Default Date From = Today - 7 days
- Default Date To = Today
- Default Status = "(All)" (SelectedIndex = 0)

---

### Test Case ID: FT-WFL-010
**Feature**: Diary Entry Completion and Rescheduling
**Priority**: High

#### Preconditions
- frmDiaryManager open with diary entries displayed
- A diary entry row is selected

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a diary entry | Row highlighted |
| 2 | Click Complete | MessageBox shows "Diary entry completed." |
| 3 | Click Reschedule | MessageBox shows "Reschedule dialog would open." |

#### Validation Rules
- Both Complete and Reschedule require a row selection (CurrentRow Is Nothing check)
- Completion marks the diary entry as done [ASSUMPTION]
- Rescheduling would update the due date [ASSUMPTION]
