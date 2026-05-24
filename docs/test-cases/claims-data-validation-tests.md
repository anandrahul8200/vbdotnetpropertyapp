# Claims Module - Data Validation Tests

## Module: CLM (Claims)
## Test Type: Data Integrity and Validation Tests
## Schema Source: `database/01-schema/004-claims-tables.sql`

---

### Test Case ID: DV-CLM-001
**Table**: Claims.Claims
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| ClaimID | INT IDENTITY(1,1) | NO | Auto-increment | PRIMARY KEY |
| ClaimNumber | VARCHAR(20) | NO | None | UNIQUE |
| PolicyID | INT | NO | None | FK -> Policy.Policies(PolicyID) |
| CustomerID | INT | NO | None | FK -> Policy.Customers(CustomerID) |
| PropertyID | INT | NO | None | FK -> Policy.Properties(PropertyID) |
| ClaimStatus | VARCHAR(20) | NO | 'FNOL' | Valid: FNOL, ASSIGNED, INVESTIGATING, ASSESSED, APPROVED, DENIED, SETTLED, CLOSED, REOPENED, LITIGATION |
| ClaimType | VARCHAR(30) | NO | None | Valid: PROPERTY_DAMAGE, THEFT, LIABILITY, WATER_DAMAGE, FIRE, WIND, HAIL, OTHER |
| CatastropheID | INT | YES | NULL | Optional FK |
| LossDate | DATETIME | NO | None | Must be within policy period |
| ReportedDate | DATETIME | NO | GETDATE() | Auto-set |
| EstimatedLoss | DECIMAL(18,2) | YES | NULL | >= 0 |
| PolicyLimit | DECIMAL(18,2) | YES | NULL | Set from coverage |
| TotalPaid | DECIMAL(18,2) | YES | 0 | Sum of approved payments |
| TotalReserve | DECIMAL(18,2) | YES | 0 | Sum of approved reserves |
| TotalRecovery | DECIMAL(18,2) | YES | 0 | Sum of recoveries |
| NetIncurred | DECIMAL(18,2) | YES | 0 | Reserve + Paid - Recovery |
| Priority | VARCHAR(10) | YES | 'NORMAL' | Valid: LOW, NORMAL, HIGH, CRITICAL |
| Complexity | VARCHAR(10) | YES | 'SIMPLE' | Valid: SIMPLE, MODERATE, COMPLEX |
| FraudScore | DECIMAL(6,2) | YES | 0 | Range 0-100 |
| IsSIUReferred | BIT | YES | 0 | Boolean |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimNumber format | All records | Match pattern CLM + 7 digits (CLM\d{7}) |
| 2 | ClaimNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | PolicyID FK valid | Insert with invalid PolicyID | FK constraint violation |
| 4 | ClaimStatus values | All records | Only valid status values |
| 5 | TotalPaid >= 0 | All records | No negative TotalPaid |
| 6 | TotalReserve >= 0 | All records | No negative TotalReserve |
| 7 | NetIncurred calculation | All records | NetIncurred = TotalReserve + TotalPaid - TotalRecovery |
| 8 | TotalPaid <= PolicyLimit | All records | Enforced by SP logic |
| 9 | FraudScore range | All records | Between 0 and 100 |
| 10 | Complexity matches EstimatedLoss | All records | COMPLEX if > $100K, MODERATE if > $25K, else SIMPLE |
| 11 | LossDate <= ReportedDate | All records | Loss cannot be reported before it happens |
| 12 | ClosedDate set when CLOSED | Status=CLOSED | ClosedDate IS NOT NULL |
| 13 | AssignedDate set when ASSIGNED | Status past FNOL | AssignedDate IS NOT NULL |

---

### Test Case ID: DV-CLM-002
**Table**: Claims.Reserves
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| ReserveID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| ReserveType | VARCHAR(20) | NO | None | Valid: CASE, EXPENSE, IBNR, BULK |
| ReserveCategory | VARCHAR(30) | YES | None | Valid: INDEMNITY, DEFENSE, ADJUSTMENT_EXPENSE, MEDICAL |
| Amount | DECIMAL(18,2) | NO | None | >= 0 |
| PreviousAmount | DECIMAL(18,2) | YES | 0 | Previous value |
| ChangeAmount | DECIMAL(18,2) | YES | 0 | Amount - PreviousAmount |
| ApprovalRequired | BIT | YES | 0 | 1 if Amount > $50,000 |
| IsApproved | BIT | YES | 1 | 0 if ApprovalRequired=1 |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimID FK valid | All records | References existing claim |
| 2 | Amount >= 0 | All records | No negative amounts |
| 3 | ChangeAmount = Amount - PreviousAmount | All records | Calculated correctly |
| 4 | ApprovalRequired consistency | Amount > $50K | ApprovalRequired = 1 |
| 5 | IsApproved consistency | ApprovalRequired=1 | IsApproved=0 until approved |
| 6 | ReserveType valid values | All records | Only CASE, EXPENSE, IBNR, BULK |

---

### Test Case ID: DV-CLM-003
**Table**: Claims.Payments
**Priority**: Critical

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| PaymentID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| PaymentNumber | VARCHAR(20) | NO | None | UNIQUE |
| PaymentType | VARCHAR(20) | NO | None | Valid: INDEMNITY, EXPENSE, PARTIAL, FINAL, SUPPLEMENT |
| PaymentMethod | VARCHAR(20) | YES | 'CHECK' | Valid: CHECK, EFT, WIRE, DRAFT |
| PayeeType | VARCHAR(20) | NO | None | Valid: INSURED, VENDOR, ATTORNEY, MORTGAGEE, LIENHOLDER |
| PayeeName | VARCHAR(200) | NO | None | Not empty |
| Amount | DECIMAL(18,2) | NO | None | > 0 |
| TaxReportable | BIT | YES | 0 | Boolean |
| Form1099Required | BIT | YES | 0 | Based on 1099 rule |
| Status | VARCHAR(20) | YES | 'PENDING' | Valid: PENDING, APPROVED, ISSUED, CLEARED, VOIDED, STOPPED |
| ApprovalRequired | BIT | YES | 0 | 1 if Amount > $10,000 |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | PaymentNumber format | All records | Match pattern PAY + 7 digits (PAY\d{7}) |
| 2 | PaymentNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | Amount > 0 | All records | No zero or negative amounts |
| 4 | Form1099Required rule | TaxReportable=1 AND PayeeType IN (VENDOR,ATTORNEY) AND Amount >= 600 | Form1099Required = 1 |
| 5 | ApprovalRequired consistency | Amount > $10K | ApprovalRequired = 1 |
| 6 | Status valid values | All records | Only valid status values |
| 7 | VoidedDate set when VOIDED | Status=VOIDED | VoidedDate IS NOT NULL |
| 8 | VoidReason set when VOIDED | Status=VOIDED | VoidReason IS NOT NULL |
| 9 | ApprovedBy set when APPROVED | Status=APPROVED and ApprovalRequired=1 | ApprovedBy IS NOT NULL |
| 10 | Sum of approved payments = Claim.TotalPaid | Per claim | Amounts match |

---

### Test Case ID: DV-CLM-004
**Table**: Claims.Activities
**Priority**: Medium

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| ActivityID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| ActivityType | VARCHAR(30) | NO | None | Valid: NOTE, PHONE_CALL, EMAIL, INSPECTION, DOCUMENT, STATUS_CHANGE, PAYMENT, RESERVE_CHANGE |
| ActivityDate | DATETIME | YES | GETDATE() | Auto-set |
| IsCompleted | BIT | YES | 0 | Boolean |
| CompletedDate | DATETIME | YES | NULL | Set when IsCompleted=1 |
| Priority | VARCHAR(10) | YES | 'NORMAL' | Valid: LOW, NORMAL, HIGH |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimID FK valid | All records | References existing claim |
| 2 | ActivityType valid values | All records | Only valid types |
| 3 | CompletedDate consistency | IsCompleted=1 | CompletedDate IS NOT NULL |
| 4 | CompletedDate null when incomplete | IsCompleted=0 | CompletedDate IS NULL |
| 5 | ActivityDate not future | All records | ActivityDate <= GETDATE() |

---

### Test Case ID: DV-CLM-005
**Table**: Claims.Assignments
**Priority**: Medium

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| AssignmentID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| AssigneeType | VARCHAR(20) | NO | None | Valid: ADJUSTER, VENDOR, EXAMINER, SIU |
| AssigneeID | INT | NO | None | References appropriate table |
| Status | VARCHAR(20) | YES | 'ASSIGNED' | Valid: ASSIGNED, IN_PROGRESS, COMPLETED, REASSIGNED, CANCELLED |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimID FK valid | All records | References existing claim |
| 2 | AssigneeType valid values | All records | Only valid types |
| 3 | Status valid values | All records | Only valid status values |
| 4 | AssignmentDate <= DueDate | When DueDate set | Due date is in future of assignment |

---

### Test Case ID: DV-CLM-006
**Table**: Claims.StatusHistory
**Priority**: High

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| StatusHistoryID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| PreviousStatus | VARCHAR(20) | YES | NULL | NULL for initial FNOL |
| NewStatus | VARCHAR(20) | NO | None | Valid status value |
| ChangeDate | DATETIME | YES | GETDATE() | Auto-set |
| ChangedBy | VARCHAR(50) | YES | None | User who made change |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimID FK valid | All records | References existing claim |
| 2 | First record has NULL PreviousStatus | First per claim | PreviousStatus IS NULL, NewStatus='FNOL' |
| 3 | Valid status transitions | All records | Each PreviousStatus->NewStatus follows matrix |
| 4 | Chronological order | Per claim | ChangeDate is ascending |
| 5 | Latest record matches Claim.ClaimStatus | Per claim | Most recent NewStatus = Claims.ClaimStatus |

---

### Test Case ID: DV-CLM-007
**Table**: Claims.Catastrophes
**Priority**: Medium

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| CatastropheID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| CatastropheNumber | VARCHAR(20) | NO | None | UNIQUE |
| CatastropheName | VARCHAR(200) | NO | None | Not empty |
| CatastropheType | VARCHAR(30) | NO | None | Valid: HURRICANE, TORNADO, EARTHQUAKE, FLOOD, WILDFIRE, HAIL, WINTER_STORM |
| EventDate | DATE | NO | None | Not future [ASSUMPTION] |
| IsActive | BIT | YES | 1 | Boolean |
| TotalClaimsCount | INT | YES | 0 | Count of linked claims |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | CatastropheNumber format | All records | Match pattern CAT + 7 digits (CAT\d{7}) |
| 2 | CatastropheNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | TotalClaimsCount accuracy | Per catastrophe | COUNT of Claims where CatastropheID matches |
| 4 | TotalReserveAmount accuracy | Per catastrophe | SUM of linked claims TotalReserve |
| 5 | TotalPaidAmount accuracy | Per catastrophe | SUM of linked claims TotalPaid |
| 6 | ClosedDate set when inactive | IsActive=0 | ClosedDate IS NOT NULL |

---

### Test Case ID: DV-CLM-008
**Table**: Claims.Subrogation
**Priority**: Medium

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| SubrogationID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| SubrogationStatus | VARCHAR(20) | YES | 'IDENTIFIED' | Valid: IDENTIFIED, DEMAND_SENT, NEGOTIATING, ARBITRATION, SETTLED, CLOSED, ABANDONED |
| ResponsibleParty | VARCHAR(200) | YES | None | Should not be empty when record exists |
| RecoveryAmount | DECIMAL(18,2) | YES | 0 | >= 0, cumulative |
| SettlementAmount | DECIMAL(18,2) | YES | NULL | Set when settled |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | ClaimID FK valid | All records | References existing claim |
| 2 | SubrogationStatus valid | All records | Only valid status values |
| 3 | RecoveryAmount >= 0 | All records | No negative recovery |
| 4 | RecoveryAmount <= DemandAmount | When both set | Recovery should not exceed demand [ASSUMPTION] |
| 5 | Claim.TotalSubrogation matches | Per claim | SUM of linked RecoveryAmount = Claim.TotalSubrogation |

---

### Test Case ID: DV-CLM-009
**Table**: Claims.Vendors
**Priority**: Low

#### Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| VendorID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| VendorNumber | VARCHAR(20) | NO | None | UNIQUE |
| VendorName | VARCHAR(200) | NO | None | Not empty |
| VendorType | VARCHAR(30) | NO | None | Valid: ADJUSTER, CONTRACTOR, APPRAISER, ENGINEER, ATTORNEY, INVESTIGATOR |
| IsActive | BIT | YES | 1 | Boolean |
| Rating | DECIMAL(3,1) | YES | NULL | Range 0-5 [ASSUMPTION] |
| TotalAssignments | INT | YES | 0 | >= 0 |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | VendorNumber format | All records | Match pattern VND + 7 digits (VND\d{7}) |
| 2 | VendorNumber uniqueness | Duplicate insert | UNIQUE constraint violation |
| 3 | VendorType valid values | All records | Only valid types |
| 4 | TotalAssignments accuracy | Per vendor | COUNT of Assignments where AssigneeID matches |
| 5 | Rating range | When set | Between 0.0 and 5.0 [ASSUMPTION] |

---

### Test Case ID: DV-CLM-010
**Table**: Claims.FraudIndicators / Claims.ClaimFraudScores
**Priority**: Medium

#### FraudIndicators Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| FraudIndicatorID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| IndicatorCode | VARCHAR(20) | NO | None | UNIQUE |
| Weight | DECIMAL(4,2) | YES | 1.0 | > 0 |
| IsActive | BIT | YES | 1 | Boolean |

#### ClaimFraudScores Column Constraints
| Column | Type | Nullable | Default | Constraint |
|--------|------|----------|---------|------------|
| ClaimFraudScoreID | INT IDENTITY(1,1) | NO | Auto | PRIMARY KEY |
| ClaimID | INT | NO | None | FK -> Claims.Claims(ClaimID) |
| IndicatorID | INT | NO | None | FK -> Claims.FraudIndicators |
| IsTriggered | BIT | YES | 0 | Boolean |
| Score | DECIMAL(6,2) | YES | 0 | = Weight if triggered, 0 otherwise |

#### Data Validation Tests
| # | Test | Condition | Expected |
|---|------|-----------|----------|
| 1 | Score = 0 when not triggered | IsTriggered=0 | Score = 0 |
| 2 | Score = Weight when triggered | IsTriggered=1 | Score = indicator Weight |
| 3 | Claim.FraudScore matches | Per claim | Normalized total matches Claims.FraudScore |
| 4 | IndicatorCode uniqueness | All records | No duplicate codes |
