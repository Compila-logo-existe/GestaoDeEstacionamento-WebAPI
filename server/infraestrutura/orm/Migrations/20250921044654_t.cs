using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoDeEstacionamento.Infraestrutura.ORM.Migrations
{
    /// <inheritdoc />
    public partial class t : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "VinculosUsuarioTenant",
                newName: "UsuarioVinculadoId");

            migrationBuilder.RenameIndex(
                name: "IX_VinculosUsuarioTenant_UsuarioId_TenantId",
                table: "VinculosUsuarioTenant",
                newName: "IX_VinculosUsuarioTenant_UsuarioVinculadoId_TenantId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Veiculos",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Veiculos_UsuarioId_Placa",
                table: "Veiculos",
                newName: "IX_Veiculos_TenantId_Placa");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Vagas",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Vagas_UsuarioId_VeiculoId",
                table: "Vagas",
                newName: "IX_Vagas_TenantId_VeiculoId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Tickets",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Tenants",
                newName: "UsuarioCriadorId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "RegistrosSaida",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosSaida_UsuarioId_TicketId",
                table: "RegistrosSaida",
                newName: "IX_RegistrosSaida_TenantId_TicketId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "RegistrosEntrada",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosEntrada_UsuarioId_TicketId",
                table: "RegistrosEntrada",
                newName: "IX_RegistrosEntrada_TenantId_TicketId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "RefreshTokens",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UsuarioAutenticadoId_UsuarioId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UsuarioAutenticadoId_TenantId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Hospedes",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Hospedes_UsuarioId_CPF",
                table: "Hospedes",
                newName: "IX_Hospedes_TenantId_CPF");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Faturamentos",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Faturamentos_UsuarioId_RegistroSaidaId",
                table: "Faturamentos",
                newName: "IX_Faturamentos_TenantId_RegistroSaidaId");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Estacionamentos",
                newName: "TenantId");

            migrationBuilder.RenameIndex(
                name: "IX_Estacionamentos_UsuarioId_Nome",
                table: "Estacionamentos",
                newName: "IX_Estacionamentos_TenantId_Nome");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "ConvitesRegistro",
                newName: "UsuarioEmissorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UsuarioVinculadoId",
                table: "VinculosUsuarioTenant",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_VinculosUsuarioTenant_UsuarioVinculadoId_TenantId",
                table: "VinculosUsuarioTenant",
                newName: "IX_VinculosUsuarioTenant_UsuarioId_TenantId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Veiculos",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Veiculos_TenantId_Placa",
                table: "Veiculos",
                newName: "IX_Veiculos_UsuarioId_Placa");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Vagas",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Vagas_TenantId_VeiculoId",
                table: "Vagas",
                newName: "IX_Vagas_UsuarioId_VeiculoId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Tickets",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "UsuarioCriadorId",
                table: "Tenants",
                newName: "UsuarioId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "RegistrosSaida",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosSaida_TenantId_TicketId",
                table: "RegistrosSaida",
                newName: "IX_RegistrosSaida_UsuarioId_TicketId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "RegistrosEntrada",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistrosEntrada_TenantId_TicketId",
                table: "RegistrosEntrada",
                newName: "IX_RegistrosEntrada_UsuarioId_TicketId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "RefreshTokens",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UsuarioAutenticadoId_TenantId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UsuarioAutenticadoId_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Hospedes",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Hospedes_TenantId_CPF",
                table: "Hospedes",
                newName: "IX_Hospedes_UsuarioId_CPF");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Faturamentos",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Faturamentos_TenantId_RegistroSaidaId",
                table: "Faturamentos",
                newName: "IX_Faturamentos_UsuarioId_RegistroSaidaId");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "Estacionamentos",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Estacionamentos_TenantId_Nome",
                table: "Estacionamentos",
                newName: "IX_Estacionamentos_UsuarioId_Nome");

            migrationBuilder.RenameColumn(
                name: "UsuarioEmissorId",
                table: "ConvitesRegistro",
                newName: "UsuarioId");
        }
    }
}
