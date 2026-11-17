using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailPOS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261114211238_Inicial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AspNetRoles",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUsers",
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Bodegas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bodegas", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Clientes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                Giro = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Comuna = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Clientes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Productos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Marca = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Modelo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Productos", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SesionesCaja",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Apertura = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Cierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                FondoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                EfectivoEsperado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                EfectivoContado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SesionesCaja", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AspNetRoleClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserClaims",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                table.ForeignKey(
                    name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserLogins",
            columns: table => new
            {
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                table.ForeignKey(
                    name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserRoles",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "AspNetRoles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "AspNetUserTokens",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                table.ForeignKey(
                    name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Variantes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ProductoId = table.Column<int>(type: "int", nullable: false),
                Sku = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                CodigoBarras = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                Color = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                Talla = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                SistemaTalla = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Precio = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Activo = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Variantes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Variantes_Productos_ProductoId",
                    column: x => x.ProductoId,
                    principalTable: "Productos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Ventas",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                ClienteId = table.Column<int>(type: "int", nullable: true),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                MedioPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ventas", x => x.Id);
                table.ForeignKey(
                    name: "FK_Ventas_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Ventas_Clientes_ClienteId",
                    column: x => x.ClienteId,
                    principalTable: "Clientes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Ventas_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Existencias",
            columns: table => new
            {
                VarianteId = table.Column<int>(type: "int", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Existencias", x => new { x.VarianteId, x.BodegaId });
                table.CheckConstraint("CK_Existencia_Cantidad", "[Cantidad] >= 0");
                table.ForeignKey(
                    name: "FK_Existencias_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Existencias_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Movimientos",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VarianteId = table.Column<int>(type: "int", nullable: false),
                BodegaId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                VentaId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Movimientos", x => x.Id);
                table.ForeignKey(
                    name: "FK_Movimientos_Bodegas_BodegaId",
                    column: x => x.BodegaId,
                    principalTable: "Bodegas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Movimientos_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DetallesVenta",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VentaId = table.Column<int>(type: "int", nullable: false),
                VarianteId = table.Column<int>(type: "int", nullable: false),
                Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DetallesVenta", x => x.Id);
                table.ForeignKey(
                    name: "FK_DetallesVenta_Variantes_VarianteId",
                    column: x => x.VarianteId,
                    principalTable: "Variantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_DetallesVenta_Ventas_VentaId",
                    column: x => x.VentaId,
                    principalTable: "Ventas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Documentos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                VentaId = table.Column<int>(type: "int", nullable: false),
                Tipo = table.Column<int>(type: "int", nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ReferenciaId = table.Column<int>(type: "int", nullable: true),
                Motivo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documentos", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documentos_Documentos_ReferenciaId",
                    column: x => x.ReferenciaId,
                    principalTable: "Documentos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Documentos_Ventas_VentaId",
                    column: x => x.VentaId,
                    principalTable: "Ventas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "DetalleDocumento",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DocumentoId = table.Column<int>(type: "int", nullable: false),
                Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DetalleDocumento", x => x.Id);
                table.ForeignKey(
                    name: "FK_DetalleDocumento_Documentos_DocumentoId",
                    column: x => x.DocumentoId,
                    principalTable: "Documentos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Devoluciones",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                SolicitudId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DetalleVentaId = table.Column<int>(type: "int", nullable: false),
                Cantidad = table.Column<int>(type: "int", nullable: false),
                ReingresarStock = table.Column<bool>(type: "bit", nullable: false),
                DocumentoId = table.Column<int>(type: "int", nullable: false),
                SesionCajaId = table.Column<int>(type: "int", nullable: false),
                Reembolso = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                MedioReembolso = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Devoluciones", x => x.Id);
                table.ForeignKey(
                    name: "FK_Devoluciones_DetallesVenta_DetalleVentaId",
                    column: x => x.DetalleVentaId,
                    principalTable: "DetallesVenta",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Devoluciones_Documentos_DocumentoId",
                    column: x => x.DocumentoId,
                    principalTable: "Documentos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Devoluciones_SesionesCaja_SesionCajaId",
                    column: x => x.SesionCajaId,
                    principalTable: "SesionesCaja",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true,
            filter: "[NormalizedName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true,
            filter: "[NormalizedUserName] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Bodegas_Nombre",
            table: "Bodegas",
            column: "Nombre",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DetalleDocumento_DocumentoId",
            table: "DetalleDocumento",
            column: "DocumentoId");

        migrationBuilder.CreateIndex(
            name: "IX_DetallesVenta_VarianteId",
            table: "DetallesVenta",
            column: "VarianteId");

        migrationBuilder.CreateIndex(
            name: "IX_DetallesVenta_VentaId",
            table: "DetallesVenta",
            column: "VentaId");

        migrationBuilder.CreateIndex(
            name: "IX_Devoluciones_DetalleVentaId",
            table: "Devoluciones",
            column: "DetalleVentaId");

        migrationBuilder.CreateIndex(
            name: "IX_Devoluciones_DocumentoId",
            table: "Devoluciones",
            column: "DocumentoId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Devoluciones_SesionCajaId",
            table: "Devoluciones",
            column: "SesionCajaId");

        migrationBuilder.CreateIndex(
            name: "IX_Devoluciones_SolicitudId",
            table: "Devoluciones",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Documentos_ReferenciaId",
            table: "Documentos",
            column: "ReferenciaId");

        migrationBuilder.CreateIndex(
            name: "IX_Documentos_SolicitudId",
            table: "Documentos",
            column: "SolicitudId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Documentos_VentaId_Tipo",
            table: "Documentos",
            columns: new[] { "VentaId", "Tipo" },
            unique: true,
            filter: "[Tipo] IN (33, 39, 52)");

        migrationBuilder.CreateIndex(
            name: "IX_Existencias_BodegaId",
            table: "Existencias",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_Movimientos_BodegaId",
            table: "Movimientos",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_Movimientos_VarianteId",
            table: "Movimientos",
            column: "VarianteId");

        migrationBuilder.CreateIndex(
            name: "IX_SesionesCaja_UsuarioId",
            table: "SesionesCaja",
            column: "UsuarioId",
            unique: true,
            filter: "[Cierre] IS NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Variantes_CodigoBarras",
            table: "Variantes",
            column: "CodigoBarras",
            unique: true,
            filter: "[CodigoBarras] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Variantes_ProductoId_Color_Talla_SistemaTalla",
            table: "Variantes",
            columns: new[] { "ProductoId", "Color", "Talla", "SistemaTalla" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Variantes_Sku",
            table: "Variantes",
            column: "Sku",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Ventas_BodegaId",
            table: "Ventas",
            column: "BodegaId");

        migrationBuilder.CreateIndex(
            name: "IX_Ventas_ClienteId",
            table: "Ventas",
            column: "ClienteId");

        migrationBuilder.CreateIndex(
            name: "IX_Ventas_SesionCajaId",
            table: "Ventas",
            column: "SesionCajaId");

        migrationBuilder.CreateIndex(
            name: "IX_Ventas_SolicitudId",
            table: "Ventas",
            column: "SolicitudId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "DetalleDocumento");

        migrationBuilder.DropTable(
            name: "Devoluciones");

        migrationBuilder.DropTable(
            name: "Existencias");

        migrationBuilder.DropTable(
            name: "Movimientos");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "DetallesVenta");

        migrationBuilder.DropTable(
            name: "Documentos");

        migrationBuilder.DropTable(
            name: "Variantes");

        migrationBuilder.DropTable(
            name: "Ventas");

        migrationBuilder.DropTable(
            name: "Productos");

        migrationBuilder.DropTable(
            name: "Bodegas");

        migrationBuilder.DropTable(
            name: "Clientes");

        migrationBuilder.DropTable(
            name: "SesionesCaja");
    }
}

