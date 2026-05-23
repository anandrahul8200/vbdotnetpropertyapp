# Policy Module - Stored Procedure Tests

## Module: POL (Policy)
## Source Files:
- `database/02-stored-procedures/001-policy-crud-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (23 total):
1. Policy.usp_Customer_Create
2. Policy.usp_Customer_Update
3. Policy.usp_Customer_GetByID
4. Policy.usp_Customer_Search
5. Policy.usp_Property_Create
6. Policy.usp_Property_GetByCustomer
7. Policy.usp_Property_GetByID
8. Policy.usp_Policy_CreateQuote
9. Policy.usp_Policy_GetDetails
10. Policy.usp_Policy_Search
11. Policy.usp_Policy_Bind
12. Policy.usp_Policy_Cancel
13. Policy.usp_Policy_Reinstate
14. Policy.usp_Policy_SaveCoverage
15. Policy.usp_Policy_UpdatePremium
16. Policy.usp_Agent_Search
17. Policy.usp_Agent_GetByID
18. Policy.usp_Agent_Update
19. Policy.usp_Agent_GetProduction
20. Policy.usp_Document_Create
21. Policy.usp_Document_Delete
22. Policy.usp_Document_GetByEntity
23. Policy.usp_Document_Search

---

### Test Case ID: SP-POL-001
**Procedure**: Policy.usp_Customer_Create
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @CustomerType CHAR(1) - Required
- @Title VARCHAR(10) - Optional
- @FirstName VARCHAR(100) - Optional
- @LastName VARCHAR(100) - Optional
- @CompanyName VARCHAR(200) - Optional
- @TaxID VARCHAR(20) - Optional
- @DateOfBirth DATE - Optional
- @Gender CHAR(1) - Optional
- @Email VARCHAR(200) - Optional
- @Phone VARCHAR(20) - Optional
- @MobilePhone VARCHAR(20) - Optional
- @AddressLine1 VARCHAR(200) - Required
- @AddressLine2 VARCHAR(200) - Optional
- @City VARCHAR(100) - Required
- @StateCode CHAR(2) - Required
- @ZipCode VARCHAR(10) - Required
- @County VARCHAR(100) - Optional
- @Occupation VARCHAR(100) - Optional
- @AnnualIncome DECIMAL(18,2) - Optional
- @CreditScore INT - Optional
- @CreatedBy VARCHAR(50) - Required
- @CustomerID INT OUTPUT
- @CustomerNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create individual customer with all fields | @CustomerType='I', @FirstName='John', @LastName='Doe', @AddressLine1='123 Main St', @City='Austin', @StateCode='TX', @ZipCode='78701', @CreatedBy='testuser' | CustomerID > 0, CustomerNumber starts with 'CUS' |
| 2 | Create commercial customer | @CustomerType='C', @CompanyName='Acme Corp', @AddressLine1='456 Oak Ave', @City='Dallas', @StateCode='TX', @ZipCode='75201', @CreatedBy='testuser' | CustomerID > 0, CustomerNumber format CUS+7digits |
| 3 | Create customer with credit score >= 750 | @CustomerType='I', @FirstName='Jane', @LastName='Smith', @CreditScore=780, @AddressLine1='789 Elm', @City='Houston', @StateCode='TX', @ZipCode='77001', @CreatedBy='testuser' | RiskTier = 'PREFERRED' |
| 4 | Create customer with credit score 650-749 | Same as above with @CreditScore=700 | RiskTier = 'STANDARD' |
| 5 | Create customer with credit score < 650 | Same as above with @CreditScore=600 | RiskTier = 'SUBSTANDARD' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL required field AddressLine1 | @CustomerType='I', @AddressLine1=NULL, @City='Austin', @StateCode='TX', @ZipCode='78701', @CreatedBy='testuser' | Cannot insert NULL into non-nullable column |
| 2 | Invalid CustomerType value | @CustomerType='X' | Check constraint violation |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Maximum length FirstName (100 chars) | @FirstName='A' repeated 100 times | Accepted successfully |
| 2 | FirstName exceeds 100 chars | @FirstName='A' repeated 101 times | String truncation or error |
| 3 | NULL CreditScore | @CreditScore=NULL | RiskTier defaults to 'STANDARD' |
| 4 | CreditScore = 0 | @CreditScore=0 | RiskTier = 'SUBSTANDARD' |
| 5 | CreditScore = 850 (max) | @CreditScore=850 | RiskTier = 'PREFERRED' |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Audit log created | AuditLog table accessible | Valid customer data | Row inserted in Audit.AuditLog with Action='INSERT' |
| 2 | CustomerNumber uniqueness | No prior customers | Valid data | Unique CustomerNumber generated |

#### Executable SQL Script
```sql
-- Setup
DECLARE @CustID INT, @CustNum VARCHAR(20);

-- Test: Create individual customer
EXEC Policy.usp_Customer_Create
    @CustomerType = 'I', @FirstName = 'Test', @LastName = 'User',
    @AddressLine1 = '123 Test St', @City = 'Austin', @StateCode = 'TX',
    @ZipCode = '78701', @CreditScore = 780, @CreatedBy = 'unittest',
    @CustomerID = @CustID OUTPUT, @CustomerNumber = @CustNum OUTPUT;
-- Expected: @CustID > 0, @CustNum like 'CUS%', RiskTier = 'PREFERRED'
SELECT @CustID AS CustomerID, @CustNum AS CustomerNumber;
SELECT RiskTier FROM Policy.Customers WHERE CustomerID = @CustID;

-- Cleanup
DELETE FROM Audit.AuditLog WHERE RecordID = @CustID AND TableName = 'Policy.Customers';
DELETE FROM Policy.Customers WHERE CustomerID = @CustID;
```

---

### Test Case ID: SP-POL-002
**Procedure**: Policy.usp_Customer_Update
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @CustomerID INT - Required
- @Title VARCHAR(10) - Optional
- @FirstName VARCHAR(100) - Optional
- @LastName VARCHAR(100) - Optional
- @CompanyName VARCHAR(200) - Optional
- @Email VARCHAR(200) - Optional
- @Phone VARCHAR(20) - Optional
- @MobilePhone VARCHAR(20) - Optional
- @AddressLine1 VARCHAR(200) - Optional
- @AddressLine2 VARCHAR(200) - Optional
- @City VARCHAR(100) - Optional
- @StateCode CHAR(2) - Optional
- @ZipCode VARCHAR(10) - Optional
- @County VARCHAR(100) - Optional
- @Occupation VARCHAR(100) - Optional
- @AnnualIncome DECIMAL(18,2) - Optional
- @CreditScore INT - Optional
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update email only | @CustomerID=1, @Email='new@email.com', @ModifiedBy='testuser' | Email updated, other fields unchanged |
| 2 | Update address fields | @CustomerID=1, @AddressLine1='999 New St', @City='Dallas', @StateCode='TX', @ZipCode='75201', @ModifiedBy='testuser' | Address updated |
| 3 | Update with NULL optional fields | @CustomerID=1, @Email=NULL, @ModifiedBy='testuser' | Existing email preserved (ISNULL logic) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent customer | @CustomerID=999999, @Email='test@test.com', @ModifiedBy='testuser' | RAISERROR: 'Customer not found: 999999' |
| 2 | NULL CustomerID | @CustomerID=NULL, @ModifiedBy='testuser' | Parameter validation error |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Update all fields simultaneously | All optional fields provided | All fields updated in single call |
| 2 | CustomerID = 0 | @CustomerID=0, @ModifiedBy='testuser' | 'Customer not found: 0' |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Audit trail for changes | Customer exists with Email='old@test.com' | @Email='new@test.com' | Audit.AuditLog records old/new values |
| 2 | Customer must exist | Customer with given ID exists | Valid update | Update succeeds |

#### Executable SQL Script
```sql
-- Setup
DECLARE @CustID INT, @CustNum VARCHAR(20);
EXEC Policy.usp_Customer_Create @CustomerType='I', @FirstName='Test', @LastName='Update',
    @AddressLine1='100 Old St', @City='Austin', @StateCode='TX', @ZipCode='78701',
    @Email='old@test.com', @CreatedBy='unittest',
    @CustomerID=@CustID OUTPUT, @CustomerNumber=@CustNum OUTPUT;

-- Test: Update email
EXEC Policy.usp_Customer_Update @CustomerID=@CustID, @Email='new@test.com', @ModifiedBy='unittest';
SELECT Email FROM Policy.Customers WHERE CustomerID = @CustID;
-- Expected: Email = 'new@test.com'

-- Test: Non-existent customer
BEGIN TRY
    EXEC Policy.usp_Customer_Update @CustomerID=999999, @Email='x@y.com', @ModifiedBy='unittest';
END TRY
BEGIN CATCH
    SELECT ERROR_MESSAGE(); -- Expected: 'Customer not found: 999999'
END CATCH

-- Cleanup
DELETE FROM Audit.AuditLog WHERE RecordID = @CustID AND TableName = 'Policy.Customers';
DELETE FROM Policy.Customers WHERE CustomerID = @CustID;
```

---

### Test Case ID: SP-POL-003
**Procedure**: Policy.usp_Customer_GetByID
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @CustomerID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get existing customer | @CustomerID=1 | Returns customer row with ActivePolicyCount, OpenClaimCount, StateName |
| 2 | Returns computed columns | @CustomerID=1 | ActivePolicyCount and OpenClaimCount populated from sub-queries |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent customer | @CustomerID=999999 | Empty result set (0 rows) |
| 2 | CustomerID = NULL | @CustomerID=NULL | Empty result set or parameter error |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | CustomerID = 0 | @CustomerID=0 | Empty result set |
| 2 | Negative ID | @CustomerID=-1 | Empty result set |

---

### Test Case ID: SP-POL-004
**Procedure**: Policy.usp_Customer_Search
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @SearchTerm VARCHAR(100) - Optional
- @CustomerNumber VARCHAR(20) - Optional
- @CustomerType CHAR(1) - Optional
- @StateCode CHAR(2) - Optional
- @City VARCHAR(100) - Optional
- @ZipCode VARCHAR(10) - Optional
- @IsActive BIT - Optional
- @PageNumber INT = 1
- @PageSize INT = 50
- @SortColumn VARCHAR(50) = 'CustomerNumber'
- @SortDirection VARCHAR(4) = 'ASC'
- @TotalRecords INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by name | @SearchTerm='Smith' | Returns customers with FirstName/LastName/CompanyName/CustomerNumber matching '%Smith%' |
| 2 | Search by state | @StateCode='TX' | Returns only TX customers |
| 3 | Search by customer number | @CustomerNumber='CUS0000001' | Returns exact match |
| 4 | Pagination page 1 | @PageNumber=1, @PageSize=10 | Returns max 10 rows, @TotalRecords has full count |
| 5 | Pagination page 2 | @PageNumber=2, @PageSize=10 | Returns next 10 rows |
| 6 | Active only filter | @IsActive=1 | Returns only active customers |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching results | @SearchTerm='ZZZZNONEXISTENT' | Empty result set, @TotalRecords=0 |
| 2 | Invalid sort column | @SortColumn='NonExistentColumn' | SQL error (QUOTENAME prevents injection but column may not exist) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | All filters NULL | No filters set | Returns all customers (up to PageSize) |
| 2 | PageNumber beyond data | @PageNumber=9999 | Empty result set |
| 3 | PageSize = 1 | @PageSize=1 | Returns exactly 1 row |
| 4 | ZipCode prefix match | @ZipCode='787' | Matches ZipCodes starting with '787%' |

---

### Test Case ID: SP-POL-005
**Procedure**: Policy.usp_Property_Create
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @CustomerID INT - Required
- @PropertyType VARCHAR(30) - Required
- @ConstructionType VARCHAR(30) - Required
- @OccupancyType VARCHAR(30) - Required
- @YearBuilt INT - Required
- @SquareFootage INT - Required
- @NumberOfStories INT = 1
- @RoofType VARCHAR(30) - Optional
- @RoofAge INT - Optional
- @HasBasement BIT = 0
- @HasPool BIT = 0
- @HasFireAlarm BIT = 0
- @HasBurglarAlarm BIT = 0
- @HasSprinklerSystem BIT = 0
- @DistanceToFireStation DECIMAL(6,2) - Optional
- @DistanceToHydrant DECIMAL(6,2) - Optional
- @FireProtectionClass INT - Optional
- @FloodZone VARCHAR(10) - Optional
- @MarketValue DECIMAL(18,2) - Optional
- @ReplacementCost DECIMAL(18,2) - Optional
- @AddressLine1 VARCHAR(200) - Required
- @AddressLine2 VARCHAR(200) - Optional
- @City VARCHAR(100) - Required
- @StateCode CHAR(2) - Required
- @ZipCode VARCHAR(10) - Required
- @County VARCHAR(100) - Optional
- @CreatedBy VARCHAR(50) - Required
- @PropertyID INT OUTPUT
- @PropertyNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create property for valid customer | @CustomerID=1, @PropertyType='SINGLE_FAMILY', @ConstructionType='FRAME', @OccupancyType='OWNER_OCCUPIED', @YearBuilt=2000, @SquareFootage=2000, @AddressLine1='100 Main', @City='Austin', @StateCode='TX', @ZipCode='78701', @CreatedBy='test' | PropertyID > 0, PropertyNumber starts with 'PRP' |
| 2 | Auto-calculate FireProtectionClass | @DistanceToFireStation=0.5, @DistanceToHydrant=0.2, @FireProtectionClass=NULL | FireProtectionClass = 3 |
| 3 | FireProtectionClass dist 3mi/0.4mi | @DistanceToFireStation=3.0, @DistanceToHydrant=0.4 | FireProtectionClass = 5 |
| 4 | FireProtectionClass dist 4mi | @DistanceToFireStation=4.0, @DistanceToHydrant=1.0 | FireProtectionClass = 7 |
| 5 | FireProtectionClass dist > 5mi | @DistanceToFireStation=6.0 | FireProtectionClass = 9 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Inactive customer | @CustomerID=(inactive customer ID) | RAISERROR: 'Active customer not found: {id}' |
| 2 | Non-existent customer | @CustomerID=999999 | RAISERROR: 'Active customer not found: 999999' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | DistanceToFireStation = 1.0 exactly | @DistanceToFireStation=1.0, @DistanceToHydrant=0.25 | FireProtectionClass = 3 (boundary: <= 1.0) |
| 2 | DistanceToFireStation = 1.01 | @DistanceToFireStation=1.01, @DistanceToHydrant=0.25 | FireProtectionClass = 5 |
| 3 | Territory lookup matches | Valid StateCode/ZipCode matching Territories | TerritoryID populated |
| 4 | Territory lookup no match | StateCode/ZipCode with no territory | TerritoryID = NULL |

---

### Test Case ID: SP-POL-006
**Procedure**: Policy.usp_Property_GetByCustomer
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @CustomerID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Customer with properties | @CustomerID=(valid with properties) | Returns all active properties ordered by PropertyNumber |
| 2 | Only active properties returned | @CustomerID with inactive properties | Only IsActive=1 returned |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Customer with no properties | @CustomerID=(no properties) | Empty result set |
| 2 | Non-existent customer | @CustomerID=999999 | Empty result set |

---

### Test Case ID: SP-POL-007
**Procedure**: Policy.usp_Property_GetByID
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PropertyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get existing property | @PropertyID=1 | Returns all property columns |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent property | @PropertyID=999999 | Empty result set |

---

### Test Case ID: SP-POL-008
**Procedure**: Policy.usp_Policy_CreateQuote
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @PolicyType VARCHAR(30) - Required
- @CustomerID INT - Required
- @PropertyID INT - Required
- @AgentID INT - Required
- @EffectiveDate DATE - Required
- @TermMonths INT = 12
- @PaymentPlan VARCHAR(20) = 'ANNUAL'
- @BillingMethod VARCHAR(20) = 'DIRECT'
- @PriorCarrier VARCHAR(100) - Optional
- @PriorPolicyNumber VARCHAR(50) - Optional
- @PriorExpiryDate DATE - Optional
- @YearsWithPriorCarrier INT - Optional
- @ClaimFreeYears INT = 0
- @CreatedBy VARCHAR(50) - Required
- @PolicyID INT OUTPUT
- @PolicyNumber VARCHAR(20) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create basic quote | @PolicyType='HO3', @CustomerID=1, @PropertyID=1, @AgentID=1, @EffectiveDate='2025-01-01', @CreatedBy='test' | PolicyID > 0, PolicyNumber format 'POL'+7digits, PolicyStatus='QUOTE' |
| 2 | Create with prior carrier info | Include @PriorCarrier, @PriorPolicyNumber, @ClaimFreeYears=5 | Policy created with prior info stored |
| 3 | Term months = 6 | @TermMonths=6, @EffectiveDate='2025-01-01' | ExpiryDate = '2025-07-01' |
| 4 | Commission rate from agent | @AgentID with CommissionRate=0.12 | Policy.CommissionRate = 0.12 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Inactive customer | @CustomerID=(inactive) | RAISERROR: 'Active customer not found' |
| 2 | Property not belonging to customer | @CustomerID=1, @PropertyID=(belongs to customer 2) | RAISERROR: 'Property not found or does not belong to customer' |
| 3 | Inactive agent | @AgentID=(inactive) | RAISERROR: 'Active agent not found' |
| 4 | Moratorium in effect | Active moratorium for state/type | RAISERROR: 'New business moratorium is in effect for this location/policy type' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | EffectiveDate = today | @EffectiveDate=GETDATE() | Quote created successfully |
| 2 | EffectiveDate in the past | @EffectiveDate='2020-01-01' | [ASSUMPTION] Quote still created (no past-date validation in SP) |
| 3 | TermMonths = 0 | @TermMonths=0 | ExpiryDate = EffectiveDate |
| 4 | ClaimFreeYears negative | @ClaimFreeYears=-1 | [ASSUMPTION] Accepted (no validation) |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | PolicyVersions record created | Valid quote creation | Valid params | PolicyVersions row with VersionNumber=1, VersionType='NEW' |
| 2 | Audit log entry | Valid creation | Valid params | AuditLog with Action='INSERT' |
| 3 | Moratorium check | Active moratorium for 'TX' and 'HO3' | @PropertyState='TX', @PolicyType='HO3' | Error raised |

---

### Test Case ID: SP-POL-009
**Procedure**: Policy.usp_Policy_GetDetails
**Source**: `database/02-stored-procedures/001-policy-crud-sps.sql`
**Parameters**:
- @PolicyID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get full policy details | @PolicyID=1 | Returns 6 result sets: Policy header with joins, Coverages, Endorsements, Claims, Billing, Notes |
| 2 | Policy with coverages | @PolicyID with coverages | Second result set contains selected coverages with PerilCount |
| 3 | Policy with no claims | @PolicyID with no claims | Fourth result set is empty |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent policy | @PolicyID=999999 | All result sets empty |

---

### Test Case ID: SP-POL-010
**Procedure**: Policy.usp_Policy_Search
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyNumber VARCHAR(20) - Optional
- @CustomerName VARCHAR(200) - Optional
- @PolicyType VARCHAR(30) - Optional
- @PolicyStatus VARCHAR(20) - Optional
- @StateCode CHAR(2) - Optional
- @EffectiveDateFrom DATE - Optional
- @EffectiveDateTo DATE - Optional
- @AgentID INT - Optional
- @PageNumber INT = 1
- @PageSize INT = 50
- @SortColumn VARCHAR(50) = 'PolicyNumber'
- @SortDirection VARCHAR(4) = 'ASC'
- @TotalRecords INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by policy number | @PolicyNumber='POL0000001' | Returns exact match |
| 2 | Search by customer name | @CustomerName='Smith' | Returns policies where customer FirstName+LastName or CompanyName LIKE '%Smith%' |
| 3 | Search by type | @PolicyType='HO3' | Returns only HO3 policies |
| 4 | Search by status | @PolicyStatus='ACTIVE' | Returns only active policies |
| 5 | Search by state | @StateCode='TX' | Returns policies with property in TX |
| 6 | Search by date range | @EffectiveDateFrom='2025-01-01', @EffectiveDateTo='2025-12-31' | Returns policies effective in range |
| 7 | Pagination | @PageNumber=1, @PageSize=10 | Returns max 10 rows, TotalRecords has full count |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching results | @PolicyNumber='NONEXISTENT' | Empty result, @TotalRecords=0 |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | All filters NULL | No params except paging | Returns all policies |
| 2 | PageNumber exceeds data | @PageNumber=99999 | Empty result set |

---

### Test Case ID: SP-POL-011
**Procedure**: Policy.usp_Policy_Bind
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Bind policy in QUOTE status | @PolicyID=(QUOTE status), @ModifiedBy='testuser' | PolicyStatus changed to 'ACTIVE', AuditLog entry with Action='BIND' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Bind policy not in QUOTE status | @PolicyID=(ACTIVE status) | RAISERROR: 'Policy must be in QUOTE status to bind' (severity 16, state 1) |
| 2 | Non-existent policy | @PolicyID=999999 | RAISERROR: 'Policy must be in QUOTE status to bind' |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Bind already-bound policy | @PolicyID=(ACTIVE status) | Error raised, no status change |
| 2 | Bind cancelled policy | @PolicyID=(CANCELLED status) | Error raised |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Audit trail created | Policy in QUOTE status | Valid bind | AuditLog: 'Policy bound - status changed to ACTIVE' |
| 2 | Transaction rollback on error | Policy not in QUOTE | Bind attempt | No data changes (transaction rolled back) |

---

### Test Case ID: SP-POL-012
**Procedure**: Policy.usp_Policy_Cancel
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @CancelReason VARCHAR(50) - Required
- @CancelDate DATE - Required
- @CalculationMethod VARCHAR(20) = 'PRO_RATA'
- @CancelledBy VARCHAR(50) - Required
- @ReturnPremium DECIMAL(18,2) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Cancel mid-term pro-rata | @PolicyID=1, @CancelReason='INSURED_REQUEST', @CancelDate=(mid-term), @CancelledBy='test' | PolicyStatus='CANCELLED', @ReturnPremium calculated correctly |
| 2 | Return premium calculation | Policy: Annual=$1200, 12-month term, cancel at 6 months | @ReturnPremium = $600 (pro-rata) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent policy | @PolicyID=999999 | Error (NULL values from SELECT) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Cancel on effective date | @CancelDate = EffectiveDate | @ReturnPremium = full AnnualPremium (0 days used) |
| 2 | Cancel on expiry date | @CancelDate = ExpiryDate | @ReturnPremium = $0 (all premium earned) |
| 3 | Cancel 1 day before expiry | @CancelDate = ExpiryDate - 1 day | Small return premium |

#### Data Dependency Tests
| # | Description | Pre-condition | Input | Expected |
|---|-------------|---------------|-------|----------|
| 1 | Audit trail | Active policy exists | Valid cancel | AuditLog: Action='CANCEL' |
| 2 | Transaction atomicity | Valid policy | Cancel | Status update + audit log in same transaction |

---

### Test Case ID: SP-POL-013
**Procedure**: Policy.usp_Policy_Reinstate
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @ReinstatementDate DATE - Required
- @BackdateToCancel BIT = 0
- @RequirePayment BIT = 1
- @PaymentAmount DECIMAL(18,2) = 0
- @Conditions VARCHAR(MAX) - Optional
- @ReinstatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Reinstate cancelled policy | @PolicyID=(cancelled), @ReinstatementDate=GETDATE(), @ReinstatedBy='test' | PolicyStatus='ACTIVE', AuditLog Action='REINSTATE' |
| 2 | Reinstate with backdate | @BackdateToCancel=1 | Policy status changed to ACTIVE |
| 3 | Reinstate with payment requirement | @RequirePayment=1, @PaymentAmount=500.00 | Policy reinstated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent policy | @PolicyID=999999 | [ASSUMPTION] No rows affected, no error raised |

---

### Test Case ID: SP-POL-014
**Procedure**: Policy.usp_Policy_SaveCoverage
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @CoverageCode VARCHAR(20) - Required
- @CoverageName VARCHAR(100) - Required
- @LimitAmount DECIMAL(18,2) - Required
- @DeductibleAmount DECIMAL(18,2) - Required
- @Premium DECIMAL(18,2) - Required
- @IsSelected BIT = 1
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Insert new coverage | @PolicyID=1, @CoverageCode='DWELLING', @CoverageName='Dwelling', @LimitAmount=250000, @DeductibleAmount=1000, @Premium=1000, @CreatedBy='test' | New row in Policy.Coverages |
| 2 | Update existing coverage (upsert) | Same PolicyID+CoverageCode, different LimitAmount | Existing row updated |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid PolicyID (FK) | @PolicyID=999999, @CoverageCode='TEST' | FK violation |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | LimitAmount = 0 | @LimitAmount=0 | Accepted |
| 2 | Premium = 0 | @Premium=0 | Accepted |
| 3 | IsSelected = 0 (deselect coverage) | @IsSelected=0 | Coverage marked inactive |

---

### Test Case ID: SP-POL-015
**Procedure**: Policy.usp_Policy_UpdatePremium
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @PolicyID INT - Required
- @AnnualPremium DECIMAL(18,2) - Required
- @TotalFees DECIMAL(18,2) - Required
- @GrossPremium DECIMAL(18,2) - Required
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update premium values | @PolicyID=1, @AnnualPremium=1500.00, @TotalFees=52.50, @GrossPremium=1552.50, @ModifiedBy='test' | All premium fields updated, ModifiedDate set |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent policy | @PolicyID=999999 | 0 rows affected |

---

### Test Case ID: SP-POL-016
**Procedure**: Policy.usp_Agent_Search
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AgentName VARCHAR(100) - Optional
- @AgentType VARCHAR(20) - Optional
- @AgencyID INT - Optional
- @StateCode CHAR(2) - Optional
- @IsActive BIT = 1

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search all active agents | No params (defaults) | Returns all active agents ordered by LastName, FirstName |
| 2 | Search by name | @AgentName='Smith' | Returns agents with FirstName+LastName LIKE '%Smith%' |
| 3 | Search by type | @AgentType='CAPTIVE' | Returns only CAPTIVE agents |
| 4 | Include inactive | @IsActive=0 | Returns only inactive agents |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching agents | @AgentName='ZZZZNONEXIST' | Empty result set |

---

### Test Case ID: SP-POL-017
**Procedure**: Policy.usp_Agent_GetByID
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AgentID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get existing agent | @AgentID=1 | Returns agent with AgencyName from LEFT JOIN |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent agent | @AgentID=999999 | Empty result set |

---

### Test Case ID: SP-POL-018
**Procedure**: Policy.usp_Agent_Update
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AgentID INT - Required
- @Email VARCHAR(200) - Optional
- @Phone VARCHAR(20) - Optional
- @CommissionRate DECIMAL(6,4) - Optional
- @IsActive BIT - Optional
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update email | @AgentID=1, @Email='new@agent.com', @ModifiedBy='test' | Email updated, ModifiedDate set |
| 2 | Update commission rate | @AgentID=1, @CommissionRate=0.15, @ModifiedBy='test' | CommissionRate = 0.15 |
| 3 | Deactivate agent | @AgentID=1, @IsActive=0, @ModifiedBy='test' | IsActive = 0 |
| 4 | NULL fields preserve existing | @AgentID=1, @Email=NULL, @ModifiedBy='test' | Existing Email unchanged (ISNULL) |

---

### Test Case ID: SP-POL-019
**Procedure**: Policy.usp_Agent_GetProduction
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @AgentID INT - Required
- @DateFrom DATE - Optional (defaults to 1 year ago)
- @DateTo DATE - Optional (defaults to today)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get production with defaults | @AgentID=1 | Returns PolicyCount, TotalPremium, TotalCommission for last year |
| 2 | Get production for date range | @AgentID=1, @DateFrom='2024-01-01', @DateTo='2024-12-31' | Returns stats for specified range |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Agent with no policies | @AgentID=(no policies) | Returns PolicyCount=0, TotalPremium=NULL, TotalCommission=NULL |

---

### Test Case ID: SP-POL-020
**Procedure**: Policy.usp_Document_Create
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required
- @DocumentType VARCHAR(30) - Required
- @FileName VARCHAR(200) - Required
- @FilePath VARCHAR(500) - Required
- @FileSize INT = 0
- @Description VARCHAR(500) - Optional
- @CreatedBy VARCHAR(50) - Required
- @DocumentID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create document for policy | @EntityType='POLICY', @EntityID=1, @DocumentType='DECLARATION', @FileName='dec.pdf', @FilePath='/docs/dec.pdf', @CreatedBy='test' | @DocumentID assigned |
| 2 | Create document with description | Include @Description='Annual declaration page' | Document created with description |

---

### Test Case ID: SP-POL-021
**Procedure**: Policy.usp_Document_Delete
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DocumentID INT - Required
- @DeletedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Delete existing document | @DocumentID=1, @DeletedBy='test' | Document removed/marked deleted |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent document | @DocumentID=999999, @DeletedBy='test' | [ASSUMPTION] No error, 0 rows affected |

---

### Test Case ID: SP-POL-022
**Procedure**: Policy.usp_Document_GetByEntity
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get documents for policy | @EntityType='POLICY', @EntityID=1 | Returns documents for entity |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Entity with no documents | @EntityType='POLICY', @EntityID=999999 | Empty result set |

---

### Test Case ID: SP-POL-023
**Procedure**: Policy.usp_Document_Search
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Optional
- @DocumentType VARCHAR(30) - Optional
- @DateFrom DATE - Optional
- @DateTo DATE - Optional
- @PageNumber INT = 1
- @PageSize INT = 50

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by entity type | @EntityType='POLICY' | Returns policy documents |
| 2 | Search by document type | @DocumentType='PHOTO' | Returns photo documents |
| 3 | Search by date range | @DateFrom='2024-01-01', @DateTo='2024-12-31' | Returns documents in range |
| 4 | Pagination | @PageNumber=1, @PageSize=10 | Returns up to 10 results |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching documents | @EntityType='NONEXIST' | Empty result set |
