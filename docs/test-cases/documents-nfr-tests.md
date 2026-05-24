# Documents Module - Non-Functional Requirements Tests

## Module: DOC (Documents)
## Test Type: Performance, Security, and Reliability Tests

---

### Test Case ID: NFR-DOC-001
**Category**: Performance
**Component**: Document Search (Policy.usp_Document_Search)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time (< 1000 documents) | < 1 second | Paginated query with filters |
| Response time (> 50000 documents) | < 3 seconds | Large document repository |
| Pagination efficiency | Consistent | Page 1 and page 100 similar response times |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Small document set | 500 documents | < 500ms |
| 2 | Large document set | 50000 documents | < 3s with pagination |
| 3 | Filter by entity type | 50000 documents, filter POLICY | < 1s |
| 4 | Filter by document type | 50000 documents, filter PHOTO | < 1s |
| 5 | Date range filter | 50000 documents, 1-year range | < 2s |
| 6 | Combined filters | Multiple filters applied | < 1s |

---

### Test Case ID: NFR-DOC-002
**Category**: Performance
**Component**: Document Retrieval (Policy.usp_Document_GetByEntity)
**Priority**: High

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Response time | < 500ms | Retrieve documents for one entity |
| Entity with many documents | < 1 second | Entity with 100+ attachments |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Entity with few documents | 3 attachments | < 200ms |
| 2 | Entity with many documents | 100 attachments | < 500ms |
| 3 | Entity with no documents | No attachments | < 100ms (empty result) |

---

### Test Case ID: NFR-DOC-003
**Category**: Performance
**Component**: Template Operations
**Priority**: Medium

#### Performance Criteria
| Metric | Target | Measurement |
|--------|--------|-------------|
| Template GetAll | < 500ms | Load all templates |
| Template GetByID | < 200ms | Single template retrieval |
| Template Save | < 500ms | Insert or update |

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Load all templates | 50 templates | < 500ms |
| 2 | Load single template with large body | MAX body text | < 200ms |
| 3 | Save new template | Large body text | < 500ms |

---

### Test Case ID: NFR-DOC-004
**Category**: Performance
**Component**: Correspondence History
**Priority**: Medium

#### Test Scenarios
| # | Scenario | Dataset | Expected |
|---|----------|---------|----------|
| 1 | Entity with few correspondence | 5 records | < 200ms |
| 2 | Entity with extensive history | 500 records | < 1s |
| 3 | Logging new correspondence | Single insert | < 300ms |

---

### Test Case ID: NFR-DOC-005
**Category**: Security
**Component**: Document Upload and Storage
**Priority**: Critical

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | File type validation | Only allowed extensions accepted [ASSUMPTION] |
| 2 | File size limit enforcement | Maximum file size configured and enforced [ASSUMPTION] |
| 3 | Path traversal in file name | Sanitized before storage, no ../ allowed |
| 4 | SQL injection in description | Parameterized query prevents injection |
| 5 | SQL injection in file name | Parameterized query prevents injection |
| 6 | Unauthorized document access | User cannot view documents without permission [ASSUMPTION] |
| 7 | Document deletion audit trail | DeletedBy recorded for accountability |
| 8 | CreatedBy cannot be spoofed | System populates from authenticated user context [ASSUMPTION] |

---

### Test Case ID: NFR-DOC-006
**Category**: Security
**Component**: Template Management
**Priority**: High

#### Security Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Only admin can manage templates | Role-based access enforced [ASSUMPTION] |
| 2 | SQL injection in template body | VARCHAR(MAX) parameter prevents injection |
| 3 | XSS in template body | Content sanitized before rendering [ASSUMPTION] |
| 4 | Template variables validated | Only known variables expanded [ASSUMPTION] |

---

### Test Case ID: NFR-DOC-007
**Category**: Reliability
**Component**: Document Upload Resilience
**Priority**: High

#### Reliability Tests
| # | Test | Expected |
|---|------|----------|
| 1 | Upload during database outage | Error caught, user notified, no partial records |
| 2 | Large file upload timeout | Timeout handled gracefully |
| 3 | Concurrent uploads to same entity | No race conditions, both stored correctly |
| 4 | File system full | Appropriate error message |
| 5 | Network file path unavailable | Error message shown to user |

---

### Test Case ID: NFR-DOC-008
**Category**: Scalability
**Component**: Document Storage
**Priority**: Medium

#### Scalability Tests
| # | Test | Dataset | Expected |
|---|------|---------|----------|
| 1 | Total document capacity | 1 million documents | Search still performs within targets |
| 2 | Documents per entity | 500 documents on one claim | GetByEntity still < 1s |
| 3 | Template count growth | 200 templates | GetAll still < 1s |
| 4 | Correspondence history depth | 1000 records per entity | GetHistory still < 2s |
