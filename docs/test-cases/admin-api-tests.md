# Admin Module - API Tests (Future-State)

## Module: ADM (Admin)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-ADM-001
**Legacy SP**: Admin.usp_User_Authenticate
**Future Endpoint**: POST /api/auth/login
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Admin.usp_User_Authenticate | POST /api/auth/login |
| Admin.usp_User_GetPermissions | GET /api/auth/permissions |
| Admin.usp_User_Create | POST /api/admin/users |
| Admin.usp_User_List | GET /api/admin/users |
| Admin.usp_User_Lock | POST /api/admin/users/{id}/lock |
| Admin.usp_User_Unlock | POST /api/admin/users/{id}/unlock |
| Admin.usp_User_ResetPassword | POST /api/admin/users/{id}/reset-password |
| Admin.usp_Config_Get | GET /api/admin/config?key=&category= |
| Admin.usp_Config_Set | PUT /api/admin/config/{key} |
| Admin.usp_Lookup_GetByCategory | GET /api/lookups/{category} |
| Admin.usp_Lookup_Create | POST /api/lookups |
| Admin.usp_Lookup_Update | PUT /api/lookups/{category}/{code} |
| Admin.usp_Lookup_GetStates | GET /api/lookups/states |
| Admin.usp_Lookup_GetAgents | GET /api/lookups/agents |
| Admin.usp_Lookup_GetCoverageTypes | GET /api/lookups/coverage-types?policyType= |
| Admin.usp_Lookup_GetDeductibleOptions | GET /api/lookups/deductible-options |
| Admin.usp_Lookup_GetPaymentPlans | GET /api/lookups/payment-plans |
| Admin.usp_Lookup_GetCatastrophes | GET /api/lookups/catastrophes |
| Admin.usp_Search_Global | GET /api/search?term=&maxResults= |
| Admin.usp_Audit_Search | GET /api/admin/audit |
| Admin.usp_ErrorLog_Search | GET /api/admin/errors |
| Admin.usp_Role_GetAll | GET /api/admin/roles |
| Admin.usp_RolePermission_GetByRole | GET /api/admin/roles/{id}/permissions |
| Admin.usp_Notification_Create | POST /api/notifications |
| Admin.usp_Notification_GetByUser | GET /api/notifications?unreadOnly= |
| Admin.usp_Notification_MarkRead | PUT /api/notifications/{id}/read |
| Admin.usp_Notification_MarkAllRead | PUT /api/notifications/read-all |

#### Request/Response Specification
**Request (POST /api/auth/login):**
```json
{
  "username": "admin",
  "password": "SecurePass1"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "fullName": "John Admin",
  "role": "Administrator",
  "permissions": ["VIEW_POLICY", "CREATE_POLICY", "MANAGE_USERS"],
  "expiresAt": "2024-01-01T01:00:00Z"
}
```

**Response (401 Unauthorized):**
```json
{
  "error": "Invalid username or password",
  "code": "AUTH_FAILED"
}
```

**Response (423 Locked):**
```json
{
  "error": "Account is locked due to too many failed attempts",
  "code": "ACCOUNT_LOCKED"
}
```

#### API Tests
| # | Test | Method | Endpoint | Expected Status |
|---|------|--------|----------|-----------------|
| 1 | Successful login | POST | /api/auth/login | 200 + JWT token |
| 2 | Invalid credentials | POST | /api/auth/login | 401 Unauthorized |
| 3 | Locked account | POST | /api/auth/login | 423 Locked |
| 4 | Missing username | POST | /api/auth/login | 400 Bad Request |
| 5 | Missing password | POST | /api/auth/login | 400 Bad Request |
| 6 | Get user permissions | GET | /api/auth/permissions | 200 + permission list |
| 7 | Create user | POST | /api/admin/users | 201 Created |
| 8 | Duplicate username | POST | /api/admin/users | 409 Conflict |
| 9 | List all users | GET | /api/admin/users | 200 + user array |
| 10 | Lock user | POST | /api/admin/users/{id}/lock | 200 OK |
| 11 | Unlock user | POST | /api/admin/users/{id}/unlock | 200 OK |
| 12 | Reset password | POST | /api/admin/users/{id}/reset-password | 200 OK |
| 13 | Get config by key | GET | /api/admin/config?key=MaxLoginAttempts | 200 + config value |
| 14 | Update config | PUT | /api/admin/config/MaxLoginAttempts | 200 OK |
| 15 | Config key not found | PUT | /api/admin/config/NonExistent | 404 Not Found |
| 16 | Get lookups | GET | /api/lookups/CLAIM_TYPE | 200 + lookup array |
| 17 | Create lookup | POST | /api/lookups | 201 Created |
| 18 | Duplicate lookup | POST | /api/lookups | 409 Conflict |
| 19 | Global search | GET | /api/search?term=Smith | 200 + results |
| 20 | Search audit log | GET | /api/admin/audit?action=LOGIN | 200 + paginated results |
| 21 | Unauthorized access (no token) | GET | /api/admin/users | 401 Unauthorized |
| 22 | Insufficient permissions | POST | /api/admin/users (non-admin) | 403 Forbidden |
