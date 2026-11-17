using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetailPOS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261117170927_CuadraturaDiaria : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Reembolso_Importe",
            table: "Reembolsos");

        migrationBuilder.DropCheckConstraint(
            name: "CK_Pago_Importe",
            table: "Pagos");

        migrationBuilder.AddColumn<DateOnly>(
            name: "FechaOperativa",
            table: "SesionesCaja",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1));

        migrationBuilder.Sql("UPDATE SesionesCaja SET FechaOperativa=CONVERT(date,Apertura AT TIME ZONE 'Pacific SA Standard Time');");

        migrationBuilder.CreateIndex(
            name: "IX_SesionesCaja_FechaOperativa_UsuarioId",
            table: "SesionesCaja",
            columns: new[] { "FechaOperativa", "UsuarioId" });

        migrationBuilder.AddCheckConstraint(
            name: "CK_Reembolso_Importe",
            table: "Reembolsos",
            sql: "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','Otros')");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Pago_Importe",
            table: "Pagos",
            sql: "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','Otros','SaldoCambio')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_SesionesCaja_FechaOperativa_UsuarioId",
            table: "SesionesCaja");

        migrationBuilder.DropCheckConstraint(
            name: "CK_Reembolso_Importe",
            table: "Reembolsos");

        migrationBuilder.DropCheckConstraint(
            name: "CK_Pago_Importe",
            table: "Pagos");

        migrationBuilder.DropColumn(
            name: "FechaOperativa",
            table: "SesionesCaja");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Reembolso_Importe",
            table: "Reembolsos",
            sql: "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia')");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Pago_Importe",
            table: "Pagos",
            sql: "[Importe]>0 AND [Medio] IN ('Efectivo','Tarjeta','Transferencia','SaldoCambio')");
    }
}
