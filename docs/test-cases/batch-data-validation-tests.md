# Batch Jobs Module - Data Validation Tests

## Module: BAT (Batch Jobs)
## Test Type: Data Integrity and Validation Tests
## Tables Covered:
- `Batch.JobLogs`
- `Policy.Policies` (status transitions during batch processing)
- `Claims.Claims` (reserve recalculation fields)
- `Claims.Activities` (reserve review flags)
- `Billing.Invoices` (cancellation notice fields)

---

### Test Case ID: DV-BAT-001
**Table**: Batch.JobLogs
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (inferred from SP usage)

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| JobLogID | BIGINT IDENTITY | PK, auto-increment | Cannot insert explicit value |
| JobName | VARCHAR(100) | NOT NULL | NULL rejected [ASSUMPTION] |
| StartTime | DATETIME | NOT NULL (set by SP) | Always populated by usp_JobLog_Start |
| EndTime | DATETIME | Nullable | NULL while RUNNING, set on Complete/Fail |
| Status | VARCHAR(20) | NOT NULL | Values: RUNNING, COMPLETED, COMPLETED_WITH_ERRORS, FAILED |
| Parameters | VARCHAR(500) | Nullable | NULL accepted |
| RecordsProcessed | INT | Nullable, default 0 | Set on completion |
| RecordsFailed | INT | Nullable, default 0 | Set on completion |
| ResultMessage | VARCHAR(500) | Nullable | Error message on failure |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | JobLogID auto-increments | Multiple inserts | Sequential IDs |
| 2 | JobName max length | 100 character name | Stored correctly |
| 3 | JobName exceeds max | 101 characters | Truncation or error |
| 4 | Status values valid | 'RUNNING', 'COMPLETED', 'COMPLETED_WITH_ERRORS', 'FAILED' | All accepted |
| 5 | Status invalid value | 'INVALID_STATUS' | Accepted (no CHECK constraint) [ASSUMPTION] |
| 6 | Parameters max length | 500 characters | Stored correctly |
| 7 | ResultMessage stores error | Long error message | Stored up to VARCHAR(500) limit |
| 8 | EndTime NULL while running | Job started, not completed | EndTime IS NULL |
| 9 | EndTime populated on complete | usp_JobLog_Complete called | EndTime = GETDATE() |

---

### Test Case ID: DV-BAT-002
**Table**: Batch.JobLogs - Status Transitions
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`

#### State Machine Tests
| # | Initial State | Action | Expected Final State |
|---|--------------|--------|---------------------|
| 1 | (new) | usp_JobLog_Start | RUNNING |
| 2 | RUNNING | usp_JobLog_Complete (RecordsFailed=0) | COMPLETED |
| 3 | RUNNING | usp_JobLog_Complete (RecordsFailed>0) | COMPLETED_WITH_ERRORS |
| 4 | RUNNING | usp_JobLog_Fail | FAILED |
| 5 | COMPLETED | usp_JobLog_Fail (re-call) | FAILED (no guard) [ASSUMPTION] |
| 6 | FAILED | usp_JobLog_Complete (re-call) | COMPLETED (no guard) [ASSUMPTION] |

#### Elapsed Time Validation
| # | Test | Expected |
|---|------|----------|
| 1 | EndTime >= StartTime | Always true for valid runs |
| 2 | Elapsed time = EndTime - StartTime | Positive duration |
| 3 | Very short run (< 1 second) | Valid, EndTime may equal StartTime |
| 4 | Very long run (> 1 hour) | Valid, no timeout in SP |

---

### Test Case ID: DV-BAT-003
**Table**: Policy.Policies - Expiration Processing Data
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (Batch.usp_Expiration_Process)

#### State Transition Tests
| # | Test | Before | After | Validation |
|---|------|--------|-------|-----------|
| 1 | Active policy expired | PolicyStatus='ACTIVE', ExpiryDate < today | PolicyStatus='EXPIRED' | Status updated correctly |
| 2 | ModifiedDate updated | Any value | GETDATE() | Timestamp refreshed |
| 3 | ModifiedBy set to batch user | Any value | @ProcessedBy value | Audit trail maintained |
| 4 | Non-active policies unaffected | PolicyStatus='CANCELLED' | PolicyStatus='CANCELLED' | No change |
| 5 | Future expiry unaffected | ExpiryDate > today | PolicyStatus='ACTIVE' | Not expired |

#### Data Integrity After Batch
| # | Test | Expected |
|---|------|----------|
| 1 | No orphaned coverages | Coverages still reference valid PolicyID |
| 2 | Billing invoices consistent | Invoice status unaffected by policy expiration |
| 3 | Claims on expired policy | Existing claims remain in current status |

---

### Test Case ID: DV-BAT-004
**Table**: Policy.Policies - Renewal Processing Data
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (Batch.usp_Renewal_Process)

#### State Transition Tests
| # | Test | Before | After | Validation |
|---|------|--------|-------|-----------|
| 1 | Policy flagged for renewal | IsRenewal=0 | IsRenewal=1 | Flag set correctly |
| 2 | ModifiedDate updated | Any value | GETDATE() | Timestamp refreshed |
| 3 | ModifiedBy set | Any value | @ProcessedBy | Audit trail |
| 4 | Only ACTIVE policies flagged | PolicyStatus='ACTIVE' | IsRenewal=1 | Status filter respected |
| 5 | Already-flagged policies excluded | IsRenewal=1 | Not re-processed | Duplicate prevention |

#### Eligibility Criteria Validation
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | Expiry within window | ExpiryDate <= CutoffDate AND ExpiryDate > today | Eligible |
| 2 | Expiry past (already expired) | ExpiryDate <= today | Not eligible |
| 3 | Expiry too far future | ExpiryDate > CutoffDate | Not eligible |
| 4 | Higher version exists | PolicyVersion > current in QUOTE/ACTIVE | Not eligible |
| 5 | No higher version | No newer version | Eligible |

---

### Test Case ID: DV-BAT-005
**Table**: Claims.Claims - Reserve Recalculation Data
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (Batch.usp_Reserve_Recalculate)

#### Calculation Validation
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TotalReserve = SUM of approved reserves | Reserves: 5000, 3000, 2000 (approved) | TotalReserve = 10000 |
| 2 | Unapproved reserves excluded | Reserve with IsApproved=0 | Not included in SUM |
| 3 | TotalPaid = SUM of qualifying payments | Payments with status APPROVED/ISSUED/CLEARED | Sum of amounts |
| 4 | Other payment statuses excluded | Payment with Status='PENDING' or 'VOIDED' | Not included |
| 5 | TotalRecovery from subrogation | Subrogation RecoveryAmounts: 1000, 500 | TotalRecovery = 1500 |
| 6 | NetIncurred formula | Reserve=10000, Paid=5000, Recovery=1500 | NetIncurred = 13500 |
| 7 | All nulls treated as zero | No reserves, no payments, no subrogation | All totals = 0, NetIncurred = 0 |
| 8 | Only open claims updated | ClaimStatus='INVESTIGATING' | Updated |
| 9 | Closed claims not updated | ClaimStatus='CLOSED' | Unchanged |
| 10 | Denied claims not updated | ClaimStatus='DENIED' | Unchanged |

---

### Test Case ID: DV-BAT-006
**Table**: Claims.Activities - Reserve Review Flags
**Source**: `database/02-stored-procedures/012-additional-sps.sql` (Batch.usp_Reserve_FlagForReview)

#### Insert Validation
| # | Test | Expected |
|---|------|----------|
| 1 | ActivityType = 'NOTE' | Correct type for review flag |
| 2 | ActivityDate = GETDATE() | Current timestamp |
| 3 | Subject = 'RESERVE REVIEW REQUIRED' | Standard subject text |
| 4 | Description = @ReviewReason | Reason text stored |
| 5 | Priority = 'HIGH' | High priority for review items |
| 6 | CreatedBy = @FlaggedBy | Batch user recorded |
| 7 | CreatedDate = GETDATE() | Timestamp recorded |
| 8 | ClaimID references valid claim | FK constraint | Valid foreign key |

---

### Test Case ID: DV-BAT-007
**Table**: Billing.Invoices - Cancellation Notice Data
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (Batch.usp_CancellationNotice_Process)

#### Update Validation
| # | Test | Before | After | Validation |
|---|------|--------|-------|-----------|
| 1 | CancellationNoticeDate set | NULL | CAST(GETDATE() AS DATE) | Today's date |
| 2 | CancellationEffectiveDate calculated | NULL | Today + @CancellationNoticeDays | 20 days from notice |
| 3 | ModifiedDate updated | Any | GETDATE() | Timestamp refreshed |
| 4 | Only OVERDUE invoices affected | Status='OVERDUE' | Updated | Status filter |
| 5 | Only un-noticed invoices affected | CancellationNoticeDate IS NULL | Updated | NULL filter |
| 6 | Only 30+ days past due | DueDate < 30 days ago | Updated | Date filter |
| 7 | Config value used for days | CANCELLATION_NOTICE_DAYS=20 | CancellationEffectiveDate = today + 20 | Config-driven |

---

### Test Case ID: DV-BAT-008
**Table**: Claims.Claims - Data Archive Validation
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql` (Batch.usp_Data_Archive)

#### Archive Eligibility Validation
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | Closed claim older than 7 years | ClaimStatus='CLOSED', ClosedDate < 7 years ago | Archived (ModifiedDate/By updated) |
| 2 | Closed claim less than 7 years old | ClaimStatus='CLOSED', ClosedDate = 5 years ago | Not archived |
| 3 | Open claim regardless of age | ClaimStatus='INVESTIGATING' | Not archived |
| 4 | Custom retention period | @YearsOld=5 | Cutoff = 5 years ago |
| 5 | Data integrity after archive | Claim archived | Related records (payments, reserves) unaffected [ASSUMPTION] |

---
