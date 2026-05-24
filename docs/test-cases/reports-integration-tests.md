# Reports Module - Integration Tests (Future-State)

## Module: RPT (Reports)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: INT-RPT-001
**Integration**: Report Service -> Policy Data Store
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Data Store Component | Data Flow |
|-----------------|---------------------|-----------|
| Executive Dashboard | Policy.Policies | Policy counts, premium totals, status distribution |
| Loss Ratio | Policy.Policies + Claims.Claims | Premium vs incurred losses JOIN |
| Production Report | Policy.Policies + Policy.Agents | Agent policy counts and premium |
| Policy KPIs | Policy.Policies + Policy.Customers | Active counts, expiring policies |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Dashboard loads policy summary | Report service | Policy store | PolicyStatus groups with counts |
| 2 | Loss ratio joins policies to claims | Report service | Policy + Claims stores | Correct LEFT JOIN results |
| 3 | Production report joins to agents | Report service | Policy + Agent stores | Agent-level aggregation |
| 4 | Policy store unavailable | Report service | Policy store (down) | 503 Service Unavailable, cached data returned [ASSUMPTION] |
| 5 | Stale data handling | Report service | Policy store | Report timestamp indicates data freshness |

---

### Test Case ID: INT-RPT-002
**Integration**: Report Service -> Claims Data Store
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Data Store Component | Data Flow |
|-----------------|---------------------|-----------|
| Claims Aging | Claims.Claims | Open claims grouped by age |
| Claims by Type | Claims.Claims | Claim type aggregation |
| Executive Dashboard | Claims.Claims | Summary stats for open claims |
| Financial Summary | Claims.Claims + Claims.Payments | Incurred and paid losses |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claims aging calculates buckets | Report service | Claims store | DATEDIFF-based aging buckets |
| 2 | Claims by type aggregation | Report service | Claims store | GROUP BY ClaimType results |
| 3 | Payment totals for financial summary | Report service | Payments store | SUM of approved payments |
| 4 | Claims store unavailable | Report service | Claims store (down) | Graceful degradation, partial report |
| 5 | Data consistency across result sets | Report service | Multiple stores | Consistent point-in-time snapshot |

---

### Test Case ID: INT-RPT-003
**Integration**: Report Service -> Billing Data Store
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Data Store Component | Data Flow |
|-----------------|---------------------|-----------|
| Billing Aging | Billing.Invoices | Open invoices by age bucket |
| Executive Dashboard | Billing.Invoices | Overdue count and total |
| Financial Summary | Billing.CommissionTransactions | Commission totals |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Billing aging buckets calculated | Report service | Billing store | DueDate-based aging |
| 2 | Outstanding billing for dashboard | Report service | Billing store | OVERDUE/PARTIAL invoice totals |
| 3 | Commission transactions aggregated | Report service | Billing store | EARNED transaction sums |
| 4 | Billing store unavailable | Report service | Billing store (down) | Partial report with missing billing section |

---

### Test Case ID: INT-RPT-004
**Integration**: Report Service -> Reinsurance Data Store
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Data Store Component | Data Flow |
|-----------------|---------------------|-----------|
| Reinsurance Summary | Reinsurance.Treaties + Cessions | Treaty-level cession aggregation |
| Financial Summary | Reinsurance.Cessions | Ceded premium total |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Treaty summary with cession counts | Report service | Reinsurance store | LEFT JOIN treaties to cessions |
| 2 | Ceded premium for financial summary | Report service | Reinsurance store | CessionType='PREMIUM' filter |
| 3 | Accounting period filter | Report service | Reinsurance store | Exact VARCHAR match on period |
| 4 | Treaty with no cessions | Report service | Reinsurance store | Treaty shown with zero counts (LEFT JOIN) |

---

### Test Case ID: INT-RPT-005
**Integration**: Report Service -> Export Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Export Component | Data Flow |
|-----------------|-----------------|-----------|
| Any report result | CSV Export | DataTable to CSV file |
| Any report result | PDF Export [ASSUMPTION] | DataTable to PDF format |
| Any report result | Excel Export [ASSUMPTION] | DataTable to XLSX format |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | CSV export of report results | Report service | Export service | Properly formatted CSV with headers |
| 2 | Large dataset export | Report service | Export service | Streaming export for 100000+ rows |
| 3 | Export with special characters | Report service | Export service | Quotes and commas properly escaped |
| 4 | Concurrent export requests | Multiple users | Export service | No file conflicts, unique file names |
| 5 | Export service unavailable | Report service | Export service (down) | Error message, report still viewable |

---

### Test Case ID: INT-RPT-006
**Integration**: Report Service -> Caching Layer
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Report Component | Cache Component | Data Flow |
|-----------------|----------------|-----------|
| Executive Dashboard | Redis/Memory cache | Cache frequently accessed KPIs |
| Policy KPIs | Redis/Memory cache | Cache active policy counts |
| Static lookups | Redis/Memory cache | Cache state codes, agent lists |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Dashboard served from cache | Report service | Cache | < 100ms response from cached data |
| 2 | Cache miss falls through to DB | Report service | Cache -> DB | Full query executed, result cached |
| 3 | Cache invalidation on data change | Data update event | Cache | Stale entries evicted |
| 4 | Cache TTL expiry | Report service | Cache | Auto-refresh after TTL (5 min) [ASSUMPTION] |
| 5 | Cache unavailable | Report service | Cache (down) | Falls through to database directly |
