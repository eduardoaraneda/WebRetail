BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Ventas] ADD [ComunaCliente] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Ventas] ADD [DireccionCliente] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Ventas] ADD [GiroCliente] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Ventas] ADD [NombreCliente] nvarchar(150) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Ventas] ADD [RutCliente] nvarchar(12) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Existencias] ADD [CostoPromedio] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Existencias] ADD [StockMinimo] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [CostoUnitario] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [MarcaVenta] nvarchar(100) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [Bodegas] ADD [Tienda] nvarchar(100) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Auditoria] (
        [Id] bigint NOT NULL IDENTITY,
        [Fecha] datetimeoffset NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Entidad] nvarchar(100) NOT NULL,
        [Registro] nvarchar(100) NOT NULL,
        [Accion] nvarchar(50) NOT NULL,
        [Campos] nvarchar(2000) NOT NULL,
        CONSTRAINT [PK_Auditoria] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Cambios] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [DevolucionId] int NOT NULL,
        [VentaNuevaId] int NOT NULL,
        [Diferencia] decimal(18,2) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Cambios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cambios_Devoluciones_DevolucionId] FOREIGN KEY ([DevolucionId]) REFERENCES [Devoluciones] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cambios_Ventas_VentaNuevaId] FOREIGN KEY ([VentaNuevaId]) REFERENCES [Ventas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [OperacionesStock] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [Tipo] nvarchar(20) NOT NULL,
        [VarianteId] int NOT NULL,
        [BodegaId] int NOT NULL,
        [DestinoId] int NULL,
        [Cantidad] int NOT NULL,
        [ConteoFisico] int NULL,
        [Motivo] nvarchar(250) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_OperacionesStock] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OperacionesStock_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperacionesStock_Bodegas_DestinoId] FOREIGN KEY ([DestinoId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OperacionesStock_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Pagos] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [VentaId] int NOT NULL,
        [DocumentoId] int NULL,
        [SesionCajaId] int NOT NULL,
        [Medio] nvarchar(20) NOT NULL,
        [Importe] decimal(18,2) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Pagos] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Pago_Importe] CHECK ([Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','SaldoCambio')),
        CONSTRAINT [FK_Pagos_Documentos_DocumentoId] FOREIGN KEY ([DocumentoId]) REFERENCES [Documentos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pagos_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Pagos_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Ventas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Proveedores] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(150) NOT NULL,
        [Rut] nvarchar(12) NULL,
        [Contacto] nvarchar(200) NULL,
        [Direccion] nvarchar(200) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Reembolsos] (
        [Id] int NOT NULL IDENTITY,
        [DevolucionId] int NOT NULL,
        [SesionCajaId] int NOT NULL,
        [Medio] nvarchar(20) NOT NULL,
        [Importe] decimal(18,2) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Reembolsos] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Reembolso_Importe] CHECK ([Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia')),
        CONSTRAINT [FK_Reembolsos_Devoluciones_DevolucionId] FOREIGN KEY ([DevolucionId]) REFERENCES [Devoluciones] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reembolsos_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Respaldos] (
        [Id] int NOT NULL IDENTITY,
        [Ruta] nvarchar(500) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [RestauracionProbada] datetimeoffset NULL,
        [Estado] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Respaldos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [RevisionesStock] (
        [Id] int NOT NULL IDENTITY,
        [VarianteId] int NOT NULL,
        [BodegaId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [CostoUnitario] decimal(18,2) NULL,
        [DevolucionId] int NULL,
        [Estado] nvarchar(20) NOT NULL,
        [Motivo] nvarchar(250) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [Resolucion] datetimeoffset NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_RevisionesStock] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RevisionesStock_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RevisionesStock_Devoluciones_DevolucionId] FOREIGN KEY ([DevolucionId]) REFERENCES [Devoluciones] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RevisionesStock_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [Compras] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [ProveedorId] int NOT NULL,
        [BodegaId] int NOT NULL,
        [Referencia] nvarchar(80) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Compras] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Compras_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Compras_Proveedores_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedores] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE TABLE [DetalleCompra] (
        [Id] int NOT NULL IDENTITY,
        [CompraId] int NOT NULL,
        [VarianteId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [CostoUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetalleCompra] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetalleCompra_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetalleCompra_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Cambios_DevolucionId] ON [Cambios] ([DevolucionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cambios_SolicitudId] ON [Cambios] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Cambios_VentaNuevaId] ON [Cambios] ([VentaNuevaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Compras_BodegaId] ON [Compras] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Compras_ProveedorId_Referencia] ON [Compras] ([ProveedorId], [Referencia]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Compras_SolicitudId] ON [Compras] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_DetalleCompra_CompraId] ON [DetalleCompra] ([CompraId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_DetalleCompra_VarianteId] ON [DetalleCompra] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_OperacionesStock_BodegaId] ON [OperacionesStock] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_OperacionesStock_DestinoId] ON [OperacionesStock] ([DestinoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OperacionesStock_SolicitudId] ON [OperacionesStock] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_OperacionesStock_VarianteId] ON [OperacionesStock] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Pagos_DocumentoId] ON [Pagos] ([DocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Pagos_SesionCajaId] ON [Pagos] ([SesionCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pagos_SolicitudId] ON [Pagos] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Pagos_VentaId] ON [Pagos] ([VentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Reembolsos_DevolucionId] ON [Reembolsos] ([DevolucionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_Reembolsos_SesionCajaId] ON [Reembolsos] ([SesionCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_RevisionesStock_BodegaId] ON [RevisionesStock] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RevisionesStock_DevolucionId] ON [RevisionesStock] ([DevolucionId]) WHERE [DevolucionId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    CREATE INDEX [IX_RevisionesStock_VarianteId] ON [RevisionesStock] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    UPDATE Bodegas SET Tienda=N'Principal' WHERE Tienda=N'';
    UPDATE d SET MarcaVenta=p.Marca FROM DetallesVenta d JOIN Variantes v ON v.Id=d.VarianteId JOIN Productos p ON p.Id=v.ProductoId;
    UPDATE v SET NombreCliente=COALESCE(c.Nombre,N'Consumidor final'),RutCliente=c.Rut,GiroCliente=c.Giro,DireccionCliente=c.Direccion,ComunaCliente=c.Comuna
        FROM Ventas v LEFT JOIN Clientes c ON c.Id=v.ClienteId;
    INSERT Pagos (SolicitudId,VentaId,SesionCajaId,Medio,Importe,Fecha)
        SELECT NEWID(),Id,SesionCajaId,MedioPago,Total,Fecha FROM Ventas WHERE Total>0;
    INSERT Reembolsos (DevolucionId,SesionCajaId,Medio,Importe,Fecha)
        SELECT Id,SesionCajaId,MedioReembolso,Reembolso,Fecha FROM Devoluciones WHERE Reembolso>0;
    INSERT RevisionesStock (VarianteId,BodegaId,Cantidad,DevolucionId,Estado,Motivo,UsuarioId,Fecha)
        SELECT dv.VarianteId,v.BodegaId,d.Cantidad,d.Id,N'Revision',N'Devolución anterior pendiente de clasificación',d.UsuarioId,d.Fecha
        FROM Devoluciones d JOIN DetallesVenta dv ON dv.Id=d.DetalleVentaId JOIN Ventas v ON v.Id=dv.VentaId WHERE d.ReingresarStock=0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117142604_OperacionCompletaSinSii'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117142604_OperacionCompletaSinSii', N'11.0.0');
END;

COMMIT;
GO

