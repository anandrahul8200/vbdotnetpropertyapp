# Claims Module - UI Tests

## Module: CLM (Claims)
## Test Type: Screen-Level UI Tests
## Forms Covered (16 total):
1. frmClaimFNOL
2. frmClaimView
3. frmClaimSearch
4. frmClaimDashboard
5. frmClaimStatusChange
6. frmClaimReserve
7. frmClaimPayment
8. frmClaimAssignment
9. frmClaimActivity
10. frmClaimSummaryReport
11. frmFraudReview
12. frmSubrogation
13. frmCatastropheManager
14. frmVendorManagement
15. frmLitigationTracker
16. frmSalvageRecovery

---

### Test Case ID: UI-CLM-001
**Form/Screen**: frmClaimFNOL
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimFNOL.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with policyID=0 | Title='First Notice of Loss (FNOL)', size 700x700, policy info blank |
| 2 | Form opens with valid policyID | Policy info populated (number, customer, address, status) |
| 3 | Claim type dropdown populated | Contains PROPERTY_DAMAGE, THEFT, LIABILITY, WATER_DAMAGE, FIRE, WIND, HAIL, OTHER |
| 4 | Priority dropdown populated | Contains LOW, NORMAL, HIGH, CRITICAL |
| 5 | Weather condition dropdown populated | Contains weather options |
| 6 | Catastrophe dropdown populated | Active catastrophes loaded |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|------------------------|
| 1 | Policy not selected | Click Save with _policyID=0 | Validation error for policy [ASSUMPTION] |
| 2 | Claim type not selected | Leave blank, click Save | Validation error [ASSUMPTION] |
| 3 | Loss description empty | Leave blank, click Save | Validation error [ASSUMPTION] |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Form loaded | btnSave | Enabled |
| 2 | Form loaded | btnCancel | Enabled |
| 3 | Form loaded | btnSelectPolicy | Enabled |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database error during Save | MessageBox with error, form remains open |
| 2 | Exception during Load | MessageBox: 'Error loading form: {message}', ErrorLogger.LogError called |

---

### Test Case ID: UI-CLM-002
**Form/Screen**: frmClaimView
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimView.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with valid claimID | Title='Claim View', size 1000x700, tabs populated |
| 2 | TabControl has 7 tabs | Summary, Coverages, Reserves, Payments, Activities, Status History, Assignments |
| 3 | Header labels populated | ClaimNumber, Status, Type, Customer, Policy, LossDate, Priority, Incurred, FraudScore |
| 4 | DataGridViews configured | ReadOnly, FullRowSelect, no user add rows |

#### Action Button Tests
| # | Button | Action | Expected Behavior |
|---|--------|--------|-------------------|
| 1 | btnSetReserve | Click | Opens frmClaimReserve with _claimID |
| 2 | btnMakePayment | Click | Opens frmClaimPayment with _claimID |
| 3 | btnAddActivity | Click | Opens frmClaimActivity with _claimID |
| 4 | btnChangeStatus | Click | Opens frmClaimStatusChange with _claimID |
| 5 | btnAssign | Click | Opens frmClaimAssignment with _claimID |
| 6 | btnFraudReview | Click | Opens frmFraudReview with _claimID |
| 7 | btnClose | Click | Form closes |

#### Data Display Tests
| # | Tab | Expected Content |
|---|-----|-----------------|
| 1 | Summary | Claim header with policy/customer/property details |
| 2 | Coverages | Grid from result set 2 of usp_Claim_GetDetails |
| 3 | Reserves | Grid ordered by CreatedDate DESC |
| 4 | Payments | Grid ordered by CreatedDate DESC |
| 5 | Activities | Top 50 activities by ActivityDate DESC |
| 6 | Status History | Grid ordered by ChangeDate DESC |
| 7 | Assignments | Grid with vendor info, ordered by AssignmentDate DESC |

---

### Test Case ID: UI-CLM-003
**Form/Screen**: frmClaimSearch
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimSearch.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Grid empty, search criteria fields available |
| 2 | Status dropdown populated | FNOL, ASSIGNED, INVESTIGATING, ASSESSED, APPROVED, DENIED, SETTLED, CLOSED, REOPENED, LITIGATION |
| 3 | Claim type dropdown populated | PROPERTY_DAMAGE, THEFT, etc. |
| 4 | Priority dropdown populated | LOW, NORMAL, HIGH, CRITICAL |

#### Search Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Search by claim number | ClaimDataAccess.Search() called, single result |
| 2 | Search by status | Filtered results |
| 3 | Search by date range | Results within range |
| 4 | Search with pagination | Page navigation works |
| 5 | Double-click result | Opens frmClaimView |

---

### Test Case ID: UI-CLM-004
**Form/Screen**: frmClaimDashboard
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimDashboard.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | ClaimDataAccess.GetDashboard() called, data displayed |
| 2 | Claims by status section | Shows count and reserve by status |
| 3 | New claims section | Last 30 days count and estimated loss |
| 4 | Overdue activities | Count of overdue items |
| 5 | Pending payments | Count and total amount |
| 6 | High priority list | CRITICAL first, then HIGH |

---

### Test Case ID: UI-CLM-005
**Form/Screen**: frmClaimStatusChange
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimStatusChange.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with claimID | Current status displayed |
| 2 | Available transitions shown | Only valid next statuses per transition matrix |

#### Validation Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Select valid transition, click Save | ClaimDataAccess.UpdateStatus() called |
| 2 | Denial requires DenialReason | If DENIED selected, reason field required [ASSUMPTION] |

---

### Test Case ID: UI-CLM-006
**Form/Screen**: frmClaimReserve
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimReserve.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with claimID | Reserve type and category dropdowns loaded |
| 2 | Reserve type options | CASE, EXPENSE, IBNR, BULK |
| 3 | Reserve category options | INDEMNITY, DEFENSE, ADJUSTMENT_EXPENSE, MEDICAL |

#### Save Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Enter valid amount, click Save | ClaimDataAccess.SetReserve() called |
| 2 | Amount > $50K | Visual indicator of approval required [ASSUMPTION] |

---

### Test Case ID: UI-CLM-007
**Form/Screen**: frmClaimPayment
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimPayment.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Create Claim Payment', size 500x420, FixedDialog |
| 2 | Payment type dropdown | INDEMNITY, EXPENSE, PARTIAL, FINAL, SUPPLEMENT (default=INDEMNITY) |
| 3 | Payment method dropdown | CHECK, EFT, WIRE, DRAFT (default=CHECK) |
| 4 | Payee type dropdown | INSURED, VENDOR, ATTORNEY, MORTGAGEE, LIENHOLDER (default=INSURED) |
| 5 | Tax reportable checkbox | chkTaxReportable unchecked by default |

#### Validation Tests
| # | Field | Action | Expected Error |
|---|-------|--------|----------------|
| 1 | txtPayeeName | Leave blank, click Save | Validation error [ASSUMPTION] |
| 2 | txtAmount | Leave blank or zero, click Save | Validation error [ASSUMPTION] |
| 3 | txtAmount | Non-numeric value | Validation error [ASSUMPTION] |

#### Save Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Valid payment data | ClaimDataAccess.CreatePayment() called with ClaimPaymentDTO |

---

### Test Case ID: UI-CLM-008
**Form/Screen**: frmClaimAssignment
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimAssignment.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with claimID | Assignment type options loaded |
| 2 | Assignee type options | ADJUSTER, VENDOR, EXAMINER, SIU |

#### Save Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Select adjuster, click Assign | usp_Claim_Assign called |

---

### Test Case ID: UI-CLM-009
**Form/Screen**: frmClaimActivity
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimActivity.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens with claimID | Activity type dropdown loaded |
| 2 | Activity type options | NOTE, PHONE_CALL, EMAIL, INSPECTION, DOCUMENT |
| 3 | Priority options | NORMAL, HIGH, LOW |

#### Save Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Enter subject and description, Save | ClaimDataAccess.CreateActivity() called |

---

### Test Case ID: UI-CLM-010
**Form/Screen**: frmClaimSummaryReport
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmClaimSummaryReport.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Report criteria available |
| 2 | Report generates | Summary data displayed |

---

### Test Case ID: UI-CLM-011
**Form/Screen**: frmFraudReview
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmFraudReview.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Fraud Review', size 800x550 |
| 2 | Header shows claim info | ClaimNumber, FraudScore, SIU status |
| 3 | Indicators grid | ReadOnly, FullRowSelect, AutoSizeColumns |
| 4 | Score gauge panel | Visual score representation (250x60) |

#### Button Tests
| # | Button | Action | Expected Behavior |
|---|--------|--------|-------------------|
| 1 | btnEvaluate ('Re-Evaluate') | Click | FraudDataAccess.EvaluateClaim() called, score refreshed |
| 2 | btnReferSIU ('Refer to SIU') | Click without reason | Validation: reason required [ASSUMPTION] |
| 3 | btnReferSIU ('Refer to SIU') | Click with reason | FraudDataAccess.ReferToSIU() called |
| 4 | btnClose | Click | Form closes |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Exception during Load | MessageBox: 'Error: {message}', ErrorLogger.LogError called |

---

### Test Case ID: UI-CLM-012
**Form/Screen**: frmSubrogation
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmSubrogation.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Subrogation entry/management interface |
| 2 | Existing subrogation records loaded | From usp_Subrogation_GetByClaim |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Create subrogation | FraudDataAccess.CreateSubrogation() called |
| 2 | Update status | FraudDataAccess.UpdateSubrogationStatus() called |
| 3 | Record recovery | FraudDataAccess.RecordRecovery() called |

---

### Test Case ID: UI-CLM-013
**Form/Screen**: frmCatastropheManager
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmCatastropheManager.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Active catastrophes listed |
| 2 | Catastrophe type options | HURRICANE, TORNADO, EARTHQUAKE, FLOOD, WILDFIRE, HAIL, WINTER_STORM |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Create catastrophe | FraudDataAccess.CreateCatastrophe() called |
| 2 | Link claim | FraudDataAccess.LinkClaimToCatastrophe() called |
| 3 | View summary | FraudDataAccess.GetCatastropheSummary() called |

---

### Test Case ID: UI-CLM-014
**Form/Screen**: frmVendorManagement
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmVendorManagement.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Vendor search/management interface |
| 2 | Vendor type options | ADJUSTER, CONTRACTOR, APPRAISER, ENGINEER, ATTORNEY, INVESTIGATOR |

#### Search Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Search vendors | FraudDataAccess.SearchVendors() called |
| 2 | Filter by type | Results filtered by VendorType |
| 3 | Preferred only checkbox | PreferredOnly parameter passed |

---

### Test Case ID: UI-CLM-015
**Form/Screen**: frmLitigationTracker
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmLitigationTracker.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Open litigation claims loaded from usp_Litigation_GetOpen |
| 2 | Grid shows | ClaimNumber, ClaimType, LitigationDate, AttorneyName, NetIncurred, CustomerName |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Update attorney | usp_Litigation_Update called |

---

### Test Case ID: UI-CLM-016
**Form/Screen**: frmSalvageRecovery
**File**: `src/PropertyInsuranceClaims/Forms/Claims/frmSalvageRecovery.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Salvage/recovery management interface |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Record salvage recovery | Recovery amount applied to claim totals [ASSUMPTION] |
