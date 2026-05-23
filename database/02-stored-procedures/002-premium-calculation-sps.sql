-- ============================================================
-- PREMIUM CALCULATION ENGINE
-- These are the most complex SPs - multi-factor rating with
-- nested calls, temp tables, and effective-dated lookups
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Master Premium Calculation (orchestrator)
-- Calls sub-procedures for each coverage, applies discounts,
-- calculates taxes/fees, and stores the rating worksheet
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Premium_Calculate
    @PolicyID INT,
    @CalculatedBy VARCHAR(50),
    @RecalculateAll BIT = 1,
    @TotalPremium DECIMAL(18,2) OUTPUT,
    @TotalTaxes DECIMAL(18,2) OUTPUT,
    @GrossPremium DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get policy and property details
        DECLARE @PolicyType VARCHAR(30), @PropertyID INT, @CustomerID INT;
        DECLARE @EffectiveDate DATE, @StateCode CHAR(2), @ZipCode VARCHAR(10);
        DECLARE @ConstructionType VARCHAR(30), @YearBuilt INT, @ProtectionClass INT;
        DECLARE @SquareFootage INT, @RoofType VARCHAR(30), @RoofAge INT;
        DECLARE @OccupancyType VARCHAR(30), @PropertyType VARCHAR(30);
        DECLARE @HasFireAlarm BIT, @HasBurglarAlarm BIT, @HasSprinklerSystem BIT;
        DECLARE @CreditScore INT, @ClaimFreeYears INT, @RenewalCount INT;
        DECLARE @MultiPolicyDiscount BIT, @TerritoryID INT;

        SELECT 
            @PolicyType = p.PolicyType, @PropertyID = p.PropertyID, @CustomerID = p.CustomerID,
            @EffectiveDate = p.EffectiveDate, @ClaimFreeYears = p.ClaimFreeYears,
            @RenewalCount = p.RenewalCount, @MultiPolicyDiscount = p.MultiPolicyDiscount
        FROM Policy.Policies p WHERE p.PolicyID = @PolicyID;

        IF @PolicyType IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        SELECT 
            @StateCode = pr.StateCode, @ZipCode = pr.ZipCode,
            @ConstructionType = pr.ConstructionType, @YearBuilt = pr.YearBuilt,
            @ProtectionClass = pr.FireProtectionClass, @SquareFootage = pr.SquareFootage,
            @RoofType = pr.RoofType, @RoofAge = pr.RoofAge,
            @OccupancyType = pr.OccupancyType, @PropertyType = pr.PropertyType,
            @HasFireAlarm = pr.HasFireAlarm, @HasBurglarAlarm = pr.HasBurglarAlarm,
            @HasSprinklerSystem = pr.HasSprinklerSystem, @TerritoryID = pr.TerritoryID
        FROM Policy.Properties pr WHERE pr.PropertyID = @PropertyID;

        SELECT @CreditScore = CreditScore FROM Policy.Customers WHERE CustomerID = @CustomerID;

        -- Temp table to accumulate coverage premiums
        CREATE TABLE #CoveragePremiums (
            CoverageID INT,
            CoverageCode VARCHAR(20),
            InsuredValue DECIMAL(18,2),
            BaseRate DECIMAL(18,6),
            BasePremium DECIMAL(18,2),
            ConstructionFactor DECIMAL(10,6),
            AgeFactor DECIMAL(10,6),
            RoofFactor DECIMAL(10,6),
            ProtectionClassFactor DECIMAL(10,6),
            DeductibleFactor DECIMAL(10,6),
            CreditFactor DECIMAL(10,6),
            ClaimsHistoryFactor DECIMAL(10,6),
            ProtectiveDeviceFactor DECIMAL(10,6),
            TerritoryFactor DECIMAL(10,6),
            OccupancyFactor DECIMAL(10,6),
            LoyaltyFactor DECIMAL(10,6),
            MultiPolicyFactor DECIMAL(10,6),
            NewHomeFactor DECIMAL(10,6),
            TotalFactor DECIMAL(10,6),
            CalculatedPremium DECIMAL(18,2),
            MinPremiumApplied BIT,
            FinalPremium DECIMAL(18,2)
        );

        -- Process each selected coverage
        DECLARE @CoverageID INT, @CoverageCode VARCHAR(20), @InsuredValue DECIMAL(18,2);
        DECLARE @DeductibleAmount DECIMAL(18,2), @DeductibleType VARCHAR(20);
        DECLARE @CoveragePremium DECIMAL(18,2);

        DECLARE coverage_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT CoverageID, CoverageCode, LimitAmount, DeductibleAmount, DeductibleType
            FROM Policy.Coverages
            WHERE PolicyID = @PolicyID AND IsSelected = 1;

        OPEN coverage_cursor;
        FETCH NEXT FROM coverage_cursor INTO @CoverageID, @CoverageCode, @InsuredValue, @DeductibleAmount, @DeductibleType;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Calculate premium for this coverage
            EXEC Underwriting.usp_Premium_CalculateCoverage
                @PolicyID = @PolicyID,
                @CoverageID = @CoverageID,
                @CoverageCode = @CoverageCode,
                @InsuredValue = @InsuredValue,
                @DeductibleAmount = @DeductibleAmount,
                @DeductibleType = @DeductibleType,
                @PolicyType = @PolicyType,
                @StateCode = @StateCode,
                @ConstructionType = @ConstructionType,
                @YearBuilt = @YearBuilt,
                @ProtectionClass = @ProtectionClass,
                @RoofType = @RoofType,
                @RoofAge = @RoofAge,
                @OccupancyType = @OccupancyType,
                @HasFireAlarm = @HasFireAlarm,
                @HasBurglarAlarm = @HasBurglarAlarm,
                @HasSprinklerSystem = @HasSprinklerSystem,
                @CreditScore = @CreditScore,
                @ClaimFreeYears = @ClaimFreeYears,
                @RenewalCount = @RenewalCount,
                @MultiPolicyDiscount = @MultiPolicyDiscount,
                @TerritoryID = @TerritoryID,
                @EffectiveDate = @EffectiveDate,
                @CalculatedBy = @CalculatedBy,
                @CoveragePremium = @CoveragePremium OUTPUT;

            -- Update coverage record
            UPDATE Policy.Coverages SET Premium = @CoveragePremium WHERE CoverageID = @CoverageID;

            FETCH NEXT FROM coverage_cursor INTO @CoverageID, @CoverageCode, @InsuredValue, @DeductibleAmount, @DeductibleType;
        END

        CLOSE coverage_cursor;
        DEALLOCATE coverage_cursor;

        -- Sum all coverage premiums
        SELECT @TotalPremium = ISNULL(SUM(Premium), 0) FROM Policy.Coverages WHERE PolicyID = @PolicyID AND IsSelected = 1;

        -- Calculate taxes and fees
        EXEC Underwriting.usp_Premium_CalculateTaxesFees
            @PolicyID = @PolicyID,
            @StateCode = @StateCode,
            @PolicyType = @PolicyType,
            @PremiumAmount = @TotalPremium,
            @EffectiveDate = @EffectiveDate,
            @TotalTaxes = @TotalTaxes OUTPUT;

        -- Gross premium
        SET @GrossPremium = @TotalPremium + @TotalTaxes;

        -- Calculate commission
        DECLARE @CommissionRate DECIMAL(6,4), @CommissionAmount DECIMAL(18,2);
        SELECT @CommissionRate = CommissionRate FROM Policy.Policies WHERE PolicyID = @PolicyID;
        SET @CommissionAmount = @TotalPremium * @CommissionRate;

        -- Update policy totals
        UPDATE Policy.Policies SET
            AnnualPremium = @TotalPremium,
            WrittenPremium = @TotalPremium,
            TotalTaxes = @TotalTaxes,
            GrossPremium = @GrossPremium,
            CommissionAmount = @CommissionAmount,
            TotalInsuredValue = (SELECT SUM(LimitAmount) FROM Policy.Coverages WHERE PolicyID = @PolicyID AND IsSelected = 1),
            ModifiedDate = GETDATE(),
            ModifiedBy = @CalculatedBy
        WHERE PolicyID = @PolicyID;

        DROP TABLE #CoveragePremiums;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        IF OBJECT_ID('tempdb..#CoveragePremiums') IS NOT NULL DROP TABLE #CoveragePremiums;
        
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), 
                'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Calculate Individual Coverage Premium
-- Heavy factor-based rating with effective-dated lookups
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Premium_CalculateCoverage
    @PolicyID INT,
    @CoverageID INT,
    @CoverageCode VARCHAR(20),
    @InsuredValue DECIMAL(18,2),
    @DeductibleAmount DECIMAL(18,2),
    @DeductibleType VARCHAR(20),
    @PolicyType VARCHAR(30),
    @StateCode CHAR(2),
    @ConstructionType VARCHAR(30),
    @YearBuilt INT,
    @ProtectionClass INT,
    @RoofType VARCHAR(30),
    @RoofAge INT,
    @OccupancyType VARCHAR(30),
    @HasFireAlarm BIT,
    @HasBurglarAlarm BIT,
    @HasSprinklerSystem BIT,
    @CreditScore INT,
    @ClaimFreeYears INT,
    @RenewalCount INT,
    @MultiPolicyDiscount BIT,
    @TerritoryID INT,
    @EffectiveDate DATE,
    @CalculatedBy VARCHAR(50) = 'SYSTEM',
    @CoveragePremium DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BaseRate DECIMAL(18,6) = 0;
    DECLARE @BasePremium DECIMAL(18,2) = 0;
    DECLARE @MinPremium DECIMAL(10,2) = 0;
    DECLARE @PropertyAge INT = YEAR(@EffectiveDate) - @YearBuilt;

    -- Rating factors (all default to 1.0 = no change)
    DECLARE @ConstructionFactor DECIMAL(10,6) = 1.0;
    DECLARE @AgeFactor DECIMAL(10,6) = 1.0;
    DECLARE @RoofFactor DECIMAL(10,6) = 1.0;
    DECLARE @ProtectionClassFactor DECIMAL(10,6) = 1.0;
    DECLARE @DeductibleFactor DECIMAL(10,6) = 1.0;
    DECLARE @CreditFactor DECIMAL(10,6) = 1.0;
    DECLARE @ClaimsHistoryFactor DECIMAL(10,6) = 1.0;
    DECLARE @ProtectiveDeviceFactor DECIMAL(10,6) = 1.0;
    DECLARE @TerritoryFactor DECIMAL(10,6) = 1.0;
    DECLARE @OccupancyFactor DECIMAL(10,6) = 1.0;
    DECLARE @LoyaltyFactor DECIMAL(10,6) = 1.0;
    DECLARE @MultiPolicyFactor DECIMAL(10,6) = 1.0;
    DECLARE @NewHomeFactor DECIMAL(10,6) = 1.0;
    DECLARE @TotalFactor DECIMAL(10,6);
    DECLARE @MinPremiumApplied BIT = 0;

    -- 1. Get base rate (per $1000 of insured value)
    SELECT TOP 1 @BaseRate = RatePer1000, @MinPremium = MinPremium
    FROM Underwriting.BaseRates
    WHERE PolicyType = @PolicyType
        AND StateCode = @StateCode
        AND ConstructionType = @ConstructionType
        AND ProtectionClass = @ProtectionClass
        AND CoverageCode = @CoverageCode
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate
        AND IsActive = 1
    ORDER BY EffectiveDate DESC;

    -- If no exact match, try without territory specificity
    IF @BaseRate = 0
    BEGIN
        SELECT TOP 1 @BaseRate = RatePer1000, @MinPremium = MinPremium
        FROM Underwriting.BaseRates
        WHERE PolicyType = @PolicyType
            AND StateCode = @StateCode
            AND ConstructionType = @ConstructionType
            AND ProtectionClass = @ProtectionClass
            AND CoverageCode = @CoverageCode
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate
            AND IsActive = 1
            AND TerritoryCode IS NULL
        ORDER BY EffectiveDate DESC;
    END

    -- Calculate base premium
    SET @BasePremium = (@InsuredValue / 1000.0) * @BaseRate;

    -- 2. Construction factor
    SELECT TOP 1 @ConstructionFactor = Factor
    FROM Underwriting.RatingFactors
    WHERE FactorType = 'CONSTRUCTION_TYPE' AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND FactorKey = @ConstructionType
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 3. Age of home factor (banded)
    SELECT TOP 1 @AgeFactor = Factor
    FROM Underwriting.RatingFactors
    WHERE FactorType = 'AGE_OF_HOME' AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND @PropertyAge BETWEEN MinRange AND MaxRange
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 4. Roof factor
    IF @RoofType IS NOT NULL
    BEGIN
        SELECT TOP 1 @RoofFactor = Factor
        FROM Underwriting.RatingFactors
        WHERE FactorType = 'ROOF_TYPE' AND PolicyType = @PolicyType
            AND (StateCode = @StateCode OR StateCode IS NULL)
            AND FactorKey = @RoofType
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY StateCode DESC, EffectiveDate DESC;

        -- Additional roof age surcharge
        IF @RoofAge > 20
        BEGIN
            DECLARE @RoofAgeSurcharge DECIMAL(10,6) = 1.0;
            SELECT TOP 1 @RoofAgeSurcharge = Factor
            FROM Underwriting.RatingFactors
            WHERE FactorType = 'ROOF_AGE' AND PolicyType = @PolicyType
                AND @RoofAge BETWEEN MinRange AND MaxRange
                AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
            ORDER BY EffectiveDate DESC;
            SET @RoofFactor = @RoofFactor * @RoofAgeSurcharge;
        END
    END

    -- 5. Protection class factor
    SELECT TOP 1 @ProtectionClassFactor = Factor
    FROM Underwriting.RatingFactors
    WHERE FactorType = 'PROTECTION_CLASS' AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND @ProtectionClass BETWEEN MinRange AND MaxRange
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 6. Deductible factor
    SELECT TOP 1 @DeductibleFactor = PremiumFactor
    FROM Underwriting.DeductibleOptions
    WHERE PolicyType = @PolicyType AND CoverageCode = @CoverageCode
        AND DeductibleType = @DeductibleType
        AND DeductibleAmount = @DeductibleAmount
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 7. Credit score factor
    IF @CreditScore IS NOT NULL
    BEGIN
        SELECT TOP 1 @CreditFactor = Factor
        FROM Underwriting.RatingFactors
        WHERE FactorType = 'CREDIT_SCORE' AND PolicyType = @PolicyType
            AND (StateCode = @StateCode OR StateCode IS NULL)
            AND @CreditScore BETWEEN MinRange AND MaxRange
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY StateCode DESC, EffectiveDate DESC;
    END

    -- 8. Claims history factor
    DECLARE @PriorClaimCount INT = 0;
    SELECT @PriorClaimCount = COUNT(*)
    FROM Claims.Claims cl
    INNER JOIN Policy.Policies pol ON cl.PolicyID = pol.PolicyID
    WHERE pol.CustomerID = (SELECT CustomerID FROM Policy.Policies WHERE PolicyID = @PolicyID)
        AND cl.LossDate >= DATEADD(YEAR, -5, @EffectiveDate)
        AND cl.ClaimStatus NOT IN ('DENIED', 'CLOSED')
        AND cl.TotalPaid > 0;

    SELECT TOP 1 @ClaimsHistoryFactor = Factor
    FROM Underwriting.RatingFactors
    WHERE FactorType = 'CLAIMS_HISTORY' AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND @PriorClaimCount BETWEEN MinRange AND MaxRange
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 9. Protective device discount
    DECLARE @DeviceCount INT = 0;
    SET @DeviceCount = CAST(@HasFireAlarm AS INT) + CAST(@HasBurglarAlarm AS INT) + CAST(@HasSprinklerSystem AS INT);
    
    IF @DeviceCount > 0
    BEGIN
        SELECT TOP 1 @ProtectiveDeviceFactor = Factor
        FROM Underwriting.RatingFactors
        WHERE FactorType = 'PROTECTIVE_DEVICE' AND PolicyType = @PolicyType
            AND (StateCode = @StateCode OR StateCode IS NULL)
            AND @DeviceCount BETWEEN MinRange AND MaxRange
            AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
        ORDER BY StateCode DESC, EffectiveDate DESC;
    END

    -- 10. Territory factor
    IF @TerritoryID IS NOT NULL
    BEGIN
        SELECT @TerritoryFactor = RiskMultiplier FROM Policy.Territories WHERE TerritoryID = @TerritoryID;
    END

    -- 11. Occupancy factor
    SELECT TOP 1 @OccupancyFactor = Factor
    FROM Underwriting.RatingFactors
    WHERE FactorType = 'OCCUPANCY' AND PolicyType = @PolicyType
        AND (StateCode = @StateCode OR StateCode IS NULL)
        AND FactorKey = @OccupancyType
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate AND IsActive = 1
    ORDER BY StateCode DESC, EffectiveDate DESC;

    -- 12. Loyalty discount (based on renewal count)
    IF @RenewalCount >= 5
        SET @LoyaltyFactor = 0.90; -- 10% discount for 5+ years
    ELSE IF @RenewalCount >= 3
        SET @LoyaltyFactor = 0.95; -- 5% discount for 3+ years

    -- 13. Multi-policy discount
    IF @MultiPolicyDiscount = 1
        SET @MultiPolicyFactor = 0.90; -- 10% multi-policy discount

    -- 14. New home discount
    IF @PropertyAge <= 5
        SET @NewHomeFactor = 0.85; -- 15% new home discount
    ELSE IF @PropertyAge <= 10
        SET @NewHomeFactor = 0.92; -- 8% newer home discount

    -- Calculate total factor
    SET @TotalFactor = @ConstructionFactor * @AgeFactor * @RoofFactor * @ProtectionClassFactor 
        * @DeductibleFactor * @CreditFactor * @ClaimsHistoryFactor * @ProtectiveDeviceFactor 
        * @TerritoryFactor * @OccupancyFactor * @LoyaltyFactor * @MultiPolicyFactor * @NewHomeFactor;

    -- Calculate final premium
    SET @CoveragePremium = ROUND(@BasePremium * @TotalFactor, 2);

    -- Apply minimum premium
    IF @CoveragePremium < @MinPremium AND @MinPremium > 0
    BEGIN
        SET @CoveragePremium = @MinPremium;
        SET @MinPremiumApplied = 1;
    END

    -- Store rating worksheet for audit trail
    DELETE FROM Underwriting.RatingWorksheets WHERE PolicyID = @PolicyID AND CoverageCode = @CoverageCode;
    
    INSERT INTO Underwriting.RatingWorksheets (
        PolicyID, CoverageCode, CalculationDate, BaseRate, InsuredValue, BasePremium,
        ConstructionFactor, AgeFactor, RoofFactor, ProtectionClassFactor,
        DeductibleFactor, CreditFactor, ClaimsHistoryFactor, ProtectiveDeviceFactor,
        TerritoryFactor, OccupancyFactor, LoyaltyFactor, MultiPolicyFactor, NewHomeFactor,
        TotalFactor, CalculatedPremium, MinPremiumApplied, FinalPremium, CalculatedBy
    ) VALUES (
        @PolicyID, @CoverageCode, GETDATE(), @BaseRate, @InsuredValue, @BasePremium,
        @ConstructionFactor, @AgeFactor, @RoofFactor, @ProtectionClassFactor,
        @DeductibleFactor, @CreditFactor, @ClaimsHistoryFactor, @ProtectiveDeviceFactor,
        @TerritoryFactor, @OccupancyFactor, @LoyaltyFactor, @MultiPolicyFactor, @NewHomeFactor,
        @TotalFactor, @CoveragePremium, @MinPremiumApplied, @CoveragePremium, @CalculatedBy
    );
END
GO

-- ============================================================
-- SP: Calculate Taxes and Fees
-- State-specific tax calculations with effective dating
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Premium_CalculateTaxesFees
    @PolicyID INT,
    @StateCode CHAR(2),
    @PolicyType VARCHAR(30),
    @PremiumAmount DECIMAL(18,2),
    @EffectiveDate DATE,
    @TotalTaxes DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TaxRate DECIMAL(6,4) = 0;
    DECLARE @SurchargeRate DECIMAL(6,4) = 0;
    DECLARE @StampingFeeRate DECIMAL(6,4) = 0;
    DECLARE @FireMarshalRate DECIMAL(6,4) = 0;
    DECLARE @PolicyFee DECIMAL(10,2) = 0;

    -- Get state-specific rates
    SELECT 
        @TaxRate = TaxRate,
        @SurchargeRate = SurchargeRate,
        @StampingFeeRate = StampingFeeRate,
        @FireMarshalRate = FireMarshalRate
    FROM Admin.States
    WHERE StateCode = @StateCode AND IsActive = 1
        AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate;

    -- Calculate individual components
    DECLARE @StateTax DECIMAL(18,2) = ROUND(@PremiumAmount * @TaxRate, 2);
    DECLARE @Surcharge DECIMAL(18,2) = ROUND(@PremiumAmount * @SurchargeRate, 2);
    DECLARE @StampingFee DECIMAL(18,2) = ROUND(@PremiumAmount * @StampingFeeRate, 2);
    DECLARE @FireMarshalFee DECIMAL(18,2) = ROUND(@PremiumAmount * @FireMarshalRate, 2);

    -- Policy fee from config
    SELECT @PolicyFee = CAST(ConfigValue AS DECIMAL(10,2))
    FROM Admin.SystemConfig WHERE ConfigKey = 'POLICY_FEE_' + @PolicyType;

    IF @PolicyFee IS NULL
        SELECT @PolicyFee = CAST(ConfigValue AS DECIMAL(10,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'POLICY_FEE_DEFAULT';

    SET @TotalTaxes = @StateTax + @Surcharge + @StampingFee + @FireMarshalFee + ISNULL(@PolicyFee, 0);

    -- Update policy with breakdown
    UPDATE Policy.Policies SET
        TotalTaxes = @TotalTaxes,
        TotalFees = ISNULL(@PolicyFee, 0),
        TotalSurcharges = @Surcharge + @StampingFee + @FireMarshalFee
    WHERE PolicyID = @PolicyID;
END
GO

-- ============================================================
-- SP: Endorsement Premium Calculation (pro-rata)
-- Calculates premium change for mid-term endorsements
-- ============================================================
CREATE OR ALTER PROCEDURE Underwriting.usp_Premium_CalculateEndorsement
    @PolicyID INT,
    @EndorsementID INT,
    @EndorsementEffectiveDate DATE,
    @CalculatedBy VARCHAR(50),
    @PremiumChange DECIMAL(18,2) OUTPUT,
    @ReturnPremium DECIMAL(18,2) OUTPUT,
    @AdditionalPremium DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @PolicyEffective DATE, @PolicyExpiry DATE, @CurrentPremium DECIMAL(18,2);
        DECLARE @DaysInTerm INT, @DaysRemaining INT, @ProRataFactor DECIMAL(10,8);

        SELECT @PolicyEffective = EffectiveDate, @PolicyExpiry = ExpiryDate, @CurrentPremium = AnnualPremium
        FROM Policy.Policies WHERE PolicyID = @PolicyID;

        -- Calculate pro-rata factor
        SET @DaysInTerm = DATEDIFF(DAY, @PolicyEffective, @PolicyExpiry);
        SET @DaysRemaining = DATEDIFF(DAY, @EndorsementEffectiveDate, @PolicyExpiry);
        SET @ProRataFactor = CAST(@DaysRemaining AS DECIMAL(10,8)) / CAST(@DaysInTerm AS DECIMAL(10,8));

        -- Recalculate full premium with new coverages/limits
        DECLARE @NewTotalPremium DECIMAL(18,2), @NewTaxes DECIMAL(18,2), @NewGross DECIMAL(18,2);
        
        EXEC Underwriting.usp_Premium_Calculate
            @PolicyID = @PolicyID,
            @CalculatedBy = @CalculatedBy,
            @RecalculateAll = 1,
            @TotalPremium = @NewTotalPremium OUTPUT,
            @TotalTaxes = @NewTaxes OUTPUT,
            @GrossPremium = @NewGross OUTPUT;

        -- Premium change = (New - Old) * pro-rata factor
        SET @PremiumChange = ROUND((@NewTotalPremium - @CurrentPremium) * @ProRataFactor, 2);

        IF @PremiumChange > 0
        BEGIN
            SET @AdditionalPremium = @PremiumChange;
            SET @ReturnPremium = 0;
        END
        ELSE
        BEGIN
            SET @AdditionalPremium = 0;
            SET @ReturnPremium = ABS(@PremiumChange);
        END

        -- Update endorsement record
        UPDATE Policy.Endorsements SET
            PremiumChange = @PremiumChange,
            ProRataFactor = @ProRataFactor,
            ReturnPremium = @ReturnPremium,
            AdditionalPremium = @AdditionalPremium
        WHERE EndorsementID = @EndorsementID;

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)) + ', EndorsementID=' + CAST(@EndorsementID AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO
