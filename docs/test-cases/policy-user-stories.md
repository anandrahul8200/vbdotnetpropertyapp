# Policy Module - User Stories

## Module: POL (Policy)
## Test Type: User Stories with Acceptance Criteria

---

### Story ID: US-POL-001
**Priority**: Critical
**Module**: POL

#### User Story
As a **policy entry clerk**,
I want to **create a new customer record with personal and contact information**,
So that **I can associate policies and properties to this customer**.

#### Acceptance Criteria

**Given** the user has policy entry permissions
**When** the user opens frmCustomerEntry with no existing ID
**Then** a blank form is displayed with title 'New Customer'

**Given** the user fills in required fields (First Name or Company, Address, City, Zip)
**When** the user clicks Save
**Then** a CustomerNumber is generated (format CUS+7 digits), and the record is persisted

**Given** a credit score is provided
**When** the customer is saved
**Then** the RiskTier is assigned: >= 750 = PREFERRED, 650-749 = STANDARD, < 650 = SUBSTANDARD

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-001 | Policy.usp_Customer_Create |
| Form/Screen | UI-POL-001 | frmCustomerEntry |
| Business Rule | BR-POL-001 | Credit Score Risk Tier Assignment |
| Functional Test | FT-POL-001 | Create Customer and Add Property |

---

### Story ID: US-POL-002
**Priority**: Critical
**Module**: POL

#### User Story
As a **policy entry clerk**,
I want to **search for existing customers by name, number, or location**,
So that **I can find and select customers for policy operations**.

#### Acceptance Criteria

**Given** the user opens frmCustomerSearch
**When** the user enters a search term and clicks Search
**Then** matching customers are displayed in a paginated grid

**Given** search results are displayed
**When** the user double-clicks a row
**Then** the customer edit form opens (or in picker mode, the ID is returned and the form closes)

**Given** no matches exist
**When** the search is executed
**Then** the grid is empty and record count shows '0 record(s) found'

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-004 | Policy.usp_Customer_Search |
| Form/Screen | UI-POL-002 | frmCustomerSearch |
| Functional Test | FT-POL-001 | Create Customer and Add Property |

---

### Story ID: US-POL-003
**Priority**: Critical
**Module**: POL

#### User Story
As a **policy entry clerk**,
I want to **add a property with construction details, protective devices, and valuation**,
So that **the property can be used for policy quoting and rating**.

#### Acceptance Criteria

**Given** an active customer exists
**When** the user opens frmPropertyEntry for that customer
**Then** a blank property form is displayed

**Given** required fields (Address, Year Built, Square Footage) are filled
**When** the user clicks Save
**Then** a PropertyNumber is generated (format PRP+7 digits) and the property is linked to the customer

**Given** DistanceToFireStation and DistanceToHydrant are provided but FireProtectionClass is not
**When** the property is saved
**Then** FireProtectionClass is auto-calculated based on distance rules

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-005 | Policy.usp_Property_Create |
| Form/Screen | UI-POL-003 | frmPropertyEntry |
| Business Rule | BR-POL-002 | Fire Protection Class Calculation |
| Functional Test | FT-POL-001 | Create Customer and Add Property |

---

### Story ID: US-POL-004
**Priority**: Critical
**Module**: POL

#### User Story
As a **policy entry clerk**,
I want to **create a new policy quote by selecting a customer, property, agent, and coverage options**,
So that **a premium can be calculated and the policy can be bound**.

#### Acceptance Criteria

**Given** an active customer with a property and an active agent exist
**When** the user fills in policy details and clicks Save on frmPolicyEntry
**Then** a quote is created with PolicyStatus='QUOTE' and PolicyNumber in format POL+7 digits

**Given** a moratorium is in effect for the property state and policy type
**When** the user attempts to create a quote
**Then** the system shows an error: 'New business moratorium is in effect for this location/policy type'

**Given** the property does not belong to the selected customer
**When** the quote creation is attempted
**Then** an error is raised: 'Property not found or does not belong to customer'

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-008 | Policy.usp_Policy_CreateQuote |
| Form/Screen | UI-POL-005 | frmPolicyEntry |
| Business Rule | BR-POL-003 | Moratorium Check |
| Business Rule | BR-POL-005 | Policy Number Format |
| Functional Test | FT-POL-002 | Create Policy Quote |

---

### Story ID: US-POL-005
**Priority**: Critical
**Module**: POL

#### User Story
As a **underwriter or policy clerk**,
I want to **bind a policy quote to make it active**,
So that **coverage is in force and billing can begin**.

#### Acceptance Criteria

**Given** a policy exists in QUOTE status with premium calculated
**When** the user clicks Bind and confirms
**Then** PolicyStatus changes to 'ACTIVE' and an audit entry is created

**Given** a policy is not in QUOTE status
**When** bind is attempted
**Then** an error is raised: 'Policy must be in QUOTE status to bind'

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-011 | Policy.usp_Policy_Bind |
| Form/Screen | UI-POL-005 | frmPolicyEntry |
| Business Rule | BR-POL-004 | Bind Requires QUOTE Status |
| Functional Test | FT-POL-003 | Calculate Premium and Bind Policy |

---

### Story ID: US-POL-006
**Priority**: High
**Module**: POL

#### User Story
As a **policy administrator**,
I want to **search for policies by number, customer, type, status, state, or date range**,
So that **I can locate policies for service or review**.

#### Acceptance Criteria

**Given** the user opens frmPolicySearch
**When** filters are entered and Search is clicked
**Then** matching policies are displayed in a paginated grid with record count

**Given** a policy is selected in the grid
**When** the user clicks Open or double-clicks
**Then** the full policy view (frmPolicyView) opens showing all details in tabs

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-010 | Policy.usp_Policy_Search |
| Stored Procedure | SP-POL-009 | Policy.usp_Policy_GetDetails |
| Form/Screen | UI-POL-006 | frmPolicySearch |
| Form/Screen | UI-POL-007 | frmPolicyView |
| Functional Test | FT-POL-004 | Search and View Policy |

---

### Story ID: US-POL-007
**Priority**: High
**Module**: POL

#### User Story
As a **policy administrator**,
I want to **cancel an active policy with a reason, effective date, and refund calculation**,
So that **coverage ends and the customer receives any unearned premium refund**.

#### Acceptance Criteria

**Given** an active policy exists
**When** the user selects a cancel reason, date, and calculation method, then clicks Calculate
**Then** the earned and return premium amounts are displayed

**Given** the calculation is shown
**When** the user clicks Process and confirms
**Then** PolicyStatus changes to 'CANCELLED' and the return premium is determined

**Given** pro-rata calculation method is selected
**When** cancellation is processed
**Then** ReturnPremium = AnnualPremium - (AnnualPremium * DaysUsed / DaysInTerm)

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-012 | Policy.usp_Policy_Cancel |
| Form/Screen | UI-POL-011 | frmPolicyCancellation |
| Business Rule | BR-POL-006 | Pro-Rata Cancellation Calculation |
| Functional Test | FT-POL-005 | Cancel Policy |

---

### Story ID: US-POL-008
**Priority**: High
**Module**: POL

#### User Story
As a **policy administrator**,
I want to **reinstate a previously cancelled policy with conditions**,
So that **coverage can be restored for the customer**.

#### Acceptance Criteria

**Given** a policy in CANCELLED status
**When** the user sets a reinstatement date and conditions and clicks Reinstate
**Then** PolicyStatus returns to 'ACTIVE' and an audit entry is created

**Given** the reinstatement requires payment
**When** the payment amount is specified
**Then** the amount due is recorded with the reinstatement

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-013 | Policy.usp_Policy_Reinstate |
| Form/Screen | UI-POL-012 | frmPolicyReinstatement |
| Functional Test | FT-POL-006 | Reinstate Cancelled Policy |

---

### Story ID: US-POL-009
**Priority**: High
**Module**: POL

#### User Story
As a **policy administrator**,
I want to **process mid-term endorsements with pro-rata premium adjustments**,
So that **coverage changes take effect immediately with appropriate billing**.

#### Acceptance Criteria

**Given** an active policy
**When** the user selects an endorsement type, sets effective date, and modifies coverages
**Then** the pro-rata factor is calculated based on remaining days in term

**Given** the calculation is complete
**When** Process is confirmed
**Then** the endorsement is applied and premium adjustments are recorded

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Form/Screen | UI-POL-010 | frmEndorsement |
| Functional Test | FT-POL-007 | Process Endorsement |

---

### Story ID: US-POL-010
**Priority**: High
**Module**: POL

#### User Story
As a **renewal processor**,
I want to **review expiring policies, compare premiums, and process renewals**,
So that **continuous coverage is maintained for eligible customers**.

#### Acceptance Criteria

**Given** policies are expiring within 30 days
**When** the user clicks Load Expiring on frmRenewal
**Then** expiring policies are listed with customer and premium information

**Given** a policy is selected for renewal
**When** Calculate is clicked
**Then** the new premium is computed and shown alongside the current premium with the change percentage

**Given** renewal is confirmed
**When** the user clicks Renew
**Then** the policy is renewed and the list is refreshed

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Form/Screen | UI-POL-014 | frmRenewal |
| Functional Test | FT-POL-008 | Renewal Processing |

---

### Story ID: US-POL-011
**Priority**: Medium
**Module**: POL

#### User Story
As a **policy manager**,
I want to **view a dashboard with KPIs including active policies, new business, and loss ratio**,
So that **I can monitor portfolio health at a glance**.

#### Acceptance Criteria

**Given** the user opens frmPolicyDashboard
**When** the dashboard loads
**Then** 6 KPI cards are displayed: Active Policies, New Business, Renewals, Cancellations, Written Premium, Loss Ratio

**Given** loss ratio exceeds 70%
**When** displayed
**Then** the value is shown in red color

**Given** the user clicks a quick action button
**When** New Quote is clicked
**Then** frmPolicyEntry opens

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Form/Screen | UI-POL-008 | frmPolicyDashboard |
| Functional Test | FT-POL-009 | Policy Dashboard Overview |

---

### Story ID: US-POL-012
**Priority**: Medium
**Module**: POL

#### User Story
As a **policy clerk**,
I want to **search for agents by name or type**,
So that **I can select the appropriate agent when creating a policy**.

#### Acceptance Criteria

**Given** the user opens frmAgentSearch
**When** search criteria are entered and Search is clicked
**Then** matching agents are displayed with their type, contact info, and commission rate

#### Linked Artifacts
| Type | ID | Name |
|------|-----|------|
| Stored Procedure | SP-POL-016 | Policy.usp_Agent_Search |
| Form/Screen | UI-POL-009 | frmAgentSearch |
| Functional Test | FT-POL-010 | Agent Search and Production Review |
