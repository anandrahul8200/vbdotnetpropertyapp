# Batch Jobs Module - Non-Functional Requirements Tests

## Module: BAT (Batch Jobs)
## Test Type: Performance, Reliability, and Scalability Tests

---

### Test Case ID: NFR-BAT-001
**Category**: Performance
**Component**: Expiration Processing (Batch.usp_Expiration_Process)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Processing time (1000 policies) | < 30 seconds | Time from start to completion |
| Processing time (10000 policies) | < 5 minutes | Time from start to completion |
| Transaction lock duration | < 10 seconds | Lock held during UPDATE |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small batch expiration | 100 expired policies | < 5s total job time |
| 2 | Medium batch expiration | 1000 expired policies | < 30s |
| 3 | Large batch expiration | 10000 expired policies | < 5min |
| 4 | No policies to expire | 0 eligible | < 2s (query only) |

---

### Test Case ID: NFR-BAT-002
**Category**: Performance
**Component**: Renewal Processing (Batch.usp_Renewal_Process)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Per-policy processing | < 500ms per policy | Average time per cursor iteration |
| Total batch (500 policies) | < 5 minutes | Start to complete |
| Memory usage | < 200MB | Peak during cursor processing |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small renewal batch | 50 policies due | < 30s |
| 2 | Medium renewal batch | 500 policies due | < 5min |
| 3 | Large renewal batch | 2000 policies due | < 20min |
| 4 | Cursor efficiency | 1000 policies | No temp table overflow |

---

### Test Case ID: NFR-BAT-003
**Category**: Performance
**Component**: Fraud Scoring Batch (FraudScoring Program)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Per-claim scoring | < 2 seconds per claim | Individual SP call time |
| Total batch (100 claims) | < 5 minutes | Start to complete |
| Memory usage | < 150MB | Peak during batch |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small scoring batch | 20 claims | < 1min |
| 2 | Medium scoring batch | 100 claims | < 5min |
| 3 | Large scoring batch (weekly backlog) | 500 claims | < 25min |
| 4 | Individual claim timeout | 1 complex claim | < 30s per evaluation |

---

### Test Case ID: NFR-BAT-004
**Category**: Performance
**Component**: Reserve Recalculation (ReserveRecalculator Program)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Batch.usp_Reserve_Recalculate (all claims) | < 2 minutes | Single UPDATE statement |
| Reserve review loop (per claim) | < 200ms | Average per iteration |
| IBNR calculation | < 60 seconds | Batch.usp_Reserve_CalculateIBNR |
| Net incurred update | < 60 seconds | Batch.usp_Reserve_UpdateNetIncurred |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small open claims set | 100 claims | Total < 1min |
| 2 | Medium open claims set | 1000 claims | Total < 5min |
| 3 | Large open claims set | 5000 claims | Total < 15min |
| 4 | Reserve review with many flags | 500 claims flagged | FlagForReview calls < 2min total |

---

### Test Case ID: NFR-BAT-005
**Category**: Performance
**Component**: Payment Batch (PaymentBatch Program)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Late fee application | < 30 seconds | Billing.usp_Invoice_ApplyLateFees |
| Auto-pay EFT (per transaction) | < 1 second | Individual payment record |
| Total batch (200 payments) | < 5 minutes | Start to complete |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Few late fees | 20 overdue invoices | < 10s |
| 2 | Many late fees | 500 overdue invoices | < 30s |
| 3 | Small EFT batch | 10 auto-pay accounts | < 30s |
| 4 | Large EFT batch | 200 auto-pay accounts | < 5min |

---

### Test Case ID: NFR-BAT-006
**Category**: Performance
**Component**: Reinsurance Allocation (ReinsuranceAllocator Program)
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Premium cessions per treaty | < 2 minutes | Per treaty processing |
| Loss cessions per treaty | < 2 minutes | Per treaty processing |
| Bordereaux generation | < 30 seconds per treaty | Per report generation |
| Total monthly run | < 30 minutes | All treaties processed |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Few treaties, few policies | 3 treaties, 50 policies | < 5min |
| 2 | Many treaties, many policies | 10 treaties, 500 policies | < 30min |
| 3 | Bordereaux with large cession volume | 1000 cessions per treaty | Reports < 1min each |

---

### Test Case ID: NFR-BAT-007
**Category**: Reliability
**Component**: Job Logging Lifecycle
**Priority**: Critical

#### Reliability Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Job log always started | 100% | Every batch run has a JobLog_Start |
| Job log always completed/failed | 100% | No RUNNING status left indefinitely |
| Error capture on failure | 100% | ErrorMessage populated on FAILED |

#### Test Scenarios
| # | Scenario | Expected |
|---|----------|----------|
| 1 | Normal completion | Status transitions: RUNNING -> COMPLETED |
| 2 | Completion with errors | Status: COMPLETED_WITH_ERRORS (RecordsFailed > 0) |
| 3 | Fatal failure | Status transitions: RUNNING -> FAILED, ErrorMessage populated |
| 4 | LogJobStart failure (DB unavailable) | Returns 0, batch continues |
| 5 | LogJobComplete failure | Exception swallowed, no crash |
| 6 | LogJobFailed failure | Exception swallowed, no crash |
| 7 | Concurrent job execution | Two instances of same job | Separate JobLogIDs, no interference |

---

### Test Case ID: NFR-BAT-008
**Category**: Reliability
**Component**: Error Recovery and Isolation
**Priority**: Critical

#### Test Scenarios
| # | Scenario | Expected |
|---|----------|----------|
| 1 | Individual record failure in loop | Failed record logged, loop continues to next |
| 2 | Database connection drop mid-batch | Outer catch fires, LogJobFailed called, ExitCode=1 |
| 3 | SP timeout on single record | Inner catch handles, failed count incremented |
| 4 | Out of memory during large batch | Process terminates, job left in RUNNING (requires monitoring) |
| 5 | Disk full during logging | ErrorLogger may fail silently, batch may continue |

---

### Test Case ID: NFR-BAT-009
**Category**: Scalability
**Component**: Batch Processing Volume
**Priority**: Medium

#### Scalability Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Linear scaling | Processing time proportional to record count | Time vs. volume graph |
| Memory stability | No memory growth per record | Stable RSS during processing |
| DB connection pooling | Single connection reused | Connection count during batch |

#### Test Scenarios
| # | Scenario | Expected |
|---|----------|----------|
| 1 | Double the volume | Processing time approximately doubles |
| 2 | 10x volume | No memory leaks, completes within SLA |
| 3 | Concurrent batch jobs | Multiple jobs can run simultaneously without deadlocks |

---

### Test Case ID: NFR-BAT-010
**Category**: Scheduling and Monitoring
**Component**: Windows Task Scheduler Integration
**Priority**: High

#### Scheduling Criteria (from register-scheduled-tasks.ps1)
| Job | Schedule | Time | Expected |
|----|----------|------|----------|
| PI-RenewalProcessor | Daily | 02:00 | Runs every day at 2 AM |
| PI-ExpirationProcessor | Daily | 03:00 | Runs every day at 3 AM |
| PI-FraudScoring | Daily | 04:00 | Runs every day at 4 AM |
| PI-PaymentBatch | Daily | 05:00 | Runs every day at 5 AM |
| PI-ReserveRecalculator | Weekly | 01:00 | Runs weekly at 1 AM |
| PI-ReinsuranceAllocator | Monthly | 06:00 | Runs monthly at 6 AM |
| PI-DataArchiver | Monthly | 23:00 | Runs monthly at 11 PM |
| PI-CancellationNotice | Daily | 08:00 | Runs daily at 8 AM |
| PI-IBNRCalculator | Monthly | 08:00 | Runs monthly at 8 AM |

#### Test Scenarios
| # | Scenario | Expected |
|---|----------|----------|
| 1 | Task registered correctly | Get-ScheduledTask returns task with correct settings |
| 2 | Task runs at scheduled time | Job executes, log entry created |
| 3 | Task runs as SYSTEM account | RunLevel=Highest, User=NT AUTHORITY\SYSTEM |
| 4 | Task starts when available | StartWhenAvailable=True, missed runs execute on wake |
| 5 | No overlap with other jobs | Staggered times prevent resource contention |

---
