# Policy Module - Unit Tests

## Module: POL (Policy)
## Test Type: Unit Tests (Data Access Layer)
## Classes Covered:
- `PolicyDataAccess` - `src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb`
- `CustomerDataAccess` - `src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb`
- `PropertyDataAccess` - `src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb`

---

### Test Case ID: UT-POL-001
**Class**: CustomerDataAccess
**Method**: Create(dto As CustomerDTO) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb`

#### Method Signature
- Parameters: dto As CustomerDTO
- Return Type: Integer (CustomerID)
- Calls: Policy.usp_Customer_Create via DatabaseHelper.ExecuteNonQuery
- Output params: @CustomerID (Int), @CustomerNumber (VarChar)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid individual customer | CustomerDTO with Type='I', FirstName, LastName, Address fields | CustomerID > 0, dto.CustomerNumber set | Happy path |
| 2 | Valid commercial customer | CustomerDTO with Type='C', CompanyName | CustomerID > 0 | Commercial path |
| 3 | All optional fields populated | Full DTO with CreditScore, Occupation, etc. | Success | All params passed to SP |
| 4 | SP throws SqlException | Database error | Exception propagates | No try-catch in method |
| 5 | CreatedBy from GlobalState | Any valid DTO | @CreatedBy = GlobalState.CurrentUser | Verify GlobalState used |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery
- Mock: DatabaseHelper.CreateParam, DatabaseHelper.CreateOutputParam
- Setup: OutputParam @CustomerID returns test value, @CustomerNumber returns 'CUS0000001'
- Verify: 23 parameters passed to SP call

---

### Test Case ID: UT-POL-002
**Class**: CustomerDataAccess
**Method**: Update(dto As CustomerDTO)
**File**: `src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb`

#### Method Signature
- Parameters: dto As CustomerDTO
- Return Type: Void (Sub)
- Calls: Policy.usp_Customer_Update via DatabaseHelper.ExecuteNonQuery

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid update | DTO with CustomerID=1, updated Email | Completes without error | |
| 2 | All fields provided | Full DTO | 18 params passed to SP | |
| 3 | SP raises error (not found) | DTO with invalid CustomerID | SqlException propagated | SP RAISERROR 'Customer not found' |
| 4 | ModifiedBy from GlobalState | Any DTO | @ModifiedBy = GlobalState.CurrentUser | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery
- Setup: Verify SP name = 'Policy.usp_Customer_Update'
- Verify: @CustomerID and @ModifiedBy passed correctly

---

### Test Case ID: UT-POL-003
**Class**: CustomerDataAccess
**Method**: GetByID(customerID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb`

#### Method Signature
- Parameters: customerID As Integer
- Return Type: DataTable
- Calls: Policy.usp_Customer_GetByID via DatabaseHelper.ExecuteStoredProcedure

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Existing customer | customerID=1 | DataTable with 1 row | |
| 2 | Non-existent customer | customerID=999999 | DataTable with 0 rows | |
| 3 | Database error | Any ID | SqlException thrown | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Setup: Return DataTable with test data for valid ID
- Verify: Single @CustomerID param passed

---

### Test Case ID: UT-POL-004
**Class**: CustomerDataAccess
**Method**: Search(criteria As CustomerSearchCriteria) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb`

#### Method Signature
- Parameters: criteria As CustomerSearchCriteria
- Return Type: DataTable
- Calls: Policy.usp_Customer_Search via DatabaseHelper.ExecuteWithOutput
- Output param: @TotalRecords (Int) - written back to criteria.TotalRecords

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Search with term | criteria.SearchTerm='Smith' | DataTable with matches | |
| 2 | Search with all filters | All criteria fields set | DataTable filtered | 12 params passed |
| 3 | Empty result | criteria.SearchTerm='NONEXIST' | Empty DataTable, TotalRecords=0 | |
| 4 | TotalRecords populated | Valid search | criteria.TotalRecords set from output param | |
| 5 | Output param is DBNull | SP returns NULL | criteria.TotalRecords = 0 | Uses If(value, 0) |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteWithOutput
- Mock: DatabaseHelper.CreateOutputParam (returns SqlParameter with .Value)
- Verify: Output param value assigned to criteria.TotalRecords

---

### Test Case ID: UT-POL-005
**Class**: PolicyDataAccess
**Method**: CreateQuote(dto As PolicyDTO) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb`

#### Method Signature
- Parameters: dto As PolicyDTO
- Return Type: Integer (PolicyID)
- Calls: Policy.usp_Policy_CreateQuote via DatabaseHelper.ExecuteNonQuery
- Output params: @PolicyID (Int), @PolicyNumber (VarChar)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid quote creation | PolicyDTO with required fields | PolicyID > 0, dto.PolicyNumber set | |
| 2 | All optional fields | DTO with PriorCarrier, PriorPolicyNumber, etc. | Success | 16 params total |
| 3 | SP throws moratorium error | Valid DTO but moratorium active | SqlException: 'New business moratorium...' | |
| 4 | SP throws customer not found | Invalid CustomerID | SqlException: 'Active customer not found' | |
| 5 | CreatedBy from GlobalState | Any DTO | @CreatedBy = GlobalState.CurrentUser | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery
- Setup: OutputParam @PolicyID returns test value, @PolicyNumber returns 'POL0000001'
- Verify: 16 parameters passed correctly

---

### Test Case ID: UT-POL-006
**Class**: PolicyDataAccess
**Method**: GetDetails(policyID As Integer) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb`

#### Method Signature
- Parameters: policyID As Integer
- Return Type: DataSet (multiple tables)
- Calls: Policy.usp_Policy_GetDetails via DatabaseHelper.ExecuteDataSet

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Existing policy | policyID=1 | DataSet with 6 tables | Header, Coverages, Endorsements, Claims, Billing, Notes |
| 2 | Non-existent policy | policyID=999999 | DataSet with empty tables | |
| 3 | Database error | Any ID | SqlException thrown | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteDataSet
- Setup: Return DataSet with multiple DataTables
- Verify: Single @PolicyID param passed

---

### Test Case ID: UT-POL-007
**Class**: PolicyDataAccess
**Method**: Search(criteria As PolicySearchCriteria) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb`

#### Method Signature
- Parameters: criteria As PolicySearchCriteria
- Return Type: DataTable
- Calls: Policy.usp_Policy_Search via DatabaseHelper.ExecuteWithOutput
- Output param: @TotalRecords (Int)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Search by policy number | criteria.PolicyNumber='POL0000001' | DataTable with match | |
| 2 | Search by customer name | criteria.CustomerName='Doe' | DataTable with matches | |
| 3 | All filters | All criteria fields set | Filtered results | 13 params passed |
| 4 | TotalRecords populated | Any search | criteria.TotalRecords updated from output | |
| 5 | Empty result | criteria.PolicyNumber='NONEXIST' | Empty table, TotalRecords=0 | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteWithOutput
- Verify: 13 parameters including paging and sorting

---

### Test Case ID: UT-POL-008
**Class**: PropertyDataAccess
**Method**: Create(dto As PropertyDTO) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb`

#### Method Signature
- Parameters: dto As PropertyDTO
- Return Type: Integer (PropertyID)
- Calls: Policy.usp_Property_Create via DatabaseHelper.ExecuteNonQuery
- Output params: @PropertyID (Int), @PropertyNumber (VarChar)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid property creation | PropertyDTO with all required fields | PropertyID > 0, dto.PropertyNumber set | |
| 2 | All optional fields | DTO with FloodZone, MarketValue, etc. | Success | 29 params total |
| 3 | SP throws customer not found | DTO with invalid CustomerID | SqlException: 'Active customer not found' | |
| 4 | CreatedBy from GlobalState | Any DTO | @CreatedBy = GlobalState.CurrentUser | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery
- Setup: OutputParam @PropertyID returns value, @PropertyNumber returns 'PRP0000001'
- Verify: 29 parameters passed

---

### Test Case ID: UT-POL-009
**Class**: PropertyDataAccess
**Method**: GetByCustomer(customerID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb`

#### Method Signature
- Parameters: customerID As Integer
- Return Type: DataTable
- Calls: Policy.usp_Property_GetByCustomer via DatabaseHelper.ExecuteStoredProcedure

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Customer with properties | customerID=1 | DataTable with property rows | Only active properties |
| 2 | Customer with no properties | customerID=999 | Empty DataTable | |
| 3 | Database error | Any ID | SqlException thrown | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Verify: Single @CustomerID param

---

### Test Case ID: UT-POL-010
**Class**: PropertyDataAccess
**Method**: GetByID(propertyID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb`

#### Method Signature
- Parameters: propertyID As Integer
- Return Type: DataTable
- Calls: Policy.usp_Property_GetByID via DatabaseHelper.ExecuteStoredProcedure

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Existing property | propertyID=1 | DataTable with 1 row | |
| 2 | Non-existent property | propertyID=999999 | Empty DataTable | |
| 3 | Database error | Any ID | SqlException thrown | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Verify: Single @PropertyID param
