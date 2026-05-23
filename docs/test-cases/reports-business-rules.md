# Reports Module - Business Rules

## Module: RPT (Reports)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-RPT-001
**Module**: RPT
**Priority**: Critical

#### Rule Description
Loss ratio is calculated as (Incurred Losses / Earned Premium) * 100, rounded to 2 decimal places. When earned premium is zero, the loss ratio returns 0 to avoid divide-by-zero errors.

#### Source
- **File**: `database/02-stored-procedures/009-reporting-sps.sql`
- **SP**: Admin.usp_Report_LossRatio
- **Code Snippet**:
```sql
CASE WHEN SUM(p.WrittenPremium) > 0 
    THEN ROUND(SUM(ISNULL(cl.NetIncurred, 0)) / SUM(p.WrittenPremium) * 100, 2) 
    ELSE 0 END AS LossRatio
```

#### Enforcement Mechanism
- Type: Database SP logic (CASE expression)
- Behavior: Returns 0 when SUM(WrittenPremium) <= 0

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-002 | usp_Report_LossRatio boundary tests |
| DV-RPT-002 | Loss Ratio data validation |
| US-RPT-004 | Loss Ratio Analysis user story |

---

### Rule ID: BR-RPT-002
**Module**: RPT
**Priority**: High

#### Rule Description
Claims aging buckets are defined as: 0-30 Days, 31-60 Days, 61-90 Days, 91-180 Days, 181-365 Days, and 365+ Days. Age is calculated as DATEDIFF(DAY, ReportedDate, @AsOfDate). Only claims with status NOT IN ('CLOSED', 'DENIED') are included.

#### Source
- **File**: `database/02-stored-procedures/009-reporting-sps.sql`
- **SP**: Admin.usp_Report_ClaimsAging
- **Code Snippet**:
```sql
CASE 
    WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 30 THEN '0-30 Days'
    WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 60 THEN '31-60 Days'
    WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 90 THEN '61-90 Days'
    WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 180 THEN '91-180 Days'
    WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 365 THEN '181-365 Days'
    ELSE '365+ Days'
END AS AgingBucket
```

#### Enforcement Mechanism
- Type: Database SP logic (CASE expression in SELECT and GROUP BY)
- Behavior: Every open claim is assigned to exactly one bucket

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-003 | usp_Report_ClaimsAging |
| DV-RPT-003 | Claims Aging data validation |
| US-RPT-005 | Claims Aging user story |

---

### Rule ID: BR-RPT-003
**Module**: RPT
**Priority**: High

#### Rule Description
Billing aging buckets are defined as: Current (not yet due), 1-30 Days, 31-60 Days, 61-90 Days, and 90+ Days past due. Age is calculated as DATEDIFF(DAY, DueDate, @AsOfDate). Only invoices with Status IN ('OPEN', 'OVERDUE', 'PARTIAL') and BalanceDue > 0 are included.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Reporting.usp_Report_BillingAging
- **Code Snippet**:
```sql
CASE 
    WHEN DATEDIFF(DAY, DueDate, @AsOfDate) <= 0 THEN 'Current'
    WHEN DATEDIFF(DAY, DueDate, @AsOfDate) <= 30 THEN '1-30 Days'
    WHEN DATEDIFF(DAY, DueDate, @AsOfDate) <= 60 THEN '31-60 Days'
    WHEN DATEDIFF(DAY, DueDate, @AsOfDate) <= 90 THEN '61-90 Days'
    ELSE '90+ Days'
END AS AgingBucket
```

#### Enforcement Mechanism
- Type: Database SP logic (CASE expression)
- Behavior: Only invoices with positive balance and open status are bucketed

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-009 | usp_Report_BillingAging |
| DV-RPT-007 | Billing Aging data validation |

---

### Rule ID: BR-RPT-004
**Module**: RPT
**Priority**: High

#### Rule Description
Earned premium is calculated on a pro-rata basis: (Days elapsed in period / Total days in policy term) * Written Premium. The LEAST function ensures the calculation does not extend beyond the reporting period end or the policy expiry date. NULLIF on the denominator prevents division by zero for zero-length terms.

#### Source
- **File**: `database/02-stored-procedures/009-reporting-sps.sql`
- **SP**: Admin.usp_Report_FinancialSummary
- **Code Snippet**:
```sql
SUM(WrittenPremium * 
    CAST(DATEDIFF(DAY, EffectiveDate, LEAST(@PeriodTo, ExpiryDate)) AS DECIMAL) / 
    NULLIF(DATEDIFF(DAY, EffectiveDate, ExpiryDate), 0))
```

#### Enforcement Mechanism
- Type: Database SP logic (arithmetic expression with NULLIF safety)
- Behavior: Returns NULL for zero-length policies, which does not affect SUM

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-005 | usp_Report_FinancialSummary boundary tests |
| DV-RPT-005 | Financial Summary data validation |
| US-RPT-007 | Financial Summary user story |

---

### Rule ID: BR-RPT-005
**Module**: RPT
**Priority**: Medium

#### Rule Description
Production report separates new business from renewals using the IsRenewal flag on policies. IsRenewal=0 counts as new business, IsRenewal=1 counts as renewal. Only policies with status ACTIVE or EXPIRED and EffectiveDate within the reporting period are included.

#### Source
- **File**: `database/02-stored-procedures/009-reporting-sps.sql`
- **SP**: Admin.usp_Report_Production
- **Code Snippet**:
```sql
SUM(CASE WHEN p.IsRenewal = 0 THEN 1 ELSE 0 END) AS NewBusinessCount,
SUM(CASE WHEN p.IsRenewal = 0 THEN p.WrittenPremium ELSE 0 END) AS NewBusinessPremium,
SUM(CASE WHEN p.IsRenewal = 1 THEN 1 ELSE 0 END) AS RenewalCount,
SUM(CASE WHEN p.IsRenewal = 1 THEN p.WrittenPremium ELSE 0 END) AS RenewalPremium,
```

#### Enforcement Mechanism
- Type: Database SP logic (conditional aggregation with CASE)
- Behavior: Each policy counted in exactly one category

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-004 | usp_Report_Production |
| DV-RPT-004 | Production Report data validation |
| US-RPT-006 | Agent Production user story |

---

### Rule ID: BR-RPT-006
**Module**: RPT
**Priority**: Medium

#### Rule Description
The Report Viewer passes NULL for filter parameters when the user selects "(All)" in dropdown filters. This tells the stored procedure to not apply that filter. State and Policy Type are the available filters.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb`
- **Method**: btnRun_Click
- **Code Snippet**:
```vb
DatabaseHelper.CreateParam("@StateCode", If(cboState.SelectedIndex > 0, cboState.SelectedItem.ToString(), Nothing)),
DatabaseHelper.CreateParam("@PolicyType", If(cboPolicyType.SelectedIndex > 0, cboPolicyType.SelectedItem.ToString(), Nothing))
```

#### Enforcement Mechanism
- Type: Application-level logic (conditional parameter assignment)
- Behavior: Nothing value becomes DBNull in SqlParameter, SP treats as "no filter"

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-RPT-007 | Report Parameter Filtering |
| UT-RPT-001 | btnRun_Click parameter handling |

---

### Rule ID: BR-RPT-007
**Module**: RPT
**Priority**: Medium

#### Rule Description
The executive dashboard excludes CLOSED and DENIED claims from the open claims summary. Loss ratio is calculated only for policies with status ACTIVE or EXPIRED that became effective within the last year from the as-of date.

#### Source
- **File**: `database/02-stored-procedures/009-reporting-sps.sql`
- **SP**: Admin.usp_Report_ExecutiveDashboard
- **Code Snippet**:
```sql
-- Claims summary
WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED');

-- Loss ratio
WHERE p.PolicyStatus IN ('ACTIVE', 'EXPIRED')
    AND p.EffectiveDate >= DATEADD(YEAR, -1, @AsOfDate);
```

#### Enforcement Mechanism
- Type: Database SP logic (WHERE clause filters)
- Behavior: Only relevant records included in aggregations

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-001 | usp_Report_ExecutiveDashboard |
| DV-RPT-001 | Executive Dashboard data validation |
| US-RPT-003 | Executive Dashboard user story |

---

### Rule ID: BR-RPT-008
**Module**: RPT
**Priority**: Medium

#### Rule Description
Policy KPIs dashboard identifies expiring policies as those with ExpiryDate between today and 30 days from today, limited to the top 20 ordered by earliest expiry. This supports proactive renewal outreach.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Reporting.usp_Dashboard_PolicyKPIs
- **Code Snippet**:
```sql
SELECT TOP 20 p.PolicyID, p.PolicyNumber, p.PolicyType, p.ExpiryDate, p.AnnualPremium,
       c.FirstName + ' ' + c.LastName AS CustomerName
FROM Policy.Policies p
WHERE p.PolicyStatus = 'ACTIVE' AND p.ExpiryDate BETWEEN @AsOfDate AND DATEADD(DAY, 30, @AsOfDate)
ORDER BY p.ExpiryDate;
```

#### Enforcement Mechanism
- Type: Database SP logic (TOP 20 with date range filter)
- Behavior: Only active policies expiring within 30 days, earliest first

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-RPT-006 | usp_Dashboard_PolicyKPIs |
| US-RPT-008 | Policy KPIs user story |
