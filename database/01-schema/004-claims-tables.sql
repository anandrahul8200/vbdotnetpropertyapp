-- ============================================================
-- CLAIMS MANAGEMENT TABLES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- Claims
CREATE TABLE Claims.Claims (
    ClaimID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimNumber VARCHAR(20) NOT NULL UNIQUE,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    PropertyID INT NOT NULL REFERENCES Policy.Properties(PropertyID),
    ClaimStatus VARCHAR(20) NOT NULL DEFAULT 'FNOL', 
    -- FNOL, ASSIGNED, INVESTIGATING, ASSESSED, APPROVED, DENIED, SETTLED, CLOSED, REOPENED, LITIGATION
    ClaimType VARCHAR(30) NOT NULL, -- PROPERTY_DAMAGE, THEFT, LIABILITY, WATER_DAMAGE, FIRE, WIND, HAIL, OTHER
    CatastropheID INT,
    LossDate DATETIME NOT NULL,
    ReportedDate DATETIME NOT NULL DEFAULT GETDATE(),
    AssignedDate DATETIME,
    ClosedDate DATETIME,
    ReopenedDate DATETIME,
    ReopenedReason VARCHAR(200),
    LossDescription VARCHAR(MAX),
    LossLocation VARCHAR(500),
    PoliceReportNumber VARCHAR(50),
    FireReportNumber VARCHAR(50),
    WeatherCondition VARCHAR(50),
    PointOfOrigin VARCHAR(200),
    EstimatedLoss DECIMAL(18,2),
    ActualLoss DECIMAL(18,2),
    DeductibleAmount DECIMAL(18,2),
    PolicyLimit DECIMAL(18,2),
    TotalPaid DECIMAL(18,2) DEFAULT 0,
    TotalReserve DECIMAL(18,2) DEFAULT 0,
    TotalRecovery DECIMAL(18,2) DEFAULT 0,
    TotalSubrogation DECIMAL(18,2) DEFAULT 0,
    NetIncurred DECIMAL(18,2) DEFAULT 0, -- Reserve + Paid - Recovery
    AdjusterID INT,
    SupervisorID INT,
    ExaminerID INT,
    VendorID INT,
    Priority VARCHAR(10) DEFAULT 'NORMAL', -- LOW, NORMAL, HIGH, CRITICAL
    Complexity VARCHAR(10) DEFAULT 'SIMPLE', -- SIMPLE, MODERATE, COMPLEX
    FraudScore DECIMAL(6,2) DEFAULT 0,
    FraudIndicators VARCHAR(MAX),
    IsSIUReferred BIT DEFAULT 0,
    SIUReferralDate DATETIME,
    IsLitigation BIT DEFAULT 0,
    LitigationDate DATETIME,
    AttorneyName VARCHAR(200),
    IsSubrogation BIT DEFAULT 0,
    SubrogationStatus VARCHAR(20),
    ThirdPartyName VARCHAR(200),
    ThirdPartyInsurer VARCHAR(200),
    ThirdPartyPolicyNumber VARCHAR(50),
    CoverageVerified BIT DEFAULT 0,
    CoverageVerifiedDate DATETIME,
    CoverageVerifiedBy VARCHAR(50),
    DenialReason VARCHAR(500),
    SettlementType VARCHAR(20), -- AGREED, APPRAISAL, ARBITRATION, LITIGATION
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50)
);
GO

-- Claim Coverages (which coverages are involved in this claim)
CREATE TABLE Claims.ClaimCoverages (
    ClaimCoverageID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    CoverageID INT NOT NULL REFERENCES Policy.Coverages(CoverageID),
    CoverageCode VARCHAR(20) NOT NULL,
    LimitApplicable DECIMAL(18,2),
    DeductibleApplicable DECIMAL(18,2),
    ReserveAmount DECIMAL(18,2) DEFAULT 0,
    PaidAmount DECIMAL(18,2) DEFAULT 0,
    RecoveryAmount DECIMAL(18,2) DEFAULT 0,
    Status VARCHAR(20) DEFAULT 'OPEN', -- OPEN, CLOSED, DENIED
    UNIQUE(ClaimID, CoverageID)
);
GO

-- Claim Reserves
CREATE TABLE Claims.Reserves (
    ReserveID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    ReserveType VARCHAR(20) NOT NULL, -- CASE, EXPENSE, IBNR, BULK
    ReserveCategory VARCHAR(30), -- INDEMNITY, DEFENSE, ADJUSTMENT_EXPENSE, MEDICAL
    CoverageCode VARCHAR(20),
    Amount DECIMAL(18,2) NOT NULL,
    PreviousAmount DECIMAL(18,2) DEFAULT 0,
    ChangeAmount DECIMAL(18,2) DEFAULT 0,
    ChangeReason VARCHAR(200),
    EffectiveDate DATETIME DEFAULT GETDATE(),
    SetBy VARCHAR(50),
    ApprovedBy VARCHAR(50),
    ApprovalRequired BIT DEFAULT 0,
    IsApproved BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Claim Payments
CREATE TABLE Claims.Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    PaymentNumber VARCHAR(20) NOT NULL UNIQUE,
    PaymentType VARCHAR(20) NOT NULL, -- INDEMNITY, EXPENSE, PARTIAL, FINAL, SUPPLEMENT
    PaymentMethod VARCHAR(20) DEFAULT 'CHECK', -- CHECK, EFT, WIRE, DRAFT
    PayeeType VARCHAR(20) NOT NULL, -- INSURED, VENDOR, ATTORNEY, MORTGAGEE, LIENHOLDER
    PayeeName VARCHAR(200) NOT NULL,
    PayeeAddress VARCHAR(500),
    Amount DECIMAL(18,2) NOT NULL,
    CheckNumber VARCHAR(20),
    CheckDate DATE,
    ClearedDate DATE,
    VoidedDate DATE,
    VoidReason VARCHAR(200),
    CoverageCode VARCHAR(20),
    InvoiceNumber VARCHAR(50),
    Description VARCHAR(500),
    TaxReportable BIT DEFAULT 0,
    Form1099Required BIT DEFAULT 0,
    Status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, ISSUED, CLEARED, VOIDED, STOPPED
    ApprovalRequired BIT DEFAULT 0,
    ApprovedBy VARCHAR(50),
    ApprovedDate DATETIME,
    IssuedBy VARCHAR(50),
    IssuedDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Claim Activities / Diary
CREATE TABLE Claims.Activities (
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    ActivityType VARCHAR(30) NOT NULL, -- NOTE, PHONE_CALL, EMAIL, INSPECTION, DOCUMENT, STATUS_CHANGE, PAYMENT, RESERVE_CHANGE
    ActivityDate DATETIME DEFAULT GETDATE(),
    DueDate DATETIME,
    CompletedDate DATETIME,
    Subject VARCHAR(200),
    Description VARCHAR(MAX),
    ContactName VARCHAR(200),
    ContactPhone VARCHAR(20),
    Duration INT, -- minutes
    IsCompleted BIT DEFAULT 0,
    AssignedTo VARCHAR(50),
    Priority VARCHAR(10) DEFAULT 'NORMAL',
    ReminderDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50)
);
GO

-- Claim Status History
CREATE TABLE Claims.StatusHistory (
    StatusHistoryID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    PreviousStatus VARCHAR(20),
    NewStatus VARCHAR(20) NOT NULL,
    ChangeDate DATETIME DEFAULT GETDATE(),
    ChangedBy VARCHAR(50),
    Reason VARCHAR(500),
    Notes VARCHAR(MAX)
);
GO

-- Catastrophe Events
CREATE TABLE Claims.Catastrophes (
    CatastropheID INT IDENTITY(1,1) PRIMARY KEY,
    CatastropheNumber VARCHAR(20) NOT NULL UNIQUE,
    CatastropheName VARCHAR(200) NOT NULL,
    CatastropheType VARCHAR(30) NOT NULL, -- HURRICANE, TORNADO, EARTHQUAKE, FLOOD, WILDFIRE, HAIL, WINTER_STORM
    EventDate DATE NOT NULL,
    EndDate DATE,
    AffectedStates VARCHAR(200),
    AffectedZipCodes VARCHAR(MAX),
    EstimatedIndustryLoss DECIMAL(18,2),
    CompanyEstimatedLoss DECIMAL(18,2),
    TotalClaimsCount INT DEFAULT 0,
    TotalPaidAmount DECIMAL(18,2) DEFAULT 0,
    TotalReserveAmount DECIMAL(18,2) DEFAULT 0,
    PCSNumber VARCHAR(20), -- Property Claim Services number
    IsActive BIT DEFAULT 1,
    DeclaredDate DATETIME DEFAULT GETDATE(),
    ClosedDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Vendors (contractors, adjusters, etc.)
CREATE TABLE Claims.Vendors (
    VendorID INT IDENTITY(1,1) PRIMARY KEY,
    VendorNumber VARCHAR(20) NOT NULL UNIQUE,
    VendorName VARCHAR(200) NOT NULL,
    VendorType VARCHAR(30) NOT NULL, -- ADJUSTER, CONTRACTOR, APPRAISER, ENGINEER, ATTORNEY, INVESTIGATOR
    ContactName VARCHAR(200),
    Phone VARCHAR(20),
    Email VARCHAR(200),
    AddressLine1 VARCHAR(200),
    City VARCHAR(100),
    StateCode CHAR(2),
    ZipCode VARCHAR(10),
    LicenseNumber VARCHAR(50),
    InsuranceCarrier VARCHAR(200),
    InsurancePolicyNumber VARCHAR(50),
    InsuranceExpiryDate DATE,
    HourlyRate DECIMAL(10,2),
    DailyRate DECIMAL(10,2),
    PreferredVendor BIT DEFAULT 0,
    Rating DECIMAL(3,1),
    TotalAssignments INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Claim Assignments (adjuster/vendor assignments)
CREATE TABLE Claims.Assignments (
    AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    AssigneeType VARCHAR(20) NOT NULL, -- ADJUSTER, VENDOR, EXAMINER, SIU
    AssigneeID INT NOT NULL,
    AssignmentDate DATETIME DEFAULT GETDATE(),
    DueDate DATETIME,
    CompletedDate DATETIME,
    Status VARCHAR(20) DEFAULT 'ASSIGNED', -- ASSIGNED, IN_PROGRESS, COMPLETED, REASSIGNED, CANCELLED
    Instructions VARCHAR(MAX),
    ReassignedFrom INT,
    ReassignReason VARCHAR(200),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50)
);
GO

-- Subrogation
CREATE TABLE Claims.Subrogation (
    SubrogationID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    SubrogationStatus VARCHAR(20) DEFAULT 'IDENTIFIED', -- IDENTIFIED, DEMAND_SENT, NEGOTIATING, ARBITRATION, SETTLED, CLOSED, ABANDONED
    ResponsibleParty VARCHAR(200),
    ResponsiblePartyInsurer VARCHAR(200),
    ResponsiblePartyPolicy VARCHAR(50),
    DemandAmount DECIMAL(18,2),
    DemandDate DATE,
    ResponseDate DATE,
    SettlementAmount DECIMAL(18,2),
    SettlementDate DATE,
    RecoveryAmount DECIMAL(18,2) DEFAULT 0,
    ArbitrationDate DATE,
    ArbitrationResult VARCHAR(200),
    Notes VARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Fraud Indicators
CREATE TABLE Claims.FraudIndicators (
    FraudIndicatorID INT IDENTITY(1,1) PRIMARY KEY,
    IndicatorCode VARCHAR(20) NOT NULL UNIQUE,
    IndicatorName VARCHAR(200) NOT NULL,
    Category VARCHAR(30), -- TIMING, FINANCIAL, BEHAVIORAL, DOCUMENTATION, HISTORY
    Weight DECIMAL(4,2) DEFAULT 1.0,
    Description VARCHAR(500),
    IsActive BIT DEFAULT 1
);
GO

-- Claim Fraud Scores
CREATE TABLE Claims.ClaimFraudScores (
    ClaimFraudScoreID INT IDENTITY(1,1) PRIMARY KEY,
    ClaimID INT NOT NULL REFERENCES Claims.Claims(ClaimID),
    IndicatorID INT NOT NULL REFERENCES Claims.FraudIndicators(FraudIndicatorID),
    IsTriggered BIT DEFAULT 0,
    Score DECIMAL(6,2) DEFAULT 0,
    Notes VARCHAR(500),
    EvaluatedDate DATETIME DEFAULT GETDATE(),
    EvaluatedBy VARCHAR(50)
);
GO
