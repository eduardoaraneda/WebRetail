using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailPOS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261114213757_MarcasTallasOfertasCuadratura : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ObservacionCierre",
            table: "SesionesCaja",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "RevisadoPor",
            table: "SesionesCaja",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "Revision",
            table: "SesionesCaja",
            type: "datetimeoffset",
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "DescuentoUnitario",
            table: "DetallesVenta",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<int>(
            name: "OfertaId",
            table: "DetallesVenta",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OfertaNombre",
            table: "DetallesVenta",
            type: "nvarchar(120)",
            maxLength: 120,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "PrecioLista",
            table: "DetallesVenta",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.CreateTable(
            name: "Cuadraturas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                Medio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Ventas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Reembolsos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Fondo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Ingresos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Retiros = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Esperado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Contado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cuadraturas", x => x.Id);
                table.ForeignKey(
                    name: "FK_Cuadraturas_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Marcas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Activa = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Marcas", x => x.Id);
                table.UniqueConstraint("AK_Marcas_Nombre", x => x.Nombre);
            });

        migrationBuilder.CreateTable(
            name: "MovimientosCaja",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Importe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MovimientosCaja", x => x.Id);
                table.CheckConstraint("CK_MovimientoCaja_Importe", "[Importe]>0 AND [Tipo] IN ('Ingreso','Retiro')");
                table.ForeignKey(
                    name: "FK_MovimientosCaja_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Tallas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Valor = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                Sistema = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Activa = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tallas", x => x.Id);
                table.UniqueConstraint("AK_Tallas_Valor_Sistema", x => new { x.Valor, x.Sistema });
            });

        migrationBuilder.CreateTable(
            name: "Ofertas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                MarcaId = table.Column<int>(type: "int", nullable: true),
                TallaId = table.Column<int>(type: "int", nullable: true),
                VarianteId = table.Column<int>(type: "int", nullable: true),
                Tipo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Inicio = table.Column<DateOnly>(type: "date", nullable: false),
                Fin = table.Column<DateOnly>(type: "date", nullable: false),
                Activa = table.Column<bool>(type: "bit", nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Creacion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ofertas", x => x.Id);
                table.CheckConstraint("CK_Oferta_Fechas", "[Fin]>=[Inicio]");
                table.CheckConstraint("CK_Oferta_Filtro", "[MarcaId] IS NOT NULL OR [TallaId] IS NOT NULL OR [VarianteId] IS NOT NULL");
                table.CheckConstraint("CK_Oferta_Valor", "([Tipo]='Porcentaje' AND [Valor]>0 AND [Valor]<=100) OR ([Tipo]='Monto' AND [Valor]>0 AND [Valor]=FLOOR([Valor]))");
                table.ForeignKey(
                    name: "FK_Ofertas_Marcas_MarcaId",
                    column: x => x.MarcaId,
                    principalTable: "Marcas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Ofertas_Tallas_TallaId",
                    column: x => x.TallaId,
                    principalTable: "Tallas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Ofertas_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Conservar los catálogos que antes se ingresaban como texto y el precio histórico.
        migrationBuilder.Sql("""
            INSERT INTO Marcas (Nombre, Activa) SELECT DISTINCT Marca, 1 FROM Productos;
            INSERT INTO Tallas (Valor, Sistema, Activa) SELECT DISTINCT Talla, SistemaTalla, 1 FROM Variantes;
            UPDATE DetallesVenta SET PrecioLista = PrecioUnitario;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Variantes_Talla_SistemaTalla",
            table: "Variantes",
            columns: new[] { "Talla", "SistemaTalla" });

        migrationBuilder.CreateIndex(
            name: "IX_Productos_Marca",
            table: "Productos",
            column: "Marca");

        migrationBuilder.CreateIndex(
            name: "IX_DetallesVenta_OfertaId",
            table: "DetallesVenta",
            column: "OfertaId");

        migrationBuilder.CreateIndex(
            name: "IX_Cuadraturas_SesionCajaId_Medio",
            table: "Cuadraturas",
            columns: new[] { "SesionCajaId", "Medio" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_MovimientosCaja_SesionCajaId",
            table: "MovimientosCaja",
            column: "SesionCajaId");

        migrationBuilder.CreateIndex(
            name: "IX_MovimientosCaja_SolicitudId",
            table: "MovimientosCaja",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Ofertas_MarcaId",
            table: "Ofertas",
            column: "MarcaId");

        migrationBuilder.CreateIndex(
            name: "IX_Ofertas_TallaId",
            table: "Ofertas",
            column: "TallaId");

        migrationBuilder.CreateIndex(
            name: "IX_Ofertas_VarianteId",
            table: "Ofertas",
            column: "VarianteId");

        migrationBuilder.AddForeignKey(
            name: "FK_DetallesVenta_Ofertas_OfertaId",
            table: "DetallesVenta",
            column: "OfertaId",
            principalTable: "Ofertas",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Productos_Marcas_Marca",
            table: "Productos",
            column: "Marca",
            principalTable: "Marcas",
            principalColumn: "Nombre",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Variantes_Tallas_Talla_SistemaTalla",
            table: "Variantes",
            columns: new[] { "Talla", "SistemaTalla" },
            principalTable: "Tallas",
            principalColumns: new[] { "Valor", "Sistema" },
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DetallesVenta_Ofertas_OfertaId",
            table: "DetallesVenta");

        migrationBuilder.DropForeignKey(
            name: "FK_Productos_Marcas_Marca",
            table: "Productos");

        migrationBuilder.DropForeignKey(
            name: "FK_Variantes_Tallas_Talla_SistemaTalla",
            table: "Variantes");

        migrationBuilder.DropTable(
            name: "Cuadraturas");

        migrationBuilder.DropTable(
            name: "MovimientosCaja");

        migrationBuilder.DropTable(
            name: "Ofertas");

        migrationBuilder.DropTable(
            name: "Marcas");

        migrationBuilder.DropTable(
            name: "Tallas");

        migrationBuilder.DropIndex(
            name: "IX_Variantes_Talla_SistemaTalla",
            table: "Variantes");

        migrationBuilder.DropIndex(
            name: "IX_Productos_Marca",
            table: "Productos");

        migrationBuilder.DropIndex(
            name: "IX_DetallesVenta_OfertaId",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "ObservacionCierre",
            table: "SesionesCaja");

        migrationBuilder.DropColumn(
            name: "RevisadoPor",
            table: "SesionesCaja");

        migrationBuilder.DropColumn(
            name: "Revision",
            table: "SesionesCaja");

        migrationBuilder.DropColumn(
            name: "DescuentoUnitario",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "OfertaId",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "OfertaNombre",
            table: "DetallesVenta");

        migrationBuilder.DropColumn(
            name: "PrecioLista",
            table: "DetallesVenta");
    }
}

