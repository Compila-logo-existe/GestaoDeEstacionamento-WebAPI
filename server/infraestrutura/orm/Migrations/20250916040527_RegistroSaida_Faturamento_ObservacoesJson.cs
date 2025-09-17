using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class RegistroSaida_Faturamento_ObservacoesJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vagas_Veiculos_VeiculoId",
                table: "Vagas");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosEntrada_DataSaidaEmUtc",
                table: "RegistrosEntrada");

            migrationBuilder.DropColumn(
                name: "VagaId",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "DataSaidaEmUtc",
                table: "RegistrosEntrada");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Veiculos",
                type: "text",
                nullable: false,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VeiculoId",
                table: "Vagas",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "RegistrosEntrada",
                type: "text",
                nullable: false,
                defaultValueSql: "'[]'",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "RegistrosSaida",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSaidaEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HospedeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    VeiculoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'[]'"),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosSaida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosSaida_Hospedes_HospedeId",
                        column: x => x.HospedeId,
                        principalTable: "Hospedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosSaida_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosSaida_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Faturamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistroSaidaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorDaDiaria = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    NumeroDeDiarias = table.Column<int>(type: "integer", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    DataEntradaEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturamentos_RegistrosSaida_RegistroSaidaId",
                        column: x => x.RegistroSaidaId,
                        principalTable: "RegistrosSaida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vagas_UsuarioId_VeiculoId",
                table: "Vagas",
                columns: new[] { "UsuarioId", "VeiculoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEntrada_UsuarioId_TicketId",
                table: "RegistrosEntrada",
                columns: new[] { "UsuarioId", "TicketId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturamentos_DataEntradaEmUtc",
                table: "Faturamentos",
                column: "DataEntradaEmUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Faturamentos_RegistroSaidaId",
                table: "Faturamentos",
                column: "RegistroSaidaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturamentos_UsuarioId_RegistroSaidaId",
                table: "Faturamentos",
                columns: new[] { "UsuarioId", "RegistroSaidaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaida_DataSaidaEmUtc",
                table: "RegistrosSaida",
                column: "DataSaidaEmUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaida_HospedeId",
                table: "RegistrosSaida",
                column: "HospedeId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaida_TicketId",
                table: "RegistrosSaida",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaida_UsuarioId_TicketId",
                table: "RegistrosSaida",
                columns: new[] { "UsuarioId", "TicketId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSaida_VeiculoId",
                table: "RegistrosSaida",
                column: "VeiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vagas_Veiculos_VeiculoId",
                table: "Vagas",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vagas_Veiculos_VeiculoId",
                table: "Vagas");

            migrationBuilder.DropTable(
                name: "Faturamentos");

            migrationBuilder.DropTable(
                name: "RegistrosSaida");

            migrationBuilder.DropIndex(
                name: "IX_Vagas_UsuarioId_VeiculoId",
                table: "Vagas");

            migrationBuilder.DropIndex(
                name: "IX_RegistrosEntrada_UsuarioId_TicketId",
                table: "RegistrosEntrada");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "Veiculos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'[]'");

            migrationBuilder.AddColumn<Guid>(
                name: "VagaId",
                table: "Veiculos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "VeiculoId",
                table: "Vagas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "RegistrosEntrada",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'[]'");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSaidaEmUtc",
                table: "RegistrosEntrada",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosEntrada_DataSaidaEmUtc",
                table: "RegistrosEntrada",
                column: "DataSaidaEmUtc");

            migrationBuilder.AddForeignKey(
                name: "FK_Vagas_Veiculos_VeiculoId",
                table: "Vagas",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
