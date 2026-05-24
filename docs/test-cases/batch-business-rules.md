# Batch Jobs Module - Business Rules

## Module: BAT (Batch Jobs)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-BAT-001
**Module**: BAT
**Priority**: Critical

#### Rule Description
Fraud score threshold for SIU referral: claims with a fraud score of 70 or greater are automatically referred to the Special Investigations Unit (SIU). The threshold is configurable via AppSettings.FraudScoreThreshold (default 70).

#### Source
- **File**: `src/PropertyInsuranceClaims.BatchJobs/FraudScoring/Program.vb`
- **Method**: Main (inner loop)
- **Code Snippet**:
```vb
If score >= 70 Then
    referred += 1
    Console.WriteLine($" Score: {score:N1} ** SIU REFERRED **")
Else
    Console.WriteLine($" Score: {score:N1}")
End If
```

#### Configuration
- **Setting**: AppSettings.FraudScoreThreshold
- **Default Value**: 70
- **Type**: Decimal (score compared with >= operator)

#### Enforcement Mechanism
- Type: Application-level logic in FraudScoring batch job
- Behavior: Claims with score >= 70 are counted as "referred" and logged with "** SIU REFERRED **"
- Note: The actual SIU referral is handled by Claims.usp_Fraud_EvaluateClaim SP which sets the score and status

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-BAT-003 | Fraud Scoring Pipeline |
| UT-BAT-003 | FraudScoring unit tests |
| SP-BAT-008 | Batch.usp_FraudScoring_Process |
| US-BAT-004 | Fraud Scoring user story |

---

### Rule ID: BR-BAT-002
**Module**: BAT
**Priority**: Critical

#### Rule Description
Policy expiration criteria: a policy is expired when its ExpiryDate is strictly less than today's date AND its current status is ACTIVE. Policies expiring today are NOT expired (they remain active until end of day). The batch updates PolicyStatus to 'EXPIRED' within a single transaction.

#### Source
- **File**: `database/02-stored-procedures/008-batch-job-sps.sql`
- **SP**: Batch.usp_Expiration_Process
- **Code Snippet**:
```sql
UPDATE Policy.Policies SET
    PolicyStatus = 'EXPIRED',
    ModifiedDate = GETDATE(),
    ModifiedBy = @ProcessedBy
WHERE PolicyStatus = 'ACTIVE'
    AND ExpiryDate < CAST(GETDATE() AS DATE);
```

#### Enforcement Mechanism
- Type: Database SP logic (single UPDATE with WHERE clause)
- Behavior: Only ACTIVE policies with ExpiryDate < today are expired
- Transaction: Wrapped in BEGIN/COMMIT TRANSACTION with rollback on error

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-006 | Batch.usp_Expiration_Process |
| FT-BAT-001 | Expiration Processing Pipeline |
| UT-BAT-001 | ExpirationProcessor unit tests |
| US-BAT-002 | Policy Expiration user story |

---

### Rule ID: BR-BAT-003
**Module**: BAT
**Priority**: Critical

#### Rule Description
Renewal eligibility rules: a policy is eligible for renewal processing when ALL of the following conditions are met:
1. PolicyStatus = 'ACTIVE'
2. ExpiryDate <= CutoffDate (today + @DaysAhead, default 60)
3. ExpiryDate > today (not already expired)
4. IsRenewal = 0 (not already flagged for renewal)
5. No newer policy version exists in QUOTE or ACTIVE status

#### Source
- **File**: `database/02-stored-procedures/008-batch-job-sps.sql`
- **SP**: Batch.usp_Renewal_Process
- **Code Snippet**:
```sql
SELECT PolicyID, PolicyNumber
FROM Policy.Policies
WHERE PolicyStatus = 'ACTIVE'
    AND ExpiryDate <= @CutoffDate
    AND ExpiryDate > CAST(GETDATE() AS DATE)
    AND IsRenewal = 0
    AND NOT EXISTS (
        SELECT 1 FROM Policy.Policies p2 
        WHERE p2.PolicyNumber = Policy.Policies.PolicyNumber 
        AND p2.PolicyVersion > Policy.Policies.PolicyVersion
        AND p2.PolicyStatus IN ('QUOTE', 'ACTIVE')
    );
```

#### Configuration
- **Setting**: @DaysAhead parameter
- **Default Value**: 60 (SP default), 30 (RenewalProcessor Program.vb default)
- **Note**: SP defaults to 60 days, but the batch program defaults to 30 days unless overridden by argument

#### Enforcement Mechanism
- Type: Database SP cursor query with multi-condition WHERE clause
- Behavior: Only policies meeting ALL 5 criteria are processed
- Per-policy error handling: individual failures logged, cursor continues

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-004 | Batch.usp_Renewal_Process |
| SP-BAT-005 | Batch.usp_Renewal_GetDuePolicies |
| FT-BAT-002 | Renewal Processing Pipeline |
| UT-BAT-002 | RenewalProcessor unit tests |
| US-BAT-003 | Policy Renewal user story |

---

### Rule ID: BR-BAT-004
**Module**: BAT
**Priority**: High

#### Rule Description
Reserve adequacy review rules: three rules determine if a claim's reserve needs review:
1. Reserve less than 10% of paid amount (currentReserve < totalPaid * 0.1 AND totalPaid > 0)
2. Aged claim with stale reserve (claimAge > 180 days AND no reserve change in 90+ days)
3. Reserve exceeds policy limit (currentReserve > policyLimit AND policyLimit > 0)

Any rule triggering sets needsReview=True and creates a HIGH priority activity.

#### Source
- **File**: `src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb`
- **Method**: Main (Step 2 inner loop)
- **Code Snippet**:
```vb
' Rule 1: Reserve less than paid (negative IBNR)
If currentReserve < totalPaid * 0.1D AndAlso totalPaid > 0 Then
    needsReview = True
    reviewReason = "Reserve less than 10% of paid amount"
End If

' Rule 2: Claim open > 180 days with no reserve change in 90 days
If claimAge > 180 Then
    Dim lastChangeDate As DateTime = CDate(If(IsDBNull(row("LastReserveChange")), DateTime.MinValue, row("LastReserveChange")))
    If (DateTime.Now - lastChangeDate).TotalDays > 90 Then
        needsReview = True
        reviewReason = "No reserve change in 90+ days on aged claim"
    End If
End If

' Rule 3: Reserve exceeds policy limit
Dim policyLimit As Decimal = CDec(If(IsDBNull(row("PolicyLimit")), 0, row("PolicyLimit")))
If currentReserve > policyLimit AndAlso policyLimit > 0 Then
    needsReview = True
    reviewReason = "Reserve exceeds policy limit"
End If
```

#### Thresholds
| Rule | Threshold | Guard Condition |
|------|-----------|-----------------|
| 1 | Reserve < 10% of paid | totalPaid > 0 (avoid division by zero scenario) |
| 2a | Claim age > 180 days | N/A |
| 2b | No reserve change > 90 days | Only checked if Rule 2a passes |
| 3 | Reserve > policy limit | policyLimit > 0 (avoid flagging when limit unknown) |

#### Enforcement Mechanism
- Type: Application-level logic in ReserveRecalculator batch job
- Behavior: Creates activity record via Batch.usp_Reserve_FlagForReview
- Activity: Priority=HIGH, Subject="RESERVE REVIEW REQUIRED", Description=reason
- Note: Last rule to trigger overwrites the reviewReason (not cumulative)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-BAT-004 | Reserve Recalculation Pipeline |
| UT-BAT-004 | ReserveRecalculator unit tests |
| SP-BAT-012 | Batch.usp_Reserve_GetClaimsForReview |
| SP-BAT-013 | Batch.usp_Reserve_FlagForReview |
| US-BAT-005 | Reserve Recalculation user story |

---

### Rule ID: BR-BAT-005
**Module**: BAT
**Priority**: High

#### Rule Description
IBNR (Incurred But Not Reported) calculation methodology: the reserve recalculator performs IBNR calculation as part of its weekly run. The calculation uses an as-of date (typically today) and is executed by Batch.usp_Reserve_CalculateIBNR. The specific IBNR methodology (e.g., chain-ladder, Bornhuetter-Ferguson) is encapsulated in the stored procedure.

#### Source
- **File**: `src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb`
- **Method**: Main (Step 3)
- **Code Snippet**:
```vb
' Step 3: Calculate IBNR reserves
Console.WriteLine("  Step 3: Calculating IBNR...")
Dim ibnrParams() As SqlParameter = {
    DatabaseHelper.CreateParam("@AsOfDate", DateTime.Today),
    DatabaseHelper.CreateParam("@CalculatedBy", "BATCH_RESERVE")
}
DatabaseHelper.ExecuteNonQuery("Batch.usp_Reserve_CalculateIBNR", ibnrParams)
```

- **SP File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP Code**:
```sql
CREATE OR ALTER PROCEDURE Batch.usp_Reserve_CalculateIBNR
    @AsOfDate DATE,
    @CalculatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    PRINT 'IBNR calculation completed as of ' + CAST(@AsOfDate AS VARCHAR(50));
END
```

#### Enforcement Mechanism
- Type: Stored procedure (currently placeholder implementation)
- Behavior: Prints confirmation message; actual calculation logic to be implemented [ASSUMPTION]
- Schedule: Weekly (part of ReserveRecalculator job)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-014 | Batch.usp_Reserve_CalculateIBNR |
| FT-BAT-004 | Reserve Recalculation Pipeline (Step 3) |
| UT-BAT-004 | ReserveRecalculator unit tests |

---

### Rule ID: BR-BAT-006
**Module**: BAT
**Priority**: High

#### Rule Description
Net incurred calculation formula: NetIncurred = TotalReserve + TotalPaid - TotalRecovery. This is recalculated for all open claims (not CLOSED or DENIED) during the reserve recalculation batch. Each component is derived from child tables:
- TotalReserve = SUM of approved reserves (Claims.Reserves WHERE IsApproved=1)
- TotalPaid = SUM of qualifying payments (Claims.Payments WHERE Status IN ('APPROVED', 'ISSUED', 'CLEARED'))
- TotalRecovery = SUM of subrogation recovery amounts (Claims.Subrogation)

#### Source
- **File**: `database/02-stored-procedures/008-batch-job-sps.sql`
- **SP**: Batch.usp_Reserve_Recalculate
- **Code Snippet**:
```sql
UPDATE Claims.Claims SET
    TotalReserve = ISNULL((SELECT SUM(Amount) FROM Claims.Reserves r WHERE r.ClaimID = Claims.Claims.ClaimID AND r.IsApproved = 1), 0),
    TotalPaid = ISNULL((SELECT SUM(Amount) FROM Claims.Payments p WHERE p.ClaimID = Claims.Claims.ClaimID AND p.Status IN ('APPROVED', 'ISSUED', 'CLEARED')), 0),
    TotalRecovery = ISNULL((SELECT SUM(RecoveryAmount) FROM Claims.Subrogation s WHERE s.ClaimID = Claims.Claims.ClaimID), 0),
    NetIncurred = ISNULL(...) + ISNULL(...) - ISNULL(...)
WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED');
```

#### Enforcement Mechanism
- Type: Database SP logic (single UPDATE with correlated subqueries)
- Behavior: Recalculates all four financial totals atomically within a transaction
- NULL handling: ISNULL(..., 0) ensures missing child records result in zero

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-007 | Batch.usp_Reserve_Recalculate |
| DV-BAT-005 | Reserve Recalculation Data Validation |
| FT-BAT-004 | Reserve Recalculation Pipeline |

---

### Rule ID: BR-BAT-007
**Module**: BAT
**Priority**: High

#### Rule Description
Data archival retention period: closed claims older than 7 years (default, configurable via @YearsOld parameter) are eligible for archival. The cutoff is calculated as DATEADD(YEAR, -@YearsOld, today). Only claims with ClaimStatus = 'CLOSED' AND ClosedDate < cutoff are archived. The comparison is strict less-than (claims closed exactly on the cutoff date are NOT archived).

#### Source
- **File**: `database/02-stored-procedures/008-batch-job-sps.sql`
- **SP**: Batch.usp_Data_Archive
- **Code Snippet**:
```sql
DECLARE @CutoffDate DATE = DATEADD(YEAR, -@YearsOld, CAST(GETDATE() AS DATE));

UPDATE Claims.Claims SET
    ModifiedDate = GETDATE(),
    ModifiedBy = @ProcessedBy
WHERE ClaimStatus = 'CLOSED' AND ClosedDate < @CutoffDate;
```

#### Configuration
- **Parameter**: @YearsOld
- **Default Value**: 7
- **Configurable**: Yes, via SP parameter

#### Enforcement Mechanism
- Type: Database SP logic
- Behavior: Currently marks claims as modified (placeholder for actual archival to archive tables)
- Note: Full archival implementation would move records to archive schema [ASSUMPTION]

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-011 | Batch.usp_Data_Archive |
| DV-BAT-008 | Data Archive Validation |
| US-BAT-008 | Data Archival user story |

---

### Rule ID: BR-BAT-008
**Module**: BAT
**Priority**: High

#### Rule Description
Cancellation notice processing criteria: invoices are eligible for cancellation notices when ALL conditions are met:
1. Invoice Status = 'OVERDUE'
2. CancellationNoticeDate IS NULL (not already noticed)
3. DueDate < 30 days ago (seriously overdue)

The cancellation effective date is calculated as today + CANCELLATION_NOTICE_DAYS (default 20 from Admin.SystemConfig).

#### Source
- **File**: `database/02-stored-procedures/008-batch-job-sps.sql`
- **SP**: Batch.usp_CancellationNotice_Process
- **Code Snippet**:
```sql
DECLARE @CancellationNoticeDays INT = 20;
SELECT @CancellationNoticeDays = CAST(ConfigValue AS INT)
FROM Admin.SystemConfig WHERE ConfigKey = 'CANCELLATION_NOTICE_DAYS';

UPDATE Billing.Invoices SET
    CancellationNoticeDate = CAST(GETDATE() AS DATE),
    CancellationEffectiveDate = DATEADD(DAY, @CancellationNoticeDays, CAST(GETDATE() AS DATE)),
    ModifiedDate = GETDATE()
WHERE Status = 'OVERDUE'
    AND CancellationNoticeDate IS NULL
    AND DueDate < DATEADD(DAY, -30, CAST(GETDATE() AS DATE));
```

#### Configuration
- **Setting**: Admin.SystemConfig.CANCELLATION_NOTICE_DAYS
- **Default Value**: 20 (hardcoded fallback if config missing)
- **Behavior**: Number of days between notice and effective cancellation

#### Enforcement Mechanism
- Type: Database SP logic within transaction
- Behavior: Sets notice date and effective cancellation date on qualifying invoices
- Transaction: Wrapped in BEGIN/COMMIT with rollback on error

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-010 | Batch.usp_CancellationNotice_Process |
| DV-BAT-007 | Cancellation Notice Data Validation |
| US-BAT-009 | Cancellation Notice user story |

---

### Rule ID: BR-BAT-009
**Module**: BAT
**Priority**: Critical

#### Rule Description
Job logging lifecycle: every batch job follows the pattern Start -> Complete/Fail. The lifecycle ensures auditability:
1. Start: inserts RUNNING record, returns JobLogID
2. Complete: updates with EndTime, RecordsProcessed, RecordsFailed, Status (COMPLETED or COMPLETED_WITH_ERRORS)
3. Fail: updates with EndTime, FAILED status, ErrorMessage

If LogJobStart fails (e.g., database unavailable), it returns 0 and the batch continues. If LogJobComplete or LogJobFailed fails, exceptions are swallowed silently (the batch does not crash due to logging failures).

#### Source
- **File**: All batch job Program.vb files (common pattern)
- **Code Snippet** (LogJobStart):
```vb
Private Function LogJobStart(jobName As String, parameters As String) As Long
    Try
        Dim params() As SqlParameter = {
            DatabaseHelper.CreateParam("@JobName", jobName),
            DatabaseHelper.CreateParam("@Parameters", parameters),
            DatabaseHelper.CreateParam("@StartTime", DateTime.Now)
        }
        Return CLng(If(DatabaseHelper.ExecuteScalar("Batch.usp_JobLog_Start", params), 0))
    Catch
        Return 0
    End Try
End Function
```

#### Status Values
| Status | Meaning | Set By |
|--------|---------|--------|
| RUNNING | Job in progress | usp_JobLog_Start |
| COMPLETED | Job finished, no failures | usp_JobLog_Complete (RecordsFailed=0) |
| COMPLETED_WITH_ERRORS | Job finished with some failures | usp_JobLog_Complete (RecordsFailed>0) |
| FAILED | Job terminated due to fatal error | usp_JobLog_Fail |

#### Enforcement Mechanism
- Type: Application-level pattern (replicated in each batch job)
- Behavior: Try/Catch in Main() guarantees either Complete or Fail is called
- Resilience: Logging failures are swallowed to prevent cascading failures

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BAT-001 | Batch.usp_JobLog_Start |
| SP-BAT-002 | Batch.usp_JobLog_Complete |
| SP-BAT-003 | Batch.usp_JobLog_Fail |
| UT-BAT-007 | Common LogJobStart pattern |
| UT-BAT-008 | Common LogJobComplete pattern |
| UT-BAT-009 | Common LogJobFailed pattern |
| NFR-BAT-007 | Job Logging Reliability |

---

### Rule ID: BR-BAT-010
**Module**: BAT
**Priority**: High

#### Rule Description
Non-payment cancellation grace period: policies are cancelled for non-payment after a 30-day grace period. The ExpirationProcessor uses @GracePeriodDays=30 when calling Batch.usp_Policy_ProcessCancellations. Additionally, cancellation notices are generated with a 20-day notice period (@NoticeDays=20) via Batch.usp_Policy_GenerateCancelNotices.

#### Source
- **File**: `src/PropertyInsuranceClaims.BatchJobs/ExpirationProcessor/Program.vb`
- **Code Snippet**:
```vb
' Step 2: Process cancellations for non-payment
Dim cancelParams() As SqlParameter = {
    DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_EXPIRATION"),
    DatabaseHelper.CreateParam("@GracePeriodDays", 30)
}
Dim dtCancelled As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Policy_ProcessCancellations", cancelParams)

' Step 3: Send cancellation notices (policies approaching cancellation)
Dim noticeParams() As SqlParameter = {
    DatabaseHelper.CreateParam("@NoticeDays", 20),
    DatabaseHelper.CreateParam("@ProcessedBy", "BATCH_EXPIRATION")
}
Dim dtNotices As DataTable = DatabaseHelper.ExecuteStoredProcedure("Batch.usp_Policy_GenerateCancelNotices", noticeParams)
```

#### Configuration
- **Grace Period**: 30 days (hardcoded in ExpirationProcessor)
- **Notice Period**: 20 days (hardcoded in ExpirationProcessor)

#### Enforcement Mechanism
- Type: Application-level constants passed to stored procedures
- Behavior: Policies with payments overdue > 30 days are cancelled; notices sent 20 days before effective cancellation

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-BAT-001 | Expiration Processing Pipeline |
| UT-BAT-001 | ExpirationProcessor unit tests |
| US-BAT-002 | Policy Expiration user story |

---
