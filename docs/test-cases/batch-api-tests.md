# Batch Jobs Module - API Tests (Future-State)

## Module: BAT (Batch Jobs)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose batch job functionality via REST APIs.
> Batch jobs are console applications scheduled via Windows Task Scheduler.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-BAT-001
**Legacy Program**: ExpirationProcessor (Program.vb)
**Future Endpoint**: POST /api/batch-jobs/expiration/run
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| ExpirationProcessor.exe | POST /api/batch-jobs/expiration/run |
| RenewalProcessor.exe | POST /api/batch-jobs/renewal/run |
| FraudScoring.exe | POST /api/batch-jobs/fraud-scoring/run |
| ReserveRecalculator.exe | POST /api/batch-jobs/reserve-recalculator/run |
| PaymentBatch.exe | POST /api/batch-jobs/payment/run |
| ReinsuranceAllocator.exe | POST /api/batch-jobs/reinsurance/run |
| Batch.usp_JobLog_Start | POST /api/batch-jobs/{id}/start (internal) |
| Batch.usp_JobLog_Complete | PUT /api/batch-jobs/{id}/complete (internal) |
| Batch.usp_JobLog_Fail | PUT /api/batch-jobs/{id}/fail (internal) |
| frmBatchJobMonitor (search) | GET /api/batch-jobs/history |
| frmBatchJobMonitor (view log) | GET /api/batch-jobs/history/{id} |

#### Request/Response Specification
**Request (POST /api/batch-jobs/expiration/run):**
```json
{
  "asOfDate": "2024-01-15",
  "processedBy": "admin"
}
```

**Response (202 Accepted):**
```json
{
  "jobLogId": 1234,
  "jobName": "ExpirationProcessor",
  "status": "RUNNING",
  "startTime": "2024-01-15T03:00:00Z",
  "message": "Job started successfully"
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | POST | /api/batch-jobs/expiration/run | Trigger expiration job | 202 Accepted | jobLogId returned, status=RUNNING |
| 2 | POST | /api/batch-jobs/expiration/run | Job already running | 409 Conflict | "ExpirationProcessor is already running" |
| 3 | POST | /api/batch-jobs/expiration/run | Unauthorized user | 403 Forbidden | "Insufficient permissions" |
| 4 | POST | /api/batch-jobs/expiration/run | Invalid request body | 400 Bad Request | Validation errors |

---

### Test Case ID: API-BAT-002
**Legacy Program**: RenewalProcessor (Program.vb)
**Future Endpoint**: POST /api/batch-jobs/renewal/run
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/batch-jobs/renewal/run):**
```json
{
  "daysAhead": 30,
  "processedBy": "admin"
}
```

**Response (202 Accepted):**
```json
{
  "jobLogId": 1235,
  "jobName": "RenewalProcessor",
  "status": "RUNNING",
  "startTime": "2024-01-15T02:00:00Z",
  "parameters": "DaysAhead=30"
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | POST | /api/batch-jobs/renewal/run | Trigger with default days | 202 Accepted | daysAhead defaults to 30 |
| 2 | POST | /api/batch-jobs/renewal/run | Custom daysAhead=60 | 202 Accepted | parameters="DaysAhead=60" |
| 3 | POST | /api/batch-jobs/renewal/run | Invalid daysAhead=-1 | 400 Bad Request | "daysAhead must be positive" |
| 4 | POST | /api/batch-jobs/renewal/run | Job already running | 409 Conflict | "RenewalProcessor is already running" |

---

### Test Case ID: API-BAT-003
**Legacy Program**: FraudScoring (Program.vb)
**Future Endpoint**: POST /api/batch-jobs/fraud-scoring/run
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/batch-jobs/fraud-scoring/run):**
```json
{
  "daysBack": 7,
  "processedBy": "admin"
}
```

**Response (202 Accepted):**
```json
{
  "jobLogId": 1236,
  "jobName": "FraudScoring",
  "status": "RUNNING",
  "startTime": "2024-01-15T04:00:00Z",
  "parameters": "DaysBack=7"
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | POST | /api/batch-jobs/fraud-scoring/run | Default parameters | 202 Accepted | daysBack=7 |
| 2 | POST | /api/batch-jobs/fraud-scoring/run | Custom daysBack=14 | 202 Accepted | parameters="DaysBack=14" |
| 3 | POST | /api/batch-jobs/fraud-scoring/run | daysBack=0 | 400 Bad Request | "daysBack must be positive" |

---

### Test Case ID: API-BAT-004
**Legacy Form**: frmBatchJobMonitor
**Future Endpoint**: GET /api/batch-jobs/history
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (GET /api/batch-jobs/history?jobName=FraudScoring&status=COMPLETED&since=2024-01-01):**

**Response (200 OK):**
```json
{
  "totalCount": 25,
  "items": [
    {
      "jobLogId": 1236,
      "jobName": "FraudScoring",
      "status": "COMPLETED",
      "startTime": "2024-01-15T04:00:00Z",
      "endTime": "2024-01-15T04:05:23Z",
      "recordsProcessed": 42,
      "recordsFailed": 0,
      "resultMessage": null,
      "elapsedSeconds": 323
    }
  ]
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | GET | /api/batch-jobs/history | No filters | 200 OK | All job history (paginated) |
| 2 | GET | /api/batch-jobs/history?jobName=FraudScoring | Filter by job | 200 OK | Only FraudScoring entries |
| 3 | GET | /api/batch-jobs/history?status=FAILED | Filter by status | 200 OK | Only failed jobs |
| 4 | GET | /api/batch-jobs/history?since=2024-01-01 | Filter by date | 200 OK | Jobs since date |
| 5 | GET | /api/batch-jobs/history/1236 | Get specific run | 200 OK | Full details with log |
| 6 | GET | /api/batch-jobs/history/99999 | Non-existent ID | 404 Not Found | "Job log not found" |

---

### Test Case ID: API-BAT-005
**Legacy SP**: Batch.usp_Data_Archive
**Future Endpoint**: POST /api/batch-jobs/data-archive/run
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/batch-jobs/data-archive/run):**
```json
{
  "yearsOld": 7,
  "processedBy": "admin"
}
```

**Response (202 Accepted):**
```json
{
  "jobLogId": 1240,
  "jobName": "DataArchiver",
  "status": "RUNNING",
  "parameters": "YearsOld=7"
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | POST | /api/batch-jobs/data-archive/run | Default retention (7 years) | 202 Accepted | yearsOld=7 |
| 2 | POST | /api/batch-jobs/data-archive/run | Custom retention (5 years) | 202 Accepted | yearsOld=5 |
| 3 | POST | /api/batch-jobs/data-archive/run | yearsOld=0 | 400 Bad Request | "yearsOld must be at least 1" |
| 4 | POST | /api/batch-jobs/data-archive/run | Unauthorized | 403 Forbidden | Requires admin role |

---

### Test Case ID: API-BAT-006
**Legacy**: Job status polling
**Future Endpoint**: GET /api/batch-jobs/status
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (GET /api/batch-jobs/status):**

**Response (200 OK):**
```json
{
  "runningJobs": [
    {
      "jobLogId": 1234,
      "jobName": "ExpirationProcessor",
      "startTime": "2024-01-15T03:00:00Z",
      "elapsedSeconds": 45
    }
  ],
  "lastCompletedRuns": {
    "ExpirationProcessor": { "lastRun": "2024-01-14T03:00:00Z", "status": "COMPLETED" },
    "RenewalProcessor": { "lastRun": "2024-01-14T02:00:00Z", "status": "COMPLETED" },
    "FraudScoring": { "lastRun": "2024-01-14T04:00:00Z", "status": "COMPLETED_WITH_ERRORS" },
    "PaymentBatch": { "lastRun": "2024-01-14T05:00:00Z", "status": "COMPLETED" },
    "ReserveRecalculator": { "lastRun": "2024-01-13T01:00:00Z", "status": "COMPLETED" },
    "ReinsuranceAllocator": { "lastRun": "2024-01-01T06:00:00Z", "status": "COMPLETED" }
  }
}
```

#### Tests
| # | Method | Endpoint | Scenario | Expected Status | Expected Response |
|---|--------|----------|----------|-----------------|-------------------|
| 1 | GET | /api/batch-jobs/status | No jobs running | 200 OK | runningJobs=[], lastCompletedRuns populated |
| 2 | GET | /api/batch-jobs/status | Job currently running | 200 OK | runningJobs contains active job |
| 3 | GET | /api/batch-jobs/status | Multiple jobs running | 200 OK | All running jobs listed |

---
