-- Aplicado con autorización en Azure el 28/10/2026 a las 17:46:12 UTC.
-- Conservar verificaciones de respaldo y destino antes de una nueva ejecución.
-- Requiere autorización de cambio de esquema y un punto de restauración previo verificado.
-- Destino exclusivo: cancho / TotatechDB.
-- No concilia ni modifica __EFMigrationsHistory.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 10000;

IF DB_NAME() <> N'TotatechDB'
    THROW 51000, 'Destino incorrecto: se requiere TotatechDB.', 1;

IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
    THROW 51001, 'No existe dbo.Productos; detener y revisar el esquema.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @FilasAntes bigint = (SELECT COUNT_BIG(*) FROM dbo.Productos);

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Productos') AND name = N'ImagenUrl'
    )
    BEGIN
        ALTER TABLE dbo.Productos ADD ImagenUrl nvarchar(500) NULL;
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.Productos') AND name = N'ImagenUrl'
          AND system_type_id = TYPE_ID(N'nvarchar')
          AND max_length = 1000 AND is_nullable = 1 AND is_computed = 0
    )
        THROW 51002, 'ImagenUrl no coincide con nvarchar(500) NULL; no se altera una columna existente.', 1;

    IF (SELECT COUNT_BIG(*) FROM dbo.Productos) <> @FilasAntes
        THROW 51003, 'Cambió el conteo de productos durante la operación.', 1;

    -- Compilar después del ALTER, sin devolver datos comerciales.
    EXEC sys.sp_executesql N'SELECT TOP (0) ImagenUrl FROM dbo.Productos;';

    COMMIT TRANSACTION;
    SELECT DB_NAME() AS BaseVerificada, @FilasAntes AS ProductosConservados,
           N'ImagenUrl nvarchar(500) NULL' AS ColumnaVerificada;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
