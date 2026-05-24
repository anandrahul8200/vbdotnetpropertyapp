# Admin Module - Unit Tests

## Module: ADM (Admin)
## Test Type: Unit Tests for Data Access Layer and Common Utilities
## Classes Covered:
- `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`
- `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`
- `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`
- `src/PropertyInsuranceClaims.Common/ErrorLogger.vb`
- `src/PropertyInsuranceClaims.Common/GlobalState.vb`
- `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`
- `src/PropertyInsuranceClaims.Common/AppSettings.vb`

---

### Test Case ID: UT-ADM-001
**Class**: AdminDataAccess
**Method**: CreateUser(username, passwordHash, firstName, lastName, email, roleID, department) As Integer
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Method Signature
```vb
Public Shared Function CreateUser(username As String, passwordHash As String, firstName As String,
    lastName As String, email As String, roleID As Integer, Optional department As String = Nothing) As Integer
```

#### Parameter Mapping
| Parameter | SP Parameter | Type |
|-----------|-------------|------|
| username | @Username | VARCHAR(50) |
| passwordHash | @PasswordHash | VARCHAR(256) |
| firstName | @FirstName | VARCHAR(100) |
| lastName | @LastName | VARCHAR(100) |
| email | @Email | VARCHAR(200) |
| roleID | @RoleID | INT |
| department | @Department | VARCHAR(50) |
| GlobalState.CurrentUser | @CreatedBy | VARCHAR(50) |
| (output) | @UserID | INT OUTPUT |

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_User_Create' |
| 2 | Passes 9 parameters (8 input + 1 output) | Valid inputs | 9 SqlParameter objects |
| 3 | Output param @UserID returned | Output returns 42 | Function returns 42 |
| 4 | CurrentUser passed as CreatedBy | GlobalState.CurrentUser='admin' | @CreatedBy='admin' |
| 5 | Optional department defaults to Nothing | department=Nothing | @Department param value = DBNull |
| 6 | ExecuteNonQuery called | Valid inputs | DatabaseHelper.ExecuteNonQuery invoked |

---

### Test Case ID: UT-ADM-002
**Class**: AdminDataAccess
**Method**: GetUserPermissions(userID) As List(Of String)
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetUserPermissions(userID As Integer) As List(Of String)
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_User_GetPermissions' |
| 2 | Returns list of uppercase permission codes | DataTable with rows | List(Of String) with ToUpper() codes |
| 3 | Empty DataTable returns empty list | No rows returned | Empty List(Of String) |
| 4 | Permission codes uppercased | Row has 'view_policy' | List contains 'VIEW_POLICY' |

---

### Test Case ID: UT-ADM-003
**Class**: AdminDataAccess
**Method**: GetConfig(key, category) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Method Signature
```vb
Public Shared Function GetConfig(Optional key As String = Nothing, Optional category As String = Nothing) As DataTable
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_Config_Get' |
| 2 | Passes @ConfigKey and @Category | key='MaxLogin', category='SECURITY' | Both params set |
| 3 | Both params optional | key=Nothing, category=Nothing | Params passed as Nothing (DBNull) |
| 4 | Returns DataTable from SP | Mock returns DataTable | Same DataTable returned |

---

### Test Case ID: UT-ADM-004
**Class**: AdminDataAccess
**Method**: SetConfig(key, value)
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_Config_Set' |
| 2 | Passes 3 parameters | key='Key1', value='Val1' | @ConfigKey, @ConfigValue, @ModifiedBy |
| 3 | CurrentUser as ModifiedBy | GlobalState.CurrentUser='admin' | @ModifiedBy='admin' |

---

### Test Case ID: UT-ADM-005
**Class**: AdminDataAccess
**Method**: SearchAuditLog(tableName, username, action, dateFrom, dateTo, pageNumber) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_Audit_Search' |
| 2 | Passes 8 parameters (7 input + 1 output) | All defaults | 8 SqlParameter objects |
| 3 | Uses ExecuteWithOutput | Valid inputs | DatabaseHelper.ExecuteWithOutput called |
| 4 | PageSize hardcoded to 100 | Any call | @PageSize=100 |
| 5 | Output param @TotalRecords | Valid call | Output param of SqlDbType.Int |

---

### Test Case ID: UT-ADM-006
**Class**: AdminDataAccess
**Method**: SearchErrorLog(procedureName, dateFrom) As DataTable
**File**: `src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Calls correct SP | Mock DatabaseHelper | SP = 'Admin.usp_ErrorLog_Search' |
| 2 | Passes 2 parameters | procedureName='test', dateFrom=today | @ProcedureName, @DateFrom |
| 3 | Uses ExecuteStoredProcedure | Valid inputs | DatabaseHelper.ExecuteStoredProcedure called |
| 4 | Optional params default to Nothing | Both Nothing | Params passed as Nothing |

---

### Test Case ID: UT-ADM-007
**Class**: SecurityHelper
**Method**: HashPassword(password, salt) As String
**File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`

#### Method Signature
```vb
Public Shared Function HashPassword(password As String, Optional salt As String = Nothing) As String
```

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Returns salt:hash format | password='Test123!' | Result contains ':' separator |
| 2 | Generates salt when none provided | salt=Nothing | Salt auto-generated (16 bytes base64) |
| 3 | Uses provided salt | salt='MySalt123' | Hash starts with 'MySalt123:' |
| 4 | Same password+salt = same hash | Same inputs twice | Identical output |
| 5 | Different passwords = different hashes | 'pass1' vs 'pass2', same salt | Different outputs |
| 6 | SHA-256 produces base64 output | Any input | Part after ':' is valid base64 |

---

### Test Case ID: UT-ADM-008
**Class**: SecurityHelper
**Method**: VerifyPassword(password, storedHash) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Correct password returns True | storedHash from HashPassword('test') | True |
| 2 | Wrong password returns False | storedHash for 'test', verify with 'wrong' | False |
| 3 | Empty storedHash returns False | storedHash='' | False |
| 4 | Null storedHash returns False | storedHash=Nothing | False |
| 5 | Hash without colon returns False | storedHash='nocolon' | False |
| 6 | Uses ordinal comparison | Valid hash | StringComparison.Ordinal used |
| 7 | Extracts salt from stored hash | storedHash='salt:hash' | Salt 'salt' extracted and used for re-hashing |

---

### Test Case ID: UT-ADM-009
**Class**: SecurityHelper
**Method**: GenerateSalt() As String
**File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Returns non-empty string | Call method | Non-empty result |
| 2 | Returns base64 encoded string | Call method | Valid base64 (16 bytes = 24 chars base64) |
| 3 | Each call produces unique salt | Call twice | Different results |
| 4 | Uses RNGCryptoServiceProvider | Inspect implementation | Cryptographically secure random |

---

### Test Case ID: UT-ADM-010
**Class**: SecurityHelper
**Method**: ValidatePasswordComplexity(password, ByRef errorMessage) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid password passes | password='Test123!' | True, errorMessage='' |
| 2 | Null password fails | password=Nothing | False, errorMessage='Password is required.' |
| 3 | Empty password fails | password='' | False, errorMessage='Password is required.' |
| 4 | Too short fails (< 8 chars) | password='Test1!' | False, errorMessage='Password must be at least 8 characters.' |
| 5 | No uppercase fails | password='test1234' | False, errorMessage='Password must contain at least one uppercase letter.' |
| 6 | No lowercase fails | password='TEST1234' | False, errorMessage='Password must contain at least one lowercase letter.' |
| 7 | No digit fails | password='TestTest' | False, errorMessage='Password must contain at least one digit.' |
| 8 | Exactly 8 chars passes | password='Testpas1' | True |
| 9 | Uses AppSettings.PasswordMinLength | Default=8 | Minimum length = 8 |

---

### Test Case ID: UT-ADM-011
**Class**: SecurityHelper
**Method**: SanitizeInput(input) As String
**File**: `src/PropertyInsuranceClaims.Common/SecurityHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Null input returns null | input=Nothing | Nothing |
| 2 | Empty input returns empty | input='' | '' |
| 3 | Single quotes doubled | input="O'Brien" | "O''Brien" |
| 4 | Double dashes removed | input='test--injection' | 'testinjection' |
| 5 | Semicolons removed | input='test;drop' | 'testdrop' |
| 6 | Input trimmed | input='  test  ' | 'test' |
| 7 | Combined sanitization | input=" test';--drop " | "test''drop" |

---

### Test Case ID: UT-ADM-012
**Class**: ValidationHelper
**Method**: IsValidEmail(email) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid email | email='user@domain.com' | True |
| 2 | Valid with subdomain | email='user@sub.domain.com' | True |
| 3 | Valid with plus | email='user+tag@domain.com' | True |
| 4 | Valid with dots | email='first.last@domain.com' | True |
| 5 | Null returns False | email=Nothing | False |
| 6 | Empty returns False | email='' | False |
| 7 | Whitespace returns False | email='   ' | False |
| 8 | Missing @ | email='nodomain.com' | False |
| 9 | Missing domain | email='user@' | False |
| 10 | Missing TLD | email='user@domain' | False |
| 11 | Single char TLD | email='user@domain.c' | False |

---

### Test Case ID: UT-ADM-013
**Class**: ValidationHelper
**Method**: IsValidPhone(phone) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | 10 digits plain | phone='5551234567' | True |
| 2 | 10 digits formatted | phone='(555) 123-4567' | True |
| 3 | 11 digits (1+10) | phone='15551234567' | True |
| 4 | 11 digits formatted | phone='1-555-123-4567' | True |
| 5 | Null returns False | phone=Nothing | False |
| 6 | Empty returns False | phone='' | False |
| 7 | 9 digits (too short) | phone='555123456' | False |
| 8 | 12 digits (too long) | phone='155512345678' | False |
| 9 | Non-numeric stripped | phone='(555) 123-4567' | Digits extracted: 5551234567 = 10 = True |

---

### Test Case ID: UT-ADM-014
**Class**: ValidationHelper
**Method**: IsValidZipCode(zip) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | 5-digit zip | zip='12345' | True |
| 2 | Zip+4 format | zip='12345-6789' | True |
| 3 | Null returns False | zip=Nothing | False |
| 4 | Empty returns False | zip='' | False |
| 5 | 4 digits | zip='1234' | False |
| 6 | 6 digits no dash | zip='123456' | False |
| 7 | Letters | zip='ABCDE' | False |
| 8 | Zip+3 (incomplete) | zip='12345-678' | False |

---

### Test Case ID: UT-ADM-015
**Class**: ValidationHelper
**Method**: IsValidStateCode(state) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid state code | state='FL' | True |
| 2 | DC valid | state='DC' | True |
| 3 | Lowercase converted | state='fl' | True (ToUpper applied) |
| 4 | With whitespace | state=' FL ' | True (Trim applied) |
| 5 | Null returns False | state=Nothing | False |
| 6 | Empty returns False | state='' | False |
| 7 | Invalid code | state='XX' | False |
| 8 | Three characters | state='FLA' | False |
| 9 | All 51 valid codes | Each of 50 states + DC | All return True |

---

### Test Case ID: UT-ADM-016
**Class**: ValidationHelper
**Method**: IsValidAmount(text, ByRef amount, minValue, maxValue) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid amount | text='1000.50' | True, amount=1000.50 |
| 2 | With dollar sign | text='$1,000.50' | True, amount=1000.50 |
| 3 | With commas | text='1,000,000' | True, amount=1000000 |
| 4 | Below minimum | text='5', minValue=10 | False |
| 5 | Above maximum | text='200', maxValue=100 | False |
| 6 | Null returns False | text=Nothing | False |
| 7 | Empty returns False | text='' | False |
| 8 | Non-numeric | text='abc' | False |
| 9 | Exactly at minimum | text='10', minValue=10 | True |
| 10 | Default min=0, max=MaxValue | text='1000' | True |

---

### Test Case ID: UT-ADM-017
**Class**: ValidationHelper
**Method**: IsValidDate(dateValue, minDate, maxDate) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid date (today) | dateValue=Today | True |
| 2 | Min default = 1900-01-01 | dateValue=#1/1/1900# | True |
| 3 | Below min default | dateValue=#12/31/1899# | False |
| 4 | Max default = Today+5 years | dateValue=Today.AddYears(5) | True |
| 5 | Above max default | dateValue=Today.AddYears(6) | False |
| 6 | Custom min/max range | minDate=Today, maxDate=Today.AddDays(30) | Date within range: True |
| 7 | Date exactly at min | dateValue=minDate | True |
| 8 | Date exactly at max | dateValue=maxDate | True |

---

### Test Case ID: UT-ADM-018
**Class**: ValidationHelper
**Method**: IsValidYearBuilt(text, ByRef year) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid year | text='2000' | True, year=2000 |
| 2 | Minimum year (1800) | text='1800' | True |
| 3 | Current year | text=DateTime.Now.Year.ToString() | True |
| 4 | Below minimum | text='1799' | False |
| 5 | Future year | text=(DateTime.Now.Year+1).ToString() | False |
| 6 | Non-numeric | text='abcd' | False |
| 7 | Empty | text='' | False |

---

### Test Case ID: UT-ADM-019
**Class**: ValidationHelper
**Method**: IsValidPolicyNumber(policyNumber) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid format | policyNumber='POL1234567' | True |
| 2 | Null returns False | policyNumber=Nothing | False |
| 3 | Empty returns False | policyNumber='' | False |
| 4 | Wrong prefix | policyNumber='CLM1234567' | False |
| 5 | Too few digits | policyNumber='POL123456' | False |
| 6 | Too many digits | policyNumber='POL12345678' | False |
| 7 | Lowercase prefix | policyNumber='pol1234567' | False (regex is case-sensitive) |
| 8 | With spaces (trimmed) | policyNumber=' POL1234567 ' | True (Trim applied) |

---

### Test Case ID: UT-ADM-020
**Class**: ValidationHelper
**Method**: IsValidClaimNumber(claimNumber) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid format | claimNumber='CLM1234567' | True |
| 2 | Null returns False | claimNumber=Nothing | False |
| 3 | Wrong prefix | claimNumber='POL1234567' | False |
| 4 | Too few digits | claimNumber='CLM123456' | False |
| 5 | Too many digits | claimNumber='CLM12345678' | False |

---

### Test Case ID: UT-ADM-021
**Class**: ValidationHelper
**Method**: FormatPhone(phone) As String
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | 10 digits formatted | phone='5551234567' | '(555) 123-4567' |
| 2 | 11 digits formatted | phone='15551234567' | '+1 (555) 123-4567' |
| 3 | Null returns empty | phone=Nothing | '' |
| 4 | Empty returns empty | phone='' | '' |
| 5 | Already formatted (strips non-digits) | phone='(555) 123-4567' | '(555) 123-4567' |
| 6 | Other length returns original | phone='12345' | '12345' |

---

### Test Case ID: UT-ADM-022
**Class**: ValidationHelper
**Method**: FormatCurrency(amount) As String
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Positive amount | amount=1234.56 | '$1,234.56' |
| 2 | Zero | amount=0 | '$0.00' |
| 3 | Negative | amount=-500 | '($500.00)' or '-$500.00' (locale dependent) |
| 4 | Large amount | amount=1000000 | '$1,000,000.00' |

---

### Test Case ID: UT-ADM-023
**Class**: ValidationHelper
**Method**: FormatCurrencyAbbreviated(amount) As String
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Millions | amount=1200000 | '$1.2M' |
| 2 | Thousands | amount=500000 | '$500K' |
| 3 | Below thousand | amount=999 | '$999' (C0 format) |
| 4 | Exactly 1M | amount=1000000 | '$1.0M' |
| 5 | Exactly 1K | amount=1000 | '$1K' |

---

### Test Case ID: UT-ADM-024
**Class**: ValidationHelper
**Method**: RequireField(control, fieldName) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Non-empty field passes | control.Text='value' | True |
| 2 | Empty field fails | control.Text='' | False, MessageBox shown: "{fieldName} is required." |
| 3 | Whitespace-only fails | control.Text='   ' | False |
| 4 | Focus set on failure | Empty control | control.Focus() called |

---

### Test Case ID: UT-ADM-025
**Class**: ValidationHelper
**Method**: RequireSelection(control, fieldName) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/ValidationHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Valid selection | control.SelectedIndex=1 | True |
| 2 | No selection (index -1) | control.SelectedIndex=-1 | False, MessageBox: "Please select a {fieldName}." |
| 3 | Placeholder selected (index 0 starting with '(') | Items(0)='(Select...)' | False |
| 4 | Focus set on failure | No selection | control.Focus() called |

---

### Test Case ID: UT-ADM-026
**Class**: ErrorLogger
**Method**: LogError(ex, context, additionalInfo)
**File**: `src/PropertyInsuranceClaims.Common/ErrorLogger.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Logs to file | ex=New Exception('test') | File created in Logs/ directory |
| 2 | File contains exception details | Valid exception | Date, Context, User, Machine, ExceptionType, Message, StackTrace |
| 3 | Logs to database | ex=valid | Calls Admin.usp_ErrorLog_Insert via DatabaseHelper |
| 4 | Database params correct | ex with message | @ErrorSeverity=16, @ErrorState=1, @ErrorProcedure=context |
| 5 | Swallows logging errors | File write throws | No exception propagated |
| 6 | Swallows database errors | DB call throws | No exception propagated |
| 7 | Inner exception logged | ex with InnerException | Inner Message and StackTrace in file |
| 8 | Log filename format | Call method | ErrorLog_yyyyMMdd.log |
| 9 | Directory created if missing | Logs/ dir does not exist | Directory.CreateDirectory called |
| 10 | Thread-safe (SyncLock) | Concurrent calls | Uses _lockObj for synchronization |

---

### Test Case ID: UT-ADM-027
**Class**: ErrorLogger
**Method**: LogMessage(message, context, severity)
**File**: `src/PropertyInsuranceClaims.Common/ErrorLogger.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Writes formatted entry | message='test', context='module' | '[date] [ERROR] [module] test' in file |
| 2 | Custom severity | severity='WARNING' | '[date] [WARNING] [context] message' |
| 3 | Default severity is ERROR | severity not provided | '[ERROR]' used |
| 4 | Swallows errors | File write fails | No exception propagated |

---

### Test Case ID: UT-ADM-028
**Class**: GlobalState
**Method**: HasPermission(permissionCode) As Boolean
**File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Permission exists (exact match) | Permissions contains 'VIEW_POLICY' | HasPermission('VIEW_POLICY') = True |
| 2 | Case insensitive | Permissions contains 'VIEW_POLICY' | HasPermission('view_policy') = True |
| 3 | Permission not in list | Permissions does not contain 'DELETE_ALL' | HasPermission('DELETE_ALL') = False |
| 4 | Empty permissions list | Permissions.Clear() | HasPermission(anything) = False |

---

### Test Case ID: UT-ADM-029
**Class**: GlobalState
**Method**: SetSession(userID, username, fullName, role, userPermissions)
**File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Sets all properties | Valid params | CurrentUserID, CurrentUser, CurrentUserFullName, CurrentRole all set |
| 2 | Permissions stored | List with 3 items | Permissions.Count = 3 |
| 3 | LoginTime set to Now | Call SetSession | LoginTime close to DateTime.Now |
| 4 | SessionID generated | Call SetSession | Non-empty GUID string (32 chars, format "N") |

---

### Test Case ID: UT-ADM-030
**Class**: GlobalState
**Method**: ClearSession()
**File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Resets UserID to 0 | After SetSession | CurrentUserID = 0 |
| 2 | Resets Username to SYSTEM | After SetSession | CurrentUser = 'SYSTEM' |
| 3 | Clears permissions | After SetSession | Permissions.Count = 0 |
| 4 | Resets LoginTime | After SetSession | LoginTime = DateTime.MinValue |
| 5 | Clears SessionID | After SetSession | SessionID = '' |

---

### Test Case ID: UT-ADM-031
**Class**: GlobalState
**Method**: IsSessionExpired() As Boolean
**File**: `src/PropertyInsuranceClaims.Common/GlobalState.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Not expired (recent login) | LoginTime = Now | False |
| 2 | Expired (old login) | LoginTime = Now.AddMinutes(-31) | True |
| 3 | Never logged in (MinValue) | LoginTime = DateTime.MinValue | True |
| 4 | Exactly at timeout | LoginTime = Now.AddMinutes(-30) | False (TotalMinutes > 30 check, not >=) |
| 5 | Uses AppSettings.SessionTimeoutMinutes | Default=30 | Timeout threshold = 30 minutes |

---

### Test Case ID: UT-ADM-032
**Class**: DatabaseHelper
**Method**: ExecuteStoredProcedure(spName, params) As DataTable
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Sets CommandType to StoredProcedure | Mock connection | cmd.CommandType = StoredProcedure |
| 2 | Sets CommandTimeout to 120 | Mock connection | cmd.CommandTimeout = 120 |
| 3 | Adds parameters to command | params array with 3 items | cmd.Parameters.AddRange called |
| 4 | Handles null params | params=Nothing | No AddRange call |
| 5 | Returns filled DataTable | Mock adapter | DataTable with data returned |
| 6 | Disposes connection | Call method | Using block ensures disposal |

---

### Test Case ID: UT-ADM-033
**Class**: DatabaseHelper
**Method**: ExecuteNonQuery(spName, params) As Integer
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Returns rows affected | Mock returns 5 | Function returns 5 |
| 2 | CommandType = StoredProcedure | Mock | Correct CommandType |
| 3 | Connection opened | Mock | conn.Open() called |
| 4 | Connection disposed | Call method | Using block ensures disposal |

---

### Test Case ID: UT-ADM-034
**Class**: DatabaseHelper
**Method**: ExecuteScalar(spName, params) As Object
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Returns scalar value | Mock returns 42 | Function returns 42 |
| 2 | Returns DBNull for no result | Mock returns DBNull | DBNull.Value returned |

---

### Test Case ID: UT-ADM-035
**Class**: DatabaseHelper
**Method**: CreateParam(name, value) As SqlParameter
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Creates param with name and value | name='@Test', value='hello' | SqlParameter with correct name and value |
| 2 | Null value becomes DBNull | name='@Test', value=Nothing | SqlParameter.Value = DBNull.Value |

---

### Test Case ID: UT-ADM-036
**Class**: DatabaseHelper
**Method**: CreateOutputParam(name, dbType) As SqlParameter
**File**: `src/PropertyInsuranceClaims.Common/DatabaseHelper.vb`

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Direction = Output | name='@ID', dbType=SqlDbType.Int | param.Direction = ParameterDirection.Output |
| 2 | VarChar gets Size=200 | dbType=SqlDbType.VarChar | param.Size = 200 |
| 3 | NVarChar gets Size=200 | dbType=SqlDbType.NVarChar | param.Size = 200 |
| 4 | Int has no size set | dbType=SqlDbType.Int | Default size |
