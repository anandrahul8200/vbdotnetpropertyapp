# Policy Module - Non-Functional / Performance Tests

## Module: POL (Policy)
## Test Type: NFR (Non-Functional Requirements)

---

### Test Case ID: NFR-POL-001
**Operation**: Customer Search
**Component**: Policy.usp_Customer_Search

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Simple name search | < 2 seconds | 10,000 customers | 1 user |
| 2 | Full-text search with pagination | < 3 seconds | 50,000 customers | 10 users |
| 3 | Complex multi-filter search | < 5 seconds | 100,000 customers | 25 users |
| 4 | Peak load search | < 10 seconds | 100,000 customers | 50 users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Double customer count | Query execution time | < 2x increase |
| 2 | Increase concurrent searches | Response time degradation | < 30% increase at 50 users |

---

### Test Case ID: NFR-POL-002
**Operation**: Policy Search
**Component**: Policy.usp_Policy_Search

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Search by policy number (exact) | < 1 second | 50,000 policies | 1 user |
| 2 | Search by customer name (LIKE) | < 3 seconds | 50,000 policies | 10 users |
| 3 | Multi-filter with date range | < 5 seconds | 100,000 policies | 25 users |
| 4 | Full table scan (no filters) | < 8 seconds | 100,000 policies | 10 users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Increase policy count 10x | Query time | < 5x increase |
| 2 | Add index on PolicyNumber | Search by number speed | < 100ms |

---

### Test Case ID: NFR-POL-003
**Operation**: Policy Quote Creation
**Component**: Policy.usp_Policy_CreateQuote

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single quote creation | < 2 seconds | N/A | 1 user |
| 2 | Concurrent quote creation | < 5 seconds | N/A | 10 users |
| 3 | Quote with moratorium check | < 3 seconds | 100 active moratoriums | 1 user |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Batch quote generation (100 quotes) | Transaction log growth | < 100MB |
| 2 | Concurrent creates | Lock contention | No deadlocks |
| 3 | PolicyNumber sequence generation | Identity collision | No duplicates |

---

### Test Case ID: NFR-POL-004
**Operation**: Policy Details Retrieval
**Component**: Policy.usp_Policy_GetDetails

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Simple policy (few coverages, no claims) | < 1 second | Standard | 1 user |
| 2 | Complex policy (many coverages, claims, billing) | < 3 seconds | 20 coverages, 10 claims, 50 invoices | 1 user |
| 3 | Concurrent policy views | < 5 seconds | Standard | 25 users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Policy with 100+ endorsements | SP execution time | < 5 seconds |
| 2 | Policy with 50+ claims | SP execution time | < 5 seconds |

---

### Test Case ID: NFR-POL-005
**Operation**: Policy Bind Transaction
**Component**: Policy.usp_Policy_Bind

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single bind operation | < 2 seconds | N/A | 1 user |
| 2 | Concurrent binds | < 3 seconds | N/A | 5 users |

#### Reliability Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Transaction rollback on audit failure | No partial status update |
| 2 | Concurrent bind of same policy | Only one succeeds, other gets status error |
| 3 | Database restart during bind | Transaction rolled back, no corruption |

---

### Test Case ID: NFR-POL-006
**Operation**: Policy Cancellation with Premium Calculation
**Component**: Policy.usp_Policy_Cancel

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single cancellation | < 2 seconds | N/A | 1 user |
| 2 | Batch cancellation (non-payment) | < 30 seconds | 100 policies | 1 (batch) |

#### Reliability Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Premium calculation precision | Return premium matches manual calculation to 2 decimal places |
| 2 | Transaction atomicity | Status update + audit log both committed or both rolled back |

---

### Test Case ID: NFR-POL-007
**Operation**: Dashboard KPI Loading
**Component**: Reporting.usp_Dashboard_PolicyKPIs

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Dashboard initial load | < 5 seconds | 50,000 policies, 20,000 claims | 1 user |
| 2 | Dashboard with multiple users | < 8 seconds | Same | 20 users |
| 3 | Dashboard refresh | < 5 seconds | Same | 1 user |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | KPI subquery execution | CPU usage | < 50% per query |
| 2 | Expiring policies query | Memory | < 100MB result set |
| 3 | Multiple dashboard sessions | DB connection pool | No pool exhaustion at 50 sessions |

---

### Test Case ID: NFR-POL-008
**Operation**: Agent Production Report
**Component**: Policy.usp_Agent_GetProduction

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single agent production | < 2 seconds | 10,000 policies | 1 user |
| 2 | All agents production | < 10 seconds | 100 agents, 50,000 policies | 1 user |

---

### Test Case ID: NFR-POL-009
**Operation**: Form Load Times
**Component**: All Policy Forms

#### Performance Tests
| # | Scenario | SLA Target | Notes |
|---|----------|-----------|-------|
| 1 | frmCustomerEntry load (new) | < 1 second | No data fetch |
| 2 | frmCustomerEntry load (edit) | < 2 seconds | GetByID call |
| 3 | frmPolicyEntry load (new) | < 2 seconds | Agent list loaded |
| 4 | frmPolicyView load | < 3 seconds | Full GetDetails with 6 result sets |
| 5 | frmPolicyDashboard load | < 5 seconds | KPIs + grids |
| 6 | frmPolicySearch after search | < 3 seconds | Paginated results |

---

### Test Case ID: NFR-POL-010
**Operation**: Document Operations
**Component**: Policy.usp_Document_Search, Policy.usp_Document_GetByEntity

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Search documents by entity | < 2 seconds | 10,000 documents | 1 user |
| 2 | Search with date range | < 3 seconds | 50,000 documents | 5 users |
| 3 | Get documents for policy | < 1 second | 50 documents per policy | 10 users |
