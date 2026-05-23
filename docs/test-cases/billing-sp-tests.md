# Billing Module - Stored Procedure Tests

## Module: BIL (Billing)
## Source Files:
- `database/02-stored-procedures/006-billing-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (11 total):
1. Billing.usp_Invoice_Generate
2. Billing.usp_Payment_Record
3. Billing.usp_Payment_Return
4. Billing.usp_Refund_Create
5. Billing.usp_Refund_Approve
6. Billing.usp_Invoice_ApplyLateFees
7. Billing.usp_Commission_Create
8. Billing.usp_Commission_GetStatement
9. Billing.usp_Billing_GetByPolicy
10. Billing.usp_Refund_GetPending
11. Billing.usp_PaymentPlan_List

---

### Test Case ID: SP-BIL-001
**Procedure**: Billing.usp_Invoice_Generate
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @InvoiceType VARCHAR(20) - Required (NEW_BUSINESS, RENEWAL, ENDORSEMENT)
- @PremiumAmount DECIMAL(18,2) - Required
- @TaxAmount DECIMAL(18,2) - Optional (default 0)
- @FeeAmount DECIMAL(18,2) - Optional (default 0)
- @SurchargeAmount DECIMAL(18,2) - Optional (default 0)
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Generate single invoice (ANNUAL plan) | @PolicyID=valid, @InvoiceType='NEW_BUSINESS', @PremiumAmount=1200, @CreatedBy='testuser' | 1 invoice created, InvoiceNumber=INV+7 digits, Status='OPEN', TotalAmount=1200 |
| 2 | Generate multi-installment invoices (4-pay) | @PolicyID=policy with 4-pay plan, @PremiumAmount=1200 | 4 invoices created (1 down payment + 3 installments) |
| 3 | Generate renewal invoice | @InvoiceType='RENEWAL', @PremiumAmount=1500 | Invoice with InvoiceType='RENEWAL' |
| 4 | Generate endorsement invoice | @InvoiceType='ENDORSEMENT', @PremiumAmount=200 | Invoice with InvoiceType='ENDORSEMENT' |
| 5 | Invoice with all surcharges | @PremiumAmount=1000, @TaxAmount=50, @FeeAmount=25, @SurchargeAmount=10 | TotalAmount=1085 |
| 6 | Down payment calculated correctly | @PolicyID=4-pay policy (25% down), @PremiumAmount=1200 | First invoice TotalAmount = $300 + FeeAmount |
| 7 | Installment amounts calculated | @PolicyID=4-pay policy (25% down), @PremiumAmount=1200 | Each installment = $300 + InstallmentFee |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |
| 2 | NULL PolicyID | @PolicyID=NULL | Error (parameter required) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Zero premium amount | @PremiumAmount=0 | Invoice created with TotalAmount=0 + fees |
| 2 | Very large premium | @PremiumAmount=9999999.99 | Invoice created, no overflow |
| 3 | Last installment rounding adjustment | 4-pay, @PremiumAmount=1000 (not evenly divisible by 3) | Last installment = RemainingAmount - (InstallmentAmount * (NumInstallments - 2)) |
| 4 | DueDate = InvoiceDate + GracePeriodDays | Valid policy | First DueDate = EffectiveDate + GracePeriodDays |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | PaymentPlan lookup | Billing.PaymentPlans has matching PlanCode | Policy with multi-pay plan | NumInstallments, DownPaymentPct, InstallmentFee, GracePeriodDays from plan |
| 2 | InvoiceNumber uniqueness | Existing invoices in DB | Valid data | Unique sequential InvoiceNumber (INV+7 digits) |
| 3 | Audit log created | Audit.AuditLog accessible | Valid data | Row with Action='INSERT', TableName='Billing.Invoices' |
| 4 | Installment invoice dates | Multi-pay plan | Valid policy | Each installment InvoiceDate = EffectiveDate + (N-1) months |
| 5 | CustomerID resolved from Policy | Policy.Policies accessible | Valid PolicyID | CustomerID set from policy record |

---

### Test Case ID: SP-BIL-002
**Procedure**: Billing.usp_Payment_Record
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @InvoiceID INT - Optional (NULL finds oldest open invoice)
- @PolicyID INT - Required
- @Amount DECIMAL(18,2) - Required
- @PaymentMethod VARCHAR(20) - Required
- @ReferenceNumber VARCHAR(50) - Optional
- @CheckNumber VARCHAR(20) - Optional
- @BankName VARCHAR(100) - Optional
- @CreditCardLast4 VARCHAR(4) - Optional
- @PaymentDate DATE - Optional (default GETDATE())
- @CreatedBy VARCHAR(50) - Required
- @PremiumPaymentID INT OUTPUT
- @PaymentNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Record check payment against specific invoice | @InvoiceID=valid, @PolicyID=valid, @Amount=500, @PaymentMethod='CHECK', @CheckNumber='12345' | PremiumPaymentID > 0, PaymentNumber=PMP+7 digits, Status='APPLIED' |
| 2 | Record payment without specifying invoice | @InvoiceID=NULL, @PolicyID=valid, @Amount=300, @PaymentMethod='EFT' | Oldest open invoice auto-selected |
| 3 | Full payment clears invoice | @Amount >= InvoiceBalance | Invoice Status='PAID', BalanceDue=0 |
| 4 | Partial payment | @Amount < InvoiceBalance | Invoice Status='PARTIAL', BalanceDue reduced |
| 5 | Overpayment cascades to next invoice | @Amount > InvoiceBalance, next invoice exists | Next invoice PaidAmount increased |
| 6 | Credit card payment | @PaymentMethod='CREDIT_CARD', @CreditCardLast4='4567' | Payment recorded with card info |
| 7 | Wire payment | @PaymentMethod='WIRE', @ReferenceNumber='WIRE-001' | Payment recorded with reference |
| 8 | Receipt number generated | Valid payment | ReceiptNumber=RCP+7 digits |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |
| 2 | No open invoice for policy | @InvoiceID=NULL, policy with all PAID invoices | Payment created with AppliedToInvoice=0 [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Payment exactly equals balance | @Amount = InvoiceBalance exactly | Invoice Status='PAID', BalanceDue=0, RemainingPayment=0 |
| 2 | Payment of $0.01 | @Amount=0.01 | Accepted, invoice remains PARTIAL |
| 3 | Overpayment with no next invoice | @Amount > InvoiceBalance, no next invoice | Overpayment not applied [ASSUMPTION] |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Invoice balance updated | Billing.Invoices accessible | Full payment | PaidAmount=TotalAmount, BalanceDue=0 |
| 2 | Audit log created | Audit.AuditLog accessible | Valid payment | Row with Action='INSERT', TableName='Billing.PremiumPayments' |
| 3 | PaymentNumber uniqueness | Existing payments in DB | Valid data | Unique sequential PaymentNumber (PMP+7 digits) |
| 4 | CustomerID from Policy | Policy.Policies accessible | Valid PolicyID | CustomerID set correctly |

---

### Test Case ID: SP-BIL-003
**Procedure**: Billing.usp_Payment_Return
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @PremiumPaymentID INT - Required
- @ReturnReason VARCHAR(200) - Required
- @NSFFee DECIMAL(10,2) - Optional (default 25.00)
- @ProcessedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Return applied payment (NSF check) | @PremiumPaymentID=valid APPLIED payment, @ReturnReason='NSF', @ProcessedBy='user' | Payment Status='RETURNED', ReturnedDate set |
| 2 | Invoice balance reversed | Valid return | Invoice PaidAmount decreased, BalanceDue increased by Amount + NSFFee |
| 3 | Custom NSF fee | @NSFFee=50.00 | Invoice TotalAmount increased by $50, BalanceDue increased by Amount + $50 |
| 4 | Default NSF fee applied | @NSFFee not specified | NSFFee defaults to $25.00 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Payment not found | @PremiumPaymentID=99999 | 'Payment not found: 99999' |
| 2 | Payment not in APPLIED status | @PremiumPaymentID=RETURNED payment | 'Payment is not in APPLIED status: RETURNED' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | NSF fee of $0 | @NSFFee=0 | Invoice BalanceDue increases by payment amount only |
| 2 | Return with no linked invoice | Payment with InvoiceID=NULL | Payment marked RETURNED, no invoice update |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Invoice status updated | Invoice previously PAID | Return payment | Invoice Status='OVERDUE' |
| 2 | Audit log created | Audit.AuditLog accessible | Valid return | Row with Action='UPDATE', FieldName='Status', OldValue='APPLIED', NewValue='RETURNED' |

---

### Test Case ID: SP-BIL-004
**Procedure**: Billing.usp_Refund_Create
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @RefundType VARCHAR(20) - Required
- @RefundMethod VARCHAR(20) - Optional (default 'CHECK')
- @Amount DECIMAL(18,2) - Required
- @CalculationMethod VARCHAR(20) - Optional (default 'PRO_RATA')
- @ProRataFactor DECIMAL(10,8) - Optional
- @EarnedPremium DECIMAL(18,2) - Optional
- @CreatedBy VARCHAR(50) - Required
- @RefundID INT OUTPUT
- @RefundNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create refund under threshold | @PolicyID=valid, @RefundType='CANCELLATION', @Amount=1000, @CalculationMethod='PRO_RATA' | RefundID > 0, RefundNumber=RFD+7 digits, Status='APPROVED' |
| 2 | Create refund over approval threshold | @Amount=6000 (threshold is 5000 from SystemConfig) | Status='PENDING' (requires approval) |
| 3 | Refund with EFT method | @RefundMethod='EFT' | RefundMethod='EFT' |
| 4 | Refund with credit card reversal | @RefundMethod='CREDIT_CARD_REVERSAL' | RefundMethod='CREDIT_CARD_REVERSAL' |
| 5 | Overpayment refund type | @RefundType='OVERPAYMENT' | RefundType set correctly |
| 6 | Duplicate payment refund | @RefundType='DUPLICATE' | RefundType set correctly |
| 7 | Short rate calculation method | @CalculationMethod='SHORT_RATE' | CalculationMethod stored |
| 8 | Flat calculation method | @CalculationMethod='FLAT' | CalculationMethod stored |
| 9 | Pro rata factor stored | @ProRataFactor=0.54794521 | ProRataFactor value stored |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found | @PolicyID=99999 | 'Policy not found: 99999' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Amount exactly at threshold ($5000) | @Amount=5000 | Status='APPROVED' (not > threshold) |
| 2 | Amount $5000.01 | @Amount=5000.01 | Status='PENDING' (> threshold) |
| 3 | Minimum refund amount | @Amount=0.01 | Refund created |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Approval threshold from config | Admin.SystemConfig has 'REFUND_APPROVAL_THRESHOLD' | Amount > threshold | Status='PENDING' |
| 2 | RefundNumber uniqueness | Existing refunds in DB | Valid data | Unique sequential RefundNumber (RFD+7 digits) |
| 3 | CustomerID resolved from Policy | Policy.Policies accessible | Valid PolicyID | CustomerID set from policy |
| 4 | Audit log created | Audit.AuditLog accessible | Valid refund | Row with Action='INSERT', TableName='Billing.Refunds' |

---

### Test Case ID: SP-BIL-005
**Procedure**: Billing.usp_Refund_Approve
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @RefundID INT - Required
- @ApprovedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Approve pending refund | @RefundID=valid PENDING refund, @ApprovedBy='manager' | Status='APPROVED', ApprovedBy='manager', ApprovedDate set |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Refund not found | @RefundID=99999 | 'Refund not found: 99999' |
| 2 | Refund not in PENDING status (APPROVED) | @RefundID=already approved | 'Refund is not in PENDING status: APPROVED' |
| 3 | Refund not in PENDING status (ISSUED) | @RefundID=already issued | 'Refund is not in PENDING status: ISSUED' |
| 4 | Refund not in PENDING status (VOIDED) | @RefundID=voided refund | 'Refund is not in PENDING status: VOIDED' |

---

### Test Case ID: SP-BIL-006
**Procedure**: Billing.usp_Invoice_ApplyLateFees
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (default GETDATE())
- @ProcessedBy VARCHAR(50) - Required
- @InvoicesProcessed INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Apply late fees to overdue invoices | @AsOfDate=today, @ProcessedBy='SYSTEM' | Overdue invoices updated, @InvoicesProcessed = count of affected |
| 2 | Invoice status changed to OVERDUE | Open invoice past due date | Status='OVERDUE', LateFeeApplied=1 |
| 3 | Late fee from payment plan used | Invoice linked to PaymentPlan with LateFeeAmount | LateFeeAmount = plan's LateFeeAmount |
| 4 | Default late fee used when no plan | Invoice without PaymentPlanID | LateFeeAmount = DEFAULT_LATE_FEE from SystemConfig (default $15.00) |
| 5 | TotalAmount and BalanceDue increased | Valid overdue invoice | TotalAmount += LateFee, BalanceDue += LateFee |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No overdue invoices | All invoices current | @InvoicesProcessed = 0 |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Invoice DueDate = AsOfDate (not overdue) | DueDate = @AsOfDate | NOT processed (condition: DueDate < @AsOfDate) |
| 2 | Invoice DueDate = AsOfDate - 1 day | DueDate = @AsOfDate - 1 | Processed (overdue by 1 day) |
| 3 | Invoice already has late fee applied | LateFeeApplied=1 | NOT processed again (LateFeeApplied = 0 required) |
| 4 | Invoice with BalanceDue = 0 | BalanceDue=0 | NOT processed (BalanceDue > 0 required) |

---

### Test Case ID: SP-BIL-007
**Procedure**: Billing.usp_Commission_Create
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @TransactionType VARCHAR(20) - Required (EARNED, REVERSAL, OVERRIDE, BONUS, CHARGEBACK)
- @PremiumAmount DECIMAL(18,2) - Required
- @CommissionRate DECIMAL(6,4) - Required
- @Description VARCHAR(200) - Optional
- @CreatedBy VARCHAR(50) - Required
- @CommissionTransactionID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create earned commission | @PolicyID=valid, @TransactionType='EARNED', @PremiumAmount=1000, @CommissionRate=0.1000 | CommissionAmount=$100, Status='PENDING' |
| 2 | Create reversal (negative amount) | @TransactionType='REVERSAL', @PremiumAmount=1000, @CommissionRate=0.1000 | CommissionAmount=-$100 |
| 3 | Create chargeback (negative amount) | @TransactionType='CHARGEBACK', @PremiumAmount=500, @CommissionRate=0.1500 | CommissionAmount=-$75 |
| 4 | Create override commission | @TransactionType='OVERRIDE', @PremiumAmount=1000, @CommissionRate=0.0200 | CommissionAmount=$20 |
| 5 | Create bonus commission | @TransactionType='BONUS', @PremiumAmount=5000, @CommissionRate=0.0100 | CommissionAmount=$50 |
| 6 | AgentID resolved from policy | Valid PolicyID with AgentID | AgentID set on transaction |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy not found (no AgentID) | @PolicyID=99999 | 'Policy not found: 99999' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Commission rate of 0 | @CommissionRate=0 | CommissionAmount=$0 |
| 2 | Commission rate of 1.0 (100%) | @CommissionRate=1.0000 | CommissionAmount = PremiumAmount |
| 3 | Very small premium | @PremiumAmount=0.01, @CommissionRate=0.1000 | CommissionAmount=$0.00 (ROUND to 2 decimals) |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | AgentID from Policy | Policy.Policies has AgentID | Valid PolicyID | CommissionTransactions.AgentID set |
| 2 | Error logged on failure | Audit.ErrorLog accessible | Exception scenario | Error row created |

---

### Test Case ID: SP-BIL-008
**Procedure**: Billing.usp_Commission_GetStatement
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @AgentID INT - Required
- @PeriodFrom DATE - Optional (default 1 month ago)
- @PeriodTo DATE - Optional (default today)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get statement with transactions | @AgentID=valid agent with transactions | Result set 1: Summary (TotalEarned, TotalReversals, TotalOverrides, TotalBonus, NetCommission). Result set 2: Transaction details |
| 2 | Get statement with no transactions | @AgentID=agent with no transactions in period | Empty result sets |
| 3 | Custom date range | @PeriodFrom='2024-01-01', @PeriodTo='2024-03-31' | Only transactions within range |
| 4 | Default date range | @PeriodFrom=NULL, @PeriodTo=NULL | Defaults to last month |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid AgentID | @AgentID=99999 | Empty result set (no error) |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Summary aggregates correctly | Multiple transaction types | Valid agent | TotalEarned = SUM(EARNED), TotalReversals = SUM(REVERSAL+CHARGEBACK) |
| 2 | Detail includes policy/customer info | Policy.Policies, Policy.Customers joined | Valid agent | PolicyNumber, PolicyType, CustomerName in detail |
| 3 | AgentName from Agents table | Policy.Agents joined | Valid agent | AgentNumber, AgentName, AgencyName in summary |

---

### Test Case ID: SP-BIL-009
**Procedure**: Billing.usp_Billing_GetByPolicy
**Source**: `database/02-stored-procedures/006-billing-sps.sql`
**Parameters**:
- @PolicyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get billing for policy with data | @PolicyID=valid policy with invoices/payments/refunds | 4 result sets: Summary, Invoices, Payments, Refunds |
| 2 | Summary totals correct | @PolicyID with billing history | TotalBilled, TotalPaid, TotalOutstanding calculated |
| 3 | Invoices ordered by date | @PolicyID with multiple invoices | Ordered by InvoiceDate ASC |
| 4 | Payments ordered by date desc | @PolicyID with payments | Ordered by PaymentDate DESC |
| 5 | Refunds ordered by date desc | @PolicyID with refunds | Ordered by CreatedDate DESC |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Policy with no billing data | @PolicyID=new policy | Summary with zeros, empty detail result sets |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | TotalBilled excludes CANCELLED | Invoices with Status='CANCELLED' | Valid policy | CANCELLED invoices excluded from TotalBilled |
| 2 | TotalOutstanding filters by status | Mix of OPEN, OVERDUE, PARTIAL, PAID | Valid policy | Only OPEN + OVERDUE + PARTIAL in Outstanding |
| 3 | PolicyNumber and GrossPremium from Policy | Policy.Policies joined | Valid policy | Policy details in summary |

---

### Test Case ID: SP-BIL-010
**Procedure**: Billing.usp_Refund_GetPending
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Returns pending and approved refunds | (no params) | All refunds with Status IN ('PENDING', 'APPROVED') |
| 2 | Includes policy and customer info | (no params) | PolicyNumber and CustomerName joined |
| 3 | Ordered by created date desc | (no params) | Most recent refunds first |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No pending refunds | All refunds ISSUED or VOIDED | Empty result set |

---

### Test Case ID: SP-BIL-011
**Procedure**: Billing.usp_PaymentPlan_List
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Returns all payment plans | (no params) | All rows from Billing.PaymentPlans |
| 2 | Ordered by PlanName | (no params) | Alphabetical order by PlanName |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Empty PaymentPlans table | No plans configured | Empty result set |
