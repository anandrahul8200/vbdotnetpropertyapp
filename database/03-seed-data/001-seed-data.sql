-- ============================================================
-- SEED DATA
-- Lookup values, system config, roles, permissions, states,
-- payment plans, fraud indicators, and sample rate data
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- ROLES & PERMISSIONS
-- ============================================================
INSERT INTO Admin.Roles (RoleName, Description) VALUES
('ADMIN', 'System Administrator - full access'),
('UNDERWRITER', 'Underwriter - policy review and approval'),
('CLAIMS_ADJUSTER', 'Claims Adjuster - claims processing'),
('CLAIMS_SUPERVISOR', 'Claims Supervisor - approvals and oversight'),
('AGENT', 'Agent - policy entry and inquiry'),
('BILLING', 'Billing Clerk - payment processing'),
('READONLY', 'Read-only access for reporting');
GO

INSERT INTO Admin.Permissions (PermissionCode, PermissionName, Module) VALUES
-- Policy module
('POL_CREATE', 'Create Policy', 'POLICY'),
('POL_EDIT', 'Edit Policy', 'POLICY'),
('POL_VIEW', 'View Policy', 'POLICY'),
('POL_CANCEL', 'Cancel Policy', 'POLICY'),
('POL_ENDORSE', 'Process Endorsement', 'POLICY'),
('POL_RENEW', 'Process Renewal', 'POLICY'),
-- Claims module
('CLM_CREATE', 'Create Claim', 'CLAIMS'),
('CLM_EDIT', 'Edit Claim', 'CLAIMS'),
('CLM_VIEW', 'View Claim', 'CLAIMS'),
('CLM_APPROVE_PAY', 'Approve Claim Payment', 'CLAIMS'),
('CLM_APPROVE_RES', 'Approve Reserve Change', 'CLAIMS'),
('CLM_CLOSE', 'Close Claim', 'CLAIMS'),
-- Underwriting
('UW_APPROVE', 'Approve Referral', 'UNDERWRITING'),
('UW_RATE_EDIT', 'Edit Rate Tables', 'UNDERWRITING'),
('UW_MORATORIUM', 'Manage Moratoriums', 'UNDERWRITING'),
-- Billing
('BIL_PAYMENT', 'Record Payment', 'BILLING'),
('BIL_REFUND', 'Process Refund', 'BILLING'),
('BIL_APPROVE', 'Approve Refund', 'BILLING'),
-- Admin
('ADM_USERS', 'Manage Users', 'ADMIN'),
('ADM_CONFIG', 'System Configuration', 'ADMIN'),
('ADM_AUDIT', 'View Audit Trail', 'ADMIN'),
('ADM_LOOKUPS', 'Manage Lookups', 'ADMIN');
GO

-- Assign all permissions to ADMIN role
INSERT INTO Admin.RolePermissions (RoleID, PermissionID)
SELECT (SELECT RoleID FROM Admin.Roles WHERE RoleName = 'ADMIN'), PermissionID
FROM Admin.Permissions;
GO
