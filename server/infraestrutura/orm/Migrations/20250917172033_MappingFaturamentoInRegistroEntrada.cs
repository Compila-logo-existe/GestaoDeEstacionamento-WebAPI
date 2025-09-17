using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class MappingFaturamentoInRegistroEntrada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrosEntrada_Faturamentos_FaturamentoId",
                table: "RegistrosEntrada");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosEntrada_FaturamentoId",
                table: "RegistrosEntrada");

            migrationBuilder.DropColumn(
                name: "FaturamentoId",
                table: "RegistrosEntrada");

            migrationBuilder.AddColumn<Guid>(
                name: "RegistroEntradaId",
                table: "Faturamentos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Faturamentos_RegistroEntradaId",
                table: "Faturamentos",
                column: "RegistroEntradaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Faturamentos_RegistrosEntrada_RegistroEntradaId",
                table: "Faturamentos",
                column: "RegistroEntradaId",
                principalTable: "RegistrosEntrada",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faturamentos_RegistrosEntrada_RegistroEntradaId",
                table: "Faturamentos");

            migrationBuilder.DropIndex(
                name: "IX_Faturamentos_RegistroEntradaId",
                table: "Faturamentos");

            migrationBuilder.DropColumn(
                name: "RegistroEntradaId",
                table: "Faturamentos");

            migrationBuilder.AddColumn<Guid>(
                name: "FaturamentoId",
                table: "RegistrosEntrada",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEntrada_FaturamentoId",
                table: "RegistrosEntrada",
                column: "FaturamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrosEntrada_Faturamentos_FaturamentoId",
                table: "RegistrosEntrada",
                column: "FaturamentoId",
                principalTable: "Faturamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
