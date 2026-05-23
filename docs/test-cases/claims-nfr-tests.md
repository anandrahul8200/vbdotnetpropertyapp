# Claims Module - Non-Functional Requirement Tests

## Module: CLM (Claims)
## Test Type: Performance, Load, and Non-Functional Tests

---

### Test Case ID: NFR-CLM-001
**Category**: Performance
**Target**: Claims.usp_Claim_Search
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Response time (simple search) | < 2 seconds | SQL Profiler duration |
| Response time (complex multi-filter) | < 5 seconds | SQL Profiler duration |
| Response time (10K+ claims) | < 5 seconds | With pagination |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Search by claim number (indexed) | 50,000 claims | < 500ms |
| 2 | Search by customer name (LIKE) | 50,000 claims | < 3 seconds |
| 3 | Search by date range + status | 50,000 claims | < 2 seconds |
| 4 | Search all (no criteria, page 1) | 50,000 claims | < 2 seconds |
| 5 | Search page 100 (deep pagination) | 50,000 claims | < 3 seconds |

---

### Test Case ID: NFR-CLM-002
**Category**: Performance
**Target**: Claims.usp_Claim_GetDashboard
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Dashboard load (all adjusters) | < 3 seconds | SQL Profiler |
| Dashboard load (single adjuster) | < 2 seconds | SQL Profiler |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Dashboard with 5,000 open claims | 5,000 open | < 3 seconds |
| 2 | Dashboard with 500 pending payments | 500 pending | < 2 seconds |
| 3 | Dashboard for adjuster with 50 claims | 50 assigned | < 1 second |
| 4 | Overdue activity count | 1,000 overdue | < 2 seconds |

---

### Test Case ID: NFR-CLM-003
**Category**: Performance
**Target**: Claims.usp_Claim_CreatePayment
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single payment creation | < 1 second | SQL Profiler |
| Policy limit check | < 500ms | Within SP execution |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Create payment (first for claim) | Single record | < 1 second |
| 2 | Create payment (claim has 100 prior payments) | 100 existing | < 1 second |
| 3 | PaymentNumber generation under load | Concurrent inserts | Unique numbers, no deadlock |

---

### Test Case ID: NFR-CLM-004
**Category**: Performance
**Target**: Claims.usp_Fraud_EvaluateClaim
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single claim evaluation | < 5 seconds | SQL Profiler (cursor-based) |
| Batch evaluation (100 claims) | < 5 minutes | Batch job timing |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Evaluate claim with 10 indicators | 10 active indicators | < 3 seconds |
| 2 | Evaluate claim with 50 indicators | 50 active indicators | < 5 seconds |
| 3 | Prior claims lookup (PRIOR_CLAIMS) | Customer with 20 claims | < 1 second |

---

### Test Case ID: NFR-CLM-005
**Category**: Performance
**Target**: Claims.usp_Claim_GetDetails
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Full claim detail load (7 result sets) | < 3 seconds | SQL Profiler |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Claim with 50 activities | 50 rows | < 2 seconds |
| 2 | Claim with 20 payments | 20 rows | < 2 seconds |
| 3 | Claim with 10 status changes | 10 rows | < 1 second |
| 4 | Claim with all data populated | All tabs full | < 3 seconds |

---

### Test Case ID: NFR-CLM-006
**Category**: Concurrency
**Target**: Claim Payment Processing
**Priority**: High

#### Test Scenarios
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Two users create payments simultaneously on same claim | Both succeed or one gets policy limit error; no data corruption |
| 2 | Approve and void same payment concurrently | One succeeds, other gets status error |
| 3 | Concurrent reserve changes | TotalReserve reflects all approved changes |
| 4 | ClaimNumber generation under concurrent FNOL | All numbers unique (no duplicates) |
| 5 | PaymentNumber generation concurrently | All numbers unique |

---

### Test Case ID: NFR-CLM-007
**Category**: Data Volume
**Target**: Claims Module Tables
**Priority**: Medium

#### Scalability Tests
| # | Scenario | Volume | Acceptance |
|---|----------|--------|------------|
| 1 | Claims table growth | 1 million claims | Search still < 5s |
| 2 | Activities table growth | 10 million activities | GetDetails still < 3s |
| 3 | Payments table growth | 5 million payments | Payment creation still < 1s |
| 4 | StatusHistory table growth | 5 million records | History retrieval still < 1s |

---

### Test Case ID: NFR-CLM-008
**Category**: Reliability
**Target**: Transaction Integrity
**Priority**: Critical

#### Test Scenarios
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Error during claim creation (mid-transaction) | All changes rolled back, ErrorLog populated |
| 2 | Error during payment creation | Payment not created, TotalPaid unchanged |
| 3 | Error during reserve setting | Reserve not created, TotalReserve unchanged |
| 4 | Error during fraud evaluation | Previous scores cleared but restored on error [ASSUMPTION] |
| 5 | Error during subrogation creation | No partial records, claim flags unchanged |

---

### Test Case ID: NFR-CLM-009
**Category**: Security
**Target**: Claims Operations
**Priority**: High

#### Test Scenarios
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | SQL injection in search parameters | Dynamic SQL uses sp_executesql with parameterized queries |
| 2 | Payment amount manipulation | Server-side validation (policy limit check in SP) |
| 3 | Status transition bypass | SP enforces valid transitions regardless of UI |
| 4 | Unauthorized payment approval | Approval logged with ApprovedBy (audit trail) |
| 5 | Audit trail completeness | All create/update/delete operations logged |

---

### Test Case ID: NFR-CLM-010
**Category**: UI Responsiveness
**Target**: Claims Forms
**Priority**: Medium

#### Test Scenarios
| # | Scenario | Acceptance |
|---|----------|------------|
| 1 | frmClaimFNOL load time | < 2 seconds |
| 2 | frmClaimView load with full data | < 3 seconds |
| 3 | frmClaimSearch results display | < 3 seconds for 50 records |
| 4 | frmClaimDashboard load | < 3 seconds |
| 5 | frmFraudReview evaluation | < 5 seconds (includes SP execution) |
| 6 | Form cursor shows WaitCursor during load | Immediate visual feedback |
