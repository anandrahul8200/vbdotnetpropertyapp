# Batch Jobs Module - Unit Tests

## Module: BAT (Batch Jobs)
## Test Type: Unit Tests for Batch Job Programs
## Programs Covered:
- `src/PropertyInsuranceClaims.BatchJobs/ExpirationProcessor/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/FraudScoring/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/PaymentBatch/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/ReinsuranceAllocator/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/RenewalProcessor/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb`

---

### Test Case ID: UT-BAT-001
**Program**: ExpirationProcessor
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/ExpirationProcessor/Program.vb`

#### Method Behavior
- JOB_NAME = "ExpirationProcessor"
- No argument parsing (uses fixed parameters)
- Three-step pipeline: expire policies, cancel for non-payment, generate notices

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | LogJobStart called with correct name | Mock DatabaseHelper | SP='Batch.usp_JobLog_Start', @JobName='ExpirationProcessor' |
| 2 | Step 1 calls correct SP | Mock | SP='Batch.usp_Policy_ProcessExpirations', @ProcessedBy='BATCH_EXPIRATION', @AsOfDate=DateTime.Today |
| 3 | Step 2 calls cancellation SP | Mock | SP='Batch.usp_Policy_ProcessCancellations', @GracePeriodDays=30 |
| 4 | Step 3 calls cancel notice SP | Mock | SP='Batch.usp_Policy_GenerateCancelNotices', @NoticeDays=20 |
| 5 | Expired count from DataTable rows | dtExpired has 5 rows | expired = 5 |
| 6 | LogJobComplete receives total | expired=3, cancelled=2 | RecordsProcessed = 5 (3+2) |
| 7 | Exception triggers LogJobFailed | SP throws | LogJobFailed called with ex.Message |
| 8 | Exception sets ExitCode=1 | SP throws | Environment.ExitCode = 1 |
| 9 | ErrorLogger.LogError called on failure | SP throws | ErrorLogger.LogError(ex, "ExpirationProcessor") |

---

### Test Case ID: UT-BAT-002
**Program**: RenewalProcessor
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/RenewalProcessor/Program.vb`

#### Method Behavior
- JOB_NAME = "RenewalProcessor"
- Parses first argument as daysAhead (default 30)
- Iterates policies from Batch.usp_Renewal_GetDuePolicies

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Default daysAhead is 30 | args = empty | daysAhead = 30 |
| 2 | Parse first argument as integer | args = {"60"} | daysAhead = 60 |
| 3 | Non-integer argument uses default | args = {"abc"} | daysAhead = 30 |
| 4 | LogJobStart parameters include DaysAhead | daysAhead=30 | @Parameters='DaysAhead=30' |
| 5 | Calls Batch.usp_Renewal_GetDuePolicies | Mock | @DaysAhead=30, @ProcessedBy='BATCH_RENEWAL' |
| 6 | Iterates each policy row | DataTable with 3 rows | 3 calls to Batch.usp_Renewal_ProcessPolicy |
| 7 | Per-policy call includes PolicyID | Row has PolicyID=42 | @PolicyID=42, @ProcessedBy='BATCH_RENEWAL' |
| 8 | Successful processing increments count | 3 successes | processed = 3 |
| 9 | Individual failure increments failed | 1 throws exception | failed = 1, ErrorLogger called with PolicyID context |
| 10 | Individual failure does not stop batch | 1 failure in middle | Remaining policies still processed |
| 11 | LogJobComplete receives correct counts | processed=4, failed=1 | RecordsProcessed=4, RecordsFailed=1 |

---

### Test Case ID: UT-BAT-003
**Program**: FraudScoring
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/FraudScoring/Program.vb`

#### Method Behavior
- JOB_NAME = "FraudScoring"
- Parses first argument as daysBack (default 7)
- Uses OUTPUT parameter @FraudScore from Claims.usp_Fraud_EvaluateClaim
- Threshold: score >= 70 triggers SIU referral count

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Default daysBack is 7 | args = empty | daysBack = 7 |
| 2 | Parse first argument | args = {"14"} | daysBack = 14 |
| 3 | LogJobStart parameters | daysBack=7 | @Parameters='DaysBack=7' |
| 4 | Calls Batch.usp_Fraud_GetClaimsToScore | Mock | @DaysBack=7 |
| 5 | For each claim calls EvaluateClaim | 3 claims | 3 calls to Claims.usp_Fraud_EvaluateClaim |
| 6 | Output param @FraudScore created | Mock | SqlDbType.Decimal, Direction=Output |
| 7 | Score >= 70 increments referred | @FraudScore returns 75.5 | referred = 1 |
| 8 | Score < 70 does not increment referred | @FraudScore returns 45.0 | referred unchanged |
| 9 | Successful score increments processed | Valid eval | processed++ |
| 10 | Individual failure increments failed | Inner exception | failed++, ErrorLogger called with ClaimID |
| 11 | Individual failure does not stop batch | 1 failure | Remaining claims processed |
| 12 | Final counts correct | 5 scored, 2 referred, 1 failed | LogJobComplete(5,1), console shows all counts |

---

### Test Case ID: UT-BAT-004
**Program**: ReserveRecalculator
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb`

#### Method Behavior
- JOB_NAME = "ReserveRecalculator"
- Four-step pipeline: get claims, review reserves (3 rules), calculate IBNR, update net incurred
- Three review rules with specific thresholds

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | LogJobStart called | Mock | @JobName='ReserveRecalculator' |
| 2 | Step 1: Calls GetClaimsForReview | Mock | @ProcessedBy='BATCH_RESERVE' |
| 3 | Rule 1: Reserve < 10% of paid | currentReserve=5, totalPaid=100 | needsReview=True, reason="Reserve less than 10% of paid amount" |
| 4 | Rule 1: Not triggered when totalPaid=0 | currentReserve=0, totalPaid=0 | needsReview=False (totalPaid > 0 guard) |
| 5 | Rule 2: Aged claim no change | claimAge=200, lastChange=100 days ago | needsReview=True, reason="No reserve change in 90+ days on aged claim" |
| 6 | Rule 2: Not triggered when claimAge <= 180 | claimAge=180 | Rule 2 skipped |
| 7 | Rule 3: Reserve exceeds limit | currentReserve=200000, policyLimit=100000 | needsReview=True, reason="Reserve exceeds policy limit" |
| 8 | Rule 3: Not triggered when limit=0 | policyLimit=0 | Rule 3 skipped (guard policyLimit > 0) |
| 9 | FlagForReview called when needed | needsReview=True | Batch.usp_Reserve_FlagForReview with ClaimID, reason, 'BATCH_RESERVE' |
| 10 | Step 3: CalculateIBNR called | Mock | @AsOfDate=today, @CalculatedBy='BATCH_RESERVE' |
| 11 | Step 4: UpdateNetIncurred called | Mock | Batch.usp_Reserve_UpdateNetIncurred with Nothing params |
| 12 | DBNull handling for TotalReserve | IsDBNull=True | Treats as 0 |
| 13 | DBNull handling for LastReserveChange | IsDBNull=True | DateTime.MinValue used |

---

### Test Case ID: UT-BAT-005
**Program**: PaymentBatch
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/PaymentBatch/Program.vb`

#### Method Behavior
- JOB_NAME = "PaymentBatch"
- Four-step pipeline: late fees, auto-pay EFT, overdue notices, commission payments
- Uses OUTPUT parameters for late fee count and payment IDs

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | LogJobStart called | Mock | @JobName='PaymentBatch' |
| 2 | Step 1: ApplyLateFees uses output param | Mock | @InvoicesProcessed OUTPUT, SqlDbType.Int |
| 3 | Step 2: ProcessAutoEFT called | Mock | @ProcessDate=today, @ProcessedBy='BATCH_PAYMENT' |
| 4 | EFT payment recorded per row | DataTable with 2 rows | 2 calls to Billing.usp_Payment_Record |
| 5 | EFT reference number format | InvoiceID=1234, today=20240115 | @ReferenceNumber='AUTO-20240115-1234' |
| 6 | EFT PaymentMethod is always EFT | Any row | @PaymentMethod='EFT' |
| 7 | Individual EFT failure | One row throws | failed++, ErrorLogger with PolicyID context |
| 8 | Step 3: OverdueNotices with DaysOverdue=10 | Mock | @DaysOverdue=10, @ProcessedBy='BATCH_PAYMENT' |
| 9 | Step 4: CommissionPayments | Mock | @PaymentDate=today, @ProcessedBy='BATCH_PAYMENT' |
| 10 | LogJobComplete total | lateFeeCount=5, autoPayCount=3 | RecordsProcessed = 8 (5+3) |

---

### Test Case ID: UT-BAT-006
**Program**: ReinsuranceAllocator
**Method**: Main(args As String())
**File**: `src/PropertyInsuranceClaims.BatchJobs/ReinsuranceAllocator/Program.vb`

#### Method Behavior
- JOB_NAME = "ReinsuranceAllocator"
- Accounting period defaults to previous month or first argument
- Four-step pipeline: load treaties, premium cessions, loss cessions, bordereaux

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Default period is previous month | args=empty, now=2024-02-15 | accountingPeriod = "2024-01" |
| 2 | Custom period from argument | args={"2024-06"} | accountingPeriod = "2024-06" |
| 3 | LogJobStart parameters | period=2024-01 | @Parameters='Period=2024-01' |
| 4 | Step 1: Load active treaties | Mock | Reinsurance.usp_Treaty_GetActive |
| 5 | Step 2: Premium cessions per treaty | 2 treaties, 3 premiums each | 6 calls to Reinsurance.usp_Cession_Create with @CessionType='PREMIUM' |
| 6 | Step 3: Loss cessions per treaty | 2 treaties, 2 losses each | 4 calls to Reinsurance.usp_Cession_Create with @CessionType='LOSS' |
| 7 | Step 4: Bordereaux per treaty | 2 treaties | 4 bordereaux generated (PREMIUM + LOSS per treaty) |
| 8 | Bordereaux uses output param | Mock | @BordereauxID OUTPUT, SqlDbType.Int |
| 9 | Individual premium cession failure | One throws | failed++, ErrorLogger with TreatyID+PolicyID context |
| 10 | Individual loss cession failure | One throws | failed++, ErrorLogger with TreatyID+ClaimID context |
| 11 | LogJobComplete total | 6 prem + 4 loss + 4 bord = 14 | RecordsProcessed=14, RecordsFailed=failed |

---

### Test Case ID: UT-BAT-007
**Program**: All Batch Jobs - Common LogJobStart Pattern
**Method**: LogJobStart(jobName, parameters) As Long
**File**: All Program.vb files

#### Common Pattern Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls Batch.usp_JobLog_Start | Mock | 3 parameters: @JobName, @Parameters, @StartTime |
| 2 | Returns JobLogID from ExecuteScalar | Returns 42 | Function returns 42 |
| 3 | Returns 0 on exception | ExecuteScalar throws | Returns 0 (swallowed exception) |
| 4 | Returns 0 on NULL result | ExecuteScalar returns Nothing | CLng(If(Nothing, 0)) = 0 |

---

### Test Case ID: UT-BAT-008
**Program**: All Batch Jobs - Common LogJobComplete Pattern
**Method**: LogJobComplete(jobLogID, processed, failed)
**File**: All Program.vb files

#### Common Pattern Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls Batch.usp_JobLog_Complete | Mock | ExecuteNonQuery called |
| 2 | Passes 5 parameters | jobLogID=1, processed=10, failed=2 | @JobLogID=1, @RecordsProcessed=10, @RecordsFailed=2, @EndTime, @Status='COMPLETED' |
| 3 | Exception swallowed | ExecuteNonQuery throws | No exception propagated |

---

### Test Case ID: UT-BAT-009
**Program**: All Batch Jobs - Common LogJobFailed Pattern
**Method**: LogJobFailed(jobLogID, errorMessage)
**File**: All Program.vb files

#### Common Pattern Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls Batch.usp_JobLog_Complete with FAILED status | Mock | @Status='FAILED', @ErrorMessage=errorMessage |
| 2 | Passes 4 parameters | jobLogID=1, errorMessage='Connection timeout' | @JobLogID=1, @EndTime, @Status='FAILED', @ErrorMessage='Connection timeout' |
| 3 | Exception swallowed | ExecuteNonQuery throws | No exception propagated |

---
