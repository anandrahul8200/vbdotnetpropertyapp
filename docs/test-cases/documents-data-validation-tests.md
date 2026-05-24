# Documents Module - Data Validation Tests

## Module: DOC (Documents)
## Test Type: Data Integrity and Validation Tests
## Data Sources:
- Document storage table (Policy.Documents or similar) [ASSUMPTION]
- Template storage (Admin.Templates or similar) [ASSUMPTION]
- Correspondence log table [ASSUMPTION]

---

### Test Case ID: DV-DOC-001
**Component**: Document Create Parameters
**Source**: Policy.usp_Document_Create

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | EntityType valid values | 'POLICY', 'CLAIM' | Accepted |
| 2 | EntityType max length | VARCHAR(20) | Max 20 characters |
| 3 | EntityID is positive integer | @EntityID > 0 | Valid entity reference |
| 4 | DocumentType valid values | PHOTO, ESTIMATE, INVOICE, POLICE_REPORT, CORRESPONDENCE, CONTRACT, INSPECTION, OTHER | All accepted |
| 5 | DocumentType max length | VARCHAR(30) | Max 30 characters |
| 6 | FileName max length | VARCHAR(200) | Max 200 characters |
| 7 | FileName contains valid characters | No null bytes or control chars | Sanitized filename [ASSUMPTION] |
| 8 | FilePath max length | VARCHAR(500) | Max 500 characters |
| 9 | FilePath format | UNC or local path | Valid file system path |
| 10 | FileSize non-negative | INT >= 0 | Default 0, accepts positive values |
| 11 | Description nullable | NULL or VARCHAR(500) | Both accepted |
| 12 | CreatedBy max length | VARCHAR(50) | Max 50 characters |
| 13 | OUTPUT DocumentID | INT | Returns generated ID |

---

### Test Case ID: DV-DOC-002
**Component**: Document Search Parameters
**Source**: Policy.usp_Document_Search

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | All NULL parameters | No filters | Returns all documents (paginated) |
| 2 | EntityType filter | VARCHAR(20) or NULL | Filters or returns all |
| 3 | DocumentType filter | VARCHAR(30) or NULL | Filters or returns all |
| 4 | DateFrom/DateTo range | Valid DATE values | Documents within range |
| 5 | DateFrom after DateTo | Invalid range | Empty result or no error [ASSUMPTION] |
| 6 | PageNumber minimum | @PageNumber = 1 | First page of results |
| 7 | PageNumber = 0 | Invalid page | Behavior undefined [ASSUMPTION] |
| 8 | PageSize minimum | @PageSize = 1 | Single result per page |
| 9 | PageSize maximum | @PageSize = 1000 | Large page returned [ASSUMPTION] |
| 10 | PageSize default | Not specified | 50 results per page |

---

### Test Case ID: DV-DOC-003
**Component**: Template Save Parameters
**Source**: Admin.usp_Template_Save

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | TemplateID = 0 means new | @TemplateID = 0 | INSERT operation |
| 2 | TemplateID > 0 means update | @TemplateID = 5 | UPDATE operation |
| 3 | TemplateName max length | VARCHAR(100) | Max 100 characters |
| 4 | TemplateName NOT NULL | Required field | Cannot be NULL |
| 5 | Category valid values | POLICY, CLAIMS, BILLING, UNDERWRITING, GENERAL | All accepted |
| 6 | Category max length | VARCHAR(30) | Max 30 characters |
| 7 | Subject max length | VARCHAR(200) | Max 200 characters |
| 8 | Body is VARCHAR(MAX) | Large text content | Accepts very long templates |
| 9 | CreatedBy max length | VARCHAR(50) | Max 50 characters |

---

### Test Case ID: DV-DOC-004
**Component**: Correspondence Log Parameters
**Source**: Admin.usp_Correspondence_Log

#### Parameter Validation Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | EntityType valid values | 'POLICY', 'CLAIM' | Accepted |
| 2 | EntityType max length | VARCHAR(20) | Max 20 characters |
| 3 | EntityID positive integer | @EntityID > 0 | Valid reference |
| 4 | TemplateID nullable | INT or NULL | Both accepted |
| 5 | RecipientName max length | VARCHAR(200) | Max 200 characters |
| 6 | RecipientName NOT NULL | Required field | Cannot be NULL [ASSUMPTION] |
| 7 | RecipientAddress nullable | VARCHAR(500) or NULL | Both accepted |
| 8 | Subject max length | VARCHAR(200) | Max 200 characters |
| 9 | DeliveryMethod valid values | PRINT, EMAIL, FAX | All accepted |
| 10 | DeliveryMethod max length | VARCHAR(20) | Max 20 characters |
| 11 | SentBy max length | VARCHAR(50) | Max 50 characters |

---

### Test Case ID: DV-DOC-005
**Component**: Document Type Consistency
**Source**: frmDocumentUpload ComboBox items

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | UI types match SP types | ComboBox items vs @DocumentType | PHOTO, ESTIMATE, INVOICE, POLICE_REPORT, CORRESPONDENCE, CONTRACT, INSPECTION, OTHER |
| 2 | No orphaned documents | EntityType + EntityID | All references point to existing entities [ASSUMPTION] |
| 3 | File path accessibility | Stored FilePath | File exists at recorded path [ASSUMPTION] |
| 4 | FileSize matches actual | @FileSize parameter | Matches actual file size on disk [ASSUMPTION] |

---

### Test Case ID: DV-DOC-006
**Component**: Template Category Consistency
**Source**: frmTemplateManager and frmCorrespondence

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | Template categories match | Manager vs SP categories | POLICY, CLAIMS, BILLING, UNDERWRITING, GENERAL |
| 2 | Template names in correspondence match saved templates | Correspondence dropdown | Items match Admin.usp_Template_GetAll results |
| 3 | TemplateID references valid | Correspondence_Log @TemplateID | Points to existing template |
| 4 | Template variables consistent | Body contains known variables | [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount] |

---

### Test Case ID: DV-DOC-007
**Component**: Correspondence Delivery Method Consistency
**Source**: frmCorrespondence and Admin.usp_Correspondence_Log

#### Data Consistency Tests
| # | Test | Input | Expected |
|---|------|-------|----------|
| 1 | UI delivery methods match SP | ComboBox items vs @DeliveryMethod | PRINT, EMAIL, FAX |
| 2 | Recipient type values | cboRecipientType items | INSURED, AGENT, VENDOR, ATTORNEY, MORTGAGEE |
| 3 | Subject not empty after template load | Template loaded | Subject populated from template name |
| 4 | SentBy matches authenticated user | System populated | Matches GlobalState.CurrentUser [ASSUMPTION] |
