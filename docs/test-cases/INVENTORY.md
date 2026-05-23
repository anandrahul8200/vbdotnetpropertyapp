# Application Inventory

## Technology Stack
- Language: VB.NET (.NET Framework)
- UI Framework: WinForms
- Database: SQL Server
- ORM/Data Access: ADO.NET via DatabaseHelper (static methods calling stored procedures)
- Build System: MSBuild (Visual Studio Solution with 3 projects)

## Summary Counts
| Category | Count |
|----------|-------|
| Stored Procedures | 154 |
| UI Forms/Screens | 60 |
| Data Access Classes | 9 |
| Model/DTO Classes | 4 |
| Common Utility Classes | 7 |
| Batch Job Programs | 6 |
| Database Tables | 60 |
| Configuration Files | 4 |

## Module Breakdown
| Module Code | Module Name | Forms | Data Access | Stored Procedures | Tables |
|-------------|-------------|-------|-------------|-------------------|--------|
| POL | Policy | 14 | PolicyDataAccess, CustomerDataAccess, PropertyDataAccess | 25 | 14 |
| CLM | Claims | 16 | ClaimDataAccess, FraudDataAccess | 28 | 12 |
| BIL | Billing | 5 | BillingDataAccess | 11 | 5 |
| ADM | Admin | 9 | AdminDataAccess | 33 | 12 |
| UND | Underwriting | 6 | UnderwritingDataAccess | 19 | 10 |
| DOC | Documents | 4 | - | 4 | - |
| WFL | Workflow | 3 | - | 8 | - |
| RPT | Reports | 1 | - | 10 | - |
| BAT | BatchJobs | - | - | 16 | - |
| RNS | Reinsurance | - | ReinsuranceDataAccess | 6 | 4 |
| COM | Common | - | - | - | - |

---

## Detailed Listing

### Stored Procedures (154 total)

#### File: 001-policy-crud-sps.sql (7 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Policy.usp_Customer_Create | Policy |
| 2 | Policy.usp_Customer_Update | Policy |
| 3 | Policy.usp_Customer_GetByID | Policy |
| 4 | Policy.usp_Customer_Search | Policy |
| 5 | Policy.usp_Property_Create | Policy |
| 6 | Policy.usp_Policy_CreateQuote | Policy |
| 7 | Policy.usp_Policy_GetDetails | Policy |

#### File: 002-premium-calculation-sps.sql (4 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Underwriting.usp_Premium_Calculate | Underwriting |
| 2 | Underwriting.usp_Premium_CalculateCoverage | Underwriting |
| 3 | Underwriting.usp_Premium_CalculateTaxesFees | Underwriting |
| 4 | Underwriting.usp_Premium_CalculateEndorsement | Underwriting |

#### File: 003-claims-processing-sps.sql (13 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Claims.usp_Claim_Create | Claims |
| 2 | Claims.usp_Claim_UpdateStatus | Claims |
| 3 | Claims.usp_Claim_SetReserve | Claims |
| 4 | Claims.usp_Claim_CreatePayment | Claims |
| 5 | Claims.usp_Claim_ApprovePayment | Claims |
| 6 | Claims.usp_Claim_VoidPayment | Claims |
| 7 | Claims.usp_Claim_CreateActivity | Claims |
| 8 | Claims.usp_Claim_CompleteActivity | Claims |
| 9 | Claims.usp_Claim_Assign | Claims |
| 10 | Claims.usp_Claim_GetDetails | Claims |
| 11 | Claims.usp_Claim_Search | Claims |
| 12 | Claims.usp_Claim_VerifyCoverage | Claims |
| 13 | Claims.usp_Claim_GetDashboard | Claims |

#### File: 004-claims-specialized-sps.sql (11 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Claims.usp_Fraud_EvaluateClaim | Claims |
| 2 | Claims.usp_Fraud_ReferToSIU | Claims |
| 3 | Claims.usp_Fraud_GetEvaluation | Claims |
| 4 | Claims.usp_Subrogation_Create | Claims |
| 5 | Claims.usp_Subrogation_UpdateStatus | Claims |
| 6 | Claims.usp_Subrogation_RecordRecovery | Claims |
| 7 | Claims.usp_Catastrophe_Create | Claims |
| 8 | Claims.usp_Catastrophe_LinkClaim | Claims |
| 9 | Claims.usp_Catastrophe_GetSummary | Claims |
| 10 | Claims.usp_Vendor_Create | Claims |
| 11 | Claims.usp_Vendor_Search | Claims |

#### File: 005-underwriting-sps.sql (15 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Underwriting.usp_Rate_GetBaseRate | Underwriting |
| 2 | Underwriting.usp_Rate_GetFactor | Underwriting |
| 3 | Underwriting.usp_Rules_Evaluate | Underwriting |
| 4 | Underwriting.usp_Referral_Process | Underwriting |
| 5 | Underwriting.usp_Referral_GetByPolicy | Underwriting |
| 6 | Underwriting.usp_Referral_SearchPending | Underwriting |
| 7 | Underwriting.usp_Moratorium_Create | Underwriting |
| 8 | Underwriting.usp_Moratorium_Lift | Underwriting |
| 9 | Underwriting.usp_Moratorium_Check | Underwriting |
| 10 | Underwriting.usp_Moratorium_GetActive | Underwriting |
| 11 | Underwriting.usp_Worksheet_GetByPolicy | Underwriting |
| 12 | Underwriting.usp_RateTable_Create | Underwriting |
| 13 | Underwriting.usp_RateTable_AddDetail | Underwriting |
| 14 | Underwriting.usp_RateTable_GetDetails | Underwriting |
| 15 | Underwriting.usp_Commission_Calculate | Underwriting |

#### File: 006-billing-sps.sql (9 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Billing.usp_Invoice_Generate | Billing |
| 2 | Billing.usp_Payment_Record | Billing |
| 3 | Billing.usp_Payment_Return | Billing |
| 4 | Billing.usp_Refund_Create | Billing |
| 5 | Billing.usp_Refund_Approve | Billing |
| 6 | Billing.usp_Invoice_ApplyLateFees | Billing |
| 7 | Billing.usp_Commission_Create | Billing |
| 8 | Billing.usp_Commission_GetStatement | Billing |
| 9 | Billing.usp_Billing_GetByPolicy | Billing |

#### File: 007-reinsurance-sps.sql (6 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Reinsurance.usp_Treaty_Create | Reinsurance |
| 2 | Reinsurance.usp_Cession_CalculatePremium | Reinsurance |
| 3 | Reinsurance.usp_Cession_CalculateLoss | Reinsurance |
| 4 | Reinsurance.usp_Bordereaux_Generate | Reinsurance |
| 5 | Reinsurance.usp_Treaty_GetSummary | Reinsurance |
| 6 | Reinsurance.usp_Treaty_GetActive | Reinsurance |

#### File: 008-batch-job-sps.sql (9 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Batch.usp_Renewal_Process | Batch |
| 2 | Batch.usp_Expiration_Process | Batch |
| 3 | Batch.usp_Reserve_Recalculate | Batch |
| 4 | Batch.usp_FraudScoring_Process | Batch |
| 5 | Batch.usp_CancellationNotice_Process | Batch |
| 6 | Batch.usp_Data_Archive | Batch |
| 7 | Batch.usp_JobLog_Start | Batch |
| 8 | Batch.usp_JobLog_Complete | Batch |
| 9 | Batch.usp_JobLog_Fail | Batch |

#### File: 009-reporting-sps.sql (5 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Admin.usp_Report_ExecutiveDashboard | Admin |
| 2 | Admin.usp_Report_LossRatio | Admin |
| 3 | Admin.usp_Report_ClaimsAging | Admin |
| 4 | Admin.usp_Report_Production | Admin |
| 5 | Admin.usp_Report_FinancialSummary | Admin |

#### File: 010-search-lookup-sps.sql (9 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Admin.usp_Lookup_GetByCategory | Admin |
| 2 | Admin.usp_Lookup_GetStates | Admin |
| 3 | Admin.usp_Lookup_GetAgents | Admin |
| 4 | Admin.usp_Lookup_GetCoverageTypes | Admin |
| 5 | Admin.usp_Lookup_GetDeductibleOptions | Admin |
| 6 | Admin.usp_Search_Global | Admin |
| 7 | Admin.usp_Lookup_GetPaymentPlans | Admin |
| 8 | Admin.usp_Lookup_GetCatastrophes | Admin |
| 9 | Policy.usp_Policy_Search | Policy |

#### File: 011-admin-utility-sps.sql (9 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Admin.usp_User_Authenticate | Admin |
| 2 | Admin.usp_User_Create | Admin |
| 3 | Admin.usp_User_GetPermissions | Admin |
| 4 | Admin.usp_Config_Get | Admin |
| 5 | Admin.usp_Config_Set | Admin |
| 6 | Admin.usp_Lookup_Create | Admin |
| 7 | Admin.usp_Lookup_Update | Admin |
| 8 | Admin.usp_Audit_Search | Admin |
| 9 | Admin.usp_ErrorLog_Search | Admin |

#### File: 012-additional-sps.sql (55 procedures)
| # | Procedure Name | Schema |
|---|----------------|--------|
| 1 | Policy.usp_Document_Create | Policy |
| 2 | Policy.usp_Document_GetByEntity | Policy |
| 3 | Policy.usp_Document_Delete | Policy |
| 4 | Policy.usp_Document_Search | Policy |
| 5 | Admin.usp_Task_Create | Admin |
| 6 | Admin.usp_Task_GetByUser | Admin |
| 7 | Admin.usp_Task_Complete | Admin |
| 8 | Admin.usp_Task_Reassign | Admin |
| 9 | Admin.usp_Notification_Create | Admin |
| 10 | Admin.usp_Notification_GetByUser | Admin |
| 11 | Admin.usp_Notification_MarkRead | Admin |
| 12 | Admin.usp_Notification_MarkAllRead | Admin |
| 13 | Policy.usp_Agent_Search | Policy |
| 14 | Policy.usp_Agent_GetByID | Policy |
| 15 | Policy.usp_Agent_Update | Policy |
| 16 | Policy.usp_Agent_GetProduction | Policy |
| 17 | Policy.usp_Policy_Cancel | Policy |
| 18 | Policy.usp_Policy_Reinstate | Policy |
| 19 | Claims.usp_Catastrophe_GetAll | Claims |
| 20 | Claims.usp_Catastrophe_Close | Claims |
| 21 | Claims.usp_Litigation_GetOpen | Claims |
| 22 | Claims.usp_Litigation_Update | Claims |
| 23 | Admin.usp_Template_GetAll | Admin |
| 24 | Admin.usp_Template_GetByID | Admin |
| 25 | Admin.usp_Template_Save | Admin |
| 26 | Admin.usp_Correspondence_Log | Admin |
| 27 | Admin.usp_Correspondence_GetHistory | Admin |
| 28 | Reporting.usp_Dashboard_PolicyKPIs | Reporting |
| 29 | Reporting.usp_Report_ClaimsByType | Reporting |
| 30 | Reporting.usp_Report_AgentProduction | Reporting |
| 31 | Reporting.usp_Report_BillingAging | Reporting |
| 32 | Reporting.usp_Report_ReinsuranceSummary | Reporting |
| 33 | Admin.usp_User_List | Admin |
| 34 | Admin.usp_User_Lock | Admin |
| 35 | Admin.usp_User_Unlock | Admin |
| 36 | Admin.usp_User_ResetPassword | Admin |
| 37 | Admin.usp_Role_GetAll | Admin |
| 38 | Admin.usp_RolePermission_GetByRole | Admin |
| 39 | Batch.usp_JobLog_Start | Batch |
| 40 | Batch.usp_JobLog_Complete | Batch |
| 41 | Admin.usp_ErrorLog_Insert | Admin |
| 42 | Policy.usp_Policy_Search | Policy |
| 43 | Policy.usp_Property_GetByID | Policy |
| 44 | Policy.usp_Property_GetByCustomer | Policy |
| 45 | Claims.usp_Assignment_GetByClaim | Claims |
| 46 | Claims.usp_Subrogation_GetByClaim | Claims |
| 47 | Billing.usp_Refund_GetPending | Billing |
| 48 | Billing.usp_PaymentPlan_List | Billing |
| 49 | Batch.usp_Renewal_GetDuePolicies | Batch |
| 50 | Batch.usp_Fraud_GetClaimsToScore | Batch |
| 51 | Batch.usp_Reserve_GetClaimsForReview | Batch |
| 52 | Batch.usp_Reserve_FlagForReview | Batch |
| 53 | Batch.usp_Reserve_CalculateIBNR | Batch |
| 54 | Batch.usp_Reserve_UpdateNetIncurred | Batch |
| 55 | Policy.usp_Policy_SaveCoverage | Policy |
| 56 | Policy.usp_Policy_UpdatePremium | Policy |
| 57 | Policy.usp_Policy_Bind | Policy |

> Note: 012-additional-sps.sql contains some duplicate procedure definitions (Batch.usp_JobLog_Start, Batch.usp_JobLog_Complete, Policy.usp_Policy_Search) that override earlier versions.

---

### UI Forms (60 total)

#### Root Forms (2 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmLogin | src/PropertyInsuranceClaims/Forms/frmLogin.vb |
| 2 | frmMain | src/PropertyInsuranceClaims/Forms/frmMain.vb |

#### Admin Forms (9 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmAuditViewer | src/PropertyInsuranceClaims/Forms/Admin/frmAuditViewer.vb |
| 2 | frmBatchJobMonitor | src/PropertyInsuranceClaims/Forms/Admin/frmBatchJobMonitor.vb |
| 3 | frmDashboardMain | src/PropertyInsuranceClaims/Forms/Admin/frmDashboardMain.vb |
| 4 | frmErrorLogViewer | src/PropertyInsuranceClaims/Forms/Admin/frmErrorLogViewer.vb |
| 5 | frmLookupMaintenance | src/PropertyInsuranceClaims/Forms/Admin/frmLookupMaintenance.vb |
| 6 | frmPasswordReset | src/PropertyInsuranceClaims/Forms/Admin/frmPasswordReset.vb |
| 7 | frmRolePermissions | src/PropertyInsuranceClaims/Forms/Admin/frmRolePermissions.vb |
| 8 | frmSystemConfig | src/PropertyInsuranceClaims/Forms/Admin/frmSystemConfig.vb |
| 9 | frmUserManagement | src/PropertyInsuranceClaims/Forms/Admin/frmUserManagement.vb |

#### Billing Forms (5 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmBillingInquiry | src/PropertyInsuranceClaims/Forms/Billing/frmBillingInquiry.vb |
| 2 | frmCommissionStatement | src/PropertyInsuranceClaims/Forms/Billing/frmCommissionStatement.vb |
| 3 | frmPaymentEntry | src/PropertyInsuranceClaims/Forms/Billing/frmPaymentEntry.vb |
| 4 | frmPaymentPlanSetup | src/PropertyInsuranceClaims/Forms/Billing/frmPaymentPlanSetup.vb |
| 5 | frmRefundProcessing | src/PropertyInsuranceClaims/Forms/Billing/frmRefundProcessing.vb |

#### Claims Forms (16 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmCatastropheManager | src/PropertyInsuranceClaims/Forms/Claims/frmCatastropheManager.vb |
| 2 | frmClaimActivity | src/PropertyInsuranceClaims/Forms/Claims/frmClaimActivity.vb |
| 3 | frmClaimAssignment | src/PropertyInsuranceClaims/Forms/Claims/frmClaimAssignment.vb |
| 4 | frmClaimDashboard | src/PropertyInsuranceClaims/Forms/Claims/frmClaimDashboard.vb |
| 5 | frmClaimFNOL | src/PropertyInsuranceClaims/Forms/Claims/frmClaimFNOL.vb |
| 6 | frmClaimPayment | src/PropertyInsuranceClaims/Forms/Claims/frmClaimPayment.vb |
| 7 | frmClaimReserve | src/PropertyInsuranceClaims/Forms/Claims/frmClaimReserve.vb |
| 8 | frmClaimSearch | src/PropertyInsuranceClaims/Forms/Claims/frmClaimSearch.vb |
| 9 | frmClaimStatusChange | src/PropertyInsuranceClaims/Forms/Claims/frmClaimStatusChange.vb |
| 10 | frmClaimSummaryReport | src/PropertyInsuranceClaims/Forms/Claims/frmClaimSummaryReport.vb |
| 11 | frmClaimView | src/PropertyInsuranceClaims/Forms/Claims/frmClaimView.vb |
| 12 | frmFraudReview | src/PropertyInsuranceClaims/Forms/Claims/frmFraudReview.vb |
| 13 | frmLitigationTracker | src/PropertyInsuranceClaims/Forms/Claims/frmLitigationTracker.vb |
| 14 | frmSalvageRecovery | src/PropertyInsuranceClaims/Forms/Claims/frmSalvageRecovery.vb |
| 15 | frmSubrogation | src/PropertyInsuranceClaims/Forms/Claims/frmSubrogation.vb |
| 16 | frmVendorManagement | src/PropertyInsuranceClaims/Forms/Claims/frmVendorManagement.vb |

#### Documents Forms (4 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmCorrespondence | src/PropertyInsuranceClaims/Forms/Documents/frmCorrespondence.vb |
| 2 | frmDocumentUpload | src/PropertyInsuranceClaims/Forms/Documents/frmDocumentUpload.vb |
| 3 | frmDocumentViewer | src/PropertyInsuranceClaims/Forms/Documents/frmDocumentViewer.vb |
| 4 | frmTemplateManager | src/PropertyInsuranceClaims/Forms/Documents/frmTemplateManager.vb |

#### Policy Forms (14 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmAgentSearch | src/PropertyInsuranceClaims/Forms/Policy/frmAgentSearch.vb |
| 2 | frmCoverageEditor | src/PropertyInsuranceClaims/Forms/Policy/frmCoverageEditor.vb |
| 3 | frmCustomerEntry | src/PropertyInsuranceClaims/Forms/Policy/frmCustomerEntry.vb |
| 4 | frmCustomerSearch | src/PropertyInsuranceClaims/Forms/Policy/frmCustomerSearch.vb |
| 5 | frmEndorsement | src/PropertyInsuranceClaims/Forms/Policy/frmEndorsement.vb |
| 6 | frmPolicyCancellation | src/PropertyInsuranceClaims/Forms/Policy/frmPolicyCancellation.vb |
| 7 | frmPolicyDashboard | src/PropertyInsuranceClaims/Forms/Policy/frmPolicyDashboard.vb |
| 8 | frmPolicyEntry | src/PropertyInsuranceClaims/Forms/Policy/frmPolicyEntry.vb |
| 9 | frmPolicyReinstatement | src/PropertyInsuranceClaims/Forms/Policy/frmPolicyReinstatement.vb |
| 10 | frmPolicySearch | src/PropertyInsuranceClaims/Forms/Policy/frmPolicySearch.vb |
| 11 | frmPolicyView | src/PropertyInsuranceClaims/Forms/Policy/frmPolicyView.vb |
| 12 | frmPropertyEntry | src/PropertyInsuranceClaims/Forms/Policy/frmPropertyEntry.vb |
| 13 | frmPropertySearch | src/PropertyInsuranceClaims/Forms/Policy/frmPropertySearch.vb |
| 14 | frmRenewal | src/PropertyInsuranceClaims/Forms/Policy/frmRenewal.vb |

#### Reports Forms (1 form)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmReportViewer | src/PropertyInsuranceClaims/Forms/Reports/frmReportViewer.vb |

#### Underwriting Forms (6 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmMoratorium | src/PropertyInsuranceClaims/Forms/Underwriting/frmMoratorium.vb |
| 2 | frmQuoteWorksheet | src/PropertyInsuranceClaims/Forms/Underwriting/frmQuoteWorksheet.vb |
| 3 | frmRateTableMaintenance | src/PropertyInsuranceClaims/Forms/Underwriting/frmRateTableMaintenance.vb |
| 4 | frmReferralQueue | src/PropertyInsuranceClaims/Forms/Underwriting/frmReferralQueue.vb |
| 5 | frmReinsuranceView | src/PropertyInsuranceClaims/Forms/Underwriting/frmReinsuranceView.vb |
| 6 | frmUnderwritingReferral | src/PropertyInsuranceClaims/Forms/Underwriting/frmUnderwritingReferral.vb |

#### Workflow Forms (3 forms)
| # | Form Name | File Path |
|---|-----------|-----------|
| 1 | frmDiaryManager | src/PropertyInsuranceClaims/Forms/Workflow/frmDiaryManager.vb |
| 2 | frmNotificationCenter | src/PropertyInsuranceClaims/Forms/Workflow/frmNotificationCenter.vb |
| 3 | frmTaskQueue | src/PropertyInsuranceClaims/Forms/Workflow/frmTaskQueue.vb |

---

### Data Access Classes (9 total)

| # | Class Name | File Path | Module |
|---|------------|-----------|--------|
| 1 | AdminDataAccess | src/PropertyInsuranceClaims/DataAccess/AdminDataAccess.vb | ADM |
| 2 | BillingDataAccess | src/PropertyInsuranceClaims/DataAccess/BillingDataAccess.vb | BIL |
| 3 | ClaimDataAccess | src/PropertyInsuranceClaims/DataAccess/ClaimDataAccess.vb | CLM |
| 4 | CustomerDataAccess | src/PropertyInsuranceClaims/DataAccess/CustomerDataAccess.vb | POL |
| 5 | FraudDataAccess | src/PropertyInsuranceClaims/DataAccess/FraudDataAccess.vb | CLM |
| 6 | PolicyDataAccess | src/PropertyInsuranceClaims/DataAccess/PolicyDataAccess.vb | POL |
| 7 | PropertyDataAccess | src/PropertyInsuranceClaims/DataAccess/PropertyDataAccess.vb | POL |
| 8 | ReinsuranceDataAccess | src/PropertyInsuranceClaims/DataAccess/ReinsuranceDataAccess.vb | RNS |
| 9 | UnderwritingDataAccess | src/PropertyInsuranceClaims/DataAccess/UnderwritingDataAccess.vb | UND |

---

### Model/DTO Classes (4 total)

| # | Class Name | File Path | Module |
|---|------------|-----------|--------|
| 1 | ClaimDTO | src/PropertyInsuranceClaims/Models/ClaimDTO.vb | CLM |
| 2 | CustomerDTO | src/PropertyInsuranceClaims/Models/CustomerDTO.vb | POL |
| 3 | PolicyDTO | src/PropertyInsuranceClaims/Models/PolicyDTO.vb | POL |
| 4 | PropertyDTO | src/PropertyInsuranceClaims/Models/PropertyDTO.vb | POL |

---

### Common Utility Classes (7 total)

| # | Class Name | File Path | Purpose |
|---|------------|-----------|---------|
| 1 | AppSettings | src/PropertyInsuranceClaims.Common/AppSettings.vb | Application settings management |
| 2 | Constants | src/PropertyInsuranceClaims.Common/Constants.vb | Status codes and constant values |
| 3 | DatabaseHelper | src/PropertyInsuranceClaims.Common/DatabaseHelper.vb | ADO.NET database access wrapper |
| 4 | ErrorLogger | src/PropertyInsuranceClaims.Common/ErrorLogger.vb | Error logging utility |
| 5 | GlobalState | src/PropertyInsuranceClaims.Common/GlobalState.vb | Session/global state management |
| 6 | SecurityHelper | src/PropertyInsuranceClaims.Common/SecurityHelper.vb | Security and authentication helpers |
| 7 | ValidationHelper | src/PropertyInsuranceClaims.Common/ValidationHelper.vb | UI input validation |

---

### Batch Jobs (6 total)

| # | Job Name | File Path | Module |
|---|----------|-----------|--------|
| 1 | ExpirationProcessor | src/PropertyInsuranceClaims.BatchJobs/ExpirationProcessor/Program.vb | BAT |
| 2 | FraudScoring | src/PropertyInsuranceClaims.BatchJobs/FraudScoring/Program.vb | BAT |
| 3 | PaymentBatch | src/PropertyInsuranceClaims.BatchJobs/PaymentBatch/Program.vb | BAT |
| 4 | ReinsuranceAllocator | src/PropertyInsuranceClaims.BatchJobs/ReinsuranceAllocator/Program.vb | BAT |
| 5 | RenewalProcessor | src/PropertyInsuranceClaims.BatchJobs/RenewalProcessor/Program.vb | BAT |
| 6 | ReserveRecalculator | src/PropertyInsuranceClaims.BatchJobs/ReserveRecalculator/Program.vb | BAT |

---

### Database Tables (60 total)

#### File: 002-admin-tables.sql (12 tables)
| # | Table Name | Schema |
|---|------------|--------|
| 1 | Admin.SystemConfig | Admin |
| 2 | Admin.Users | Admin |
| 3 | Admin.Roles | Admin |
| 4 | Admin.Departments | Admin |
| 5 | Admin.Permissions | Admin |
| 6 | Admin.RolePermissions | Admin |
| 7 | Admin.LookupCategories | Admin |
| 8 | Admin.LookupValues | Admin |
| 9 | Admin.States | Admin |
| 10 | Audit.AuditLog | Audit |
| 11 | Audit.ErrorLog | Audit |
| 12 | Batch.JobLog | Batch |

#### File: 003-policy-tables.sql (14 tables)
| # | Table Name | Schema |
|---|------------|--------|
| 1 | Policy.Customers | Policy |
| 2 | Policy.Agents | Policy |
| 3 | Policy.Agencies | Policy |
| 4 | Policy.Territories | Policy |
| 5 | Policy.Regions | Policy |
| 6 | Policy.Properties | Policy |
| 7 | Policy.Policies | Policy |
| 8 | Policy.PolicyVersions | Policy |
| 9 | Policy.Coverages | Policy |
| 10 | Policy.Perils | Policy |
| 11 | Policy.CoveragePerils | Policy |
| 12 | Policy.Endorsements | Policy |
| 13 | Policy.Documents | Policy |
| 14 | Policy.Notes | Policy |

#### File: 004-claims-tables.sql (12 tables)
| # | Table Name | Schema |
|---|------------|--------|
| 1 | Claims.Claims | Claims |
| 2 | Claims.ClaimCoverages | Claims |
| 3 | Claims.Reserves | Claims |
| 4 | Claims.Payments | Claims |
| 5 | Claims.Activities | Claims |
| 6 | Claims.StatusHistory | Claims |
| 7 | Claims.Catastrophes | Claims |
| 8 | Claims.Vendors | Claims |
| 9 | Claims.Assignments | Claims |
| 10 | Claims.Subrogation | Claims |
| 11 | Claims.FraudIndicators | Claims |
| 12 | Claims.ClaimFraudScores | Claims |

#### File: 005-underwriting-tables.sql (10 tables)
| # | Table Name | Schema |
|---|------------|--------|
| 1 | Underwriting.RateTables | Underwriting |
| 2 | Underwriting.RateTableDetails | Underwriting |
| 3 | Underwriting.BaseRates | Underwriting |
| 4 | Underwriting.RatingFactors | Underwriting |
| 5 | Underwriting.DeductibleOptions | Underwriting |
| 6 | Underwriting.Rules | Underwriting |
| 7 | Underwriting.Referrals | Underwriting |
| 8 | Underwriting.RatingWorksheets | Underwriting |
| 9 | Underwriting.CommissionSchedules | Underwriting |
| 10 | Underwriting.Moratoriums | Underwriting |

#### File: 006-billing-reinsurance-tables.sql (9 tables)
| # | Table Name | Schema |
|---|------------|--------|
| 1 | Billing.Invoices | Billing |
| 2 | Billing.PremiumPayments | Billing |
| 3 | Billing.PaymentPlans | Billing |
| 4 | Billing.CommissionTransactions | Billing |
| 5 | Billing.Refunds | Billing |
| 6 | Reinsurance.Treaties | Reinsurance |
| 7 | Reinsurance.Reinsurers | Reinsurance |
| 8 | Reinsurance.Cessions | Reinsurance |
| 9 | Reinsurance.Bordereaux | Reinsurance |

#### File: 007-schema-patches.sql (3 tables)
| # | Table Name | Schema | Notes |
|---|------------|--------|-------|
| 1 | Admin.Holidays | Admin | Added via patch |
| 2 | Policy.Notes | Policy | Conditional creation (IF NOT EXISTS) |
| 3 | Admin.UserRoles | Admin | Added via patch |

---

### Configuration Files (4 total)

| # | File | Path | Purpose |
|---|------|------|---------|
| 1 | PropertyInsuranceClaims.sln | src/PropertyInsuranceClaims.sln | Visual Studio solution file |
| 2 | PropertyInsuranceClaims.vbproj | src/PropertyInsuranceClaims/PropertyInsuranceClaims.vbproj | Main application project |
| 3 | PropertyInsuranceClaims.Common.vbproj | src/PropertyInsuranceClaims.Common/PropertyInsuranceClaims.Common.vbproj | Common library project |
| 4 | PropertyInsuranceClaims.BatchJobs.vbproj | src/PropertyInsuranceClaims.BatchJobs/PropertyInsuranceClaims.BatchJobs.vbproj | Batch jobs project |

---

### Deployment Scripts

| # | Script | Path | Purpose |
|---|--------|------|---------|
| 1 | deploy-app.ps1 | deploy/deploy-app.ps1 | Application deployment |
| 2 | deploy-database.ps1 | deploy/deploy-database.ps1 | Database deployment |
| 3 | setup-scheduled-tasks.ps1 | deploy/setup-scheduled-tasks.ps1 | Scheduled task setup |
| 4 | register-scheduled-tasks.ps1 | deploy/batch-jobs/register-scheduled-tasks.ps1 | Batch job task registration |
| 5 | run-cancellation-notice.ps1 | deploy/batch-jobs/run-cancellation-notice.ps1 | Cancellation notice batch |
| 6 | run-claims-aging.ps1 | deploy/batch-jobs/run-claims-aging.ps1 | Claims aging batch |
| 7 | run-commission-processor.ps1 | deploy/batch-jobs/run-commission-processor.ps1 | Commission processing batch |
| 8 | run-data-archiver.ps1 | deploy/batch-jobs/run-data-archiver.ps1 | Data archival batch |
| 9 | run-database-maintenance.ps1 | deploy/batch-jobs/run-database-maintenance.ps1 | Database maintenance batch |
| 10 | run-expiration-processor.ps1 | deploy/batch-jobs/run-expiration-processor.ps1 | Policy expiration batch |
| 11 | run-fraud-scoring.ps1 | deploy/batch-jobs/run-fraud-scoring.ps1 | Fraud scoring batch |
| 12 | run-ibnr-calculator.ps1 | deploy/batch-jobs/run-ibnr-calculator.ps1 | IBNR calculation batch |
| 13 | run-notification-sender.ps1 | deploy/batch-jobs/run-notification-sender.ps1 | Notification sending batch |
| 14 | run-payment-batch.ps1 | deploy/batch-jobs/run-payment-batch.ps1 | Payment processing batch |
| 15 | run-policy-audit.ps1 | deploy/batch-jobs/run-policy-audit.ps1 | Policy audit batch |
| 16 | run-reinsurance-allocator.ps1 | deploy/batch-jobs/run-reinsurance-allocator.ps1 | Reinsurance allocation batch |
| 17 | run-renewal-processor.ps1 | deploy/batch-jobs/run-renewal-processor.ps1 | Policy renewal batch |
| 18 | run-report-generator.ps1 | deploy/batch-jobs/run-report-generator.ps1 | Report generation batch |
| 19 | run-reserve-recalculator.ps1 | deploy/batch-jobs/run-reserve-recalculator.ps1 | Reserve recalculation batch |

---

### Seed Data Files

| # | File | Path | Purpose |
|---|------|------|---------|
| 1 | 001-seed-data.sql | database/03-seed-data/001-seed-data.sql | Reference/lookup data |
| 2 | 002-test-data.sql | database/03-seed-data/002-test-data.sql | Test data for development |

---

## Module Mapping

| Module Code | Full Name | Description | Primary Components |
|-------------|-----------|-------------|-------------------|
| POL | Policy | Policy lifecycle management | Forms (14), DataAccess (3), DTOs (3), Tables (14), SPs (25) |
| CLM | Claims | Claims processing and management | Forms (16), DataAccess (2), DTOs (1), Tables (12), SPs (28) |
| BIL | Billing | Billing, payments, and commissions | Forms (5), DataAccess (1), Tables (5), SPs (11) |
| ADM | Admin | System administration and configuration | Forms (9), DataAccess (1), Tables (12), SPs (33) |
| UND | Underwriting | Rating, rules, and referrals | Forms (6), DataAccess (1), Tables (10), SPs (19) |
| DOC | Documents | Document management and correspondence | Forms (4), SPs (4) |
| WFL | Workflow | Tasks, notifications, and diary | Forms (3), SPs (8) |
| RPT | Reports | Reporting and dashboards | Forms (1), SPs (10) |
| BAT | BatchJobs | Scheduled batch processing | BatchJobs (6), SPs (16) |
| RNS | Reinsurance | Reinsurance treaties and cessions | DataAccess (1), Tables (4), SPs (6) |
| COM | Common | Shared utilities and helpers | Utilities (7) |
