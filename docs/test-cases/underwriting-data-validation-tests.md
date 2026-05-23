# Underwriting Module - Data Validation Tests

## Module: UND (Underwriting)
## Test Type: Data Integrity and Validation Tests
## Schema Source: `database/01-schema/005-underwriting-tables.sql`

---

### Test Case ID: DV-UND-001
**Table**: Underwriting.BaseRates
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| BaseRateID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PolicyType | VARCHAR(30) | NO | None | Valid policy types |
| StateCode | CHAR(2) | NO | None | Valid state code |
| TerritoryCode | VARCHAR(20) | YES | NULL | Territory identifier or NULL (state-level) |
| ConstructionType | VARCHAR(30) | NO | None | Valid construction types |
| ProtectionClass | INT | NO | None | Range 1-10 |
| CoverageCode | VARCHAR(20) | NO | None | Valid coverage code |
| RatePer1000 | DECIMAL(10,6) | NO | None | > 0 |
| MinPremium | DECIMAL(10,2) | YES | 0 | >= 0 |
| EffectiveDate | DATE | NO | None | Not null |
| ExpiryDate | DATE | NO | '9999-12-31' | >= EffectiveDate |
| IsActive | BIT | YES | 1 | Boolean |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | RatePer1000 positive | All records | RatePer1000 > 0 |
| 2 | MinPremium non-negative | All records | MinPremium >= 0 |
| 3 | ExpiryDate >= EffectiveDate | All records | Date range valid |
| 4 | ProtectionClass 1-10 | All records | Value between 1 and 10 |
| 5 | StateCode valid | All records | 2-character valid state |
| 6 | Territory-specific vs state-level | TerritoryCode NULL or valid | NULL = state-level rate |
| 7 | No overlapping effective periods | Same key combination | No overlapping date ranges for same criteria |
| 8 | Active rates exist for current date | Production data | At least one active rate per coverage type |

---

### Test Case ID: DV-UND-002
**Table**: Underwriting.RatingFactors
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| RatingFactorID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| FactorType | VARCHAR(30) | NO | None | Valid: AGE_OF_HOME, ROOF_TYPE, CREDIT_SCORE, CLAIMS_HISTORY, PROTECTIVE_DEVICE, CONSTRUCTION_TYPE, PROTECTION_CLASS, OCCUPANCY, ROOF_AGE |
| PolicyType | VARCHAR(30) | NO | None | Valid policy type |
| StateCode | CHAR(2) | YES | NULL | State-specific or NULL (generic) |
| FactorKey | VARCHAR(50) | NO | None | Key or range descriptor |
| FactorKeyDescription | VARCHAR(200) | YES | NULL | Human-readable description |
| MinRange | DECIMAL(18,4) | YES | NULL | For range-based factors |
| MaxRange | DECIMAL(18,4) | YES | NULL | For range-based factors |
| Factor | DECIMAL(10,6) | NO | None | Multiplier (> 0) |
| EffectiveDate | DATE | NO | None | Not null |
| ExpiryDate | DATE | NO | '9999-12-31' | >= EffectiveDate |
| IsActive | BIT | YES | 1 | Boolean |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | Factor positive | All records | Factor > 0 (cannot be negative or zero) |
| 2 | Range consistency | Range-based factors | MaxRange >= MinRange |
| 3 | Range-based has MinRange/MaxRange | FactorType in (AGE_OF_HOME, CREDIT_SCORE, CLAIMS_HISTORY, PROTECTIVE_DEVICE, PROTECTION_CLASS, ROOF_AGE) | MinRange and MaxRange NOT NULL |
| 4 | Key-based has FactorKey | FactorType in (CONSTRUCTION_TYPE, ROOF_TYPE, OCCUPANCY) | FactorKey is meaningful value |
| 5 | ExpiryDate >= EffectiveDate | All records | Valid date range |
| 6 | No overlapping ranges | Same FactorType+PolicyType+StateCode | Ranges do not overlap |
| 7 | StateCode NULL or valid | All records | NULL for generic, valid 2-char for state-specific |
| 8 | FactorType values valid | All records | Only known FactorType values |

---

### Test Case ID: DV-UND-003
**Table**: Underwriting.Moratoriums
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| MoratoriumID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| MoratoriumName | VARCHAR(200) | NO | None | Not empty |
| MoratoriumType | VARCHAR(20) | NO | None | Valid: NEW_BUSINESS, RENEWAL, ALL |
| Reason | VARCHAR(500) | YES | NULL | Optional |
| AffectedStates | VARCHAR(200) | YES | NULL | Comma-separated state codes or NULL (all) |
| AffectedZipCodes | VARCHAR(MAX) | YES | NULL | Comma-separated zips or NULL (all) |
| AffectedPolicyTypes | VARCHAR(200) | YES | NULL | Comma-separated types or NULL (all) |
| StartDate | DATE | NO | None | Not null |
| EndDate | DATE | YES | NULL | NULL = indefinite; if set, >= StartDate |
| IsActive | BIT | YES | 1 | Boolean |
| DeclaredBy | VARCHAR(50) | YES | None | Username |
| DeclaredDate | DATETIME | YES | GETDATE() | Auto-set |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | MoratoriumType values | All records | Only NEW_BUSINESS, RENEWAL, or ALL |
| 2 | EndDate >= StartDate | EndDate NOT NULL | EndDate on or after StartDate |
| 3 | IsActive consistency | Active moratoriums | GETDATE() BETWEEN StartDate AND ISNULL(EndDate, '9999-12-31') |
| 4 | AffectedStates format | Non-NULL values | Comma-separated 2-character state codes [ASSUMPTION] |
| 5 | MoratoriumName not empty | All records | LEN(MoratoriumName) > 0 |
| 6 | DeclaredBy populated | All records | Username of declarer present |

---

### Test Case ID: DV-UND-004
**Table**: Underwriting.Referrals
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| ReferralID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| RuleID | INT | YES | None | FK -> Underwriting.Rules(RuleID) |
| ReferralReason | VARCHAR(500) | NO | None | Not empty |
| ReferralStatus | VARCHAR(20) | YES | 'PENDING' | Valid: PENDING, APPROVED, DECLINED, CONDITIONAL |
| ReferralDate | DATETIME | YES | GETDATE() | Auto-set |
| AssignedTo | VARCHAR(50) | YES | NULL | Underwriter username |
| ReviewedBy | VARCHAR(50) | YES | NULL | Set on decision |
| ReviewDate | DATETIME | YES | NULL | Set on decision |
| Decision | VARCHAR(20) | YES | NULL | Same as ReferralStatus update |
| DecisionNotes | VARCHAR(MAX) | YES | NULL | Optional |
| Conditions | VARCHAR(MAX) | YES | NULL | Required for CONDITIONAL |
| ExpiryDate | DATE | YES | NULL | Approval expiry |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | PolicyID FK valid | All records | References valid Policy.Policies row |
| 2 | ReferralStatus values | All records | Only PENDING, APPROVED, DECLINED, CONDITIONAL |
| 3 | ReviewedBy set when decided | Status != PENDING | ReviewedBy NOT NULL |
| 4 | ReviewDate set when decided | Status != PENDING | ReviewDate NOT NULL |
| 5 | Conditions set for CONDITIONAL | Status = CONDITIONAL | Conditions NOT NULL and not empty |
| 6 | ReferralReason not empty | All records | LEN(ReferralReason) > 0 |
| 7 | ReferralDate <= ReviewDate | Reviewed records | Decision not before referral |
| 8 | Decision matches ReferralStatus | Processed records | Decision = ReferralStatus |

---

### Test Case ID: DV-UND-005
**Table**: Underwriting.RatingWorksheets
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| WorksheetID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| CoverageCode | VARCHAR(20) | NO | None | Coverage identifier |
| CalculationDate | DATETIME | YES | GETDATE() | Auto-set |
| BaseRate | DECIMAL(18,6) | YES | None | >= 0 |
| InsuredValue | DECIMAL(18,2) | YES | None | >= 0 |
| BasePremium | DECIMAL(18,2) | YES | None | = (InsuredValue/1000) * BaseRate |
| ConstructionFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| AgeFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| RoofFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| ProtectionClassFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| DeductibleFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| CreditFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| ClaimsHistoryFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| ProtectiveDeviceFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| TerritoryFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| OccupancyFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| LoyaltyFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| MultiPolicyFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| NewHomeFactor | DECIMAL(10,6) | YES | 1.0 | > 0 |
| TotalFactor | DECIMAL(10,6) | YES | None | Product of all 13 factors |
| CalculatedPremium | DECIMAL(18,2) | YES | None | = BasePremium * TotalFactor |
| MinPremiumApplied | BIT | YES | 0 | Boolean |
| FinalPremium | DECIMAL(18,2) | YES | None | = MAX(CalculatedPremium, MinPremium) |
| CalculatedBy | VARCHAR(50) | YES | None | Username |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | All factors > 0 | All records | No negative or zero factors |
| 2 | TotalFactor = product of 13 factors | All records | Multiplicative consistency |
| 3 | BasePremium = (InsuredValue/1000) * BaseRate | All records | Calculation correct |
| 4 | FinalPremium >= MinPremium when MinPremiumApplied=1 | MinPremiumApplied records | Floor enforced |
| 5 | FinalPremium = CalculatedPremium when MinPremiumApplied=0 | Non-minimum records | No adjustment |
| 6 | PolicyID FK valid | All records | References valid policy |
| 7 | One worksheet per PolicyID+CoverageCode | All records | Unique combination (DELETE before INSERT) |

---

### Test Case ID: DV-UND-006
**Table**: Underwriting.RateTables / Underwriting.RateTableDetails
**Priority**: High

#### Column Constraints (RateTables)
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| RateTableID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| RateTableCode | VARCHAR(30) | NO | None | Part of unique constraint |
| RateTableName | VARCHAR(100) | NO | None | Not empty |
| RateType | VARCHAR(20) | NO | None | Valid: BASE, FACTOR, DISCOUNT, SURCHARGE, TAX |
| PolicyType | VARCHAR(30) | NO | None | Valid policy type |
| StateCode | CHAR(2) | YES | NULL | State or NULL |
| EffectiveDate | DATE | NO | None | Not null |
| ExpiryDate | DATE | NO | '9999-12-31' | >= EffectiveDate |
| Version | INT | YES | 1 | >= 1, auto-incremented |
| FilingNumber | VARCHAR(50) | YES | NULL | Regulatory filing reference |
| IsActive | BIT | YES | 1 | Boolean |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | UNIQUE(RateTableCode, StateCode, EffectiveDate) | All records | No duplicates |
| 2 | Version auto-increments | Same Code+State | Each version > previous |
| 3 | ExpiryDate >= EffectiveDate | All records | Valid date range |
| 4 | RateType values valid | All records | Only BASE, FACTOR, DISCOUNT, SURCHARGE, TAX |
| 5 | Only one active version per Code+State | Active records | At most one with ExpiryDate='9999-12-31' |
| 6 | RateTableDetails FK valid | All detail rows | RateTableID references valid header |
| 7 | Detail Rate > 0 | All detail rows | Positive rate values |
| 8 | Detail range consistency | Range-based rows | MaxValue >= MinValue when both set |

---

### Test Case ID: DV-UND-007
**Table**: Underwriting.CommissionSchedules
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| CommissionScheduleID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| ScheduleCode | VARCHAR(20) | NO | None | Identifier |
| ScheduleName | VARCHAR(100) | NO | None | Not empty |
| PolicyType | VARCHAR(30) | NO | None | Valid policy type |
| TransactionType | VARCHAR(20) | NO | None | Valid: NEW, RENEWAL, ENDORSEMENT |
| CommissionPercent | DECIMAL(6,4) | NO | None | Between 0 and 1 |
| OverridePercent | DECIMAL(6,4) | YES | 0 | >= 0 |
| BonusPercent | DECIMAL(6,4) | YES | 0 | >= 0 |
| MinPremium | DECIMAL(18,2) | YES | 0 | >= 0 |
| MaxPremium | DECIMAL(18,2) | YES | NULL | NULL or > MinPremium |
| EffectiveDate | DATE | NO | None | Not null |
| ExpiryDate | DATE | NO | '9999-12-31' | >= EffectiveDate |
| IsActive | BIT | YES | 1 | Boolean |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | CommissionPercent between 0 and 1 | All records | Valid percentage range |
| 2 | TransactionType values valid | All records | Only NEW, RENEWAL, ENDORSEMENT |
| 3 | MaxPremium > MinPremium | MaxPremium NOT NULL | Valid range |
| 4 | No overlapping premium ranges | Same PolicyType+TransactionType+Date | Premium bands do not overlap |
| 5 | ExpiryDate >= EffectiveDate | All records | Valid date range |
