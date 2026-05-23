# Policy Module - Functional Tests

## Module: POL (Policy)
## Test Type: End-to-End Business Workflow Tests

---

### Test Case ID: FT-POL-001
**Workflow**: Create Customer and Add Property
**Module**: POL
**Priority**: Critical

#### Pre-conditions
- [ ] User is logged in with policy entry permissions
- [ ] Database is accessible

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmCustomerEntry | -- | New (customerID=0) | Form loads with title 'New Customer', all fields empty |
| 2 | Select customer type | cboCustomerType | Individual | Type set to 'I' |
| 3 | Enter first name | txtFirstName | John | Field accepts input |
| 4 | Enter last name | txtLastName | Doe | Field accepts input |
| 5 | Enter address | txtAddressLine1 | 123 Main St | Field accepts input |
| 6 | Enter city | txtCity | Austin | Field accepts input |
| 7 | Select state | cboState | TX | State selected |
| 8 | Enter zip | txtZipCode | 78701 | Field accepts input |
| 9 | Enter credit score | txtCreditScore | 780 | Field accepts input |
| 10 | Click Save | btnSave | -- | CustomerDataAccess.Create called, MessageBox 'Customer saved successfully.' |
| 11 | Verify customer number | lblCustomerNumber | -- | Displays CUS+7digits |
| 12 | Open frmPropertyEntry | -- | New (customerID from step 10) | Form loads with title 'New Property' |
| 13 | Enter address | txtAddressLine1 | 456 Oak Ave | Field accepts input |
| 14 | Enter year built | txtYearBuilt | 2005 | Field accepts input |
| 15 | Enter square footage | txtSquareFootage | 2500 | Field accepts input |
| 16 | Select property type | cboPropertyType | SINGLE_FAMILY | Type selected |
| 17 | Click Save | btnSave | -- | PropertyDataAccess.Create called, MessageBox 'Property saved successfully.' |

#### Post-conditions
- [ ] Customer record exists in Policy.Customers with generated CustomerNumber
- [ ] Property record exists in Policy.Properties linked to CustomerID
- [ ] Audit log entries created for both INSERT operations
- [ ] RiskTier set to 'PREFERRED' (credit score 780 >= 750)

#### Related Artifacts
- Stored Procedure: Policy.usp_Customer_Create (SP-POL-001)
- Stored Procedure: Policy.usp_Property_Create (SP-POL-005)
- Form: frmCustomerEntry (UI-POL-001)
- Form: frmPropertyEntry (UI-POL-003)
- Business Rule: BR-POL-001 (Credit Score Risk Tier)

---

### Test Case ID: FT-POL-002
**Workflow**: Create Policy Quote
**Module**: POL
**Priority**: Critical

#### Pre-conditions
- [ ] Active customer exists (from FT-POL-001)
- [ ] Active property exists linked to customer
- [ ] Active agent exists in system
- [ ] No moratorium in effect for the location/policy type

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmPolicyEntry | -- | New (policyID=0) | Form loads with title 'New Policy Quote' |
| 2 | Select policy type | cboPolicyType | HO3 | Type selected |
| 3 | Click Select Customer | btnSelectCustomer | -- | frmCustomerSearch opens as dialog |
| 4 | Search customer | txtSearch, btnSearch | 'Doe' | Grid shows matching customers |
| 5 | Double-click customer | dgvResults | -- | Dialog closes, lblCustomerName shows selected customer |
| 6 | Click Select Property | btnSelectProperty | -- | frmPropertySearch opens with customerID |
| 7 | Select property | dgvResults / btnOpen | -- | Dialog closes, lblPropertyAddress shows property |
| 8 | Select agent | cboAgent | Agent from list | Agent selected |
| 9 | Set effective date | dtpEffectiveDate | 01/01/2025 | Date set |
| 10 | Set term | cboTermMonths | 12 | 12-month term |
| 11 | Set payment plan | cboPaymentPlan | ANNUAL | Annual billing |
| 12 | Enter prior carrier | txtPriorCarrier | State Farm | Prior info captured |
| 13 | Enter claim-free years | txtClaimFreeYears | 3 | Years set |
| 14 | Click Save | btnSave | -- | PolicyDataAccess.CreateQuote called, PolicyNumber assigned, status shows [QUOTE] |

#### Post-conditions
- [ ] Policy record exists in Policy.Policies with PolicyStatus='QUOTE'
- [ ] PolicyNumber format: POL+7digits
- [ ] PolicyVersions record created (VersionNumber=1, VersionType='NEW')
- [ ] ExpiryDate = EffectiveDate + 12 months
- [ ] CommissionRate populated from agent record

#### Related Artifacts
- Stored Procedure: Policy.usp_Policy_CreateQuote (SP-POL-008)
- Form: frmPolicyEntry (UI-POL-005)
- Form: frmCustomerSearch (UI-POL-002)
- Form: frmPropertySearch (UI-POL-004)
- Business Rule: BR-POL-003 (Moratorium Check)
- Business Rule: BR-POL-005 (Policy Number Format)

---

### Test Case ID: FT-POL-003
**Workflow**: Calculate Premium and Bind Policy
**Module**: POL
**Priority**: Critical

#### Pre-conditions
- [ ] Policy exists in QUOTE status (from FT-POL-002)
- [ ] Coverages loaded in grid (default coverages populated)

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Verify coverage grid | dgvCoverages | -- | Default coverages shown: DWELLING, OTHER_STRUCTURES, PERSONAL_PROPERTY, LOSS_OF_USE, LIABILITY, MEDICAL selected; FLOOD, EARTHQUAKE, SEWER_BACKUP, JEWELRY unselected |
| 2 | Modify dwelling limit | dgvCoverages[DWELLING].LimitAmount | 350000 | Value accepted |
| 3 | Click Calculate | btnCalculate | -- | Premium column populated for selected coverages, totals updated |
| 4 | Verify premium display | lblTotalPremium | -- | Shows calculated premium |
| 5 | Verify taxes display | lblTotalTaxes | -- | Shows 3.5% of premium |
| 6 | Verify gross premium | lblGrossPremium | -- | Shows premium + taxes |
| 7 | Verify Bind enabled | btnBind | -- | Button enabled after calculation |
| 8 | Click Save | btnSave | -- | Coverages saved via usp_Policy_SaveCoverage, premium updated via usp_Policy_UpdatePremium |
| 9 | Click Bind | btnBind | -- | Confirmation dialog: 'Bind this policy? This will make it active.' |
| 10 | Confirm bind | Yes | -- | usp_Policy_Bind called, status changes to [ACTIVE], success message |

#### Post-conditions
- [ ] Policy.PolicyStatus = 'ACTIVE'
- [ ] All selected coverages saved in Policy.Coverages
- [ ] AnnualPremium, TotalFees, GrossPremium updated on policy
- [ ] AuditLog entry with Action='BIND'

#### Related Artifacts
- Stored Procedure: Policy.usp_Policy_SaveCoverage (SP-POL-014)
- Stored Procedure: Policy.usp_Policy_UpdatePremium (SP-POL-015)
- Stored Procedure: Policy.usp_Policy_Bind (SP-POL-011)
- Form: frmPolicyEntry (UI-POL-005)
- Business Rule: BR-POL-004 (Bind requires QUOTE status)

---

### Test Case ID: FT-POL-004
**Workflow**: Search and View Policy
**Module**: POL
**Priority**: High

#### Pre-conditions
- [ ] Active policy exists (from FT-POL-003)

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmPolicySearch | -- | -- | Form loads with empty search fields |
| 2 | Enter policy number | txtPolicyNumber | POL0000001 | Field accepts input |
| 3 | Click Search | btnSearch | -- | PolicyDataAccess.Search called, grid shows matching policy |
| 4 | Verify record count | lblRecordCount | -- | Shows '1 record(s)' |
| 5 | Double-click row | dgvResults | -- | frmPolicyView opens with policyID |
| 6 | Verify header | lblPolicyNumber | -- | Shows policy number in bold |
| 7 | Verify status | lblStatus | -- | Shows 'Status: ACTIVE' |
| 8 | Verify customer | lblCustomer | -- | Shows customer name and number |
| 9 | Verify tabs | tabControl | -- | Tabs: Summary, Coverages, Claims, Billing, Notes |
| 10 | Click Coverages tab | tabCoverages | -- | Grid shows policy coverages |

#### Post-conditions
- [ ] PolicyDataAccess.GetDetails called with policyID
- [ ] All 6 result sets from SP bound to respective UI elements

#### Related Artifacts
- Stored Procedure: Policy.usp_Policy_Search (SP-POL-010)
- Stored Procedure: Policy.usp_Policy_GetDetails (SP-POL-009)
- Form: frmPolicySearch (UI-POL-006)
- Form: frmPolicyView (UI-POL-007)

---

### Test Case ID: FT-POL-005
**Workflow**: Cancel Policy
**Module**: POL
**Priority**: High

#### Pre-conditions
- [ ] Active policy exists with known AnnualPremium
- [ ] Cancel date is mid-term (between effective and expiry)

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmPolicyCancellation | -- | policyID | Form loads with policy info displayed |
| 2 | Select cancel reason | cboCancelReason | NON_PAYMENT | Reason selected |
| 3 | Set cancel date | dtpCancelDate | Mid-term date | Date set |
| 4 | Select calculation method | cboCalculationMethod | PRO_RATA | Method selected |
| 5 | Click Calculate | btnCalculate | -- | Earned premium and return premium displayed |
| 6 | Verify return premium | lblReturnPremium | -- | Shows calculated return amount |
| 7 | Click Process | btnProcess | -- | Confirmation: 'Cancel this policy? This cannot be undone.' |
| 8 | Confirm | Yes | -- | Policy cancelled, refund message shown |

#### Post-conditions
- [ ] Policy.PolicyStatus = 'CANCELLED'
- [ ] ReturnPremium calculated via pro-rata formula
- [ ] AuditLog entry with Action='CANCEL'
- [ ] Form closes with DialogResult.OK

#### Related Artifacts
- Stored Procedure: Policy.usp_Policy_Cancel (SP-POL-012)
- Form: frmPolicyCancellation (UI-POL-011)
- Business Rule: BR-POL-006 (Pro-rata cancellation calculation)

---

### Test Case ID: FT-POL-006
**Workflow**: Reinstate Cancelled Policy
**Module**: POL
**Priority**: High

#### Pre-conditions
- [ ] Policy in CANCELLED status exists

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmPolicyReinstatement | -- | policyID | Form loads showing policy info, cancel date, reason |
| 2 | Set reinstatement date | dtpReinstatementDate | Today | Date set |
| 3 | Check require payment | chkRequirePayment | Checked | Payment required |
| 4 | Enter payment amount | txtPaymentAmount | 450.00 | Amount entered |
| 5 | Optionally check backdate | chkBackdateEffective | -- | Backdate option |
| 6 | Enter conditions | txtConditions | 'Payment received' | Text entered |
| 7 | Click Reinstate | btnReinstate | -- | Confirmation: 'Reinstate this policy?' |
| 8 | Confirm | Yes | -- | 'Policy reinstated successfully.' message |

#### Post-conditions
- [ ] Policy.PolicyStatus = 'ACTIVE'
- [ ] AuditLog entry with Action='REINSTATE'
- [ ] Form closes with DialogResult.OK

#### Related Artifacts
- Stored Procedure: Policy.usp_Policy_Reinstate (SP-POL-013)
- Form: frmPolicyReinstatement (UI-POL-012)

---

### Test Case ID: FT-POL-007
**Workflow**: Process Endorsement (Mid-term Change)
**Module**: POL
**Priority**: High

#### Pre-conditions
- [ ] Active policy exists with coverages

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmEndorsement | -- | policyID | Form loads with policy info, current premium, coverage grid |
| 2 | Select endorsement type | cboEndorsementType | INCREASE_LIMITS | Type selected |
| 3 | Set effective date | dtpEffectiveDate | Mid-term date | Date within policy term |
| 4 | Modify coverage limit | dgvCoverages[NewLimit] | 350000 | New limit entered |
| 5 | Enter description | txtDescription | 'Increase dwelling limit' | Description entered |
| 6 | Click Calculate | btnCalculate | -- | Pro-rata factor calculated and displayed |
| 7 | Verify premium impact | lblPremiumChange | -- | Shows premium change amount |
| 8 | Click Process | btnProcess | -- | Confirmation: 'Process this endorsement?' |
| 9 | Confirm | Yes | -- | 'Endorsement processed successfully.' |

#### Post-conditions
- [ ] Pro-rata factor correctly computed based on remaining days in term
- [ ] Form closes with DialogResult.OK

#### Related Artifacts
- Form: frmEndorsement (UI-POL-010)
- Stored Procedure: Policy.usp_Policy_GetDetails (SP-POL-009)

---

### Test Case ID: FT-POL-008
**Workflow**: Renewal Processing
**Module**: POL
**Priority**: High

#### Pre-conditions
- [ ] Policies exist that expire within 30 days

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmRenewal | -- | -- | Form loads with empty expiring policies grid |
| 2 | Click Load Expiring | btnLoadExpiring | -- | Calls Batch.usp_Renewal_GetDuePolicies, grid populated |
| 3 | Select policy | dgvExpiringPolicies | Row click | btnSelectPolicy enabled |
| 4 | Click Select for Renewal | btnSelectPolicy | -- | Renewal details panel shown with policy info, coverage comparison |
| 5 | Verify current premium | lblCurrentPremium | -- | Shows current annual premium |
| 6 | Click Calculate | btnCalculate | -- | New premium calculated (simulated 5% increase), comparison shown |
| 7 | Verify change display | lblPremiumChange | -- | Shows change amount and percentage |
| 8 | Click Renew | btnRenew | -- | Confirmation: 'Process this renewal?' |
| 9 | Confirm | Yes | -- | 'Policy renewed successfully.' |

#### Post-conditions
- [ ] Renewal processed, details panel hidden
- [ ] Expiring policies list refreshed

#### Related Artifacts
- Form: frmRenewal (UI-POL-014)
- Stored Procedure: Batch.usp_Renewal_GetDuePolicies

---

### Test Case ID: FT-POL-009
**Workflow**: Policy Dashboard Overview
**Module**: POL
**Priority**: Medium

#### Pre-conditions
- [ ] User logged in with policy permissions
- [ ] Policies, claims data exists in system

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmPolicyDashboard | -- | -- | Dashboard loads with KPI cards, grids |
| 2 | Verify KPI cards | All KPI panels | -- | Active Policies, New Business, Renewals, Cancellations, Written Premium, Loss Ratio displayed |
| 3 | Verify expiring policies grid | dgvExpiringPolicies | -- | Shows policies expiring in next 30 days |
| 4 | Verify recent activity | dgvRecentActivity | -- | Shows last 7 days of activity |
| 5 | Click New Quote | btnNewQuote | -- | frmPolicyEntry opens as dialog |
| 6 | Click Customer Search | btnCustomerSearch | -- | frmCustomerSearch opens as dialog |
| 7 | Click Policy Search | btnPolicySearch | -- | frmPolicySearch opens |
| 8 | Click Refresh | btnRefresh | -- | Dashboard data reloaded |

#### Post-conditions
- [ ] Reporting.usp_Dashboard_PolicyKPIs called with today's date
- [ ] All KPI values populated from result set

#### Related Artifacts
- Stored Procedure: Reporting.usp_Dashboard_PolicyKPIs
- Form: frmPolicyDashboard (UI-POL-008)

---

### Test Case ID: FT-POL-010
**Workflow**: Agent Search and Production Review
**Module**: POL
**Priority**: Medium

#### Pre-conditions
- [ ] Active agents exist in system

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|---------------|---------------|-------------|-----------------|
| 1 | Open frmAgentSearch | -- | -- | Form loads with search fields |
| 2 | Enter agent name | txtAgentName | 'Smith' | Name entered |
| 3 | Select type | cboAgentType | (All) | No type filter |
| 4 | Click Search | btnSearch | -- | Grid populated with matching agents, count label updated |
| 5 | Verify results | dgvAgents | -- | Shows AgentID, AgentNumber, AgentType, FirstName, LastName, Email, Phone, CommissionRate, IsActive |

#### Post-conditions
- [ ] Policy.usp_Agent_Search called with parameters
- [ ] Results displayed in grid

#### Related Artifacts
- Stored Procedure: Policy.usp_Agent_Search (SP-POL-016)
- Form: frmAgentSearch (UI-POL-009)
