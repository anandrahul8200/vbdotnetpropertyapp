# Batch Jobs Module - Stored Procedure Tests

## Module: BAT (Batch Jobs)
## Source Files:
- `database/02-stored-procedures/008-batch-job-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (15 total):
1. Batch.usp_Renewal_Process
2. Batch.usp_Renewal_GetDuePolicies
3. Batch.usp_Expiration_Process
4. Batch.usp_Reserve_Recalculate
5. Batch.usp_Reserve_GetClaimsForReview
6. Batch.usp_Reserve_FlagForReview
7. Batch.usp_Reserve_CalculateIBNR
8. Batch.usp_Reserve_UpdateNetIncurred
9. Batch.usp_FraudScoring_Process
10. Batch.usp_Fraud_GetClaimsToScore
11. Batch.usp_CancellationNotice_Process
12. Batch.usp_Data_Archive
13. Batch.usp_JobLog_Start
14. Batch.usp_JobLog_Complete
15. Batch.usp_JobLog_Fail

---

### Test Case ID: SP-BAT-001
**Procedure**: Batch.usp_JobLog_Start
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @JobName VARCHAR(100) - Required
- @Parameters VARCHAR(500) - Optional (default NULL)
- @JobLogID BIGINT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Start job log with name and parameters | @JobName='RenewalProcessor', @Parameters='DaysAhead=30' | @JobLogID > 0, row inserted into Batch.JobLogs |
| 2 | Start job log with NULL parameters | @JobName='ExpirationProcessor', @Parameters=NULL | @JobLogID > 0, Parameters column NULL |
| 3 | StartTime defaults to GETDATE() | @JobName='FraudScoring' | StartTime = approximately GETDATE() |
| 4 | Status set to RUNNING | @JobName='PaymentBatch' | Batch.JobLogs.Status = 'RUNNING' |
| 5 | Multiple concurrent job starts | Two calls with different job names | Two distinct JobLogID values returned |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL job name | @JobName=NULL | Constraint violation (NOT NULL) [ASSUMPTION] |
| 2 | Job name exceeds max length | @JobName=101 character string | Truncation or error |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Job name at max length (100 chars) | @JobName=100 character string | Accepted and stored |
| 2 | Parameters at max length (500 chars) | @Parameters=500 character string | Accepted and stored |
| 3 | Empty string job name | @JobName='' | Row inserted with empty name [ASSUMPTION] |

---

### Test Case ID: SP-BAT-002
**Procedure**: Batch.usp_JobLog_Complete
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @JobLogID BIGINT - Required
- @RecordsProcessed INT - Optional (default 0)
- @RecordsFailed INT - Optional (default 0)
- @ResultMessage VARCHAR(500) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Complete job with zero failures | @JobLogID=valid, @RecordsProcessed=100, @RecordsFailed=0 | Status='COMPLETED', EndTime set |
| 2 | Complete job with some failures | @JobLogID=valid, @RecordsProcessed=95, @RecordsFailed=5 | Status='COMPLETED_WITH_ERRORS' |
| 3 | EndTime auto-set to GETDATE() | @JobLogID=valid | EndTime = approximately GETDATE() |
| 4 | ResultMessage stored | @JobLogID=valid, @ResultMessage='Processed all policies' | ResultMessage column populated |
| 5 | Default parameters (0 processed, 0 failed) | @JobLogID=valid only | RecordsProcessed=0, RecordsFailed=0, Status='COMPLETED' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid JobLogID | @JobLogID=999999 (non-existent) | No rows updated (0 affected) |
| 2 | NULL JobLogID | @JobLogID=NULL | Error or no update |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Zero records processed | @RecordsProcessed=0 | Status='COMPLETED' (zero failures) |
| 2 | Large records processed count | @RecordsProcessed=2147483647 | Stored correctly (INT max) |
| 3 | ResultMessage at max length | @ResultMessage=500 chars | Stored correctly |

---

### Test Case ID: SP-BAT-003
**Procedure**: Batch.usp_JobLog_Fail
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @JobLogID BIGINT - Required
- @ErrorMessage VARCHAR(MAX) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Mark job as failed with error message | @JobLogID=valid, @ErrorMessage='Connection timeout' | Status='FAILED', EndTime set, ResultMessage=error |
| 2 | Long error message | @JobLogID=valid, @ErrorMessage=large text | Stored in ResultMessage column |
| 3 | EndTime auto-set | @JobLogID=valid, @ErrorMessage='Error' | EndTime = approximately GETDATE() |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid JobLogID | @JobLogID=999999, @ErrorMessage='err' | No rows updated |
| 2 | NULL error message | @JobLogID=valid, @ErrorMessage=NULL | Row updated with NULL ResultMessage [ASSUMPTION] |

---

### Test Case ID: SP-BAT-004
**Procedure**: Batch.usp_Renewal_Process
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @DaysAhead INT - Optional (default 60)
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @PoliciesProcessed INT OUTPUT
- @PoliciesFailed INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Process renewals with default 60 days | @ProcessedBy='BATCH_RENEWAL' | @PoliciesProcessed > 0 for eligible policies |
| 2 | Custom days ahead | @DaysAhead=30 | Only policies expiring within 30 days processed |
| 3 | Policy marked with IsRenewal=1 | Valid eligible policy | Policy.Policies.IsRenewal = 1, ModifiedDate = GETDATE() |
| 4 | ModifiedBy set to ProcessedBy | @ProcessedBy='BATCH_RENEWAL' | ModifiedBy = 'BATCH_RENEWAL' |
| 5 | Policy already renewed excluded | Policy with higher version in QUOTE/ACTIVE | Not included in cursor |
| 6 | Only ACTIVE policies eligible | Mix of statuses | Only PolicyStatus='ACTIVE' processed |
| 7 | Policy not already flagged for renewal | IsRenewal=0 required | IsRenewal=1 policies excluded |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No eligible policies | @DaysAhead=0 (no policies expire today) | @PoliciesProcessed=0, @PoliciesFailed=0 |
| 2 | Individual policy error during cursor | Policy with constraint violation | @PoliciesFailed incremented, error logged to Audit.ErrorLog |
| 3 | Catastrophic failure | Database error outside cursor | Error logged, THROW re-raises |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | DaysAhead = 0 | @DaysAhead=0 | CutoffDate = today, only policies expiring today |
| 2 | DaysAhead = 365 | @DaysAhead=365 | Policies expiring within 1 year |
| 3 | Policy expiry exactly on cutoff date | ExpiryDate = CutoffDate | Included (ExpiryDate <= @CutoffDate) |
| 4 | Policy expiry exactly today | ExpiryDate = today | Excluded (ExpiryDate > GETDATE()) |

---

### Test Case ID: SP-BAT-005
**Procedure**: Batch.usp_Renewal_GetDuePolicies
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DaysAhead INT - Optional (default 30)
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get policies expiring within 30 days | @DaysAhead=30 | Returns ACTIVE policies with ExpiryDate between now and now+30 days |
| 2 | Returns required columns | Default params | PolicyID, PolicyNumber, PolicyType, ExpiryDate, AnnualPremium, CustomerName, CustomerNumber |
| 3 | Ordered by ExpiryDate | Default params | Results sorted by ExpiryDate ascending |
| 4 | Customer name joined | Valid policy | CustomerName = FirstName + ' ' + LastName |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No policies due | @DaysAhead=0 (none expire today exactly) | Empty result set |
| 2 | Only expired policies exist | All policies already expired | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | DaysAhead = 1 | @DaysAhead=1 | Only policies expiring tomorrow |
| 2 | Policy expiry exactly at boundary | ExpiryDate = DATEADD(DAY, 30, GETDATE()) | Included in results |

---

### Test Case ID: SP-BAT-006
**Procedure**: Batch.usp_Expiration_Process
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @PoliciesExpired INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Expire policies past expiry date | @ProcessedBy='BATCH_EXPIRATION' | Policies with ExpiryDate < today get status EXPIRED |
| 2 | Count returned correctly | Multiple expired policies | @PoliciesExpired = @@ROWCOUNT |
| 3 | ModifiedDate and ModifiedBy set | @ProcessedBy='BATCH_EXPIRATION' | ModifiedDate=GETDATE(), ModifiedBy='BATCH_EXPIRATION' |
| 4 | Transaction committed on success | Valid execution | All updates committed |
| 5 | Only ACTIVE policies affected | Mix of CANCELLED/EXPIRED/ACTIVE | Only PolicyStatus='ACTIVE' updated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No policies to expire | All policies have future expiry dates | @PoliciesExpired=0 |
| 2 | Database error during update | Simulated constraint failure | Transaction rolled back, error logged to Audit.ErrorLog, THROW |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Policy expiring today | ExpiryDate = today | NOT expired (condition is ExpiryDate < today) |
| 2 | Policy expired yesterday | ExpiryDate = yesterday | Expired |
| 3 | Large batch of expirations | 10000 policies eligible | All updated in single transaction |

---

### Test Case ID: SP-BAT-007
**Procedure**: Batch.usp_Reserve_Recalculate
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @ClaimsUpdated INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Recalculate reserves for open claims | @ProcessedBy='BATCH_RESERVE' | TotalReserve = SUM(approved reserves), TotalPaid = SUM(approved/issued/cleared payments) |
| 2 | TotalRecovery calculated | Claims with subrogation | TotalRecovery = SUM(RecoveryAmount from Claims.Subrogation) |
| 3 | NetIncurred formula | Valid claim | NetIncurred = TotalReserve + TotalPaid - TotalRecovery |
| 4 | ModifiedDate and ModifiedBy set | @ProcessedBy='BATCH_RESERVE' | ModifiedDate=GETDATE(), ModifiedBy='BATCH_RESERVE' |
| 5 | Only open claims updated | Mix of open/closed/denied | ClaimStatus NOT IN ('CLOSED', 'DENIED') only |
| 6 | NULL reserves treated as zero | Claim with no reserve rows | TotalReserve = 0 (ISNULL handling) |
| 7 | Count returned correctly | Multiple claims | @ClaimsUpdated = @@ROWCOUNT |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No open claims | All claims closed/denied | @ClaimsUpdated=0 |
| 2 | Database error | Simulated failure | Transaction rolled back, error logged, THROW |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Claim with zero reserves and zero payments | No child records | All totals = 0, NetIncurred = 0 |
| 2 | Very large reserve amounts | TotalReserve = DECIMAL(18,2) max | Calculated correctly without overflow |
| 3 | Negative recovery (negative subrogation) | Negative RecoveryAmount | NetIncurred increased [ASSUMPTION] |

---

### Test Case ID: SP-BAT-008
**Procedure**: Batch.usp_FraudScoring_Process
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @ClaimsScored INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Score unscored claims from last 7 days | @ProcessedBy='BATCH_FRAUD' | Claims with FraudScore=0 and ReportedDate >= 7 days ago processed |
| 2 | Calls Claims.usp_Fraud_EvaluateClaim for each | Valid claims | Each claim gets fraud evaluation |
| 3 | ClaimsScored incremented for each success | 5 claims processed | @ClaimsScored=5 |
| 4 | Only open claims scored | Mix of open/closed/denied | ClaimStatus NOT IN ('CLOSED', 'DENIED') |
| 5 | Only unscored claims (FraudScore=0) | Mix of scored/unscored | FraudScore=0 filter applied |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No claims to score | All claims already scored | @ClaimsScored=0 |
| 2 | Individual claim evaluation fails | One claim errors | Error logged to Audit.ErrorLog, cursor continues |
| 3 | Catastrophic failure outside cursor | Database down | Error logged, THROW re-raises |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Claim reported exactly 7 days ago | ReportedDate = DATEADD(DAY, -7, GETDATE()) | Included |
| 2 | Claim reported 8 days ago | ReportedDate = DATEADD(DAY, -8, GETDATE()) | Excluded |
| 3 | Large batch of unscored claims | 500 claims | All processed via cursor |

---

### Test Case ID: SP-BAT-009
**Procedure**: Batch.usp_Fraud_GetClaimsToScore
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DaysBack INT - Optional (default 7)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get claims reported in last 7 days | @DaysBack=7 | Claims with ReportedDate >= 7 days ago |
| 2 | Returns ClaimID and ClaimNumber | Default params | Two columns returned |
| 3 | Excludes closed/denied claims | Mix of statuses | ClaimStatus NOT IN ('CLOSED', 'DENIED') |
| 4 | Ordered by ReportedDate DESC | Multiple claims | Most recent first |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No recent claims | @DaysBack=0 | Empty result set |
| 2 | All claims closed/denied | No open claims in window | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | DaysBack = 1 | @DaysBack=1 | Only claims from last 24 hours |
| 2 | DaysBack = 365 | @DaysBack=365 | Claims from entire year |
| 3 | Claim reported exactly at boundary | ReportedDate = DATEADD(DAY, -7, GETDATE()) | Included |

---

### Test Case ID: SP-BAT-010
**Procedure**: Batch.usp_CancellationNotice_Process
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @NoticesSent INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Generate cancellation notices for overdue invoices | @ProcessedBy='BATCH_CANCEL' | Overdue invoices > 30 days past due get notice dates set |
| 2 | CancellationNoticeDays from SystemConfig | Config key 'CANCELLATION_NOTICE_DAYS' | Default 20 days used for CancellationEffectiveDate |
| 3 | CancellationNoticeDate set to today | Valid overdue invoices | CancellationNoticeDate = CAST(GETDATE() AS DATE) |
| 4 | CancellationEffectiveDate calculated | @CancellationNoticeDays=20 | CancellationEffectiveDate = today + 20 days |
| 5 | Only invoices without existing notice | Mix of noticed/un-noticed | CancellationNoticeDate IS NULL filter |
| 6 | Only OVERDUE status invoices | Mix of statuses | Status = 'OVERDUE' filter |
| 7 | Only invoices > 30 days past due | Mix of due dates | DueDate < DATEADD(DAY, -30, today) |
| 8 | Transaction committed on success | Valid execution | All updates committed |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No eligible invoices | All invoices current or already noticed | @NoticesSent=0 |
| 2 | Config key missing | 'CANCELLATION_NOTICE_DAYS' not in SystemConfig | Default @CancellationNoticeDays=20 used |
| 3 | Database error | Simulated failure | Transaction rolled back, error logged, THROW |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Invoice exactly 30 days overdue | DueDate = 30 days ago | NOT included (condition is > 30 days) |
| 2 | Invoice 31 days overdue | DueDate = 31 days ago | Included |

---

### Test Case ID: SP-BAT-011
**Procedure**: Batch.usp_Data_Archive
**Source**: `database/02-stored-procedures/008-batch-job-sps.sql`
**Parameters**:
- @YearsOld INT - Optional (default 7)
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @RecordsArchived INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Archive closed claims older than 7 years | @YearsOld=7, @ProcessedBy='BATCH_ARCHIVE' | Claims with ClaimStatus='CLOSED' and ClosedDate < cutoff updated |
| 2 | ModifiedDate and ModifiedBy set | @ProcessedBy='BATCH_ARCHIVE' | ModifiedDate=GETDATE(), ModifiedBy='BATCH_ARCHIVE' |
| 3 | RecordsArchived count returned | Multiple eligible claims | @RecordsArchived = @@ROWCOUNT |
| 4 | Custom retention period | @YearsOld=5 | CutoffDate = 5 years ago |
| 5 | Only CLOSED claims archived | Mix of open/closed | ClaimStatus='CLOSED' filter |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No eligible claims | All claims closed less than 7 years ago | @RecordsArchived=0 |
| 2 | Database error | Simulated failure | Error logged to Audit.ErrorLog, THROW |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Claim closed exactly 7 years ago | ClosedDate = exactly cutoff | NOT archived (condition is < cutoff) |
| 2 | Claim closed 7 years + 1 day ago | ClosedDate = cutoff - 1 day | Archived |
| 3 | YearsOld = 0 | @YearsOld=0 | CutoffDate = today, all closed claims archived |

---

### Test Case ID: SP-BAT-012
**Procedure**: Batch.usp_Reserve_GetClaimsForReview
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ProcessedBy VARCHAR(50) - Optional (default 'SYSTEM')

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get open claims with reserves | @ProcessedBy='BATCH_RESERVE' | Claims with TotalReserve > 0 and open status returned |
| 2 | Returns required columns | Default params | ClaimID, ClaimNumber, TotalReserve, TotalPaid, PolicyLimit, ClaimAgeDays, LastReserveChange |
| 3 | ClaimAgeDays calculated | Valid claims | DATEDIFF(DAY, ReportedDate, GETDATE()) |
| 4 | LastReserveChange from reserves table | Claim with reserve history | MAX(CreatedDate) from Claims.Reserves |
| 5 | Ordered by NetIncurred DESC | Multiple claims | Highest net incurred first |
| 6 | Excludes closed/denied claims | Mix of statuses | ClaimStatus NOT IN ('CLOSED', 'DENIED') |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No claims with reserves | All claims have TotalReserve=0 | Empty result set |
| 2 | All claims closed | No open claims | Empty result set |

---

### Test Case ID: SP-BAT-013
**Procedure**: Batch.usp_Reserve_FlagForReview
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @ReviewReason VARCHAR(200) - Required
- @FlaggedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Flag claim for review | @ClaimID=valid, @ReviewReason='Reserve exceeds policy limit', @FlaggedBy='BATCH_RESERVE' | Activity row inserted in Claims.Activities |
| 2 | Activity type set to NOTE | Valid params | ActivityType='NOTE' |
| 3 | Subject set to standard text | Valid params | Subject='RESERVE REVIEW REQUIRED' |
| 4 | Priority set to HIGH | Valid params | Priority='HIGH' |
| 5 | Description contains reason | @ReviewReason='test reason' | Description='test reason' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid ClaimID | @ClaimID=999999 (non-existent) | FK violation or orphaned activity [ASSUMPTION] |
| 2 | NULL review reason | @ReviewReason=NULL | NULL stored in Description [ASSUMPTION] |

---

### Test Case ID: SP-BAT-014
**Procedure**: Batch.usp_Reserve_CalculateIBNR
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AsOfDate DATE - Required
- @CalculatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Calculate IBNR as of date | @AsOfDate='2024-12-31', @CalculatedBy='BATCH_RESERVE' | IBNR calculation completed (PRINT output) |
| 2 | Current date calculation | @AsOfDate=today, @CalculatedBy='BATCH_RESERVE' | Executes without error |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Future date | @AsOfDate = 1 year from now | Executes (no date validation in current implementation) [ASSUMPTION] |
| 2 | NULL date | @AsOfDate=NULL | Error or uses NULL [ASSUMPTION] |

---

### Test Case ID: SP-BAT-015
**Procedure**: Batch.usp_Reserve_UpdateNetIncurred
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update net incurred for all open claims | (none) | NetIncurred = TotalReserve + TotalPaid - TotalRecovery |
| 2 | Only open claims updated | Mix of statuses | ClaimStatus NOT IN ('CLOSED', 'DENIED') |
| 3 | Row count printed | Multiple claims | PRINT message with @@ROWCOUNT |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No open claims | All closed/denied | 0 rows updated |

---
