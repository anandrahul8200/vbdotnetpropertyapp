# Billing Module - API Tests (Future-State)

## Module: BIL (Billing)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-BIL-001
**Legacy SP**: Billing.usp_Invoice_Generate
**Future Endpoint**: POST /api/billing/invoices
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Billing.usp_Invoice_Generate | POST /api/billing/invoices |
| Billing.usp_Billing_GetByPolicy | GET /api/billing/policies/{policyId} |
| Billing.usp_Payment_Record | POST /api/billing/payments |
| Billing.usp_Payment_Return | POST /api/billing/payments/{paymentId}/return |
| Billing.usp_Refund_Create | POST /api/billing/refunds |
| Billing.usp_Refund_Approve | POST /api/billing/refunds/{refundId}/approve |
| Billing.usp_Refund_GetPending | GET /api/billing/refunds?status=pending |
| Billing.usp_Invoice_ApplyLateFees | POST /api/billing/invoices/apply-late-fees |
| Billing.usp_Commission_Create | POST /api/billing/commissions |
| Billing.usp_Commission_GetStatement | GET /api/billing/commissions/statement?agentId=&from=&to= |
| Billing.usp_PaymentPlan_List | GET /api/billing/payment-plans |

#### Request/Response Specification
**Request (POST /api/billing/invoices):**
```json
{
  "policyId": 1,
  "invoiceType": "NEW_BUSINESS",
  "premiumAmount": 1200.00,
  "taxAmount": 50.00,
  "feeAmount": 25.00,
  "surchargeAmount": 10.00
}
```

**Success Response (201 Created):**
```json
{
  "invoices": [
    {
      "invoiceId": 1,
      "invoiceNumber": "INV0000001",
      "invoiceType": "NEW_BUSINESS",
      "invoiceDate": "2024-03-15",
      "dueDate": "2024-04-14",
      "totalAmount": 1285.00,
      "balanceDue": 1285.00,
      "status": "OPEN",
      "installmentNumber": 1,
      "totalInstallments": 1
    }
  ]
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "VALIDATION_ERROR",
  "message": "Policy not found: 99999",
  "field": "policyId"
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Generate invoice for annual plan | POST | /api/billing/invoices | 201 | Single invoice returned |
| 2 | Generate invoices for multi-pay plan | POST | /api/billing/invoices | 201 | Multiple invoices returned |
| 3 | Invalid policyId | POST | /api/billing/invoices | 400 | 'Policy not found' error |
| 4 | Missing required fields | POST | /api/billing/invoices | 400 | Validation error list |
| 5 | Negative premium amount | POST | /api/billing/invoices | 400 | Validation error |

---

### Test Case ID: API-BIL-002
**Legacy SP**: Billing.usp_Payment_Record
**Future Endpoint**: POST /api/billing/payments
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/billing/payments):**
```json
{
  "policyId": 1,
  "invoiceId": null,
  "amount": 500.00,
  "paymentMethod": "CHECK",
  "referenceNumber": "REF-001",
  "checkNumber": "10001",
  "bankName": "First National Bank",
  "paymentDate": "2024-03-15"
}
```

**Success Response (201 Created):**
```json
{
  "premiumPaymentId": 1,
  "paymentNumber": "PMP0000001",
  "receiptNumber": "RCP0000001",
  "status": "APPLIED",
  "invoiceId": 5,
  "invoiceBalanceRemaining": 0.00
}
```

**Error Response (404 Not Found):**
```json
{
  "error": "NOT_FOUND",
  "message": "Policy not found: 99999"
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Record payment (specific invoice) | POST | /api/billing/payments | 201 | Payment recorded, invoice updated |
| 2 | Record payment (auto-allocate) | POST | /api/billing/payments | 201 | Oldest open invoice selected |
| 3 | Overpayment | POST | /api/billing/payments | 201 | Overflow applied to next invoice |
| 4 | Invalid policy | POST | /api/billing/payments | 404 | Policy not found error |
| 5 | Invalid payment method | POST | /api/billing/payments | 400 | Validation error |
| 6 | Zero amount | POST | /api/billing/payments | 400 | Validation error |

---

### Test Case ID: API-BIL-003
**Legacy SP**: Billing.usp_Payment_Return
**Future Endpoint**: POST /api/billing/payments/{paymentId}/return
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/billing/payments/1/return):**
```json
{
  "returnReason": "NSF - Insufficient Funds",
  "nsfFee": 25.00
}
```

**Success Response (200 OK):**
```json
{
  "premiumPaymentId": 1,
  "status": "RETURNED",
  "returnedDate": "2024-03-20",
  "returnReason": "NSF - Insufficient Funds",
  "nsfFee": 25.00,
  "invoiceStatus": "OVERDUE",
  "invoiceNewBalance": 525.00
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Return applied payment | POST | /api/billing/payments/1/return | 200 | Payment marked RETURNED, invoice reversed |
| 2 | Return already-returned payment | POST | /api/billing/payments/1/return | 409 | Conflict: not in APPLIED status |
| 3 | Payment not found | POST | /api/billing/payments/99999/return | 404 | Not found error |
| 4 | Custom NSF fee | POST | /api/billing/payments/1/return | 200 | NSF fee applied as specified |

---

### Test Case ID: API-BIL-004
**Legacy SP**: Billing.usp_Refund_Create
**Future Endpoint**: POST /api/billing/refunds
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/billing/refunds):**
```json
{
  "policyId": 1,
  "refundType": "CANCELLATION",
  "refundMethod": "CHECK",
  "amount": 750.00,
  "calculationMethod": "PRO_RATA",
  "proRataFactor": 0.54794521,
  "earnedPremium": 450.00
}
```

**Success Response (201 Created):**
```json
{
  "refundId": 1,
  "refundNumber": "RFD0000001",
  "status": "APPROVED",
  "amount": 750.00,
  "calculationMethod": "PRO_RATA"
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Create refund under threshold | POST | /api/billing/refunds | 201 | Status=APPROVED |
| 2 | Create refund over threshold | POST | /api/billing/refunds | 201 | Status=PENDING |
| 3 | Invalid policy | POST | /api/billing/refunds | 404 | Policy not found |
| 4 | Invalid refund type | POST | /api/billing/refunds | 400 | Validation error |
| 5 | Negative amount | POST | /api/billing/refunds | 400 | Validation error |

---

### Test Case ID: API-BIL-005
**Legacy SP**: Billing.usp_Refund_Approve
**Future Endpoint**: POST /api/billing/refunds/{refundId}/approve
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/billing/refunds/1/approve):**
```json
{}
```

**Success Response (200 OK):**
```json
{
  "refundId": 1,
  "status": "APPROVED",
  "approvedBy": "manager",
  "approvedDate": "2024-03-20T10:00:00Z"
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Approve pending refund | POST | /api/billing/refunds/1/approve | 200 | Status changed to APPROVED |
| 2 | Approve non-pending refund | POST | /api/billing/refunds/1/approve | 409 | Conflict: not in PENDING status |
| 3 | Refund not found | POST | /api/billing/refunds/99999/approve | 404 | Not found error |

---

### Test Case ID: API-BIL-006
**Legacy SP**: Billing.usp_Billing_GetByPolicy
**Future Endpoint**: GET /api/billing/policies/{policyId}
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (GET /api/billing/policies/1):**
No request body.

**Success Response (200 OK):**
```json
{
  "policyId": 1,
  "policyNumber": "POL0000001",
  "grossPremium": 1500.00,
  "paymentPlan": "QUARTERLY",
  "summary": {
    "totalBilled": 1500.00,
    "totalPaid": 750.00,
    "totalOutstanding": 750.00
  },
  "invoices": [...],
  "payments": [...],
  "refunds": [...]
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Get billing for policy with data | GET | /api/billing/policies/1 | 200 | Full billing details returned |
| 2 | Get billing for new policy | GET | /api/billing/policies/2 | 200 | Summary with zeros, empty arrays |
| 3 | Policy not found | GET | /api/billing/policies/99999 | 404 | Not found error |

---

### Test Case ID: API-BIL-007
**Legacy SP**: Billing.usp_Commission_GetStatement
**Future Endpoint**: GET /api/billing/commissions/statement
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (GET /api/billing/commissions/statement?agentId=1&from=2024-01-01&to=2024-03-31):**
No request body.

**Success Response (200 OK):**
```json
{
  "agentId": 1,
  "agentNumber": "AGT001",
  "agentName": "John Smith",
  "agencyName": "Smith Insurance Agency",
  "period": {
    "from": "2024-01-01",
    "to": "2024-03-31"
  },
  "summary": {
    "totalEarned": 5000.00,
    "totalReversals": -200.00,
    "totalOverrides": 100.00,
    "totalBonus": 500.00,
    "netCommission": 5400.00
  },
  "transactions": [...]
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Get statement with transactions | GET | /api/billing/commissions/statement?agentId=1 | 200 | Summary and detail returned |
| 2 | Get statement with custom date range | GET | /api/billing/commissions/statement?agentId=1&from=...&to=... | 200 | Filtered by date |
| 3 | No transactions in period | GET | /api/billing/commissions/statement?agentId=1 | 200 | Zero totals, empty transactions |
| 4 | Missing agentId | GET | /api/billing/commissions/statement | 400 | Validation error |
