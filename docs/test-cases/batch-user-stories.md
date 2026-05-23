# Batch Jobs Module - User Stories

## Module: BAT (Batch Jobs)
## Test Type: User Stories and Acceptance Criteria

---

### User Story ID: US-BAT-001
**Title**: Batch Job Monitoring
**Priority**: High

#### Story
As an operations administrator, I want to view the run history of all batch jobs so that I can verify jobs are running successfully and identify failures.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Batch jobs have run previously | I open the Batch Job Monitor form | dgvJobHistory displays job run history |
| 2 | I want to filter by job name | I select a job from cboJobName and click Search | Only runs for that job are displayed |
| 3 | I want to filter by status | I select FAILED from cboStatus and click Search | Only failed runs displayed |
| 4 | I want to see recent runs | I set dtpDateFrom to 7 days ago | Runs from last 7 days shown |
| 5 | I want to see run details | I select a row and click View Log | Job execution details displayed |
| 6 | No runs match filters | I search with restrictive filters | Empty grid, count shows "0 job runs" |

#### Form
- **Screen**: frmBatchJobMonitor
- **Stored Procedure**: [ASSUMPTION] Batch.usp_JobLog_Search or similar query
- **File**: `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

---

### User Story ID: US-BAT-002
**Title**: Policy Expiration Processing
**Priority**: Critical

#### Story
As a system administrator, I want expired policies to be automatically marked as EXPIRED each day so that policy statuses accurately reflect their current state.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Active policies with ExpiryDate in the past | ExpirationProcessor runs (scheduled daily at 3 AM) | Policies updated to status EXPIRED |
| 2 | Policies overdue for payment (30+ days) | ExpirationProcessor runs | Policies cancelled for non-payment |
| 3 | Policies approaching cancellation | ExpirationProcessor runs | Cancellation notices generated (20-day notice period) |
| 4 | Job completes successfully | ExpirationProcessor finishes | Job log shows COMPLETED with counts |
| 5 | Job encounters fatal error | Database unavailable | Job log shows FAILED, ExitCode=1, error logged |
| 6 | No policies to process | All policies current | Job completes with 0 processed |

#### Scheduling
- **Task Name**: PI-ExpirationProcessor
- **Schedule**: Daily at 03:00
- **Script**: `deploy/batch-jobs/run-expiration-processor.ps1`

---

### User Story ID: US-BAT-003
**Title**: Policy Renewal Processing
**Priority**: Critical

#### Story
As a system administrator, I want policies approaching expiration to be automatically flagged for renewal so that renewal offers can be generated before policies lapse.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Active policies expiring within 30 days | RenewalProcessor runs (daily at 2 AM) | Policies retrieved via Batch.usp_Renewal_GetDuePolicies |
| 2 | Eligible policy identified | RenewalProcessor processes a policy | Batch.usp_Renewal_ProcessPolicy called, IsRenewal=1 set |
| 3 | Custom days-ahead parameter | Argument "60" passed | Policies within 60 days processed |
| 4 | Individual policy fails | One policy has constraint issue | Error logged, remaining policies continue processing |
| 5 | Job summary accurate | 10 processed, 1 failed | Log shows RecordsProcessed=10, RecordsFailed=1 |
| 6 | Policy already renewed | Higher version exists in QUOTE/ACTIVE | Policy skipped (not re-processed) |

#### Scheduling
- **Task Name**: PI-RenewalProcessor
- **Schedule**: Daily at 02:00
- **Script**: `deploy/batch-jobs/run-renewal-processor.ps1`

---

### User Story ID: US-BAT-004
**Title**: Fraud Scoring Batch
**Priority**: Critical

#### Story
As a claims manager, I want new claims to be automatically scored for fraud indicators so that suspicious claims are referred to SIU without manual intervention.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Open claims reported in last 7 days | FraudScoring runs (daily at 4 AM) | Claims evaluated via Claims.usp_Fraud_EvaluateClaim |
| 2 | Claim scores >= 70 | Fraud evaluation returns high score | Claim referred to SIU ("** SIU REFERRED **" logged) |
| 3 | Claim scores < 70 | Fraud evaluation returns low score | Score recorded, no SIU referral |
| 4 | Custom days-back parameter | Argument "14" passed | Claims from last 14 days evaluated |
| 5 | Individual claim evaluation fails | One claim has data issue | Error logged with ClaimID context, batch continues |
| 6 | Job summary includes referral count | 5 scored, 2 referred, 1 failed | Console shows all three counts |
| 7 | Already-scored claims excluded | Claims with FraudScore > 0 | Not re-evaluated [ASSUMPTION] |

#### Business Rules
- Fraud score threshold for SIU referral: **>= 70** (AppSettings.FraudScoreThreshold)
- Default scoring window: 7 days back

#### Scheduling
- **Task Name**: PI-FraudScoring
- **Schedule**: Daily at 04:00
- **Script**: `deploy/batch-jobs/run-fraud-scoring.ps1`

---

### User Story ID: US-BAT-005
**Title**: Reserve Recalculation and Review
**Priority**: High

#### Story
As a claims supervisor, I want reserves to be periodically reviewed and claims flagged when reserves appear inadequate so that financial projections remain accurate.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Open claims with reserves | ReserveRecalculator runs (weekly at 1 AM) | Claims reviewed against adequacy rules |
| 2 | Reserve < 10% of paid amount | Claim has large payments vs. small reserve | Flagged: "Reserve less than 10% of paid amount" |
| 3 | Aged claim (>180 days) with stale reserve (no change in 90+ days) | Old claim, no recent reserve activity | Flagged: "No reserve change in 90+ days on aged claim" |
| 4 | Reserve exceeds policy limit | Reserve amount > PolicyLimit | Flagged: "Reserve exceeds policy limit" |
| 5 | IBNR calculation performed | After reserve review | Batch.usp_Reserve_CalculateIBNR called with today's date |
| 6 | Net incurred updated | After IBNR | Batch.usp_Reserve_UpdateNetIncurred refreshes all open claims |
| 7 | Flagged claims create activities | Claim meets review criteria | Claims.Activities row: Priority=HIGH, Subject="RESERVE REVIEW REQUIRED" |

#### Scheduling
- **Task Name**: PI-ReserveRecalculator
- **Schedule**: Weekly at 01:00
- **Script**: `deploy/batch-jobs/run-reserve-recalculator.ps1`

---

### User Story ID: US-BAT-006
**Title**: Payment Batch Processing
**Priority**: High

#### Story
As a billing administrator, I want daily payment processing to automatically apply late fees, process auto-pay transactions, and generate overdue notices so that billing operations run without manual intervention.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Overdue invoices exist | PaymentBatch runs (daily at 5 AM) | Late fees applied via Billing.usp_Invoice_ApplyLateFees |
| 2 | Auto-pay EFT accounts configured | PaymentBatch step 2 | EFT transactions processed, payments recorded |
| 3 | EFT reference number generated | Payment recorded | Format: AUTO-yyyyMMdd-{InvoiceID} |
| 4 | EFT payment fails (e.g., insufficient funds) | Individual transaction errors | Error logged, remaining payments continue |
| 5 | Invoices overdue 10+ days | PaymentBatch step 3 | Overdue notices generated |
| 6 | Agent commissions due | PaymentBatch step 4 | Commission payments processed |
| 7 | Job totals accurate | 5 late fees + 10 auto-pays | RecordsProcessed = 15 |

#### Scheduling
- **Task Name**: PI-PaymentBatch
- **Schedule**: Daily at 05:00
- **Script**: `deploy/batch-jobs/run-payment-batch.ps1`

---

### User Story ID: US-BAT-007
**Title**: Reinsurance Allocation
**Priority**: Medium

#### Story
As a reinsurance analyst, I want premiums and losses to be automatically allocated to treaties each month so that cession records and bordereaux reports are current.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Active treaties exist | ReinsuranceAllocator runs (monthly at 6 AM) | Active treaties loaded via Reinsurance.usp_Treaty_GetActive |
| 2 | Unallocated premiums exist | Step 2 processes premiums | Premium cessions created per treaty via Reinsurance.usp_Cession_Create |
| 3 | Unallocated losses exist | Step 3 processes losses | Loss cessions created per treaty |
| 4 | Monthly reports needed | Step 4 | PREMIUM and LOSS bordereaux generated per treaty |
| 5 | Default accounting period | No argument | Previous month (e.g., "2024-01" if run in Feb) |
| 6 | Custom accounting period | Argument "2024-06" | Specified period used |
| 7 | Individual cession failure | One policy/claim errors | Error logged, remaining allocations continue |

#### Scheduling
- **Task Name**: PI-ReinsuranceAllocator
- **Schedule**: Monthly at 06:00
- **Script**: `deploy/batch-jobs/run-reinsurance-allocator.ps1`

---

### User Story ID: US-BAT-008
**Title**: Data Archival
**Priority**: Medium

#### Story
As a system administrator, I want old closed claims to be archived after the retention period so that database performance is maintained and compliance requirements are met.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Closed claims older than 7 years exist | Data Archive job runs (monthly at 11 PM) | Claims marked as archived |
| 2 | Default retention period | No parameter specified | 7 years retention |
| 3 | Custom retention period | @YearsOld=5 | 5-year cutoff used |
| 4 | Only CLOSED claims eligible | Open claims exist | Open claims unaffected regardless of age |
| 5 | Claim closed exactly at cutoff | ClosedDate = 7 years ago exactly | NOT archived (strict less-than comparison) |
| 6 | Archive count reported | 50 claims archived | RecordsArchived=50 |

#### Scheduling
- **Task Name**: PI-DataArchiver
- **Schedule**: Monthly at 23:00
- **Script**: `deploy/batch-jobs/run-data-archiver.ps1`

---

### User Story ID: US-BAT-009
**Title**: Cancellation Notice Processing
**Priority**: High

#### Story
As a billing administrator, I want cancellation notices to be automatically generated for severely overdue invoices so that policyholders are formally notified before cancellation.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | OVERDUE invoices > 30 days past due without notice | CancellationNotice job runs (daily at 8 AM) | CancellationNoticeDate set to today |
| 2 | Notice period configured | SystemConfig: CANCELLATION_NOTICE_DAYS=20 | CancellationEffectiveDate = today + 20 days |
| 3 | Config key missing | No CANCELLATION_NOTICE_DAYS entry | Default 20 days used |
| 4 | Invoice already has notice | CancellationNoticeDate IS NOT NULL | Skipped (not re-noticed) |
| 5 | Invoice only 25 days overdue | DueDate = 25 days ago | Not eligible (must be > 30 days) |
| 6 | Transaction consistency | Multiple invoices updated | All or none (transaction wraps update) |

#### Scheduling
- **Task Name**: PI-CancellationNotice
- **Schedule**: Daily at 08:00
- **Script**: `deploy/batch-jobs/run-cancellation-notice.ps1`

---

### User Story ID: US-BAT-010
**Title**: Batch Job Failure Alerting
**Priority**: High

#### Story
As an operations administrator, I want to be alerted when batch jobs fail so that I can investigate and resolve issues before they impact business operations.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A batch job encounters a fatal error | Exception in Main() catch block | Job log marked FAILED with error message |
| 2 | Error details captured | Exception thrown | ErrorLogger.LogError called with exception and job name |
| 3 | Exit code indicates failure | Fatal error occurs | Environment.ExitCode = 1 |
| 4 | Task Scheduler detects failure | ExitCode != 0 | Task history shows failure [ASSUMPTION] |
| 5 | Job log shows failure in monitor | Job failed | frmBatchJobMonitor shows FAILED status when filtered |
| 6 | Error message preserved | ex.Message = "Connection timeout" | ResultMessage in JobLogs = "Connection timeout" |

#### Form
- **Screen**: frmBatchJobMonitor (for viewing failures)
- **Stored Procedure**: Batch.usp_JobLog_Fail
- **File**: All batch job Program.vb files

---
