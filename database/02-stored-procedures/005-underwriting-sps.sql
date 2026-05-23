-- ============================================================
-- UNDERWRITING STORED PROCEDURES
-- Rate table management, referrals, moratoriums, rules
-- evaluation, quote comparison, and commission calculation
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Get Base Rate for Coverage
-- Effective-dated lookup with territory fallback
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Rate_GetBaseRate
    @PolicyType VARCHAR(30),
    @StateCode CHAR(2),
    @ConstructionType VARCHAR(30),
    @ProtectionClass INT,
    @CoverageCode VARCHAR(20),
    @TerritoryCode VARCHAR(20) = NULL,
    @EffectiveDate DATE,
    @RatePer1000 DECIMAL(10,6) OUTPUT,
    @MinPremium DECIMAL(10,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @RatePer1000 = 0;
    SET @MinPremium = 0;

    -- Try territory-specific rate first
    IF @TerritoryCode IS NOT NULL
    BEGIN
        SELECT TOP 1 @RatePer1000 = RatePer1000, @MinPremium = MinPremium
        FROM Underwriting.BaseRates
        WHERE PolicyType = @PolicyType AND StateCode = @StateCode
            AND ConstructionType = @ConstructionType AND ProtectionClass = @ProtectionClass
            AND CoverageCode = @CoverageCode AND TerritoryCode = @TerritoryCode
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY EffectiveDate DESC;
    END

    -- Fallback to state-level rate
    IF @RatePer1000 = 0
    BEGIN
        SELECT TOP 1 @RatePer1000 = RatePer1000, @MinPremium = MinPremium
        FROM Underwriting.BaseRates
        WHERE PolicyType = @PolicyType AND StateCode = @StateCode
            AND ConstructionType = @ConstructionType AND ProtectionClass = @ProtectionClass
            AND CoverageCode = @CoverageCode AND TerritoryCode IS NULL
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY EffectiveDate DESC;
    END
END
GO

-- ============================================================
-- SP: Get Rating Factor
-- Generic factor lookup by type with state fallback
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Rate_GetFactor
    @FactorType VARCHAR(30),
    @PolicyType VARCHAR(30),
    @StateCode CHAR(2),
    @FactorKey VARCHAR(50) = NULL,
    @RangeValue DECIMAL(18,4) = NULL,
    @EffectiveDate DATE,
    @Factor DECIMAL(10,6) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Factor = 1.0; -- default: no change

    IF @FactorKey IS NOT NULL
    BEGIN
        -- Key-based lookup (e.g., CONSTRUCTION_TYPE = 'FRAME')
        SELECT TOP 1 @Factor = Factor
        FROM Underwriting.RatingFactors
        WHERE FactorType = @FactorType AND PolicyType = @PolicyType
            AND (StateCode = @StateCode OR StateCode IS NULL)
            AND FactorKey = @FactorKey
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY StateCode DESC, EffectiveDate DESC;
    END
    ELSE IF @RangeValue IS NOT NULL
    BEGIN
        -- Range-based lookup (e.g., CREDIT_SCORE between 700-749)
        SELECT TOP 1 @Factor = Factor
        FROM Underwriting.RatingFactors
        WHERE FactorType = @FactorType AND PolicyType = @PolicyType
            AND (StateCode = @StateCode OR StateCode IS NULL)
            AND @RangeValue BETWEEN MinRange AND MaxRange
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY StateCode DESC, EffectiveDate DESC;
    END
END
GO

-- ============================================================
-- SP: Evaluate Underwriting Rules for a Policy
-- Checks all applicable rules, returns referral/decline actions
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Rules_Evaluate
    @PolicyID INT,
    @EvaluatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @PolicyType VARCHAR(30), @StateCode CHAR(2), @CustomerID INT, @PropertyID INT;
        DECLARE @EffectiveDate DATE, @InsuredValue DECIMAL(18,2);
        DECLARE @YearBuilt INT, @ProtectionClass INT, @CreditScore INT;
        DECLARE @ClaimFreeYears INT, @RoofAge INT;

        -- Get policy details
        SELECT @PolicyType = p.PolicyType, @CustomerID = p.CustomerID, @PropertyID = p.PropertyID,
               @EffectiveDate = p.EffectiveDate, @ClaimFreeYears = p.ClaimFreeYears
        FROM Policy.Policies p WHERE p.PolicyID = @PolicyID;

        IF @PolicyType IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        -- Get property details
        SELECT @StateCode = StateCode, @YearBuilt = YearBuilt, 
               @ProtectionClass = FireProtectionClass, @RoofAge = RoofAge
        FROM Policy.Properties WHERE PropertyID = @PropertyID;

        SELECT @CreditScore = CreditScore FROM Policy.Customers WHERE CustomerID = @CustomerID;
        SELECT @InsuredValue = SUM(LimitAmount) FROM Policy.Coverages WHERE PolicyID = @PolicyID AND IsSelected = 1;

        -- Temp table for rule results
        CREATE TABLE #RuleResults (
            RuleID INT, RuleCode VARCHAR(30), RuleName VARCHAR(200),
            ActionType VARCHAR(20), ActionValue VARCHAR(200),
            Severity VARCHAR(10), IsTriggered BIT
        );

        -- Evaluate built-in rules
        DECLARE @PropertyAge INT = YEAR(@EffectiveDate) - @YearBuilt;

        -- Rule: Property age > 75 years → REFER
        IF @PropertyAge > 75
            INSERT INTO #RuleResults VALUES (NULL, 'AGE_75_PLUS', 'Property over 75 years old', 'REFER', NULL, 'HIGH', 1);

        -- Rule: Protection class 9-10 → REFER
        IF @ProtectionClass >= 9
            INSERT INTO #RuleResults VALUES (NULL, 'PROT_CLASS_9_10', 'Protection class 9 or 10', 'REFER', NULL, 'MEDIUM', 1);

        -- Rule: Roof age > 25 → REFER
        IF @RoofAge IS NOT NULL AND @RoofAge > 25
            INSERT INTO #RuleResults VALUES (NULL, 'ROOF_AGE_25', 'Roof over 25 years old', 'REFER', NULL, 'MEDIUM', 1);

        -- Rule: Credit score < 550 → DECLINE
        IF @CreditScore IS NOT NULL AND @CreditScore < 550
            INSERT INTO #RuleResults VALUES (NULL, 'CREDIT_BELOW_550', 'Credit score below 550', 'DECLINE', NULL, 'CRITICAL', 1);

        -- Rule: Insured value > $2M → REFER
        IF @InsuredValue > 2000000
            INSERT INTO #RuleResults VALUES (NULL, 'HIGH_VALUE', 'Total insured value exceeds $2M', 'REFER', NULL, 'HIGH', 1);

        -- Rule: Prior claims >= 3 in 3 years → REFER
        DECLARE @PriorClaims INT;
        SELECT @PriorClaims = COUNT(*) FROM Claims.Claims
        WHERE CustomerID = @CustomerID AND LossDate >= DATEADD(YEAR, -3, @EffectiveDate)
            AND ClaimStatus NOT IN ('DENIED');
        IF @PriorClaims >= 3
            INSERT INTO #RuleResults VALUES (NULL, 'PRIOR_CLAIMS_3', '3+ claims in last 3 years', 'REFER', NULL, 'HIGH', 1);

        -- Evaluate database-stored rules
        INSERT INTO #RuleResults (RuleID, RuleCode, RuleName, ActionType, ActionValue, Severity, IsTriggered)
        SELECT r.RuleID, r.RuleCode, r.RuleName, r.ActionType, r.ActionValue, r.Severity, 1
        FROM Underwriting.Rules r
        WHERE r.IsActive = 1
            AND (r.PolicyType = @PolicyType OR r.PolicyType IS NULL)
            AND (r.StateCode = @StateCode OR r.StateCode IS NULL)
            AND @EffectiveDate BETWEEN r.EffectiveDate AND r.ExpiryDate
            AND r.RuleCategory = 'REFERRAL'
            AND NOT EXISTS (SELECT 1 FROM #RuleResults rr WHERE rr.RuleCode = r.RuleCode);

        -- Create referrals for triggered rules
        INSERT INTO Underwriting.Referrals (PolicyID, RuleID, ReferralReason, ReferralStatus, ReferralDate, CreatedDate)
        SELECT @PolicyID, RuleID, RuleName, 'PENDING', GETDATE(), GETDATE()
        FROM #RuleResults
        WHERE IsTriggered = 1 AND ActionType IN ('REFER', 'DECLINE');

        -- Return results
        SELECT * FROM #RuleResults WHERE IsTriggered = 1 ORDER BY 
            CASE Severity WHEN 'CRITICAL' THEN 1 WHEN 'HIGH' THEN 2 WHEN 'MEDIUM' THEN 3 ELSE 4 END;

        DROP TABLE #RuleResults;

    END TRY
    BEGIN CATCH
        IF OBJECT_ID('tempdb..#RuleResults') IS NOT NULL DROP TABLE #RuleResults;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Process Underwriting Referral (approve/decline/conditional)
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Referral_Process
    @ReferralID INT,
    @Decision VARCHAR(20), -- APPROVED, DECLINED, CONDITIONAL
    @DecisionNotes VARCHAR(MAX) = NULL,
    @Conditions VARCHAR(MAX) = NULL,
    @ReviewedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PolicyID INT, @CurrentStatus VARCHAR(20);
        SELECT @PolicyID = PolicyID, @CurrentStatus = ReferralStatus
        FROM Underwriting.Referrals WHERE ReferralID = @ReferralID;

        IF @PolicyID IS NULL
        BEGIN
            RAISERROR('Referral not found: %d', 16, 1, @ReferralID);
            RETURN;
        END

        IF @CurrentStatus <> 'PENDING'
        BEGIN
            RAISERROR('Referral is not in PENDING status. Current: %s', 16, 1, @CurrentStatus);
            RETURN;
        END

        -- Update referral
        UPDATE Underwriting.Referrals SET
            ReferralStatus = @Decision,
            Decision = @Decision,
            DecisionNotes = @DecisionNotes,
            Conditions = @Conditions,
            ReviewedBy = @ReviewedBy,
            ReviewDate = GETDATE()
        WHERE ReferralID = @ReferralID;

        -- If declined, update policy status
        IF @Decision = 'DECLINED'
        BEGIN
            UPDATE Policy.Policies SET
                PolicyStatus = 'DECLINED',
                ModifiedDate = GETDATE(),
                ModifiedBy = @ReviewedBy
            WHERE PolicyID = @PolicyID AND PolicyStatus IN ('QUOTE', 'REFERRED');
        END

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
        VALUES ('Underwriting.Referrals', @ReferralID, 'UPDATE', 'ReferralStatus', @CurrentStatus, @Decision, @ReviewedBy, GETDATE());

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
-- SP: Get Referrals for Policy
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Referral_GetByPolicy
    @PolicyID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT r.*, ru.RuleCode, ru.RuleCategory, ru.Severity
    FROM Underwriting.Referrals r
    LEFT JOIN Underwriting.Rules ru ON r.RuleID = ru.RuleID
    WHERE r.PolicyID = @PolicyID
    ORDER BY r.ReferralDate DESC;
END
GO

-- ============================================================
-- SP: Search Pending Referrals (underwriter workqueue)
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Referral_SearchPending
    @AssignedTo VARCHAR(50) = NULL,
    @PolicyType VARCHAR(30) = NULL,
    @StateCode CHAR(2) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT @TotalRecords = COUNT(*)
    FROM Underwriting.Referrals r
    INNER JOIN Policy.Policies p ON r.PolicyID = p.PolicyID
    INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
    WHERE r.ReferralStatus = 'PENDING'
        AND (@AssignedTo IS NULL OR r.AssignedTo = @AssignedTo)
        AND (@PolicyType IS NULL OR p.PolicyType = @PolicyType)
        AND (@StateCode IS NULL OR pr.StateCode = @StateCode);

    SELECT r.ReferralID, r.PolicyID, r.ReferralReason, r.ReferralDate, r.AssignedTo,
           p.PolicyNumber, p.PolicyType, p.AnnualPremium,
           c.FirstName + ' ' + c.LastName AS CustomerName,
           pr.StateCode, pr.City,
           ru.Severity
    FROM Underwriting.Referrals r
    INNER JOIN Policy.Policies p ON r.PolicyID = p.PolicyID
    INNER JOIN Policy.Customers c ON p.CustomerID = c.CustomerID
    INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
    LEFT JOIN Underwriting.Rules ru ON r.RuleID = ru.RuleID
    WHERE r.ReferralStatus = 'PENDING'
        AND (@AssignedTo IS NULL OR r.AssignedTo = @AssignedTo)
        AND (@PolicyType IS NULL OR p.PolicyType = @PolicyType)
        AND (@StateCode IS NULL OR pr.StateCode = @StateCode)
    ORDER BY CASE ru.Severity WHEN 'CRITICAL' THEN 1 WHEN 'HIGH' THEN 2 WHEN 'MEDIUM' THEN 3 ELSE 4 END, r.ReferralDate
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================
-- SP: Create/Update Moratorium
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Moratorium_Create
    @MoratoriumName VARCHAR(200),
    @MoratoriumType VARCHAR(20),
    @Reason VARCHAR(500) = NULL,
    @AffectedStates VARCHAR(200) = NULL,
    @AffectedZipCodes VARCHAR(MAX) = NULL,
    @AffectedPolicyTypes VARCHAR(200) = NULL,
    @StartDate DATE,
    @EndDate DATE = NULL,
    @DeclaredBy VARCHAR(50),
    @MoratoriumID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Underwriting.Moratoriums (
            MoratoriumName, MoratoriumType, Reason,
            AffectedStates, AffectedZipCodes, AffectedPolicyTypes,
            StartDate, EndDate, IsActive, DeclaredBy, DeclaredDate, CreatedDate
        ) VALUES (
            @MoratoriumName, @MoratoriumType, @Reason,
            @AffectedStates, @AffectedZipCodes, @AffectedPolicyTypes,
            @StartDate, @EndDate, 1, @DeclaredBy, GETDATE(), GETDATE()
        );

        SET @MoratoriumID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Underwriting.Moratoriums', @MoratoriumID, 'INSERT', @DeclaredBy, GETDATE());

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
-- SP: Lift/End Moratorium
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Moratorium_Lift
    @MoratoriumID INT,
    @LiftedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Underwriting.Moratoriums SET
        IsActive = 0,
        EndDate = CAST(GETDATE() AS DATE)
    WHERE MoratoriumID = @MoratoriumID AND IsActive = 1;

    IF @@ROWCOUNT = 0
        RAISERROR('Active moratorium not found: %d', 16, 1, @MoratoriumID);

    INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
    VALUES ('Underwriting.Moratoriums', @MoratoriumID, 'UPDATE', 'IsActive', '1', '0', @LiftedBy, GETDATE());
END
GO

-- ============================================================
-- SP: Check Moratorium (is writing allowed?)
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Moratorium_Check
    @PolicyType VARCHAR(30),
    @StateCode CHAR(2),
    @ZipCode VARCHAR(10),
    @TransactionType VARCHAR(20) = 'NEW_BUSINESS', -- NEW_BUSINESS, RENEWAL
    @IsMoratorium BIT OUTPUT,
    @MoratoriumName VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsMoratorium = 0;
    SET @MoratoriumName = NULL;

    SELECT TOP 1 @IsMoratorium = 1, @MoratoriumName = MoratoriumName
    FROM Underwriting.Moratoriums
    WHERE IsActive = 1
        AND MoratoriumType IN (@TransactionType, 'ALL')
        AND GETDATE() BETWEEN StartDate AND ISNULL(EndDate, '9999-12-31')
        AND (AffectedStates LIKE '%' + @StateCode + '%' OR AffectedStates IS NULL)
        AND (AffectedPolicyTypes LIKE '%' + @PolicyType + '%' OR AffectedPolicyTypes IS NULL)
        AND (AffectedZipCodes LIKE '%' + @ZipCode + '%' OR AffectedZipCodes IS NULL)
    ORDER BY StartDate DESC;
END
GO

-- ============================================================
-- SP: Get Active Moratoriums
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Moratorium_GetActive
AS
BEGIN
    SET NOCOUNT ON;

    SELECT m.*,
        (SELECT COUNT(DISTINCT p.PolicyID) FROM Policy.Policies p
         INNER JOIN Policy.Properties pr ON p.PropertyID = pr.PropertyID
         WHERE p.PolicyStatus = 'ACTIVE'
            AND (m.AffectedStates LIKE '%' + pr.StateCode + '%' OR m.AffectedStates IS NULL)
            AND (m.AffectedPolicyTypes LIKE '%' + p.PolicyType + '%' OR m.AffectedPolicyTypes IS NULL)
        ) AS AffectedPolicyCount
    FROM Underwriting.Moratoriums m
    WHERE m.IsActive = 1 AND GETDATE() BETWEEN m.StartDate AND ISNULL(m.EndDate, '9999-12-31')
    ORDER BY m.StartDate DESC;
END
GO

-- ============================================================
-- SP: Get Rating Worksheet for Policy
-- Returns the full rating breakdown for display
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Worksheet_GetByPolicy
    @PolicyID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT rw.*, cov.CoverageName, cov.LimitAmount, cov.DeductibleAmount
    FROM Underwriting.RatingWorksheets rw
    INNER JOIN Policy.Coverages cov ON rw.PolicyID = cov.PolicyID AND rw.CoverageCode = cov.CoverageCode
    WHERE rw.PolicyID = @PolicyID
    ORDER BY cov.CoverageCode;
END
GO

-- ============================================================
-- SP: Manage Rate Table (Create/Update)
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_RateTable_Create
    @RateTableCode VARCHAR(30),
    @RateTableName VARCHAR(100),
    @RateType VARCHAR(20),
    @PolicyType VARCHAR(30),
    @StateCode CHAR(2) = NULL,
    @EffectiveDate DATE,
    @ExpiryDate DATE = '9999-12-31',
    @FilingNumber VARCHAR(50) = NULL,
    @CreatedBy VARCHAR(50),
    @RateTableID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Expire any existing active version
        UPDATE Underwriting.RateTables SET
            ExpiryDate = DATEADD(DAY, -1, @EffectiveDate),
            IsActive = CASE WHEN DATEADD(DAY, -1, @EffectiveDate) < CAST(GETDATE() AS DATE) THEN 0 ELSE IsActive END
        WHERE RateTableCode = @RateTableCode
            AND (StateCode = @StateCode OR (@StateCode IS NULL AND StateCode IS NULL))
            AND ExpiryDate = '9999-12-31' AND IsActive = 1;

        -- Get next version
        DECLARE @Version INT;
        SELECT @Version = ISNULL(MAX(Version), 0) + 1
        FROM Underwriting.RateTables
        WHERE RateTableCode = @RateTableCode
            AND (StateCode = @StateCode OR (@StateCode IS NULL AND StateCode IS NULL));

        INSERT INTO Underwriting.RateTables (
            RateTableCode, RateTableName, RateType, PolicyType, StateCode,
            EffectiveDate, ExpiryDate, Version, FilingNumber, IsActive, CreatedDate
        ) VALUES (
            @RateTableCode, @RateTableName, @RateType, @PolicyType, @StateCode,
            @EffectiveDate, @ExpiryDate, @Version, @FilingNumber, 1, GETDATE()
        );

        SET @RateTableID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Underwriting.RateTables', @RateTableID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Add Rate Table Detail Row
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_RateTable_AddDetail
    @RateTableID INT,
    @FactorCode VARCHAR(30),
    @FactorValue VARCHAR(100) = NULL,
    @MinValue DECIMAL(18,4) = NULL,
    @MaxValue DECIMAL(18,4) = NULL,
    @Rate DECIMAL(18,6),
    @FlatAmount DECIMAL(18,2) = 0,
    @DisplayOrder INT = 0,
    @RateDetailID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Underwriting.RateTables WHERE RateTableID = @RateTableID)
    BEGIN
        RAISERROR('Rate table not found: %d', 16, 1, @RateTableID);
        RETURN;
    END

    INSERT INTO Underwriting.RateTableDetails (
        RateTableID, FactorCode, FactorValue, MinValue, MaxValue,
        Rate, FlatAmount, DisplayOrder, IsActive
    ) VALUES (
        @RateTableID, @FactorCode, @FactorValue, @MinValue, @MaxValue,
        @Rate, @FlatAmount, @DisplayOrder, 1
    );

    SET @RateDetailID = SCOPE_IDENTITY();
END
GO

-- ============================================================
-- SP: Get Rate Table with Details
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_RateTable_GetDetails
    @RateTableID INT = NULL,
    @RateTableCode VARCHAR(30) = NULL,
    @StateCode CHAR(2) = NULL,
    @EffectiveDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @EffectiveDate IS NULL SET @EffectiveDate = CAST(GETDATE() AS DATE);

    -- Resolve RateTableID if not provided
    IF @RateTableID IS NULL AND @RateTableCode IS NOT NULL
    BEGIN
        SELECT TOP 1 @RateTableID = RateTableID
        FROM Underwriting.RateTables
        WHERE RateTableCode = @RateTableCode
            AND (StateCode = @StateCode OR (@StateCode IS NULL AND StateCode IS NULL))
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY EffectiveDate DESC;
    END

    -- Rate table header
    SELECT * FROM Underwriting.RateTables WHERE RateTableID = @RateTableID;

    -- Rate table details
    SELECT * FROM Underwriting.RateTableDetails
    WHERE RateTableID = @RateTableID AND IsActive = 1
    ORDER BY DisplayOrder, FactorCode, MinValue;
END
GO

-- ============================================================
-- SP: Calculate Agent Commission
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Commission_Calculate
    @PolicyID INT,
    @TransactionType VARCHAR(20) = 'NEW', -- NEW, RENEWAL, ENDORSEMENT
    @PremiumAmount DECIMAL(18,2),
    @CommissionAmount DECIMAL(18,2) OUTPUT,
    @CommissionRate DECIMAL(6,4) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @PolicyType VARCHAR(30), @AgentID INT;
    SELECT @PolicyType = PolicyType, @AgentID = AgentID
    FROM Policy.Policies WHERE PolicyID = @PolicyID;

    -- Get commission rate from schedule
    SET @CommissionRate = 0;
    SELECT TOP 1 @CommissionRate = CommissionPercent
    FROM Underwriting.CommissionSchedules
    WHERE PolicyType = @PolicyType AND TransactionType = @TransactionType
        AND @PremiumAmount BETWEEN MinPremium AND ISNULL(MaxPremium, 99999999)
        AND GETDATE() BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY EffectiveDate DESC;

    -- Fallback to agent's default rate
    IF @CommissionRate = 0
        SELECT @CommissionRate = CommissionRate FROM Policy.Agents WHERE AgentID = @AgentID;

    SET @CommissionAmount = ROUND(@PremiumAmount * @CommissionRate, 2);

    -- Update policy
    UPDATE Policy.Policies SET
        CommissionRate = @CommissionRate,
        CommissionAmount = @CommissionAmount
    WHERE PolicyID = @PolicyID;
END
GO
