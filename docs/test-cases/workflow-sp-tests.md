# Workflow Module - Stored Procedure Tests

## Module: WFL (Workflow)
## Source Files:
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (8 total):
1. Admin.usp_Task_Create
2. Admin.usp_Task_GetByUser
3. Admin.usp_Task_Complete
4. Admin.usp_Task_Reassign
5. Admin.usp_Notification_Create
6. Admin.usp_Notification_GetByUser
7. Admin.usp_Notification_MarkRead
8. Admin.usp_Notification_MarkAllRead

---

### Test Case ID: SP-WFL-001
**Procedure**: Admin.usp_Task_Create
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TaskType VARCHAR(30) - Required
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required
- @Subject VARCHAR(200) - Required
- @Description VARCHAR(MAX) - Optional (default NULL)
- @AssignedTo VARCHAR(50) - Required
- @DueDate DATETIME - Optional (default NULL)
- @Priority VARCHAR(10) - Optional (default 'NORMAL')
- @CreatedBy VARCHAR(50) - Required
- @TaskID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create task with all fields | @TaskType='REVIEW', @EntityType='CLAIM', @EntityID=100, @Subject='Review claim', @Description='Detailed review', @AssignedTo='adjuster1', @DueDate='2025-07-15', @Priority='HIGH', @CreatedBy='manager1' | @TaskID > 0, PRINT confirms creation |
| 2 | Create task with minimal fields | @TaskType='FOLLOWUP', @EntityType='POLICY', @EntityID=1, @Subject='Follow up', @AssignedTo='agent1', @CreatedBy='system' | @TaskID > 0, defaults applied |
| 3 | Default priority | @Priority not specified | Defaults to 'NORMAL' |
| 4 | NULL due date | @DueDate=NULL | No due date set |
| 5 | NULL description | @Description=NULL | Accepted, description is optional |
| 6 | Various task types | @TaskType='REVIEW'/'APPROVAL'/'FOLLOWUP'/'INSPECTION' | All types accepted |
| 7 | Various entity types | @EntityType='POLICY'/'CLAIM'/'BILLING' | All entity references accepted |
| 8 | Priority levels | @Priority='HIGH'/'NORMAL'/'LOW' | All levels accepted |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL Subject | @Subject=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 2 | NULL AssignedTo | @AssignedTo=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 3 | NULL CreatedBy | @CreatedBy=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 4 | Invalid AssignedTo user | @AssignedTo='nonexistent' | Task created but may not appear in user's queue [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Subject at max length (200 chars) | 200 character subject | Stored correctly |
| 2 | TaskType at max length (30 chars) | 30 character type | Stored correctly |
| 3 | Description as VARCHAR(MAX) | Very large description | Stored correctly |
| 4 | DueDate in the past | @DueDate='2020-01-01' | Accepted (overdue immediately) |
| 5 | OUTPUT @TaskID initialized | @TaskID declared | Set to 0 per SP code |

---

### Test Case ID: SP-WFL-002
**Procedure**: Admin.usp_Task_GetByUser
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @Username VARCHAR(50) - Required
- @Status VARCHAR(20) - Optional (default NULL)
- @Module VARCHAR(30) - Optional (default NULL)
- @Priority VARCHAR(10) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all tasks for user | @Username='adjuster1', all others NULL | All tasks assigned to user |
| 2 | Filter by status | @Username='adjuster1', @Status='PENDING' | Only pending tasks |
| 3 | Filter by module | @Username='adjuster1', @Module='CLAIMS' | Only claims module tasks |
| 4 | Filter by priority | @Username='adjuster1', @Priority='HIGH' | Only high priority tasks |
| 5 | Multiple filters | @Username='adjuster1', @Status='PENDING', @Priority='HIGH' | Combined filters applied |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent user | @Username='nonexistent' | Empty result set |
| 2 | Invalid status filter | @Status='INVALID' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | User with no tasks | Valid user, no assigned tasks | Empty result set (WHERE 1=0 in stub) |
| 2 | NULL filters return all | @Status=NULL, @Module=NULL, @Priority=NULL | No filtering applied |

---

### Test Case ID: SP-WFL-003
**Procedure**: Admin.usp_Task_Complete
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TaskID INT - Required
- @CompletedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Complete existing task | @TaskID=1, @CompletedBy='adjuster1' | PRINT confirms completion |
| 2 | Completion timestamp | Valid completion | CompletedDate set to GETDATE() [ASSUMPTION] |
| 3 | Status updated | Valid completion | Status changed to 'COMPLETED' [ASSUMPTION] |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent task | @TaskID=99999 | PRINT still executes (no validation in stub) |
| 2 | Already completed task | @TaskID of completed task | May update again [ASSUMPTION] |
| 3 | NULL CompletedBy | @CompletedBy=NULL | Error [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | CompletedBy max length (50 chars) | 50 character username | Stored correctly |

---

### Test Case ID: SP-WFL-004
**Procedure**: Admin.usp_Task_Reassign
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TaskID INT - Required
- @NewAssignee VARCHAR(50) - Required
- @ReassignedBy VARCHAR(50) - Required
- @Reason VARCHAR(200) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Reassign task | @TaskID=1, @NewAssignee='adjuster2', @ReassignedBy='manager1' | PRINT confirms "Task reassigned to: adjuster2" |
| 2 | Reassign with reason | @TaskID=1, @NewAssignee='adjuster2', @ReassignedBy='manager1', @Reason='Workload balancing' | Reason stored |
| 3 | Reassign without reason | @Reason=NULL | Accepted, reason optional |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent task | @TaskID=99999 | PRINT still executes |
| 2 | NULL NewAssignee | @NewAssignee=NULL | Error [ASSUMPTION] |
| 3 | Reassign to same user | @NewAssignee=current assignee | Allowed (no validation) [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | NewAssignee max length (50 chars) | 50 character username | Stored correctly |
| 2 | Reason max length (200 chars) | 200 character reason | Stored correctly |
| 3 | Reason is NULL | @Reason=NULL | Optional, no error |

---

### Test Case ID: SP-WFL-005
**Procedure**: Admin.usp_Notification_Create
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @NotificationType VARCHAR(30) - Required
- @Subject VARCHAR(200) - Required
- @Message VARCHAR(MAX) - Required
- @EntityType VARCHAR(20) - Optional (default NULL)
- @EntityID INT - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create notification with entity | @UserID=1, @NotificationType='TASK_ASSIGNED', @Subject='New task', @Message='A task has been assigned to you', @EntityType='CLAIM', @EntityID=100 | PRINT confirms creation |
| 2 | Create notification without entity | @UserID=1, @NotificationType='SYSTEM', @Subject='System update', @Message='Maintenance scheduled', @EntityType=NULL, @EntityID=NULL | Notification created without entity link |
| 3 | Various notification types | TASK_ASSIGNED, CLAIM_UPDATE, POLICY_EXPIRING, PAYMENT_DUE, SYSTEM | All types accepted [ASSUMPTION] |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent UserID | @UserID=99999 | Notification created but no recipient [ASSUMPTION] |
| 2 | NULL Subject | @Subject=NULL | Error [ASSUMPTION] |
| 3 | NULL Message | @Message=NULL | Error [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Subject max length (200 chars) | 200 character subject | Stored correctly |
| 2 | Message as VARCHAR(MAX) | Very large message | Stored correctly |
| 3 | NotificationType max length (30 chars) | 30 character type | Stored correctly |
| 4 | EntityType and EntityID both NULL | No entity link | Valid notification without entity context |

---

### Test Case ID: SP-WFL-006
**Procedure**: Admin.usp_Notification_GetByUser
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @UnreadOnly BIT - Optional (default 0)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all notifications | @UserID=1, @UnreadOnly=0 | All notifications for user (read and unread) |
| 2 | Get unread only | @UserID=1, @UnreadOnly=1 | Only unread notifications |
| 3 | Default includes all | @UnreadOnly not specified | Returns all (default 0) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent user | @UserID=99999 | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | User with no notifications | Valid user, no notifications | Empty result set (WHERE 1=0 in stub) |
| 2 | @UnreadOnly=0 means all | Explicit 0 value | Both read and unread returned |
| 3 | @UnreadOnly=1 means unread | Explicit 1 value | Only IsRead=0 notifications |

---

### Test Case ID: SP-WFL-007
**Procedure**: Admin.usp_Notification_MarkRead
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @NotificationID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Mark notification as read | @NotificationID=1 | PRINT confirms "Notification marked read: 1" |
| 2 | IsRead flag updated | Valid ID | IsRead = 1, ReadDate = GETDATE() [ASSUMPTION] |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent notification | @NotificationID=99999 | PRINT still executes |
| 2 | Already read notification | @NotificationID of read notification | No error, idempotent |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Mark same notification twice | @NotificationID=1 twice | Idempotent, no error |

---

### Test Case ID: SP-WFL-008
**Procedure**: Admin.usp_Notification_MarkAllRead
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Mark all notifications read | @UserID=1 | PRINT confirms "All notifications marked read for user: 1" |
| 2 | All unread become read | User with 10 unread | All 10 marked as read [ASSUMPTION] |
| 3 | Already-read not affected | Mix of read/unread | Only unread ones updated [ASSUMPTION] |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | User with no notifications | @UserID=99999 | PRINT still executes, no rows affected |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | User with no unread | All already read | No rows updated, no error |
| 2 | User with many unread | 1000 unread notifications | All marked in single operation |
