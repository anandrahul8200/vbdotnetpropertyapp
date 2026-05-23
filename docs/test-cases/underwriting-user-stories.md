# Underwriting Module - User Stories

## Module: UND (Underwriting)
## Test Type: User Stories and Acceptance Criteria

---

### User Story ID: US-UND-001
**Title**: Premium Calculation for New Quote
**Priority**: Critical

#### Story
As an underwriter, I want to calculate the premium for a policy quote so that I can provide an accurate price to the customer based on all applicable rating factors.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | A policy exists with selected coverages | I open the Rating Worksheet form | Worksheet loads with factor breakdown per coverage |
| 2 | Policy has property details configured | I click Recalculate | Premium calculated using 13 rating factors |
| 3 | Base rate exists for the coverage | Premium is calculated | BasePremium = (InsuredValue / 1000) * RatePer1000 |
| 4 | All factors default to 1.0 | No special conditions apply | Premium equals base premium |
| 5 | Calculated premium < minimum | MinPremium configured | MinPremium applied instead |
| 6 | Calculation completes | Result displayed | Total premium shown as currency, worksheet grid updated |
| 7 | Error occurs during calculation | Database error | Error message displayed, ErrorLogger called |

#### Form
- **Screen**: frmQuoteWorksheet
- **Data Access**: UnderwritingDataAccess.CalculatePremium()
- **Stored Procedure**: Underwriting.usp_Premium_Calculate

---

### User Story ID: US-UND-002
**Title**: Underwriting Referral Review
**Priority**: Critical

#### Story
As an underwriter, I want to review and process pending referrals so that I can approve, decline, or conditionally approve policies that have triggered underwriting rules.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Pending referrals exist | I open the Referral Queue | Grid shows pending referrals with policy details |
| 2 | Referral is selected | I view details | Policy summary, risk factors, and severity displayed |
| 3 | Referral is selected | I click Approve with notes | Referral status = APPROVED, success message shown |
| 4 | Referral is selected | I click Decline | Confirmation dialog appears first |
| 5 | Decline confirmed | Decision processed | Referral status = DECLINED, policy status = DECLINED |
| 6 | Conditional selected, no conditions entered | I click Conditional | Validation: 'Please enter conditions for conditional approval.' |
| 7 | Conditional with conditions | I click Conditional | Referral status = CONDITIONAL, conditions stored |
| 8 | Decision processed | Grid refreshes | Processed referral removed from pending list |
| 9 | No referral selected | I click Approve/Decline | Message: 'Please select a referral.' |

#### Form
- **Screen**: frmReferralQueue, frmUnderwritingReferral
- **Data Access**: UnderwritingDataAccess.ProcessReferral(), UnderwritingDataAccess.GetPendingReferrals()
- **Stored Procedure**: Underwriting.usp_Referral_Process, Underwriting.usp_Referral_SearchPending

---

### User Story ID: US-UND-003
**Title**: Underwriting Rules Evaluation
**Priority**: Critical

#### Story
As an underwriter, I want the system to automatically evaluate underwriting rules against a policy so that risks requiring manual review are identified and flagged.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Policy with property over 75 years old | Rules are evaluated | AGE_75_PLUS rule triggers REFER (HIGH severity) |
| 2 | Policy with protection class >= 9 | Rules are evaluated | PROT_CLASS_9_10 rule triggers REFER (MEDIUM) |
| 3 | Policy with roof over 25 years | Rules are evaluated | ROOF_AGE_25 rule triggers REFER (MEDIUM) |
| 4 | Policy with credit score < 550 | Rules are evaluated | CREDIT_BELOW_550 triggers DECLINE (CRITICAL) |
| 5 | Policy with TIV > $2M | Rules are evaluated | HIGH_VALUE rule triggers REFER (HIGH) |
| 6 | Customer with 3+ claims in 3 years | Rules are evaluated | PRIOR_CLAIMS_3 triggers REFER (HIGH) |
| 7 | No rules triggered | Policy within normal parameters | Empty result, no referrals created |
| 8 | Rules triggered | Evaluation completes | Referral records created in Underwriting.Referrals |
| 9 | Results displayed | Evaluation completes | Ordered by severity: CRITICAL first |

#### Form
- **Screen**: frmUnderwritingReferral (dgvRiskFactors)
- **Data Access**: UnderwritingDataAccess.EvaluateRules()
- **Stored Procedure**: Underwriting.usp_Rules_Evaluate

---

### User Story ID: US-UND-004
**Title**: Moratorium Management
**Priority**: High

#### Story
As an underwriting manager, I want to create, view, and lift moratoriums so that I can restrict new business or renewals in areas affected by catastrophic events.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | I need to restrict writing | I open Moratorium Management | Active moratoriums listed in grid |
| 2 | Catastrophic event occurs | I click New Moratorium | Entry form opens with name, type, states, dates |
| 3 | Moratorium details entered | I click Create | Moratorium created, grid refreshes |
| 4 | Name is blank | I click Create | Validation: 'Name is required.' |
| 5 | Active moratorium exists | I select and click Lift | Confirmation: 'Lift moratorium: {name}?' |
| 6 | Lift confirmed | Moratorium lifted | IsActive=0, EndDate=today, grid refreshes |
| 7 | I need to check a location | I click Check Location | InputBox prompts for state and zip |
| 8 | Moratorium active for location | Check completes | Warning: 'MORATORIUM IN EFFECT... New business is restricted.' |
| 9 | No moratorium for location | Check completes | Info: 'No moratorium... Writing is allowed.' |

#### Form
- **Screen**: frmMoratorium, frmMoratoriumEntry
- **Data Access**: UnderwritingDataAccess.CheckMoratorium(), UnderwritingDataAccess.GetActiveMoratoriums()
- **Stored Procedure**: Underwriting.usp_Moratorium_Create, Underwriting.usp_Moratorium_Lift, Underwriting.usp_Moratorium_Check

---

### User Story ID: US-UND-005
**Title**: Rate Table Maintenance
**Priority**: High

#### Story
As an actuarial analyst, I want to create and maintain rate tables so that the premium calculation engine uses current, filed rates.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Rate tables exist | I open Rate Table Maintenance | Dropdown populated with rate table codes |
| 2 | I select a rate table and click Load | Details loaded | Header info (name, version, effective, status) and detail grid shown |
| 3 | I need a new table | I click New Table | Creation panel appears with fields for code, name, type, state, effective date |
| 4 | Code and Name provided | I click Create | Rate table created, version auto-incremented, previous expired |
| 5 | Code or Name missing | I click Create | Validation: 'Code and Name are required.' |
| 6 | Table loaded | I click Add Row | New row added to detail grid |
| 7 | No table loaded | I click Add Row | Info: 'Please load a rate table first.' |
| 8 | Rate table types | Creating new table | Types: BASE, FACTOR, DISCOUNT, SURCHARGE, TAX |
| 9 | Policy types | Creating new table | Types: HO3, HO4, HO6, DP3, BOP |

#### Form
- **Screen**: frmRateTableMaintenance
- **Data Access**: Direct DatabaseHelper calls
- **Stored Procedure**: Underwriting.usp_RateTable_Create, Underwriting.usp_RateTable_GetDetails, Underwriting.usp_RateTable_AddDetail

---

### User Story ID: US-UND-006
**Title**: Quote Comparison Worksheet
**Priority**: High

#### Story
As an underwriter, I want to view the complete rating worksheet for a policy so that I can see how each factor contributes to the final premium and verify the calculation.

#### Acceptance Criteria
| # | Given | When | Then |
|---|-------|------|------|
| 1 | Policy has been rated | I open the worksheet | All coverages shown with factor breakdown |
| 2 | Worksheet displayed | I review factors | Each of the 13 factors visible per coverage |
| 3 | Multiple coverages rated | Grid loaded | One row per coverage, ordered by CoverageCode |
| 4 | Total premium calculated | Bottom of display | Sum of all FinalPremium values shown as currency |
| 5 | Rates need updating | I click Recalculate | Full premium recalculation triggered, grid refreshes |
| 6 | Recalculation succeeds | Complete | Message: 'Premium recalculated.' |

#### Form
- **Screen**: frmQuoteWorksheet
- **Data Access**: UnderwritingDataAccess.GetRatingWorksheet(), UnderwritingDataAccess.CalculatePremium()
- **Stored Procedure**: Underwriting.usp_Worksheet_GetByPolicy, Underwriting.usp_Premium_Calculate
