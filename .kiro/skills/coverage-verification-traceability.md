# Skill: Coverage Verification & Traceability

## Description
A verification skill that ensures NOTHING was skipped during test generation. It cross-references all discovered artifacts against generated test cases, produces a complete traceability matrix, coverage report, and gap analysis with actionable recommendations.

## When to Use
- After running "Legacy App Test Generation" skill on a module
- After all modules have been processed (final verification pass)
- When asked to verify test coverage completeness
- When asked to produce a traceability matrix or gap analysis
- As a quality gate before signing off on test documentation

## Prerequisites
- "Legacy App Test Generation" skill must have been run first
- `docs/test-cases/INVENTORY.md` must exist
- Test case files must exist in `docs/test-cases/` folder

## Dependency
- Runs AFTER: `legacy-app-test-generation` skill
- Input: INVENTORY.md + all generated test case files

---

## STEP 1: LOAD INVENTORY

### 1.1 Read the Inventory
- Open `docs/test-cases/INVENTORY.md`
- Parse all discovered items into categorized lists:
  - All Stored Procedures (name, schema, file path)
  - All UI Forms/Screens (name, file path)
  - All Data Access/DAO classes and methods (class, method, file path)
  - All Model/DTO classes (name, file path)
  - All Database Tables (name, columns, relationships)
  - All Configuration Files (name, path)

### 1.2 Count Totals
Produce a count summary:

| Category | Count |
|----------|-------|
| Stored Procedures | [X] |
| Functions/Triggers | [X] |
| UI Forms/Screens | [X] |
| Data Access Methods | [X] |
| Model/DTO Classes | [X] |
| Database Tables | [X] |
| Total Artifacts | [X] |


---

## STEP 2: CROSS-REFERENCE

### 2.1 Load All Test Case Files
- Read all `docs/test-cases/[module]-*.md` files
- Parse all test case IDs and their linked artifacts
- Build a reverse index: Artifact → [list of test cases covering it]

### 2.2 Verify Each Stored Procedure
For each SP in the inventory, check:
- [ ] Has at least ONE SP test (SP-[MODULE]-[NUM])
- [ ] Has at least ONE functional test referencing it (FT-[MODULE]-[NUM])
- [ ] Has at least ONE future API test mapping (API-[MODULE]-[NUM])
- [ ] Has at least ONE business rule extracted (BR-[MODULE]-[NUM]) — if it contains validation logic
- [ ] Has at least ONE user story linked (US-[MODULE]-[NUM]) — if it's user-facing

### 2.3 Verify Each UI Form/Screen
For each form in the inventory, check:
- [ ] Has at least ONE UI test (UI-[MODULE]-[NUM])
- [ ] Has at least ONE functional test referencing it (FT-[MODULE]-[NUM])
- [ ] Has at least ONE user story (US-[MODULE]-[NUM])
- [ ] All controls on the form have validation tests (if required fields exist)

### 2.4 Verify Each Data Access Method
For each DAO method in the inventory, check:
- [ ] Has at least ONE unit test (UT-[MODULE]-[NUM])
- [ ] Has at least ONE integration test mapping (MT-[MODULE]-[NUM])

### 2.5 Verify Each Database Table
For each table in the inventory, check:
- [ ] Has at least ONE data validation test (DV-[MODULE]-[NUM])
- [ ] FK relationships have referential integrity tests
- [ ] Unique constraints have duplicate-check tests

### 2.6 Verify Business Rules
For each business rule discovered:
- [ ] Has at least ONE test case that validates the rule
- [ ] Rule source (file, line) is documented
- [ ] Enforcement mechanism is identified

### 2.7 Verify User Stories
For each user story:
- [ ] Has linked acceptance criteria in Given/When/Then format
- [ ] Has at least ONE functional test validating it
- [ ] Has linked artifacts (SP, form, business rule)


---

## STEP 3: PRODUCE TRACEABILITY MATRIX

**File**: `docs/test-cases/TRACEABILITY-MATRIX.md`

### 3.1 Full Traceability Matrix

#### Stored Procedures → Test Coverage

| SP Name | SP Tests | Functional Tests | API Tests | Business Rules | User Stories | Status |
|---------|----------|-----------------|-----------|----------------|--------------|--------|
| [schema].[sp_name] | SP-MOD-001 | FT-MOD-001 | API-MOD-001 | BR-MOD-001 | US-MOD-001 | ✅ Covered |
| [schema].[sp_name] | — | — | — | — | — | ❌ NOT COVERED |

#### UI Forms/Screens → Test Coverage

| Form Name | UI Tests | Functional Tests | User Stories | Status |
|-----------|----------|-----------------|--------------|--------|
| [form_name] | UI-MOD-001 | FT-MOD-001 | US-MOD-001 | ✅ Covered |
| [form_name] | — | — | — | ❌ NOT COVERED |

#### Data Access Methods → Test Coverage

| Class.Method | Unit Tests | Integration Tests | Status |
|-------------|-----------|-------------------|--------|
| [Class].[Method] | UT-MOD-001 | MT-MOD-001 | ✅ Covered |
| [Class].[Method] | — | — | ❌ NOT COVERED |

#### Database Tables → Test Coverage

| Table Name | Data Validation Tests | FK Tests | Constraint Tests | Status |
|-----------|----------------------|----------|-----------------|--------|
| [table] | DV-MOD-001 | DV-MOD-002 | DV-MOD-003 | ✅ Covered |
| [table] | — | — | — | ❌ NOT COVERED |

#### Business Rules → Test Coverage

| Rule ID | Rule Description | Validating Tests | Status |
|---------|-----------------|-----------------|--------|
| BR-MOD-001 | [description] | SP-MOD-001, FT-MOD-001 | ✅ Covered |
| BR-MOD-005 | [description] | — | ❌ NOT COVERED |

### 3.2 Cross-Reference Summary

| From → To | Total Links | Missing Links | Coverage % |
|-----------|-------------|---------------|------------|
| SP → SP Tests | [X] | [Y] | [Z%] |
| SP → Functional Tests | [X] | [Y] | [Z%] |
| SP → API Tests | [X] | [Y] | [Z%] |
| SP → Business Rules | [X] | [Y] | [Z%] |
| Form → UI Tests | [X] | [Y] | [Z%] |
| Form → User Stories | [X] | [Y] | [Z%] |
| DAO → Unit Tests | [X] | [Y] | [Z%] |
| Table → Data Validation | [X] | [Y] | [Z%] |


---

## STEP 4: GAP REPORT

**File**: `docs/test-cases/GAP-ANALYSIS.md`

### 4.1 Coverage Summary

| Category | Total | Covered | Gaps | Coverage % | Target |
|----------|-------|---------|------|------------|--------|
| Stored Procedures | [X] | [Y] | [Z] | [%] | 100% |
| UI Forms/Screens | [X] | [Y] | [Z] | [%] | 100% |
| Data Access Methods | [X] | [Y] | [Z] | [%] | 100% |
| Database Tables | [X] | [Y] | [Z] | [%] | 100% |
| Business Rules | [X] | [Y] | [Z] | [%] | 100% |
| User Stories | [X] | [Y] | [Z] | [%] | 100% |

Overall Score: [X]% coverage across all categories

### 4.2 Detailed Gaps

#### Stored Procedures Without Tests
| # | SP Name | File Path | Possible Reason |
|---|---------|-----------|-----------------|
| 1 | [schema].[sp_name] | [path] | [reason] |

#### Forms Without Tests
| # | Form Name | File Path | Possible Reason |
|---|-----------|-----------|-----------------|
| 1 | [form_name] | [path] | [reason] |

#### Business Rules Without Validating Tests
| # | Rule ID | Description | Source | Possible Reason |
|---|---------|-------------|--------|-----------------|
| 1 | BR-MOD-XXX | [description] | [file:line] | [reason] |

#### Data Validations Without Tests
| # | Table | Constraint | Type | Possible Reason |
|---|-------|-----------|------|-----------------|
| 1 | [table] | [FK/unique/check] | [type] | [reason] |

#### DAO Methods Without Unit Tests
| # | Class.Method | File Path | Possible Reason |
|---|-------------|-----------|-----------------|
| 1 | [class].[method] | [path] | [reason] |


---

## STEP 5: RECOMMENDATIONS

**File**: `docs/test-cases/COVERAGE-REPORT.md`

For each gap identified, provide:

### Priority 1: Critical Gaps (Business-Critical, Active Code)
| # | Item | Type | Recommendation | Effort |
|---|------|------|---------------|--------|
| 1 | [item] | [SP/Form/Rule] | Generate [test type] immediately | [Low/Med/High] |

### Priority 2: Important Gaps (Active Code, Lower Risk)
| # | Item | Type | Recommendation | Effort |
|---|------|------|---------------|--------|
| 1 | [item] | [SP/Form/Rule] | Generate [test type] in next iteration | [Low/Med/High] |

### Priority 3: Low Priority / Potential Dead Code
| # | Item | Type | Recommendation | Effort |
|---|------|------|---------------|--------|
| 1 | [item] | [SP/Form/Rule] | Verify if still in use; skip if dead code | [Low] |

### Dead Code Analysis

Items that appear to be unused (no references from any form, DAO, or other SP):

| # | Item | Type | Evidence | Recommendation |
|---|------|------|----------|---------------|
| 1 | [item] | [SP] | No callers found in codebase | Mark as dead code, consider removal |
| 2 | [item] | [Form] | Not referenced in any menu or navigation | Verify with team, may be hidden feature |

### Summary Statistics

- **Total Gaps Found**: [X]
- **Critical (must fix)**: [Y]
- **Important (should fix)**: [Z]
- **Low priority / Dead code**: [W]
- **Estimated Effort to Close All Gaps**: [hours/days]

---

## OUTPUT FILES

All output is saved to:

```
docs/test-cases/
├── INVENTORY.md                (from Skill 1, read as input)
├── TRACEABILITY-MATRIX.md      (generated by this skill)
├── COVERAGE-REPORT.md          (generated by this skill)
└── GAP-ANALYSIS.md             (generated by this skill)
```

---

## RULES

1. **Coverage target: 100%** — every SP, every form, every business rule must have at least one test case
2. **Every gap must be explicitly listed** — no silent omissions
3. **Provide a reason for each gap** — why it might have been missed:
   - Dead code (never called)
   - Too complex (needs manual analysis)
   - Dynamic/runtime generated (can't detect statically)
   - External dependency (third-party, COM, etc.)
   - Batch/scheduled job (not triggered by UI)
4. **Flag dead code explicitly** — SPs that are never called from any form/DataAccess class
5. **Run after Skill 1 for each module** — verify incrementally
6. **Final full-pass after all modules** — run against the entire codebase for comprehensive verification
7. **Prioritize gaps** — Critical > Important > Low Priority
8. **Include effort estimates** — Low (< 1 hour), Medium (1-4 hours), High (4+ hours)
9. **Cross-reference bidirectionally** — every test should trace back to an artifact, every artifact should trace forward to tests
10. **Track progress over time** — if run multiple times, show improvement

---

## WORKFLOW INTEGRATION

### How to Use With Skill 1

```
1. Run Skill 1 on Module A → generates test artifacts
2. Run Skill 2 on Module A → verifies coverage, flags gaps
3. Fix gaps (generate missing test cases using Skill 1 patterns)
4. Re-run Skill 2 → verify gaps are closed
5. Repeat steps 1-4 for next module
6. After ALL modules → Run Skill 2 on entire codebase (final pass)
7. Deliver: Complete test suite + 100% traceability
```

### Success Criteria
- All SPs have at least one test case
- All forms have at least one UI test
- All business rules have validating tests
- All data relationships have integrity tests
- All gaps are documented with reasons and priorities
- Dead code is identified and flagged
- Traceability matrix links everything together
