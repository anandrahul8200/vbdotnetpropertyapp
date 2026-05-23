-- ============================================================
-- ADMIN / LOOKUP TABLES
-- ============================================================
USE PropertyInsuranceDB;
GO

-- System Configuration
CREATE TABLE Admin.SystemConfig (
    ConfigID INT IDENTITY(1,1) PRIMARY KEY,
    ConfigKey VARCHAR(100) NOT NULL UNIQUE,
    ConfigValue VARCHAR(500) NOT NULL,
    Description VARCHAR(500),
    DataType VARCHAR(20) DEFAULT 'STRING',
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy VARCHAR(50)
);
GO

-- Users
CREATE TABLE Admin.Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(256) NOT NULL,
    Salt VARCHAR(128) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(200),
    Phone VARCHAR(20),
    RoleID INT NOT NULL,
    DepartmentID INT,
    SupervisorID INT,
    IsActive BIT DEFAULT 1,
    IsLocked BIT DEFAULT 0,
    FailedLoginAttempts INT DEFAULT 0,
    LastLoginDate DATETIME,
    PasswordExpiryDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);
GO

-- Roles
CREATE TABLE Admin.Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName VARCHAR(50) NOT NULL UNIQUE,
    Description VARCHAR(200),
    MaxClaimApprovalAmount DECIMAL(18,2),
    CanApproveUnderwriting BIT DEFAULT 0,
    CanProcessPayments BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Departments
CREATE TABLE Admin.Departments (
    DepartmentID INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    DepartmentCode VARCHAR(10) NOT NULL UNIQUE,
    ManagerID INT,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Permissions
CREATE TABLE Admin.Permissions (
    PermissionID INT IDENTITY(1,1) PRIMARY KEY,
    PermissionCode VARCHAR(30) NOT NULL UNIQUE,
    PermissionName VARCHAR(100) NOT NULL,
    Module VARCHAR(50) NOT NULL,
    Description VARCHAR(200),
    IsActive BIT DEFAULT 1
);
GO

-- Role-Permission Mapping
CREATE TABLE Admin.RolePermissions (
    RolePermissionID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL REFERENCES Admin.Roles(RoleID),
    PermissionID INT NOT NULL REFERENCES Admin.Permissions(PermissionID),
    CanRead BIT DEFAULT 1,
    CanWrite BIT DEFAULT 0,
    CanDelete BIT DEFAULT 0,
    CanApprove BIT DEFAULT 0,
    UNIQUE(RoleID, PermissionID)
);
GO

-- Lookup Categories
CREATE TABLE Admin.LookupCategories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(200),
    IsSystem BIT DEFAULT 0,
    IsActive BIT DEFAULT 1
);
GO

-- Lookup Values
CREATE TABLE Admin.LookupValues (
    LookupID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT NOT NULL REFERENCES Admin.LookupCategories(CategoryID),
    LookupCode VARCHAR(50) NOT NULL,
    LookupValue VARCHAR(200) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    ParentLookupID INT,
    IsDefault BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    EffectiveDate DATE DEFAULT GETDATE(),
    ExpiryDate DATE DEFAULT '9999-12-31',
    UNIQUE(CategoryID, LookupCode)
);
GO

-- States/Provinces (for regulatory calculations)
CREATE TABLE Admin.States (
    StateID INT IDENTITY(1,1) PRIMARY KEY,
    StateCode CHAR(2) NOT NULL UNIQUE,
    StateName VARCHAR(100) NOT NULL,
    TaxRate DECIMAL(6,4) DEFAULT 0,
    SurchargeRate DECIMAL(6,4) DEFAULT 0,
    StampingFeeRate DECIMAL(6,4) DEFAULT 0,
    FireMarshalRate DECIMAL(6,4) DEFAULT 0,
    WindpoolEligible BIT DEFAULT 0,
    EarthquakeZone INT DEFAULT 0,
    FloodZoneRequired BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    EffectiveDate DATE DEFAULT GETDATE(),
    ExpiryDate DATE DEFAULT '9999-12-31'
);
GO

-- Audit Log
CREATE TABLE Audit.AuditLog (
    AuditID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName VARCHAR(100) NOT NULL,
    RecordID INT NOT NULL,
    Action VARCHAR(10) NOT NULL, -- INSERT, UPDATE, DELETE
    FieldName VARCHAR(100),
    OldValue VARCHAR(MAX),
    NewValue VARCHAR(MAX),
    UserID INT,
    Username VARCHAR(50),
    ActionDate DATETIME DEFAULT GETDATE(),
    IPAddress VARCHAR(50),
    MachineName VARCHAR(100)
);
GO

-- Error Log
CREATE TABLE Audit.ErrorLog (
    ErrorID BIGINT IDENTITY(1,1) PRIMARY KEY,
    ErrorDate DATETIME DEFAULT GETDATE(),
    ErrorNumber INT,
    ErrorSeverity INT,
    ErrorState INT,
    ErrorProcedure VARCHAR(200),
    ErrorLine INT,
    ErrorMessage VARCHAR(MAX),
    UserID INT,
    AdditionalInfo VARCHAR(MAX)
);
GO

-- Batch Job Log
CREATE TABLE Batch.JobLog (
    JobLogID BIGINT IDENTITY(1,1) PRIMARY KEY,
    JobName VARCHAR(100) NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    Status VARCHAR(20) DEFAULT 'RUNNING', -- RUNNING, COMPLETED, FAILED, CANCELLED
    RecordsProcessed INT DEFAULT 0,
    RecordsFailed INT DEFAULT 0,
    ErrorMessage VARCHAR(MAX),
    Parameters VARCHAR(MAX),
    RunBy VARCHAR(50)
);
GO
