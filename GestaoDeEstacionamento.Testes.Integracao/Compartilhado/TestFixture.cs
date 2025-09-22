using DotNet.Testcontainers.Containers;
using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloFaturamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloHospede;
using GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;
using Testcontainers.PostgreSql;

namespace GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

[TestClass]
public abstract class TestFixture
{
    protected AppDbContext dbContext;

    protected RepositorioHospede repositorioHospede;
    protected RepositorioVeiculo repositorioVeiculo;
    protected RepositorioEstacionamento repositorioEstacionamento;
    protected RepositorioVaga repositorioVaga;
    protected RepositorioRegistroEntrada repositorioRegistroEntrada;
    protected RepositorioTicket repositorioTicket;
    protected RepositorioRegistroSaida repositorioRegistroSaida;
    protected RepositorioFaturamento repositorioFaturamento;
    protected RepositorioTenant repositorioTenant;
    protected RepositorioUsuarioTenant repositorioUsuarioTenant;
    protected RepositorioConviteRegistro repositorioConviteRegistro;
    protected RepositorioRefreshToken repositorioRefreshToken;

    private static IDatabaseContainer? dbContainer;

    [AssemblyInitialize]
    public static async Task Setup(TestContext _)
    {
        dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithName("gesta-de-estacionamento-db-testes")
            .WithDatabase("GestaoEstacionamentoDb")
            .WithUsername("postgres")
            .WithPassword("SenhaSuperSecreta")
            .WithCleanUp(true)
            .Build();

        await InicializarBancoDadosAsync();
    }

    [AssemblyCleanup]
    public static async Task Teardown()
    {
        await EncerrarBancoDadosAsync();
    }

    [TestInitialize]
    public virtual void ConfigurarTestes()
    {
        if (dbContainer is null)
            throw new ArgumentNullException("O banco de dados não foi inicializado corretamente.");

        dbContext = AppDbContextFactory.CriarDbContext(dbContainer.GetConnectionString());

        ConfigurarTabelas(dbContext);

        repositorioHospede = new(dbContext);
        repositorioVeiculo = new(dbContext);
        repositorioEstacionamento = new(dbContext);
        repositorioVaga = new(dbContext);
        repositorioRegistroEntrada = new(dbContext);
        repositorioRegistroSaida = new(dbContext);
        repositorioFaturamento = new(dbContext);
        repositorioTenant = new(dbContext);
        repositorioUsuarioTenant = new(dbContext);
        repositorioConviteRegistro = new(dbContext);
        repositorioRefreshToken = new(dbContext);

        BuilderSetup.SetCreatePersistenceMethod<Hospede>(h => repositorioHospede.CadastrarRegistroAsync(h).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Hospede>>(h => repositorioHospede.CadastrarEntidades(h).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<Veiculo>(v => repositorioVeiculo.CadastrarRegistroAsync(v).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Veiculo>>(v => repositorioVeiculo.CadastrarEntidades(v).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<Estacionamento>(e => repositorioEstacionamento.CadastrarRegistroAsync(e).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Estacionamento>>(e => repositorioEstacionamento.CadastrarEntidades(e).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<Vaga>(v => repositorioVaga.CadastrarRegistroAsync(v).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Vaga>>(v => repositorioVaga.CadastrarEntidades(v).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<RegistroEntrada>(r => repositorioRegistroEntrada.CadastrarRegistroAsync(r).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<RegistroEntrada>>(r => repositorioRegistroEntrada.CadastrarEntidades(r).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<RegistroSaida>(r => repositorioRegistroSaida.CadastrarRegistroAsync(r).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<RegistroSaida>>(r => repositorioRegistroSaida.CadastrarEntidades(r).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<Faturamento>(f => repositorioFaturamento.CadastrarRegistroAsync(f).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Faturamento>>(f => repositorioFaturamento.CadastrarEntidades(f).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<Tenant>(t => repositorioTenant.CadastrarRegistroAsync(t).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<Tenant>>(t => repositorioTenant.CadastrarEntidades(t).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<VinculoUsuarioTenant>(v => repositorioUsuarioTenant.CadastrarRegistroAsync(v).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<VinculoUsuarioTenant>>(v => repositorioUsuarioTenant.CadastrarEntidades(v).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<ConviteRegistro>(c => repositorioConviteRegistro.CadastrarRegistroAsync(c).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<ConviteRegistro>>(c => repositorioConviteRegistro.CadastrarEntidades(c).GetAwaiter().GetResult());

        BuilderSetup.SetCreatePersistenceMethod<RefreshToken>(r => repositorioRefreshToken.CadastrarRegistroAsync(r).GetAwaiter().GetResult());
        BuilderSetup.SetCreatePersistenceMethod<IList<RefreshToken>>(r => repositorioRefreshToken.CadastrarEntidades(r).GetAwaiter().GetResult());

    }

    private static async Task InicializarBancoDadosAsync()
    {
        await dbContainer!.StartAsync();
    }

    private static async Task EncerrarBancoDadosAsync()
    {
        await dbContainer!.StopAsync();
        await dbContainer.DisposeAsync();
    }

    private static void ConfigurarTabelas(AppDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        dbContext.Hospedes.RemoveRange(dbContext.Hospedes);
        dbContext.Veiculos.RemoveRange(dbContext.Veiculos);
        dbContext.Estacionamentos.RemoveRange(dbContext.Estacionamentos);
        dbContext.Vagas.RemoveRange(dbContext.Vagas);
        dbContext.Tickets.RemoveRange(dbContext.Tickets);
        dbContext.RegistrosEntrada.RemoveRange(dbContext.RegistrosEntrada);
        dbContext.RegistrosSaida.RemoveRange(dbContext.RegistrosSaida);
        dbContext.Faturamentos.RemoveRange(dbContext.Faturamentos);
        dbContext.Tenants.RemoveRange(dbContext.Tenants);
        dbContext.VinculosUsuarioTenant.RemoveRange(dbContext.VinculosUsuarioTenant);
        dbContext.ConvitesRegistro.RemoveRange(dbContext.ConvitesRegistro);
        dbContext.RefreshTokens.RemoveRange(dbContext.RefreshTokens);

        dbContext.SaveChanges();
    }
}
