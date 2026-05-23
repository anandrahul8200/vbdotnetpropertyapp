-- ============================================================
-- CLAIMS PROCESSING STORED PROCEDURES
-- Core claims lifecycle: FNOL, status changes, reserves,
-- payments, activities, assignments, and claim retrieval
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Create Claim (First Notice of Loss - FNOL)
-- Validates policy/coverage, generates claim number, sets
-- initial reserve, assigns adjuster, logs activity
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_Create
    @PolicyID INT,
    @ClaimType VARCHAR(30),
    @LossDate DATETIME,
    @LossDescription VARCHAR(MAX),
    @LossLocation VARCHAR(500) = NULL,
    @EstimatedLoss DECIMAL(18,2) = NULL,
    @PoliceReportNumber VARCHAR(50) = NULL,
    @FireReportNumber VARCHAR(50) = NULL,
    @WeatherCondition VARCHAR(50) = NULL,
    @PointOfOrigin VARCHAR(200) = NULL,
    @CatastropheID INT = NULL,
    @Priority VARCHAR(10) = 'NORMAL',
    @CreatedBy VARCHAR(50),
    @ClaimID INT OUTPUT,
    @ClaimNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate policy exists and is active
        DECLARE @CustomerID INT, @PropertyID INT, @PolicyStatus VARCHAR(20);
        DECLARE @PolicyEffective DATE, @PolicyExpiry DATE;

        SELECT @CustomerID = CustomerID, @PropertyID = PropertyID,
               @PolicyStatus = PolicyStatus, @PolicyEffective = EffectiveDate,
               @PolicyExpiry = ExpiryDate
        FROM Policy.Policies
        WHERE PolicyID = @PolicyID;

        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        IF @PolicyStatus NOT IN ('ACTIVE', 'PENDING_CANCEL')
        BEGIN
            RAISERROR('Policy is not active. Current status: %s', 16, 1, @PolicyStatus);
            RETURN;
        END

        -- Validate loss date is within policy period
        IF CAST(@LossDate AS DATE) < @PolicyEffective OR CAST(@LossDate AS DATE) > @PolicyExpiry
        BEGIN
            RAISERROR('Loss date is outside the policy period', 16, 1);
            RETURN;
        END

        -- Generate claim number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(ClaimID), 0) + 1 FROM Claims.Claims;
        SET @ClaimNumber = 'CLM' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Determine complexity based on estimated loss
        DECLARE @Complexity VARCHAR(10) = 'SIMPLE';
        IF @EstimatedLoss IS NOT NULL
        BEGIN
            IF @EstimatedLoss > 100000 SET @Complexity = 'COMPLEX';
            ELSE IF @EstimatedLoss > 25000 SET @Complexity = 'MODERATE';
        END

        -- Get deductible from policy's primary coverage
        DECLARE @DeductibleAmount DECIMAL(18,2) = 0;
        DECLARE @PolicyLimit DECIMAL(18,2) = 0;
        SELECT TOP 1 @DeductibleAmount = DeductibleAmount, @PolicyLimit = LimitAmount
        FROM Policy.Coverages
        WHERE PolicyID = @PolicyID AND IsSelected = 1
        ORDER BY LimitAmount DESC;

        -- Insert claim
        INSERT INTO Claims.Claims (
            ClaimNumber, PolicyID, CustomerID, PropertyID,
            ClaimStatus, ClaimType, CatastropheID,
            LossDate, ReportedDate, LossDescription, LossLocation,
            PoliceReportNumber, FireReportNumber, WeatherCondition, PointOfOrigin,
            EstimatedLoss, DeductibleAmount, PolicyLimit,
            Priority, Complexity,
            CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
        ) VALUES (
            @ClaimNumber, @PolicyID, @CustomerID, @PropertyID,
            'FNOL', @ClaimType, @CatastropheID,
            @LossDate, GETDATE(), @LossDescription, @LossLocation,
            @PoliceReportNumber, @FireReportNumber, @WeatherCondition, @PointOfOrigin,
            @EstimatedLoss, @DeductibleAmount, @PolicyLimit,
            @Priority, @Complexity,
            GETDATE(), @CreatedBy, GETDATE(), @CreatedBy
        );

        SET @ClaimID = SCOPE_IDENTITY();

        -- Create initial status history
        INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
        VALUES (@ClaimID, NULL, 'FNOL', GETDATE(), @CreatedBy, 'First Notice of Loss received');

        -- Log FNOL activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'FNOL Received', 
                'First Notice of Loss created. Type: ' + @ClaimType + '. Estimated loss: $' + ISNULL(CAST(@EstimatedLoss AS VARCHAR(50)), 'Unknown'),
                @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Claims', @ClaimID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Update Claim Status
-- Validates allowed status transitions, logs history
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_UpdateStatus
    @ClaimID INT,
    @NewStatus VARCHAR(20),
    @Reason VARCHAR(500) = NULL,
    @Notes VARCHAR(MAX) = NULL,
    @DenialReason VARCHAR(500) = NULL,
    @ModifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get current status
        DECLARE @CurrentStatus VARCHAR(20);
        SELECT @CurrentStatus = ClaimStatus FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @CurrentStatus IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        -- Validate status transition
        DECLARE @IsValid BIT = 0;
        SELECT @IsValid = CASE
            WHEN @CurrentStatus = 'FNOL' AND @NewStatus IN ('ASSIGNED', 'DENIED', 'CLOSED') THEN 1
            WHEN @CurrentStatus = 'ASSIGNED' AND @NewStatus IN ('INVESTIGATING', 'DENIED', 'CLOSED') THEN 1
            WHEN @CurrentStatus = 'INVESTIGATING' AND @NewStatus IN ('ASSESSED', 'DENIED', 'CLOSED', 'LITIGATION') THEN 1
            WHEN @CurrentStatus = 'ASSESSED' AND @NewStatus IN ('APPROVED', 'DENIED', 'CLOSED') THEN 1
            WHEN @CurrentStatus = 'APPROVED' AND @NewStatus IN ('SETTLED', 'CLOSED') THEN 1
            WHEN @CurrentStatus = 'DENIED' AND @NewStatus IN ('REOPENED', 'CLOSED') THEN 1
            WHEN @CurrentStatus = 'SETTLED' AND @NewStatus IN ('CLOSED', 'REOPENED') THEN 1
            WHEN @CurrentStatus = 'CLOSED' AND @NewStatus IN ('REOPENED') THEN 1
            WHEN @CurrentStatus = 'REOPENED' AND @NewStatus IN ('INVESTIGATING', 'ASSIGNED') THEN 1
            WHEN @CurrentStatus = 'LITIGATION' AND @NewStatus IN ('SETTLED', 'CLOSED', 'DENIED') THEN 1
            ELSE 0
        END;

        IF @IsValid = 0
        BEGIN
            RAISERROR('Invalid status transition from %s to %s', 16, 1, @CurrentStatus, @NewStatus);
            RETURN;
        END

        -- Update claim status
        UPDATE Claims.Claims SET
            ClaimStatus = @NewStatus,
            ClosedDate = CASE WHEN @NewStatus = 'CLOSED' THEN GETDATE() ELSE ClosedDate END,
            ReopenedDate = CASE WHEN @NewStatus = 'REOPENED' THEN GETDATE() ELSE ReopenedDate END,
            ReopenedReason = CASE WHEN @NewStatus = 'REOPENED' THEN @Reason ELSE ReopenedReason END,
            DenialReason = CASE WHEN @NewStatus = 'DENIED' THEN @DenialReason ELSE DenialReason END,
            AssignedDate = CASE WHEN @NewStatus = 'ASSIGNED' THEN GETDATE() ELSE AssignedDate END,
            ModifiedDate = GETDATE(),
            ModifiedBy = @ModifiedBy
        WHERE ClaimID = @ClaimID;

        -- Log status history
        INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason, Notes)
        VALUES (@ClaimID, @CurrentStatus, @NewStatus, GETDATE(), @ModifiedBy, @Reason, @Notes);

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'STATUS_CHANGE', GETDATE(), 
                'Status changed: ' + @CurrentStatus + ' → ' + @NewStatus,
                ISNULL(@Reason, '') + CASE WHEN @Notes IS NOT NULL THEN ' | ' + @Notes ELSE '' END,
                @ModifiedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
        VALUES ('Claims.Claims', @ClaimID, 'UPDATE', 'ClaimStatus', @CurrentStatus, @NewStatus, @ModifiedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)) + ', NewStatus=' + @NewStatus);
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Set/Change Claim Reserve
-- Creates reserve record, updates claim totals, checks
-- approval thresholds, logs activity
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_SetReserve
    @ClaimID INT,
    @ReserveType VARCHAR(20),
    @ReserveCategory VARCHAR(30) = 'INDEMNITY',
    @CoverageCode VARCHAR(20) = NULL,
    @Amount DECIMAL(18,2),
    @ChangeReason VARCHAR(200) = NULL,
    @SetBy VARCHAR(50),
    @ReserveID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate claim exists and is open
        DECLARE @ClaimStatus VARCHAR(20);
        SELECT @ClaimStatus = ClaimStatus FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @ClaimStatus IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        IF @ClaimStatus IN ('CLOSED', 'DENIED')
        BEGIN
            RAISERROR('Cannot set reserve on a closed or denied claim', 16, 1);
            RETURN;
        END

        -- Get previous reserve amount for this type/category
        DECLARE @PreviousAmount DECIMAL(18,2) = 0;
        SELECT TOP 1 @PreviousAmount = Amount
        FROM Claims.Reserves
        WHERE ClaimID = @ClaimID AND ReserveType = @ReserveType 
            AND ReserveCategory = @ReserveCategory
            AND ISNULL(CoverageCode, '') = ISNULL(@CoverageCode, '')
        ORDER BY CreatedDate DESC;

        DECLARE @ChangeAmount DECIMAL(18,2) = @Amount - @PreviousAmount;

        -- Check if approval is required (threshold from config)
        DECLARE @ApprovalThreshold DECIMAL(18,2) = 50000;
        SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'RESERVE_APPROVAL_THRESHOLD';

        DECLARE @ApprovalRequired BIT = CASE WHEN @Amount > @ApprovalThreshold THEN 1 ELSE 0 END;

        -- Insert reserve record
        INSERT INTO Claims.Reserves (
            ClaimID, ReserveType, ReserveCategory, CoverageCode,
            Amount, PreviousAmount, ChangeAmount, ChangeReason,
            EffectiveDate, SetBy, ApprovalRequired, IsApproved,
            CreatedDate, ModifiedDate
        ) VALUES (
            @ClaimID, @ReserveType, @ReserveCategory, @CoverageCode,
            @Amount, @PreviousAmount, @ChangeAmount, @ChangeReason,
            GETDATE(), @SetBy, @ApprovalRequired, 
            CASE WHEN @ApprovalRequired = 1 THEN 0 ELSE 1 END,
            GETDATE(), GETDATE()
        );

        SET @ReserveID = SCOPE_IDENTITY();

        -- Update claim total reserve
        UPDATE Claims.Claims SET
            TotalReserve = (SELECT ISNULL(SUM(Amount), 0) FROM Claims.Reserves 
                           WHERE ClaimID = @ClaimID AND IsApproved = 1),
            NetIncurred = (SELECT ISNULL(SUM(Amount), 0) FROM Claims.Reserves 
                          WHERE ClaimID = @ClaimID AND IsApproved = 1)
                        + TotalPaid - TotalRecovery,
            ModifiedDate = GETDATE(),
            ModifiedBy = @SetBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'RESERVE_CHANGE', GETDATE(),
                'Reserve ' + CASE WHEN @PreviousAmount = 0 THEN 'set' ELSE 'changed' END + ': ' + @ReserveType,
                @ReserveCategory + ' reserve ' + CASE WHEN @ChangeAmount >= 0 THEN 'increased' ELSE 'decreased' END 
                + ' by $' + CAST(ABS(@ChangeAmount) AS VARCHAR(50)) + ' to $' + CAST(@Amount AS VARCHAR(50))
                + CASE WHEN @ApprovalRequired = 1 THEN ' (PENDING APPROVAL)' ELSE '' END,
                @SetBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Reserves', @ReserveID, 'INSERT', @SetBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)) + ', Amount=' + CAST(@Amount AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Create Claim Payment
-- Validates against reserve/limit, generates payment number,
-- checks approval threshold, updates claim totals
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_CreatePayment
    @ClaimID INT,
    @PaymentType VARCHAR(20),
    @PaymentMethod VARCHAR(20) = 'CHECK',
    @PayeeType VARCHAR(20),
    @PayeeName VARCHAR(200),
    @PayeeAddress VARCHAR(500) = NULL,
    @Amount DECIMAL(18,2),
    @CoverageCode VARCHAR(20) = NULL,
    @InvoiceNumber VARCHAR(50) = NULL,
    @Description VARCHAR(500) = NULL,
    @TaxReportable BIT = 0,
    @CreatedBy VARCHAR(50),
    @PaymentID INT OUTPUT,
    @PaymentNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Validate claim exists and is in payable status
        DECLARE @ClaimStatus VARCHAR(20), @TotalPaid DECIMAL(18,2), @PolicyLimit DECIMAL(18,2);
        DECLARE @TotalReserve DECIMAL(18,2), @DeductibleAmount DECIMAL(18,2);

        SELECT @ClaimStatus = ClaimStatus, @TotalPaid = TotalPaid, 
               @PolicyLimit = PolicyLimit, @TotalReserve = TotalReserve,
               @DeductibleAmount = DeductibleAmount
        FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @ClaimStatus IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        IF @ClaimStatus IN ('FNOL', 'CLOSED', 'DENIED')
        BEGIN
            RAISERROR('Cannot create payment on claim with status: %s', 16, 1, @ClaimStatus);
            RETURN;
        END

        -- Check policy limit
        IF (@TotalPaid + @Amount) > @PolicyLimit
        BEGIN
            DECLARE @ErrMsg VARCHAR(500);
            SET @ErrMsg = 'Payment would exceed policy limit. Limit: ' + CAST(@PolicyLimit AS VARCHAR(50)) + ', Already paid: ' + CAST(@TotalPaid AS VARCHAR(50)) + ', Requested: ' + CAST(@Amount AS VARCHAR(50));
            RAISERROR(@ErrMsg, 16, 1);
            RETURN;
        END

        -- Generate payment number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(PaymentID), 0) + 1 FROM Claims.Payments;
        SET @PaymentNumber = 'PAY' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Check approval threshold
        DECLARE @ApprovalThreshold DECIMAL(18,2) = 10000;
        SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'PAYMENT_APPROVAL_THRESHOLD';

        DECLARE @ApprovalRequired BIT = CASE WHEN @Amount > @ApprovalThreshold THEN 1 ELSE 0 END;

        -- Determine 1099 requirement
        DECLARE @Form1099Required BIT = 0;
        IF @TaxReportable = 1 AND @PayeeType IN ('VENDOR', 'ATTORNEY') AND @Amount >= 600
            SET @Form1099Required = 1;

        -- Insert payment
        INSERT INTO Claims.Payments (
            ClaimID, PaymentNumber, PaymentType, PaymentMethod,
            PayeeType, PayeeName, PayeeAddress, Amount,
            CoverageCode, InvoiceNumber, Description,
            TaxReportable, Form1099Required,
            Status, ApprovalRequired,
            CreatedDate, CreatedBy, ModifiedDate
        ) VALUES (
            @ClaimID, @PaymentNumber, @PaymentType, @PaymentMethod,
            @PayeeType, @PayeeName, @PayeeAddress, @Amount,
            @CoverageCode, @InvoiceNumber, @Description,
            @TaxReportable, @Form1099Required,
            CASE WHEN @ApprovalRequired = 1 THEN 'PENDING' ELSE 'APPROVED' END,
            @ApprovalRequired,
            GETDATE(), @CreatedBy, GETDATE()
        );

        SET @PaymentID = SCOPE_IDENTITY();

        -- Update claim totals (only if auto-approved)
        IF @ApprovalRequired = 0
        BEGIN
            UPDATE Claims.Claims SET
                TotalPaid = TotalPaid + @Amount,
                NetIncurred = TotalReserve + (TotalPaid + @Amount) - TotalRecovery,
                ModifiedDate = GETDATE(),
                ModifiedBy = @CreatedBy
            WHERE ClaimID = @ClaimID;
        END

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'PAYMENT', GETDATE(),
                'Payment created: ' + @PaymentNumber,
                @PaymentType + ' payment of $' + CAST(@Amount AS VARCHAR(50)) + ' to ' + @PayeeName
                + CASE WHEN @ApprovalRequired = 1 THEN ' (PENDING APPROVAL)' ELSE ' (AUTO-APPROVED)' END,
                @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Payments', @PaymentID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)) + ', Amount=' + CAST(@Amount AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Approve Claim Payment
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_ApprovePayment
    @PaymentID INT,
    @ApprovedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ClaimID INT, @Amount DECIMAL(18,2), @Status VARCHAR(20);
        SELECT @ClaimID = ClaimID, @Amount = Amount, @Status = Status
        FROM Claims.Payments WHERE PaymentID = @PaymentID;

        IF @ClaimID IS NULL
        BEGIN
            RAISERROR('Payment not found: %d', 16, 1, @PaymentID);
            RETURN;
        END

        IF @Status <> 'PENDING'
        BEGIN
            RAISERROR('Payment is not in PENDING status. Current: %s', 16, 1, @Status);
            RETURN;
        END

        -- Approve payment
        UPDATE Claims.Payments SET
            Status = 'APPROVED',
            ApprovedBy = @ApprovedBy,
            ApprovedDate = GETDATE(),
            ModifiedDate = GETDATE()
        WHERE PaymentID = @PaymentID;

        -- Update claim totals
        UPDATE Claims.Claims SET
            TotalPaid = TotalPaid + @Amount,
            NetIncurred = TotalReserve + (TotalPaid + @Amount) - TotalRecovery,
            ModifiedDate = GETDATE(),
            ModifiedBy = @ApprovedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'PAYMENT', GETDATE(), 'Payment approved',
                'Payment #' + CAST(@PaymentID AS VARCHAR(50)) + ' for $' + CAST(@Amount AS VARCHAR(50)) + ' approved',
                @ApprovedBy);

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
-- SP: Void Claim Payment
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_VoidPayment
    @PaymentID INT,
    @VoidReason VARCHAR(200),
    @VoidedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @ClaimID INT, @Amount DECIMAL(18,2), @Status VARCHAR(20);
        SELECT @ClaimID = ClaimID, @Amount = Amount, @Status = Status
        FROM Claims.Payments WHERE PaymentID = @PaymentID;

        IF @ClaimID IS NULL
        BEGIN
            RAISERROR('Payment not found: %d', 16, 1, @PaymentID);
            RETURN;
        END

        IF @Status NOT IN ('APPROVED', 'ISSUED')
        BEGIN
            RAISERROR('Only APPROVED or ISSUED payments can be voided. Current: %s', 16, 1, @Status);
            RETURN;
        END

        -- Void payment
        UPDATE Claims.Payments SET
            Status = 'VOIDED',
            VoidedDate = GETDATE(),
            VoidReason = @VoidReason,
            ModifiedDate = GETDATE()
        WHERE PaymentID = @PaymentID;

        -- Reverse claim totals
        UPDATE Claims.Claims SET
            TotalPaid = TotalPaid - @Amount,
            NetIncurred = TotalReserve + (TotalPaid - @Amount) - TotalRecovery,
            ModifiedDate = GETDATE(),
            ModifiedBy = @VoidedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'PAYMENT', GETDATE(), 'Payment voided',
                'Payment #' + CAST(@PaymentID AS VARCHAR(50)) + ' for $' + CAST(@Amount AS VARCHAR(50)) + ' voided. Reason: ' + @VoidReason,
                @VoidedBy);

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
-- SP: Create Claim Activity / Diary Entry
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_CreateActivity
    @ClaimID INT,
    @ActivityType VARCHAR(30),
    @Subject VARCHAR(200),
    @Description VARCHAR(MAX) = NULL,
    @DueDate DATETIME = NULL,
    @ContactName VARCHAR(200) = NULL,
    @ContactPhone VARCHAR(20) = NULL,
    @Duration INT = NULL,
    @AssignedTo VARCHAR(50) = NULL,
    @Priority VARCHAR(10) = 'NORMAL',
    @ReminderDate DATETIME = NULL,
    @CreatedBy VARCHAR(50),
    @ActivityID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Validate claim exists
        IF NOT EXISTS (SELECT 1 FROM Claims.Claims WHERE ClaimID = @ClaimID)
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        INSERT INTO Claims.Activities (
            ClaimID, ActivityType, ActivityDate, DueDate,
            Subject, Description, ContactName, ContactPhone,
            Duration, IsCompleted, AssignedTo, Priority, ReminderDate,
            CreatedDate, CreatedBy
        ) VALUES (
            @ClaimID, @ActivityType, GETDATE(), @DueDate,
            @Subject, @Description, @ContactName, @ContactPhone,
            @Duration, 0, @AssignedTo, @Priority, @ReminderDate,
            GETDATE(), @CreatedBy
        );

        SET @ActivityID = SCOPE_IDENTITY();

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Complete Activity
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_CompleteActivity
    @ActivityID INT,
    @CompletedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Claims.Activities SET
        IsCompleted = 1,
        CompletedDate = GETDATE()
    WHERE ActivityID = @ActivityID AND IsCompleted = 0;

    IF @@ROWCOUNT = 0
        RAISERROR('Activity not found or already completed: %d', 16, 1, @ActivityID);
END
GO

-- ============================================================
-- SP: Assign Claim (adjuster/vendor/examiner)
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_Assign
    @ClaimID INT,
    @AssigneeType VARCHAR(20),
    @AssigneeID INT,
    @DueDate DATETIME = NULL,
    @Instructions VARCHAR(MAX) = NULL,
    @CreatedBy VARCHAR(50),
    @AssignmentID INT OUTPUT
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

        -- Cancel any existing active assignment of same type
        UPDATE Claims.Assignments SET
            Status = 'REASSIGNED',
            ReassignedFrom = @AssigneeID,
            ReassignReason = 'Reassigned to new ' + @AssigneeType
        WHERE ClaimID = @ClaimID AND AssigneeType = @AssigneeType 
            AND Status IN ('ASSIGNED', 'IN_PROGRESS');

        -- Create new assignment
        INSERT INTO Claims.Assignments (
            ClaimID, AssigneeType, AssigneeID, AssignmentDate,
            DueDate, Status, Instructions, CreatedDate, CreatedBy
        ) VALUES (
            @ClaimID, @AssigneeType, @AssigneeID, GETDATE(),
            @DueDate, 'ASSIGNED', @Instructions, GETDATE(), @CreatedBy
        );

        SET @AssignmentID = SCOPE_IDENTITY();

        -- Update claim with adjuster if type is ADJUSTER
        IF @AssigneeType = 'ADJUSTER'
        BEGIN
            UPDATE Claims.Claims SET
                AdjusterID = @AssigneeID,
                AssignedDate = GETDATE(),
                ClaimStatus = CASE WHEN @ClaimStatus = 'FNOL' THEN 'ASSIGNED' ELSE ClaimStatus END,
                ModifiedDate = GETDATE(),
                ModifiedBy = @CreatedBy
            WHERE ClaimID = @ClaimID;

            -- Log status change if moved from FNOL
            IF @ClaimStatus = 'FNOL'
            BEGIN
                INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
                VALUES (@ClaimID, 'FNOL', 'ASSIGNED', GETDATE(), @CreatedBy, 'Adjuster assigned');
            END
        END

        -- Update vendor assignment count
        IF @AssigneeType = 'VENDOR'
        BEGIN
            UPDATE Claims.Vendors SET TotalAssignments = TotalAssignments + 1
            WHERE VendorID = @AssigneeID;
        END

        -- Log activity
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(),
                @AssigneeType + ' assigned',
                @AssigneeType + ' ID ' + CAST(@AssigneeID AS VARCHAR(50)) + ' assigned to claim',
                @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Claims.Assignments', @AssignmentID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)) + ', AssigneeType=' + @AssigneeType);
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get Claim Details (full view with related data)
-- Returns multiple result sets for tabbed display
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_GetDetails
    @ClaimID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Claim header with policy/customer/property info
    SELECT 
        cl.*,
        p.PolicyNumber, p.PolicyType, p.PolicyStatus, p.EffectiveDate AS PolicyEffective, p.ExpiryDate AS PolicyExpiry,
        c.CustomerNumber, c.FirstName, c.LastName, c.CompanyName, c.Phone AS CustomerPhone, c.Email AS CustomerEmail,
        pr.PropertyNumber, pr.AddressLine1 AS PropertyAddress, pr.City AS PropertyCity, 
        pr.StateCode AS PropertyState, pr.ZipCode AS PropertyZip,
        cat.CatastropheName, cat.CatastropheNumber
    FROM Claims.Claims cl
    INNER JOIN Policy.Policies p ON cl.PolicyID = p.PolicyID
    INNER JOIN Policy.Customers c ON cl.CustomerID = c.CustomerID
    INNER JOIN Policy.Properties pr ON cl.PropertyID = pr.PropertyID
    LEFT JOIN Claims.Catastrophes cat ON cl.CatastropheID = cat.CatastropheID
    WHERE cl.ClaimID = @ClaimID;

    -- Result Set 2: Claim coverages
    SELECT cc.*, cov.CoverageName, cov.LimitAmount AS PolicyCoverageLimit
    FROM Claims.ClaimCoverages cc
    INNER JOIN Policy.Coverages cov ON cc.CoverageID = cov.CoverageID
    WHERE cc.ClaimID = @ClaimID;

    -- Result Set 3: Reserves
    SELECT r.*
    FROM Claims.Reserves r
    WHERE r.ClaimID = @ClaimID
    ORDER BY r.CreatedDate DESC;

    -- Result Set 4: Payments
    SELECT pay.*
    FROM Claims.Payments pay
    WHERE pay.ClaimID = @ClaimID
    ORDER BY pay.CreatedDate DESC;

    -- Result Set 5: Activities (recent 50)
    SELECT TOP 50 a.*
    FROM Claims.Activities a
    WHERE a.ClaimID = @ClaimID
    ORDER BY a.ActivityDate DESC;

    -- Result Set 6: Status history
    SELECT sh.*
    FROM Claims.StatusHistory sh
    WHERE sh.ClaimID = @ClaimID
    ORDER BY sh.ChangeDate DESC;

    -- Result Set 7: Assignments
    SELECT asn.*, v.VendorName, v.VendorType, v.Phone AS VendorPhone
    FROM Claims.Assignments asn
    LEFT JOIN Claims.Vendors v ON asn.AssigneeType = 'VENDOR' AND asn.AssigneeID = v.VendorID
    WHERE asn.ClaimID = @ClaimID
    ORDER BY asn.AssignmentDate DESC;
END
GO

-- ============================================================
-- SP: Search Claims (dynamic with pagination)
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_Search
    @ClaimNumber VARCHAR(20) = NULL,
    @PolicyNumber VARCHAR(20) = NULL,
    @CustomerName VARCHAR(200) = NULL,
    @ClaimStatus VARCHAR(20) = NULL,
    @ClaimType VARCHAR(30) = NULL,
    @LossDateFrom DATE = NULL,
    @LossDateTo DATE = NULL,
    @AdjusterID INT = NULL,
    @CatastropheID INT = NULL,
    @Priority VARCHAR(10) = NULL,
    @MinAmount DECIMAL(18,2) = NULL,
    @MaxAmount DECIMAL(18,2) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @SortColumn VARCHAR(50) = 'ReportedDate',
    @SortDirection VARCHAR(4) = 'DESC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @CountSQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = ' WHERE 1=1 ';
    DECLARE @Params NVARCHAR(MAX);

    IF @ClaimNumber IS NOT NULL
        SET @WhereClause += ' AND cl.ClaimNumber = @ClaimNumber ';
    IF @PolicyNumber IS NOT NULL
        SET @WhereClause += ' AND p.PolicyNumber = @PolicyNumber ';
    IF @CustomerName IS NOT NULL
        SET @WhereClause += ' AND (c.FirstName + '' '' + c.LastName LIKE ''%'' + @CustomerName + ''%'' OR c.CompanyName LIKE ''%'' + @CustomerName + ''%'') ';
    IF @ClaimStatus IS NOT NULL
        SET @WhereClause += ' AND cl.ClaimStatus = @ClaimStatus ';
    IF @ClaimType IS NOT NULL
        SET @WhereClause += ' AND cl.ClaimType = @ClaimType ';
    IF @LossDateFrom IS NOT NULL
        SET @WhereClause += ' AND CAST(cl.LossDate AS DATE) >= @LossDateFrom ';
    IF @LossDateTo IS NOT NULL
        SET @WhereClause += ' AND CAST(cl.LossDate AS DATE) <= @LossDateTo ';
    IF @AdjusterID IS NOT NULL
        SET @WhereClause += ' AND cl.AdjusterID = @AdjusterID ';
    IF @CatastropheID IS NOT NULL
        SET @WhereClause += ' AND cl.CatastropheID = @CatastropheID ';
    IF @Priority IS NOT NULL
        SET @WhereClause += ' AND cl.Priority = @Priority ';
    IF @MinAmount IS NOT NULL
        SET @WhereClause += ' AND cl.NetIncurred >= @MinAmount ';
    IF @MaxAmount IS NOT NULL
        SET @WhereClause += ' AND cl.NetIncurred <= @MaxAmount ';

    -- Count
    SET @CountSQL = '
        SELECT @TotalRecords = COUNT(*) 
        FROM Claims.Claims cl
        INNER JOIN Policy.Policies p ON cl.PolicyID = p.PolicyID
        INNER JOIN Policy.Customers c ON cl.CustomerID = c.CustomerID '
        + @WhereClause;

    SET @Params = '@ClaimNumber VARCHAR(20), @PolicyNumber VARCHAR(20), @CustomerName VARCHAR(200), @ClaimStatus VARCHAR(20), @ClaimType VARCHAR(30), @LossDateFrom DATE, @LossDateTo DATE, @AdjusterID INT, @CatastropheID INT, @Priority VARCHAR(10), @MinAmount DECIMAL(18,2), @MaxAmount DECIMAL(18,2), @TotalRecords INT OUTPUT';

    EXEC sp_executesql @CountSQL, @Params, @ClaimNumber, @PolicyNumber, @CustomerName, @ClaimStatus, @ClaimType, @LossDateFrom, @LossDateTo, @AdjusterID, @CatastropheID, @Priority, @MinAmount, @MaxAmount, @TotalRecords OUTPUT;

    -- Results
    SET @SQL = '
        SELECT cl.ClaimID, cl.ClaimNumber, cl.ClaimStatus, cl.ClaimType, cl.LossDate, cl.ReportedDate,
               cl.EstimatedLoss, cl.TotalPaid, cl.TotalReserve, cl.NetIncurred, cl.Priority, cl.Complexity,
               p.PolicyNumber, p.PolicyType,
               c.CustomerNumber, c.FirstName, c.LastName, c.CompanyName,
               cl.FraudScore, cl.IsSIUReferred, cl.IsLitigation
        FROM Claims.Claims cl
        INNER JOIN Policy.Policies p ON cl.PolicyID = p.PolicyID
        INNER JOIN Policy.Customers c ON cl.CustomerID = c.CustomerID '
        + @WhereClause + '
        ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + CASE WHEN @SortDirection = 'DESC' THEN 'DESC' ELSE 'ASC' END + '
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY';

    SET @Params = '@ClaimNumber VARCHAR(20), @PolicyNumber VARCHAR(20), @CustomerName VARCHAR(200), @ClaimStatus VARCHAR(20), @ClaimType VARCHAR(30), @LossDateFrom DATE, @LossDateTo DATE, @AdjusterID INT, @CatastropheID INT, @Priority VARCHAR(10), @MinAmount DECIMAL(18,2), @MaxAmount DECIMAL(18,2), @PageNumber INT, @PageSize INT';

    EXEC sp_executesql @SQL, @Params, @ClaimNumber, @PolicyNumber, @CustomerName, @ClaimStatus, @ClaimType, @LossDateFrom, @LossDateTo, @AdjusterID, @CatastropheID, @Priority, @MinAmount, @MaxAmount, @PageNumber, @PageSize;
END
GO

-- ============================================================
-- SP: Verify Claim Coverage
-- Checks which policy coverages apply to this claim type
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_VerifyCoverage
    @ClaimID INT,
    @VerifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @PolicyID INT, @ClaimType VARCHAR(30);
        SELECT @PolicyID = PolicyID, @ClaimType = ClaimType
        FROM Claims.Claims WHERE ClaimID = @ClaimID;

        IF @PolicyID IS NULL
        BEGIN
            RAISERROR('Claim not found: %d', 16, 1, @ClaimID);
            RETURN;
        END

        -- Remove existing coverage links for re-verification
        DELETE FROM Claims.ClaimCoverages WHERE ClaimID = @ClaimID;

        -- Link applicable coverages based on claim type and coverage perils
        INSERT INTO Claims.ClaimCoverages (ClaimID, CoverageID, CoverageCode, LimitApplicable, DeductibleApplicable, Status)
        SELECT @ClaimID, cov.CoverageID, cov.CoverageCode, cov.LimitAmount, cov.DeductibleAmount, 'OPEN'
        FROM Policy.Coverages cov
        INNER JOIN Policy.CoveragePerils cp ON cov.CoverageID = cp.CoverageID
        INNER JOIN Policy.Perils p ON cp.PerilID = p.PerilID
        WHERE cov.PolicyID = @PolicyID 
            AND cov.IsSelected = 1
            AND cp.IsIncluded = 1
            AND p.PerilCode = @ClaimType;

        -- If no peril match, try broader coverage match
        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO Claims.ClaimCoverages (ClaimID, CoverageID, CoverageCode, LimitApplicable, DeductibleApplicable, Status)
            SELECT @ClaimID, cov.CoverageID, cov.CoverageCode, cov.LimitAmount, cov.DeductibleAmount, 'OPEN'
            FROM Policy.Coverages cov
            WHERE cov.PolicyID = @PolicyID 
                AND cov.IsSelected = 1
                AND cov.CoverageCode IN ('DWELLING', 'OTHER_STRUCTURES', 'PERSONAL_PROPERTY', 'LOSS_OF_USE');
        END

        -- Update claim coverage verified flag
        UPDATE Claims.Claims SET
            CoverageVerified = 1,
            CoverageVerifiedDate = GETDATE(),
            CoverageVerifiedBy = @VerifiedBy,
            PolicyLimit = (SELECT ISNULL(SUM(LimitApplicable), 0) FROM Claims.ClaimCoverages WHERE ClaimID = @ClaimID),
            DeductibleAmount = (SELECT ISNULL(MIN(DeductibleApplicable), 0) FROM Claims.ClaimCoverages WHERE ClaimID = @ClaimID),
            ModifiedDate = GETDATE(),
            ModifiedBy = @VerifiedBy
        WHERE ClaimID = @ClaimID;

        -- Log activity
        DECLARE @CoverageCount INT;
        SELECT @CoverageCount = COUNT(*) FROM Claims.ClaimCoverages WHERE ClaimID = @ClaimID;

        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, CreatedBy)
        VALUES (@ClaimID, 'NOTE', GETDATE(), 'Coverage verified',
                CAST(@CoverageCount AS VARCHAR(50)) + ' coverage(s) confirmed applicable to this claim',
                @VerifiedBy);

        COMMIT TRANSACTION;

        -- Return applicable coverages
        SELECT cc.*, cov.CoverageName
        FROM Claims.ClaimCoverages cc
        INNER JOIN Policy.Coverages cov ON cc.CoverageID = cov.CoverageID
        WHERE cc.ClaimID = @ClaimID;

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
-- SP: Get Claims Dashboard Summary
-- Returns aggregate stats for the claims dashboard
-- ============================================================
CREATE OR ALTER PROCEDURE Claims.usp_Claim_GetDashboard
    @AdjusterID INT = NULL,
    @AsOfDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @AsOfDate IS NULL SET @AsOfDate = CAST(GETDATE() AS DATE);

    -- Open claims by status
    SELECT ClaimStatus, COUNT(*) AS ClaimCount, 
           SUM(TotalReserve) AS TotalReserve, SUM(NetIncurred) AS TotalIncurred
    FROM Claims.Claims
    WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED')
        AND (@AdjusterID IS NULL OR AdjusterID = @AdjusterID)
    GROUP BY ClaimStatus;

    -- Claims opened in last 30 days
    SELECT COUNT(*) AS NewClaimsLast30Days,
           SUM(EstimatedLoss) AS EstimatedLossLast30Days
    FROM Claims.Claims
    WHERE ReportedDate >= DATEADD(DAY, -30, @AsOfDate)
        AND (@AdjusterID IS NULL OR AdjusterID = @AdjusterID);

    -- Overdue activities
    SELECT COUNT(*) AS OverdueActivities
    FROM Claims.Activities a
    INNER JOIN Claims.Claims cl ON a.ClaimID = cl.ClaimID
    WHERE a.IsCompleted = 0 AND a.DueDate < @AsOfDate
        AND cl.ClaimStatus NOT IN ('CLOSED', 'DENIED')
        AND (@AdjusterID IS NULL OR cl.AdjusterID = @AdjusterID);

    -- Payments pending approval
    SELECT COUNT(*) AS PendingPayments, SUM(Amount) AS PendingPaymentTotal
    FROM Claims.Payments pay
    INNER JOIN Claims.Claims cl ON pay.ClaimID = cl.ClaimID
    WHERE pay.Status = 'PENDING'
        AND (@AdjusterID IS NULL OR cl.AdjusterID = @AdjusterID);

    -- High priority claims
    SELECT cl.ClaimID, cl.ClaimNumber, cl.ClaimType, cl.Priority, cl.NetIncurred, cl.ClaimStatus,
           c.FirstName + ' ' + c.LastName AS CustomerName
    FROM Claims.Claims cl
    INNER JOIN Policy.Customers c ON cl.CustomerID = c.CustomerID
    WHERE cl.Priority IN ('HIGH', 'CRITICAL')
        AND cl.ClaimStatus NOT IN ('CLOSED', 'DENIED')
        AND (@AdjusterID IS NULL OR cl.AdjusterID = @AdjusterID)
    ORDER BY CASE cl.Priority WHEN 'CRITICAL' THEN 1 WHEN 'HIGH' THEN 2 END, cl.ReportedDate;
END
GO
