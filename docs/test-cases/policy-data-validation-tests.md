# Policy Module - Data Validation Tests

## Module: POL (Policy)
## Test Type: Data Integrity and Validation Tests
## Schema Source: `database/01-schema/003-policy-tables.sql`
## Tables Covered:
- Policy.Customers
- Policy.Properties
- Policy.Policies
- Policy.Coverages
- Policy.Agents
- Policy.Agencies
- Policy.Territories
- Policy.Regions
- Policy.PolicyVersions
- Policy.Endorsements
- Policy.Documents
- Policy.Notes
- Policy.Perils
- Policy.CoveragePerils

---

### Test Case ID: DV-POL-001
**Table/Entity**: Policy.Customers
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Customers | Policy.Properties | CustomerID | Delete customer with properties | FK violation error |
| 2 | Policy.Customers | Policy.Policies | CustomerID | Delete customer with policies | FK violation error |
| 3 | Policy.Customers | Policy.Notes | CustomerID | Delete customer with notes | FK violation error |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Customers | CustomerNumber | Insert duplicate CustomerNumber | Unique constraint violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | CustomerNumber | VARCHAR | 20 | 21 chars | Truncation or error |
| 2 | CustomerType | CHAR(1) | 1 | 'XX' | Truncation to 'X' |
| 3 | FirstName | VARCHAR | 100 | 101 chars | Truncation or error |
| 4 | Email | VARCHAR | 200 | 201 chars | Truncation or error |
| 5 | StateCode | CHAR(2) | 2 | 'TXX' | Truncation to 'TX' |
| 6 | ZipCode | VARCHAR | 10 | 11 chars | Truncation or error |
| 7 | CreditScore | INT | -- | 2147483648 (overflow) | Arithmetic overflow |
| 8 | AnnualIncome | DECIMAL(18,2) | -- | 9999999999999999.999 | Overflow error |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | CustomerType | 'I' | Insert without CustomerType | Value = 'I' |
| 2 | Country | 'US' | Insert without Country | Value = 'US' |
| 3 | PreferredContact | 'EMAIL' | Insert without PreferredContact | Value = 'EMAIL' |
| 4 | DoNotContact | 0 | Insert without DoNotContact | Value = 0 (False) |
| 5 | RiskTier | 'STANDARD' | Insert without RiskTier | Value = 'STANDARD' |
| 6 | IsActive | 1 | Insert without IsActive | Value = 1 (True) |
| 7 | CreatedDate | GETDATE() | Insert without CreatedDate | Current datetime |

---

### Test Case ID: DV-POL-002
**Table/Entity**: Policy.Properties
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Customers | Policy.Properties | CustomerID | Insert property with invalid CustomerID | FK violation |
| 2 | Policy.Properties | Policy.Policies | PropertyID | Delete property with policies | FK violation |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Properties | PropertyNumber | Insert duplicate PropertyNumber | Unique constraint violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | PropertyType | VARCHAR | 30 | 31 chars | Truncation or error |
| 2 | YearBuilt | INT | -- | 0 or negative | Accepted (no CHECK constraint) |
| 3 | SquareFootage | INT | -- | -1 | Accepted (no CHECK constraint) |
| 4 | FireProtectionClass | INT | -- | 0 or 11 | Accepted (no CHECK) |
| 5 | DistanceToFireStation | DECIMAL(6,2) | -- | 9999.999 | Overflow |
| 6 | MarketValue | DECIMAL(18,2) | -- | Negative value | Accepted |
| 7 | AddressLine1 | VARCHAR | 200 | 201 chars | Truncation or error |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | CustomerID | Insert with NULL | NOT NULL violation |
| 2 | PropertyNumber | Insert with NULL | NOT NULL violation |
| 3 | PropertyType | Insert with NULL | NOT NULL violation |
| 4 | AddressLine1 | Insert with NULL | NOT NULL violation |
| 5 | City | Insert with NULL | NOT NULL violation |
| 6 | StateCode | Insert with NULL | NOT NULL violation |
| 7 | ZipCode | Insert with NULL | NOT NULL violation |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | NumberOfStories | 1 | Insert without value | Value = 1 |
| 2 | NumberOfUnits | 1 | Insert without value | Value = 1 |
| 3 | HasBasement | 0 | Insert without value | Value = 0 |
| 4 | HasPool | 0 | Insert without value | Value = 0 |
| 5 | IsActive | 1 | Insert without value | Value = 1 |

---

### Test Case ID: DV-POL-003
**Table/Entity**: Policy.Policies
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Customers | Policy.Policies | CustomerID | Insert policy with invalid CustomerID | FK violation |
| 2 | Policy.Properties | Policy.Policies | PropertyID | Insert policy with invalid PropertyID | FK violation |
| 3 | Policy.Agents | Policy.Policies | AgentID | Insert policy with invalid AgentID | FK violation |
| 4 | Policy.Policies | Policy.Coverages | PolicyID | Delete policy with coverages | FK violation |
| 5 | Policy.Policies | Policy.PolicyVersions | PolicyID | Delete policy with versions | FK violation |
| 6 | Policy.Policies | Policy.Endorsements | PolicyID | Delete policy with endorsements | FK violation |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Policies | PolicyNumber | Insert duplicate PolicyNumber | Unique constraint violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | PolicyNumber | VARCHAR | 20 | 21 chars | Truncation or error |
| 2 | PolicyType | VARCHAR | 30 | 31 chars | Truncation or error |
| 3 | PolicyStatus | VARCHAR | 20 | 21 chars | Truncation or error |
| 4 | AnnualPremium | DECIMAL(18,2) | -- | Negative value | [ASSUMPTION] Accepted (no CHECK) |
| 5 | CommissionRate | DECIMAL(6,4) | -- | 1.00001 (5 decimals) | Rounded or error |
| 6 | TermMonths | INT | -- | 0 or negative | Accepted (no CHECK) |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | PolicyNumber | NULL | NOT NULL violation |
| 2 | PolicyType | NULL | NOT NULL violation |
| 3 | PolicyStatus | NULL | NOT NULL violation |
| 4 | CustomerID | NULL | NOT NULL violation |
| 5 | PropertyID | NULL | NOT NULL violation |
| 6 | AgentID | NULL | NOT NULL violation |
| 7 | EffectiveDate | NULL | NOT NULL violation |
| 8 | ExpiryDate | NULL | NOT NULL violation |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | PolicyVersion | 1 | Insert without value | Value = 1 |
| 2 | PolicyStatus | 'QUOTE' | Insert without value | Value = 'QUOTE' |
| 3 | TermMonths | 12 | Insert without value | Value = 12 |
| 4 | PaymentPlan | 'ANNUAL' | Insert without value | Value = 'ANNUAL' |
| 5 | BillingMethod | 'DIRECT' | Insert without value | Value = 'DIRECT' |
| 6 | ClaimFreeYears | 0 | Insert without value | Value = 0 |
| 7 | IsRenewal | 0 | Insert without value | Value = 0 |
| 8 | RenewalCount | 0 | Insert without value | Value = 0 |

---

### Test Case ID: DV-POL-004
**Table/Entity**: Policy.Coverages
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Policies | Policy.Coverages | PolicyID | Insert coverage with invalid PolicyID | FK violation |
| 2 | Policy.Coverages | Policy.CoveragePerils | CoverageID | Delete coverage with perils | FK violation |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | CoverageCode | VARCHAR | 20 | 21 chars | Truncation or error |
| 2 | LimitAmount | DECIMAL(18,2) | -- | Negative | [ASSUMPTION] Accepted |
| 3 | DeductibleAmount | DECIMAL(18,2) | -- | Negative | [ASSUMPTION] Accepted |
| 4 | CoinsurancePercent | DECIMAL(6,4) | -- | 1.0001 | Accepted or rounded |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | DeductibleType | 'FLAT' | Insert without value | Value = 'FLAT' |
| 2 | IsRequired | 0 | Insert without value | Value = 0 |
| 3 | IsSelected | 1 | Insert without value | Value = 1 |
| 4 | CoinsurancePercent | 0.80 | Insert without value | Value = 0.80 |
| 5 | WaitingPeriodDays | 0 | Insert without value | Value = 0 |

---

### Test Case ID: DV-POL-005
**Table/Entity**: Policy.Agents
**Database**: PropertyInsuranceDB

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Agents | AgentNumber | Insert duplicate AgentNumber | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | AgentNumber | NULL | NOT NULL violation |
| 2 | AgentType | NULL | NOT NULL violation |
| 3 | FirstName | NULL | NOT NULL violation |
| 4 | LastName | NULL | NOT NULL violation |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | CommissionRate | 0.10 | Insert without value | Value = 0.10 |
| 2 | OverrideRate | 0 | Insert without value | Value = 0 |
| 3 | HierarchyLevel | 1 | Insert without value | Value = 1 |
| 4 | IsActive | 1 | Insert without value | Value = 1 |

---

### Test Case ID: DV-POL-006
**Table/Entity**: Policy.Agencies
**Database**: PropertyInsuranceDB

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Agencies | AgencyCode | Insert duplicate AgencyCode | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | AgencyCode | NULL | NOT NULL violation |
| 2 | AgencyName | NULL | NOT NULL violation |

---

### Test Case ID: DV-POL-007
**Table/Entity**: Policy.PolicyVersions
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Policies | Policy.PolicyVersions | PolicyID | Insert version with invalid PolicyID | FK violation |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.PolicyVersions | PolicyID, VersionNumber | Insert duplicate combo | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | PolicyID | NULL | NOT NULL violation |
| 2 | VersionNumber | NULL | NOT NULL violation |
| 3 | VersionType | NULL | NOT NULL violation |
| 4 | EffectiveDate | NULL | NOT NULL violation |

---

### Test Case ID: DV-POL-008
**Table/Entity**: Policy.Endorsements
**Database**: PropertyInsuranceDB

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | Policy.Policies | Policy.Endorsements | PolicyID | Insert with invalid PolicyID | FK violation |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | Policy.Endorsements | PolicyID, EndorsementNumber | Insert duplicate combo | Unique constraint violation |

#### NOT NULL Constraint Tests
| # | Column | Test Action | Expected |
|---|--------|-------------|----------|
| 1 | PolicyID | NULL | NOT NULL violation |
| 2 | EndorsementNumber | NULL | NOT NULL violation |
| 3 | EndorsementType | NULL | NOT NULL violation |
| 4 | EffectiveDate | NULL | NOT NULL violation |

#### Default Value Tests
| # | Column | Expected Default | Test Action | Expected |
|---|--------|------------------|-------------|----------|
| 1 | EndorsementStatus | 'PENDING' | Insert without value | Value = 'PENDING' |
| 2 | PremiumChange | 0 | Insert without value | Value = 0 |
| 3 | ReturnPremium | 0 | Insert without value | Value = 0 |
| 4 | AdditionalPremium | 0 | Insert without value | Value = 0 |
