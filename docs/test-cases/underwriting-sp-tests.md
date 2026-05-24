# Underwriting Module - Stored Procedure Tests

## Module: UND (Underwriting)
## Source Files:
- `database/02-stored-procedures/002-premium-calculation-sps.sql`
- `database/02-stored-procedures/005-underwriting-sps.sql`

## Stored Procedures Covered (19 total):
1. Underwriting.usp_Premium_Calculate
2. Underwriting.usp_Premium_CalculateCoverage
3. Underwriting.usp_Premium_CalculateTaxesFees
4. Underwriting.usp_Premium_CalculateEndorsement
5. Underwriting.usp_Rate_GetBaseRate
6. Underwriting.usp_Rate_GetFactor
7. Underwriting.usp_Rules_Evaluate
8. Underwriting.usp_Referral_Process
9. Underwriting.usp_Referral_GetByPolicy
10. Underwriting.usp_Referral_SearchPending
11. Underwriting.usp_Moratorium_Create
12. Underwriting.usp_Moratorium_Lift
13. Underwriting.usp_Moratorium_Check
14. Underwriting.usp_Moratorium_GetActive
15. Underwriting.usp_Worksheet_GetByPolicy
16. Underwriting.usp_RateTable_Create
17. Underwriting.usp_RateTable_AddDetail
18. Underwriting.usp_RateTable_GetDetails
19. Underwriting.usp_Commission_Calculate

---

### Test Case ID: SP-UND-001
**Procedure**: Underwriting.usp_Premium_Calculate
**Source**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @CalculatedBy VARCHAR(50) - Required
- @RecalculateAll BIT - Optional (default 1)
- @TotalPremium DECIMAL(18,2) OUTPUT
- @TotalTaxes DECIMAL(18,2) OUTPUT
- @GrossPremium DECIMAL(18,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Calculate premium for policy with single coverage | @PolicyID=valid (1 selected coverage), @CalculatedBy='testuser' | TotalPremium > 0, coverage premium updated |
| 2 | Calculate premium for policy with multiple coverages | @PolicyID=valid (3+ coverages), @CalculatedBy='testuser' | TotalPremium = sum of all coverage premiums |
| 3 | GrossPremium includes taxes | Valid policy | GrossPremium = TotalPremium + TotalTaxes |
| 4 | Commission calculated from policy rate | Valid policy with CommissionRate | CommissionAmount = TotalPremium * CommissionRate |
| 5 | Policy totals updated | Valid policy | AnnualPremium, WrittenPremium, TotalTaxes, GrossPremium updated |
| 6 | TotalInsuredValue updated | Valid policy | TotalInsuredValue = SUM(LimitAmount) of selected coverages |
| 7 | Only selected coverages processed | Policy with IsSelected=1 and IsSelected=0 coverages | Only IsSelected=1 coverages in cursor |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |
| 2 | NULL PolicyID | @PolicyID=NULL | Error (parameter required) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Policy with no selected coverages | @PolicyID=valid (all IsSelected=0) | TotalPremium = 0, no cursor iterations |
| 2 | Policy with zero insured values | All LimitAmount = 0 | TotalPremium = 0 or MinPremium per coverage |
| 3 | Transaction rollback on error | Force error mid-calculation | All changes rolled back, error logged |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Policy.Policies record exists | Valid policy with property | @PolicyID=valid | PolicyType, PropertyID, CustomerID resolved |
| 2 | Policy.Properties record exists | Valid property linked | @PolicyID=valid | StateCode, ConstructionType, YearBuilt etc. resolved |
| 3 | Policy.Customers credit score | Valid customer | @PolicyID=valid | CreditScore retrieved for rating |
| 4 | #CoveragePremiums temp table | N/A | Valid policy | Temp table created and dropped in transaction |
| 5 | Audit.ErrorLog on failure | Error occurs | Force error | ErrorLog row with PolicyID in AdditionalInfo |

---

### Test Case ID: SP-UND-002
**Procedure**: Underwriting.usp_Premium_CalculateCoverage
**Source**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
**Parameters**:
- @PolicyID INT, @CoverageID INT, @CoverageCode VARCHAR(20)
- @InsuredValue DECIMAL(18,2), @DeductibleAmount DECIMAL(18,2), @DeductibleType VARCHAR(20)
- @PolicyType VARCHAR(30), @StateCode CHAR(2), @ConstructionType VARCHAR(30)
- @YearBuilt INT, @ProtectionClass INT, @RoofType VARCHAR(30), @RoofAge INT
- @OccupancyType VARCHAR(30), @HasFireAlarm BIT, @HasBurglarAlarm BIT, @HasSprinklerSystem BIT
- @CreditScore INT, @ClaimFreeYears INT, @RenewalCount INT
- @MultiPolicyDiscount BIT, @TerritoryID INT, @EffectiveDate DATE
- @CalculatedBy VARCHAR(50) - Optional (default 'SYSTEM')
- @CoveragePremium DECIMAL(18,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Basic premium calculation (all defaults) | Valid inputs, all factors = 1.0 | CoveragePremium = (InsuredValue/1000) * BaseRate |
| 2 | Construction factor applied | @ConstructionType='FRAME' (factor > 1) | Premium increased by construction factor |
| 3 | Age factor applied (range-based) | @YearBuilt producing PropertyAge in banded range | Age factor multiplied into total |
| 4 | Roof factor with age surcharge | @RoofType='ASPHALT', @RoofAge=25 | RoofFactor = base roof * roof age surcharge |
| 5 | Protection class factor | @ProtectionClass=8 | Protection class factor applied |
| 6 | Deductible factor from DeductibleOptions | @DeductibleType='FLAT', @DeductibleAmount=1000 | PremiumFactor from DeductibleOptions table |
| 7 | Credit score factor (range) | @CreditScore=720 | Factor for 700-749 range applied |
| 8 | Claims history factor | Customer with 2 prior claims in 5 years | Factor for 2-claim range |
| 9 | Protective device discount (3 devices) | @HasFireAlarm=1, @HasBurglarAlarm=1, @HasSprinklerSystem=1 | DeviceCount=3, factor for 3 range |
| 10 | Territory factor from Territories | @TerritoryID=valid | RiskMultiplier from Policy.Territories |
| 11 | Occupancy factor (key-based) | @OccupancyType='OWNER_OCCUPIED' | Factor for occupancy key |
| 12 | Loyalty discount (5+ renewals) | @RenewalCount=5 | LoyaltyFactor = 0.90 |
| 13 | Multi-policy discount | @MultiPolicyDiscount=1 | MultiPolicyFactor = 0.90 |
| 14 | New home discount (age <= 5) | @YearBuilt producing age <= 5 | NewHomeFactor = 0.85 |
| 15 | Minimum premium applied | Low insured value producing premium < MinPremium | CoveragePremium = MinPremium |
| 16 | Rating worksheet stored | Valid calculation | RatingWorksheets row with all factors |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No base rate found | Invalid combination of PolicyType/State/Construction/Protection/Coverage | BaseRate = 0, BasePremium = 0 |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | InsuredValue = 0 | @InsuredValue=0 | BasePremium = 0, MinPremium may apply |
| 2 | CreditScore IS NULL | @CreditScore=NULL | Credit factor lookup skipped, factor = 1.0 |
| 3 | RoofType IS NULL | @RoofType=NULL | Roof factor lookup skipped, factor = 1.0 |
| 4 | RoofAge = 20 (boundary) | @RoofAge=20 | Roof age surcharge NOT applied (condition is > 20) |
| 5 | RoofAge = 21 | @RoofAge=21 | Roof age surcharge IS applied |
| 6 | TerritoryID IS NULL | @TerritoryID=NULL | Territory factor = 1.0 |
| 7 | No prior claims | 0 claims in 5 years | ClaimsHistoryFactor from 0-range |
| 8 | DeviceCount = 0 | All devices = 0 | Protective device lookup skipped |

---

### Test Case ID: SP-UND-003
**Procedure**: Underwriting.usp_Premium_CalculateTaxesFees
**Source**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @StateCode CHAR(2) - Required
- @PolicyType VARCHAR(30) - Required
- @PremiumAmount DECIMAL(18,2) - Required
- @EffectiveDate DATE - Required
- @TotalTaxes DECIMAL(18,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | State taxes calculated | @StateCode='FL', @PremiumAmount=1000 | TotalTaxes = StateTax + Surcharge + StampingFee + FireMarshalFee + PolicyFee |
| 2 | Policy fee from config (type-specific) | @PolicyType='HO3' | PolicyFee from POLICY_FEE_HO3 config |
| 3 | Policy fee fallback to default | @PolicyType with no specific config | PolicyFee from POLICY_FEE_DEFAULT config |
| 4 | Policy updated with breakdown | Valid inputs | Policy TotalTaxes, TotalFees, TotalSurcharges updated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid state code | @StateCode='XX' | All rates = 0, TotalTaxes = PolicyFee only |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Zero premium | @PremiumAmount=0 | All percentage-based taxes = 0, PolicyFee still applies |
| 2 | No policy fee configured | Neither type nor default exists | PolicyFee = NULL, ISNULL handles it as 0 |

---

### Test Case ID: SP-UND-004
**Procedure**: Underwriting.usp_Premium_CalculateEndorsement
**Source**: `database/02-stored-procedures/002-premium-calculation-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @EndorsementID INT - Required
- @EndorsementEffectiveDate DATE - Required
- @CalculatedBy VARCHAR(50) - Required
- @PremiumChange DECIMAL(18,2) OUTPUT
- @ReturnPremium DECIMAL(18,2) OUTPUT
- @AdditionalPremium DECIMAL(18,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Endorsement with premium increase | Coverage limit increased | AdditionalPremium > 0, ReturnPremium = 0 |
| 2 | Endorsement with premium decrease | Coverage limit decreased | ReturnPremium > 0, AdditionalPremium = 0 |
| 3 | Pro-rata factor calculated | Mid-term endorsement | ProRataFactor = DaysRemaining / DaysInTerm |
| 4 | Endorsement record updated | Valid inputs | PremiumChange, ProRataFactor, ReturnPremium, AdditionalPremium set |
| 5 | Calls usp_Premium_Calculate | Valid endorsement | Full premium recalculated |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Endorsement on first day of term | @EndorsementEffectiveDate = PolicyEffectiveDate | ProRataFactor = 1.0 (full term) |
| 2 | Endorsement on last day | @EndorsementEffectiveDate = PolicyExpiryDate - 1 | ProRataFactor near 0 |
| 3 | No premium change | Same coverages/limits | PremiumChange = 0, both return and additional = 0 |

---

### Test Case ID: SP-UND-005
**Procedure**: Underwriting.usp_Rate_GetBaseRate
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyType VARCHAR(30) - Required
- @StateCode CHAR(2) - Required
- @ConstructionType VARCHAR(30) - Required
- @ProtectionClass INT - Required
- @CoverageCode VARCHAR(20) - Required
- @TerritoryCode VARCHAR(20) - Optional (default NULL)
- @EffectiveDate DATE - Required
- @RatePer1000 DECIMAL(10,6) OUTPUT
- @MinPremium DECIMAL(10,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Territory-specific rate found | @TerritoryCode=valid territory with rate | RatePer1000 from territory-specific row |
| 2 | Fallback to state-level rate | @TerritoryCode=valid but no territory rate | RatePer1000 from TerritoryCode IS NULL row |
| 3 | TerritoryCode NULL uses state-level directly | @TerritoryCode=NULL | State-level rate returned |
| 4 | Most recent effective rate selected | Multiple rates, different EffectiveDates | TOP 1 ORDER BY EffectiveDate DESC |
| 5 | MinPremium returned with rate | Valid lookup | Both RatePer1000 and MinPremium populated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching rate at any level | Invalid combination | RatePer1000 = 0, MinPremium = 0 |
| 2 | Rate exists but inactive | IsActive = 0 | Not selected, outputs remain 0 |
| 3 | Rate exists but expired | EffectiveDate outside range | Not selected |

---

### Test Case ID: SP-UND-006
**Procedure**: Underwriting.usp_Rate_GetFactor
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @FactorType VARCHAR(30) - Required
- @PolicyType VARCHAR(30) - Required
- @StateCode CHAR(2) - Required
- @FactorKey VARCHAR(50) - Optional (default NULL)
- @RangeValue DECIMAL(18,4) - Optional (default NULL)
- @EffectiveDate DATE - Required
- @Factor DECIMAL(10,6) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Key-based lookup | @FactorType='CONSTRUCTION_TYPE', @FactorKey='FRAME' | Factor from matching key row |
| 2 | Range-based lookup | @FactorType='CREDIT_SCORE', @RangeValue=720 | Factor where 720 BETWEEN MinRange AND MaxRange |
| 3 | State-specific overrides generic | State row and NULL row exist | State-specific factor returned (ORDER BY StateCode DESC) |
| 4 | Generic factor (no state match) | Only StateCode IS NULL row | Generic factor returned |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching factor | Invalid FactorType/Key | Factor = 1.0 (default) |
| 2 | Both FactorKey and RangeValue NULL | @FactorKey=NULL, @RangeValue=NULL | Factor = 1.0 (no lookup) |

---

### Test Case ID: SP-UND-007
**Procedure**: Underwriting.usp_Rules_Evaluate
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @EvaluatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Property age > 75 triggers REFER | Policy with YearBuilt=1940 | AGE_75_PLUS rule triggered, severity HIGH |
| 2 | Protection class >= 9 triggers REFER | @ProtectionClass=9 | PROT_CLASS_9_10 rule triggered, severity MEDIUM |
| 3 | Roof age > 25 triggers REFER | @RoofAge=30 | ROOF_AGE_25 rule triggered, severity MEDIUM |
| 4 | Credit < 550 triggers DECLINE | @CreditScore=500 | CREDIT_BELOW_550 triggered, severity CRITICAL |
| 5 | Insured value > M triggers REFER | TIV=2500000 | HIGH_VALUE rule triggered, severity HIGH |
| 6 | 3+ claims in 3 years triggers REFER | 3 non-DENIED claims | PRIOR_CLAIMS_3 triggered, severity HIGH |
| 7 | Multiple rules triggered | Property age 80, roof age 30 | Multiple rows in result |
| 8 | Referrals created | Rules triggered | Underwriting.Referrals rows inserted |
| 9 | Results ordered by severity | Multiple severities | CRITICAL first, then HIGH, MEDIUM |
| 10 | Database-stored rules evaluated | Active rule in Underwriting.Rules | Rule included in results |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |
| 2 | No rules triggered | All values within normal ranges | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Property age exactly 75 | YearBuilt producing age=75 | NOT triggered (> 75 required) |
| 2 | Protection class = 8 | @ProtectionClass=8 | NOT triggered (>= 9 required) |
| 3 | Roof age exactly 25 | @RoofAge=25 | NOT triggered (> 25 required) |
| 4 | Credit score = 550 | @CreditScore=550 | NOT triggered (< 550 required) |
| 5 | Insured value = M exactly | TIV=2000000 | NOT triggered (> M required) |
| 6 | Credit score IS NULL | @CreditScore=NULL | Credit rule skipped |
| 7 | Roof age IS NULL | @RoofAge=NULL | Roof rule skipped (explicit NULL check) |

---

### Test Case ID: SP-UND-008
**Procedure**: Underwriting.usp_Referral_Process
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @ReferralID INT - Required
- @Decision VARCHAR(20) - Required (APPROVED, DECLINED, CONDITIONAL)
- @DecisionNotes VARCHAR(MAX) - Optional (default NULL)
- @Conditions VARCHAR(MAX) - Optional (default NULL)
- @ReviewedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Approve referral | @Decision='APPROVED' | ReferralStatus='APPROVED', ReviewedBy set, ReviewDate set |
| 2 | Decline referral | @Decision='DECLINED' | ReferralStatus='DECLINED', policy status='DECLINED' |
| 3 | Conditional approval | @Decision='CONDITIONAL', @Conditions='Install sprinklers' | Status='CONDITIONAL', Conditions stored |
| 4 | Audit log created | Any decision | AuditLog row with old/new ReferralStatus |
| 5 | Policy declined on DECLINED decision | @Decision='DECLINED' | Policy.Policies.PolicyStatus='DECLINED' (if QUOTE or REFERRED) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Referral not found | @ReferralID=99999 | 'Referral not found: 99999' |
| 2 | Referral not PENDING | Already APPROVED referral | 'Referral is not in PENDING status. Current: APPROVED' |

---

### Test Case ID: SP-UND-009
**Procedure**: Underwriting.usp_Referral_GetByPolicy
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Return referrals for policy | @PolicyID with referrals | All referrals with Rule details joined |
| 2 | Ordered by date descending | Multiple referrals | Most recent first |
| 3 | No referrals | @PolicyID with no referrals | Empty result set |

---

### Test Case ID: SP-UND-010
**Procedure**: Underwriting.usp_Moratorium_Check
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyType VARCHAR(30) - Required
- @StateCode CHAR(2) - Required
- @ZipCode VARCHAR(10) - Required
- @TransactionType VARCHAR(20) - Optional (default 'NEW_BUSINESS')
- @IsMoratorium BIT OUTPUT
- @MoratoriumName VARCHAR(200) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Active moratorium matches | Matching state, zip, type | IsMoratorium=1, MoratoriumName populated |
| 2 | No moratorium | Non-matching criteria | IsMoratorium=0, MoratoriumName=NULL |
| 3 | ALL type matches NEW_BUSINESS | MoratoriumType='ALL', @TransactionType='NEW_BUSINESS' | IsMoratorium=1 |
| 4 | ALL type matches RENEWAL | MoratoriumType='ALL', @TransactionType='RENEWAL' | IsMoratorium=1 |
| 5 | NULL AffectedStates matches all | AffectedStates IS NULL | Matches any state |
| 6 | NULL AffectedPolicyTypes matches all | AffectedPolicyTypes IS NULL | Matches any policy type |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Moratorium with NULL EndDate | Active, no end date | Matches (ISNULL converts to 9999-12-31) |
| 2 | Expired moratorium | EndDate < GETDATE() | Not matched |
| 3 | Future moratorium | StartDate > GETDATE() | Not matched |

---

### Test Case ID: SP-UND-011
**Procedure**: Underwriting.usp_Moratorium_Create
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @MoratoriumName VARCHAR(200) - Required
- @MoratoriumType VARCHAR(20) - Required
- @Reason VARCHAR(500) - Optional
- @AffectedStates VARCHAR(200) - Optional
- @AffectedZipCodes VARCHAR(MAX) - Optional
- @AffectedPolicyTypes VARCHAR(200) - Optional
- @StartDate DATE - Required
- @EndDate DATE - Optional (default NULL)
- @DeclaredBy VARCHAR(50) - Required
- @MoratoriumID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create moratorium | All required fields | MoratoriumID > 0, IsActive=1, DeclaredDate set |
| 2 | Create with end date | @EndDate=future date | EndDate stored |
| 3 | Create without end date | @EndDate=NULL | EndDate NULL (indefinite) |
| 4 | Audit log created | Valid inputs | AuditLog row with Action='INSERT' |

---

### Test Case ID: SP-UND-012
**Procedure**: Underwriting.usp_Moratorium_Lift
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @MoratoriumID INT - Required
- @LiftedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Lift active moratorium | @MoratoriumID=active moratorium | IsActive=0, EndDate=today |
| 2 | Audit log created | Valid lift | AuditLog row with IsActive 1->0 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Moratorium not found or inactive | @MoratoriumID=99999 | 'Active moratorium not found: 99999' |

---

### Test Case ID: SP-UND-013
**Procedure**: Underwriting.usp_Referral_SearchPending
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @AssignedTo VARCHAR(50) - Optional (default NULL)
- @PolicyType VARCHAR(30) - Optional (default NULL)
- @StateCode CHAR(2) - Optional (default NULL)
- @PageNumber INT - Optional (default 1)
- @PageSize INT - Optional (default 50)
- @TotalRecords INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Return all pending | No filters | All PENDING referrals, ordered by severity then date |
| 2 | Filter by AssignedTo | @AssignedTo='underwriter1' | Only referrals assigned to that user |
| 3 | Filter by PolicyType | @PolicyType='HO3' | Only HO3 policy referrals |
| 4 | Filter by StateCode | @StateCode='FL' | Only FL property referrals |
| 5 | Pagination | @PageNumber=2, @PageSize=10 | Rows 11-20, TotalRecords = total count |
| 6 | TotalRecords output | Any query | Correct count of matching records |
| 7 | Results include policy and customer info | Valid data | PolicyNumber, CustomerName, Premium, Severity returned |

---

### Test Case ID: SP-UND-014
**Procedure**: Underwriting.usp_Commission_Calculate
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @TransactionType VARCHAR(20) - Optional (default 'NEW')
- @PremiumAmount DECIMAL(18,2) - Required
- @CommissionAmount DECIMAL(18,2) OUTPUT
- @CommissionRate DECIMAL(6,4) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Schedule rate found | Matching CommissionSchedules row | CommissionRate from schedule |
| 2 | Fallback to agent rate | No schedule match | CommissionRate from Policy.Agents |
| 3 | Commission calculated | @PremiumAmount=1000, rate=0.10 | CommissionAmount = 100.00 |
| 4 | Policy updated | Valid calculation | Policy.CommissionRate and CommissionAmount updated |
| 5 | NEW transaction type | @TransactionType='NEW' | Matches NEW schedule rows |
| 6 | RENEWAL transaction type | @TransactionType='RENEWAL' | Matches RENEWAL schedule rows |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | No schedule and no agent rate | No matches anywhere | CommissionRate = 0, Amount = 0 |
| 2 | Premium at schedule boundary | @PremiumAmount = MinPremium exactly | Matches (BETWEEN inclusive) |

---

### Test Case ID: SP-UND-015
**Procedure**: Underwriting.usp_Worksheet_GetByPolicy
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @PolicyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Return worksheet with coverage details | @PolicyID with calculated premiums | All worksheet rows with CoverageName, LimitAmount, DeductibleAmount joined |
| 2 | Ordered by CoverageCode | Multiple coverages | Alphabetical by CoverageCode |
| 3 | No worksheet data | @PolicyID not yet calculated | Empty result set |

---

### Test Case ID: SP-UND-016
**Procedure**: Underwriting.usp_RateTable_Create
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @RateTableCode VARCHAR(30) - Required
- @RateTableName VARCHAR(100) - Required
- @RateType VARCHAR(20) - Required
- @PolicyType VARCHAR(30) - Required
- @StateCode CHAR(2) - Optional (default NULL)
- @EffectiveDate DATE - Required
- @ExpiryDate DATE - Optional (default '9999-12-31')
- @FilingNumber VARCHAR(50) - Optional (default NULL)
- @CreatedBy VARCHAR(50) - Required
- @RateTableID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create new rate table | All required fields | RateTableID > 0, Version=1, IsActive=1 |
| 2 | Version increments | Same RateTableCode+State | Version = max existing + 1 |
| 3 | Expires previous version | Existing active table | Previous ExpiryDate set to new EffectiveDate - 1 |
| 4 | Audit log created | Valid inputs | AuditLog row with Action='INSERT' |

---

### Test Case ID: SP-UND-017
**Procedure**: Underwriting.usp_RateTable_AddDetail
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @RateTableID INT - Required
- @FactorCode VARCHAR(30) - Required
- @FactorValue VARCHAR(100) - Optional (default NULL)
- @MinValue DECIMAL(18,4) - Optional (default NULL)
- @MaxValue DECIMAL(18,4) - Optional (default NULL)
- @Rate DECIMAL(18,6) - Required
- @FlatAmount DECIMAL(18,2) - Optional (default 0)
- @DisplayOrder INT - Optional (default 0)
- @RateDetailID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Add key-based detail row | @FactorValue='FRAME', @Rate=1.15 | RateDetailID > 0, IsActive=1 |
| 2 | Add range-based detail row | @MinValue=0, @MaxValue=5, @Rate=0.85 | Row with min/max range stored |
| 3 | Add with flat amount | @Rate=1.0, @FlatAmount=50.00 | Both rate and flat stored |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Rate table not found | @RateTableID=99999 | 'Rate table not found: 99999' |

---

### Test Case ID: SP-UND-018
**Procedure**: Underwriting.usp_RateTable_GetDetails
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**:
- @RateTableID INT - Optional (default NULL)
- @RateTableCode VARCHAR(30) - Optional (default NULL)
- @StateCode CHAR(2) - Optional (default NULL)
- @EffectiveDate DATE - Optional (default NULL, uses GETDATE())

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Lookup by RateTableID | @RateTableID=valid | Header + Details result sets |
| 2 | Lookup by Code+State | @RateTableCode='BASE_RATES_HO3', @StateCode='FL' | Resolves RateTableID, returns header+details |
| 3 | Default EffectiveDate = today | @EffectiveDate=NULL | Uses GETDATE() for date filtering |
| 4 | Details ordered | Valid table | ORDER BY DisplayOrder, FactorCode, MinValue |
| 5 | Only active details | Mix of active/inactive | Only IsActive=1 details returned |

---

### Test Case ID: SP-UND-019
**Procedure**: Underwriting.usp_Moratorium_GetActive
**Source**: `database/02-stored-procedures/005-underwriting-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Return active moratoriums | Active moratoriums exist | All active moratoriums with AffectedPolicyCount |
| 2 | AffectedPolicyCount calculated | Active policies in affected area | Count of matching policies |
| 3 | Ordered by StartDate descending | Multiple moratoriums | Most recent first |
| 4 | No active moratoriums | All inactive/expired | Empty result set |
