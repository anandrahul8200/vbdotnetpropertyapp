-- ============================================================
-- UNDERWRITING & RATING TABLES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- Rate Tables (effective-dated)
CREATE TABLE Underwriting.RateTables (
    RateTableID INT IDENTITY(1,1) PRIMARY KEY,
    RateTableCode VARCHAR(30) NOT NULL,
    RateTableName VARCHAR(100) NOT NULL,
    RateType VARCHAR(20) NOT NULL, -- BASE, FACTOR, DISCOUNT, SURCHARGE, TAX
    PolicyType VARCHAR(30) NOT NULL,
    StateCode CHAR(2),
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL DEFAULT '9999-12-31',
    Version INT DEFAULT 1,
    ApprovedBy VARCHAR(50),
    ApprovedDate DATETIME,
    FilingNumber VARCHAR(50),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UNIQUE(RateTableCode, StateCode, EffectiveDate)
);
GO

-- Rate Table Details
CREATE TABLE Underwriting.RateTableDetails (
    RateDetailID INT IDENTITY(1,1) PRIMARY KEY,
    RateTableID INT NOT NULL REFERENCES Underwriting.RateTables(RateTableID),
    FactorCode VARCHAR(30) NOT NULL,
    FactorValue VARCHAR(100), -- the lookup key (e.g., 'FRAME', 'MASONRY')
    MinValue DECIMAL(18,4),
    MaxValue DECIMAL(18,4),
    Rate DECIMAL(18,6) NOT NULL, -- the rate/factor to apply
    FlatAmount DECIMAL(18,2) DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1
);
GO

-- Base Rates (per $1000 of coverage)
CREATE TABLE Underwriting.BaseRates (
    BaseRateID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyType VARCHAR(30) NOT NULL,
    StateCode CHAR(2) NOT NULL,
    TerritoryCode VARCHAR(20),
    ConstructionType VARCHAR(30) NOT NULL,
    ProtectionClass INT NOT NULL, -- 1-10
    CoverageCode VARCHAR(20) NOT NULL,
    RatePer1000 DECIMAL(10,6) NOT NULL,
    MinPremium DECIMAL(10,2) DEFAULT 0,
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL DEFAULT '9999-12-31',
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Rating Factors
CREATE TABLE Underwriting.RatingFactors (
    RatingFactorID INT IDENTITY(1,1) PRIMARY KEY,
    FactorType VARCHAR(30) NOT NULL, -- AGE_OF_HOME, ROOF_TYPE, DEDUCTIBLE, CREDIT_SCORE, CLAIMS_HISTORY, PROTECTIVE_DEVICE
    PolicyType VARCHAR(30) NOT NULL,
    StateCode CHAR(2),
    FactorKey VARCHAR(50) NOT NULL,
    FactorKeyDescription VARCHAR(200),
    MinRange DECIMAL(18,4),
    MaxRange DECIMAL(18,4),
    Factor DECIMAL(10,6) NOT NULL, -- multiplier (1.0 = no change, >1 = surcharge, <1 = discount)
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL DEFAULT '9999-12-31',
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Deductible Options
CREATE TABLE Underwriting.DeductibleOptions (
    DeductibleOptionID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyType VARCHAR(30) NOT NULL,
    CoverageCode VARCHAR(20) NOT NULL,
    DeductibleType VARCHAR(20) NOT NULL, -- FLAT, PERCENTAGE
    DeductibleAmount DECIMAL(18,2),
    DeductiblePercent DECIMAL(6,4),
    PremiumFactor DECIMAL(10,6) NOT NULL, -- discount/surcharge factor
    IsDefault BIT DEFAULT 0,
    MinInsuredValue DECIMAL(18,2),
    MaxInsuredValue DECIMAL(18,2),
    StateCode CHAR(2),
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL DEFAULT '9999-12-31',
    IsActive BIT DEFAULT 1
);
GO

-- Underwriting Rules
CREATE TABLE Underwriting.Rules (
    RuleID INT IDENTITY(1,1) PRIMARY KEY,
    RuleCode VARCHAR(30) NOT NULL UNIQUE,
    RuleName VARCHAR(200) NOT NULL,
    RuleCategory VARCHAR(30) NOT NULL, -- ELIGIBILITY, PRICING, LIMIT, REFERRAL, DECLINE
    PolicyType VARCHAR(30),
    StateCode CHAR(2),
    RuleExpression VARCHAR(MAX) NOT NULL, -- business rule logic (pseudo-code or SQL condition)
    ActionType VARCHAR(20) NOT NULL, -- ACCEPT, REFER, DECLINE, SURCHARGE, EXCLUDE
    ActionValue VARCHAR(200),
    Severity VARCHAR(10) DEFAULT 'MEDIUM', -- LOW, MEDIUM, HIGH, CRITICAL
    Priority INT DEFAULT 100,
    IsActive BIT DEFAULT 1,
    EffectiveDate DATE DEFAULT GETDATE(),
    ExpiryDate DATE DEFAULT '9999-12-31',
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Underwriting Referrals
CREATE TABLE Underwriting.Referrals (
    ReferralID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    RuleID INT REFERENCES Underwriting.Rules(RuleID),
    ReferralReason VARCHAR(500) NOT NULL,
    ReferralStatus VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, DECLINED, CONDITIONAL
    ReferralDate DATETIME DEFAULT GETDATE(),
    AssignedTo VARCHAR(50),
    ReviewedBy VARCHAR(50),
    ReviewDate DATETIME,
    Decision VARCHAR(20),
    DecisionNotes VARCHAR(MAX),
    Conditions VARCHAR(MAX),
    ExpiryDate DATE,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Underwriting Worksheets (rating calculation audit)
CREATE TABLE Underwriting.RatingWorksheets (
    WorksheetID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CoverageCode VARCHAR(20) NOT NULL,
    CalculationDate DATETIME DEFAULT GETDATE(),
    BaseRate DECIMAL(18,6),
    InsuredValue DECIMAL(18,2),
    BasePremium DECIMAL(18,2),
    -- Individual factors applied
    ConstructionFactor DECIMAL(10,6) DEFAULT 1.0,
    AgeFactor DECIMAL(10,6) DEFAULT 1.0,
    RoofFactor DECIMAL(10,6) DEFAULT 1.0,
    ProtectionClassFactor DECIMAL(10,6) DEFAULT 1.0,
    DeductibleFactor DECIMAL(10,6) DEFAULT 1.0,
    CreditFactor DECIMAL(10,6) DEFAULT 1.0,
    ClaimsHistoryFactor DECIMAL(10,6) DEFAULT 1.0,
    ProtectiveDeviceFactor DECIMAL(10,6) DEFAULT 1.0,
    TerritoryFactor DECIMAL(10,6) DEFAULT 1.0,
    OccupancyFactor DECIMAL(10,6) DEFAULT 1.0,
    LoyaltyFactor DECIMAL(10,6) DEFAULT 1.0,
    MultiPolicyFactor DECIMAL(10,6) DEFAULT 1.0,
    NewHomeFactor DECIMAL(10,6) DEFAULT 1.0,
    -- Calculated values
    TotalFactor DECIMAL(10,6),
    CalculatedPremium DECIMAL(18,2),
    MinPremiumApplied BIT DEFAULT 0,
    FinalPremium DECIMAL(18,2),
    CalculatedBy VARCHAR(50)
);
GO

-- Commission Schedules
CREATE TABLE Underwriting.CommissionSchedules (
    CommissionScheduleID INT IDENTITY(1,1) PRIMARY KEY,
    ScheduleCode VARCHAR(20) NOT NULL,
    ScheduleName VARCHAR(100) NOT NULL,
    PolicyType VARCHAR(30) NOT NULL,
    TransactionType VARCHAR(20) NOT NULL, -- NEW, RENEWAL, ENDORSEMENT
    CommissionPercent DECIMAL(6,4) NOT NULL,
    OverridePercent DECIMAL(6,4) DEFAULT 0,
    BonusPercent DECIMAL(6,4) DEFAULT 0,
    MinPremium DECIMAL(18,2) DEFAULT 0,
    MaxPremium DECIMAL(18,2),
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL DEFAULT '9999-12-31',
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Moratorium (temporary restrictions on writing/renewing in certain areas)
CREATE TABLE Underwriting.Moratoriums (
    MoratoriumID INT IDENTITY(1,1) PRIMARY KEY,
    MoratoriumName VARCHAR(200) NOT NULL,
    MoratoriumType VARCHAR(20) NOT NULL, -- NEW_BUSINESS, RENEWAL, ALL
    Reason VARCHAR(500),
    AffectedStates VARCHAR(200),
    AffectedZipCodes VARCHAR(MAX),
    AffectedPolicyTypes VARCHAR(200),
    StartDate DATE NOT NULL,
    EndDate DATE,
    IsActive BIT DEFAULT 1,
    DeclaredBy VARCHAR(50),
    DeclaredDate DATETIME DEFAULT GETDATE(),
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO
