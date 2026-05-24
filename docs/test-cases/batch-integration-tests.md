# Batch Jobs Module - Integration Tests (Future-State)

## Module: BAT (Batch Jobs)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> Batch jobs are standalone console applications calling stored procedures directly.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: MT-BAT-001
**Integration**: Expiration Processor -> Policy Service -> Notification Service
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| ExpirationProcessor | Policy Service | Expire policies past expiry date |
| ExpirationProcessor | Policy Service | Cancel policies for non-payment (GracePeriodDays=30) |
| ExpirationProcessor | Notification Service | Generate cancellation notices (NoticeDays=20) |
| ExpirationProcessor | Job Log Service | Start/Complete/Fail lifecycle |
| ExpirationProcessor | Error Log Service | Error capture on failure |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Expiration updates policy status | Batch service | Policy service | PolicyStatus changed to EXPIRED |
| 2 | Cancellation updates policy status | Batch service | Policy service | PolicyStatus changed to CANCELLED |
| 3 | Notice generation triggers notification | Batch service | Notification service | Cancellation notice queued for delivery |
| 4 | Job start logged | Batch service | Job log service | RUNNING status recorded |
| 5 | Job completion logged | Batch service | Job log service | COMPLETED status with counts |
| 6 | Policy service unavailable | Batch service | Policy service (down) | Job fails, FAILED status logged |
| 7 | Notification service unavailable | Batch service | Notification service (down) | Expiration succeeds, notices queued for retry |

---

### Test Case ID: MT-BAT-002
**Integration**: Renewal Processor -> Policy Service -> Quote Service
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| RenewalProcessor | Policy Service | Get policies due for renewal (DaysAhead window) |
| RenewalProcessor | Quote Service | Generate renewal quote for each policy |
| RenewalProcessor | Policy Service | Mark original policy as pending renewal (IsRenewal=1) |
| RenewalProcessor | Job Log Service | Lifecycle logging |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Due policies retrieved | Batch service | Policy service | ACTIVE policies within DaysAhead window |
| 2 | Renewal quote created | Batch service | Quote service | New policy version in QUOTE status |
| 3 | Original policy flagged | Batch service | Policy service | IsRenewal=1, ModifiedBy=BATCH_RENEWAL |
| 4 | Individual policy failure | Batch service | Quote service | Error logged, batch continues |
| 5 | Policy service unavailable | Batch service | Policy service (down) | Job fails with 0 processed |
| 6 | Quote service unavailable | Batch service | Quote service (down) | All individual policies fail, job completes with errors |

---

### Test Case ID: MT-BAT-003
**Integration**: Fraud Scoring -> Claims Service -> SIU Service
**Priority**: Critical
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| FraudScoring | Claims Service | Get claims to score (DaysBack window) |
| FraudScoring | Fraud Engine | Evaluate fraud indicators per claim |
| FraudScoring | SIU Service | Auto-refer claims with score >= 70 |
| FraudScoring | Job Log Service | Lifecycle logging |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Claims retrieved for scoring | Batch service | Claims service | Open claims from last DaysBack days |
| 2 | Fraud evaluation executed | Batch service | Fraud engine | FraudScore output returned |
| 3 | Score >= 70 triggers SIU referral | Batch service | SIU service | Referral created for high-score claims |
| 4 | Score < 70 no referral | Batch service | SIU service | No referral generated |
| 5 | Fraud engine unavailable | Batch service | Fraud engine (down) | Individual claims fail, batch continues |
| 6 | SIU service unavailable | Batch service | SIU service (down) | Scoring succeeds, referrals queued |
| 7 | Claims service returns empty | Batch service | Claims service | Job completes with 0 processed |

---

### Test Case ID: MT-BAT-004
**Integration**: Reserve Recalculator -> Claims Service -> Actuarial Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| ReserveRecalculator | Claims Service | Get open claims with reserves |
| ReserveRecalculator | Claims Service | Flag claims for review (activity created) |
| ReserveRecalculator | Actuarial Service | Calculate IBNR reserves |
| ReserveRecalculator | Claims Service | Update net incurred totals |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Open claims retrieved | Batch service | Claims service | Claims with TotalReserve > 0, not closed/denied |
| 2 | Review flag created | Batch service | Claims service | Activity record with Priority=HIGH |
| 3 | IBNR calculated | Batch service | Actuarial service | IBNR reserves computed as of date |
| 4 | Net incurred updated | Batch service | Claims service | NetIncurred = Reserve + Paid - Recovery |
| 5 | Claims service unavailable | Batch service | Claims service (down) | Job fails |
| 6 | Actuarial service unavailable | Batch service | Actuarial service (down) | Review completes, IBNR step fails |

---

### Test Case ID: MT-BAT-005
**Integration**: Payment Batch -> Billing Service -> Payment Gateway
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| PaymentBatch | Billing Service | Apply late fees to overdue invoices |
| PaymentBatch | Payment Gateway | Process auto-pay EFT transactions |
| PaymentBatch | Billing Service | Record payment against invoice |
| PaymentBatch | Notification Service | Generate overdue notices |
| PaymentBatch | Commission Service | Process agent commission payments |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Late fees applied | Batch service | Billing service | Overdue invoices updated with fee |
| 2 | EFT transaction processed | Batch service | Payment gateway | Funds transferred, payment recorded |
| 3 | EFT failure (insufficient funds) | Batch service | Payment gateway | Failed record logged, next payment continues |
| 4 | Overdue notices sent | Batch service | Notification service | Notices queued for DaysOverdue=10 invoices |
| 5 | Commission payments processed | Batch service | Commission service | Agent commissions calculated and recorded |
| 6 | Payment gateway unavailable | Batch service | Payment gateway (down) | All EFT payments fail, job completes with errors |
| 7 | Billing service unavailable | Batch service | Billing service (down) | Job fails at step 1 |

---

### Test Case ID: MT-BAT-006
**Integration**: Reinsurance Allocator -> Treaty Service -> Reporting Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Batch Component | Target Component | Data Flow |
|----------------|-----------------|-----------|
| ReinsuranceAllocator | Treaty Service | Get active treaties |
| ReinsuranceAllocator | Cession Service | Create premium and loss cessions |
| ReinsuranceAllocator | Reporting Service | Generate bordereaux reports |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Active treaties loaded | Batch service | Treaty service | All active treaties returned |
| 2 | Premium cessions created | Batch service | Cession service | One cession per unallocated premium per treaty |
| 3 | Loss cessions created | Batch service | Cession service | One cession per unallocated loss per treaty |
| 4 | Bordereaux generated | Batch service | Reporting service | PREMIUM + LOSS reports per treaty |
| 5 | Treaty service unavailable | Batch service | Treaty service (down) | Job fails at step 1 |
| 6 | Cession service unavailable | Batch service | Cession service (down) | Individual cessions fail, batch continues |

---

### Test Case ID: MT-BAT-007
**Integration**: Job Monitoring -> Job Log Service -> Alert Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Monitoring Component | Target Component | Data Flow |
|---------------------|-----------------|-----------|
| Job Monitor UI | Job Log Service | Query job history with filters |
| Job Log Service | Alert Service | FAILED job triggers alert |
| Alert Service | Email/SMS Service | Notify operations team |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Job history queried | Monitor UI | Job log service | Filtered results returned |
| 2 | Failed job triggers alert | Job log service | Alert service | Alert created for FAILED status |
| 3 | Long-running job alert | Job log service | Alert service | Alert if RUNNING > threshold |
| 4 | Alert delivery | Alert service | Email service | Operations team notified |
| 5 | Job log service unavailable | Monitor UI | Job log service (down) | Error displayed in UI |

---
