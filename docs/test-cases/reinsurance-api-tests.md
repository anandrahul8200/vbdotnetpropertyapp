# Reinsurance Module - API Tests (Future-State)

## Module: RNS (Reinsurance)
## Source Files:
- `database/02-stored-procedures/007-reinsurance-sps.sql`
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality as REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-RNS-001
**Legacy SP**: Reinsurance.usp_Treaty_Create
**Future Endpoint**: POST /api/reinsurance/treaties
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Treaty_Create | POST /api/reinsurance/treaties |
| SP: Reinsurance.usp_Treaty_GetActive | GET /api/reinsurance/treaties?status=active |
| SP: Reinsurance.usp_Treaty_GetSummary | GET /api/reinsurance/treaties/{id}/summary |

#### Request/Response Specification
**Request (POST /api/reinsurance/treaties):**
```json
{
  "treatyName": "QS Treaty 2024 (mapped from @TreatyName)",
  "treatyType": "QUOTA_SHARE (mapped from @TreatyType)",
  "reinsurerID": 1,
  "effectiveDate": "2024-01-01 (mapped from @EffectiveDate)",
  "expiryDate": "2024-12-31 (mapped from @ExpiryDate)",
  "retentionAmount": 500000.00,
  "retentionPercent": 0.60,
  "cessionPercent": 0.40,
  "cessionLimit": 2000000.00,
  "attachmentPoint": null,
  "exhaustionPoint": null,
  "premiumRate": 0.025,
  "minimumPremium": 50000.00,
  "depositPremium": 25000.00,
  "commissionRate": 0.30,
  "coveredPerils": "FIRE,WIND,HAIL",
  "coveredStates": "FL,TX,CA",
  "coveredPolicyTypes": "HO3,HO5"
}
```

**Success Response (201 Created):**
```json
{
  "treatyID": 1,
  "treatyNumber": "TRY0000001",
  "treatyName": "QS Treaty 2024",
  "treatyType": "QUOTA_SHARE",
  "status": "ACTIVE",
  "createdDate": "2024-01-01T00:00:00Z"
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Create valid quota share treaty | POST | /api/reinsurance/treaties | {valid QS data} | 201 Created | {treatyID, treatyNumber} |
| 2 | Create valid surplus treaty | POST | /api/reinsurance/treaties | {valid SURPLUS data with retentionAmount} | 201 Created | {treatyID, treatyNumber} |
| 3 | Create valid XOL treaty | POST | /api/reinsurance/treaties | {valid EXCESS_OF_LOSS with attachment/exhaustion} | 201 Created | {treatyID, treatyNumber} |
| 4 | Missing required treatyName | POST | /api/reinsurance/treaties | {treatyName: null} | 400 Bad Request | {error: "treatyName is required"} |
| 5 | Invalid treaty type | POST | /api/reinsurance/treaties | {treatyType: "INVALID"} | 400 Bad Request | {error: "Invalid treaty type"} |
| 6 | Invalid reinsurer ID | POST | /api/reinsurance/treaties | {reinsurerID: 99999} | 404 Not Found | {error: "Reinsurer not found"} |
| 7 | Effective date after expiry | POST | /api/reinsurance/treaties | {effectiveDate > expiryDate} | 400 Bad Request | {error: "Effective date must be before expiry date"} |
| 8 | Unauthorized access | POST | /api/reinsurance/treaties | {valid} | 401 Unauthorized | {error: "Authentication required"} |
| 9 | Insufficient permissions | POST | /api/reinsurance/treaties | {valid, read-only user} | 403 Forbidden | {error: "Insufficient permissions"} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Underwriter or Admin

---

### Test Case ID: API-RNS-002
**Legacy SP**: Reinsurance.usp_Treaty_GetActive
**Future Endpoint**: GET /api/reinsurance/treaties
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Treaty_GetActive (@AsOfDate) | GET /api/reinsurance/treaties?status=active&asOfDate=2024-06-15 |

#### Request/Response Specification
**Request (GET /api/reinsurance/treaties?status=active):**
Query parameters: status (optional), asOfDate (optional, defaults to today)

**Success Response (200 OK):**
```json
{
  "treaties": [
    {
      "treatyID": 1,
      "treatyNumber": "TRY0000001",
      "treatyName": "QS Treaty 2024",
      "treatyType": "QUOTA_SHARE",
      "reinsurerName": "Munich Re",
      "amBestRating": "A+",
      "effectiveDate": "2024-01-01",
      "expiryDate": "2024-12-31",
      "totalCededPremium": 500000.00,
      "totalCededLoss": 125000.00
    }
  ],
  "totalCount": 1
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Get all active treaties | GET | /api/reinsurance/treaties?status=active | -- | 200 OK | {treaties: [...]} |
| 2 | Get treaties for specific date | GET | /api/reinsurance/treaties?asOfDate=2024-06-15 | -- | 200 OK | {treaties filtered by date} |
| 3 | No active treaties | GET | /api/reinsurance/treaties?status=active | -- | 200 OK | {treaties: [], totalCount: 0} |
| 4 | Invalid date format | GET | /api/reinsurance/treaties?asOfDate=invalid | -- | 400 Bad Request | {error: "Invalid date format"} |
| 5 | Unauthorized | GET | /api/reinsurance/treaties | -- | 401 Unauthorized | {error: "Authentication required"} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Underwriter, Admin, or Viewer

---

### Test Case ID: API-RNS-003
**Legacy SP**: Reinsurance.usp_Treaty_GetSummary
**Future Endpoint**: GET /api/reinsurance/treaties/{id}/summary
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Treaty_GetSummary (@TreatyID) | GET /api/reinsurance/treaties/{id}/summary |

#### Request/Response Specification
**Success Response (200 OK):**
```json
{
  "treaty": {
    "treatyID": 1,
    "treatyNumber": "TRY0000001",
    "treatyName": "QS Treaty 2024",
    "reinsurerName": "Munich Re",
    "amBestRating": "A+",
    "spRating": "AA-"
  },
  "cessionSummary": [
    {"cessionType": "PREMIUM", "transactionCount": 150, "totalGross": 1500000, "totalCeded": 600000, "totalRetained": 900000},
    {"cessionType": "LOSS", "transactionCount": 25, "totalGross": 500000, "totalCeded": 200000, "totalRetained": 300000}
  ],
  "bordereaux": [
    {"bordereauxID": 5, "reportingPeriod": "2024-06", "reportType": "PREMIUM", "status": "SUBMITTED"}
  ],
  "recentCessions": [...]
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Get summary for valid treaty | GET | /api/reinsurance/treaties/1/summary | -- | 200 OK | {full summary} |
| 2 | Treaty not found | GET | /api/reinsurance/treaties/99999/summary | -- | 404 Not Found | {error: "Treaty not found"} |
| 3 | Treaty with no cessions | GET | /api/reinsurance/treaties/2/summary | -- | 200 OK | {treaty: {...}, cessionSummary: [], recentCessions: []} |
| 4 | Unauthorized | GET | /api/reinsurance/treaties/1/summary | -- | 401 Unauthorized | {error} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Underwriter or Admin

---

### Test Case ID: API-RNS-004
**Legacy SP**: Reinsurance.usp_Cession_CalculatePremium
**Future Endpoint**: POST /api/reinsurance/cessions/calculate-premium
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Cession_CalculatePremium | POST /api/reinsurance/cessions/calculate-premium |

#### Request/Response Specification
**Request:**
```json
{
  "policyID": 100,
  "grossPremium": 10000.00
}
```

**Success Response (200 OK):**
```json
{
  "cessions": [
    {
      "cessionID": 1,
      "treatyID": 1,
      "treatyNumber": "TRY0000001",
      "cessionType": "PREMIUM",
      "grossAmount": 10000.00,
      "cededAmount": 4000.00,
      "retainedAmount": 6000.00,
      "cessionPercent": 0.40
    }
  ],
  "totalGross": 10000.00,
  "totalCeded": 4000.00,
  "totalRetained": 6000.00
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Calculate premium cession | POST | /api/reinsurance/cessions/calculate-premium | {policyID: 100, grossPremium: 10000} | 200 OK | {cessions: [...]} |
| 2 | Invalid policy ID | POST | /api/reinsurance/cessions/calculate-premium | {policyID: 99999} | 404 Not Found | {error: "Policy not found"} |
| 3 | No matching treaties | POST | /api/reinsurance/cessions/calculate-premium | {policyID: valid, no treaties match} | 200 OK | {cessions: [], totalCeded: 0} |
| 4 | Zero premium | POST | /api/reinsurance/cessions/calculate-premium | {grossPremium: 0} | 400 Bad Request | {error: "Gross premium must be > 0"} |
| 5 | Negative premium | POST | /api/reinsurance/cessions/calculate-premium | {grossPremium: -100} | 400 Bad Request | {error: "Invalid premium amount"} |
| 6 | Unauthorized | POST | /api/reinsurance/cessions/calculate-premium | {valid} | 401 Unauthorized | {error} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Underwriter or Admin

---

### Test Case ID: API-RNS-005
**Legacy SP**: Reinsurance.usp_Cession_CalculateLoss
**Future Endpoint**: POST /api/reinsurance/cessions/calculate-loss
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Cession_CalculateLoss | POST /api/reinsurance/cessions/calculate-loss |

#### Request/Response Specification
**Request:**
```json
{
  "claimID": 50,
  "lossAmount": 250000.00
}
```

**Success Response (200 OK):**
```json
{
  "cessions": [
    {
      "cessionID": 10,
      "treatyID": 3,
      "treatyNumber": "TRY0000003",
      "cessionType": "LOSS",
      "grossAmount": 250000.00,
      "cededAmount": 150000.00,
      "retainedAmount": 100000.00,
      "cessionPercent": 0.60
    }
  ],
  "totalGross": 250000.00,
  "totalCeded": 150000.00,
  "totalRetained": 100000.00
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Calculate loss cession | POST | /api/reinsurance/cessions/calculate-loss | {claimID: 50, lossAmount: 250000} | 200 OK | {cessions: [...]} |
| 2 | Invalid claim ID | POST | /api/reinsurance/cessions/calculate-loss | {claimID: 99999} | 404 Not Found | {error: "Claim not found"} |
| 3 | Loss below XOL attachment | POST | /api/reinsurance/cessions/calculate-loss | {lossAmount < attachmentPoint} | 200 OK | {cessions: [], totalCeded: 0} |
| 4 | Zero loss amount | POST | /api/reinsurance/cessions/calculate-loss | {lossAmount: 0} | 400 Bad Request | {error: "Loss amount must be > 0"} |
| 5 | Unauthorized | POST | /api/reinsurance/cessions/calculate-loss | {valid} | 401 Unauthorized | {error} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Claims Adjuster, Underwriter, or Admin

---

### Test Case ID: API-RNS-006
**Legacy SP**: Reinsurance.usp_Bordereaux_Generate
**Future Endpoint**: POST /api/reinsurance/bordereaux/generate
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: Reinsurance.usp_Bordereaux_Generate | POST /api/reinsurance/bordereaux/generate |
| DataAccess: GetBordereaux | GET /api/reinsurance/treaties/{id}/bordereaux |

#### Request/Response Specification
**Request (POST /api/reinsurance/bordereaux/generate):**
```json
{
  "treatyID": 1,
  "reportingPeriod": "2024-06",
  "reportType": "PREMIUM"
}
```

**Success Response (201 Created):**
```json
{
  "bordereauxID": 5,
  "treatyID": 1,
  "reportingPeriod": "2024-06",
  "reportType": "PREMIUM",
  "totalGross": 150000.00,
  "totalCeded": 60000.00,
  "totalRetained": 90000.00,
  "recordCount": 25,
  "status": "DRAFT",
  "generatedDate": "2024-07-01T10:00:00Z"
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Generate premium bordereaux | POST | /api/reinsurance/bordereaux/generate | {treatyID: 1, reportingPeriod: "2024-06", reportType: "PREMIUM"} | 201 Created | {bordereauxID, totals} |
| 2 | Generate loss bordereaux | POST | /api/reinsurance/bordereaux/generate | {reportType: "LOSS"} | 201 Created | {bordereauxID, totals} |
| 3 | Generate outstanding bordereaux | POST | /api/reinsurance/bordereaux/generate | {reportType: "OUTSTANDING"} | 201 Created | {bordereauxID, totals} |
| 4 | Treaty not found | POST | /api/reinsurance/bordereaux/generate | {treatyID: 99999} | 404 Not Found | {error: "Treaty not found: 99999"} |
| 5 | Invalid report type | POST | /api/reinsurance/bordereaux/generate | {reportType: "INVALID"} | 400 Bad Request | {error: "Invalid report type"} |
| 6 | Missing reporting period | POST | /api/reinsurance/bordereaux/generate | {reportingPeriod: null} | 400 Bad Request | {error: "Reporting period required"} |
| 7 | Unauthorized | POST | /api/reinsurance/bordereaux/generate | {valid} | 401 Unauthorized | {error} |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = Underwriter or Admin
