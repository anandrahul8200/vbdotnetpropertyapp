# Workflow Module - Unit Tests

## Module: WFL (Workflow)
## Test Type: Unit Tests for Workflow Operations
## Classes Covered:
- `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`
- `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`

---

### Test Case ID: UT-WFL-001
**Class**: frmTaskQueue
**Method**: LoadTasks
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

#### Method Behavior
- Queries pending activities/tasks assigned to current user
- Updates lblTaskCount with pending task count

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Task count label updated | Method called | lblTaskCount.Text = "0 pending tasks" |
| 2 | Called on form load | Form loads | LoadTasks() invoked during frmTaskQueue_Load |
| 3 | Called on refresh | Click Refresh | LoadTasks() invoked |
| 4 | Called after completion | Task completed | LoadTasks() invoked to refresh |

---

### Test Case ID: UT-WFL-002
**Class**: frmTaskQueue
**Method**: btnComplete_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

#### Method Behavior
- Returns if dgvTasks.CurrentRow Is Nothing
- Shows "Task marked complete." message
- Calls LoadTasks() to refresh

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately, no message |
| 2 | Row selected | CurrentRow is valid | MessageBox "Task marked complete." |
| 3 | Grid refreshed after complete | Valid completion | LoadTasks() called |

---

### Test Case ID: UT-WFL-003
**Class**: frmTaskQueue
**Method**: btnReassign_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

#### Method Behavior
- Returns if dgvTasks.CurrentRow Is Nothing
- Shows "Reassignment dialog would open."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately |
| 2 | Row selected | CurrentRow is valid | MessageBox "Reassignment dialog would open." |

---

### Test Case ID: UT-WFL-004
**Class**: frmTaskQueue
**Method**: btnOpen_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`

#### Method Behavior
- Returns if dgvTasks.CurrentRow Is Nothing
- Shows "Would open the related entity."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately |
| 2 | Row selected | CurrentRow is valid | MessageBox "Would open the related entity." |

---

### Test Case ID: UT-WFL-005
**Class**: frmNotificationCenter
**Method**: btnMarkRead_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`

#### Method Behavior
- Checks dgvNotifications.CurrentRow IsNot Nothing
- If selected, shows "Marked as read."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | No action |
| 2 | Row selected | CurrentRow IsNot Nothing | MessageBox "Marked as read." |

---

### Test Case ID: UT-WFL-006
**Class**: frmNotificationCenter
**Method**: btnMarkAllRead_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`

#### Method Behavior
- Shows "All notifications marked as read." (no selection required)

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No selection required | Any state | MessageBox "All notifications marked as read." |
| 2 | With unread notifications | Multiple unread | Same message shown |
| 3 | With no unread | All read | Same message shown |

---

### Test Case ID: UT-WFL-007
**Class**: frmNotificationCenter
**Method**: btnDelete_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`

#### Method Behavior
- Checks dgvNotifications.CurrentRow IsNot Nothing
- If selected, shows "Notification deleted."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | No action |
| 2 | Row selected | CurrentRow IsNot Nothing | MessageBox "Notification deleted." |

---

### Test Case ID: UT-WFL-008
**Class**: frmNotificationCenter
**Method**: btnRefresh_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`

#### Method Behavior
- Updates lblUnreadCount.Text to "0 unread notifications"

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Refresh updates label | Click Refresh | lblUnreadCount.Text = "0 unread notifications" |

---

### Test Case ID: UT-WFL-009
**Class**: frmDiaryManager
**Method**: btnSearch_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`

#### Method Behavior
- Updates lblCount.Text to "0 diary entries"

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Search updates count label | Click Search | lblCount.Text = "0 diary entries" |

---

### Test Case ID: UT-WFL-010
**Class**: frmDiaryManager
**Method**: btnComplete_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`

#### Method Behavior
- Returns if dgvDiary.CurrentRow Is Nothing
- Shows "Diary entry completed."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately |
| 2 | Row selected | CurrentRow is valid | MessageBox "Diary entry completed." |

---

### Test Case ID: UT-WFL-011
**Class**: frmDiaryManager
**Method**: btnReschedule_Click
**File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`

#### Method Behavior
- Returns if dgvDiary.CurrentRow Is Nothing
- Shows "Reschedule dialog would open."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately |
| 2 | Row selected | CurrentRow is valid | MessageBox "Reschedule dialog would open." |
