# Nimbus ERP

Micro ERP full stack construído como projeto de portfólio: **.NET 10 + React 19**,
com Clean Architecture no backend, organização por feature no frontend e
infraestrutura completa em Docker.

O planejamento completo (22 sprints, critérios de aceite e dependências entre
módulos) está em [SDD.md](SDD.md).

> **Status atual:** Sprint 4 concluída — primeiro módulo de negócio no ar: CRUD
> de Clientes ponta a ponta, com validação de CPF/CNPJ, busca, filtros e
> paginação no servidor.

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
npm run dev         # servidor de desenvolvimento (copia os temas antes)
npm run temas       # copia os temas do PrimeReact para public/temas/
npm run build       # type-check + build de produção
npm run lint        # oxlint (falha com qualquer warning)
npm run typecheck   # apenas o TypeScript
npm test            # Vitest (jsdom + Testing Library)
npm run test:watch  # Vitest em modo observador
```

---

## Cadastro de Clientes

Primeiro módulo de negócio, e o **molde que Fornecedores, Usuários e Produtos
replicam** nas Sprints 5 a 7. O padrão estabelecido aqui:

| Camada | O que foi definido |
|---|---|
| Domínio | Entidade com invariantes + objetos de valor (`Documento`, `Endereco`) |
| Aplicação | Serviço com listar/obter/criar/atualizar/excluir e DTOs por operação |
| Persistência | Repositório com filtro, ordenação por enum e paginação em SQL |
| API | `GET/POST/PUT/DELETE` em `/api/clientes`, todos autenticados |
| Frontend | DataTable com paginação no servidor + formulário em diálogo |

**Regras de negócio que valem a pena conhecer:**

- **CPF e CNPJ são validados por dígito verificador**, não por formato. O
  algoritmo roda nas duas pontas: no navegador para retorno imediato, no
  domínio como autoridade final. Documentos com todos os dígitos iguais
  (`111.111.111-11`) são rejeitados, embora passem no cálculo.
- **O documento é guardado sem máscara.** `529.982.247-25` e `52998224725` são
  o mesmo cliente, e a checagem de duplicidade normaliza antes de comparar.
- **Endereço é tudo ou nada.** Ou nenhum campo, ou o conjunto que o torna
  utilizável — meio endereço não serve para entrega nem para nota fiscal.
- **Nome fantasia só existe para pessoa jurídica**, inclusive ao trocar o tipo
  de um cadastro já existente.
- **Exclusão é lógica.** O registro sai das listagens mas permanece na tabela.
  O índice único do documento é filtrado por `Excluido = 0`, então o CPF de um
  cliente excluído pode ser recadastrado.

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

## Tema

Três modos, selecionáveis no cabeçalho e também na tela de login: **claro**,
**escuro** e **sistema** (acompanha o sistema operacional, e é o padrão). A
preferência fica no `localStorage`.

Dois detalhes que fazem a diferença na prática:

- **Sem flash ao carregar.** Um script inline no `index.html` resolve o tema e
  insere o `<link>` do CSS antes da primeira pintura. Se isso ficasse para
  quando o React montasse, quem usa modo escuro veria um lampejo branco a cada
  carregamento.
- **Sem flash ao trocar.** O CSS novo é inserido em paralelo e o antigo só é
  removido quando o novo termina de carregar. Trocar o `href` do próprio
  `<link>` deixaria a tela sem estilo por alguns quadros.

Os temas do PrimeReact são copiados de `node_modules` para `public/temas/` por
[scripts/copiar-temas.mjs](frontend/scripts/copiar-temas.mjs), que roda
automaticamente antes de `npm run dev` e `npm run build`. A cópia é necessária
porque o `theme.css` referencia as fontes por caminho relativo — um import com
`?url` do Vite emitiria o CSS sem reescrever essas URLs e as fontes quebrariam.
As fontes são idênticas entre os temas, então ficam numa pasta única: economiza
~700 KB e evita que o navegador as baixe de novo a cada troca de tema.

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
- [x] **Sprint 3** — Tema / dark mode
- [x] **Sprint 4** — CRUD de Clientes
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
