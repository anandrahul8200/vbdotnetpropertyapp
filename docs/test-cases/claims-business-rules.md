# Claims Module - Business Rules

## Module: CLM (Claims)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-CLM-001
**Module**: CLM
**Priority**: Critical

#### Rule Description
Claim status transitions follow a defined matrix. Only these transitions are allowed:
- FNOL -> ASSIGNED, DENIED, CLOSED
- ASSIGNED -> INVESTIGATING, DENIED, CLOSED
- INVESTIGATING -> ASSESSED, DENIED, CLOSED, LITIGATION
- ASSESSED -> APPROVED, DENIED, CLOSED
- APPROVED -> SETTLED, CLOSED
- DENIED -> REOPENED, CLOSED
- SETTLED -> CLOSED, REOPENED
- CLOSED -> REOPENED
- REOPENED -> INVESTIGATING, ASSIGNED
- LITIGATION -> SETTLED, CLOSED, DENIED

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_UpdateStatus
- **Code Snippet**:
```sql
SELECT @IsValid = CASE
    WHEN @CurrentStatus = 'FNOL' AND @NewStatus IN ('ASSIGNED', 'DENIED', 'CLOSED') THEN 1
    WHEN @CurrentStatus = 'ASSIGNED' AND @NewStatus IN ('INVESTIGATING', 'DENIED', 'CLOSED') THEN 1
    WHEN @CurrentStatus = 'INVESTIGATING' AND @NewStatus IN ('ASSESSED', 'DENIED', 'CLOSED', 'LITIGATION') THEN 1
    WHEN @CurrentStatus = 'ASSESSED' AND @NewStatus IN ('APPROVED', 'DENIED', 'CLOSED') THEN 1
    WHEN @CurrentStatus = 'APPROVED' AND @NewStatus IN ('SETTLED', 'CLOSED') THEN 1
    WHEN @CurrentStatus = 'DENIED' AND @NewStatus IN ('REOPENED', 'CLOSED') THEN 1
    WHEN @CurrentStatus = 'SETTLED' AND @NewStatus IN ('CLOSED', 'REOPENED') THEN 1
    WHEN @CurrentStatus = 'CLOSED' AND @NewStatus IN ('REOPENED') THEN 1
    WHEN @CurrentStatus = 'REOPENED' AND @NewStatus IN ('INVESTIGATING', 'ASSIGNED') THEN 1
    WHEN @CurrentStatus = 'LITIGATION' AND @NewStatus IN ('SETTLED', 'CLOSED', 'DENIED') THEN 1
    ELSE 0
END;
```

#### Enforcement Mechanism
- Type: SP logic (CASE statement with RAISERROR on invalid transition)
- Behavior: Returns error 'Invalid status transition from %s to %s'

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-002 | Status transition validation in usp_Claim_UpdateStatus |
| FT-CLM-001 | End-to-end claim lifecycle status transitions |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | FNOL -> INVESTIGATING (skip ASSIGNED) | Error: Invalid status transition |
| 2 | CLOSED -> ASSIGNED (invalid) | Error: Invalid status transition |
| 3 | APPROVED -> FNOL (backward) | Error: Invalid status transition |
| 4 | REOPENED -> CLOSED (not allowed) | Error: Invalid status transition |
| 5 | LITIGATION -> APPROVED (not allowed) | Error: Invalid status transition |

---

### Rule ID: BR-CLM-002
**Module**: CLM
**Priority**: Critical

#### Rule Description
Reserve amounts exceeding $50,000 require approval. The threshold is configurable via Admin.SystemConfig key 'RESERVE_APPROVAL_THRESHOLD'. When approval is required, the reserve is created with IsApproved = 0 and ApprovalRequired = 1.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_SetReserve
- **Code Snippet**:
```sql
DECLARE @ApprovalThreshold DECIMAL(18,2) = 50000;
SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
FROM Admin.SystemConfig WHERE ConfigKey = 'RESERVE_APPROVAL_THRESHOLD';

DECLARE @ApprovalRequired BIT = CASE WHEN @Amount > @ApprovalThreshold THEN 1 ELSE 0 END;
```

#### Enforcement Mechanism
- Type: SP logic (threshold comparison)
- Behavior: Reserves > threshold are inserted with ApprovalRequired=1, IsApproved=0

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-003 | Reserve approval threshold tests |
| BR-CLM-002 | Business rule validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Amount = $50,000.00 (exactly at threshold) | No approval required (uses > not >=) |
| 2 | Amount = $50,000.01 | Approval required |
| 3 | Amount = $49,999.99 | No approval required |
| 4 | SystemConfig key missing | Uses default $50,000 |

---

### Rule ID: BR-CLM-003
**Module**: CLM
**Priority**: Critical

#### Rule Description
Payment amounts exceeding $10,000 require approval. The threshold is configurable via Admin.SystemConfig key 'PAYMENT_APPROVAL_THRESHOLD'. Payments not requiring approval are auto-approved with Status = 'APPROVED'. Payments requiring approval are set to Status = 'PENDING'.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_CreatePayment
- **Code Snippet**:
```sql
DECLARE @ApprovalThreshold DECIMAL(18,2) = 10000;
SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
FROM Admin.SystemConfig WHERE ConfigKey = 'PAYMENT_APPROVAL_THRESHOLD';

DECLARE @ApprovalRequired BIT = CASE WHEN @Amount > @ApprovalThreshold THEN 1 ELSE 0 END;
```

#### Enforcement Mechanism
- Type: SP logic (threshold comparison)
- Behavior: Status set to PENDING or APPROVED based on threshold; claim TotalPaid only updated for auto-approved

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-004 | Payment approval threshold tests |
| FT-CLM-003 | Payment workflow end-to-end |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Amount = $10,000.00 (exactly at threshold) | No approval required (uses > not >=) |
| 2 | Amount = $10,000.01 | Approval required, Status = 'PENDING' |
| 3 | Amount = $9,999.99 | Auto-approved, Status = 'APPROVED' |
| 4 | SystemConfig key missing | Uses default $10,000 |

---

### Rule ID: BR-CLM-004
**Module**: CLM
**Priority**: Critical

#### Rule Description
Payment cannot exceed policy limit. The check is: TotalPaid + Amount must not exceed PolicyLimit. If exceeded, the SP raises an error with the message showing limit, already paid, and requested amounts.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_CreatePayment
- **Code Snippet**:
```sql
IF (@TotalPaid + @Amount) > @PolicyLimit
BEGIN
    DECLARE @ErrMsg VARCHAR(500);
    SET @ErrMsg = 'Payment would exceed policy limit. Limit: ' + CAST(@PolicyLimit AS VARCHAR(50))
        + ', Already paid: ' + CAST(@TotalPaid AS VARCHAR(50))
        + ', Requested: ' + CAST(@Amount AS VARCHAR(50));
    RAISERROR(@ErrMsg, 16, 1);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (sum comparison with RAISERROR)
- Behavior: Rejects payment if cumulative total would exceed policy limit

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-004 | Policy limit check in CreatePayment |
| FT-CLM-003 | Payment workflow boundary tests |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | TotalPaid + Amount = PolicyLimit exactly | Allowed (uses > not >=) |
| 2 | TotalPaid + Amount exceeds by $0.01 | Error: Payment would exceed policy limit |
| 3 | First payment exactly equal to PolicyLimit | Allowed |
| 4 | PolicyLimit = 0 and Amount > 0 | Error |

---

### Rule ID: BR-CLM-005
**Module**: CLM
**Priority**: High

#### Rule Description
Claim complexity is automatically determined based on estimated loss at FNOL:
- EstimatedLoss > $100,000: Complexity = 'COMPLEX'
- EstimatedLoss > $25,000 (and <= $100,000): Complexity = 'MODERATE'
- EstimatedLoss <= $25,000 or NULL: Complexity = 'SIMPLE'

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_Create
- **Code Snippet**:
```sql
DECLARE @Complexity VARCHAR(10) = 'SIMPLE';
IF @EstimatedLoss IS NOT NULL
BEGIN
    IF @EstimatedLoss > 100000 SET @Complexity = 'COMPLEX';
    ELSE IF @EstimatedLoss > 25000 SET @Complexity = 'MODERATE';
END
```

#### Enforcement Mechanism
- Type: SP logic (computed during INSERT)
- Behavior: Complexity column set automatically; evaluated only if EstimatedLoss is not NULL

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-001 | Complexity determination in usp_Claim_Create |
| DV-CLM-001 | Claims table Complexity column values |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | EstimatedLoss = $100,000.00 (exact boundary) | MODERATE (uses > not >=) |
| 2 | EstimatedLoss = $100,000.01 | COMPLEX |
| 3 | EstimatedLoss = $25,000.00 (exact boundary) | SIMPLE (uses > not >=) |
| 4 | EstimatedLoss = $25,000.01 | MODERATE |
| 5 | EstimatedLoss = NULL | SIMPLE (default) |
| 6 | EstimatedLoss = $0.00 | SIMPLE |

---

### Rule ID: BR-CLM-006
**Module**: CLM
**Priority**: Critical

#### Rule Description
Loss date must fall within the policy period (EffectiveDate to ExpiryDate). If the loss date is outside, the claim cannot be created.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_Create
- **Code Snippet**:
```sql
IF CAST(@LossDate AS DATE) < @PolicyEffective OR CAST(@LossDate AS DATE) > @PolicyExpiry
BEGIN
    RAISERROR('Loss date is outside the policy period', 16, 1);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (date range validation with RAISERROR)
- Behavior: Rejects claim creation if loss date is before effective date or after expiry date

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-001 | Loss date validation in usp_Claim_Create |
| FT-CLM-001 | FNOL creation validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | LossDate = EffectiveDate exactly | Allowed |
| 2 | LossDate = ExpiryDate exactly | Allowed |
| 3 | LossDate = EffectiveDate - 1 day | Error: Loss date is outside the policy period |
| 4 | LossDate = ExpiryDate + 1 day | Error: Loss date is outside the policy period |

---

### Rule ID: BR-CLM-007
**Module**: CLM
**Priority**: Critical

#### Rule Description
Policy must be in ACTIVE or PENDING_CANCEL status for a claim to be created. Any other status will reject the claim creation.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_Create
- **Code Snippet**:
```sql
IF @PolicyStatus NOT IN ('ACTIVE', 'PENDING_CANCEL')
BEGIN
    RAISERROR('Policy is not active. Current status: %s', 16, 1, @PolicyStatus);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (status check with RAISERROR)
- Behavior: Rejects claim if policy status is not ACTIVE or PENDING_CANCEL

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-001 | Policy status validation in usp_Claim_Create |
| FT-CLM-001 | FNOL creation against various policy states |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | PolicyStatus = 'ACTIVE' | Allowed |
| 2 | PolicyStatus = 'PENDING_CANCEL' | Allowed |
| 3 | PolicyStatus = 'CANCELLED' | Error: Policy is not active. Current status: CANCELLED |
| 4 | PolicyStatus = 'EXPIRED' | Error: Policy is not active. Current status: EXPIRED |
| 5 | PolicyStatus = 'QUOTE' | Error: Policy is not active. Current status: QUOTE |

---

### Rule ID: BR-CLM-008
**Module**: CLM
**Priority**: High

#### Rule Description
1099 reporting requirement is determined by three conditions being true simultaneously:
- TaxReportable = 1 (marked by user)
- PayeeType IN ('VENDOR', 'ATTORNEY')
- Amount >= $600

When all three are met, Form1099Required is set to 1.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_CreatePayment
- **Code Snippet**:
```sql
DECLARE @Form1099Required BIT = 0;
IF @TaxReportable = 1 AND @PayeeType IN ('VENDOR', 'ATTORNEY') AND @Amount >= 600
    SET @Form1099Required = 1;
```

#### Enforcement Mechanism
- Type: SP logic (conditional flag setting)
- Behavior: Form1099Required flag stored on payment record

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-004 | 1099 requirement logic in CreatePayment |
| DV-CLM-003 | Payments table Form1099Required validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | TaxReportable=1, PayeeType='VENDOR', Amount=$600.00 | Form1099Required = 1 |
| 2 | TaxReportable=1, PayeeType='ATTORNEY', Amount=$600.00 | Form1099Required = 1 |
| 3 | TaxReportable=1, PayeeType='VENDOR', Amount=$599.99 | Form1099Required = 0 |
| 4 | TaxReportable=0, PayeeType='VENDOR', Amount=$1000.00 | Form1099Required = 0 |
| 5 | TaxReportable=1, PayeeType='INSURED', Amount=$1000.00 | Form1099Required = 0 |
| 6 | TaxReportable=1, PayeeType='MORTGAGEE', Amount=$5000.00 | Form1099Required = 0 |

---

### Rule ID: BR-CLM-009
**Module**: CLM
**Priority**: Medium

#### Rule Description
Claim number format is CLM followed by 7 digits (zero-padded), generated from ClaimID sequence.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_Create
- **Code Snippet**:
```sql
DECLARE @Sequence INT;
SELECT @Sequence = ISNULL(MAX(ClaimID), 0) + 1 FROM Claims.Claims;
SET @ClaimNumber = 'CLM' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);
```

#### Enforcement Mechanism
- Type: SP logic (auto-generated, UNIQUE constraint on column)
- Behavior: Format CLM + 7 digits (e.g., CLM0000001)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-001 | Claim number generation |
| DV-CLM-001 | ClaimNumber format validation |

---

### Rule ID: BR-CLM-010
**Module**: CLM
**Priority**: Medium

#### Rule Description
Payment number format is PAY followed by 7 digits (zero-padded), generated from PaymentID sequence.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_CreatePayment
- **Code Snippet**:
```sql
DECLARE @Sequence INT;
SELECT @Sequence = ISNULL(MAX(PaymentID), 0) + 1 FROM Claims.Payments;
SET @PaymentNumber = 'PAY' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);
```

#### Enforcement Mechanism
- Type: SP logic (auto-generated, UNIQUE constraint on column)
- Behavior: Format PAY + 7 digits (e.g., PAY0000001)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-004 | Payment number generation |
| DV-CLM-003 | PaymentNumber format validation |

---

### Rule ID: BR-CLM-011
**Module**: CLM
**Priority**: High

#### Rule Description
Fraud score threshold for automatic SIU referral is 70 (configurable via Admin.SystemConfig key 'FRAUD_SIU_THRESHOLD'). When the calculated fraud score meets or exceeds this threshold, the claim is automatically referred to the Special Investigations Unit. The score is normalized to a 0-100 scale.

#### Source
- **File**: `database/02-stored-procedures/004-claims-specialized-sps.sql`
- **SP/Method**: Claims.usp_Fraud_EvaluateClaim
- **Code Snippet**:
```sql
DECLARE @SIUThreshold DECIMAL(6,2) = 70.0;
SELECT @SIUThreshold = CAST(ConfigValue AS DECIMAL(6,2))
FROM Admin.SystemConfig WHERE ConfigKey = 'FRAUD_SIU_THRESHOLD';

IF @FraudScore >= @SIUThreshold
BEGIN
    UPDATE Claims.Claims SET
        IsSIUReferred = 1,
        SIUReferralDate = GETDATE()
    WHERE ClaimID = @ClaimID AND IsSIUReferred = 0;
END
```

#### Enforcement Mechanism
- Type: SP logic (auto-referral on threshold)
- Behavior: Sets IsSIUReferred=1 and creates SIU referral activity

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-014 | Fraud evaluation auto-referral |
| FT-CLM-005 | Fraud workflow end-to-end |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | FraudScore = 70.00 (exactly at threshold) | Auto-referred (uses >=) |
| 2 | FraudScore = 69.99 | Not referred |
| 3 | FraudScore = 100.00 | Auto-referred |
| 4 | Claim already SIU referred (IsSIUReferred=1) | No duplicate referral (WHERE IsSIUReferred = 0) |
| 5 | SystemConfig key missing | Uses default threshold of 70 |

---

### Rule ID: BR-CLM-012
**Module**: CLM
**Priority**: High

#### Rule Description
Only APPROVED or ISSUED payments can be voided. Voiding reverses the claim TotalPaid by subtracting the payment amount and recalculates NetIncurred.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_VoidPayment
- **Code Snippet**:
```sql
IF @Status NOT IN ('APPROVED', 'ISSUED')
BEGIN
    RAISERROR('Only APPROVED or ISSUED payments can be voided. Current: %s', 16, 1, @Status);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (status check with RAISERROR)
- Behavior: Rejects void for PENDING, CLEARED, VOIDED, or STOPPED payments

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-006 | Payment void status validation |
| FT-CLM-003 | Payment void workflow |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Status = 'APPROVED' | Void allowed |
| 2 | Status = 'ISSUED' | Void allowed |
| 3 | Status = 'PENDING' | Error: Only APPROVED or ISSUED payments can be voided |
| 4 | Status = 'VOIDED' | Error (already voided) |
| 5 | Status = 'CLEARED' | Error |

---

### Rule ID: BR-CLM-013
**Module**: CLM
**Priority**: High

#### Rule Description
Payments cannot be created on claims with status FNOL, CLOSED, or DENIED. Claims must progress to at least ASSIGNED status before payments are allowed.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_CreatePayment
- **Code Snippet**:
```sql
IF @ClaimStatus IN ('FNOL', 'CLOSED', 'DENIED')
BEGIN
    RAISERROR('Cannot create payment on claim with status: %s', 16, 1, @ClaimStatus);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (status check with RAISERROR)
- Behavior: Rejects payment creation on non-payable claims

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-004 | Payment status validation |
| FT-CLM-003 | Payment workflow preconditions |

---

### Rule ID: BR-CLM-014
**Module**: CLM
**Priority**: Medium

#### Rule Description
Reserves cannot be set on CLOSED or DENIED claims.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_SetReserve
- **Code Snippet**:
```sql
IF @ClaimStatus IN ('CLOSED', 'DENIED')
BEGIN
    RAISERROR('Cannot set reserve on a closed or denied claim', 16, 1);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP logic (status check with RAISERROR)
- Behavior: Rejects reserve changes on closed/denied claims

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-003 | Reserve claim status validation |

---

### Rule ID: BR-CLM-015
**Module**: CLM
**Priority**: Medium

#### Rule Description
When an adjuster is assigned to a claim in FNOL status, the claim automatically transitions to ASSIGNED status with a corresponding status history entry.

#### Source
- **File**: `database/02-stored-procedures/003-claims-processing-sps.sql`
- **SP/Method**: Claims.usp_Claim_Assign
- **Code Snippet**:
```sql
IF @AssigneeType = 'ADJUSTER'
BEGIN
    UPDATE Claims.Claims SET
        AdjusterID = @AssigneeID,
        AssignedDate = GETDATE(),
        ClaimStatus = CASE WHEN @ClaimStatus = 'FNOL' THEN 'ASSIGNED' ELSE ClaimStatus END,
        ModifiedDate = GETDATE(),
        ModifiedBy = @CreatedBy
    WHERE ClaimID = @ClaimID;

    IF @ClaimStatus = 'FNOL'
    BEGIN
        INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
        VALUES (@ClaimID, 'FNOL', 'ASSIGNED', GETDATE(), @CreatedBy, 'Adjuster assigned');
    END
END
```

#### Enforcement Mechanism
- Type: SP logic (conditional status update)
- Behavior: Only transitions from FNOL; other statuses unchanged

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-CLM-008 | Adjuster assignment auto-status change |
| FT-CLM-002 | Claim assignment workflow |
