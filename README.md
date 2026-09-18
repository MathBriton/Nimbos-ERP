# Nimbus ERP

Micro ERP full stack construído como projeto de portfólio: **.NET 10 + React 19**,
com Clean Architecture no backend, organização por feature no frontend e
infraestrutura completa em Docker.

O planejamento completo (22 sprints, critérios de aceite e dependências entre
módulos) está em [SDD.md](SDD.md).

> **Status atual:** Sprint 0 concluída — esqueleto do projeto de pé, com API,
> banco e frontend subindo juntos via `docker compose up`.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core 10 |
| Banco | SQL Server 2022 |
| Frontend | React 19, TypeScript 6, Vite 8 |
| UI | PrimeReact 10 (MIT) + PrimeIcons |
| Docs da API | OpenAPI + [Scalar](https://scalar.com) |
| Infra | Docker + Docker Compose |
| Testes | xUnit |

---

## Como executar

### Opção 1 — Docker Compose (recomendado)

Sobe API, banco e frontend de uma vez, já com as migrations aplicadas:

```bash
docker compose up --build
```

| Serviço | URL |
|---|---|
| Frontend | http://localhost:8080 |
| API | http://localhost:5080 |
| Documentação da API | http://localhost:5080/scalar/v1 |
| Health checks | http://localhost:5080/health |
| SQL Server | `localhost:1433` (usuário `sa`) |

Para parar e limpar tudo, inclusive o volume do banco:

```bash
docker compose down --volumes
```

As portas e a senha do `sa` podem ser trocadas por variáveis de ambiente —
veja os valores padrão em [docker-compose.yml](docker-compose.yml):
`PORTA_API`, `PORTA_FRONTEND`, `PORTA_BANCO`, `MSSQL_SA_PASSWORD`.

### Opção 2 — Execução local (desenvolvimento)

**Pré-requisitos:** .NET SDK 10, Node.js 22+, e um SQL Server acessível
(o mais simples é subir só o banco: `docker compose up banco`).

```bash
# 1. Restaurar as ferramentas locais (dotnet-ef)
dotnet tool restore

# 2. Backend  ->  http://localhost:5080
dotnet run --project src/Nimbus.Api

# 3. Frontend (em outro terminal)  ->  http://localhost:5173
cd frontend
cp .env.example .env
npm install
npm run dev
```

A API aplica as migrations pendentes automaticamente na subida. Para desativar
esse comportamento, defina `Banco__AplicarMigrationsNaSubida=false`.

---

## Estrutura do repositório

```
Nimbus-ERP/
├── Directory.Build.props        Configurações de build compartilhadas
├── Directory.Packages.props     Central Package Management (versões dos pacotes)
├── docker-compose.yml           API + SQL Server + frontend
├── Nimbus.slnx                  Solution (formato slnx do .NET 10)
│
├── src/
│   ├── Nimbus.Domain/           Entidades e regras de negócio (zero dependências)
│   ├── Nimbus.Application/      Casos de uso, DTOs e contratos (portas)
│   ├── Nimbus.Infrastructure/   EF Core, SQL Server, adaptadores
│   └── Nimbus.Api/              Controllers, pipeline HTTP, composição da DI
│
├── tests/
│   └── Nimbus.Tests/            Testes unitários e de arquitetura (xUnit)
│
└── frontend/
    └── src/
        ├── app/                 Bootstrap: providers, layout, rotas
        ├── features/            Um diretório por módulo do ERP
        │   └── <modulo>/
        │       ├── componentes/
        │       ├── hooks/
        │       ├── servicos/
        │       └── tipos/
        ├── shared/              Cliente HTTP, tipos e utilitários comuns
        └── styles/              CSS global
```

### Regra de dependência (Clean Architecture)

```
Api  ──>  Application  ──>  Domain
 │                            ▲
 └──>  Infrastructure  ───────┘
```

O **Domain** não referencia nenhum framework — nem EF Core, nem ASP.NET Core.
Essa regra não é só documentação: há testes de arquitetura em
[tests/Nimbus.Tests/Arquitetura/](tests/Nimbus.Tests/Arquitetura/) que quebram
o build se alguém violá-la.

---

## Comandos úteis

### Backend

```bash
dotnet build Nimbus.slnx          # compila (warnings são tratados como erro)
dotnet test Nimbus.slnx           # roda os testes
dotnet format Nimbus.slnx         # aplica as regras do .editorconfig
```

### Migrations

```bash
# Criar uma nova migration
dotnet ef migrations add NomeDaMigration \
  --project src/Nimbus.Infrastructure \
  --startup-project src/Nimbus.Api \
  --output-dir Persistencia/Migrations

# Aplicar no banco
dotnet ef database update \
  --project src/Nimbus.Infrastructure \
  --startup-project src/Nimbus.Api
```

### Frontend

```bash
cd frontend
npm run dev         # servidor de desenvolvimento
npm run build       # type-check + build de produção
npm run lint        # oxlint (falha com qualquer warning)
npm run typecheck   # apenas o TypeScript
```

---

## Convenções

- **Código e domínio em português.** Entidades, casos de uso e colunas do banco
  usam a nomenclatura do negócio (`MovimentacaoEstoque`, `ContaFinanceira`).
  Assinaturas herdadas de interfaces de framework mantêm os nomes originais.
- **Warnings são erros.** `TreatWarningsAsErrors` está ligado em toda a solution
  e o `oxlint` roda com `--max-warnings=0`.
- **Versões centralizadas.** Nenhum `.csproj` declara versão de pacote; tudo vive
  em `Directory.Packages.props`.
- **Exclusão lógica.** Registros de negócio nunca são apagados fisicamente
  (campo `Excluido`), para preservar auditoria e histórico.
- **Erros padronizados.** Toda falha da API responde em `ProblemDetails`
  (RFC 9457); violações de regra de negócio viram HTTP 400.

---

## Roadmap

O roadmap completo está em [SDD.md](SDD.md). Progresso:

- [x] **Sprint 0** — Setup do projeto
- [ ] **Sprint 1** — Autenticação com JWT
- [ ] **Sprint 2** — Layout base e sidebar
- [ ] **Sprint 3** — Tema / dark mode
- [ ] **Sprint 4** — CRUD de Clientes
- [ ] **Sprint 5** — CRUD de Fornecedores
- [ ] **Sprint 6** — CRUD de Usuários internos
- [ ] **Sprint 7** — CRUD de Produtos
- [ ] **Sprint 8** — Estoque / inventário
- [ ] **Sprint 9** — Pedidos de compra
- [ ] **Sprint 10** — Pedidos de venda
- [ ] **Sprint 11** — Financeiro (contas a pagar/receber)
- [ ] **Sprint 12** — Dashboard com gráficos
- [ ] **Sprint 13** — Auditoria / logs
- [ ] **Sprint 14** — Notificações em tempo real (SignalR)
- [ ] **Sprint 15** — RBAC (tela de permissões)
- [ ] **Sprint 16** — Relatórios (PDF/Excel)
- [ ] **Sprint 17** — Relatórios com drill-down
- [ ] **Sprint 18** — Motor de regras / alertas
- [ ] **Sprint 19** — Jobs agendados
- [ ] **Sprint 20** — Busca global (Ctrl+K)
- [ ] **Sprint 21** — CI/CD (GitHub Actions)
- [ ] **Sprint 22** — Health checks + dashboard de status
