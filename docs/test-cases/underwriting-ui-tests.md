# Underwriting Module - UI Tests

## Module: UND (Underwriting)
## Test Type: Screen-Level UI Tests
## Forms Covered (6 total):
1. frmQuoteWorksheet
2. frmReferralQueue
3. frmUnderwritingReferral
4. frmMoratorium
5. frmRateTableMaintenance
6. frmReinsuranceView

---

### Test Case ID: UI-UND-001
**Form/Screen**: frmQuoteWorksheet
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmQuoteWorksheet.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Rating Worksheet', size 900x500, CenterParent |
| 2 | Constructor accepts policyID | _policyID stored from parameter |
| 3 | DataGridView configured | ReadOnly=True, AllowUserToAddRows=False, AutoSizeColumnsMode=Fill |
| 4 | AlternatingRows styled | AlternatingRowsDefaultCellStyle.BackColor = AliceBlue |
| 5 | Buttons visible | btnRecalculate (text '&Recalculate'), btnClose (text '&Close') |
| 6 | Worksheet data loaded on open | LoadWorksheet() called, dgvWorksheet bound |
| 7 | Total premium label | lblTotalPremium shows sum of FinalPremium formatted as currency |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Recalculate | UnderwritingDataAccess.CalculatePremium called, worksheet reloaded |
| 2 | Recalculate success | MessageBox: 'Premium recalculated.' |
| 3 | Click Close | Form closes |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Load error | MessageBox with error, ErrorLogger.LogError called |
| 2 | Recalculate error | MessageBox: 'Error: {message}' |
| 3 | Cursor during recalculate | WaitCursor during operation, Default after |

---

### Test Case ID: UI-UND-002
**Form/Screen**: frmReferralQueue
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReferralQueue.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Underwriting Referral Queue', size 1000x650, CenterParent |
| 2 | Grid configured | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect, AutoSizeColumnsMode=Fill |
| 3 | Referrals loaded on open | LoadReferrals() called |
| 4 | Hidden columns | ReferralID and PolicyID columns hidden |
| 5 | Count label | lblCount shows '{N} pending referral(s)' |
| 6 | Decision buttons | btnApprove (LightGreen), btnConditional (LightYellow), btnDecline (LightCoral) |
| 7 | Notes and Conditions fields | txtNotes and txtConditions multiline |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|-------------------|--------|----------------|
| 1 | No referral selected | Approve | Displays 'Please select a referral.' |
| 2 | No referral selected | Decline | Displays 'Please select a referral.' |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Approve | ProcessDecision('APPROVED') called, success message, grid refreshed |
| 2 | Click Decline | Confirmation dialog shown first, then ProcessDecision('DECLINED') |
| 3 | Click Conditional (no conditions) | Validation: 'Please enter conditions for conditional approval.' |
| 4 | Click Conditional (with conditions) | ProcessDecision('CONDITIONAL') called |
| 5 | Click Refresh | LoadReferrals() called |
| 6 | Click View Policy | frmPolicyView opened with PolicyID from selected row |
| 7 | After successful decision | txtNotes and txtConditions cleared, grid reloaded |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Load error | MessageBox: 'Error loading referrals: {message}' |
| 2 | Decision error | MessageBox: 'Error: {message}', ErrorLogger.LogError called |
| 3 | Cursor during operations | WaitCursor during load/decision, Default after |

---

### Test Case ID: UI-UND-003
**Form/Screen**: frmUnderwritingReferral
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmUnderwritingReferral.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Underwriting Referral Review', size 850x700, CenterParent |
| 2 | Constructor accepts referralID and policyID | Both stored |
| 3 | Referral header populated | lblReferralReason, lblReferralDate, lblSeverity |
| 4 | Severity color coding | CRITICAL=Red, HIGH=DarkOrange, other=Black |
| 5 | Policy summary group | PolicyNumber, PolicyType, Customer, Property, Premium, TIV |
| 6 | Risk factors grid | dgvRiskFactors with evaluated rules data |
| 7 | Decision dropdown | Items: APPROVED, DECLINED, CONDITIONAL; SelectedIndex=0 |
| 8 | Conditions field disabled by default | txtConditions.Enabled=False |
| 9 | Expiry date disabled by default | dtpExpiryDate.Enabled=False, default value +30 days |

#### Decision Toggle Tests
| # | Decision Selected | txtConditions | Expected State |
|---|-------------------|---------------|----------------|
| 1 | APPROVED | Disabled | Conditions not required |
| 2 | DECLINED | Disabled | Conditions not required |
| 3 | CONDITIONAL | Enabled | Conditions required |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Submit (APPROVED) | ProcessReferral called, success message, dialog closes OK |
| 2 | Click Submit (DECLINED) | Confirmation dialog shown first |
| 3 | Click Submit (CONDITIONAL, no conditions) | Validation: 'Please enter conditions for conditional approval.' |
| 4 | Click Submit (CONDITIONAL, with conditions) | ProcessReferral called with conditions |
| 5 | Click View Policy | frmPolicyView opened with _policyID |
| 6 | Click Rating Worksheet | frmQuoteWorksheet opened with _policyID |
| 7 | Click Close | Form closes |
| 8 | Check Has Expiry | dtpExpiryDate becomes enabled |

---

### Test Case ID: UI-UND-004
**Form/Screen**: frmMoratorium
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmMoratorium.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Moratorium Management', size 900x500, CenterParent |
| 2 | Grid configured | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect, AutoSizeColumnsMode=Fill |
| 3 | Active moratoriums loaded | LoadMoratoriums() calls GetActiveMoratoriums() |
| 4 | Buttons visible | New Moratorium, Lift Selected, Check Location, Refresh |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click New Moratorium | frmMoratoriumEntry dialog opens |
| 2 | New dialog returns OK | Grid reloaded |
| 3 | Click Lift Selected | Confirmation: 'Lift moratorium: {name}?' |
| 4 | Confirm Lift | usp_Moratorium_Lift called, success message, grid reloaded |
| 5 | Click Check Location | InputBox for state, then zip; CheckMoratorium called |
| 6 | Check returns moratorium active | Warning: 'MORATORIUM IN EFFECT...' |
| 7 | Check returns no moratorium | Info: 'No moratorium...Writing is allowed.' |
| 8 | Click Refresh | LoadMoratoriums() called |

---

### Test Case ID: UI-UND-005
**Form/Screen**: frmRateTableMaintenance
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmRateTableMaintenance.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Rate Table Maintenance', size 950x650, CenterParent |
| 2 | Rate table code dropdown populated | Items include BASE_RATES_HO3, CONSTRUCTION_FACTORS, etc. |
| 3 | State filter defaults to (All) | cboState.SelectedIndex=0 |
| 4 | Policy type filter | Items: (All), HO3, HO4, HO6, DP3 |
| 5 | New table panel hidden | grpNewTable.Visible=False |
| 6 | Detail grid configured | AllowUserToAddRows=False, AlternatingRows=AliceBlue |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Load | usp_RateTable_GetDetails called, header labels and grid populated |
| 2 | Header shows table info | lblTableName, lblVersion, lblEffective, lblStatus |
| 3 | Click New Table | grpNewTable becomes visible |
| 4 | Create table (valid) | usp_RateTable_Create called, success message, panel hides |
| 5 | Create table (missing code/name) | Validation: 'Code and Name are required.' |
| 6 | Click Cancel New | grpNewTable hides |
| 7 | Click Add Row (no table loaded) | Info: 'Please load a rate table first.' |
| 8 | Click Add Row (table loaded) | New row added to grid |
| 9 | Click Delete Row | Current row removed |
| 10 | Click Export | Info message (placeholder) |
| 11 | Click Import | Info message (placeholder) |
| 12 | Click Close | Form closes |

---

### Test Case ID: UI-UND-006
**Form/Screen**: frmReinsuranceView
**File**: `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens | Title='Reinsurance Management', size 1000x650, CenterParent |
| 2 | TabControl with 3 tabs | Treaties, Cessions, Bordereaux |
| 3 | Treaties loaded on open | LoadTreaties() called, dgvTreaties bound |
| 4 | TreatyID column hidden | dgvTreaties.Columns('TreatyID').Visible=False |
| 5 | Treaty combos populated | cboTreatyFilter and cboTreatyForBord filled from treaties |
| 6 | Bordereaux report types | Items: PREMIUM, LOSS, OUTSTANDING |
| 7 | Period defaults to current month | txtAccountingPeriod and txtBordPeriod = DateTime.Now yyyy-MM |

#### Action Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Click Load Cessions | GetCessionsByTreaty called, summary label updated |
| 2 | Cession summary format | 'Gross: {C0} | Ceded: {C0}' |
| 3 | Click Generate Bordereaux | GenerateBordereaux called, success message |
| 4 | Click Refresh Treaties | LoadTreaties() called |
| 5 | Click View Treaty Details | Treaty details placeholder message |
| 6 | Click Close | Form closes |
