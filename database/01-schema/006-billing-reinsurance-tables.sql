-- ============================================================
-- BILLING & PAYMENTS TABLES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- Invoices
CREATE TABLE Billing.Invoices (
    InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber VARCHAR(20) NOT NULL UNIQUE,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    InvoiceType VARCHAR(20) NOT NULL, -- NEW_BUSINESS, RENEWAL, ENDORSEMENT, INSTALLMENT, REINSTATEMENT
    InvoiceDate DATE NOT NULL DEFAULT GETDATE(),
    DueDate DATE NOT NULL,
    PremiumAmount DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    FeeAmount DECIMAL(18,2) DEFAULT 0,
    SurchargeAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    BalanceDue DECIMAL(18,2),
    Status VARCHAR(20) DEFAULT 'OPEN', -- OPEN, PAID, PARTIAL, OVERDUE, CANCELLED, WRITTEN_OFF
    InstallmentNumber INT DEFAULT 1,
    TotalInstallments INT DEFAULT 1,
    PaymentPlanID INT,
    LateFeeApplied BIT DEFAULT 0,
    LateFeeAmount DECIMAL(18,2) DEFAULT 0,
    CancellationNoticeDate DATE,
    CancellationEffectiveDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Premium Payments (received from customers)
CREATE TABLE Billing.PremiumPayments (
    PremiumPaymentID INT IDENTITY(1,1) PRIMARY KEY,
    PaymentNumber VARCHAR(20) NOT NULL UNIQUE,
    InvoiceID INT REFERENCES Billing.Invoices(InvoiceID),
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    PaymentDate DATE NOT NULL DEFAULT GETDATE(),
    Amount DECIMAL(18,2) NOT NULL,
    PaymentMethod VARCHAR(20) NOT NULL, -- CHECK, CREDIT_CARD, EFT, CASH, WIRE
    ReferenceNumber VARCHAR(50),
    CheckNumber VARCHAR(20),
    BankName VARCHAR(100),
    CreditCardLast4 VARCHAR(4),
    Status VARCHAR(20) DEFAULT 'APPLIED', -- APPLIED, RETURNED, REFUNDED, PENDING
    ReturnedDate DATE,
    ReturnReason VARCHAR(200),
    NSFFee DECIMAL(10,2) DEFAULT 0,
    AppliedToInvoice BIT DEFAULT 1,
    ReceiptNumber VARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50)
);
GO

-- Payment Plans
CREATE TABLE Billing.PaymentPlans (
    PaymentPlanID INT IDENTITY(1,1) PRIMARY KEY,
    PlanCode VARCHAR(20) NOT NULL UNIQUE,
    PlanName VARCHAR(100) NOT NULL,
    NumberOfInstallments INT NOT NULL,
    DownPaymentPercent DECIMAL(6,4) NOT NULL,
    InstallmentFee DECIMAL(10,2) DEFAULT 0,
    LateFeeAmount DECIMAL(10,2) DEFAULT 0,
    GracePeriodDays INT DEFAULT 10,
    CancellationNoticeDays INT DEFAULT 20,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Commission Transactions
CREATE TABLE Billing.CommissionTransactions (
    CommissionTransactionID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    AgentID INT NOT NULL REFERENCES Policy.Agents(AgentID),
    TransactionType VARCHAR(20) NOT NULL, -- EARNED, REVERSAL, OVERRIDE, BONUS, CHARGEBACK
    TransactionDate DATE NOT NULL DEFAULT GETDATE(),
    PremiumAmount DECIMAL(18,2),
    CommissionRate DECIMAL(6,4),
    CommissionAmount DECIMAL(18,2) NOT NULL,
    Status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, PAID, REVERSED
    PaymentDate DATE,
    PaymentReference VARCHAR(50),
    Description VARCHAR(200),
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Refunds
CREATE TABLE Billing.Refunds (
    RefundID INT IDENTITY(1,1) PRIMARY KEY,
    RefundNumber VARCHAR(20) NOT NULL UNIQUE,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    RefundType VARCHAR(20) NOT NULL, -- CANCELLATION, ENDORSEMENT, OVERPAYMENT, DUPLICATE
    RefundMethod VARCHAR(20) DEFAULT 'CHECK', -- CHECK, EFT, CREDIT_CARD_REVERSAL
    Amount DECIMAL(18,2) NOT NULL,
    CalculationMethod VARCHAR(20), -- PRO_RATA, SHORT_RATE, FLAT
    ProRataFactor DECIMAL(10,8),
    EarnedPremium DECIMAL(18,2),
    ReturnPremium DECIMAL(18,2),
    Status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, ISSUED, CLEARED, VOIDED
    ApprovedBy VARCHAR(50),
    ApprovedDate DATETIME,
    IssuedDate DATE,
    CheckNumber VARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50)
);
GO

-- ============================================================
-- REINSURANCE TABLES
-- ============================================================

-- Reinsurance Treaties
CREATE TABLE Reinsurance.Treaties (
    TreatyID INT IDENTITY(1,1) PRIMARY KEY,
    TreatyNumber VARCHAR(20) NOT NULL UNIQUE,
    TreatyName VARCHAR(200) NOT NULL,
    TreatyType VARCHAR(30) NOT NULL, -- QUOTA_SHARE, SURPLUS, EXCESS_OF_LOSS, CATASTROPHE, FACULTATIVE
    ReinsurerID INT,
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    RetentionAmount DECIMAL(18,2), -- company keeps this amount
    RetentionPercent DECIMAL(6,4),
    CessionPercent DECIMAL(6,4), -- % ceded to reinsurer
    CessionLimit DECIMAL(18,2),
    AttachmentPoint DECIMAL(18,2), -- for excess of loss
    ExhaustionPoint DECIMAL(18,2),
    ReinstatementCount INT DEFAULT 1,
    ReinstatementPercent DECIMAL(6,4),
    PremiumRate DECIMAL(10,6),
    MinimumPremium DECIMAL(18,2),
    DepositPremium DECIMAL(18,2),
    CommissionRate DECIMAL(6,4), -- ceding commission
    ProfitCommissionRate DECIMAL(6,4),
    CoveredPerils VARCHAR(500),
    CoveredStates VARCHAR(200),
    CoveredPolicyTypes VARCHAR(200),
    Status VARCHAR(20) DEFAULT 'ACTIVE', -- DRAFT, ACTIVE, EXPIRED, COMMUTED
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Reinsurers
CREATE TABLE Reinsurance.Reinsurers (
    ReinsurerID INT IDENTITY(1,1) PRIMARY KEY,
    ReinsurerCode VARCHAR(20) NOT NULL UNIQUE,
    ReinsurerName VARCHAR(200) NOT NULL,
    AMBestRating VARCHAR(10),
    SPRating VARCHAR(10),
    ContactName VARCHAR(200),
    Phone VARCHAR(20),
    Email VARCHAR(200),
    AddressLine1 VARCHAR(200),
    City VARCHAR(100),
    StateCode CHAR(2),
    Country VARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Reinsurance Cessions (individual policy/claim cessions)
CREATE TABLE Reinsurance.Cessions (
    CessionID INT IDENTITY(1,1) PRIMARY KEY,
    TreatyID INT NOT NULL REFERENCES Reinsurance.Treaties(TreatyID),
    PolicyID INT REFERENCES Policy.Policies(PolicyID),
    ClaimID INT,
    CessionType VARCHAR(20) NOT NULL, -- PREMIUM, LOSS, RESERVE
    GrossAmount DECIMAL(18,2) NOT NULL,
    CededAmount DECIMAL(18,2) NOT NULL,
    RetainedAmount DECIMAL(18,2) NOT NULL,
    CessionPercent DECIMAL(6,4),
    TransactionDate DATE NOT NULL DEFAULT GETDATE(),
    AccountingPeriod VARCHAR(10), -- YYYY-MM
    Status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, REPORTED, SETTLED, REVERSED
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Reinsurance Bordereaux (periodic reporting)
CREATE TABLE Reinsurance.Bordereaux (
    BordereauxID INT IDENTITY(1,1) PRIMARY KEY,
    TreatyID INT NOT NULL REFERENCES Reinsurance.Treaties(TreatyID),
    ReportingPeriod VARCHAR(10) NOT NULL, -- YYYY-MM
    ReportType VARCHAR(20) NOT NULL, -- PREMIUM, LOSS, OUTSTANDING
    TotalGross DECIMAL(18,2) DEFAULT 0,
    TotalCeded DECIMAL(18,2) DEFAULT 0,
    TotalRetained DECIMAL(18,2) DEFAULT 0,
    RecordCount INT DEFAULT 0,
    GeneratedDate DATETIME DEFAULT GETDATE(),
    SubmittedDate DATETIME,
    Status VARCHAR(20) DEFAULT 'DRAFT', -- DRAFT, SUBMITTED, ACCEPTED, DISPUTED
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO
