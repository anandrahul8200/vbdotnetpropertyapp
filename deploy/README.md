# Deployment Guide — Property Insurance Claims Management System

## Prerequisites

### MSSQL Server EC2
- Windows Server 2019/2022
- SQL Server 2019 (Developer or Standard edition)
- Mixed-mode authentication enabled
- Port 1433 open to the App Server security group
- Minimum: t3.large (2 vCPU, 8 GB RAM)

### App Server EC2
- Windows Server 2019/2022
- .NET Framework 4.8 (pre-installed on Windows Server 2019+)
- RDP access (port 3389) for end users
- WinRM enabled for remote deployment (port 5985/5986)
- Minimum: t3.medium (2 vCPU, 4 GB RAM)

## Deployment Steps

### 1. Deploy Database

From your local machine (or a jump box with network access to the DB server):

```powershell
cd deploy

# Full deployment (schema + SPs + seed data)
.\deploy-database.ps1 -ServerName "10.0.1.50" -Username "sa" -Password "YourSAPassword"

# Or with Windows Auth (if domain-joined)
.\deploy-database.ps1 -ServerName "DBSERVER01" -UseWindowsAuth

# Re-deploy only stored procedures (after code changes)
.\deploy-database.ps1 -ServerName "10.0.1.50" -Username "sa" -Password "YourPass" -SkipSchema -SkipSeedData
```

### 2. Create Application Database User

After schema deployment, create a dedicated app user on the SQL Server:

```sql
USE PropertyInsuranceDB;
GO
CREATE LOGIN AppUser WITH PASSWORD = 'YourAppPassword123!';
CREATE USER AppUser FOR LOGIN AppUser;
EXEC sp_addrolemember 'db_datareader', 'AppUser';
EXEC sp_addrolemember 'db_datawriter', 'AppUser';
GRANT EXECUTE TO AppUser;
GO
```

### 3. Deploy Application

```powershell
# Build and deploy to app server
.\deploy-app.ps1 -TargetServer "10.0.1.100" -DBServer "10.0.1.50" -DBUsername "AppUser" -DBPassword "YourAppPassword123!"

# Skip build (deploy pre-built package)
.\deploy-app.ps1 -TargetServer "10.0.1.100" -DBServer "10.0.1.50" -SkipBuild
```

### 4. Setup Batch Jobs (run ON the app server)

RDP into the app server and run:

```powershell
cd C:\PropertyInsurance
.\setup-scheduled-tasks.ps1
```

### 5. Create Initial Admin User

Run this on the SQL Server after deployment:

```sql
USE PropertyInsuranceDB;
EXEC Admin.usp_User_Create 
    @Username = 'admin',
    @PasswordHash = '<hash from SecurityHelper.HashPassword>',
    @FirstName = 'System',
    @LastName = 'Administrator',
    @Email = 'admin@company.com',
    @RoleID = 1,
    @CreatedBy = 'SYSTEM',
    @UserID = NULL;
```

## Security Notes

- Never store passwords in scripts — use AWS Secrets Manager or parameter prompts
- Restrict SQL Server port 1433 to only the app server's security group
- Use HTTPS for any future web-facing components
- Enable SQL Server audit logging for compliance
- Rotate the AppUser password periodically

## Batch Job Schedule

| Job | Schedule | Time | Purpose |
|-----|----------|------|---------|
| RenewalProcessor | Daily | 2:00 AM | Process policy renewals |
| ExpirationProcessor | Daily | 3:00 AM | Expire/cancel policies |
| FraudScoring | Daily | 4:00 AM | Score new claims for fraud |
| PaymentBatch | Daily | 5:00 AM | Late fees, auto-pay, notices |
| ReserveRecalculator | Weekly (Sun) | 1:00 AM | Review reserves, IBNR |
| ReinsuranceAllocator | Monthly (1st) | 6:00 AM | Treaty allocations, bordereaux |

## Troubleshooting

- **Connection errors**: Check security group rules, verify SQL Server is listening on 1433
- **Build failures**: Ensure .NET Framework 4.8 SDK or VS Build Tools are installed
- **Deployment failures**: Verify WinRM is enabled on target (`Enable-PSRemoting -Force`)
- **Batch job failures**: Check logs at `C:\PropertyInsurance\Logs\`
