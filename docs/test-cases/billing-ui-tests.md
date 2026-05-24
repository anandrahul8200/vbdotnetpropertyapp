# Billing Module - UI Tests

## Module: BIL (Billing)
## Test Type: Screen-Level UI Tests
## Forms Covered (5 total):
1. frmBillingInquiry
2. frmPaymentEntry
3. frmRefundProcessing
4. frmCommissionStatement
5. frmPaymentPlanSetup

---

### Test Case ID: UI-BIL-001
**Form/Screen**: frmBillingInquiry
**File**: `src/PropertyInsuranceClaims/Forms/Billing/frmBillingInquiry.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Billing Inquiry', size 900x600, CenterParent |
| 2 | Policy number field visible | txtPolicyNumber visible and enabled |
| 3 | Lookup button visible | btnLookup with text '&Lookup' |
| 4 | TabControl with 3 tabs | Tabs: 'Invoices', 'Payments', 'Refunds' |
| 5 | DataGridViews configured | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect, AutoSizeColumnsMode=Fill |
| 6 | Action buttons initially disabled | btnRecordPayment.Enabled=False, btnCreateRefund.Enabled=False |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Form loaded (no policy looked up) | btnRecordPayment | Disabled |
| 2 | Form loaded (no policy looked up) | btnCreateRefund | Disabled |
| 3 | After successful lookup | btnRecordPayment | Enabled |
| 4 | After successful lookup | btnCreateRefund | Enabled |

#### Data Display Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Lookup with valid policy | Summary labels show Policy info, Billed, Paid, Outstanding (formatted as currency) |
| 2 | Lookup with valid policy | dgvInvoices bound to DataSet Tables(1) |
| 3 | Lookup with valid policy | dgvPayments bound to DataSet Tables(2) |
| 4 | Lookup with valid policy | dgvRefunds bound to DataSet Tables(3) |
| 5 | Outstanding label formatting | Bold font for emphasis |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Record Payment | Opens frmPaymentEntry with _policyID as parameter |
| 2 | Payment dialog returns OK | btnLookup.PerformClick() triggers refresh |
| 3 | Click Create Refund | MessageBox: 'Refund creation dialog would open here.' |
| 4 | Empty policy number, click Lookup | Returns immediately (no action) |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database error during Lookup | MessageBox: 'Error: {message}', ErrorLogger.LogError called |
| 2 | Cursor during lookup | WaitCursor during operation, Default after |

---

### Test Case ID: UI-BIL-002
**Form/Screen**: frmPaymentEntry
**File**: `src/PropertyInsuranceClaims/Forms/Billing/frmPaymentEntry.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Record Payment', size 450x350, CenterParent, FixedDialog, MaximizeBox=False |
| 2 | Amount field visible | txtAmount visible and empty |
| 3 | Payment date defaults to today | dtpPaymentDate.Value = today, Format=Short |
| 4 | Payment method dropdown populated | Items: CHECK, CREDIT_CARD, EFT, CASH, WIRE; SelectedIndex=0 (CHECK) |
| 5 | Check fields visible by default | txtCheckNumber.Visible=True, txtBankName.Visible=True |
| 6 | Credit card field hidden by default | txtCreditCardLast4.Visible=False |
| 7 | CreditCardLast4 MaxLength | MaxLength=4 |

#### Payment Method Toggle Tests
| # | Method Selected | txtCheckNumber | txtBankName | txtCreditCardLast4 |
|---|----------------|---------------|-------------|-------------------|
| 1 | CHECK | Visible | Visible | Hidden |
| 2 | CREDIT_CARD | Hidden | Hidden | Visible |
| 3 | EFT | Hidden | Visible | Hidden |
| 4 | CASH | Hidden | Hidden | Hidden |
| 5 | WIRE | Hidden | Hidden | Hidden |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|------------------------|
| 1 | Amount empty | Click Record | 'Enter a valid positive amount.' |
| 2 | Amount = 0 | Click Record | 'Enter a valid positive amount.' |
| 3 | Amount negative | Click Record | 'Enter a valid positive amount.' |
| 4 | Amount non-numeric | Click Record | 'Enter a valid positive amount.' |

#### Save Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Valid payment entry | BillingDataAccess.RecordPayment called with _policyID, amount, method, Nothing, referenceNumber, checkNumber |
| 2 | Successful save | MessageBox: 'Payment recorded successfully.', DialogResult=OK, form closes |
| 3 | Error during save | MessageBox: 'Error: {message}', form stays open, ErrorLogger.LogError called |
| 4 | Cancel clicked | DialogResult=Cancel, form closes |
| 5 | Cursor during save | WaitCursor set, Default restored in Finally |

---

### Test Case ID: UI-BIL-003
**Form/Screen**: frmRefundProcessing
**File**: `src/PropertyInsuranceClaims/Forms/Billing/frmRefundProcessing.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Refund Processing', size 900x650, CenterParent |
| 2 | Pending refunds grid loads | dgvPendingRefunds populated from Billing.usp_Refund_GetPending |
| 3 | Refund type dropdown | Items: CANCELLATION, ENDORSEMENT, OVERPAYMENT, DUPLICATE; SelectedIndex=0 |
| 4 | Refund method dropdown | Items: CHECK, EFT, CREDIT_CARD_REVERSAL; SelectedIndex=0 |
| 5 | Calculation method dropdown | Items: PRO_RATA, SHORT_RATE, FLAT; SelectedIndex=0 |
| 6 | Approve button color | BackColor=LightGreen |
| 7 | Void button color | BackColor=LightCoral |

#### Action Button Tests
| # | Button | Pre-condition | Expected Behavior |
|---|--------|---------------|-------------------|
| 1 | Approve | Row selected in grid | Billing.usp_Refund_Approve called with RefundID and CurrentUser |
| 2 | Approve | No row selected | Returns immediately (no action) |
| 3 | Issue | Row selected | MessageBox: 'Refund issued. Check number generated.' |
| 4 | Issue | No row selected | Returns immediately |
| 5 | Void | Row selected | Confirmation dialog: 'Void this refund?' |
| 6 | Void confirmed | User clicks Yes | MessageBox: 'Refund voided.', grid refreshed |
| 7 | Void cancelled | User clicks No | No action |
| 8 | Refresh | Any state | LoadPendingRefunds() called |
| 9 | Close | Any state | Form closes |

#### Create Refund Validation Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | No policy looked up (_policyID=0) | 'Please lookup a policy first.' |
| 2 | Invalid amount (empty) | 'Enter a valid positive amount.' |
| 3 | Invalid amount (zero) | 'Enter a valid positive amount.' |
| 4 | Invalid amount (negative) | 'Enter a valid positive amount.' |
| 5 | Valid creation | BillingDataAccess.CreateRefund called, success message, fields cleared, grid refreshed |

#### Policy Lookup Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Empty policy number | Returns immediately |
| 2 | Valid policy number | _policyID set, lblPolicyInfo shows info |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Error during create refund | MessageBox: 'Error: {message}', ErrorLogger.LogError called |
| 2 | Error during approve | MessageBox: 'Error: {message}' |
| 3 | Error loading pending refunds | ErrorLogger.LogError called (no user message) |

---

### Test Case ID: UI-BIL-004
**Form/Screen**: frmCommissionStatement
**File**: `src/PropertyInsuranceClaims/Forms/Billing/frmCommissionStatement.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Commission Statement', size 900x600, CenterParent |
| 2 | Agent dropdown | cboAgent with '(Select Agent)' default, DropDownList style |
| 3 | Period From defaults | First day of current month |
| 4 | Period To defaults | Today |
| 5 | Export button initially disabled | btnExport.Enabled=False |
| 6 | Summary labels empty | lblAgentName, lblTotalEarned, lblTotalReversals, lblNetCommission empty |
| 7 | Agent name label font | Font size 11, Bold |
| 8 | Net commission label font | Bold |

#### Generate Button Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Click Generate | BillingDataAccess.GetCommissionStatement called with agentID, periodFrom, periodTo |
| 2 | Data returned | Summary labels populated with formatted currency values |
| 3 | Detail data returned | dgvDetail.DataSource bound to Tables(1) |
| 4 | After generate | btnExport.Enabled=True |
| 5 | Error during generate | MessageBox: 'Error: {message}', ErrorLogger.LogError called |
| 6 | Cursor during generate | WaitCursor during operation, Default after |

#### Export Button Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Click Export | MessageBox: 'Export to CSV/Excel would happen here.' |

---

### Test Case ID: UI-BIL-005
**Form/Screen**: frmPaymentPlanSetup
**File**: `src/PropertyInsuranceClaims/Forms/Billing/frmPaymentPlanSetup.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Payment Plan Setup', size 900x700, CenterParent |
| 2 | Plan grid loads | dgvPlans populated from Billing.usp_PaymentPlan_List |
| 3 | PaymentPlanID column hidden | dgvPlans.Columns("PaymentPlanID").Visible=False |
| 4 | Preview premium default | txtPreviewPremium.Text='1500.00' |
| 5 | Preview grid columns | #, Due Date, Premium, Fee, Total Due, Cumulative |
| 6 | Three sections visible | Plan list (top), Plan details (middle), Preview (bottom) |

#### Plan Management Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click New | _editPlanID=0, fields cleared, txtPlanCode.Enabled=True, focus on PlanCode |
| 2 | Click Edit (row selected) | Fields populated from grid row, txtPlanCode.Enabled=False |
| 3 | Click Edit (no row) | Returns immediately |
| 4 | Click Deactivate (row selected) | Confirmation: 'Deactivate this plan?', Yes -> 'Plan deactivated.' |
| 5 | Click Deactivate (no row) | Returns immediately |
| 6 | Click Refresh | LoadPlans() called |
| 7 | Click Close | Form closes |

#### Save Validation Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Empty plan code | 'Plan code and name are required.' |
| 2 | Empty plan name | 'Plan code and name are required.' |
| 3 | Invalid installments (non-numeric) | 'Valid number of installments required.' |
| 4 | Installments < 1 | 'Valid number of installments required.' |
| 5 | Valid data | 'Payment plan saved.', grid refreshed |

#### Installment Preview Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Invalid premium (zero) | 'Enter a valid premium amount.' |
| 2 | Invalid premium (non-numeric) | 'Enter a valid premium amount.' |
| 3 | Valid preview (4 installments, 25% down, $5 fee) | 4 rows in grid: Row 1='1 (Down)' with down payment, Rows 2-4 with installments |
| 4 | Down payment calculation | DownPayment = ROUND(premium * downPct, 2) |
| 5 | Installment calculation | Each = ROUND(remaining / (numInstallments - 1), 2) + fee |
| 6 | Last installment adjustment | Last = remaining - (installmentAmount * (numInstallments - 2)) |
| 7 | Due dates | Each due date = today + (N-1) months + graceDays |
| 8 | Cumulative total | Running sum of all Total Due values |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Error during form load | MessageBox: 'Error: {message}', ErrorLogger.LogError called |
| 2 | Error during LoadPlans | ErrorLogger.LogError called (no user message) |
| 3 | Error generating preview | MessageBox: 'Error generating preview: {message}' |
