-- PROPUESTA: requiere aprobación específica del lote antes de ejecutar en Azure SQL.
-- Solo dbo.Productos.ImagenUrl para IDs 1, 2, 3, 5, 7 y 11.
-- Descripcion y demás datos comerciales se validan, nunca se actualizan.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET LOCK_TIMEOUT 10000;

IF DB_NAME() <> N'TotatechDB'
    THROW 51200, N'Base inesperada. Se requiere TotatechDB.', 1;
IF @@TRANCOUNT <> 0
    THROW 51201, N'Ejecutar sin una transacción exterior.', 1;

DECLARE @Lote TABLE (
    IdProducto int PRIMARY KEY, Nombre nvarchar(max), Descripcion nvarchar(max),
    Precio decimal(18,2), Stock int, IdCategoria int, IdProveedor int,
    ImagenAnterior nvarchar(500) NULL, ImagenNueva nvarchar(500) NOT NULL
);
INSERT INTO @Lote
    (IdProducto, Nombre, Descripcion, Precio, Stock, IdCategoria, IdProveedor, ImagenAnterior, ImagenNueva)
VALUES
    (1, N'asus', N'Inalambrico', 155000.00, 101, 11, 1, NULL, N'/images/categorias/Notebooks/2 compragamer_Imganen_general_56118_Notebook_ASUS_TUF_Gaming_A16.jpg'),
    (2, N'Samsung', N'564', 160000.00, 102, 12, 2, NULL, N'/images/categorias/Celulares/Celular Samsung S26 Ultra 12gb 256gb Cobalt Violet.png'),
    (3, N'Disco ssd', N'65446', 165000.00, 103, 13, 3, NULL, N'/images/categorias/Almacenamiento/snv3s-1000g.png'),
    (5, N'Teclado', N'312131', 175000.00, 105, 15, 5, NULL, N'/images/categorias/Perisfericos/Teclados/1compragamer_Imganen_general_42700_Teclado_Mecanico_.jpg'),
    (7, N'HP', N'3121231', 185000.00, 107, 11, 7, NULL, N'/images/categorias/Notebooks/3compragamer_Imganen_general_56094_Notebook_HP_Victus_Gaming_15_15.jpg'),
    (11, N'Mouse', N'Inalambrico', 12000.00, 20, 15, 6, NULL, N'/uploads/productos/936ad1ba8e8d425492a6fe2058abbe0b.jpg');

DECLARE @Pendientes int, @Filas int = 0;
BEGIN TRY
    BEGIN TRANSACTION;
    -- Estabiliza las seis filas hasta COMMIT/ROLLBACK, sin aceptar diferencias de collation.
    IF (SELECT COUNT_BIG(*) FROM dbo.Productos p WITH (UPDLOCK, HOLDLOCK)
        JOIN @Lote e ON p.IdProducto = e.IdProducto
        WHERE 1 = 1
          AND CONVERT(varbinary(max), p.Nombre) = CONVERT(varbinary(max), e.Nombre)
          AND CONVERT(varbinary(max), p.Descripcion) = CONVERT(varbinary(max), e.Descripcion)
          AND p.Precio = e.Precio AND p.Stock = e.Stock
          AND p.IdCategoria = e.IdCategoria AND p.IdProveedor = e.IdProveedor
          AND (p.ImagenUrl IS NULL OR
               CONVERT(varbinary(max), p.ImagenUrl) = CONVERT(varbinary(max), e.ImagenNueva))) <> 6
        THROW 51202, N'Alguna fila no coincide, no existe o tiene otra imagen. Lote cancelado.', 1;

    SELECT @Pendientes = COUNT(*) FROM dbo.Productos p
        JOIN @Lote e ON p.IdProducto = e.IdProducto WHERE p.ImagenUrl IS NULL;
    -- No completar estados parciales ajenos. Solo admite las seis NULL o las seis ya aplicadas.
    IF @Pendientes NOT IN (0, 6)
        THROW 51203, N'Estado parcial inesperado. No se actualizó ninguna fila.', 1;

    SELECT N'ANTES' AS Etapa, p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock,
           p.IdCategoria, p.IdProveedor, p.ImagenUrl
        FROM dbo.Productos p JOIN @Lote e ON p.IdProducto = e.IdProducto ORDER BY p.IdProducto;
    IF @Pendientes = 6
    BEGIN
        UPDATE p SET ImagenUrl = e.ImagenNueva
            FROM dbo.Productos p JOIN @Lote e ON p.IdProducto = e.IdProducto
            WHERE p.ImagenUrl IS NULL;
        SET @Filas = @@ROWCOUNT;
        IF @Filas <> 6 THROW 51204, N'Cantidad de filas afectadas inesperada.', 1;
    END;

    IF (SELECT COUNT_BIG(*) FROM dbo.Productos p JOIN @Lote e ON p.IdProducto = e.IdProducto
        WHERE CONVERT(varbinary(max), p.ImagenUrl) = CONVERT(varbinary(max), e.ImagenNueva)) <> 6
        THROW 51205, N'El estado posterior no coincide con el lote.', 1;
    SELECT N'DESPUES' AS Etapa, p.IdProducto, p.Nombre, p.Descripcion, p.Precio, p.Stock,
           p.IdCategoria, p.IdProveedor, p.ImagenUrl
        FROM dbo.Productos p JOIN @Lote e ON p.IdProducto = e.IdProducto ORDER BY p.IdProducto;
    COMMIT TRANSACTION;
    SELECT @Filas AS FilasActualizadas;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
