# Admin Module - UI Tests

## Module: ADM (Admin)
## Test Type: Screen-Level UI Tests
## Forms Covered:
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

---

### Test Case ID: UI-ADM-001
**Form**: frmLogin
**File**: `src/PropertyInsuranceClaims/Forms/frmLogin.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | AppSettings.ApplicationName & " - Login" |
| Size | 400 x 250 |
| StartPosition | CenterScreen |
| FormBorderStyle | FixedDialog |
| MaximizeBox | False |
| MinimizeBox | False |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| lblTitle | Label | Font=Segoe UI 12pt Bold, Text=ApplicationName |
| txtUsername | TextBox | Size=220x20, gets focus on load |
| txtPassword | TextBox | Size=220x20, PasswordChar=bullet |
| lblError | Label | ForeColor=Red, Visible=False initially |
| btnLogin | Button | Text="&Login", Size=100x35, AcceptButton |
| btnCancel | Button | Text="&Exit", Size=100x35, CancelButton |
| lblVersion | Label | ForeColor=Gray, Text="Version {ApplicationVersion}" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads correctly | Open app | All controls visible, proper layout |
| 2 | Password masking | Type in password field | Characters shown as bullets |
| 3 | Enter key triggers login | Press Enter | btnLogin_Click fires (AcceptButton) |
| 4 | Escape key exits | Press Escape | btnCancel_Click fires (CancelButton) |
| 5 | Error label appears on failure | Failed login | lblError visible with red text |
| 6 | Wait cursor during auth | Click Login | Cursor=WaitCursor then Default |
| 7 | Username gets focus on load | Form loads | txtUsername has input focus |
| 8 | Password cleared on failure | Failed login | txtPassword.Text = "", focus on txtPassword |
| 9 | Version label shows | Form loads | "Version 1.0.0" (default from AppSettings) |

---

### Test Case ID: UI-ADM-002
**Form**: frmMain
**File**: `src/PropertyInsuranceClaims/Forms/frmMain.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "{ApplicationName} - {CurrentUserFullName}" |
| Size | 1280 x 800 |
| StartPosition | CenterScreen |
| IsMdiContainer | True |
| WindowState | Maximized |

#### Menu Structure
| Menu | Items |
|------|-------|
| &Policy | &Customer Search, &New Customer, separator, &Policy Search, New &Quote |
| &Claims | Claim &Search, New &FNOL |
| &Underwriting | &Referral Queue, &Moratoriums, &Rate Tables |
| &Billing | Billing &Inquiry, Record &Payment, &Commission Statement |
| &Admin | &User Management, &System Config, &Audit Viewer, &Lookup Maintenance |
| &Help | &About |

#### Status Bar
| Label | Content |
|-------|---------|
| lblStatusUser | "User: {GlobalState.CurrentUser}" |
| lblStatusRole | "Role: {GlobalState.CurrentRole}" |
| lblStatusDate | DateTime.Now formatted as "yyyy-MM-dd HH:mm" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | MDI child singleton | Open same form twice | Existing form activated, not duplicated |
| 2 | Dashboard auto-opens | Login completes | frmDashboardMain shown as maximized MDI child |
| 3 | Close confirmation | Click X | "Are you sure you want to exit?" Yes/No dialog |
| 4 | Cancel close | Click No on exit dialog | Application remains open |
| 5 | Admin menu items | Click Admin menu | 4 items: User Management, System Config, Audit Viewer, Lookup Maintenance |
| 6 | About dialog | Help > About | Message box with app name, version, company |

---

### Test Case ID: UI-ADM-003
**Form**: frmUserManagement
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmUserManagement.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "User Management" |
| Size | 800 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| btnNew | Button | Text="&New", Size=80x30 |
| btnEdit | Button | Text="&Edit", Size=80x30 |
| btnLock | Button | Text="&Lock/Unlock", Size=100x30 |
| btnResetPassword | Button | Text="Reset &Password", Size=120x30 |
| btnRefresh | Button | Text="Re&fresh", Size=80x30 |
| dgvUsers | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Grid loads on form load | Open form | dgvUsers populated with user data |
| 2 | No row selected actions | Click Edit with no selection | No action (CurrentRow=Nothing check) |
| 3 | Refresh reloads data | Click Refresh | LoadUsers() called, grid updated |
| 4 | Reset password confirmation | Click Reset Password | Yes/No confirmation dialog |

---

### Test Case ID: UI-ADM-004
**Form**: frmRolePermissions
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmRolePermissions.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Role Permissions" |
| Size | 600 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboRole | ComboBox | DropDownStyle=DropDownList, Items: Administrator/Underwriter/Claims Adjuster/Billing Clerk/Agent/Read Only |
| dgvPermissions | DataGridView | AllowUserToAddRows=False, Columns: Granted(CheckBox)/Module(ReadOnly)/Permission(ReadOnly) |
| btnSave | Button | Text="&Save" |
| btnClose | Button | Text="&Close" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Default role selected | Form loads | SelectedIndex=0 (Administrator) |
| 2 | Permission checkboxes | Click checkbox | Toggles grant state |
| 3 | Save confirmation | Click Save | "Permissions saved for role: {role}" message |
| 4 | Close form | Click Close | Form closes |

---

### Test Case ID: UI-ADM-005
**Form**: frmPasswordReset
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmPasswordReset.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Reset User Password" |
| Size | 400 x 250 |
| StartPosition | CenterParent |
| FormBorderStyle | FixedDialog |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| txtUsername | TextBox | Size=200x20 |
| txtNewPassword | TextBox | Size=200x20, PasswordChar="*" |
| txtConfirmPassword | TextBox | Size=200x20, PasswordChar="*" |
| chkForceChange | CheckBox | Text="Force password change on next login", Checked=True |
| btnReset | Button | Text="&Reset", Size=90x30 |
| btnCancel | Button | Text="&Cancel", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Force change checked by default | Form loads | chkForceChange.Checked = True |
| 2 | Password fields masked | Type passwords | Characters shown as asterisks |
| 3 | Validation messages | Leave fields blank | Appropriate warning MessageBox |
| 4 | Form closes on cancel | Click Cancel | Form closes |
| 5 | Form closes on success | Valid reset | Form closes after success message |

---

### Test Case ID: UI-ADM-006
**Form**: frmSystemConfig
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmSystemConfig.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "System Configuration" |
| Size | 800 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboCategory | ComboBox | DropDownStyle=DropDownList, Items: (All)/GENERAL/SECURITY/BILLING/CLAIMS/UNDERWRITING/BATCH |
| dgvConfig | DataGridView | AllowUserToAddRows=False, AllowUserToDeleteRows=False, Only ConfigValue editable |
| btnSave | Button | Text="&Save" |
| btnRefresh | Button | Text="&Refresh" |
| lblStatus | Label | ForeColor=Green, shows save timestamp |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Config loads on form load | Open form | Grid populated with config values |
| 2 | Only ConfigValue is editable | Try editing other columns | Other columns are ReadOnly |
| 3 | Category filter triggers reload | Change category | LoadConfig() called |
| 4 | Save status shows | Click Save | Green label: "Saved {n} settings at HH:mm:ss" |
| 5 | Wait cursor during save | Click Save | Cursor=WaitCursor then Default |

---

### Test Case ID: UI-ADM-007
**Form**: frmLookupMaintenance
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmLookupMaintenance.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Lookup Maintenance" |
| Size | 700 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboCategory | ComboBox | DropDownStyle=DropDownList, 10 categories loaded |
| dgvLookups | DataGridView | AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| btnAdd | Button | Text="&Add" |
| btnSave | Button | Text="&Save" |
| btnRefresh | Button | Text="&Refresh" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Categories loaded on form load | Open form | 10 categories in dropdown, first selected |
| 2 | Grid updates on category change | Select different category | Grid refreshed for new category |
| 3 | Add prompts for input | Click Add | InputBox for code, then InputBox for value |
| 4 | Add cancelled on empty input | Leave InputBox blank | No action taken |
| 5 | Duplicate code error | Add existing code | Error MessageBox from SP |

---

### Test Case ID: UI-ADM-008
**Form**: frmAuditViewer
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmAuditViewer.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Audit Trail Viewer" |
| Size | 1000 x 600 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| txtTableName | TextBox | Size=130x20 |
| txtUsername | TextBox | Size=100x20 |
| cboAction | ComboBox | DropDownStyle=DropDownList, Items: (All)/INSERT/UPDATE/DELETE/LOGIN |
| dtpDateFrom | DateTimePicker | Format=Short, Value=Now.AddDays(-7) |
| dtpDateTo | DateTimePicker | Format=Short |
| btnSearch | Button | Text="&Search" |
| dgvAudit | DataGridView | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect |
| lblRecordCount | Label | Shows "{n} record(s)" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Default date range | Form loads | From = 7 days ago, To = today |
| 2 | Action dropdown default | Form loads | SelectedIndex=0 ("(All)") |
| 3 | Record count updated | Search completes | Label shows total from output param |
| 4 | Wait cursor during search | Click Search | Cursor=WaitCursor then Default |
| 5 | Error handling | Database error | Error MessageBox, logged via ErrorLogger |

---

### Test Case ID: UI-ADM-009
**Form**: frmErrorLogViewer
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmErrorLogViewer.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Error Log Viewer" |
| Size | 900 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| txtProcedure | TextBox | Size=150x20 |
| dtpDateFrom | DateTimePicker | Format=Short, Value=Today.AddDays(-7) |
| btnSearch | Button | Text="&Search" |
| btnClear | Button | Text="&Clear" |
| btnClose | Button | Text="&Close" |
| dgvErrors | DataGridView | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect |
| lblCount | Label | Shows "{n} error(s)" |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Search populates grid | Click Search | Grid loaded via AdminDataAccess.SearchErrorLog |
| 2 | Count label | Search completes | "{n} error(s)" from dt.Rows.Count |
| 3 | Clear resets all | Click Clear | txtProcedure cleared, grid=Nothing, label="" |
| 4 | Close button | Click Close | Form closes |

---

### Test Case ID: UI-ADM-010
**Form**: frmBatchJobMonitor
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Batch Job Monitor" |
| Size | 900 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboJobName | ComboBox | DropDownStyle=DropDownList, Items: (All)/RenewalProcessor/ExpirationProcessor/FraudScoring/PaymentBatch/ReserveRecalculator/ReinsuranceAllocator |
| cboStatus | ComboBox | DropDownStyle=DropDownList, Items: (All)/COMPLETED/FAILED/RUNNING |
| dtpDateFrom | DateTimePicker | Format=Short, Value=Today.AddDays(-7) |
| btnSearch | Button | Text="&Search" |
| btnViewLog | Button | Text="&View Log" |
| btnClose | Button | Text="&Close" |
| dgvJobHistory | DataGridView | ReadOnly=True, AllowUserToAddRows=False, FullRowSelect |
| lblCount | Label | Shows job run count |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Default filter values | Form loads | Job=(All), Status=(All), Since=7 days ago |
| 2 | View Log requires selection | Click View Log without row | No action (CurrentRow=Nothing check) |
| 3 | View Log with selection | Select row, click View Log | Message box with log info |
| 4 | Close button | Click Close | Form closes |

---

### Test Case ID: UI-ADM-011
**Form**: frmDashboardMain
**File**: `src/PropertyInsuranceClaims/Forms/Admin/frmDashboardMain.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Dashboard" |
| Size | 800 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| lblWelcome | Label | Font=Segoe UI 16pt Bold, Text="Welcome, {FullName}" |
| lblLastLogin | Label | ForeColor=Gray, shows last login time |
| KPI Panel - Open Claims | Panel | BorderStyle=FixedSingle, accent=SteelBlue |
| KPI Panel - Active Policies | Panel | BorderStyle=FixedSingle, accent=ForestGreen |
| KPI Panel - My Tasks | Panel | BorderStyle=FixedSingle, accent=DarkOrange |
| KPI Panel - Overdue | Panel | BorderStyle=FixedSingle, accent=Crimson |
| btnNewQuote | Button | Text="New Quote", Size=150x40 |
| btnNewClaim | Button | Text="New FNOL", Size=150x40 |
| btnCustomerSearch | Button | Text="Customer Search", Size=150x40 |
| btnClaimSearch | Button | Text="Claim Search", Size=150x40 |
| btnMyTasks | Button | Text="My Tasks", Size=150x40 |
| btnReports | Button | Text="Reports", Size=150x40 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Welcome message | Form loads | "Welcome, {GlobalState.CurrentUserFullName}" |
| 2 | KPI cards display | Form loads | 4 colored cards with values |
| 3 | Quick actions section | Form loads | "Quick Actions" header with 6 buttons |
| 4 | Banner image (optional) | File exists | PictureBox displayed with stretched image |
| 5 | Banner image missing | File missing | No error, banner not shown |
