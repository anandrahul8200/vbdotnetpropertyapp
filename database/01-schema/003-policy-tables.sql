-- ============================================================
-- POLICY MANAGEMENT TABLES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- Customers / Policyholders
CREATE TABLE Policy.Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber VARCHAR(20) NOT NULL UNIQUE,
    CustomerType CHAR(1) NOT NULL DEFAULT 'I', -- I=Individual, C=Commercial, T=Trust
    Title VARCHAR(10),
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    CompanyName VARCHAR(200),
    TaxID VARCHAR(20),
    DateOfBirth DATE,
    Gender CHAR(1),
    MaritalStatus VARCHAR(20),
    Occupation VARCHAR(100),
    AnnualIncome DECIMAL(18,2),
    CreditScore INT,
    Email VARCHAR(200),
    Phone VARCHAR(20),
    MobilePhone VARCHAR(20),
    AddressLine1 VARCHAR(200),
    AddressLine2 VARCHAR(200),
    City VARCHAR(100),
    StateCode CHAR(2),
    ZipCode VARCHAR(10),
    County VARCHAR(100),
    Country VARCHAR(50) DEFAULT 'US',
    PreferredContact VARCHAR(20) DEFAULT 'EMAIL',
    DoNotContact BIT DEFAULT 0,
    RiskTier VARCHAR(20) DEFAULT 'STANDARD', -- PREFERRED, STANDARD, SUBSTANDARD
    CustomerSince DATE DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50)
);
GO

-- Agents / Producers
CREATE TABLE Policy.Agents (
    AgentID INT IDENTITY(1,1) PRIMARY KEY,
    AgentNumber VARCHAR(20) NOT NULL UNIQUE,
    AgentType VARCHAR(20) NOT NULL, -- CAPTIVE, INDEPENDENT, BROKER
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    LicenseNumber VARCHAR(50),
    LicenseState CHAR(2),
    LicenseExpiryDate DATE,
    AgencyID INT,
    CommissionRate DECIMAL(6,4) DEFAULT 0.10,
    OverrideRate DECIMAL(6,4) DEFAULT 0,
    Email VARCHAR(200),
    Phone VARCHAR(20),
    TerritoryID INT,
    HierarchyLevel INT DEFAULT 1,
    ParentAgentID INT,
    AppointmentDate DATE,
    TerminationDate DATE,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Agencies
CREATE TABLE Policy.Agencies (
    AgencyID INT IDENTITY(1,1) PRIMARY KEY,
    AgencyCode VARCHAR(20) NOT NULL UNIQUE,
    AgencyName VARCHAR(200) NOT NULL,
    AgencyType VARCHAR(20), -- DIRECT, INDEPENDENT, MGA
    AddressLine1 VARCHAR(200),
    City VARCHAR(100),
    StateCode CHAR(2),
    ZipCode VARCHAR(10),
    Phone VARCHAR(20),
    Email VARCHAR(200),
    RegionID INT,
    TerritoryID INT,
    ContractDate DATE,
    ContractExpiryDate DATE,
    CommissionScheduleID INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Territories
CREATE TABLE Policy.Territories (
    TerritoryID INT IDENTITY(1,1) PRIMARY KEY,
    TerritoryCode VARCHAR(20) NOT NULL UNIQUE,
    TerritoryName VARCHAR(100) NOT NULL,
    RegionID INT,
    StateCode CHAR(2),
    ZipCodeRangeStart VARCHAR(10),
    ZipCodeRangeEnd VARCHAR(10),
    RiskMultiplier DECIMAL(6,4) DEFAULT 1.0,
    IsActive BIT DEFAULT 1
);
GO

-- Regions
CREATE TABLE Policy.Regions (
    RegionID INT IDENTITY(1,1) PRIMARY KEY,
    RegionCode VARCHAR(20) NOT NULL UNIQUE,
    RegionName VARCHAR(100) NOT NULL,
    RegionManagerID INT,
    IsActive BIT DEFAULT 1
);
GO

-- Properties (Insured Locations)
CREATE TABLE Policy.Properties (
    PropertyID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    PropertyNumber VARCHAR(20) NOT NULL UNIQUE,
    PropertyType VARCHAR(30) NOT NULL, -- SINGLE_FAMILY, CONDO, TOWNHOUSE, COMMERCIAL, APARTMENT, MOBILE_HOME
    ConstructionType VARCHAR(30), -- FRAME, MASONRY, FIRE_RESISTIVE, STEEL, MIXED
    OccupancyType VARCHAR(30), -- OWNER_OCCUPIED, TENANT, VACANT, SEASONAL
    YearBuilt INT,
    SquareFootage INT,
    NumberOfStories INT DEFAULT 1,
    NumberOfUnits INT DEFAULT 1,
    RoofType VARCHAR(30),
    RoofAge INT,
    HeatingType VARCHAR(30),
    ElectricalType VARCHAR(30),
    PlumbingType VARCHAR(30),
    HasBasement BIT DEFAULT 0,
    HasPool BIT DEFAULT 0,
    HasFireAlarm BIT DEFAULT 0,
    HasBurglarAlarm BIT DEFAULT 0,
    HasSprinklerSystem BIT DEFAULT 0,
    DistanceToFireStation DECIMAL(6,2), -- miles
    DistanceToHydrant DECIMAL(6,2), -- miles
    FireProtectionClass INT, -- 1-10
    FloodZone VARCHAR(10),
    EarthquakeZone INT,
    WindZone VARCHAR(10),
    MarketValue DECIMAL(18,2),
    ReplacementCost DECIMAL(18,2),
    LandValue DECIMAL(18,2),
    LastAppraisalDate DATE,
    AddressLine1 VARCHAR(200) NOT NULL,
    AddressLine2 VARCHAR(200),
    City VARCHAR(100) NOT NULL,
    StateCode CHAR(2) NOT NULL,
    ZipCode VARCHAR(10) NOT NULL,
    County VARCHAR(100),
    Latitude DECIMAL(10,7),
    Longitude DECIMAL(10,7),
    TerritoryID INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50)
);
GO

-- Policies
CREATE TABLE Policy.Policies (
    PolicyID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyNumber VARCHAR(20) NOT NULL UNIQUE,
    PolicyVersion INT DEFAULT 1,
    PolicyType VARCHAR(30) NOT NULL, -- HOMEOWNERS, DWELLING_FIRE, COMMERCIAL_PROPERTY, RENTERS, CONDO
    PolicyStatus VARCHAR(20) NOT NULL DEFAULT 'QUOTE', -- QUOTE, BOUND, ACTIVE, CANCELLED, EXPIRED, NON_RENEWED, SUSPENDED
    CustomerID INT NOT NULL REFERENCES Policy.Customers(CustomerID),
    PropertyID INT NOT NULL REFERENCES Policy.Properties(PropertyID),
    AgentID INT NOT NULL REFERENCES Policy.Agents(AgentID),
    UnderwriterID INT,
    EffectiveDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    OriginalEffectiveDate DATE,
    CancellationDate DATE,
    CancellationReason VARCHAR(100),
    RenewalOfPolicyID INT,
    TermMonths INT DEFAULT 12,
    AnnualPremium DECIMAL(18,2),
    WrittenPremium DECIMAL(18,2),
    EarnedPremium DECIMAL(18,2),
    TotalTaxes DECIMAL(18,2) DEFAULT 0,
    TotalFees DECIMAL(18,2) DEFAULT 0,
    TotalSurcharges DECIMAL(18,2) DEFAULT 0,
    GrossPremium DECIMAL(18,2),
    CommissionAmount DECIMAL(18,2),
    CommissionRate DECIMAL(6,4),
    PaymentPlan VARCHAR(20) DEFAULT 'ANNUAL', -- ANNUAL, SEMI_ANNUAL, QUARTERLY, MONTHLY
    BillingMethod VARCHAR(20) DEFAULT 'DIRECT', -- DIRECT, AGENCY
    DeductibleAmount DECIMAL(18,2),
    TotalInsuredValue DECIMAL(18,2),
    PriorCarrier VARCHAR(100),
    PriorPolicyNumber VARCHAR(50),
    PriorExpiryDate DATE,
    YearsWithPriorCarrier INT,
    ClaimFreeYears INT DEFAULT 0,
    MultiPolicyDiscount BIT DEFAULT 0,
    LoyaltyDiscount BIT DEFAULT 0,
    ClaimFreeDiscount BIT DEFAULT 0,
    ProtectiveDeviceDiscount BIT DEFAULT 0,
    NewHomeDiscount BIT DEFAULT 0,
    RiskScore DECIMAL(6,2),
    UnderwritingTier VARCHAR(20),
    IsRenewal BIT DEFAULT 0,
    RenewalCount INT DEFAULT 0,
    BoundDate DATETIME,
    BoundBy VARCHAR(50),
    IssuedDate DATETIME,
    IssuedBy VARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50)
);
GO

-- Policy Versions (full history)
CREATE TABLE Policy.PolicyVersions (
    PolicyVersionID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    VersionNumber INT NOT NULL,
    VersionType VARCHAR(20) NOT NULL, -- NEW, RENEWAL, ENDORSEMENT, CANCELLATION, REINSTATEMENT
    EffectiveDate DATE NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    PremiumChange DECIMAL(18,2) DEFAULT 0,
    Description VARCHAR(500),
    EndorsementID INT,
    ProcessedBy VARCHAR(50),
    UNIQUE(PolicyID, VersionNumber)
);
GO

-- Coverages
CREATE TABLE Policy.Coverages (
    CoverageID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    CoverageCode VARCHAR(20) NOT NULL,
    CoverageName VARCHAR(100) NOT NULL,
    CoverageType VARCHAR(30), -- DWELLING, OTHER_STRUCTURES, PERSONAL_PROPERTY, LOSS_OF_USE, LIABILITY, MEDICAL
    LimitAmount DECIMAL(18,2),
    DeductibleAmount DECIMAL(18,2),
    DeductibleType VARCHAR(20) DEFAULT 'FLAT', -- FLAT, PERCENTAGE
    CoveragePercentage DECIMAL(6,4), -- e.g., Other Structures = 10% of Dwelling
    Premium DECIMAL(18,2),
    IsRequired BIT DEFAULT 0,
    IsSelected BIT DEFAULT 1,
    EffectiveDate DATE,
    ExpiryDate DATE,
    CoinsurancePercent DECIMAL(6,4) DEFAULT 0.80,
    AggregateLimit DECIMAL(18,2),
    PerOccurrenceLimit DECIMAL(18,2),
    WaitingPeriodDays INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Perils (covered causes of loss)
CREATE TABLE Policy.Perils (
    PerilID INT IDENTITY(1,1) PRIMARY KEY,
    PerilCode VARCHAR(20) NOT NULL UNIQUE,
    PerilName VARCHAR(100) NOT NULL,
    PerilCategory VARCHAR(30), -- FIRE, WATER, WIND, THEFT, LIABILITY, NATURAL_DISASTER
    Description VARCHAR(500),
    IsStandard BIT DEFAULT 1,
    RequiresAdditionalPremium BIT DEFAULT 0,
    IsActive BIT DEFAULT 1
);
GO

-- Coverage-Peril Mapping
CREATE TABLE Policy.CoveragePerils (
    CoveragePerilID INT IDENTITY(1,1) PRIMARY KEY,
    CoverageID INT NOT NULL REFERENCES Policy.Coverages(CoverageID),
    PerilID INT NOT NULL REFERENCES Policy.Perils(PerilID),
    IsIncluded BIT DEFAULT 1,
    IsExcluded BIT DEFAULT 0,
    SubLimit DECIMAL(18,2),
    AdditionalDeductible DECIMAL(18,2),
    AdditionalPremium DECIMAL(18,2) DEFAULT 0,
    UNIQUE(CoverageID, PerilID)
);
GO

-- Endorsements
CREATE TABLE Policy.Endorsements (
    EndorsementID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
    EndorsementNumber VARCHAR(20) NOT NULL,
    EndorsementType VARCHAR(30) NOT NULL, -- COVERAGE_CHANGE, LIMIT_CHANGE, DEDUCTIBLE_CHANGE, ADD_PROPERTY, REMOVE_PROPERTY, NAME_CHANGE, ADDRESS_CHANGE
    EndorsementStatus VARCHAR(20) DEFAULT 'PENDING', -- PENDING, APPROVED, APPLIED, REJECTED, CANCELLED
    EffectiveDate DATE NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    ApprovalDate DATETIME,
    ApprovedBy VARCHAR(50),
    Description VARCHAR(500),
    PremiumChange DECIMAL(18,2) DEFAULT 0,
    ProRataFactor DECIMAL(10,8),
    ReturnPremium DECIMAL(18,2) DEFAULT 0,
    AdditionalPremium DECIMAL(18,2) DEFAULT 0,
    ChangeDetails VARCHAR(MAX), -- JSON or XML with before/after values
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50),
    UNIQUE(PolicyID, EndorsementNumber)
);
GO

-- Policy Documents
CREATE TABLE Policy.Documents (
    DocumentID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT REFERENCES Policy.Policies(PolicyID),
    ClaimID INT,
    DocumentType VARCHAR(30) NOT NULL, -- DECLARATION, ENDORSEMENT, INVOICE, CORRESPONDENCE, PHOTO, APPRAISAL, REPORT
    DocumentName VARCHAR(200) NOT NULL,
    FilePath VARCHAR(500),
    FileSize INT,
    MimeType VARCHAR(100),
    Description VARCHAR(500),
    UploadedDate DATETIME DEFAULT GETDATE(),
    UploadedBy VARCHAR(50),
    IsActive BIT DEFAULT 1
);
GO

-- Policy Notes
CREATE TABLE Policy.Notes (
    NoteID INT IDENTITY(1,1) PRIMARY KEY,
    PolicyID INT REFERENCES Policy.Policies(PolicyID),
    ClaimID INT,
    CustomerID INT REFERENCES Policy.Customers(CustomerID),
    NoteType VARCHAR(30) DEFAULT 'GENERAL', -- GENERAL, UNDERWRITING, CLAIMS, BILLING, SYSTEM
    NoteText VARCHAR(MAX) NOT NULL,
    IsInternal BIT DEFAULT 1,
    Priority VARCHAR(10) DEFAULT 'NORMAL', -- LOW, NORMAL, HIGH, URGENT
    CreatedDate DATETIME DEFAULT GETDATE(),
    CreatedBy VARCHAR(50),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO
