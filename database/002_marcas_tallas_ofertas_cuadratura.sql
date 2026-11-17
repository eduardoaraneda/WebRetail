BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [SesionesCaja] ADD [ObservacionCierre] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [SesionesCaja] ADD [RevisadoPor] nvarchar(450) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [SesionesCaja] ADD [Revision] datetimeoffset NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [DescuentoUnitario] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [OfertaId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [OfertaNombre] nvarchar(120) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD [PrecioLista] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE TABLE [Cuadraturas] (
        [Id] int NOT NULL IDENTITY,
        [SesionCajaId] int NOT NULL,
        [Medio] nvarchar(20) NOT NULL,
        [Ventas] decimal(18,2) NOT NULL,
        [Reembolsos] decimal(18,2) NOT NULL,
        [Fondo] decimal(18,2) NOT NULL,
        [Ingresos] decimal(18,2) NOT NULL,
        [Retiros] decimal(18,2) NOT NULL,
        [Esperado] decimal(18,2) NOT NULL,
        [Contado] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Cuadraturas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cuadraturas_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE TABLE [Marcas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_Marcas] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Marcas_Nombre] UNIQUE ([Nombre])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE TABLE [MovimientosCaja] (
        [Id] bigint NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [SesionCajaId] int NOT NULL,
        [Tipo] nvarchar(10) NOT NULL,
        [Importe] decimal(18,2) NOT NULL,
        [Motivo] nvarchar(250) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_MovimientosCaja] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_MovimientoCaja_Importe] CHECK ([Importe]>0 AND [Tipo] IN ('Ingreso','Retiro')),
        CONSTRAINT [FK_MovimientosCaja_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE TABLE [Tallas] (
        [Id] int NOT NULL IDENTITY,
        [Valor] nvarchar(15) NOT NULL,
        [Sistema] nvarchar(10) NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_Tallas] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Tallas_Valor_Sistema] UNIQUE ([Valor], [Sistema])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE TABLE [Ofertas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        [MarcaId] int NULL,
        [TallaId] int NULL,
        [VarianteId] int NULL,
        [Tipo] nvarchar(15) NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [Inicio] date NOT NULL,
        [Fin] date NOT NULL,
        [Activa] bit NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Creacion] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Ofertas] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Oferta_Fechas] CHECK ([Fin]>=[Inicio]),
        CONSTRAINT [CK_Oferta_Filtro] CHECK ([MarcaId] IS NOT NULL OR [TallaId] IS NOT NULL OR [VarianteId] IS NOT NULL),
        CONSTRAINT [CK_Oferta_Valor] CHECK (([Tipo]='Porcentaje' AND [Valor]>0 AND [Valor]<=100) OR ([Tipo]='Monto' AND [Valor]>0 AND [Valor]=FLOOR([Valor]))),
        CONSTRAINT [FK_Ofertas_Marcas_MarcaId] FOREIGN KEY ([MarcaId]) REFERENCES [Marcas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ofertas_Tallas_TallaId] FOREIGN KEY ([TallaId]) REFERENCES [Tallas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ofertas_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    INSERT INTO Marcas (Nombre, Activa) SELECT DISTINCT Marca, 1 FROM Productos;
    INSERT INTO Tallas (Valor, Sistema, Activa) SELECT DISTINCT Talla, SistemaTalla, 1 FROM Variantes;
    UPDATE DetallesVenta SET PrecioLista = PrecioUnitario;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_Variantes_Talla_SistemaTalla] ON [Variantes] ([Talla], [SistemaTalla]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_Productos_Marca] ON [Productos] ([Marca]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_OfertaId] ON [DetallesVenta] ([OfertaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cuadraturas_SesionCajaId_Medio] ON [Cuadraturas] ([SesionCajaId], [Medio]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_MovimientosCaja_SesionCajaId] ON [MovimientosCaja] ([SesionCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MovimientosCaja_SolicitudId] ON [MovimientosCaja] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_Ofertas_MarcaId] ON [Ofertas] ([MarcaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_Ofertas_TallaId] ON [Ofertas] ([TallaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    CREATE INDEX [IX_Ofertas_VarianteId] ON [Ofertas] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [DetallesVenta] ADD CONSTRAINT [FK_DetallesVenta_Ofertas_OfertaId] FOREIGN KEY ([OfertaId]) REFERENCES [Ofertas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [Productos] ADD CONSTRAINT [FK_Productos_Marcas_Marca] FOREIGN KEY ([Marca]) REFERENCES [Marcas] ([Nombre]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    ALTER TABLE [Variantes] ADD CONSTRAINT [FK_Variantes_Tallas_Talla_SistemaTalla] FOREIGN KEY ([Talla], [SistemaTalla]) REFERENCES [Tallas] ([Valor], [Sistema]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114213757_MarcasTallasOfertasCuadratura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261114213757_MarcasTallasOfertasCuadratura', N'11.0.0');
END;

COMMIT;
GO

