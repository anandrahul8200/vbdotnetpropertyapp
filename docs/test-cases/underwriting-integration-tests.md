# Underwriting Module - Integration Tests (Future-State)

## Module: UND (Underwriting)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: MT-UND-001
**Integration**: Underwriting -> Policy Module
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Underwriting Component | Policy Component | Data Flow |
|------------------------|-----------------|-----------|
| usp_Premium_Calculate | Policy.Policies (lookup) | PolicyID -> PolicyType, CustomerID, PropertyID, EffectiveDate, CommissionRate |
| usp_Premium_Calculate | Policy.Properties (lookup) | PropertyID -> StateCode, ConstructionType, YearBuilt, ProtectionClass, RoofType, etc. |
| usp_Premium_Calculate | Policy.Customers (lookup) | CustomerID -> CreditScore |
| usp_Premium_Calculate | Policy.Coverages (cursor) | PolicyID -> Selected coverages with LimitAmount, DeductibleAmount |
| usp_Premium_Calculate | Policy.Territories (lookup) | TerritoryID -> RiskMultiplier |
| usp_Rules_Evaluate | Policy.Policies, Properties, Customers | Policy details for rule evaluation |
| usp_Rules_Evaluate | Claims.Claims (lookup) | CustomerID -> Prior claim count |
| usp_Referral_Process | Policy.Policies (update) | DECLINE decision -> PolicyStatus = 'DECLINED' |
| usp_Commission_Calculate | Policy.Policies (lookup) | PolicyID -> PolicyType, AgentID |
| usp_Commission_Calculate | Policy.Agents (fallback) | AgentID -> CommissionRate |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Premium calculation resolves policy data | Underwriting service | Policy service | PolicyType, PropertyID, CustomerID retrieved |
| 2 | Property details fetched for rating | Underwriting service | Policy service | All property attributes available |
| 3 | Customer credit score fetched | Underwriting service | Policy service | CreditScore for credit factor |
| 4 | Coverage cursor iterates selected | Underwriting service | Policy service | Only IsSelected=1 coverages |
| 5 | Territory risk multiplier fetched | Underwriting service | Policy service | RiskMultiplier for territory factor |
| 6 | Claims history queried | Underwriting service | Claims service | Prior claims in 5-year window |
| 7 | Policy declined on referral decline | Underwriting service | Policy service | PolicyStatus updated to DECLINED |
| 8 | Policy not found | Underwriting service | Policy service | Error raised, calculation aborted |
| 9 | Commission agent lookup | Underwriting service | Policy service | AgentID resolved, default rate retrieved |

---

### Test Case ID: MT-UND-002
**Integration**: Underwriting -> Rating Data (Internal)
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Underwriting Component | Rating Data | Data Flow |
|------------------------|-------------|-----------|
| usp_Premium_CalculateCoverage | Underwriting.BaseRates | Rate per $1000 lookup |
| usp_Premium_CalculateCoverage | Underwriting.RatingFactors | 13 factor lookups |
| usp_Premium_CalculateCoverage | Underwriting.DeductibleOptions | Deductible premium factor |
| usp_Premium_CalculateCoverage | Underwriting.RatingWorksheets | Audit trail storage |
| usp_Premium_CalculateTaxesFees | Admin.States | State tax rates |
| usp_Premium_CalculateTaxesFees | Admin.SystemConfig | Policy fee configuration |
| usp_Commission_Calculate | Underwriting.CommissionSchedules | Commission rate lookup |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Base rate effective date filtering | Premium calculation | BaseRates table | Only rates within effective window |
| 2 | Rating factor state preference | Premium calculation | RatingFactors table | State-specific over generic |
| 3 | Deductible option lookup | Premium calculation | DeductibleOptions table | PremiumFactor for deductible combo |
| 4 | Rating worksheet audit | Premium calculation | RatingWorksheets table | Full factor breakdown stored |
| 5 | State tax rate lookup | Tax calculation | Admin.States | TaxRate, SurchargeRate, etc. |
| 6 | Commission schedule tiered lookup | Commission calc | CommissionSchedules | Rate by premium band |
| 7 | Rate data not found (graceful) | Premium calculation | BaseRates table | Default values used (rate=0, factor=1.0) |

---

### Test Case ID: MT-UND-003
**Integration**: Underwriting -> Billing Module
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Underwriting Component | Billing Component | Data Flow |
|------------------------|-------------------|-----------|
| Premium calculation | Invoice generation | Calculated premium -> invoice amount |
| Endorsement calculation | Endorsement billing | PremiumChange -> additional/return premium |
| Commission calculation | Commission processing | CommissionAmount -> agent commission |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Premium calculated triggers invoice | Underwriting | Billing service | Invoice generated with correct premium |
| 2 | Endorsement premium change billed | Underwriting | Billing service | Additional premium invoiced or return premium refunded |
| 3 | Commission amount passed to billing | Underwriting | Billing service | Commission transaction created for agent |
| 4 | Premium recalculation updates billing | Underwriting | Billing service | Existing invoices adjusted |

---

### Test Case ID: MT-UND-004
**Integration**: Underwriting -> Audit/Logging
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Underwriting Component | Audit Component | Data Flow |
|------------------------|-----------------|-----------|
| usp_Premium_Calculate | Audit.ErrorLog | Error details on failure |
| usp_Rules_Evaluate | Audit.ErrorLog | Error details on failure |
| usp_Referral_Process | Audit.AuditLog | Decision audit trail |
| usp_Moratorium_Create | Audit.AuditLog | Moratorium creation audit |
| usp_Moratorium_Lift | Audit.AuditLog | Moratorium lift audit |
| usp_RateTable_Create | Audit.AuditLog | Rate table creation audit |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Premium calculation error logged | Underwriting | Audit service | ErrorLog entry with PolicyID in AdditionalInfo |
| 2 | Referral decision audited | Underwriting | Audit service | AuditLog with old/new status, username, timestamp |
| 3 | Moratorium actions audited | Underwriting | Audit service | Create and lift actions logged |
| 4 | Rate table versioning audited | Underwriting | Audit service | New version creation logged |
| 5 | Transaction rollback on failure | Underwriting | Audit service | Error logged even after rollback |
