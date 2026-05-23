-- ============================================================
-- BILLING STORED PROCEDURES
-- Invoice generation, premium payments, refunds, payment
-- plans, late fees, and commission transactions
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Generate Invoice for Policy
-- Creates invoice(s) based on payment plan
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Invoice_Generate
    @PolicyID INT,
    @InvoiceType VARCHAR(20), -- NEW_BUSINESS, RENEWAL, ENDORSEMENT
    @PremiumAmount DECIMAL(18,2),
    @TaxAmount DECIMAL(18,2) = 0,
    @FeeAmount DECIMAL(18,2) = 0,
    @SurchargeAmount DECIMAL(18,2) = 0,
    @CreatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CustomerID INT, @PaymentPlan VARCHAR(20), @EffectiveDate DATE;
        SELECT @CustomerID = CustomerID, @PaymentPlan = PaymentPlan, @EffectiveDate = EffectiveDate
        FROM Policy.Policies WHERE PolicyID = @PolicyID;

        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        DECLARE @TotalAmount DECIMAL(18,2) = @PremiumAmount + @TaxAmount + @FeeAmount + @SurchargeAmount;

        -- Get payment plan details
        DECLARE @NumInstallments INT = 1, @DownPaymentPct DECIMAL(6,4) = 1.0;
        DECLARE @InstallmentFee DECIMAL(10,2) = 0, @GracePeriodDays INT = 30;
        DECLARE @PaymentPlanID INT;

        IF @PaymentPlan <> 'ANNUAL'
        BEGIN
            SELECT TOP 1 @PaymentPlanID = PaymentPlanID, @NumInstallments = NumberOfInstallments,
                   @DownPaymentPct = DownPaymentPercent, @InstallmentFee = InstallmentFee,
                   @GracePeriodDays = GracePeriodDays
            FROM Billing.PaymentPlans
            WHERE PlanCode = @PaymentPlan AND IsActive = 1;
        END

        -- Generate invoices
        DECLARE @InstallmentNum INT = 1;
        DECLARE @DownPayment DECIMAL(18,2) = ROUND(@TotalAmount * @DownPaymentPct, 2);
        DECLARE @RemainingAmount DECIMAL(18,2) = @TotalAmount - @DownPayment;
        DECLARE @InstallmentAmount DECIMAL(18,2);
        DECLARE @InvoiceDate DATE = @EffectiveDate;
        DECLARE @DueDate DATE;
        DECLARE @InvoiceNumber VARCHAR(20);
        DECLARE @Sequence INT;

        -- First invoice (down payment or full amount)
        SELECT @Sequence = ISNULL(MAX(InvoiceID), 0) + 1 FROM Billing.Invoices;
        SET @InvoiceNumber = 'INV' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);
        SET @DueDate = DATEADD(DAY, @GracePeriodDays, @InvoiceDate);

        INSERT INTO Billing.Invoices (
            InvoiceNumber, PolicyID, CustomerID, InvoiceType,
            InvoiceDate, DueDate, PremiumAmount, TaxAmount, FeeAmount, SurchargeAmount,
            TotalAmount, PaidAmount, BalanceDue, Status,
            InstallmentNumber, TotalInstallments, PaymentPlanID,
            CreatedDate, CreatedBy, ModifiedDate
        ) VALUES (
            @InvoiceNumber, @PolicyID, @CustomerID, @InvoiceType,
            @InvoiceDate, @DueDate, 
            CASE WHEN @NumInstallments = 1 THEN @PremiumAmount ELSE ROUND(@PremiumAmount * @DownPaymentPct, 2) END,
            CASE WHEN @NumInstallments = 1 THEN @TaxAmount ELSE ROUND(@TaxAmount * @DownPaymentPct, 2) END,
            @FeeAmount, @SurchargeAmount,
            CASE WHEN @NumInstallments = 1 THEN @TotalAmount ELSE @DownPayment + @FeeAmount END,
            0,
            CASE WHEN @NumInstallments = 1 THEN @TotalAmount ELSE @DownPayment + @FeeAmount END,
            'OPEN', 1, @NumInstallments, @PaymentPlanID,
            GETDATE(), @CreatedBy, GETDATE()
        );

        -- Generate installment invoices if multi-pay
        IF @NumInstallments > 1 AND @RemainingAmount > 0
        BEGIN
            SET @InstallmentAmount = ROUND(@RemainingAmount / (@NumInstallments - 1), 2);
            SET @InstallmentNum = 2;

            WHILE @InstallmentNum <= @NumInstallments
            BEGIN
                SET @InvoiceDate = DATEADD(MONTH, @InstallmentNum - 1, @EffectiveDate);
                SET @DueDate = DATEADD(DAY, @GracePeriodDays, @InvoiceDate);

                SELECT @Sequence = ISNULL(MAX(InvoiceID), 0) + 1 FROM Billing.Invoices;
                SET @InvoiceNumber = 'INV' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

                -- Last installment gets remainder to avoid rounding issues
                IF @InstallmentNum = @NumInstallments
                    SET @InstallmentAmount = @RemainingAmount - (@InstallmentAmount * (@NumInstallments - 2));

                INSERT INTO Billing.Invoices (
                    InvoiceNumber, PolicyID, CustomerID, InvoiceType,
                    InvoiceDate, DueDate, PremiumAmount, TaxAmount, FeeAmount, SurchargeAmount,
                    TotalAmount, PaidAmount, BalanceDue, Status,
                    InstallmentNumber, TotalInstallments, PaymentPlanID,
                    CreatedDate, CreatedBy, ModifiedDate
                ) VALUES (
                    @InvoiceNumber, @PolicyID, @CustomerID, 'INSTALLMENT',
                    @InvoiceDate, @DueDate, @InstallmentAmount, 0, @InstallmentFee, 0,
                    @InstallmentAmount + @InstallmentFee, 0, @InstallmentAmount + @InstallmentFee, 'OPEN',
                    @InstallmentNum, @NumInstallments, @PaymentPlanID,
                    GETDATE(), @CreatedBy, GETDATE()
                );

                SET @InstallmentNum = @InstallmentNum + 1;
            END
        END

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Billing.Invoices', @PolicyID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Record Premium Payment
-- Applies payment to invoice, updates balances
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Payment_Record
    @InvoiceID INT = NULL,
    @PolicyID INT,
    @Amount DECIMAL(18,2),
    @PaymentMethod VARCHAR(20),
    @ReferenceNumber VARCHAR(50) = NULL,
    @CheckNumber VARCHAR(20) = NULL,
    @BankName VARCHAR(100) = NULL,
    @CreditCardLast4 VARCHAR(4) = NULL,
    @PaymentDate DATE = NULL,
    @CreatedBy VARCHAR(50),
    @PremiumPaymentID INT OUTPUT,
    @PaymentNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @PaymentDate IS NULL SET @PaymentDate = CAST(GETDATE() AS DATE);

        -- Validate policy
        DECLARE @CustomerID INT;
        SELECT @CustomerID = CustomerID FROM Policy.Policies WHERE PolicyID = @PolicyID;
        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        -- If no invoice specified, find oldest open invoice
        IF @InvoiceID IS NULL
        BEGIN
            SELECT TOP 1 @InvoiceID = InvoiceID
            FROM Billing.Invoices
            WHERE PolicyID = @PolicyID AND Status IN ('OPEN', 'OVERDUE', 'PARTIAL')
            ORDER BY DueDate ASC;
        END

        -- Generate payment number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(PremiumPaymentID), 0) + 1 FROM Billing.PremiumPayments;
        SET @PaymentNumber = 'PMP' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Generate receipt number
        DECLARE @ReceiptNumber VARCHAR(20);
        SET @ReceiptNumber = 'RCP' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Insert payment
        INSERT INTO Billing.PremiumPayments (
            PaymentNumber, InvoiceID, PolicyID, CustomerID,
            PaymentDate, Amount, PaymentMethod,
            ReferenceNumber, CheckNumber, BankName, CreditCardLast4,
            Status, AppliedToInvoice, ReceiptNumber,
            CreatedDate, CreatedBy
        ) VALUES (
            @PaymentNumber, @InvoiceID, @PolicyID, @CustomerID,
            @PaymentDate, @Amount, @PaymentMethod,
            @ReferenceNumber, @CheckNumber, @BankName, @CreditCardLast4,
            'APPLIED', CASE WHEN @InvoiceID IS NOT NULL THEN 1 ELSE 0 END, @ReceiptNumber,
            GETDATE(), @CreatedBy
        );

        SET @PremiumPaymentID = SCOPE_IDENTITY();

        -- Apply to invoice
        IF @InvoiceID IS NOT NULL
        BEGIN
            DECLARE @InvoiceBalance DECIMAL(18,2), @RemainingPayment DECIMAL(18,2);
            SELECT @InvoiceBalance = BalanceDue FROM Billing.Invoices WHERE InvoiceID = @InvoiceID;

            SET @RemainingPayment = @Amount;

            -- Apply to current invoice
            IF @RemainingPayment >= @InvoiceBalance
            BEGIN
                UPDATE Billing.Invoices SET
                    PaidAmount = TotalAmount,
                    BalanceDue = 0,
                    Status = 'PAID',
                    ModifiedDate = GETDATE()
                WHERE InvoiceID = @InvoiceID;

                SET @RemainingPayment = @RemainingPayment - @InvoiceBalance;
            END
            ELSE
            BEGIN
                UPDATE Billing.Invoices SET
                    PaidAmount = PaidAmount + @RemainingPayment,
                    BalanceDue = BalanceDue - @RemainingPayment,
                    Status = 'PARTIAL',
                    ModifiedDate = GETDATE()
                WHERE InvoiceID = @InvoiceID;

                SET @RemainingPayment = 0;
            END

            -- Apply overpayment to next invoice if any
            IF @RemainingPayment > 0
            BEGIN
                DECLARE @NextInvoiceID INT;
                SELECT TOP 1 @NextInvoiceID = InvoiceID
                FROM Billing.Invoices
                WHERE PolicyID = @PolicyID AND InvoiceID > @InvoiceID 
                    AND Status IN ('OPEN', 'OVERDUE')
                ORDER BY DueDate ASC;

                IF @NextInvoiceID IS NOT NULL
                BEGIN
                    UPDATE Billing.Invoices SET
                        PaidAmount = PaidAmount + @RemainingPayment,
                        BalanceDue = BalanceDue - @RemainingPayment,
                        Status = CASE WHEN BalanceDue - @RemainingPayment <= 0 THEN 'PAID' ELSE 'PARTIAL' END,
                        ModifiedDate = GETDATE()
                    WHERE InvoiceID = @NextInvoiceID;
                END
            END
        END

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Billing.PremiumPayments', @PremiumPaymentID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, AdditionalInfo)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                'PolicyID=' + CAST(@PolicyID AS VARCHAR(50)) + ', Amount=' + CAST(@Amount AS VARCHAR(50)));
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Process Returned Payment (NSF/bounced check)
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Payment_Return
    @PremiumPaymentID INT,
    @ReturnReason VARCHAR(200),
    @NSFFee DECIMAL(10,2) = 25.00,
    @ProcessedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @InvoiceID INT, @Amount DECIMAL(18,2), @PolicyID INT, @Status VARCHAR(20);
        SELECT @InvoiceID = InvoiceID, @Amount = Amount, @PolicyID = PolicyID, @Status = Status
        FROM Billing.PremiumPayments WHERE PremiumPaymentID = @PremiumPaymentID;

        IF @PolicyID IS NULL
        BEGIN
            RAISERROR('Payment not found: %d', 16, 1, @PremiumPaymentID);
            RETURN;
        END

        IF @Status <> 'APPLIED'
        BEGIN
            RAISERROR('Payment is not in APPLIED status: %s', 16, 1, @Status);
            RETURN;
        END

        -- Mark payment as returned
        UPDATE Billing.PremiumPayments SET
            Status = 'RETURNED',
            ReturnedDate = CAST(GETDATE() AS DATE),
            ReturnReason = @ReturnReason,
            NSFFee = @NSFFee
        WHERE PremiumPaymentID = @PremiumPaymentID;

        -- Reverse invoice application
        IF @InvoiceID IS NOT NULL
        BEGIN
            UPDATE Billing.Invoices SET
                PaidAmount = PaidAmount - @Amount,
                BalanceDue = BalanceDue + @Amount + @NSFFee,
                TotalAmount = TotalAmount + @NSFFee,
                Status = 'OVERDUE',
                ModifiedDate = GETDATE()
            WHERE InvoiceID = @InvoiceID;
        END

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
        VALUES ('Billing.PremiumPayments', @PremiumPaymentID, 'UPDATE', 'Status', 'APPLIED', 'RETURNED', @ProcessedBy, GETDATE());

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
-- SP: Create Refund
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Refund_Create
    @PolicyID INT,
    @RefundType VARCHAR(20),
    @RefundMethod VARCHAR(20) = 'CHECK',
    @Amount DECIMAL(18,2),
    @CalculationMethod VARCHAR(20) = 'PRO_RATA',
    @ProRataFactor DECIMAL(10,8) = NULL,
    @EarnedPremium DECIMAL(18,2) = NULL,
    @CreatedBy VARCHAR(50),
    @RefundID INT OUTPUT,
    @RefundNumber VARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CustomerID INT;
        SELECT @CustomerID = CustomerID FROM Policy.Policies WHERE PolicyID = @PolicyID;

        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        -- Generate refund number
        DECLARE @Sequence INT;
        SELECT @Sequence = ISNULL(MAX(RefundID), 0) + 1 FROM Billing.Refunds;
        SET @RefundNumber = 'RFD' + RIGHT('0000000' + CAST(@Sequence AS VARCHAR(50)), 7);

        -- Check approval threshold
        DECLARE @ApprovalThreshold DECIMAL(18,2) = 5000;
        SELECT @ApprovalThreshold = CAST(ConfigValue AS DECIMAL(18,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'REFUND_APPROVAL_THRESHOLD';

        INSERT INTO Billing.Refunds (
            RefundNumber, PolicyID, CustomerID, RefundType, RefundMethod,
            Amount, CalculationMethod, ProRataFactor, EarnedPremium, ReturnPremium,
            Status, CreatedDate, CreatedBy
        ) VALUES (
            @RefundNumber, @PolicyID, @CustomerID, @RefundType, @RefundMethod,
            @Amount, @CalculationMethod, @ProRataFactor, @EarnedPremium, @Amount,
            CASE WHEN @Amount > @ApprovalThreshold THEN 'PENDING' ELSE 'APPROVED' END,
            GETDATE(), @CreatedBy
        );

        SET @RefundID = SCOPE_IDENTITY();

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Billing.Refunds', @RefundID, 'INSERT', @CreatedBy, GETDATE());

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
-- SP: Approve and Issue Refund
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Refund_Approve
    @RefundID INT,
    @ApprovedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Status VARCHAR(20);
    SELECT @Status = Status FROM Billing.Refunds WHERE RefundID = @RefundID;

    IF @Status IS NULL
    BEGIN
        RAISERROR('Refund not found: %d', 16, 1, @RefundID);
        RETURN;
    END

    IF @Status <> 'PENDING'
    BEGIN
        RAISERROR('Refund is not in PENDING status: %s', 16, 1, @Status);
        RETURN;
    END

    UPDATE Billing.Refunds SET
        Status = 'APPROVED',
        ApprovedBy = @ApprovedBy,
        ApprovedDate = GETDATE()
    WHERE RefundID = @RefundID;
END
GO

-- ============================================================
-- SP: Apply Late Fees to Overdue Invoices (batch)
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Invoice_ApplyLateFees
    @AsOfDate DATE = NULL,
    @ProcessedBy VARCHAR(50),
    @InvoicesProcessed INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @AsOfDate IS NULL SET @AsOfDate = CAST(GETDATE() AS DATE);

        -- Get late fee amount from payment plan or default
        DECLARE @DefaultLateFee DECIMAL(10,2) = 15.00;
        SELECT @DefaultLateFee = CAST(ConfigValue AS DECIMAL(10,2))
        FROM Admin.SystemConfig WHERE ConfigKey = 'DEFAULT_LATE_FEE';

        -- Find overdue invoices that haven't had late fee applied
        UPDATE Billing.Invoices SET
            Status = 'OVERDUE',
            LateFeeApplied = 1,
            LateFeeAmount = ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
            TotalAmount = TotalAmount + ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
            BalanceDue = BalanceDue + ISNULL((SELECT LateFeeAmount FROM Billing.PaymentPlans WHERE PaymentPlanID = Billing.Invoices.PaymentPlanID), @DefaultLateFee),
            ModifiedDate = GETDATE()
        WHERE Status = 'OPEN' AND DueDate < @AsOfDate AND LateFeeApplied = 0 AND BalanceDue > 0;

        SET @InvoicesProcessed = @@ROWCOUNT;

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
-- SP: Create Commission Transaction
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Commission_Create
    @PolicyID INT,
    @TransactionType VARCHAR(20), -- EARNED, REVERSAL, OVERRIDE, BONUS, CHARGEBACK
    @PremiumAmount DECIMAL(18,2),
    @CommissionRate DECIMAL(6,4),
    @Description VARCHAR(200) = NULL,
    @CreatedBy VARCHAR(50),
    @CommissionTransactionID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @AgentID INT;
        SELECT @AgentID = AgentID FROM Policy.Policies WHERE PolicyID = @PolicyID;

        IF @AgentID IS NULL
        BEGIN
            RAISERROR('Policy not found: %d', 16, 1, @PolicyID);
            RETURN;
        END

        DECLARE @CommissionAmount DECIMAL(18,2) = ROUND(@PremiumAmount * @CommissionRate, 2);

        -- Reversals and chargebacks are negative
        IF @TransactionType IN ('REVERSAL', 'CHARGEBACK')
            SET @CommissionAmount = -ABS(@CommissionAmount);

        INSERT INTO Billing.CommissionTransactions (
            PolicyID, AgentID, TransactionType, TransactionDate,
            PremiumAmount, CommissionRate, CommissionAmount,
            Status, Description, CreatedDate
        ) VALUES (
            @PolicyID, @AgentID, @TransactionType, CAST(GETDATE() AS DATE),
            @PremiumAmount, @CommissionRate, @CommissionAmount,
            'PENDING', @Description, GETDATE()
        );

        SET @CommissionTransactionID = SCOPE_IDENTITY();

    END TRY
    BEGIN CATCH
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get Agent Commission Statement
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Commission_GetStatement
    @AgentID INT,
    @PeriodFrom DATE = NULL,
    @PeriodTo DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PeriodFrom IS NULL SET @PeriodFrom = DATEADD(MONTH, -1, CAST(GETDATE() AS DATE));
    IF @PeriodTo IS NULL SET @PeriodTo = CAST(GETDATE() AS DATE);

    -- Summary
    SELECT 
        @AgentID AS AgentID,
        a.AgentNumber, a.FirstName + ' ' + a.LastName AS AgentName,
        ag.AgencyName,
        SUM(CASE WHEN ct.TransactionType = 'EARNED' THEN ct.CommissionAmount ELSE 0 END) AS TotalEarned,
        SUM(CASE WHEN ct.TransactionType IN ('REVERSAL', 'CHARGEBACK') THEN ct.CommissionAmount ELSE 0 END) AS TotalReversals,
        SUM(CASE WHEN ct.TransactionType = 'OVERRIDE' THEN ct.CommissionAmount ELSE 0 END) AS TotalOverrides,
        SUM(CASE WHEN ct.TransactionType = 'BONUS' THEN ct.CommissionAmount ELSE 0 END) AS TotalBonus,
        SUM(ct.CommissionAmount) AS NetCommission
    FROM Billing.CommissionTransactions ct
    INNER JOIN Policy.Agents a ON ct.AgentID = a.AgentID
    LEFT JOIN Policy.Agencies ag ON a.AgencyID = ag.AgencyID
    WHERE ct.AgentID = @AgentID
        AND ct.TransactionDate BETWEEN @PeriodFrom AND @PeriodTo
    GROUP BY a.AgentNumber, a.FirstName, a.LastName, ag.AgencyName;

    -- Detail
    SELECT ct.*, p.PolicyNumber, p.PolicyType,
           c.FirstName + ' ' + c.LastName AS CustomerName
    FROM Billing.CommissionTransactions ct
    INNER JOIN Policy.Policies p ON ct.PolicyID = p.PolicyID
    INNER JOIN Policy.Customers c ON p.CustomerID = c.CustomerID
    WHERE ct.AgentID = @AgentID
        AND ct.TransactionDate BETWEEN @PeriodFrom AND @PeriodTo
    ORDER BY ct.TransactionDate DESC;
END
GO

-- ============================================================
-- SP: Get Billing Inquiry (policy billing history)
-- ============================================================
CREATE OR ALTER PROCEDURE Billing.usp_Billing_GetByPolicy
    @PolicyID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Policy billing summary
    SELECT 
        p.PolicyID, p.PolicyNumber, p.GrossPremium, p.PaymentPlan,
        (SELECT ISNULL(SUM(TotalAmount), 0) FROM Billing.Invoices WHERE PolicyID = @PolicyID AND Status <> 'CANCELLED') AS TotalBilled,
        (SELECT ISNULL(SUM(PaidAmount), 0) FROM Billing.Invoices WHERE PolicyID = @PolicyID) AS TotalPaid,
        (SELECT ISNULL(SUM(BalanceDue), 0) FROM Billing.Invoices WHERE PolicyID = @PolicyID AND Status IN ('OPEN', 'OVERDUE', 'PARTIAL')) AS TotalOutstanding
    FROM Policy.Policies p WHERE p.PolicyID = @PolicyID;

    -- Invoices
    SELECT i.*
    FROM Billing.Invoices i
    WHERE i.PolicyID = @PolicyID
    ORDER BY i.InvoiceDate;

    -- Payments
    SELECT pp.*
    FROM Billing.PremiumPayments pp
    WHERE pp.PolicyID = @PolicyID
    ORDER BY pp.PaymentDate DESC;

    -- Refunds
    SELECT r.*
    FROM Billing.Refunds r
    WHERE r.PolicyID = @PolicyID
    ORDER BY r.CreatedDate DESC;
END
GO
