using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessTokenVersionId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAutenticadoId = table.Column<Guid>(type: "uuid", nullable: false),
                    HashDoToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiraEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevogadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubstituidoPorHashDoToken = table.Column<string>(type: "text", nullable: true),
                    EnderecoIpDeCriacao = table.Column<string>(type: "text", nullable: true),
                    UserAgentDeCriacao = table.Column<string>(type: "text", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_HashDoToken",
                table: "RefreshTokens",
                column: "HashDoToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioAutenticadoId_UsuarioId",
                table: "RefreshTokens",
                columns: new[] { "UsuarioAutenticadoId", "UsuarioId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "AccessTokenVersionId",
                table: "AspNetUsers");
        }
    }
}
