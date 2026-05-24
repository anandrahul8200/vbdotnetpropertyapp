# Underwriting Module - API Tests (Future-State)

## Module: UND (Underwriting)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-UND-001
**Legacy SP**: Underwriting.usp_Premium_Calculate
**Future Endpoint**: POST /api/underwriting/premium/calculate
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Underwriting.usp_Premium_Calculate | POST /api/underwriting/premium/calculate |
| Underwriting.usp_Premium_CalculateEndorsement | POST /api/underwriting/premium/endorsement |
| Underwriting.usp_Rate_GetBaseRate | GET /api/underwriting/rates/base |
| Underwriting.usp_Rate_GetFactor | GET /api/underwriting/rates/factor |
| Underwriting.usp_Rules_Evaluate | POST /api/underwriting/rules/evaluate |
| Underwriting.usp_Referral_Process | POST /api/underwriting/referrals/{id}/decision |
| Underwriting.usp_Referral_GetByPolicy | GET /api/underwriting/referrals?policyId={id} |
| Underwriting.usp_Referral_SearchPending | GET /api/underwriting/referrals/pending |
| Underwriting.usp_Moratorium_Create | POST /api/underwriting/moratoriums |
| Underwriting.usp_Moratorium_Lift | POST /api/underwriting/moratoriums/{id}/lift |
| Underwriting.usp_Moratorium_Check | GET /api/underwriting/moratoriums/check |
| Underwriting.usp_Moratorium_GetActive | GET /api/underwriting/moratoriums/active |
| Underwriting.usp_Worksheet_GetByPolicy | GET /api/underwriting/worksheets/{policyId} |
| Underwriting.usp_RateTable_Create | POST /api/underwriting/rate-tables |
| Underwriting.usp_RateTable_AddDetail | POST /api/underwriting/rate-tables/{id}/details |
| Underwriting.usp_RateTable_GetDetails | GET /api/underwriting/rate-tables/{id} |
| Underwriting.usp_Commission_Calculate | POST /api/underwriting/commissions/calculate |

#### Request/Response Specification
**Request (POST /api/underwriting/premium/calculate):**
```json
{
  "policyId": 1,
  "recalculateAll": true
}
```

**Success Response (200 OK):**
```json
{
  "policyId": 1,
  "totalPremium": 1500.00,
  "totalTaxes": 85.50,
  "grossPremium": 1585.50,
  "commissionAmount": 150.00,
  "coverages": [
    {
      "coverageCode": "DWELL",
      "insuredValue": 300000.00,
      "baseRate": 3.500000,
      "basePremium": 1050.00,
      "totalFactor": 0.945000,
      "calculatedPremium": 992.25,
      "minPremiumApplied": false,
      "finalPremium": 992.25
    }
  ]
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
| 1 | Calculate premium for valid policy | POST | /api/underwriting/premium/calculate | 200 | Premium breakdown returned |
| 2 | Policy not found | POST | /api/underwriting/premium/calculate | 404 | Not found error |
| 3 | Missing policyId | POST | /api/underwriting/premium/calculate | 400 | Validation error |
| 4 | Get rating worksheet | GET | /api/underwriting/worksheets/1 | 200 | Factor breakdown per coverage |
| 5 | Evaluate rules | POST | /api/underwriting/rules/evaluate | 200 | Triggered rules list |
| 6 | Search pending referrals | GET | /api/underwriting/referrals/pending | 200 | Paginated referral list |
| 7 | Process referral decision | POST | /api/underwriting/referrals/1/decision | 200 | Decision recorded |
| 8 | Check moratorium | GET | /api/underwriting/moratoriums/check?state=FL&zip=33101&type=HO3 | 200 | Moratorium status |
| 9 | Create moratorium | POST | /api/underwriting/moratoriums | 201 | Moratorium created |
| 10 | Lift moratorium | POST | /api/underwriting/moratoriums/1/lift | 200 | Moratorium lifted |
| 11 | Get active moratoriums | GET | /api/underwriting/moratoriums/active | 200 | List of active moratoriums |
| 12 | Calculate commission | POST | /api/underwriting/commissions/calculate | 200 | Commission amount returned |
| 13 | Get base rate | GET | /api/underwriting/rates/base?type=HO3&state=FL&construction=FRAME&protection=5&coverage=DWELL | 200 | Rate and min premium |
| 14 | Get factor | GET | /api/underwriting/rates/factor?type=CREDIT_SCORE&policyType=HO3&state=FL&rangeValue=720 | 200 | Factor value |

---

### Test Case ID: API-UND-002
**Legacy SP**: Underwriting.usp_Referral_Process
**Future Endpoint**: POST /api/underwriting/referrals/{id}/decision
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/underwriting/referrals/1/decision):**
```json
{
  "decision": "APPROVED",
  "notes": "Risk acceptable per guidelines",
  "conditions": null
}
```

**Success Response (200 OK):**
```json
{
  "referralId": 1,
  "policyId": 100,
  "decision": "APPROVED",
  "reviewedBy": "underwriter1",
  "reviewDate": "2024-03-15T10:30:00Z"
}
```

**Error Responses:**
```json
{
  "error": "NOT_FOUND",
  "message": "Referral not found: 99999"
}
```
```json
{
  "error": "INVALID_STATE",
  "message": "Referral is not in PENDING status. Current: APPROVED"
}
```

#### Test Cases
| # | Test | Method | URL | Status | Expected |
|---|------|--------|-----|--------|----------|
| 1 | Approve referral | POST | /api/underwriting/referrals/1/decision | 200 | Status=APPROVED |
| 2 | Decline referral | POST | /api/underwriting/referrals/1/decision | 200 | Status=DECLINED, policy declined |
| 3 | Conditional approval | POST | /api/underwriting/referrals/1/decision | 200 | Status=CONDITIONAL, conditions stored |
| 4 | Referral not found | POST | /api/underwriting/referrals/99999/decision | 404 | Not found error |
| 5 | Already processed | POST | /api/underwriting/referrals/1/decision | 409 | Conflict - not PENDING |
| 6 | Missing decision field | POST | /api/underwriting/referrals/1/decision | 400 | Validation error |
| 7 | Invalid decision value | POST | /api/underwriting/referrals/1/decision | 400 | Invalid enum value |
| 8 | CONDITIONAL without conditions | POST | /api/underwriting/referrals/1/decision | 400 | Conditions required |
