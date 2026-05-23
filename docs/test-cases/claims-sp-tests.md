# Claims Module - Stored Procedure Tests

## Module: CLM (Claims)
## Source Files:
- `database/02-stored-procedures/003-claims-processing-sps.sql`
- `database/02-stored-procedures/004-claims-specialized-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (30 total):
1. Claims.usp_Claim_Create
2. Claims.usp_Claim_UpdateStatus
3. Claims.usp_Claim_SetReserve
4. Claims.usp_Claim_CreatePayment
5. Claims.usp_Claim_ApprovePayment
6. Claims.usp_Claim_VoidPayment
7. Claims.usp_Claim_CreateActivity
8. Claims.usp_Claim_CompleteActivity
9. Claims.usp_Claim_Assign
10. Claims.usp_Claim_GetDetails
11. Claims.usp_Claim_Search
12. Claims.usp_Claim_VerifyCoverage
13. Claims.usp_Claim_GetDashboard
14. Claims.usp_Fraud_EvaluateClaim
15. Claims.usp_Fraud_ReferToSIU
16. Claims.usp_Fraud_GetEvaluation
17. Claims.usp_Subrogation_Create
18. Claims.usp_Subrogation_UpdateStatus
19. Claims.usp_Subrogation_RecordRecovery
20. Claims.usp_Subrogation_GetByClaim
21. Claims.usp_Catastrophe_Create
22. Claims.usp_Catastrophe_LinkClaim
23. Claims.usp_Catastrophe_GetSummary
24. Claims.usp_Catastrophe_GetAll
25. Claims.usp_Catastrophe_Close
26. Claims.usp_Vendor_Create
27. Claims.usp_Vendor_Search
28. Claims.usp_Litigation_GetOpen
29. Claims.usp_Litigation_Update
30. Claims.usp_Assignment_GetByClaim

---

### Test Case ID: SP-CLM-001
**Procedure**: Claims.usp_Claim_Create
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @ClaimType VARCHAR(30) - Required
- @LossDate DATETIME - Required
- @LossDescription VARCHAR(MAX) - Required
- @LossLocation VARCHAR(500) - Optional
- @EstimatedLoss DECIMAL(18,2) - Optional
- @PoliceReportNumber VARCHAR(50) - Optional
- @FireReportNumber VARCHAR(50) - Optional
- @WeatherCondition VARCHAR(50) - Optional
- @PointOfOrigin VARCHAR(200) - Optional
- @CatastropheID INT - Optional
- @Priority VARCHAR(10) - Optional (default 'NORMAL')
- @CreatedBy VARCHAR(50) - Required
- @ClaimID INT OUTPUT
- @ClaimNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create claim on active policy | @PolicyID=valid active policy, @ClaimType='FIRE', @LossDate=within period, @LossDescription='desc', @CreatedBy='testuser' | ClaimID > 0, ClaimNumber starts with 'CLM', status = 'FNOL' |
| 2 | Create claim on PENDING_CANCEL policy | @PolicyID=pending cancel policy, valid loss date | Claim created successfully |
| 3 | Create claim with estimated loss > $100K | @EstimatedLoss=150000 | Complexity = 'COMPLEX' |
| 4 | Create claim with estimated loss > $25K | @EstimatedLoss=50000 | Complexity = 'MODERATE' |
| 5 | Create claim with estimated loss <= $25K | @EstimatedLoss=10000 | Complexity = 'SIMPLE' |
| 6 | Create claim with NULL estimated loss | @EstimatedLoss=NULL | Complexity = 'SIMPLE' |
| 7 | Create claim linked to catastrophe | @CatastropheID=valid ID | Claim.CatastropheID set |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |
| 2 | Policy not active (CANCELLED) | @PolicyID=cancelled policy | 'Policy is not active. Current status: CANCELLED' |
| 3 | Policy not active (EXPIRED) | @PolicyID=expired policy | 'Policy is not active. Current status: EXPIRED' |
| 4 | Loss date before policy effective | @LossDate=EffectiveDate - 1 day | 'Loss date is outside the policy period' |
| 5 | Loss date after policy expiry | @LossDate=ExpiryDate + 1 day | 'Loss date is outside the policy period' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Loss date = EffectiveDate exactly | @LossDate=EffectiveDate | Accepted |
| 2 | Loss date = ExpiryDate exactly | @LossDate=ExpiryDate | Accepted |
| 3 | EstimatedLoss = $100,000.00 | @EstimatedLoss=100000 | Complexity = 'MODERATE' (> not >=) |
| 4 | EstimatedLoss = $25,000.00 | @EstimatedLoss=25000 | Complexity = 'SIMPLE' (> not >=) |
| 5 | EstimatedLoss = $100,000.01 | @EstimatedLoss=100000.01 | Complexity = 'COMPLEX' |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | StatusHistory created | Claims.StatusHistory accessible | Valid claim | Row with NewStatus='FNOL', PreviousStatus=NULL |
| 2 | Activity logged | Claims.Activities accessible | Valid claim | Row with ActivityType='NOTE', Subject='FNOL Received' |
| 3 | Audit log created | Audit.AuditLog accessible | Valid claim | Row with Action='INSERT', TableName='Claims.Claims' |
| 4 | ClaimNumber uniqueness | Existing claims in DB | Valid data | Unique sequential ClaimNumber |

---

### Test Case ID: SP-CLM-002
**Procedure**: Claims.usp_Claim_UpdateStatus
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @NewStatus VARCHAR(20) - Required
- @Reason VARCHAR(500) - Optional
- @Notes VARCHAR(MAX) - Optional
- @DenialReason VARCHAR(500) - Optional
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | FNOL to ASSIGNED | @ClaimID=FNOL claim, @NewStatus='ASSIGNED' | Status updated, AssignedDate set |
| 2 | ASSIGNED to INVESTIGATING | @ClaimID=ASSIGNED claim, @NewStatus='INVESTIGATING' | Status updated |
| 3 | INVESTIGATING to ASSESSED | Valid claim, @NewStatus='ASSESSED' | Status updated |
| 4 | ASSESSED to APPROVED | Valid claim, @NewStatus='APPROVED' | Status updated |
| 5 | APPROVED to SETTLED | Valid claim, @NewStatus='SETTLED' | Status updated |
| 6 | SETTLED to CLOSED | Valid claim, @NewStatus='CLOSED' | Status updated, ClosedDate set |
| 7 | CLOSED to REOPENED | Valid claim, @NewStatus='REOPENED', @Reason='New evidence' | ReopenedDate set, ReopenedReason stored |
| 8 | REOPENED to INVESTIGATING | Valid claim | Status updated |
| 9 | INVESTIGATING to LITIGATION | Valid claim | Status updated |
| 10 | DENIED to REOPENED | Valid claim, @Reason='Appeal' | ReopenedDate set |
| 11 | Transition to DENIED with reason | @NewStatus='DENIED', @DenialReason='Not covered' | DenialReason stored |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |
| 2 | FNOL to INVESTIGATING (invalid) | FNOL claim, @NewStatus='INVESTIGATING' | 'Invalid status transition from FNOL to INVESTIGATING' |
| 3 | CLOSED to ASSIGNED (invalid) | CLOSED claim, @NewStatus='ASSIGNED' | 'Invalid status transition from CLOSED to ASSIGNED' |
| 4 | APPROVED to FNOL (invalid) | APPROVED claim, @NewStatus='FNOL' | 'Invalid status transition from APPROVED to FNOL' |
| 5 | REOPENED to CLOSED (invalid) | REOPENED claim, @NewStatus='CLOSED' | 'Invalid status transition from REOPENED to CLOSED' |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | StatusHistory logged | Valid transition | Any valid change | Row in Claims.StatusHistory with PreviousStatus and NewStatus |
| 2 | Activity created | Valid transition | Any valid change | Row with ActivityType='STATUS_CHANGE' |
| 3 | AuditLog entry | Valid transition | Any valid change | Row with FieldName='ClaimStatus' |

---

### Test Case ID: SP-CLM-003
**Procedure**: Claims.usp_Claim_SetReserve
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @ReserveType VARCHAR(20) - Required
- @ReserveCategory VARCHAR(30) - Optional (default 'INDEMNITY')
- @CoverageCode VARCHAR(20) - Optional
- @Amount DECIMAL(18,2) - Required
- @ChangeReason VARCHAR(200) - Optional
- @SetBy VARCHAR(50) - Required
- @ReserveID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Set initial reserve <= $50K | @Amount=25000, @ReserveType='CASE' | ReserveID > 0, IsApproved=1, ApprovalRequired=0 |
| 2 | Set reserve > $50K | @Amount=75000 | ReserveID > 0, IsApproved=0, ApprovalRequired=1 |
| 3 | Change existing reserve | Same type/category, new amount | PreviousAmount populated, ChangeAmount calculated |
| 4 | Reserve on INVESTIGATING claim | Valid claim in INVESTIGATING | Created successfully |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |
| 2 | Reserve on CLOSED claim | @ClaimID=closed claim | 'Cannot set reserve on a closed or denied claim' |
| 3 | Reserve on DENIED claim | @ClaimID=denied claim | 'Cannot set reserve on a closed or denied claim' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Amount = $50,000.00 exactly | @Amount=50000 | ApprovalRequired=0 (uses > not >=) |
| 2 | Amount = $50,000.01 | @Amount=50000.01 | ApprovalRequired=1 |
| 3 | Amount = $0 | @Amount=0 | Accepted, ChangeAmount negative if previous > 0 |

---

### Test Case ID: SP-CLM-004
**Procedure**: Claims.usp_Claim_CreatePayment
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @PaymentType VARCHAR(20) - Required
- @PaymentMethod VARCHAR(20) - Optional (default 'CHECK')
- @PayeeType VARCHAR(20) - Required
- @PayeeName VARCHAR(200) - Required
- @PayeeAddress VARCHAR(500) - Optional
- @Amount DECIMAL(18,2) - Required
- @CoverageCode VARCHAR(20) - Optional
- @InvoiceNumber VARCHAR(50) - Optional
- @Description VARCHAR(500) - Optional
- @TaxReportable BIT - Optional (default 0)
- @CreatedBy VARCHAR(50) - Required
- @PaymentID INT OUTPUT
- @PaymentNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Payment <= $10K (auto-approved) | @Amount=5000, @PayeeType='INSURED' | Status='APPROVED', TotalPaid updated |
| 2 | Payment > $10K (pending) | @Amount=15000 | Status='PENDING', TotalPaid NOT updated |
| 3 | 1099 required (vendor >= $600) | @TaxReportable=1, @PayeeType='VENDOR', @Amount=600 | Form1099Required=1 |
| 4 | 1099 required (attorney >= $600) | @TaxReportable=1, @PayeeType='ATTORNEY', @Amount=1000 | Form1099Required=1 |
| 5 | 1099 not required (insured) | @TaxReportable=1, @PayeeType='INSURED', @Amount=1000 | Form1099Required=0 |
| 6 | Payment at policy limit exactly | @Amount = PolicyLimit - TotalPaid | Accepted (uses > not >=) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |
| 2 | Claim in FNOL status | @ClaimID=FNOL claim | 'Cannot create payment on claim with status: FNOL' |
| 3 | Claim in CLOSED status | @ClaimID=closed claim | 'Cannot create payment on claim with status: CLOSED' |
| 4 | Claim in DENIED status | @ClaimID=denied claim | 'Cannot create payment on claim with status: DENIED' |
| 5 | Exceeds policy limit | @Amount > PolicyLimit - TotalPaid | 'Payment would exceed policy limit...' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Amount = $10,000.00 exactly | @Amount=10000 | Auto-approved (uses > not >=) |
| 2 | Amount = $10,000.01 | @Amount=10000.01 | Pending approval |
| 3 | Amount = $599.99 with vendor tax | @TaxReportable=1, @PayeeType='VENDOR', @Amount=599.99 | Form1099Required=0 |
| 4 | Amount = $600.00 with vendor tax | @TaxReportable=1, @PayeeType='VENDOR', @Amount=600 | Form1099Required=1 |

---

### Test Case ID: SP-CLM-005
**Procedure**: Claims.usp_Claim_ApprovePayment
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @PaymentID INT - Required
- @ApprovedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Approve pending payment | @PaymentID=pending payment | Status='APPROVED', ApprovedDate set, TotalPaid updated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Payment not found | @PaymentID=99999 | 'Payment not found: 99999' |
| 2 | Payment not PENDING | @PaymentID=approved payment | 'Payment is not in PENDING status. Current: APPROVED' |

---

### Test Case ID: SP-CLM-006
**Procedure**: Claims.usp_Claim_VoidPayment
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @PaymentID INT - Required
- @VoidReason VARCHAR(200) - Required
- @VoidedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Void APPROVED payment | @PaymentID=approved payment, @VoidReason='Duplicate' | Status='VOIDED', TotalPaid reduced by Amount |
| 2 | Void ISSUED payment | @PaymentID=issued payment | Status='VOIDED', TotalPaid reduced |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Payment not found | @PaymentID=99999 | 'Payment not found: 99999' |
| 2 | Void PENDING payment | @PaymentID=pending | 'Only APPROVED or ISSUED payments can be voided. Current: PENDING' |
| 3 | Void already VOIDED | @PaymentID=voided | 'Only APPROVED or ISSUED payments can be voided. Current: VOIDED' |

---

### Test Case ID: SP-CLM-007
**Procedure**: Claims.usp_Claim_CreateActivity
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @ActivityType VARCHAR(30) - Required
- @Subject VARCHAR(200) - Required
- @Description VARCHAR(MAX) - Optional
- @DueDate DATETIME - Optional
- @ContactName VARCHAR(200) - Optional
- @ContactPhone VARCHAR(20) - Optional
- @Duration INT - Optional
- @AssignedTo VARCHAR(50) - Optional
- @Priority VARCHAR(10) - Optional (default 'NORMAL')
- @ReminderDate DATETIME - Optional
- @CreatedBy VARCHAR(50) - Required
- @ActivityID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create note activity | @ActivityType='NOTE', @Subject='test' | ActivityID > 0, IsCompleted=0 |
| 2 | Create with due date | @DueDate=future date | Activity created with DueDate |
| 3 | Create with assignment | @AssignedTo='adjuster1' | AssignedTo populated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-008
**Procedure**: Claims.usp_Claim_CompleteActivity
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ActivityID INT - Required
- @CompletedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Complete open activity | @ActivityID=open activity | IsCompleted=1, CompletedDate set |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Activity not found | @ActivityID=99999 | 'Activity not found or already completed: 99999' |
| 2 | Already completed | @ActivityID=completed activity | 'Activity not found or already completed' |

---

### Test Case ID: SP-CLM-009
**Procedure**: Claims.usp_Claim_Assign
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @AssigneeType VARCHAR(20) - Required
- @AssigneeID INT - Required
- @DueDate DATETIME - Optional
- @Instructions VARCHAR(MAX) - Optional
- @CreatedBy VARCHAR(50) - Required
- @AssignmentID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Assign adjuster to FNOL claim | @AssigneeType='ADJUSTER', FNOL claim | AssignmentID > 0, ClaimStatus changes to 'ASSIGNED' |
| 2 | Assign adjuster to non-FNOL claim | INVESTIGATING claim | AssignmentID > 0, ClaimStatus unchanged |
| 3 | Assign vendor | @AssigneeType='VENDOR' | AssignmentID > 0, Vendor.TotalAssignments incremented |
| 4 | Reassign adjuster | Existing adjuster assigned | Old assignment='REASSIGNED', new one created |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-010
**Procedure**: Claims.usp_Claim_GetDetails
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get existing claim details | @ClaimID=valid | 7 result sets returned (header, coverages, reserves, payments, activities, status history, assignments) |
| 2 | Claim with no payments | @ClaimID=claim without payments | Empty payments result set |

---

### Test Case ID: SP-CLM-011
**Procedure**: Claims.usp_Claim_Search
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimNumber VARCHAR(20) - Optional
- @PolicyNumber VARCHAR(20) - Optional
- @CustomerName VARCHAR(200) - Optional
- @ClaimStatus VARCHAR(20) - Optional
- @ClaimType VARCHAR(30) - Optional
- @LossDateFrom DATE - Optional
- @LossDateTo DATE - Optional
- @AdjusterID INT - Optional
- @CatastropheID INT - Optional
- @Priority VARCHAR(10) - Optional
- @MinAmount DECIMAL(18,2) - Optional
- @MaxAmount DECIMAL(18,2) - Optional
- @PageNumber INT - Optional (default 1)
- @PageSize INT - Optional (default 50)
- @SortColumn VARCHAR(50) - Optional (default 'ReportedDate')
- @SortDirection VARCHAR(4) - Optional (default 'DESC')
- @TotalRecords INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by claim number | @ClaimNumber='CLM0000001' | Matching claim returned, TotalRecords=1 |
| 2 | Search by status | @ClaimStatus='FNOL' | All FNOL claims returned |
| 3 | Search by date range | @LossDateFrom, @LossDateTo | Claims within range |
| 4 | Pagination | @PageNumber=2, @PageSize=10 | Second page of results |
| 5 | No criteria (all claims) | All NULL | All claims, paginated |

---

### Test Case ID: SP-CLM-012
**Procedure**: Claims.usp_Claim_VerifyCoverage
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @VerifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Verify with matching peril | Claim type matches coverage perils | ClaimCoverages linked, CoverageVerified=1 |
| 2 | Verify with no peril match | No matching perils | Falls back to broad coverages (DWELLING, etc.) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-013
**Procedure**: Claims.usp_Claim_GetDashboard
**Source**: `database/02-stored-procedures/003-claims-processing-sps.sql`
**Parameters**:
- @AdjusterID INT - Optional
- @AsOfDate DATE - Optional

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Dashboard for all adjusters | @AdjusterID=NULL | 5 result sets (by status, new claims, overdue, pending payments, high priority) |
| 2 | Dashboard for specific adjuster | @AdjusterID=valid | Filtered to adjuster's claims only |

---

### Test Case ID: SP-CLM-014
**Procedure**: Claims.usp_Fraud_EvaluateClaim
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @EvaluatedBy VARCHAR(50) - Required
- @FraudScore DECIMAL(6,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Evaluate with no triggers | Clean claim | FraudScore = 0 or low, no SIU referral |
| 2 | Evaluate with score >= 70 | Claim triggering multiple indicators | FraudScore >= 70, IsSIUReferred=1, SIUReferralDate set |
| 3 | LATE_REPORT triggered | Reported > 30 days after loss | Indicator triggered with days noted |
| 4 | WEEKEND_LOSS triggered | Loss on Saturday/Sunday | Indicator triggered |
| 5 | NEW_POLICY_CLAIM triggered | Claim within 60 days of inception | Indicator triggered |
| 6 | PRIOR_CLAIMS triggered | Customer has 3+ claims in 3 years | Indicator triggered |
| 7 | NO_POLICE_REPORT triggered | THEFT claim without police report | Indicator triggered |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-015
**Procedure**: Claims.usp_Fraud_ReferToSIU
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @ReferralReason VARCHAR(500) - Required
- @ReferredBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Manual SIU referral | Valid claim, @ReferralReason='Suspicious pattern' | IsSIUReferred=1, SIU assignment created |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-016
**Procedure**: Claims.usp_Fraud_GetEvaluation
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @ClaimID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get evaluation for scored claim | @ClaimID=evaluated claim | 2 result sets (summary, indicator details) |

---

### Test Case ID: SP-CLM-017
**Procedure**: Claims.usp_Subrogation_Create
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @ResponsibleParty VARCHAR(200) - Required
- @ResponsiblePartyInsurer VARCHAR(200) - Optional
- @ResponsiblePartyPolicy VARCHAR(50) - Optional
- @DemandAmount DECIMAL(18,2) - Optional
- @Notes VARCHAR(MAX) - Optional
- @CreatedBy VARCHAR(50) - Required
- @SubrogationID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create subrogation | Valid claim, @ResponsibleParty='John Doe' | SubrogationID > 0, Claim.IsSubrogation=1, SubrogationStatus='IDENTIFIED' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |

---

### Test Case ID: SP-CLM-018
**Procedure**: Claims.usp_Subrogation_UpdateStatus
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @SubrogationID INT - Required
- @NewStatus VARCHAR(20) - Required
- @DemandAmount DECIMAL(18,2) - Optional
- @DemandDate DATE - Optional
- @SettlementAmount DECIMAL(18,2) - Optional
- @SettlementDate DATE - Optional
- @ArbitrationDate DATE - Optional
- @ArbitrationResult VARCHAR(200) - Optional
- @Notes VARCHAR(MAX) - Optional
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update to DEMAND_SENT | @NewStatus='DEMAND_SENT', @DemandAmount=5000 | Status updated, DemandAmount set |
| 2 | Update to SETTLED | @NewStatus='SETTLED', @SettlementAmount=3000 | Status updated, SettlementAmount set |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Record not found | @SubrogationID=99999 | 'Subrogation record not found: 99999' |

---

### Test Case ID: SP-CLM-019
**Procedure**: Claims.usp_Subrogation_RecordRecovery
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @SubrogationID INT - Required
- @RecoveryAmount DECIMAL(18,2) - Required
- @RecoveryDate DATE - Optional
- @Notes VARCHAR(MAX) - Optional
- @RecordedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Record recovery | @RecoveryAmount=5000 | Subrogation.RecoveryAmount incremented, Claim.TotalRecovery and TotalSubrogation updated, NetIncurred recalculated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Record not found | @SubrogationID=99999 | 'Subrogation record not found: 99999' |

---

### Test Case ID: SP-CLM-020
**Procedure**: Claims.usp_Subrogation_GetByClaim
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ClaimID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get subrogation records | @ClaimID=claim with subrogation | Records ordered by CreatedDate DESC |

---

### Test Case ID: SP-CLM-021
**Procedure**: Claims.usp_Catastrophe_Create
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @CatastropheName VARCHAR(200) - Required
- @CatastropheType VARCHAR(30) - Required
- @EventDate DATE - Required
- @EndDate DATE - Optional
- @AffectedStates VARCHAR(200) - Optional
- @AffectedZipCodes VARCHAR(MAX) - Optional
- @EstimatedIndustryLoss DECIMAL(18,2) - Optional
- @PCSNumber VARCHAR(20) - Optional
- @CreatedBy VARCHAR(50) - Required
- @CatastropheID INT OUTPUT
- @CatastropheNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create catastrophe | @CatastropheName='Hurricane Test', @CatastropheType='HURRICANE', @EventDate='2024-01-01' | CatastropheID > 0, CatastropheNumber format CAT+7 digits, IsActive=1 |

---

### Test Case ID: SP-CLM-022
**Procedure**: Claims.usp_Catastrophe_LinkClaim
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @CatastropheID INT - Required
- @LinkedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Link claim to active catastrophe | Valid claim and active catastrophe | Claim.CatastropheID set, catastrophe counts updated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Claim not found | @ClaimID=99999 | 'Claim not found: 99999' |
| 2 | Inactive catastrophe | @CatastropheID=inactive | 'Active catastrophe not found: X' |

---

### Test Case ID: SP-CLM-023
**Procedure**: Claims.usp_Catastrophe_GetSummary
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @CatastropheID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get summary with linked claims | Valid catastrophe with claims | 3 result sets (header, claims, status breakdown) |

---

### Test Case ID: SP-CLM-024
**Procedure**: Claims.usp_Catastrophe_GetAll
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ActiveOnly BIT - Optional (default 1)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get active catastrophes | @ActiveOnly=1 | Only IsActive=1 records |
| 2 | Get all catastrophes | @ActiveOnly=0 | All records including closed |

---

### Test Case ID: SP-CLM-025
**Procedure**: Claims.usp_Catastrophe_Close
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @CatastropheID INT - Required
- @ClosedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Close catastrophe | Valid ID | IsActive=0, ClosedDate set |

---

### Test Case ID: SP-CLM-026
**Procedure**: Claims.usp_Vendor_Create
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @VendorName VARCHAR(200) - Required
- @VendorType VARCHAR(30) - Required
- @ContactName VARCHAR(200) - Optional
- @Phone VARCHAR(20) - Optional
- @Email VARCHAR(200) - Optional
- @AddressLine1 VARCHAR(200) - Optional
- @City VARCHAR(100) - Optional
- @StateCode CHAR(2) - Optional
- @ZipCode VARCHAR(10) - Optional
- @LicenseNumber VARCHAR(50) - Optional
- @HourlyRate DECIMAL(10,2) - Optional
- @DailyRate DECIMAL(10,2) - Optional
- @PreferredVendor BIT - Optional (default 0)
- @CreatedBy VARCHAR(50) - Required
- @VendorID INT OUTPUT
- @VendorNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create vendor | @VendorName='Test Vendor', @VendorType='ADJUSTER' | VendorID > 0, VendorNumber format VND+7 digits, IsActive=1 |

---

### Test Case ID: SP-CLM-027
**Procedure**: Claims.usp_Vendor_Search
**Source**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
**Parameters**:
- @VendorName VARCHAR(200) - Optional
- @VendorType VARCHAR(30) - Optional
- @StateCode CHAR(2) - Optional
- @PreferredOnly BIT - Optional (default 0)
- @IsActive BIT - Optional (default 1)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by type | @VendorType='ADJUSTER' | All active adjusters |
| 2 | Search preferred only | @PreferredOnly=1 | Only PreferredVendor=1 |
| 3 | Search by name | @VendorName='Test' | LIKE match on VendorName |
| 4 | Sort order | Any search | Ordered by PreferredVendor DESC, Rating DESC, VendorName |

---

### Test Case ID: SP-CLM-028
**Procedure**: Claims.usp_Litigation_GetOpen
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get open litigation claims | None | Claims with IsLitigation=1 and status not CLOSED/DENIED |

---

### Test Case ID: SP-CLM-029
**Procedure**: Claims.usp_Litigation_Update
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @AttorneyName VARCHAR(200) - Optional
- @LitigationDate DATETIME - Optional
- @Notes VARCHAR(MAX) - Optional
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update attorney name | @AttorneyName='Smith & Jones' | AttorneyName updated |
| 2 | Update litigation date | @LitigationDate=valid date | LitigationDate updated |

---

### Test Case ID: SP-CLM-030
**Procedure**: Claims.usp_Assignment_GetByClaim
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ClaimID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get assignments for claim | @ClaimID=claim with assignments | Assignments with vendor info, ordered by AssignmentDate DESC |
