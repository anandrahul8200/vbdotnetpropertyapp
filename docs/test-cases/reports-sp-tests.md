# Reports Module - Stored Procedure Tests

## Module: RPT (Reports)
## Source Files:
- `database/02-stored-procedures/009-reporting-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (10 total):
1. Admin.usp_Report_ExecutiveDashboard
2. Admin.usp_Report_LossRatio
3. Admin.usp_Report_ClaimsAging
4. Admin.usp_Report_Production
5. Admin.usp_Report_FinancialSummary
6. Reporting.usp_Dashboard_PolicyKPIs
7. Reporting.usp_Report_ClaimsByType
8. Reporting.usp_Report_AgentProduction
9. Reporting.usp_Report_BillingAging
10. Reporting.usp_Report_ReinsuranceSummary

---

### Test Case ID: SP-RPT-001
**Procedure**: Admin.usp_Report_ExecutiveDashboard
**Source**: `database/02-stored-procedures/009-reporting-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (default NULL, defaults to CAST(GETDATE() AS DATE))

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Execute with default date | @AsOfDate=NULL | Returns 5 result sets: policy counts by status, written premium by month, claims summary, loss ratio, outstanding billing |
| 2 | Execute with specific date | @AsOfDate='2025-06-30' | Returns data as of specified date |
| 3 | Policy counts by status | Default | Result set with PolicyStatus, PolicyCount, TotalPremium columns |
| 4 | Written premium last 12 months | Default | Result set with Month, PoliciesWritten, WrittenPremium grouped by yyyy-MM |
| 5 | Claims summary for open claims | Default | TotalOpenClaims, TotalOutstandingReserve, TotalPaidYTD, TotalIncurred |
| 6 | Loss ratio calculation | Default | EarnedPremium, IncurredLosses, LossRatio (rounded to 2 decimal places) |
| 7 | Outstanding billing | Default | OverdueInvoices count, TotalOverdue amount for OVERDUE/PARTIAL invoices |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Future date | @AsOfDate='2099-12-31' | Returns zero counts (no data in future ranges) |
| 2 | Very old date | @AsOfDate='1900-01-01' | Returns zero counts (no matching data) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | NULL AsOfDate defaults to today | @AsOfDate=NULL | Uses CAST(GETDATE() AS DATE) |
| 2 | Loss ratio with zero premium | No policies in range | LossRatio = 0 (CASE WHEN handles divide-by-zero) |
| 3 | Written premium 12-month boundary | @AsOfDate at month boundary | DATEADD(MONTH, -12, @AsOfDate) correctly filters |

---

### Test Case ID: SP-RPT-002
**Procedure**: Admin.usp_Report_LossRatio
**Source**: `database/02-stored-procedures/009-reporting-sps.sql`
**Parameters**:
- @PeriodFrom DATE - Required
- @PeriodTo DATE - Required
- @GroupBy VARCHAR(20) - Optional (default 'POLICY_TYPE', options: POLICY_TYPE, STATE, AGENT, MONTH)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Group by POLICY_TYPE | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31', @GroupBy='POLICY_TYPE' | PolicyType, PolicyCount, EarnedPremium, IncurredLosses, LossRatio |
| 2 | Group by STATE | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31', @GroupBy='STATE' | StateCode, PolicyCount, EarnedPremium, IncurredLosses, LossRatio ordered by LossRatio DESC |
| 3 | Group by AGENT | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31', @GroupBy='AGENT' | AgentNumber, AgentName, PolicyCount, EarnedPremium, IncurredLosses, LossRatio ordered by LossRatio DESC |
| 4 | Default GroupBy | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31' | Defaults to POLICY_TYPE grouping |
| 5 | Loss ratio calculation | Policies with claims | ROUND(SUM(IncurredLosses) / SUM(EarnedPremium) * 100, 2) |
| 6 | Policies with no claims | @GroupBy='POLICY_TYPE' | IncurredLosses = 0, LossRatio = 0 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | PeriodFrom after PeriodTo | @PeriodFrom='2025-12-31', @PeriodTo='2024-01-01' | Empty result set (no matching policies) |
| 2 | Invalid GroupBy value | @GroupBy='INVALID' | No result set returned (no matching IF branch) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Zero earned premium | No policies in range | LossRatio = 0 (CASE WHEN SUM > 0 handles division) |
| 2 | Same from and to date | @PeriodFrom=@PeriodTo='2024-06-15' | Returns policies effective on that single day |
| 3 | Claims with LossDate exactly on boundary | LossDate = @PeriodFrom or @PeriodTo | Included (BETWEEN is inclusive) |

---

### Test Case ID: SP-RPT-003
**Procedure**: Admin.usp_Report_ClaimsAging
**Source**: `database/02-stored-procedures/009-reporting-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (default NULL, defaults to CAST(GETDATE() AS DATE))

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Default aging buckets | @AsOfDate=NULL | Returns AgingBucket, ClaimCount, TotalReserve, TotalPaid, TotalIncurred, AvgIncurred |
| 2 | Specific as-of date | @AsOfDate='2025-03-31' | Aging calculated from ReportedDate to specified date |
| 3 | Bucket: 0-30 Days | Claims reported within 30 days | Grouped in '0-30 Days' bucket |
| 4 | Bucket: 31-60 Days | Claims reported 31-60 days ago | Grouped in '31-60 Days' bucket |
| 5 | Bucket: 61-90 Days | Claims reported 61-90 days ago | Grouped in '61-90 Days' bucket |
| 6 | Bucket: 91-180 Days | Claims reported 91-180 days ago | Grouped in '91-180 Days' bucket |
| 7 | Bucket: 181-365 Days | Claims reported 181-365 days ago | Grouped in '181-365 Days' bucket |
| 8 | Bucket: 365+ Days | Claims reported over 365 days ago | Grouped in '365+ Days' bucket |
| 9 | Only open claims included | Mix of OPEN, CLOSED, DENIED claims | Only claims NOT IN ('CLOSED', 'DENIED') counted |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No open claims exist | All claims CLOSED/DENIED | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Claim exactly 30 days old | DATEDIFF = 30 | Falls in '0-30 Days' bucket (<=30) |
| 2 | Claim exactly 31 days old | DATEDIFF = 31 | Falls in '31-60 Days' bucket |
| 3 | Claim exactly 365 days old | DATEDIFF = 365 | Falls in '181-365 Days' bucket (<=365) |
| 4 | Claim exactly 366 days old | DATEDIFF = 366 | Falls in '365+ Days' bucket |
| 5 | Results ordered by age | Multiple buckets | ORDER BY MIN(DATEDIFF) ascending |

---

### Test Case ID: SP-RPT-004
**Procedure**: Admin.usp_Report_Production
**Source**: `database/02-stored-procedures/009-reporting-sps.sql`
**Parameters**:
- @PeriodFrom DATE - Required
- @PeriodTo DATE - Required
- @AgentID INT - Optional (default NULL, all agents)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | All agents production | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31', @AgentID=NULL | AgentNumber, AgentName, AgencyName, NewBusinessCount/Premium, RenewalCount/Premium, TotalPolicies, TotalPremium, TotalCommission |
| 2 | Single agent production | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31', @AgentID=1 | Only specified agent's data |
| 3 | New business vs renewals | Policies with IsRenewal=0 and IsRenewal=1 | Correctly separated counts and premiums |
| 4 | Results ordered by TotalPremium DESC | Multiple agents | Highest premium agent first |
| 5 | Only ACTIVE/EXPIRED policies | Policies in various statuses | Only PolicyStatus IN ('ACTIVE', 'EXPIRED') counted |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent agent ID | @AgentID=99999 | Empty result set |
| 2 | No policies in period | @PeriodFrom='1900-01-01', @PeriodTo='1900-12-31' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | EffectiveDate on boundary | EffectiveDate = @PeriodFrom | Included (BETWEEN is inclusive) |
| 2 | Agent with zero policies | Agent exists but no policies in range | Agent not returned (INNER JOIN filters) |
| 3 | CommissionAmount is NULL | Policy without commission | SUM handles NULL via ISNULL or aggregate |

---

### Test Case ID: SP-RPT-005
**Procedure**: Admin.usp_Report_FinancialSummary
**Source**: `database/02-stored-procedures/009-reporting-sps.sql`
**Parameters**:
- @PeriodFrom DATE - Required
- @PeriodTo DATE - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Full financial summary | @PeriodFrom='2024-01-01', @PeriodTo='2024-12-31' | 7 rows: Written Premium, Earned Premium, Incurred Losses, Paid Losses, Outstanding Reserves, Commissions, Ceded Premium |
| 2 | Written Premium category | Valid period | SUM of WrittenPremium for ACTIVE/EXPIRED policies in period |
| 3 | Earned Premium calculation | Valid period | Pro-rata earned based on DATEDIFF days |
| 4 | Incurred Losses | Valid period | SUM of NetIncurred for claims with LossDate in period |
| 5 | Paid Losses | Valid period | SUM of Amount from Claims.Payments with status APPROVED/ISSUED/CLEARED |
| 6 | Outstanding Reserves | Valid period | SUM of TotalReserve for open claims (not date-filtered) |
| 7 | Commissions | Valid period | SUM from CommissionTransactions where TransactionType='EARNED' |
| 8 | Ceded Premium | Valid period | SUM of CededAmount from Reinsurance.Cessions where CessionType='PREMIUM' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No data in period | @PeriodFrom='1900-01-01', @PeriodTo='1900-12-31' | All amounts = NULL or 0 |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Earned premium divide-by-zero | Policy with same EffectiveDate and ExpiryDate | NULLIF(DATEDIFF(...), 0) prevents division by zero |
| 2 | LEAST function for partial term | Policy expiring before PeriodTo | Uses LEAST(@PeriodTo, ExpiryDate) for pro-rata calculation |
| 3 | Category column consistency | Any period | Returns exactly 7 UNION ALL categories |

---

### Test Case ID: SP-RPT-006
**Procedure**: Reporting.usp_Dashboard_PolicyKPIs
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (default NULL, defaults to CAST(GETDATE() AS DATE))

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | KPI summary result set | @AsOfDate=NULL | ActivePolicies, NewBusiness, Renewals, Cancellations, WrittenPremium, LossRatio |
| 2 | Expiring policies result set | @AsOfDate=NULL | TOP 20 policies expiring within 30 days, ordered by ExpiryDate |
| 3 | Recent activity result set | @AsOfDate=NULL | TOP 20 activities from last 7 days with Activity, Reference, ActivityDate |
| 4 | NewBusiness count | Specific date | Policies created within last month with PolicyStatus='ACTIVE' |
| 5 | Renewals count | Specific date | Policies with IsRenewal=1 created within last month |
| 6 | Cancellations count | Specific date | Policies cancelled (ModifiedDate) within last month |
| 7 | Loss ratio calculation | Policies with claims | TotalPaid / WrittenPremium * 100, returns 0 if no premium |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No active policies | Empty database | ActivePolicies=0, WrittenPremium=0, LossRatio=0 |
| 2 | No expiring policies | No policies expiring in 30 days | Second result set empty |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Policy expiring exactly today | ExpiryDate = @AsOfDate | Included in expiring policies (BETWEEN inclusive) |
| 2 | Policy expiring exactly 30 days out | ExpiryDate = DATEADD(DAY, 30, @AsOfDate) | Included |
| 3 | Loss ratio with zero WrittenPremium | No active policies | Returns 0 (CASE WHEN handles) |

---

### Test Case ID: SP-RPT-007
**Procedure**: Reporting.usp_Report_ClaimsByType
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DateFrom DATE - Optional (default NULL, defaults to DATEADD(YEAR, -1, GETDATE()))
- @DateTo DATE - Optional (default NULL, defaults to GETDATE())

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Default date range (last year) | @DateFrom=NULL, @DateTo=NULL | ClaimType, ClaimCount, TotalEstimatedLoss, TotalPaid, TotalReserve |
| 2 | Custom date range | @DateFrom='2024-01-01', @DateTo='2024-06-30' | Claims filtered by LossDate |
| 3 | Grouped by ClaimType | Default | One row per distinct ClaimType |
| 4 | Ordered by ClaimCount DESC | Default | Most frequent claim types first |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No claims in range | @DateFrom='1900-01-01', @DateTo='1900-12-31' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | LossDate on boundary | LossDate = @DateFrom or @DateTo | Included (BETWEEN is inclusive) |
| 2 | NULL defaults | Both NULL | Uses last year to today |

---

### Test Case ID: SP-RPT-008
**Procedure**: Reporting.usp_Report_AgentProduction
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DateFrom DATE - Optional (default NULL, defaults to DATEADD(YEAR, -1, GETDATE()))
- @DateTo DATE - Optional (default NULL, defaults to GETDATE())

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Default date range | @DateFrom=NULL, @DateTo=NULL | AgentNumber, AgentName, AgencyName, PolicyCount, TotalPremium, TotalCommission |
| 2 | Custom date range | @DateFrom='2024-01-01', @DateTo='2024-12-31' | Filtered by p.CreatedDate BETWEEN range |
| 3 | Only active agents | Default | WHERE a.IsActive = 1 |
| 4 | Ordered by TotalPremium DESC | Default | Highest producing agent first |
| 5 | Agent with no policies in range | Active agent, no policies | PolicyCount=NULL or 0, included via LEFT JOIN |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No active agents | All agents inactive | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | CreatedDate on boundary | CreatedDate = @DateFrom | Included (BETWEEN inclusive) |
| 2 | NULL defaults | Both NULL | Uses DATEADD(YEAR, -1, GETDATE()) to GETDATE() |

---

### Test Case ID: SP-RPT-009
**Procedure**: Reporting.usp_Report_BillingAging
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AsOfDate DATE - Optional (default NULL, defaults to CAST(GETDATE() AS DATE))

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Aging buckets returned | @AsOfDate=NULL | AgingBucket, InvoiceCount, TotalBalance |
| 2 | Current bucket | Invoices not yet due | DueDate >= @AsOfDate grouped as 'Current' |
| 3 | 1-30 Days bucket | DueDate 1-30 days past | Grouped as '1-30 Days' |
| 4 | 31-60 Days bucket | DueDate 31-60 days past | Grouped as '31-60 Days' |
| 5 | 61-90 Days bucket | DueDate 61-90 days past | Grouped as '61-90 Days' |
| 6 | 90+ Days bucket | DueDate over 90 days past | Grouped as '90+ Days' |
| 7 | Only open invoices | Mix of statuses | Only Status IN ('OPEN', 'OVERDUE', 'PARTIAL') with BalanceDue > 0 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No open invoices | All invoices PAID | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Invoice due today | DueDate = @AsOfDate, DATEDIFF=0 | Falls in 'Current' bucket (<=0) |
| 2 | Invoice 1 day past due | DATEDIFF(DAY, DueDate, @AsOfDate) = 1 | Falls in '1-30 Days' bucket |
| 3 | Invoice exactly 30 days past | DATEDIFF = 30 | Falls in '1-30 Days' bucket (<=30) |
| 4 | Invoice exactly 31 days past | DATEDIFF = 31 | Falls in '31-60 Days' bucket |
| 5 | BalanceDue = 0 excluded | Invoice with zero balance | Not included (WHERE BalanceDue > 0) |

---

### Test Case ID: SP-RPT-010
**Procedure**: Reporting.usp_Report_ReinsuranceSummary
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TreatyID INT - Optional (default NULL, all treaties)
- @AccountingPeriod VARCHAR(10) - Optional (default NULL, all periods)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | All treaties summary | @TreatyID=NULL, @AccountingPeriod=NULL | TreatyNumber, TreatyName, TreatyType, CessionCount, TotalGross, TotalCeded, TotalRetained |
| 2 | Single treaty | @TreatyID=1 | Only specified treaty's data |
| 3 | Specific accounting period | @AccountingPeriod='2024-Q1' | Filtered by accounting period |
| 4 | Both filters | @TreatyID=1, @AccountingPeriod='2024-Q1' | Combined filter applied |
| 5 | Treaty with no cessions | Treaty exists, no cessions | CessionCount=0, amounts=NULL (LEFT JOIN) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent treaty ID | @TreatyID=99999 | Empty result set |
| 2 | Non-existent accounting period | @AccountingPeriod='9999-Q9' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | NULL filters return all | Both NULL | No WHERE filtering applied |
| 2 | AccountingPeriod exact match | VARCHAR comparison | Exact string match required |
