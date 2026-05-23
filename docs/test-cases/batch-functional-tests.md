# Batch Jobs Module - Functional Tests

## Module: BAT (Batch Jobs)
## Test Type: End-to-End Functional Tests
## Source Files:
- `src/PropertyInsuranceClaims.BatchJobs/ExpirationProcessor/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/FraudScoring/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/PaymentBatch/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/ReinsuranceAllocator/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/RenewalProcessor/Program.vb`
- `src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb`

---

### Test Case ID: FT-BAT-001
**Feature**: Expiration Processing Pipeline
**Priority**: Critical

#### Preconditions
- Database contains ACTIVE policies with ExpiryDate in the past
- Batch.JobLogs table accessible
- Billing.Invoices contains overdue invoices for cancellation testing

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run ExpirationProcessor.exe | Console outputs timestamp and "Starting ExpirationProcessor..." |
| 2 | Job log started | Batch.usp_JobLog_Start called with @JobName='ExpirationProcessor', returns JobLogID |
| 3 | Step 1: Process expirations | Calls Batch.usp_Policy_ProcessExpirations with @ProcessedBy='BATCH_EXPIRATION', @AsOfDate=DateTime.Today |
| 4 | Expired count displayed | Console shows "Expired: N policies" |
| 5 | Step 2: Process non-payment cancellations | Calls Batch.usp_Policy_ProcessCancellations with @GracePeriodDays=30 |
| 6 | Cancelled count displayed | Console shows "Cancelled for non-payment: N policies" |
| 7 | Step 3: Generate cancellation notices | Calls Batch.usp_Policy_GenerateCancelNotices with @NoticeDays=20 |
| 8 | Notice count displayed | Console shows "Notices generated: N" |
| 9 | Job completed | Batch.usp_JobLog_Complete called with RecordsProcessed = expired + cancelled |
| 10 | Final summary | Console shows timestamp, "Completed. Expired: X, Cancelled: Y, Failed: Z" |

#### Error Handling
| Scenario | Expected Behavior |
|----------|-------------------|
| Database connection failure | Catch block: LogJobFailed called, "FATAL ERROR" logged, Environment.ExitCode = 1 |
| SP execution error | ErrorLogger.LogError called with exception and JOB_NAME |

---

### Test Case ID: FT-BAT-002
**Feature**: Renewal Processing Pipeline
**Priority**: Critical

#### Preconditions
- Database contains ACTIVE policies with ExpiryDate within the configured DaysAhead window
- RenewalProcessor.exe available

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run RenewalProcessor.exe | Console outputs "Starting RenewalProcessor..." |
| 2 | Parse arguments | Default daysAhead=30, or first argument parsed as integer |
| 3 | Job log started | Batch.usp_JobLog_Start called with @JobName='RenewalProcessor', @Parameters='DaysAhead=30' |
| 4 | Get due policies | Calls Batch.usp_Renewal_GetDuePolicies with @DaysAhead and @ProcessedBy='BATCH_RENEWAL' |
| 5 | Count displayed | Console shows "Found N policies due for renewal" |
| 6 | Process each policy | For each row: calls Batch.usp_Renewal_ProcessPolicy with @PolicyID and @ProcessedBy='BATCH_RENEWAL' |
| 7 | Per-policy status | Console shows "Processing POL-XXXXXXX... OK" or "FAILED: message" |
| 8 | Job completed | LogJobComplete called with processed and failed counts |
| 9 | Final summary | "Completed. Processed: X, Failed: Y" |

#### Argument Parsing
| Input | Expected DaysAhead |
|-------|-------------------|
| No arguments | 30 (default) |
| "60" | 60 |
| "abc" (non-integer) | 30 (default, TryParse fails) |

---

### Test Case ID: FT-BAT-003
**Feature**: Fraud Scoring Pipeline
**Priority**: Critical

#### Preconditions
- Open claims with FraudScore needing evaluation
- Claims.usp_Fraud_EvaluateClaim SP available
- AppSettings.FraudScoreThreshold = 70

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run FraudScoring.exe | Console outputs "Starting FraudScoring..." |
| 2 | Parse arguments | Default daysBack=7, or first argument parsed as integer |
| 3 | Job log started | Batch.usp_JobLog_Start called with @Parameters='DaysBack=7' |
| 4 | Get claims to score | Calls Batch.usp_Fraud_GetClaimsToScore with @DaysBack |
| 5 | Count displayed | Console shows "Found N claims to evaluate" |
| 6 | Score each claim | For each claim: calls Claims.usp_Fraud_EvaluateClaim with @ClaimID and @EvaluatedBy='BATCH_FRAUD' |
| 7 | Output param read | @FraudScore OUTPUT parameter captured |
| 8 | Score >= 70: SIU referral | Console shows "Score: X.X ** SIU REFERRED **" |
| 9 | Score < 70: normal | Console shows "Score: X.X" |
| 10 | Individual claim error | Catch: failed count incremented, ErrorLogger called with ClaimID context |
| 11 | Job completed | LogJobComplete called with processed and failed |
| 12 | Final summary | "Completed. Scored: X, Referred: Y, Failed: Z" |

#### Business Rules
- Fraud score threshold for SIU referral: score >= 70
- Only claims reported within @DaysBack days are evaluated
- Individual claim failures do not stop the batch

---

### Test Case ID: FT-BAT-004
**Feature**: Reserve Recalculation Pipeline
**Priority**: High

#### Preconditions
- Open claims with reserves exist in the database
- Batch.usp_Reserve_GetClaimsForReview, Batch.usp_Reserve_FlagForReview, Batch.usp_Reserve_CalculateIBNR, Batch.usp_Reserve_UpdateNetIncurred available

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run ReserveRecalculator.exe | Console outputs "Starting ReserveRecalculator..." |
| 2 | Job log started | LogJobStart called with JOB_NAME='ReserveRecalculator' |
| 3 | Step 1: Load claims | Calls Batch.usp_Reserve_GetClaimsForReview with @ProcessedBy='BATCH_RESERVE' |
| 4 | Step 2: Review reserves | Each claim evaluated against 3 rules |
| 5 | Rule 1: Reserve < 10% of paid | If currentReserve < totalPaid * 0.1 AND totalPaid > 0 | Flagged: "Reserve less than 10% of paid amount" |
| 6 | Rule 2: Aged claim no reserve change | If claimAge > 180 AND lastReserveChange > 90 days ago | Flagged: "No reserve change in 90+ days on aged claim" |
| 7 | Rule 3: Reserve exceeds policy limit | If currentReserve > policyLimit AND policyLimit > 0 | Flagged: "Reserve exceeds policy limit" |
| 8 | Flag for review | Calls Batch.usp_Reserve_FlagForReview with ClaimID, reason, 'BATCH_RESERVE' |
| 9 | Step 3: Calculate IBNR | Calls Batch.usp_Reserve_CalculateIBNR with @AsOfDate=today, @CalculatedBy='BATCH_RESERVE' |
| 10 | Step 4: Update net incurred | Calls Batch.usp_Reserve_UpdateNetIncurred |
| 11 | Job completed | LogJobComplete with processed and failed |
| 12 | Final summary | "Completed. Reviewed: X, Flagged: Y, Failed: Z" |

#### Reserve Review Rules
| Rule | Condition | Reason |
|------|-----------|--------|
| 1 | currentReserve < totalPaid * 0.1 AND totalPaid > 0 | "Reserve less than 10% of paid amount" |
| 2 | claimAge > 180 AND (DateTime.Now - lastReserveChange).TotalDays > 90 | "No reserve change in 90+ days on aged claim" |
| 3 | currentReserve > policyLimit AND policyLimit > 0 | "Reserve exceeds policy limit" |

---

### Test Case ID: FT-BAT-005
**Feature**: Payment Batch Processing Pipeline
**Priority**: High

#### Preconditions
- Overdue invoices exist for late fee processing
- Auto-pay EFT accounts configured
- Commission payments pending

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run PaymentBatch.exe | Console outputs "Starting PaymentBatch..." |
| 2 | Job log started | LogJobStart called with JOB_NAME='PaymentBatch' |
| 3 | Step 1: Apply late fees | Calls Billing.usp_Invoice_ApplyLateFees with @AsOfDate=today, @ProcessedBy='BATCH_PAYMENT' |
| 4 | Late fee count from output param | @InvoicesProcessed OUTPUT captured |
| 5 | Step 2: Process auto-pay EFT | Calls Batch.usp_Payment_ProcessAutoEFT with @ProcessDate=today |
| 6 | For each EFT row | Calls Billing.usp_Payment_Record with @PaymentMethod='EFT', @ReferenceNumber='AUTO-yyyyMMdd-{InvoiceID}' |
| 7 | Individual EFT failure | Catch: failed++, ErrorLogger with PolicyID context, "FAILED: message" |
| 8 | Step 3: Generate overdue notices | Calls Batch.usp_Payment_GenerateOverdueNotices with @DaysOverdue=10 |
| 9 | Step 4: Process commissions | Calls Batch.usp_Commission_ProcessPayments with @PaymentDate=today |
| 10 | Job completed | LogJobComplete with lateFeeCount + autoPayCount processed, failed count |
| 11 | Final summary | "Completed. Late fees: X, Auto-pay: Y, Failed: Z" |

#### EFT Reference Number Format
- Pattern: `AUTO-{yyyyMMdd}-{InvoiceID}`
- Example: `AUTO-20240115-1234`

---

### Test Case ID: FT-BAT-006
**Feature**: Reinsurance Allocation Pipeline
**Priority**: High

#### Preconditions
- Active reinsurance treaties exist
- Unallocated premiums and losses for the accounting period
- Reinsurance.usp_Treaty_GetActive, Batch.usp_Reinsurance_GetUnallocated, Reinsurance.usp_Cession_Create, Reinsurance.usp_Bordereaux_Generate available

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Run ReinsuranceAllocator.exe | Console outputs "Starting ReinsuranceAllocator..." |
| 2 | Determine accounting period | Default: previous month (DateTime.Now.AddMonths(-1).ToString("yyyy-MM")), or first argument |
| 3 | Job log started | LogJobStart with @Parameters='Period=yyyy-MM' |
| 4 | Step 1: Load active treaties | Calls Reinsurance.usp_Treaty_GetActive |
| 5 | Step 2: Premium cessions | For each treaty: get unallocated premiums, create cessions via Reinsurance.usp_Cession_Create |
| 6 | Step 3: Loss cessions | For each treaty: get unallocated losses, create loss cessions |
| 7 | Step 4: Generate bordereaux | For each treaty: generate PREMIUM and LOSS bordereaux via Reinsurance.usp_Bordereaux_Generate |
| 8 | Job completed | LogJobComplete with premiumCessions + lossCessions + bordereaux, failed |
| 9 | Final summary | Premium Cessions, Loss Cessions, Bordereaux, Failed counts |

#### Accounting Period Logic
| Input | Expected Period |
|-------|----------------|
| No arguments | Previous month (e.g., "2024-01" if run in Feb 2024) |
| "2024-06" | "2024-06" |

---
