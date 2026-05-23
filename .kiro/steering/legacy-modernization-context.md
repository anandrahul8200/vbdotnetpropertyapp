# Legacy Application Modernization Context

This steering file provides context about the legacy application modernization
approach, guiding Kiro's behavior when working with legacy codebases.

---

## Project Goal

Modernize a legacy thick-client desktop application by:
1. First: Building a comprehensive test safety net (characterization tests)
2. Then: Refactoring and modernizing with confidence (tests catch regressions)
3. Finally: Migrating to modern architecture (tests validate equivalence)

---

## Technology Detection Rules

When scanning a repository, identify the technology stack using these indicators:

### Language Detection
| Indicator | Technology |
|-----------|-----------|
| *.vb, *.vbproj | VB.NET |
| *.cs, *.csproj | C# |
| *.java, pom.xml, build.gradle | Java |
| *.py, requirements.txt | Python |
| *.Designer.vb or *.Designer.cs | WinForms |
| *.xaml | WPF |
| *.jsp, *.aspx | Web Forms |

### UI Framework Detection
| Indicator | Framework |
|-----------|-----------|
| System.Windows.Forms references | WinForms |
| *.Designer.vb / *.Designer.cs files | WinForms (with visual designer) |
| *.xaml + code-behind | WPF |
| Imports System.Windows.Forms (VB.NET) | WinForms |
| using System.Windows.Forms; (C#) | WinForms |

### Database Detection
| Indicator | Database |
|-----------|----------|
| System.Data.SqlClient | SQL Server |
| Oracle.ManagedDataAccess | Oracle |
| Npgsql | PostgreSQL |
| MySql.Data | MySQL |
| System.Data.OleDb + .mdb/.accdb | MS Access |
| EXEC, sp_, usp_ patterns in SQL | SQL Server stored procedures |


---

## Legacy Code Patterns to Recognize

### Common VB.NET WinForms Patterns
- Code-behind monoliths: All logic in form event handlers (btnSave_Click, Form_Load)
- Module-based utilities: Shared functions in .bas or Module files
- Global variables: State stored in public shared/static variables
- Direct database calls: SqlCommand / OleDbCommand directly in form code
- No dependency injection: Everything tightly coupled
- String concatenation SQL: Inline SQL built with string concatenation (security risk)

### Common Data Access Patterns in Legacy Apps
- ADO.NET direct: SqlConnection → SqlCommand → SqlDataReader
- DataSet/DataTable: Typed or untyped datasets for data binding
- TableAdapter: Auto-generated data access via Visual Studio designers
- Stored Procedure calls: Business logic in database layer
- Inline SQL: Queries embedded in code-behind

### Common UI Patterns in Legacy WinForms
- MDI (Multiple Document Interface): Parent form with child forms
- Tab-based navigation: TabControl with multiple panels
- Master-detail forms: Grid + detail panel
- Lookup dialogs: Modal forms that return selected values
- Wizard-style forms: Multi-step form with Next/Back/Finish

---

## Module Identification Strategy

When the codebase doesn't have clear module boundaries, identify modules by:

1. Folder structure: Top-level folders often represent modules
2. Form groupings: Forms that share a prefix (e.g., frmPolicy_*, frmClaim_*)
3. SP naming conventions: SPs prefixed by entity (e.g., usp_Policy_*, usp_Claim_*)
4. Menu structure: Main menu items often map to functional modules
5. Database schema: Schema names or table prefixes indicate modules
6. Namespace/project grouping: Namespaces or sub-projects suggest module boundaries

### Default Module Mapping
If modules cannot be clearly identified, use these generic groupings:
- CORE — Core/shared functionality
- ADMIN — Administration and configuration
- DATA — Data management and maintenance
- RPT — Reporting
- UTIL — Utilities and helpers

---

## Processing Strategy for Large Codebases

When a codebase is too large to process in a single context window:

### Batch Processing Order
1. Inventory first — Always do full discovery/inventory in the first pass
2. Critical modules first — Process the most business-critical modules before less important ones
3. Module-by-module — Process one complete module at a time (all test types for that module)
4. Verify incrementally — Run coverage verification after each module
5. Final pass — Run full traceability check after all modules

### Context Window Management
- If a module has more than ~50 SPs or ~20 forms, split into sub-modules
- Reference INVENTORY.md across sessions for continuity
- Commit generated test cases to Git after each module (preserves progress)
- Use the docs/test-cases/ folder as the source of truth for what's done

---

## Handling Ambiguity in Legacy Code

Legacy code often has unclear intent. Follow these principles:

1. Document what it DOES, not what it SHOULD do — characterization testing captures actual behavior
2. Flag suspicious logic — if code seems buggy, note it but still test the current behavior
3. Mark dead code — if a function/SP is never called, flag it but don't skip it entirely
4. Note missing error handling — document where exceptions would propagate unhandled
5. Identify hardcoded values — magic numbers, hardcoded strings that should be configurable


---

## Modernization Path Awareness

When generating future-state tests (API, Integration), consider these common modernization paths:

### Desktop to Web Migration Patterns
| Legacy (WinForms) | Modern (Web) |
|-------------------|--------------|
| Form with Save button | POST /api/[resource] |
| Form Load (read data) | GET /api/[resource]/{id} |
| Grid with Edit row | PUT /api/[resource]/{id} |
| Delete button with confirm | DELETE /api/[resource]/{id} |
| Search form with filters | GET /api/[resource]?filter=value |
| Lookup dialog | GET /api/[lookup-resource] |
| Report form | GET /api/reports/[report-name] |

### SP to Service Layer Mapping
| Legacy | Modern |
|--------|--------|
| usp_Entity_Create | EntityService.CreateAsync() |
| usp_Entity_GetById | EntityService.GetByIdAsync() |
| usp_Entity_Update | EntityService.UpdateAsync() |
| usp_Entity_Delete | EntityService.DeleteAsync() |
| usp_Entity_Search | EntityService.SearchAsync() |
| usp_Entity_Validate | EntityValidator.ValidateAsync() |

---

## Communication Standards

When generating documentation and interacting with the user:

- Be explicit about what was found — show counts, file paths, specific names
- Show progress — indicate which module is being processed and how many remain
- Highlight risks — if business logic is only in SPs (no application layer), flag this
- Suggest improvements — note obvious code smells or security issues found during analysis
- Use consistent formatting — follow the templates defined in the skills exactly
- Never silently skip items — if something is skipped, explain why

---

## Version Control Integration

- After generating test artifacts for a module, commit to a feature branch
- Branch naming: feature/test-generation-[module] or feature/test-documentation
- Commit message format: docs: generate [test-type] tests for [module] module
- Push after each module is complete (enables incremental review)
