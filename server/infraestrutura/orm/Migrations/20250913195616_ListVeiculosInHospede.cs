using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class ListVeiculosInHospede : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Veiculos_HospedeId",
                table: "Veiculos");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_HospedeId",
                table: "Veiculos",
                column: "HospedeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Veiculos_HospedeId",
                table: "Veiculos");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_HospedeId",
                table: "Veiculos",
                column: "HospedeId",
                unique: true);
        }
    }
}
