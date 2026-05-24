# Admin Module - Data Validation Tests

## Module: ADM (Admin)
## Test Type: Data Integrity and Validation Tests
## Tables Covered:
- `Admin.Users`
- `Admin.Roles`
- `Admin.Permissions`
- `Admin.RolePermissions`
- `Admin.SystemConfig`
- `Admin.LookupCategories`
- `Admin.LookupValues`
- `Admin.States`
- `Audit.AuditLog`
- `Audit.ErrorLog`
- `Batch.JobLog`

---

### Test Case ID: DV-ADM-001
**Table**: Admin.Users
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| UserID | INT IDENTITY | PK, auto-increment | Cannot insert explicit value |
| Username | VARCHAR(50) | NOT NULL, UNIQUE | Duplicate rejected, NULL rejected |
| PasswordHash | VARCHAR(256) | NOT NULL | NULL rejected |
| Salt | VARCHAR(128) | NOT NULL | NULL rejected |
| FirstName | VARCHAR(100) | NOT NULL | NULL rejected |
| LastName | VARCHAR(100) | NOT NULL | NULL rejected |
| Email | VARCHAR(200) | Nullable | NULL accepted |
| Phone | VARCHAR(20) | Nullable | NULL accepted |
| RoleID | INT | NOT NULL | NULL rejected |
| IsActive | BIT | DEFAULT 1 | Defaults to 1 if not specified |
| IsLocked | BIT | DEFAULT 0 | Defaults to 0 if not specified |
| FailedLoginAttempts | INT | DEFAULT 0 | Defaults to 0 |
| CreatedDate | DATETIME | DEFAULT GETDATE() | Auto-populated |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Username uniqueness | Insert duplicate username | Constraint violation error |
| 2 | Username max length | 51 character username | Truncation or error |
| 3 | PasswordHash format | salt:base64hash (salt:hash format) | Stored correctly |
| 4 | Email max length | 201 character email | Truncation or error |
| 5 | IsActive defaults | No IsActive specified | Value = 1 |
| 6 | IsLocked defaults | No IsLocked specified | Value = 0 |
| 7 | FailedLoginAttempts range | Negative value | Accepted (no CHECK constraint) [ASSUMPTION] |

---

### Test Case ID: DV-ADM-002
**Table**: Admin.Roles
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| RoleID | INT IDENTITY | PK | Auto-increment |
| RoleName | VARCHAR(50) | NOT NULL, UNIQUE | Duplicate rejected |
| Description | VARCHAR(200) | Nullable | NULL accepted |
| MaxClaimApprovalAmount | DECIMAL(18,2) | Nullable | NULL accepted |
| CanApproveUnderwriting | BIT | DEFAULT 0 | Defaults to 0 |
| CanProcessPayments | BIT | DEFAULT 0 | Defaults to 0 |
| IsActive | BIT | DEFAULT 1 | Defaults to 1 |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | RoleName uniqueness | Duplicate role name | Constraint violation |
| 2 | RoleName NOT NULL | NULL role name | Error |
| 3 | MaxClaimApprovalAmount precision | 1234567890123456.99 | Stored correctly (18,2) |
| 4 | Boolean defaults | No values for BIT fields | CanApproveUnderwriting=0, CanProcessPayments=0, IsActive=1 |

---

### Test Case ID: DV-ADM-003
**Table**: Admin.Permissions
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| PermissionID | INT IDENTITY | PK | Auto-increment |
| PermissionCode | VARCHAR(30) | NOT NULL, UNIQUE | Duplicate rejected |
| PermissionName | VARCHAR(100) | NOT NULL | NULL rejected |
| Module | VARCHAR(50) | NOT NULL | NULL rejected |
| IsActive | BIT | DEFAULT 1 | Defaults to 1 |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | PermissionCode uniqueness | Duplicate code | Constraint violation |
| 2 | Module NOT NULL | NULL module | Error |
| 3 | PermissionCode max length | 31 characters | Error or truncation |

---

### Test Case ID: DV-ADM-004
**Table**: Admin.RolePermissions
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| RolePermissionID | INT IDENTITY | PK | Auto-increment |
| RoleID | INT | NOT NULL, FK -> Admin.Roles | Invalid FK rejected |
| PermissionID | INT | NOT NULL, FK -> Admin.Permissions | Invalid FK rejected |
| CanRead | BIT | DEFAULT 1 | Defaults to 1 |
| CanWrite | BIT | DEFAULT 0 | Defaults to 0 |
| CanDelete | BIT | DEFAULT 0 | Defaults to 0 |
| CanApprove | BIT | DEFAULT 0 | Defaults to 0 |
| (RoleID, PermissionID) | | UNIQUE constraint | Duplicate pair rejected |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | FK to valid Role | RoleID=existing | Accepted |
| 2 | FK to invalid Role | RoleID=99999 | FK violation error |
| 3 | FK to valid Permission | PermissionID=existing | Accepted |
| 4 | FK to invalid Permission | PermissionID=99999 | FK violation error |
| 5 | Unique pair constraint | Same RoleID+PermissionID twice | Unique constraint violation |
| 6 | Permission level defaults | No CanRead/Write/Delete/Approve | CanRead=1, others=0 |

---

### Test Case ID: DV-ADM-005
**Table**: Admin.SystemConfig
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| ConfigID | INT IDENTITY | PK | Auto-increment |
| ConfigKey | VARCHAR(100) | NOT NULL, UNIQUE | Duplicate rejected |
| ConfigValue | VARCHAR(500) | NOT NULL | NULL rejected |
| Description | VARCHAR(500) | Nullable | NULL accepted |
| DataType | VARCHAR(20) | DEFAULT 'STRING' | Defaults to 'STRING' |
| IsActive | BIT | DEFAULT 1 | Defaults to 1 |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | ConfigKey uniqueness | Duplicate key | Constraint violation |
| 2 | ConfigValue NOT NULL | NULL value | Error |
| 3 | ConfigValue max length | 501 characters | Error or truncation |
| 4 | DataType default | No DataType specified | 'STRING' |

---

### Test Case ID: DV-ADM-006
**Table**: Admin.LookupValues
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| LookupID | INT IDENTITY | PK | Auto-increment |
| CategoryID | INT | NOT NULL, FK -> LookupCategories | Invalid FK rejected |
| LookupCode | VARCHAR(50) | NOT NULL | NULL rejected |
| LookupValue | VARCHAR(200) | NOT NULL | NULL rejected |
| DisplayOrder | INT | DEFAULT 0 | Defaults to 0 |
| IsActive | BIT | DEFAULT 1 | Defaults to 1 |
| (CategoryID, LookupCode) | | UNIQUE constraint | Duplicate pair rejected |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | CategoryID FK valid | CategoryID=existing | Accepted |
| 2 | CategoryID FK invalid | CategoryID=99999 | FK violation |
| 3 | Unique code per category | Same CategoryID+LookupCode | Unique constraint violation |
| 4 | Same code different category | Same code, different CategoryID | Accepted |
| 5 | EffectiveDate/ExpiryDate defaults | No dates specified | EffectiveDate=today, ExpiryDate='9999-12-31' |

---

### Test Case ID: DV-ADM-007
**Table**: Admin.States
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| StateID | INT IDENTITY | PK | Auto-increment |
| StateCode | CHAR(2) | NOT NULL, UNIQUE | Duplicate rejected |
| StateName | VARCHAR(100) | NOT NULL | NULL rejected |
| TaxRate | DECIMAL(6,4) | DEFAULT 0 | Defaults to 0 |
| SurchargeRate | DECIMAL(6,4) | DEFAULT 0 | Defaults to 0 |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | StateCode uniqueness | Duplicate 'FL' | Constraint violation |
| 2 | StateCode exactly 2 chars | CHAR(2) type | Padded or exact |
| 3 | Rate precision | TaxRate=0.1234 | Stored as 0.1234 (4 decimals) |
| 4 | Rate overflow | TaxRate=99.99999 | Error (exceeds 6,4 precision) |

---

### Test Case ID: DV-ADM-008
**Table**: Audit.AuditLog
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| AuditID | BIGINT IDENTITY | PK | Auto-increment (BIGINT for high volume) |
| TableName | VARCHAR(100) | NOT NULL | NULL rejected |
| RecordID | INT | NOT NULL | NULL rejected |
| Action | VARCHAR(10) | NOT NULL | NULL rejected |
| ActionDate | DATETIME | DEFAULT GETDATE() | Auto-populated |
| OldValue | VARCHAR(MAX) | Nullable | Large text accepted |
| NewValue | VARCHAR(MAX) | Nullable | Large text accepted |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TableName NOT NULL | NULL | Error |
| 2 | Action values | INSERT, UPDATE, DELETE, LOGIN | All accepted |
| 3 | Large OldValue/NewValue | MAX length text | Stored correctly |
| 4 | ActionDate auto-set | No date provided | GETDATE() used |

---

### Test Case ID: DV-ADM-009
**Table**: Audit.ErrorLog
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| ErrorID | BIGINT IDENTITY | PK | Auto-increment |
| ErrorDate | DATETIME | DEFAULT GETDATE() | Auto-populated |
| ErrorMessage | VARCHAR(MAX) | Nullable | Large messages accepted |
| ErrorProcedure | VARCHAR(200) | Nullable | NULL accepted |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | ErrorDate auto-set | No date | GETDATE() used |
| 2 | Large ErrorMessage | MAX length | Stored correctly |
| 3 | All fields nullable except ErrorID/ErrorDate | NULLs for optional fields | Accepted |

---

### Test Case ID: DV-ADM-010
**Table**: Batch.JobLog
**Source**: `database/01-schema/002-admin-tables.sql`

#### Column Constraints
| Column | Type | Constraint | Test |
|--------|------|-----------|------|
| JobLogID | BIGINT IDENTITY | PK | Auto-increment |
| JobName | VARCHAR(100) | NOT NULL | NULL rejected |
| StartTime | DATETIME | NOT NULL | NULL rejected |
| Status | VARCHAR(20) | DEFAULT 'RUNNING' | Defaults to RUNNING |
| RecordsProcessed | INT | DEFAULT 0 | Defaults to 0 |
| RecordsFailed | INT | DEFAULT 0 | Defaults to 0 |

#### Data Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | JobName NOT NULL | NULL | Error |
| 2 | StartTime NOT NULL | NULL | Error |
| 3 | Status default | No status provided | 'RUNNING' |
| 4 | Valid status values | RUNNING, COMPLETED, FAILED, CANCELLED | All accepted |
