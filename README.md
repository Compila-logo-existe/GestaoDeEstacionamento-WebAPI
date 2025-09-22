# 🚗 Valet / Gestão de Estacionamento — Web API

## 📖 Introdução
A **Valet / Gestão de Estacionamento** é uma API multi‑tenant para controle operacional de estacionamentos: **autenticação e convites**, **check‑in/checkout**, **gestão de vagas** e **faturamento**.  
Projetada com foco em **segurança, observabilidade e testes**, usa **.NET 8**, **EF Core (PostgreSQL)**, **Redis cache**, **JWT (Identity)**, **MediatR**, **FluentValidation**, **AutoMapper** e **Serilog** — com **Health Checks** e **Swagger** habilitados.

> *"Se pode estacionar, deve registrar. Se pode cobrar, deve auditar."*

---

## 🧩 Módulos do Sistema
- **Autenticação & Convites**
  - Registro, login, refresh e logout de usuários.
  - Convites entre tenants e aceite por token.
  - **Policies**: `PlatformAdminPolicy`, `AdminPolicy`, `UserPolicy`, `AdminOrPlatformAdminPolicy`.
- **Recepção (Check‑in/Checkout)**
  - Registrar **entrada** e **saída** de veículos.
  - Consultar registros por período e **detalhes do veículo**.
- **Estacionamento**
  - Configuração do estacionamento (capacidade/layout).
  - **Ocupar** e **liberar** vaga; **status de vagas** em tempo real (cache).
- **Faturamento**
  - Obter **valor atual** e **gerar relatório** financeiro.
- **Tenants (Plataforma)**
  - Criação / administração de tenants de plataforma.

> **Multi‑tenant**: resolução de Tenant por **claims** (`tenant_id`, `tenant_slug`) e/ou headers `X-Tenant-Id` / `X-Tenant-Slug` (middleware de resolução).

---

## 🧭 Endpoints (resumo)
| Controller | Rota base | Verbo | Caminho |
|---|---|---|---|
| AutenticacaoController | `/auth` | **POST** | `/registrar` |
|  |  | **POST** | `/autenticar` |
|  |  | **POST** | `/refresh` |
|  |  | **POST** | `/sair` |
| ConvitesController | `/(sem rota base)` | **POST** | `/tenants/{tenantId:guid}/convites` |
|  |  | **POST** | `/convites/{token}/aceitar` |
| EstacionamentoController | `/estacionamento` | **POST** | `/configuracao/` |
|  |  | **GET** | `/status-vagas/` |
|  |  | **GET** | `/vaga/` |
|  |  | **POST** | `/ocupar-vaga/` |
|  |  | **POST** | `/liberar-vaga/` |
| FaturamentoController | `/faturamento` | **GET** | `/obter-valor-atual` |
|  |  | **GET** | `/gerar-relatorio-financeiro` |
| PlatformTenantsController | `platform/tenants` | — | — |
| RecepcaoCheckinController | `/recepcao` | **POST** | `/registrar-entrada` |
|  |  | **POST** | `/registrar-saida` |
|  |  | **GET** | `/registros` |
|  |  | **GET** | `/detalhes-veiculo/` |
|  |  | **GET** | `/registros-veiculo/` |

> Documentação interativa: **Swagger** disponível na raiz `/` (RoutePrefix vazio).  
> Health: `/health/live` e `/health/ready`.

---

## 🏗️ Arquitetura e Pastas
- `core/dominio — Domínio (entidades, regras)`
- `core/aplicacao — Aplicação (MediatR, validações, logging, cache)`
- `infraestrutura/orm — Infra (EF Core, repositórios, migrations)`
- `web-api — API ASP.NET Core (controllers, identity, swagger)`
- `testes/unidades — Testes unitários (MSTest, Moq, NBuilder)`
- `testes/integracao — Testes de integração (MSTest)`

---

## 🧰 Tecnologias
![Tecnologias](https://skillicons.dev/icons?i=github,visualstudio,vscode,cs,dotnet,postgres,redis,docker)

- **.NET 8 / ASP.NET Core Web API**  
- **Entity Framework Core** (Npgsql, Migrations)  
- **Identity + JWT Bearer** (roles e policies)  
- **MediatR**, **FluentValidation**, **AutoMapper**  
- **Serilog** (console, arquivo e New Relic Logs)  
- **Redis** (IDistributedCache)  
- **Swagger/OpenAPI** e **HealthChecks**  

---

## 🚀 Como Executar

### Opção A — Local (dotnet)
1) **Pré‑requisitos**
   - .NET SDK **8.0+**
   - PostgreSQL **16+**
   - Redis **6+**  
   - Visual Studio 2022+ ou VS Code

2) **Defina as variáveis de ambiente** (recomendado via *User Secrets* no projeto `web-api`):
```bash
dotnet user-secrets init
dotnet user-secrets set "SQL_CONNECTION_STRING" "Host"
dotnet user-secrets set "REDIS_CONNECTION_STRING" "localhost:6379"
dotnet user-secrets set "CORS_ALLOWED_ORIGINS" "https://localhost:5173;http://localhost:5173"
dotnet user-secrets set "PLATFORM_ADMIN_EMAIL" "admin@plataforma.local"
dotnet user-secrets set "PLATFORM_ADMIN_PASSWORD" "Troque@123"
dotnet user-secrets set "PLATFORM_ADMIN_FULLNAME" "Plataforma Admin"
dotnet user-secrets set "NEWRELIC_LICENSE_KEY" "***"
dotnet user-secrets set "LUCKYPENNYSOFTWARE_LICENSE_KEY" "***"
dotnet user-secrets list
```

3) **Restaurar, compilar e rodar**
```bash
dotnet restore
dotnet build -c Release
dotnet run --project web-api/GestaoDeEstacionamento.WebAPI.csproj
```

4) **Acesse**
- API/Swagger → `https://localhost:8081` ou `http://localhost:8080` *(Kestrel expõe 8080/8081)*

### Opção B — Docker Compose
1) **Ajuste `.env` (se necessário)** com as variáveis do bloco acima.  
2) **Suba a stack** (WebAPI + Postgres + Redis):
```bash
docker compose up --build -d
```
> Se precisar expor portas, adicione no serviço `gestaodeestacionamento.webapi`:
```yaml
ports:
  - "8080:8080"
  - "8081:8081"
```

Volumes criados: `dbdata`, `redisdata`. Rede: `gestaoEstacionamento-net`.

---

## 🔐 Autenticação e Autorização
- **Bearer JWT** no header `Authorization: Bearer <token>`.
- **Policies** disponíveis:
  - `PlatformAdminPolicy` (role: *PlataformaAdmin*)
  - `AdminPolicy` (role: *Admin*)
  - `UserPolicy` (role: *User*)
  - `AdminOrPlatformAdminPolicy` (roles: *Admin* ou *PlataformaAdmin*)
- **Seed de usuário Plataforma** na inicialização (*first‑run*):  
  Configurar `PLATFORM_ADMIN_EMAIL`, `PLATFORM_ADMIN_PASSWORD`, `PLATFORM_ADMIN_FULLNAME`.

---

## ⚙️ Variáveis de Ambiente
| Chave | Descrição |
|------|-----------|
| `SQL_CONNECTION_STRING` | Conexão Npgsql do EF Core. |
| `REDIS_CONNECTION_STRING` | Conexão do Redis (StackExchange.Redis). |
| `CORS_ALLOWED_ORIGINS` | Origens permitidas no CORS (separadas por `;`). |
| `PLATFORM_ADMIN_EMAIL` | E‑mail do admin da plataforma (seed). |
| `PLATFORM_ADMIN_PASSWORD` | Senha do admin da plataforma (seed). |
| `PLATFORM_ADMIN_FULLNAME` | Nome do admin da plataforma (seed). |
| `NEWRELIC_LICENSE_KEY` | License key para **Serilog → New Relic Logs**. |
| `LUCKYPENNYSOFTWARE_LICENSE_KEY` | License do mapeamento **AutoMapper**. |

---

## 🧪 Testes
- **Unitários** (`testes/unidades`) — **MSTest**, **Moq**, **NBuilder**.  
- **Integração** (`testes/integracao`) — **MSTest**.  
```bash
dotnet test
```

---

## 📦 Migrações (EF Core)
- As migrations ficam em `infraestrutura/orm/Migrations`.  
- O **DbContext** é `AppDbContext` (Identity + multi‑tenant).  
- No *startup*, `Database.Migrate()` aplica automaticamente as migrações.

---

## 🧠 Filosofia do Projeto
> "Cada entrada precisa de contexto.  
> Cada vaga precisa de estado.  
> Cada cobrança precisa de lastro — e de logs."

— *Compila, Logo Existe*
