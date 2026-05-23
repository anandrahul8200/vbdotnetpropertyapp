# Admin Module - Integration Tests (Future-State)

## Module: ADM (Admin)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: MT-ADM-001
**Integration**: Auth Service -> User Store
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Auth Component | User Store Component | Data Flow |
|---------------|---------------------|-----------|
| Login endpoint | Admin.Users | Username + PasswordHash lookup |
| Permission load | Admin.Permissions via RolePermissions | UserID -> Permissions list |
| Session management | GlobalState (in-memory) | UserID, Role, Permissions stored |
| Account lockout | Admin.Users.IsLocked | FailedLoginAttempts threshold |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Login resolves user and role | Auth service | User store | UserID, FullName, RoleName returned |
| 2 | Permissions loaded post-auth | Auth service | Permission store | List of PermissionCodes returned |
| 3 | Failed login updates counter | Auth service | User store | FailedLoginAttempts incremented |
| 4 | Account locked on threshold | Auth service | User store | IsLocked=1 after 5 failures |
| 5 | Successful login resets counter | Auth service | User store | FailedLoginAttempts = 0 |
| 6 | User store unavailable | Auth service | User store (down) | 503 Service Unavailable |

---

### Test Case ID: MT-ADM-002
**Integration**: Admin Service -> Audit Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Admin Component | Audit Component | Data Flow |
|----------------|----------------|-----------|
| User Create | Audit.AuditLog | Action=INSERT, TableName=Admin.Users |
| User Lock/Unlock | Audit.AuditLog | Action=UPDATE |
| Config Change | Audit.AuditLog | OldValue, NewValue recorded |
| Login event | Audit.AuditLog | Action=LOGIN, IP address |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | User creation logged | Admin service | Audit service | AuditLog entry with INSERT action |
| 2 | Config change logged with old/new values | Admin service | Audit service | FieldName, OldValue, NewValue populated |
| 3 | Login event logged | Auth service | Audit service | LOGIN action with IP |
| 4 | Audit service unavailable | Admin service | Audit service (down) | Admin operation succeeds, audit queued |
| 5 | Audit search pagination | UI service | Audit service | Paginated results with total count |

---

### Test Case ID: MT-ADM-003
**Integration**: Admin Service -> Notification Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Admin Component | Notification Component | Data Flow |
|----------------|----------------------|-----------|
| User Create | Notification_Create | Welcome notification to new user |
| Password Reset | Notification_Create | Reset confirmation to user |
| Account Locked | Notification_Create | Alert to admin users |
| Task Assignment | Notification_Create | Task notification to assignee |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | New user gets welcome notification | Admin service | Notification service | Notification created for new UserID |
| 2 | Password reset notification | Admin service | Notification service | NotificationType='PASSWORD_RESET' |
| 3 | Mark notification read | UI service | Notification service | IsRead flag updated |
| 4 | Mark all read | UI service | Notification service | All user notifications marked read |
| 5 | Get unread notifications | UI service | Notification service | Only unread items returned |

---

### Test Case ID: MT-ADM-004
**Integration**: Admin Service -> Error Logging Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Application Component | Error Service | Data Flow |
|----------------------|--------------|-----------|
| ErrorLogger.LogError | Audit.ErrorLog (DB) | Exception details stored |
| ErrorLogger.LogError | File system | Log file written |
| Any caught exception | ErrorLogger | Error context captured |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Application error logged to DB | Any service | Error logging service | Row in ErrorLog table |
| 2 | Application error logged to file | Any service | File system | Entry in daily log file |
| 3 | DB logging fails, file logging succeeds | Error service | Both targets | File entry exists, no DB entry, no crash |
| 4 | Both logging targets fail | Error service | Both (down) | Application continues (errors swallowed) |
| 5 | Error search returns recent errors | UI service | Error logging service | Filtered, paginated results |

---

### Test Case ID: MT-ADM-005
**Integration**: Lookup Service -> All Modules
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Lookup Component | Consumer Module | Data Flow |
|-----------------|----------------|-----------|
| usp_Lookup_GetByCategory | Policy, Claims, Billing forms | Dropdown values |
| usp_Lookup_GetStates | Policy entry, Property forms | State codes + rates |
| usp_Lookup_GetAgents | Policy entry | Agent selection list |
| usp_Lookup_GetCoverageTypes | Policy coverage tab | Coverage options |
| usp_Lookup_GetDeductibleOptions | Policy coverage tab | Deductible choices |
| usp_Lookup_GetPaymentPlans | Billing setup | Payment plan options |
| usp_Lookup_GetCatastrophes | Claims FNOL entry | CAT event selection |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Policy form loads state dropdown | Policy module | Lookup service | Active states with tax rates |
| 2 | Claims form loads catastrophes | Claims module | Lookup service | Active catastrophe events |
| 3 | Coverage types filtered by policy type | Policy module | Lookup service | Only matching coverage codes |
| 4 | Deductible options state-specific | Policy module | Lookup service | State-filtered options |
| 5 | Lookup service unavailable | Any module | Lookup service (down) | Graceful error, cached data [ASSUMPTION] |
| 6 | New lookup value available immediately | Admin creates lookup | Consumer module | New value appears in dropdown |

---

### Test Case ID: MT-ADM-006
**Integration**: Task Service -> All Modules
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Task Component | Consumer Module | Data Flow |
|---------------|----------------|-----------|
| usp_Task_Create | Policy, Claims, Underwriting | Task assigned to user |
| usp_Task_GetByUser | Dashboard, Task Queue | User task list |
| usp_Task_Complete | Any module | Task marked complete |
| usp_Task_Reassign | Admin, Supervisors | Task moved to new user |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claim referral creates task | Claims module | Task service | Task for assigned adjuster |
| 2 | Policy renewal creates task | Batch job | Task service | Task for agent |
| 3 | Task completion | Any module | Task service | Status updated, CompletedDate set |
| 4 | Task reassignment | Admin service | Task service | New assignee, reason logged |
| 5 | User dashboard shows pending tasks | UI service | Task service | Filtered by username and status |
