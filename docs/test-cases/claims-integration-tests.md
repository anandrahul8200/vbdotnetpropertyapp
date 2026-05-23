# Claims Module - Integration Tests (Future-State)

## Module: CLM (Claims)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: MT-CLM-001
**Integration**: Claims -> Policy Module
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Claims Component | Policy Component | Data Flow |
|-----------------|-----------------|-----------|
| usp_Claim_Create | Policy.Policies (lookup) | PolicyID -> CustomerID, PropertyID, PolicyStatus, Dates |
| usp_Claim_Create | Policy.Coverages (lookup) | PolicyID -> DeductibleAmount, PolicyLimit |
| usp_Claim_VerifyCoverage | Policy.Coverages, CoveragePerils, Perils | PolicyID + ClaimType -> Applicable coverages |
| Fraud: NEW_POLICY_CLAIM | Policy.Policies.EffectiveDate | Check if claim < 60 days from inception |
| Fraud: PREMIUM_INCREASE | Policy.Endorsements | Check if limits increased < 90 days before loss |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claim created on active policy | Claims service | Policy service | Policy data resolved correctly |
| 2 | Policy cancelled after claim | Claims service | Policy service | Existing claims unaffected |
| 3 | Coverage verification | Claims service | Policy service | Applicable coverages returned |
| 4 | Policy limit update affects claim | Policy service | Claims service | Future payments use new limit |
| 5 | Policy not found | Claims service | Policy service | 404 / claim creation rejected |

---

### Test Case ID: MT-CLM-002
**Integration**: Claims -> Billing Module
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Claims Component | Billing Component | Data Flow |
|-----------------|-------------------|-----------|
| Payment creation | Billing.PaymentRecords | Payment triggers billing entry |
| Payment void | Billing.PaymentRecords | Void reverses billing entry |
| Subrogation recovery | Billing.RecoveryRecords | Recovery posted to billing |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Approved payment posted to billing | Claims | Billing | Billing record created with amount |
| 2 | Voided payment reversal | Claims | Billing | Billing record reversed |
| 3 | Subrogation recovery | Claims | Billing | Recovery credited |
| 4 | Payment method EFT | Claims | Billing/Bank | EFT file generated [ASSUMPTION] |

---

### Test Case ID: MT-CLM-003
**Integration**: Claims -> Reinsurance Module
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Claims Component | Reinsurance Component | Data Flow |
|-----------------|----------------------|-----------|
| Large loss claims | Reinsurance.Cessions | Claims exceeding retention trigger cession |
| Catastrophe claims | Reinsurance.Treaties | CAT claims allocated to catastrophe treaty |
| Reserve changes | Reinsurance.Reserves | Reserve changes propagate to treaty |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claim exceeds retention | Claims (reserve > retention) | Reinsurance | Cession record created |
| 2 | CAT claim linked | Claims (CatastropheID set) | Reinsurance | Allocated to CAT treaty |
| 3 | Reserve increase above threshold | Claims (reserve change) | Reinsurance | Ceded amount updated |
| 4 | Recovery reduces cession | Claims (subrogation) | Reinsurance | Ceded recovery applied |

---

### Test Case ID: MT-CLM-004
**Integration**: Claims -> Notification/Workflow
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Claims Event | Notification/Workflow Action |
|-------------|------------------------------|
| FNOL created | Notify assigned team, create task |
| Status change | Notify stakeholders |
| Payment pending approval | Notify approver |
| SIU referral | Notify SIU team |
| Reserve > threshold | Notify supervisor for approval |
| Activity overdue | Reminder notification |

#### Test Scenarios
| # | Scenario | Trigger | Expected |
|---|----------|---------|----------|
| 1 | FNOL notification | Claim created | Email/task to claims team |
| 2 | Approval request | Payment > $10K | Notification to approver |
| 3 | SIU referral alert | FraudScore >= 70 | Alert to SIU team |
| 4 | Overdue reminder | Activity past DueDate | Reminder to assignee |
| 5 | Catastrophe alert | CAT declared | Bulk notification to adjusters |

---

### Test Case ID: MT-CLM-005
**Integration**: Claims -> Document Management
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Claims Component | Document Component | Data Flow |
|-----------------|-------------------|-----------|
| Claim activities | Policy.usp_Document_Create | Photos, reports attached to claim |
| Fraud evidence | Document storage | SIU evidence documents |
| Payment vouchers | Document generation | Check images, EFT confirmations |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Attach photo to claim | Claims UI | Document service | Document linked to ClaimID |
| 2 | Generate payment voucher | Payment approved | Document service | PDF generated |
| 3 | SIU evidence upload | Fraud review | Document service | Secured document stored |
| 4 | Claim file retrieval | Claims view | Document service | All docs for claim returned |

---

### Test Case ID: MT-CLM-006
**Integration**: Claims -> Audit/Compliance
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Claims Operation | Audit Action |
|-----------------|--------------|
| Any claim modification | Audit.AuditLog entry |
| Any error | Audit.ErrorLog entry |
| Status transition | StatusHistory record |
| Payment operations | Full payment audit trail |
| Fraud scoring | Evaluation results persisted |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claim created | usp_Claim_Create | AuditLog | Action='INSERT', Table='Claims.Claims' |
| 2 | Status changed | usp_Claim_UpdateStatus | AuditLog | FieldName='ClaimStatus', OldValue/NewValue |
| 3 | Payment approved | usp_Claim_ApprovePayment | AuditLog | Action logged with ApprovedBy |
| 4 | SP error occurs | Any SP CATCH block | ErrorLog | Full error details captured |
| 5 | Transaction rollback | Error during transaction | AuditLog | No partial records |

---

### Test Case ID: MT-CLM-007
**Integration**: Claims Data Access Layer -> Database
**Priority**: Critical
**Status**: Current (Integration between VB.NET and SQL Server)

#### Current Integration Tests (ADO.NET)
| # | Scenario | Component | Expected |
|---|----------|-----------|----------|
| 1 | ClaimDataAccess.Create() executes SP | DatabaseHelper -> SQL Server | SP called with correct params, output read |
| 2 | ClaimDataAccess.Search() with output param | DatabaseHelper.ExecuteWithOutput | TotalRecords populated from output |
| 3 | FraudDataAccess.EvaluateClaim() decimal output | DatabaseHelper -> SP | Decimal output read correctly |
| 4 | ClaimDataAccess.GetDetails() returns DataSet | DatabaseHelper.ExecuteDataSet | 7 tables in DataSet |
| 5 | Connection failure handling | DatabaseHelper throws | Exception propagates to form |
| 6 | Timeout on long query | Dashboard with large data | Timeout exception handled gracefully |
