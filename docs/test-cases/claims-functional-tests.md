# Claims Module - Functional Tests

## Module: CLM (Claims)
## Test Type: End-to-End Functional Tests

---

### Test Case ID: FT-CLM-001
**Workflow**: FNOL Creation and Initial Processing
**Priority**: Critical

#### Preconditions
- Active policy exists with valid coverage period
- User has claims entry permission

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmClaimFNOL with active PolicyID | Form loads, policy info displayed |
| 2 | Select claim type (FIRE) | ClaimType set |
| 3 | Enter loss date within policy period | Date accepted |
| 4 | Enter loss description | Text stored |
| 5 | Enter estimated loss ($50,000) | Amount accepted |
| 6 | Click Save | ClaimDataAccess.Create() called |
| 7 | Verify claim created | ClaimID > 0, ClaimNumber=CLM+7 digits, Status=FNOL |
| 8 | Verify complexity | Complexity=MODERATE ($50K > $25K) |
| 9 | Verify status history | Row with NewStatus='FNOL' |
| 10 | Verify activity log | 'FNOL Received' activity created |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter loss date outside policy period | Error: 'Loss date is outside the policy period' |
| 2 | Submit against CANCELLED policy | Error: 'Policy is not active. Current status: CANCELLED' |

---

### Test Case ID: FT-CLM-002
**Workflow**: Claim Assignment and Status Transition
**Priority**: High

#### Preconditions
- Claim exists in FNOL status
- Adjuster available in system

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmClaimAssignment for FNOL claim | Form loads |
| 2 | Select AssigneeType = ADJUSTER | Type selected |
| 3 | Select valid AdjusterID | Adjuster selected |
| 4 | Click Assign | Claims.usp_Claim_Assign called |
| 5 | Verify assignment created | AssignmentID > 0, Status='ASSIGNED' |
| 6 | Verify claim status changed | ClaimStatus='ASSIGNED' (auto from FNOL) |
| 7 | Verify status history | Row: FNOL -> ASSIGNED, Reason='Adjuster assigned' |
| 8 | Open frmClaimStatusChange | Form loads with current status |
| 9 | Change to INVESTIGATING | Valid transition, status updated |
| 10 | Verify only valid options shown | Only ASSESSED, DENIED, CLOSED, LITIGATION available |

---

### Test Case ID: FT-CLM-003
**Workflow**: Payment Creation, Approval, and Voiding
**Priority**: Critical

#### Preconditions
- Claim in ASSIGNED or later status (not FNOL/CLOSED/DENIED)
- PolicyLimit = $100,000, TotalPaid = $0

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmClaimPayment for claim | Form loads |
| 2 | Enter PaymentType=INDEMNITY, PayeeType=INSURED, Amount=$5,000 | Fields populated |
| 3 | Click Save | Payment created, auto-approved (< $10K threshold) |
| 4 | Verify PaymentNumber | Format PAY+7 digits |
| 5 | Verify claim totals | TotalPaid = $5,000 |
| 6 | Create another payment, Amount=$15,000 | Payment created with Status=PENDING (> $10K) |
| 7 | Verify TotalPaid unchanged | Still $5,000 (pending not counted) |
| 8 | Approve pending payment | usp_Claim_ApprovePayment called |
| 9 | Verify TotalPaid updated | Now $20,000 |
| 10 | Void the $5,000 payment | usp_Claim_VoidPayment called |
| 11 | Verify TotalPaid reduced | Now $15,000 |
| 12 | Attempt payment of $90,000 | Error: 'Payment would exceed policy limit' |

---

### Test Case ID: FT-CLM-004
**Workflow**: Reserve Management Lifecycle
**Priority**: High

#### Preconditions
- Claim in open status (not CLOSED/DENIED)

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmClaimReserve | Form loads |
| 2 | Set CASE reserve, INDEMNITY, $30,000 | Reserve created, auto-approved (<= $50K) |
| 3 | Verify claim TotalReserve | Updated to $30,000 |
| 4 | Change reserve to $75,000 | New reserve, ApprovalRequired=1, IsApproved=0 |
| 5 | Verify activity log | Shows 'PENDING APPROVAL' |
| 6 | Verify TotalReserve | Only counts approved reserves |

---

### Test Case ID: FT-CLM-005
**Workflow**: Fraud Evaluation and SIU Referral
**Priority**: High

#### Preconditions
- Claim exists with characteristics triggering fraud indicators

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmFraudReview for claim | Form loads with current score |
| 2 | Click Evaluate | FraudDataAccess.EvaluateClaim() called |
| 3 | Verify score returned | FraudScore on 0-100 scale |
| 4 | If score >= 70 | IsSIUReferred=1, auto-referral activity logged |
| 5 | If score < 70, manually refer | Enter reason, click Refer to SIU |
| 6 | Verify referral | FraudDataAccess.ReferToSIU() called, IsSIUReferred=1 |
| 7 | View indicator breakdown | dgvIndicators shows triggered/not triggered per indicator |

---

### Test Case ID: FT-CLM-006
**Workflow**: Subrogation Lifecycle
**Priority**: Medium

#### Preconditions
- Claim exists with identified third-party responsibility

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmSubrogation | Form loads |
| 2 | Enter responsible party details | Fields populated |
| 3 | Create subrogation | FraudDataAccess.CreateSubrogation() called |
| 4 | Verify claim flags | IsSubrogation=1, SubrogationStatus='IDENTIFIED' |
| 5 | Update status to DEMAND_SENT | FraudDataAccess.UpdateSubrogationStatus() called |
| 6 | Record recovery of $5,000 | FraudDataAccess.RecordRecovery() called |
| 7 | Verify claim totals | TotalRecovery += $5,000, TotalSubrogation += $5,000, NetIncurred recalculated |

---

### Test Case ID: FT-CLM-007
**Workflow**: Catastrophe Management
**Priority**: Medium

#### Preconditions
- Claims exist that can be linked to catastrophe

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmCatastropheManager | Form loads |
| 2 | Create catastrophe (HURRICANE, 2024-06-01) | FraudDataAccess.CreateCatastrophe() called |
| 3 | Verify CatastropheNumber | Format CAT+7 digits |
| 4 | Link existing claim | FraudDataAccess.LinkClaimToCatastrophe() called |
| 5 | Verify catastrophe counts | TotalClaimsCount, TotalReserveAmount, TotalPaidAmount updated |
| 6 | View summary | 3 result sets returned |
| 7 | Close catastrophe | IsActive=0, ClosedDate set |

---

### Test Case ID: FT-CLM-008
**Workflow**: Full Claim Lifecycle (Happy Path)
**Priority**: Critical

#### Preconditions
- Active policy with coverage

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create FNOL | Status=FNOL, complexity determined |
| 2 | Assign adjuster | Status=ASSIGNED |
| 3 | Change status to INVESTIGATING | Status=INVESTIGATING |
| 4 | Verify coverage | CoverageVerified=1 |
| 5 | Set reserve ($40K) | TotalReserve updated, auto-approved |
| 6 | Run fraud evaluation | Score calculated |
| 7 | Change status to ASSESSED | Status=ASSESSED |
| 8 | Change status to APPROVED | Status=APPROVED |
| 9 | Create payment ($8K to insured) | Auto-approved, TotalPaid updated |
| 10 | Change status to SETTLED | Status=SETTLED |
| 11 | Change status to CLOSED | Status=CLOSED, ClosedDate set |
| 12 | Verify status history | All transitions logged |

---

### Test Case ID: FT-CLM-009
**Workflow**: Claim Search and View
**Priority**: High

#### Preconditions
- Multiple claims exist in system

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmClaimSearch | Form loads, grid empty |
| 2 | Enter search criteria (status=FNOL) | Criteria populated |
| 3 | Click Search | ClaimDataAccess.Search() called, grid populated |
| 4 | Verify pagination | TotalRecords populated, paging works |
| 5 | Double-click a result | frmClaimView opens with ClaimID |
| 6 | Verify all tabs | Summary, Coverages, Reserves, Payments, Activities, History, Assignments tabs populated |

---

### Test Case ID: FT-CLM-010
**Workflow**: Litigation Tracking
**Priority**: Medium

#### Preconditions
- Claim exists in INVESTIGATING status

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Change claim status to LITIGATION | Valid transition from INVESTIGATING |
| 2 | Open frmLitigationTracker | Form loads with open litigation claims |
| 3 | Update attorney name | Claims.usp_Litigation_Update called |
| 4 | Verify claim shows in open litigation | IsLitigation=1, status not CLOSED/DENIED |
