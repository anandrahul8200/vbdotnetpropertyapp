# Billing Module - Integration Tests (Future-State)

## Module: BIL (Billing)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: MT-BIL-001
**Integration**: Billing -> Policy Module
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Billing Component | Policy Component | Data Flow |
|-------------------|-----------------|-----------|
| usp_Invoice_Generate | Policy.Policies (lookup) | PolicyID -> CustomerID, PaymentPlan, EffectiveDate |
| usp_Payment_Record | Policy.Policies (lookup) | PolicyID -> CustomerID |
| usp_Refund_Create | Policy.Policies (lookup) | PolicyID -> CustomerID |
| usp_Commission_Create | Policy.Policies (lookup) | PolicyID -> AgentID |
| usp_Commission_GetStatement | Policy.Agents, Policy.Agencies | AgentID -> AgentNumber, AgentName, AgencyName |
| PaymentPlan resolution | Billing.PaymentPlans | PlanCode -> NumInstallments, DownPaymentPct, Fees |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Invoice generation resolves policy data | Billing service | Policy service | CustomerID, PaymentPlan, EffectiveDate retrieved |
| 2 | Payment resolves CustomerID from policy | Billing service | Policy service | CustomerID set on payment record |
| 3 | Commission resolves AgentID from policy | Billing service | Policy service | AgentID set on commission transaction |
| 4 | Policy cancelled triggers refund | Policy service | Billing service | Refund created for return premium |
| 5 | Policy not found | Billing service | Policy service | 404 / operation rejected |
| 6 | Policy renewed triggers new invoices | Policy service | Billing service | Renewal invoices generated |

---

### Test Case ID: MT-BIL-002
**Integration**: Billing -> Agent/Commission Module
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Billing Component | Agent Component | Data Flow |
|-------------------|----------------|-----------|
| usp_Commission_Create | Policy.Agents | AgentID resolved from policy |
| usp_Commission_GetStatement | Policy.Agents, Policy.Agencies | Agent details for statement header |
| Commission calculation | Policy.Agents.CommissionRate | Default rate from agent record |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Commission created for new business | Billing | Agent service | EARNED transaction with correct rate |
| 2 | Commission reversed on cancellation | Billing | Agent service | REVERSAL transaction (negative amount) |
| 3 | Override commission for agency | Billing | Agent service | OVERRIDE transaction for supervising agent |
| 4 | Chargeback on returned payment | Billing | Agent service | CHARGEBACK transaction (negative amount) |
| 5 | Statement aggregates all transaction types | Billing | Agent service | Summary totals correct across types |
| 6 | Agent not found on policy | Billing | Agent service | Error raised, no commission created |

---

### Test Case ID: MT-BIL-003
**Integration**: Billing -> Payment Gateway (External)
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Billing Component | External System | Data Flow |
|-------------------|-----------------|-----------|
| Payment recording (CREDIT_CARD) | Payment gateway | Authorization, capture |
| Payment recording (EFT) | ACH processor | EFT file generation |
| Refund issuance (CREDIT_CARD_REVERSAL) | Payment gateway | Refund/void transaction |
| Refund issuance (EFT) | ACH processor | EFT refund file |
| Payment return (NSF) | Bank notification | NSF notification triggers return |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Credit card payment authorized | Billing | Payment gateway | Authorization code returned, payment recorded |
| 2 | Credit card declined | Billing | Payment gateway | Decline reason returned, payment not recorded |
| 3 | EFT payment submitted | Billing | ACH processor | EFT batch file generated [ASSUMPTION] |
| 4 | EFT payment returned (NSF) | ACH processor | Billing | usp_Payment_Return triggered |
| 5 | Credit card refund | Billing | Payment gateway | Reversal transaction processed |
| 6 | Gateway timeout | Billing | Payment gateway | Retry or pending status [ASSUMPTION] |

---

### Test Case ID: MT-BIL-004
**Integration**: Billing -> Notification Module
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Billing Component | Notification Component | Data Flow |
|-------------------|----------------------|-----------|
| Invoice generated | Email/letter | Invoice notice sent to customer |
| Payment due (reminder) | Email | Payment reminder before due date |
| Payment overdue | Email/letter | Late notice sent |
| Payment received | Email | Payment confirmation/receipt |
| Refund issued | Email/letter | Refund notification |
| Cancellation notice | Letter | Non-payment cancellation warning |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Invoice generated triggers notice | Billing | Notification service | Invoice notice queued for delivery |
| 2 | Payment due in 10 days | Billing (batch) | Notification service | Payment reminder sent [ASSUMPTION] |
| 3 | Invoice overdue (late fee applied) | Billing | Notification service | Late payment notice sent |
| 4 | Payment received | Billing | Notification service | Receipt/confirmation sent |
| 5 | Refund approved and issued | Billing | Notification service | Refund notification sent |
| 6 | Cancellation notice (non-pay) | Billing | Notification service | CancellationNoticeDays before cancellation date |

---

### Test Case ID: MT-BIL-005
**Integration**: Billing -> Reporting Module
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Billing Component | Reporting Component | Data Flow |
|-------------------|-------------------|-----------|
| Billing.Invoices | Billing aging report | Outstanding balances by aging bucket |
| Billing.PremiumPayments | Cash receipts report | Payments received by period |
| Billing.CommissionTransactions | Commission report | Agent commissions by period |
| Billing.Refunds | Refund activity report | Refunds issued by type/period |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Billing aging report | Billing data | Reporting service | Correct aging buckets (Current, 1-30, 31-60, 61-90, 90+) |
| 2 | Cash receipts report | Payment data | Reporting service | Totals by payment method and period |
| 3 | Commission statement | Commission data | Reporting service | Agent-level and agency-level summaries |
| 4 | Refund activity report | Refund data | Reporting service | Refunds by type, method, status |
| 5 | Policy-level billing summary | All billing data | Reporting service | Consolidated view per policy |
