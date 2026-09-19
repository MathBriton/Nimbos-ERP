# Nimbus ERP

Micro ERP full stack construído como projeto de portfólio: **.NET 10 + React 19**,
com Clean Architecture no backend, organização por feature no frontend e
infraestrutura completa em Docker.

O planejamento completo (22 sprints, critérios de aceite e dependências entre
módulos) está em [SDD.md](SDD.md).

> **Status atual:** Sprint 2 concluída — aplicação navegável: sidebar com todos
> os módulos do ERP, rotas protegidas e layout responsivo.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API, Entity Framework Core 10 |
| Banco | SQL Server 2022 |
| Frontend | React 19, TypeScript 6, Vite 8 |
| UI | PrimeReact 10 (MIT) + PrimeIcons |
| Roteamento | React Router 8 |
| Autenticação | JWT Bearer (HMAC-SHA256) + BCrypt |
| Docs da API | OpenAPI + [Scalar](https://scalar.com) |
| Infra | Docker + Docker Compose |
| Testes | xUnit (backend) + Vitest & Testing Library (frontend) |

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

### Credenciais de acesso

Na primeira subida a API cria o usuário administrador automaticamente:

| E-mail | Senha |
|---|---|
| `admin@erp.com` | `Admin123!` |

Os valores vêm da seção `Seed:Administrador` do `appsettings.json` e podem ser
sobrescritos por variável de ambiente
(`Seed__Administrador__Email`, `Seed__Administrador__Senha`). Para desligar o
seed por completo, use `Seed__Habilitado=false`.

> Em qualquer ambiente exposto, troque `Jwt:Segredo` e a senha do seed — ambos
> têm valores de desenvolvimento versionados no repositório de propósito, para
> que `docker compose up` funcione sem configuração manual.

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
        │   ├── layout/          Sidebar, cabeçalho e a definição do menu
        │   └── rotas/           Mapa de rotas, gerado a partir do menu
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
npm test            # Vitest (jsdom + Testing Library)
npm run test:watch  # Vitest em modo observador
```

---

## Navegação

O menu é declarado em um único lugar,
[app/layout/menu.ts](frontend/src/app/layout/menu.ts), e a sidebar **e** as rotas
são geradas a partir dele. Adicionar um módulo é mexer em um arquivo só, sem
risco de sidebar e roteador saírem de sincronia — e todo caminho que ainda não
tem tela recebe automaticamente um placeholder informando em qual sprint ele
chega.

| Seção | Módulos |
|---|---|
| — | Dashboard |
| Cadastros | Clientes, Fornecedores, Produtos, Usuários |
| Operação | Estoque, Pedidos de compra, Pedidos de venda |
| Financeiro | Contas |
| Análise | Relatórios, Auditoria |
| Sistema | Configurações, Status |

**Responsividade:** acima de 960px a sidebar ocupa espaço fixo e pode ser
recolhida a só ícones (preferência guardada no `localStorage`). Abaixo disso ela
vira gaveta sobreposta, que fecha ao navegar, ao clicar fora e com `Esc`.
Fechada, recebe `aria-hidden` e `inert`, para não ficar alcançável por leitor de
tela nem por `Tab`.

---

## Autenticação

Fluxo stateless com JWT Bearer:

```
POST /api/auth/login   { email, senha }  ->  { token, expiraEm, usuario }
GET  /api/auth/eu      Authorization: Bearer <token>  ->  { id, nome, email }
```

O token carrega as claims curtas do padrão JWT (`sub`, `email`, `name`, `jti`)
em vez das URIs longas de `ClaimTypes` — por isso a API roda com
`MapInboundClaims = false`. A validação exige explicitamente `HmacSha256`, para
que um token `alg: none` não seja aceito.

**Decisões de segurança relevantes:**

- Senhas com **BCrypt**, work factor 12. A senha em texto puro nunca é
  persistida nem registrada em log.
- E-mail inexistente e senha errada devolvem **a mesma mensagem genérica**, e o
  caso "e-mail inexistente" ainda gasta uma verificação de hash de isca — sem
  isso, a diferença de tempo de resposta revelaria quais contas existem.
- **Bloqueio temporário** de 15 minutos após 5 falhas consecutivas. O bloqueio é
  checado *antes* da senha, que é justamente o que torna a força bruta barata de
  recusar.
- Conta inativa só é revelada a quem já acertou a senha.
- O 401 por token expirado vem com o header `X-Token-Expirado`, exposto via
  CORS, para o frontend distinguir "sessão expirada" de "credencial inválida".

**No frontend**, a sessão fica em `localStorage` e é revalidada na API a cada
carregamento da página. Isso expõe o token a XSS; a alternativa mais segura
(cookie `httpOnly`) exigiria API e frontend no mesmo site ou CORS com
credenciais mais CSRF token, complexidade que não se paga neste estágio. O
mitigador é a validade curta do token. A decisão está documentada em
[armazenamentoDeSessao.ts](frontend/src/features/autenticacao/servicos/armazenamentoDeSessao.ts).

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
- [x] **Sprint 1** — Autenticação com JWT
- [x] **Sprint 2** — Layout base e sidebar
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
