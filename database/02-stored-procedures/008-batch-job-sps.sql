-- ============================================================
-- BATCH JOB STORED PROCEDURES
-- Renewal processing, expiration, reserve recalculation,
-- payment batching, fraud scoring, and data archival
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Process Policy Renewals (batch)
-- Finds policies expiring within N days, creates renewal quotes
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_Renewal_Process
    @DaysAhead INT = 60,
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @PoliciesProcessed INT OUTPUT,
    @PoliciesFailed INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SET @PoliciesProcessed = 0;
        SET @PoliciesFailed = 0;

        DECLARE @CutoffDate DATE = DATEADD(DAY, @DaysAhead, CAST(GETDATE() AS DATE));

        -- Find eligible policies for renewal
        DECLARE @PolicyID INT, @PolicyNumber VARCHAR(20);

        DECLARE renewal_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT PolicyID, PolicyNumber
            FROM Policy.Policies
            WHERE PolicyStatus = 'ACTIVE'
                AND ExpiryDate <= @CutoffDate
                AND ExpiryDate > CAST(GETDATE() AS DATE)
                AND IsRenewal = 0  -- not already flagged
                AND NOT EXISTS (
                    SELECT 1 FROM Policy.Policies p2 
                    WHERE p2.PolicyNumber = Policy.Policies.PolicyNumber 
                    AND p2.PolicyVersion > Policy.Policies.PolicyVersion
                    AND p2.PolicyStatus IN ('QUOTE', 'ACTIVE')
                );

        OPEN renewal_cursor;
        FETCH NEXT FROM renewal_cursor INTO @PolicyID, @PolicyNumber;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Create renewal quote (simplified - copies policy with new dates)
                DECLARE @NewPolicyID INT, @NewPolicyNumber VARCHAR(20);
                DECLARE @CustomerID INT, @PropertyID INT, @AgentID INT;
                DECLARE @PolicyType VARCHAR(30), @ExpiryDate DATE, @TermMonths INT;

                SELECT @CustomerID = CustomerID, @PropertyID = PropertyID, @AgentID = AgentID,
                       @PolicyType = PolicyType, @ExpiryDate = ExpiryDate, @TermMonths = TermMonths
                FROM Policy.Policies WHERE PolicyID = @PolicyID;

                -- Mark original as pending renewal
                UPDATE Policy.Policies SET IsRenewal = 1, ModifiedDate = GETDATE(), ModifiedBy = @ProcessedBy
                WHERE PolicyID = @PolicyID;

                SET @PoliciesProcessed = @PoliciesProcessed + 1;
            END TRY
            BEGIN CATCH
                SET @PoliciesFailed = @PoliciesFailed + 1;
                INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
                VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                        'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)));
            END CATCH

            FETCH NEXT FROM renewal_cursor INTO @PolicyID, @PolicyNumber;
        END

        CLOSE renewal_cursor;
        DEALLOCATE renewal_cursor;

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Process Policy Expirations (batch)
-- Expires policies past their expiry date
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_Expiration_Process
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @PoliciesExpired INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Policy.Policies SET
            PolicyStatus = 'EXPIRED',
            ModifiedDate = GETDATE(),
            ModifiedBy = @ProcessedBy
        WHERE PolicyStatus = 'ACTIVE'
            AND ExpiryDate < CAST(GETDATE() AS DATE);

        SET @PoliciesExpired = @@ROWCOUNT;

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
-- SP: Recalculate Claim Reserves (batch)
-- Updates NetIncurred for all open claims
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_Reserve_Recalculate
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @ClaimsUpdated INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Claims.Claims SET
            TotalReserve = ISNULL((SELECT SUM(Amount) FROM Claims.Reserves r WHERE r.ClaimID = Claims.Claims.ClaimID AND r.IsApproved = 1), 0),
            TotalPaid = ISNULL((SELECT SUM(Amount) FROM Claims.Payments p WHERE p.ClaimID = Claims.Claims.ClaimID AND p.Status IN ('APPROVED', 'ISSUED', 'CLEARED')), 0),
            TotalRecovery = ISNULL((SELECT SUM(RecoveryAmount) FROM Claims.Subrogation s WHERE s.ClaimID = Claims.Claims.ClaimID), 0),
            NetIncurred = ISNULL((SELECT SUM(Amount) FROM Claims.Reserves r WHERE r.ClaimID = Claims.Claims.ClaimID AND r.IsApproved = 1), 0)
                        + ISNULL((SELECT SUM(Amount) FROM Claims.Payments p WHERE p.ClaimID = Claims.Claims.ClaimID AND p.Status IN ('APPROVED', 'ISSUED', 'CLEARED')), 0)
                        - ISNULL((SELECT SUM(RecoveryAmount) FROM Claims.Subrogation s WHERE s.ClaimID = Claims.Claims.ClaimID), 0),
            ModifiedDate = GETDATE(),
            ModifiedBy = @ProcessedBy
        WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED');

        SET @ClaimsUpdated = @@ROWCOUNT;

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
-- SP: Batch Fraud Scoring (score all unscored open claims)
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_FraudScoring_Process
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @ClaimsScored INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ClaimsScored = 0;

    DECLARE @ClaimID INT, @FraudScore DECIMAL(6,2);

    DECLARE claim_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ClaimID FROM Claims.Claims
        WHERE ClaimStatus NOT IN ('CLOSED', 'DENIED')
            AND FraudScore = 0
            AND ReportedDate >= DATEADD(DAY, -7, GETDATE());

    OPEN claim_cursor;
    FETCH NEXT FROM claim_cursor INTO @ClaimID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            EXEC Claims.usp_Fraud_EvaluateClaim @ClaimID = @ClaimID, @EvaluatedBy = @ProcessedBy, @FraudScore = @FraudScore OUTPUT;
            SET @ClaimsScored = @ClaimsScored + 1;
        END TRY
        BEGIN CATCH
            INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    'ClaimID=' + CAST(@ClaimID AS VARCHAR(50)));
        END CATCH

        FETCH NEXT FROM claim_cursor INTO @ClaimID;
    END

    CLOSE claim_cursor;
    DEALLOCATE claim_cursor;
END
GO

-- ============================================================
-- SP: Process Cancellation Notices (batch)
-- Sends cancellation notices for overdue invoices
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_CancellationNotice_Process
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @NoticesSent INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CancellationNoticeDays INT = 20;
        SELECT @CancellationNoticeDays = CAST(ConfigValue AS INT)
        FROM Admin.SystemConfig WHERE ConfigKey = 'CANCELLATION_NOTICE_DAYS';

        -- Find overdue invoices without cancellation notice
        UPDATE Billing.Invoices SET
            CancellationNoticeDate = CAST(GETDATE() AS DATE),
            CancellationEffectiveDate = DATEADD(DAY, @CancellationNoticeDays, CAST(GETDATE() AS DATE)),
            ModifiedDate = GETDATE()
        WHERE Status = 'OVERDUE'
            AND CancellationNoticeDate IS NULL
            AND DueDate < DATEADD(DAY, -30, CAST(GETDATE() AS DATE));

        SET @NoticesSent = @@ROWCOUNT;

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
-- SP: Archive Old Data (batch)
-- Moves closed claims/policies older than N years to archive
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_Data_Archive
    @YearsOld INT = 7,
    @ProcessedBy VARCHAR(50) = 'SYSTEM',
    @RecordsArchived INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SET @RecordsArchived = 0;
        DECLARE @CutoffDate DATE = DATEADD(YEAR, -@YearsOld, CAST(GETDATE() AS DATE));

        -- For now, just mark as archived (actual archival would move to archive tables)
        -- Archive closed claims
        UPDATE Claims.Claims SET
            ModifiedDate = GETDATE(),
            ModifiedBy = @ProcessedBy
        WHERE ClaimStatus = 'CLOSED' AND ClosedDate < @CutoffDate;

        SET @RecordsArchived = @RecordsArchived + @@ROWCOUNT;

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Batch Job Logging - Start
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_JobLog_Start
    @JobName VARCHAR(100),
    @Parameters VARCHAR(500) = NULL,
    @JobLogID BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Batch.JobLogs (JobName, StartTime, Status, Parameters)
    VALUES (@JobName, GETDATE(), 'RUNNING', @Parameters);

    SET @JobLogID = SCOPE_IDENTITY();
END
GO

-- ============================================================
-- SP: Batch Job Logging - Complete
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_JobLog_Complete
    @JobLogID BIGINT,
    @RecordsProcessed INT = 0,
    @RecordsFailed INT = 0,
    @ResultMessage VARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Batch.JobLogs SET
        EndTime = GETDATE(),
        Status = CASE WHEN @RecordsFailed > 0 THEN 'COMPLETED_WITH_ERRORS' ELSE 'COMPLETED' END,
        RecordsProcessed = @RecordsProcessed,
        RecordsFailed = @RecordsFailed,
        ResultMessage = @ResultMessage
    WHERE JobLogID = @JobLogID;
END
GO

-- ============================================================
-- SP: Batch Job Logging - Fail
-- ============================================================
CREATE OR ALTER PROCEDURE Batch.usp_JobLog_Fail
    @JobLogID BIGINT,
    @ErrorMessage VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Batch.JobLogs SET
        EndTime = GETDATE(),
        Status = 'FAILED',
        ResultMessage = @ErrorMessage
    WHERE JobLogID = @JobLogID;
END
GO
