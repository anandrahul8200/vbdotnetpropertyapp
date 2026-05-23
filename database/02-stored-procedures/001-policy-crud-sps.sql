-- ============================================================
-- POLICY CRUD STORED PROCEDURES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Create Customer
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Customer_Create
    @CustomerType CHAR(1),
    @Title VARCHAR(10) = NULL,
    @FirstName VARCHAR(100) = NULL,
    @LastName VARCHAR(100) = NULL,
    @CompanyName VARCHAR(200) = NULL,
    @TaxID VARCHAR(20) = NULL,
    @DateOfBirth DATE = NULL,
    @Gender CHAR(1) = NULL,
    @Email VARCHAR(200) = NULL,
    @Phone VARCHAR(20) = NULL,
    @MobilePhone VARCHAR(20) = NULL,
    @AddressLine1 VARCHAR(200),
    @AddressLine2 VARCHAR(200) = NULL,
    @City VARCHAR(100),
    @StateCode CHAR(2),
    @ZipCode VARCHAR(10),
    @County VARCHAR(100) = NULL,
    @Occupation VARCHAR(100) = NULL,
    @AnnualIncome DECIMAL(18,2) = NULL,
    @CreditScore INT = NULL,
    @CreatedBy VARCHAR(50),
    @CustomerID INT OUTPUT,
    @CustomerNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Generate customer number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(CustomerID), 0) + 1 FROM Policy.Customers;
        SET @CustomerNumber = 'CUS' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Determine risk tier based on credit score
        DECLARE @RiskTier VARCHAR(20) = 'STANDARD';
        IF @CreditScore IS NOT NULL
        BEGIN
            IF @CreditScore >= 750 SET @RiskTier = 'PREFERRED';
            ELSE IF @CreditScore >= 650 SET @RiskTier = 'STANDARD';
            ELSE SET @RiskTier = 'SUBSTANDARD';
        END

        INSERT INTO Policy.Customers (
            CustomerNumber, CustomerType, Title, FirstName, LastName, CompanyName,
            TaxID, DateOfBirth, Gender, Email, Phone, MobilePhone,
            AddressLine1, AddressLine2, City, StateCode, ZipCode, County,
            Occupation, AnnualIncome, CreditScore, RiskTier,
            CustomerSince, IsActive, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
        ) VALUES (
            @CustomerNumber, @CustomerType, @Title, @FirstName, @LastName, @CompanyName,
            @TaxID, @DateOfBirth, @Gender, @Email, @Phone, @MobilePhone,
            @AddressLine1, @AddressLine2, @City, @StateCode, @ZipCode, @County,
            @Occupation, @AnnualIncome, @CreditScore, @RiskTier,
            GETDATE(), 1, GETDATE(), @CreatedBy, GETDATE(), @CreatedBy
        );

        SET @CustomerID = SCOPE_IDENTITY();

        -- Audit log
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Policy.Customers', @CustomerID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Update Customer
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Customer_Update
    @CustomerID INT,
    @Title VARCHAR(10) = NULL,
    @FirstName VARCHAR(100) = NULL,
    @LastName VARCHAR(100) = NULL,
    @CompanyName VARCHAR(200) = NULL,
    @Email VARCHAR(200) = NULL,
    @Phone VARCHAR(20) = NULL,
    @MobilePhone VARCHAR(20) = NULL,
    @AddressLine1 VARCHAR(200) = NULL,
    @AddressLine2 VARCHAR(200) = NULL,
    @City VARCHAR(100) = NULL,
    @StateCode CHAR(2) = NULL,
    @ZipCode VARCHAR(10) = NULL,
    @County VARCHAR(100) = NULL,
    @Occupation VARCHAR(100) = NULL,
    @AnnualIncome DECIMAL(18,2) = NULL,
    @CreditScore INT = NULL,
    @ModifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Validate customer exists
        IF NOT EXISTS (SELECT 1 FROM Policy.Customers WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR('Customer not found: %d', 16, 1, @CustomerID);
            RETURN;
        END

        -- Track changes for audit
        DECLARE @OldValues TABLE (FieldName VARCHAR(100), OldValue VARCHAR(500), NewValue VARCHAR(500));

        INSERT INTO @OldValues
        SELECT 'Email', Email, @Email FROM Policy.Customers WHERE CustomerID = @CustomerID AND Email <> ISNULL(@Email, Email)
        UNION ALL
        SELECT 'Phone', Phone, @Phone FROM Policy.Customers WHERE CustomerID = @CustomerID AND Phone <> ISNULL(@Phone, Phone)
        UNION ALL
        SELECT 'AddressLine1', AddressLine1, @AddressLine1 FROM Policy.Customers WHERE CustomerID = @CustomerID AND AddressLine1 <> ISNULL(@AddressLine1, AddressLine1);

        -- Update
        UPDATE Policy.Customers SET
            Title = ISNULL(@Title, Title),
            FirstName = ISNULL(@FirstName, FirstName),
            LastName = ISNULL(@LastName, LastName),
            CompanyName = ISNULL(@CompanyName, CompanyName),
            Email = ISNULL(@Email, Email),
            Phone = ISNULL(@Phone, Phone),
            MobilePhone = ISNULL(@MobilePhone, MobilePhone),
            AddressLine1 = ISNULL(@AddressLine1, AddressLine1),
            AddressLine2 = ISNULL(@AddressLine2, AddressLine2),
            City = ISNULL(@City, City),
            StateCode = ISNULL(@StateCode, StateCode),
            ZipCode = ISNULL(@ZipCode, ZipCode),
            County = ISNULL(@County, County),
            Occupation = ISNULL(@Occupation, Occupation),
            AnnualIncome = ISNULL(@AnnualIncome, AnnualIncome),
            CreditScore = ISNULL(@CreditScore, CreditScore),
            ModifiedDate = GETDATE(),
            ModifiedBy = @ModifiedBy
        WHERE CustomerID = @CustomerID;

        -- Audit changes
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
        SELECT 'Policy.Customers', @CustomerID, 'UPDATE', FieldName, OldValue, NewValue, @ModifiedBy, GETDATE()
        FROM @OldValues;

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get Customer By ID
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Customer_GetByID
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.*,
        s.StateName,
        (SELECT COUNT(*) FROM Policy.Policies p WHERE p.CustomerID = c.CustomerID AND p.PolicyStatus = 'ACTIVE') AS ActivePolicyCount,
        (SELECT COUNT(*) FROM Claims.Claims cl WHERE cl.CustomerID = c.CustomerID AND cl.ClaimStatus NOT IN ('CLOSED', 'DENIED')) AS OpenClaimCount
    FROM Policy.Customers c
    LEFT JOIN Admin.States s ON c.StateCode = s.StateCode
    WHERE c.CustomerID = @CustomerID;
END
GO

-- ============================================================
-- SP: Search Customers
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Customer_Search
    @SearchTerm VARCHAR(100) = NULL,
    @CustomerNumber VARCHAR(20) = NULL,
    @CustomerType CHAR(1) = NULL,
    @StateCode CHAR(2) = NULL,
    @City VARCHAR(100) = NULL,
    @ZipCode VARCHAR(10) = NULL,
    @IsActive BIT = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortColumn VARCHAR(50) = 'CustomerNumber',
    @SortDirection VARCHAR(4) = 'ASC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Dynamic search with pagination
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    DECLARE @Params NVARCHAR(MAX);

    IF @SearchTerm IS NOT NULL
        SET @WhereClause += ' AND (c.FirstName LIKE ''%'' + @SearchTerm + ''%'' OR c.LastName LIKE ''%'' + @SearchTerm + ''%'' OR c.CompanyName LIKE ''%'' + @SearchTerm + ''%'' OR c.CustomerNumber LIKE ''%'' + @SearchTerm + ''%'') ';
    IF @CustomerNumber IS NOT NULL
        SET @WhereClause += ' AND c.CustomerNumber = @CustomerNumber ';
    IF @CustomerType IS NOT NULL
        SET @WhereClause += ' AND c.CustomerType = @CustomerType ';
    IF @StateCode IS NOT NULL
        SET @WhereClause += ' AND c.StateCode = @StateCode ';
    IF @City IS NOT NULL
        SET @WhereClause += ' AND c.City LIKE ''%'' + @City + ''%'' ';
    IF @ZipCode IS NOT NULL
        SET @WhereClause += ' AND c.ZipCode LIKE @ZipCode + ''%'' ';
    IF @IsActive IS NOT NULL
        SET @WhereClause += ' AND c.IsActive = @IsActive ';

    -- Count total
    SET @CountSQL = 'SELECT @TotalRecords = COUNT(*) FROM Policy.Customers c ' + @WhereClause;
    SET @Params = '@SearchTerm VARCHAR(100), @CustomerNumber VARCHAR(20), @CustomerType CHAR(1), @StateCode CHAR(2), @City VARCHAR(100), @ZipCode VARCHAR(10), @IsActive BIT, @TotalRecords INT OUTPUT';
    
    EXEC sp_executesql @CountSQL, @Params, @SearchTerm, @CustomerNumber, @CustomerType, @StateCode, @City, @ZipCode, @IsActive, @TotalRecords OUTPUT;

    -- Get page
    SET @SQL = '
        SELECT c.*, 
            (SELECT COUNT(*) FROM Policy.Policies p WHERE p.CustomerID = c.CustomerID AND p.PolicyStatus = ''ACTIVE'') AS ActivePolicyCount
        FROM Policy.Customers c ' + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + CASE WHEN @SortDirection = 'DESC' THEN 'DESC' ELSE 'ASC' END + '
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY';

    SET @Params = '@SearchTerm VARCHAR(100), @CustomerNumber VARCHAR(20), @CustomerType CHAR(1), @StateCode CHAR(2), @City VARCHAR(100), @ZipCode VARCHAR(10), @IsActive BIT, @PageNumber INT, @PageSize INT';
    
    EXEC sp_executesql @SQL, @Params, @SearchTerm, @CustomerNumber, @CustomerType, @StateCode, @City, @ZipCode, @IsActive, @PageNumber, @PageSize;
END
GO

-- ============================================================
-- SP: Create Property
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Property_Create
    @CustomerID INT,
    @PropertyType VARCHAR(30),
    @ConstructionType VARCHAR(30),
    @OccupancyType VARCHAR(30),
    @YearBuilt INT,
    @SquareFootage INT,
    @NumberOfStories INT = 1,
    @RoofType VARCHAR(30) = NULL,
    @RoofAge INT = NULL,
    @HasBasement BIT = 0,
    @HasPool BIT = 0,
    @HasFireAlarm BIT = 0,
    @HasBurglarAlarm BIT = 0,
    @HasSprinklerSystem BIT = 0,
    @DistanceToFireStation DECIMAL(6,2) = NULL,
    @DistanceToHydrant DECIMAL(6,2) = NULL,
    @FireProtectionClass INT = NULL,
    @FloodZone VARCHAR(10) = NULL,
    @MarketValue DECIMAL(18,2) = NULL,
    @ReplacementCost DECIMAL(18,2) = NULL,
    @AddressLine1 VARCHAR(200),
    @AddressLine2 VARCHAR(200) = NULL,
    @City VARCHAR(100),
    @StateCode CHAR(2),
    @ZipCode VARCHAR(10),
    @County VARCHAR(100) = NULL,
    @CreatedBy VARCHAR(50),
    @PropertyID INT OUTPUT,
    @PropertyNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate customer
        IF NOT EXISTS (SELECT 1 FROM Policy.Customers WHERE CustomerID = @CustomerID AND IsActive = 1)
        BEGIN
            RAISERROR('Active customer not found: %d', 16, 1, @CustomerID);
            RETURN;
        END

        -- Generate property number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(PropertyID), 0) + 1 FROM Policy.Properties;
        SET @PropertyNumber = 'PRP' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Determine fire protection class if not provided
        IF @FireProtectionClass IS NULL
        BEGIN
            SET @FireProtectionClass = 
                CASE 
                    WHEN @DistanceToFireStation <= 1.0 AND @DistanceToHydrant <= 0.25 THEN 3
                    WHEN @DistanceToFireStation <= 3.0 AND @DistanceToHydrant <= 0.5 THEN 5
                    WHEN @DistanceToFireStation <= 5.0 THEN 7
                    ELSE 9
                END;
        END

        -- Determine territory
        DECLARE @TerritoryID INT;
        SELECT TOP 1 @TerritoryID = TerritoryID 
        FROM Policy.Territories 
        WHERE StateCode = @StateCode 
            AND @ZipCode BETWEEN ZipCodeRangeStart AND ZipCodeRangeEnd
            AND IsActive = 1;

        INSERT INTO Policy.Properties (
            CustomerID, PropertyNumber, PropertyType, ConstructionType, OccupancyType,
            YearBuilt, SquareFootage, NumberOfStories, RoofType, RoofAge,
            HasBasement, HasPool, HasFireAlarm, HasBurglarAlarm, HasSprinklerSystem,
            DistanceToFireStation, DistanceToHydrant, FireProtectionClass, FloodZone,
            MarketValue, ReplacementCost, AddressLine1, AddressLine2,
            City, StateCode, ZipCode, County, TerritoryID,
            IsActive, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
        ) VALUES (
            @CustomerID, @PropertyNumber, @PropertyType, @ConstructionType, @OccupancyType,
            @YearBuilt, @SquareFootage, @NumberOfStories, @RoofType, @RoofAge,
            @HasBasement, @HasPool, @HasFireAlarm, @HasBurglarAlarm, @HasSprinklerSystem,
            @DistanceToFireStation, @DistanceToHydrant, @FireProtectionClass, @FloodZone,
            @MarketValue, @ReplacementCost, @AddressLine1, @AddressLine2,
            @City, @StateCode, @ZipCode, @County, @TerritoryID,
            1, GETDATE(), @CreatedBy, GETDATE(), @CreatedBy
        );

        SET @PropertyID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Policy.Properties', @PropertyID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Create Policy (Initial Quote)
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Policy_CreateQuote
    @PolicyType VARCHAR(30),
    @CustomerID INT,
    @PropertyID INT,
    @AgentID INT,
    @EffectiveDate DATE,
    @TermMonths INT = 12,
    @PaymentPlan VARCHAR(20) = 'ANNUAL',
    @BillingMethod VARCHAR(20) = 'DIRECT',
    @PriorCarrier VARCHAR(100) = NULL,
    @PriorPolicyNumber VARCHAR(50) = NULL,
    @PriorExpiryDate DATE = NULL,
    @YearsWithPriorCarrier INT = NULL,
    @ClaimFreeYears INT = 0,
    @CreatedBy VARCHAR(50),
    @PolicyID INT OUTPUT,
    @PolicyNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate inputs
        IF NOT EXISTS (SELECT 1 FROM Policy.Customers WHERE CustomerID = @CustomerID AND IsActive = 1)
            RAISERROR('Active customer not found', 16, 1);
        IF NOT EXISTS (SELECT 1 FROM Policy.Properties WHERE PropertyID = @PropertyID AND CustomerID = @CustomerID AND IsActive = 1)
            RAISERROR('Property not found or does not belong to customer', 16, 1);
        IF NOT EXISTS (SELECT 1 FROM Policy.Agents WHERE AgentID = @AgentID AND IsActive = 1)
            RAISERROR('Active agent not found', 16, 1);

        -- Check moratorium
        DECLARE @PropertyState CHAR(2), @PropertyZip VARCHAR(10);
        SELECT @PropertyState = StateCode, @PropertyZip = ZipCode FROM Policy.Properties WHERE PropertyID = @PropertyID;

        IF EXISTS (
            SELECT 1 FROM Underwriting.Moratoriums 
            WHERE IsActive = 1 
                AND MoratoriumType IN ('NEW_BUSINESS', 'ALL')
                AND GETDATE() BETWEEN StartDate AND ISNULL(EndDate, '9999-12-31')
                AND (AffectedStates LIKE '%' + @PropertyState + '%' OR AffectedStates IS NULL)
                AND (AffectedPolicyTypes LIKE '%' + @PolicyType + '%' OR AffectedPolicyTypes IS NULL)
        )
        BEGIN
            RAISERROR('New business moratorium is in effect for this location/policy type', 16, 1);
            RETURN;
        END

        -- Generate policy number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(PolicyID), 0) + 1 FROM Policy.Policies;
        SET @PolicyNumber = 'POL' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Calculate expiry date
        DECLARE @ExpiryDate DATE = DATEADD(MONTH, @TermMonths, @EffectiveDate);

        -- Get agent commission rate
        DECLARE @CommissionRate DECIMAL(6,4);
        SELECT @CommissionRate = CommissionRate FROM Policy.Agents WHERE AgentID = @AgentID;

        INSERT INTO Policy.Policies (
            PolicyNumber, PolicyVersion, PolicyType, PolicyStatus,
            CustomerID, PropertyID, AgentID,
            EffectiveDate, ExpiryDate, OriginalEffectiveDate, TermMonths,
            PaymentPlan, BillingMethod, CommissionRate,
            PriorCarrier, PriorPolicyNumber, PriorExpiryDate, YearsWithPriorCarrier,
            ClaimFreeYears, IsRenewal, RenewalCount,
            CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
        ) VALUES (
            @PolicyNumber, 1, @PolicyType, 'QUOTE',
            @CustomerID, @PropertyID, @AgentID,
            @EffectiveDate, @ExpiryDate, @EffectiveDate, @TermMonths,
            @PaymentPlan, @BillingMethod, @CommissionRate,
            @PriorCarrier, @PriorPolicyNumber, @PriorExpiryDate, @YearsWithPriorCarrier,
            @ClaimFreeYears, 0, 0,
            GETDATE(), @CreatedBy, GETDATE(), @CreatedBy
        );

        SET @PolicyID = SCOPE_IDENTITY();

        -- Create version record
        INSERT INTO Policy.PolicyVersions (PolicyID, VersionNumber, VersionType, EffectiveDate, ProcessedBy)
        VALUES (@PolicyID, 1, 'NEW', @EffectiveDate, @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Policy.Policies', @PolicyID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get Policy Details (full view)
-- ============================================================
CREATE OR ALTER PROCEDURE Policy.usp_Policy_GetDetails
    @PolicyID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Policy header
    SELECT 
        p.*,
        c.CustomerNumber, c.FirstName, c.LastName, c.CompanyName, c.CustomerType,
        pr.PropertyNumber, pr.PropertyType, pr.AddressLine1 AS PropertyAddress, pr.City AS PropertyCity, pr.StateCode AS PropertyState, pr.ZipCode AS PropertyZip,
        a.AgentNumber, a.FirstName AS AgentFirstName, a.LastName AS AgentLastName,
        ag.AgencyName
    FROM Policy.Policies p
    INNER JOIN Policy.Customers c ON p.CustomerID = c.CustomerID
    INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
    INNER JOIN Policy.Agents a ON p.AgentID = a.AgentID
    LEFT JOIN Policy.Agencies ag ON a.AgencyID = ag.AgencyID
    WHERE p.PolicyID = @PolicyID;

    -- Coverages
    SELECT cv.*, 
        (SELECT COUNT(*) FROM Policy.CoveragePerils cp WHERE cp.CoverageID = cv.CoverageID AND cp.IsIncluded = 1) AS PerilCount
    FROM Policy.Coverages cv
    WHERE cv.PolicyID = @PolicyID AND cv.IsSelected = 1
    ORDER BY cv.CoverageCode;

    -- Endorsements
    SELECT e.*
    FROM Policy.Endorsements e
    WHERE e.PolicyID = @PolicyID
    ORDER BY e.EffectiveDate DESC;

    -- Claims summary
    SELECT 
        cl.ClaimNumber, cl.ClaimStatus, cl.ClaimType, cl.LossDate, 
        cl.TotalPaid, cl.TotalReserve, cl.NetIncurred
    FROM Claims.Claims cl
    WHERE cl.PolicyID = @PolicyID
    ORDER BY cl.LossDate DESC;

    -- Billing summary
    SELECT 
        i.InvoiceNumber, i.InvoiceDate, i.DueDate, i.TotalAmount, i.PaidAmount, i.BalanceDue, i.Status
    FROM Billing.Invoices i
    WHERE i.PolicyID = @PolicyID
    ORDER BY i.InvoiceDate DESC;

    -- Notes
    SELECT n.NoteID, n.NoteType, n.NoteText, n.CreatedDate, n.CreatedBy, n.Priority
    FROM Policy.Notes n
    WHERE n.PolicyID = @PolicyID
    ORDER BY n.CreatedDate DESC;
END
GO
