-- ============================================================
-- Property Insurance Claims Management System
-- Database Creation Script
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'PropertyInsuranceDB')
BEGIN
    ALTER DATABASE PropertyInsuranceDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE PropertyInsuranceDB;
END
GO

CREATE DATABASE PropertyInsuranceDB;
GO

USE PropertyInsuranceDB;
GO

-- ============================================================
-- SCHEMA DEFINITIONS
-- ============================================================
CREATE SCHEMA Policy AUTHORIZATION dbo;
GO
CREATE SCHEMA Claims AUTHORIZATION dbo;
GO
CREATE SCHEMA Underwriting AUTHORIZATION dbo;
GO
CREATE SCHEMA Billing AUTHORIZATION dbo;
GO
CREATE SCHEMA Reinsurance AUTHORIZATION dbo;
GO
CREATE SCHEMA Admin AUTHORIZATION dbo;
GO
CREATE SCHEMA Batch AUTHORIZATION dbo;
GO
CREATE SCHEMA Audit AUTHORIZATION dbo;
GO
