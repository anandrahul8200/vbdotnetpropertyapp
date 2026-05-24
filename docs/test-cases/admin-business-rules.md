# Admin Module - Business Rules

## Module: ADM (Admin)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-ADM-001
**Module**: ADM
**Priority**: Critical

#### Rule Description
Password complexity requirements: passwords must be at least 8 characters long (configurable via AppSettings.PasswordMinLength, default 8), contain at least one uppercase letter, at least one lowercase letter, and at least one digit.

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`
- **Method**: ValidatePasswordComplexity
- **Code Snippet**:
```vb
If password.Length < AppSettings.PasswordMinLength Then
    errorMessage = $"Password must be at least {AppSettings.PasswordMinLength} characters."
    Return False
End If
If Not password.Any(Function(c) Char.IsUpper(c)) Then
    errorMessage = "Password must contain at least one uppercase letter."
    Return False
End If
If Not password.Any(Function(c) Char.IsLower(c)) Then
    errorMessage = "Password must contain at least one lowercase letter."
    Return False
End If
If Not password.Any(Function(c) Char.IsDigit(c)) Then
    errorMessage = "Password must contain at least one digit."
    Return False
End If
```

#### Enforcement Mechanism
- Type: Application-level validation (SecurityHelper class)
- Behavior: Returns False with specific error message for first failed check

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-010 | SecurityHelper.ValidatePasswordComplexity |
| FT-ADM-004 | Password Reset validation |
| US-ADM-004 | Password Reset user story |

---

### Rule ID: BR-ADM-002
**Module**: ADM
**Priority**: Critical

#### Rule Description
Account lockout after maximum failed login attempts: after 5 consecutive failed login attempts (configurable via AppSettings.MaxLoginAttempts, default 5), the user account is automatically locked. The lock is triggered when FailedLoginAttempts >= 4 on a failed attempt (meaning the 5th failure triggers the lock). Successful login resets the counter to 0.

#### Source
- **File**: `database/02-stored-procedures/011-admin-utility-sps.sql`
- **SP**: Admin.usp_User_Authenticate
- **Code Snippet**:
```sql
-- Increment failed attempts
UPDATE Admin.Users SET
    FailedLoginAttempts = FailedLoginAttempts + 1,
    IsLocked = CASE WHEN FailedLoginAttempts >= 4 THEN 1 ELSE IsLocked END,
    LockedDate = CASE WHEN FailedLoginAttempts >= 4 THEN GETDATE() ELSE LockedDate END
WHERE Username = @Username AND IsActive = 1;
```

#### Configuration
- **Setting**: AppSettings.MaxLoginAttempts
- **Default Value**: 5
- **Source**: `src/PropertyInsuranceClaims.Common/AppSettings.vb`

#### Enforcement Mechanism
- Type: Database SP logic (CASE expression on UPDATE)
- Behavior: IsLocked=1 and LockedDate set when threshold reached
- Note: The check is FailedLoginAttempts >= 4 BEFORE increment, so the 5th attempt triggers lockout

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-ADM-001 | usp_User_Authenticate lockout tests |
| FT-ADM-002 | Failed Login and Account Lockout |
| US-ADM-002 | Account Lockout Protection |
| NFR-ADM-004 | Security - Authentication |

---

### Rule ID: BR-ADM-003
**Module**: ADM
**Priority**: High

#### Rule Description
Session timeout after 30 minutes of inactivity. The session is considered expired when the elapsed time since login exceeds AppSettings.SessionTimeoutMinutes (default 30). The check uses a strict greater-than comparison (TotalMinutes > 30).

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`
- **Method**: IsSessionExpired
- **Code Snippet**:
```vb
Public Shared Function IsSessionExpired() As Boolean
    If LoginTime = DateTime.MinValue Then Return True
    Return (DateTime.Now - LoginTime).TotalMinutes > AppSettings.SessionTimeoutMinutes
End Function
```

#### Configuration
- **Setting**: AppSettings.SessionTimeoutMinutes
- **Default Value**: 30
- **Source**: `src/PropertyInsuranceClaims.Common/AppSettings.vb`

#### Enforcement Mechanism
- Type: Application-level check (GlobalState class)
- Behavior: Returns True if no login or if timeout exceeded
- Note: LoginTime = DateTime.MinValue also returns True (never logged in or cleared session)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-031 | GlobalState.IsSessionExpired() |
| FT-ADM-013 | Session Timeout |
| US-ADM-010 | Session Timeout user story |

---

### Rule ID: BR-ADM-004
**Module**: ADM
**Priority**: Critical

#### Rule Description
Password hashing uses SHA-256 with a unique random salt per user. The salt is generated using RNGCryptoServiceProvider (16 bytes, base64 encoded). The stored format is "salt:base64hash" where the salt and hash are separated by a colon. Password verification extracts the salt from the stored hash and re-hashes the input for comparison using ordinal (case-sensitive, culture-invariant) string comparison.

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`
- **Methods**: HashPassword, VerifyPassword, GenerateSalt
- **Code Snippet**:
```vb
Public Shared Function HashPassword(password As String, Optional salt As String = Nothing) As String
    If String.IsNullOrEmpty(salt) Then salt = GenerateSalt()
    Using sha256 As SHA256 = SHA256.Create()
        Dim combined As String = salt & password
        Dim bytes As Byte() = Encoding.UTF8.GetBytes(combined)
        Dim hash As Byte() = sha256.ComputeHash(bytes)
        Return salt & ":" & Convert.ToBase64String(hash)
    End Using
End Function
```

#### Enforcement Mechanism
- Type: Application-level cryptography (SecurityHelper class)
- Storage: Admin.Users.PasswordHash (VARCHAR 256) and Admin.Users.Salt (VARCHAR 128)
- Comparison: String.Equals with StringComparison.Ordinal

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-007 | SecurityHelper.HashPassword |
| UT-ADM-008 | SecurityHelper.VerifyPassword |
| UT-ADM-009 | SecurityHelper.GenerateSalt |
| NFR-ADM-005 | Password Security |

---

### Rule ID: BR-ADM-005
**Module**: ADM
**Priority**: High

#### Rule Description
Permission-based access control using case-insensitive permission codes. Permissions are loaded at login from Admin.Permissions via Admin.RolePermissions and Admin.UserRoles. The HasPermission check converts the input to uppercase before checking the Permissions list (which is also stored uppercase).

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`
- **Method**: HasPermission
- **Code Snippet**:
```vb
Public Shared Function HasPermission(permissionCode As String) As Boolean
    Return Permissions.Contains(permissionCode.ToUpper())
End Function
```

#### Loading Mechanism
- **File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`
- **Code**:
```vb
For Each row As DataRow In dt.Rows
    permissions.Add(row("PermissionCode").ToString().ToUpper())
Next
```

#### Enforcement Mechanism
- Type: Application-level check (GlobalState)
- Behavior: Returns True only if exact uppercase match found in loaded permissions
- Note: Permissions loaded once at login, not refreshed until re-login

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-028 | GlobalState.HasPermission |
| SP-ADM-003 | usp_User_GetPermissions |
| US-ADM-005 | Role and Permission Management |

---

### Rule ID: BR-ADM-006
**Module**: ADM
**Priority**: High

#### Rule Description
Input sanitization as defense-in-depth against SQL injection. Single quotes are doubled (escaped), double-dashes (SQL comment markers) are removed, semicolons (statement terminators) are removed, and input is trimmed. This is a secondary defense since all database calls use parameterized stored procedures via DatabaseHelper.

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`
- **Method**: SanitizeInput
- **Code Snippet**:
```vb
Public Shared Function SanitizeInput(input As String) As String
    If String.IsNullOrEmpty(input) Then Return input
    Return input.Replace("'", "''").Replace("--", "").Replace(";", "").Trim()
End Function
```

#### Enforcement Mechanism
- Type: Application-level string manipulation
- Behavior: Modifies input before use (defense in depth)
- Primary defense: Parameterized queries via SqlParameter in DatabaseHelper

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-011 | SecurityHelper.SanitizeInput |
| NFR-ADM-006 | Input Sanitization security tests |

---

### Rule ID: BR-ADM-007
**Module**: ADM
**Priority**: High

#### Rule Description
Error logging follows a dual-target strategy: errors are logged to both a daily log file and the database. If either target fails, the other continues. If both fail, the error is swallowed to prevent logging from crashing the application. The file log includes: Date, Context, User, Machine, Exception Type, Message, Stack Trace, and Inner Exception details.

#### Source
- **File**: `src/PropertyInsuranceClaims.Common/ErrorLogger.vb`
- **Methods**: LogError, LogToFile, LogToDatabase
- **Code Snippet**:
```vb
Public Shared Sub LogError(ex As Exception, context As String, Optional additionalInfo As String = Nothing)
    Try
        LogToFile(ex, context, additionalInfo)
        LogToDatabase(ex, context, additionalInfo)
    Catch
        ' Swallow - logging should never crash the app
    End Try
End Sub
```

#### Enforcement Mechanism
- Type: Application-level error handling pattern
- File target: {BaseDirectory}/Logs/ErrorLog_yyyyMMdd.log
- DB target: Admin.usp_ErrorLog_Insert
- Thread safety: SyncLock on _lockObj for file writes

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-ADM-026 | ErrorLogger.LogError |
| UT-ADM-027 | ErrorLogger.LogMessage |
| NFR-ADM-007 | Error Logging reliability |

---

### Rule ID: BR-ADM-008
**Module**: ADM
**Priority**: Medium

#### Rule Description
Configuration changes require audit trail with old and new values. When a system configuration value is changed via usp_Config_Set, the old value is retrieved first, then the update is applied, and an audit log entry is created with the FieldName (ConfigKey), OldValue, and NewValue.

#### Source
- **File**: `database/02-stored-procedures/011-admin-utility-sps.sql`
- **SP**: Admin.usp_Config_Set
- **Code Snippet**:
```sql
DECLARE @OldValue VARCHAR(500);
SELECT @OldValue = ConfigValue FROM Admin.SystemConfig WHERE ConfigKey = @ConfigKey;
...
INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
VALUES ('Admin.SystemConfig', 0, 'UPDATE', @ConfigKey, @OldValue, @ConfigValue, @ModifiedBy, GETDATE());
```

#### Enforcement Mechanism
- Type: SP-level audit logging
- Behavior: Every config change creates an audit trail entry
- Note: Non-existent keys raise error (no silent failures)

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-ADM-009 | usp_Config_Set |
| FT-ADM-006 | System Configuration Management |
| US-ADM-006 | System Configuration user story |

---

### Rule ID: BR-ADM-009
**Module**: ADM
**Priority**: Medium

#### Rule Description
User creation is transactional: if any step fails (user insert, role assignment, or audit logging), the entire operation is rolled back. On error, details are logged to Audit.ErrorLog and the error is re-thrown via THROW.

#### Source
- **File**: `database/02-stored-procedures/011-admin-utility-sps.sql`
- **SP**: Admin.usp_User_Create
- **Code Snippet**:
```sql
BEGIN TRY
    BEGIN TRANSACTION;
    -- Insert user, assign role, create audit entry
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    INSERT INTO Audit.ErrorLog (...) VALUES (...);
    THROW;
END CATCH
```

#### Enforcement Mechanism
- Type: Database transaction with TRY/CATCH
- Behavior: All-or-nothing user creation
- Error handling: Rollback + ErrorLog insert + THROW

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-ADM-002 | usp_User_Create |
| US-ADM-003 | User Management |

---

### Rule ID: BR-ADM-010
**Module**: ADM
**Priority**: Medium

#### Rule Description
Lookup values must be unique within a category. The combination of CategoryID and LookupCode has a UNIQUE constraint at the database level, and the usp_Lookup_Create SP also checks for existence before insert, raising a descriptive error message.

#### Source
- **File**: `database/02-stored-procedures/011-admin-utility-sps.sql`
- **SP**: Admin.usp_Lookup_Create
- **Code Snippet**:
```sql
IF EXISTS (SELECT 1 FROM Admin.LookupValues WHERE CategoryID = @CategoryID AND LookupCode = @LookupCode)
BEGIN
    RAISERROR('Lookup already exists: %s / %s', 16, 1, @Category, @LookupCode);
    RETURN;
END
```

#### Enforcement Mechanism
- Type: SP-level check + database UNIQUE constraint (defense in depth)
- Behavior: Descriptive error message before constraint violation would occur

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-ADM-010 | usp_Lookup_Create |
| FT-ADM-007 | Lookup Maintenance |
| DV-ADM-006 | LookupValues data validation |
