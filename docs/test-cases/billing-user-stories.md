# Billing Module - User Stories

## Module: BIL (Billing)
## Test Type: User Stories and Acceptance Criteria

---

### User Story ID: US-BIL-001
**Title**: Billing Inquiry
**Priority**: Critical

#### Story
As a billing representative, I want to view the complete billing history for a policy so that I can answer customer questions about invoices, payments, and outstanding balances.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A policy exists with billing activity | I enter the policy number and click Lookup | Policy billing summary displayed (TotalBilled, TotalPaid, Outstanding) |
| 2 | Policy has invoices | I view the Invoices tab | All invoices displayed ordered by InvoiceDate |
| 3 | Policy has payments | I view the Payments tab | All payments displayed ordered by PaymentDate (most recent first) |
| 4 | Policy has refunds | I view the Refunds tab | All refunds displayed ordered by CreatedDate (most recent first) |
| 5 | Policy found | Billing data loaded | Record Payment and Create Refund buttons enabled |
| 6 | Policy number is blank | I click Lookup | No action taken |

#### Form
- **Screen**: frmBillingInquiry
- **Data Access**: BillingDataAccess.GetBillingByPolicy()
- **Stored Procedure**: Billing.usp_Billing_GetByPolicy

---

### User Story ID: US-BIL-002
**Title**: Payment Entry
**Priority**: Critical

#### Story
As a billing representative, I want to record a premium payment received from a customer so that their account balance is updated and the invoice is marked as paid or partially paid.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A policy has an open invoice | I enter amount, select payment method, and click Record | Payment recorded, invoice balance updated |
| 2 | Payment amount >= invoice balance | Payment is recorded | Invoice status changes to PAID, BalanceDue = 0 |
| 3 | Payment amount < invoice balance | Payment is recorded | Invoice status changes to PARTIAL, BalanceDue reduced |
| 4 | Payment amount > invoice balance | Payment is recorded | Overpayment applied to next open invoice |
| 5 | No invoice specified | Payment is recorded | Oldest open invoice auto-selected |
| 6 | Payment method is CHECK | I select CHECK | Check number and bank fields visible |
| 7 | Payment method is CREDIT_CARD | I select CREDIT_CARD | Card last 4 field visible |
| 8 | Amount is zero or negative | I click Record | Validation error: 'Enter a valid positive amount.' |
| 9 | Amount is non-numeric | I click Record | Validation error: 'Enter a valid positive amount.' |

#### Form
- **Screen**: frmPaymentEntry
- **Data Access**: BillingDataAccess.RecordPayment()
- **Stored Procedure**: Billing.usp_Payment_Record

---

### User Story ID: US-BIL-003
**Title**: Refund Processing
**Priority**: Critical

#### Story
As a billing supervisor, I want to create, approve, and issue refunds so that customers receive return premiums for cancellations, endorsements, and overpayments.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A policy is eligible for refund | I lookup the policy, enter details, and click Create Refund | Refund created with RefundNumber (RFD+7 digits) |
| 2 | Refund amount <= $5,000 (approval threshold) | Refund is created | Status auto-set to APPROVED |
| 3 | Refund amount > $5,000 | Refund is created | Status set to PENDING (requires manual approval) |
| 4 | Refund is PENDING | I select it and click Approve | Status changes to APPROVED, ApprovedBy and ApprovedDate set |
| 5 | Refund is APPROVED | I click Issue | Refund issued, check number generated |
| 6 | Refund needs to be cancelled | I click Void and confirm | Refund voided |
| 7 | No policy looked up | I click Create Refund | Validation: 'Please lookup a policy first.' |
| 8 | Invalid amount | I click Create Refund | Validation: 'Enter a valid positive amount.' |
| 9 | Calculation method is PRO_RATA (default) | Refund created | CalculationMethod = 'PRO_RATA' |

#### Form
- **Screen**: frmRefundProcessing
- **Data Access**: BillingDataAccess.CreateRefund()
- **Stored Procedure**: Billing.usp_Refund_Create, Billing.usp_Refund_Approve, Billing.usp_Refund_GetPending

---

### User Story ID: US-BIL-004
**Title**: Commission Statement Viewing
**Priority**: High

#### Story
As an agent or agency manager, I want to view my commission statement for a specified period so that I can see earned commissions, reversals, and net payable amount.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Agent has commission transactions | I select the agent, set date range, and click Generate | Statement displayed with summary and detail |
| 2 | Statement generated | I view the summary | TotalEarned, TotalReversals, TotalOverrides, TotalBonus, NetCommission shown |
| 3 | Statement generated | I view the detail grid | Individual transactions with PolicyNumber, CustomerName, Type, Amount |
| 4 | No date range specified | I click Generate | Defaults to last month (PeriodFrom = first of prior month, PeriodTo = today) |
| 5 | Agent has no transactions in period | I click Generate | Empty summary (zero values) and empty detail grid |
| 6 | Statement generated | I click Export | Export functionality triggered |

#### Form
- **Screen**: frmCommissionStatement
- **Data Access**: BillingDataAccess.GetCommissionStatement()
- **Stored Procedure**: Billing.usp_Commission_GetStatement

---

### User Story ID: US-BIL-005
**Title**: Payment Plan Setup
**Priority**: High

#### Story
As a billing administrator, I want to configure payment plans with installment counts, down payment percentages, and fee schedules so that customers can choose flexible payment options.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | System has configured plans | I open the Payment Plan Setup form | All plans displayed in grid |
| 2 | I want to add a new plan | I click New, fill in details, and click Save | New plan created with PlanCode, Name, installments, etc. |
| 3 | I want to modify an existing plan | I select a plan and click Edit | Fields populated from selected plan, PlanCode disabled |
| 4 | I want to deactivate a plan | I select a plan and click Deactivate | Confirmation dialog, then plan deactivated (IsActive=0) |
| 5 | I want to preview installment schedule | I enter a premium and click Generate Preview | Schedule grid shows installment breakdown with dates, amounts, and fees |
| 6 | Plan code or name missing | I click Save | Validation: 'Plan code and name are required.' |
| 7 | Invalid installments value | I click Save | Validation: 'Valid number of installments required.' |
| 8 | Preview with 25% down, 4 installments, $5 fee | Premium = $1500 | Down payment = $375 + $5 fee, 3 installments = $375 + $5 each |

#### Form
- **Screen**: frmPaymentPlanSetup
- **Data Access**: DatabaseHelper.ExecuteStoredProcedure("Billing.usp_PaymentPlan_List")
- **Stored Procedure**: Billing.usp_PaymentPlan_List

---

### User Story ID: US-BIL-006
**Title**: Invoice Generation for New Business
**Priority**: Critical

#### Story
As the system, I want to automatically generate invoices when a policy is bound so that the customer receives billing for their premium based on their selected payment plan.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Policy is bound with ANNUAL payment plan | Invoice generation triggered | Single invoice created for full premium + taxes + fees |
| 2 | Policy is bound with multi-pay plan | Invoice generation triggered | Down payment invoice + installment invoices created |
| 3 | Multi-pay plan (4 installments, 25% down) | Invoice generation triggered | 1st invoice = 25% of total + fees, 3 installments for remainder |
| 4 | Installment dates | Invoices generated | Each installment due 1 month apart starting from effective date |
| 5 | Grace period | Invoices generated | Each DueDate = InvoiceDate + GracePeriodDays |
| 6 | Last installment | Multi-pay generated | Last installment adjusted for rounding (no penny discrepancy) |
| 7 | Invalid policy | Generation attempted | Error: 'Policy not found: {ID}' |

#### Form
- **Screen**: (triggered programmatically, not direct UI)
- **Data Access**: BillingDataAccess.GenerateInvoice()
- **Stored Procedure**: Billing.usp_Invoice_Generate

---

### User Story ID: US-BIL-007
**Title**: Late Fee Management
**Priority**: High

#### Story
As the system (batch process), I want to apply late fees to overdue invoices so that customers who miss their due dates are charged appropriately per their payment plan terms.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Open invoices exist past their due date | Batch process runs | Late fees applied to eligible invoices |
| 2 | Invoice has LateFeeApplied = 0 and BalanceDue > 0 | Late fee batch runs | LateFeeApplied = 1, LateFeeAmount set, TotalAmount and BalanceDue increased |
| 3 | Invoice linked to a payment plan | Late fee applied | LateFeeAmount from PaymentPlan's LateFeeAmount |
| 4 | Invoice not linked to payment plan | Late fee applied | LateFeeAmount from SystemConfig 'DEFAULT_LATE_FEE' (default $15.00) |
| 5 | Invoice already has late fee (LateFeeApplied=1) | Batch runs again | Not processed again (idempotent) |
| 6 | Invoice has zero balance | Batch runs | Not processed (BalanceDue > 0 required) |
| 7 | Invoice status was OPEN | Late fee applied | Status changed to OVERDUE |

#### Form
- **Screen**: (batch process, no direct UI)
- **Data Access**: (direct SP call from batch job)
- **Stored Procedure**: Billing.usp_Invoice_ApplyLateFees

---

### User Story ID: US-BIL-008
**Title**: Returned Payment Processing
**Priority**: High

#### Story
As a billing representative, I want to process returned payments (NSF/bounced checks) so that the payment is reversed, the invoice balance is restored, and an NSF fee is applied.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Payment is in APPLIED status | I process the return | Payment status = RETURNED, ReturnedDate set, ReturnReason stored |
| 2 | Payment was applied to an invoice | Return is processed | Invoice PaidAmount decreased, BalanceDue increased by Amount + NSFFee |
| 3 | NSF fee default | No custom fee specified | NSFFee = $25.00 |
| 4 | Custom NSF fee | Fee specified | NSFFee set to specified amount |
| 5 | Invoice status after return | Return processed | Invoice Status = OVERDUE |
| 6 | Payment not in APPLIED status | Return attempted | Error: 'Payment is not in APPLIED status: {status}' |
| 7 | Payment not found | Return attempted | Error: 'Payment not found: {ID}' |

#### Form
- **Screen**: (no dedicated UI - handled via billing inquiry) [ASSUMPTION]
- **Data Access**: (direct SP call)
- **Stored Procedure**: Billing.usp_Payment_Return

---

### User Story ID: US-BIL-009
**Title**: Commission Creation
**Priority**: High

#### Story
As the system, I want to create commission transactions when premiums are earned so that agents are compensated for the business they write.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Policy has an assigned agent | Premium is earned | EARNED commission transaction created |
| 2 | Commission rate is provided | Transaction created | CommissionAmount = ROUND(PremiumAmount * CommissionRate, 2) |
| 3 | Transaction type is REVERSAL | Transaction created | CommissionAmount is negative |
| 4 | Transaction type is CHARGEBACK | Transaction created | CommissionAmount is negative |
| 5 | Transaction type is EARNED | Transaction created | CommissionAmount is positive |
| 6 | Transaction type is OVERRIDE | Transaction created | CommissionAmount is positive |
| 7 | Transaction type is BONUS | Transaction created | CommissionAmount is positive |
| 8 | Policy has no agent | Commission attempted | Error: 'Policy not found: {ID}' |

#### Form
- **Screen**: (triggered programmatically)
- **Data Access**: (direct SP call)
- **Stored Procedure**: Billing.usp_Commission_Create
