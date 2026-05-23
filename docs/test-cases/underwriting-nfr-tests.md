# Underwriting Module - Non-Functional Requirement Tests

## Module: UND (Underwriting)
## Test Type: Performance, Load, and Non-Functional Tests

---

### Test Case ID: NFR-UND-001
**Category**: Performance
**Target**: Underwriting.usp_Premium_Calculate
**Priority**: Critical

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single coverage premium calculation | < 1 second | SQL Profiler duration |
| Multi-coverage premium (5 coverages) | < 3 seconds | SQL Profiler duration |
| Multi-coverage premium (10 coverages) | < 5 seconds | SQL Profiler duration |
| Concurrent premium calculations | < 5 seconds | Load test |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Calculate premium (1 coverage) | 1 cursor iteration, 13 factor lookups | < 1 second |
| 2 | Calculate premium (5 coverages) | 5 iterations, 65 factor lookups | < 3 seconds |
| 3 | Calculate premium (10 coverages) | 10 iterations, 130 factor lookups | < 5 seconds |
| 4 | Concurrent calculations (20 policies) | 20 simultaneous executions | No deadlocks, < 5 seconds each |
| 5 | Large RatingFactors table | 100,000 factor rows | Factor lookups < 100ms each |
| 6 | Temp table #CoveragePremiums cleanup | Any calculation | No temp table leaks on error |

---

### Test Case ID: NFR-UND-002
**Category**: Performance
**Target**: Underwriting.usp_Rate_GetBaseRate / usp_Rate_GetFactor
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single rate lookup | < 100ms | SQL Profiler duration |
| Factor lookup (key-based) | < 100ms | SQL Profiler duration |
| Factor lookup (range-based) | < 100ms | SQL Profiler duration |
| Rate lookup with territory fallback | < 200ms | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Base rate lookup (territory hit) | 10,000 BaseRates rows | < 50ms |
| 2 | Base rate lookup (fallback to state) | 10,000 BaseRates rows | < 100ms |
| 3 | Key-based factor lookup | 50,000 RatingFactors rows | < 50ms |
| 4 | Range-based factor lookup | 50,000 RatingFactors rows | < 50ms |
| 5 | Concurrent rate lookups (100) | 100 simultaneous | < 100ms each, no contention |

---

### Test Case ID: NFR-UND-003
**Category**: Performance
**Target**: Underwriting.usp_Rules_Evaluate
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Rules evaluation (built-in only) | < 1 second | SQL Profiler duration |
| Rules evaluation (built-in + 50 DB rules) | < 2 seconds | SQL Profiler duration |
| Claims history count query | < 500ms | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Evaluate 6 built-in rules | Small dataset | < 500ms |
| 2 | Evaluate with 50 database-stored rules | 50 active rules | < 2 seconds |
| 3 | Claims history lookup (large history) | 10,000 claims for customer | < 500ms |
| 4 | Concurrent evaluations (10 policies) | 10 simultaneous | < 2 seconds each |
| 5 | #RuleResults temp table cleanup | Error during evaluation | Temp table dropped |

---

### Test Case ID: NFR-UND-004
**Category**: Performance
**Target**: Underwriting.usp_Referral_SearchPending
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Search with no filters | < 2 seconds | SQL Profiler duration |
| Search with all filters | < 1 second | SQL Profiler duration |
| Pagination (page 1 of 100) | < 1 second | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Unfiltered search | 10,000 pending referrals | < 2 seconds |
| 2 | Filtered by AssignedTo | 10,000 referrals, 100 for user | < 1 second |
| 3 | Pagination performance (last page) | 10,000 referrals, page 200 | < 2 seconds |
| 4 | TotalRecords COUNT query | 10,000 referrals | < 500ms |

---

### Test Case ID: NFR-UND-005
**Category**: Performance
**Target**: Underwriting.usp_Moratorium_Check
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Moratorium check | < 200ms | SQL Profiler duration |
| Check with many active moratoriums | < 500ms | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Check with 0 active moratoriums | Empty table | < 50ms |
| 2 | Check with 10 active moratoriums | 10 active rows | < 100ms |
| 3 | Check with 100 active moratoriums | 100 active, LIKE matching | < 500ms |
| 4 | Concurrent moratorium checks (50) | 50 simultaneous | < 200ms each |
| 5 | LIKE pattern performance on large zip list | AffectedZipCodes with 1000 zips | < 500ms [ASSUMPTION] |

---

### Test Case ID: NFR-UND-006
**Category**: Scalability
**Target**: Premium Calculation Engine
**Priority**: High

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Batch premium recalculation (100 policies) | 100 sequential calculations | < 2 minutes total |
| 2 | Batch premium recalculation (1000 policies) | 1000 sequential calculations | < 20 minutes total |
| 3 | RatingFactors table growth | 500,000 rows | Factor lookups still < 100ms |
| 4 | BaseRates table growth | 100,000 rows | Rate lookups still < 100ms |
| 5 | RatingWorksheets table growth | 1,000,000 rows | INSERT/DELETE still < 200ms |
