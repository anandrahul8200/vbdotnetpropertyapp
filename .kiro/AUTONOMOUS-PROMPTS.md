# Kiro Autonomous Mode — Session Prompts

Use these prompts in order, one per Autonomous session.
Each session processes one module and commits results to the `feature/test-documentation` branch.

---

## Session 1: Discovery & Inventory

```
Run Step 1 (DISCOVERY) of the "Legacy App Test Generation" skill.

Scan the entire repository automatically. Identify:
- All stored procedures (in database/02-stored-procedures/*.sql)
- All UI forms (in src/PropertyInsuranceClaims/Forms/**/*.vb)
- All DataAccess classes (in src/PropertyInsuranceClaims/DataAccess/*.vb)
- All Models (in src/PropertyInsuranceClaims/Models/*.vb)
- All Common/Helpers (in src/PropertyInsuranceClaims.Common/*.vb)
- All Batch Jobs (in src/PropertyInsuranceClaims.BatchJobs/*.vb)
- All database schema tables (in database/01-schema/*.sql)
- All seed/test data (in database/03-seed-data/*.sql)
- All deploy scripts (in deploy/*.ps1 and deploy/batch-jobs/*.ps1)

Produce docs/test-cases/INVENTORY.md with:
1. Technology Stack identification
2. Complete summary counts per category
3. Detailed listing of EVERY artifact with file path
4. Module grouping (Policy, Claims, Billing, Underwriting, Admin, Documents, Workflow, Reports, Batch)

Follow all conventions in .kiro/steering/testing-standards.md and .kiro/skills/legacy-app-test-generation.md exactly.

Commit to branch "feature/test-documentation" and push.
```

---

## Session 2: Policy Module

```
Run the "Legacy App Test Generation" skill for the POLICY module only.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:
- Forms: src/PropertyInsuranceClaims/Forms/Policy/*.vb (all forms: frmCustomerEntry, frmCustomerSearch, frmPropertyEntry, frmPropertyList, frmPolicyEntry, frmPolicySearch, frmPolicyList, frmPolicyRenewal, frmPolicyEndorsement, frmPolicyCancellation, frmCoverageSelection, frmQuoteGeneration, frmPolicyComparison, frmPolicyDocuments)
- DataAccess: src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb
- DataAccess: src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb
- DataAccess: src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb
- Stored Procedures: database/02-stored-procedures/001-policy-crud-sps.sql
- Stored Procedures: database/02-stored-procedures/002-premium-calculation-sps.sql
- Schema: database/01-schema/003-policy-tables.sql
- Models: src/PropertyInsuranceClaims/Models/PolicyModels.vb
- Models: src/PropertyInsuranceClaims/Models/CustomerModels.vb
- Models: src/PropertyInsuranceClaims/Models/PropertyModels.vb

Generate ALL 10 test artifact types — save to docs/test-cases/:
1. policy-sp-tests.md
2. policy-functional-tests.md
3. policy-ui-tests.md
4. policy-unit-tests.md
5. policy-nfr-tests.md
6. policy-api-tests.md
7. policy-integration-tests.md
8. policy-data-validation-tests.md
9. policy-user-stories.md
10. policy-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-POL-001, FT-POL-001, UI-POL-001, UT-POL-001, NFR-POL-001, API-POL-001, MT-POL-001, DV-POL-001, US-POL-001, BR-POL-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 3: Claims Module

```
Run the "Legacy App Test Generation" skill for the CLAIMS module only.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:
- Forms: src/PropertyInsuranceClaims/Forms/Claims/*.vb (all forms: frmClaimEntry, frmFNOL, frmClaimDashboard, frmClaimSearch, frmClaimPayment, frmClaimReserve, frmClaimNotes, frmClaimTimeline, frmFraudIndicators, frmSubrogation, frmLitigation, frmClaimApproval, frmClaimReopen, frmClaimSupplement, frmCatastropheTracker, frmSalvage)
- DataAccess: src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb
- DataAccess: src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb
- Stored Procedures: database/02-stored-procedures/003-claims-processing-sps.sql
- Stored Procedures: database/02-stored-procedures/004-claims-specialized-sps.sql
- Schema: database/01-schema/004-claims-tables.sql
- Models: src/PropertyInsuranceClaims/Models/ClaimModels.vb

Generate ALL 10 test artifact types — save to docs/test-cases/:
1. claims-sp-tests.md
2. claims-functional-tests.md
3. claims-ui-tests.md
4. claims-unit-tests.md
5. claims-nfr-tests.md
6. claims-api-tests.md
7. claims-integration-tests.md
8. claims-data-validation-tests.md
9. claims-user-stories.md
10. claims-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-CLM-001, FT-CLM-001, UI-CLM-001, UT-CLM-001, NFR-CLM-001, API-CLM-001, MT-CLM-001, DV-CLM-001, US-CLM-001, BR-CLM-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 4: Billing Module

```
Run the "Legacy App Test Generation" skill for the BILLING module only.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:
- Forms: src/PropertyInsuranceClaims/Forms/Billing/*.vb (all forms: frmPaymentEntry, frmPaymentPlan, frmCommissionStatement, frmRefundProcess, frmBillingInquiry)
- DataAccess: src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb
- Stored Procedures: database/02-stored-procedures/006-billing-sps.sql
- Schema: database/01-schema/006-billing-reinsurance-tables.sql

Generate ALL 10 test artifact types — save to docs/test-cases/:
1. billing-sp-tests.md
2. billing-functional-tests.md
3. billing-ui-tests.md
4. billing-unit-tests.md
5. billing-nfr-tests.md
6. billing-api-tests.md
7. billing-integration-tests.md
8. billing-data-validation-tests.md
9. billing-user-stories.md
10. billing-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-BIL-001, FT-BIL-001, UI-BIL-001, UT-BIL-001, NFR-BIL-001, API-BIL-001, MT-BIL-001, DV-BIL-001, US-BIL-001, BR-BIL-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 5: Underwriting Module

```
Run the "Legacy App Test Generation" skill for the UNDERWRITING module only.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:
- Forms: src/PropertyInsuranceClaims/Forms/Underwriting/*.vb (all forms: frmUnderwritingQueue, frmRiskAssessment, frmRateQuote, frmUnderwritingReferral, frmMoratoriumManager, frmReinsuranceView)
- DataAccess: src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb
- DataAccess: src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb
- Stored Procedures: database/02-stored-procedures/005-underwriting-sps.sql
- Stored Procedures: database/02-stored-procedures/007-reinsurance-sps.sql
- Schema: database/01-schema/005-underwriting-tables.sql

Generate ALL 10 test artifact types — save to docs/test-cases/:
1. underwriting-sp-tests.md
2. underwriting-functional-tests.md
3. underwriting-ui-tests.md
4. underwriting-unit-tests.md
5. underwriting-nfr-tests.md
6. underwriting-api-tests.md
7. underwriting-integration-tests.md
8. underwriting-data-validation-tests.md
9. underwriting-user-stories.md
10. underwriting-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-UND-001, FT-UND-001, UI-UND-001, UT-UND-001, NFR-UND-001, API-UND-001, MT-UND-001, DV-UND-001, US-UND-001, BR-UND-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 6: Admin Module

```
Run the "Legacy App Test Generation" skill for the ADMIN module only.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:
- Forms: src/PropertyInsuranceClaims/Forms/Admin/*.vb (all forms: frmDashboardMain, frmUserManagement, frmRolePermissions, frmSystemConfig, frmAuditViewer, frmErrorLogViewer, frmBatchJobMonitor, frmLookupMaintenance, frmPasswordReset)
- DataAccess: src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb
- Stored Procedures: database/02-stored-procedures/011-admin-utility-sps.sql
- Stored Procedures: database/02-stored-procedures/008-batch-job-sps.sql
- Schema: database/01-schema/002-admin-tables.sql
- Common: src/PropertyInsuranceClaims.Common/SecurityHelper.vb
- Common: src/PropertyInsuranceClaims.Common/ErrorLogger.vb

Generate ALL 10 test artifact types — save to docs/test-cases/:
1. admin-sp-tests.md
2. admin-functional-tests.md
3. admin-ui-tests.md
4. admin-unit-tests.md
5. admin-nfr-tests.md
6. admin-api-tests.md
7. admin-integration-tests.md
8. admin-data-validation-tests.md
9. admin-user-stories.md
10. admin-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-ADM-001, FT-ADM-001, UI-ADM-001, UT-ADM-001, NFR-ADM-001, API-ADM-001, MT-ADM-001, DV-ADM-001, US-ADM-001, BR-ADM-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 7: Documents + Workflow + Reports + Batch Jobs

```
Run the "Legacy App Test Generation" skill for DOCUMENTS, WORKFLOW, REPORTS, and BATCH modules.

First read:
- .kiro/skills/legacy-app-test-generation.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- .kiro/steering/legacy-modernization-context.md (for context)
- docs/test-cases/INVENTORY.md (for reference)

Scope — read these ACTUAL source files:

DOCUMENTS module:
- Forms: src/PropertyInsuranceClaims/Forms/Documents/*.vb (frmDocumentUpload, frmDocumentViewer, frmCorrespondence, frmTemplateManager)

WORKFLOW module:
- Forms: src/PropertyInsuranceClaims/Forms/Workflow/*.vb (frmTaskQueue, frmDiaryManager, frmNotificationCenter)

REPORTS module:
- Forms: src/PropertyInsuranceClaims/Forms/Reports/*.vb (frmReportViewer)
- Stored Procedures: database/02-stored-procedures/009-reporting-sps.sql
- Stored Procedures: database/02-stored-procedures/010-search-lookup-sps.sql
- Stored Procedures: database/02-stored-procedures/012-additional-sps.sql

BATCH module:
- Batch Jobs: src/PropertyInsuranceClaims.BatchJobs/*.vb (ExpirationProcessor, FraudScoringJob, PaymentBatchProcessor, ReinsuranceAllocator, RenewalProcessor, ReserveRecalculator)
- Deploy Scripts: deploy/batch-jobs/*.ps1

Generate ALL 10 test artifact types for each module — save to docs/test-cases/:

Documents:
1. documents-sp-tests.md (if applicable)
2. documents-functional-tests.md
3. documents-ui-tests.md
4. documents-unit-tests.md
5. documents-nfr-tests.md
6. documents-api-tests.md
7. documents-integration-tests.md
8. documents-data-validation-tests.md
9. documents-user-stories.md
10. documents-business-rules.md

Workflow:
1. workflow-functional-tests.md
2. workflow-ui-tests.md
3. workflow-unit-tests.md
4. workflow-user-stories.md
5. workflow-business-rules.md

Reports:
1. reports-sp-tests.md
2. reports-functional-tests.md
3. reports-ui-tests.md
4. reports-nfr-tests.md
5. reports-user-stories.md

Batch:
1. batch-sp-tests.md
2. batch-functional-tests.md
3. batch-unit-tests.md
4. batch-nfr-tests.md
5. batch-business-rules.md

RULES:
- Read the ACTUAL source code for each file
- Extract real validation logic, error messages, SP parameters, control names
- Do NOT invent behavior — flag unknowns with [ASSUMPTION]
- Include file paths and references for every test case
- Use IDs: SP-DOC-001, UI-DOC-001, SP-WRK-001, UI-WRK-001, SP-RPT-001, UI-RPT-001, SP-BAT-001, UT-BAT-001

Commit to branch "feature/test-documentation" and push.
```

---

## Session 8: Coverage Verification & Final Traceability

```
Run the "Coverage Verification & Traceability" skill.

First read:
- .kiro/skills/coverage-verification-traceability.md (for process)
- .kiro/steering/testing-standards.md (for conventions)
- docs/test-cases/INVENTORY.md (full inventory)

Then read ALL test case files in docs/test-cases/:
- policy-*.md, claims-*.md, billing-*.md, underwriting-*.md
- admin-*.md, documents-*.md, workflow-*.md, reports-*.md, batch-*.md

Execute the full verification:

1. CROSS-REFERENCE: For every item in INVENTORY.md, verify it has at least one test case covering it.

2. Produce docs/test-cases/TRACEABILITY-MATRIX.md:
   - Every SP → linked test cases
   - Every Form → linked test cases
   - Every DAO method → linked test cases
   - Every Table → linked test cases
   - Every Business Rule → linked test cases

3. Produce docs/test-cases/COVERAGE-REPORT.md:
   - Coverage percentage per category
   - Coverage percentage per module
   - Overall coverage score
   - Recommendations for each priority level

4. Produce docs/test-cases/GAP-ANALYSIS.md:
   - Every uncovered item explicitly listed
   - Reason for each gap
   - Priority (Critical/High/Medium/Low)
   - Dead code identification (SPs never called, forms never referenced)
   - Effort estimate to close gaps

5. If CRITICAL or HIGH priority gaps are found, generate the missing test cases immediately and add them to the appropriate module files.

Target: 100% coverage of all stored procedures, forms, business rules, and data access methods.

Follow .kiro/steering/ and .kiro/skills/ conventions exactly.

Commit to branch "feature/test-documentation" and push.
```

---

## Recovery Prompt (Use If a Session Fails or Stops Mid-Way)

```
Continue the "Legacy App Test Generation" skill on this repository.

First read:
- .kiro/skills/legacy-app-test-generation.md
- .kiro/steering/testing-standards.md
- .kiro/steering/legacy-modernization-context.md
- docs/test-cases/INVENTORY.md

Then check what test files already exist in docs/test-cases/ to determine what has been completed.

Pick up from where the previous session stopped. Complete any partially-generated module, then continue to the next unprocessed module.

Read ACTUAL source code. Do NOT invent behavior. Flag unknowns with [ASSUMPTION].

Follow all conventions in .kiro/steering/ exactly.

Commit to branch "feature/test-documentation" and push.
```

---

## Quick Reference

| Session | Module | Estimated Time |
|---------|--------|---------------|
| 1 | Discovery & Inventory | ~10 min |
| 2 | Policy (14 forms, 2 SP files) | ~20 min |
| 3 | Claims (16 forms, 2 SP files) | ~20 min |
| 4 | Billing (5 forms, 1 SP file) | ~10 min |
| 5 | Underwriting (6 forms, 2 SP files) | ~15 min |
| 6 | Admin (9 forms, 2 SP files) | ~15 min |
| 7 | Documents + Workflow + Reports + Batch | ~15 min |
| 8 | Coverage Verification & Traceability | ~10 min |
| **Total** | **All modules** | **~2 hours** |
