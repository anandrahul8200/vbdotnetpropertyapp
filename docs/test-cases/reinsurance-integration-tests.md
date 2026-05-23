# Reinsurance Module - Integration Tests (Future-State)

## Module: RNS (Reinsurance)
## Source Files:
- `database/02-stored-procedures/007-reinsurance-sps.sql`
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`
- `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality as described.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: MT-RNS-001
**Integration**: Policy Module -> Reinsurance Cession (Premium)
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | New policy triggers premium cession | Policy Service | Reinsurance Service | PolicyID, GrossPremium | Cession records created for all matching treaties |
| 2 | Policy endorsement recalculates cession | Policy Service | Reinsurance Service | PolicyID, adjusted premium | New cession records for premium difference |
| 3 | Policy cancellation reverses cessions | Policy Service | Reinsurance Service | PolicyID, return premium | Reversal cession records created |
| 4 | Policy type determines treaty match | Policy Service | Treaty Matcher | PolicyType from policy | Only treaties with matching CoveredPolicyTypes |
| 5 | Property state determines treaty match | Policy/Property Service | Treaty Matcher | StateCode from property | Only treaties with matching CoveredStates |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All cession calculations succeed | Transaction commits, all cession records persisted |
| 2 | Error during cursor iteration | Transaction rolls back, no partial cessions, ErrorLog entry |
| 3 | Policy lookup fails | Transaction rolls back, no cessions created |
| 4 | Concurrent premium cession for same policy | Serializable isolation prevents duplicate cessions |

#### Data Flow Tests
| # | Flow | Input | Intermediate State | Final State |
|---|------|-------|-------------------|-------------|
| 1 | Premium cession (QS) | PolicyID=100, GrossPremium=10000, QS Treaty 40% | CededAmount=4000 calculated | Cession record: Gross=10000, Ceded=4000, Retained=6000, Status=PENDING |
| 2 | Premium cession (Surplus) | PolicyID=100, TIV=1M, Retention=500K | SurplusPct=50% calculated | Cession record: Ceded=50% of premium |
| 3 | Premium cession (XOL) | PolicyID=100, XOL Treaty 5% flat rate | CededAmount=5% of premium | Cession record: Ceded=flat rate amount |

---

### Test Case ID: MT-RNS-002
**Integration**: Claims Module -> Reinsurance Cession (Loss)
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Claim payment triggers loss cession | Claims Service | Reinsurance Service | ClaimID, LossAmount | Loss cession records created |
| 2 | Claim reserve change updates cessions | Claims Service | Reinsurance Service | ClaimID, revised amount | [ASSUMPTION] Existing cessions updated or new records |
| 3 | Claim recovery reduces cessions | Claims Service | Reinsurance Service | ClaimID, recovery amount | [ASSUMPTION] Recovery cession created |
| 4 | XOL attachment point validation | Claims Service | Reinsurance Service | LossAmount vs AttachmentPoint | Cession only if loss > attachment |
| 5 | Surplus uses existing premium % | Reinsurance Service | Cession History | TreatyID, PolicyID | Latest premium cession % applied to loss |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All loss cessions calculated | Transaction commits, all records persisted |
| 2 | Claim lookup fails | Transaction rolls back, ErrorLog entry with ClaimID |
| 3 | Treaty cursor returns no matches | Transaction commits (no-op), no cessions created |
| 4 | Concurrent loss cessions for same claim | Proper isolation prevents duplicates |

#### Data Flow Tests
| # | Flow | Input | Intermediate State | Final State |
|---|------|-------|-------------------|-------------|
| 1 | Loss cession (QS) | ClaimID=50, LossAmount=100000, QS 40% | CededLoss=40000 | Cession: Gross=100000, Ceded=40000, Retained=60000, Type=LOSS |
| 2 | Loss cession (XOL) | ClaimID=50, Loss=500000, Attach=100K, Exhaust=500K | CededLoss=400000 (capped at layer) | Cession: Ceded=400000 |
| 3 | Loss cession (Surplus) | ClaimID=50, Prior premium %=0.50 | CededLoss=50% of loss | Cession: Ceded=LossAmount*0.50 |

---

### Test Case ID: MT-RNS-003
**Integration**: Reinsurance Cessions -> Bordereaux Generation
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Generate premium bordereaux aggregates cessions | Bordereaux Service | Cession Repository | TreatyID, Period, Type=PREMIUM | Sum of PREMIUM cessions for period |
| 2 | Generate loss bordereaux aggregates cessions | Bordereaux Service | Cession Repository | TreatyID, Period, Type=LOSS | Sum of LOSS cessions for period |
| 3 | OUTSTANDING type aggregates all types | Bordereaux Service | Cession Repository | TreatyID, Period, Type=OUTSTANDING | Sum of ALL cession types |
| 4 | Generation updates cession status | Bordereaux Service | Cession Repository | PENDING cessions | Cessions updated to REPORTED |
| 5 | Bordereaux submission to reinsurer | Bordereaux Service | External Integration | BordereauxID | [ASSUMPTION] Status updated to SUBMITTED |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Aggregation and insert succeed | Bordereaux record created, cessions updated to REPORTED |
| 2 | Treaty validation fails | Error raised: "Treaty not found: N", no records created |
| 3 | Cession status update fails | Transaction rolls back, bordereaux not created |
| 4 | Concurrent generation for same treaty/period | Both succeed (separate bordereaux records allowed) |

#### Data Flow Tests
| # | Flow | Input | Intermediate State | Final State |
|---|------|-------|-------------------|-------------|
| 1 | Premium bordereaux | TreatyID=1, Period=2024-06, Type=PREMIUM | SUM(Gross)=150K, SUM(Ceded)=60K, COUNT=25 | Bordereaux: TotalGross=150K, TotalCeded=60K, RecordCount=25, Status=DRAFT |
| 2 | Cession status update | PENDING cessions in period | Status updated | Cessions: Status=REPORTED |

---

### Test Case ID: MT-RNS-004
**Integration**: Reinsurance Module -> Audit System
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Treaty creation logs audit | Treaty Service | Audit Service | TreatyID, Action=INSERT | AuditLog entry created |
| 2 | Bordereaux generation logs audit | Bordereaux Service | Audit Service | BordereauxID, Action=INSERT | AuditLog entry created |
| 3 | Error during cession logs to ErrorLog | Cession Service | Error Logger | Error details | ErrorLog entry with procedure name and additional info |
| 4 | All operations include username | Any Service | Audit Service | CreatedBy/GeneratedBy | Username captured from authentication context |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Audit log write succeeds | Main transaction commits with audit record |
| 2 | Error log written on failure | ErrorLog entry persisted even after main transaction rollback |
| 3 | Audit log unavailable | [ASSUMPTION] Main operation may still succeed |

---

### Test Case ID: MT-RNS-005
**Integration**: frmReinsuranceView -> ReinsuranceDataAccess -> Database
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | Form load calls GetActiveTreaties | frmReinsuranceView | ReinsuranceDataAccess | None | DataTable populates dgvTreaties |
| 2 | Load cessions calls GetCessionsByTreaty | frmReinsuranceView | ReinsuranceDataAccess | TreatyID, Period | DataTable populates dgvCessions |
| 3 | Generate calls GenerateBordereaux | frmReinsuranceView | ReinsuranceDataAccess | TreatyID, Period, Type | BordereauxID returned |
| 4 | Refresh calls GetBordereaux | frmReinsuranceView | ReinsuranceDataAccess | TreatyID | DataTable populates dgvBordereaux |

#### Error Propagation Tests
| # | Scenario | Source Error | Expected UI Behavior |
|---|----------|-------------|---------------------|
| 1 | Database connection lost | SqlException | MessageBox with error message, cursor restored |
| 2 | SP raises custom error | RAISERROR in SP | MessageBox with error text |
| 3 | Timeout during long operation | SqlException (timeout) | MessageBox, cursor restored to Default |
