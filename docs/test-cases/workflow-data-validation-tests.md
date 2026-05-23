# Workflow Module - Data Validation Tests

## Module: WFL (Workflow)
## Test Type: Data Integrity and Validation Tests
## Data Sources:
- Task storage table [ASSUMPTION]
- Notification storage table [ASSUMPTION]
- Diary/Activity entries (Claims.Activities) [ASSUMPTION]

---

### Test Case ID: DV-WFL-001
**Component**: Task Create Parameters
**Source**: Admin.usp_Task_Create

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TaskType max length | VARCHAR(30) | Max 30 characters |
| 2 | TaskType required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 3 | EntityType valid values | 'POLICY', 'CLAIM', 'BILLING' | Known entity types |
| 4 | EntityType max length | VARCHAR(20) | Max 20 characters |
| 5 | EntityID positive | INT > 0 | References valid entity |
| 6 | Subject max length | VARCHAR(200) | Max 200 characters |
| 7 | Subject required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 8 | Description nullable | VARCHAR(MAX) or NULL | Both accepted |
| 9 | AssignedTo max length | VARCHAR(50) | Max 50 characters |
| 10 | AssignedTo required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 11 | DueDate nullable | DATETIME or NULL | Both accepted |
| 12 | DueDate format | Valid DATETIME | Proper date-time value |
| 13 | Priority valid values | 'HIGH', 'NORMAL', 'LOW' | Only these three accepted |
| 14 | Priority default | Not specified | Defaults to 'NORMAL' |
| 15 | Priority max length | VARCHAR(10) | Max 10 characters |
| 16 | CreatedBy max length | VARCHAR(50) | Max 50 characters |
| 17 | CreatedBy required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 18 | OUTPUT TaskID | INT | Returns generated ID |

---

### Test Case ID: DV-WFL-002
**Component**: Task GetByUser Parameters
**Source**: Admin.usp_Task_GetByUser

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Username max length | VARCHAR(50) | Max 50 characters |
| 2 | Username required | NOT NULL | Must specify user |
| 3 | Status nullable | VARCHAR(20) or NULL | NULL means all statuses |
| 4 | Module nullable | VARCHAR(30) or NULL | NULL means all modules |
| 5 | Priority nullable | VARCHAR(10) or NULL | NULL means all priorities |
| 6 | NULL filters return all tasks | All optional params NULL | No additional filtering |

---

### Test Case ID: DV-WFL-003
**Component**: Task Reassign Parameters
**Source**: Admin.usp_Task_Reassign

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TaskID is positive | INT > 0 | Valid task reference |
| 2 | NewAssignee max length | VARCHAR(50) | Max 50 characters |
| 3 | NewAssignee required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 4 | ReassignedBy max length | VARCHAR(50) | Max 50 characters |
| 5 | ReassignedBy required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 6 | Reason nullable | VARCHAR(200) or NULL | Both accepted |
| 7 | Reason max length | VARCHAR(200) | Max 200 characters |

---

### Test Case ID: DV-WFL-004
**Component**: Notification Create Parameters
**Source**: Admin.usp_Notification_Create

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | UserID positive | INT > 0 | Valid user reference |
| 2 | NotificationType max length | VARCHAR(30) | Max 30 characters |
| 3 | NotificationType required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 4 | Subject max length | VARCHAR(200) | Max 200 characters |
| 5 | Subject required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 6 | Message as VARCHAR(MAX) | Large text | Accepts very long messages |
| 7 | Message required | NOT NULL | Cannot be NULL [ASSUMPTION] |
| 8 | EntityType nullable | VARCHAR(20) or NULL | Optional entity link |
| 9 | EntityID nullable | INT or NULL | Optional with EntityType |
| 10 | EntityType and EntityID paired | Both NULL or both populated | Consistent pair [ASSUMPTION] |

---

### Test Case ID: DV-WFL-005
**Component**: Notification GetByUser Parameters
**Source**: Admin.usp_Notification_GetByUser

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | UserID positive | INT > 0 | Valid user reference |
| 2 | UnreadOnly is BIT | 0 or 1 | Boolean flag |
| 3 | UnreadOnly default | Not specified | Defaults to 0 (all notifications) |
| 4 | UnreadOnly = 0 | Explicit 0 | Returns all (read + unread) |
| 5 | UnreadOnly = 1 | Explicit 1 | Returns only unread (IsRead = 0) |

---

### Test Case ID: DV-WFL-006
**Component**: Task Status Consistency
**Source**: frmTaskQueue and stored procedures

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Status values consistent | UI filter vs DB values | PENDING, COMPLETED match across layers |
| 2 | Module values consistent | cboFilter items vs @Module param | POLICY, CLAIMS, UNDERWRITING, BILLING |
| 3 | Priority values consistent | cboPriority items vs @Priority param | HIGH, NORMAL, LOW |
| 4 | AssignedTo references valid user | @AssignedTo value | Exists in Admin.Users [ASSUMPTION] |
| 5 | CreatedBy references valid user | @CreatedBy value | Exists in Admin.Users [ASSUMPTION] |
| 6 | CompletedBy references valid user | @CompletedBy value | Exists in Admin.Users [ASSUMPTION] |
| 7 | Task cannot be completed twice | Status already COMPLETED | Prevented or idempotent [ASSUMPTION] |

---

### Test Case ID: DV-WFL-007
**Component**: Notification State Consistency
**Source**: frmNotificationCenter and stored procedures

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | IsRead flag values | BIT field | Only 0 (unread) or 1 (read) |
| 2 | UserID references valid user | @UserID value | Exists in Admin.Users |
| 3 | NotificationType known values | VARCHAR(30) | TASK_ASSIGNED, CLAIM_UPDATE, POLICY_EXPIRING, PAYMENT_DUE, SYSTEM [ASSUMPTION] |
| 4 | MarkRead is idempotent | Mark already-read notification | No error, no state change |
| 5 | MarkAllRead scoped correctly | @UserID filter | Only affects specified user's notifications |
| 6 | Notification ordering | CreatedDate | Newest first in display [ASSUMPTION] |

---

### Test Case ID: DV-WFL-008
**Component**: Diary Entry Consistency
**Source**: frmDiaryManager

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Status values consistent | cboStatus items | (All), PENDING, OVERDUE, COMPLETED |
| 2 | Date range valid | dtpDateFrom <= dtpDateTo | From date not after To date |
| 3 | Default date range | Form load | 7 days back to today |
| 4 | Diary linked to valid claim | EntityID reference | Claim exists in Claims.Claims [ASSUMPTION] |
| 5 | Completed diary has completion date | Status=COMPLETED | CompletedDate populated [ASSUMPTION] |
| 6 | Overdue determination | DueDate < today AND Status=PENDING | Correctly identified as overdue [ASSUMPTION] |
