-- ============================================================
-- ADMIN & UTILITY STORED PROCEDURES
-- User management, system config, audit viewer, lookup
-- maintenance, and utility functions
-- ============================================================
USE PropertyInsuranceDB;
GO

-- ============================================================
-- SP: Authenticate User
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_User_Authenticate
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @IPAddress VARCHAR(50) = NULL,
    @IsAuthenticated BIT OUTPUT,
    @UserID INT OUTPUT,
    @FullName VARCHAR(200) OUTPUT,
    @RoleName VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IsAuthenticated = 0;
    SET @UserID = 0;

    SELECT @UserID = u.UserID, @FullName = u.FirstName + ' ' + u.LastName,
           @RoleName = r.RoleName
    FROM Admin.Users u
    INNER JOIN Admin.UserRoles ur ON u.UserID = ur.UserID
    INNER JOIN Admin.Roles r ON ur.RoleID = r.RoleID
    WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash
        AND u.IsActive = 1 AND u.IsLocked = 0;

    IF @UserID > 0
    BEGIN
        SET @IsAuthenticated = 1;

        -- Update last login
        UPDATE Admin.Users SET
            LastLoginDate = GETDATE(),
            FailedLoginAttempts = 0
        WHERE UserID = @UserID;

        -- Log login
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate, AdditionalInfo)
        VALUES ('Admin.Users', @UserID, 'LOGIN', @Username, GETDATE(), 'IP: ' + ISNULL(@IPAddress, 'Unknown'));
    END
    ELSE
    BEGIN
        -- Increment failed attempts
        UPDATE Admin.Users SET
            FailedLoginAttempts = FailedLoginAttempts + 1,
            IsLocked = CASE WHEN FailedLoginAttempts >= 4 THEN 1 ELSE IsLocked END,
            LockedDate = CASE WHEN FailedLoginAttempts >= 4 THEN GETDATE() ELSE LockedDate END
        WHERE Username = @Username AND IsActive = 1;
    END
END
GO

-- ============================================================
-- SP: Create User
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_User_Create
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @FirstName VARCHAR(100),
    @LastName VARCHAR(100),
    @Email VARCHAR(200),
    @RoleID INT,
    @Department VARCHAR(50) = NULL,
    @CreatedBy VARCHAR(50),
    @UserID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- Check username uniqueness
        IF EXISTS (SELECT 1 FROM Admin.Users WHERE Username = @Username)
        BEGIN
            RAISERROR('Username already exists: %s', 16, 1, @Username);
            RETURN;
        END

        INSERT INTO Admin.Users (Username, PasswordHash, FirstName, LastName, Email, Department, IsActive, CreatedDate, CreatedBy)
        VALUES (@Username, @PasswordHash, @FirstName, @LastName, @Email, @Department, 1, GETDATE(), @CreatedBy);

        SET @UserID = SCOPE_IDENTITY();

        -- Assign role
        INSERT INTO Admin.UserRoles (UserID, RoleID, AssignedDate, AssignedBy)
        VALUES (@UserID, @RoleID, GETDATE(), @CreatedBy);

        -- Audit
        INSERT INTO Audit.AuditLog (TableName, RecordID, Action, Username, ActionDate)
        VALUES ('Admin.Users', @UserID, 'INSERT', @CreatedBy, GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        INSERT INTO Audit.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE());
        THROW;
    END CATCH
END
GO

-- ============================================================
-- SP: Get User Permissions
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_User_GetPermissions
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT p.PermissionCode, p.PermissionName, p.Module
    FROM Admin.Permissions p
    INNER JOIN Admin.RolePermissions rp ON p.PermissionID = rp.PermissionID
    INNER JOIN Admin.UserRoles ur ON rp.RoleID = ur.RoleID
    WHERE ur.UserID = @UserID
    ORDER BY p.Module, p.PermissionCode;
END
GO

-- ============================================================
-- SP: Get/Set System Configuration
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Config_Get
    @ConfigKey VARCHAR(100) = NULL,
    @Category VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ConfigKey, ConfigValue, Description, Category, DataType
    FROM Admin.SystemConfig
    WHERE (@ConfigKey IS NULL OR ConfigKey = @ConfigKey)
        AND (@Category IS NULL OR Category = @Category)
    ORDER BY Category, ConfigKey;
END
GO

CREATE OR ALTER PROCEDURE Admin.usp_Config_Set
    @ConfigKey VARCHAR(100),
    @ConfigValue VARCHAR(500),
    @ModifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldValue VARCHAR(500);
    SELECT @OldValue = ConfigValue FROM Admin.SystemConfig WHERE ConfigKey = @ConfigKey;

    IF @OldValue IS NULL
    BEGIN
        RAISERROR('Configuration key not found: %s', 16, 1, @ConfigKey);
        RETURN;
    END

    UPDATE Admin.SystemConfig SET
        ConfigValue = @ConfigValue,
        ModifiedDate = GETDATE(),
        ModifiedBy = @ModifiedBy
    WHERE ConfigKey = @ConfigKey;

    -- Audit
    INSERT INTO Audit.AuditLog (TableName, RecordID, Action, FieldName, OldValue, NewValue, Username, ActionDate)
    VALUES ('Admin.SystemConfig', 0, 'UPDATE', @ConfigKey, @OldValue, @ConfigValue, @ModifiedBy, GETDATE());
END
GO

-- ============================================================
-- SP: Manage Lookup Values
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Lookup_Create
    @Category VARCHAR(50),
    @LookupCode VARCHAR(30),
    @LookupValue VARCHAR(200),
    @Description VARCHAR(500) = NULL,
    @SortOrder INT = 0,
    @CreatedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CategoryID INT;
    SELECT @CategoryID = CategoryID FROM Admin.LookupCategories WHERE CategoryName = @Category;
    
    IF @CategoryID IS NULL
    BEGIN
        INSERT INTO Admin.LookupCategories (CategoryName) VALUES (@Category);
        SET @CategoryID = SCOPE_IDENTITY();
    END

    IF EXISTS (SELECT 1 FROM Admin.LookupValues WHERE CategoryID = @CategoryID AND LookupCode = @LookupCode)
    BEGIN
        RAISERROR('Lookup already exists: %s / %s', 16, 1, @Category, @LookupCode);
        RETURN;
    END

    INSERT INTO Admin.LookupValues (CategoryID, LookupCode, LookupValue, DisplayOrder, IsActive)
    VALUES (@CategoryID, @LookupCode, @LookupValue, @SortOrder, 1);
END
GO

CREATE OR ALTER PROCEDURE Admin.usp_Lookup_Update
    @Category VARCHAR(50),
    @LookupCode VARCHAR(30),
    @LookupValue VARCHAR(200) = NULL,
    @Description VARCHAR(500) = NULL,
    @SortOrder INT = NULL,
    @IsActive BIT = NULL,
    @ModifiedBy VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CategoryID INT;
    SELECT @CategoryID = CategoryID FROM Admin.LookupCategories WHERE CategoryName = @Category;

    UPDATE Admin.LookupValues SET
        LookupValue = ISNULL(@LookupValue, LookupValue),
        DisplayOrder = ISNULL(@SortOrder, DisplayOrder),
        IsActive = ISNULL(@IsActive, IsActive)
    WHERE CategoryID = @CategoryID AND LookupCode = @LookupCode;

    IF @@ROWCOUNT = 0
        RAISERROR('Lookup not found: %s / %s', 16, 1, @Category, @LookupCode);
END
GO

-- ============================================================
-- SP: View Audit Trail
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_Audit_Search
    @TableName VARCHAR(100) = NULL,
    @RecordID INT = NULL,
    @Username VARCHAR(50) = NULL,
    @Action VARCHAR(20) = NULL,
    @DateFrom DATETIME = NULL,
    @DateTo DATETIME = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 100,
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @DateFrom IS NULL SET @DateFrom = DATEADD(DAY, -30, GETDATE());
    IF @DateTo IS NULL SET @DateTo = GETDATE();

    SELECT @TotalRecords = COUNT(*)
    FROM Audit.AuditLog
    WHERE (@TableName IS NULL OR TableName = @TableName)
        AND (@RecordID IS NULL OR RecordID = @RecordID)
        AND (@Username IS NULL OR Username = @Username)
        AND (@Action IS NULL OR Action = @Action)
        AND ActionDate BETWEEN @DateFrom AND @DateTo;

    SELECT *
    FROM Audit.AuditLog
    WHERE (@TableName IS NULL OR TableName = @TableName)
        AND (@RecordID IS NULL OR RecordID = @RecordID)
        AND (@Username IS NULL OR Username = @Username)
        AND (@Action IS NULL OR Action = @Action)
        AND ActionDate BETWEEN @DateFrom AND @DateTo
    ORDER BY ActionDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ============================================================
-- SP: View Error Log
-- ============================================================
CREATE OR ALTER PROCEDURE Admin.usp_ErrorLog_Search
    @ProcedureName VARCHAR(200) = NULL,
    @DateFrom DATETIME = NULL,
    @DateTo DATETIME = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    IF @DateFrom IS NULL SET @DateFrom = DATEADD(DAY, -7, GETDATE());
    IF @DateTo IS NULL SET @DateTo = GETDATE();

    SELECT *
    FROM Audit.ErrorLog
    WHERE (@ProcedureName IS NULL OR ErrorProcedure LIKE '%' + @ProcedureName + '%')
        AND ErrorDate BETWEEN @DateFrom AND @DateTo
    ORDER BY ErrorDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
