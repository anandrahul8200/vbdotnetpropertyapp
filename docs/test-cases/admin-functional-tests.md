# Admin Module - Functional Tests

## Module: ADM (Admin)
## Test Type: End-to-End Functional Tests
## Source Files:
- `src/PropertyInsuranceClaims/Forms/frmLogin.vb`
- `src/PropertyInsuranceClaims/Forms/frmMain.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmUserManagement.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmRolePermissions.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmPasswordReset.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmSystemConfig.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmLookupMaintenance.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmAuditViewer.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmErrorLogViewer.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`
- `src/PropertyInsuranceClaims/Forms/Admin/frmDashboardMain.vb`
- `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

---

### Test Case ID: FT-ADM-001
**Feature**: User Login
**Priority**: Critical

#### Preconditions
- Application is running
- Valid user exists in Admin.Users (IsActive=1, IsLocked=0)
- User has a role and permissions assigned

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Launch application | frmLogin displayed with Username, Password fields, Login button, Exit button |
| 2 | Enter valid username and password | Fields accept input, password masked with bullet character |
| 3 | Click Login | Cursor changes to WaitCursor, authentication call made |
| 4 | Authentication succeeds | GlobalState.SetSession called with UserID, Username, FullName, Role, Permissions |
| 5 | frmMain opens | MDI parent form shown, title = AppSettings.ApplicationName + CurrentUserFullName |
| 6 | Dashboard auto-opens | frmDashboardMain displayed as MDI child, maximized |
| 7 | Status bar shows user info | User, Role, and Date shown in status strip |

#### Validation Rules
- Username and Password cannot be empty (error: "Please enter username and password.")
- Failed login shows "Invalid username or password." and clears password field
- Errors during login show "Login failed. Please try again." and log via ErrorLogger

---

### Test Case ID: FT-ADM-002
**Feature**: Failed Login and Account Lockout
**Priority**: Critical

#### Preconditions
- Valid user exists with FailedLoginAttempts < 5

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter valid username, wrong password | "Invalid username or password." shown, password cleared |
| 2 | Repeat 4 more times (total 5 failures) | Account locked in database (IsLocked=1, LockedDate set) |
| 3 | Attempt login with correct password | Authentication fails (user is locked) |

#### Business Rules
- Max login attempts: 5 (AppSettings.MaxLoginAttempts)
- Lock triggers when FailedLoginAttempts >= 4 on a failed attempt (SP logic)
- Successful login resets FailedLoginAttempts to 0

---

### Test Case ID: FT-ADM-003
**Feature**: User Management
**Priority**: High

#### Preconditions
- Logged in as Administrator with Manage Users permission
- frmUserManagement open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Admin > User Management | frmUserManagement loads, DataGridView populated via usp_User_List |
| 2 | Click "New" button | New user dialog indicated (placeholder in current implementation) |
| 3 | Click "Edit" with row selected | Edit user dialog indicated |
| 4 | Click "Lock/Unlock" with row selected | User lock/unlock toggled, grid refreshed |
| 5 | Click "Reset Password" with row selected | Confirmation dialog shown, password reset on Yes |
| 6 | Click "Refresh" | Grid reloaded from usp_User_List |

---

### Test Case ID: FT-ADM-004
**Feature**: Password Reset
**Priority**: High

#### Preconditions
- Logged in as Administrator
- frmPasswordReset open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open password reset form | Fields: Username, New Password, Confirm Password, Force Change checkbox (checked by default) |
| 2 | Leave username blank, click Reset | "Username required." validation message |
| 3 | Enter passwords that don't match | "Passwords don't match." validation message |
| 4 | Enter password less than 8 chars | "Password must be at least 8 characters." validation message |
| 5 | Enter valid data and click Reset | "Password reset for: {username}" success message, form closes |

---

### Test Case ID: FT-ADM-005
**Feature**: Role and Permission Management
**Priority**: High

#### Preconditions
- Logged in as Administrator
- frmRolePermissions open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Role Permissions form | Role dropdown loaded with: Administrator, Underwriter, Claims Adjuster, Billing Clerk, Agent, Read Only |
| 2 | Select a role | Permissions grid shows CheckBox (Granted), Module, Permission columns |
| 3 | Toggle permission checkboxes | Checkboxes togglable for each permission row |
| 4 | Click Save | "Permissions saved for role: {rolename}" confirmation |

---

### Test Case ID: FT-ADM-006
**Feature**: System Configuration Management
**Priority**: High

#### Preconditions
- Logged in as Administrator
- frmSystemConfig open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open System Config form | Category dropdown with (All), GENERAL, SECURITY, BILLING, CLAIMS, UNDERWRITING, BATCH |
| 2 | Select category | Grid filtered by category via usp_Config_Get |
| 3 | Edit ConfigValue cell | Only ConfigValue column is editable |
| 4 | Click Save | Each row saved via usp_Config_Set, status label shows "Saved {count} settings at {time}" |
| 5 | Click Refresh | Grid reloaded from database |

#### Error Handling
- Save errors show message box and log via ErrorLogger

---

### Test Case ID: FT-ADM-007
**Feature**: Lookup Maintenance
**Priority**: Medium

#### Preconditions
- Logged in with appropriate permissions
- frmLookupMaintenance open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Lookup Maintenance | Category dropdown loaded: CLAIM_TYPE, POLICY_TYPE, CONSTRUCTION_TYPE, ROOF_TYPE, OCCUPANCY_TYPE, PROPERTY_TYPE, PAYMENT_METHOD, VENDOR_TYPE, WEATHER_CONDITION, FLOOD_ZONE |
| 2 | Select category | Grid populated via usp_Lookup_GetByCategory |
| 3 | Click Add | Input box prompts for code (converted to uppercase, trimmed) then value |
| 4 | Enter code and value | New lookup created via usp_Lookup_Create, grid refreshed |
| 5 | Click Refresh | Grid reloaded for current category |

#### Validation Rules
- Empty code or value cancels the add operation
- Duplicate code shows error message from SP

---

### Test Case ID: FT-ADM-008
**Feature**: Audit Trail Viewer
**Priority**: Medium

#### Preconditions
- Logged in with View Audit permission
- frmAuditViewer open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Audit Viewer | Filter panel: Table, User, Action dropdown (All/INSERT/UPDATE/DELETE/LOGIN), From date (default -7 days), To date |
| 2 | Click Search with defaults | Grid populated via usp_Audit_Search, record count label updated |
| 3 | Filter by Table | Only matching table entries returned |
| 4 | Filter by User | Only matching username entries returned |
| 5 | Filter by Action | Only matching action type entries returned |
| 6 | Filter by date range | Only entries within range returned |

#### Implementation Notes
- PageSize hardcoded to 500 in UI
- DateTo has 1 day added (AddDays(1)) to include full end day
- Empty filter fields passed as Nothing (NULL to SP)
- TotalRecords output displayed in label

---

### Test Case ID: FT-ADM-009
**Feature**: Error Log Viewer
**Priority**: Medium

#### Preconditions
- Logged in with appropriate permissions
- frmErrorLogViewer open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Error Log Viewer | Filter panel: Procedure text, Since date (default -7 days) |
| 2 | Click Search | Grid populated via AdminDataAccess.SearchErrorLog, count label updated |
| 3 | Filter by procedure name | LIKE filter applied ('%procedure%') |
| 4 | Click Clear | Procedure field cleared, grid cleared, count label cleared |

---

### Test Case ID: FT-ADM-010
**Feature**: Batch Job Monitor
**Priority**: Medium

#### Preconditions
- Logged in with appropriate permissions
- frmBatchJobMonitor open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open Batch Job Monitor | Filter panel: Job dropdown (All + 6 job names), Status (All/COMPLETED/FAILED/RUNNING), Since date |
| 2 | Click Search | Job count label updated |
| 3 | Select row and click View Log | Message box indicating log display |

---

### Test Case ID: FT-ADM-011
**Feature**: Dashboard (Main Landing Page)
**Priority**: High

#### Preconditions
- User logged in successfully
- frmDashboardMain displayed

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Dashboard loads after login | Welcome message shows "Welcome, {GlobalState.CurrentUserFullName}" |
| 2 | KPI cards display | 4 cards: Open Claims, Active Policies, My Tasks, Overdue |
| 3 | Quick action buttons present | New Quote, New FNOL, Customer Search, Claim Search, My Tasks, Reports |
| 4 | Click New Quote | frmPolicyEntry opens as dialog |
| 5 | Click New FNOL | frmClaimFNOL opens as dialog |
| 6 | Click Customer Search | frmCustomerSearch opens as dialog |
| 7 | Click Claim Search | frmClaimSearch opens as dialog |

---

### Test Case ID: FT-ADM-012
**Feature**: Global Search
**Priority**: High

#### Preconditions
- User logged in
- Global search available

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Enter search term | Term passed to usp_Search_Global |
| 2 | Results returned | UNION ALL of Customer, Policy, Claim, Property matches |
| 3 | Each result shows EntityType, EntityID, EntityNumber, DisplayName, Category | Columns populated per entity type |
| 4 | No matches | Empty result set (no error) |

---

### Test Case ID: FT-ADM-013
**Feature**: Session Timeout
**Priority**: High

#### Preconditions
- User logged in
- SessionTimeoutMinutes = 30 (AppSettings)

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Login sets LoginTime | GlobalState.LoginTime = DateTime.Now |
| 2 | Check session after 29 minutes | GlobalState.IsSessionExpired() = False |
| 3 | Check session after 31 minutes | GlobalState.IsSessionExpired() = True |
| 4 | Session expired action | User should be redirected to login [ASSUMPTION] |

---

### Test Case ID: FT-ADM-014
**Feature**: Application Exit
**Priority**: Medium

#### Preconditions
- User logged in, frmMain open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Close frmMain (X button or Alt+F4) | Confirmation dialog: "Are you sure you want to exit?" |
| 2 | Click No | Close cancelled, application remains open |
| 3 | Click Yes | Application closes |
