# Reinsurance Module - Stored Procedure Tests

## Module: RNS (Reinsurance)
## Source Files:
- `database/02-stored-procedures/007-reinsurance-sps.sql`

## Stored Procedures Covered (6 total):
1. Reinsurance.usp_Treaty_Create
2. Reinsurance.usp_Cession_CalculatePremium
3. Reinsurance.usp_Cession_CalculateLoss
4. Reinsurance.usp_Bordereaux_Generate
5. Reinsurance.usp_Treaty_GetSummary
6. Reinsurance.usp_Treaty_GetActive

---

### Test Case ID: SP-RNS-001
**Procedure**: Reinsurance.usp_Treaty_Create
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @TreatyName VARCHAR(200) - Required
- @TreatyType VARCHAR(30) - Required (QUOTA_SHARE, SURPLUS, EXCESS_OF_LOSS, CATASTROPHE, FACULTATIVE)
- @ReinsurerID INT - Optional (default NULL)
- @EffectiveDate DATE - Required
- @ExpiryDate DATE - Required
- @RetentionAmount DECIMAL(18,2) - Optional
- @RetentionPercent DECIMAL(6,4) - Optional
- @CessionPercent DECIMAL(6,4) - Optional
- @CessionLimit DECIMAL(18,2) - Optional
- @AttachmentPoint DECIMAL(18,2) - Optional
- @ExhaustionPoint DECIMAL(18,2) - Optional
- @PremiumRate DECIMAL(10,6) - Optional
- @MinimumPremium DECIMAL(18,2) - Optional
- @DepositPremium DECIMAL(18,2) - Optional
- @CommissionRate DECIMAL(6,4) - Optional
- @CoveredPerils VARCHAR(500) - Optional
- @CoveredStates VARCHAR(200) - Optional
- @CoveredPolicyTypes VARCHAR(200) - Optional
- @CreatedBy VARCHAR(50) - Required
- @TreatyID INT OUTPUT
- @TreatyNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create quota share treaty | @TreatyName='QS Treaty 2024', @TreatyType='QUOTA_SHARE', @EffectiveDate='2024-01-01', @ExpiryDate='2024-12-31', @CessionPercent=0.40, @CreatedBy='admin' | Treaty created, @TreatyID assigned, @TreatyNumber = 'TRY' + 7 digits, Status='ACTIVE' |
| 2 | Create surplus treaty with retention | @TreatyName='Surplus 2024', @TreatyType='SURPLUS', @RetentionAmount=500000, @CessionLimit=2000000, @EffectiveDate='2024-01-01', @ExpiryDate='2024-12-31', @CreatedBy='admin' | Treaty created with RetentionAmount and CessionLimit stored |
| 3 | Create excess-of-loss treaty | @TreatyName='XOL Layer 1', @TreatyType='EXCESS_OF_LOSS', @AttachmentPoint=1000000, @ExhaustionPoint=5000000, @CessionPercent=0.05, @EffectiveDate='2024-01-01', @ExpiryDate='2024-12-31', @CreatedBy='admin' | Treaty created with AttachmentPoint and ExhaustionPoint |
| 4 | Create treaty with reinsurer assigned | @TreatyName='Test', @TreatyType='QUOTA_SHARE', @ReinsurerID=valid_id, @EffectiveDate='2024-01-01', @ExpiryDate='2024-12-31', @CreatedBy='admin' | Treaty created with ReinsurerID set |
| 5 | Create treaty with covered perils/states/types | @CoveredPerils='FIRE,WIND,HAIL', @CoveredStates='FL,TX,CA', @CoveredPolicyTypes='HO3,HO5', other required params | Treaty created with filter fields populated |
| 6 | Create treaty with commission and premium rates | @CommissionRate=0.30, @PremiumRate=0.025, @MinimumPremium=50000, @DepositPremium=25000, other required params | Treaty created with financial terms stored |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL TreatyName | @TreatyName=NULL, other valid params | Insert fails (NOT NULL constraint on TreatyName) |
| 2 | NULL TreatyType | @TreatyType=NULL, other valid params | Insert fails (NOT NULL constraint on TreatyType) |
| 3 | NULL EffectiveDate | @EffectiveDate=NULL, other valid params | Insert fails (NOT NULL constraint on EffectiveDate) |
| 4 | NULL ExpiryDate | @ExpiryDate=NULL, other valid params | Insert fails (NOT NULL constraint on ExpiryDate) |
| 5 | Invalid ReinsurerID (FK violation) | @ReinsurerID=99999, other valid params | FK constraint violation error |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | TreatyName at max length (200 chars) | @TreatyName='A' * 200 | Treaty created successfully |
| 2 | TreatyName exceeds max length | @TreatyName='A' * 201 | Truncation or error |
| 3 | CessionPercent at maximum (0.9999) | @CessionPercent=0.9999 | Treaty created with 99.99% cession |
| 4 | CessionPercent at zero | @CessionPercent=0 | Treaty created with 0% cession |
| 5 | EffectiveDate equals ExpiryDate | @EffectiveDate='2024-01-01', @ExpiryDate='2024-01-01' | Treaty created (single-day treaty) |
| 6 | Very large AttachmentPoint | @AttachmentPoint=9999999999999999.99 | Stored without overflow |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | TreatyNumber sequential generation | Existing treaties in DB | Valid params | TreatyNumber = 'TRY' + (MAX(TreatyID) + 1) zero-padded to 7 digits |
| 2 | Audit log entry created | Audit.AuditLog accessible | Valid params | Row with Action='INSERT', TableName='Reinsurance.Treaties', Username=@CreatedBy |
| 3 | Reinsurer FK reference | Reinsurance.Reinsurers has matching ReinsurerID | @ReinsurerID=valid | Treaty created with FK link |
| 4 | Transaction rollback on failure | Simulate error mid-insert | Invalid data | No partial records, ErrorLog entry created |

#### Executable SQL Script
```sql
-- Setup
INSERT INTO Reinsurance.Reinsurers (ReinsurerCode, ReinsurerName, AMBestRating, IsActive)
VALUES ('TEST01', 'Test Reinsurer', 'A+', 1);
DECLARE @TestReinsurerID INT = SCOPE_IDENTITY();

-- Test: Create quota share treaty
DECLARE @NewTreatyID INT, @NewTreatyNumber VARCHAR(20);
EXEC Reinsurance.usp_Treaty_Create
    @TreatyName = 'Test QS Treaty',
    @TreatyType = 'QUOTA_SHARE',
    @ReinsurerID = @TestReinsurerID,
    @EffectiveDate = '2024-01-01',
    @ExpiryDate = '2024-12-31',
    @CessionPercent = 0.40,
    @CreatedBy = 'test_user',
    @TreatyID = @NewTreatyID OUTPUT,
    @TreatyNumber = @NewTreatyNumber OUTPUT;
-- Expected: @NewTreatyID > 0, @NewTreatyNumber LIKE 'TRY%'

SELECT * FROM Reinsurance.Treaties WHERE TreatyID = @NewTreatyID;
-- Expected: Status='ACTIVE', TreatyType='QUOTA_SHARE'

-- Cleanup
DELETE FROM Reinsurance.Treaties WHERE TreatyID = @NewTreatyID;
DELETE FROM Reinsurance.Reinsurers WHERE ReinsurerID = @TestReinsurerID;
```

---

### Test Case ID: SP-RNS-002
**Procedure**: Reinsurance.usp_Cession_CalculatePremium
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @GrossPremium DECIMAL(18,2) - Required
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Quota share premium cession | @PolicyID=valid (matching active QS treaty with CessionPercent=0.40), @GrossPremium=10000, @CreatedBy='admin' | Cession record inserted: CededAmount = 10000 * 0.40 = 4000, RetainedAmount = 6000, CessionType='PREMIUM' |
| 2 | Surplus treaty - TIV exceeds retention | @PolicyID=policy with TIV=1000000, treaty RetentionAmount=500000, @GrossPremium=5000 | CededAmount = 5000 * ((1000000 - 500000) / 1000000) = 2500 |
| 3 | Surplus treaty - TIV below retention | @PolicyID=policy with TIV=400000, treaty RetentionAmount=500000, @GrossPremium=5000 | No cession created (RetainedAmount = full GrossPremium) |
| 4 | Surplus treaty - cession limit applied | @PolicyID=policy with TIV=5000000, treaty RetentionAmount=500000, CessionLimit=2000000, @GrossPremium=10000 | SurplusPct = CessionLimit / TIV = 2000000/5000000 = 0.40, CededAmount = 4000 |
| 5 | Excess-of-loss premium cession (flat rate) | @PolicyID=valid, treaty CessionPercent=0.05, @GrossPremium=20000 | CededAmount = 20000 * 0.05 = 1000 |
| 6 | Multiple applicable treaties | @PolicyID matches 2+ active treaties | Multiple Cession records created (one per treaty) |
| 7 | Treaty filtered by PolicyType match | @PolicyID=HO3 policy, treaty CoveredPolicyTypes='HO3,HO5' | Cession calculated for matching treaty |
| 8 | Treaty filtered by StateCode match | @PolicyID=FL policy, treaty CoveredStates='FL,TX' | Cession calculated for matching treaty |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid PolicyID | @PolicyID=99999, @GrossPremium=1000, @CreatedBy='admin' | No cession records created (policy type/state lookup returns NULL, no matching treaties) |
| 2 | No active treaties match | @PolicyID=valid but no matching active treaties | No cession records created (cursor returns no rows) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Zero gross premium | @GrossPremium=0 | CededAmount = 0, no cession inserted (CededAmount > 0 check) |
| 2 | Very large gross premium | @GrossPremium=9999999.99 | Cession calculated without overflow |
| 3 | Policy effective date on treaty boundary | Policy EffectiveDate = Treaty ExpiryDate | Treaty IS matched (BETWEEN is inclusive) |
| 4 | CessionPercent rounds to 2 decimals | @GrossPremium=3333.33, CessionPercent=0.3333 | CededAmount = ROUND(3333.33 * 0.3333, 2) = 1111.00 |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Policy lookup resolves PolicyType | Policy.Policies has PolicyID with PolicyType | Valid @PolicyID | Cursor filter uses PolicyType |
| 2 | Property StateCode resolved | Policy.Properties linked to policy | Valid @PolicyID | Cursor filter uses StateCode |
| 3 | Only ACTIVE treaties considered | Mix of ACTIVE, EXPIRED, DRAFT treaties | Valid @PolicyID | Only ACTIVE treaties with date overlap |
| 4 | AccountingPeriod set to current month | System date = 2024-06-15 | Valid params | Cession.AccountingPeriod = '2024-06' |
| 5 | ErrorLog on failure | Transaction error occurs | Force error | Audit.ErrorLog gets 'PolicyID=N' in AdditionalInfo |

#### Executable SQL Script
```sql
-- Setup: Create treaty and policy for testing
-- [ASSUMPTION] Test requires pre-existing policy and property records
DECLARE @TestPolicyID INT = 1; -- Use existing test policy

-- Test: Calculate premium cession for quota share
EXEC Reinsurance.usp_Cession_CalculatePremium
    @PolicyID = @TestPolicyID,
    @GrossPremium = 10000.00,
    @CreatedBy = 'test_user';
-- Expected: Cession records created for each matching active treaty

SELECT * FROM Reinsurance.Cessions
WHERE PolicyID = @TestPolicyID AND CessionType = 'PREMIUM'
ORDER BY CreatedDate DESC;
-- Expected: CededAmount calculated per treaty type logic

-- Cleanup
DELETE FROM Reinsurance.Cessions WHERE PolicyID = @TestPolicyID AND CreatedDate >= CAST(GETDATE() AS DATE);
```

---

### Test Case ID: SP-RNS-003
**Procedure**: Reinsurance.usp_Cession_CalculateLoss
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @ClaimID INT - Required
- @LossAmount DECIMAL(18,2) - Required
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Quota share loss cession | @ClaimID=valid (policy matches QS treaty, CessionPercent=0.40), @LossAmount=50000, @CreatedBy='admin' | CededLoss = 50000 * 0.40 = 20000, RetainedLoss = 30000, CessionType='LOSS' |
| 2 | Excess-of-loss - loss exceeds attachment point | @ClaimID=valid, treaty AttachmentPt=100000, ExhaustionPt=500000, @LossAmount=250000 | CededLoss = 250000 - 100000 = 150000 |
| 3 | Excess-of-loss - loss exceeds exhaustion point | @ClaimID=valid, treaty AttachmentPt=100000, ExhaustionPt=500000, @LossAmount=1000000 | CededLoss = 500000 - 100000 = 400000 (capped at layer) |
| 4 | Excess-of-loss - loss below attachment point | @ClaimID=valid, treaty AttachmentPt=100000, @LossAmount=80000 | No cession created (RetainedLoss = full amount) |
| 5 | Surplus treaty - uses existing premium cession percentage | @ClaimID=valid, existing premium cession CessionPercent=0.50, @LossAmount=100000 | CededLoss = 100000 * 0.50 = 50000 |
| 6 | Surplus treaty - no prior premium cession | @ClaimID=valid, no existing premium cession for treaty/policy | CededLoss = 0 (ExistingPct defaults to 0) |
| 7 | Multiple treaties apply | @ClaimID matches 2+ active treaties | Multiple loss cession records created |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid ClaimID | @ClaimID=99999, @LossAmount=50000, @CreatedBy='admin' | No cession records (PolicyID lookup returns NULL) |
| 2 | No matching active treaties | @ClaimID=valid but policy has no matching treaties | No cession records created |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Loss amount equals attachment point exactly | @LossAmount = AttachmentPoint | No cession for XOL (loss must EXCEED attachment) |
| 2 | Loss amount = attachment + 1 | @LossAmount = AttachmentPoint + 0.01 | CededLoss = 0.01 |
| 3 | Zero loss amount | @LossAmount=0 | No cession created (CededLoss = 0) |
| 4 | Loss equals exhaustion - attachment | @LossAmount = ExhaustionPt | CededLoss = ExhaustionPt - AttachmentPt (full layer) |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Claim resolves to PolicyID | Claims.Claims has ClaimID with PolicyID | Valid @ClaimID | PolicyID used for treaty lookup |
| 2 | Policy resolves to PolicyType and StateCode | Policy.Policies and Policy.Properties linked | Valid claim | Treaty filter uses PolicyType and StateCode |
| 3 | Surplus uses latest premium cession % | Reinsurance.Cessions has prior PREMIUM record | Valid claim on surplus treaty | Most recent CessionPercent used |
| 4 | ClaimID stored in cession record | Valid data | Valid @ClaimID | Cession.ClaimID = @ClaimID |
| 5 | ErrorLog on failure | Transaction error | Force error | Audit.ErrorLog gets 'ClaimID=N' in AdditionalInfo |

#### Executable SQL Script
```sql
-- Setup
-- [ASSUMPTION] Test requires existing claim linked to policy with matching active treaty
DECLARE @TestClaimID INT = 1; -- Use existing test claim

-- Test: Calculate loss cession
EXEC Reinsurance.usp_Cession_CalculateLoss
    @ClaimID = @TestClaimID,
    @LossAmount = 250000.00,
    @CreatedBy = 'test_user';
-- Expected: Loss cession records created per treaty type logic

SELECT * FROM Reinsurance.Cessions
WHERE ClaimID = @TestClaimID AND CessionType = 'LOSS'
ORDER BY CreatedDate DESC;
-- Expected: CededAmount based on treaty type (QS=proportional, XOL=layer, Surplus=existing %)

-- Cleanup
DELETE FROM Reinsurance.Cessions WHERE ClaimID = @TestClaimID AND CessionType = 'LOSS' AND CreatedDate >= CAST(GETDATE() AS DATE);
```

---

### Test Case ID: SP-RNS-004
**Procedure**: Reinsurance.usp_Bordereaux_Generate
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @TreatyID INT - Required
- @ReportingPeriod VARCHAR(10) - Required (format YYYY-MM)
- @ReportType VARCHAR(20) - Required (PREMIUM, LOSS, OUTSTANDING)
- @GeneratedBy VARCHAR(50) - Required
- @BordereauxID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Generate premium bordereaux | @TreatyID=valid, @ReportingPeriod='2024-06', @ReportType='PREMIUM', @GeneratedBy='admin' | Bordereaux record created with TotalGross/TotalCeded/TotalRetained from PREMIUM cessions, Status='DRAFT' |
| 2 | Generate loss bordereaux | @TreatyID=valid, @ReportingPeriod='2024-06', @ReportType='LOSS', @GeneratedBy='admin' | Bordereaux record aggregates LOSS cessions for period |
| 3 | Generate outstanding bordereaux | @TreatyID=valid, @ReportingPeriod='2024-06', @ReportType='OUTSTANDING', @GeneratedBy='admin' | Bordereaux record aggregates ALL cession types for period |
| 4 | Cessions marked as reported | @TreatyID with PENDING cessions | After generation | Matching cessions updated to Status='REPORTED' |
| 5 | RecordCount matches cession count | @TreatyID with 10 PENDING premium cessions in period | @ReportType='PREMIUM' | Bordereaux.RecordCount = 10 |
| 6 | Period with no cessions | @TreatyID=valid, @ReportingPeriod='2099-01' | Bordereaux created with TotalGross=0, TotalCeded=0, RecordCount=0 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Treaty not found | @TreatyID=99999, @ReportingPeriod='2024-06', @ReportType='PREMIUM', @GeneratedBy='admin' | RAISERROR: 'Treaty not found: 99999' |
| 2 | NULL TreatyID | @TreatyID=NULL | Parameter error |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | ReportingPeriod format variations | @ReportingPeriod='2024-1' (no zero-pad) | [ASSUMPTION] May not match AccountingPeriod format in Cessions |
| 2 | Very large aggregation | Treaty with 10000+ cessions in period | Bordereaux totals calculated correctly |
| 3 | Multiple generations for same period | Generate twice for same treaty/period/type | Two separate Bordereaux records created |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Treaty existence validation | Reinsurance.Treaties has TreatyID | Valid @TreatyID | Passes validation check |
| 2 | Only PENDING cessions updated | Mix of PENDING and REPORTED cessions | Valid params | Only PENDING cessions set to REPORTED |
| 3 | Audit log entry | Audit.AuditLog accessible | Valid params | Row with Action='INSERT', TableName='Reinsurance.Bordereaux' |
| 4 | OUTSTANDING type matches all CessionTypes | Cessions with type PREMIUM and LOSS | @ReportType='OUTSTANDING' | All cessions included in totals |

#### Executable SQL Script
```sql
-- Setup
DECLARE @TestTreatyID INT;
SELECT TOP 1 @TestTreatyID = TreatyID FROM Reinsurance.Treaties WHERE Status = 'ACTIVE';

-- Insert test cessions
INSERT INTO Reinsurance.Cessions (TreatyID, PolicyID, CessionType, GrossAmount, CededAmount, RetainedAmount, CessionPercent, TransactionDate, AccountingPeriod, Status)
VALUES (@TestTreatyID, 1, 'PREMIUM', 10000, 4000, 6000, 0.40, GETDATE(), FORMAT(GETDATE(), 'yyyy-MM'), 'PENDING');

-- Test: Generate bordereaux
DECLARE @NewBordID INT;
EXEC Reinsurance.usp_Bordereaux_Generate
    @TreatyID = @TestTreatyID,
    @ReportingPeriod = '2024-06',
    @ReportType = 'PREMIUM',
    @GeneratedBy = 'test_user',
    @BordereauxID = @NewBordID OUTPUT;
-- Expected: @NewBordID > 0

SELECT * FROM Reinsurance.Bordereaux WHERE BordereauxID = @NewBordID;
-- Expected: Status='DRAFT', TotalGross=10000, TotalCeded=4000

-- Verify cessions updated
SELECT Status FROM Reinsurance.Cessions WHERE TreatyID = @TestTreatyID AND AccountingPeriod = '2024-06';
-- Expected: Status='REPORTED'

-- Cleanup
DELETE FROM Reinsurance.Bordereaux WHERE BordereauxID = @NewBordID;
DELETE FROM Reinsurance.Cessions WHERE TreatyID = @TestTreatyID AND AccountingPeriod = FORMAT(GETDATE(), 'yyyy-MM');
```

---

### Test Case ID: SP-RNS-005
**Procedure**: Reinsurance.usp_Treaty_GetSummary
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @TreatyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get summary for treaty with cessions | @TreatyID=valid (with cessions and bordereaux) | Returns 4 result sets: treaty header, cession summary by type, bordereaux history, recent cessions (top 50) |
| 2 | Treaty header includes reinsurer info | @TreatyID=valid with ReinsurerID | First result set includes ReinsurerName, AMBestRating, SPRating from JOIN |
| 3 | Cession summary grouped by type | @TreatyID with PREMIUM and LOSS cessions | Second result set has rows for each CessionType with TransactionCount, TotalGross, TotalCeded, TotalRetained |
| 4 | Bordereaux history ordered by period DESC | @TreatyID with multiple bordereaux | Third result set ordered by ReportingPeriod DESC |
| 5 | Recent cessions include policy/claim numbers | @TreatyID with linked cessions | Fourth result set includes PolicyNumber and ClaimNumber from LEFT JOINs |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent treaty | @TreatyID=99999 | Empty result sets returned (no error raised) |
| 2 | NULL TreatyID | @TreatyID=NULL | Empty result sets (WHERE clause does not match) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Treaty with no cessions | @TreatyID=valid but no cessions | Second result set empty, fourth result set empty |
| 2 | Treaty with more than 50 recent cessions | @TreatyID with 100+ cessions | Fourth result set limited to TOP 50, ordered by TransactionDate DESC |
| 3 | Treaty with no reinsurer assigned | @TreatyID where ReinsurerID=NULL | First result set has NULL for ReinsurerName, AMBestRating, SPRating (LEFT JOIN) |

#### Executable SQL Script
```sql
-- Test: Get treaty summary
DECLARE @TestTreatyID INT;
SELECT TOP 1 @TestTreatyID = TreatyID FROM Reinsurance.Treaties WHERE Status = 'ACTIVE';

EXEC Reinsurance.usp_Treaty_GetSummary @TreatyID = @TestTreatyID;
-- Expected: 4 result sets returned
-- Result 1: Treaty details with reinsurer info
-- Result 2: Cession summary by CessionType
-- Result 3: Bordereaux records ordered by ReportingPeriod DESC
-- Result 4: Top 50 recent cessions with PolicyNumber and ClaimNumber
```

---

### Test Case ID: SP-RNS-006
**Procedure**: Reinsurance.usp_Treaty_GetActive
**Source**: `database/02-stored-procedures/007-reinsurance-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (defaults to GETDATE() if NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get active treaties as of today | @AsOfDate=NULL | Returns all treaties where Status='ACTIVE' AND today BETWEEN EffectiveDate AND ExpiryDate, ordered by TreatyType, TreatyName |
| 2 | Get active treaties for specific date | @AsOfDate='2024-06-15' | Returns treaties active on that date |
| 3 | Result includes reinsurer info | Treaties with ReinsurerID set | ReinsurerName, AMBestRating from LEFT JOIN |
| 4 | Result includes ceded premium total | Treaties with PREMIUM cessions | TotalCededPremium = SUM of CededAmount for PREMIUM type |
| 5 | Result includes ceded loss total | Treaties with LOSS cessions | TotalCededLoss = SUM of CededAmount for LOSS type |
| 6 | Treaties without reinsurer included | Treaty with ReinsurerID=NULL | Row returned with NULL ReinsurerName (LEFT JOIN) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No active treaties for future date | @AsOfDate='2099-01-01' | Empty result set (no error) |
| 2 | No active treaties for past date | @AsOfDate='1900-01-01' | Empty result set (no error) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | AsOfDate equals treaty EffectiveDate | @AsOfDate = treaty.EffectiveDate | Treaty IS included (BETWEEN is inclusive) |
| 2 | AsOfDate equals treaty ExpiryDate | @AsOfDate = treaty.ExpiryDate | Treaty IS included (BETWEEN is inclusive) |
| 3 | EXPIRED status treaty within date range | Treaty with Status='EXPIRED' but dates valid | NOT included (Status='ACTIVE' filter) |
| 4 | DRAFT status treaty | Treaty with Status='DRAFT' | NOT included |
| 5 | Treaties with zero cessions | New treaty with no cessions | TotalCededPremium=0, TotalCededLoss=0 (ISNULL handles) |

#### Executable SQL Script
```sql
-- Test: Get active treaties
EXEC Reinsurance.usp_Treaty_GetActive @AsOfDate = NULL;
-- Expected: All currently active treaties with reinsurer info and cession totals
-- Ordered by TreatyType, TreatyName

-- Test: Get active treaties for specific date
EXEC Reinsurance.usp_Treaty_GetActive @AsOfDate = '2024-06-15';
-- Expected: Treaties active on 2024-06-15

-- Test: No results for far future
EXEC Reinsurance.usp_Treaty_GetActive @AsOfDate = '2099-01-01';
-- Expected: Empty result set
```
