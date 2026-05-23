# Reports Module - API Tests (Future-State)

## Module: RPT (Reports)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-RPT-001
**Legacy SP**: Admin.usp_Report_ExecutiveDashboard
**Future Endpoint**: GET /api/reports/executive-dashboard
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Admin.usp_Report_ExecutiveDashboard | GET /api/reports/executive-dashboard?asOfDate= |
| Admin.usp_Report_LossRatio | GET /api/reports/loss-ratio?periodFrom=&periodTo=&groupBy= |
| Admin.usp_Report_ClaimsAging | GET /api/reports/claims-aging?asOfDate= |
| Admin.usp_Report_Production | GET /api/reports/production?periodFrom=&periodTo=&agentId= |
| Admin.usp_Report_FinancialSummary | GET /api/reports/financial-summary?periodFrom=&periodTo= |
| Reporting.usp_Dashboard_PolicyKPIs | GET /api/reports/policy-kpis?asOfDate= |
| Reporting.usp_Report_ClaimsByType | GET /api/reports/claims-by-type?dateFrom=&dateTo= |
| Reporting.usp_Report_AgentProduction | GET /api/reports/agent-production?dateFrom=&dateTo= |
| Reporting.usp_Report_BillingAging | GET /api/reports/billing-aging?asOfDate= |
| Reporting.usp_Report_ReinsuranceSummary | GET /api/reports/reinsurance-summary?treatyId=&accountingPeriod= |

#### Request/Response Specification
**Request (GET /api/reports/executive-dashboard):**
```
GET /api/reports/executive-dashboard?asOfDate=2025-06-30
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "policySummary": [
    { "policyStatus": "ACTIVE", "policyCount": 1500, "totalPremium": 4500000.00 }
  ],
  "writtenPremiumTrend": [
    { "month": "2025-01", "policiesWritten": 45, "writtenPremium": 135000.00 }
  ],
  "claimsSummary": {
    "totalOpenClaims": 230,
    "totalOutstandingReserve": 2100000.00,
    "totalPaidYTD": 1800000.00,
    "totalIncurred": 3900000.00
  },
  "lossRatio": {
    "earnedPremium": 4200000.00,
    "incurredLosses": 2940000.00,
    "lossRatio": 70.00
  },
  "outstandingBilling": {
    "overdueInvoices": 85,
    "totalOverdue": 125000.00
  }
}
```

---

### Test Case ID: API-RPT-002
**Legacy SP**: Admin.usp_Report_LossRatio
**Future Endpoint**: GET /api/reports/loss-ratio
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/loss-ratio?periodFrom=2024-01-01&periodTo=2024-12-31&groupBy=POLICY_TYPE
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "periodFrom": "2024-01-01",
  "periodTo": "2024-12-31",
  "groupBy": "POLICY_TYPE",
  "data": [
    {
      "groupValue": "HO3",
      "policyCount": 500,
      "earnedPremium": 1500000.00,
      "incurredLosses": 900000.00,
      "lossRatio": 60.00
    }
  ]
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Missing periodFrom | 400 Bad Request: "periodFrom is required" |
| 2 | Missing periodTo | 400 Bad Request: "periodTo is required" |
| 3 | Invalid groupBy value | 400 Bad Request: "groupBy must be one of: POLICY_TYPE, STATE, AGENT, MONTH" |
| 4 | periodFrom after periodTo | 400 Bad Request: "periodFrom must be before periodTo" |
| 5 | Unauthorized access | 401 Unauthorized |
| 6 | Insufficient permissions | 403 Forbidden |

---

### Test Case ID: API-RPT-003
**Legacy SP**: Admin.usp_Report_ClaimsAging
**Future Endpoint**: GET /api/reports/claims-aging
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/claims-aging?asOfDate=2025-03-31
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "asOfDate": "2025-03-31",
  "agingBuckets": [
    {
      "bucket": "0-30 Days",
      "claimCount": 45,
      "totalReserve": 450000.00,
      "totalPaid": 120000.00,
      "totalIncurred": 570000.00,
      "avgIncurred": 12666.67
    }
  ]
}
```

---

### Test Case ID: API-RPT-004
**Legacy SP**: Admin.usp_Report_Production
**Future Endpoint**: GET /api/reports/production
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/production?periodFrom=2024-01-01&periodTo=2024-12-31&agentId=5
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "periodFrom": "2024-01-01",
  "periodTo": "2024-12-31",
  "agents": [
    {
      "agentNumber": "AGT-001",
      "agentName": "John Smith",
      "agencyName": "Smith Insurance Agency",
      "newBusinessCount": 25,
      "newBusinessPremium": 75000.00,
      "renewalCount": 40,
      "renewalPremium": 120000.00,
      "totalPolicies": 65,
      "totalPremium": 195000.00,
      "totalCommission": 19500.00
    }
  ]
}
```

---

### Test Case ID: API-RPT-005
**Legacy SP**: Admin.usp_Report_FinancialSummary
**Future Endpoint**: GET /api/reports/financial-summary
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/financial-summary?periodFrom=2024-01-01&periodTo=2024-12-31
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "periodFrom": "2024-01-01",
  "periodTo": "2024-12-31",
  "categories": [
    { "category": "Written Premium", "amount": 5000000.00 },
    { "category": "Earned Premium", "amount": 4200000.00 },
    { "category": "Incurred Losses", "amount": 2940000.00 },
    { "category": "Paid Losses", "amount": 2100000.00 },
    { "category": "Outstanding Reserves", "amount": 1800000.00 },
    { "category": "Commissions", "amount": 500000.00 },
    { "category": "Ceded Premium (Reinsurance)", "amount": 750000.00 }
  ]
}
```

---

### Test Case ID: API-RPT-006
**Legacy SP**: Reporting.usp_Dashboard_PolicyKPIs
**Future Endpoint**: GET /api/reports/policy-kpis
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/policy-kpis?asOfDate=2025-06-30
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "kpis": {
    "activePolicies": 1500,
    "newBusiness": 45,
    "renewals": 60,
    "cancellations": 8,
    "writtenPremium": 4500000.00,
    "lossRatio": 65.50
  },
  "expiringPolicies": [
    {
      "policyId": 101,
      "policyNumber": "POL-2024-001",
      "policyType": "HO3",
      "expiryDate": "2025-07-15",
      "annualPremium": 2500.00,
      "customerName": "Jane Doe"
    }
  ],
  "recentActivity": [
    { "activity": "Policy Created", "reference": "POL-2025-100", "activityDate": "2025-06-28" }
  ]
}
```

---

### Test Case ID: API-RPT-007
**Legacy SP**: Reporting.usp_Report_ClaimsByType
**Future Endpoint**: GET /api/reports/claims-by-type
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/claims-by-type?dateFrom=2024-01-01&dateTo=2024-12-31
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "dateFrom": "2024-01-01",
  "dateTo": "2024-12-31",
  "claimTypes": [
    {
      "claimType": "WATER",
      "claimCount": 120,
      "totalEstimatedLoss": 960000.00,
      "totalPaid": 720000.00,
      "totalReserve": 240000.00
    }
  ]
}
```

---

### Test Case ID: API-RPT-008
**Legacy SP**: Reporting.usp_Report_AgentProduction
**Future Endpoint**: GET /api/reports/agent-production
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/agent-production?dateFrom=2024-01-01&dateTo=2024-12-31
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "agentNumber": "AGT-001",
      "agentName": "John Smith",
      "agencyName": "Smith Insurance Agency",
      "policyCount": 65,
      "totalPremium": 195000.00,
      "totalCommission": 19500.00
    }
  ]
}
```

---

### Test Case ID: API-RPT-009
**Legacy SP**: Reporting.usp_Report_BillingAging
**Future Endpoint**: GET /api/reports/billing-aging
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/billing-aging?asOfDate=2025-06-30
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "asOfDate": "2025-06-30",
  "agingBuckets": [
    { "bucket": "Current", "invoiceCount": 200, "totalBalance": 350000.00 },
    { "bucket": "1-30 Days", "invoiceCount": 45, "totalBalance": 67500.00 },
    { "bucket": "31-60 Days", "invoiceCount": 20, "totalBalance": 30000.00 },
    { "bucket": "61-90 Days", "invoiceCount": 10, "totalBalance": 15000.00 },
    { "bucket": "90+ Days", "invoiceCount": 5, "totalBalance": 12500.00 }
  ]
}
```

---

### Test Case ID: API-RPT-010
**Legacy SP**: Reporting.usp_Report_ReinsuranceSummary
**Future Endpoint**: GET /api/reports/reinsurance-summary
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/reports/reinsurance-summary?treatyId=1&accountingPeriod=2024-Q1
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "treatyNumber": "TR-2024-001",
      "treatyName": "Quota Share 2024",
      "treatyType": "QUOTA_SHARE",
      "cessionCount": 150,
      "totalGross": 3000000.00,
      "totalCeded": 900000.00,
      "totalRetained": 2100000.00
    }
  ]
}
```

#### Common API Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | No auth token | 401 Unauthorized |
| 2 | Expired token | 401 Unauthorized |
| 3 | User without report permissions | 403 Forbidden |
| 4 | Invalid date format | 400 Bad Request |
| 5 | Rate limiting | 429 Too Many Requests after threshold |
