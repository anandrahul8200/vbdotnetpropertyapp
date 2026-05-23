# Documents Module - Business Rules

## Module: DOC (Documents)
## Test Type: Business Rules Catalog

---

### Rule ID: BR-DOC-001
**Module**: DOC
**Priority**: High

#### Rule Description
A file must be selected before uploading a document. The system validates that the file path text field is not empty or null before processing the upload.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- **Method**: btnUpload_Click
- **Code Snippet**:
```vb
If String.IsNullOrEmpty(txtFilePath.Text) Then
    MessageBox.Show("Please select a file.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
End If
```

#### Enforcement Mechanism
- Type: Application-level validation (String.IsNullOrEmpty check)
- Behavior: Shows warning message and returns without processing upload

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-DOC-001 | btnUpload_Click validation |
| FT-DOC-001 | Document Upload |
| US-DOC-001 | Upload Document user story |

---

### Rule ID: BR-DOC-002
**Module**: DOC
**Priority**: High

#### Rule Description
Document deletion requires explicit user confirmation. A Yes/No dialog is presented before the delete operation is executed. The deletion does not proceed if no document row is selected in the grid.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- **Method**: btnDelete_Click
- **Code Snippet**:
```vb
If dgvExistingDocs.CurrentRow Is Nothing Then Return
If MessageBox.Show("Delete this document?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
    LoadDocuments()
End If
```

#### Enforcement Mechanism
- Type: Application-level validation (null check + confirmation dialog)
- Behavior: Returns if no selection; requires Yes to proceed

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-DOC-003 | btnDelete_Click |
| FT-DOC-002 | Document Deletion |
| US-DOC-003 | Delete Documents user story |

---

### Rule ID: BR-DOC-003
**Module**: DOC
**Priority**: High

#### Rule Description
Template name is required when saving a template. The system validates that the template name field is not empty or whitespace-only before saving.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`
- **Method**: btnSave_Click
- **Code Snippet**:
```vb
If String.IsNullOrWhiteSpace(txtTemplateName.Text) Then
    MessageBox.Show("Template name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning) : Return
End If
```

#### Enforcement Mechanism
- Type: Application-level validation (String.IsNullOrWhiteSpace check)
- Behavior: Shows warning message and returns without saving

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-DOC-006 | btnSave_Click validation |
| FT-DOC-006 | Template Management |
| US-DOC-005 | Manage Templates user story |

---

### Rule ID: BR-DOC-004
**Module**: DOC
**Priority**: Medium

#### Rule Description
Template loading in the correspondence form requires a template to be selected (SelectedIndex > 0). The "(Select Template)" placeholder at index 0 is not a valid selection.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`
- **Method**: btnLoadTemplate_Click
- **Code Snippet**:
```vb
If cboTemplate.SelectedIndex <= 0 Then Return
txtSubject.Text = cboTemplate.SelectedItem.ToString()
txtBody.Text = "Dear [Insured Name]," & Environment.NewLine & Environment.NewLine & ...
```

#### Enforcement Mechanism
- Type: Application-level validation (index check)
- Behavior: Returns without action if placeholder selected

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-DOC-005 | btnLoadTemplate_Click |
| FT-DOC-004 | Correspondence Generation |
| US-DOC-004 | Generate Correspondence user story |

---

### Rule ID: BR-DOC-005
**Module**: DOC
**Priority**: Medium

#### Rule Description
Document type is limited to a predefined set of values. The upload form presents a dropdown list with exactly 8 document types. The user cannot enter arbitrary types.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- **Method**: InitializeControls
- **Code Snippet**:
```vb
cboDocumentType.Items.AddRange({"PHOTO", "ESTIMATE", "INVOICE", "POLICE_REPORT", "CORRESPONDENCE", "CONTRACT", "INSPECTION", "OTHER"})
cboDocumentType.SelectedIndex = 0
```

#### Enforcement Mechanism
- Type: UI constraint (ComboBox with DropDownStyle=DropDownList)
- Behavior: User can only select from predefined list, cannot type custom values

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UI-DOC-001 | frmDocumentUpload controls |
| DV-DOC-005 | Document Type Consistency |
| SP-DOC-001 | usp_Document_Create @DocumentType parameter |

---

### Rule ID: BR-DOC-006
**Module**: DOC
**Priority**: Medium

#### Rule Description
Delivery method for correspondence is limited to PRINT, EMAIL, or FAX. These are the only channels supported by the system for outbound communication.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`
- **Method**: InitializeControls
- **Code Snippet**:
```vb
cboDeliveryMethod.Items.AddRange({"PRINT", "EMAIL", "FAX"}) : cboDeliveryMethod.SelectedIndex = 0
```

#### Enforcement Mechanism
- Type: UI constraint (ComboBox with DropDownStyle=DropDownList)
- Behavior: Default is PRINT; user selects from 3 options only

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UI-DOC-003 | frmCorrespondence controls |
| DV-DOC-007 | Delivery Method Consistency |
| SP-DOC-008 | usp_Correspondence_Log @DeliveryMethod parameter |

---

### Rule ID: BR-DOC-007
**Module**: DOC
**Priority**: Medium

#### Rule Description
Template categories control where templates are available and how they are filtered. The supported categories are: POLICY, CLAIMS, BILLING, UNDERWRITING, and GENERAL.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`
- **Method**: InitializeControls
- **Code Snippet**:
```vb
cboCategory.Items.AddRange({"POLICY", "CLAIMS", "BILLING", "UNDERWRITING", "GENERAL"})
```
- **SP**: Admin.usp_Template_GetAll
- **Parameter**: @Category VARCHAR(30)

#### Enforcement Mechanism
- Type: UI constraint (ComboBox DropDownList) + SP parameter filtering
- Behavior: Templates grouped by category for organization and filtering

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-DOC-005 | usp_Template_GetAll with @Category filter |
| DV-DOC-006 | Template Category Consistency |
| UI-DOC-004 | frmTemplateManager |

---

### Rule ID: BR-DOC-008
**Module**: DOC
**Priority**: Medium

#### Rule Description
Template deletion requires explicit confirmation. A Yes/No dialog prevents accidental removal of templates that may be in active use for correspondence generation.

#### Source
- **File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`
- **Method**: btnDelete_Click
- **Code Snippet**:
```vb
If MessageBox.Show("Delete this template?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
    MessageBox.Show("Template deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
End If
```

#### Enforcement Mechanism
- Type: Application-level confirmation dialog
- Behavior: Only deletes when user confirms Yes

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| UT-DOC-008 | btnDelete_Click confirmation |
| FT-DOC-006 | Template Management |

---

### Rule ID: BR-DOC-009
**Module**: DOC
**Priority**: Low

#### Rule Description
Document search supports pagination with configurable page size. Default page size is 50 documents per page. This controls the volume of data returned to the UI for large document repositories.

#### Source
- **File**: `database/02-stored-procedures/012-additional-sps.sql`
- **SP**: Policy.usp_Document_Search
- **Parameters**: @PageNumber INT = 1, @PageSize INT = 50

#### Enforcement Mechanism
- Type: Database SP logic (OFFSET/FETCH or equivalent pagination)
- Behavior: Returns only the requested page of results

#### Linked Test Cases
| Test ID | Description |
|---------|-------------|
| SP-DOC-004 | usp_Document_Search pagination |
| US-DOC-006 | Search Documents user story |
