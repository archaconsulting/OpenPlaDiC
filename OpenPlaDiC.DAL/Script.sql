/* 
=============================================================================
   OPENPLADIC - FULL SYSTEM KERNEL SCRIPT
   Version: 1.0 (Production Ready)
=============================================================================
*/

-- 1. CATÁLOGOS Y NÚCLEO DE IDENTIDAD
CREATE TABLE DataType (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    SqlDefinition NVARCHAR(100) NOT NULL
);

CREATE TABLE [User] (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(120) NOT NULL,
    Number INT IDENTITY(1,1) NOT NULL,
    Folio AS 'USR-' + RIGHT('0000000000' + CONVERT(NVARCHAR(10), Number), 10) PERSISTED,
    Email NVARCHAR(320) NOT NULL UNIQUE,
    Username NVARCHAR(320) UNIQUE,
    Password NVARCHAR(MAX) NOT NULL DEFAULT '0000',
    IsMaster BIT NOT NULL DEFAULT 0,
    IsConfirmed BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    PasswordSalt NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NULL
);

CREATE TABLE Profile (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(120) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL
);

CREATE TABLE UserProfile (
    UserId UNIQUEIDENTIFIER NOT NULL REFERENCES [User](Id),
    ProfileId UNIQUEIDENTIFIER NOT NULL REFERENCES Profile(Id),
    PRIMARY KEY (UserId, ProfileId)
);

-- 2. MOTOR DE METADATOS (ENTIDADES Y PROPIEDADES)

CREATE TABLE Entity (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(120) NOT NULL UNIQUE, -- DB Physical Name
    Number INT IDENTITY(1,1) NOT NULL,
    Folio AS 'ENT-' + RIGHT('0000000000' + CONVERT(NVARCHAR(10), Number), 10) PERSISTED,
    Prefix NVARCHAR(10) NOT NULL UNIQUE,
    
    -- UI Configuration
    Label NVARCHAR(120) NOT NULL,
    Icon NVARCHAR(120),
    UseNameField BIT NOT NULL DEFAULT 1,
    NameLabel NVARCHAR(120) NOT NULL DEFAULT 'Name',
    NameHelpText NVARCHAR(480) NOT NULL DEFAULT 'Standard identifier for the record',
    
    -- System Flags
    IsSystem BIT NOT NULL DEFAULT 0,
    IsLocked BIT NOT NULL DEFAULT 0,
    IsVisible BIT NOT NULL DEFAULT 1,
    IsDetail BIT NOT NULL DEFAULT 0,
    IsRelated BIT NOT NULL DEFAULT 1,
    HasControlledAccess BIT NOT NULL DEFAULT 1,
    
    -- Data Engine & Queries
    HasCustomQuery BIT NOT NULL DEFAULT 0,
    ListQuery NVARCHAR(MAX),      -- Custom SELECT for Index views
    RelatedQuery NVARCHAR(MAX),   -- Custom SELECT for Foreign Key lookups
    FieldsQuery NVARCHAR(MAX),    -- Custom SELECT for metadata
    FilterQuery NVARCHAR(MAX),    -- Default WHERE clause
    
    -- Pagination & Limits
    ListMaxRecords INT NOT NULL DEFAULT -1,
    RelatedMaxRecords INT NOT NULL DEFAULT -1,
    
    -- Server-Side Razor Triggers (Executed by Kernel)
    OnBeforeInsert NVARCHAR(MAX) NULL,
    OnAfterInsert NVARCHAR(MAX) NULL,
    OnBeforeUpdate NVARCHAR(MAX) NULL,
    OnAfterUpdate NVARCHAR(MAX) NULL,
    OnBeforeDelete NVARCHAR(MAX) NULL,
    OnAfterDelete NVARCHAR(MAX) NULL,

    -- Audit & State
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL REFERENCES [User](Id)
);




CREATE TABLE EntityProperty (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    EntityId UNIQUEIDENTIFIER NOT NULL REFERENCES Entity(Id) ON DELETE CASCADE,
    Name NVARCHAR(120) NOT NULL,
    Label NVARCHAR(120) NOT NULL,
    DataTypeId INT NOT NULL REFERENCES DataType(Id),
    SourceDefinition NVARCHAR(MAX), -- Para listas o tablas relacionadas
    IsRequired BIT NOT NULL DEFAULT 0,
    IsUnique BIT NOT NULL DEFAULT 0,
    IsIndexed BIT NOT NULL DEFAULT 0,
    AllowCascadeDelete BIT NOT NULL DEFAULT 0,
    -- Layout
    GridRow INT NOT NULL DEFAULT 0,
    GridColumn INT NOT NULL DEFAULT 0,
    OnList BIT NOT NULL DEFAULT 0,
    Sequence INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    UNIQUE(EntityId, Name)
);




-- 3. SEGURIDAD, VISTAS DINÁMICAS Y LOGS
CREATE TABLE DynamicView (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(120) NOT NULL UNIQUE,
    Label NVARCHAR(120) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    ViewType NVARCHAR(10) NOT NULL DEFAULT 'VIEW', -- VIEW, ACTION, API, APIEX, TASK
    -- Scheduler para TASK
    FrequencyMinutes INT DEFAULT 0,
    NextExecutionDateTime DATETIME2,
    AccessLevel INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL
);

CREATE TABLE AccessControl (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ProfileId UNIQUEIDENTIFIER NULL REFERENCES Profile(Id),
    UserId UNIQUEIDENTIFIER NULL REFERENCES [User](Id),
    EntityId UNIQUEIDENTIFIER NULL REFERENCES Entity(Id),
    DynamicViewId UNIQUEIDENTIFIER NULL REFERENCES DynamicView(Id),
    AccessLevel INT NOT NULL DEFAULT 0,
    CanRead BIT NOT NULL DEFAULT 1, CanCreate BIT NOT NULL DEFAULT 0,
    CanUpdate BIT NOT NULL DEFAULT 0, CanDelete BIT NOT NULL DEFAULT 0,
    CanExecute BIT NOT NULL DEFAULT 0,
    CONSTRAINT CHK_AccessTarget CHECK ((ProfileId IS NOT NULL AND UserId IS NULL) OR (ProfileId IS NULL AND UserId IS NOT NULL))
);

CREATE TABLE [Record] (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    EntityId UNIQUEIDENTIFIER NOT NULL REFERENCES Entity(Id),
    Folio NVARCHAR(60) NOT NULL,
    SearchContent NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL
);

CREATE TABLE EventLog (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    Type NVARCHAR(50) NOT NULL,
    EventInfo NVARCHAR(MAX),
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById UNIQUEIDENTIFIER NOT NULL
);

GO

-- 4. PROCEDIMIENTOS ALMACENADOS DEL KERNEL

-- 4.1 Gestión de Índices
CREATE PROCEDURE sp_Core_ManageIndex
    @EntityName NVARCHAR(120), @PropertyName NVARCHAR(120), @ShouldExist BIT, @IsUnique BIT = 0
AS
BEGIN
    DECLARE @IndexName NVARCHAR(200) = CASE WHEN @IsUnique = 1 THEN 'UX_' ELSE 'IX_' END + @EntityName + '_' + @PropertyName;
    IF @ShouldExist = 1 BEGIN
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = @IndexName AND object_id = OBJECT_ID(@EntityName))
        BEGIN
            DECLARE @sql NVARCHAR(MAX) = N'CREATE ' + CASE WHEN @IsUnique = 1 THEN 'UNIQUE ' ELSE '' END + 'INDEX ' + QUOTENAME(@IndexName) + ' ON ' + QUOTENAME(@EntityName) + '(' + QUOTENAME(@PropertyName) + ')';
            EXEC sp_executesql @sql;
        END
    END ELSE BEGIN
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = @IndexName AND object_id = OBJECT_ID(@EntityName))
        BEGIN
            DECLARE @dropSql NVARCHAR(MAX) = N'DROP INDEX ' + QUOTENAME(@IndexName) + ' ON ' + QUOTENAME(@EntityName);
            EXEC sp_executesql @dropSql;
        END
    END
END;
GO

-- 4.2 Borrado Lógico en Cascada
CREATE PROCEDURE sp_Core_ApplySoftDeleteCascade
    @ParentEntityName NVARCHAR(120), @ParentRecordId UNIQUEIDENTIFIER, @DeletedById UNIQUEIDENTIFIER
AS
BEGIN
    DECLARE @ChildTable NVARCHAR(120), @ChildColumn NVARCHAR(120);
    DECLARE cascade_cursor CURSOR FOR 
    SELECT e.Name, ep.Name FROM EntityProperty ep JOIN Entity e ON ep.EntityId = e.Id
    WHERE ep.SourceDefinition = @ParentEntityName AND ep.AllowCascadeDelete = 1 AND ep.DataTypeId IN (10, 19, 20);
    OPEN cascade_cursor; FETCH NEXT FROM cascade_cursor INTO @ChildTable, @ChildColumn;
    WHILE @@FETCH_STATUS = 0 BEGIN
        DECLARE @sql NVARCHAR(MAX) = N'UPDATE ' + QUOTENAME(@ChildTable) + N' SET IsDeleted = 1, UpdatedAt = GETDATE() WHERE ' + QUOTENAME(@ChildColumn) + N' = @ParentId AND IsDeleted = 0';
        EXEC sp_executesql @sql, N'@ParentId UNIQUEIDENTIFIER', @ParentRecordId;
        FETCH NEXT FROM cascade_cursor INTO @ChildTable, @ChildColumn;
    END
    CLOSE cascade_cursor; DEALLOCATE cascade_cursor;
END;
GO


--exec sp_Core_AddProperty 'Proveedor','TipoProveedor','Tipo de proveedor',17,'0:Transportista,1:Otro',1,0,1,1,1,1,0,'00000000-0000-0000-0000-000000000000';


-- 4.3 Crear Propiedad (Campo Dinámico)
CREATE OR ALTER PROCEDURE sp_Core_AddProperty
    @EntityName NVARCHAR(120), @PropertyName NVARCHAR(120), @Label NVARCHAR(120), @DataTypeId INT,
    @RelatedEntityName NVARCHAR(120) = NULL, @IsRequired BIT = 0, @IsUnique BIT = 0, @GridRow INT, @GridColumn INT, @OnList BIT,
    @IsIndexed BIT = 0, @AllowCascadeDelete BIT = 0, @CreatedById UNIQUEIDENTIFIER, @DefaultValue NVARCHAR(120) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EntityId UNIQUEIDENTIFIER = (SELECT Id FROM Entity WHERE Name = @EntityName);
    DECLARE @SqlType NVARCHAR(100) = (SELECT SqlDefinition FROM DataType WHERE Id = @DataTypeId);
    DECLARE @DefVal NVARCHAR(100) = '';



    IF(@IsRequired = 1)
    BEGIN
        IF(@DefaultValue IS NOT NULL)
        BEGIN
            SET @DefVal = ' ' + @DefaultValue +' ';
        END
        ELSE
        BEGIN
            SET @DefVal =
            CASE 
                WHEN @DataTypeId IN (1,2,11,17) THEN ' DEFAULT 0 '
                WHEN @DataTypeId = 0 THEN ' DEFAULT '''' '
                WHEN @DataTypeId IN (3, 103) THEN ' DEFAULT GETDATE() '
                WHEN @DataTypeId = 10 THEN ' '
            END           

        END


    END




    BEGIN TRY
        BEGIN TRANSACTION;




        INSERT INTO EntityProperty (EntityId, Name, Label, DataTypeId, IsRequired, IsUnique, IsIndexed, AllowCascadeDelete, CreatedById, SourceDefinition, GridRow, GridColumn, OnList)
        VALUES (@EntityId, @PropertyName, @Label, @DataTypeId, @IsRequired, @IsUnique, @IsIndexed, @AllowCascadeDelete, @CreatedById, @RelatedEntityName, @GridRow, @GridColumn, @OnList);


        

        DECLARE @sql NVARCHAR(MAX) = N'ALTER TABLE ' + QUOTENAME(@EntityName) + ' ADD ' + QUOTENAME(@PropertyName) + ' ' + @SqlType + CASE WHEN @IsRequired = 1 THEN ' NOT NULL ' + @DefVal ELSE ' NULL' END;
        
        select @sql;

        IF @DataTypeId IN (10, 19, 20) AND @RelatedEntityName IS NOT NULL
            SET @sql += N' CONSTRAINT ' + QUOTENAME('FK_' + @EntityName + '_' + @PropertyName) + ' FOREIGN KEY REFERENCES ' + QUOTENAME(@RelatedEntityName) + '(Id)' + CASE WHEN @AllowCascadeDelete = 1 THEN ' ON DELETE CASCADE' ELSE '' END;
        EXEC sp_executesql @sql;

        IF @IsIndexed = 1 OR @IsUnique = 1 EXEC sp_Core_ManageIndex @EntityName, @PropertyName, 1, @IsUnique;
        COMMIT;
    END TRY BEGIN CATCH ROLLBACK; THROW; END CATCH
END;
GO

-- 5. SEMILLAS (SEED DATA)
INSERT INTO DataType (Id, Name, SqlDefinition) VALUES 
(0, 'String', 'NVARCHAR(120)'), (1, 'Integer', 'INT'), (2, 'Numeric', 'NUMERIC(15,4)'), (3, 'Date', 'DATE'), (103, 'DateTime', 'DATETIME2'), (7, 'LongText', 'NVARCHAR(MAX)'), (11, 'Boolean', 'BIT'), (10, 'RelatedId', 'UNIQUEIDENTIFIER');

INSERT INTO [User] (Id, Name, Email, Username, Password, IsMaster, IsConfirmed, IsActive, CreatedById)
VALUES ('00000000-0000-0000-0000-000000000000', 'Super User', 'admin@openpladic.org', 'admin', '0000', 1, 1, 1, '00000000-0000-0000-0000-000000000000');
GO


INSERT INTO DataType (Id, Name, SqlDefinition) 
VALUES (17, 'ListValue', 'INT'); -- Físicamente en SQL es un entero corto
GO


CREATE INDEX IX_Record_SearchContent ON [Record] (Folio) INCLUDE (SearchContent);

GO

CREATE or ALTER PROCEDURE sp_Core_CreateEntity
    @Name NVARCHAR(128),
    @Label NVARCHAR(128),
    @PageSize INT,
    @Prefix NVARCHAR(5),
    @Icon NVARCHAR(50),
    @CreatedById UNIQUEIDENTIFIER,
    @UseNameField BIT = 1,
    @NameLabel NVARCHAR(120) = 'Nombre'
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Validar si ya existe en la metadata
        IF EXISTS (SELECT 1 FROM Entity WHERE Name = @Name)
        BEGIN
            THROW 50001, 'La entidad ya existe en el Kernel.', 1;
        END

        -- 2. Insertar en la tabla maestra de Metadata
        DECLARE @EntityId UNIQUEIDENTIFIER = NEWID();
        
        INSERT INTO Entity (Id, Name, Label, Prefix, Icon, IsAvailable, IsSystem, CreatedAt, CreatedById, UseNameField, NameLabel, PageSize)
        VALUES (@EntityId, @Name, @Label, @Prefix, @Icon, 1, 0, GETDATE(), @CreatedById, @UseNameField, @NameLabel, @PageSize);

        -- 3. Crear la TABLA FÍSICA con sus columnas base del Kernel
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'CREATE TABLE ' + QUOTENAME(@Name) + ' (
            Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
            Name NVARCHAR(200) NULL,
            Number INT IDENTITY(1,1) NOT NULL,
            Folio AS ''' + UPPER(@Prefix) + '-'' + RIGHT(''0000000000'' + CONVERT(NVARCHAR(10), Number), 10) PERSISTED,
            IsAvailable BIT NOT NULL DEFAULT 1,
            IsDeleted BIT NOT NULL DEFAULT 0,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
            CreatedById UNIQUEIDENTIFIER NOT NULL,
            UpdatedAt DATETIME2 NULL,
            UpdatedById UNIQUEIDENTIFIER NULL
        );';
        
        EXEC sp_executesql @Sql;

        -- 4. Opcional: Insertar el campo 'Name' en EntityProperty si la tabla usará este campo estándar
        -- INSERT INTO EntityProperty ... (dependiendo de tu lógica de UseNameField)

        -- Dentro de sp_Core_CreateEntity, después de crear la tabla física:
        IF @UseNameField = 1
        BEGIN
            INSERT INTO EntityProperty (Id, EntityId, Name, Label, DataTypeId, GridRow, GridColumn, IsRequired, IsVisible, IsEditable, Sequence, CreatedById, OnList)
            VALUES (NEWID(), @EntityId, 'Name', @NameLabel, 1, 1, 1, 1, 1, 1, 0, @CreatedById,1);
        END


        COMMIT TRANSACTION;
        SELECT 1 AS IsSuccess, 'Entidad creada correctamente en el Kernel' AS Message;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        SELECT 0 AS IsSuccess, @ErrMsg AS Message;
    END CATCH
END;
GO

ALTER TABLE EntityProperty add UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
ALTER TABLE EntityProperty add UpdatedById UNIQUEIDENTIFIER;

ALTER TABLE Entity add UpdatedById UNIQUEIDENTIFIER;


GO


CREATE OR ALTER PROCEDURE sp_Core_UpdatePropertyMetadata
    @PropertyId UNIQUEIDENTIFIER,
    @Label NVARCHAR(128),
    @GridRow INT,
    @GridColumn INT,
    @IsRequired BIT,
    @OnList BIT,
    @UpdatedById UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE EntityProperty
        SET Label = @Label,
            GridRow = @GridRow,
            GridColumn = @GridColumn,
            IsRequired = @IsRequired,
            OnList = @OnList,
            UpdatedAt = GETDATE(),
            UpdatedById = @UpdatedById
        WHERE Id = @PropertyId;

        DECLARE @PropertyName varchar(120)

        DECLARE @EntityId UNIQUEIDENTIFIER


        SELECT @PropertyName = UPPER(Name), @EntityId = EntityId FROM EntityProperty WHERE Id = @PropertyId; 

        IF(@PropertyName = 'NAME')
        BEGIN

            UPDATE Entity SET NameLabel = @Label WHERE Id = @EntityId

        END

        SELECT 1 AS IsSuccess, 'Metadata actualizada correctamente' AS Message;
    END TRY
    BEGIN CATCH
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO



ALTER TABLE AccessControl add CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
ALTER TABLE AccessControl add CreatedById UNIQUEIDENTIFIER NOT NULL;

GO





ALTER TABLE DynamicView ADD IsPublic BIT NOT NULL DEFAULT 0;
GO


ALTER TABLE [dbo].[Entity] 
ADD [PageSize] INT NOT NULL DEFAULT 20; -- Nacen protegidas contra sobrecargas
GO

ALTER TABLE EntityProperty ADD IsVisible BIT NOT NULL DEFAULT 1;
ALTER TABLE EntityProperty ADD IsEditable BIT NOT NULL DEFAULT 1;
GO

CREATE TABLE SystemParameter (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Key] NVARCHAR(120) NOT NULL UNIQUE,
    [Value] NVARCHAR(MAX) NULL,
    Description NVARCHAR(MAX) NULL,
    Category NVARCHAR(50) NOT NULL DEFAULT 'GENERAL',
    IsSystem BIT NOT NULL DEFAULT 0,
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedById UNIQUEIDENTIFIER NULL
);

-- Semillas para la configuración de correo
INSERT INTO SystemParameter ([Key], [Value], Description, Category, IsSystem)
VALUES 
('SMTP_HOST', '://example.com', 'Servidor de salida SMTP', 'EMAIL', 1),
('SMTP_PORT', '587', 'Puerto del servidor SMTP', 'EMAIL', 1),
('SMTP_USER', 'notificaciones@openpladic.org', 'Usuario de correo', 'EMAIL', 1),
('SMTP_PASS', 'password_seguro', 'Contraseña de correo', 'EMAIL', 1),
('SMTP_SENDER_NAME', 'OpenPlaDiC Core', 'Nombre que aparece como remitente', 'EMAIL', 1);


GO

CREATE TABLE LoginLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(120) NOT NULL, -- Guardamos el nombre intentado
    UserId UNIQUEIDENTIFIER NULL,    -- Solo si el login fue exitoso
    LoginDate DATETIME2 DEFAULT GETDATE(),
    IPAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    Status NVARCHAR(20), -- 'SUCCESS', 'FAILED', 'LOCKED'
    Message NVARCHAR(MAX)
);

GO

CREATE OR ALTER PROCEDURE sp_Core_DropProperty
    @EntityName NVARCHAR(128),
    @PropertyName NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Obtener el ID de la entidad y de la propiedad
        DECLARE @EntityId UNIQUEIDENTIFIER;
        DECLARE @PropertyId UNIQUEIDENTIFIER;
        DECLARE @ConstraintName NVARCHAR(200);

        SELECT @EntityId = Id FROM Entity WHERE Name = @EntityName;
        SELECT @PropertyId = Id FROM EntityProperty WHERE EntityId = @EntityId AND Name = @PropertyName;

        SELECT @ConstraintName = df.name
        FROM sys.default_constraints df
        INNER JOIN sys.columns c ON df.parent_object_id = c.object_id AND df.parent_column_id = c.column_id
        WHERE df.parent_object_id = OBJECT_ID(@EntityName) AND c.name = @PropertyName;




        IF @PropertyId IS NULL
        BEGIN
            THROW 50001, 'La propiedad no existe en la metadata.', 1;
        END;

        IF @ConstraintName IS NOT NULL
        BEGIN
            DECLARE @SqlD NVARCHAR(MAX);
            SET @SqlD = N'ALTER TABLE '+ QUOTENAME(@EntityName) + ' DROP CONSTRAINT ' + @ConstraintName;

            EXEC(@SqlD);
        END
            



        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = N'ALTER TABLE ' + QUOTENAME(@EntityName) + ' DROP COLUMN ' + QUOTENAME(@PropertyName);
        EXEC sp_executesql @Sql;

        -- 3. Limpiar permisos o registros relacionados en el AccessControl (si aplica)
        -- DELETE FROM AccessControl WHERE EntityPropertyId = @PropertyId;

        -- 4. Eliminar la metadata de la propiedad
        DELETE FROM EntityProperty WHERE Id = @PropertyId;

        COMMIT TRANSACTION;
        SELECT 1 AS IsSuccess, 'Propiedad eliminada correctamente' AS Message;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SELECT 0 AS IsSuccess, @ErrorMessage AS Message;
    END CATCH
END
GO


CREATE PROCEDURE sp_Core_DropEntity
    @EntityName NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Borrar tabla física
        DECLARE @Sql NVARCHAR(MAX) = N'DROP TABLE ' + QUOTENAME(@EntityName);
        EXEC sp_executesql @Sql;

        -- 2. Borrar metadata
        DECLARE @EntityId UNIQUEIDENTIFIER;
        SELECT @EntityId = Id FROM Entity WHERE Name = @EntityName;

        DELETE FROM AccessControl WHERE EntityId = @EntityId;
        DELETE FROM EntityProperty WHERE EntityId = @EntityId;
        DELETE FROM Entity WHERE Id = @EntityId;

        COMMIT TRANSACTION;
        SELECT 1 AS IsSuccess, 'Entidad eliminada del Kernel' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT 0 AS IsSuccess, ERROR_MESSAGE() AS Message;
    END CATCH
END

GO

CREATE TABLE AuditLog (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EntityName NVARCHAR(128) NOT NULL,
    RecordId UNIQUEIDENTIFIER NOT NULL,
    Folio NVARCHAR(50) NULL,
    Action NVARCHAR(20) NOT NULL, -- 'INSERT', 'UPDATE', 'DELETE'
    OldValues NVARCHAR(MAX) NULL, -- JSON
    NewValues NVARCHAR(MAX) NULL, -- JSON
    ChangeDate DATETIME2 DEFAULT GETDATE(),
    UserId UNIQUEIDENTIFIER NOT NULL
);

GO
