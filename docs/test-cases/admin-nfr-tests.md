# Admin Module - Non-Functional Requirements Tests

## Module: ADM (Admin)
## Test Type: Performance, Security, and Reliability Tests

---

### Test Case ID: NFR-ADM-001
**Category**: Performance
**Component**: Global Search (usp_Search_Global)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 1000 records) | < 2 seconds | Time from execution to results returned |
| Response time (> 10000 records) | < 5 seconds | Time from execution to results returned |
| Concurrent searches | 10 simultaneous | No deadlocks or timeouts |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Single term, few results | 100 customers, search='Smith' | < 500ms |
| 2 | Single term, many results | 10000 customers, search='a' | < 2s |
| 3 | Cross-entity search | All entity tables populated | < 3s |
| 4 | LIKE pattern performance | Wildcard search '%term%' | Index scan acceptable |

---

### Test Case ID: NFR-ADM-002
**Category**: Performance
**Component**: Audit Log Search (usp_Audit_Search)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (30-day range) | < 3 seconds | Paginated query with filters |
| Response time (large dataset, 1M rows) | < 5 seconds | With pagination (100 per page) |
| Count query | < 2 seconds | @TotalRecords calculation |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Default search (30 days) | 50000 audit entries | < 2s |
| 2 | Filtered by table and user | Large dataset | < 1s with index |
| 3 | Pagination (page 50 of 100) | 500000 entries | < 3s |
| 4 | Date range spanning 1 year | Large dataset | < 5s |

---

### Test Case ID: NFR-ADM-003
**Category**: Performance
**Component**: User List (usp_User_List)
**Priority**: Medium

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small user base | 50 users | < 200ms |
| 2 | Large user base | 5000 users | < 1s |
| 3 | LEFT JOIN performance | Users with/without roles | No full table scan |

---

### Test Case ID: NFR-ADM-004
**Category**: Security
**Component**: Authentication (usp_User_Authenticate)
**Priority**: Critical

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Password hash never logged in plain text | Audit log contains no password data |
| 2 | Account lockout after 5 failures | IsLocked=1 after 5 failed attempts |
| 3 | Locked account cannot authenticate | Even with correct password |
| 4 | IP address logged on login | AuditLog.AdditionalInfo contains IP |
| 5 | SQL injection in username | Parameterized query prevents injection |
| 6 | SQL injection in password | Parameterized query prevents injection |
| 7 | Session timeout enforced | 30-minute inactivity timeout |
| 8 | Password not stored in plain text | Only SHA-256 hash with salt stored |

---

### Test Case ID: NFR-ADM-005
**Category**: Security
**Component**: Password Complexity (SecurityHelper.ValidatePasswordComplexity)
**Priority**: Critical

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Minimum 8 characters enforced | Passwords < 8 rejected |
| 2 | Uppercase required | No uppercase = rejected |
| 3 | Lowercase required | No lowercase = rejected |
| 4 | Digit required | No digit = rejected |
| 5 | Salt uniqueness | Each user has unique salt |
| 6 | SHA-256 algorithm used | Hash output matches SHA-256 length |

---

### Test Case ID: NFR-ADM-006
**Category**: Security
**Component**: Input Sanitization (SecurityHelper.SanitizeInput)
**Priority**: High

#### Security Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | SQL injection single quote | "'; DROP TABLE Users;--" | "'' DROP TABLE Users" |
| 2 | Double dash comment | "test--comment" | "testcomment" |
| 3 | Semicolon command separator | "test;DROP" | "testDROP" |
| 4 | Combined attack | "admin'--; DROP" | "admin'' DROP" |

---

### Test Case ID: NFR-ADM-007
**Category**: Reliability
**Component**: Error Logging (ErrorLogger)
**Priority**: High

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | File logging survives DB failure | DB down, file log still works |
| 2 | Logging never crashes application | Any logging exception swallowed |
| 3 | Concurrent error logging | Thread-safe via SyncLock |
| 4 | Log directory auto-created | Missing Logs/ dir created on first write |
| 5 | Daily log rotation | New file each day (ErrorLog_yyyyMMdd.log) |

---

### Test Case ID: NFR-ADM-008
**Category**: Performance
**Component**: Concurrent Login
**Priority**: High

#### Test Scenarios
| # | Scenario | Load | Expected |
|---|----------|------|----------|
| 1 | 10 concurrent logins | 10 users | All authenticate within 2s |
| 2 | 50 concurrent logins | 50 users | All authenticate within 5s |
| 3 | Failed login under load | Mix of valid/invalid | No deadlocks on FailedLoginAttempts update |

---

### Test Case ID: NFR-ADM-009
**Category**: Reliability
**Component**: Database Connection Management (DatabaseHelper)
**Priority**: Critical

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Connection disposed after use | Using block ensures disposal |
| 2 | Connection timeout (120s) | CommandTimeout = 120 seconds |
| 3 | Connection pool not exhausted | Under normal load, connections returned to pool |
| 4 | SqlDataAdapter disposed | Using block ensures disposal |
