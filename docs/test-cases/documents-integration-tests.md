# Documents Module - Integration Tests (Future-State)

## Module: DOC (Documents)
## Test Type: Future-State Integration Tests

> **STATUS: FUTURE-STATE** - These tests are for the modernized architecture.
> The legacy application uses tightly coupled ADO.NET data access via DatabaseHelper.
> These tests define integration boundaries for the target-state microservices architecture.

---

### Test Case ID: INT-DOC-001
**Integration**: Document Service -> File Storage
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Document Component | Storage Component | Data Flow |
|-------------------|-------------------|-----------|
| Document Create | File system / blob storage | File binary stored |
| Document GetByEntity | File system / blob storage | File path resolved to downloadable URL |
| Document Delete | File system / blob storage | Physical file removed |
| Document Search | Database (metadata only) | File content not searched |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Upload stores file to blob | Document service | File storage | File accessible at stored path |
| 2 | Download retrieves correct file | Document service | File storage | Matching file returned |
| 3 | Delete removes physical file | Document service | File storage | File no longer accessible |
| 4 | Storage unavailable on upload | Document service | File storage (down) | 503 error, metadata not created |
| 5 | Storage unavailable on download | Document service | File storage (down) | 503 error, metadata still intact |
| 6 | Orphaned metadata cleanup | Document service | File storage | Periodic job removes orphaned records [ASSUMPTION] |

---

### Test Case ID: INT-DOC-002
**Integration**: Document Service -> Policy/Claims Service
**Priority**: High
**Status**: FUTURE-STATE

#### Integration Points
| Document Component | Entity Component | Data Flow |
|-------------------|-----------------|-----------|
| Document Create (EntityType=POLICY) | Policy service | Validates policy exists |
| Document Create (EntityType=CLAIM) | Claims service | Validates claim exists |
| Document GetByEntity | Policy/Claims service | Entity context for display |
| Document Delete | Policy/Claims service | Check no active references |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Create document validates entity exists | Document service | Policy service | 404 if entity not found |
| 2 | Document linked to policy | Document service | Policy service | PolicyID validated before attachment |
| 3 | Document linked to claim | Document service | Claims service | ClaimID validated before attachment |
| 4 | Entity service unavailable | Document service | Policy service (down) | Upload fails gracefully, retry suggested |
| 5 | Entity deleted with documents | Policy/Claims service | Document service | Documents orphaned or cascade deleted [ASSUMPTION] |

---

### Test Case ID: INT-DOC-003
**Integration**: Correspondence Service -> Template Service
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Correspondence Component | Template Component | Data Flow |
|-------------------------|-------------------|-----------|
| Load template | Template_GetByID | Template body retrieved |
| Merge variables | Template engine | Placeholders replaced with entity data |
| Send correspondence | Delivery service | PRINT/EMAIL/FAX routing |
| Log correspondence | Correspondence_Log | Record of sent communication |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Template loaded for correspondence | Correspondence service | Template service | Template body and subject returned |
| 2 | Variable merging | Correspondence service | Template engine | [InsuredName], [PolicyNumber], [ClaimNumber], [Date], [Amount] replaced |
| 3 | Template not found | Correspondence service | Template service | 404 error, correspondence not sent |
| 4 | Template service unavailable | Correspondence service | Template service (down) | Error message, manual entry fallback |
| 5 | Correspondence logged after send | Correspondence service | Correspondence log | Full audit trail of sent items |

---

### Test Case ID: INT-DOC-004
**Integration**: Correspondence Service -> Delivery Channel
**Priority**: Medium
**Status**: FUTURE-STATE

#### Integration Points
| Correspondence Component | Delivery Component | Data Flow |
|-------------------------|-------------------|-----------|
| PRINT delivery | Print queue service | Document queued for printing |
| EMAIL delivery | Email service (SMTP) | Email sent to recipient |
| FAX delivery | Fax gateway | Document faxed to number |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | Print delivery queues document | Correspondence service | Print service | Document in print queue |
| 2 | Email delivery sends message | Correspondence service | SMTP service | Email delivered to recipient |
| 3 | Fax delivery transmits | Correspondence service | Fax gateway | Fax confirmation received |
| 4 | Email service unavailable | Correspondence service | SMTP (down) | Error logged, retry queued |
| 5 | Invalid email address | Correspondence service | SMTP service | Bounce handled, error logged |
| 6 | Multiple delivery methods | Correspondence service | Print + Email | Both channels processed |

---

### Test Case ID: INT-DOC-005
**Integration**: Document Service -> Search/Index Service
**Priority**: Low
**Status**: FUTURE-STATE

#### Integration Points
| Document Component | Search Component | Data Flow |
|-------------------|-----------------|-----------|
| Document Create | Search index | Metadata indexed for search |
| Document Delete | Search index | Entry removed from index |
| Document Search | Search index | Full-text and filtered queries |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | New document indexed | Document service | Search service | Document searchable after creation |
| 2 | Deleted document removed from index | Document service | Search service | No longer returned in search results |
| 3 | Search by entity type and date | UI service | Search service | Filtered results returned via pagination |
| 4 | Search index unavailable | Document service | Search service (down) | Document created, index update queued |
| 5 | Index rebuild from source | Admin operation | Search service | Full re-index from document metadata |

---

### Test Case ID: INT-DOC-006
**Integration**: Template Service -> Version Control
**Priority**: Low
**Status**: FUTURE-STATE

#### Integration Points
| Template Component | Version Component | Data Flow |
|-------------------|------------------|-----------|
| Template Save (new) | Version history | Initial version created |
| Template Save (update) | Version history | New version, previous preserved |
| Template GetByID | Version history | Current active version returned |

#### Test Scenarios
| # | Scenario | Source | Target | Expected |
|---|----------|--------|--------|----------|
| 1 | New template creates version 1 | Template service | Version store | Version = 1 |
| 2 | Update creates new version | Template service | Version store | Version incremented |
| 3 | Previous version accessible | Template service | Version store | Historical versions retrievable [ASSUMPTION] |
| 4 | Rollback to previous version | Admin operation | Version store | Active version reverted |
