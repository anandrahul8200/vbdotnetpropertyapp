# Reinsurance Module - Non-Functional Requirements Tests

## Module: RNS (Reinsurance)
## Source Files:
- `database/02-stored-procedures/007-reinsurance-sps.sql`
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`
- `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

---

### Test Case ID: NFR-RNS-001
**Operation**: Premium Cession Calculation (Bulk Portfolio)
**Component**: Reinsurance.usp_Cession_CalculatePremium

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single policy premium cession | < 2 seconds | 1 policy, 5 active treaties | 1 user |
| 2 | Batch premium cession (small portfolio) | < 30 seconds | 100 policies, 5 treaties | 1 user |
| 3 | Batch premium cession (medium portfolio) | < 2 minutes | 1,000 policies, 10 treaties | 1 user |
| 4 | Batch premium cession (large portfolio) | < 10 minutes | 10,000 policies, 10 treaties | 1 user |
| 5 | Concurrent premium cession calculations | < 5 seconds per policy | 100 policies | 10 concurrent users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Increase treaty count from 5 to 50 | Query time for treaty cursor | < 3x increase |
| 2 | Increase cession history from 10K to 100K records | Cession insert time | < 2x increase |
| 3 | Increase concurrent cession calculations | Deadlock frequency | Zero deadlocks |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Cursor-based treaty iteration | Memory per connection | < 50MB |
| 2 | Large batch cession processing | Tempdb usage | < 1GB |
| 3 | Transaction log growth during bulk insert | Log file growth | < 500MB per batch |
| 4 | Connection pool during concurrent operations | DB connection pool | No pool exhaustion |

#### Reliability Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database restart during cession calculation | Transaction rolled back, no partial cessions |
| 2 | Deadlock during concurrent cession inserts | Automatic retry or graceful failure with rollback |
| 3 | Cursor remains open on error | Cursor closed and deallocated in CATCH block |

---

### Test Case ID: NFR-RNS-002
**Operation**: Loss Cession Calculation
**Component**: Reinsurance.usp_Cession_CalculateLoss

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Single claim loss cession | < 2 seconds | 1 claim, 5 active treaties | 1 user |
| 2 | Batch loss cession (catastrophe event) | < 5 minutes | 500 claims, 10 treaties | 1 user |
| 3 | Loss cession with surplus lookup | < 3 seconds | 1 claim, surplus treaty with prior premium cession | 1 user |
| 4 | Concurrent loss cession calculations | < 5 seconds per claim | 50 claims | 10 concurrent users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Increase claims per treaty from 100 to 10,000 | Per-claim calculation time | < 2x increase |
| 2 | Surplus treaty with extensive cession history | Lookup time for existing premium % | < 1 second |
| 3 | Multiple XOL layers (10+ treaties) | Total calculation time per claim | < 5 seconds |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Catastrophe batch processing | CPU utilization | < 80% sustained |
| 2 | Large claim with many matching treaties | Memory per session | < 100MB |
| 3 | Extended batch operation | Transaction log | No log full errors |

---

### Test Case ID: NFR-RNS-003
**Operation**: Bordereaux Generation
**Component**: Reinsurance.usp_Bordereaux_Generate

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Generate bordereaux (small treaty) | < 5 seconds | 100 cessions in period | 1 user |
| 2 | Generate bordereaux (medium treaty) | < 30 seconds | 5,000 cessions in period | 1 user |
| 3 | Generate bordereaux (large treaty) | < 2 minutes | 50,000 cessions in period | 1 user |
| 4 | Concurrent bordereaux generation | < 30 seconds each | 5 treaties, 1000 cessions each | 5 concurrent users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Increase cession count per period | Aggregation query time | Linear growth |
| 2 | Update PENDING to REPORTED (bulk update) | Update duration | < 5 seconds per 1000 rows |
| 3 | Concurrent generation for same treaty | Locking behavior | No blocking > 30 seconds |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Large aggregation query | Tempdb for sort/group | < 500MB |
| 2 | Bulk status update | Lock escalation | No table-level locks |
| 3 | Multiple bordereaux in same transaction | Transaction duration | < 60 seconds |

---

### Test Case ID: NFR-RNS-004
**Operation**: Treaty List and Summary Retrieval
**Component**: Reinsurance.usp_Treaty_GetActive, Reinsurance.usp_Treaty_GetSummary

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Get active treaties list | < 2 seconds | 50 active treaties, 100K total cessions | 1 user |
| 2 | Get treaty summary (small treaty) | < 2 seconds | 500 cessions, 12 bordereaux | 1 user |
| 3 | Get treaty summary (large treaty) | < 5 seconds | 50,000 cessions, 60 bordereaux | 1 user |
| 4 | Concurrent treaty list queries | < 3 seconds | 50 treaties | 20 concurrent users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Subquery for TotalCededPremium/Loss | Query time with 1M cessions | < 5 seconds |
| 2 | TOP 50 recent cessions with JOINs | Query time | < 2 seconds |
| 3 | Increase active treaty count to 200 | List retrieval time | < 3 seconds |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Correlated subqueries in GetActive | CPU per query | < 5% of total CPU |
| 2 | Multi-result-set summary | Network bandwidth | < 1MB per call |
| 3 | Repeated summary calls (caching potential) | Query plan cache | Plans cached and reused |

---

### Test Case ID: NFR-RNS-005
**Operation**: frmReinsuranceView Form Responsiveness
**Component**: frmReinsuranceView

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | Form load with treaty list | < 3 seconds | 50 treaties | 1 user |
| 2 | Load cessions grid | < 3 seconds | 500 cessions | 1 user |
| 3 | Refresh bordereaux grid | < 2 seconds | 100 bordereaux records | 1 user |
| 4 | Generate bordereaux (UI wait) | < 10 seconds | 1000 cessions to aggregate | 1 user |

#### Reliability Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database timeout during grid load | Error message displayed, cursor restored to Default |
| 2 | Memory pressure with large grid | DataGridView handles gracefully, no OutOfMemory |
| 3 | Rapid button clicks during processing | UI remains responsive (WaitCursor indicates processing) |
| 4 | Form opened multiple times | Each instance independent, no shared state issues |
