# Billing Module - Unit Tests

## Module: BIL (Billing)
## Test Type: Unit Tests for Data Access Layer
## Classes Covered:
- `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

---

### Test Case ID: UT-BIL-001
**Class**: BillingDataAccess
**Method**: GenerateInvoice(policyID, invoiceType, premiumAmount, taxAmount, feeAmount, surchargeAmount)
**File**: `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

#### Method Signature
```vb
Public Shared Sub GenerateInvoice(policyID As Integer, invoiceType As String, premiumAmount As Decimal,
                                  taxAmount As Decimal, feeAmount As Decimal, surchargeAmount As Decimal)
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |
| invoiceType | @InvoiceType | VARCHAR(20) |
| premiumAmount | @PremiumAmount | DECIMAL(18,2) |
| taxAmount | @TaxAmount | DECIMAL(18,2) |
| feeAmount | @FeeAmount | DECIMAL(18,2) |
| surchargeAmount | @SurchargeAmount | DECIMAL(18,2) |
| GlobalState.CurrentUser | @CreatedBy | VARCHAR(50) |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Billing.usp_Invoice_Generate' |
| 2 | Passes 7 parameters | Valid inputs | 7 SqlParameter objects in array |
| 3 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 4 | CurrentUser passed as CreatedBy | GlobalState.CurrentUser='admin' | @CreatedBy='admin' |
| 5 | All amount types passed | premiumAmount=1000, taxAmount=50, feeAmount=25, surchargeAmount=10 | Correct values in params |

---

### Test Case ID: UT-BIL-002
**Class**: BillingDataAccess
**Method**: RecordPayment(policyID, amount, paymentMethod, Optional invoiceID, Optional referenceNumber, Optional checkNumber) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function RecordPayment(policyID As Integer, amount As Decimal, paymentMethod As String,
                                     Optional invoiceID As Integer? = Nothing,
                                     Optional referenceNumber As String = Nothing,
                                     Optional checkNumber As String = Nothing) As Integer
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| invoiceID | @InvoiceID | INT (nullable) |
| policyID | @PolicyID | INT |
| amount | @Amount | DECIMAL(18,2) |
| paymentMethod | @PaymentMethod | VARCHAR(20) |
| referenceNumber | @ReferenceNumber | VARCHAR(50) |
| checkNumber | @CheckNumber | VARCHAR(20) |
| GlobalState.CurrentUser | @CreatedBy | VARCHAR(50) |
| (output) | @PremiumPaymentID | INT OUTPUT |
| (output) | @PaymentNumber | VARCHAR OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Billing.usp_Payment_Record' |
| 2 | Returns PremiumPaymentID from output param | Output param returns 42 | Function returns 42 |
| 3 | Output params created correctly | Mock DatabaseHelper | @PremiumPaymentID=SqlDbType.Int, @PaymentNumber=SqlDbType.VarChar |
| 4 | Optional params default to Nothing | invoiceID=Nothing, referenceNumber=Nothing, checkNumber=Nothing | Params passed with Nothing values |
| 5 | All params passed when provided | invoiceID=5, referenceNumber='REF001', checkNumber='CHK001' | Values correctly set on SqlParameter objects |
| 6 | Total parameter count | All provided | 9 parameters (7 input + 2 output) |
| 7 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 8 | CInt conversion of output | Output param value | CInt(paymentIDParam.Value) returned |

---

### Test Case ID: UT-BIL-003
**Class**: BillingDataAccess
**Method**: GetBillingByPolicy(policyID As Integer) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetBillingByPolicy(policyID As Integer) As DataSet
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Billing.usp_Billing_GetByPolicy' |
| 2 | Returns DataSet | Mock returns DataSet | Function returns DataSet object |
| 3 | Single parameter passed | policyID=100 | 1 SqlParameter with value 100 |
| 4 | ExecuteDataSet called | Valid input | DatabaseHelper.ExecuteDataSet invoked |

---

### Test Case ID: UT-BIL-004
**Class**: BillingDataAccess
**Method**: CreateRefund(policyID, refundType, amount, Optional calculationMethod) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CreateRefund(policyID As Integer, refundType As String, amount As Decimal,
                                    Optional calculationMethod As String = "PRO_RATA") As Integer
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |
| refundType | @RefundType | VARCHAR(20) |
| amount | @Amount | DECIMAL(18,2) |
| calculationMethod | @CalculationMethod | VARCHAR(20) |
| GlobalState.CurrentUser | @CreatedBy | VARCHAR(50) |
| (output) | @RefundID | INT OUTPUT |
| (output) | @RefundNumber | VARCHAR OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Billing.usp_Refund_Create' |
| 2 | Returns RefundID from output param | Output param returns 10 | Function returns 10 |
| 3 | Default calculationMethod is PRO_RATA | calculationMethod not provided | @CalculationMethod='PRO_RATA' |
| 4 | Custom calculationMethod passed | calculationMethod='SHORT_RATE' | @CalculationMethod='SHORT_RATE' |
| 5 | Output params created | Mock DatabaseHelper | @RefundID=SqlDbType.Int, @RefundNumber=SqlDbType.VarChar |
| 6 | Total parameter count | All provided | 7 parameters (5 input + 2 output) |
| 7 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 8 | CInt conversion of output | Output param value | CInt(refundIDParam.Value) returned |

---

### Test Case ID: UT-BIL-005
**Class**: BillingDataAccess
**Method**: GetCommissionStatement(agentID, Optional periodFrom, Optional periodTo) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetCommissionStatement(agentID As Integer, Optional periodFrom As Date? = Nothing,
                                               Optional periodTo As Date? = Nothing) As DataSet
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| agentID | @AgentID | INT |
| periodFrom | @PeriodFrom | DATE (nullable) |
| periodTo | @PeriodTo | DATE (nullable) |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Billing.usp_Commission_GetStatement' |
| 2 | Returns DataSet | Mock returns DataSet | Function returns DataSet object |
| 3 | Three parameters passed | agentID=1, periodFrom=date1, periodTo=date2 | 3 SqlParameter objects |
| 4 | Optional params default to Nothing | periodFrom=Nothing, periodTo=Nothing | @PeriodFrom and @PeriodTo with Nothing values |
| 5 | ExecuteDataSet called | Valid input | DatabaseHelper.ExecuteDataSet invoked |
| 6 | Nullable date handling | periodFrom=Nothing | SqlParameter value handled correctly for nullable Date |
