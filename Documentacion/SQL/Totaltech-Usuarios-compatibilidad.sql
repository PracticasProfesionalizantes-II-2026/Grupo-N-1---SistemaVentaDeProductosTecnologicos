-- Preparado para revisión. Requiere autorización específica y respaldo previo.
-- Replica solo GestionAdministrativaUsuarios; no ejecuta migraciones ni altera su historial.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 10000;

IF DB_NAME() <> N'TotatechDB'
    THROW 51100, 'Se requiere la base TotatechDB.', 1;
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL OR NOT EXISTS (
    SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Usuarios')
      AND name=N'IdUsuario' AND system_type_id=TYPE_ID(N'int') AND is_nullable=0
)
    THROW 51101, 'Usuarios no tiene la estructura previa esperada.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @UsuariosAntes bigint = (SELECT COUNT_BIG(*) FROM dbo.Usuarios);
    DECLARE @RolesAntes bigint = (SELECT SUM(CONVERT(bigint, Rol)) FROM dbo.Usuarios);
    DECLARE @ActivoExiste bit = CASE WHEN COL_LENGTH(N'dbo.Usuarios',N'Activo') IS NULL THEN 0 ELSE 1 END;
    DECLARE @VersionExiste bit = CASE WHEN COL_LENGTH(N'dbo.Usuarios',N'VersionSesion') IS NULL THEN 0 ELSE 1 END;
    DECLARE @AuditoriaExiste bit = CASE WHEN OBJECT_ID(N'dbo.AuditoriaUsuarios',N'U') IS NULL THEN 0 ELSE 1 END;
    IF NOT ((@ActivoExiste=0 AND @VersionExiste=0 AND @AuditoriaExiste=0)
         OR (@ActivoExiste=1 AND @VersionExiste=1 AND @AuditoriaExiste=1))
        THROW 51102, 'Esquema parcialmente actualizado: revisar antes de ejecutar.', 1;

    IF @ActivoExiste=0
    BEGIN
        -- Los registros históricos reciben los defaults del modelo, sin cambiar sus roles.
        ALTER TABLE dbo.Usuarios ADD Activo bit NOT NULL
            CONSTRAINT DF_Usuarios_Activo_Totaltech DEFAULT (CONVERT(bit,1));
        ALTER TABLE dbo.Usuarios ADD VersionSesion int NOT NULL
            CONSTRAINT DF_Usuarios_VersionSesion_Totaltech DEFAULT (1);
        CREATE TABLE dbo.AuditoriaUsuarios (
            IdAuditoriaUsuario int NOT NULL IDENTITY(1,1),
            IdActor int NOT NULL,
            IdUsuario int NOT NULL,
            Accion nvarchar(32) NOT NULL,
            FechaUtc datetime2 NOT NULL,
            CamposModificados nvarchar(256) NOT NULL,
            RolAnterior int NULL,
            RolNuevo int NULL,
            ActivoAnterior bit NULL,
            ActivoNuevo bit NULL,
            CONSTRAINT PK_AuditoriaUsuarios PRIMARY KEY (IdAuditoriaUsuario),
            CONSTRAINT FK_AuditoriaUsuarios_Usuarios_IdActor FOREIGN KEY (IdActor)
                REFERENCES dbo.Usuarios(IdUsuario) ON DELETE NO ACTION,
            CONSTRAINT FK_AuditoriaUsuarios_Usuarios_IdUsuario FOREIGN KEY (IdUsuario)
                REFERENCES dbo.Usuarios(IdUsuario) ON DELETE NO ACTION
        );
        CREATE INDEX IX_AuditoriaUsuarios_IdActor ON dbo.AuditoriaUsuarios(IdActor);
        CREATE INDEX IX_AuditoriaUsuarios_IdUsuario ON dbo.AuditoriaUsuarios(IdUsuario);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Usuarios')
          AND name=N'Activo' AND system_type_id=TYPE_ID(N'bit') AND is_nullable=0)
       OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Usuarios')
          AND name=N'VersionSesion' AND system_type_id=TYPE_ID(N'int') AND is_nullable=0)
        THROW 51103, 'Los tipos de Activo/VersionSesion no coinciden con el modelo.', 1;

    DECLARE @Columnas TABLE (Nombre sysname, Tipo sysname, Longitud smallint, Nullable bit);
    INSERT INTO @Columnas VALUES
        (N'IdAuditoriaUsuario',N'int',4,0),(N'IdActor',N'int',4,0),(N'IdUsuario',N'int',4,0),
        (N'Accion',N'nvarchar',64,0),(N'FechaUtc',N'datetime2',8,0),
        (N'CamposModificados',N'nvarchar',512,0),(N'RolAnterior',N'int',4,1),
        (N'RolNuevo',N'int',4,1),(N'ActivoAnterior',N'bit',1,1),(N'ActivoNuevo',N'bit',1,1);
    IF EXISTS (SELECT 1 FROM @Columnas e LEFT JOIN sys.columns c
        ON c.object_id=OBJECT_ID(N'dbo.AuditoriaUsuarios') AND c.name=e.Nombre
        WHERE c.column_id IS NULL OR TYPE_NAME(c.system_type_id)<>e.Tipo
           OR c.max_length<>e.Longitud OR c.is_nullable<>e.Nullable)
        THROW 51104, 'AuditoriaUsuarios no coincide con la estructura esperada.', 1;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.AuditoriaUsuarios')
        AND name=N'IdAuditoriaUsuario' AND is_identity=1)
        THROW 51105, 'Falta la identidad de AuditoriaUsuarios.', 1;
    IF (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.AuditoriaUsuarios')
        AND referenced_object_id=OBJECT_ID(N'dbo.Usuarios') AND is_disabled=0 AND is_not_trusted=0
        AND delete_referential_action=0
        AND name IN (N'FK_AuditoriaUsuarios_Usuarios_IdActor',N'FK_AuditoriaUsuarios_Usuarios_IdUsuario')) <> 2
        THROW 51106, 'Revisar claves foráneas de auditoría.', 1;
    IF (SELECT COUNT(*) FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.AuditoriaUsuarios')
        AND name IN (N'IX_AuditoriaUsuarios_IdActor',N'IX_AuditoriaUsuarios_IdUsuario') AND is_disabled=0) <> 2
        THROW 51107, 'Revisar índices de auditoría.', 1;
    IF (SELECT COUNT_BIG(*) FROM dbo.Usuarios)<>@UsuariosAntes
       OR ISNULL((SELECT SUM(CONVERT(bigint,Rol)) FROM dbo.Usuarios),0)<>ISNULL(@RolesAntes,0)
        THROW 51108, 'Cambió el conteo de usuarios o sus roles.', 1;
    EXEC sys.sp_executesql N'SELECT TOP (0) Activo,VersionSesion FROM dbo.Usuarios;';
    COMMIT TRANSACTION;
    SELECT @UsuariosAntes AS UsuariosConservados, N'Compatibilidad de usuarios verificada' AS Resultado;
END TRY
BEGIN CATCH
    IF XACT_STATE()<>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
