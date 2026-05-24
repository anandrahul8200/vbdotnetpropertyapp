# Workflow Module - Integration Tests (Future-State)

## Module: WFL (Workflow)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: INT-WFL-001
**Integration**: Task Service -> Entity Services (Policy/Claims/Billing)
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Task Component | Entity Component | Data Flow |
|---------------|-----------------|-----------|
| Task Create (EntityType=POLICY) | Policy service | Validates policy exists |
| Task Create (EntityType=CLAIM) | Claims service | Validates claim exists |
| Task Open | Policy/Claims service | Navigates to entity detail |
| Task Context Display | Policy/Claims service | Entity summary for task list |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Task created references valid entity | Task service | Policy/Claims service | EntityID validated before task creation |
| 2 | Open task navigates to entity | Task service | Policy/Claims service | Entity detail page opened |
| 3 | Entity context shown in task list | Task service | Policy/Claims service | PolicyNumber or ClaimNumber shown with task |
| 4 | Entity service unavailable | Task service | Entity service (down) | Task still created, entity context unavailable |
| 5 | Entity deleted with pending tasks | Entity service | Task service | Tasks orphaned or auto-closed [ASSUMPTION] |

---

### Test Case ID: INT-WFL-002
**Integration**: Task Service -> Notification Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Task Component | Notification Component | Data Flow |
|---------------|----------------------|-----------|
| Task Create | Notification_Create | Notify assignee of new task |
| Task Reassign | Notification_Create | Notify new assignee |
| Task Overdue | Notification_Create | Alert assignee of overdue |
| Task Complete | Notification_Create | Notify creator of completion [ASSUMPTION] |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | New task triggers notification | Task service | Notification service | Assignee receives notification |
| 2 | Reassignment notifies new assignee | Task service | Notification service | New assignee notified, old assignee optionally notified |
| 3 | Overdue task generates alert | Scheduler | Notification service | Daily check creates overdue notifications |
| 4 | Notification service unavailable | Task service | Notification service (down) | Task created, notification queued for retry |
| 5 | Bulk task creation | Task service | Notification service | One notification per task, no flooding |

---

### Test Case ID: INT-WFL-003
**Integration**: Notification Service -> User Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Notification Component | User Component | Data Flow |
|-----------------------|---------------|-----------|
| Notification Create | User validation | UserID must exist |
| GetByUser | User authentication | Only own notifications |
| MarkRead/MarkAllRead | User authentication | Scoped to authenticated user |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Notification to valid user | Notification service | User service | UserID validated, notification created |
| 2 | Notification to invalid user | Notification service | User service | Error or notification discarded |
| 3 | User retrieves own notifications | UI service | Notification service | Only authenticated user's notifications returned |
| 4 | Cross-user notification access | UI service | Notification service | 403 Forbidden |
| 5 | User service unavailable | Notification service | User service (down) | Notification created, delivery deferred |

---

### Test Case ID: INT-WFL-004
**Integration**: Diary Service -> Claims Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Diary Component | Claims Component | Data Flow |
|----------------|-----------------|-----------|
| Diary entries | Claims.Activities | Diary entries mapped to claim activities |
| Diary search | Claims.Claims | Claims context for diary display |
| Diary complete | Claims.Activities | Activity status updated |
| Diary reschedule | Claims.Activities | New follow-up date set |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Diary entries linked to claims | Diary service | Claims service | Claim number and status shown |
| 2 | Complete diary updates claim activity | Diary service | Claims service | Activity marked complete |
| 3 | Reschedule creates new diary date | Diary service | Claims service | Follow-up date updated |
| 4 | Claims service unavailable | Diary service | Claims service (down) | Diary entries shown without claim context |
| 5 | Claim closed with open diary entries | Claims service | Diary service | Diary entries auto-completed [ASSUMPTION] |

---

### Test Case ID: INT-WFL-005
**Integration**: Task Service -> Audit Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Task Component | Audit Component | Data Flow |
|---------------|----------------|-----------|
| Task Create | Audit.AuditLog | Task creation logged |
| Task Complete | Audit.AuditLog | Completion logged with user |
| Task Reassign | Audit.AuditLog | Reassignment logged with reason |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Task creation audited | Task service | Audit service | AuditLog entry: Action=CREATE, Entity=Task |
| 2 | Task completion audited | Task service | Audit service | CompletedBy and CompletedDate recorded |
| 3 | Reassignment audited with reason | Task service | Audit service | OldAssignee, NewAssignee, Reason logged |
| 4 | Audit service unavailable | Task service | Audit service (down) | Task operation succeeds, audit queued |

---

### Test Case ID: INT-WFL-006
**Integration**: Workflow Service -> Email/Push Notification Channel
**Priority**: Low
**Status**: FUTURE-STATE

#### Integration Points
| Workflow Component | Channel Component | Data Flow |
|-------------------|------------------|-----------|
| High-priority notification | Email service | Email sent for urgent items |
| Task overdue alert | Email service | Daily digest email |
| Real-time notification | WebSocket/Push | Instant in-app notification |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | High priority task sends email | Task service | Email service | Assignee receives email notification |
| 2 | Daily overdue digest | Scheduler | Email service | Summary of overdue tasks emailed |
| 3 | Real-time push notification | Notification service | WebSocket | Instant notification in browser [ASSUMPTION] |
| 4 | Email service unavailable | Task service | Email service (down) | In-app notification still works, email queued |
| 5 | User email preferences | Notification service | Email service | Respects user opt-out settings [ASSUMPTION] |
