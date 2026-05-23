# Documents Module - API Tests (Future-State)

## Module: DOC (Documents)
## Test Type: Future-State REST API Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application does not currently expose this functionality via REST APIs.
> These tests serve as a specification for the target-state implementation.

---

### Test Case ID: API-DOC-001
**Legacy SP**: Policy.usp_Document_Create
**Future Endpoint**: POST /api/documents
**Status**: FUTURE-STATE (not yet implemented)

#### Endpoint Mapping
| Legacy | Future |
|--------|--------|
| Policy.usp_Document_Create | POST /api/documents |
| Policy.usp_Document_GetByEntity | GET /api/documents?entityType=&entityId= |
| Policy.usp_Document_Delete | DELETE /api/documents/{id} |
| Policy.usp_Document_Search | GET /api/documents/search |
| Admin.usp_Template_GetAll | GET /api/templates?category= |
| Admin.usp_Template_GetByID | GET /api/templates/{id} |
| Admin.usp_Template_Save | POST /api/templates or PUT /api/templates/{id} |
| Admin.usp_Correspondence_Log | POST /api/correspondence |
| Admin.usp_Correspondence_GetHistory | GET /api/correspondence?entityType=&entityId= |

#### Request/Response Specification
**Request (POST /api/documents):**
```json
{
  "entityType": "POLICY",
  "entityId": 1,
  "documentType": "PHOTO",
  "fileName": "property-front.jpg",
  "filePath": "/uploads/2025/06/property-front.jpg",
  "fileSize": 245000,
  "description": "Front view of insured property"
}
```

**Response (201 Created):**
```json
{
  "documentId": 42,
  "entityType": "POLICY",
  "entityId": 1,
  "documentType": "PHOTO",
  "fileName": "property-front.jpg",
  "fileSize": 245000,
  "description": "Front view of insured property",
  "uploadDate": "2025-06-30T14:30:00Z",
  "createdBy": "admin"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Missing entityType | 400 Bad Request: "entityType is required" |
| 2 | Missing entityId | 400 Bad Request: "entityId is required" |
| 3 | Missing documentType | 400 Bad Request: "documentType is required" |
| 4 | Missing fileName | 400 Bad Request: "fileName is required" |
| 5 | Invalid entityType | 400 Bad Request: "entityType must be POLICY or CLAIM" |
| 6 | Invalid documentType | 400 Bad Request: "documentType not recognized" |
| 7 | Unauthorized | 401 Unauthorized |
| 8 | File too large | 413 Payload Too Large |

---

### Test Case ID: API-DOC-002
**Legacy SP**: Policy.usp_Document_GetByEntity
**Future Endpoint**: GET /api/documents?entityType=&entityId=
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/documents?entityType=POLICY&entityId=1
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "documents": [
    {
      "documentId": 1,
      "entityType": "POLICY",
      "entityId": 1,
      "documentType": "PHOTO",
      "fileName": "property-front.jpg",
      "fileSize": 245000,
      "uploadDate": "2025-01-15T10:00:00Z",
      "createdBy": "admin"
    }
  ]
}
```

---

### Test Case ID: API-DOC-003
**Legacy SP**: Policy.usp_Document_Delete
**Future Endpoint**: DELETE /api/documents/{id}
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
DELETE /api/documents/42
Authorization: Bearer {token}
```

**Response (204 No Content)**

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Delete existing document | 204 No Content |
| 2 | Delete non-existent document | 404 Not Found |
| 3 | Unauthorized deletion | 401 Unauthorized |
| 4 | Insufficient permissions | 403 Forbidden |

---

### Test Case ID: API-DOC-004
**Legacy SP**: Policy.usp_Document_Search
**Future Endpoint**: GET /api/documents/search
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/documents/search?entityType=POLICY&documentType=PHOTO&dateFrom=2024-01-01&dateTo=2024-12-31&page=1&pageSize=50
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "page": 1,
  "pageSize": 50,
  "totalCount": 120,
  "documents": [
    {
      "documentId": 1,
      "entityType": "POLICY",
      "entityId": 1,
      "documentType": "PHOTO",
      "fileName": "photo.jpg",
      "uploadDate": "2024-03-15T09:00:00Z"
    }
  ]
}
```

---

### Test Case ID: API-DOC-005
**Legacy SP**: Admin.usp_Template_GetAll
**Future Endpoint**: GET /api/templates
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/templates?category=POLICY
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "templates": [
    {
      "templateId": 1,
      "templateName": "Cancellation Notice",
      "category": "POLICY"
    },
    {
      "templateId": 4,
      "templateName": "Renewal Notice",
      "category": "POLICY"
    },
    {
      "templateId": 5,
      "templateName": "Non-Renewal Notice",
      "category": "POLICY"
    }
  ]
}
```

---

### Test Case ID: API-DOC-006
**Legacy SP**: Admin.usp_Template_GetByID
**Future Endpoint**: GET /api/templates/{id}
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/templates/1
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "templateId": 1,
  "templateName": "Cancellation Notice",
  "category": "POLICY",
  "subject": "Notice of Policy Cancellation",
  "body": "Dear [InsuredName],\n\nThis is to notify you that policy [PolicyNumber] has been cancelled effective [Date]...",
  "variables": ["InsuredName", "PolicyNumber", "Date", "Amount"]
}
```

---

### Test Case ID: API-DOC-007
**Legacy SP**: Admin.usp_Template_Save
**Future Endpoint**: POST /api/templates or PUT /api/templates/{id}
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/templates - create new):**
```json
{
  "templateName": "New Notice",
  "category": "POLICY",
  "subject": "Important Notice",
  "body": "Dear [InsuredName],\n\nPlease be advised..."
}
```

**Response (201 Created):**
```json
{
  "templateId": 7,
  "templateName": "New Notice",
  "category": "POLICY",
  "subject": "Important Notice",
  "createdBy": "admin",
  "createdDate": "2025-06-30T14:30:00Z"
}
```

#### Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Missing templateName | 400 Bad Request: "templateName is required" |
| 2 | Missing category | 400 Bad Request: "category is required" |
| 3 | Invalid category | 400 Bad Request: "category must be one of: POLICY, CLAIMS, BILLING, UNDERWRITING, GENERAL" |
| 4 | Template name too long (> 100) | 400 Bad Request: "templateName exceeds maximum length" |

---

### Test Case ID: API-DOC-008
**Legacy SP**: Admin.usp_Correspondence_Log
**Future Endpoint**: POST /api/correspondence
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request (POST /api/correspondence):**
```json
{
  "entityType": "POLICY",
  "entityId": 1,
  "templateId": 1,
  "recipientName": "John Doe",
  "recipientAddress": "123 Main St, Anytown, ST 12345",
  "subject": "Cancellation Notice",
  "deliveryMethod": "PRINT"
}
```

**Response (201 Created):**
```json
{
  "correspondenceId": 100,
  "entityType": "POLICY",
  "entityId": 1,
  "subject": "Cancellation Notice",
  "deliveryMethod": "PRINT",
  "sentBy": "admin",
  "sentDate": "2025-06-30T14:30:00Z"
}
```

---

### Test Case ID: API-DOC-009
**Legacy SP**: Admin.usp_Correspondence_GetHistory
**Future Endpoint**: GET /api/correspondence?entityType=&entityId=
**Status**: FUTURE-STATE (not yet implemented)

#### Request/Response Specification
**Request:**
```
GET /api/correspondence?entityType=POLICY&entityId=1
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "history": [
    {
      "correspondenceId": 100,
      "templateName": "Cancellation Notice",
      "recipientName": "John Doe",
      "subject": "Cancellation Notice",
      "deliveryMethod": "PRINT",
      "sentBy": "admin",
      "sentDate": "2025-06-30T14:30:00Z"
    }
  ]
}
```

#### Common API Validation Tests
| # | Test | Expected |
|---|------|----------|
| 1 | No auth token | 401 Unauthorized |
| 2 | Expired token | 401 Unauthorized |
| 3 | User without document permissions | 403 Forbidden |
| 4 | Invalid JSON body | 400 Bad Request |
| 5 | Rate limiting | 429 Too Many Requests after threshold |
