# Reinsurance Module - Unit Tests

## Module: RNS (Reinsurance)
## Source Files:
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

## Classes Covered:
1. ReinsuranceDataAccess

## Methods Covered (7 total):
1. GetActiveTreaties()
2. GetTreatyDetails(treatyID)
3. CreateTreaty(...)
4. CalculateCession(treatyID, policyID, grossAmount)
5. GetCessionsByTreaty(treatyID, accountingPeriod)
6. GenerateBordereaux(treatyID, reportingPeriod, reportType)
7. GetBordereaux(treatyID)
8. GetReinsurers()

---

### Test Case ID: UT-RNS-001
**Class**: ReinsuranceDataAccess
**Method**: GetActiveTreaties()
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: None
- Return Type: DataTable
- Calls: DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Treaty_GetActive", Nothing)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Active treaties exist | -- | DataTable with treaty rows | Happy path |
| 2 | No active treaties | -- | Empty DataTable | No rows returned |
| 3 | Database connection failure | -- | Throws SqlException | Connection unavailable |
| 4 | SP execution error | -- | Throws SqlException | SP raises error |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Setup: Return DataTable with treaty columns (TreatyID, TreatyNumber, TreatyName, TreatyType, ReinsurerName, etc.)
- Verify: Called with "Reinsurance.usp_Treaty_GetActive" and Nothing (no parameters)

---

### Test Case ID: UT-RNS-002
**Class**: ReinsuranceDataAccess
**Method**: GetTreatyDetails(treatyID As Integer)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyID (Integer)
- Return Type: DataSet
- Calls: DatabaseHelper.ExecuteDataSet("Reinsurance.usp_Treaty_GetDetails", params)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid treaty ID | treatyID=1 | DataSet with treaty detail tables | Happy path |
| 2 | Non-existent treaty ID | treatyID=99999 | DataSet with empty tables | No error thrown |
| 3 | Zero treaty ID | treatyID=0 | DataSet with empty tables | Edge case |
| 4 | Database error | treatyID=1 | Throws SqlException | SP error |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteDataSet
- Setup: Return DataSet with multiple DataTables (treaty header, cessions, bordereaux)
- Verify: @TreatyID parameter created with correct value

---

### Test Case ID: UT-RNS-003
**Class**: ReinsuranceDataAccess
**Method**: CreateTreaty(treatyName, treatyType, reinsurerID, effectiveDate, expiryDate, retentionAmount, cessionPercent)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyName (String), treatyType (String), reinsurerID (Integer), effectiveDate (Date), expiryDate (Date), retentionAmount (Decimal), cessionPercent (Decimal)
- Return Type: Integer (TreatyID)
- Calls: DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Treaty_Create", params)
- Uses: GlobalState.CurrentUser for @CreatedBy
- Output params: @TreatyID (Int), @TreatyNumber (VarChar)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid treaty creation | All valid params | Integer > 0 (new TreatyID) | Happy path |
| 2 | Treaty name provided | treatyName="Test Treaty" | @TreatyName param = "Test Treaty" | Parameter mapping |
| 3 | Treaty type QUOTA_SHARE | treatyType="QUOTA_SHARE" | @TreatyType param = "QUOTA_SHARE" | |
| 4 | CreatedBy from GlobalState | -- | @CreatedBy = GlobalState.CurrentUser | Implicit parameter |
| 5 | Output param TreatyID returned | -- | Returns CInt(idParam.Value) | Return value is output param |
| 6 | Database error | Valid params, SP fails | Throws SqlException | Error propagation |
| 7 | NULL output param value | SP returns NULL in @TreatyID | [ASSUMPTION] Throws InvalidCastException on CInt(Nothing) | |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery, DatabaseHelper.CreateParam, DatabaseHelper.CreateOutputParam
- Mock: GlobalState.CurrentUser (return "test_user")
- Setup: CreateOutputParam returns SqlParameter with preset Value after ExecuteNonQuery
- Verify: 10 parameters passed (8 input + 2 output), SP name = "Reinsurance.usp_Treaty_Create"

---

### Test Case ID: UT-RNS-004
**Class**: ReinsuranceDataAccess
**Method**: CalculateCession(treatyID, policyID, grossAmount)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyID (Integer), policyID (Integer), grossAmount (Decimal)
- Return Type: DataTable
- Calls: DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Cession_Calculate", params)
- Uses: GlobalState.CurrentUser for @CreatedBy
- Note: Hardcodes @CessionType = "PREMIUM"

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid cession calculation | treatyID=1, policyID=100, grossAmount=10000 | DataTable with cession result | Happy path |
| 2 | CessionType always PREMIUM | Any input | @CessionType param = "PREMIUM" | Hardcoded value |
| 3 | Zero gross amount | grossAmount=0 | DataTable (may be empty) | Edge case |
| 4 | Database error | Valid params | Throws SqlException | Error propagation |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Mock: GlobalState.CurrentUser
- Setup: Return DataTable with cession columns
- Verify: 5 parameters passed (@TreatyID, @PolicyID, @GrossAmount, @CessionType, @CreatedBy)
- Verify: SP name = "Reinsurance.usp_Cession_Calculate"

---

### Test Case ID: UT-RNS-005
**Class**: ReinsuranceDataAccess
**Method**: GetCessionsByTreaty(treatyID, accountingPeriod)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyID (Integer), accountingPeriod (String, Optional, default Nothing)
- Return Type: DataTable
- Calls: DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Cession_GetByTreaty", params)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | With accounting period | treatyID=1, accountingPeriod="2024-06" | DataTable with filtered cessions | Happy path |
| 2 | Without accounting period | treatyID=1, accountingPeriod=Nothing | DataTable with all cessions for treaty | Optional param |
| 3 | No cessions found | treatyID=99999 | Empty DataTable | No matching records |
| 4 | Database error | Valid params | Throws SqlException | Error propagation |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Setup: Return DataTable with columns: GrossAmount, CededAmount, RetainedAmount, CessionType, etc.
- Verify: 2 parameters passed (@TreatyID, @AccountingPeriod)
- Verify: SP name = "Reinsurance.usp_Cession_GetByTreaty"

---

### Test Case ID: UT-RNS-006
**Class**: ReinsuranceDataAccess
**Method**: GenerateBordereaux(treatyID, reportingPeriod, reportType)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyID (Integer), reportingPeriod (String), reportType (String)
- Return Type: Integer (BordereauxID)
- Calls: DatabaseHelper.ExecuteNonQuery("Reinsurance.usp_Bordereaux_Generate", params)
- Uses: GlobalState.CurrentUser for @GeneratedBy
- Output param: @BordereauxID (Int)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid generation | treatyID=1, reportingPeriod="2024-06", reportType="PREMIUM" | Integer > 0 (new BordereauxID) | Happy path |
| 2 | LOSS report type | reportType="LOSS" | Integer > 0 | Different aggregation |
| 3 | OUTSTANDING report type | reportType="OUTSTANDING" | Integer > 0 | All cession types |
| 4 | GeneratedBy from GlobalState | -- | @GeneratedBy = GlobalState.CurrentUser | Implicit parameter |
| 5 | Treaty not found | treatyID=99999 | Throws SqlException | SP raises error |
| 6 | Database error | Valid params | Throws SqlException | Error propagation |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteNonQuery, DatabaseHelper.CreateParam, DatabaseHelper.CreateOutputParam
- Mock: GlobalState.CurrentUser
- Setup: CreateOutputParam returns SqlParameter with Value set after ExecuteNonQuery
- Verify: 5 parameters passed (4 input + 1 output), SP name = "Reinsurance.usp_Bordereaux_Generate"

---

### Test Case ID: UT-RNS-007
**Class**: ReinsuranceDataAccess
**Method**: GetBordereaux(treatyID)
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: treatyID (Integer)
- Return Type: DataTable
- Calls: DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Bordereaux_GetByTreaty", params)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Bordereaux exist for treaty | treatyID=1 | DataTable with bordereaux rows | Happy path |
| 2 | No bordereaux for treaty | treatyID=99999 | Empty DataTable | No records |
| 3 | Database error | treatyID=1 | Throws SqlException | Error propagation |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Setup: Return DataTable with bordereaux columns
- Verify: 1 parameter (@TreatyID), SP name = "Reinsurance.usp_Bordereaux_GetByTreaty"

---

### Test Case ID: UT-RNS-008
**Class**: ReinsuranceDataAccess
**Method**: GetReinsurers()
**File**: `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`

#### Method Signature
- Parameters: None
- Return Type: DataTable
- Calls: DatabaseHelper.ExecuteStoredProcedure("Reinsurance.usp_Reinsurer_List", Nothing)

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Reinsurers exist | -- | DataTable with reinsurer rows | Happy path |
| 2 | No reinsurers | -- | Empty DataTable | No records |
| 3 | Database error | -- | Throws SqlException | Error propagation |

#### Mock/Stub Requirements
- Mock: DatabaseHelper.ExecuteStoredProcedure
- Setup: Return DataTable with reinsurer columns (ReinsurerID, ReinsurerCode, ReinsurerName, AMBestRating, etc.)
- Verify: Called with "Reinsurance.usp_Reinsurer_List" and Nothing
