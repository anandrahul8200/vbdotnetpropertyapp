# Workflow Module - API Tests (Future-State)

## Module: WFL (Workflow)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-WFL-001
**Legacy SP**: Admin.usp_Task_Create
**Future Endpoint**: POST /api/tasks
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Admin.usp_Task_Create | POST /api/tasks |
| Admin.usp_Task_GetByUser | GET /api/tasks?status=&module=&priority= |
| Admin.usp_Task_Complete | POST /api/tasks/{id}/complete |
| Admin.usp_Task_Reassign | POST /api/tasks/{id}/reassign |
| Admin.usp_Notification_Create | POST /api/notifications |
| Admin.usp_Notification_GetByUser | GET /api/notifications?unreadOnly= |
| Admin.usp_Notification_MarkRead | PUT /api/notifications/{id}/read |
| Admin.usp_Notification_MarkAllRead | PUT /api/notifications/read-all |

#### Request/Response Specification
**Request (POST /api/tasks):**
```json
{
  "taskType": "REVIEW",
  "entityType": "CLAIM",
  "entityId": 100,
  "subject": "Review claim documentation",
  "description": "Please review all uploaded documents for completeness",
  "assignedTo": "adjuster1",
  "dueDate": "2025-07-15T17:00:00Z",
  "priority": "HIGH"
}
```

**Response (201 Created):**
```json
{
  "taskId": 42,
  "taskType": "REVIEW",
  "entityType": "CLAIM",
  "entityId": 100,
  "subject": "Review claim documentation",
  "assignedTo": "adjuster1",
  "dueDate": "2025-07-15T17:00:00Z",
  "priority": "HIGH",
  "status": "PENDING",
  "createdBy": "manager1",
  "createdDate": "2025-06-30T14:30:00Z"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Missing taskType | 400 Bad Request: "taskType is required" |
| 2 | Missing subject | 400 Bad Request: "subject is required" |
| 3 | Missing assignedTo | 400 Bad Request: "assignedTo is required" |
| 4 | Invalid priority | 400 Bad Request: "priority must be HIGH, NORMAL, or LOW" |
| 5 | Invalid entityType | 400 Bad Request: "entityType not recognized" |
| 6 | Unauthorized | 401 Unauthorized |
| 7 | Non-existent assignee | 400 Bad Request: "assignedTo user not found" |

---

### Test Case ID: API-WFL-002
**Legacy SP**: Admin.usp_Task_GetByUser
**Future Endpoint**: GET /api/tasks
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/tasks?status=PENDING&module=CLAIMS&priority=HIGH
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "tasks": [
    {
      "taskId": 42,
      "taskType": "REVIEW",
      "entityType": "CLAIM",
      "entityId": 100,
      "subject": "Review claim documentation",
      "assignedTo": "adjuster1",
      "dueDate": "2025-07-15T17:00:00Z",
      "priority": "HIGH",
      "status": "PENDING",
      "createdDate": "2025-06-30T14:30:00Z"
    }
  ],
  "totalCount": 1
}
```

---

### Test Case ID: API-WFL-003
**Legacy SP**: Admin.usp_Task_Complete
**Future Endpoint**: POST /api/tasks/{id}/complete
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
POST /api/tasks/42/complete
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "taskId": 42,
  "status": "COMPLETED",
  "completedBy": "adjuster1",
  "completedDate": "2025-07-10T16:45:00Z"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Complete non-existent task | 404 Not Found |
| 2 | Complete already-completed task | 409 Conflict: "Task already completed" |
| 3 | Unauthorized completion | 401 Unauthorized |
| 4 | User not assigned to task | 403 Forbidden (unless manager) |

---

### Test Case ID: API-WFL-004
**Legacy SP**: Admin.usp_Task_Reassign
**Future Endpoint**: POST /api/tasks/{id}/reassign
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```json
{
  "newAssignee": "adjuster2",
  "reason": "Workload balancing"
}
```

**Response (200 OK):**
```json
{
  "taskId": 42,
  "previousAssignee": "adjuster1",
  "newAssignee": "adjuster2",
  "reassignedBy": "manager1",
  "reason": "Workload balancing",
  "reassignedDate": "2025-07-05T10:00:00Z"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Missing newAssignee | 400 Bad Request: "newAssignee is required" |
| 2 | Non-existent new assignee | 400 Bad Request: "newAssignee user not found" |
| 3 | Reassign completed task | 409 Conflict: "Cannot reassign completed task" |
| 4 | Non-manager reassignment | 403 Forbidden |

---

### Test Case ID: API-WFL-005
**Legacy SP**: Admin.usp_Notification_Create
**Future Endpoint**: POST /api/notifications
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/notifications):**
```json
{
  "userId": 1,
  "notificationType": "TASK_ASSIGNED",
  "subject": "New task assigned",
  "message": "A claim review task has been assigned to you",
  "entityType": "CLAIM",
  "entityId": 100
}
```

**Response (201 Created):**
```json
{
  "notificationId": 200,
  "userId": 1,
  "notificationType": "TASK_ASSIGNED",
  "subject": "New task assigned",
  "isRead": false,
  "createdDate": "2025-06-30T14:30:00Z"
}
```

---

### Test Case ID: API-WFL-006
**Legacy SP**: Admin.usp_Notification_GetByUser
**Future Endpoint**: GET /api/notifications
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/notifications?unreadOnly=true
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "notifications": [
    {
      "notificationId": 200,
      "notificationType": "TASK_ASSIGNED",
      "subject": "New task assigned",
      "message": "A claim review task has been assigned to you",
      "entityType": "CLAIM",
      "entityId": 100,
      "isRead": false,
      "createdDate": "2025-06-30T14:30:00Z"
    }
  ],
  "unreadCount": 5,
  "totalCount": 25
}
```

---

### Test Case ID: API-WFL-007
**Legacy SP**: Admin.usp_Notification_MarkRead
**Future Endpoint**: PUT /api/notifications/{id}/read
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
PUT /api/notifications/200/read
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "notificationId": 200,
  "isRead": true,
  "readDate": "2025-06-30T15:00:00Z"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Mark non-existent notification | 404 Not Found |
| 2 | Mark other user's notification | 403 Forbidden |
| 3 | Mark already-read | 200 OK (idempotent) |

---

### Test Case ID: API-WFL-008
**Legacy SP**: Admin.usp_Notification_MarkAllRead
**Future Endpoint**: PUT /api/notifications/read-all
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
PUT /api/notifications/read-all
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "markedCount": 5,
  "readDate": "2025-06-30T15:00:00Z"
}
```

#### Common API Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | No auth token | 401 Unauthorized |
| 2 | Expired token | 401 Unauthorized |
| 3 | User without workflow permissions | 403 Forbidden |
| 4 | Invalid JSON body | 400 Bad Request |
| 5 | Rate limiting | 429 Too Many Requests after threshold |
