# Policy Module - Integration Tests (Future-State)

## Module: POL (Policy)
## Test Type: Module / Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently have a service layer or dependency injection.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: MT-POL-001
**Integration**: PolicyService -> CustomerRepository -> Database
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Create customer flow | PolicyService | CustomerRepository | CustomerDTO | Customer persisted, ID returned |
| 2 | Customer validation | PolicyService | CustomerValidator | CustomerDTO | Validation passes/fails |
| 3 | Audit logging | CustomerRepository | AuditService | AuditEntry | Audit record created |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Customer create succeeds | Transaction commits, customer + audit persisted |
| 2 | Validation fails | No database write, validation errors returned |
| 3 | Database error during create | Transaction rolled back, error returned to caller |

---

### Test Case ID: MT-POL-002
**Integration**: PolicyService -> PropertyRepository -> CustomerRepository
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Create property flow | PolicyService | PropertyRepository | PropertyDTO | Property persisted |
| 2 | Customer ownership check | PropertyRepository | CustomerRepository | CustomerID | Active customer verified |
| 3 | Territory lookup | PropertyRepository | TerritoryService | StateCode, ZipCode | TerritoryID resolved |
| 4 | Fire protection calculation | PropertyRepository | RatingEngine | Distance values | FireProtectionClass computed |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All steps succeed | Property + audit created |
| 2 | Customer validation fails | No property created, error returned |
| 3 | Territory not found | Property created with NULL TerritoryID |

---

### Test Case ID: MT-POL-003
**Integration**: PolicyService -> QuoteEngine -> UnderwritingService -> RatingEngine
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Quote creation flow | PolicyService | QuoteEngine | PolicyDTO | Quote created in QUOTE status |
| 2 | Moratorium check | QuoteEngine | UnderwritingService | State, PolicyType | Pass/Fail |
| 3 | Agent validation | QuoteEngine | AgentRepository | AgentID | Active agent verified |
| 4 | Premium calculation | QuoteEngine | RatingEngine | Coverages, Property, Risk factors | Premium amounts |
| 5 | Commission calculation | QuoteEngine | CommissionService | AgentID, Premium | Commission amount |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Full quote creation succeeds | Policy + version + audit all committed |
| 2 | Moratorium blocks quote | No policy created, moratorium error returned |
| 3 | Rating engine fails | Transaction rolled back, error logged |
| 4 | Agent not found | Transaction rolled back, validation error |

#### Data Flow Tests
| # | Flow | Input | Intermediate State | Final State |
|---|------|-------|-------------------|-------------|
| 1 | New quote | Customer+Property+Agent | PolicyStatus=QUOTE | Policy persisted with number |
| 2 | Quote to bind | PolicyID | Validate QUOTE status | PolicyStatus=ACTIVE |
| 3 | Active to cancel | PolicyID+Reason+Date | Calculate return premium | PolicyStatus=CANCELLED |
| 4 | Cancel to reinstate | PolicyID+Date | Validate CANCELLED | PolicyStatus=ACTIVE |

---

### Test Case ID: MT-POL-004
**Integration**: PolicyService -> CoverageService -> RatingEngine
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Save coverage | PolicyService | CoverageRepository | CoverageDTO | Upsert coverage |
| 2 | Premium recalculation | CoverageService | RatingEngine | All coverages | Total premium updated |
| 3 | Coverage validation | CoverageService | CoverageValidator | Limits, deductibles | Validation result |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Save multiple coverages | All saved atomically |
| 2 | One coverage invalid | All changes rolled back |
| 3 | Premium update after save | Policy premium totals recalculated |

---

### Test Case ID: MT-POL-005
**Integration**: PolicyService -> CancellationEngine -> BillingService -> RefundService
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Cancel policy | PolicyService | CancellationEngine | PolicyID, Reason, Date | Return premium calculated |
| 2 | Pro-rata calculation | CancellationEngine | PremiumCalculator | Dates, Premium | Return amount |
| 3 | Refund creation | CancellationEngine | RefundService | Amount, PolicyID | Refund record created |
| 4 | Billing adjustment | CancellationEngine | BillingService | PolicyID | Outstanding invoices voided |
| 5 | Notification | CancellationEngine | NotificationService | CustomerID | Cancel notice queued |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Full cancellation | Status + audit + refund all committed |
| 2 | Refund creation fails | Status reverted, no partial cancellation |
| 3 | Billing adjustment fails | Transaction rolled back |

---

### Test Case ID: MT-POL-006
**Integration**: PolicyService -> RenewalEngine -> RatingEngine -> BillingService
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Renewal processing | PolicyService | RenewalEngine | Expiring PolicyID | New policy created |
| 2 | Re-rating | RenewalEngine | RatingEngine | Updated risk factors | New premium calculated |
| 3 | Billing setup | RenewalEngine | BillingService | New PolicyID | Invoices generated |
| 4 | Old policy expiration | RenewalEngine | PolicyRepository | Original PolicyID | Status = EXPIRED |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Successful renewal | New policy active, old policy expired |
| 2 | Rating fails | No new policy, old policy unchanged |
| 3 | Non-renewal decision | Old policy marked NON_RENEWED, no new policy |

---

### Test Case ID: MT-POL-007
**Integration**: EndorsementService -> PolicyService -> CoverageService -> BillingService
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Process endorsement | EndorsementService | PolicyService | Changes | Policy version incremented |
| 2 | Coverage modification | EndorsementService | CoverageService | New limits | Coverages updated |
| 3 | Pro-rata premium | EndorsementService | PremiumCalculator | Date, changes | Additional/return premium |
| 4 | Billing adjustment | EndorsementService | BillingService | Premium change | Invoice created/credit issued |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All steps succeed | Endorsement + coverage + billing all committed |
| 2 | Premium calculation error | All changes rolled back |

---

### Test Case ID: MT-POL-008
**Integration**: DocumentService -> StorageService -> PolicyRepository
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Upload document | DocumentService | StorageService | File bytes | File stored, path returned |
| 2 | Link to entity | DocumentService | PolicyRepository | EntityType, EntityID, Path | Document record created |
| 3 | Delete document | DocumentService | StorageService | FilePath | File removed |
| 4 | Search documents | DocumentService | DocumentRepository | Filters | Matching documents |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Upload succeeds | File stored + DB record created |
| 2 | Storage fails | No DB record, error returned |
| 3 | DB record fails after storage | File cleaned up (compensating transaction) |
