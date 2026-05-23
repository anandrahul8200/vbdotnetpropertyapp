# Billing Module - Business Rules

## Module: BIL (Billing)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-BIL-001
**Module**: BIL
**Priority**: Critical

#### Rule Description
Multi-installment invoice generation calculates down payment and installment amounts based on the payment plan configuration. The total amount is split into a down payment (first invoice) and equal installments for the remainder. The last installment is adjusted for rounding to ensure the sum of all installments equals the total premium.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_Generate
- **Code Snippet**:
```sql
DECLARE @DownPayment DECIMAL(18,2) = ROUND(@TotalAmount * @DownPaymentPct, 2);
DECLARE @RemainingAmount DECIMAL(18,2) = @TotalAmount - @DownPayment;
DECLARE @InstallmentAmount DECIMAL(18,2);
...
SET @InstallmentAmount = ROUND(@RemainingAmount / (@NumInstallments - 1), 2);
...
-- Last installment gets remainder to avoid rounding issues
IF @InstallmentNum = @NumInstallments
    SET @InstallmentAmount = @RemainingAmount - (@InstallmentAmount * (@NumInstallments - 2));
```

#### Enforcement Mechanism
- Type: SP logic (calculation with rounding adjustment)
- Behavior: Ensures total of all installments exactly equals the total premium amount

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-001 | Multi-installment invoice generation |
| FT-BIL-002 | Invoice Generation (Multi-Pay Installment Plan) |
| US-BIL-006 | Invoice Generation for New Business |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Premium not evenly divisible (e.g., $1000 / 3 installments) | Last installment adjusted: $1000 - ($333.33 * 2) = $333.34 |
| 2 | Very small down payment percentage (1%) | Down payment = ROUND(total * 0.01, 2), remainder split among installments |
| 3 | 2 installments (down + 1 remaining) | Second installment = TotalAmount - DownPayment exactly |
| 4 | Down payment of 100% (ANNUAL) | @NumInstallments = 1, single invoice for full amount |

---

### Rule ID: BR-BIL-002
**Module**: BIL
**Priority**: Critical

#### Rule Description
Down payment percentage is defined per payment plan in Billing.PaymentPlans. The down payment is the first invoice amount and equals ROUND(TotalAmount * DownPaymentPercent, 2). For ANNUAL plans, DownPaymentPercent = 1.0 (100%), resulting in a single invoice.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_Generate
- **Code Snippet**:
```sql
DECLARE @NumInstallments INT = 1, @DownPaymentPct DECIMAL(6,4) = 1.0;
...
IF @PaymentPlan <> 'ANNUAL'
BEGIN
    SELECT TOP 1 @PaymentPlanID = PaymentPlanID, @NumInstallments = NumberOfInstallments,
           @DownPaymentPct = DownPaymentPercent, @InstallmentFee = InstallmentFee,
           @GracePeriodDays = GracePeriodDays
    FROM Billing.PaymentPlans
    WHERE PlanCode = @PaymentPlan AND IsActive = 1;
END
```

#### Enforcement Mechanism
- Type: SP logic (lookup from PaymentPlans table, defaults for ANNUAL)
- Behavior: ANNUAL plan defaults to single full payment; other plans use configured percentage

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-001 | Payment plan lookup in invoice generation |
| DV-BIL-003 | PaymentPlans table validation |
| US-BIL-005 | Payment Plan Setup |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Policy has PaymentPlan='ANNUAL' | Defaults apply: NumInstallments=1, DownPaymentPct=1.0 |
| 2 | PaymentPlan code not found in table | Plan lookup returns no rows; defaults remain (1 installment, 100% down) [ASSUMPTION] |
| 3 | Inactive plan (IsActive=0) | Plan not selected; defaults apply [ASSUMPTION] |

---

### Rule ID: BR-BIL-003
**Module**: BIL
**Priority**: High

#### Rule Description
Grace period days define the number of days after the invoice date that the payment is due. DueDate = InvoiceDate + GracePeriodDays. The default grace period is 30 days for ANNUAL plans. Multi-pay plans use the GracePeriodDays from the Billing.PaymentPlans table (default 10 days per schema).

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_Generate
- **Code Snippet**:
```sql
DECLARE @GracePeriodDays INT = 30;
...
SET @DueDate = DATEADD(DAY, @GracePeriodDays, @InvoiceDate);
```

#### Enforcement Mechanism
- Type: SP logic (DATEADD calculation)
- Behavior: DueDate calculated as InvoiceDate + configured grace period

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-001 | DueDate calculation verification |
| SP-BIL-006 | Late fee application (compares DueDate to AsOfDate) |
| DV-BIL-001 | DueDate >= InvoiceDate validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | GracePeriodDays = 0 | DueDate = InvoiceDate (due immediately) |
| 2 | GracePeriodDays crosses month boundary | DueDate correctly calculated across months |
| 3 | GracePeriodDays for installment vs first invoice | Same grace period applied to all invoices in plan |

---

### Rule ID: BR-BIL-004
**Module**: BIL
**Priority**: High

#### Rule Description
Installment fee is applied to each invoice in a multi-pay plan. The fee is defined in Billing.PaymentPlans.InstallmentFee and is added to the first invoice and each subsequent installment invoice. The fee increases the TotalAmount and BalanceDue of each invoice.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_Generate
- **Code Snippet**:
```sql
-- First invoice includes FeeAmount from parameter
CASE WHEN @NumInstallments = 1 THEN @TotalAmount ELSE @DownPayment + @FeeAmount END,
...
-- Installment invoices include InstallmentFee
INSERT INTO Billing.Invoices (..., TotalAmount, ...) VALUES (
    ..., @InstallmentAmount + @InstallmentFee, ...
);
```

#### Enforcement Mechanism
- Type: SP logic (fee added to each invoice TotalAmount)
- Behavior: Each installment invoice TotalAmount = InstallmentAmount + InstallmentFee

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-001 | Installment fee in invoice generation |
| FT-BIL-002 | Multi-pay workflow with fees |
| UI-BIL-005 | Payment plan preview with fees |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | InstallmentFee = 0 | TotalAmount = InstallmentAmount only |
| 2 | ANNUAL plan (single invoice) | FeeAmount parameter used for first invoice; no InstallmentFee |
| 3 | Large installment fee relative to premium | Fee added regardless of premium amount |

---

### Rule ID: BR-BIL-005
**Module**: BIL
**Priority**: High

#### Rule Description
Late fees are applied to overdue invoices via batch process. An invoice is eligible for late fee when: Status = 'OPEN', DueDate < AsOfDate, LateFeeApplied = 0, and BalanceDue > 0. The late fee amount comes from the PaymentPlan's LateFeeAmount if the invoice is linked to a plan, otherwise from Admin.SystemConfig 'DEFAULT_LATE_FEE' (default $15.00). Late fees are applied only once per invoice.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_ApplyLateFees
- **Code Snippet**:
```sql
DECLARE @DefaultLateFee DECIMAL(10,2) = 15.00;
SELECT @DefaultLateFee = CAST(ConfigValue AS DECIMAL(10,2))
FROM Admin.SystemConfig WHERE ConfigKey = 'DEFAULT_LATE_FEE';

UPDATE Billing.Invoices SET
    Status = 'OVERDUE',
    LateFeeApplied = 1,
    LateFeeAmount = ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
    TotalAmount = TotalAmount + ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
    BalanceDue = BalanceDue + ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
    ModifiedDate = GETDATE()
WHERE Status = 'OPEN' AND DueDate < @AsOfDate AND LateFeeApplied = 0 AND BalanceDue > 0;
```

#### Enforcement Mechanism
- Type: SP logic (batch UPDATE with eligibility conditions)
- Behavior: One-time late fee application per invoice; idempotent batch operation

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-006 | Late fee application batch |
| FT-BIL-006 | Late Fee Application workflow |
| US-BIL-007 | Late Fee Management |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Invoice exactly on due date (DueDate = AsOfDate) | NOT processed (condition: DueDate < AsOfDate, not <=) |
| 2 | Invoice with LateFeeApplied=1 already | NOT processed again (idempotent) |
| 3 | Invoice with BalanceDue=0 (paid after being overdue) | NOT processed |
| 4 | PaymentPlan has LateFeeAmount=0 | Late fee of $0 applied (LateFeeApplied=1 but no financial impact) [ASSUMPTION] |
| 5 | SystemConfig key missing | Default $15.00 used |

---

### Rule ID: BR-BIL-006
**Module**: BIL
**Priority**: Critical

#### Rule Description
Refund calculation uses three methods: PRO_RATA (default), SHORT_RATE, and FLAT. For cancellation refunds, PRO_RATA calculates the unearned premium based on the pro rata factor (days remaining / days in term). The refund amount is stored in the Amount and ReturnPremium fields.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Refund_Create
- **Code Snippet**:
```sql
@CalculationMethod VARCHAR(20) = 'PRO_RATA',
@ProRataFactor DECIMAL(10,8) = NULL,
@EarnedPremium DECIMAL(18,2) = NULL,
...
INSERT INTO Billing.Refunds (
    ..., Amount, CalculationMethod, ProRataFactor, EarnedPremium, ReturnPremium, ...
) VALUES (
    ..., @Amount, @CalculationMethod, @ProRataFactor, @EarnedPremium, @Amount, ...
);
```

#### Enforcement Mechanism
- Type: SP parameter with default value; calculation done by caller
- Behavior: Method stored for audit trail; actual amount calculated by calling code

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-004 | Refund creation with calculation methods |
| FT-BIL-004 | Refund Processing workflow |
| US-BIL-003 | Refund Processing |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | PRO_RATA with ProRataFactor provided | Factor stored for audit |
| 2 | SHORT_RATE calculation | Higher penalty than PRO_RATA; amount reflects short rate table [ASSUMPTION] |
| 3 | FLAT refund | Fixed amount regardless of time elapsed |
| 4 | Refund amount exceeds total premium paid | SP does not validate amount vs premium [ASSUMPTION] |

---

### Rule ID: BR-BIL-007
**Module**: BIL
**Priority**: Critical

#### Rule Description
Payment recording automatically allocates payment to invoices. If no InvoiceID is specified, the oldest open invoice (Status IN 'OPEN', 'OVERDUE', 'PARTIAL') is selected. If the payment exceeds the invoice balance, the overpayment is applied to the next open invoice. Invoice status transitions: OPEN/OVERDUE -> PARTIAL (if partial payment) or PAID (if fully paid).

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Payment_Record
- **Code Snippet**:
```sql
-- If no invoice specified, find oldest open invoice
IF @InvoiceID IS NULL
BEGIN
    SELECT TOP 1 @InvoiceID = InvoiceID
    FROM Billing.Invoices
    WHERE PolicyID = @PolicyID AND Status IN ('OPEN', 'OVERDUE', 'PARTIAL')
    ORDER BY DueDate ASC;
END
...
-- Apply overpayment to next invoice if any
IF @RemainingPayment > 0
BEGIN
    SELECT TOP 1 @NextInvoiceID = InvoiceID
    FROM Billing.Invoices
    WHERE PolicyID = @PolicyID AND InvoiceID > @InvoiceID 
        AND Status IN ('OPEN', 'OVERDUE')
    ORDER BY DueDate ASC;
    ...
END
```

#### Enforcement Mechanism
- Type: SP logic (auto-selection and cascading allocation)
- Behavior: Payments flow to oldest invoice first, overflow to next

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-002 | Payment recording with invoice allocation |
| FT-BIL-003 | Payment Recording and Invoice Allocation |
| US-BIL-002 | Payment Entry |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Payment exactly equals invoice balance | Invoice PAID, no overflow |
| 2 | Overpayment with no next invoice | Overflow not applied; payment still recorded |
| 3 | Multiple overdue invoices | Oldest by DueDate selected first |
| 4 | All invoices already PAID | InvoiceID remains NULL, payment recorded with AppliedToInvoice=0 |

---

### Rule ID: BR-BIL-008
**Module**: BIL
**Priority**: Critical

#### Rule Description
Invoice number format is 'INV' followed by 7 digits (e.g., INV0000001). The sequence is generated from MAX(InvoiceID) + 1, zero-padded to 7 digits. Payment number format is 'PMP' followed by 7 digits. Refund number format is 'RFD' followed by 7 digits.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Invoice_Generate, Billing.usp_Payment_Record, Billing.usp_Refund_Create
- **Code Snippet**:
```sql
-- Invoice Number
SELECT @Sequence = ISNULL(MAX(InvoiceID), 0) + 1 FROM Billing.Invoices;
SET @InvoiceNumber = 'INV' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

-- Payment Number
SELECT @Sequence = ISNULL(MAX(PremiumPaymentID), 0) + 1 FROM Billing.PremiumPayments;
SET @PaymentNumber = 'PMP' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

-- Refund Number
SELECT @Sequence = ISNULL(MAX(RefundID), 0) + 1 FROM Billing.Refunds;
SET @RefundNumber = 'RFD' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);
```

#### Enforcement Mechanism
- Type: SP logic (MAX + 1 with zero-padding) and UNIQUE constraint on table columns
- Behavior: Sequential numbering with format prefix; uniqueness enforced by database

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-001 | InvoiceNumber generation |
| SP-BIL-002 | PaymentNumber generation |
| SP-BIL-004 | RefundNumber generation |
| DV-BIL-001 | InvoiceNumber format validation |
| DV-BIL-002 | PaymentNumber format validation |
| DV-BIL-005 | RefundNumber format validation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | First invoice (no existing records) | ISNULL(MAX(InvoiceID), 0) + 1 = 1; INV0000001 |
| 2 | Concurrent inserts | Potential race condition on MAX query; UNIQUE constraint catches duplicates |
| 3 | Sequence exceeds 7 digits (> 9,999,999) | Number exceeds format width; RIGHT function truncates to last 7 chars |
| 4 | Gaps in IDs (deleted records) | Sequence based on MAX not COUNT; gaps are acceptable |

---

### Rule ID: BR-BIL-009
**Module**: BIL
**Priority**: High

#### Rule Description
Refund approval threshold is configurable via Admin.SystemConfig key 'REFUND_APPROVAL_THRESHOLD'. Refunds with Amount > threshold require manual approval (Status='PENDING'). Refunds at or below the threshold are auto-approved (Status='APPROVED'). Default threshold is $5,000.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Refund_Create
- **Code Snippet**:
```sql
DECLARE @ApprovalThreshold DECIMAL(18,2) = 5000;
SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
FROM Admin.SystemConfig WHERE ConfigKey = 'REFUND_APPROVAL_THRESHOLD';

INSERT INTO Billing.Refunds (..., Status, ...)
VALUES (...,
    CASE WHEN @Amount > @ApprovalThreshold THEN 'PENDING' ELSE 'APPROVED' END,
    ...
);
```

#### Enforcement Mechanism
- Type: SP logic (CASE statement comparing amount to configurable threshold)
- Behavior: Automatic approval routing based on amount

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-004 | Refund creation with threshold logic |
| SP-BIL-005 | Refund approval process |
| FT-BIL-004 | Refund Processing workflow |
| US-BIL-003 | Refund Processing |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Amount exactly $5,000 | Status = 'APPROVED' (condition is > not >=) |
| 2 | Amount $5,000.01 | Status = 'PENDING' |
| 3 | SystemConfig key missing | Default $5,000 threshold used |
| 4 | Threshold set to 0 | All refunds require approval (Amount > 0 always) |
| 5 | Threshold set to very large value | All refunds auto-approved |

---

### Rule ID: BR-BIL-010
**Module**: BIL
**Priority**: High

#### Rule Description
Commission amounts are calculated as ROUND(PremiumAmount * CommissionRate, 2). For REVERSAL and CHARGEBACK transaction types, the commission amount is negated (negative value). For EARNED, OVERRIDE, and BONUS types, the amount is positive. The AgentID is resolved from the policy record.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Commission_Create
- **Code Snippet**:
```sql
DECLARE @CommissionAmount DECIMAL(18,2) = ROUND(@PremiumAmount * @CommissionRate, 2);

-- Reversals and chargebacks are negative
IF @TransactionType IN ('REVERSAL', 'CHARGEBACK')
    SET @CommissionAmount = -ABS(@CommissionAmount);
```

#### Enforcement Mechanism
- Type: SP logic (calculation with sign adjustment based on transaction type)
- Behavior: Ensures reversals are always negative regardless of input sign

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-007 | Commission creation with sign logic |
| DV-BIL-004 | CommissionTransactions sign validation |
| US-BIL-009 | Commission Creation |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | REVERSAL with already-negative calculation | -ABS() ensures result is negative |
| 2 | CommissionRate = 0 | CommissionAmount = $0 (still creates record) |
| 3 | Very small CommissionRate (e.g., 0.0001) | ROUND to 2 decimals may yield $0.00 |
| 4 | Agent changed on policy after commission created | Original AgentID on commission; no auto-update |

---

### Rule ID: BR-BIL-011
**Module**: BIL
**Priority**: High

#### Rule Description
Returned payment (NSF) processing reverses the invoice application and adds an NSF fee. The payment status changes to RETURNED, the invoice PaidAmount is decreased by the original payment amount, and the BalanceDue is increased by the payment amount plus the NSF fee. The default NSF fee is $25.00. The invoice status is set to OVERDUE.

#### Source
- **File**: `database/02-stored-procedures/006-billing-sps.sql`
- **SP/Method**: Billing.usp_Payment_Return
- **Code Snippet**:
```sql
@NSFFee DECIMAL(10,2) = 25.00,
...
UPDATE Billing.PremiumPayments SET
    Status = 'RETURNED',
    ReturnedDate = CAST(GETDATE() AS DATE),
    ReturnReason = @ReturnReason,
    NSFFee = @NSFFee
WHERE PremiumPaymentID = @PremiumPaymentID;

-- Reverse invoice application
IF @InvoiceID IS NOT NULL
BEGIN
    UPDATE Billing.Invoices SET
        PaidAmount = PaidAmount - @Amount,
        BalanceDue = BalanceDue + @Amount + @NSFFee,
        TotalAmount = TotalAmount + @NSFFee,
        Status = 'OVERDUE',
        ModifiedDate = GETDATE()
    WHERE InvoiceID = @InvoiceID;
END
```

#### Enforcement Mechanism
- Type: SP logic (reversal + fee application in transaction)
- Behavior: Atomic reversal ensuring invoice balance and payment status are consistent

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-BIL-003 | Payment return processing |
| FT-BIL-007 | Returned Payment (NSF) Processing |
| US-BIL-008 | Returned Payment Processing |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | NSF fee of $0 | BalanceDue increases by payment amount only; TotalAmount unchanged |
| 2 | Payment not linked to invoice (InvoiceID=NULL) | Payment marked RETURNED but no invoice reversal |
| 3 | Return a payment that fully paid the invoice | Invoice goes from PAID (BalanceDue=0) to OVERDUE with original amount + NSF fee |
| 4 | Return already-returned payment | Error: 'Payment is not in APPLIED status: RETURNED' |
