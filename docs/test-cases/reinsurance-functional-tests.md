# Reinsurance Module - Functional Tests

## Module: RNS (Reinsurance)
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`
- `database/02-stored-procedures/007-reinsurance-sps.sql`

## Workflows Covered:
1. Treaty Setup and Management
2. Premium Cession Calculation
3. Loss Cession Calculation
4. Bordereaux Generation and Submission
5. Treaty Reporting and Summary

---

### Test Case ID: FT-RNS-001
**Workflow**: Treaty Creation
**Module**: Reinsurance
**Priority**: Critical

#### Pre-conditions
- [ ] User has access to Reinsurance Management form
- [ ] At least one reinsurer exists in Reinsurance.Reinsurers
- [ ] System date is current

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Open frmReinsuranceView | -- | -- | Form loads, Treaties tab active, dgvTreaties populated |
| 2 | Click "New Treaty" button | btnNewTreaty | -- | Treaty creation dialog opens [ASSUMPTION] |
| 3 | Enter treaty name | TreatyName field | "QS Treaty 2024" | Field accepts input |
| 4 | Select treaty type | TreatyType dropdown | "QUOTA_SHARE" | Type selected |
| 5 | Select reinsurer | Reinsurer dropdown | Valid reinsurer | Reinsurer selected |
| 6 | Enter effective date | EffectiveDate | 2024-01-01 | Date accepted |
| 7 | Enter expiry date | ExpiryDate | 2024-12-31 | Date accepted |
| 8 | Enter cession percent | CessionPercent | 40% | Value accepted |
| 9 | Save treaty | Save button | -- | Treaty saved, TreatyNumber generated (TRY+7 digits) |
| 10 | Verify in grid | dgvTreaties | -- | New treaty appears in treaties grid |

#### Post-conditions
- [ ] Reinsurance.Treaties has new record with Status='ACTIVE'
- [ ] TreatyNumber follows format TRY + 7 digits
- [ ] Audit.AuditLog has INSERT record for the new treaty
- [ ] Treaties grid refreshed showing new entry

#### Related Artifacts
- Stored Procedure: Reinsurance.usp_Treaty_Create
- Form: frmReinsuranceView
- Business Rule: BR-RNS-001 (Treaty Number Format)
- Data Access: ReinsuranceDataAccess.CreateTreaty

---

### Test Case ID: FT-RNS-002
**Workflow**: Premium Cession Calculation
**Module**: Reinsurance
**Priority**: Critical

#### Pre-conditions
- [ ] At least one active treaty exists with covered policy types/states
- [ ] Policy exists with matching type and state
- [ ] Policy has an associated property with StateCode

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Open frmReinsuranceView | -- | -- | Form loads successfully |
| 2 | Navigate to Cessions tab | tabCessions | -- | Cessions tab displayed with filter panel |
| 3 | Select treaty from dropdown | cboTreatyFilter | Active treaty | Treaty selected |
| 4 | Enter accounting period | txtAccountingPeriod | "2024-06" | Period entered |
| 5 | Click "Calculate New" | btnCalculateCession | -- | Cession calculation dialog opens [ASSUMPTION] |
| 6 | Select policy | Policy selection | Valid policy matching treaty coverage | Policy selected |
| 7 | Enter gross premium | GrossPremium field | 10000.00 | Value accepted |
| 8 | Confirm calculation | Confirm/Save | -- | Cession calculated and stored |
| 9 | Click "Load" | btnLoadCessions | -- | dgvCessions shows new cession record |
| 10 | Verify summary | lblCessionSummary | -- | "Gross: $10,000 | Ceded: $4,000" (for 40% QS) |

#### Post-conditions
- [ ] Reinsurance.Cessions has new PREMIUM record
- [ ] CededAmount calculated per treaty type logic
- [ ] RetainedAmount = GrossAmount - CededAmount
- [ ] AccountingPeriod set to current YYYY-MM
- [ ] Status = 'PENDING'

#### Related Artifacts
- Stored Procedure: Reinsurance.usp_Cession_CalculatePremium
- Form: frmReinsuranceView (Cessions tab)
- Business Rule: BR-RNS-003 (Quota Share Calculation)
- Business Rule: BR-RNS-004 (Surplus Calculation)
- Business Rule: BR-RNS-005 (Excess-of-Loss Premium)

---

### Test Case ID: FT-RNS-003
**Workflow**: Loss Cession Calculation
**Module**: Reinsurance
**Priority**: Critical

#### Pre-conditions
- [ ] Active treaty exists covering the claim's policy type and state
- [ ] Claim exists with valid PolicyID
- [ ] Policy has associated property for state lookup

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Trigger loss cession | System/batch process | ClaimID, LossAmount | Cession calculation initiated |
| 2 | System identifies claim policy | -- | -- | PolicyID, PolicyType, StateCode resolved |
| 3 | System finds matching treaties | -- | -- | Active treaties with matching coverage identified |
| 4 | Calculate per treaty type | -- | -- | QS: proportional, XOL: layer logic, Surplus: existing % |
| 5 | Record cession | -- | -- | Cession record(s) inserted with CessionType='LOSS' |
| 6 | Verify in Cessions tab | btnLoadCessions | -- | Loss cession appears in grid |

#### Post-conditions
- [ ] Reinsurance.Cessions has LOSS record(s) for each matching treaty
- [ ] For XOL: ceded only if LossAmount > AttachmentPoint
- [ ] For Surplus: uses existing premium cession percentage
- [ ] ClaimID stored in cession record

#### Related Artifacts
- Stored Procedure: Reinsurance.usp_Cession_CalculateLoss
- Business Rule: BR-RNS-006 (Excess-of-Loss Layer Logic)
- Business Rule: BR-RNS-007 (Surplus Loss Follows Premium)

---

### Test Case ID: FT-RNS-004
**Workflow**: Bordereaux Generation
**Module**: Reinsurance
**Priority**: High

#### Pre-conditions
- [ ] Active treaty exists with PENDING cessions for the reporting period
- [ ] User has access to Bordereaux tab in frmReinsuranceView
- [ ] Cessions exist for the selected treaty and period

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Open frmReinsuranceView | -- | -- | Form loads |
| 2 | Navigate to Bordereaux tab | tabBordereaux | -- | Bordereaux tab displayed |
| 3 | Select treaty | cboTreatyForBord | Active treaty | Treaty selected |
| 4 | Enter reporting period | txtBordPeriod | "2024-06" | Period entered |
| 5 | Select report type | cboReportType | "PREMIUM" | Type selected (options: PREMIUM, LOSS, OUTSTANDING) |
| 6 | Click "Generate" | btnGenerateBord | -- | Cursor changes to wait, then success message "Bordereaux generated (ID: N)." |
| 7 | Click "Refresh" | btnRefreshBord | -- | dgvBordereaux shows new record |
| 8 | Verify totals | dgvBordereaux row | -- | TotalGross, TotalCeded, RecordCount match cession data |

#### Post-conditions
- [ ] Reinsurance.Bordereaux has new record with Status='DRAFT'
- [ ] TotalGross, TotalCeded, TotalRetained aggregated from matching cessions
- [ ] RecordCount = number of matching cessions
- [ ] Matching PENDING cessions updated to Status='REPORTED'
- [ ] Audit.AuditLog has INSERT record

#### Related Artifacts
- Stored Procedure: Reinsurance.usp_Bordereaux_Generate
- Form: frmReinsuranceView (Bordereaux tab)
- Data Access: ReinsuranceDataAccess.GenerateBordereaux
- Business Rule: BR-RNS-008 (Bordereaux Reporting Period)

---

### Test Case ID: FT-RNS-005
**Workflow**: Bordereaux Submission
**Module**: Reinsurance
**Priority**: High

#### Pre-conditions
- [ ] Generated bordereaux exists with Status='DRAFT'
- [ ] User is on Bordereaux tab with record selected

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Select bordereaux record | dgvBordereaux | Existing DRAFT record | Row selected |
| 2 | Click "Submit" | btnSubmitBord | -- | MessageBox: "Bordereaux submitted to reinsurer." |
| 3 | Verify status update | dgvBordereaux | -- | [ASSUMPTION] Status changes to 'SUBMITTED' |

#### Post-conditions
- [ ] [ASSUMPTION] Reinsurance.Bordereaux.Status updated to 'SUBMITTED'
- [ ] [ASSUMPTION] SubmittedDate set to current datetime
- [ ] Success message displayed to user

#### Related Artifacts
- Form: frmReinsuranceView (Bordereaux tab)
- Business Rule: BR-RNS-009 (Bordereaux Status Lifecycle)

---

### Test Case ID: FT-RNS-006
**Workflow**: Treaty Summary and Reporting
**Module**: Reinsurance
**Priority**: Medium

#### Pre-conditions
- [ ] Active treaty exists with cession and bordereaux history
- [ ] User has access to treaty details view

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Open frmReinsuranceView | -- | -- | Form loads, Treaties tab shown |
| 2 | Select treaty in grid | dgvTreaties | Existing active treaty | Row selected |
| 3 | Click "View Details" | btnViewTreaty | -- | Treaty details view opens (MessageBox placeholder currently) |
| 4 | View treaty header | -- | -- | Treaty name, type, dates, retention, cession % displayed |
| 5 | View cession summary | -- | -- | PREMIUM and LOSS totals grouped |
| 6 | View bordereaux history | -- | -- | List of bordereaux ordered by period descending |

#### Post-conditions
- [ ] All treaty information displayed accurately
- [ ] Cession totals match database aggregations
- [ ] No data modified (read-only operation)

#### Related Artifacts
- Stored Procedure: Reinsurance.usp_Treaty_GetSummary
- Form: frmReinsuranceView (Treaties tab)
- Data Access: ReinsuranceDataAccess.GetTreatyDetails

---

### Test Case ID: FT-RNS-007
**Workflow**: Cession Loading by Treaty and Period
**Module**: Reinsurance
**Priority**: Medium

#### Pre-conditions
- [ ] Active treaties exist with cessions
- [ ] User is on Cessions tab

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Navigate to Cessions tab | tabCessions | -- | Filter panel visible with treaty dropdown and period field |
| 2 | Select treaty | cboTreatyFilter | First treaty | Treaty selected |
| 3 | Enter period | txtAccountingPeriod | "2024-06" | Period entered (default is current YYYY-MM) |
| 4 | Click "Load" | btnLoadCessions | -- | Cursor changes to wait, grid populated |
| 5 | Verify grid data | dgvCessions | -- | Shows cessions for selected treaty and period |
| 6 | Verify summary label | lblCessionSummary | -- | "Gross: $X | Ceded: $Y" with correct totals |

#### Post-conditions
- [ ] dgvCessions displays filtered cession records
- [ ] Summary label shows accurate Gross and Ceded totals
- [ ] No data modified (read-only)

#### Related Artifacts
- Data Access: ReinsuranceDataAccess.GetCessionsByTreaty
- Form: frmReinsuranceView (Cessions tab)

---

### Test Case ID: FT-RNS-008
**Workflow**: Bordereaux Export
**Module**: Reinsurance
**Priority**: Medium

#### Pre-conditions
- [ ] Generated bordereaux exists
- [ ] User is on Bordereaux tab

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Select bordereaux record | dgvBordereaux | Existing record | Row selected |
| 2 | Click "Export" | btnExportBord | -- | MessageBox: "Export to Excel/CSV would happen here." |

#### Post-conditions
- [ ] [ASSUMPTION] In production, file would be exported to user-selected path
- [ ] Export format would be Excel or CSV

#### Related Artifacts
- Form: frmReinsuranceView (Bordereaux tab)
