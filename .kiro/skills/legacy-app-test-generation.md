# Skill: Legacy App Test Generation

## Description
A comprehensive, reusable skill that works on ANY legacy codebase to automatically discover all artifacts and generate complete test documentation covering stored procedures, UI, functional, unit, non-functional, API (future-state), integration, data validation tests, user stories, and business rules.

## When to Use
- When asked to generate test cases for a legacy application
- When modernizing a legacy app and need a testing safety net
- When performing test coverage analysis on an existing codebase
- When creating test documentation for a system with no existing tests

## Prerequisites
- Access to the source code repository
- No hardcoded paths required — everything is auto-discovered

---

## STEP 1: DISCOVERY (Auto-detect — No Hardcoded Paths)

### 1.1 Scan Repository Structure
- Recursively scan the entire repository
- Build a complete directory tree
- Identify the technology stack automatically:
  - **VB.NET**: Look for `*.vb`, `*.vbproj`, `*.sln` files
  - **C#**: Look for `*.cs`, `*.csproj`, `*.sln` files
  - **Java**: Look for `*.java`, `pom.xml`, `build.gradle` files
  - **Python**: Look for `*.py`, `requirements.txt`, `setup.py` files
  - **Other**: Adapt based on file extensions found

### 1.2 Identify Stored Procedures
- Search for `*.sql` files containing `CREATE PROCEDURE`, `CREATE PROC`, `ALTER PROCEDURE`
- Search for `*.sql` files containing `CREATE FUNCTION`, `CREATE TRIGGER`
- Look in folders named: `SQL`, `Database`, `DB`, `StoredProcs`, `Procedures`, `Scripts`, `Migrations`
- Also check embedded SQL in code files (inline SQL strings)
- Extract: procedure name, parameters (name, type, direction), body logic, error handling

### 1.3 Identify UI Forms/Screens
- **VB.NET/WinForms**: `*.vb` + `*.Designer.vb` files in `Forms`, `Views`, `UI`, `Screens`, `Windows` folders
- **WPF**: `*.xaml` + `*.xaml.vb` or `*.xaml.cs` files
- **Java Swing/AWT**: `*.java` files extending JFrame, JPanel, JDialog
- **JSP/Web**: `*.jsp`, `*.aspx`, `*.cshtml`, `*.html` files in `Views`, `Pages` folders
- **C# WinForms**: `*.cs` + `*.Designer.cs` files
- Extract: all controls (name, type, text/label), event handlers, validation logic

### 1.4 Identify Data Access / DAO Classes
- Files with names containing: `DataAccess`, `DAO`, `Repository`, `Dal`, `DbHelper`, `Service`, `Gateway`
- Files in folders named: `DataAccess`, `DAL`, `Repository`, `Data`, `Persistence`, `Infrastructure`
- Extract: method names, SP calls, SQL queries, parameters, return types

### 1.5 Identify Model / DTO Classes
- Files in folders named: `Models`, `Entities`, `DTOs`, `Domain`, `POCO`, `BusinessObjects`
- Files with names containing: `Model`, `Entity`, `DTO`, `BO`, `Record`
- Extract: properties/fields, data types, validation attributes, relationships

### 1.6 Identify Configuration Files
- `*.config`, `*.json`, `*.xml`, `*.yaml`, `*.yml`, `*.properties`, `*.ini`
- Connection strings, app settings, feature flags
- Environment-specific configurations

### 1.7 Identify Database Tables and Relationships
- Look for SQL schema files (`CREATE TABLE` statements)
- Look for Entity Framework migrations or ORM mapping files
- Look for ERD documentation or database project files (`.dbproj`, `.sqlproj`)
- Extract: table names, columns, data types, FK relationships, constraints, indexes

### 1.8 Produce Complete Inventory
Generate `docs/test-cases/INVENTORY.md` with:

```markdown
# Application Inventory

## Technology Stack
- Language: [detected]
- UI Framework: [detected]
- Database: [detected]
- ORM/Data Access: [detected]

## Summary Counts
| Category | Count |
|----------|-------|
| Stored Procedures | X |
| Functions/Triggers | X |
| UI Forms/Screens | X |
| Data Access Classes | X |
| Model/DTO Classes | X |
| Database Tables | X |
| Configuration Files | X |

## Detailed Listing
[List every item discovered with file path]
```

---

## STEP 2: GENERATE TEST ARTIFACTS

For each discovered artifact, generate ALL of the following categories. Process module-by-module if codebase is too large for one pass.

---

### A. STORED PROCEDURE TESTS

**File**: `docs/test-cases/[module]-sp-tests.md`

For each stored procedure, generate:

```markdown
### Test Case ID: SP-[MODULE]-[NUMBER]
**Procedure**: [schema].[procedure_name]
**Parameters**: [list with types]

#### Positive Tests (Valid Inputs → Expected Output)
| # | Description | Input Parameters | Expected Result |
|---|------------|-----------------|-----------------|
| 1 | [description] | @param1='value', @param2=123 | Returns [expected] |

#### Negative Tests (Invalid Inputs → Expected Errors)
| # | Description | Input Parameters | Expected Error |
|---|------------|-----------------|----------------|
| 1 | [description] | @param1=NULL | Error: [message] |

#### Boundary Tests (Edge Cases)
| # | Description | Input Parameters | Expected Behavior |
|---|------------|-----------------|-------------------|
| 1 | Max length input | @param1='[255 chars]' | [behavior] |
| 2 | Zero/empty value | @param1='' | [behavior] |
| 3 | Minimum date | @date='1900-01-01' | [behavior] |

#### Data Dependency Tests (FK Relationships)
| # | Description | Pre-condition | Input | Expected |
|---|------------|--------------|-------|----------|
| 1 | Parent record must exist | Insert parent first | @parentId=1 | Success |
| 2 | Orphan FK reference | No parent exists | @parentId=999 | FK violation |

#### Executable SQL Script
```sql
-- Setup
-- [Insert required test data]

-- Test: Positive case
EXEC [schema].[procedure_name] @param1='value', @param2=123
-- Expected: [result description]

-- Test: Negative case
EXEC [schema].[procedure_name] @param1=NULL
-- Expected: Error [number/message]

-- Cleanup
-- [Delete test data]
```
```

---

### B. FUNCTIONAL TESTS (E2E Business Workflows)

**File**: `docs/test-cases/[module]-functional-tests.md`

Infer workflows from: Form → DataAccess → SP call chains.

```markdown
### Test Case ID: FT-[MODULE]-[NUMBER]
**Workflow**: [workflow name]
**Module**: [module name]
**Priority**: Critical / High / Medium / Low

#### Pre-conditions
- [ ] [Data that must exist before test starts]
- [ ] [User role/permissions required]
- [ ] [System state required]

#### Test Steps
| Step | Screen/Action | Field/Control | Value/Input | Expected Result |
|------|--------------|---------------|-------------|-----------------|
| 1 | Open [form] | — | — | Form loads, all fields empty |
| 2 | Enter data | [field name] | [value] | Field accepts input |
| 3 | Click [button] | — | — | [expected behavior] |
| 4 | Verify | [element] | — | [expected state] |

#### Post-conditions
- [ ] [What changed in the database]
- [ ] [What state the UI is in]
- [ ] [What notifications/messages appeared]

#### Related Artifacts
- Stored Procedure: [SP name]
- Form: [form name]
- Business Rule: BR-[MODULE]-[NUMBER]
```

---

### C. UI TESTS (Screen-Level)

**File**: `docs/test-cases/[module]-ui-tests.md`

For each form/screen discovered:

```markdown
### Test Case ID: UI-[MODULE]-[NUMBER]
**Form/Screen**: [form name]
**File**: [file path]

#### Form Load Tests
| # | Test | Expected Result |
|---|------|-----------------|
| 1 | Form opens without error | All controls visible and properly positioned |
| 2 | Dropdowns populated | [dropdown] contains [expected items] |
| 3 | Default values set | [field] defaults to [value] |
| 4 | Focus on correct control | [first field] has focus |

#### Required Field Validation Tests
| # | Field | Action | Expected Error Message |
|---|-------|--------|----------------------|
| 1 | [field name] | Leave blank, click Save | "[specific error message]" |
| 2 | [field name] | Leave blank, tab away | "[specific error message]" |

#### Input Format Validation Tests
| # | Field | Invalid Input | Expected Behavior |
|---|-------|--------------|-------------------|
| 1 | [date field] | "abc" | Error: "Invalid date format" |
| 2 | [number field] | "xyz" | Error: "Must be numeric" |
| 3 | [email field] | "notanemail" | Error: "Invalid email" |
| 4 | [phone field] | "123" | Error: "Invalid phone format" |

#### Button State Tests
| # | Context/Condition | Button | Expected State |
|---|------------------|--------|----------------|
| 1 | Form just loaded, no data | Save | Disabled |
| 2 | All required fields filled | Save | Enabled |
| 3 | No row selected in grid | Delete | Disabled |

#### Dirty Flag / Unsaved Changes Tests
| # | Action | Expected Behavior |
|---|--------|-------------------|
| 1 | Modify field, click Close | "Unsaved changes" warning dialog |
| 2 | Modify field, click Save, click Close | No warning, closes normally |
| 3 | Open form, change nothing, click Close | No warning, closes normally |

#### Navigation Tests
| # | Action | Expected Result |
|---|--------|-----------------|
| 1 | Open from menu [path] | Form opens as [modal/modeless] |
| 2 | Click [link/button] | Navigates to [target form] |
| 3 | Press Escape | [closes/does nothing] |
| 4 | Tab order | Focus moves: [control1] → [control2] → [control3] |

#### Error Handling Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database connection lost during Save | Error message: "[text]", form remains open |
| 2 | SP returns error | Error displayed: "[text]", no data corruption |
| 3 | Timeout during long operation | Progress indicator shown, graceful timeout message |
```

---

### D. UNIT TESTS (DataAccess/DAO Layer)

**File**: `docs/test-cases/[module]-unit-tests.md`

```markdown
### Test Case ID: UT-[MODULE]-[NUMBER]
**Class**: [class name]
**Method**: [method name]
**File**: [file path]

#### Method Signature
- Parameters: [name: type, ...]
- Return Type: [type]
- Throws: [exception types]

#### Test Scenarios
| # | Scenario | Input | Expected Return | Notes |
|---|----------|-------|-----------------|-------|
| 1 | Valid input | [values] | [expected object/value] | Happy path |
| 2 | Null parameter | null | Throws ArgumentNullException | |
| 3 | Empty result from SP | — | Returns empty list / null | |
| 4 | SP throws SQL exception | — | Wraps in [AppException] or rethrows | |
| 5 | Connection failure | — | Throws [specific exception] | |

#### Mock/Stub Requirements
- Mock: [database connection / SP call]
- Setup: [what the mock should return]
```

---

### E. NFR / PERFORMANCE TESTS (Non-Functional)

**File**: `docs/test-cases/[module]-nfr-tests.md`

```markdown
### Test Case ID: NFR-[MODULE]-[NUMBER]
**Operation**: [description]
**Component**: [SP / Form / Service]

#### Performance Tests
| # | Scenario | SLA Target | Data Volume | Concurrent Users |
|---|----------|-----------|-------------|-----------------|
| 1 | [operation] | < 2 seconds | 10K records | 1 user |
| 2 | [operation] | < 5 seconds | 50K records | 10 users |
| 3 | [operation] | < 10 seconds | 100K records | 50 users |

#### Scalability Tests
| # | Scenario | Metric to Measure | Threshold |
|---|----------|------------------|-----------|
| 1 | Increase concurrent users | Response time degradation | < 20% increase |
| 2 | Increase data volume 10x | Query execution time | < 3x increase |

#### Resource Usage Tests
| # | Scenario | Resource | Threshold |
|---|----------|----------|-----------|
| 1 | Extended operation | Memory usage | < 500MB |
| 2 | Bulk processing | CPU usage | < 80% sustained |
| 3 | Concurrent connections | DB connection pool | No pool exhaustion |

#### Reliability Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | Database restart during operation | Graceful reconnection |
| 2 | Network timeout | Retry with backoff |
| 3 | Disk full | Graceful error, no corruption |
```

---

### F. API TESTS (Future-State Baseline for Modernization)

**File**: `docs/test-cases/[module]-api-tests.md`

> **NOTE**: These are FUTURE tests. The legacy app has no APIs yet. These map current SP functionality to future REST endpoints.

```markdown
### Test Case ID: API-[MODULE]-[NUMBER]
**Legacy SP**: [procedure name]
**Future Endpoint**: [HTTP METHOD] /api/[resource]
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| SP: usp_[Entity]_Create | POST /api/[entities] |
| SP: usp_[Entity]_GetById | GET /api/[entities]/{id} |
| SP: usp_[Entity]_Update | PUT /api/[entities]/{id} |
| SP: usp_[Entity]_Delete | DELETE /api/[entities]/{id} |
| SP: usp_[Entity]_Search | GET /api/[entities]?filter=value |

#### Request/Response Specification
**Request:**
```json
{
  "field1": "value (mapped from @param1)",
  "field2": 123 (mapped from @param2)
}
```

**Success Response (200/201):**
```json
{
  "id": 1,
  "field1": "value",
  "createdDate": "2024-01-01T00:00:00Z"
}
```

#### Test Scenarios
| # | Scenario | Method | Path | Body | Expected Status | Expected Response |
|---|----------|--------|------|------|----------------|-------------------|
| 1 | Create valid | POST | /api/[resource] | {valid} | 201 Created | {created object} |
| 2 | Create invalid | POST | /api/[resource] | {invalid} | 400 Bad Request | {validation errors} |
| 3 | Get existing | GET | /api/[resource]/1 | — | 200 OK | {object} |
| 4 | Get non-existent | GET | /api/[resource]/999 | — | 404 Not Found | {error} |
| 5 | Update valid | PUT | /api/[resource]/1 | {valid} | 200 OK | {updated object} |
| 6 | Delete existing | DELETE | /api/[resource]/1 | — | 204 No Content | — |
| 7 | Unauthorized | POST | /api/[resource] | {valid} | 401 Unauthorized | {error} |

#### Auth Requirements
- Authentication: [Bearer token / API key / None]
- Authorization: [Role required]
```

---

### G. MODULE / INTEGRATION TESTS (Future-State Baseline)

**File**: `docs/test-cases/[module]-integration-tests.md`

> **NOTE**: These are FUTURE tests for the modernized architecture.

```markdown
### Test Case ID: MT-[MODULE]-[NUMBER]
**Integration**: [Component A] → [Component B]
**Status**: FUTURE-STATE

#### Component Interaction Tests
| # | Scenario | Source | Target | Data Flow | Expected |
|---|----------|--------|--------|-----------|----------|
| 1 | [workflow step] | [Service A] | [DAO B] | [data] | [result] |
| 2 | Cross-module call | [Module A] | [Module B] | [data] | [result] |

#### Transaction Behavior Tests
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | All steps succeed | Transaction commits, all data persisted |
| 2 | Step 3 fails | Transaction rolls back, no partial data |
| 3 | Timeout in middle | Transaction rolls back, resources released |

#### Data Flow Tests
| # | Flow | Input | Intermediate State | Final State |
|---|------|-------|-------------------|-------------|
| 1 | [Entity] creation flow | [input] | [mid-state] | [end-state] |
```

---

### H. DATA VALIDATION TESTS

**File**: `docs/test-cases/[module]-data-validation-tests.md`

```markdown
### Test Case ID: DV-[MODULE]-[NUMBER]
**Table/Entity**: [table name]
**Database**: [database name]

#### Referential Integrity Tests
| # | Parent Table | Child Table | FK Column | Test Action | Expected |
|---|-------------|------------|-----------|-------------|----------|
| 1 | [Parent] | [Child] | [FK col] | Delete parent with children | FK violation error |
| 2 | [Parent] | [Child] | [FK col] | Insert child with invalid parent ID | FK violation error |

#### Business Rule Enforcement Tests
| # | Rule | Table | Test Action | Expected |
|---|------|-------|-------------|----------|
| 1 | [rule description] | [table] | [violating action] | [error/prevention] |

#### Data Type Validation Tests
| # | Column | Type | Max Length | Test Value | Expected |
|---|--------|------|-----------|-----------|----------|
| 1 | [column] | VARCHAR | 50 | [51 chars] | Truncation or error |
| 2 | [column] | DECIMAL(10,2) | — | 99999999.999 | Overflow error |
| 3 | [column] | DATE | — | '9999-12-31' | Accepted or rejected |

#### Unique Constraint Tests
| # | Table | Columns | Test Action | Expected |
|---|-------|---------|-------------|----------|
| 1 | [table] | [unique cols] | Insert duplicate | Unique constraint violation |

#### Cascade Behavior Tests
| # | Parent Table | Child Table | On Delete | On Update | Test | Expected |
|---|-------------|------------|-----------|-----------|------|----------|
| 1 | [parent] | [child] | CASCADE | CASCADE | Delete parent | Children deleted |
| 2 | [parent] | [child] | RESTRICT | CASCADE | Delete parent | Error, blocked |
```

---

### I. USER STORIES

**File**: `docs/test-cases/[module]-user-stories.md`

```markdown
### Story ID: US-[MODULE]-[NUMBER]
**Priority**: Critical / High / Medium / Low
**Module**: [module name]

#### User Story
As a **[role]**,
I want to **[action]**,
So that **[outcome/business value]**.

#### Acceptance Criteria

**Given** [precondition/context]
**When** [action performed]
**Then** [expected outcome]

**Given** [alternative precondition]
**When** [alternative action]
**Then** [alternative outcome]

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-[MODULE]-[NUM] | [SP name] |
| Form/Screen | UI-[MODULE]-[NUM] | [form name] |
| Business Rule | BR-[MODULE]-[NUM] | [rule description] |
| Functional Test | FT-[MODULE]-[NUM] | [test name] |
```

---

### J. BUSINESS RULES

**File**: `docs/test-cases/[module]-business-rules.md`

```markdown
### Rule ID: BR-[MODULE]-[NUMBER]
**Module**: [module name]
**Priority**: Critical / High / Medium / Low

#### Rule Description
[Plain English description of the business rule]

#### Source
- **File**: [file path]
- **SP/Method**: [procedure or method name]
- **Line/Section**: [where in the code this is enforced]
- **Code Snippet**:
```
[relevant code that enforces this rule]
```

#### Enforcement Mechanism
- Type: [Error message / Status check / DB constraint / UI validation / SP logic]
- Error Message: "[exact error text shown to user or raised]"
- Behavior: [what happens when rule is violated]

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| [ID] | [validates this rule how] |

#### Edge Cases
| # | Scenario | Expected Behavior |
|---|----------|-------------------|
| 1 | [boundary case] | [rule applies/doesn't apply] |
```

---

## STEP 3: OUTPUT STRUCTURE

All output files are saved under `docs/test-cases/` with the following naming:

```
docs/test-cases/
├── INVENTORY.md                          (Full application inventory)
├── [module]-sp-tests.md                  (Stored procedure tests)
├── [module]-functional-tests.md          (E2E business workflow tests)
├── [module]-ui-tests.md                  (Screen-level UI tests)
├── [module]-unit-tests.md                (DataAccess/DAO unit tests)
├── [module]-nfr-tests.md                (Non-functional/performance tests)
├── [module]-api-tests.md                 (Future-state API tests)
├── [module]-integration-tests.md         (Future-state integration tests)
├── [module]-data-validation-tests.md     (Data integrity tests)
├── [module]-user-stories.md              (User stories with acceptance criteria)
└── [module]-business-rules.md            (Business rules catalog)
```

---

## RULES

1. **No hardcoded paths** — discover everything automatically by scanning the repository
2. **Language agnostic** — work with VB.NET, C#, Java, Python, or any other language found
3. **Database agnostic** — work with SQL Server, Oracle, PostgreSQL, MySQL, or any other DB
4. **Module-by-module processing** — if codebase is too large for one context pass, process in batches
5. **Always include Test Case IDs** — for traceability (format: [TYPE]-[MODULE]-[NUMBER])
6. **Mark future-state tests clearly** — API and Integration tests are for the modernized architecture
7. **Extract actual values from code** — don't invent error messages or validation rules; read them from the source
8. **Preserve business logic** — document what the code actually does, even if it seems incorrect
9. **Flag assumptions** — when inferring behavior that isn't explicit in code, mark it as [ASSUMPTION]
10. **Include file paths** — always reference the source file for each test case
