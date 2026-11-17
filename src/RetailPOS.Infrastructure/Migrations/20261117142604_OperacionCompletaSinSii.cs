using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailPOS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261117142604_OperacionCompletaSinSii : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ComunaCliente",
            table: "Ventas",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DireccionCliente",
            table: "Ventas",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "GiroCliente",
            table: "Ventas",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NombreCliente",
            table: "Ventas",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "RutCliente",
            table: "Ventas",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "CostoPromedio",
            table: "Existencias",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "StockMinimo",
            table: "Existencias",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<decimal>(
            name: "CostoUnitario",
            table: "DetallesVenta",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MarcaVenta",
            table: "DetallesVenta",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Tienda",
            table: "Bodegas",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<bool>(
            name: "Activo",
            table: "AspNetUsers",
            type: "bit",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateTable(
            name: "Auditoria",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Registro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Campos = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Auditoria", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Cambios",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DevolucionId = table.Column<int>(type: "int", nullable: false),
                VentaNuevaId = table.Column<int>(type: "int", nullable: false),
                Diferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cambios", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cambios_Devoluciones_DevolucionId",
                    column: x => x.DevolucionId,
                    principalTable: "Devoluciones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Cambios_Ventas_VentaNuevaId",
                    column: x => x.VentaNuevaId,
                    principalTable: "Ventas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "OperacionesStock",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                VarianteId = table.Column<int>(type: "int", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                DestinoId = table.Column<int>(type: "int", nullable: true),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                ConteoFisico = table.Column<int>(type: "int", nullable: true),
                Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OperacionesStock", x => x.Id);
                table.ForeignKey(
                    name: "FK_OperacionesStock_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_OperacionesStock_Bodegas_DestinoId",
                    column: x => x.DestinoId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_OperacionesStock_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Pagos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VentaId = table.Column<int>(type: "int", nullable: false),
                DocumentoId = table.Column<int>(type: "int", nullable: true),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                Medio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pagos", x => x.Id);
                table.CheckConstraint("CK_Pago_Importe", "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','SaldoCambio')");
                table.ForeignKey(
                    name: "FK_Pagos_Documentos_DocumentoId",
                    column: x => x.DocumentoId,
                    principalTable: "Documentos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Pagos_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Pagos_Ventas_VentaId",
                    column: x => x.VentaId,
                    principalTable: "Ventas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Proveedores",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                Contacto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Activo = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Proveedores", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Reembolsos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DevolucionId = table.Column<int>(type: "int", nullable: false),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                Medio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reembolsos", x => x.Id);
                table.CheckConstraint("CK_Reembolso_Importe", "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia')");
                table.ForeignKey(
                    name: "FK_Reembolsos_Devoluciones_DevolucionId",
                    column: x => x.DevolucionId,
                    principalTable: "Devoluciones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Reembolsos_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Respaldos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Ruta = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                RestauracionProbada = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Estado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Respaldos", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RevisionesStock",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VarianteId = table.Column<int>(type: "int", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                DevolucionId = table.Column<int>(type: "int", nullable: true),
                Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Resolucion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RevisionesStock", x => x.Id);
                table.ForeignKey(
                    name: "FK_RevisionesStock_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RevisionesStock_Devoluciones_DevolucionId",
                    column: x => x.DevolucionId,
                    principalTable: "Devoluciones",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RevisionesStock_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Compras",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ProveedorId = table.Column<int>(type: "int", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                Referencia = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Compras", x => x.Id);
                table.ForeignKey(
                    name: "FK_Compras_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Compras_Proveedores_ProveedorId",
                    column: x => x.ProveedorId,
                    principalTable: "Proveedores",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DetalleCompra",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CompraId = table.Column<int>(type: "int", nullable: false),
                VarianteId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                CostoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DetalleCompra", x => x.Id);
                table.ForeignKey(
                    name: "FK_DetalleCompra_Compras_CompraId",
                    column: x => x.CompraId,
                    principalTable: "Compras",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DetalleCompra_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Cambios_DevolucionId",
            table: "Cambios",
            column: "DevolucionId");

        migrationBuilder.CreateIndex(
            name: "IX_Cambios_SolicitudId",
            table: "Cambios",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Cambios_VentaNuevaId",
            table: "Cambios",
            column: "VentaNuevaId");

        migrationBuilder.CreateIndex(
            name: "IX_Compras_BodegaId",
            table: "Compras",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_Compras_ProveedorId_Referencia",
            table: "Compras",
            columns: new[] { "ProveedorId", "Referencia" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Compras_SolicitudId",
            table: "Compras",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DetalleCompra_CompraId",
            table: "DetalleCompra",
            column: "CompraId");

        migrationBuilder.CreateIndex(
            name: "IX_DetalleCompra_VarianteId",
            table: "DetalleCompra",
            column: "VarianteId");

        migrationBuilder.CreateIndex(
            name: "IX_OperacionesStock_BodegaId",
            table: "OperacionesStock",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_OperacionesStock_DestinoId",
            table: "OperacionesStock",
            column: "DestinoId");

        migrationBuilder.CreateIndex(
            name: "IX_OperacionesStock_SolicitudId",
            table: "OperacionesStock",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OperacionesStock_VarianteId",
            table: "OperacionesStock",
            column: "VarianteId");

        migrationBuilder.CreateIndex(
            name: "IX_Pagos_DocumentoId",
            table: "Pagos",
            column: "DocumentoId");

        migrationBuilder.CreateIndex(
            name: "IX_Pagos_SesionCajaId",
            table: "Pagos",
            column: "SesionCajaId");

        migrationBuilder.CreateIndex(
            name: "IX_Pagos_SolicitudId",
            table: "Pagos",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Pagos_VentaId",
            table: "Pagos",
            column: "VentaId");

        migrationBuilder.CreateIndex(
            name: "IX_Reembolsos_DevolucionId",
            table: "Reembolsos",
            column: "DevolucionId");

        migrationBuilder.CreateIndex(
            name: "IX_Reembolsos_SesionCajaId",
            table: "Reembolsos",
            column: "SesionCajaId");

        migrationBuilder.CreateIndex(
            name: "IX_RevisionesStock_BodegaId",
            table: "RevisionesStock",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_RevisionesStock_DevolucionId",
            table: "RevisionesStock",
            column: "DevolucionId",
            unique: true,
            filter: "[DevolucionId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_RevisionesStock_VarianteId",
            table: "RevisionesStock",
            column: "VarianteId");

        migrationBuilder.Sql("""
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
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Auditoria");

        migrationBuilder.DropTable(
            name: "Cambios");

        migrationBuilder.DropTable(
            name: "DetalleCompra");

        migrationBuilder.DropTable(
            name: "OperacionesStock");

        migrationBuilder.DropTable(
            name: "Pagos");

        migrationBuilder.DropTable(
            name: "Reembolsos");

        migrationBuilder.DropTable(
            name: "Respaldos");

        migrationBuilder.DropTable(
            name: "RevisionesStock");

        migrationBuilder.DropTable(
            name: "Compras");

        migrationBuilder.DropTable(
            name: "Proveedores");

        migrationBuilder.DropColumn(
            name: "ComunaCliente",
            table: "Ventas");

        migrationBuilder.DropColumn(
            name: "DireccionCliente",
            table: "Ventas");

        migrationBuilder.DropColumn(
            name: "GiroCliente",
            table: "Ventas");

        migrationBuilder.DropColumn(
            name: "NombreCliente",
            table: "Ventas");

        migrationBuilder.DropColumn(
            name: "RutCliente",
            table: "Ventas");

        migrationBuilder.DropColumn(
            name: "CostoPromedio",
            table: "Existencias");

        migrationBuilder.DropColumn(
            name: "StockMinimo",
            table: "Existencias");

        migrationBuilder.DropColumn(
            name: "CostoUnitario",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "MarcaVenta",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "Tienda",
            table: "Bodegas");

        migrationBuilder.DropColumn(
            name: "Activo",
            table: "AspNetUsers");
    }
}

