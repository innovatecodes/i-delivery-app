# Plano de Implementação Orientado por Estado — iDelivery SaaS Multi-Tenant

## 0. Objetivo

Este plano substitui o plano linear anterior. O agente deve trabalhar de forma **orientada ao estado real do repositório**.

O objetivo é construir o iDelivery, mas antes de implementar qualquer coisa o agente deve descobrir:

1. o que já existe;
2. o que está parcialmente implementado;
3. o que está implementado mas incompleto ou incorreto;
4. o que está testado;
5. o que está documentado;
6. o que corresponde às etapas deste plano;
7. quais etapas podem ser consideradas concluídas;
8. quais dependências ainda faltam.

> **Regra central: NÃO implementar algo simplesmente porque está descrito como "próxima etapa". Primeiro inspecionar o estado atual.**

---

# 1. REGRA ZERO — DISCOVERY ANTES DE QUALQUER ALTERAÇÃO

Antes de criar, editar, mover ou excluir qualquer arquivo, o agente deve executar uma fase de descoberta.

## Regra Crítica de Ambiente e Terminal (Windows / PowerShell)

O ambiente de execução padrão é o **Windows (PowerShell)**.

1. **Proibido usar `&&`**. Utilize ponto e vírgula (`;`) ou comandos separados.
2. **Proibido usar heredoc Unix (`cat << 'EOF'`)**.
3. **Proibido usar pastas temporárias do Linux (`/tmp/`)**.
4. **Proibido poluir a raiz do projeto** com testes, rascunhos ou scripts.
5. **Testes** devem ser executados dentro da pasta de testes correta (`tests/`), utilizando `dotnet test`.

## 1.1 Inspeção obrigatória

Inspecionar, conforme disponível:

- árvore do repositório;
- `README.md`;
- `docs/`;
- arquivos de planejamento;
- `.gitignore`;
- `docker-compose.yml`;
- Dockerfiles;
- solution `.sln` / `.slnx`;
- projetos `.csproj`;
- `package.json`;
- configuração Angular;
- `appsettings*.json`;
- migrations;
- testes;
- configurações EF Core;
- Commands, Queries, Handlers e Validators;
- entidades, aggregates e Value Objects;
- Domain Events;
- autenticação/autorização;
- repositories;
- DbContext;
- controllers/endpoints;
- frontend;
- arquivos de configuração;
- scripts;
- histórico Git quando necessário.

Não assumir nomes de pastas ou projetos. Descobrir primeiro.

## 1.2 Verificação de build e testes

Antes de modificar código, executar, quando aplicável:

- build do backend;
- testes do backend;
- build do frontend;
- testes do frontend;
- validações relevantes de Docker;
- migrations/estado do banco, sem destruir dados.

Se algo falhar, registrar como **estado atual**. Não corrigir automaticamente durante a descoberta.

## 1.3 Classificação

Cada capacidade deve ser classificada como:

- `CONCLUÍDA`
- `PARCIAL`
- `IMPLEMENTADA_COM_PROBLEMAS`
- `NÃO IMPLEMENTADA`
- `NÃO APLICÁVEL`
- `BLOQUEADA`

## 1.4 Evidência obrigatória

Para cada conclusão, apontar evidências:

- arquivos;
- classes/records/interfaces;
- endpoints;
- testes;
- migrations;
- configuração;
- comportamento observado.

Exemplo:

```text
Etapa 08 — Usuário

Status: PARCIAL

Evidências:
- User.cs existe
- PasswordHash é nullable
- existe IUserRepository
- não existe teste de criação de usuário
- ativação ainda não está coberta

Conclusão:
A etapa não deve ser recriada do zero.
Deve ser continuada a partir do estado atual.
```

---

# 2. INVENTÁRIO DE ESTADO

Depois da descoberta, criar ou atualizar:

```text
docs/implementation-status.md
```

Esse arquivo é o **mapa oficial do estado do projeto**.

Estrutura mínima:

```markdown
# Implementation Status

## Última análise
Data:

## Backend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|

## Frontend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|

## Bloqueios
- ...

## Problemas arquiteturais encontrados
- ...

## Testes
- Backend build:
- Backend tests:
- Frontend build:
- Frontend tests:

## Próxima etapa recomendada
...
```

Atualizar depois de cada etapa concluída.

---

# 3. RECONCILIAÇÃO COM O PLANO

Antes de iniciar uma etapa:

1. consultar `docs/implementation-status.md`;
2. verificar novamente o código;
3. confirmar se a etapa ainda é necessária;
4. verificar dependências;
5. identificar código reutilizável;
6. definir o menor conjunto de alterações.

## 3.1 Regra anti-duplicação

Se algo já existe:

- não recriar;
- não duplicar;
- não substituir sem necessidade;
- não criar segunda abstração equivalente;
- não criar nova entidade se a existente atende.

Primeiro avaliar se o código atual pode ser completado/refatorado.

## 3.2 Capacidade não significa necessariamente endpoint HTTP

Uma capacidade do plano representa uma **capacidade arquitetural/funcional**, e não necessariamente uma API HTTP completa.

Por exemplo, B04 — Tenant pode estar `CONCLUÍDA` quando:

- o agregado Tenant está correto;
- casos de uso estão implementados;
- persistência está implementada;
- regras de negócio estão implementadas;
- testes relevantes existem;
- build passa;
- arquitetura está correta.

A ausência de um `TenantController` **não torna automaticamente B04 incompleta**.

A exposição HTTP dessas capacidades pertence à camada API e deve ser avaliada principalmente em:

```text
B19 — API e qualidade
```

Exemplo:

```text
B04 — Tenant
├── Domain              concluído
├── Application         concluído
├── Infrastructure      concluído
├── Tests               concluído
└── HTTP API            pode ser tratada em B19
```

---

# 4. FLUXO OBRIGATÓRIO DO AGENTE

Para cada etapa selecionada:

```text
DISCOVER
  ↓
MAPEAR ESTADO ATUAL
  ↓
COMPARAR COM REQUISITOS
  ↓
IDENTIFICAR GAPS
  ↓
PLANEJAR MENOR ALTERAÇÃO NECESSÁRIA
  ↓
IMPLEMENTAR
  ↓
TESTAR
  ↓
REVISAR ARQUITETURA
  ↓
ATUALIZAR STATUS
  ↓
RESUMIR
  ↓
PARAR
```

Nunca pular Discovery.

---

# 5. NÃO REFAZER O QUE JÁ EXISTE

Antes de criar qualquer tipo, pesquisar se já existe equivalente.

Pesquisar por:

- nome;
- responsabilidade;
- namespace;
- interface;
- propriedades;
- comportamento;
- endpoint;
- teste;
- configuração.

Exemplos:

```text
Tenant
ITenantRepository
CreateTenant
TenantConfiguration
TenantId
```

```text
Jwt
Token
RefreshToken
Password
Authentication
Authorization
Login
Register
```

```text
ICommand
IQuery
ICommandHandler
IQueryHandler
Dispatcher
Handler
```

Se já houver implementação equivalente, avaliá-la antes de criar outra.

---

# 6. CRITÉRIOS DE DECISÃO

### Caso A — Está correto

Marcar como `CONCLUÍDA`.

### Caso B — Está incompleto

Marcar como `PARCIAL` e implementar somente o que falta.

### Caso C — Está funcional, mas viola princípios

Marcar como `IMPLEMENTADA_COM_PROBLEMAS`. Corrigir somente se relevante para a etapa atual ou se bloquear etapas seguintes.

### Caso D — Está duplicado

Escolher implementação canônica, remover/refatorar duplicidade com cautela e testar.

### Caso E — Não é possível concluir

Marcar como `BLOQUEADA` e explicar a dependência.

---

# 7. FLUXO FUNCIONAL QUE O AGENTE DEVE ENTENDER

## 7.1 SaaS

```text
SaaS Público
    ↓
Cadastro de Tenant
    ↓
Trial de 7 dias
    ↓
Tenant + usuário gestor
    ↓
Ativação por e-mail
    ↓
Login
    ↓
Dashboard do Tenant
```

## 7.2 Cliente

```text
Catálogo público
    ↓
Categorias
    ↓
Produtos
    ↓
Detalhes
    ↓
Carrinho
    ↓
Finalizar pedido
    ↓
Login/Cadastro quando necessário
    ↓
Endereço
    ↓
Entrega
    ↓
Pagamento
    ↓
Confirmação
    ↓
Pedido
    ↓
Acompanhamento
    ↓
Entrega
```

## 7.3 Tenant

```text
Login
    ↓
Dashboard
    ↓
Categorias
    ↓
Produtos
    ↓
Receber pedido
    ↓
Confirmar
    ↓
Preparar
    ↓
Pronto
    ↓
Encaminhar para delivery
    ↓
Acompanhar entrega
```

## 7.4 Delivery

```text
Login
    ↓
Entregas disponíveis/atribuídas
    ↓
Aceitar/assumir quando permitido
    ↓
Sair para entrega
    ↓
Rastreamento/geolocalização
    ↓
Entregar
    ↓
Confirmar entrega
```

## 7.5 Super Admin

```text
Login
    ↓
Dashboard SaaS
    ↓
Tenants
    ↓
Planos
    ↓
Assinaturas
    ↓
Status/utilização
    ↓
Bloquear/desbloquear
```

---

# 8. PLANO DE CAPACIDADES

## B01 — Monorepo e infraestrutura

- apps/client
- apps/server
- docs
- Docker
- configuração inicial

## B02 — Fundação .NET

- solution
- projetos
- referências
- Clean Architecture
- health checks
- configuração

## B03 — Shared Kernel

- Entity
- AggregateRoot
- ValueObject quando necessário
- DomainEvent
- Result/Errors se adotado
- CQRS próprio
- dispatcher próprio

## B04 — Tenant

- agregado Tenant
- identidade
- status
- dados essenciais
- persistência
- EF Core
- PostgreSQL

**Importante:** B04 avalia a capacidade Tenant no domínio, aplicação e infraestrutura. A ausência de Controller HTTP não impede automaticamente B04 de ser concluída.

Se Commands/Queries já existirem, devem ser reutilizados posteriormente pela API.

Exemplos:

```text
CreateTenantCommand
GetTenantQuery
GetTenantsQuery
ActivateTenantCommand
BlockTenantCommand
UpdateTenantCommand
DeleteTenantCommand
```

Não criar novos Commands/Queries apenas para atender à API se equivalentes já existirem.

## B05 — Roles e Users

- roles
- usuário
- credenciais
- tenant
- status

## B06 — Autenticação

- Register
- Login
- JWT
- Refresh Token
- hash seguro de senha
- ativação por e-mail
- eventos de registro/ativação

## B07 — Multi-tenancy

- contexto do tenant
- isolamento
- autorização
- ownership
- proteção contra TenantId arbitrário

## B08 — Catálogo

- categorias
- produtos
- CRUD
- consultas
- filtros
- paginação no banco

## B09 — Carrinho

- adicionar
- quantidade
- remover
- limpar
- consulta
- validação de tenant/produto

## B10 — Customer e endereço

- cliente
- conta
- perfil
- endereços
- Value Objects quando apropriado

## B11 — Delivery settings

V1:

- grátis
- grátis acima de valor
- taxa fixa
- taxa por distância

## B12 — Pedido e checkout

- Order
- estados
- transições
- snapshot
- cálculo
- criação
- limpeza do carrinho

## B13 — Pagamento V1

- CASH
- CARD_ON_DELIVERY
- abstração extensível

## B14 — Gestão de pedidos

- tenant gerencia pedidos
- confirmação
- preparo
- pronto
- cancelamento

## B15 — Delivery

- entregadores
- associação ao tenant
- atribuição
- fluxo de entrega

## B16 — Rastreamento

- geolocalização
- localização do entregador
- rota
- distância
- cálculo automático da taxa
- abstração de mapas/geolocalização

## B17 — SaaS

- Super Admin
- gestão de tenants
- planos
- assinaturas
- trial 7 dias
- mensal
- trimestral
- ciclo de assinatura

## B18 — Notificações

- Domain Events
- notificações internas
- realtime
- abstração de canais
- consulta
- lidas/não lidas

O agente deve avaliar cada requisito individualmente. Uma implementação parcial de e-mail não significa automaticamente que B18 está concluída.

## B19 — API e qualidade

- OpenAPI
- erros
- autenticação
- autorização
- controllers/endpoints
- paginação
- testes de API
- contratos HTTP
- integração dos Commands/Queries existentes com a API

### Regra importante

B19 é responsável pela **exposição HTTP das capacidades que precisam ser consumidas pela API**.

Não assumir que cada entidade ou Aggregate exige um Controller próprio.

Antes de criar Controllers:

1. descobrir endpoints existentes;
2. descobrir Commands e Queries existentes;
3. mapear casos de uso que precisam ser expostos;
4. verificar se já existe Controller equivalente;
5. reutilizar Commands/Queries existentes;
6. criar somente endpoints necessários;
7. manter Controllers finos;
8. criar testes de integração da API;
9. revisar autenticação e multi-tenancy.

Exemplo:

```text
HTTP
 ↓
TenantController
 ↓
CreateTenantCommand
 ↓
CreateTenantCommandHandler
```

ou:

```text
HTTP
 ↓
TenantController
 ↓
GetTenantQuery
 ↓
GetTenantQueryHandler
```

A existência de uma entidade ou Aggregate não implica automaticamente a criação de um Controller.

Não utilizar:

```text
Entity → Controller
```

nem:

```text
Aggregate → Controller
```

A regra correta é:

```text
Caso de uso necessário pela API
        ↓
Command/Query
        ↓
Endpoint HTTP
        ↓
Controller quando apropriado
```

A granularidade dos Controllers deve ser definida pelos recursos e casos de uso HTTP, e não simplesmente pela estrutura do Domain.

### Exemplo de organização

```text
Controllers/
├── AuthController.cs
├── TenantsController.cs
├── ProductsController.cs
├── CategoriesController.cs
├── OrdersController.cs
└── DeliveriesController.cs
```

Esta árvore é apenas ilustrativa. Não criar esses Controllers automaticamente.

### Mapeamento de API

Antes de considerar B19 concluída, o agente deve possuir um mapeamento dos contratos reais, por exemplo:

```text
Recurso       Endpoint                  Operação
---------------------------------------------------------------
Auth          POST /api/auth/login      LoginCommand
Auth          POST /api/auth/register   RegisterCommand

Tenant        POST /api/tenants         CreateTenantCommand
Tenant        GET /api/tenants/{id}     GetTenantQuery

Product       POST /api/products        CreateProductCommand
Product       GET /api/products         GetProductsQuery

Order         POST /api/orders          CreateOrderCommand
Order         GET /api/orders/{id}      GetOrderQuery
```

Os exemplos acima são ilustrativos. Não criar endpoints automaticamente.

---

## F01 — Angular bootstrap

- Angular 20
- standalone
- TypeScript
- Tailwind
- Font Awesome
- Router
- HttpClient
- PWA base
- estrutura por features

## F02 — Design System

- Button
- Input
- Select
- Modal
- Drawer
- Card
- Badge
- Table
- Pagination
- Empty State
- Loading
- Toast
- Confirm Dialog

## F03 — Frontend Auth

- login
- cadastro
- recuperação quando API existir
- token handling
- guards
- interceptors
- user state

## F04 — Área pública SaaS

- landing
- benefícios
- planos
- trial
- FAQ
- CTA

## F05 — Catálogo

- categorias
- produtos
- busca
- filtros
- paginação
- detalhes

## F06 — Carrinho e checkout

- carrinho
- identificação
- endereço
- entrega
- pagamento
- resumo
- criação do pedido

## F07 — Cliente

- perfil
- endereços
- pedidos
- detalhes

## F08 — Tenant

- dashboard
- produtos
- categorias
- pedidos

## F09 — Delivery

- dashboard
- entregas
- status
- confirmação
- mapa/rastreamento

## F10 — Super Admin

- tenants
- planos
- assinaturas

## F11 — Realtime/Notifications

- SignalR ou tecnologia adequada
- central de notificações
- badges
- toasts

## F12 — Push/PWA

- Web Push
- service worker
- manifest
- cache
- instalação

## F13 — Qualidade frontend

- mobile first
- responsividade
- acessibilidade
- performance
- SEO público
- lazy loading
- testes
- Docker/Nginx

---

# 9. BACKEND — PRINCÍPIOS OBRIGATÓRIOS

- C# / .NET
- ASP.NET Core
- EF Core
- PostgreSQL
- Clean Architecture
- DDD
- SOLID
- CQRS próprio, sem MediatR
- Domain Events
- FluentValidation
- testes unitários e integração
- controllers finos
- regras de negócio fora de Controllers/DbContext/Repositories
- Dependency Inversion
- isolamento multi-tenant

## CQRS

Commands e Queries são explícitos.

Cada um deve possuir seu Handler.

O dispatcher deve ser próprio.

Não utilizar MediatR ou outra biblioteca de Mediator.

---

# 10. API, CONTROLLERS E ENDPOINTS

A API pertence à camada de apresentação.

Sua responsabilidade é adaptar HTTP para os casos de uso da Application.

```text
HTTP Request
    ↓
Controller
    ↓
Command / Query
    ↓
Dispatcher
    ↓
Handler
    ↓
Domain / Application
    ↓
Repository / Infrastructure
    ↓
Database
```

## Controllers podem

- receber requisições HTTP;
- realizar model binding;
- receber parâmetros de rota/query/body;
- obter informações do usuário autenticado;
- obter contexto do tenant;
- encaminhar Commands/Queries;
- mapear resultados para respostas HTTP;
- retornar status codes apropriados.

## Controllers não podem

- conter regras de negócio;
- acessar diretamente `DbContext`;
- acessar diretamente repositories para executar casos de uso;
- criar entidades de domínio;
- calcular preços;
- calcular taxas;
- decidir regras de pedido;
- controlar transições de estado;
- implementar regras de autorização de negócio;
- confiar em `TenantId` enviado arbitrariamente pelo cliente;
- duplicar lógica dos Handlers.

## 10.1 Um recurso não implica um Controller obrigatório

Não utilizar:

```text
Entity → Controller
```

nem:

```text
Aggregate → Controller
```

A regra correta é:

```text
Caso de uso necessário pela API
        ↓
Command/Query
        ↓
Endpoint HTTP
        ↓
Controller quando apropriado
```

## 10.2 Regra de reutilização

Se existir:

```text
CreateTenantCommand
CreateTenantCommandHandler
```

não criar serviços paralelos apenas para atender o Controller.

O Controller deve utilizar o caso de uso existente.

---

# 11. UM ARQUIVO POR TIPO

Regra obrigatória em todo backend C#.

Cada `.cs` deve conter apenas um:

- `class`;
- `interface`;
- `record`;
- `enum`;
- `struct`;
- `abstract class`;

público/internal.

O arquivo deve possuir o mesmo nome do tipo.

Exemplo:

```text
UserRegisteredDomainEvent.cs
UserActivatedDomainEvent.cs
CreateTenantCommand.cs
CreateTenantCommandHandler.cs
CreateTenantCommandValidator.cs
```

Tipos privados auxiliares aninhados são a única exceção.

Antes de concluir qualquer etapa:

1. verificar arquivos `.cs` criados/alterados;
2. detectar múltiplos tipos;
3. separar quando necessário;
4. executar build/testes finais.

Não colocar comentários de cabeçalho antes de `using`, `namespace` ou declaração de tipo.

---

# 12. SEGURANÇA E IDENTIDADE

- domínio não conhece infraestrutura de hashing;
- hashing de senha deve ser responsabilidade de abstração apropriada;
- implementação concreta fica fora do domínio;
- tokens devem ser tratados por abstrações de autenticação;
- JWT não deve contaminar entidades de domínio;
- refresh token deve possuir política clara de expiração/revogação;
- login deve respeitar status da conta;
- TenantId nunca deve ser confiado cegamente vindo do cliente.

Não criar hashes temporários apenas para satisfazer persistência quando o fluxo arquitetural prevê senha ainda não definida.

---

# 13. DOMAIN EVENTS E E-MAIL

Registro:

```text
Register
  ↓
User criado como pendente
  ↓
UserRegisteredDomainEvent
  ↓
handler
  ↓
token/código de ativação
  ↓
IEmailSender
  ↓
e-mail
```

Ativação:

```text
ActivateAccount
  ↓
validar token
  ↓
ativar usuário
  ↓
UserActivatedDomainEvent
```

O domínio não envia e-mail.

A Infrastructure implementa o envio.

Não duplicar esse mecanismo para Tenant Admin/Super Admin criados por outro usuário.

---

# 14. PEDIDO — REGRAS IMPORTANTES

O pedido deve possuir snapshot dos itens:

- produto;
- nome;
- preço;
- quantidade;
- subtotal.

Pedidos antigos não podem mudar porque o produto foi alterado depois.

Estados devem ser controlados pelo domínio.

Fluxo base:

```text
PENDING
→ CONFIRMED
→ PREPARING
→ READY_FOR_DELIVERY
→ OUT_FOR_DELIVERY
→ DELIVERED
```

Com cancelamento somente nas transições permitidas.

---

# 15. DELIVERY E TAXA

V1 deve suportar:

```text
Entrega grátis
Entrega grátis acima de determinado valor
Taxa fixa
Taxa por distância
```

Para cálculo por distância:

```text
Tenant
  ↓
Endereço de entrega
  ↓
Provedor de mapas
  ↓
Rota
  ↓
Distância
  ↓
Política de entrega
  ↓
Taxa
```

A aplicação deve depender de abstração.

O domínio não deve depender diretamente de Google Maps, Mapbox ou outro provedor.

---

# 16. FRONTEND — REGRA DE CONTRATO

O Angular consome a API.

Não duplicar regras de negócio do backend.

Frontend pode conter:

- validação de formulário;
- estado visual;
- UX;
- guards;
- interceptors;
- transformação de apresentação.

Backend continua sendo autoridade para:

- autorização;
- tenant;
- preços;
- estoque quando aplicável;
- regras de pedido;
- taxa;
- pagamento;
- transições de estado.

---

# 17. TESTES

O agente deve identificar testes existentes antes de criar novos.

Não duplicar testes.

Para cada alteração:

- atualizar testes existentes quando apropriado;
- criar testes apenas para comportamento novo ou ainda descoberto;
- executar build;
- executar testes.

Principais áreas:

- Tenant
- User/Auth
- Multi-tenancy
- Catalog
- Cart
- Checkout
- Order
- Payment
- Delivery
- Subscription
- Notifications
- API

---

# 18. GIT

Não executar `git commit` automaticamente.

**Importante:** sempre execute `git add .` ou adicione explicitamente todos os arquivos modificados e criados antes de sugerir ou realizar qualquer commit.

O agente deve sugerir um commit pequeno e semanticamente relacionado em **Português do Brasil (PT-BR)**, seguindo o padrão imperativo.

Conventional Commits:

```text
feat:
fix:
refactor:
test:
docs:
chore:
build:
ci:
```

Não misturar várias capacidades independentes em um único commit.

Não alterar histórico Git sem autorização explícita.

---

# 19. CHECKLIST ANTES DE IMPLEMENTAR UMA ETAPA

```text
[ ] Li o implementation-status.md
[ ] Inspecionei os arquivos relacionados
[ ] Pesquisei implementações equivalentes
[ ] Verifiquei dependências
[ ] Identifiquei código reutilizável
[ ] Identifiquei gaps
[ ] Defini o menor escopo necessário
[ ] Não estou recriando funcionalidade existente
[ ] Não estou antecipando etapa futura
[ ] Verifiquei se a capacidade exige exposição HTTP
[ ] Verifiquei se já existem Commands/Queries reutilizáveis
[ ] Verifiquei se já existe Controller/endpoint equivalente
```

---

# 20. CHECKLIST ANTES DE CONCLUIR UMA ETAPA

```text
[ ] Implementação limitada ao escopo
[ ] Testes atualizados/criados
[ ] Build executado
[ ] Testes executados
[ ] Arquivos C# verificados
[ ] Um tipo por arquivo
[ ] Nomes dos arquivos corretos
[ ] Arquitetura revisada
[ ] Multi-tenancy revisado quando aplicável
[ ] Segurança revisada quando aplicável
[ ] API/Controllers revisados quando aplicável
[ ] implementation-status.md atualizado
[ ] Commit sugerido
```

---

# 21. REGRA DE PARADA

Ao concluir uma etapa, o agente DEVE PARAR.

Resposta obrigatória:

```text
Etapa analisada/concluída: <nome>

Status antes da alteração:

<CONCLUÍDA/PARCIAL/IMPLEMENTADA_COM_PROBLEMAS/NÃO IMPLEMENTADA>

O que já existia:

- ...

O que foi implementado nesta etapa:

- ...

O que não foi alterado:

- ...

Testes:

- Build: OK/FALHOU/NÃO APLICÁVEL
- Testes: OK/FALHOU/NÃO APLICÁVEL

Status atualizado:

- ...

Commit sugerido:

<commit>

A etapa foi concluída.

O que deseja fazer agora?

0. Deseja seguir para a próxima etapa?

1. Deseja realizar o commit sugerido?

2. Deseja verificar/revisar o que foi implementado antes de continuar?

3. Deseja apontar algum ajuste, ponto específico ou bug no código atual antes de prosseguir?

Informe a opção para confirmar sua decisão no formato:

0 para (Y), 1 para (Y), 2 para (Y), 3 para (Y)

AGUARDANDO RESPOSTA DO USUÁRIO.
```

Mesmo que a etapa seja considerada `CONCLUÍDA` sem alteração, o agente deve parar.

**Nunca avançar automaticamente.**

---

# 22. REGRA PARA ETAPA JÁ CONCLUÍDA

Se o agente descobrir que uma etapa já está completamente implementada:

1. não reimplementar;
2. validar evidências;
3. executar apenas verificações necessárias;
4. marcar `CONCLUÍDA`;
5. atualizar `implementation-status.md`;
6. informar que a etapa foi pulada por já existir;
7. parar.

Exemplo:

```text
Etapa B04 — Tenant

Status: CONCLUÍDA

A implementação já existente atende aos requisitos desta etapa.

Nenhum código foi recriado.

A existência ou ausência de Controller HTTP será avaliada
separadamente na capacidade B19 — API e qualidade.
```

---

# 23. REGRA PARA IMPLEMENTAÇÃO PARCIAL

Se estiver parcialmente implementada:

```text
Etapa B06 — Autenticação

Status: PARCIAL

Já existe:
- JWT
- Login
- Refresh Token

Falta:
- ativação por e-mail
- testes de expiração
- evento de registro

Ação:
Implementar somente os gaps.
```

Não começar novamente do zero.

---

# 24. REGRA PARA CONFLITOS COM O CÓDIGO ATUAL

Se o código atual divergir deste plano:

1. não assumir que o plano está certo;
2. não assumir que o código está certo;
3. analisar a responsabilidade;
4. verificar impacto nas etapas seguintes;
5. preferir a solução mais simples e arquiteturalmente consistente;
6. documentar a decisão em `implementation-status.md`;
7. somente refatorar o necessário.

---

# 25. DOCUMENTAÇÃO DE DECISÕES

Quando uma decisão arquitetural relevante for tomada, registrar em:

```text
docs/architecture-decisions.md
```

Formato:

```markdown
## ADR-XXX — <título>

### Contexto

...

### Decisão

...

### Motivo

...

### Impacto

...
```

Não criar ADR para decisões triviais.

---

# 26. DEFINIÇÃO DE PRONTO

Uma capacidade só pode ser `CONCLUÍDA` quando:

1. requisito implementado;
2. integração com código existente correta;
3. regras de negócio respeitadas;
4. testes relevantes existentes;
5. build passa;
6. arquitetura não possui violação relevante;
7. segurança adequada quando aplicável;
8. documentação/status atualizado.

**"Existe um arquivo com esse nome" NÃO significa concluído.**

Da mesma forma:

**"Existe um Controller com esse nome" NÃO significa que a API está concluída.**

O agente deve verificar o fluxo completo quando aplicável:

```text
Endpoint
  ↓
Controller
  ↓
Command/Query
  ↓
Dispatcher
  ↓
Handler
  ↓
Domain
  ↓
Infrastructure
  ↓
Database
```

---

# 27. ORDEM DE PRIORIZAÇÃO DINÂMICA

Depois do primeiro Discovery, o agente deve escolher a próxima ação com esta prioridade:

1. corrigir bloqueios que impedem o sistema de compilar/testar;
2. completar fundações arquiteturais ausentes;
3. completar dependências de domínio;
4. completar casos de uso essenciais;
5. completar persistência;
6. completar API;
7. completar testes;
8. somente então avançar para capacidades dependentes;
9. frontend somente quando os contratos backend necessários existirem.

A prioridade pode alterar a ordem numérica das capacidades, mas o agente deve explicar o motivo e atualizar o status.

---

# 28. PRIMEIRA AÇÃO DO AGENTE

Ao receber este plano, o agente NÃO deve começar a programar.

A primeira resposta operacional deve ser:

```text
INICIANDO DISCOVERY DO REPOSITÓRIO.

Vou:

1. Inspecionar a estrutura atual.
2. Identificar backend/frontend.
3. Mapear arquitetura e dependências.
4. Localizar funcionalidades já implementadas.
5. Verificar builds e testes.
6. Comparar o estado real com este plano.
7. Criar/atualizar docs/implementation-status.md.
8. Apresentar o diagnóstico.

Nenhuma alteração funcional será feita nesta fase.
```

Depois executar somente a descoberta.

Ao terminar o Discovery, PARAR e aguardar autorização para implementar.

---

# 29. RESULTADO ESPERADO

O agente deve funcionar como um **engenheiro entrando em um projeto existente**, e não como um gerador que assume um repositório vazio.

```text
PLANO
  ↓
DISCOVERY
  ↓
ESTADO REAL
  ↓
MAPA DE IMPLEMENTAÇÃO
  ↓
GAPS
  ↓
PRÓXIMA CAPACIDADE
  ↓
IMPLEMENTAÇÃO INCREMENTAL
  ↓
VALIDAÇÃO
  ↓
STATUS
  ↓
PARADA
  ↓
AUTORIZAÇÃO DO USUÁRIO
```

A principal regra é:

> **O código existente é a fonte de verdade sobre o que já foi implementado. O plano é a fonte de verdade sobre o que ainda precisa ser alcançado. O agente deve reconciliar os dois antes de agir.**

---

# 30. REGRA FINAL SOBRE DOMÍNIO, APPLICATION E API

Para evitar interpretações incorretas sobre o estado das capacidades, o agente deve distinguir claramente:

## DOMAIN

Responsabilidade:

- entidades;
- aggregates;
- Value Objects;
- regras de negócio;
- Domain Events.

## APPLICATION

Responsabilidade:

- Commands;
- Queries;
- Handlers;
- Validators;
- orquestração dos casos de uso;
- abstrações necessárias.

## INFRASTRUCTURE

Responsabilidade:

- EF Core;
- PostgreSQL;
- repositories;
- serviços externos;
- e-mail;
- JWT;
- implementações de abstrações.

## API

Responsabilidade:

- HTTP;
- Controllers;
- autenticação HTTP;
- autorização HTTP;
- model binding;
- respostas HTTP;
- OpenAPI.

Uma capacidade pode estar concluída em:

```text
Domain
Application
Infrastructure
Tests
```

sem necessariamente possuir:

```text
Controller
Endpoint HTTP
```

Quando isso ocorrer, o status da capacidade funcional pode continuar como `CONCLUÍDA`, enquanto a exposição HTTP permanece como trabalho de `B19 — API e qualidade`.

Exemplo:

```text
B04 — Tenant

Domain          concluído
Application     concluído
Infrastructure  concluído
Tests           concluído
API             pendente de B19
```

Isso **não deve ser interpretado como duplicidade ou implementação incompleta de Tenant**.

Quando B19 for executada, o agente deve reutilizar os casos de uso existentes:

```text
HTTP
 ↓
Controller
 ↓
Existing Command / Query
 ↓
Existing Handler
 ↓
Existing Domain Logic
```

e não recriar a funcionalidade de Tenant na camada API.

---

# FIM
