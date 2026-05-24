# Admin Module - Stored Procedure Tests

## Module: ADM (Admin)
## Source Files:
- `database/02-stored-procedures/011-admin-utility-sps.sql`
- `database/02-stored-procedures/010-search-lookup-sps.sql`
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (28 total):
1. Admin.usp_User_Authenticate
2. Admin.usp_User_Create
3. Admin.usp_User_GetPermissions
4. Admin.usp_User_List
5. Admin.usp_User_Lock
6. Admin.usp_User_Unlock
7. Admin.usp_User_ResetPassword
8. Admin.usp_Config_Get
9. Admin.usp_Config_Set
10. Admin.usp_Lookup_Create
11. Admin.usp_Lookup_Update
12. Admin.usp_Lookup_GetByCategory
13. Admin.usp_Lookup_GetStates
14. Admin.usp_Lookup_GetAgents
15. Admin.usp_Lookup_GetCoverageTypes
16. Admin.usp_Lookup_GetDeductibleOptions
17. Admin.usp_Lookup_GetPaymentPlans
18. Admin.usp_Lookup_GetCatastrophes
19. Admin.usp_Search_Global
20. Admin.usp_Audit_Search
21. Admin.usp_ErrorLog_Search
22. Admin.usp_ErrorLog_Insert
23. Admin.usp_Role_GetAll
24. Admin.usp_RolePermission_GetByRole
25. Admin.usp_Notification_Create
26. Admin.usp_Notification_GetByUser
27. Admin.usp_Notification_MarkRead
28. Admin.usp_Notification_MarkAllRead

---

### Test Case ID: SP-ADM-001
**Procedure**: Admin.usp_User_Authenticate
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @Username VARCHAR(50) - Required
- @PasswordHash VARCHAR(256) - Required
- @IPAddress VARCHAR(50) - Optional (default NULL)
- @IsAuthenticated BIT OUTPUT
- @UserID INT OUTPUT
- @FullName VARCHAR(200) OUTPUT
- @RoleName VARCHAR(50) OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Successful authentication | @Username='validuser', @PasswordHash=matching hash, @IPAddress='192.168.1.1' | @IsAuthenticated=1, @UserID>0, @FullName='First Last', @RoleName='Administrator' |
| 2 | Login updates LastLoginDate | Valid credentials | Admin.Users.LastLoginDate = GETDATE() |
| 3 | Login resets FailedLoginAttempts to 0 | Valid credentials after previous failures | FailedLoginAttempts = 0 |
| 4 | Audit log entry created on login | Valid credentials | Audit.AuditLog row: Action='LOGIN', TableName='Admin.Users', AdditionalInfo contains IP |
| 5 | Authentication with NULL IPAddress | @Username='validuser', @PasswordHash=valid, @IPAddress=NULL | @IsAuthenticated=1, AdditionalInfo='IP: Unknown' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Invalid username | @Username='nonexistent', @PasswordHash='anyhash' | @IsAuthenticated=0, @UserID=0 |
| 2 | Invalid password hash | @Username='validuser', @PasswordHash='wronghash' | @IsAuthenticated=0, @UserID=0 |
| 3 | Inactive user | @Username='inactiveuser' (IsActive=0), valid hash | @IsAuthenticated=0 |
| 4 | Locked user | @Username='lockeduser' (IsLocked=1), valid hash | @IsAuthenticated=0 |
| 5 | Failed login increments counter | @Username='validuser', @PasswordHash='wrong' | FailedLoginAttempts incremented by 1 |
| 6 | Account locked after 5 failed attempts | 5th failed login (FailedLoginAttempts>=4) | IsLocked=1, LockedDate=GETDATE() |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Username at max length (50 chars) | @Username=50 character string | Processes normally |
| 2 | PasswordHash at max length (256 chars) | @PasswordHash=256 character string | Processes normally |
| 3 | Empty username | @Username='' | @IsAuthenticated=0 |
| 4 | Auto-lock threshold (FailedLoginAttempts=4) | 5th failed attempt | IsLocked set to 1, LockedDate set |

---

### Test Case ID: SP-ADM-002
**Procedure**: Admin.usp_User_Create
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @Username VARCHAR(50) - Required
- @PasswordHash VARCHAR(256) - Required
- @FirstName VARCHAR(100) - Required
- @LastName VARCHAR(100) - Required
- @Email VARCHAR(200) - Required
- @RoleID INT - Required
- @Department VARCHAR(50) - Optional (default NULL)
- @CreatedBy VARCHAR(50) - Required
- @UserID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create user with all fields | All params populated | @UserID > 0, user inserted with IsActive=1 |
| 2 | Create user without department | @Department=NULL | User created, Department is NULL |
| 3 | Role assigned in UserRoles | Valid @RoleID | Admin.UserRoles row created with AssignedDate=GETDATE() |
| 4 | Audit log entry created | Valid data | Audit.AuditLog: Action='INSERT', TableName='Admin.Users' |
| 5 | Transaction commits on success | Valid data | Both Users and UserRoles rows exist |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Duplicate username | @Username=existing username | 'Username already exists: {username}' (severity 16) |
| 2 | Invalid RoleID | @RoleID=99999 (nonexistent) | Foreign key violation |
| 3 | Transaction rollback on error | Failure mid-transaction | No partial data; error logged in Audit.ErrorLog |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Username at max length (50) | 50 char username | User created successfully |
| 2 | Email at max length (200) | 200 char email | User created successfully |

---

### Test Case ID: SP-ADM-003
**Procedure**: Admin.usp_User_GetPermissions
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @UserID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | User with multiple permissions | @UserID=user with Admin role | Distinct permission rows with PermissionCode, PermissionName, Module |
| 2 | Results ordered by Module, PermissionCode | @UserID=valid | Results sorted alphabetically by Module then PermissionCode |
| 3 | User with multiple roles | @UserID=user with 2 roles | DISTINCT permissions from both roles (no duplicates) |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent user | @UserID=99999 | Empty result set (no error) |
| 2 | User with no role assignments | @UserID=user with no UserRoles | Empty result set |

---

### Test Case ID: SP-ADM-004
**Procedure**: Admin.usp_User_List
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Returns all users with role info | None | Columns: UserID, Username, FirstName, LastName, Email, IsActive, IsLocked, LastLoginDate, FailedLoginAttempts, RoleName |
| 2 | Results ordered by Username | None | Alphabetical order by Username |
| 3 | LEFT JOIN includes users without roles | None | Users without UserRoles row still appear (RoleName=NULL) |

---

### Test Case ID: SP-ADM-005
**Procedure**: Admin.usp_User_Lock
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @LockedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Lock active user | @UserID=valid, @LockedBy='admin' | IsLocked=1, LockedDate=GETDATE() |
| 2 | Lock already locked user | @UserID=locked user | LockedDate updated to current time |

---

### Test Case ID: SP-ADM-006
**Procedure**: Admin.usp_User_Unlock
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @UnlockedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Unlock locked user | @UserID=locked user, @UnlockedBy='admin' | IsLocked=0, FailedLoginAttempts=0 |
| 2 | Unlock resets failed attempts | @UserID=user with FailedLoginAttempts=4 | FailedLoginAttempts=0 |

---

### Test Case ID: SP-ADM-007
**Procedure**: Admin.usp_User_ResetPassword
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @NewPasswordHash VARCHAR(256) - Required
- @NewSalt VARCHAR(128) - Required
- @ResetBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Reset password for valid user | All params valid | PasswordHash updated, Salt updated, ModifiedDate=GETDATE() |
| 2 | New hash stored correctly | @NewPasswordHash='newhash', @NewSalt='newsalt' | Values persisted in Admin.Users |

---

### Test Case ID: SP-ADM-008
**Procedure**: Admin.usp_Config_Get
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @ConfigKey VARCHAR(100) - Optional (default NULL)
- @Category VARCHAR(50) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all config values | @ConfigKey=NULL, @Category=NULL | All rows from Admin.SystemConfig |
| 2 | Get by specific key | @ConfigKey='MaxLoginAttempts' | Single row with matching ConfigKey |
| 3 | Get by category | @Category='SECURITY' | All rows where Category='SECURITY' |
| 4 | Get by key and category | @ConfigKey='MaxLoginAttempts', @Category='SECURITY' | Single matching row |
| 5 | Results ordered by Category, ConfigKey | Both NULL | Alphabetical order |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent key | @ConfigKey='NonExistentKey' | Empty result set |
| 2 | Non-existent category | @Category='INVALID' | Empty result set |

---

### Test Case ID: SP-ADM-009
**Procedure**: Admin.usp_Config_Set
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @ConfigKey VARCHAR(100) - Required
- @ConfigValue VARCHAR(500) - Required
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update existing config value | @ConfigKey='MaxLoginAttempts', @ConfigValue='10', @ModifiedBy='admin' | ConfigValue updated, ModifiedDate=GETDATE(), ModifiedBy='admin' |
| 2 | Audit log created with old/new values | Valid key change | AuditLog: FieldName=@ConfigKey, OldValue=previous, NewValue=@ConfigValue |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent config key | @ConfigKey='NonExistent' | 'Configuration key not found: NonExistent' (severity 16) |

---

### Test Case ID: SP-ADM-010
**Procedure**: Admin.usp_Lookup_Create
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @Category VARCHAR(50) - Required
- @LookupCode VARCHAR(30) - Required
- @LookupValue VARCHAR(200) - Required
- @Description VARCHAR(500) - Optional (default NULL)
- @SortOrder INT - Optional (default 0)
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create lookup in existing category | @Category='CLAIM_TYPE', @LookupCode='NEW', @LookupValue='New Type' | LookupValues row created with IsActive=1 |
| 2 | Create lookup in new category | @Category='NEW_CATEGORY', @LookupCode='CODE1', @LookupValue='Value1' | LookupCategories row created, then LookupValues row |
| 3 | Create with sort order | @SortOrder=5 | DisplayOrder=5 |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Duplicate code in same category | @Category='CLAIM_TYPE', @LookupCode=existing code | 'Lookup already exists: CLAIM_TYPE / {code}' (severity 16) |

---

### Test Case ID: SP-ADM-011
**Procedure**: Admin.usp_Lookup_Update
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @Category VARCHAR(50) - Required
- @LookupCode VARCHAR(30) - Required
- @LookupValue VARCHAR(200) - Optional (default NULL)
- @Description VARCHAR(500) - Optional (default NULL)
- @SortOrder INT - Optional (default NULL)
- @IsActive BIT - Optional (default NULL)
- @ModifiedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Update lookup value | @Category='CLAIM_TYPE', @LookupCode='FIRE', @LookupValue='Fire Damage' | LookupValue updated |
| 2 | Deactivate lookup | @IsActive=0 | IsActive set to 0 |
| 3 | Update sort order only | @SortOrder=10, other optional=NULL | Only DisplayOrder updated |
| 4 | ISNULL preserves existing values | @LookupValue=NULL | LookupValue unchanged |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent lookup | @Category='CLAIM_TYPE', @LookupCode='NONEXIST' | 'Lookup not found: CLAIM_TYPE / NONEXIST' (severity 16) |
| 2 | Non-existent category | @Category='INVALID' | 'Lookup not found' (CategoryID will be NULL, @@ROWCOUNT=0) |

---

### Test Case ID: SP-ADM-012
**Procedure**: Admin.usp_Lookup_GetByCategory
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @Category VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get active lookups for category | @Category='CLAIM_TYPE' | Rows with LookupCode, LookupValue, DisplayOrder where IsActive=1 |
| 2 | Results ordered by DisplayOrder then LookupValue | @Category=valid | Sorted by DisplayOrder ASC, then LookupValue ASC |
| 3 | Only active lookups returned | @Category with inactive items | Inactive items excluded |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent category | @Category='NONEXIST' | Empty result set |

---

### Test Case ID: SP-ADM-013
**Procedure**: Admin.usp_Lookup_GetStates
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @ActiveOnly BIT - Optional (default 1)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get active states | @ActiveOnly=1 | States where IsActive=1 AND current date between EffectiveDate/ExpiryDate |
| 2 | Get all states | @ActiveOnly=0 | All states regardless of IsActive |
| 3 | Returns rate columns | @ActiveOnly=1 | StateCode, StateName, TaxRate, SurchargeRate |
| 4 | Ordered by StateName | Default | Alphabetical by StateName |

---

### Test Case ID: SP-ADM-014
**Procedure**: Admin.usp_Lookup_GetAgents
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @StateCode CHAR(2) - Optional (default NULL)
- @AgencyID INT - Optional (default NULL)
- @ActiveOnly BIT - Optional (default 1)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all active agents | All defaults | AgentID, AgentNumber, AgentName, LicenseNumber, CommissionRate, AgencyName |
| 2 | Filter by state | @StateCode='FL' | Only agents in FL |
| 3 | Filter by agency | @AgencyID=1 | Only agents in that agency |
| 4 | Include inactive | @ActiveOnly=0 | All agents regardless of status |
| 5 | Ordered by LastName, FirstName | Default | Alphabetical sort |

---

### Test Case ID: SP-ADM-015
**Procedure**: Admin.usp_Lookup_GetCoverageTypes
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @PolicyType VARCHAR(30) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get coverages for HO3 | @PolicyType='HO3' | CoverageCode, CoverageName, Description, IsRequired, DefaultLimit, DefaultDeductible |
| 2 | Includes universal coverages | @PolicyType='HO3' | Rows where PolicyType='HO3' OR PolicyType='ALL' |
| 3 | Only active coverages | Valid type | IsActive=1 filter applied |
| 4 | Ordered by SortOrder then name | Valid type | SortOrder ASC, CoverageName ASC |

---

### Test Case ID: SP-ADM-016
**Procedure**: Admin.usp_Lookup_GetDeductibleOptions
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @PolicyType VARCHAR(30) - Required
- @CoverageCode VARCHAR(20) - Required
- @StateCode CHAR(2) - Optional (default NULL)
- @EffectiveDate DATE - Optional (default GETDATE())

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get deductible options | @PolicyType='HO3', @CoverageCode='DWELLING' | DeductibleType, DeductibleAmount, DeductiblePercent, PremiumFactor, IsDefault |
| 2 | State-specific options | @StateCode='FL' | Options where StateCode='FL' OR StateCode IS NULL |
| 3 | Date-effective filtering | @EffectiveDate=today | Only options where date BETWEEN EffectiveDate AND ExpiryDate |
| 4 | Ordered by DeductibleAmount | Valid params | Ascending by DeductibleAmount |

---

### Test Case ID: SP-ADM-017
**Procedure**: Admin.usp_Search_Global
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @SearchTerm VARCHAR(100) - Required
- @MaxResults INT - Optional (default 20)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search by customer name | @SearchTerm='Smith' | CUSTOMER rows matching FirstName/LastName/CompanyName |
| 2 | Search by policy number | @SearchTerm='POL0001' | POLICY rows matching PolicyNumber |
| 3 | Search by claim number | @SearchTerm='CLM0001' | CLAIM rows matching ClaimNumber |
| 4 | Search by property address | @SearchTerm='Main St' | PROPERTY rows matching AddressLine1/PropertyNumber |
| 5 | Cross-entity results | @SearchTerm=term matching multiple entity types | UNION ALL results from customers, policies, claims, properties |
| 6 | Respects MaxResults per entity | @MaxResults=5 | At most 5 results per entity type |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matches | @SearchTerm='ZZZZZZZZZ' | Empty result set |

---

### Test Case ID: SP-ADM-018
**Procedure**: Admin.usp_Audit_Search
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @TableName VARCHAR(100) - Optional (default NULL)
- @RecordID INT - Optional (default NULL)
- @Username VARCHAR(50) - Optional (default NULL)
- @Action VARCHAR(20) - Optional (default NULL)
- @DateFrom DATETIME - Optional (default GETDATE()-30 days)
- @DateTo DATETIME - Optional (default GETDATE())
- @PageNumber INT - Optional (default 1)
- @PageSize INT - Optional (default 100)
- @TotalRecords INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search with all defaults | All NULL/defaults | Last 30 days of audit entries, max 100 per page |
| 2 | Filter by TableName | @TableName='Admin.Users' | Only rows where TableName='Admin.Users' |
| 3 | Filter by Username | @Username='admin' | Only rows where Username='admin' |
| 4 | Filter by Action | @Action='LOGIN' | Only LOGIN actions |
| 5 | Filter by date range | @DateFrom='2024-01-01', @DateTo='2024-01-31' | Only rows in Jan 2024 |
| 6 | Pagination works | @PageNumber=2, @PageSize=10 | Rows 11-20, @TotalRecords=total count |
| 7 | @TotalRecords output | Valid filters | Correct count of matching records |
| 8 | Results ordered by ActionDate DESC | Default | Most recent first |

---

### Test Case ID: SP-ADM-019
**Procedure**: Admin.usp_ErrorLog_Search
**Source**: `database/02-stored-procedures/011-admin-utility-sps.sql`
**Parameters**:
- @ProcedureName VARCHAR(200) - Optional (default NULL)
- @DateFrom DATETIME - Optional (default GETDATE()-7 days)
- @DateTo DATETIME - Optional (default GETDATE())
- @PageNumber INT - Optional (default 1)
- @PageSize INT - Optional (default 50)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search with defaults | All NULL | Last 7 days of errors, max 50 per page |
| 2 | Filter by procedure name (LIKE) | @ProcedureName='User' | Rows where ErrorProcedure LIKE '%User%' |
| 3 | Pagination | @PageNumber=2, @PageSize=10 | Rows 11-20 |
| 4 | Results ordered by ErrorDate DESC | Default | Most recent first |

---

### Test Case ID: SP-ADM-020
**Procedure**: Admin.usp_ErrorLog_Insert
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @ErrorNumber INT - Optional (default 0)
- @ErrorSeverity INT - Optional (default 0)
- @ErrorState INT - Optional (default 0)
- @ErrorProcedure VARCHAR(200) - Optional (default NULL)
- @ErrorLine INT - Optional (default 0)
- @ErrorMessage VARCHAR(MAX) - Required
- @AdditionalInfo VARCHAR(MAX) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Insert error with all fields | All populated | Row inserted in Audit.ErrorLog with ErrorDate=GETDATE() |
| 2 | Insert with minimal fields | @ErrorMessage='Test error' only | Row inserted, other fields have defaults |

---

### Test Case ID: SP-ADM-021
**Procedure**: Admin.usp_Role_GetAll
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Returns all roles | None | All columns from Admin.Roles ordered by RoleName |

---

### Test Case ID: SP-ADM-022
**Procedure**: Admin.usp_RolePermission_GetByRole
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @RoleID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get permissions for role | @RoleID=valid | RolePermission rows with PermissionCode, PermissionName, Module |
| 2 | Role with no permissions | @RoleID=role with no assignments | Empty result set |

---

### Test Case ID: SP-ADM-023
**Procedure**: Admin.usp_Notification_Create
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @NotificationType VARCHAR(30) - Required
- @Subject VARCHAR(200) - Required
- @Message VARCHAR(MAX) - Required
- @EntityType VARCHAR(20) - Optional (default NULL)
- @EntityID INT - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create notification with entity link | All params | Notification created for user |
| 2 | Create notification without entity | @EntityType=NULL, @EntityID=NULL | Notification created (standalone) |

---

### Test Case ID: SP-ADM-024
**Procedure**: Admin.usp_Notification_GetByUser
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required
- @UnreadOnly BIT - Optional (default 0)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all notifications | @UserID=valid, @UnreadOnly=0 | All notifications for user |
| 2 | Get unread only | @UserID=valid, @UnreadOnly=1 | Only unread notifications |

---

### Test Case ID: SP-ADM-025
**Procedure**: Admin.usp_Notification_MarkRead
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @NotificationID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Mark single notification read | @NotificationID=valid | Notification marked as read |

---

### Test Case ID: SP-ADM-026
**Procedure**: Admin.usp_Notification_MarkAllRead
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @UserID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Mark all notifications read for user | @UserID=valid | All user notifications marked read |

---

### Test Case ID: SP-ADM-027
**Procedure**: Admin.usp_Lookup_GetPaymentPlans
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**: None

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Returns active payment plans | None | PlanCode, PlanName, NumberOfInstallments, DownPaymentPercent, InstallmentFee where IsActive=1 |
| 2 | Ordered by NumberOfInstallments | None | Ascending order |

---

### Test Case ID: SP-ADM-028
**Procedure**: Admin.usp_Lookup_GetCatastrophes
**Source**: `database/02-stored-procedures/010-search-lookup-sps.sql`
**Parameters**:
- @ActiveOnly BIT - Optional (default 1)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get active catastrophes | @ActiveOnly=1 | CatastropheID, CatastropheNumber, CatastropheName, CatastropheType, EventDate, AffectedStates, TotalClaimsCount where IsActive=1 |
| 2 | Get all catastrophes | @ActiveOnly=0 | All records regardless of IsActive |
| 3 | Ordered by EventDate DESC | Default | Most recent events first |
