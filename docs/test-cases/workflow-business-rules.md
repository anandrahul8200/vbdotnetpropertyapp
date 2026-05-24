# Workflow Module - Business Rules

## Module: WFL (Workflow)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-WFL-001
**Module**: WFL
**Priority**: High

#### Rule Description
Task priority defaults to 'NORMAL' when not explicitly specified. Valid priority levels are HIGH, NORMAL, and LOW. Priority determines display order in the task queue and may trigger additional notifications for HIGH priority items.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Admin.usp_Task_Create
- **Parameter**: @Priority VARCHAR(10) = 'NORMAL'

#### Enforcement Mechanism
- Type: Database SP default parameter value
- Behavior: If @Priority not provided, defaults to 'NORMAL'

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-001 | usp_Task_Create default priority |
| US-WFL-009 | Create Tasks user story |
| DV-WFL-001 | Task Create parameter validation |

---

### Rule ID: BR-WFL-002
**Module**: WFL
**Priority**: High

#### Rule Description
Notifications are scoped to a specific user via UserID. A user can only view and manage their own notifications. The MarkAllRead operation only affects the specified user's unread notifications, not all notifications in the system.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Admin.usp_Notification_MarkAllRead
- **Parameter**: @UserID INT

#### Enforcement Mechanism
- Type: Database SP parameter scoping
- Behavior: All operations filter by @UserID

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-008 | usp_Notification_MarkAllRead |
| SP-WFL-006 | usp_Notification_GetByUser |
| NFR-WFL-006 | Notification Security |

---

### Rule ID: BR-WFL-003
**Module**: WFL
**Priority**: High

#### Rule Description
Task actions (Complete, Reassign, Open) require a task to be selected in the grid. The system checks if CurrentRow Is Nothing before proceeding with any operation. This prevents null reference errors and ensures intentional action.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`
- **Methods**: btnComplete_Click, btnReassign_Click, btnOpen_Click
- **Code Snippet**:
```vb
If dgvTasks.CurrentRow Is Nothing Then Return
```

#### Enforcement Mechanism
- Type: Application-level validation (null check)
- Behavior: Returns immediately without action if no row selected

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-WFL-002 | btnComplete_Click |
| UT-WFL-003 | btnReassign_Click |
| UT-WFL-004 | btnOpen_Click |
| UI-WFL-001 | frmTaskQueue controls |

---

### Rule ID: BR-WFL-004
**Module**: WFL
**Priority**: High

#### Rule Description
Diary/notification actions (Mark Read, Delete, Complete, Reschedule) require a row to be selected. The system checks CurrentRow before proceeding.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`
- **Code Snippet (Notification)**:
```vb
If dgvNotifications.CurrentRow IsNot Nothing Then MessageBox.Show("Marked as read.", ...)
```
- **Code Snippet (Diary)**:
```vb
If dgvDiary.CurrentRow Is Nothing Then Return
```

#### Enforcement Mechanism
- Type: Application-level validation (null check)
- Behavior: Actions silently do nothing without selection

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-WFL-005 | btnMarkRead_Click |
| UT-WFL-007 | btnDelete_Click |
| UT-WFL-010 | Diary btnComplete_Click |
| UT-WFL-011 | Diary btnReschedule_Click |

---

### Rule ID: BR-WFL-005
**Module**: WFL
**Priority**: Medium

#### Rule Description
Mark All Read operates without requiring a row selection. Unlike individual Mark Read which needs a specific notification selected, Mark All Read applies to all unread notifications for the current user in a single operation.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb`
- **Method**: btnMarkAllRead_Click
- **Code Snippet**:
```vb
Private Sub btnMarkAllRead_Click(sender As Object, e As EventArgs) Handles btnMarkAllRead.Click
    MessageBox.Show("All notifications marked as read.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
End Sub
```

#### Enforcement Mechanism
- Type: Application-level behavior (no selection check)
- Behavior: Always executes regardless of grid selection state

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-WFL-006 | btnMarkAllRead_Click |
| FT-WFL-007 | Mark All Notifications as Read |
| SP-WFL-008 | usp_Notification_MarkAllRead |

---

### Rule ID: BR-WFL-006
**Module**: WFL
**Priority**: Medium

#### Rule Description
Task reassignment requires a new assignee and the identity of who performed the reassignment. An optional reason can be provided. The reason field provides audit context for why the reassignment occurred.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Admin.usp_Task_Reassign
- **Parameters**: @TaskID INT, @NewAssignee VARCHAR(50), @ReassignedBy VARCHAR(50), @Reason VARCHAR(200) = NULL

#### Enforcement Mechanism
- Type: Database SP parameter requirements
- Behavior: @NewAssignee and @ReassignedBy required; @Reason optional with default NULL

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-004 | usp_Task_Reassign |
| US-WFL-003 | Reassign a Task user story |
| DV-WFL-003 | Reassign parameter validation |

---

### Rule ID: BR-WFL-007
**Module**: WFL
**Priority**: Medium

#### Rule Description
The diary manager defaults to showing entries from the last 7 days. This provides a practical default view without overwhelming the user with historical entries. Users can adjust the date range as needed.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb`
- **Method**: InitializeControls
- **Code Snippet**:
```vb
dtpDateFrom.Value = DateTime.Today.AddDays(-7)
```

#### Enforcement Mechanism
- Type: UI default value
- Behavior: Date From defaults to 7 days ago; user can change

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UI-WFL-003 | frmDiaryManager controls |
| FT-WFL-009 | Diary Manager Search |
| US-WFL-008 | Manage Diary Entries user story |

---

### Rule ID: BR-WFL-008
**Module**: WFL
**Priority**: Medium

#### Rule Description
Task queue filters use "(All)" as the default selection meaning no filter is applied. When Module or Priority is set to "(All)" (SelectedIndex = 0), the corresponding SP parameter receives NULL, which the stored procedure interprets as "return all records regardless of this field."

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb`
- **Method**: InitializeControls
- **Code Snippet**:
```vb
cboFilter.Items.AddRange({"(All)", "POLICY", "CLAIMS", "UNDERWRITING", "BILLING"}) : cboFilter.SelectedIndex = 0
cboPriority.Items.AddRange({"(All)", "HIGH", "NORMAL", "LOW"}) : cboPriority.SelectedIndex = 0
```

#### Enforcement Mechanism
- Type: UI convention (index 0 = no filter) + SP NULL parameter handling
- Behavior: "(All)" translates to NULL parameter value in SP call [ASSUMPTION]

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UI-WFL-001 | frmTaskQueue controls |
| SP-WFL-002 | usp_Task_GetByUser filter parameters |
| DV-WFL-006 | Task Status Consistency |

---

### Rule ID: BR-WFL-009
**Module**: WFL
**Priority**: Low

#### Rule Description
Notification retrieval supports an UnreadOnly filter. When @UnreadOnly=0 (default), all notifications are returned. When @UnreadOnly=1, only unread notifications (IsRead=0) are returned. This enables efficient inbox views.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Admin.usp_Notification_GetByUser
- **Parameter**: @UnreadOnly BIT = 0

#### Enforcement Mechanism
- Type: Database SP parameter with BIT flag
- Behavior: 0 = all, 1 = unread only

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-WFL-006 | usp_Notification_GetByUser |
| DV-WFL-005 | Notification GetByUser parameters |
| US-WFL-005 | View Notifications user story |
