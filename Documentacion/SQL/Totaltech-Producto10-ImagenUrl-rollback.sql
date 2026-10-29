-- ROLLBACK PROPUESTO: no ejecutar salvo necesidad y autorización del procedimiento.
-- Solo revierte la asociación conocida; no elimina archivos ni restaura la base completa.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 10000;
IF DB_NAME() <> N'TotatechDB'
    THROW 51100, N'Base inesperada. Se requiere TotatechDB.', 1;
IF @@TRANCOUNT <> 0
    THROW 51101, N'Ejecutar sin una transacción exterior.', 1;

DECLARE @Imagen nvarchar(500) = N'/uploads/productos/dcd308fe347a4dcc86a2d59fb9d50098.webp';
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.Productos WITH (UPDLOCK, HOLDLOCK)
        WHERE IdProducto = 10
          AND CONVERT(varbinary(max), Nombre) = CONVERT(varbinary(max), N'asus vivobook')
          AND CONVERT(varbinary(max), Descripcion) = CONVERT(varbinary(max), N'87997')
          AND Precio = 200000.00 AND Stock = 110 AND IdCategoria = 11 AND IdProveedor = 10
          AND CONVERT(varbinary(max), ImagenUrl) = CONVERT(varbinary(max), @Imagen)
    )
        THROW 51102, N'El producto o su asociación cambió. Rollback cancelado.', 1;
    UPDATE dbo.Productos SET ImagenUrl = NULL WHERE IdProducto = 10;
    IF @@ROWCOUNT <> 1 THROW 51103, N'Cantidad de filas inesperada.', 1;
    SELECT IdProducto, Nombre, Precio, Stock, IdCategoria, IdProveedor, ImagenUrl
        FROM dbo.Productos WHERE IdProducto = 10;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
