# Billing Module - Functional Tests

## Module: BIL (Billing)
## Test Type: End-to-End Functional Tests

---

### Test Case ID: FT-BIL-001
**Workflow**: Invoice Generation (Annual Pay)
**Priority**: Critical

#### Preconditions
- Active policy exists with PaymentPlan='ANNUAL'
- Premium amount calculated

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call BillingDataAccess.GenerateInvoice with valid policyID | SP Billing.usp_Invoice_Generate called |
| 2 | Verify InvoiceType='NEW_BUSINESS' | InvoiceType set correctly |
| 3 | Verify single invoice created | Only 1 invoice record for ANNUAL plan |
| 4 | Verify InvoiceNumber format | Starts with 'INV' + 7 digits |
| 5 | Verify TotalAmount | PremiumAmount + TaxAmount + FeeAmount + SurchargeAmount |
| 6 | Verify DueDate | EffectiveDate + GracePeriodDays (default 30) |
| 7 | Verify Status='OPEN' | New invoice is OPEN |
| 8 | Verify BalanceDue = TotalAmount | No payment applied yet |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Generate invoice for non-existent policy | Error: 'Policy not found: {ID}' |
| 2 | Generate invoice with invalid InvoiceType | Error or unexpected behavior [ASSUMPTION] |

---

### Test Case ID: FT-BIL-002
**Workflow**: Invoice Generation (Multi-Pay Installment Plan)
**Priority**: Critical

#### Preconditions
- Active policy exists with PaymentPlan matching a Billing.PaymentPlans record (e.g., QUARTERLY with 4 installments, 25% down)
- PaymentPlan is active

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call GenerateInvoice with PolicyID (multi-pay policy) | SP Billing.usp_Invoice_Generate called |
| 2 | Verify PaymentPlan looked up | NumInstallments, DownPaymentPct, InstallmentFee, GracePeriodDays from plan |
| 3 | Verify first invoice (down payment) | TotalAmount = DownPayment + FeeAmount, InstallmentNumber=1, TotalInstallments=N |
| 4 | Verify down payment calculation | DownPayment = ROUND(TotalAmount * DownPaymentPct, 2) |
| 5 | Verify installment invoices created | (N-1) additional invoices with InvoiceType='INSTALLMENT' |
| 6 | Verify installment amount | Each = ROUND(RemainingAmount / (N-1), 2) + InstallmentFee |
| 7 | Verify last installment rounding | Last = RemainingAmount - (InstallmentAmount * (N-2)) to avoid rounding issues |
| 8 | Verify installment dates | Each InvoiceDate = EffectiveDate + (InstallmentNum - 1) months |
| 9 | Verify all invoices have unique InvoiceNumbers | INV+7 digit format, all unique |
| 10 | Verify audit log created | AuditLog row with Action='INSERT' |

---

### Test Case ID: FT-BIL-003
**Workflow**: Payment Recording and Invoice Allocation
**Priority**: Critical

#### Preconditions
- Policy with open invoice(s)
- Invoice has BalanceDue > 0

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmPaymentEntry with policyID | Form loads with fields for amount, date, method |
| 2 | Enter Amount=$500, Method='CHECK', CheckNumber='10001' | Fields populated |
| 3 | Click Record | BillingDataAccess.RecordPayment() called |
| 4 | Verify PaymentNumber generated | Format PMP+7 digits |
| 5 | Verify ReceiptNumber generated | Format RCP+7 digits |
| 6 | Verify payment status | Status='APPLIED' |
| 7 | Verify invoice updated | PaidAmount increased, BalanceDue decreased |
| 8 | If payment > balance, verify overflow to next invoice | Next invoice balance reduced |
| 9 | Success message displayed | 'Payment recorded successfully.' |
| 10 | Dialog closes with OK result | frmBillingInquiry refreshes |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter invalid amount (zero or negative) | Validation: 'Enter a valid positive amount.' |
| 2 | Enter non-numeric amount | Decimal.TryParse fails, validation message shown |
| 3 | Database error during record | MessageBox with error message, form remains open |

---

### Test Case ID: FT-BIL-004
**Workflow**: Refund Processing (Creation to Approval)
**Priority**: Critical

#### Preconditions
- Policy exists with overpayment or cancellation
- User has refund creation permission

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmRefundProcessing | Form loads, pending refunds grid populated |
| 2 | Enter policy number, click Lookup | Policy info displayed |
| 3 | Select RefundType='CANCELLATION' | Type selected |
| 4 | Select CalculationMethod='PRO_RATA' | Method selected (default) |
| 5 | Enter Amount=$1500 | Amount field populated |
| 6 | Click Create Refund | BillingDataAccess.CreateRefund() called |
| 7 | Verify RefundNumber generated | Format RFD+7 digits |
| 8 | Verify status based on threshold | If Amount <= $5000: APPROVED. If Amount > $5000: PENDING |
| 9 | Success message displayed | 'Refund created (ID: {refundID}).' |
| 10 | Pending refunds grid refreshed | New refund appears in grid |
| 11 | Select pending refund, click Approve | Billing.usp_Refund_Approve called |
| 12 | Verify refund approved | Status='APPROVED', ApprovedBy set, ApprovedDate set |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create refund without looking up policy | Validation: 'Please lookup a policy first.' |
| 2 | Enter invalid amount | Validation: 'Enter a valid positive amount.' |
| 3 | Approve non-PENDING refund | Error: 'Refund is not in PENDING status: {status}' |

---

### Test Case ID: FT-BIL-005
**Workflow**: Commission Statement Generation
**Priority**: High

#### Preconditions
- Agent exists with commission transactions in period
- Commission transactions have various types (EARNED, REVERSAL, OVERRIDE, BONUS)

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmCommissionStatement | Form loads, agent dropdown populated |
| 2 | Select agent from dropdown | Agent selected |
| 3 | Set PeriodFrom and PeriodTo dates | Date range configured |
| 4 | Click Generate | BillingDataAccess.GetCommissionStatement() called |
| 5 | Verify summary displayed | AgentName, TotalEarned, TotalReversals, NetCommission labels populated |
| 6 | Verify detail grid populated | DataGridView bound to transaction details |
| 7 | Verify export button enabled | btnExport.Enabled = True after data loaded |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Generate for agent with no data | Empty result set, labels show $0 values |
| 2 | Database error during generate | MessageBox with error, ErrorLogger called |

---

### Test Case ID: FT-BIL-006
**Workflow**: Late Fee Application (Batch Process)
**Priority**: High

#### Preconditions
- Open invoices exist with DueDate < current date
- Invoices have LateFeeApplied = 0 and BalanceDue > 0

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Execute Billing.usp_Invoice_ApplyLateFees with @AsOfDate=today | Batch process runs |
| 2 | Verify overdue invoices identified | Only invoices with Status='OPEN', DueDate < AsOfDate, LateFeeApplied=0, BalanceDue > 0 |
| 3 | Verify late fee amount | From PaymentPlan's LateFeeAmount or DEFAULT_LATE_FEE from SystemConfig |
| 4 | Verify invoice updated | Status='OVERDUE', LateFeeApplied=1, TotalAmount increased, BalanceDue increased |
| 5 | Verify @InvoicesProcessed output | Count of affected invoices |

---

### Test Case ID: FT-BIL-007
**Workflow**: Returned Payment (NSF) Processing
**Priority**: High

#### Preconditions
- Payment exists with Status='APPLIED' and linked to an invoice

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call Billing.usp_Payment_Return with valid PremiumPaymentID | Return process executes |
| 2 | Verify payment status | Status='RETURNED', ReturnedDate set, ReturnReason stored |
| 3 | Verify NSF fee applied | NSFFee=$25.00 (default) on payment record |
| 4 | Verify invoice reversed | PaidAmount decreased by payment Amount |
| 5 | Verify invoice balance increased | BalanceDue increased by Amount + NSFFee |
| 6 | Verify invoice status | Status='OVERDUE' |
| 7 | Verify audit trail | AuditLog row with OldValue='APPLIED', NewValue='RETURNED' |

---

### Test Case ID: FT-BIL-008
**Workflow**: Billing Inquiry Lookup
**Priority**: High

#### Preconditions
- Policy exists with billing history (invoices, payments, refunds)

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmBillingInquiry | Form loads with policy lookup field |
| 2 | Enter policy number, click Lookup | BillingDataAccess.GetBillingByPolicy() called |
| 3 | Verify summary labels | Policy info, TotalBilled, TotalPaid, Outstanding displayed |
| 4 | Verify Invoices tab | dgvInvoices populated with invoice records |
| 5 | Verify Payments tab | dgvPayments populated with payment records |
| 6 | Verify Refunds tab | dgvRefunds populated with refund records |
| 7 | Verify action buttons enabled | btnRecordPayment and btnCreateRefund enabled |
| 8 | Click Record Payment | frmPaymentEntry opens with policyID |
| 9 | Payment dialog returns OK | Billing inquiry refreshes (btnLookup.PerformClick) |
