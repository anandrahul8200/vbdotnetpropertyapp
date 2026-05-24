# Billing Module - Data Validation Tests

## Module: BIL (Billing)
## Test Type: Data Integrity and Validation Tests
## Schema Source: `database/01-schema/006-billing-reinsurance-tables.sql`

---

### Test Case ID: DV-BIL-001
**Table**: Billing.Invoices
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| InvoiceID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| InvoiceNumber | VARCHAR(20) | NO | None | UNIQUE |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| CustomerID | INT | NO | None | FK -> Policy.Customers(CustomerID) |
| InvoiceType | VARCHAR(20) | NO | None | Valid: NEW_BUSINESS, RENEWAL, ENDORSEMENT, INSTALLMENT, REINSTATEMENT |
| InvoiceDate | DATE | NO | GETDATE() | Not null |
| DueDate | DATE | NO | None | Must be >= InvoiceDate |
| PremiumAmount | DECIMAL(18,2) | NO | None | >= 0 |
| TaxAmount | DECIMAL(18,2) | YES | 0 | >= 0 |
| FeeAmount | DECIMAL(18,2) | YES | 0 | >= 0 |
| SurchargeAmount | DECIMAL(18,2) | YES | 0 | >= 0 |
| TotalAmount | DECIMAL(18,2) | NO | None | = PremiumAmount + TaxAmount + FeeAmount + SurchargeAmount (+ LateFeeAmount if applied) |
| PaidAmount | DECIMAL(18,2) | YES | 0 | >= 0, <= TotalAmount |
| BalanceDue | DECIMAL(18,2) | YES | None | = TotalAmount - PaidAmount |
| Status | VARCHAR(20) | YES | 'OPEN' | Valid: OPEN, PAID, PARTIAL, OVERDUE, CANCELLED, WRITTEN_OFF |
| InstallmentNumber | INT | YES | 1 | >= 1 |
| TotalInstallments | INT | YES | 1 | >= 1, >= InstallmentNumber |
| PaymentPlanID | INT | YES | NULL | FK -> Billing.PaymentPlans(PaymentPlanID) [ASSUMPTION] |
| LateFeeApplied | BIT | YES | 0 | Boolean |
| LateFeeAmount | DECIMAL(18,2) | YES | 0 | >= 0 |
| CancellationNoticeDate | DATE | YES | NULL | Set when cancellation notice sent |
| CancellationEffectiveDate | DATE | YES | NULL | Must be > CancellationNoticeDate |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |
| CreatedBy | VARCHAR(50) | YES | None | Username |
| ModifiedDate | DATETIME | YES | GETDATE() | Updated on change |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | InvoiceNumber format | All records | Match pattern INV + 7 digits (INV\d{7}) |
| 2 | InvoiceNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | PolicyID FK valid | Insert with invalid PolicyID | FK constraint violation |
| 4 | CustomerID FK valid | Insert with invalid CustomerID | FK constraint violation |
| 5 | InvoiceType values | All records | Only valid InvoiceType values |
| 6 | Status values | All records | Only valid Status values |
| 7 | TotalAmount calculation | All records | TotalAmount = PremiumAmount + TaxAmount + FeeAmount + SurchargeAmount + LateFeeAmount |
| 8 | BalanceDue calculation | All records | BalanceDue = TotalAmount - PaidAmount |
| 9 | PaidAmount <= TotalAmount | All records | No overpayment on single invoice |
| 10 | DueDate >= InvoiceDate | All records | Due date not before invoice date |
| 11 | InstallmentNumber <= TotalInstallments | All records | Installment number within range |
| 12 | Status=PAID implies BalanceDue=0 | PAID records | BalanceDue must be 0 |
| 13 | Status=OPEN implies BalanceDue>0 | OPEN records | BalanceDue must be > 0 |
| 14 | LateFeeApplied=1 implies LateFeeAmount>0 | Records with late fee | Fee amount set when flag is true |

---

### Test Case ID: DV-BIL-002
**Table**: Billing.PremiumPayments
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| PremiumPaymentID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PaymentNumber | VARCHAR(20) | NO | None | UNIQUE |
| InvoiceID | INT | YES | None | FK -> Billing.Invoices(InvoiceID) |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| CustomerID | INT | NO | None | FK -> Policy.Customers(CustomerID) |
| PaymentDate | DATE | NO | GETDATE() | Not null |
| Amount | DECIMAL(18,2) | NO | None | > 0 |
| PaymentMethod | VARCHAR(20) | NO | None | Valid: CHECK, CREDIT_CARD, EFT, CASH, WIRE |
| ReferenceNumber | VARCHAR(50) | YES | None | Optional reference |
| CheckNumber | VARCHAR(20) | YES | None | Required when PaymentMethod=CHECK [ASSUMPTION] |
| BankName | VARCHAR(100) | YES | None | Optional |
| CreditCardLast4 | VARCHAR(4) | YES | None | Exactly 4 digits when provided |
| Status | VARCHAR(20) | YES | 'APPLIED' | Valid: APPLIED, RETURNED, REFUNDED, PENDING |
| ReturnedDate | DATE | YES | NULL | Set when Status=RETURNED |
| ReturnReason | VARCHAR(200) | YES | NULL | Set when Status=RETURNED |
| NSFFee | DECIMAL(10,2) | YES | 0 | >= 0 |
| AppliedToInvoice | BIT | YES | 1 | 1 if linked to invoice |
| ReceiptNumber | VARCHAR(20) | YES | None | Format RCP+7 digits |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |
| CreatedBy | VARCHAR(50) | YES | None | Username |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | PaymentNumber format | All records | Match pattern PMP + 7 digits (PMP\d{7}) |
| 2 | PaymentNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | PolicyID FK valid | Insert with invalid PolicyID | FK constraint violation |
| 4 | Amount > 0 | All records | No zero or negative amounts |
| 5 | PaymentMethod values | All records | Only valid PaymentMethod values |
| 6 | Status values | All records | Only valid Status values |
| 7 | CreditCardLast4 length | When provided | Exactly 4 characters |
| 8 | ReturnedDate set when RETURNED | Status=RETURNED | ReturnedDate IS NOT NULL |
| 9 | ReturnReason set when RETURNED | Status=RETURNED | ReturnReason IS NOT NULL |
| 10 | ReceiptNumber format | All records | Match pattern RCP + 7 digits (RCP\d{7}) |
| 11 | InvoiceID FK valid | When InvoiceID provided | FK constraint satisfied |
| 12 | AppliedToInvoice consistency | InvoiceID NOT NULL | AppliedToInvoice = 1 |

---

### Test Case ID: DV-BIL-003
**Table**: Billing.PaymentPlans
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| PaymentPlanID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PlanCode | VARCHAR(20) | NO | None | UNIQUE |
| PlanName | VARCHAR(100) | NO | None | Not null |
| NumberOfInstallments | INT | NO | None | >= 1 |
| DownPaymentPercent | DECIMAL(6,4) | NO | None | > 0, <= 1.0 |
| InstallmentFee | DECIMAL(10,2) | YES | 0 | >= 0 |
| LateFeeAmount | DECIMAL(10,2) | YES | 0 | >= 0 |
| GracePeriodDays | INT | YES | 10 | >= 0 |
| CancellationNoticeDays | INT | YES | 20 | >= 0 |
| IsActive | BIT | YES | 1 | Boolean |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | PlanCode uniqueness | Duplicate insert | UNIQUE constraint violation |
| 2 | NumberOfInstallments >= 1 | All records | No zero or negative installments |
| 3 | DownPaymentPercent range | All records | Between 0 (exclusive) and 1.0 (inclusive) |
| 4 | InstallmentFee >= 0 | All records | No negative fees |
| 5 | LateFeeAmount >= 0 | All records | No negative late fees |
| 6 | GracePeriodDays >= 0 | All records | No negative grace period |
| 7 | CancellationNoticeDays >= 0 | All records | No negative notice days |
| 8 | ANNUAL plan has 1 installment, 100% down | PlanCode='ANNUAL' | NumberOfInstallments=1, DownPaymentPercent=1.0 [ASSUMPTION] |

---

### Test Case ID: DV-BIL-004
**Table**: Billing.CommissionTransactions
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| CommissionTransactionID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| AgentID | INT | NO | None | FK -> Policy.Agents(AgentID) |
| TransactionType | VARCHAR(20) | NO | None | Valid: EARNED, REVERSAL, OVERRIDE, BONUS, CHARGEBACK |
| TransactionDate | DATE | NO | GETDATE() | Not null |
| PremiumAmount | DECIMAL(18,2) | YES | None | The premium base for calculation |
| CommissionRate | DECIMAL(6,4) | YES | None | Between 0 and 1.0 |
| CommissionAmount | DECIMAL(18,2) | NO | None | Positive for EARNED/OVERRIDE/BONUS, negative for REVERSAL/CHARGEBACK |
| Status | VARCHAR(20) | YES | 'PENDING' | Valid: PENDING, APPROVED, PAID, REVERSED |
| PaymentDate | DATE | YES | NULL | Set when Status=PAID |
| PaymentReference | VARCHAR(50) | YES | NULL | Reference for payment check/EFT |
| Description | VARCHAR(200) | YES | NULL | Optional description |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | PolicyID FK valid | Insert with invalid PolicyID | FK constraint violation |
| 2 | AgentID FK valid | Insert with invalid AgentID | FK constraint violation |
| 3 | TransactionType values | All records | Only valid TransactionType values |
| 4 | Status values | All records | Only valid Status values |
| 5 | REVERSAL/CHARGEBACK amounts negative | TransactionType IN (REVERSAL, CHARGEBACK) | CommissionAmount < 0 |
| 6 | EARNED/OVERRIDE/BONUS amounts positive | TransactionType IN (EARNED, OVERRIDE, BONUS) | CommissionAmount >= 0 |
| 7 | CommissionRate range | All records | Between 0 and 1.0 |
| 8 | CommissionAmount calculation | All records | ABS(CommissionAmount) = ROUND(PremiumAmount * CommissionRate, 2) |
| 9 | PaymentDate set when PAID | Status=PAID | PaymentDate IS NOT NULL |

---

### Test Case ID: DV-BIL-005
**Table**: Billing.Refunds
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| RefundID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| RefundNumber | VARCHAR(20) | NO | None | UNIQUE |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| CustomerID | INT | NO | None | FK -> Policy.Customers(CustomerID) |
| RefundType | VARCHAR(20) | NO | None | Valid: CANCELLATION, ENDORSEMENT, OVERPAYMENT, DUPLICATE |
| RefundMethod | VARCHAR(20) | YES | 'CHECK' | Valid: CHECK, EFT, CREDIT_CARD_REVERSAL |
| Amount | DECIMAL(18,2) | NO | None | > 0 |
| CalculationMethod | VARCHAR(20) | YES | None | Valid: PRO_RATA, SHORT_RATE, FLAT |
| ProRataFactor | DECIMAL(10,8) | YES | None | Between 0 and 1 when provided |
| EarnedPremium | DECIMAL(18,2) | YES | None | >= 0 when provided |
| ReturnPremium | DECIMAL(18,2) | YES | None | = Amount (set in SP) |
| Status | VARCHAR(20) | YES | 'PENDING' | Valid: PENDING, APPROVED, ISSUED, CLEARED, VOIDED |
| ApprovedBy | VARCHAR(50) | YES | None | Set when Status >= APPROVED |
| ApprovedDate | DATETIME | YES | None | Set when Status >= APPROVED |
| IssuedDate | DATE | YES | NULL | Set when Status >= ISSUED |
| CheckNumber | VARCHAR(20) | YES | NULL | Set for CHECK method when issued |
| CreatedDate | DATETIME | YES | GETDATE() | Auto-set |
| CreatedBy | VARCHAR(50) | YES | None | Username |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | RefundNumber format | All records | Match pattern RFD + 7 digits (RFD\d{7}) |
| 2 | RefundNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | PolicyID FK valid | Insert with invalid PolicyID | FK constraint violation |
| 4 | CustomerID FK valid | Insert with invalid CustomerID | FK constraint violation |
| 5 | RefundType values | All records | Only valid RefundType values |
| 6 | RefundMethod values | All records | Only valid RefundMethod values |
| 7 | Status values | All records | Only valid Status values |
| 8 | CalculationMethod values | When provided | Only valid CalculationMethod values |
| 9 | Amount > 0 | All records | No zero or negative refund amounts |
| 10 | ProRataFactor range | When provided | Between 0 and 1 (exclusive of 0) |
| 11 | ApprovedBy set when APPROVED | Status IN (APPROVED, ISSUED, CLEARED) | ApprovedBy IS NOT NULL |
| 12 | ApprovedDate set when APPROVED | Status IN (APPROVED, ISSUED, CLEARED) | ApprovedDate IS NOT NULL |
| 13 | IssuedDate set when ISSUED | Status IN (ISSUED, CLEARED) | IssuedDate IS NOT NULL |
| 14 | Status transition valid | All records | PENDING -> APPROVED -> ISSUED -> CLEARED, or PENDING/APPROVED -> VOIDED |
