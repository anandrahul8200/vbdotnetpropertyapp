# Workflow Module - Non-Functional Requirements Tests

## Module: WFL (Workflow)
## Test Type: Performance, Security, and Reliability Tests

---

### Test Case ID: NFR-WFL-001
**Category**: Performance
**Component**: Task Queue (Admin.usp_Task_GetByUser)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 100 tasks) | < 500ms | Query for user's pending tasks |
| Response time (> 1000 tasks) | < 2 seconds | Large task queue |
| With filters | < 500ms | Filtered queries should be faster |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | User with few tasks | 20 tasks assigned | < 200ms |
| 2 | User with many tasks | 500 tasks assigned | < 1s |
| 3 | Filtered by status | 500 tasks, filter PENDING | < 500ms |
| 4 | Filtered by priority | 500 tasks, filter HIGH | < 500ms |
| 5 | Combined module + priority filter | Large dataset | < 500ms |

---

### Test Case ID: NFR-WFL-002
**Category**: Performance
**Component**: Notification Retrieval (Admin.usp_Notification_GetByUser)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 50 notifications) | < 300ms | User's notifications |
| Response time (> 500 notifications) | < 1 second | Heavy notification user |
| Unread-only filter | < 200ms | Should use index on IsRead |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Few notifications | 10 notifications | < 100ms |
| 2 | Many notifications, all | 500 notifications | < 1s |
| 3 | Many notifications, unread only | 500 total, 50 unread, @UnreadOnly=1 | < 200ms |
| 4 | Mark all read (batch update) | 100 unread notifications | < 500ms |

---

### Test Case ID: NFR-WFL-003
**Category**: Performance
**Component**: Task Creation (Admin.usp_Task_Create)
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Single task creation | < 200ms | INSERT operation |
| Burst creation | < 1 second for 10 tasks | Multiple rapid inserts |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Single task creation | One task | < 200ms |
| 2 | Rapid sequential creation | 10 tasks in loop | < 1s total |
| 3 | With large description | VARCHAR(MAX) body | < 300ms |
| 4 | Concurrent creation | 5 users creating tasks | No deadlocks |

---

### Test Case ID: NFR-WFL-004
**Category**: Performance
**Component**: Diary Manager Search
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Search with date range | < 1 second | 7-day default range |
| Search with all filters | < 1 second | Date + status filter |
| Large diary volume | < 3 seconds | 10000 diary entries |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Default 7-day search | 500 entries in range | < 500ms |
| 2 | Status filter OVERDUE | Large dataset | < 500ms |
| 3 | Wide date range (1 year) | 10000 entries | < 2s |
| 4 | No results | Empty date range | < 100ms |

---

### Test Case ID: NFR-WFL-005
**Category**: Security
**Component**: Task Access Control
**Priority**: High

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | User can only see own tasks | @Username filter matches authenticated user |
| 2 | Task reassignment authorization | Only managers can reassign [ASSUMPTION] |
| 3 | Task completion audit trail | CompletedBy recorded for accountability |
| 4 | SQL injection in username | Parameterized query prevents injection |
| 5 | SQL injection in task subject | Parameterized query prevents injection |
| 6 | Cross-user task access prevented | Cannot complete another user's task without reassignment [ASSUMPTION] |

---

### Test Case ID: NFR-WFL-006
**Category**: Security
**Component**: Notification Security
**Priority**: Medium

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | User can only see own notifications | @UserID matches authenticated user |
| 2 | Cannot mark other user's notifications | UserID validated against session |
| 3 | Notification content sanitized | No XSS in notification message [ASSUMPTION] |
| 4 | SQL injection in message | VARCHAR(MAX) parameter prevents injection |
| 5 | MarkAllRead scoped to user | Only affects @UserID's notifications |

---

### Test Case ID: NFR-WFL-007
**Category**: Reliability
**Component**: Task Workflow Resilience
**Priority**: High

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Task creation during DB outage | Error caught, user notified |
| 2 | Task completion partial failure | Transaction ensures atomicity [ASSUMPTION] |
| 3 | Concurrent task completion | Same task completed by two users | Only one succeeds [ASSUMPTION] |
| 4 | Reassignment during completion | Race condition handled gracefully [ASSUMPTION] |
| 5 | Notification delivery guaranteed | Notifications persist even on UI failure |

---

### Test Case ID: NFR-WFL-008
**Category**: Reliability
**Component**: Notification Delivery
**Priority**: Medium

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Notification creation failure | Error logged, source operation not blocked [ASSUMPTION] |
| 2 | Mark read idempotent | Marking already-read notification causes no error |
| 3 | Mark all read with concurrent new | New notifications after mark-all stay unread [ASSUMPTION] |
| 4 | Large batch mark-all | 1000 notifications | Completes without timeout |

---

### Test Case ID: NFR-WFL-009
**Category**: Scalability
**Component**: Workflow Volume
**Priority**: Medium

#### Scalability Tests
| # | Test | Dataset | Expected |
|---|------|---------|----------|
| 1 | System with many users | 500 active users, each with tasks | Individual queries still fast |
| 2 | High task creation rate | 1000 tasks/hour | No performance degradation |
| 3 | Notification accumulation | 100000 notifications total | GetByUser still < 1s with UserID index |
| 4 | Diary entries over time | 50000 diary entries | Search still performs with date filter |
