# Policy Module - UI Tests

## Module: POL (Policy)
## Test Type: Screen-Level UI Tests
## Forms Covered (14 total):
1. frmCustomerEntry
2. frmCustomerSearch
3. frmPropertyEntry
4. frmPropertySearch
5. frmPolicyEntry
6. frmPolicySearch
7. frmPolicyView
8. frmPolicyDashboard
9. frmAgentSearch
10. frmCoverageEditor
11. frmEndorsement
12. frmPolicyCancellation
13. frmPolicyReinstatement
14. frmRenewal

---

### Test Case ID: UI-POL-001
**Form/Screen**: frmCustomerEntry
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmCustomerEntry.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens for new customer (ID=0) | Title = 'New Customer', size 600x650, all fields empty |
| 2 | Form opens for existing customer (ID>0) | Title = 'Edit Customer', fields populated from CustomerDataAccess.GetByID |
| 3 | Customer type dropdown populated | Contains 'Individual' and 'Commercial' options |
| 4 | Default customer type | First item selected (Individual) |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|------------------------|
| 1 | txtFirstName + txtCompanyName | Both blank, click Save | 'First Name or Company Name is required.' |
| 2 | txtAddressLine1 | Leave blank, click Save | 'Address is required.' |
| 3 | txtCity | Leave blank, click Save | 'City is required.' |
| 4 | txtZipCode | Leave blank, click Save | 'Zip Code is required.' |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Form loaded | btnSave | Enabled |
| 2 | Form loaded | btnCancel | Enabled |

#### Dirty Flag / Unsaved Changes Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Modify any field, click Close (X) | Warning: 'You have unsaved changes. Discard?' YesNo |
| 2 | Save successfully, then close | No warning, closes normally |
| 3 | Open form, change nothing, close | No warning, closes normally |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database error during Save | MessageBox: 'Error saving: {message}', form remains open |
| 2 | Customer not found during edit load | MessageBox: 'Customer not found.', form closes |
| 3 | Any exception during Load | MessageBox: 'Error loading form: {message}' |

---

### Test Case ID: UI-POL-002
**Form/Screen**: frmCustomerSearch
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmCustomerSearch.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Customer Search', size 1000x600, grid empty |
| 2 | Dropdowns populated | Customer type has (All)/Individual/Commercial; State loaded |
| 3 | Active Only checked by default | chkActiveOnly.Checked = True |
| 4 | Open button disabled initially | btnOpen.Enabled = False |

#### Search Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Click Search with criteria | Grid populated, CustomerID column hidden |
| 2 | Record count updated | lblRecordCount shows 'X record(s) found' |
| 3 | Page label updated | lblPage shows 'Page 1' |
| 4 | Enter key in search field | Triggers search (KeyDown handler) |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click New Customer | frmCustomerEntry opens as dialog with ID=0 |
| 2 | Click Open (row selected) | frmCustomerEntry opens with selected CustomerID |
| 3 | Double-click grid row | Same as Open button |
| 4 | Next page button | PageNumber incremented, search re-executed |
| 5 | Previous page button | PageNumber decremented (min 1) |
| 6 | Used as dialog (Modal=True) | Double-click sets SelectedCustomerID and closes |

#### Clear Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Clear | All search fields reset, grid cleared, btnOpen disabled |

---

### Test Case ID: UI-POL-003
**Form/Screen**: frmPropertyEntry
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPropertyEntry.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens for new property | Title = 'New Property', size 650x750, FixedDialog |
| 2 | Form opens for edit | Title = 'Edit Property', fields populated |
| 3 | Dropdowns populated | PropertyType (6 items), ConstructionType (5), OccupancyType (5), RoofType (6), FloodZone (11) |
| 4 | Default selections | All dropdowns default to index 0 |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|------------------------|
| 1 | txtAddressLine1 | Leave blank, click Save | 'Address is required.' |
| 2 | txtYearBuilt | Leave blank or non-numeric | 'Valid year built is required.' |
| 3 | txtSquareFootage | Leave blank or non-numeric | 'Valid square footage is required.' |

#### Dirty Flag / Unsaved Changes Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Modify field, close form | Warning: 'Discard unsaved changes?' YesNo |
| 2 | Save then close | No warning |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Save fails | MessageBox: 'Error saving: {message}', form remains open |

---

### Test Case ID: UI-POL-004
**Form/Screen**: frmPropertySearch
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPropertySearch.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Select Property', size 750x400, FixedDialog |
| 2 | Properties loaded | PropertyDataAccess.GetByCustomer called, grid populated |
| 3 | Select button state | Enabled only if properties exist |
| 4 | Info label | Shows 'Select a property or create a new one:' |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Select | Sets SelectedPropertyID from grid, closes form |
| 2 | Double-click row | Same as Select |
| 3 | Click New Property | frmPropertyEntry opens as dialog for customerID |
| 4 | After new property added | Grid refreshes (LoadProperties called again) |
| 5 | Click Cancel | SelectedPropertyID = 0, form closes |

---

### Test Case ID: UI-POL-005
**Form/Screen**: frmPolicyEntry
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicyEntry.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | New policy form | Title = 'New Policy Quote', size 800x700, FixedDialog |
| 2 | Edit policy form | Title = 'Edit Policy', data loaded from PolicyDataAccess.GetDetails |
| 3 | Lookups populated | PolicyType: HO3/HO4/HO6/DP3/BOP; Term: 6/12; PaymentPlan: ANNUAL/SEMI_ANNUAL/QUARTERLY/MONTHLY |
| 4 | Agent dropdown loaded | Calls Policy.usp_Agent_Search, populates cboAgent |
| 5 | Coverage grid defaults | 10 rows: 6 selected (DWELLING through MEDICAL), 4 unselected |
| 6 | Bind button disabled initially | btnBind.Enabled = False |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|------------------------|
| 1 | Customer not selected | Click Save with _customerID=0 | 'Please select a customer.' |
| 2 | Property not selected | Click Save with _propertyID=0 | 'Please select a property.' |
| 3 | Property before customer | Click Select Property with no customer | 'Please select a customer first.' |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Before Calculate | btnBind | Disabled |
| 2 | After Calculate | btnBind | Enabled |

#### Dirty Flag / Unsaved Changes Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Select customer, close | Warning: 'Discard unsaved changes?' |
| 2 | Select property, close | Warning displayed |

---

### Test Case ID: UI-POL-006
**Form/Screen**: frmPolicySearch
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicySearch.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Policy Search', size 1100x600 |
| 2 | Lookups | PolicyType: (All)/HO3/HO4/HO6/DP3/BOP; Status: (All)/QUOTE/REFERRED/BOUND/ACTIVE/PENDING_CANCEL/CANCELLED/EXPIRED |
| 3 | Date pickers disabled | dtpEffectiveFrom and dtpEffectiveTo disabled until chkDateFilter checked |
| 4 | Open button disabled | btnOpen.Enabled = False |

#### Search Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Search with filters | Grid populated, PolicyID column hidden |
| 2 | Date filter toggle | Checking chkDateFilter enables date pickers |
| 3 | Clear | All fields reset, grid cleared |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Open (normal mode) | frmPolicyView opens as MDI child |
| 2 | Click Open (picker mode) | Sets SelectedPolicyID, closes form |
| 3 | Double-click row | Same as Open button |
| 4 | Click New Quote | frmPolicyEntry opens as dialog with ID=0 |
| 5 | Pagination | Next/Prev update PageNumber and re-execute search |

---

### Test Case ID: UI-POL-007
**Form/Screen**: frmPolicyView
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicyView.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Policy View', size 900x650, data loaded from PolicyDataAccess.GetDetails |
| 2 | Header populated | Policy number (bold, 14pt), status, type, customer, property, agent, dates, premium |
| 3 | Tabs present | Summary, Coverages, Claims, Billing, Notes |
| 4 | Coverages grid | Populated from result set index 1 |
| 5 | Claims grid | Populated from result set index 3 |
| 6 | Billing grid | Populated from result set index 4 |
| 7 | Notes grid | Populated from result set index 5 |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Endorsement | MessageBox: 'Endorsement form would open here.' |
| 2 | Click New Claim | frmClaimFNOL opens as dialog |
| 3 | Click Renew | MessageBox: 'Renewal processing would start here.' |
| 4 | Click Close | Form closes |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Policy not found | MessageBox: 'Policy not found.', form closes |

---

### Test Case ID: UI-POL-008
**Form/Screen**: frmPolicyDashboard
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicyDashboard.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Dashboard opens | Title = 'Policy Dashboard', size 1100x750 |
| 2 | KPI cards displayed | 6 cards: Active Policies, New Business, Renewals, Cancellations, Written Premium, Loss Ratio |
| 3 | Period dropdown | Options: This Month/Last Month/This Quarter/This Year/Last 12 Months |
| 4 | Data loads automatically | Reporting.usp_Dashboard_PolicyKPIs called on load |
| 5 | Loss ratio color coding | Green (<= 55%), DarkOrange (55-70%), Red (> 70%) |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click New Quote | frmPolicyEntry opens as dialog |
| 2 | Click Customer Search | frmCustomerSearch opens as dialog |
| 3 | Click Policy Search | frmPolicySearch opens as MDI child |
| 4 | Click Referral Queue | frmReferralQueue opens as MDI child |
| 5 | Click Refresh | LoadDashboard re-executed |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database error during load | Error logged via ErrorLogger, dashboard shows partial/empty data (no crash) |

---

### Test Case ID: UI-POL-009
**Form/Screen**: frmAgentSearch
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmAgentSearch.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Agent Search', size 800x450 |
| 2 | Agent type dropdown | (All)/CAPTIVE/INDEPENDENT/BROKER, default (All) |

#### Search Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Click Search | Policy.usp_Agent_Search called, grid populated |
| 2 | Agent count label | Shows 'X agent(s)' |

---

### Test Case ID: UI-POL-010
**Form/Screen**: frmEndorsement
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmEndorsement.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Process Endorsement', size 800x600, FixedDialog |
| 2 | Policy data loaded | Policy number, type, current premium displayed |
| 3 | Endorsement types | INCREASE_LIMITS/DECREASE_LIMITS/ADD_COVERAGE/REMOVE_COVERAGE/CHANGE_DEDUCTIBLE/ADDRESS_CHANGE/MORTGAGEE_CHANGE/OTHER |
| 4 | Coverage grid populated | Current limits and deductibles from policy coverages |
| 5 | Process button disabled | btnProcess.Enabled = False until Calculate clicked |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Before Calculate | btnProcess | Disabled |
| 2 | After Calculate | btnProcess | Enabled |

---

### Test Case ID: UI-POL-011
**Form/Screen**: frmPolicyCancellation
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicyCancellation.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Cancel Policy', size 500x400, FixedDialog |
| 2 | Cancel reasons | NON_PAYMENT/INSURED_REQUEST/UNDERWRITING/MATERIAL_MISREPRESENTATION/PROPERTY_SOLD/DUPLICATE_COVERAGE/OTHER |
| 3 | Calculation methods | PRO_RATA/SHORT_RATE/FLAT, default PRO_RATA |
| 4 | Process button disabled | btnProcess.Enabled = False |
| 5 | Premium labels | Show '--' initially |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | Before Calculate | btnProcess | Disabled |
| 2 | After Calculate | btnProcess | Enabled |

---

### Test Case ID: UI-POL-012
**Form/Screen**: frmPolicyReinstatement
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmPolicyReinstatement.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Reinstate Policy', size 500x380, FixedDialog |
| 2 | Policy info displayed | Policy ID, cancel date, cancel reason shown |
| 3 | Require payment checked | chkRequirePayment.Checked = True by default |
| 4 | Default payment amount | txtPaymentAmount shows '450.00' |

---

### Test Case ID: UI-POL-013
**Form/Screen**: frmCoverageEditor
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmCoverageEditor.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Coverage Editor - Policy {ID}', size 750x450 |
| 2 | Coverage grid populated | 8 default rows with Active/Code/Name/Limit/Deductible/Premium columns |
| 3 | Total premium shown | lblTotalPremium displays sum |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Add Coverage | MessageBox: 'Add coverage dialog would open.' |
| 2 | Click Remove | Selected row removed from grid |
| 3 | Click Save | MessageBox: 'Coverages saved.' |
| 4 | Click Close | Form closes |

---

### Test Case ID: UI-POL-014
**Form/Screen**: frmRenewal
**File**: `src/PropertyInsuranceClaims/Forms/Policy/frmRenewal.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title = 'Renewal Processing', size 1000x700 |
| 2 | Renewal details hidden | grpRenewalDetails.Visible = False initially |
| 3 | Select button disabled | btnSelectPolicy.Enabled = False |
| 4 | Term dropdown | 6/12 months, default 12 |
| 5 | Payment plan dropdown | ANNUAL/SEMI_ANNUAL/QUARTERLY/MONTHLY |

#### Action Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Click Load Expiring | Batch.usp_Renewal_GetDuePolicies called, grid populated |
| 2 | Select policy | Renewal details shown, coverage comparison grid populated |
| 3 | Click Calculate | New premium computed (5% increase simulated), btnRenew enabled |
| 4 | Click Non-Renew | Confirmation: 'Non-renew this policy? This cannot be undone.' |
| 5 | Click Close | Form closes |
