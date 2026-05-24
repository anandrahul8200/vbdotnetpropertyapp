# Claims Module - Unit Tests

## Module: CLM (Claims)
## Test Type: Unit Tests for Data Access Layer
## Classes Covered:
- `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`
- `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

---

### Test Case ID: UT-CLM-001
**Class**: ClaimDataAccess
**Method**: Create(dto As ClaimDTO) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function Create(dto As ClaimDTO) As Integer
```

#### Parameter Mapping (DTO to SP)
| DTO Property | SP Parameter | Type |
|-------------|-------------|------|
| dto.PolicyID | @PolicyID | INT |
| dto.ClaimType | @ClaimType | VARCHAR(30) |
| dto.LossDate | @LossDate | DATETIME |
| dto.LossDescription | @LossDescription | VARCHAR(MAX) |
| dto.LossLocation | @LossLocation | VARCHAR(500) |
| dto.EstimatedLoss | @EstimatedLoss | DECIMAL(18,2) |
| dto.PoliceReportNumber | @PoliceReportNumber | VARCHAR(50) |
| dto.FireReportNumber | @FireReportNumber | VARCHAR(50) |
| dto.WeatherCondition | @WeatherCondition | VARCHAR(50) |
| dto.PointOfOrigin | @PointOfOrigin | VARCHAR(200) |
| dto.CatastropheID | @CatastropheID | INT |
| dto.Priority | @Priority | VARCHAR(10) |
| GlobalState.CurrentUser | @CreatedBy | VARCHAR(50) |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Successful creation returns ClaimID | Valid DTO | ClaimID > 0, dto.ClaimID set, dto.ClaimNumber set |
| 2 | SP called is Claims.usp_Claim_Create | Mock DatabaseHelper | Correct SP name passed |
| 3 | Output params read correctly | SP returns ClaimID=1, ClaimNumber='CLM0000001' | dto.ClaimID=1, dto.ClaimNumber='CLM0000001' |
| 4 | All parameters passed | Full DTO | 15 parameters in List(Of SqlParameter) |

---

### Test Case ID: UT-CLM-002
**Class**: ClaimDataAccess
**Method**: UpdateStatus(claimID, newStatus, reason, Optional denialReason)
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Sub UpdateStatus(claimID As Integer, newStatus As String, reason As String, Optional denialReason As String = Nothing)
```

#### Parameter Mapping
| Parameter | SP Parameter |
|-----------|-------------|
| claimID | @ClaimID |
| newStatus | @NewStatus |
| reason | @Reason |
| denialReason | @DenialReason |
| GlobalState.CurrentUser | @ModifiedBy |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Claims.usp_Claim_UpdateStatus' |
| 2 | Passes 5 parameters | Valid inputs | 5 SqlParameter objects |
| 3 | Optional denialReason null | denialReason=Nothing | @DenialReason param value=Nothing |
| 4 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |

---

### Test Case ID: UT-CLM-003
**Class**: ClaimDataAccess
**Method**: GetDetails(claimID As Integer) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetDetails(claimID As Integer) As DataSet
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_GetDetails' |
| 2 | Returns DataSet | Valid claimID | DataSet with 7 tables |
| 3 | Passes ClaimID parameter | claimID=1 | @ClaimID=1 |

---

### Test Case ID: UT-CLM-004
**Class**: ClaimDataAccess
**Method**: Search(criteria As ClaimSearchCriteria) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function Search(criteria As ClaimSearchCriteria) As DataTable
```

#### Parameter Mapping
| Criteria Property | SP Parameter |
|------------------|-------------|
| ClaimNumber | @ClaimNumber |
| PolicyNumber | @PolicyNumber |
| CustomerName | @CustomerName |
| ClaimStatus | @ClaimStatus |
| ClaimType | @ClaimType |
| LossDateFrom | @LossDateFrom |
| LossDateTo | @LossDateTo |
| AdjusterID | @AdjusterID |
| CatastropheID | @CatastropheID |
| Priority | @Priority |
| MinAmount | @MinAmount |
| MaxAmount | @MaxAmount |
| PageNumber | @PageNumber |
| PageSize | @PageSize |
| SortColumn | @SortColumn |
| SortDirection | @SortDirection |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_Search' |
| 2 | TotalRecords populated | Output param returns 100 | criteria.TotalRecords=100 |
| 3 | Passes 17 parameters (16 + output) | Full criteria | 17 SqlParameter objects |
| 4 | Uses ExecuteWithOutput | Mock | DatabaseHelper.ExecuteWithOutput called |

---

### Test Case ID: UT-CLM-005
**Class**: ClaimDataAccess
**Method**: SetReserve(claimID, reserveType, category, amount, reason) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function SetReserve(claimID As Integer, reserveType As String, category As String, amount As Decimal, reason As String) As Integer
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_SetReserve' |
| 2 | Returns ReserveID | Output param=5 | Return value=5 |
| 3 | Passes 7 parameters (6 + output) | Valid inputs | Correct count |
| 4 | Uses GlobalState.CurrentUser for @SetBy | CurrentUser='adjuster1' | @SetBy='adjuster1' |

---

### Test Case ID: UT-CLM-006
**Class**: ClaimDataAccess
**Method**: CreatePayment(dto As ClaimPaymentDTO) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CreatePayment(dto As ClaimPaymentDTO) As Integer
```

#### Parameter Mapping
| DTO Property | SP Parameter |
|-------------|-------------|
| dto.ClaimID | @ClaimID |
| dto.PaymentType | @PaymentType |
| dto.PaymentMethod | @PaymentMethod |
| dto.PayeeType | @PayeeType |
| dto.PayeeName | @PayeeName |
| dto.PayeeAddress | @PayeeAddress |
| dto.Amount | @Amount |
| dto.CoverageCode | @CoverageCode |
| dto.InvoiceNumber | @InvoiceNumber |
| dto.Description | @Description |
| dto.TaxReportable | @TaxReportable |
| GlobalState.CurrentUser | @CreatedBy |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_CreatePayment' |
| 2 | Returns PaymentID | Output param=10 | Return value=10 |
| 3 | Sets dto.PaymentID and dto.PaymentNumber | SP returns values | Both properties updated |
| 4 | Passes 14 parameters (12 + 2 output) | Full DTO | 14 SqlParameter objects |

---

### Test Case ID: UT-CLM-007
**Class**: ClaimDataAccess
**Method**: CreateActivity(claimID, activityType, subject, description, dueDate, assignedTo) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CreateActivity(claimID As Integer, activityType As String, subject As String, description As String, Optional dueDate As Date? = Nothing, Optional assignedTo As String = Nothing) As Integer
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_CreateActivity' |
| 2 | Returns ActivityID | Output param=3 | Return value=3 |
| 3 | Optional params passed as Nothing | No dueDate/assignedTo | @DueDate=Nothing, @AssignedTo=Nothing |

---

### Test Case ID: UT-CLM-008
**Class**: ClaimDataAccess
**Method**: GetDashboard(Optional adjusterID As Integer? = Nothing) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Claim_GetDashboard' |
| 2 | Returns DataSet | Valid call | DataSet with 5 tables |
| 3 | AdjusterID=Nothing | No filter | @AdjusterID param value=Nothing |
| 4 | AdjusterID=5 | Specific adjuster | @AdjusterID param value=5 |

---

### Test Case ID: UT-CLM-009
**Class**: FraudDataAccess
**Method**: EvaluateClaim(claimID As Integer) As Decimal
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Method Signature
```vb
Public Shared Function EvaluateClaim(claimID As Integer) As Decimal
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Fraud_EvaluateClaim' |
| 2 | Returns fraud score | Output param=75.50 | Return value=75.50 |
| 3 | Uses GlobalState.CurrentUser for @EvaluatedBy | CurrentUser='investigator1' | Correct param |
| 4 | Null output returns 0 | Output param=DBNull | Return value=0 |

---

### Test Case ID: UT-CLM-010
**Class**: FraudDataAccess
**Method**: ReferToSIU(claimID As Integer, reason As String)
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Fraud_ReferToSIU' |
| 2 | Passes 3 parameters | Valid inputs | @ClaimID, @ReferralReason, @ReferredBy |

---

### Test Case ID: UT-CLM-011
**Class**: FraudDataAccess
**Method**: GetFraudEvaluation(claimID As Integer) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Fraud_GetEvaluation' |
| 2 | Returns DataSet | Valid claimID | DataSet with 2 tables (summary + indicators) |

---

### Test Case ID: UT-CLM-012
**Class**: FraudDataAccess
**Method**: CreateSubrogation(claimID, responsibleParty, insurer, policyNumber, demandAmount) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Subrogation_Create' |
| 2 | Returns SubrogationID | Output param=1 | Return value=1 |
| 3 | Optional params | insurer=Nothing | @ResponsiblePartyInsurer=Nothing |

---

### Test Case ID: UT-CLM-013
**Class**: FraudDataAccess
**Method**: UpdateSubrogationStatus(subrogationID, newStatus, settlementAmount, notes)
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Subrogation_UpdateStatus' |
| 2 | Passes 5 parameters | Valid inputs | @SubrogationID, @NewStatus, @SettlementAmount, @Notes, @ModifiedBy |

---

### Test Case ID: UT-CLM-014
**Class**: FraudDataAccess
**Method**: RecordRecovery(subrogationID As Integer, amount As Decimal)
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Subrogation_RecordRecovery' |
| 2 | Passes 3 parameters | Valid inputs | @SubrogationID, @RecoveryAmount, @RecordedBy |

---

### Test Case ID: UT-CLM-015
**Class**: FraudDataAccess
**Method**: CreateCatastrophe(name, catType, eventDate, affectedStates) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Catastrophe_Create' |
| 2 | Returns CatastropheID | Output param=1 | Return value=1 |
| 3 | Two output params | SP returns ID and Number | CatastropheID output read |

---

### Test Case ID: UT-CLM-016
**Class**: FraudDataAccess
**Method**: LinkClaimToCatastrophe(claimID, catastropheID)
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Catastrophe_LinkClaim' |
| 2 | Passes 3 parameters | Valid inputs | @ClaimID, @CatastropheID, @LinkedBy |

---

### Test Case ID: UT-CLM-017
**Class**: FraudDataAccess
**Method**: GetCatastropheSummary(catastropheID As Integer) As DataSet
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Catastrophe_GetSummary' |
| 2 | Returns DataSet | Valid ID | DataSet with 3 tables |

---

### Test Case ID: UT-CLM-018
**Class**: FraudDataAccess
**Method**: SearchVendors(vendorType, stateCode, preferredOnly) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock | SP = 'Claims.usp_Vendor_Search' |
| 2 | Passes 4 parameters | Valid inputs | @VendorType, @StateCode, @PreferredOnly, @IsActive=True |
| 3 | Uses ExecuteStoredProcedure | Mock | DatabaseHelper.ExecuteStoredProcedure called |
