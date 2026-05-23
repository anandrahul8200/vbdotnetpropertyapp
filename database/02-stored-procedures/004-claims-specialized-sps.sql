-- ============================================================
-- CLAIMS SPECIALIZED STORED PROCEDURES
-- Fraud scoring, SIU referral, subrogation management,
-- catastrophe handling, and recovery tracking
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Evaluate Fraud Indicators for a Claim
-- Checks all active indicators, calculates composite score,
-- auto-refers to SIU if threshold exceeded
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Fraud_EvaluateClaim
    @ClaimID INT,
    @EvaluatedBy VARCHAR(50),
    @FraudScore DECIMAL(6,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate claim
        DECLARE @ClaimStatus VARCHAR(20), @ClaimType VARCHAR(30), @LossDate DATETIME;
        DECLARE @ReportedDate DATETIME, @EstimatedLoss DECIMAL(18,2), @CustomerID INT;
        DECLARE @PolicyID INT;

        SELECT @ClaimStatus = ClaimStatus, @ClaimType = ClaimType, @LossDate = LossDate,
               @ReportedDate = ReportedDate, @EstimatedLoss = EstimatedLoss,
               @CustomerID = CustomerID, @PolicyID = PolicyID
        FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @ClaimStatus IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        -- Clear previous scores for this claim
        DELETE FROM Claims.ClaimFraudScores WHERE ClaimID = @ClaimID;

        -- Evaluate each indicator
        DECLARE @IndicatorID INT, @IndicatorCode VARCHAR(20), @Weight DECIMAL(4,2);
        DECLARE @Category VARCHAR(30);
        DECLARE @IsTriggered BIT, @IndicatorNotes VARCHAR(500);
        DECLARE @TotalScore DECIMAL(6,2) = 0;
        DECLARE @MaxPossibleScore DECIMAL(6,2) = 0;

        DECLARE indicator_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT FraudIndicatorID, IndicatorCode, Category, Weight
            FROM Claims.FraudIndicators WHERE IsActive = 1;

        OPEN indicator_cursor;
        FETCH NEXT FROM indicator_cursor INTO @IndicatorID, @IndicatorCode, @Category, @Weight;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            SET @IsTriggered = 0;
            SET @IndicatorNotes = NULL;
            SET @MaxPossibleScore = @MaxPossibleScore + @Weight;

            -- TIMING indicators
            IF @IndicatorCode = 'LATE_REPORT' AND DATEDIFF(DAY, @LossDate, @ReportedDate) > 30
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'Reported ' + CAST(DATEDIFF(DAY, @LossDate, @ReportedDate) AS VARCHAR(50)) + ' days after loss';
            END

            IF @IndicatorCode = 'WEEKEND_LOSS' AND DATEPART(WEEKDAY, @LossDate) IN (1, 7)
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'Loss occurred on weekend';
            END

            IF @IndicatorCode = 'HOLIDAY_LOSS' AND EXISTS (
                SELECT 1 FROM Admin.Holidays WHERE HolidayDate = CAST(@LossDate AS DATE))
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'Loss occurred on holiday';
            END

            -- FINANCIAL indicators
            IF @IndicatorCode = 'NEW_POLICY_CLAIM' AND EXISTS (
                SELECT 1 FROM Policy.Policies WHERE PolicyID = @PolicyID 
                AND DATEDIFF(DAY, EffectiveDate, @LossDate) < 60)
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'Claim within 60 days of policy inception';
            END

            IF @IndicatorCode = 'LIMIT_CLAIM' AND @EstimatedLoss IS NOT NULL
            BEGIN
                DECLARE @PolicyLimit DECIMAL(18,2);
                SELECT @PolicyLimit = PolicyLimit FROM Claims.Claims WHERE ClaimID = @ClaimID;
                IF @EstimatedLoss >= (@PolicyLimit * 0.80)
                BEGIN
                    SET @IsTriggered = 1;
                    SET @IndicatorNotes = 'Estimated loss is ' + CAST(CAST((@EstimatedLoss / @PolicyLimit) * 100 AS INT) AS VARCHAR(50)) + '% of policy limit';
                END
            END

            IF @IndicatorCode = 'PREMIUM_INCREASE' AND EXISTS (
                SELECT 1 FROM Policy.Endorsements WHERE PolicyID = @PolicyID 
                AND EndorsementType = 'INCREASE_LIMITS'
                AND DATEDIFF(DAY, EffectiveDate, @LossDate) < 90)
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'Coverage increased within 90 days of loss';
            END

            -- HISTORY indicators
            IF @IndicatorCode = 'PRIOR_CLAIMS'
            BEGIN
                DECLARE @PriorClaimCount INT;
                SELECT @PriorClaimCount = COUNT(*)
                FROM Claims.Claims
                WHERE CustomerID = @CustomerID AND ClaimID <> @ClaimID
                    AND LossDate >= DATEADD(YEAR, -3, @LossDate);
                IF @PriorClaimCount >= 3
                BEGIN
                    SET @IsTriggered = 1;
                    SET @IndicatorNotes = CAST(@PriorClaimCount AS VARCHAR(50)) + ' claims in last 3 years';
                END
            END

            IF @IndicatorCode = 'PRIOR_DENIED'
            BEGIN
                IF EXISTS (SELECT 1 FROM Claims.Claims WHERE CustomerID = @CustomerID 
                          AND ClaimID <> @ClaimID AND ClaimStatus = 'DENIED')
                BEGIN
                    SET @IsTriggered = 1;
                    SET @IndicatorNotes = 'Customer has prior denied claims';
                END
            END

            -- BEHAVIORAL indicators
            IF @IndicatorCode = 'NO_POLICE_REPORT' AND @ClaimType IN ('THEFT', 'VANDALISM') 
                AND (SELECT PoliceReportNumber FROM Claims.Claims WHERE ClaimID = @ClaimID) IS NULL
            BEGIN
                SET @IsTriggered = 1;
                SET @IndicatorNotes = 'No police report for theft/vandalism claim';
            END

            -- Insert score record
            INSERT INTO Claims.ClaimFraudScores (ClaimID, IndicatorID, IsTriggered, Score, Notes, EvaluatedDate, EvaluatedBy)
            VALUES (@ClaimID, @IndicatorID, @IsTriggered, 
                    CASE WHEN @IsTriggered = 1 THEN @Weight ELSE 0 END,
                    @IndicatorNotes, GETDATE(), @EvaluatedBy);

            IF @IsTriggered = 1
                SET @TotalScore = @TotalScore + @Weight;

            FETCH NEXT FROM indicator_cursor INTO @IndicatorID, @IndicatorCode, @Category, @Weight;
        END

        CLOSE indicator_cursor;
        DEALLOCATE indicator_cursor;

        -- Normalize score to 0-100 scale
        IF @MaxPossibleScore > 0
            SET @FraudScore = ROUND((@TotalScore / @MaxPossibleScore) * 100, 2);
        ELSE
            SET @FraudScore = 0;

        -- Update claim fraud score
        UPDATE Claims.Claims SET
            FraudScore = @FraudScore,
            ModifiedDate = GETDATE(),
            ModifiedBy = @EvaluatedBy
        WHERE ClaimID = @ClaimID;

        -- Auto-refer to SIU if score exceeds threshold
        DECLARE @SIUThreshold DECIMAL(6,2) = 70.0;
        SELECT @SIUThreshold = CAST(ConfigValue AS DECIMAL(6,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'FRAUD_SIU_THRESHOLD';

        IF @FraudScore >= @SIUThreshold
        BEGIN
            UPDATE Claims.Claims SET
                IsSIUReferred = 1,
                SIUReferralDate = GETDATE()
            WHERE ClaimID = @ClaimID AND IsSIUReferred = 0;

            INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
            VALUES (@ClaimID, 'NOTE', GETDATE(), 'AUTO: SIU Referral',
                    'Claim auto-referred to SIU. Fraud score: ' + CAST(@FraudScore AS VARCHAR(50)) + '/100 (threshold: ' + CAST(@SIUThreshold AS VARCHAR(50)) + ')',
                    @EvaluatedBy);
        END

        -- Log evaluation activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'Fraud evaluation completed',
                'Score: ' + CAST(@FraudScore AS VARCHAR(50)) + '/100. Indicators triggered: ' + CAST(CAST(@TotalScore AS INT) AS VARCHAR(50)),
                @EvaluatedBy);

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
-- SP: Refer Claim to SIU (manual referral)
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Fraud_ReferToSIU
    @ClaimID INT,
    @ReferralReason VARCHAR(500),
    @ReferredBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Validate claim
        IF NOT EXISTS (SELECT 1 FROM Claims.Claims WHERE ClaimID = @ClaimID)
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        UPDATE Claims.Claims SET
            IsSIUReferred = 1,
            SIUReferralDate = GETDATE(),
            ModifiedDate = GETDATE(),
            ModifiedBy = @ReferredBy
        WHERE ClaimID = @ClaimID;

        -- Create SIU assignment
        INSERT INTO Claims.Assignments (ClaimID, AssigneeType, AssigneeID, AssignmentDate, Status, Instructions, CreatedDate, CreatedBy)
        VALUES (@ClaimID, 'SIU', 0, GETDATE(), 'ASSIGNED', @ReferralReason, GETDATE(), @ReferredBy);

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'SIU Referral (Manual)',
                'Claim referred to Special Investigations Unit. Reason: ' + @ReferralReason,
                @ReferredBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
        VALUES ('Claims.Claims', @ClaimID, 'UPDATE', 'IsSIUReferred', '0', '1', @ReferredBy, GETDATE());

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get Fraud Evaluation Details
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Fraud_GetEvaluation
    @ClaimID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Fraud score summary
    SELECT cl.ClaimID, cl.ClaimNumber, cl.FraudScore, cl.IsSIUReferred, cl.SIUReferralDate
    FROM Claims.Claims cl WHERE cl.ClaimID = @ClaimID;

    -- Individual indicator results
    SELECT cfs.*, fi.IndicatorCode, fi.IndicatorName, fi.Category, fi.Description AS IndicatorDescription
    FROM Claims.ClaimFraudScores cfs
    INNER JOIN Claims.FraudIndicators fi ON cfs.IndicatorID = fi.FraudIndicatorID
    WHERE cfs.ClaimID = @ClaimID
    ORDER BY cfs.IsTriggered DESC, fi.Category, fi.Weight DESC;
END
GO

-- ============================================================
-- SP: Create Subrogation Record
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Subrogation_Create
    @ClaimID INT,
    @ResponsibleParty VARCHAR(200),
    @ResponsiblePartyInsurer VARCHAR(200) = NULL,
    @ResponsiblePartyPolicy VARCHAR(50) = NULL,
    @DemandAmount DECIMAL(18,2) = NULL,
    @Notes VARCHAR(MAX) = NULL,
    @CreatedBy VARCHAR(50),
    @SubrogationID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate claim
        DECLARE @ClaimStatus VARCHAR(20);
        SELECT @ClaimStatus = ClaimStatus FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @ClaimStatus IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        -- Create subrogation record
        INSERT INTO Claims.Subrogation (
            ClaimID, SubrogationStatus, ResponsibleParty,
            ResponsiblePartyInsurer, ResponsiblePartyPolicy,
            DemandAmount, Notes, CreatedDate, ModifiedDate
        ) VALUES (
            @ClaimID, 'IDENTIFIED', @ResponsibleParty,
            @ResponsiblePartyInsurer, @ResponsiblePartyPolicy,
            @DemandAmount, @Notes, GETDATE(), GETDATE()
        );

        SET @SubrogationID = SCOPE_IDENTITY();

        -- Update claim subrogation flag
        UPDATE Claims.Claims SET
            IsSubrogation = 1,
            SubrogationStatus = 'IDENTIFIED',
            ThirdPartyName = @ResponsibleParty,
            ThirdPartyInsurer = @ResponsiblePartyInsurer,
            ThirdPartyPolicyNumber = @ResponsiblePartyPolicy,
            ModifiedDate = GETDATE(),
            ModifiedBy = @CreatedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'Subrogation identified',
                'Responsible party: ' + @ResponsibleParty 
                + ISNULL(' (Insurer: ' + @ResponsiblePartyInsurer + ')', ''),
                @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Subrogation', @SubrogationID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Update Subrogation Status
-- Tracks demand sent, negotiation, settlement
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Subrogation_UpdateStatus
    @SubrogationID INT,
    @NewStatus VARCHAR(20),
    @DemandAmount DECIMAL(18,2) = NULL,
    @DemandDate DATE = NULL,
    @SettlementAmount DECIMAL(18,2) = NULL,
    @SettlementDate DATE = NULL,
    @ArbitrationDate DATE = NULL,
    @ArbitrationResult VARCHAR(200) = NULL,
    @Notes VARCHAR(MAX) = NULL,
    @ModifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ClaimID INT, @CurrentStatus VARCHAR(20);
        SELECT @ClaimID = ClaimID, @CurrentStatus = SubrogationStatus
        FROM Claims.Subrogation WHERE SubrogationID = @SubrogationID;

        IF @ClaimID IS NULL
        BEGIN
            RAISERROR('Subrogation record not found: %d', 16, 1, @SubrogationID);
            RETURN;
        END

        -- Update subrogation record
        UPDATE Claims.Subrogation SET
            SubrogationStatus = @NewStatus,
            DemandAmount = ISNULL(@DemandAmount, DemandAmount),
            DemandDate = ISNULL(@DemandDate, DemandDate),
            SettlementAmount = ISNULL(@SettlementAmount, SettlementAmount),
            SettlementDate = ISNULL(@SettlementDate, SettlementDate),
            ArbitrationDate = ISNULL(@ArbitrationDate, ArbitrationDate),
            ArbitrationResult = ISNULL(@ArbitrationResult, ArbitrationResult),
            Notes = ISNULL(@Notes, Notes),
            ModifiedDate = GETDATE()
        WHERE SubrogationID = @SubrogationID;

        -- Update claim subrogation status
        UPDATE Claims.Claims SET
            SubrogationStatus = @NewStatus,
            ModifiedDate = GETDATE(),
            ModifiedBy = @ModifiedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(),
                'Subrogation status: ' + @CurrentStatus + ' → ' + @NewStatus,
                ISNULL(@Notes, 'Status updated'),
                @ModifiedBy);

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
-- SP: Record Subrogation Recovery
-- Records money recovered and updates claim totals
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Subrogation_RecordRecovery
    @SubrogationID INT,
    @RecoveryAmount DECIMAL(18,2),
    @RecoveryDate DATE = NULL,
    @Notes VARCHAR(MAX) = NULL,
    @RecordedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @RecoveryDate IS NULL SET @RecoveryDate = CAST(GETDATE() AS DATE);

        DECLARE @ClaimID INT;
        SELECT @ClaimID = ClaimID FROM Claims.Subrogation WHERE SubrogationID = @SubrogationID;

        IF @ClaimID IS NULL
        BEGIN
            RAISERROR('Subrogation record not found: %d', 16, 1, @SubrogationID);
            RETURN;
        END

        -- Update subrogation recovery
        UPDATE Claims.Subrogation SET
            RecoveryAmount = RecoveryAmount + @RecoveryAmount,
            ModifiedDate = GETDATE()
        WHERE SubrogationID = @SubrogationID;

        -- Update claim totals
        UPDATE Claims.Claims SET
            TotalRecovery = TotalRecovery + @RecoveryAmount,
            TotalSubrogation = TotalSubrogation + @RecoveryAmount,
            NetIncurred = TotalReserve + TotalPaid - (TotalRecovery + @RecoveryAmount),
            ModifiedDate = GETDATE(),
            ModifiedBy = @RecordedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'Subrogation recovery received',
                'Recovery of $' + CAST(@RecoveryAmount AS VARCHAR(50)) + ' recorded on ' + CAST(@RecoveryDate AS VARCHAR(50))
                + ISNULL('. ' + @Notes, ''),
                @RecordedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Subrogation', @SubrogationID, 'UPDATE', @RecordedBy, GETDATE());

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
-- SP: Create/Declare Catastrophe Event
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Catastrophe_Create
    @CatastropheName VARCHAR(200),
    @CatastropheType VARCHAR(30),
    @EventDate DATE,
    @EndDate DATE = NULL,
    @AffectedStates VARCHAR(200) = NULL,
    @AffectedZipCodes VARCHAR(MAX) = NULL,
    @EstimatedIndustryLoss DECIMAL(18,2) = NULL,
    @PCSNumber VARCHAR(20) = NULL,
    @CreatedBy VARCHAR(50),
    @CatastropheID INT OUTPUT,
    @CatastropheNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Generate catastrophe number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(CatastropheID), 0) + 1 FROM Claims.Catastrophes;
        SET @CatastropheNumber = 'CAT' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        INSERT INTO Claims.Catastrophes (
            CatastropheNumber, CatastropheName, CatastropheType,
            EventDate, EndDate, AffectedStates, AffectedZipCodes,
            EstimatedIndustryLoss, PCSNumber, IsActive, DeclaredDate, CreatedDate
        ) VALUES (
            @CatastropheNumber, @CatastropheName, @CatastropheType,
            @EventDate, @EndDate, @AffectedStates, @AffectedZipCodes,
            @EstimatedIndustryLoss, @PCSNumber, 1, GETDATE(), GETDATE()
        );

        SET @CatastropheID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Catastrophes', @CatastropheID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Link Claim to Catastrophe
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Catastrophe_LinkClaim
    @ClaimID INT,
    @CatastropheID INT,
    @LinkedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate both exist
    IF NOT EXISTS (SELECT 1 FROM Claims.Claims WHERE ClaimID = @ClaimID)
    BEGIN
        RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM Claims.Catastrophes WHERE CatastropheID = @CatastropheID AND IsActive = 1)
    BEGIN
        RAISERROR('Active catastrophe not found: %d', 16, 1, @CatastropheID);
        RETURN;
    END

    -- Link claim
    UPDATE Claims.Claims SET
        CatastropheID = @CatastropheID,
        ModifiedDate = GETDATE(),
        ModifiedBy = @LinkedBy
    WHERE ClaimID = @ClaimID;

    -- Update catastrophe counts
    UPDATE Claims.Catastrophes SET
        TotalClaimsCount = (SELECT COUNT(*) FROM Claims.Claims WHERE CatastropheID = @CatastropheID),
        TotalReserveAmount = (SELECT ISNULL(SUM(TotalReserve), 0) FROM Claims.Claims WHERE CatastropheID = @CatastropheID),
        TotalPaidAmount = (SELECT ISNULL(SUM(TotalPaid), 0) FROM Claims.Claims WHERE CatastropheID = @CatastropheID)
    WHERE CatastropheID = @CatastropheID;

    -- Log activity
    DECLARE @CatName VARCHAR(200);
    SELECT @CatName = CatastropheName FROM Claims.Catastrophes WHERE CatastropheID = @CatastropheID;

    INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
    VALUES (@ClaimID, 'NOTE', GETDATE(), 'Linked to catastrophe',
            'Claim linked to CAT event: ' + @CatName, @LinkedBy);
END
GO

-- ============================================================
-- SP: Get Catastrophe Summary with Claims
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Catastrophe_GetSummary
    @CatastropheID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Catastrophe header
    SELECT * FROM Claims.Catastrophes WHERE CatastropheID = @CatastropheID;

    -- Claims linked to this catastrophe
    SELECT cl.ClaimID, cl.ClaimNumber, cl.ClaimStatus, cl.ClaimType,
           cl.LossDate, cl.EstimatedLoss, cl.TotalPaid, cl.TotalReserve, cl.NetIncurred,
           c.FirstName + ' ' + c.LastName AS CustomerName,
           pr.AddressLine1 + ', ' + pr.City + ' ' + pr.StateCode AS PropertyAddress
    FROM Claims.Claims cl
    INNER JOIN Policy.Customers c ON cl.CustomerID = c.CustomerID
    INNER JOIN Policy.Properties pr ON cl.PropertyID = pr.PropertyID
    WHERE cl.CatastropheID = @CatastropheID
    ORDER BY cl.LossDate;

    -- Summary by status
    SELECT ClaimStatus, COUNT(*) AS ClaimCount, 
           SUM(TotalReserve) AS TotalReserve, SUM(TotalPaid) AS TotalPaid
    FROM Claims.Claims
    WHERE CatastropheID = @CatastropheID
    GROUP BY ClaimStatus;
END
GO

-- ============================================================
-- SP: Create/Update Vendor
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Vendor_Create
    @VendorName VARCHAR(200),
    @VendorType VARCHAR(30),
    @ContactName VARCHAR(200) = NULL,
    @Phone VARCHAR(20) = NULL,
    @Email VARCHAR(200) = NULL,
    @AddressLine1 VARCHAR(200) = NULL,
    @City VARCHAR(100) = NULL,
    @StateCode CHAR(2) = NULL,
    @ZipCode VARCHAR(10) = NULL,
    @LicenseNumber VARCHAR(50) = NULL,
    @HourlyRate DECIMAL(10,2) = NULL,
    @DailyRate DECIMAL(10,2) = NULL,
    @PreferredVendor BIT = 0,
    @CreatedBy VARCHAR(50),
    @VendorID INT OUTPUT,
    @VendorNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Generate vendor number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(VendorID), 0) + 1 FROM Claims.Vendors;
        SET @VendorNumber = 'VND' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        INSERT INTO Claims.Vendors (
            VendorNumber, VendorName, VendorType, ContactName,
            Phone, Email, AddressLine1, City, StateCode, ZipCode,
            LicenseNumber, HourlyRate, DailyRate, PreferredVendor,
            IsActive, CreatedDate, ModifiedDate
        ) VALUES (
            @VendorNumber, @VendorName, @VendorType, @ContactName,
            @Phone, @Email, @AddressLine1, @City, @StateCode, @ZipCode,
            @LicenseNumber, @HourlyRate, @DailyRate, @PreferredVendor,
            1, GETDATE(), GETDATE()
        );

        SET @VendorID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Vendors', @VendorID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Search Vendors
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Vendor_Search
    @VendorName VARCHAR(200) = NULL,
    @VendorType VARCHAR(30) = NULL,
    @StateCode CHAR(2) = NULL,
    @PreferredOnly BIT = 0,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT v.*
    FROM Claims.Vendors v
    WHERE (@VendorName IS NULL OR v.VendorName LIKE '%' + @VendorName + '%')
        AND (@VendorType IS NULL OR v.VendorType = @VendorType)
        AND (@StateCode IS NULL OR v.StateCode = @StateCode)
        AND (@PreferredOnly = 0 OR v.PreferredVendor = 1)
        AND v.IsActive = @IsActive
    ORDER BY v.PreferredVendor DESC, v.Rating DESC, v.VendorName;
END
GO
