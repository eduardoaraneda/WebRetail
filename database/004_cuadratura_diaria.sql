BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    ALTER TABLE [Reembolsos] DROP CONSTRAINT [CK_Reembolso_Importe];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    ALTER TABLE [Pagos] DROP CONSTRAINT [CK_Pago_Importe];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    ALTER TABLE [SesionesCaja] ADD [FechaOperativa] date NOT NULL DEFAULT '0001-01-01';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    UPDATE SesionesCaja SET FechaOperativa=CONVERT(date,Apertura AT TIME ZONE 'Pacific SA Standard Time');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    CREATE INDEX [IX_SesionesCaja_FechaOperativa_UsuarioId] ON [SesionesCaja] ([FechaOperativa], [UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    EXEC(N'ALTER TABLE [Reembolsos] ADD CONSTRAINT [CK_Reembolso_Importe] CHECK ([Importe]>0 AND [Medio] IN (''Efectivo'',''Tarjeta'',''Transferencia'',''Otros''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    EXEC(N'ALTER TABLE [Pagos] ADD CONSTRAINT [CK_Pago_Importe] CHECK ([Importe]>0 AND [Medio] IN (''Efectivo'',''Tarjeta'',''Transferencia'',''Otros'',''SaldoCambio''))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261117170927_CuadraturaDiaria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261117170927_CuadraturaDiaria', N'11.0.0');
END;

COMMIT;
GO

