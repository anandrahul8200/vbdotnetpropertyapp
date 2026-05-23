-- ============================================================
-- SEARCH & LOOKUP STORED PROCEDURES
-- Generic lookups, dropdown population, global search
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Get Lookup Values by Category
-- Used to populate dropdowns throughout the application
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetByCategory
    @Category VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT lv.LookupCode, lv.LookupValue, lv.DisplayOrder
    FROM Admin.LookupValues lv
    INNER JOIN Admin.LookupCategories lc ON lv.CategoryID = lc.CategoryID
    WHERE lc.CategoryName = @Category AND lv.IsActive = 1
    ORDER BY lv.DisplayOrder, lv.LookupValue;
END
GO

-- ============================================================
-- SP: Get States List (for dropdowns)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetStates
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT StateCode, StateName, TaxRate, SurchargeRate
    FROM Admin.States
    WHERE (@ActiveOnly = 0 OR IsActive = 1)
        AND GETDATE() BETWEEN EffectiveDate AND ExpiryDate
    ORDER BY StateName;
END
GO

-- ============================================================
-- SP: Get Agents List (for dropdowns/assignment)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetAgents
    @StateCode CHAR(2) = NULL,
    @AgencyID INT = NULL,
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT a.AgentID, a.AgentNumber, a.FirstName + ' ' + a.LastName AS AgentName,
           a.LicenseNumber, a.CommissionRate, ag.AgencyName
    FROM Policy.Agents a
    LEFT JOIN Policy.Agencies ag ON a.AgencyID = ag.AgencyID
    WHERE (@ActiveOnly = 0 OR a.IsActive = 1)
        AND (@StateCode IS NULL OR a.StateCode = @StateCode)
        AND (@AgencyID IS NULL OR a.AgencyID = @AgencyID)
    ORDER BY a.LastName, a.FirstName;
END
GO

-- ============================================================
-- SP: Get Coverage Types (for policy entry)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetCoverageTypes
    @PolicyType VARCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CoverageCode, CoverageName, Description, IsRequired, DefaultLimit, DefaultDeductible
    FROM Policy.CoverageTypes
    WHERE (PolicyType = @PolicyType OR PolicyType = 'ALL') AND IsActive = 1
    ORDER BY SortOrder, CoverageName;
END
GO

-- ============================================================
-- SP: Get Deductible Options (for coverage selection)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetDeductibleOptions
    @PolicyType VARCHAR(30),
    @CoverageCode VARCHAR(20),
    @StateCode CHAR(2) = NULL,
    @EffectiveDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @EffectiveDate IS NULL SET @EffectiveDate = CAST(GETDATE() AS DATE);

    SELECT DeductibleType, DeductibleAmount, DeductiblePercent, PremiumFactor, IsDefault
    FROM Underwriting.DeductibleOptions
    WHERE PolicyType = @PolicyType AND CoverageCode = @CoverageCode
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY DeductibleAmount;
END
GO

-- ============================================================
-- SP: Global Search (search across all entities)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Search_Global
    @SearchTerm VARCHAR(100),
    @MaxResults INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    -- Search customers
    SELECT TOP (@MaxResults) 'CUSTOMER' AS EntityType, CustomerID AS EntityID,
           CustomerNumber AS EntityNumber, 
           ISNULL(FirstName + ' ' + LastName, CompanyName) AS DisplayName,
           'Customer' AS Category
    FROM Policy.Customers
    WHERE CustomerNumber LIKE '%' + @SearchTerm + '%'
        OR FirstName LIKE '%' + @SearchTerm + '%'
        OR LastName LIKE '%' + @SearchTerm + '%'
        OR CompanyName LIKE '%' + @SearchTerm + '%'

    UNION ALL

    -- Search policies
    SELECT TOP (@MaxResults) 'POLICY', PolicyID, PolicyNumber,
           PolicyNumber + ' (' + PolicyType + ' - ' + PolicyStatus + ')',
           'Policy'
    FROM Policy.Policies
    WHERE PolicyNumber LIKE '%' + @SearchTerm + '%'

    UNION ALL

    -- Search claims
    SELECT TOP (@MaxResults) 'CLAIM', ClaimID, ClaimNumber,
           ClaimNumber + ' (' + ClaimType + ' - ' + ClaimStatus + ')',
           'Claim'
    FROM Claims.Claims
    WHERE ClaimNumber LIKE '%' + @SearchTerm + '%'

    UNION ALL

    -- Search properties by address
    SELECT TOP (@MaxResults) 'PROPERTY', PropertyID, PropertyNumber,
           AddressLine1 + ', ' + City + ' ' + StateCode,
           'Property'
    FROM Policy.Properties
    WHERE PropertyNumber LIKE '%' + @SearchTerm + '%'
        OR AddressLine1 LIKE '%' + @SearchTerm + '%'

    ORDER BY EntityType, EntityNumber;
END
GO

-- ============================================================
-- SP: Get Payment Plans (for policy entry)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetPaymentPlans
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PlanCode, PlanName, NumberOfInstallments, DownPaymentPercent, InstallmentFee
    FROM Billing.PaymentPlans
    WHERE IsActive = 1
    ORDER BY NumberOfInstallments;
END
GO

-- ============================================================
-- SP: Get Catastrophe Events (for claim entry)
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_GetCatastrophes
    @ActiveOnly BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CatastropheID, CatastropheNumber, CatastropheName, CatastropheType,
           EventDate, AffectedStates, TotalClaimsCount
    FROM Claims.Catastrophes
    WHERE (@ActiveOnly = 0 OR IsActive = 1)
    ORDER BY EventDate DESC;
END
GO

-- ============================================================
-- SP: Policy Search (advanced)
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Policy_Search
    @PolicyNumber VARCHAR(20) = NULL,
    @CustomerName VARCHAR(200) = NULL,
    @PolicyType VARCHAR(30) = NULL,
    @PolicyStatus VARCHAR(20) = NULL,
    @StateCode CHAR(2) = NULL,
    @AgentID INT = NULL,
    @EffectiveDateFrom DATE = NULL,
    @EffectiveDateTo DATE = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortColumn VARCHAR(50) = 'PolicyNumber',
    @SortDirection VARCHAR(4) = 'ASC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX), @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    DECLARE @Params NVARCHAR(MAX);

    IF @PolicyNumber IS NOT NULL
        SET @WhereClause += ' AND p.PolicyNumber LIKE ''%'' + @PolicyNumber + ''%'' ';
    IF @CustomerName IS NOT NULL
        SET @WhereClause += ' AND (c.FirstName + '' '' + c.LastName LIKE ''%'' + @CustomerName + ''%'' OR c.CompanyName LIKE ''%'' + @CustomerName + ''%'') ';
    IF @PolicyType IS NOT NULL
        SET @WhereClause += ' AND p.PolicyType = @PolicyType ';
    IF @PolicyStatus IS NOT NULL
        SET @WhereClause += ' AND p.PolicyStatus = @PolicyStatus ';
    IF @StateCode IS NOT NULL
        SET @WhereClause += ' AND pr.StateCode = @StateCode ';
    IF @AgentID IS NOT NULL
        SET @WhereClause += ' AND p.AgentID = @AgentID ';
    IF @EffectiveDateFrom IS NOT NULL
        SET @WhereClause += ' AND p.EffectiveDate >= @EffectiveDateFrom ';
    IF @EffectiveDateTo IS NOT NULL
        SET @WhereClause += ' AND p.EffectiveDate <= @EffectiveDateTo ';

    SET @CountSQL = '
        SELECT @TotalRecords = COUNT(*)
        FROM Policy.Policies p
        INNER JOIN Policy.Customers c ON p.CustomerID = c.CustomerID
        INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID '
        + @WhereClause;

    SET @Params = '@PolicyNumber VARCHAR(20), @CustomerName VARCHAR(200), @PolicyType VARCHAR(30), @PolicyStatus VARCHAR(20), @StateCode CHAR(2), @AgentID INT, @EffectiveDateFrom DATE, @EffectiveDateTo DATE, @TotalRecords INT OUTPUT';
    EXEC sp_executesql @CountSQL, @Params, @PolicyNumber, @CustomerName, @PolicyType, @PolicyStatus, @StateCode, @AgentID, @EffectiveDateFrom, @EffectiveDateTo, @TotalRecords OUTPUT;

    SET @SQL = '
        SELECT p.PolicyID, p.PolicyNumber, p.PolicyType, p.PolicyStatus, p.EffectiveDate, p.ExpiryDate,
               p.GrossPremium, p.TotalInsuredValue,
               c.CustomerNumber, c.FirstName, c.LastName, c.CompanyName,
               pr.StateCode, pr.City, pr.AddressLine1,
               a.FirstName + '' '' + a.LastName AS AgentName
        FROM Policy.Policies p
        INNER JOIN Policy.Customers c ON p.CustomerID = c.CustomerID
        INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
        INNER JOIN Policy.Agents a ON p.AgentID = a.AgentID '
        + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + CASE WHEN @SortDirection = 'DESC' THEN 'DESC' ELSE 'ASC' END + '
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY';

    SET @Params = '@PolicyNumber VARCHAR(20), @CustomerName VARCHAR(200), @PolicyType VARCHAR(30), @PolicyStatus VARCHAR(20), @StateCode CHAR(2), @AgentID INT, @EffectiveDateFrom DATE, @EffectiveDateTo DATE, @PageNumber INT, @PageSize INT';
    EXEC sp_executesql @SQL, @Params, @PolicyNumber, @CustomerName, @PolicyType, @PolicyStatus, @StateCode, @AgentID, @EffectiveDateFrom, @EffectiveDateTo, @PageNumber, @PageSize;
END
GO
