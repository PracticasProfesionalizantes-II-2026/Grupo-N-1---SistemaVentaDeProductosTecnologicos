-- PROPUESTA: requiere aprobación específica antes de ejecutar en Azure SQL.
-- Único cambio: dbo.Productos.ImagenUrl, IdProducto=10. No ejecutar migraciones.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 10000;

IF DB_NAME() <> N'TotatechDB'
    THROW 51000, N'Base inesperada. Se requiere TotatechDB.', 1;
IF @@TRANCOUNT <> 0
    THROW 51001, N'Ejecutar sin una transacción exterior.', 1;

DECLARE @Imagen nvarchar(500) = N'/uploads/productos/dcd308fe347a4dcc86a2d59fb9d50098.webp';
BEGIN TRY
    BEGIN TRANSACTION;
    -- UPDLOCK/HOLDLOCK mantiene estable la fila hasta COMMIT o ROLLBACK.
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Productos WITH (UPDLOCK, HOLDLOCK)
        WHERE IdProducto = 10
          AND CONVERT(varbinary(max), Nombre) = CONVERT(varbinary(max), N'asus vivobook')
          AND CONVERT(varbinary(max), Descripcion) = CONVERT(varbinary(max), N'87997')
          AND Precio = 200000.00 AND Stock = 110 AND IdCategoria = 11 AND IdProveedor = 10
          AND (ImagenUrl IS NULL OR CONVERT(varbinary(max), ImagenUrl) = CONVERT(varbinary(max), @Imagen))
    )
        THROW 51002, N'El producto cambió, no existe o tiene otra imagen. No se actualizó.', 1;

    SELECT N'ANTES' AS Etapa, IdProducto, Nombre, Descripcion, Precio, Stock,
           IdCategoria, IdProveedor, ImagenUrl FROM dbo.Productos WHERE IdProducto = 10;
    IF EXISTS (SELECT 1 FROM dbo.Productos WHERE IdProducto = 10 AND ImagenUrl IS NULL)
    BEGIN
        UPDATE dbo.Productos SET ImagenUrl = @Imagen WHERE IdProducto = 10 AND ImagenUrl IS NULL;
        IF @@ROWCOUNT <> 1 THROW 51003, N'Cantidad de filas inesperada.', 1;
    END;
    SELECT N'DESPUES' AS Etapa, IdProducto, Nombre, Descripcion, Precio, Stock,
           IdCategoria, IdProveedor, ImagenUrl FROM dbo.Productos WHERE IdProducto = 10;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
