# Documents Module - Unit Tests

## Module: DOC (Documents)
## Test Type: Unit Tests for Document Operations
## Classes Covered:
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

---

### Test Case ID: UT-DOC-001
**Class**: frmDocumentUpload
**Method**: btnUpload_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`

#### Method Behavior
- Validates that txtFilePath.Text is not empty
- If empty, shows warning "Please select a file." and returns
- If valid, shows success "Document uploaded successfully."
- Clears txtFilePath and txtDescription
- Calls LoadDocuments() to refresh grid

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Validation: empty file path | txtFilePath.Text = "" | MessageBox "Please select a file.", returns |
| 2 | Validation: null file path | txtFilePath.Text = Nothing | String.IsNullOrEmpty = True, shows warning |
| 3 | Successful upload | txtFilePath.Text = "C:\file.pdf" | MessageBox "Document uploaded successfully." |
| 4 | Fields cleared after upload | Valid file path | txtFilePath.Clear() and txtDescription.Clear() called |
| 5 | Grid refreshed after upload | Valid upload | LoadDocuments() called |

---

### Test Case ID: UT-DOC-002
**Class**: frmDocumentUpload
**Method**: btnBrowse_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`

#### Method Behavior
- Creates OpenFileDialog with filter "All Files|*.*|Images|*.jpg;*.png|PDFs|*.pdf"
- Title = "Select Document"
- If DialogResult.OK, sets txtFilePath.Text to selected filename

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Dialog created with correct filter | Method called | Filter includes All Files, Images, PDFs |
| 2 | Dialog title | Method called | Title = "Select Document" |
| 3 | File path set on OK | DialogResult.OK with "test.pdf" | txtFilePath.Text = "test.pdf" |
| 4 | No change on Cancel | DialogResult.Cancel | txtFilePath.Text unchanged |

---

### Test Case ID: UT-DOC-003
**Class**: frmDocumentUpload
**Method**: btnDelete_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`

#### Method Behavior
- Returns immediately if dgvExistingDocs.CurrentRow Is Nothing
- Shows confirmation "Delete this document?" with YesNo buttons
- If Yes, calls LoadDocuments()

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No row selected | CurrentRow = Nothing | Returns immediately, no dialog |
| 2 | Confirmation shown | Row selected | MessageBox with "Delete this document?" |
| 3 | Yes confirmed | DialogResult.Yes | LoadDocuments() called |
| 4 | No confirmed | DialogResult.No | Nothing happens |

---

### Test Case ID: UT-DOC-004
**Class**: frmDocumentViewer
**Method**: btnDownload_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb`

#### Method Behavior
- Creates SaveFileDialog with FileName pre-set and Filter "All Files|*.*"
- If DialogResult.OK, shows "Document saved to: {path}"

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Save dialog pre-populates filename | Method called | FileName = "Document_001.pdf" |
| 2 | Filter is All Files | Method called | Filter = "All Files\|*.*" |
| 3 | Success message on save | DialogResult.OK, path="C:\output.pdf" | "Document saved to: C:\output.pdf" |
| 4 | No action on cancel | DialogResult.Cancel | No message shown |

---

### Test Case ID: UT-DOC-005
**Class**: frmCorrespondence
**Method**: btnLoadTemplate_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`

#### Method Behavior
- Returns if cboTemplate.SelectedIndex <= 0
- Sets txtSubject.Text to cboTemplate.SelectedItem.ToString()
- Sets txtBody.Text to letter template with "Dear [Insured Name]," and template-specific content

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | No action if no template selected | SelectedIndex = 0 | Returns immediately |
| 2 | Subject set to template name | SelectedIndex = 1 ("Cancellation Notice") | txtSubject.Text = "Cancellation Notice" |
| 3 | Body contains greeting | Any template loaded | Contains "Dear [Insured Name]," |
| 4 | Body contains template reference | "Renewal Reminder" selected | Contains "renewal reminder" (lowercase) |
| 5 | Body contains sign-off | Any template | Contains "Sincerely," and "Insurance Company" |

---

### Test Case ID: UT-DOC-006
**Class**: frmTemplateManager
**Method**: btnSave_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

#### Method Behavior
- Validates txtTemplateName.Text is not empty/whitespace
- If invalid, shows "Template name is required." and returns
- If valid, shows "Template saved."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Validation: empty name | txtTemplateName.Text = "" | "Template name is required." warning |
| 2 | Validation: whitespace only | txtTemplateName.Text = "   " | IsNullOrWhiteSpace = True, warning shown |
| 3 | Successful save | txtTemplateName.Text = "New Template" | "Template saved." |

---

### Test Case ID: UT-DOC-007
**Class**: frmTemplateManager
**Method**: btnNew_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

#### Method Behavior
- Clears txtTemplateName, txtSubjectTemplate, txtBodyTemplate
- Sets focus to txtTemplateName

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | All fields cleared | Fields have content | All three TextBoxes cleared |
| 2 | Focus set to name | Method called | txtTemplateName.Focus() called |

---

### Test Case ID: UT-DOC-008
**Class**: frmTemplateManager
**Method**: btnDelete_Click
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

#### Method Behavior
- Shows confirmation "Delete this template?" with YesNo
- If Yes, shows "Template deleted."

#### Tests
| # | Test | Setup | Expected Result |
|---|------|-------|-----------------|
| 1 | Confirmation shown | Click Delete | "Delete this template?" YesNo dialog |
| 2 | Yes deletes template | DialogResult.Yes | "Template deleted." |
| 3 | No cancels | DialogResult.No | No deletion message shown |
