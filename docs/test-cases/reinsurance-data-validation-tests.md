# Reinsurance Module - Data Validation Tests

## Module: RNS (Reinsurance)
## Source Files:
- `database/01-schema/006-billing-reinsurance-tables.sql`

## Tables Covered:
1. Reinsurance.Treaties
2. Reinsurance.Reinsurers
3. Reinsurance.Cessions
4. Reinsurance.Bordereaux

---

### Test Case ID: DV-RNS-001
**Table/Entity**: Reinsurance.Treaties
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Reinsurance.Reinsurers | Reinsurance.Treaties | ReinsurerID | Insert treaty with invalid ReinsurerID | FK violation error |
| 2 | Reinsurance.Reinsurers | Reinsurance.Treaties | ReinsurerID | Delete reinsurer with existing treaties | FK violation error |
| 3 | Reinsurance.Treaties | Reinsurance.Cessions | TreatyID | Delete treaty with existing cessions | FK violation error |
| 4 | Reinsurance.Treaties | Reinsurance.Bordereaux | TreatyID | Delete treaty with existing bordereaux | FK violation error |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Reinsurance.Treaties | TreatyNumber | Insert duplicate TreatyNumber | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | TreatyNumber | INSERT with TreatyNumber=NULL | NOT NULL violation |
| 2 | TreatyName | INSERT with TreatyName=NULL | NOT NULL violation |
| 3 | TreatyType | INSERT with TreatyType=NULL | NOT NULL violation |
| 4 | EffectiveDate | INSERT with EffectiveDate=NULL | NOT NULL violation |
| 5 | ExpiryDate | INSERT with ExpiryDate=NULL | NOT NULL violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | TreatyNumber | VARCHAR | 20 | 21-character string | Truncation or error |
| 2 | TreatyName | VARCHAR | 200 | 201-character string | Truncation or error |
| 3 | TreatyType | VARCHAR | 30 | 31-character string | Truncation or error |
| 4 | RetentionAmount | DECIMAL(18,2) | -- | 9999999999999999.999 | Overflow error |
| 5 | CessionPercent | DECIMAL(6,4) | -- | 99.99999 | Overflow error (max 9.9999) |
| 6 | AttachmentPoint | DECIMAL(18,2) | -- | -1.00 | [ASSUMPTION] Accepted (no CHECK constraint in schema) |
| 7 | CoveredPerils | VARCHAR | 500 | 501-character string | Truncation or error |
| 8 | CoveredStates | VARCHAR | 200 | 201-character string | Truncation or error |
| 9 | CoveredPolicyTypes | VARCHAR | 200 | 201-character string | Truncation or error |

#### Default Value Tests
| # | Column | Expected Default | Test Action |
|---|--------|-----------------|-------------|
| 1 | ReinstatementCount | 1 | INSERT without specifying ReinstatementCount |
| 2 | Status | 'ACTIVE' | INSERT without specifying Status |
| 3 | CreatedDate | GETDATE() | INSERT without specifying CreatedDate |
| 4 | ModifiedDate | GETDATE() | INSERT without specifying ModifiedDate |

---

### Test Case ID: DV-RNS-002
**Table/Entity**: Reinsurance.Reinsurers
**Database**: PropertyInsuranceDB

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Reinsurance.Reinsurers | ReinsurerCode | Insert duplicate ReinsurerCode | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | ReinsurerCode | INSERT with ReinsurerCode=NULL | NOT NULL violation |
| 2 | ReinsurerName | INSERT with ReinsurerName=NULL | NOT NULL violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | ReinsurerCode | VARCHAR | 20 | 21-character string | Truncation or error |
| 2 | ReinsurerName | VARCHAR | 200 | 201-character string | Truncation or error |
| 3 | AMBestRating | VARCHAR | 10 | 11-character string | Truncation or error |
| 4 | SPRating | VARCHAR | 10 | 11-character string | Truncation or error |
| 5 | Phone | VARCHAR | 20 | 21-character string | Truncation or error |
| 6 | Email | VARCHAR | 200 | 201-character string | Truncation or error |
| 7 | StateCode | CHAR | 2 | 3-character string | Truncation or error |
| 8 | Country | VARCHAR | 50 | 51-character string | Truncation or error |

#### Default Value Tests
| # | Column | Expected Default | Test Action |
|---|--------|-----------------|-------------|
| 1 | IsActive | 1 (True) | INSERT without specifying IsActive |
| 2 | CreatedDate | GETDATE() | INSERT without specifying CreatedDate |

#### Business Rule Enforcement Tests
| # | Rule | Table | Test Action | Expected |
|---|------|-------|-------------|----------|
| 1 | ReinsurerCode must be unique | Reinsurance.Reinsurers | Insert duplicate code | Unique violation |
| 2 | [ASSUMPTION] IsActive flag soft-deletes | Reinsurance.Reinsurers | Set IsActive=0 | Record not returned in active queries |

---

### Test Case ID: DV-RNS-003
**Table/Entity**: Reinsurance.Cessions
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Reinsurance.Treaties | Reinsurance.Cessions | TreatyID | Insert cession with invalid TreatyID | FK violation error |
| 2 | Policy.Policies | Reinsurance.Cessions | PolicyID | Insert cession with invalid PolicyID | FK violation error |
| 3 | Reinsurance.Treaties | Reinsurance.Cessions | TreatyID | Delete treaty with cessions | FK violation error |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | TreatyID | INSERT with TreatyID=NULL | NOT NULL violation |
| 2 | CessionType | INSERT with CessionType=NULL | NOT NULL violation |
| 3 | GrossAmount | INSERT with GrossAmount=NULL | NOT NULL violation |
| 4 | CededAmount | INSERT with CededAmount=NULL | NOT NULL violation |
| 5 | RetainedAmount | INSERT with RetainedAmount=NULL | NOT NULL violation |
| 6 | TransactionDate | INSERT with TransactionDate=NULL | NOT NULL violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | CessionType | VARCHAR | 20 | 21-character string | Truncation or error |
| 2 | GrossAmount | DECIMAL(18,2) | -- | 9999999999999999.999 | Overflow error |
| 3 | CededAmount | DECIMAL(18,2) | -- | Negative value | [ASSUMPTION] Accepted (no CHECK constraint) |
| 4 | CessionPercent | DECIMAL(6,4) | -- | 99.99999 | Overflow error |
| 5 | AccountingPeriod | VARCHAR | 10 | 11-character string | Truncation or error |
| 6 | Status | VARCHAR | 20 | 21-character string | Truncation or error |

#### Default Value Tests
| # | Column | Expected Default | Test Action |
|---|--------|-----------------|-------------|
| 1 | TransactionDate | GETDATE() (cast to DATE) | INSERT without specifying TransactionDate |
| 2 | Status | 'PENDING' | INSERT without specifying Status |
| 3 | CreatedDate | GETDATE() | INSERT without specifying CreatedDate |

#### Business Rule Enforcement Tests
| # | Rule | Table | Test Action | Expected |
|---|------|-------|-------------|----------|
| 1 | CessionType must be valid | Reinsurance.Cessions | Insert CessionType='INVALID' | [ASSUMPTION] No CHECK constraint, accepted |
| 2 | RetainedAmount = GrossAmount - CededAmount | Reinsurance.Cessions | Insert with mismatched amounts | [ASSUMPTION] No CHECK constraint, accepted at DB level (enforced by SP) |
| 3 | AccountingPeriod format YYYY-MM | Reinsurance.Cessions | Insert '2024-13' | [ASSUMPTION] No format validation at DB level |

---

### Test Case ID: DV-RNS-004
**Table/Entity**: Reinsurance.Bordereaux
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Reinsurance.Treaties | Reinsurance.Bordereaux | TreatyID | Insert bordereaux with invalid TreatyID | FK violation error |
| 2 | Reinsurance.Treaties | Reinsurance.Bordereaux | TreatyID | Delete treaty with bordereaux | FK violation error |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | TreatyID | INSERT with TreatyID=NULL | NOT NULL violation |
| 2 | ReportingPeriod | INSERT with ReportingPeriod=NULL | NOT NULL violation |
| 3 | ReportType | INSERT with ReportType=NULL | NOT NULL violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | ReportingPeriod | VARCHAR | 10 | 11-character string | Truncation or error |
| 2 | ReportType | VARCHAR | 20 | 21-character string | Truncation or error |
| 3 | TotalGross | DECIMAL(18,2) | -- | 9999999999999999.999 | Overflow error |
| 4 | TotalCeded | DECIMAL(18,2) | -- | Negative value | [ASSUMPTION] Accepted |
| 5 | RecordCount | INT | -- | 2147483648 | Overflow error |
| 6 | Status | VARCHAR | 20 | 21-character string | Truncation or error |

#### Default Value Tests
| # | Column | Expected Default | Test Action |
|---|--------|-----------------|-------------|
| 1 | TotalGross | 0 | INSERT without specifying TotalGross |
| 2 | TotalCeded | 0 | INSERT without specifying TotalCeded |
| 3 | TotalRetained | 0 | INSERT without specifying TotalRetained |
| 4 | RecordCount | 0 | INSERT without specifying RecordCount |
| 5 | GeneratedDate | GETDATE() | INSERT without specifying GeneratedDate |
| 6 | Status | 'DRAFT' | INSERT without specifying Status |
| 7 | CreatedDate | GETDATE() | INSERT without specifying CreatedDate |

#### Business Rule Enforcement Tests
| # | Rule | Table | Test Action | Expected |
|---|------|-------|-------------|----------|
| 1 | ReportType must be valid (PREMIUM, LOSS, OUTSTANDING) | Reinsurance.Bordereaux | Insert ReportType='INVALID' | [ASSUMPTION] No CHECK constraint at DB level |
| 2 | Status lifecycle (DRAFT->SUBMITTED->ACCEPTED/DISPUTED) | Reinsurance.Bordereaux | Update Status out of order | [ASSUMPTION] No CHECK constraint, enforced by application |
| 3 | TotalRetained = TotalGross - TotalCeded | Reinsurance.Bordereaux | Insert mismatched values | [ASSUMPTION] No CHECK constraint, enforced by SP |

#### Cascade Behavior Tests
| # | Parent Table | Child Table | On Delete | On Update | Test | Expected |
|---|-------------|------------|-----------|-----------|------|----------|
| 1 | Reinsurance.Treaties | Reinsurance.Cessions | NO ACTION | NO ACTION | Delete treaty | Error (FK prevents) |
| 2 | Reinsurance.Treaties | Reinsurance.Bordereaux | NO ACTION | NO ACTION | Delete treaty | Error (FK prevents) |
| 3 | Policy.Policies | Reinsurance.Cessions | NO ACTION | NO ACTION | Delete policy | Error if cessions exist |
