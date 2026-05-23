# Testing Standards & Conventions

This steering file defines the testing standards, naming conventions, frameworks,
and output formats to follow when generating test artifacts for legacy application modernization.

---

## Test Case ID Format

All test cases MUST use the following ID format for traceability:

```
[TYPE]-[MODULE]-[NUMBER]
```

### Type Prefixes
| Prefix | Category |
|--------|----------|
| SP | Stored Procedure Tests |
| FT | Functional / E2E Tests |
| UI | UI / Screen-Level Tests |
| UT | Unit Tests (DAO/Service layer) |
| NFR | Non-Functional / Performance Tests |
| API | Future-State API Tests |
| MT | Module / Integration Tests |
| DV | Data Validation Tests |
| US | User Stories |
| BR | Business Rules |

### Module Names
- Use short, uppercase abbreviations (3-5 chars)
- Examples: POL (Policy), CLM (Claims), BIL (Billing), CUS (Customer), ADM (Admin)
- Derive from folder structure or logical grouping in the codebase

### Numbering
- Sequential within each type-module combination
- Start at 001, zero-padded to 3 digits
- Example: SP-POL-001, SP-POL-002, UI-CUS-001


---

## Test Naming Conventions

### Unit Test Methods (when generating executable test code)
```
[MethodUnderTest]_[Scenario]_[ExpectedResult]
```
Examples:
- CreateCustomer_ValidInput_ReturnsCustomerId
- GetPolicy_InvalidId_ThrowsNotFoundException
- CalculatePremium_NullParameters_ThrowsArgumentException

### Test Class Names
```
[ClassUnderTest]Tests
```
Examples:
- CustomerRepositoryTests
- PolicyServiceTests
- PremiumCalculatorTests

---

## Frameworks & Tools

### For .NET Applications (VB.NET / C#)
| Purpose | Tool | NuGet Package |
|---------|------|---------------|
| Test Framework | xUnit | xunit |
| Assertions | FluentAssertions | FluentAssertions |
| Mocking | FakeItEasy or Moq | FakeItEasy / Moq |
| UI Automation | FlaUI | FlaUI.UIA3 |
| Performance | BenchmarkDotNet | BenchmarkDotNet |
| Code Coverage | Coverlet | coverlet.collector |

### For Database Testing
| Purpose | Tool |
|---------|------|
| SQL Server | tSQLt or direct SQL scripts |
| Generic | Direct executable SQL scripts with setup/teardown |

---

## Test Structure (Arrange-Act-Assert)

All tests MUST follow the AAA pattern:

```
// Arrange - Set up test data and preconditions
[setup code]

// Act - Execute the operation under test
[execution code]

// Assert - Verify the expected outcome
[assertion code]
```

For test case documentation (non-executable):
```
Pre-conditions: [what must be true before]
Action: [what is performed]
Expected Result: [what should happen]
Post-conditions: [what is true after]
```

---

## Output File Standards

### Markdown Format
- All test documentation files use `.md` format
- Use tables for structured data (test scenarios, parameters)
- Use code blocks for SQL scripts, code snippets
- Use checkboxes for verification checklists

### File Location
```
docs/test-cases/
├── INVENTORY.md
├── TRACEABILITY-MATRIX.md
├── COVERAGE-REPORT.md
├── GAP-ANALYSIS.md
├── [module]-sp-tests.md
├── [module]-functional-tests.md
├── [module]-ui-tests.md
├── [module]-unit-tests.md
├── [module]-nfr-tests.md
├── [module]-api-tests.md
├── [module]-integration-tests.md
├── [module]-data-validation-tests.md
├── [module]-user-stories.md
└── [module]-business-rules.md
```

### File Naming
- All lowercase
- Hyphens for separators (not underscores)
- Module prefix matches the module abbreviation (lowercase)
- Example: policy-sp-tests.md, claims-ui-tests.md

---

## Priority Levels

Use these priority levels consistently across all artifacts:

| Priority | Meaning | Test Coverage Requirement |
|----------|---------|--------------------------|
| Critical | Core business function, revenue-impacting, data integrity | Must have tests in ALL categories |
| High | Important workflow, frequently used, compliance-related | Must have SP + Functional + UI tests |
| Medium | Standard functionality, moderate usage | Must have at least SP + Functional tests |
| Low | Rarely used, admin/utility, reporting | Must have at least one test case |

---

## User Story Format

Always use this format:

```
As a [role],
I want to [action],
So that [business value/outcome].

Acceptance Criteria:
Given [precondition]
When [action]
Then [expected outcome]
```

---

## Business Rule Documentation Format

```
Rule: [Plain English description]
Source: [file:line or SP name]
Enforcement: [How it's enforced - error message, constraint, validation]
Impact: [What happens if violated]
Test: [Test case ID that validates this rule]
```

---

## Future-State Test Markers

When generating tests for the modernized architecture (API tests, Integration tests), always mark:

> STATUS: FUTURE-STATE — This test is for the modernized architecture.
> The legacy application does not currently expose this functionality as described.
> This test serves as a specification for the target-state implementation.

---

## Quality Gates

Before marking any module as "complete":

1. All stored procedures have at least one SP test
2. All forms/screens have at least one UI test
3. All business rules are documented with validating tests
4. All user-facing workflows have functional tests
5. All DAO methods have unit tests
6. All tables have data validation tests
7. Traceability matrix links all artifacts
8. Gap analysis shows 0 critical/high priority gaps
9. Dead code is identified and flagged

---

## Assumptions Handling

When behavior cannot be determined from code alone:

> [ASSUMPTION]: [Description of what is assumed]
> Basis: [Why this assumption was made]
> Verification needed: [What to check with the team/SME]

Always flag assumptions — never silently invent behavior.
