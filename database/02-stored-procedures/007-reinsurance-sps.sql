-- ============================================================
-- REINSURANCE STORED PROCEDURES
-- Treaty management, cession calculations, bordereaux
-- generation, and recovery tracking
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Create Reinsurance Treaty
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Treaty_Create
    @TreatyName VARCHAR(200),
    @TreatyType VARCHAR(30),
    @ReinsurerID INT = NULL,
    @EffectiveDate DATE,
    @ExpiryDate DATE,
    @RetentionAmount DECIMAL(18,2) = NULL,
    @RetentionPercent DECIMAL(6,4) = NULL,
    @CessionPercent DECIMAL(6,4) = NULL,
    @CessionLimit DECIMAL(18,2) = NULL,
    @AttachmentPoint DECIMAL(18,2) = NULL,
    @ExhaustionPoint DECIMAL(18,2) = NULL,
    @PremiumRate DECIMAL(10,6) = NULL,
    @MinimumPremium DECIMAL(18,2) = NULL,
    @DepositPremium DECIMAL(18,2) = NULL,
    @CommissionRate DECIMAL(6,4) = NULL,
    @CoveredPerils VARCHAR(500) = NULL,
    @CoveredStates VARCHAR(200) = NULL,
    @CoveredPolicyTypes VARCHAR(200) = NULL,
    @CreatedBy VARCHAR(50),
    @TreatyID INT OUTPUT,
    @TreatyNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Generate treaty number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(TreatyID), 0) + 1 FROM Reinsurance.Treaties;
        SET @TreatyNumber = 'TRY' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        INSERT INTO Reinsurance.Treaties (
            TreatyNumber, TreatyName, TreatyType, ReinsurerID,
            EffectiveDate, ExpiryDate, RetentionAmount, RetentionPercent,
            CessionPercent, CessionLimit, AttachmentPoint, ExhaustionPoint,
            PremiumRate, MinimumPremium, DepositPremium, CommissionRate,
            CoveredPerils, CoveredStates, CoveredPolicyTypes,
            Status, CreatedDate, ModifiedDate
        ) VALUES (
            @TreatyNumber, @TreatyName, @TreatyType, @ReinsurerID,
            @EffectiveDate, @ExpiryDate, @RetentionAmount, @RetentionPercent,
            @CessionPercent, @CessionLimit, @AttachmentPoint, @ExhaustionPoint,
            @PremiumRate, @MinimumPremium, @DepositPremium, @CommissionRate,
            @CoveredPerils, @CoveredStates, @CoveredPolicyTypes,
            'ACTIVE', GETDATE(), GETDATE()
        );

        SET @TreatyID = SCOPE_IDENTITY();

        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Reinsurance.Treaties', @TreatyID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Calculate and Record Premium Cession
-- Determines how much premium to cede based on treaty type
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Cession_CalculatePremium
    @PolicyID INT,
    @GrossPremium DECIMAL(18,2),
    @CreatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PolicyType VARCHAR(30), @StateCode CHAR(2), @EffectiveDate DATE;
        DECLARE @TotalInsuredValue DECIMAL(18,2);

        SELECT @PolicyType = p.PolicyType, @EffectiveDate = p.EffectiveDate,
               @TotalInsuredValue = p.TotalInsuredValue
        FROM Policy.Policies p WHERE p.PolicyID = @PolicyID;

        SELECT @StateCode = pr.StateCode
        FROM Policy.Properties pr
        INNER JOIN Policy.Policies p ON pr.PropertyID = p.PropertyID
        WHERE p.PolicyID = @PolicyID;

        -- Find applicable treaties
        DECLARE @TreatyID INT, @TreatyType VARCHAR(30), @CessionPct DECIMAL(6,4);
        DECLARE @RetentionAmt DECIMAL(18,2), @CessionLimit DECIMAL(18,2);
        DECLARE @AttachmentPt DECIMAL(18,2), @ExhaustionPt DECIMAL(18,2);

        DECLARE treaty_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TreatyID, TreatyType, CessionPercent, RetentionAmount, CessionLimit, AttachmentPoint, ExhaustionPoint
            FROM Reinsurance.Treaties
            WHERE Status = 'ACTIVE'
                AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate
                AND (CoveredPolicyTypes LIKE '%' + @PolicyType + '%' OR CoveredPolicyTypes IS NULL)
                AND (CoveredStates LIKE '%' + @StateCode + '%' OR CoveredStates IS NULL);

        OPEN treaty_cursor;
        FETCH NEXT FROM treaty_cursor INTO @TreatyID, @TreatyType, @CessionPct, @RetentionAmt, @CessionLimit, @AttachmentPt, @ExhaustionPt;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @CededAmount DECIMAL(18,2) = 0;
            DECLARE @RetainedAmount DECIMAL(18,2) = 0;

            IF @TreatyType = 'QUOTA_SHARE'
            BEGIN
                SET @CededAmount = ROUND(@GrossPremium * @CessionPct, 2);
                SET @RetainedAmount = @GrossPremium - @CededAmount;
            END
            ELSE IF @TreatyType = 'SURPLUS'
            BEGIN
                -- Cede premium proportional to TIV above retention
                IF @TotalInsuredValue > @RetentionAmt
                BEGIN
                    DECLARE @SurplusPct DECIMAL(6,4) = 
                        CAST((@TotalInsuredValue - @RetentionAmt) AS DECIMAL(18,2)) / @TotalInsuredValue;
                    IF @CessionLimit IS NOT NULL AND (@TotalInsuredValue - @RetentionAmt) > @CessionLimit
                        SET @SurplusPct = CAST(@CessionLimit AS DECIMAL(18,2)) / @TotalInsuredValue;
                    SET @CededAmount = ROUND(@GrossPremium * @SurplusPct, 2);
                    SET @RetainedAmount = @GrossPremium - @CededAmount;
                END
                ELSE
                BEGIN
                    SET @RetainedAmount = @GrossPremium;
                END
            END
            ELSE IF @TreatyType = 'EXCESS_OF_LOSS'
            BEGIN
                -- Premium cession is flat rate for XOL
                SET @CededAmount = ROUND(@GrossPremium * ISNULL(@CessionPct, 0), 2);
                SET @RetainedAmount = @GrossPremium - @CededAmount;
            END

            -- Record cession if amount > 0
            IF @CededAmount > 0
            BEGIN
                INSERT INTO Reinsurance.Cessions (
                    TreatyID, PolicyID, CessionType, GrossAmount, CededAmount, RetainedAmount,
                    CessionPercent, TransactionDate, AccountingPeriod, Status, CreatedDate
                ) VALUES (
                    @TreatyID, @PolicyID, 'PREMIUM', @GrossPremium, @CededAmount, @RetainedAmount,
                    CAST(@CededAmount AS DECIMAL(6,4)) / NULLIF(@GrossPremium, 0),
                    CAST(GETDATE() AS DATE), FORMAT(GETDATE(), 'yyyy-MM'), 'PENDING', GETDATE()
                );
            END

            FETCH NEXT FROM treaty_cursor INTO @TreatyID, @TreatyType, @CessionPct, @RetentionAmt, @CessionLimit, @AttachmentPt, @ExhaustionPt;
        END

        CLOSE treaty_cursor;
        DEALLOCATE treaty_cursor;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Calculate Loss Cession for a Claim
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Cession_CalculateLoss
    @ClaimID INT,
    @LossAmount DECIMAL(18,2),
    @CreatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PolicyID INT, @PolicyType VARCHAR(30), @StateCode CHAR(2), @EffectiveDate DATE;

        SELECT @PolicyID = cl.PolicyID
        FROM Claims.Claims cl WHERE cl.ClaimID = @ClaimID;

        SELECT @PolicyType = p.PolicyType, @EffectiveDate = p.EffectiveDate
        FROM Policy.Policies p WHERE p.PolicyID = @PolicyID;

        SELECT @StateCode = pr.StateCode
        FROM Policy.Properties pr INNER JOIN Policy.Policies p ON pr.PropertyID = p.PropertyID
        WHERE p.PolicyID = @PolicyID;

        -- Find applicable treaties for loss
        DECLARE @TreatyID INT, @TreatyType VARCHAR(30), @CessionPct DECIMAL(6,4);
        DECLARE @RetentionAmt DECIMAL(18,2), @AttachmentPt DECIMAL(18,2), @ExhaustionPt DECIMAL(18,2);

        DECLARE treaty_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TreatyID, TreatyType, CessionPercent, RetentionAmount, AttachmentPoint, ExhaustionPoint
            FROM Reinsurance.Treaties
            WHERE Status = 'ACTIVE'
                AND @EffectiveDate BETWEEN EffectiveDate AND ExpiryDate
                AND (CoveredPolicyTypes LIKE '%' + @PolicyType + '%' OR CoveredPolicyTypes IS NULL)
                AND (CoveredStates LIKE '%' + @StateCode + '%' OR CoveredStates IS NULL);

        OPEN treaty_cursor;
        FETCH NEXT FROM treaty_cursor INTO @TreatyID, @TreatyType, @CessionPct, @RetentionAmt, @AttachmentPt, @ExhaustionPt;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @CededLoss DECIMAL(18,2) = 0;
            DECLARE @RetainedLoss DECIMAL(18,2) = 0;

            IF @TreatyType = 'QUOTA_SHARE'
            BEGIN
                SET @CededLoss = ROUND(@LossAmount * @CessionPct, 2);
                SET @RetainedLoss = @LossAmount - @CededLoss;
            END
            ELSE IF @TreatyType = 'EXCESS_OF_LOSS'
            BEGIN
                IF @LossAmount > @AttachmentPt
                BEGIN
                    SET @CededLoss = LEAST(@LossAmount - @AttachmentPt, @ExhaustionPt - @AttachmentPt);
                    SET @RetainedLoss = @LossAmount - @CededLoss;
                END
                ELSE
                BEGIN
                    SET @RetainedLoss = @LossAmount;
                END
            END
            ELSE IF @TreatyType = 'SURPLUS'
            BEGIN
                -- Use same proportion as premium cession
                DECLARE @ExistingPct DECIMAL(6,4) = 0;
                SELECT TOP 1 @ExistingPct = CessionPercent
                FROM Reinsurance.Cessions
                WHERE TreatyID = @TreatyID AND PolicyID = @PolicyID AND CessionType = 'PREMIUM'
                ORDER BY TransactionDate DESC;

                SET @CededLoss = ROUND(@LossAmount * ISNULL(@ExistingPct, 0), 2);
                SET @RetainedLoss = @LossAmount - @CededLoss;
            END

            IF @CededLoss > 0
            BEGIN
                INSERT INTO Reinsurance.Cessions (
                    TreatyID, PolicyID, ClaimID, CessionType, GrossAmount, CededAmount, RetainedAmount,
                    CessionPercent, TransactionDate, AccountingPeriod, Status, CreatedDate
                ) VALUES (
                    @TreatyID, @PolicyID, @ClaimID, 'LOSS', @LossAmount, @CededLoss, @RetainedLoss,
                    CAST(@CededLoss AS DECIMAL(6,4)) / NULLIF(@LossAmount, 0),
                    CAST(GETDATE() AS DATE), FORMAT(GETDATE(), 'yyyy-MM'), 'PENDING', GETDATE()
                );
            END

            FETCH NEXT FROM treaty_cursor INTO @TreatyID, @TreatyType, @CessionPct, @RetentionAmt, @AttachmentPt, @ExhaustionPt;
        END

        CLOSE treaty_cursor;
        DEALLOCATE treaty_cursor;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Generate Bordereaux Report
-- Aggregates cessions for a treaty/period
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Bordereaux_Generate
    @TreatyID INT,
    @ReportingPeriod VARCHAR(10), -- YYYY-MM
    @ReportType VARCHAR(20), -- PREMIUM, LOSS, OUTSTANDING
    @GeneratedBy VARCHAR(50),
    @BordereauxID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate treaty
        IF NOT EXISTS (SELECT 1 FROM Reinsurance.Treaties WHERE TreatyID = @TreatyID)
        BEGIN
            RAISERROR('Treaty not found: %d', 16, 1, @TreatyID);
            RETURN;
        END

        -- Calculate totals
        DECLARE @TotalGross DECIMAL(18,2) = 0, @TotalCeded DECIMAL(18,2) = 0;
        DECLARE @TotalRetained DECIMAL(18,2) = 0, @RecordCount INT = 0;

        SELECT @TotalGross = ISNULL(SUM(GrossAmount), 0),
               @TotalCeded = ISNULL(SUM(CededAmount), 0),
               @TotalRetained = ISNULL(SUM(RetainedAmount), 0),
               @RecordCount = COUNT(*)
        FROM Reinsurance.Cessions
        WHERE TreatyID = @TreatyID AND AccountingPeriod = @ReportingPeriod
            AND CessionType = CASE @ReportType WHEN 'PREMIUM' THEN 'PREMIUM' WHEN 'LOSS' THEN 'LOSS' ELSE CessionType END;

        -- Create bordereaux record
        INSERT INTO Reinsurance.Bordereaux (
            TreatyID, ReportingPeriod, ReportType,
            TotalGross, TotalCeded, TotalRetained, RecordCount,
            GeneratedDate, Status, CreatedDate
        ) VALUES (
            @TreatyID, @ReportingPeriod, @ReportType,
            @TotalGross, @TotalCeded, @TotalRetained, @RecordCount,
            GETDATE(), 'DRAFT', GETDATE()
        );

        SET @BordereauxID = SCOPE_IDENTITY();

        -- Mark cessions as reported
        UPDATE Reinsurance.Cessions SET Status = 'REPORTED'
        WHERE TreatyID = @TreatyID AND AccountingPeriod = @ReportingPeriod
            AND CessionType = CASE @ReportType WHEN 'PREMIUM' THEN 'PREMIUM' WHEN 'LOSS' THEN 'LOSS' ELSE CessionType END
            AND Status = 'PENDING';

        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Reinsurance.Bordereaux', @BordereauxID, 'INSERT', @GeneratedBy, GETDATE());

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
-- SP: Get Treaty Summary with Cession Totals
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Treaty_GetSummary
    @TreatyID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Treaty header
    SELECT t.*, r.ReinsurerName, r.AMBestRating, r.SPRating
    FROM Reinsurance.Treaties t
    LEFT JOIN Reinsurance.Reinsurers r ON t.ReinsurerID = r.ReinsurerID
    WHERE t.TreatyID = @TreatyID;

    -- Cession summary by type
    SELECT CessionType,
           COUNT(*) AS TransactionCount,
           SUM(GrossAmount) AS TotalGross,
           SUM(CededAmount) AS TotalCeded,
           SUM(RetainedAmount) AS TotalRetained
    FROM Reinsurance.Cessions
    WHERE TreatyID = @TreatyID
    GROUP BY CessionType;

    -- Bordereaux history
    SELECT * FROM Reinsurance.Bordereaux
    WHERE TreatyID = @TreatyID
    ORDER BY ReportingPeriod DESC;

    -- Recent cessions (last 50)
    SELECT TOP 50 c.*, p.PolicyNumber, cl.ClaimNumber
    FROM Reinsurance.Cessions c
    LEFT JOIN Policy.Policies p ON c.PolicyID = p.PolicyID
    LEFT JOIN Claims.Claims cl ON c.ClaimID = cl.ClaimID
    WHERE c.TreatyID = @TreatyID
    ORDER BY c.TransactionDate DESC;
END
GO

-- ============================================================
-- SP: Get Active Treaties List
-- ============================================================
CREATE OR ALTER PROCEDURE Reinsurance.usp_Treaty_GetActive
    @AsOfDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @AsOfDate IS NULL SET @AsOfDate = CAST(GETDATE() AS DATE);

    SELECT t.*, r.ReinsurerName, r.AMBestRating,
        (SELECT ISNULL(SUM(CededAmount), 0) FROM Reinsurance.Cessions WHERE TreatyID = t.TreatyID AND CessionType = 'PREMIUM') AS TotalCededPremium,
        (SELECT ISNULL(SUM(CededAmount), 0) FROM Reinsurance.Cessions WHERE TreatyID = t.TreatyID AND CessionType = 'LOSS') AS TotalCededLoss
    FROM Reinsurance.Treaties t
    LEFT JOIN Reinsurance.Reinsurers r ON t.ReinsurerID = r.ReinsurerID
    WHERE t.Status = 'ACTIVE' AND @AsOfDate BETWEEN t.EffectiveDate AND t.ExpiryDate
    ORDER BY t.TreatyType, t.TreatyName;
END
GO
