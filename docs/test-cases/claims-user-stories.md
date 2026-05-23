# Claims Module - User Stories

## Module: CLM (Claims)
## Test Type: User Stories and Acceptance Criteria

---

### User Story ID: US-CLM-001
**Title**: First Notice of Loss (FNOL) Entry
**Priority**: Critical

#### Story
As a claims representative, I want to create a new claim (FNOL) against an active policy so that we can begin the claims process when a customer reports a loss.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A policy exists with status ACTIVE or PENDING_CANCEL | I enter all required FNOL details and click Save | A new claim is created with status FNOL, ClaimNumber format CLM+7 digits |
| 2 | Loss date is within the policy period | I submit the FNOL | Claim is created successfully |
| 3 | Loss date is outside the policy period | I submit the FNOL | Error: 'Loss date is outside the policy period' |
| 4 | Policy status is CANCELLED | I try to create a claim | Error: 'Policy is not active. Current status: CANCELLED' |
| 5 | Estimated loss > $100,000 | Claim is created | Complexity is set to COMPLEX |
| 6 | Estimated loss > $25,000 and <= $100,000 | Claim is created | Complexity is set to MODERATE |
| 7 | Estimated loss <= $25,000 or NULL | Claim is created | Complexity is set to SIMPLE |

#### Form
- **Screen**: frmClaimFNOL
- **Data Access**: ClaimDataAccess.Create()
- **Stored Procedure**: Claims.usp_Claim_Create

---

### User Story ID: US-CLM-002
**Title**: Claim Assignment
**Priority**: High

#### Story
As a claims supervisor, I want to assign adjusters, vendors, or examiners to a claim so that the claim can be investigated and resolved.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A claim exists in any open status | I assign an adjuster | Assignment record created, AdjusterID updated on claim |
| 2 | Claim is in FNOL status | I assign an adjuster | Claim status auto-transitions to ASSIGNED |
| 3 | Claim is in INVESTIGATING status | I assign an adjuster | Claim status remains INVESTIGATING |
| 4 | An adjuster is already assigned | I assign a different adjuster | Previous assignment marked REASSIGNED, new one created |
| 5 | I assign a vendor | Vendor is assigned | Vendor TotalAssignments incremented |

#### Form
- **Screen**: frmClaimAssignment
- **Data Access**: ClaimDataAccess (via SP)
- **Stored Procedure**: Claims.usp_Claim_Assign

---

### User Story ID: US-CLM-003
**Title**: Reserve Management
**Priority**: Critical

#### Story
As a claims adjuster, I want to set and change reserves on a claim so that we maintain accurate loss projections.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Claim is in an open status (not CLOSED/DENIED) | I set a reserve amount | Reserve record created, claim TotalReserve updated |
| 2 | Reserve amount > $50,000 | I set the reserve | ApprovalRequired = 1, IsApproved = 0, activity shows 'PENDING APPROVAL' |
| 3 | Reserve amount <= $50,000 | I set the reserve | Auto-approved (IsApproved = 1) |
| 4 | Claim is CLOSED | I try to set a reserve | Error: 'Cannot set reserve on a closed or denied claim' |
| 5 | Previous reserve exists for same type/category | I change the amount | PreviousAmount and ChangeAmount calculated correctly |

#### Form
- **Screen**: frmClaimReserve
- **Data Access**: ClaimDataAccess.SetReserve()
- **Stored Procedure**: Claims.usp_Claim_SetReserve

---

### User Story ID: US-CLM-004
**Title**: Payment Processing
**Priority**: Critical

#### Story
As a claims adjuster, I want to create payments against a claim so that we can compensate the insured or pay vendors for services.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Claim is in payable status (not FNOL/CLOSED/DENIED) | I create a payment | Payment record created with PaymentNumber format PAY+7 digits |
| 2 | Payment amount > $10,000 | I create the payment | Status = PENDING, ApprovalRequired = 1, TotalPaid not updated |
| 3 | Payment amount <= $10,000 | I create the payment | Status = APPROVED (auto), TotalPaid updated |
| 4 | TotalPaid + Amount > PolicyLimit | I create the payment | Error: 'Payment would exceed policy limit' |
| 5 | TaxReportable=1, PayeeType=VENDOR, Amount >= $600 | I create the payment | Form1099Required = 1 |
| 6 | Claim is in FNOL status | I try to create a payment | Error: 'Cannot create payment on claim with status: FNOL' |

#### Form
- **Screen**: frmClaimPayment
- **Data Access**: ClaimDataAccess.CreatePayment()
- **Stored Procedure**: Claims.usp_Claim_CreatePayment

---

### User Story ID: US-CLM-005
**Title**: Status Change Management
**Priority**: High

#### Story
As a claims adjuster, I want to change the status of a claim following the allowed transition rules so that the claim progresses through its lifecycle.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Claim is in FNOL status | I change to ASSIGNED | Status updated, StatusHistory logged |
| 2 | Claim is in INVESTIGATING | I change to ASSESSED | Status updated, activity logged |
| 3 | I attempt an invalid transition | I submit the change | Error: 'Invalid status transition from X to Y' |
| 4 | Status changed to CLOSED | ClosedDate is set | ClosedDate = GETDATE() |
| 5 | Status changed to REOPENED | Reason is required [ASSUMPTION] | ReopenedDate set, ReopenedReason stored |
| 6 | Status changed to DENIED | Denial details captured | DenialReason stored |

#### Form
- **Screen**: frmClaimStatusChange
- **Data Access**: ClaimDataAccess.UpdateStatus()
- **Stored Procedure**: Claims.usp_Claim_UpdateStatus

---

### User Story ID: US-CLM-006
**Title**: Fraud Detection and Review
**Priority**: High

#### Story
As a claims investigator, I want to evaluate fraud indicators on a claim and review the fraud score so that I can identify potentially fraudulent claims.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A claim exists | I trigger fraud evaluation | All active indicators evaluated, score calculated (0-100 scale) |
| 2 | Fraud score >= 70 (threshold) | Evaluation completes | Claim auto-referred to SIU (IsSIUReferred = 1) |
| 3 | Fraud score < 70 | Evaluation completes | No SIU referral |
| 4 | Claim reported > 30 days after loss | LATE_REPORT indicator | Triggered with note showing days elapsed |
| 5 | Claim within 60 days of policy inception | NEW_POLICY_CLAIM indicator | Triggered |
| 6 | Customer has 3+ claims in last 3 years | PRIOR_CLAIMS indicator | Triggered |
| 7 | I manually refer to SIU | With a reason | IsSIUReferred = 1, SIU assignment created |

#### Form
- **Screen**: frmFraudReview
- **Data Access**: FraudDataAccess.EvaluateClaim(), FraudDataAccess.ReferToSIU()
- **Stored Procedures**: Claims.usp_Fraud_EvaluateClaim, Claims.usp_Fraud_ReferToSIU

---

### User Story ID: US-CLM-007
**Title**: Subrogation Management
**Priority**: Medium

#### Story
As a claims adjuster, I want to identify subrogation opportunities and track recovery efforts so that we can recover costs from responsible third parties.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A claim exists | I create a subrogation record | SubrogationID returned, claim IsSubrogation = 1, status = IDENTIFIED |
| 2 | Subrogation exists | I update status to DEMAND_SENT | Status updated, activity logged |
| 3 | Subrogation exists | I record a recovery of $5,000 | RecoveryAmount incremented, claim TotalRecovery and TotalSubrogation updated |
| 4 | Recovery recorded | NetIncurred recalculated | NetIncurred = TotalReserve + TotalPaid - TotalRecovery |

#### Form
- **Screen**: frmSubrogation
- **Data Access**: FraudDataAccess.CreateSubrogation(), FraudDataAccess.UpdateSubrogationStatus(), FraudDataAccess.RecordRecovery()
- **Stored Procedures**: Claims.usp_Subrogation_Create, Claims.usp_Subrogation_UpdateStatus, Claims.usp_Subrogation_RecordRecovery

---

### User Story ID: US-CLM-008
**Title**: Catastrophe Event Management
**Priority**: Medium

#### Story
As a claims manager, I want to declare catastrophe events and link claims to them so that we can track and report on disaster-related losses.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | I enter catastrophe details | I create the event | CatastropheID returned, CatastropheNumber format CAT+7 digits |
| 2 | A catastrophe and claim both exist | I link the claim | Claim.CatastropheID set, catastrophe counts updated |
| 3 | Catastrophe is inactive | I try to link a claim | Error: 'Active catastrophe not found' |
| 4 | I view a catastrophe summary | Summary loads | Header, linked claims list, and status breakdown returned |
| 5 | I close a catastrophe | IsActive = 0 | ClosedDate set |

#### Form
- **Screen**: frmCatastropheManager
- **Data Access**: FraudDataAccess.CreateCatastrophe(), FraudDataAccess.LinkClaimToCatastrophe(), FraudDataAccess.GetCatastropheSummary()
- **Stored Procedures**: Claims.usp_Catastrophe_Create, Claims.usp_Catastrophe_LinkClaim, Claims.usp_Catastrophe_GetSummary, Claims.usp_Catastrophe_GetAll, Claims.usp_Catastrophe_Close

---

### User Story ID: US-CLM-009
**Title**: Claims Dashboard
**Priority**: High

#### Story
As a claims adjuster or manager, I want to see a dashboard summarizing open claims, pending items, and high-priority work so that I can prioritize my activities.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | I open the dashboard | Data loads | Open claims grouped by status with counts and reserves |
| 2 | I have an AdjusterID | Dashboard filters by adjuster | Only my assigned claims shown |
| 3 | AdjusterID is NULL | Dashboard shows all | All open claims aggregated |
| 4 | Activities are past due date | Overdue count shown | Count of incomplete activities past due |
| 5 | Payments in PENDING status | Pending payments shown | Count and total of pending approval payments |
| 6 | High/Critical priority claims exist | Priority list shown | Ordered by CRITICAL first, then HIGH |

#### Form
- **Screen**: frmClaimDashboard
- **Data Access**: ClaimDataAccess.GetDashboard()
- **Stored Procedure**: Claims.usp_Claim_GetDashboard

---

### User Story ID: US-CLM-010
**Title**: Vendor Management
**Priority**: Low

#### Story
As a claims coordinator, I want to manage vendors (contractors, adjusters, appraisers) so that we can assign qualified service providers to claims.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | I enter vendor details | I create the vendor | VendorID returned, VendorNumber format VND+7 digits |
| 2 | I search vendors | Results filtered | By name, type, state, preferred status |
| 3 | Search with PreferredOnly = 1 | Only preferred shown | PreferredVendor = 1 results only |
| 4 | Results returned | Sorted correctly | By PreferredVendor DESC, Rating DESC, VendorName |

#### Form
- **Screen**: frmVendorManagement
- **Data Access**: FraudDataAccess.SearchVendors()
- **Stored Procedures**: Claims.usp_Vendor_Create, Claims.usp_Vendor_Search
