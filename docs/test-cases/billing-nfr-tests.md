# Billing Module - Non-Functional Requirement Tests

## Module: BIL (Billing)
## Test Type: Performance, Load, and Non-Functional Tests

---

### Test Case ID: NFR-BIL-001
**Category**: Performance
**Target**: Billing.usp_Invoice_Generate
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single invoice generation (ANNUAL) | < 1 second | SQL Profiler duration |
| Multi-installment generation (10-pay) | < 2 seconds | SQL Profiler duration |
| Concurrent invoice generation | < 3 seconds | Load test |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Generate single annual invoice | 1 record insert | < 500ms |
| 2 | Generate 4-installment plan | 4 record inserts | < 1 second |
| 3 | Generate 12-installment plan | 12 record inserts | < 2 seconds |
| 4 | Sequential generation (InvoiceNumber uniqueness under load) | 50 concurrent calls | No duplicates, < 3 seconds each |
| 5 | Invoice generation with 100K existing invoices | 100,000 existing | < 2 seconds |

---

### Test Case ID: NFR-BIL-002
**Category**: Performance
**Target**: Billing.usp_Payment_Record
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Single payment recording | < 1 second | SQL Profiler duration |
| Payment with invoice allocation | < 1 second | SQL Profiler duration |
| Payment with overflow to next invoice | < 1.5 seconds | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Record payment (specific invoice) | Single update | < 500ms |
| 2 | Record payment (auto-find oldest invoice) | Search + update | < 1 second |
| 3 | Overpayment cascading to next invoice | 2 invoice updates | < 1 second |
| 4 | PaymentNumber generation under load | 100 concurrent inserts | Unique numbers, no deadlock |
| 5 | Payment recording with 50K existing payments | 50,000 records | < 1 second |

---

### Test Case ID: NFR-BIL-003
**Category**: Performance
**Target**: Billing.usp_Invoice_ApplyLateFees
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Batch late fee application (100 invoices) | < 5 seconds | SQL Profiler duration |
| Batch late fee application (1000 invoices) | < 30 seconds | SQL Profiler duration |
| Batch late fee application (10000 invoices) | < 5 minutes | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Apply late fees (small batch) | 100 overdue invoices | < 5 seconds |
| 2 | Apply late fees (medium batch) | 1,000 overdue invoices | < 30 seconds |
| 3 | Apply late fees (large batch) | 10,000 overdue invoices | < 5 minutes |
| 4 | No overdue invoices (short-circuit) | 0 eligible | < 500ms |
| 5 | Mixed eligible/ineligible invoices | 5,000 total, 500 eligible | < 5 seconds |

---

### Test Case ID: NFR-BIL-004
**Category**: Performance
**Target**: Billing.usp_Billing_GetByPolicy
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Billing inquiry (typical policy) | < 2 seconds | SQL Profiler duration |
| Billing inquiry (heavy history) | < 5 seconds | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Policy with 5 invoices, 5 payments | 10 records | < 1 second |
| 2 | Policy with 50 invoices, 100 payments | 150 records | < 2 seconds |
| 3 | Policy with 200+ invoices (long history) | 200+ records | < 5 seconds |
| 4 | Policy with no billing data | 0 records | < 500ms |

---

### Test Case ID: NFR-BIL-005
**Category**: Performance
**Target**: Billing.usp_Commission_GetStatement
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| Statement generation (single agent) | < 3 seconds | SQL Profiler duration |
| Statement with large transaction history | < 5 seconds | SQL Profiler duration |

#### Test Scenarios
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Agent with 50 transactions in period | 50 records | < 2 seconds |
| 2 | Agent with 500 transactions in period | 500 records | < 3 seconds |
| 3 | Agent with no transactions | 0 records | < 500ms |
| 4 | Wide date range (full year) | Variable | < 5 seconds |

---

### Test Case ID: NFR-BIL-006
**Category**: Concurrency
**Target**: Invoice and Payment Number Generation
**Priority**: Critical

#### Concurrency Criteria
| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| InvoiceNumber uniqueness | No duplicates | Concurrent insert test |
| PaymentNumber uniqueness | No duplicates | Concurrent insert test |
| RefundNumber uniqueness | No duplicates | Concurrent insert test |
| Transaction isolation | No phantom reads | Under concurrent operations |

#### Test Scenarios
| # | Scenario | Concurrency Level | Acceptance |
|---|----------|-------------------|------------|
| 1 | 10 simultaneous invoice generations | 10 threads | All unique InvoiceNumbers, no deadlocks |
| 2 | 20 simultaneous payment recordings | 20 threads | All unique PaymentNumbers, no deadlocks |
| 3 | Payment + late fee on same invoice | 2 concurrent operations | Correct final balance, no lost update |
| 4 | Multiple payments on same policy | 5 concurrent | All applied correctly, no double-pay |

---

### Test Case ID: NFR-BIL-007
**Category**: Data Volume
**Target**: All Billing Operations
**Priority**: Medium

#### Scalability Tests
| # | Scenario | Data Volume | Acceptance |
|---|----------|-------------|------------|
| 1 | Invoice table with 1M records | 1,000,000 invoices | All queries < 5 seconds |
| 2 | Payment table with 500K records | 500,000 payments | Payment recording < 2 seconds |
| 3 | Refund table with 100K records | 100,000 refunds | Pending refund query < 3 seconds |
| 4 | Commission table with 200K records | 200,000 transactions | Statement generation < 5 seconds |
