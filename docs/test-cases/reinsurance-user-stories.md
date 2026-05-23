# Reinsurance Module - User Stories

## Module: RNS (Reinsurance)
## Source Files:
- `src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb`
- `src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb`
- `database/02-stored-procedures/007-reinsurance-sps.sql`

---

### Story ID: US-RNS-001
**Priority**: Critical
**Module**: Reinsurance

#### User Story
As an **underwriter**,
I want to **create a new reinsurance treaty with defined terms**,
So that **the company can transfer risk to a reinsurer and manage portfolio exposure**.

#### Acceptance Criteria

**Given** the user has access to the Reinsurance Management form
**When** the user clicks "New Treaty" and enters treaty details (name, type, dates, cession terms)
**Then** a new treaty is created with an auto-generated TreatyNumber (TRY + 7 digits) and Status='ACTIVE'

**Given** the treaty type is QUOTA_SHARE
**When** the user specifies a cession percentage (e.g., 40%)
**Then** the treaty stores the CessionPercent for proportional calculations

**Given** the treaty type is EXCESS_OF_LOSS
**When** the user specifies attachment and exhaustion points
**Then** the treaty stores both values for layer-based loss cession calculations

**Given** the treaty type is SURPLUS
**When** the user specifies a retention amount and optional cession limit
**Then** the treaty stores both values for TIV-based proportional calculations

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-RNS-001 | Reinsurance.usp_Treaty_Create |
| Form/Screen | UI-RNS-002 | frmReinsuranceView - Treaties Tab |
| Business Rule | BR-RNS-001 | Treaty Number Format |
| Functional Test | FT-RNS-001 | Treaty Creation |

---

### Story ID: US-RNS-002
**Priority**: Critical
**Module**: Reinsurance

#### User Story
As an **underwriter**,
I want to **view all currently active treaties with their cession totals**,
So that **I can monitor the company's reinsurance program and capacity utilization**.

#### Acceptance Criteria

**Given** the user opens the Reinsurance Management form
**When** the Treaties tab loads
**Then** all active treaties (Status='ACTIVE' and current date within effective/expiry range) are displayed with TotalCededPremium and TotalCededLoss

**Given** active treaties exist with associated reinsurers
**When** the treaty list is displayed
**Then** the ReinsurerName and AMBestRating are shown alongside each treaty

**Given** the user wants to see treaty details
**When** the user selects a treaty and clicks "View Details"
**Then** a detailed summary is shown including cession summary by type, bordereaux history, and recent cessions

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-RNS-006 | Reinsurance.usp_Treaty_GetActive |
| Stored Procedure | SP-RNS-005 | Reinsurance.usp_Treaty_GetSummary |
| Form/Screen | UI-RNS-002 | frmReinsuranceView - Treaties Tab |
| Functional Test | FT-RNS-006 | Treaty Summary and Reporting |

---

### Story ID: US-RNS-003
**Priority**: Critical
**Module**: Reinsurance

#### User Story
As a **reinsurance analyst**,
I want to **calculate premium cessions for policies based on active treaty terms**,
So that **the correct proportion of premium is allocated to each reinsurer**.

#### Acceptance Criteria

**Given** a policy with a known PolicyType and StateCode
**When** premium cession is calculated
**Then** all active treaties matching the policy's type and state (within effective dates) are identified

**Given** a matching QUOTA_SHARE treaty with CessionPercent=40%
**When** premium cession is calculated for GrossPremium=$10,000
**Then** CededAmount = $4,000 and RetainedAmount = $6,000

**Given** a matching SURPLUS treaty with RetentionAmount=$500,000
**When** premium cession is calculated for a policy with TIV=$1,000,000
**Then** SurplusPct = (1,000,000 - 500,000) / 1,000,000 = 50%, CededAmount = 50% of premium

**Given** a matching SURPLUS treaty with RetentionAmount=$500K and CessionLimit=$2M
**When** TIV exceeds RetentionAmount + CessionLimit
**Then** SurplusPct is capped using CessionLimit / TIV

**Given** a matching EXCESS_OF_LOSS treaty
**When** premium cession is calculated
**Then** CededAmount = GrossPremium * CessionPercent (flat rate for XOL premium)

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-RNS-002 | Reinsurance.usp_Cession_CalculatePremium |
| Form/Screen | UI-RNS-003 | frmReinsuranceView - Cessions Tab |
| Business Rule | BR-RNS-003 | Quota Share Calculation |
| Business Rule | BR-RNS-004 | Surplus Treaty Calculation |
| Business Rule | BR-RNS-005 | Excess-of-Loss Premium |
| Functional Test | FT-RNS-002 | Premium Cession Calculation |

---

### Story ID: US-RNS-004
**Priority**: Critical
**Module**: Reinsurance

#### User Story
As a **reinsurance analyst**,
I want to **calculate loss cessions for claims based on active treaty terms**,
So that **the correct portion of each loss is recovered from reinsurers**.

#### Acceptance Criteria

**Given** a claim linked to a policy with matching active treaties
**When** loss cession is calculated for a QUOTA_SHARE treaty
**Then** CededLoss = LossAmount * CessionPercent

**Given** an EXCESS_OF_LOSS treaty with AttachmentPoint=$100K and ExhaustionPoint=$500K
**When** LossAmount exceeds the AttachmentPoint (e.g., $250K)
**Then** CededLoss = LossAmount - AttachmentPoint = $150K

**Given** an EXCESS_OF_LOSS treaty with the same attachment/exhaustion points
**When** LossAmount exceeds the ExhaustionPoint (e.g., $1M)
**Then** CededLoss is capped at ExhaustionPoint - AttachmentPoint = $400K

**Given** a SURPLUS treaty
**When** loss cession is calculated
**Then** the existing premium cession percentage (from the most recent PREMIUM cession for the same treaty/policy) is applied to the loss

**Given** a SURPLUS treaty with no prior premium cession for the policy
**When** loss cession is calculated
**Then** CededLoss = 0 (ExistingPct defaults to 0)

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-RNS-003 | Reinsurance.usp_Cession_CalculateLoss |
| Business Rule | BR-RNS-006 | Excess-of-Loss Layer Logic |
| Business Rule | BR-RNS-007 | Surplus Loss Follows Premium |
| Functional Test | FT-RNS-003 | Loss Cession Calculation |

---

### Story ID: US-RNS-005
**Priority**: High
**Module**: Reinsurance

#### User Story
As a **reinsurance analyst**,
I want to **generate bordereaux reports for a specific treaty and reporting period**,
So that **cession data can be formally reported to and settled with reinsurers**.

#### Acceptance Criteria

**Given** a treaty with PENDING cessions in a specific accounting period
**When** the user selects the treaty, period, and report type (PREMIUM/LOSS/OUTSTANDING) and clicks "Generate"
**Then** a new bordereaux record is created with aggregated TotalGross, TotalCeded, TotalRetained, and RecordCount

**Given** bordereaux is generated successfully
**When** the generation completes
**Then** all matching PENDING cessions are updated to Status='REPORTED'

**Given** the report type is OUTSTANDING
**When** bordereaux is generated
**Then** all cession types (PREMIUM and LOSS) are included in the aggregation

**Given** no PENDING cessions exist for the period
**When** bordereaux is generated
**Then** a record is created with TotalGross=0, TotalCeded=0, RecordCount=0

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-RNS-004 | Reinsurance.usp_Bordereaux_Generate |
| Form/Screen | UI-RNS-004 | frmReinsuranceView - Bordereaux Tab |
| Business Rule | BR-RNS-008 | Bordereaux Reporting Period |
| Business Rule | BR-RNS-009 | Bordereaux Status Lifecycle |
| Functional Test | FT-RNS-004 | Bordereaux Generation |

---

### Story ID: US-RNS-006
**Priority**: High
**Module**: Reinsurance

#### User Story
As a **reinsurance analyst**,
I want to **submit bordereaux to reinsurers and export report data**,
So that **reinsurance settlements can be processed and records maintained**.

#### Acceptance Criteria

**Given** a generated bordereaux with Status='DRAFT'
**When** the user selects the record and clicks "Submit"
**Then** the bordereaux status is updated and a confirmation message is displayed

**Given** a bordereaux record exists
**When** the user clicks "Export"
**Then** the data is exported in a standard format (Excel/CSV) for external distribution

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Form/Screen | UI-RNS-004 | frmReinsuranceView - Bordereaux Tab |
| Functional Test | FT-RNS-005 | Bordereaux Submission |
| Functional Test | FT-RNS-008 | Bordereaux Export |

---

### Story ID: US-RNS-007
**Priority**: Medium
**Module**: Reinsurance

#### User Story
As an **underwriter**,
I want to **view cession details filtered by treaty and accounting period**,
So that **I can track what premium and losses have been ceded under each treaty**.

#### Acceptance Criteria

**Given** the user navigates to the Cessions tab
**When** the user selects a treaty from the dropdown and enters an accounting period
**Then** the cessions grid displays all matching cession records

**Given** cessions are loaded successfully
**When** the grid is populated
**Then** a summary label shows total Gross and total Ceded amounts in currency format

**Given** the cessions contain DBNull values in GrossAmount or CededAmount
**When** the summary is calculated
**Then** DBNull values are treated as zero (no calculation errors)

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Form/Screen | UI-RNS-003 | frmReinsuranceView - Cessions Tab |
| Unit Test | UT-RNS-005 | ReinsuranceDataAccess.GetCessionsByTreaty |
| Functional Test | FT-RNS-007 | Cession Loading by Treaty and Period |

---

### Story ID: US-RNS-008
**Priority**: Medium
**Module**: Reinsurance

#### User Story
As a **reinsurance manager**,
I want to **manage reinsurer information and assign reinsurers to treaties**,
So that **counterparty relationships are properly tracked and rated**.

#### Acceptance Criteria

**Given** reinsurer records exist in the system
**When** the reinsurer list is retrieved
**Then** all active reinsurers with their codes, names, ratings (AMBest, S&P), and contact information are displayed

**Given** a reinsurer is being assigned to a treaty
**When** the treaty is created or updated
**Then** the ReinsurerID links the treaty to the reinsurer record

**Given** a treaty is viewed with a linked reinsurer
**When** the treaty summary is displayed
**Then** the reinsurer's name and ratings are shown

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Unit Test | UT-RNS-008 | ReinsuranceDataAccess.GetReinsurers |
| Data Validation | DV-RNS-002 | Reinsurance.Reinsurers table |
| Business Rule | BR-RNS-010 | Reinsurer Rating Requirements |
