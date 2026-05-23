# Claims Module - API Tests (Future-State)

## Module: CLM (Claims)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-CLM-001
**Legacy SP**: Claims.usp_Claim_Create
**Future Endpoint**: POST /api/claims
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Claim_Create | POST /api/claims |
| Claims.usp_Claim_GetDetails | GET /api/claims/{id} |
| Claims.usp_Claim_UpdateStatus | PATCH /api/claims/{id}/status |
| Claims.usp_Claim_Search | GET /api/claims?claimNumber=&status=&type= |
| Claims.usp_Claim_Assign | POST /api/claims/{id}/assignments |
| Claims.usp_Claim_GetDashboard | GET /api/claims/dashboard |

#### Request/Response Specification
**Request (POST /api/claims):**
```json
{
  "policyId": 1,
  "claimType": "FIRE",
  "lossDate": "2024-03-15T14:30:00",
  "lossDescription": "Kitchen fire caused by electrical fault",
  "lossLocation": "123 Main St, Austin TX",
  "estimatedLoss": 50000.00,
  "policeReportNumber": "APD-2024-12345",
  "fireReportNumber": "AFD-2024-67890",
  "weatherCondition": "CLEAR",
  "priority": "HIGH",
  "catastropheId": null
}
```

**Success Response (201 Created):**
```json
{
  "claimId": 1,
  "claimNumber": "CLM0000001",
  "claimStatus": "FNOL",
  "complexity": "MODERATE",
  "createdDate": "2024-03-15T15:00:00Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "VALIDATION_ERROR",
  "message": "Loss date is outside the policy period",
  "field": "lossDate"
}
```

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/claims | Valid FNOL | 201 Created |
| 2 | POST | /api/claims | Invalid policy | 404 Not Found |
| 3 | POST | /api/claims | Inactive policy | 422 Unprocessable |
| 4 | POST | /api/claims | Loss date outside period | 400 Bad Request |
| 5 | GET | /api/claims/1 | Existing claim | 200 OK |
| 6 | GET | /api/claims/99999 | Non-existent | 404 Not Found |
| 7 | PATCH | /api/claims/1/status | Valid transition | 200 OK |
| 8 | PATCH | /api/claims/1/status | Invalid transition | 422 Unprocessable |

---

### Test Case ID: API-CLM-002
**Legacy SP**: Claims.usp_Claim_SetReserve
**Future Endpoint**: POST /api/claims/{id}/reserves
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Claim_SetReserve | POST /api/claims/{id}/reserves |

#### Request/Response Specification
**Request (POST /api/claims/1/reserves):**
```json
{
  "reserveType": "CASE",
  "reserveCategory": "INDEMNITY",
  "amount": 50000.00,
  "changeReason": "Initial reserve based on inspection"
}
```

**Success Response (201 Created):**
```json
{
  "reserveId": 1,
  "amount": 50000.00,
  "approvalRequired": false,
  "isApproved": true
}
```

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/claims/1/reserves | Amount <= $50K | 201, approvalRequired=false |
| 2 | POST | /api/claims/1/reserves | Amount > $50K | 201, approvalRequired=true |
| 3 | POST | /api/claims/1/reserves | Closed claim | 422 Unprocessable |

---

### Test Case ID: API-CLM-003
**Legacy SP**: Claims.usp_Claim_CreatePayment, ApprovePayment, VoidPayment
**Future Endpoint**: POST /api/claims/{id}/payments
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Claim_CreatePayment | POST /api/claims/{id}/payments |
| Claims.usp_Claim_ApprovePayment | POST /api/payments/{id}/approve |
| Claims.usp_Claim_VoidPayment | POST /api/payments/{id}/void |

#### Request/Response Specification
**Request (POST /api/claims/1/payments):**
```json
{
  "paymentType": "INDEMNITY",
  "paymentMethod": "CHECK",
  "payeeType": "INSURED",
  "payeeName": "John Doe",
  "amount": 5000.00,
  "description": "Partial payment for repairs",
  "taxReportable": false
}
```

**Success Response (201 Created):**
```json
{
  "paymentId": 1,
  "paymentNumber": "PAY0000001",
  "status": "APPROVED",
  "approvalRequired": false,
  "form1099Required": false
}
```

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/claims/1/payments | Amount <= $10K | 201, status=APPROVED |
| 2 | POST | /api/claims/1/payments | Amount > $10K | 201, status=PENDING |
| 3 | POST | /api/claims/1/payments | Exceeds policy limit | 422 Unprocessable |
| 4 | POST | /api/claims/1/payments | FNOL claim | 422 Unprocessable |
| 5 | POST | /api/payments/1/approve | Pending payment | 200 OK |
| 6 | POST | /api/payments/1/void | Approved payment | 200 OK |
| 7 | POST | /api/payments/1/void | Pending payment | 422 Unprocessable |

---

### Test Case ID: API-CLM-004
**Legacy SP**: Claims.usp_Fraud_EvaluateClaim, usp_Fraud_ReferToSIU
**Future Endpoint**: POST /api/claims/{id}/fraud/evaluate
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Fraud_EvaluateClaim | POST /api/claims/{id}/fraud/evaluate |
| Claims.usp_Fraud_ReferToSIU | POST /api/claims/{id}/fraud/refer-siu |
| Claims.usp_Fraud_GetEvaluation | GET /api/claims/{id}/fraud |

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/claims/1/fraud/evaluate | Valid claim | 200, returns fraudScore |
| 2 | POST | /api/claims/1/fraud/refer-siu | With reason | 200 OK |
| 3 | GET | /api/claims/1/fraud | Evaluated claim | 200, indicators listed |

---

### Test Case ID: API-CLM-005
**Legacy SP**: Claims.usp_Subrogation_*
**Future Endpoint**: /api/claims/{id}/subrogation
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Subrogation_Create | POST /api/claims/{id}/subrogation |
| Claims.usp_Subrogation_UpdateStatus | PATCH /api/subrogation/{id}/status |
| Claims.usp_Subrogation_RecordRecovery | POST /api/subrogation/{id}/recovery |
| Claims.usp_Subrogation_GetByClaim | GET /api/claims/{id}/subrogation |

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/claims/1/subrogation | Valid creation | 201 Created |
| 2 | PATCH | /api/subrogation/1/status | Update to DEMAND_SENT | 200 OK |
| 3 | POST | /api/subrogation/1/recovery | Record $5000 | 200 OK |

---

### Test Case ID: API-CLM-006
**Legacy SP**: Claims.usp_Catastrophe_*
**Future Endpoint**: /api/catastrophes
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Catastrophe_Create | POST /api/catastrophes |
| Claims.usp_Catastrophe_LinkClaim | POST /api/catastrophes/{id}/claims |
| Claims.usp_Catastrophe_GetSummary | GET /api/catastrophes/{id}/summary |
| Claims.usp_Catastrophe_GetAll | GET /api/catastrophes |
| Claims.usp_Catastrophe_Close | POST /api/catastrophes/{id}/close |

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/catastrophes | Valid creation | 201 Created |
| 2 | POST | /api/catastrophes/1/claims | Link valid claim | 200 OK |
| 3 | POST | /api/catastrophes/1/claims | Inactive catastrophe | 422 Unprocessable |
| 4 | GET | /api/catastrophes | Active only | 200, filtered list |
| 5 | POST | /api/catastrophes/1/close | Close active | 200 OK |

---

### Test Case ID: API-CLM-007
**Legacy SP**: Claims.usp_Vendor_*
**Future Endpoint**: /api/vendors
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Claims.usp_Vendor_Create | POST /api/vendors |
| Claims.usp_Vendor_Search | GET /api/vendors?name=&type=&state= |

#### API Test Cases
| # | Method | Path | Scenario | Expected Status |
|---|--------|------|----------|-----------------|
| 1 | POST | /api/vendors | Valid creation | 201 Created |
| 2 | GET | /api/vendors?type=ADJUSTER | Filter by type | 200, filtered list |
| 3 | GET | /api/vendors?preferredOnly=true | Preferred vendors | 200, preferred only |
