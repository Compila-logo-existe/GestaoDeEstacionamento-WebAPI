using FluentAssertions;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using System.Net;
using System.Net.Http.Json;

namespace GestaoDeEstacionamento.Testes.API.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de API de ModuloAutenticacao")]
public class RegistrarTestes : TestFixture
{
    [TestMethod]
    public async Task DeveRegistrarComSucesso()
    {
        // Arrange
        RegistrarUsuarioCommand usuario = new(
            "Fulano Da Silva",
            "fulanodasilva@yahoo.com",
            "Senha@1234",
            "Senha@1234",
            null,
            "eteste"
        );

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/auth/registrar", usuario);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        AccessToken? result = await response.Content.ReadFromJsonAsync<AccessToken>();
        result.Should().NotBeNull();
        result.Chave.Should().NotBeEmpty();
    }

    [TestMethod]
    public async Task NaoDeveRegistrarComSenhasDiferentes()
    {
        // Arrange
        RegistrarUsuarioCommand usuario = new(
            "Fulano Da Silva",
            "fulanodasilva@yahoo.com",
            "Senha@1234",
            "Senha@1234!",
            null,
            "eteste"
        );

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/auth/registrar", usuario);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        object? result = await response.Content.ReadFromJsonAsync<object>();
        result.Should().NotBeNull();
    }
}
