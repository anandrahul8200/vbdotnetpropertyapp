# Underwriting Module - Unit Tests

## Module: UND (Underwriting)
## Test Type: Unit Tests for Data Access Layer
## Classes Covered:
- `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

---

### Test Case ID: UT-UND-001
**Class**: UnderwritingDataAccess
**Method**: CalculatePremium(policyID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CalculatePremium(policyID As Integer) As DataTable
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |
| GlobalState.CurrentUser | @CalculatedBy | VARCHAR(50) |
| True (hardcoded) | @RecalculateAll | BIT |
| (output) | @TotalPremium | DECIMAL(18,2) OUTPUT |
| (output) | @TotalTaxes | DECIMAL(18,2) OUTPUT |
| (output) | @GrossPremium | DECIMAL(18,2) OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Premium_Calculate' |
| 2 | Passes 6 parameters (3 input + 3 output) | Valid inputs | 6 SqlParameter objects |
| 3 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 4 | CurrentUser passed as CalculatedBy | GlobalState.CurrentUser='admin' | @CalculatedBy='admin' |
| 5 | RecalculateAll hardcoded to True | Any call | @RecalculateAll=True |
| 6 | Returns DataTable with 3 columns | Valid outputs | Columns: TotalPremium, TotalTaxes, GrossPremium (Decimal) |
| 7 | Output values converted to Decimal | Output params have values | CDec() applied to each output |
| 8 | Single row in result | Any call | DataTable has exactly 1 row |

---

### Test Case ID: UT-UND-002
**Class**: UnderwritingDataAccess
**Method**: EvaluateRules(policyID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function EvaluateRules(policyID As Integer) As DataTable
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |
| GlobalState.CurrentUser | @EvaluatedBy | VARCHAR(50) |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Rules_Evaluate' |
| 2 | Passes 2 parameters | Valid inputs | 2 SqlParameter objects in array |
| 3 | ExecuteStoredProcedure called | Valid inputs | DatabaseHelper.ExecuteStoredProcedure invoked |
| 4 | Returns DataTable from SP | SP returns data | DataTable result set |
| 5 | CurrentUser passed as EvaluatedBy | GlobalState.CurrentUser='underwriter1' | @EvaluatedBy='underwriter1' |

---

### Test Case ID: UT-UND-003
**Class**: UnderwritingDataAccess
**Method**: ProcessReferral(referralID As Integer, decision As String, notes As String, conditions As String)
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Sub ProcessReferral(referralID As Integer, decision As String, notes As String, conditions As String)
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| referralID | @ReferralID | INT |
| decision | @Decision | VARCHAR(20) |
| notes | @DecisionNotes | VARCHAR(MAX) |
| conditions | @Conditions | VARCHAR(MAX) |
| GlobalState.CurrentUser | @ReviewedBy | VARCHAR(50) |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Referral_Process' |
| 2 | Passes 5 parameters | Valid inputs | 5 SqlParameter objects in array |
| 3 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 4 | Decision values passed correctly | decision='APPROVED' | @Decision='APPROVED' |
| 5 | Notes passed | notes='Approved per guidelines' | @DecisionNotes='Approved per guidelines' |
| 6 | Conditions passed | conditions='Install sprinklers' | @Conditions='Install sprinklers' |
| 7 | CurrentUser as ReviewedBy | GlobalState.CurrentUser='mgr1' | @ReviewedBy='mgr1' |

---

### Test Case ID: UT-UND-004
**Class**: UnderwritingDataAccess
**Method**: GetPendingReferrals(Optional assignedTo As String = Nothing, Optional pageNumber As Integer = 1) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetPendingReferrals(Optional assignedTo As String = Nothing, Optional pageNumber As Integer = 1) As DataTable
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| assignedTo | @AssignedTo | VARCHAR(50) |
| pageNumber | @PageNumber | INT |
| 50 (hardcoded) | @PageSize | INT |
| (output) | @TotalRecords | INT OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Referral_SearchPending' |
| 2 | PageSize hardcoded to 50 | Any call | @PageSize=50 |
| 3 | Output param for TotalRecords | Any call | SqlDbType.Int output parameter created |
| 4 | ExecuteWithOutput called | Valid inputs | DatabaseHelper.ExecuteWithOutput invoked |
| 5 | Default assignedTo is Nothing | No parameter provided | @AssignedTo=Nothing |
| 6 | Default pageNumber is 1 | No parameter provided | @PageNumber=1 |
| 7 | Custom assignedTo passed | assignedTo='user1' | @AssignedTo='user1' |

---

### Test Case ID: UT-UND-005
**Class**: UnderwritingDataAccess
**Method**: GetRatingWorksheet(policyID As Integer) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetRatingWorksheet(policyID As Integer) As DataTable
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyID | @PolicyID | INT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Worksheet_GetByPolicy' |
| 2 | Single parameter | Valid input | 1 SqlParameter in array |
| 3 | ExecuteStoredProcedure called | Valid input | DatabaseHelper.ExecuteStoredProcedure invoked |
| 4 | Returns DataTable | SP returns data | DataTable result |

---

### Test Case ID: UT-UND-006
**Class**: UnderwritingDataAccess
**Method**: CheckMoratorium(policyType As String, stateCode As String, zipCode As String) As Boolean
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CheckMoratorium(policyType As String, stateCode As String, zipCode As String) As Boolean
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| policyType | @PolicyType | VARCHAR(30) |
| stateCode | @StateCode | CHAR(2) |
| zipCode | @ZipCode | VARCHAR(10) |
| (output) | @IsMoratorium | BIT OUTPUT |
| (output) | @MoratoriumName | VARCHAR(200) OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Moratorium_Check' |
| 2 | Passes 5 parameters (3 input + 2 output) | Valid inputs | 5 SqlParameter objects |
| 3 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |
| 4 | MoratoriumName output param size | Any call | nameParam.Size = 200 |
| 5 | Returns Boolean from output | @IsMoratorium=True | Function returns True |
| 6 | Returns False when no moratorium | @IsMoratorium=False | Function returns False |
| 7 | CBool conversion | Output param value | CBool(isMoratoriumParam.Value) |

---

### Test Case ID: UT-UND-007
**Class**: UnderwritingDataAccess
**Method**: GetActiveMoratoriums() As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetActiveMoratoriums() As DataTable
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| (none) | (none) | N/A |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Underwriting.usp_Moratorium_GetActive' |
| 2 | No parameters passed | Any call | params = Nothing |
| 3 | ExecuteStoredProcedure called | Any call | DatabaseHelper.ExecuteStoredProcedure invoked |
| 4 | Returns DataTable | SP returns data | DataTable result |
