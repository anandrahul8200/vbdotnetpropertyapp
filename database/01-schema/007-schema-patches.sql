-- ============================================================
-- SCHEMA PATCHES
-- Adds columns referenced by SPs but missing from base schema
-- ============================================================
USE PropertyInsuranceDB;
GO

-- Add AdditionalInfo to AuditLog
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Audit.AuditLog') AND name = 'AdditionalInfo')
    ALTER TABLE Audit.AuditLog ADD AdditionalInfo VARCHAR(MAX) NULL;
GO

-- Add LockedDate to Users
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Admin.Users') AND name = 'LockedDate')
    ALTER TABLE Admin.Users ADD LockedDate DATETIME NULL;
GO

-- Add Department to Users (string version for SP compatibility)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Admin.Users') AND name = 'Department')
    ALTER TABLE Admin.Users ADD Department VARCHAR(50) NULL;
GO

-- Add CreatedBy to Users
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Admin.Users') AND name = 'CreatedBy')
    ALTER TABLE Admin.Users ADD CreatedBy VARCHAR(50) NULL;
GO

-- Add StateCode to Agents (for search)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Agents') AND name = 'StateCode')
    ALTER TABLE Policy.Agents ADD StateCode CHAR(2) NULL;
GO

-- Add AdditionalInfo to ErrorLog
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Audit.ErrorLog') AND name = 'AdditionalInfo')
    ALTER TABLE Audit.ErrorLog ADD AdditionalInfo VARCHAR(MAX) NULL;
GO

-- Add ErrorDate to ErrorLog (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Audit.ErrorLog') AND name = 'ErrorDate')
    ALTER TABLE Audit.ErrorLog ADD ErrorDate DATETIME DEFAULT GETDATE();
GO

-- Add CoverageName to Coverages (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Coverages') AND name = 'CoverageName')
    ALTER TABLE Policy.Coverages ADD CoverageName VARCHAR(100) NULL;
GO

-- Add MultiPolicyDiscount to Policies (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Policies') AND name = 'MultiPolicyDiscount')
    ALTER TABLE Policy.Policies ADD MultiPolicyDiscount BIT DEFAULT 0;
GO

-- Add TotalInsuredValue to Policies (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Policies') AND name = 'TotalInsuredValue')
    ALTER TABLE Policy.Policies ADD TotalInsuredValue DECIMAL(18,2) NULL;
GO

-- Add GrossPremium to Policies (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Policies') AND name = 'GrossPremium')
    ALTER TABLE Policy.Policies ADD GrossPremium DECIMAL(18,2) NULL;
GO

-- Add TotalFees to Policies (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Policies') AND name = 'TotalFees')
    ALTER TABLE Policy.Policies ADD TotalFees DECIMAL(18,2) DEFAULT 0;
GO

-- Add TotalSurcharges to Policies (if missing)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Policy.Policies') AND name = 'TotalSurcharges')
    ALTER TABLE Policy.Policies ADD TotalSurcharges DECIMAL(18,2) DEFAULT 0;
GO

-- Create Holidays table if not exists (used by fraud scoring)
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('Admin.Holidays') AND type = 'U')
BEGIN
    CREATE TABLE Admin.Holidays (
        HolidayID INT IDENTITY(1,1) PRIMARY KEY,
        HolidayDate DATE NOT NULL UNIQUE,
        HolidayName VARCHAR(100) NOT NULL
    );
END
GO

-- Create Notes table if not exists
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('Policy.Notes') AND type = 'U')
BEGIN
    CREATE TABLE Policy.Notes (
        NoteID INT IDENTITY(1,1) PRIMARY KEY,
        PolicyID INT NOT NULL REFERENCES Policy.Policies(PolicyID),
        NoteType VARCHAR(20) DEFAULT 'GENERAL',
        NoteText VARCHAR(MAX),
        Priority VARCHAR(10) DEFAULT 'NORMAL',
        CreatedDate DATETIME DEFAULT GETDATE(),
        CreatedBy VARCHAR(50)
    );
END
GO

-- Add UserRoles table if structure differs (SP expects separate table)
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('Admin.UserRoles') AND type = 'U')
BEGIN
    CREATE TABLE Admin.UserRoles (
        UserRoleID INT IDENTITY(1,1) PRIMARY KEY,
        UserID INT NOT NULL REFERENCES Admin.Users(UserID),
        RoleID INT NOT NULL REFERENCES Admin.Roles(RoleID),
        AssignedDate DATETIME DEFAULT GETDATE(),
        AssignedBy VARCHAR(50),
        UNIQUE(UserID, RoleID)
    );
END
GO

PRINT 'Schema patches applied successfully.';
GO

-- Add Category to SystemConfig
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Admin.SystemConfig') AND name = 'Category')
    ALTER TABLE Admin.SystemConfig ADD Category VARCHAR(50) NULL;
GO

PRINT 'Additional patches applied.';
GO
