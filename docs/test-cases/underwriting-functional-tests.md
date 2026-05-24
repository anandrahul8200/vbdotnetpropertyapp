# Underwriting Module - Functional Tests

## Module: UND (Underwriting)
## Test Type: End-to-End Functional Tests

---

### Test Case ID: FT-UND-001
**Workflow**: Premium Calculation End-to-End
**Priority**: Critical

#### Preconditions
- Active policy exists with at least one selected coverage (IsSelected=1)
- Property details populated (StateCode, ConstructionType, YearBuilt, etc.)
- Base rates configured for the policy/coverage combination

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call UnderwritingDataAccess.CalculatePremium(policyID) | SP Underwriting.usp_Premium_Calculate called |
| 2 | Verify cursor iterates selected coverages | Each IsSelected=1 coverage processed |
| 3 | Verify base rate lookup per coverage | BaseRate found from Underwriting.BaseRates |
| 4 | Verify all 13 factors calculated | Construction, Age, Roof, ProtectionClass, Deductible, Credit, Claims, Device, Territory, Occupancy, Loyalty, MultiPolicy, NewHome |
| 5 | Verify TotalFactor = product of all factors | Multiplicative combination |
| 6 | Verify CoveragePremium = BasePremium * TotalFactor | Rounded to 2 decimal places |
| 7 | Verify minimum premium applied if needed | If CoveragePremium < MinPremium |
| 8 | Verify taxes/fees calculated | usp_Premium_CalculateTaxesFees called |
| 9 | Verify GrossPremium = TotalPremium + TotalTaxes | Sum correct |
| 10 | Verify policy record updated | AnnualPremium, GrossPremium, CommissionAmount set |
| 11 | Verify rating worksheet stored | RatingWorksheets rows for each coverage |
| 12 | Verify output parameters returned | TotalPremium, TotalTaxes, GrossPremium outputs populated |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Calculate premium for non-existent policy | Error: 'Policy not found: {ID}' |
| 2 | Policy with no selected coverages | TotalPremium = 0, no cursor processing |

---

### Test Case ID: FT-UND-002
**Workflow**: Rate Lookup and Territory Fallback
**Priority**: Critical

#### Preconditions
- BaseRates table has territory-specific and state-level rates
- RatingFactors table has state-specific and generic factors

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Lookup base rate with valid territory code | Territory-specific rate returned |
| 2 | Lookup base rate with territory having no rate | Falls back to state-level (TerritoryCode IS NULL) |
| 3 | Lookup factor with state-specific row | State row returned (ORDER BY StateCode DESC) |
| 4 | Lookup factor with only generic row | Generic row returned (StateCode IS NULL) |
| 5 | Key-based factor lookup | Exact match on FactorKey |
| 6 | Range-based factor lookup | Value BETWEEN MinRange AND MaxRange |

---

### Test Case ID: FT-UND-003
**Workflow**: Underwriting Rules Evaluation
**Priority**: Critical

#### Preconditions
- Policy with property details that trigger at least one rule
- Underwriting.Rules table has active rules

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call UnderwritingDataAccess.EvaluateRules(policyID) | SP Underwriting.usp_Rules_Evaluate called |
| 2 | Verify built-in rules evaluated | Property age, protection class, roof, credit, TIV, claims checked |
| 3 | Verify database rules evaluated | Active rules from Underwriting.Rules included |
| 4 | Verify referral records created | Underwriting.Referrals rows for triggered rules |
| 5 | Verify results ordered by severity | CRITICAL > HIGH > MEDIUM > LOW |
| 6 | Verify DECLINE action identified | Credit < 550 returns DECLINE action |

#### Negative Path
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Evaluate policy with no risk triggers | Empty result set returned |
| 2 | Evaluate non-existent policy | Error: 'Policy not found: {ID}' |

---

### Test Case ID: FT-UND-004
**Workflow**: Referral Processing
**Priority**: Critical

#### Preconditions
- Pending referral exists in Underwriting.Referrals
- Referral linked to a policy in QUOTE or REFERRED status

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Load pending referrals via GetPendingReferrals() | DataTable with pending referrals returned |
| 2 | Select referral and approve | UnderwritingDataAccess.ProcessReferral(id, 'APPROVED', notes, conditions) |
| 3 | Verify referral status updated | ReferralStatus = 'APPROVED', ReviewedBy set |
| 4 | Verify audit log entry | AuditLog with old=PENDING, new=APPROVED |
| 5 | Select referral and decline | ProcessReferral(id, 'DECLINED', notes, '') |
| 6 | Verify policy status updated on decline | Policy.PolicyStatus = 'DECLINED' |
| 7 | Select referral and conditional approve | ProcessReferral(id, 'CONDITIONAL', notes, conditions) |
| 8 | Verify conditions stored | Referral.Conditions populated |

---

### Test Case ID: FT-UND-005
**Workflow**: Moratorium Management
**Priority**: High

#### Preconditions
- User has underwriting permissions

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Create moratorium for FL state, HO3 policies | usp_Moratorium_Create called, MoratoriumID returned |
| 2 | Verify moratorium active | GetActiveMoratoriums includes new moratorium |
| 3 | Check moratorium for FL HO3 | CheckMoratorium returns True |
| 4 | Check moratorium for TX HO3 | CheckMoratorium returns False |
| 5 | Lift moratorium | usp_Moratorium_Lift called, IsActive=0, EndDate=today |
| 6 | Verify moratorium no longer active | GetActiveMoratoriums excludes lifted moratorium |
| 7 | Check moratorium for FL HO3 again | CheckMoratorium returns False |

---

### Test Case ID: FT-UND-006
**Workflow**: Commission Calculation
**Priority**: High

#### Preconditions
- Policy with known AgentID
- CommissionSchedules or Agent.CommissionRate configured

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Calculate commission for new business | usp_Commission_Calculate with TransactionType='NEW' |
| 2 | Verify schedule lookup | CommissionSchedules checked by PolicyType+TransactionType+PremiumRange |
| 3 | Verify rate applied | CommissionAmount = PremiumAmount * CommissionRate |
| 4 | Verify policy updated | Policy.CommissionRate and CommissionAmount set |
| 5 | Calculate with no schedule (fallback) | No matching schedule row | Agent default rate used |

---

### Test Case ID: FT-UND-007
**Workflow**: Endorsement Premium Calculation
**Priority**: High

#### Preconditions
- Active policy mid-term with modified coverages

#### Test Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Calculate endorsement premium | usp_Premium_CalculateEndorsement called |
| 2 | Verify pro-rata factor | DaysRemaining / DaysInTerm |
| 3 | Verify full premium recalculated | usp_Premium_Calculate called internally |
| 4 | Verify premium change | (NewPremium - OldPremium) * ProRataFactor |
| 5 | Verify additional premium for increase | AdditionalPremium > 0 if change positive |
| 6 | Verify return premium for decrease | ReturnPremium > 0 if change negative |
| 7 | Verify endorsement record updated | PremiumChange, ProRataFactor, ReturnPremium, AdditionalPremium set |
