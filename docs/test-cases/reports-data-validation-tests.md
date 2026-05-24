# Reports Module - Data Validation Tests

## Module: RPT (Reports)
## Test Type: Data Integrity and Validation Tests
## Data Sources:
- `Policy.Policies` - Premium, status, dates
- `Claims.Claims` - Reserves, paid amounts, incurred losses
- `Claims.Payments` - Payment amounts and statuses
- `Billing.Invoices` - Balance due, status, due dates
- `Billing.CommissionTransactions` - Commission amounts
- `Reinsurance.Treaties` - Treaty details
- `Reinsurance.Cessions` - Cession amounts
- `Policy.Agents` - Agent information
- `Policy.Properties` - State codes

---

### Test Case ID: DV-RPT-001
**Report**: Executive Dashboard
**Source**: Admin.usp_Report_ExecutiveDashboard

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | PolicyStatus values are valid | Query Policy.Policies | Only known statuses: ACTIVE, EXPIRED, CANCELLED, QUOTE |
| 2 | GrossPremium is non-negative | SUM(GrossPremium) | >= 0 (no negative premiums) |
| 3 | WrittenPremium is non-negative | SUM(WrittenPremium) | >= 0 |
| 4 | TotalReserve is non-negative | SUM(TotalReserve) for open claims | >= 0 |
| 5 | TotalPaid is non-negative | SUM(TotalPaid) | >= 0 |
| 6 | NetIncurred = TotalReserve + TotalPaid - TotalRecovery | Calculated field | Verify consistency |
| 7 | LossRatio between 0 and reasonable max | Calculated percentage | 0 <= LossRatio (no upper bound, but flag if > 200%) |
| 8 | BalanceDue is positive for overdue | OVERDUE/PARTIAL invoices | BalanceDue > 0 |
| 9 | Month format is yyyy-MM | Written premium trend | Matches format pattern |
| 10 | CreatedDate within 12-month window | DATEADD filter | Only policies within range included |

---

### Test Case ID: DV-RPT-002
**Report**: Loss Ratio Report
**Source**: Admin.usp_Report_LossRatio

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | PolicyCount uses DISTINCT PolicyID | COUNT(DISTINCT p.PolicyID) | No duplicate counting |
| 2 | EarnedPremium is WrittenPremium aggregate | SUM(p.WrittenPremium) | Matches manual calculation |
| 3 | IncurredLosses uses ISNULL for NULL claims | LEFT JOIN with no claims | Returns 0, not NULL |
| 4 | LossRatio percentage calculation | Formula | ROUND(IncurredLosses / EarnedPremium * 100, 2) |
| 5 | LossDate filtered by period | BETWEEN @PeriodFrom AND @PeriodTo | Only claims within range |
| 6 | Policy overlap check | EffectiveDate <= @PeriodTo AND ExpiryDate >= @PeriodFrom | Policies active during period |
| 7 | StateCode is valid 2-char code | GROUP BY STATE | All values are exactly 2 characters |
| 8 | AgentName concatenation | FirstName + ' ' + LastName | Properly formatted full name |

---

### Test Case ID: DV-RPT-003
**Report**: Claims Aging Report
**Source**: Admin.usp_Report_ClaimsAging

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | DATEDIFF calculation correctness | Known ReportedDate and AsOfDate | Correct day count |
| 2 | Bucket boundaries are contiguous | All buckets | No gaps: 0-30, 31-60, 61-90, 91-180, 181-365, 365+ |
| 3 | Every open claim falls in exactly one bucket | All open claims | SUM of all bucket ClaimCounts = total open claims |
| 4 | TotalReserve >= 0 for each bucket | Aggregated data | No negative reserves |
| 5 | AvgIncurred = TotalIncurred / ClaimCount | Calculation check | Average matches manual division |
| 6 | Excluded statuses | CLOSED and DENIED | Never appear in results |
| 7 | TotalPaid <= NetIncurred | Business logic | Paid should not exceed incurred [ASSUMPTION] |

---

### Test Case ID: DV-RPT-004
**Report**: Production Report
**Source**: Admin.usp_Report_Production

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | NewBusinessCount + RenewalCount = TotalPolicies | Per agent row | Sum matches total |
| 2 | NewBusinessPremium + RenewalPremium = TotalPremium | Per agent row | Sum matches total |
| 3 | IsRenewal flag correctness | Policy.IsRenewal field | 0 = new business, 1 = renewal |
| 4 | Only ACTIVE/EXPIRED counted | PolicyStatus filter | No QUOTE or CANCELLED policies |
| 5 | CommissionAmount non-negative | Per policy | >= 0 |
| 6 | AgentNumber format | Policy.Agents | Valid agent number format |
| 7 | EffectiveDate within period | BETWEEN filter | Only policies effective in range |

---

### Test Case ID: DV-RPT-005
**Report**: Financial Summary
**Source**: Admin.usp_Report_FinancialSummary

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Written Premium > Earned Premium | Same period | Written >= Earned (pro-rata reduces) |
| 2 | Earned premium calculation | DATEDIFF formula | Days used / Total days * AnnualPremium |
| 3 | NULLIF prevents divide-by-zero | Policies with 0-day term | Returns NULL, not error |
| 4 | LEAST function boundary | ExpiryDate < @PeriodTo | Uses ExpiryDate as end |
| 5 | Payment status filter | APPROVED, ISSUED, CLEARED | Only these 3 statuses summed |
| 6 | Commission TransactionType | WHERE TransactionType = 'EARNED' | Only EARNED type included |
| 7 | Cession CessionType | WHERE CessionType = 'PREMIUM' | Only PREMIUM cessions for ceded amount |
| 8 | All 7 categories present | UNION ALL query | Exactly 7 rows returned always |
| 9 | Outstanding reserves not date-filtered | Claims WHERE clause | All open claims regardless of date |

---

### Test Case ID: DV-RPT-006
**Report**: Policy KPIs Dashboard
**Source**: Reporting.usp_Dashboard_PolicyKPIs

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | ActivePolicies count | PolicyStatus = 'ACTIVE' | Matches manual count |
| 2 | NewBusiness within last month | CreatedDate >= DATEADD(MONTH, -1, @AsOfDate) | Correct date filter |
| 3 | Renewals flag | IsRenewal = 1 | Only renewal policies counted |
| 4 | Cancellations by ModifiedDate | PolicyStatus = 'CANCELLED' | ModifiedDate used, not EffectiveDate |
| 5 | WrittenPremium for active only | PolicyStatus = 'ACTIVE' | Only active policies summed |
| 6 | TOP 20 expiring policies | ExpiryDate within 30 days | Maximum 20 results |
| 7 | TOP 20 recent activities | CreatedDate within 7 days | Maximum 20 results |
| 8 | LossRatio handles zero premium | No active policies | Returns 0 (CASE WHEN) |

---

### Test Case ID: DV-RPT-007
**Report**: Billing Aging
**Source**: Reporting.usp_Report_BillingAging

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Only open invoices | Status IN ('OPEN', 'OVERDUE', 'PARTIAL') | No PAID or CANCELLED invoices |
| 2 | BalanceDue > 0 filter | Zero balance invoices | Excluded from results |
| 3 | Bucket boundaries | DATEDIFF from DueDate | Current (<=0), 1-30, 31-60, 61-90, 90+ |
| 4 | InvoiceCount is positive | Per bucket | > 0 for each returned bucket |
| 5 | TotalBalance is positive | Per bucket | > 0 (BalanceDue > 0 filter ensures this) |
| 6 | Sum of all buckets = total outstanding | All buckets | Total matches unbucketed query |

---

### Test Case ID: DV-RPT-008
**Report**: Reinsurance Summary
**Source**: Reporting.usp_Report_ReinsuranceSummary

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TotalGross >= TotalCeded + TotalRetained | Per treaty | Gross = Ceded + Retained |
| 2 | CessionCount matches detail count | COUNT(c.CessionID) | Matches actual cession records |
| 3 | TreatyNumber is unique | Per treaty | No duplicate treaty numbers |
| 4 | NULL handling for treaties without cessions | LEFT JOIN | CessionCount=0, amounts=NULL |
| 5 | AccountingPeriod format | VARCHAR(10) | Consistent format (e.g., '2024-Q1') [ASSUMPTION] |
