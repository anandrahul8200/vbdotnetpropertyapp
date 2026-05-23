# Admin Module - User Stories

## Module: ADM (Admin)
## Test Type: User Story Acceptance Criteria

---

### User Story ID: US-ADM-001
**Title**: User Login
**As a** system user
**I want to** log in with my username and password
**So that** I can access the application with my assigned permissions

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Login form displays username and password fields | UI shows TextBox controls |
| 2 | Password is masked during entry | PasswordChar = bullet character |
| 3 | Successful login opens the main application | frmMain displayed with dashboard |
| 4 | My permissions are loaded from the database | GlobalState.Permissions populated via usp_User_GetPermissions |
| 5 | Status bar shows my username and role | lblStatusUser and lblStatusRole updated |
| 6 | Failed login shows error without revealing which field is wrong | Generic "Invalid username or password." message |
| 7 | I can exit the application from the login screen | Exit button closes application |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-001 | User Login |
| FT-ADM-002 | Failed Login and Account Lockout |
| UI-ADM-001 | frmLogin UI Tests |
| SP-ADM-001 | usp_User_Authenticate |

---

### User Story ID: US-ADM-002
**Title**: Account Lockout Protection
**As a** system administrator
**I want** accounts to be locked after too many failed login attempts
**So that** brute force attacks are prevented

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Account locks after 5 consecutive failed attempts | AppSettings.MaxLoginAttempts = 5 |
| 2 | Locked accounts cannot log in even with correct credentials | usp_User_Authenticate checks IsLocked=0 |
| 3 | Failed attempt counter resets on successful login | FailedLoginAttempts set to 0 on success |
| 4 | Admin can unlock locked accounts | usp_User_Unlock resets IsLocked and FailedLoginAttempts |
| 5 | Lock event is timestamped | LockedDate = GETDATE() when locked |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-002 | Failed Login and Account Lockout |
| SP-ADM-001 | usp_User_Authenticate (lockout logic) |
| SP-ADM-006 | usp_User_Unlock |
| BR-ADM-002 | Account Lockout Rule |

---

### User Story ID: US-ADM-003
**Title**: User Management
**As a** system administrator
**I want to** create, edit, lock, unlock, and manage user accounts
**So that** I can control who has access to the system

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can view a list of all users with their status | frmUserManagement loads via usp_User_List |
| 2 | I can create new users with a role assignment | usp_User_Create with role |
| 3 | I can lock/unlock user accounts | usp_User_Lock / usp_User_Unlock |
| 4 | I can reset a user's password | usp_User_ResetPassword |
| 5 | User creation validates username uniqueness | SP raises error on duplicate |
| 6 | All user changes are audited | Audit.AuditLog entries created |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-003 | User Management |
| UI-ADM-003 | frmUserManagement |
| SP-ADM-002 | usp_User_Create |
| SP-ADM-004 | usp_User_List |
| SP-ADM-005 | usp_User_Lock |

---

### User Story ID: US-ADM-004
**Title**: Password Reset
**As a** system administrator
**I want to** reset a user's password
**So that** locked-out users can regain access

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can specify a new password for any user | frmPasswordReset with username field |
| 2 | New password must be confirmed (entered twice) | Passwords must match validation |
| 3 | New password must meet complexity requirements | Min 8 chars, 1 upper, 1 lower, 1 digit |
| 4 | I can force password change on next login | chkForceChange checkbox (default checked) |
| 5 | Password is stored as salted SHA-256 hash | SecurityHelper.HashPassword used |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-004 | Password Reset |
| UI-ADM-005 | frmPasswordReset |
| SP-ADM-007 | usp_User_ResetPassword |
| BR-ADM-001 | Password Complexity Rule |

---

### User Story ID: US-ADM-005
**Title**: Role and Permission Management
**As a** system administrator
**I want to** assign permissions to roles
**So that** I can control what each role can do in the system

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can view all available roles | Role dropdown loaded |
| 2 | I can see all permissions for a selected role | Permission grid with checkboxes |
| 3 | I can grant/revoke permissions per role | Toggle checkboxes and save |
| 4 | Permission checks are case-insensitive | GlobalState.HasPermission uses ToUpper() |
| 5 | Changes take effect on next login | Permissions reloaded at login |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-005 | Role and Permission Management |
| UI-ADM-004 | frmRolePermissions |
| SP-ADM-021 | usp_Role_GetAll |
| SP-ADM-022 | usp_RolePermission_GetByRole |

---

### User Story ID: US-ADM-006
**Title**: System Configuration
**As a** system administrator
**I want to** view and modify system configuration settings
**So that** I can adjust application behavior without code changes

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can view all configuration settings by category | Category filter with grid |
| 2 | I can edit configuration values inline | Only ConfigValue column editable |
| 3 | Changes are audited with old and new values | Audit log records OldValue/NewValue |
| 4 | Non-existent keys cannot be created via this form | usp_Config_Set only updates existing keys |
| 5 | I receive confirmation after saving | Status label shows save count and time |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-006 | System Configuration Management |
| UI-ADM-006 | frmSystemConfig |
| SP-ADM-008 | usp_Config_Get |
| SP-ADM-009 | usp_Config_Set |

---

### User Story ID: US-ADM-007
**Title**: Lookup Value Maintenance
**As a** system administrator
**I want to** manage lookup/reference data used in dropdown fields
**So that** I can add or modify options without database scripts

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can view lookup values by category | Category dropdown with grid |
| 2 | I can add new lookup values | Add button with input prompts |
| 3 | Lookup codes are stored uppercase and trimmed | Code converted via ToUpper().Trim() |
| 4 | Duplicate codes within a category are rejected | SP raises error |
| 5 | New categories are auto-created if needed | usp_Lookup_Create creates category if not found |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-007 | Lookup Maintenance |
| UI-ADM-007 | frmLookupMaintenance |
| SP-ADM-010 | usp_Lookup_Create |
| SP-ADM-012 | usp_Lookup_GetByCategory |

---

### User Story ID: US-ADM-008
**Title**: Audit Trail Viewing
**As a** compliance officer
**I want to** search and view the audit trail of all system changes
**So that** I can verify regulatory compliance and investigate issues

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can search by table, user, action, and date range | Filter controls available |
| 2 | Results show who changed what, when | ActionDate, Username, TableName, Action columns |
| 3 | Results are paginated for performance | PageSize=500 in UI, SP supports pagination |
| 4 | Default shows last 7 days | DateFrom default = Now.AddDays(-7) |
| 5 | Total record count displayed | @TotalRecords output shown in label |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-008 | Audit Trail Viewer |
| UI-ADM-008 | frmAuditViewer |
| SP-ADM-018 | usp_Audit_Search |

---

### User Story ID: US-ADM-009
**Title**: Error Log Viewing
**As a** system administrator
**I want to** view application errors logged in the database
**So that** I can diagnose and resolve technical issues

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can filter errors by procedure name | LIKE filter on ErrorProcedure |
| 2 | I can filter by date | DateFrom filter |
| 3 | Default shows last 7 days of errors | dtpDateFrom = Today.AddDays(-7) |
| 4 | Error count is displayed | Label shows row count |
| 5 | I can clear filters and results | Clear button resets all |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-009 | Error Log Viewer |
| UI-ADM-009 | frmErrorLogViewer |
| SP-ADM-019 | usp_ErrorLog_Search |

---

### User Story ID: US-ADM-010
**Title**: Session Timeout
**As a** security officer
**I want** inactive sessions to expire automatically
**So that** unattended workstations do not remain accessible

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Sessions expire after 30 minutes of inactivity | AppSettings.SessionTimeoutMinutes = 30 |
| 2 | Expired session detected via IsSessionExpired() | Checks (Now - LoginTime).TotalMinutes > 30 |
| 3 | Expired session requires re-login | User redirected to login [ASSUMPTION] |
| 4 | Session state cleared on timeout | GlobalState.ClearSession() called [ASSUMPTION] |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-ADM-013 | Session Timeout |
| UT-ADM-031 | GlobalState.IsSessionExpired() |
| BR-ADM-003 | Session Timeout Rule |
