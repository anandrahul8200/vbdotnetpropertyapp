# Underwriting Module - Business Rules

## Module: UND (Underwriting)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-UND-001
**Module**: UND
**Priority**: Critical

#### Rule Description
Multi-factor rating algorithm calculates coverage premium by multiplying a base premium (InsuredValue / 1000 * RatePer1000) by the product of 13 rating factors: Construction, Age, Roof, Protection Class, Deductible, Credit, Claims History, Protective Device, Territory, Occupancy, Loyalty, Multi-Policy, and New Home. Each factor defaults to 1.0 (no change). The total factor is the multiplicative product of all individual factors.

#### Source
- **File**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
- **SP/Method**: Underwriting.usp_Premium_CalculateCoverage
- **Code Snippet**:
```sql
SET @TotalFactor = @ConstructionFactor * @AgeFactor * @RoofFactor * @ProtectionClassFactor
    * @DeductibleFactor * @CreditFactor * @ClaimsHistoryFactor * @ProtectiveDeviceFactor
    * @TerritoryFactor * @OccupancyFactor * @LoyaltyFactor * @MultiPolicyFactor * @NewHomeFactor;
SET @CoveragePremium = ROUND(@BasePremium * @TotalFactor, 2);
```

#### Enforcement Mechanism
- Type: SP logic (multiplicative factor chain)
- Behavior: All 13 factors multiply together; result applied to base premium then rounded to 2 decimal places

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-002 | Coverage premium calculation with all factors |
| FT-UND-001 | Premium Calculation End-to-End |
| US-UND-001 | Premium Calculation for New Quote |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All factors = 1.0 (no adjustments) | FinalPremium = BasePremium exactly |
| 2 | Single factor < 1 (discount) | Premium reduced proportionally |
| 3 | Multiple discounts stack multiplicatively | e.g., 0.90 * 0.90 = 0.81 factor (19% total discount) |
| 4 | Factor > 1 (surcharge) combined with factor < 1 | Partial offset, net depends on magnitudes |
| 5 | Very small insured value ($1,000) | BasePremium = 1 * Rate; MinPremium may apply |

---

### Rule ID: BR-UND-002
**Module**: UND
**Priority**: Critical

#### Rule Description
Effective-dated rate lookups with territory fallback. When looking up a base rate, the system first tries a territory-specific rate (TerritoryCode = provided value). If no rate is found (RatePer1000 = 0), it falls back to the state-level rate (TerritoryCode IS NULL). Both lookups filter by PolicyType, StateCode, ConstructionType, ProtectionClass, CoverageCode, and effective date range.

#### Source
- **File**: `database/02-stored-procedures/005-underwriting-sps.sql`
- **SP/Method**: Underwriting.usp_Rate_GetBaseRate
- **Code Snippet**:
```sql
-- Try territory-specific rate first
IF @TerritoryCode IS NOT NULL
BEGIN
    SELECT TOP 1 @RatePer1000 = RatePer1000, @MinPremium = MinPremium
    FROM Underwriting.BaseRates
    WHERE PolicyType = @PolicyType AND StateCode = @StateCode
        AND ConstructionType = @ConstructionType AND ProtectionClass = @ProtectionClass
        AND CoverageCode = @CoverageCode AND TerritoryCode = @TerritoryCode
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY EffectiveDate DESC;
END
-- Fallback to state-level rate
IF @RatePer1000 = 0
BEGIN
    SELECT TOP 1 ... WHERE ... AND TerritoryCode IS NULL ...
END
```

#### Enforcement Mechanism
- Type: SP logic (two-tier lookup with fallback)
- Behavior: Territory-specific rates override state-level rates; state-level used as default

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-005 | Base rate lookup with territory fallback |
| FT-UND-002 | Rate Lookup and Territory Fallback |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Territory-specific rate exists | Territory rate used, no fallback |
| 2 | No territory rate, state-level exists | Falls back to state-level rate |
| 3 | No rate exists at either level | RatePer1000 = 0, BasePremium = 0 |
| 4 | Expired rate (EffectiveDate past ExpiryDate) | Not selected; falls through |
| 5 | Multiple active rates for same criteria | TOP 1 ORDER BY EffectiveDate DESC selects most recent |

---

### Rule ID: BR-UND-003
**Module**: UND
**Priority**: Critical

#### Rule Description
Factor lookup supports two modes: key-based (exact match on FactorKey, e.g., CONSTRUCTION_TYPE = 'FRAME') and range-based (value falls between MinRange and MaxRange, e.g., CREDIT_SCORE between 700 and 749). State-specific factors take precedence over state-null factors via ORDER BY StateCode DESC.

#### Source
- **File**: `database/02-stored-procedures/005-underwriting-sps.sql`
- **SP/Method**: Underwriting.usp_Rate_GetFactor
- **Code Snippet**:
```sql
IF @FactorKey IS NOT NULL
BEGIN
    -- Key-based lookup
    SELECT TOP 1 @Factor = Factor FROM Underwriting.RatingFactors
    WHERE FactorType = @FactorType AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND FactorKey = @FactorKey
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;
END
ELSE IF @RangeValue IS NOT NULL
BEGIN
    -- Range-based lookup
    SELECT TOP 1 @Factor = Factor FROM Underwriting.RatingFactors
    WHERE ... AND @RangeValue BETWEEN MinRange AND MaxRange ...
    ORDER BY StateCode DESC, EffectiveDate DESC;
END
```

#### Enforcement Mechanism
- Type: SP logic (dual-mode lookup with state preference)
- Behavior: State-specific factors override generic; key vs range determined by parameter provided

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-006 | Factor lookup key-based and range-based |
| FT-UND-002 | Rate Lookup and Territory Fallback |
| BR-UND-001 | Multi-factor rating algorithm |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Both FactorKey and RangeValue NULL | Factor = 1.0 (default, no lookup performed) |
| 2 | State-specific factor exists | StateCode DESC ordering picks state row over NULL |
| 3 | Only generic factor (StateCode IS NULL) | Generic factor used |
| 4 | No matching factor at all | Factor = 1.0 (default) |
| 5 | Range boundary value (value = MinRange exactly) | Matches the range (BETWEEN is inclusive) |

---

### Rule ID: BR-UND-004
**Module**: UND
**Priority**: Critical

#### Rule Description
Minimum premium enforcement. After calculating the factored premium, if the result is less than the MinPremium for that coverage (from BaseRates table), the MinPremium is applied instead. The MinPremiumApplied flag is set to 1 for audit purposes.

#### Source
- **File**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
- **SP/Method**: Underwriting.usp_Premium_CalculateCoverage
- **Code Snippet**:
```sql
IF @CoveragePremium < @MinPremium AND @MinPremium > 0
BEGIN
    SET @CoveragePremium = @MinPremium;
    SET @MinPremiumApplied = 1;
END
```

#### Enforcement Mechanism
- Type: SP logic (floor enforcement)
- Behavior: Premium never falls below configured minimum for the coverage

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-002 | Coverage premium calculation - minimum premium |
| FT-UND-001 | Premium Calculation End-to-End |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Calculated premium > MinPremium | Calculated premium used, MinPremiumApplied = 0 |
| 2 | Calculated premium < MinPremium | MinPremium applied, MinPremiumApplied = 1 |
| 3 | Calculated premium = MinPremium exactly | Calculated premium used (not less than) |
| 4 | MinPremium = 0 | No minimum enforcement (condition requires MinPremium > 0) |
| 5 | MinPremium not found (base rate not found) | MinPremium = 0, no floor applied |

---

### Rule ID: BR-UND-005
**Module**: UND
**Priority**: Critical

#### Rule Description
Moratorium enforcement checks whether new business or renewals are restricted for a given policy type, state, and zip code. Active moratoriums filter by MoratoriumType (NEW_BUSINESS, RENEWAL, or ALL), and use LIKE matching against AffectedStates, AffectedPolicyTypes, and AffectedZipCodes. NULL in any affected field means all values are restricted.

#### Source
- **File**: `database/02-stored-procedures/005-underwriting-sps.sql`
- **SP/Method**: Underwriting.usp_Moratorium_Check
- **Code Snippet**:
```sql
SELECT TOP 1 @IsMoratorium = 1, @MoratoriumName = MoratoriumName
FROM Underwriting.Moratoriums
WHERE IsActive = 1
    AND MoratoriumType IN (@TransactionType, 'ALL')
    AND GETDATE() BETWEEN StartDate AND ISNULL(EndDate, '9999-12-31')
    AND (AffectedStates LIKE '%' + @StateCode + '%' OR AffectedStates IS NULL)
    AND (AffectedPolicyTypes LIKE '%' + @PolicyType + '%' OR AffectedPolicyTypes IS NULL)
    AND (AffectedZipCodes LIKE '%' + @ZipCode + '%' OR AffectedZipCodes IS NULL)
ORDER BY StartDate DESC;
```

#### Enforcement Mechanism
- Type: SP logic (existence check with pattern matching)
- Behavior: Returns IsMoratorium = 1 if any active moratorium matches; NULL affected fields match all

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-010 | Moratorium check |
| FT-UND-005 | Moratorium Management Workflow |
| US-UND-004 | Moratorium Management |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | No active moratorium exists | IsMoratorium = 0 |
| 2 | Moratorium with NULL AffectedStates | Matches ALL states |
| 3 | Moratorium type = ALL | Matches both NEW_BUSINESS and RENEWAL transactions |
| 4 | Moratorium with EndDate = NULL | Active indefinitely (ISNULL converts to 9999-12-31) |
| 5 | Expired moratorium (EndDate < today) | Not matched |
| 6 | Zip code partial match risk | LIKE '%12345%' could match '112345' [ASSUMPTION] |

---

### Rule ID: BR-UND-006
**Module**: UND
**Priority**: High

#### Rule Description
Underwriting referral and decline rules. The rules engine evaluates built-in rules (property age > 75, protection class >= 9, roof age > 25, credit < 550, insured value > $2M, prior claims >= 3 in 3 years) and database-stored rules. Credit score < 550 triggers DECLINE (severity CRITICAL); others trigger REFER.

#### Source
- **File**: `database/02-stored-procedures/005-underwriting-sps.sql`
- **SP/Method**: Underwriting.usp_Rules_Evaluate
- **Code Snippet**:
```sql
IF @PropertyAge > 75
    INSERT INTO #RuleResults VALUES (NULL, 'AGE_75_PLUS', 'Property over 75 years old', 'REFER', NULL, 'HIGH', 1);
IF @ProtectionClass >= 9
    INSERT INTO #RuleResults VALUES (NULL, 'PROT_CLASS_9_10', 'Protection class 9 or 10', 'REFER', NULL, 'MEDIUM', 1);
IF @CreditScore IS NOT NULL AND @CreditScore < 550
    INSERT INTO #RuleResults VALUES (NULL, 'CREDIT_BELOW_550', 'Credit score below 550', 'DECLINE', NULL, 'CRITICAL', 1);
```

#### Enforcement Mechanism
- Type: SP logic (conditional rule evaluation with referral creation)
- Behavior: Triggered rules create referral records; results ordered by severity

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-007 | Rules evaluation |
| FT-UND-003 | Underwriting Rules Evaluation |
| US-UND-002 | Underwriting Referral Review |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | No rules triggered | Empty result set, no referrals created |
| 2 | Multiple rules triggered | Multiple referral records created |
| 3 | Credit score IS NULL | CREDIT_BELOW_550 rule skipped |
| 4 | Property age exactly 75 | Rule NOT triggered (condition is > 75) |
| 5 | Prior claims count excludes DENIED | Only non-DENIED claims counted |

---

### Rule ID: BR-UND-007
**Module**: UND
**Priority**: High

#### Rule Description
Loyalty discount based on renewal count. 5+ renewals = 10% discount (0.90), 3-4 renewals = 5% discount (0.95), fewer than 3 = no discount (1.0).

#### Source
- **File**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
- **SP/Method**: Underwriting.usp_Premium_CalculateCoverage
- **Code Snippet**:
```sql
IF @RenewalCount >= 5
    SET @LoyaltyFactor = 0.90;
ELSE IF @RenewalCount >= 3
    SET @LoyaltyFactor = 0.95;
```

#### Enforcement Mechanism
- Type: SP logic (tiered discount based on renewal count)
- Behavior: Hardcoded thresholds in premium calculation

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-002 | Coverage premium calculation |
| BR-UND-001 | Multi-factor rating algorithm |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | RenewalCount = 0 | LoyaltyFactor = 1.0 |
| 2 | RenewalCount = 3 | LoyaltyFactor = 0.95 |
| 3 | RenewalCount = 5 | LoyaltyFactor = 0.90 |
| 4 | RenewalCount = 100 | LoyaltyFactor = 0.90 (capped) |

---

### Rule ID: BR-UND-008
**Module**: UND
**Priority**: High

#### Rule Description
Multi-policy discount. When MultiPolicyDiscount = 1, factor = 0.90 (10% discount). Otherwise factor = 1.0.

#### Source
- **File**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
- **SP/Method**: Underwriting.usp_Premium_CalculateCoverage
- **Code Snippet**:
```sql
IF @MultiPolicyDiscount = 1
    SET @MultiPolicyFactor = 0.90;
```

#### Enforcement Mechanism
- Type: SP logic (flag-based flat discount)
- Behavior: Boolean policy flag determines 10% discount

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-002 | Coverage premium calculation |
| BR-UND-001 | Multi-factor rating algorithm |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | MultiPolicyDiscount = 1 | Factor = 0.90 |
| 2 | MultiPolicyDiscount = 0 | Factor = 1.0 |

---

### Rule ID: BR-UND-009
**Module**: UND
**Priority**: High

#### Rule Description
New home discount. Property age <= 5 years = 15% discount (0.85). Property age 6-10 years = 8% discount (0.92). Property age > 10 = no discount (1.0). Property age = YEAR(EffectiveDate) - YearBuilt.

#### Source
- **File**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
- **SP/Method**: Underwriting.usp_Premium_CalculateCoverage
- **Code Snippet**:
```sql
IF @PropertyAge <= 5
    SET @NewHomeFactor = 0.85;
ELSE IF @PropertyAge <= 10
    SET @NewHomeFactor = 0.92;
```

#### Enforcement Mechanism
- Type: SP logic (age-based tiered discount)
- Behavior: Hardcoded thresholds; PropertyAge = YEAR(@EffectiveDate) - @YearBuilt

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-002 | Coverage premium calculation |
| BR-UND-001 | Multi-factor rating algorithm |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | PropertyAge = 0 (brand new) | Factor = 0.85 |
| 2 | PropertyAge = 5 | Factor = 0.85 (boundary) |
| 3 | PropertyAge = 6 | Factor = 0.92 |
| 4 | PropertyAge = 10 | Factor = 0.92 (boundary) |
| 5 | PropertyAge = 11 | Factor = 1.0 |

---

### Rule ID: BR-UND-010
**Module**: UND
**Priority**: High

#### Rule Description
Commission calculation uses tiered schedule from Underwriting.CommissionSchedules (by PolicyType, TransactionType, premium range). Falls back to agent default CommissionRate if no schedule match. CommissionAmount = ROUND(PremiumAmount * CommissionRate, 2).

#### Source
- **File**: `database/02-stored-procedures/005-underwriting-sps.sql`
- **SP/Method**: Underwriting.usp_Commission_Calculate
- **Code Snippet**:
```sql
SELECT TOP 1 @CommissionRate = CommissionPercent
FROM Underwriting.CommissionSchedules
WHERE PolicyType = @PolicyType AND TransactionType = @TransactionType
    AND @PremiumAmount BETWEEN MinPremium AND ISNULL(MaxPremium, 99999999)
    AND GETDATE() BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
ORDER BY EffectiveDate DESC;
IF @CommissionRate = 0
    SELECT @CommissionRate = CommissionRate FROM Policy.Agents WHERE AgentID = @AgentID;
SET @CommissionAmount = ROUND(@PremiumAmount * @CommissionRate, 2);
```

#### Enforcement Mechanism
- Type: SP logic (schedule lookup with agent fallback)
- Behavior: Schedule rate takes precedence; agent default only when no schedule matches

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-UND-014 | Commission calculation |
| FT-UND-006 | Commission Calculation Workflow |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Schedule rate found | Schedule rate used |
| 2 | No schedule match, agent has rate | Agent default rate used |
| 3 | No schedule, no agent rate | CommissionRate = 0, CommissionAmount = 0 |
| 4 | MaxPremium NULL in schedule | ISNULL converts to 99999999 (effectively unlimited) |
| 5 | Premium at exact MinPremium boundary | Matches (BETWEEN is inclusive) |
