# Policy Module - Business Rules

## Module: POL (Policy)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-POL-001
**Module**: POL
**Priority**: Critical

#### Rule Description
When a customer is created, the system assigns a Risk Tier based on credit score:
- Credit score >= 750: RiskTier = 'PREFERRED'
- Credit score >= 650 and < 750: RiskTier = 'STANDARD'
- Credit score < 650: RiskTier = 'SUBSTANDARD'
- Credit score is NULL: RiskTier = 'STANDARD' (default)

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Customer_Create
- **Code Snippet**:
```sql
DECLARE @RiskTier VARCHAR(20) = 'STANDARD';
IF @CreditScore IS NOT NULL
BEGIN
    IF @CreditScore >= 750 SET @RiskTier = 'PREFERRED';
    ELSE IF @CreditScore >= 650 SET @RiskTier = 'STANDARD';
    ELSE SET @RiskTier = 'SUBSTANDARD';
END
```

#### Enforcement Mechanism
- Type: SP logic (computed during INSERT)
- Behavior: RiskTier column set automatically; not directly editable by user via form

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-001 | Tests credit score 780 -> PREFERRED, 700 -> STANDARD, 600 -> SUBSTANDARD |
| FT-POL-001 | End-to-end customer creation verifies RiskTier |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | CreditScore = 750 (exact boundary) | RiskTier = 'PREFERRED' |
| 2 | CreditScore = 749 | RiskTier = 'STANDARD' |
| 3 | CreditScore = 650 (exact boundary) | RiskTier = 'STANDARD' |
| 4 | CreditScore = 649 | RiskTier = 'SUBSTANDARD' |
| 5 | CreditScore = NULL | RiskTier = 'STANDARD' (default) |
| 6 | CreditScore = 0 | RiskTier = 'SUBSTANDARD' |

---

### Rule ID: BR-POL-002
**Module**: POL
**Priority**: High

#### Rule Description
Fire Protection Class is auto-calculated when not provided, based on distance to fire station and hydrant:
- Distance to fire station <= 1.0 mi AND distance to hydrant <= 0.25 mi: Class 3
- Distance to fire station <= 3.0 mi AND distance to hydrant <= 0.5 mi: Class 5
- Distance to fire station <= 5.0 mi: Class 7
- Distance to fire station > 5.0 mi: Class 9

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Property_Create
- **Code Snippet**:
```sql
IF @FireProtectionClass IS NULL
BEGIN
    SET @FireProtectionClass =
        CASE
            WHEN @DistanceToFireStation <= 1.0 AND @DistanceToHydrant <= 0.25 THEN 3
            WHEN @DistanceToFireStation <= 3.0 AND @DistanceToHydrant <= 0.5 THEN 5
            WHEN @DistanceToFireStation <= 5.0 THEN 7
            ELSE 9
        END;
END
```

#### Enforcement Mechanism
- Type: SP logic (computed during INSERT if not explicitly provided)
- Behavior: FireProtectionClass auto-assigned; if provided by user, the provided value is used

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-005 | Tests all distance combinations and boundary values |
| FT-POL-001 | Property creation flow |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | DistanceToFireStation = 1.0, Hydrant = 0.25 | Class 3 (boundary inclusive) |
| 2 | DistanceToFireStation = 1.01, Hydrant = 0.25 | Class 5 |
| 3 | DistanceToFireStation = 3.0, Hydrant = 0.5 | Class 5 (boundary inclusive) |
| 4 | DistanceToFireStation = 3.0, Hydrant = 0.6 | Class 7 (hydrant > 0.5) |
| 5 | DistanceToFireStation = 5.0 | Class 7 |
| 6 | DistanceToFireStation = 5.01 | Class 9 |
| 7 | Both distances NULL | [ASSUMPTION] FireProtectionClass = 9 (ELSE branch) |
| 8 | FireProtectionClass explicitly provided | Provided value used, no calculation |

---

### Rule ID: BR-POL-003
**Module**: POL
**Priority**: Critical

#### Rule Description
Before creating a policy quote, the system checks for active moratoriums. If a moratorium is active for the property's state and the requested policy type, quote creation is blocked.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Policy_CreateQuote
- **Code Snippet**:
```sql
IF EXISTS (
    SELECT 1 FROM Underwriting.Moratoriums
    WHERE IsActive = 1
        AND MoratoriumType IN ('NEW_BUSINESS', 'ALL')
        AND GETDATE() BETWEEN StartDate AND ISNULL(EndDate, '9999-12-31')
        AND (AffectedStates LIKE '%' + @PropertyState + '%' OR AffectedStates IS NULL)
        AND (AffectedPolicyTypes LIKE '%' + @PolicyType + '%' OR AffectedPolicyTypes IS NULL)
)
BEGIN
    RAISERROR('New business moratorium is in effect for this location/policy type', 16, 1);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: RAISERROR in stored procedure
- Error Message: 'New business moratorium is in effect for this location/policy type'
- Behavior: Quote creation blocked, transaction not initiated

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-008 | Negative test with moratorium active |
| FT-POL-002 | Quote creation pre-condition: no moratorium |
| US-POL-004 | User story acceptance criteria for moratorium |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Moratorium with NULL AffectedStates | Applies to ALL states |
| 2 | Moratorium with NULL AffectedPolicyTypes | Applies to ALL policy types |
| 3 | Moratorium with EndDate = NULL | Active indefinitely |
| 4 | Moratorium type = 'RENEWAL' only | Does not block new business |
| 5 | Expired moratorium (EndDate < today) | Does not block |

---

### Rule ID: BR-POL-004
**Module**: POL
**Priority**: Critical

#### Rule Description
A policy can only be bound (status changed to ACTIVE) if it is currently in QUOTE status. Any other status will result in an error.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP/Method**: Policy.usp_Policy_Bind
- **Code Snippet**:
```sql
IF NOT EXISTS (SELECT 1 FROM Policy.Policies WHERE PolicyID = @PolicyID AND PolicyStatus = 'QUOTE')
BEGIN
    RAISERROR('Policy must be in QUOTE status to bind', 16, 1);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: RAISERROR in stored procedure
- Error Message: 'Policy must be in QUOTE status to bind'
- Behavior: Status remains unchanged, transaction rolled back

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-011 | Tests bind with QUOTE status (success) and non-QUOTE (failure) |
| FT-POL-003 | End-to-end bind workflow |
| US-POL-005 | Bind user story |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | PolicyStatus = 'ACTIVE' | Error raised |
| 2 | PolicyStatus = 'CANCELLED' | Error raised |
| 3 | PolicyStatus = 'EXPIRED' | Error raised |
| 4 | PolicyID does not exist | Error raised (no matching row) |
| 5 | Concurrent bind attempts | First succeeds, second fails (status already ACTIVE) |

---

### Rule ID: BR-POL-005
**Module**: POL
**Priority**: High

#### Rule Description
System-generated numbers follow specific formats:
- Customer Number: 'CUS' + 7 zero-padded digits (e.g., CUS0000001)
- Property Number: 'PRP' + 7 zero-padded digits (e.g., PRP0000001)
- Policy Number: 'POL' + 7 zero-padded digits (e.g., POL0000001)

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Customer_Create, Policy.usp_Property_Create, Policy.usp_Policy_CreateQuote
- **Code Snippet** (Customer example):
```sql
DECLARE @Sequence INT;
SELECT @Sequence = ISNULL(MAX(CustomerID), 0) + 1 FROM Policy.Customers;
SET @CustomerNumber = 'CUS' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);
```

#### Enforcement Mechanism
- Type: SP logic (generated during INSERT)
- Behavior: Numbers are auto-generated and cannot be overridden by the user
- Uniqueness: Enforced by UNIQUE constraint on the Number column

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-001 | Customer number format verified |
| SP-POL-005 | Property number format verified |
| SP-POL-008 | Policy number format verified |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | First record in empty table | Number ends in 0000001 |
| 2 | After record deletion | Sequence based on MAX(ID)+1, gaps possible |
| 3 | Concurrent inserts | [ASSUMPTION] Possible collision if no locking (identity-based sequence) |

---

### Rule ID: BR-POL-006
**Module**: POL
**Priority**: High

#### Rule Description
Policy cancellation return premium is calculated using pro-rata method:
1. Determine days in term: DATEDIFF(DAY, EffectiveDate, ExpiryDate)
2. Determine days used: DATEDIFF(DAY, EffectiveDate, CancelDate)
3. Calculate pro-rata factor: DaysUsed / DaysInTerm
4. Earned premium: AnnualPremium * ProRataFactor (rounded to 2 decimals)
5. Return premium: AnnualPremium - EarnedPremium

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP/Method**: Policy.usp_Policy_Cancel
- **Code Snippet**:
```sql
DECLARE @DaysInTerm INT = DATEDIFF(DAY, @EffectiveDate, @ExpiryDate);
DECLARE @DaysUsed INT = DATEDIFF(DAY, @EffectiveDate, @CancelDate);
DECLARE @ProRata DECIMAL(10,8) = CAST(@DaysUsed AS DECIMAL) / CAST(@DaysInTerm AS DECIMAL);
DECLARE @EarnedPremium DECIMAL(18,2) = ROUND(@AnnualPremium * @ProRata, 2);
SET @ReturnPremium = @AnnualPremium - @EarnedPremium;
```

#### Enforcement Mechanism
- Type: SP calculation logic
- Behavior: Return premium computed and returned via OUTPUT parameter

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-012 | Tests pro-rata calculation at various cancel dates |
| FT-POL-005 | End-to-end cancellation workflow |
| US-POL-007 | Cancel policy user story |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Cancel on effective date (0 days used) | Return = full AnnualPremium |
| 2 | Cancel on expiry date (all days used) | Return = $0 |
| 3 | Cancel at exact midpoint | Return = AnnualPremium / 2 |
| 4 | Cancel 1 day after effective | Return = nearly full premium |
| 5 | Rounding differences | ROUND to 2 decimals applied |

---

### Rule ID: BR-POL-007
**Module**: POL
**Priority**: High

#### Rule Description
Property ownership validation: A property must belong to the specified customer (CustomerID match) and must be active (IsActive=1) before it can be used in a policy quote.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Policy_CreateQuote
- **Code Snippet**:
```sql
IF NOT EXISTS (SELECT 1 FROM Policy.Properties WHERE PropertyID = @PropertyID AND CustomerID = @CustomerID AND IsActive = 1)
    RAISERROR('Property not found or does not belong to customer', 16, 1);
```

#### Enforcement Mechanism
- Type: RAISERROR in stored procedure
- Error Message: 'Property not found or does not belong to customer'
- Behavior: Quote creation blocked

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-008 | Negative test with mismatched property |
| US-POL-004 | Quote creation ownership validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Property belongs to different customer | Error raised |
| 2 | Property is inactive (IsActive=0) | Error raised |
| 3 | PropertyID does not exist | Error raised |

---

### Rule ID: BR-POL-008
**Module**: POL
**Priority**: High

#### Rule Description
Commission rate is inherited from the assigned agent's record at the time of quote creation. The agent must be active (IsActive=1) for assignment.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Policy_CreateQuote
- **Code Snippet**:
```sql
IF NOT EXISTS (SELECT 1 FROM Policy.Agents WHERE AgentID = @AgentID AND IsActive = 1)
    RAISERROR('Active agent not found', 16, 1);

DECLARE @CommissionRate DECIMAL(6,4);
SELECT @CommissionRate = CommissionRate FROM Policy.Agents WHERE AgentID = @AgentID;
```

#### Enforcement Mechanism
- Type: SP validation + lookup
- Error Message: 'Active agent not found'
- Behavior: CommissionRate copied to policy record

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-008 | Tests active agent validation and commission assignment |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Agent with CommissionRate = 0 | Policy gets CommissionRate = 0 |
| 2 | Agent deactivated after quote | Policy retains original rate |
| 3 | Inactive agent (IsActive=0) | Error: 'Active agent not found' |

---

### Rule ID: BR-POL-009
**Module**: POL
**Priority**: Medium

#### Rule Description
Customer record validation during update: The stored procedure verifies the customer exists before performing any update. If the CustomerID is not found, an error is raised.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Customer_Update
- **Code Snippet**:
```sql
IF NOT EXISTS (SELECT 1 FROM Policy.Customers WHERE CustomerID = @CustomerID)
BEGIN
    RAISERROR('Customer not found: %d', 16, 1, @CustomerID);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: RAISERROR in stored procedure
- Error Message: 'Customer not found: {CustomerID}'
- Behavior: No update performed, error propagated to caller

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-002 | Tests non-existent customer update |

---

### Rule ID: BR-POL-010
**Module**: POL
**Priority**: Medium

#### Rule Description
The usp_Customer_Update uses ISNULL pattern for all optional parameters, meaning NULL values in parameters preserve existing data rather than overwriting with NULL.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Customer_Update
- **Code Snippet**:
```sql
UPDATE Policy.Customers SET
    Title = ISNULL(@Title, Title),
    FirstName = ISNULL(@FirstName, FirstName),
    ...
```

#### Enforcement Mechanism
- Type: SP logic (ISNULL pattern)
- Behavior: Only non-NULL parameters update the corresponding column

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-002 | Tests NULL parameters preserve existing values |
| UT-POL-002 | Unit test for Update method |

---

### Rule ID: BR-POL-011
**Module**: POL
**Priority**: Medium

#### Rule Description
Active customer validation for property creation: Only active customers (IsActive=1) can have properties added.

#### Source
- **File**: `database/02-stored-procedures/001-policy-crud-sps.sql`
- **SP/Method**: Policy.usp_Property_Create
- **Code Snippet**:
```sql
IF NOT EXISTS (SELECT 1 FROM Policy.Customers WHERE CustomerID = @CustomerID AND IsActive = 1)
BEGIN
    RAISERROR('Active customer not found: %d', 16, 1, @CustomerID);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: RAISERROR in stored procedure
- Error Message: 'Active customer not found: {CustomerID}'
- Behavior: Property creation blocked

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-POL-005 | Tests inactive customer property creation |

---

### Rule ID: BR-POL-012
**Module**: POL
**Priority**: Medium

#### Rule Description
Form-level validation in frmCustomerEntry requires either First Name or Company Name (for commercial customers). Both cannot be blank.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Policy/frmCustomerEntry.vb`
- **SP/Method**: ValidateForm()
- **Code Snippet**:
```vb
If String.IsNullOrWhiteSpace(txtFirstName.Text) AndAlso String.IsNullOrWhiteSpace(txtCompanyName.Text) Then
    MessageBox.Show("First Name or Company Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    txtFirstName.Focus()
    Return False
End If
```

#### Enforcement Mechanism
- Type: UI validation (form-level, pre-save)
- Error Message: 'First Name or Company Name is required.'
- Behavior: Save is blocked, focus set to txtFirstName

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UI-POL-001 | Required field validation tests |
