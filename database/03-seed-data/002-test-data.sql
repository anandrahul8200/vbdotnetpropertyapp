-- ============================================================
-- TEST DATA GENERATION
-- Generates 10K+ rows of realistic insurance business data
-- ============================================================
USE PropertyInsuranceDB;
GO

SET NOCOUNT ON;
PRINT 'Starting test data generation...';
PRINT '================================';

-- ============================================================
-- AGENTS & AGENCIES (50 agents, 10 agencies)
-- ============================================================
PRINT 'Creating agencies and agents...';

INSERT INTO Policy.Agencies (AgencyCode, AgencyName, AddressLine1, City, StateCode, ZipCode, Phone, IsActive, CreatedDate)
VALUES 
('AGY001', 'Sunshine Insurance Group', '100 Main St', 'Tampa', 'FL', '33601', '813-555-0001', 1, GETDATE()),
('AGY002', 'Coastal Coverage Agency', '200 Beach Rd', 'Miami', 'FL', '33101', '305-555-0002', 1, GETDATE()),
('AGY003', 'Palmetto Insurance Partners', '300 Palm Ave', 'Orlando', 'FL', '32801', '407-555-0003', 1, GETDATE()),
('AGY004', 'Gulf State Underwriters', '400 Gulf Blvd', 'St Petersburg', 'FL', '33701', '727-555-0004', 1, GETDATE()),
('AGY005', 'Atlantic Risk Advisors', '500 Ocean Dr', 'Jacksonville', 'FL', '32099', '904-555-0005', 1, GETDATE()),
('AGY006', 'Southern Shield Insurance', '600 Peach St', 'Atlanta', 'GA', '30301', '404-555-0006', 1, GETDATE()),
('AGY007', 'Magnolia Insurance Group', '700 Magnolia Ln', 'Savannah', 'GA', '31401', '912-555-0007', 1, GETDATE()),
('AGY008', 'Lone Star Coverage', '800 Texas Ave', 'Houston', 'TX', '77001', '713-555-0008', 1, GETDATE()),
('AGY009', 'Bayou Insurance Partners', '900 Canal St', 'New Orleans', 'LA', '70112', '504-555-0009', 1, GETDATE()),
('AGY010', 'Carolina Risk Management', '1000 Trade St', 'Charlotte', 'NC', '28201', '704-555-0010', 1, GETDATE());

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    INSERT INTO Policy.Agents (AgentNumber, AgentType, FirstName, LastName, LicenseNumber, LicenseState, 
        AgencyID, CommissionRate, Email, Phone, AppointmentDate, IsActive, CreatedDate, ModifiedDate)
    VALUES (
        'AGT' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        CASE WHEN @i % 3 = 0 THEN 'INDEPENDENT' WHEN @i % 5 = 0 THEN 'BROKER' ELSE 'CAPTIVE' END,
        CASE @i % 10 WHEN 1 THEN 'James' WHEN 2 THEN 'Mary' WHEN 3 THEN 'David' WHEN 4 THEN 'Sarah' WHEN 5 THEN 'Michael'
             WHEN 6 THEN 'Jennifer' WHEN 7 THEN 'William' WHEN 8 THEN 'Linda' WHEN 9 THEN 'Richard' ELSE 'Patricia' END,
        CASE @i % 8 WHEN 1 THEN 'Anderson' WHEN 2 THEN 'Martinez' WHEN 3 THEN 'Thompson' WHEN 4 THEN 'Wilson'
             WHEN 5 THEN 'Taylor' WHEN 6 THEN 'Brown' WHEN 7 THEN 'Davis' ELSE 'Miller' END,
        'LIC' + RIGHT('00000' + CAST(10000 + @i AS VARCHAR), 6),
        CASE WHEN @i <= 25 THEN 'FL' WHEN @i <= 35 THEN 'GA' WHEN @i <= 45 THEN 'TX' ELSE 'NC' END,
        ((@i - 1) / 5) + 1,
        CASE WHEN @i % 4 = 0 THEN 0.12 WHEN @i % 3 = 0 THEN 0.08 ELSE 0.10 END,
        'agent' + CAST(@i AS VARCHAR) + '@insurance.com',
        '555-' + RIGHT('0000' + CAST(1000 + @i AS VARCHAR), 4),
        DATEADD(YEAR, -(@i % 10), GETDATE()),
        1, GETDATE(), GETDATE()
    );
    SET @i = @i + 1;
END;
PRINT '  50 agents created across 10 agencies';
GO

-- ============================================================
-- CUSTOMERS (500 customers)
-- ============================================================
PRINT 'Creating 500 customers...';

DECLARE @i INT = 1;
DECLARE @firstNames TABLE (ID INT, Name VARCHAR(50));
INSERT INTO @firstNames VALUES (1,'Robert'),(2,'Maria'),(3,'James'),(4,'Patricia'),(5,'John'),(6,'Jennifer'),(7,'Michael'),(8,'Linda'),(9,'David'),(10,'Elizabeth'),
(11,'William'),(12,'Barbara'),(13,'Richard'),(14,'Susan'),(15,'Joseph'),(16,'Jessica'),(17,'Thomas'),(18,'Sarah'),(19,'Charles'),(20,'Karen');

DECLARE @lastNames TABLE (ID INT, Name VARCHAR(50));
INSERT INTO @lastNames VALUES (1,'Smith'),(2,'Johnson'),(3,'Williams'),(4,'Brown'),(5,'Jones'),(6,'Garcia'),(7,'Miller'),(8,'Davis'),(9,'Rodriguez'),(10,'Martinez'),
(11,'Hernandez'),(12,'Lopez'),(13,'Gonzalez'),(14,'Wilson'),(15,'Anderson'),(16,'Thomas'),(17,'Taylor'),(18,'Moore'),(19,'Jackson'),(20,'Martin');

DECLARE @streets TABLE (ID INT, Name VARCHAR(100));
INSERT INTO @streets VALUES (1,'Oak'),(2,'Pine'),(3,'Maple'),(4,'Cedar'),(5,'Elm'),(6,'Birch'),(7,'Walnut'),(8,'Cherry'),(9,'Willow'),(10,'Cypress');

DECLARE @cities TABLE (ID INT, City VARCHAR(50), St CHAR(2), Zip VARCHAR(10));
INSERT INTO @cities VALUES (1,'Tampa','FL','33601'),(2,'Miami','FL','33101'),(3,'Orlando','FL','32801'),(4,'Jacksonville','FL','32099'),
(5,'St Petersburg','FL','33701'),(6,'Fort Lauderdale','FL','33301'),(7,'Tallahassee','FL','32301'),(8,'Atlanta','GA','30301'),
(9,'Houston','TX','77001'),(10,'Charlotte','NC','28201');

WHILE @i <= 500
BEGIN
    DECLARE @fn VARCHAR(50), @ln VARCHAR(50), @city VARCHAR(50), @st CHAR(2), @zip VARCHAR(10), @street VARCHAR(100);
    SELECT @fn = Name FROM @firstNames WHERE ID = ((@i - 1) % 20) + 1;
    SELECT @ln = Name FROM @lastNames WHERE ID = ((@i - 1) % 20) + 1;
    SELECT @city = City, @st = St, @zip = Zip FROM @cities WHERE ID = ((@i - 1) % 10) + 1;
    SELECT @street = Name FROM @streets WHERE ID = ((@i - 1) % 10) + 1;

    INSERT INTO Policy.Customers (CustomerNumber, CustomerType, FirstName, LastName, CompanyName,
        Email, Phone, AddressLine1, City, StateCode, ZipCode, 
        CreditScore, RiskTier, Occupation, CustomerSince, IsActive, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)
    VALUES (
        'CUS' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        CASE WHEN @i % 8 = 0 THEN 'C' ELSE 'I' END,
        CASE WHEN @i % 8 = 0 THEN NULL ELSE @fn END,
        CASE WHEN @i % 8 = 0 THEN NULL ELSE @ln END,
        CASE WHEN @i % 8 = 0 THEN @ln + ' ' + CASE @i % 4 WHEN 0 THEN 'Properties LLC' WHEN 1 THEN 'Investments Inc' WHEN 2 THEN 'Holdings Corp' ELSE 'Realty Group' END ELSE NULL END,
        LOWER(@fn) + '.' + LOWER(@ln) + CAST(@i AS VARCHAR) + '@email.com',
        '555-' + RIGHT('0000' + CAST(1000 + @i AS VARCHAR), 4),
        CAST(100 + (@i * 7) % 900 AS VARCHAR) + ' ' + @street + CASE @i % 4 WHEN 0 THEN ' Street' WHEN 1 THEN ' Avenue' WHEN 2 THEN ' Drive' ELSE ' Lane' END,
        @city, @st, @zip,
        600 + (@i * 3) % 200,
        CASE WHEN 600 + (@i * 3) % 200 >= 750 THEN 'PREFERRED' WHEN 600 + (@i * 3) % 200 >= 650 THEN 'STANDARD' ELSE 'SUBSTANDARD' END,
        CASE @i % 6 WHEN 0 THEN 'Engineer' WHEN 1 THEN 'Teacher' WHEN 2 THEN 'Doctor' WHEN 3 THEN 'Lawyer' WHEN 4 THEN 'Manager' ELSE 'Retired' END,
        DATEADD(DAY, -(@i * 3) % 3650, GETDATE()),
        1, GETDATE(), 'admin', GETDATE(), 'admin'
    );
    SET @i = @i + 1;
END;
PRINT '  500 customers created';
GO

-- ============================================================
-- PROPERTIES (500 properties)
-- ============================================================
PRINT 'Creating 500 properties...';

DECLARE @i INT = 1;
WHILE @i <= 500
BEGIN
    INSERT INTO Policy.Properties (CustomerID, PropertyNumber, PropertyType, ConstructionType, OccupancyType,
        YearBuilt, SquareFootage, NumberOfStories, RoofType, RoofAge,
        HasBasement, HasPool, HasFireAlarm, HasBurglarAlarm, HasSprinklerSystem,
        FireProtectionClass, MarketValue, ReplacementCost,
        AddressLine1, City, StateCode, ZipCode, IsActive, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)
    VALUES (
        @i,
        'PRP' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        CASE @i % 5 WHEN 0 THEN 'CONDO' WHEN 1 THEN 'SINGLE_FAMILY' WHEN 2 THEN 'SINGLE_FAMILY' WHEN 3 THEN 'TOWNHOUSE' ELSE 'MULTI_FAMILY' END,
        CASE @i % 4 WHEN 0 THEN 'MASONRY' WHEN 1 THEN 'FRAME' WHEN 2 THEN 'MASONRY_VENEER' ELSE 'FIRE_RESISTIVE' END,
        CASE @i % 3 WHEN 0 THEN 'OWNER_OCCUPIED' WHEN 1 THEN 'OWNER_OCCUPIED' ELSE 'TENANT_OCCUPIED' END,
        1960 + (@i % 60),
        1000 + (@i * 7) % 3000,
        CASE WHEN @i % 5 = 0 THEN 2 WHEN @i % 10 = 0 THEN 3 ELSE 1 END,
        CASE @i % 5 WHEN 0 THEN 'TILE' WHEN 1 THEN 'ASPHALT_SHINGLE' WHEN 2 THEN 'METAL' WHEN 3 THEN 'ASPHALT_SHINGLE' ELSE 'SLATE' END,
        (@i % 30) + 1,
        CASE WHEN @i % 4 = 0 THEN 1 ELSE 0 END,
        CASE WHEN @i % 6 = 0 THEN 1 ELSE 0 END,
        CASE WHEN @i % 2 = 0 THEN 1 ELSE 0 END,
        CASE WHEN @i % 3 = 0 THEN 1 ELSE 0 END,
        CASE WHEN @i % 5 = 0 THEN 1 ELSE 0 END,
        (@i % 9) + 1,
        150000 + (@i * 500) % 500000,
        180000 + (@i * 600) % 600000,
        (SELECT AddressLine1 FROM Policy.Customers WHERE CustomerID = @i),
        (SELECT City FROM Policy.Customers WHERE CustomerID = @i),
        (SELECT StateCode FROM Policy.Customers WHERE CustomerID = @i),
        (SELECT ZipCode FROM Policy.Customers WHERE CustomerID = @i),
        1, GETDATE(), 'admin', GETDATE(), 'admin'
    );
    SET @i = @i + 1;
END;
PRINT '  500 properties created';
GO

-- ============================================================
-- POLICIES (600 policies)
-- ============================================================
PRINT 'Creating 600 policies...';

DECLARE @i INT = 1;
WHILE @i <= 600
BEGIN
    DECLARE @custID INT = ((@i - 1) % 500) + 1;
    DECLARE @effDate DATE = DATEADD(DAY, -(@i * 2) % 730, GETDATE());
    DECLARE @expDate DATE = DATEADD(YEAR, 1, @effDate);
    DECLARE @status VARCHAR(20) = CASE 
        WHEN @i <= 400 THEN 'ACTIVE'
        WHEN @i <= 450 THEN 'EXPIRED'
        WHEN @i <= 500 THEN 'CANCELLED'
        WHEN @i <= 550 THEN 'QUOTE'
        ELSE 'BOUND'
    END;
    DECLARE @premium DECIMAL(18,2) = 800 + (@i * 13) % 3000;

    INSERT INTO Policy.Policies (PolicyNumber, PolicyVersion, PolicyType, PolicyStatus,
        CustomerID, PropertyID, AgentID, EffectiveDate, ExpiryDate, OriginalEffectiveDate, TermMonths,
        PaymentPlan, BillingMethod, CommissionRate, AnnualPremium, WrittenPremium, TotalTaxes, GrossPremium,
        ClaimFreeYears, IsRenewal, RenewalCount, CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)
    VALUES (
        'POL' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        1,
        CASE @i % 4 WHEN 0 THEN 'HO4' WHEN 1 THEN 'HO3' WHEN 2 THEN 'HO3' ELSE 'HO6' END,
        @status,
        @custID,
        @custID,
        ((@i - 1) % 50) + 1,
        @effDate, @expDate, @effDate, 12,
        CASE @i % 3 WHEN 0 THEN 'QUARTERLY' WHEN 1 THEN 'ANNUAL' ELSE 'MONTHLY' END,
        'DIRECT', 0.10, @premium, @premium, ROUND(@premium * 0.035, 2), ROUND(@premium * 1.035, 2),
        @i % 8, CASE WHEN @i % 4 = 0 THEN 1 ELSE 0 END, @i % 4,
        GETDATE(), 'admin', GETDATE(), 'admin'
    );
    SET @i = @i + 1;
END;
PRINT '  600 policies created';
GO

-- ============================================================
-- CLAIMS (300 claims)
-- ============================================================
PRINT 'Creating 300 claims...';

DECLARE @i INT = 1;
WHILE @i <= 300
BEGIN
    DECLARE @policyID INT = ((@i - 1) % 400) + 1;
    DECLARE @custID2 INT, @propID2 INT;
    SELECT @custID2 = CustomerID, @propID2 = PropertyID FROM Policy.Policies WHERE PolicyID = @policyID;
    
    DECLARE @lossDate DATETIME = DATEADD(DAY, -(@i * 3) % 365, GETDATE());
    DECLARE @claimStatus VARCHAR(20) = CASE 
        WHEN @i <= 50 THEN 'FNOL'
        WHEN @i <= 100 THEN 'ASSIGNED'
        WHEN @i <= 150 THEN 'INVESTIGATING'
        WHEN @i <= 180 THEN 'ASSESSED'
        WHEN @i <= 200 THEN 'APPROVED'
        WHEN @i <= 230 THEN 'SETTLED'
        WHEN @i <= 270 THEN 'CLOSED'
        WHEN @i <= 285 THEN 'DENIED'
        ELSE 'LITIGATION'
    END;
    DECLARE @estLoss DECIMAL(18,2) = 2000 + (@i * 37) % 80000;
    DECLARE @reserve DECIMAL(18,2) = @estLoss * 0.8;
    DECLARE @paid DECIMAL(18,2) = CASE WHEN @claimStatus IN ('SETTLED','CLOSED','APPROVED') THEN @estLoss * 0.7 ELSE 0 END;

    INSERT INTO Claims.Claims (ClaimNumber, PolicyID, CustomerID, PropertyID, ClaimStatus, ClaimType,
        LossDate, ReportedDate, LossDescription, EstimatedLoss, DeductibleAmount, PolicyLimit,
        TotalPaid, TotalReserve, NetIncurred, AdjusterID, Priority, Complexity, FraudScore,
        CreatedDate, CreatedBy, ModifiedDate, ModifiedBy)
    VALUES (
        'CLM' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        @policyID, @custID2, @propID2, @claimStatus,
        CASE @i % 7 WHEN 0 THEN 'FIRE' WHEN 1 THEN 'WATER_DAMAGE' WHEN 2 THEN 'WIND' WHEN 3 THEN 'HAIL' 
             WHEN 4 THEN 'THEFT' WHEN 5 THEN 'PROPERTY_DAMAGE' ELSE 'LIABILITY' END,
        @lossDate, DATEADD(DAY, @i % 5, @lossDate),
        CASE @i % 5 WHEN 0 THEN 'Water pipe burst in kitchen causing flooding' 
             WHEN 1 THEN 'Wind damage to roof during storm' 
             WHEN 2 THEN 'Fire started in garage, spread to main structure'
             WHEN 3 THEN 'Hail damage to roof and siding'
             ELSE 'Theft of personal property during break-in' END,
        @estLoss, 1000 + (@i % 4) * 500, 250000 + (@i % 5) * 50000,
        @paid, @reserve, @reserve + @paid,
        ((@i - 1) % 50) + 1,
        CASE @i % 4 WHEN 0 THEN 'HIGH' WHEN 1 THEN 'NORMAL' WHEN 2 THEN 'NORMAL' ELSE 'LOW' END,
        CASE WHEN @estLoss > 50000 THEN 'COMPLEX' WHEN @estLoss > 20000 THEN 'MODERATE' ELSE 'SIMPLE' END,
        (@i * 7) % 100,
        GETDATE(), 'admin', GETDATE(), 'admin'
    );
    SET @i = @i + 1;
END;
PRINT '  300 claims created';
GO

-- ============================================================
-- RESERVES (600 reserve records)
-- ============================================================
PRINT 'Creating reserves...';

DECLARE @i INT = 1;
WHILE @i <= 300
BEGIN
    INSERT INTO Claims.Reserves (ClaimID, ReserveType, ReserveCategory, Amount, PreviousAmount, ChangeAmount, SetBy, IsApproved, CreatedDate)
    VALUES (@i, 'CASE', 'INDEMNITY', 
        (SELECT ISNULL(TotalReserve, 5000) FROM Claims.Claims WHERE ClaimID = @i),
        0, (SELECT ISNULL(TotalReserve, 5000) FROM Claims.Claims WHERE ClaimID = @i),
        'admin', 1, GETDATE());
    
    -- Add expense reserve for half
    IF @i % 2 = 0
        INSERT INTO Claims.Reserves (ClaimID, ReserveType, ReserveCategory, Amount, PreviousAmount, ChangeAmount, SetBy, IsApproved, CreatedDate)
        VALUES (@i, 'EXPENSE', 'ADJUSTMENT_EXPENSE', 2000 + (@i % 3000), 0, 2000 + (@i % 3000), 'admin', 1, GETDATE());
    
    SET @i = @i + 1;
END;
PRINT '  600 reserve records created';
GO

-- ============================================================
-- CLAIM ACTIVITIES (1500 activities)
-- ============================================================
PRINT 'Creating claim activities...';

DECLARE @i INT = 1;
WHILE @i <= 300
BEGIN
    -- Initial FNOL note
    INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, IsCompleted, CreatedBy, CreatedDate)
    VALUES (@i, 'NOTE', DATEADD(DAY, -(@i % 30), GETDATE()), 'FNOL Received', 'First notice of loss received and logged.', 1, 'admin', GETDATE());
    
    -- Phone call
    INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, ContactName, Duration, IsCompleted, CreatedBy, CreatedDate)
    VALUES (@i, 'PHONE_CALL', DATEADD(DAY, -(@i % 25), GETDATE()), 'Contact insured', 'Called insured to discuss claim details.', 'Insured', 15, 1, 'admin', GETDATE());
    
    -- Inspection for some
    IF @i % 3 = 0
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, Subject, Description, IsCompleted, CreatedBy, CreatedDate)
        VALUES (@i, 'INSPECTION', DATEADD(DAY, -(@i % 20), GETDATE()), 'Property inspection', 'On-site inspection completed. Damage documented.', 1, 'admin', GETDATE());
    
    -- Pending activity for open claims
    IF @i <= 150
        INSERT INTO Claims.Activities (ClaimID, ActivityType, ActivityDate, DueDate, Subject, Description, IsCompleted, AssignedTo, Priority, CreatedBy, CreatedDate)
        VALUES (@i, 'NOTE', GETDATE(), DATEADD(DAY, 7, GETDATE()), 'Follow up required', 'Need to follow up with insured on documentation.', 0, 'admin', 'NORMAL', 'admin', GETDATE());
    
    SET @i = @i + 1;
END;
PRINT '  1500+ activities created';
GO

-- ============================================================
-- INVOICES & PAYMENTS (1200 invoices, 800 payments)
-- ============================================================
PRINT 'Creating billing data...';

DECLARE @i INT = 1;
WHILE @i <= 600
BEGIN
    DECLARE @polPremium DECIMAL(18,2);
    DECLARE @polCustID INT;
    SELECT @polPremium = ISNULL(GrossPremium, 1500), @polCustID = CustomerID FROM Policy.Policies WHERE PolicyID = @i;
    
    -- Create invoice
    INSERT INTO Billing.Invoices (InvoiceNumber, PolicyID, CustomerID, InvoiceType, InvoiceDate, DueDate,
        PremiumAmount, TaxAmount, TotalAmount, PaidAmount, BalanceDue, Status, InstallmentNumber, TotalInstallments,
        CreatedDate, CreatedBy)
    VALUES (
        'INV' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7),
        @i, @polCustID, 
        CASE WHEN @i % 4 = 0 THEN 'RENEWAL' ELSE 'NEW_BUSINESS' END,
        DATEADD(DAY, -(@i * 2) % 365, GETDATE()),
        DATEADD(DAY, -(@i * 2) % 365 + 30, GETDATE()),
        @polPremium * 0.97, @polPremium * 0.03, @polPremium,
        CASE WHEN @i <= 400 THEN @polPremium ELSE @polPremium * 0.5 END,
        CASE WHEN @i <= 400 THEN 0 ELSE @polPremium * 0.5 END,
        CASE WHEN @i <= 400 THEN 'PAID' WHEN @i <= 500 THEN 'PARTIAL' ELSE 'OPEN' END,
        1, 1, GETDATE(), 'admin'
    );
    
    -- Create payment for paid invoices
    IF @i <= 400
        INSERT INTO Billing.PremiumPayments (PaymentNumber, InvoiceID, PolicyID, CustomerID, PaymentDate, Amount, PaymentMethod, Status, AppliedToInvoice, CreatedDate, CreatedBy)
        VALUES ('PMP' + RIGHT('0000000' + CAST(@i AS VARCHAR), 7), @i, @i, @polCustID,
            DATEADD(DAY, -(@i * 2) % 365 + 15, GETDATE()), @polPremium,
            CASE @i % 3 WHEN 0 THEN 'CHECK' WHEN 1 THEN 'EFT' ELSE 'CREDIT_CARD' END,
            'APPLIED', 1, GETDATE(), 'admin');
    
    SET @i = @i + 1;
END;
PRINT '  600 invoices, 400 payments created';
GO

-- ============================================================
-- STATUS HISTORY (900 records)
-- ============================================================
PRINT 'Creating claim status history...';

DECLARE @i INT = 1;
WHILE @i <= 300
BEGIN
    INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
    VALUES (@i, NULL, 'FNOL', DATEADD(DAY, -30, GETDATE()), 'admin', 'First Notice of Loss received');
    
    IF @i > 50
        INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
        VALUES (@i, 'FNOL', 'ASSIGNED', DATEADD(DAY, -25, GETDATE()), 'admin', 'Adjuster assigned');
    
    IF @i > 100
        INSERT INTO Claims.StatusHistory (ClaimID, PreviousStatus, NewStatus, ChangeDate, ChangedBy, Reason)
        VALUES (@i, 'ASSIGNED', 'INVESTIGATING', DATEADD(DAY, -20, GETDATE()), 'admin', 'Investigation started');
    
    SET @i = @i + 1;
END;
PRINT '  900 status history records created';
GO

-- ============================================================
-- FINAL COUNTS
-- ============================================================
PRINT '';
PRINT '================================';
PRINT 'Test data generation complete!';
PRINT '================================';
SELECT 'Customers' AS Entity, COUNT(*) AS Records FROM Policy.Customers
UNION ALL SELECT 'Properties', COUNT(*) FROM Policy.Properties
UNION ALL SELECT 'Agents', COUNT(*) FROM Policy.Agents
UNION ALL SELECT 'Policies', COUNT(*) FROM Policy.Policies
UNION ALL SELECT 'Claims', COUNT(*) FROM Claims.Claims
UNION ALL SELECT 'Reserves', COUNT(*) FROM Claims.Reserves
UNION ALL SELECT 'Activities', COUNT(*) FROM Claims.Activities
UNION ALL SELECT 'Invoices', COUNT(*) FROM Billing.Invoices
UNION ALL SELECT 'Payments', COUNT(*) FROM Billing.PremiumPayments
UNION ALL SELECT 'StatusHistory', COUNT(*) FROM Claims.StatusHistory;
GO
