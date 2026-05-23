-- ============================================================
-- REPORTING STORED PROCEDURES
-- Executive dashboards, loss ratios, production reports,
-- claims aging, financial summaries
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Executive Dashboard Summary
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Report_ExecutiveDashboard
    @AsOfDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @AsOfDate IS NULL SET @AsOfDate = CAST(GETDATE() AS DATE);

    -- Policy counts by status
    SELECT PolicyStatus, COUNT(*) AS PolicyCount, SUM(GrossPremium) AS TotalPremium
    FROM Policy.Policies
    GROUP BY PolicyStatus;

    -- Written premium by month (last 12 months)
    SELECT FORMAT(CreatedDate, 'yyyy-MM') AS Month,
           COUNT(*) AS PoliciesWritten,
           SUM(WrittenPremium) AS WrittenPremium
    FROM Policy.Policies
    WHERE PolicyStatus IN ('ACTIVE', 'EXPIRED', 'CANCELLED')
        AND CreatedDate >= DATEADD(MONTH, -12, @AsOfDate)
    GROUP BY FORMAT(CreatedDate, 'yyyy-MM')
    ORDER BY Month;

    -- Claims summary
    SELECT 
        COUNT(*) AS TotalOpenClaims,
        SUM(TotalReserve) AS TotalOutstandingReserve,
        SUM(TotalPaid) AS TotalPaidYTD,
        SUM(NetIncurred) AS TotalIncurred
    FROM Claims.Claims
    WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED');

    -- Loss ratio (earned premium vs incurred losses)
    SELECT 
        SUM(p.WrittenPremium) AS EarnedPremium,
        SUM(cl.NetIncurred) AS IncurredLosses,
        CASE WHEN SUM(p.WrittenPremium) > 0 
            THEN ROUND(SUM(cl.NetIncurred) / SUM(p.WrittenPremium) * 100, 2) 
            ELSE 0 END AS LossRatio
    FROM Policy.Policies p
    LEFT JOIN Claims.Claims cl ON p.PolicyID = cl.PolicyID
    WHERE p.PolicyStatus IN ('ACTIVE', 'EXPIRED')
        AND p.EffectiveDate >= DATEADD(YEAR, -1, @AsOfDate);

    -- Outstanding billing
    SELECT 
        COUNT(*) AS OverdueInvoices,
        SUM(BalanceDue) AS TotalOverdue
    FROM Billing.Invoices
    WHERE Status IN ('OVERDUE', 'PARTIAL');
END
GO

-- ============================================================
-- SP: Loss Ratio Report by Policy Type/State
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Report_LossRatio
    @PeriodFrom DATE,
    @PeriodTo DATE,
    @GroupBy VARCHAR(20) = 'POLICY_TYPE' -- POLICY_TYPE, STATE, AGENT, MONTH
AS
BEGIN
    SET NOCOUNT ON;

    IF @GroupBy = 'POLICY_TYPE'
    BEGIN
        SELECT p.PolicyType,
               COUNT(DISTINCT p.PolicyID) AS PolicyCount,
               SUM(p.WrittenPremium) AS EarnedPremium,
               SUM(ISNULL(cl.NetIncurred, 0)) AS IncurredLosses,
               CASE WHEN SUM(p.WrittenPremium) > 0 
                   THEN ROUND(SUM(ISNULL(cl.NetIncurred, 0)) / SUM(p.WrittenPremium) * 100, 2) 
                   ELSE 0 END AS LossRatio
        FROM Policy.Policies p
        LEFT JOIN Claims.Claims cl ON p.PolicyID = cl.PolicyID AND cl.LossDate BETWEEN @PeriodFrom AND @PeriodTo
        WHERE p.EffectiveDate <= @PeriodTo AND p.ExpiryDate >= @PeriodFrom
        GROUP BY p.PolicyType;
    END
    ELSE IF @GroupBy = 'STATE'
    BEGIN
        SELECT pr.StateCode,
               COUNT(DISTINCT p.PolicyID) AS PolicyCount,
               SUM(p.WrittenPremium) AS EarnedPremium,
               SUM(ISNULL(cl.NetIncurred, 0)) AS IncurredLosses,
               CASE WHEN SUM(p.WrittenPremium) > 0 
                   THEN ROUND(SUM(ISNULL(cl.NetIncurred, 0)) / SUM(p.WrittenPremium) * 100, 2) 
                   ELSE 0 END AS LossRatio
        FROM Policy.Policies p
        INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
        LEFT JOIN Claims.Claims cl ON p.PolicyID = cl.PolicyID AND cl.LossDate BETWEEN @PeriodFrom AND @PeriodTo
        WHERE p.EffectiveDate <= @PeriodTo AND p.ExpiryDate >= @PeriodFrom
        GROUP BY pr.StateCode
        ORDER BY LossRatio DESC;
    END
    ELSE IF @GroupBy = 'AGENT'
    BEGIN
        SELECT a.AgentNumber, a.FirstName + ' ' + a.LastName AS AgentName,
               COUNT(DISTINCT p.PolicyID) AS PolicyCount,
               SUM(p.WrittenPremium) AS EarnedPremium,
               SUM(ISNULL(cl.NetIncurred, 0)) AS IncurredLosses,
               CASE WHEN SUM(p.WrittenPremium) > 0 
                   THEN ROUND(SUM(ISNULL(cl.NetIncurred, 0)) / SUM(p.WrittenPremium) * 100, 2) 
                   ELSE 0 END AS LossRatio
        FROM Policy.Policies p
        INNER JOIN Policy.Agents a ON p.AgentID = a.AgentID
        LEFT JOIN Claims.Claims cl ON p.PolicyID = cl.PolicyID AND cl.LossDate BETWEEN @PeriodFrom AND @PeriodTo
        WHERE p.EffectiveDate <= @PeriodTo AND p.ExpiryDate >= @PeriodFrom
        GROUP BY a.AgentNumber, a.FirstName, a.LastName
        ORDER BY LossRatio DESC;
    END
END
GO

-- ============================================================
-- SP: Claims Aging Report
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Report_ClaimsAging
    @AsOfDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @AsOfDate IS NULL SET @AsOfDate = CAST(GETDATE() AS DATE);

    SELECT 
        CASE 
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 30 THEN '0-30 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 60 THEN '31-60 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 90 THEN '61-90 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 180 THEN '91-180 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 365 THEN '181-365 Days'
            ELSE '365+ Days'
        END AS AgingBucket,
        COUNT(*) AS ClaimCount,
        SUM(cl.TotalReserve) AS TotalReserve,
        SUM(cl.TotalPaid) AS TotalPaid,
        SUM(cl.NetIncurred) AS TotalIncurred,
        AVG(cl.NetIncurred) AS AvgIncurred
    FROM Claims.Claims cl
    WHERE cl.ClaimStatus NOT IN ('CLOSED', 'DENIED')
    GROUP BY 
        CASE 
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 30 THEN '0-30 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 60 THEN '31-60 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 90 THEN '61-90 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 180 THEN '91-180 Days'
            WHEN DATEDIFF(DAY, cl.ReportedDate, @AsOfDate) <= 365 THEN '181-365 Days'
            ELSE '365+ Days'
        END
    ORDER BY MIN(DATEDIFF(DAY, cl.ReportedDate, @AsOfDate));
END
GO

-- ============================================================
-- SP: Production Report (new business/renewals by agent)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Report_Production
    @PeriodFrom DATE,
    @PeriodTo DATE,
    @AgentID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.AgentNumber, a.FirstName + ' ' + a.LastName AS AgentName,
        ag.AgencyName,
        SUM(CASE WHEN p.IsRenewal = 0 THEN 1 ELSE 0 END) AS NewBusinessCount,
        SUM(CASE WHEN p.IsRenewal = 0 THEN p.WrittenPremium ELSE 0 END) AS NewBusinessPremium,
        SUM(CASE WHEN p.IsRenewal = 1 THEN 1 ELSE 0 END) AS RenewalCount,
        SUM(CASE WHEN p.IsRenewal = 1 THEN p.WrittenPremium ELSE 0 END) AS RenewalPremium,
        COUNT(*) AS TotalPolicies,
        SUM(p.WrittenPremium) AS TotalPremium,
        SUM(p.CommissionAmount) AS TotalCommission
    FROM Policy.Policies p
    INNER JOIN Policy.Agents a ON p.AgentID = a.AgentID
    LEFT JOIN Policy.Agencies ag ON a.AgencyID = ag.AgencyID
    WHERE p.PolicyStatus IN ('ACTIVE', 'EXPIRED')
        AND p.EffectiveDate BETWEEN @PeriodFrom AND @PeriodTo
        AND (@AgentID IS NULL OR p.AgentID = @AgentID)
    GROUP BY a.AgentNumber, a.FirstName, a.LastName, ag.AgencyName
    ORDER BY TotalPremium DESC;
END
GO

-- ============================================================
-- SP: Financial Summary Report
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Report_FinancialSummary
    @PeriodFrom DATE,
    @PeriodTo DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Premium summary
    SELECT 'Written Premium' AS Category,
           SUM(WrittenPremium) AS Amount
    FROM Policy.Policies
    WHERE EffectiveDate BETWEEN @PeriodFrom AND @PeriodTo AND PolicyStatus IN ('ACTIVE', 'EXPIRED')
    UNION ALL
    SELECT 'Earned Premium',
           SUM(WrittenPremium * 
               CAST(DATEDIFF(DAY, EffectiveDate, LEAST(@PeriodTo, ExpiryDate)) AS DECIMAL) / 
               NULLIF(DATEDIFF(DAY, EffectiveDate, ExpiryDate), 0))
    FROM Policy.Policies
    WHERE PolicyStatus IN ('ACTIVE', 'EXPIRED') AND EffectiveDate <= @PeriodTo AND ExpiryDate >= @PeriodFrom
    UNION ALL
    SELECT 'Incurred Losses',
           SUM(NetIncurred)
    FROM Claims.Claims
    WHERE LossDate BETWEEN @PeriodFrom AND @PeriodTo
    UNION ALL
    SELECT 'Paid Losses',
           SUM(Amount)
    FROM Claims.Payments
    WHERE Status IN ('APPROVED', 'ISSUED', 'CLEARED') AND CreatedDate BETWEEN @PeriodFrom AND @PeriodTo
    UNION ALL
    SELECT 'Outstanding Reserves',
           SUM(TotalReserve)
    FROM Claims.Claims
    WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED')
    UNION ALL
    SELECT 'Commissions',
           SUM(CommissionAmount)
    FROM Billing.CommissionTransactions
    WHERE TransactionDate BETWEEN @PeriodFrom AND @PeriodTo AND TransactionType = 'EARNED'
    UNION ALL
    SELECT 'Ceded Premium (Reinsurance)',
           SUM(CededAmount)
    FROM Reinsurance.Cessions
    WHERE CessionType = 'PREMIUM' AND TransactionDate BETWEEN @PeriodFrom AND @PeriodTo;
END
GO
