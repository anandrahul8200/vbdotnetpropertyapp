# Documents Module - UI Tests

## Module: DOC (Documents)
## Test Type: Screen-Level UI Tests
## Forms Covered:
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`
- `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

---

### Test Case ID: UI-DOC-001
**Form**: frmDocumentUpload
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Document Management - {entityType} #{entityID}" |
| Size | 700 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| lblEntityInfo | Label | AutoSize=True, Font=Bold, Text="{entityType} ID: {entityID}" |
| grpUpload | GroupBox | Text="Upload Document", Size=660x130 |
| txtFilePath | TextBox | Size=400x20, ReadOnly=True |
| btnBrowse | Button | Text="Browse...", Size=80x25 |
| cboDocumentType | ComboBox | DropDownStyle=DropDownList, Items=8 types, SelectedIndex=0 |
| txtDescription | TextBox | Size=400x20 |
| btnUpload | Button | Text="&Upload", Size=80x25 |
| grpDocs | GroupBox | Text="Attached Documents", Size=660x240 |
| dgvExistingDocs | DataGridView | ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect, AutoSizeColumnsMode=Fill |
| btnDelete | Button | Text="&Delete", Size=100x30 |
| btnClose | Button | Text="&Close", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads with entity info | Open form | lblEntityInfo shows entity type and ID in bold |
| 2 | File path read-only | Try to type | txtFilePath.ReadOnly = True |
| 3 | Document type defaults | Form loads | cboDocumentType.SelectedIndex = 0 (PHOTO) |
| 4 | Document type options | Click dropdown | 8 items: PHOTO, ESTIMATE, INVOICE, POLICE_REPORT, CORRESPONDENCE, CONTRACT, INSPECTION, OTHER |
| 5 | Browse opens file dialog | Click Browse | OpenFileDialog with filter for All Files, Images, PDFs |
| 6 | File path populated on select | Select file in dialog | txtFilePath.Text = selected file path |
| 7 | Upload validation | Click Upload with empty path | MessageBox "Please select a file." warning |
| 8 | Successful upload feedback | Valid upload | MessageBox "Document uploaded successfully." |
| 9 | Fields cleared after upload | Successful upload | txtFilePath and txtDescription cleared |
| 10 | Delete requires selection | No row selected | btnDelete_Click returns immediately |
| 11 | Delete confirmation | Row selected, click Delete | "Delete this document?" Yes/No dialog |
| 12 | Close button works | Click Close | Form closes |

---

### Test Case ID: UI-DOC-002
**Form**: frmDocumentViewer
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Document Viewer" |
| Size | 600 x 500 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| lblFileName | Label | AutoSize=True, Font=Bold, Text="Document_001.pdf" |
| lblFileType | Label | AutoSize=True, Text="Type: PDF \| Size: 245 KB" |
| lblUploadDate | Label | AutoSize=True, Text="Uploaded: 01/15/2026 by admin" |
| pnlPreview | Panel | Size=560x330, BorderStyle=FixedSingle, BackColor=White |
| btnDownload | Button | Text="&Download", Size=100x30 |
| btnPrint | Button | Text="&Print", Size=80x30 |
| btnClose | Button | Text="&Close", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads with document info | Open form | File name, type, size, upload date displayed |
| 2 | Preview panel background | Form loads | pnlPreview.BackColor = White |
| 3 | No preview message | Unsupported file type | "Preview not available for this file type." in gray |
| 4 | Download opens save dialog | Click Download | SaveFileDialog with pre-populated filename |
| 5 | Download success message | Save file | "Document saved to: {path}" |
| 6 | Print placeholder | Click Print | "Print dialog would open here." |
| 7 | Close button works | Click Close | Form closes |

---

### Test Case ID: UI-DOC-003
**Form**: frmCorrespondence
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Correspondence" |
| Size | 800 x 600 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| cboTemplate | ComboBox | DropDownStyle=DropDownList, Items=8 (including "(Select Template)"), SelectedIndex=0 |
| btnLoadTemplate | Button | Text="Load", Size=80x24 |
| cboRecipientType | ComboBox | DropDownStyle=DropDownList, Items=INSURED/AGENT/VENDOR/ATTORNEY/MORTGAGEE, SelectedIndex=0 |
| txtRecipientName | TextBox | Size=200x20 |
| txtRecipientAddress | TextBox | Size=400x20 |
| txtSubject | TextBox | Size=500x20 |
| txtBody | TextBox | Size=640x100, Multiline=True, ScrollBars=Vertical |
| cboDeliveryMethod | ComboBox | DropDownStyle=DropDownList, Items=PRINT/EMAIL/FAX, SelectedIndex=0 |
| btnPreview | Button | Text="Pre&view", Size=80x25 |
| btnSend | Button | Text="&Send", Size=80x25 |
| btnSaveDraft | Button | Text="Save &Draft", Size=90x25 |
| dgvHistory | DataGridView | ReadOnly=True, AllowUserToAddRows=False, AutoSizeColumnsMode=Fill |
| btnClose | Button | Text="&Close", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads with compose and history sections | Open form | grpCompose and grpHistory GroupBoxes visible |
| 2 | Template dropdown defaults | Form loads | "(Select Template)" selected |
| 3 | Template options | Click dropdown | 7 templates plus "(Select Template)" |
| 4 | Load does nothing without template | SelectedIndex=0, click Load | Returns immediately (no change) |
| 5 | Load populates subject and body | Select template, click Load | txtSubject = template name, txtBody = letter text |
| 6 | Body is multiline | Form loads | txtBody.Multiline = True with vertical scrollbar |
| 7 | Recipient type defaults | Form loads | "INSURED" selected |
| 8 | Delivery method defaults | Form loads | "PRINT" selected |
| 9 | Preview shows placeholder | Click Preview | "Letter preview would display here." |
| 10 | Send shows confirmation | Click Send | "Correspondence sent/printed." |
| 11 | Save Draft shows confirmation | Click Save Draft | "Draft saved." |
| 12 | History grid read-only | Inspect dgvHistory | ReadOnly = True |
| 13 | Close button works | Click Close | Form closes |

---

### Test Case ID: UI-DOC-004
**Form**: frmTemplateManager
**File**: `src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb`

#### Form Properties
| Property | Expected Value |
|----------|---------------|
| Title | "Template Manager" |
| Size | 800 x 550 |
| StartPosition | CenterParent |

#### Controls
| Control | Type | Properties |
|---------|------|------------|
| dgvTemplates | DataGridView | Size=250x440, ReadOnly=True, AllowUserToAddRows=False, SelectionMode=FullRowSelect |
| txtTemplateName | TextBox | Size=300x20 |
| cboCategory | ComboBox | DropDownStyle=DropDownList, Items=POLICY/CLAIMS/BILLING/UNDERWRITING/GENERAL, SelectedIndex=0 |
| txtSubjectTemplate | TextBox | Size=380x20 |
| txtBodyTemplate | TextBox | Size=380x250, Multiline=True, ScrollBars=Vertical |
| btnNew | Button | Text="&New", Size=80x30 |
| btnSave | Button | Text="&Save", Size=80x30 |
| btnDelete | Button | Text="&Delete", Size=80x30 |
| btnClose | Button | Text="&Close", Size=80x30 |

#### UI Tests
| # | Test | Action | Expected |
|---|------|--------|----------|
| 1 | Form loads with templates | Open form | dgvTemplates has 7 pre-loaded rows |
| 2 | Template list columns | Form loads | "Name" and "Category" columns |
| 3 | Pre-loaded templates | Form loads | Cancellation Notice (POLICY), Renewal Reminder (POLICY), Claim Acknowledgment (CLAIMS), Payment Reminder (BILLING), Claim Denial (CLAIMS), Welcome Letter (POLICY), Non-Renewal Notice (POLICY) |
| 4 | Variable hint shown | Form loads | Gray label: "Variables: [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount]" |
| 5 | New clears all fields | Click New | txtTemplateName, txtSubjectTemplate, txtBodyTemplate cleared, focus on name |
| 6 | Save validation | Click Save with empty name | "Template name is required." warning |
| 7 | Successful save | Enter name and click Save | "Template saved." |
| 8 | Delete confirmation | Click Delete | "Delete this template?" Yes/No |
| 9 | Confirmed delete | Click Yes | "Template deleted." |
| 10 | Category defaults | Form loads | cboCategory.SelectedIndex = 0 (POLICY) |
| 11 | Body supports multiline | Inspect txtBodyTemplate | Multiline=True with vertical scrollbar |
| 12 | Close button works | Click Close | Form closes |
