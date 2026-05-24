# Documents Module - Functional Tests

## Module: DOC (Documents)
## Test Type: End-to-End Functional Tests
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

---

### Test Case ID: FT-DOC-001
**Feature**: Document Upload
**Priority**: High

#### Preconditions
- User is logged in with document management permissions
- frmDocumentUpload opened for a specific entity (POLICY or CLAIM)

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmDocumentUpload with entityType and entityID | Form displays with title "Document Management - {entityType} #{entityID}" |
| 2 | Click Browse button | OpenFileDialog opens with filter "All Files|*.*\|Images\|*.jpg;*.png\|PDFs\|*.pdf" |
| 3 | Select a file | txtFilePath populated with selected file path |
| 4 | Select document type from dropdown | cboDocumentType selection (PHOTO, ESTIMATE, INVOICE, POLICE_REPORT, CORRESPONDENCE, CONTRACT, INSPECTION, OTHER) |
| 5 | Enter description | txtDescription accepts text |
| 6 | Click Upload | MessageBox shows "Document uploaded successfully." |
| 7 | Fields cleared after upload | txtFilePath and txtDescription cleared |
| 8 | Document list refreshed | LoadDocuments() called to refresh grid |

#### Validation Rules
- File path cannot be empty (error: "Please select a file.")
- Document type defaults to "PHOTO" (SelectedIndex = 0)
- Description is optional

---

### Test Case ID: FT-DOC-002
**Feature**: Document Deletion
**Priority**: High

#### Preconditions
- frmDocumentUpload open with existing documents displayed in grid
- A document row is selected in dgvExistingDocs

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Select a document in the grid | Row highlighted (FullRowSelect mode) |
| 2 | Click Delete button | Confirmation dialog: "Delete this document?" with Yes/No |
| 3 | Click Yes | Document deleted, LoadDocuments() refreshes grid |
| 4 | Click No | No deletion occurs |

#### Validation Rules
- Delete does nothing if no row selected (dgvExistingDocs.CurrentRow Is Nothing)
- Confirmation required before deletion (MessageBoxButtons.YesNo)

---

### Test Case ID: FT-DOC-003
**Feature**: Document Viewing
**Priority**: High

#### Preconditions
- frmDocumentViewer opened with a valid documentID

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmDocumentViewer | Form displays "Document Viewer" title, 600x500 size |
| 2 | Document info displayed | File name, type, size, upload date shown in labels |
| 3 | Preview panel shown | pnlPreview with white background, "Preview not available for this file type." for unsupported types |
| 4 | Click Download | SaveFileDialog opens with filename pre-populated |
| 5 | Confirm save location | MessageBox shows "Document saved to: {path}" |
| 6 | Click Print | MessageBox shows "Print dialog would open here." |
| 7 | Click Close | Form closes |

---

### Test Case ID: FT-DOC-004
**Feature**: Correspondence Generation from Template
**Priority**: High

#### Preconditions
- frmCorrespondence is open
- Templates are available in the system

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmCorrespondence | Form displays with Compose and History sections |
| 2 | Select template from dropdown | Options: (Select Template), Cancellation Notice, Renewal Reminder, Claim Acknowledgment, Payment Reminder, Policy Welcome Letter, Non-Renewal Notice, Claim Denial Letter |
| 3 | Click Load button | txtSubject populated with template name, txtBody populated with template body text |
| 4 | Select recipient type | cboRecipientType: INSURED, AGENT, VENDOR, ATTORNEY, MORTGAGEE |
| 5 | Enter recipient name and address | txtRecipientName and txtRecipientAddress accept input |
| 6 | Select delivery method | cboDeliveryMethod: PRINT, EMAIL, FAX |
| 7 | Click Send | MessageBox shows "Correspondence sent/printed." |

#### Validation Rules
- Template must be selected (SelectedIndex > 0) before Load fires
- Body includes "Dear [Insured Name]," and template-specific content
- Delivery defaults to "PRINT" (SelectedIndex = 0)

---

### Test Case ID: FT-DOC-005
**Feature**: Template Preview and Draft Save
**Priority**: Medium

#### Preconditions
- frmCorrespondence is open
- Template loaded and recipient information entered

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Click Preview | MessageBox shows "Letter preview would display here." |
| 2 | Click Save Draft | MessageBox shows "Draft saved." |

---

### Test Case ID: FT-DOC-006
**Feature**: Template Management
**Priority**: High

#### Preconditions
- User has admin permissions
- frmTemplateManager is open

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmTemplateManager | Form displays template list (7 pre-loaded) and editor panel |
| 2 | View template list | dgvTemplates shows Name and Category columns with existing templates |
| 3 | Click New button | txtTemplateName, txtSubjectTemplate, txtBodyTemplate cleared, focus on name field |
| 4 | Enter template name, category, subject, body | All fields accept input |
| 5 | Click Save | MessageBox shows "Template saved." |
| 6 | Click Delete on selected template | Confirmation: "Delete this template?" |
| 7 | Confirm deletion | MessageBox shows "Template deleted." |

#### Validation Rules
- Template name is required (error: "Template name is required.")
- Categories: POLICY, CLAIMS, BILLING, UNDERWRITING, GENERAL
- Available variables: [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount]

---

### Test Case ID: FT-DOC-007
**Feature**: Correspondence History
**Priority**: Medium

#### Preconditions
- frmCorrespondence is open
- Previous correspondence has been sent for the entity

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Open frmCorrespondence | History section shows dgvHistory grid |
| 2 | History grid displays | ReadOnly grid with AutoSizeColumnsMode=Fill |
| 3 | Past correspondence listed | Template used, recipient, delivery method, date shown [ASSUMPTION] |

---

### Test Case ID: FT-DOC-008
**Feature**: Document Search
**Priority**: Medium

#### Preconditions
- Documents exist in the system across multiple entities

#### Steps
| Step | Action | Expected Result |
|------|--------|-----------------|
| 1 | Call Policy.usp_Document_Search | Returns matching documents |
| 2 | Filter by entity type | Only POLICY or CLAIM documents returned |
| 3 | Filter by document type | Only specified type (PHOTO, ESTIMATE, etc.) returned |
| 4 | Filter by date range | Documents within DateFrom-DateTo |
| 5 | Navigate pages | PageNumber and PageSize control results |
