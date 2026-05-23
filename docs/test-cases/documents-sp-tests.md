# Documents Module - Stored Procedure Tests

## Module: DOC (Documents)
## Source Files:
- `database/02-stored-procedures/012-additional-sps.sql`

## Stored Procedures Covered (8 total):
1. Policy.usp_Document_Create
2. Policy.usp_Document_GetByEntity
3. Policy.usp_Document_Delete
4. Policy.usp_Document_Search
5. Admin.usp_Template_GetAll
6. Admin.usp_Template_GetByID
7. Admin.usp_Template_Save
8. Admin.usp_Correspondence_Log
9. Admin.usp_Correspondence_GetHistory

---

### Test Case ID: SP-DOC-001
**Procedure**: Policy.usp_Document_Create
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required
- @DocumentType VARCHAR(30) - Required
- @FileName VARCHAR(200) - Required
- @FilePath VARCHAR(500) - Required
- @FileSize INT - Optional (default 0)
- @Description VARCHAR(500) - Optional (default NULL)
- @CreatedBy VARCHAR(50) - Required
- @DocumentID INT OUTPUT

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create document for policy | @EntityType='POLICY', @EntityID=1, @DocumentType='PHOTO', @FileName='photo.jpg', @FilePath='\\server\docs\photo.jpg', @CreatedBy='admin' | @DocumentID > 0, PRINT message confirms creation |
| 2 | Create document for claim | @EntityType='CLAIM', @EntityID=100, @DocumentType='ESTIMATE', @FileName='estimate.pdf', @FilePath='\\server\docs\estimate.pdf', @CreatedBy='adjuster1' | @DocumentID > 0 |
| 3 | Create with description | All params + @Description='Front of property' | Document created with description |
| 4 | Create with file size | @FileSize=245000 | FileSize stored correctly |
| 5 | Default file size | @FileSize not specified | Defaults to 0 |
| 6 | All document types | @DocumentType='PHOTO'/'ESTIMATE'/'INVOICE'/'POLICE_REPORT'/'CORRESPONDENCE'/'CONTRACT'/'INSPECTION'/'OTHER' | Each type accepted |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL EntityType | @EntityType=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 2 | NULL FileName | @FileName=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 3 | NULL CreatedBy | @CreatedBy=NULL | Error (NOT NULL constraint) [ASSUMPTION] |
| 4 | Invalid EntityID | @EntityID=0 or negative | Document created but orphaned [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | FileName at max length (200 chars) | 200 character filename | Stored correctly |
| 2 | FilePath at max length (500 chars) | 500 character path | Stored correctly |
| 3 | Description at max length (500 chars) | 500 character description | Stored correctly |
| 4 | FileSize max INT | @FileSize=2147483647 | Stored correctly |
| 5 | OUTPUT parameter initialized | @DocumentID declared | Set to 0 initially per SP code |

---

### Test Case ID: SP-DOC-002
**Procedure**: Policy.usp_Document_GetByEntity
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get documents for policy | @EntityType='POLICY', @EntityID=1 | Result set with DocumentID, EntityType, EntityID, FileName, DocumentType, UploadDate |
| 2 | Get documents for claim | @EntityType='CLAIM', @EntityID=100 | Documents attached to that claim |
| 3 | Entity with multiple documents | Entity with 5 attached docs | Returns all 5 rows |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent entity | @EntityType='POLICY', @EntityID=99999 | Empty result set |
| 2 | Invalid entity type | @EntityType='INVALID' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Entity with no documents | Valid entity, no attachments | Empty result set (WHERE 1=0 in stub) |

---

### Test Case ID: SP-DOC-003
**Procedure**: Policy.usp_Document_Delete
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @DocumentID INT - Required
- @DeletedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Delete existing document | @DocumentID=1, @DeletedBy='admin' | PRINT message confirms deletion |
| 2 | Audit trail of deletion | Valid delete | DeletedBy and deletion timestamp recorded [ASSUMPTION] |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Delete non-existent document | @DocumentID=99999 | No error (PRINT still executes) |
| 2 | NULL DeletedBy | @DeletedBy=NULL | Error or NULL stored [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Delete same document twice | @DocumentID=1 called twice | Second call no-op or error [ASSUMPTION] |

---

### Test Case ID: SP-DOC-004
**Procedure**: Policy.usp_Document_Search
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Optional (default NULL)
- @DocumentType VARCHAR(30) - Optional (default NULL)
- @DateFrom DATE - Optional (default NULL)
- @DateTo DATE - Optional (default NULL)
- @PageNumber INT - Optional (default 1)
- @PageSize INT - Optional (default 50)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Search with no filters | All NULL | Returns all documents (paginated) |
| 2 | Filter by entity type | @EntityType='POLICY' | Only policy documents |
| 3 | Filter by document type | @DocumentType='PHOTO' | Only photo documents |
| 4 | Filter by date range | @DateFrom='2024-01-01', @DateTo='2024-12-31' | Documents within date range |
| 5 | Pagination page 1 | @PageNumber=1, @PageSize=50 | First 50 results |
| 6 | Pagination page 2 | @PageNumber=2, @PageSize=50 | Results 51-100 |
| 7 | Combined filters | @EntityType='CLAIM', @DocumentType='ESTIMATE' | Both filters applied |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No matching documents | @EntityType='NONEXISTENT' | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Page beyond results | @PageNumber=999 | Empty result set |
| 2 | PageSize of 1 | @PageSize=1 | Returns single row per page |
| 3 | Default pagination | No page params | Page 1, 50 per page |

---

### Test Case ID: SP-DOC-005
**Procedure**: Admin.usp_Template_GetAll
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @Category VARCHAR(30) - Optional (default NULL)

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get all templates | @Category=NULL | Returns all 6 templates: Cancellation Notice, Claim Acknowledgment, Payment Reminder, Renewal Notice, Non-Renewal Notice, Claim Denial Letter |
| 2 | Filter by POLICY category | @Category='POLICY' | Returns Cancellation Notice, Renewal Notice, Non-Renewal Notice |
| 3 | Filter by CLAIMS category | @Category='CLAIMS' | Returns Claim Acknowledgment, Claim Denial Letter |
| 4 | Filter by BILLING category | @Category='BILLING' | Returns Payment Reminder |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent category | @Category='NONEXISTENT' | All templates returned (WHERE 1=1 always true, @Category not used in current stub) [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | NULL category returns all | @Category=NULL | All 6 templates returned |
| 2 | Result set columns | Any | TemplateID, TemplateName, Category |

---

### Test Case ID: SP-DOC-006
**Procedure**: Admin.usp_Template_GetByID
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TemplateID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get template by ID | @TemplateID=1 | Returns TemplateID=1, Body='Template Body' |
| 2 | Get different template | @TemplateID=5 | Returns TemplateID=5, Body='Template Body' |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | Non-existent template | @TemplateID=99999 | Returns row with that ID (stub returns @TemplateID directly) |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | TemplateID = 0 | @TemplateID=0 | Returns row with TemplateID=0 |

---

### Test Case ID: SP-DOC-007
**Procedure**: Admin.usp_Template_Save
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @TemplateID INT - Optional (default 0, indicates new template)
- @TemplateName VARCHAR(100) - Required
- @Category VARCHAR(30) - Required
- @Subject VARCHAR(200) - Required
- @Body VARCHAR(MAX) - Required
- @CreatedBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Create new template | @TemplateID=0, @TemplateName='Test Template', @Category='POLICY', @Subject='Test Subject', @Body='Body text', @CreatedBy='admin' | PRINT confirms save |
| 2 | Update existing template | @TemplateID=1, @TemplateName='Updated Name', ... | Template updated |
| 3 | Template with long body | @Body = large text (MAX) | Stored correctly |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL TemplateName | @TemplateName=NULL | Error [ASSUMPTION] |
| 2 | Empty Body | @Body='' | Stored as empty string |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | TemplateName max length (100) | 100 character name | Stored correctly |
| 2 | Subject max length (200) | 200 character subject | Stored correctly |
| 3 | TemplateID=0 means new | @TemplateID=0 | Creates new template (INSERT) |
| 4 | TemplateID>0 means update | @TemplateID=5 | Updates existing (UPDATE) |

---

### Test Case ID: SP-DOC-008
**Procedure**: Admin.usp_Correspondence_Log
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required
- @TemplateID INT - Optional (default NULL)
- @RecipientName VARCHAR(200) - Required
- @RecipientAddress VARCHAR(500) - Optional (default NULL)
- @Subject VARCHAR(200) - Required
- @DeliveryMethod VARCHAR(20) - Required
- @SentBy VARCHAR(50) - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Log correspondence for policy | @EntityType='POLICY', @EntityID=1, @RecipientName='John Doe', @Subject='Cancellation Notice', @DeliveryMethod='PRINT', @SentBy='admin' | PRINT confirms logging |
| 2 | Log with template reference | @TemplateID=1 plus other params | Template ID recorded |
| 3 | Log with email delivery | @DeliveryMethod='EMAIL' | Email delivery logged |
| 4 | Log with fax delivery | @DeliveryMethod='FAX' | Fax delivery logged |
| 5 | Log with recipient address | @RecipientAddress='123 Main St, City, ST 12345' | Address stored |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | NULL RecipientName | @RecipientName=NULL | Error [ASSUMPTION] |
| 2 | NULL Subject | @Subject=NULL | Error [ASSUMPTION] |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | RecipientName at max (200 chars) | 200 character name | Stored correctly |
| 2 | RecipientAddress at max (500 chars) | 500 character address | Stored correctly |
| 3 | NULL TemplateID | No template | Logged without template reference |
| 4 | DeliveryMethod values | PRINT, EMAIL, FAX | All valid delivery methods |

---

### Test Case ID: SP-DOC-009
**Procedure**: Admin.usp_Correspondence_GetHistory
**Source**: `database/02-stored-procedures/012-additional-sps.sql`
**Parameters**:
- @EntityType VARCHAR(20) - Required
- @EntityID INT - Required

#### Positive Tests
| # | Description | Input Parameters | Expected Result |
|---|-------------|------------------|-----------------|
| 1 | Get history for policy | @EntityType='POLICY', @EntityID=1 | Returns correspondence history records |
| 2 | Get history for claim | @EntityType='CLAIM', @EntityID=100 | Returns correspondence for that claim |
| 3 | Entity with multiple correspondence | Multiple logs for entity | Returns all records |

#### Negative Tests
| # | Description | Input Parameters | Expected Error |
|---|-------------|------------------|----------------|
| 1 | No correspondence history | @EntityType='POLICY', @EntityID=99999 | Empty result set |

#### Boundary Tests
| # | Description | Input Parameters | Expected Behavior |
|---|-------------|------------------|-------------------|
| 1 | Entity with no history | Valid entity, no correspondence | Empty result set (WHERE 1=0 in stub) |
