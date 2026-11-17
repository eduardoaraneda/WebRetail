IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Bodegas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(120) NOT NULL,
        CONSTRAINT [PK_Bodegas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(150) NOT NULL,
        [Rut] nvarchar(12) NULL,
        [Giro] nvarchar(150) NULL,
        [Direccion] nvarchar(200) NULL,
        [Comuna] nvarchar(100) NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Productos] (
        [Id] int NOT NULL IDENTITY,
        [Marca] nvarchar(100) NOT NULL,
        [Modelo] nvarchar(150) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [SesionesCaja] (
        [Id] int NOT NULL IDENTITY,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Apertura] datetimeoffset NOT NULL,
        [Cierre] datetimeoffset NULL,
        [FondoInicial] decimal(18,2) NOT NULL,
        [EfectivoEsperado] decimal(18,2) NULL,
        [EfectivoContado] decimal(18,2) NULL,
        CONSTRAINT [PK_SesionesCaja] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Variantes] (
        [Id] int NOT NULL IDENTITY,
        [ProductoId] int NOT NULL,
        [Sku] nvarchar(60) NOT NULL,
        [CodigoBarras] nvarchar(60) NULL,
        [Color] nvarchar(60) NOT NULL,
        [Talla] nvarchar(15) NOT NULL,
        [SistemaTalla] nvarchar(10) NOT NULL,
        [Precio] decimal(18,2) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Variantes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Variantes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Ventas] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [BodegaId] int NOT NULL,
        [ClienteId] int NULL,
        [SesionCajaId] int NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [MedioPago] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Ventas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ventas_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ventas_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Existencias] (
        [VarianteId] int NOT NULL,
        [BodegaId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Existencias] PRIMARY KEY ([VarianteId], [BodegaId]),
        CONSTRAINT [CK_Existencia_Cantidad] CHECK ([Cantidad] >= 0),
        CONSTRAINT [FK_Existencias_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Existencias_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Movimientos] (
        [Id] bigint NOT NULL IDENTITY,
        [VarianteId] int NOT NULL,
        [BodegaId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [Motivo] nvarchar(250) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [VentaId] int NULL,
        CONSTRAINT [PK_Movimientos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Movimientos_Bodegas_BodegaId] FOREIGN KEY ([BodegaId]) REFERENCES [Bodegas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Movimientos_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [DetallesVenta] (
        [Id] int NOT NULL IDENTITY,
        [VentaId] int NOT NULL,
        [VarianteId] int NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetallesVenta] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetallesVenta_Variantes_VarianteId] FOREIGN KEY ([VarianteId]) REFERENCES [Variantes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_DetallesVenta_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Ventas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Documentos] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [VentaId] int NOT NULL,
        [Tipo] int NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        [ReferenciaId] int NULL,
        [Motivo] nvarchar(250) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [Estado] nvarchar(30) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_Documentos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Documentos_Documentos_ReferenciaId] FOREIGN KEY ([ReferenciaId]) REFERENCES [Documentos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documentos_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Ventas] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [DetalleDocumento] (
        [Id] int NOT NULL IDENTITY,
        [DocumentoId] int NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Cantidad] int NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DetalleDocumento] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DetalleDocumento_Documentos_DocumentoId] FOREIGN KEY ([DocumentoId]) REFERENCES [Documentos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE TABLE [Devoluciones] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] uniqueidentifier NOT NULL,
        [DetalleVentaId] int NOT NULL,
        [Cantidad] int NOT NULL,
        [ReingresarStock] bit NOT NULL,
        [DocumentoId] int NOT NULL,
        [SesionCajaId] int NOT NULL,
        [Reembolso] decimal(18,2) NOT NULL,
        [MedioReembolso] nvarchar(20) NOT NULL,
        [UsuarioId] nvarchar(450) NOT NULL,
        [Fecha] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Devoluciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Devoluciones_DetallesVenta_DetalleVentaId] FOREIGN KEY ([DetalleVentaId]) REFERENCES [DetallesVenta] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Devoluciones_Documentos_DocumentoId] FOREIGN KEY ([DocumentoId]) REFERENCES [Documentos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Devoluciones_SesionesCaja_SesionCajaId] FOREIGN KEY ([SesionCajaId]) REFERENCES [SesionesCaja] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bodegas_Nombre] ON [Bodegas] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetalleDocumento_DocumentoId] ON [DetalleDocumento] ([DocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_VarianteId] ON [DetallesVenta] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_DetallesVenta_VentaId] ON [DetallesVenta] ([VentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Devoluciones_DetalleVentaId] ON [Devoluciones] ([DetalleVentaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Devoluciones_DocumentoId] ON [Devoluciones] ([DocumentoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Devoluciones_SesionCajaId] ON [Devoluciones] ([SesionCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Devoluciones_SolicitudId] ON [Devoluciones] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Documentos_ReferenciaId] ON [Documentos] ([ReferenciaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Documentos_SolicitudId] ON [Documentos] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Documentos_VentaId_Tipo] ON [Documentos] ([VentaId], [Tipo]) WHERE [Tipo] IN (33, 39, 52)');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Existencias_BodegaId] ON [Existencias] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Movimientos_BodegaId] ON [Movimientos] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Movimientos_VarianteId] ON [Movimientos] ([VarianteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_SesionesCaja_UsuarioId] ON [SesionesCaja] ([UsuarioId]) WHERE [Cierre] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Variantes_CodigoBarras] ON [Variantes] ([CodigoBarras]) WHERE [CodigoBarras] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Variantes_ProductoId_Color_Talla_SistemaTalla] ON [Variantes] ([ProductoId], [Color], [Talla], [SistemaTalla]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Variantes_Sku] ON [Variantes] ([Sku]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_BodegaId] ON [Ventas] ([BodegaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_ClienteId] ON [Ventas] ([ClienteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE INDEX [IX_Ventas_SesionCajaId] ON [Ventas] ([SesionCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ventas_SolicitudId] ON [Ventas] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261114211238_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261114211238_Inicial', N'11.0.0');
END;

COMMIT;
GO

