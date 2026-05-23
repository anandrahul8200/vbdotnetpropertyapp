# Policy Module - API Tests (Future-State)

## Module: POL (Policy)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-POL-001
**Legacy SP**: Policy.usp_Customer_Create
**Future Endpoint**: POST /api/customers
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Customer_Create | POST /api/customers |
| Policy.usp_Customer_GetByID | GET /api/customers/{id} |
| Policy.usp_Customer_Update | PUT /api/customers/{id} |
| Policy.usp_Customer_Search | GET /api/customers?searchTerm=&type=&state= |

#### Request/Response Specification
**Request (POST /api/customers):**
```json
{
  "customerType": "I",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@email.com",
  "phone": "512-555-1234",
  "addressLine1": "123 Main St",
  "city": "Austin",
  "stateCode": "TX",
  "zipCode": "78701",
  "creditScore": 780,
  "occupation": "Engineer",
  "annualIncome": 95000.00
}
```

**Success Response (201 Created):**
```json
{
  "customerId": 1,
  "customerNumber": "CUS0000001",
  "customerType": "I",
  "firstName": "John",
  "lastName": "Doe",
  "riskTier": "PREFERRED",
  "customerSince": "2025-01-15T00:00:00Z",
  "isActive": true
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Create valid individual | POST | /api/customers | {valid individual} | 201 Created | Customer with ID and number |
| 2 | Create valid commercial | POST | /api/customers | {type: "C", companyName} | 201 Created | Customer created |
| 3 | Missing required fields | POST | /api/customers | {no addressLine1} | 400 Bad Request | Validation errors |
| 4 | Get by ID | GET | /api/customers/1 | -- | 200 OK | Full customer object |
| 5 | Get non-existent | GET | /api/customers/999999 | -- | 404 Not Found | Error message |
| 6 | Update valid | PUT | /api/customers/1 | {email: "new@test.com"} | 200 OK | Updated customer |
| 7 | Update non-existent | PUT | /api/customers/999999 | {email: "x"} | 404 Not Found | Error |
| 8 | Search customers | GET | /api/customers?searchTerm=Smith | -- | 200 OK | Paginated list |
| 9 | Unauthorized | POST | /api/customers | {valid} | 401 Unauthorized | Auth error |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = PolicyAdmin or PolicyEntry

---

### Test Case ID: API-POL-002
**Legacy SP**: Policy.usp_Property_Create
**Future Endpoint**: POST /api/customers/{customerId}/properties
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Property_Create | POST /api/customers/{customerId}/properties |
| Policy.usp_Property_GetByCustomer | GET /api/customers/{customerId}/properties |
| Policy.usp_Property_GetByID | GET /api/properties/{id} |

#### Request/Response Specification
**Request (POST /api/customers/{customerId}/properties):**
```json
{
  "propertyType": "SINGLE_FAMILY",
  "constructionType": "FRAME",
  "occupancyType": "OWNER_OCCUPIED",
  "yearBuilt": 2005,
  "squareFootage": 2500,
  "numberOfStories": 2,
  "addressLine1": "456 Oak Ave",
  "city": "Austin",
  "stateCode": "TX",
  "zipCode": "78701",
  "hasFireAlarm": true,
  "hasBurglarAlarm": true,
  "marketValue": 450000.00,
  "replacementCost": 380000.00
}
```

**Success Response (201 Created):**
```json
{
  "propertyId": 1,
  "propertyNumber": "PRP0000001",
  "customerId": 1,
  "propertyType": "SINGLE_FAMILY",
  "fireProtectionClass": 5,
  "isActive": true
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Create valid property | POST | /api/customers/1/properties | {valid} | 201 Created | Property with ID |
| 2 | Invalid customer | POST | /api/customers/999/properties | {valid} | 404 Not Found | Customer not found |
| 3 | Inactive customer | POST | /api/customers/{inactive}/properties | {valid} | 400 Bad Request | Customer not active |
| 4 | Get properties by customer | GET | /api/customers/1/properties | -- | 200 OK | Array of properties |
| 5 | Get property by ID | GET | /api/properties/1 | -- | 200 OK | Property object |
| 6 | Get non-existent | GET | /api/properties/999999 | -- | 404 Not Found | Error |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = PolicyAdmin or PolicyEntry

---

### Test Case ID: API-POL-003
**Legacy SP**: Policy.usp_Policy_CreateQuote
**Future Endpoint**: POST /api/policies/quotes
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Policy_CreateQuote | POST /api/policies/quotes |
| Policy.usp_Policy_GetDetails | GET /api/policies/{id} |
| Policy.usp_Policy_Search | GET /api/policies?number=&status=&type= |
| Policy.usp_Policy_Bind | POST /api/policies/{id}/bind |
| Policy.usp_Policy_Cancel | POST /api/policies/{id}/cancel |
| Policy.usp_Policy_Reinstate | POST /api/policies/{id}/reinstate |

#### Request/Response Specification
**Request (POST /api/policies/quotes):**
```json
{
  "policyType": "HO3",
  "customerId": 1,
  "propertyId": 1,
  "agentId": 1,
  "effectiveDate": "2025-06-01",
  "termMonths": 12,
  "paymentPlan": "ANNUAL",
  "billingMethod": "DIRECT",
  "priorCarrier": "State Farm",
  "claimFreeYears": 3
}
```

**Success Response (201 Created):**
```json
{
  "policyId": 1,
  "policyNumber": "POL0000001",
  "policyStatus": "QUOTE",
  "effectiveDate": "2025-06-01",
  "expiryDate": "2026-06-01",
  "commissionRate": 0.10
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Create valid quote | POST | /api/policies/quotes | {valid} | 201 Created | Quote with number |
| 2 | Moratorium in effect | POST | /api/policies/quotes | {blocked location} | 409 Conflict | Moratorium error |
| 3 | Invalid customer | POST | /api/policies/quotes | {customerId: 999} | 400 Bad Request | Customer not found |
| 4 | Property not owned by customer | POST | /api/policies/quotes | {mismatched} | 400 Bad Request | Ownership error |
| 5 | Bind policy | POST | /api/policies/1/bind | {} | 200 OK | Status = ACTIVE |
| 6 | Bind non-QUOTE policy | POST | /api/policies/1/bind | {} | 409 Conflict | Must be in QUOTE |
| 7 | Cancel policy | POST | /api/policies/1/cancel | {reason, date} | 200 OK | Return premium |
| 8 | Reinstate policy | POST | /api/policies/1/reinstate | {date, conditions} | 200 OK | Status = ACTIVE |
| 9 | Get policy details | GET | /api/policies/1 | -- | 200 OK | Full policy with coverages |
| 10 | Search policies | GET | /api/policies?status=ACTIVE | -- | 200 OK | Paginated list |

#### Auth Requirements
- Authentication: Bearer token (JWT)
- Authorization: Role = PolicyAdmin, PolicyEntry, or Underwriter

---

### Test Case ID: API-POL-004
**Legacy SP**: Policy.usp_Policy_SaveCoverage
**Future Endpoint**: PUT /api/policies/{id}/coverages
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Policy_SaveCoverage | PUT /api/policies/{id}/coverages/{code} |
| Policy.usp_Policy_UpdatePremium | PUT /api/policies/{id}/premium |

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Save coverage | PUT | /api/policies/1/coverages/DWELLING | {limit, deductible, premium} | 200 OK | Updated coverage |
| 2 | Add new coverage | PUT | /api/policies/1/coverages/FLOOD | {new coverage} | 201 Created | New coverage |
| 3 | Invalid policy | PUT | /api/policies/999/coverages/X | {valid} | 404 Not Found | Policy not found |
| 4 | Update premium | PUT | /api/policies/1/premium | {annual, fees, gross} | 200 OK | Updated amounts |

---

### Test Case ID: API-POL-005
**Legacy SP**: Policy.usp_Agent_Search, Policy.usp_Agent_GetByID, Policy.usp_Agent_Update
**Future Endpoint**: /api/agents
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Agent_Search | GET /api/agents?name=&type=&active= |
| Policy.usp_Agent_GetByID | GET /api/agents/{id} |
| Policy.usp_Agent_Update | PUT /api/agents/{id} |
| Policy.usp_Agent_GetProduction | GET /api/agents/{id}/production?from=&to= |

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Search agents | GET | /api/agents?name=Smith | -- | 200 OK | Agent list |
| 2 | Get agent | GET | /api/agents/1 | -- | 200 OK | Agent with agency |
| 3 | Update agent | PUT | /api/agents/1 | {email, phone} | 200 OK | Updated agent |
| 4 | Get production | GET | /api/agents/1/production | -- | 200 OK | Stats object |
| 5 | Non-existent agent | GET | /api/agents/999999 | -- | 404 Not Found | Error |

---

### Test Case ID: API-POL-006
**Legacy SP**: Policy.usp_Document_Create, usp_Document_GetByEntity, usp_Document_Delete, usp_Document_Search
**Future Endpoint**: /api/documents
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Document_Create | POST /api/documents |
| Policy.usp_Document_GetByEntity | GET /api/{entityType}/{entityId}/documents |
| Policy.usp_Document_Delete | DELETE /api/documents/{id} |
| Policy.usp_Document_Search | GET /api/documents?entityType=&type=&from=&to= |

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Upload document | POST | /api/documents | {multipart with metadata} | 201 Created | Document with ID |
| 2 | Get entity documents | GET | /api/policies/1/documents | -- | 200 OK | Document list |
| 3 | Delete document | DELETE | /api/documents/1 | -- | 204 No Content | -- |
| 4 | Search documents | GET | /api/documents?entityType=POLICY | -- | 200 OK | Paginated list |
| 5 | Delete non-existent | DELETE | /api/documents/999 | -- | 404 Not Found | Error |
