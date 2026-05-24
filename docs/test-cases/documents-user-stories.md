# Documents Module - User Stories

## Module: DOC (Documents)
## Test Type: User Story Acceptance Criteria

---

### User Story ID: US-DOC-001
**Title**: Upload Document to Policy or Claim
**As a** claims adjuster or underwriter
**I want to** attach documents (photos, estimates, reports) to a policy or claim
**So that** all relevant documentation is stored with the entity for reference

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Upload form shows the entity type and ID | lblEntityInfo displays "{entityType} ID: {entityID}" |
| 2 | I can browse for files on my computer | OpenFileDialog with All Files, Images, PDFs filter |
| 3 | I must select a document type from predefined list | cboDocumentType with 8 options, required selection |
| 4 | I can optionally add a description | txtDescription accepts text, not required |
| 5 | Upload validates that a file is selected | Error "Please select a file." if path empty |
| 6 | Successful upload confirms and clears the form | "Document uploaded successfully." message, fields cleared |
| 7 | Document list refreshes after upload | LoadDocuments() called to show new attachment |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-001 | Document Upload |
| UI-DOC-001 | frmDocumentUpload |
| UT-DOC-001 | btnUpload_Click validation |
| SP-DOC-001 | usp_Document_Create |

---

### User Story ID: US-DOC-002
**Title**: View and Download Documents
**As a** claims adjuster or policyholder service representative
**I want to** view and download documents attached to a policy or claim
**So that** I can review documentation without searching through physical files

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | Document viewer shows file name, type, size, and upload date | Labels display document metadata |
| 2 | Preview panel shows content when possible | pnlPreview displays supported file types |
| 3 | Unsupported files show "Preview not available" message | Gray text in preview panel |
| 4 | I can download the document to my computer | SaveFileDialog with pre-populated filename |
| 5 | Download confirmation shows saved path | "Document saved to: {path}" message |
| 6 | I can print the document | Print button available |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-003 | Document Viewing |
| UI-DOC-002 | frmDocumentViewer |
| UT-DOC-004 | btnDownload_Click |

---

### User Story ID: US-DOC-003
**Title**: Delete Documents
**As a** document manager
**I want to** delete documents that are no longer needed or were uploaded in error
**So that** the document repository stays clean and accurate

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I must select a document before deleting | Delete returns if no row selected |
| 2 | System confirms before deletion | "Delete this document?" with Yes/No |
| 3 | Deletion records who performed it | @DeletedBy parameter captures username |
| 4 | Document list refreshes after deletion | LoadDocuments() called |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-002 | Document Deletion |
| UT-DOC-003 | btnDelete_Click |
| SP-DOC-003 | usp_Document_Delete |

---

### User Story ID: US-DOC-004
**Title**: Generate Correspondence from Template
**As a** customer service representative
**I want to** generate letters and notices using pre-built templates
**So that** correspondence is consistent and professional across the organization

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can select from available templates | cboTemplate with 7 template options |
| 2 | Loading a template populates the subject and body | txtSubject and txtBody filled with template content |
| 3 | Template body includes greeting and sign-off | "Dear [Insured Name]," and "Sincerely, Insurance Company" |
| 4 | I can select recipient type | INSURED, AGENT, VENDOR, ATTORNEY, MORTGAGEE options |
| 5 | I can enter recipient name and address | Free-text fields for recipient details |
| 6 | I can choose delivery method | PRINT, EMAIL, or FAX |
| 7 | Sending confirms success | "Correspondence sent/printed." message |
| 8 | I can save as draft | "Draft saved." confirmation |
| 9 | I can preview before sending | Preview available via button |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-004 | Correspondence Generation from Template |
| FT-DOC-005 | Template Preview and Draft Save |
| UI-DOC-003 | frmCorrespondence |
| UT-DOC-005 | btnLoadTemplate_Click |
| SP-DOC-008 | usp_Correspondence_Log |

---

### User Story ID: US-DOC-005
**Title**: Manage Correspondence Templates
**As a** system administrator
**I want to** create, edit, and delete correspondence templates
**So that** the organization has up-to-date letter templates for common communications

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can see a list of existing templates with categories | dgvTemplates shows Name and Category columns |
| 2 | I can create a new template | New button clears fields for entry |
| 3 | Template name is required | Validation: "Template name is required." |
| 4 | I can assign a category | POLICY, CLAIMS, BILLING, UNDERWRITING, GENERAL |
| 5 | I can define subject and body text | Free-text fields for template content |
| 6 | Available merge variables are shown | Hint: [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount] |
| 7 | I can delete templates with confirmation | "Delete this template?" confirmation |
| 8 | Save confirms success | "Template saved." message |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-006 | Template Management |
| UI-DOC-004 | frmTemplateManager |
| UT-DOC-006 | btnSave_Click |
| UT-DOC-007 | btnNew_Click |
| SP-DOC-005 | usp_Template_GetAll |
| SP-DOC-007 | usp_Template_Save |

---

### User Story ID: US-DOC-006
**Title**: Search Documents
**As a** claims adjuster
**I want to** search for documents across entities using filters
**So that** I can find specific documentation quickly without opening each entity

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | I can filter by entity type (POLICY or CLAIM) | @EntityType parameter |
| 2 | I can filter by document type | @DocumentType parameter |
| 3 | I can filter by date range | @DateFrom and @DateTo parameters |
| 4 | Results are paginated | @PageNumber and @PageSize control output |
| 5 | Default page size is 50 | @PageSize defaults to 50 |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-008 | Document Search |
| SP-DOC-004 | usp_Document_Search |
| NFR-DOC-001 | Search performance |

---

### User Story ID: US-DOC-007
**Title**: View Correspondence History
**As a** customer service representative
**I want to** see a history of all correspondence sent for a policy or claim
**So that** I know what communications have already been sent to avoid duplicates

#### Acceptance Criteria
| # | Criterion | Verification |
|---|-----------|-------------|
| 1 | History shows all past correspondence for the entity | Admin.usp_Correspondence_GetHistory returns records |
| 2 | Each record shows template used, recipient, delivery method, and date | dgvHistory displays relevant columns [ASSUMPTION] |
| 3 | History displayed in the Correspondence form | grpHistory section of frmCorrespondence |

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| FT-DOC-007 | Correspondence History |
| SP-DOC-009 | usp_Correspondence_GetHistory |
