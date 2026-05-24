# Reports Module - Non-Functional Requirements Tests

## Module: RPT (Reports)
## Test Type: Performance, Security, and Reliability Tests

---

### Test Case ID: NFR-RPT-001
**Category**: Performance
**Component**: Executive Dashboard (Admin.usp_Report_ExecutiveDashboard)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 10000 policies) | < 3 seconds | Time from execution to all 5 result sets returned |
| Response time (> 100000 policies) | < 10 seconds | Large dataset performance |
| Concurrent dashboard loads | 20 simultaneous | No deadlocks or timeouts |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small dataset | 1000 policies, 200 claims | < 1s |
| 2 | Medium dataset | 50000 policies, 10000 claims | < 3s |
| 3 | Large dataset | 500000 policies, 100000 claims | < 10s |
| 4 | Multiple result sets | All 5 result sets | Combined time < target |
| 5 | Concurrent access | 20 simultaneous calls | No blocking, consistent results |

---

### Test Case ID: NFR-RPT-002
**Category**: Performance
**Component**: Loss Ratio Report (Admin.usp_Report_LossRatio)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (1 year range) | < 5 seconds | Full year with GROUP BY |
| Response time (multi-year) | < 10 seconds | 5-year range |
| JOIN performance | Acceptable | Policy-Claims LEFT JOIN |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Group by POLICY_TYPE | 50000 policies | < 2s |
| 2 | Group by STATE | 50000 policies with properties | < 3s (extra JOIN) |
| 3 | Group by AGENT | 50000 policies with agents | < 3s (extra JOIN) |
| 4 | 5-year date range | Large historical data | < 10s |
| 5 | Single month range | Narrow filter | < 1s |

---

### Test Case ID: NFR-RPT-003
**Category**: Performance
**Component**: Claims Aging Report (Admin.usp_Report_ClaimsAging)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 5000 open claims) | < 2 seconds | Aging calculation and grouping |
| Response time (> 50000 open claims) | < 8 seconds | Large open claim portfolio |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Few open claims | 500 open claims | < 500ms |
| 2 | Many open claims | 50000 open claims | < 5s |
| 3 | DATEDIFF calculation load | Large dataset | CASE expression evaluation time acceptable |
| 4 | GROUP BY with aggregates | Multiple buckets | Aggregate functions (SUM, AVG, COUNT) perform well |

---

### Test Case ID: NFR-RPT-004
**Category**: Performance
**Component**: Financial Summary (Admin.usp_Report_FinancialSummary)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (1 year) | < 5 seconds | 7 UNION ALL queries combined |
| Each sub-query | < 2 seconds individual | Single category calculation |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Full year financial summary | 100000 policies, 20000 claims | < 5s |
| 2 | Earned premium calculation | Complex DATEDIFF/LEAST formula | < 2s |
| 3 | Payment aggregation | 50000 payment records | < 2s |
| 4 | Commission aggregation | 30000 commission transactions | < 1s |
| 5 | UNION ALL efficiency | All 7 queries | No excessive temp table usage |

---

### Test Case ID: NFR-RPT-005
**Category**: Performance
**Component**: Policy KPIs Dashboard (Reporting.usp_Dashboard_PolicyKPIs)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time | < 3 seconds | All 3 result sets combined |
| Subquery performance | < 1 second each | Individual scalar subqueries |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | KPI calculations | 100000 policies | < 2s for scalar queries |
| 2 | TOP 20 expiring policies | Large active portfolio | < 1s with index on ExpiryDate |
| 3 | Recent activity query | High-volume new policies | < 1s with index on CreatedDate |

---

### Test Case ID: NFR-RPT-006
**Category**: Performance
**Component**: Billing Aging (Reporting.usp_Report_BillingAging)
**Priority**: Medium

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Few outstanding invoices | 500 open invoices | < 500ms |
| 2 | Many outstanding invoices | 50000 open invoices | < 3s |
| 3 | DATEDIFF and CASE grouping | Large dataset | Acceptable without index hint |

---

### Test Case ID: NFR-RPT-007
**Category**: Performance
**Component**: Agent Production (Reporting.usp_Report_AgentProduction)
**Priority**: Medium

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small agent base | 50 agents, 5000 policies | < 1s |
| 2 | Large agent base | 500 agents, 100000 policies | < 5s |
| 3 | LEFT JOIN performance | Agents with/without policies | No Cartesian product |

---

### Test Case ID: NFR-RPT-008
**Category**: Security
**Component**: Report Access Control
**Priority**: High

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Report data respects user permissions | Only authorized reports accessible |
| 2 | SQL injection via parameter controls | Parameterized queries prevent injection |
| 3 | Export file path validation | No path traversal in SaveFileDialog |
| 4 | Sensitive financial data access | Only users with appropriate role can view financial reports [ASSUMPTION] |

---

### Test Case ID: NFR-RPT-009
**Category**: Reliability
**Component**: Report Error Recovery
**Priority**: Medium

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Database timeout during report | Exception caught, error message shown, cursor restored |
| 2 | Connection lost mid-report | Graceful failure with user-friendly message |
| 3 | Large result set memory | DataTable handles 100000+ rows without OutOfMemoryException [ASSUMPTION] |
| 4 | Concurrent report executions | Multiple reports from different users do not block |

---

### Test Case ID: NFR-RPT-010
**Category**: Scalability
**Component**: Report Data Volume
**Priority**: Medium

#### Scalability Tests
| # | Test | Dataset | Expected |
|---|------|---------|----------|
| 1 | 1 million policies | Stress test | Reports complete within 30 seconds |
| 2 | 5 years of historical data | Date ranges span full history | No timeout |
| 3 | DataGridView rendering | 100000 rows in grid | UI remains responsive [ASSUMPTION] |
