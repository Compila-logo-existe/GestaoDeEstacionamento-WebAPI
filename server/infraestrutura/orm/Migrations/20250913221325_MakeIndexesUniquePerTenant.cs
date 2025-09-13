using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class MakeIndexesUniquePerTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Hospedes_CPF",
                table: "Hospedes");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_UsuarioId_Placa",
                table: "Veiculos",
                columns: new[] { "UsuarioId", "Placa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hospedes_UsuarioId_CPF",
                table: "Hospedes",
                columns: new[] { "UsuarioId", "CPF" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Veiculos_UsuarioId_Placa",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Hospedes_UsuarioId_CPF",
                table: "Hospedes");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos",
                column: "Placa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hospedes_CPF",
                table: "Hospedes",
                column: "CPF",
                unique: true);
        }
    }
}
