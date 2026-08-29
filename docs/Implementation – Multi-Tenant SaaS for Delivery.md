# Plano de Implementação — SaaS Multi-Tenant para Sistema de Delivery

## 1. Objetivo

Construir um SaaS multi-tenant para gerenciamento de delivery, com:

- Área pública do SaaS.
- Cadastro e autenticação de usuários.
- Gestão de tenants.
- Gestão de produtos, categorias e opções.
- Catálogo público de produtos.
- Carrinho.
- Checkout.
- Pedidos.
- Pagamentos inicialmente por dinheiro ou cartão na entrega.
- Gestão de pedidos por tenant.
- Gestão de entregadores.
- Atualização de status dos pedidos.
- Notificações em tempo real.
- Push notifications.
- PWA.
- Planos SaaS com trial gratuito de 7 dias.
- Cobrança mensal e trimestral.
- Arquitetura preparada para futuros meios de pagamento e canais de notificação, como WhatsApp e e-mail.

---

# REGRAS GERAIS DE EXECUÇÃO

## Regra principal: uma etapa por vez

O agente **NÃO deve executar várias etapas simultaneamente**.

Para cada etapa:

1. Ler a etapa atual.
2. Implementar somente o que pertence à etapa.
3. Criar ou atualizar os testes necessários.
4. Executar build.
5. Executar testes.
6. Corrigir problemas encontrados.
7. Revisar acoplamento e responsabilidades.
8. Informar resumidamente o que foi implementado.
9. Sugerir uma mensagem de commit.
10. **PARAR e perguntar explicitamente ao usuário o que ele deseja fazer a seguir.**

O agente somente poderá continuar quando o usuário confirmar que o commit foi realizado ou solicitar explicitamente a continuação.

### Pergunta obrigatória ao final de cada etapa

Ao concluir **cada etapa de implementação**, o agente deve obrigatoriamente:

1. Apresentar um resumo objetivo do que foi implementado.
2. Sugerir a mensagem de commit correspondente à etapa, quando aplicável.
3. **Interromper a execução e perguntar ao usuário o que deseja fazer em seguida.**
4. **Nunca assumir uma opção, executar ações automaticamente ou avançar para a próxima etapa sem a resposta explícita do usuário.**

A pergunta deve ser apresentada sempre neste formato:

```text
O que deseja fazer agora?

0. Deseja seguir para a próxima etapa?
1. Deseja realizar o commit sugerido?
2. Deseja verificar/revisar o que foi implementado antes de continuar?
3. Deseja apontar algum ajuste, ponto específico ou bug no código atual antes de prosseguir?

Informe a opção para confirmar sua decisão no formato:
0 para (Y), 1 para (Y), 2 para (Y), 3 para (Y) ou qualquer tecla para interromper a execução.
```

### Não fazer

- Não antecipar funcionalidades de etapas futuras.
- Não criar entidades de domínio desnecessárias antecipadamente.
- Não criar abstrações sem necessidade real.
- Não misturar responsabilidades entre camadas.
- Não implementar frontend enquanto o Plano 1 não estiver concluído.
- Não implementar regras de negócio diretamente em Controllers.
- Não criar um "God Service".
- Não ignorar testes.
- Não alterar funcionalidades já concluídas sem necessidade.
- Não fazer grandes refatorações fora do escopo da etapa atual.

---

# GIT E COMMITS

Cada etapa deve resultar em um commit pequeno e semanticamente relacionado.

Formato recomendado:

```text
<tipo>: <descrição>
```

Exemplos:

```text
feat: adiciona estrutura inicial do projeto
feat: configura arquitetura do backend
feat: adiciona entidade tenant
feat: implementa autenticação
feat: adiciona gerenciamento de categorias
feat: implementa catálogo de produtos
test: adiciona testes para criação de pedido
```

Preferir Conventional Commits.

Tipos principais:

- `feat`
- `fix`
- `refactor`
- `test`
- `docs`
- `chore`
- `build`
- `ci`

---

# ARQUITETURA GERAL

O projeto será um monorepo:

```text
Project/
├── apps/
│   ├── client/
│   └── server/
├── docs/
├── docker/
│   ├── client/
│   └── server/
├── docker-compose.yml
├── .gitignore
├── README.md
└── ...
```

## Backend

Tecnologias:

- C#
- .NET
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- CQRS (implementação **pura**, sem uso de bibliotecas de Mediator, como MediatR)
- Clean Architecture
- DDD
- FluentValidation
- Testes unitários e de integração

### Estrutura final esperada

> **Nota:** A estrutura abaixo é uma referência arquitetural. Não criar arquivos fictícios ou vazios apenas para reproduzir a árvore. Criar somente aquilo que for necessário para o código existente.

Organizar o backend aproximadamente da seguinte maneira:

```text
apps/
└── server/
    ├── src/
    │   ├── IDelivery.Domain/
    │   │   ├── Common/
    │   │   │   ├── Entities/
    │   │   │   │   ├── Entity.cs
    │   │   │   │   └── AggregateRoot.cs
    │   │   │   │
    │   │   │   ├── ValueObjects/
    │   │   │   │   └── ValueObject.cs
    │   │   │   │
    │   │   │   └── DomainEvents/
    │   │   │       ├── IDomainEvent.cs
    │   │   │       └── DomainEvent.cs
    │   │   │
    │   │   ├── Errors/
    │   │   │   └── DomainErrors.cs
    │   │   │
    │   │   └── Result.cs
    │   │
    │   ├── IDelivery.Application/
    │   │   ├── Abstractions/
    │   │   │   ├── Persistence/
    │   │   │   │   ├── IRepository.cs
    │   │   │   │   └── IUnitOfWork.cs
    │   │   │   │
    │   │   │   ├── Authentication/
    │   │   │   │   └── ICurrentUser.cs
    │   │   │   │
    │   │   │   ├── Services/
    │   │   │   │   └── IEmailService.cs
    │   │   │   │
    │   │   │   └── Events/
    │   │   │       └── IDomainEventDispatcher.cs
    │   │   │
    │   │   ├── CQRS/
    │   │   │   ├── ICommand.cs
    │   │   │   ├── ICommandHandler.cs
    │   │   │   ├── IQuery.cs
    │   │   │   └── IQueryHandler.cs
    │   │   │
    │   │   ├── Common/
    │   │   │   ├── Behaviors/
    │   │   │   ├── Exceptions/
    │   │   │   └── Models/
    │   │   │
    │   │   ├── Orders/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateOrder/
    │   │   │   │   │   ├── CreateOrderCommand.cs
    │   │   │   │   │   ├── CreateOrderCommandHandler.cs
    │   │   │   │   │   └── CreateOrderCommandValidator.cs
    │   │   │   │   │
    │   │   │   │   └── CancelOrder/
    │   │   │   │       ├── CancelOrderCommand.cs
    │   │   │   │       └── CancelOrderCommandHandler.cs
    │   │   │   │
    │   │   │   └── Queries/
    │   │   │       ├── GetOrder/
    │   │   │       │   ├── GetOrderQuery.cs
    │   │   │       │   ├── GetOrderQueryHandler.cs
    │   │   │       │   └── OrderResponse.cs
    │   │   │       │
    │   │   │       └── GetOrders/
    │   │   │           ├── GetOrdersQuery.cs
    │   │   │           ├── GetOrdersQueryHandler.cs
    │   │   │           └── OrderListItemResponse.cs
    │   │   │
    │   │   ├── Customers/
    │   │   │   ├── Commands/
    │   │   │   └── Queries/
    │   │   │
    │   │   ├── Products/
    │   │   │   ├── Commands/
    │   │   │   └── Queries/
    │   │   │
    │   │   ├── Tenants/
    │   │   │   ├── Commands/
    │   │   │   └── Queries/
    │   │   │
    │   │   └── DependencyInjection.cs
    │   │
    │   ├── IDelivery.Infrastructure/
    │   │   ├── Persistence/
    │   │   │   ├── Context/
    │   │   │   │   └── ApplicationDbContext.cs
    │   │   │   │
    │   │   │   ├── Configurations/
    │   │   │   ├── Repositories/
    │   │   │   ├── Migrations/
    │   │   │   └── UnitOfWork.cs
    │   │   │
    │   │   ├── Events/
    │   │   │   └── DomainEventDispatcher.cs
    │   │   │
    │   │   ├── Authentication/
    │   │   ├── Email/
    │   │   ├── ExternalServices/
    │   │   └── DependencyInjection.cs
    │   │
    │   └── IDelivery.API/
    │       ├── Controllers/
    │       ├── Middleware/
    │       ├── Extensions/
    │       ├── Configuration/
    │       ├── Program.cs
    │       └── appsettings.json
    │
    └── tests/
        ├── IDelivery.Domain.Tests/
        ├── IDelivery.Application.Tests/
        ├── IDelivery.Infrastructure.Tests/
        └── IDelivery.API.Tests/
```

## Frontend

Tecnologias:

- Angular 20
- Standalone Components
- TypeScript
- Tailwind CSS
- Font Awesome
- PWA
- Mobile First
- Component-Based Architecture

---

# PLANO 1 — BACKEND + INFRAESTRUTURA

## FASE 1 — Estrutura inicial e Docker

### Etapa 1 — Monorepo

Criar a estrutura inicial:

```text
Project/
├── apps/
│   ├── client/
│   └── server/
├── docs/
├── docker/
│   ├── client/
│   └── server/
├── .gitignore
├── README.md
└── docker-compose.yml
```

Nesta etapa ainda não implementar regras de negócio.

### Objetivos

- Um único repositório GitHub.
- Separação clara entre frontend e backend.
- Estrutura preparada para desenvolvimento futuro.
- Documentação inicial.

### Commit sugerido

```text
chore: cria estrutura inicial do monorepo
```

---

## Etapa 2 — Bootstrap do backend

Criar a solução .NET dentro de:

```text
apps/server/
```

Preparar:

```text
src/
├── Api/
├── Application/
├── Domain/
└── Infrastructure/

tests/
├── UnitTests/
└── IntegrationTests/
```

Configurar:

- Solution.
- Projetos.
- Referências entre projetos.
- Nullable.
- Implicit usings.
- Configuração inicial.
- Health check básico.

### Regra arquitetural

A dependência deve apontar para dentro:

```text
Api
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application / Domain
```

O Domain não deve depender de Infrastructure ou Api.

### Commit

```text
feat: configura estrutura inicial do backend
```

---

## Etapa 3 — Docker do backend

Criar Dockerfile para o backend.

Preparar ambiente para:

- API.
- PostgreSQL.
- Variáveis de ambiente.
- Health checks.

Validar:

```bash
docker compose build
docker compose up
```

### Commit

```text
build: adiciona dockerização inicial do backend
```

---

# FASE 2 — Fundação do domínio

## Etapa 4 — Shared Kernel mínimo

Criar somente abstrações realmente compartilhadas.

Exemplos:

- Entity base.
- AggregateRoot.
- ValueObject base, se necessário.
- DomainEvent.
- Result/Errors, se adotado.
- Interfaces fundamentais.
- Abstrações próprias de CQRS: `ICommand`, `IQuery`, `ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>` (implementação própria, sem MediatR).
- Mecanismo próprio de dispatch de Commands/Queries e de Domain Events (sem depender de bibliotecas de Mediator).

Não criar dezenas de abstrações antecipadamente.

### Commit

```text
feat: adiciona fundamentos do dominio
```

---

## Etapa 5 — Tenant

Criar o primeiro agregado de negócio relacionado ao multi-tenancy.

O Tenant deverá representar uma empresa/restaurante/estabelecimento que utiliza o SaaS.

Preparar:

- Tenant.
- Identidade do tenant.
- Status ativo/bloqueado.
- Dados essenciais.

Regras de negócio devem ficar no domínio.

### Commit

```text
feat: adiciona agregado de tenant
```

---

## Etapa 6 — Persistência do Tenant

Implementar:

- DbContext.
- Configuração EF Core.
- PostgreSQL.
- Fluent API.
- Migration.
- Repository quando realmente necessário.

### Commit

```text
feat: adiciona persistencia de tenant
```

---

# FASE 3 — Identidade e autorização

## Etapa 7 — Roles

Criar os papéis fundamentais.

Exemplo:

```text
SUPER_ADMIN
TENANT_ADMIN
DELIVERY
CUSTOMER
```

A modelagem deve permitir evolução futura.

### Commit

```text
feat: adiciona papeis e autorizacao
```

---

## Etapa 8 — Usuário

Criar agregado/modelo de usuário conforme a decisão arquitetural.

Responsabilidades:

- Identidade.
- Credenciais.
- Role.
- TenantId quando aplicável.
- Status.
- Relacionamento adequado com Tenant.

Não colocar lógica de autenticação dentro da entidade.

### Commit

```text
feat: adiciona gerenciamento de usuarios
```

---

## Etapa 9 — Cadastro e login

Implementar CQRS (puro, sem MediatR) para:

```text
Register
Login
Refresh Token
```

Preparar JWT.

Regras:

- Senhas armazenadas com hash seguro.
- Access token.
- Refresh token.
- Expiração.
- Revogação quando aplicável.

### Ativação de conta por e-mail (via Domain Events)

O cadastro de usuário deve prever ativação de conta por e-mail, disparada via **Domain Events**, quando aplicável:

- Ao registrar um usuário (`Register`), a entidade deve nascer com status **pendente de ativação** e levantar um Domain Event (ex.: `UserRegisteredDomainEvent`), sem acoplar a lógica de envio de e-mail dentro do domínio.
- Um handler de aplicação/infraestrutura deve reagir a esse Domain Event para gerar um token/código de ativação e enviar o e-mail de ativação, através de uma abstração própria (ex.: `IEmailSender`), sem lógica de envio de e-mail no domínio.
- Implementar Command `ActivateAccount` (ou equivalente) para validar o token e alterar o status da conta para **ativa**, levantando um novo Domain Event (ex.: `UserActivatedDomainEvent`) quando aplicável.
- Login deve respeitar o status de ativação da conta, bloqueando o acesso de contas ainda não ativadas, conforme regra de negócio definida.
- Esse mesmo mecanismo de Domain Event + e-mail de ativação deve ser reutilizado, quando necessário, na criação de contas de **gestores da aplicação** (ex.: usuários com role `TENANT_ADMIN` ou `SUPER_ADMIN` criados por outro usuário), evitando duplicar a lógica de envio de e-mail em cada fluxo de criação de usuário.
- A abstração de envio de e-mail deve ficar isolada na Infrastructure, preparada para evoluir futuramente (ex.: outros provedores de e-mail), sem alterar o domínio.

### Commit

```text
feat: implementa autenticacao com jwt e ativacao de conta por email
```

---

## Etapa 10 — Autorização multi-tenant

Garantir isolamento entre tenants.

Regra fundamental:

> Um usuário pertencente a um tenant não pode acessar dados pertencentes a outro tenant sem autorização explícita.

Preparar:

- Tenant context.
- Policies.
- Authorization handlers, quando necessários.
- Filtros/guards apropriados.
- Validação de ownership.

Não confiar somente no `TenantId` enviado pelo cliente.

### Commit

```text
feat: implementa isolamento multi-tenant
```

---

# FASE 4 — Catálogo

## Etapa 11 — Categorias

Implementar:

- Categoria.
- Criação.
- Edição.
- Ativação/desativação.
- Exclusão conforme regra de domínio.

Usar Commands.

### Commit

```text
feat: implementa gerenciamento de categorias
```

---

## Etapa 12 — Produtos

Implementar:

- Produto.
- Nome.
- Descrição.
- Preço.
- Imagem.
- Categoria.
- Status.
- Tenant.

Separar Commands e Queries.

### Commit

```text
feat: implementa gerenciamento de produtos
```

---

## Etapa 13 — Consulta de catálogo

Implementar Query para:

- Listar produtos.
- Buscar por nome.
- Filtrar por categoria.
- Filtrar por status.

### Commit

```text
feat: implementa consulta do catalogo
```

---

## Etapa 14 — Paginação no backend

A paginação deve ser executada no banco de dados.

Nunca:

```text
buscar tudo → memória → paginar
```

Usar paginação adequada com:

- Page.
- PageSize.
- TotalCount.
- Items.

### Commit

```text
feat: adiciona paginacao ao catalogo
```

---

# FASE 5 — Carrinho

## Etapa 15 — Carrinho

Implementar agregado de carrinho.

Deve permitir:

- Adicionar produto.
- Alterar quantidade.
- Remover produto.
- Limpar carrinho.
- Consultar carrinho.

Regras de domínio devem validar:

- Quantidade.
- Produto ativo.
- Tenant correto.
- Preço válido.

### Commit

```text
feat: implementa carrinho de compras
```

---

## Etapa 16 — Snapshot dos itens

Preparar o pedido para não depender do preço atual do produto.

Quando o pedido for criado, guardar informações necessárias do item:

- Produto.
- Nome.
- Preço.
- Quantidade.
- Subtotal.

Isso evita que alteração futura do catálogo altere pedidos antigos.

### Commit

```text
feat: adiciona snapshot dos itens do pedido
```

---

# FASE 6 — Checkout e pedidos

## Etapa 17 — Customer

Implementar cliente final.

O cliente deverá poder:

- Criar conta.
- Fazer login.
- Manter seus dados.
- Possuir endereços.
- Realizar pedidos.

### Commit

```text
feat: implementa gerenciamento de clientes
```

---

## Etapa 18 — Endereço de entrega

Implementar endereço do cliente.

Preparar Value Object quando fizer sentido.

Exemplo:

```text
Street
Number
Complement
Neighborhood
City
State
ZipCode
Reference
```

### Commit

```text
feat: adiciona endereco de entrega
```

---

## Etapa 19 — Delivery settings

Criar configuração de entrega do tenant.

V1:

- Entrega grátis.
- Entrega grátis acima de determinado valor.
- Taxa fixa.
- Taxa por distância.

A regra deve ficar preparada para evolução.

### Commit

```text
feat: implementa configuracao de entrega
```

---

## Etapa 20 — Pedido

Criar agregado Order.

Preparar estados como:

```text
PENDING
CONFIRMED
PREPARING
READY_FOR_DELIVERY
OUT_FOR_DELIVERY
DELIVERED
CANCELLED
```

O domínio deve controlar as transições permitidas.

### Commit

```text
feat: adiciona agregado de pedido
```

---

## Etapa 21 — Criar pedido pelo checkout

Implementar o fluxo:

```text
Carrinho
 ↓
Checkout
 ↓
Validação
 ↓
Cálculo
 ↓
Criação do pedido
 ↓
Limpeza do carrinho
```

### Commit

```text
feat: implementa checkout e criacao de pedidos
```

---

# FASE 7 — Pagamento

## Etapa 22 — Pagamento V1

Inicialmente suportar:

```text
CASH
CARD_ON_DELIVERY
```

Criar abstração preparada para futuros meios.

Não acoplar o domínio a um provedor específico.

### Commit

```text
feat: implementa pagamentos na entrega
```

---

# FASE 8 — Gestão de pedidos

## Etapa 23 — Gestão pelo tenant

Tenant poderá:

- Listar pedidos.
- Visualizar pedido.
- Confirmar.
- Iniciar preparo.
- Marcar como pronto.
- Cancelar quando permitido.

### Commit

```text
feat: implementa gestao de pedidos pelo tenant
```

---

## Etapa 24 — Delivery

Implementar entidade/agregado e regras para entregadores.

Permitir:

- Cadastro.
- Ativação/desativação.
- Associação ao tenant.
- Visualização dos pedidos atribuídos.

### Commit

```text
feat: implementa gerenciamento de entregadores
```

---

## Etapa 25 — Fluxo do entregador

Permitir ao entregador:

- Visualizar entrega.
- Aceitar/assumir entrega, conforme regra.
- Marcar saída para entrega.
- Confirmar entrega.
- Reportar cancelamento quando permitido.

### Commit

```text
feat: implementa fluxo de entrega
```

---

## Etapa 25.1 — Delivery: rastreamento, geolocalização, rotas, distância e cálculo automático de taxa

Implementar no domínio e na aplicação de Delivery:

- Rastreamento.
- Geolocalização.
- Rotas.
- Distância.
- Cálculo automático de taxa.

Regras:

- Registrar/atualizar a localização do entregador durante a entrega (rastreamento e geolocalização).
- Calcular a rota entre o tenant e o endereço de entrega.
- Calcular a distância do trajeto a partir da rota.
- Calcular automaticamente a taxa de entrega com base na distância (ou outro critério definido), sem depender de valor fixo informado manualmente.
- Manter a lógica de cálculo isolada em serviço de domínio/aplicação próprio, sem acoplar Controllers a provedores externos de mapas/geolocalização.
- Preparar abstração (interface) para o provedor de mapas/geolocalização, permitindo trocar o provedor futuramente sem alterar o domínio.

### Commit

```text
feat: adiciona rastreamento, geolocalizacao, rotas, distancia e calculo automatico de taxa de entrega
```

---

# FASE 9 — Super Admin / SaaS

## Etapa 26 — Gestão de tenants

SUPER_ADMIN poderá:

- Criar tenant.
- Editar tenant.
- Bloquear.
- Desbloquear.
- Visualizar.
- Gerenciar status.

Ao criar um tenant que já implique a criação de um usuário gestor (`TENANT_ADMIN`), reutilizar o mecanismo de Domain Event + e-mail de ativação definido na Etapa 9, sem duplicar a lógica de envio de e-mail.

### Commit

```text
feat: implementa gestao de tenants
```

---

## Etapa 27 — Planos SaaS

Criar conceitos de:

```text
Plan
Subscription
```

Planos iniciais:

```text
TRIAL
MONTHLY
QUARTERLY
```

Trial:

```text
7 dias grátis
```

### Commit

```text
feat: adiciona planos e assinaturas
```

---

## Etapa 28 — Ciclo da assinatura

Implementar estados:

```text
TRIAL
ACTIVE
PAST_DUE
CANCELLED
EXPIRED
```

Preparar o domínio para futuros gateways de pagamento.

### Commit

```text
feat: implementa ciclo de assinatura
```

---

## Etapa 29 — Área pública do SaaS

Backend deverá fornecer dados necessários para:

- Landing page.
- Planos.
- Benefícios.
- Trial.
- Cadastro de tenant.

### Commit

```text
feat: adiciona endpoints publicos do saas
```

---

# FASE 10 — Notificações

## Etapa 30 — Domain Events

Implementar eventos de domínio relevantes.

Exemplos:

```text
OrderCreated
OrderConfirmed
OrderReady
OrderOutForDelivery
OrderDelivered
OrderCancelled
```

### Commit

```text
feat: implementa eventos de dominio
```

---

## Etapa 31 — Notification abstraction

Criar abstração:

```text
INotificationService
```

O domínio não deve conhecer:

- Firebase.
- Web Push.
- WhatsApp.
- E-mail.

Preparar arquitetura para múltiplos canais.

### Commit

```text
feat: adiciona abstracao de notificacoes
```

---

## Etapa 32 — API de notificações

Implementar persistência e consulta de notificações.

Permitir:

- Criar.
- Listar.
- Marcar como lida.
- Contar não lidas.

### Commit

```text
feat: implementa notificacoes internas
```

---

# FASE 11 — API final e qualidade

## Etapa 33 — OpenAPI

Documentar API.

Organizar:

- Endpoints.
- Responses.
- Errors.
- Authentication.
- Pagination.

### Commit

```text
docs: documenta api
```

---

## Etapa 34 — Testes

Criar testes para os principais fluxos:

- Tenant.
- Autenticação.
- Multi-tenancy.
- Produtos.
- Carrinho.
- Checkout.
- Pedido.
- Pagamento.
- Entregador.
- Assinatura.

### Commit

```text
test: amplia cobertura dos fluxos principais
```

---

## Etapa 35 — Revisão arquitetural

Verificar:

- SOLID.
- DDD.
- CQRS.
- Clean Architecture.
- Isolamento multi-tenant.
- Acoplamento.
- Responsabilidades.
- Testabilidade.

Corrigir somente problemas encontrados.

### Commit

```text
refactor: revisa arquitetura do backend
```

---

# CRITÉRIO PARA ENCERRAR O PLANO 1

O Plano 1 estará concluído quando:

- API estiver funcional.
- Banco estiver funcionando.
- Multi-tenancy estiver isolado.
- Autenticação estiver funcionando.
- Catálogo estiver funcionando.
- Carrinho estiver funcionando.
- Checkout estiver funcionando.
- Pedidos estiverem funcionando.
- Pagamentos V1 estiverem funcionando.
- Gestão de tenants estiver funcionando.
- Gestão de entregadores estiver funcionando.
- Assinaturas estiverem modeladas.
- Notificações internas estiverem funcionando.
- Eventos de domínio estiverem implementados.
- API estiver documentada.
- Testes principais estiverem funcionando.
- Docker estiver funcionando.

**Somente depois disso iniciar o Plano 2.**

---

# PLANO 2 — FRONTEND ANGULAR 20

## Regra

O frontend só começa quando o Plano 1 estiver concluído.

O Angular deverá consumir exclusivamente os contratos disponibilizados pela API.

Não duplicar regras de negócio do backend no frontend.

---

# FASE 1 — Estrutura Angular

## Etapa F1 — Bootstrap

Criar Angular 20 Standalone em:

```text
apps/client/
```

Configurar:

- TypeScript.
- Standalone.
- Tailwind CSS.
- Font Awesome.
- Angular Router.
- HttpClient.
- Environments.
- Estrutura por features.

### Arquitetura

```text
apps/client/src/app/
├── core/
├── shared/
├── layout/
├── features/
│   ├── auth/
│   ├── catalog/
│   ├── cart/
│   ├── checkout/
│   ├── orders/
│   ├── customer/
│   ├── delivery/
│   ├── tenant/
│   ├── admin/
│   ├── subscription/
│   └── notifications/
└── app.routes.ts
```

### Commit

```text
feat: configura estrutura inicial do frontend
```

---

# FASE 2 — Design System

## Etapa F2 — Base visual

Criar componentes reutilizáveis:

- Button.
- Input.
- Select.
- Modal.
- Drawer.
- Card.
- Badge.
- Table.
- Pagination.
- Empty State.
- Loading.
- Toast.
- Confirm Dialog.

Mobile First.

Visual moderno e consistente.

### Commit

```text
feat: cria componentes base da interface
```

---

# FASE 3 — Autenticação

## Etapa F3 — Auth

Criar telas:

- Login.
- Cadastro.
- Recuperação de senha, quando disponível na API.
- Seleção/fluxo conforme papel.

Criar:

- AuthService.
- Token handling.
- Guards.
- Interceptors.
- User state.

### Commit

```text
feat: implementa autenticacao no frontend
```

---

# FASE 4 — Área pública

## Etapa F4 — Landing Page

Criar:

- Hero.
- Benefícios.
- Funcionalidades.
- Planos.
- CTA.
- Trial de 7 dias.
- FAQ.
- Footer.

### Commit

```text
feat: cria landing page do saas
```

---

## Etapa F5 — Planos

Criar página de planos:

```text
7 dias grátis
Mensal
Trimestral
```

Integrar com API.

### Commit

```text
feat: adiciona pagina de planos
```

---

# FASE 5 — Catálogo público

## Etapa F6 — Catálogo

Criar:

- Categorias.
- Produtos.
- Busca.
- Filtros.
- Paginação.
- Detalhes do produto.

Mobile First.

### Commit

```text
feat: implementa catalogo publico
```

---

## Etapa F7 — Produto

Tela/modal/drawer de produto:

- Imagem.
- Nome.
- Descrição.
- Preço.
- Opções.
- Quantidade.
- Adicionar ao carrinho.

### Commit

```text
feat: implementa detalhes do produto
```

---

# FASE 6 — Carrinho e checkout

## Etapa F8 — Carrinho

Criar:

- Itens.
- Quantidades.
- Subtotais.
- Total.
- Remoção.
- CTA para checkout.

### Commit

```text
feat: implementa carrinho no frontend
```

---

## Etapa F9 — Checkout

Criar fluxo:

```text
Carrinho
 ↓
Identificação
 ↓
Endereço
 ↓
Entrega
 ↓
Pagamento
 ↓
Resumo
 ↓
Pedido
```

Pagamento V1:

```text
Dinheiro
Cartão na entrega
```

### Commit

```text
feat: implementa checkout
```

---

# FASE 7 — Cliente

## Etapa F10 — Área do cliente

Criar:

- Perfil.
- Endereços.
- Histórico de pedidos.
- Detalhes do pedido.

### Commit

```text
feat: cria area do cliente
```

---

# FASE 8 — Gestão do tenant

## Etapa F11 — Dashboard

Criar dashboard do tenant:

- Pedidos.
- Faturamento.
- Produtos.
- Categorias.
- Entregas.
- Indicadores.

### Commit

```text
feat: cria dashboard do tenant
```

---

## Etapa F12 — Produtos

Criar CRUD visual:

- Listagem.
- Criação.
- Edição.
- Ativação/desativação.
- Exclusão.

### Commit

```text
feat: implementa gestao de produtos no tenant
```

---

## Etapa F13 — Categorias

Criar CRUD de categorias.

### Commit

```text
feat: implementa gestao de categorias no tenant
```

---

# FASE 9 — Pedidos

## Etapa F14 — Gestão de pedidos

Criar interface visual para:

- Novos pedidos.
- Confirmar.
- Preparando.
- Pronto.
- Em entrega.
- Entregue.
- Cancelado.

### Commit

```text
feat: implementa painel de pedidos do tenant
```

---

# FASE 10 — Delivery

## Etapa F15 — Dashboard do entregador

Criar:

- Pedidos disponíveis.
- Pedidos atribuídos.
- Detalhes.
- Status.
- Confirmar entrega.
- Cancelamento quando permitido.

### Commit

```text
feat: cria painel do entregador
```

---

## Etapa F15.1 — Delivery: rastreamento, geolocalização, rotas, distância e taxa automática

Criar no frontend:

- Rastreamento.
- Geolocalização.
- Rotas.
- Distância.
- Cálculo automático de taxa.

Detalhes:

- Exibir a localização do entregador em tempo real (rastreamento/geolocalização) para o cliente e para o tenant acompanharem a entrega.
- Exibir a rota da entrega no mapa.
- Exibir a distância calculada.
- Exibir a taxa de entrega calculada automaticamente já no carrinho/checkout, antes da confirmação do pedido.

### Commit

```text
feat: exibe rastreamento, geolocalizacao, rotas, distancia e taxa automatica de entrega
```

---

# FASE 11 — Super Admin

## Etapa F16 — Gestão de tenants

Criar:

- Dashboard.
- Lista de tenants.
- Criar.
- Editar.
- Bloquear.
- Desbloquear.
- Detalhes.

### Commit

```text
feat: cria painel de gestao de tenants
```

---

## Etapa F17 — Assinaturas

Criar:

- Planos.
- Assinatura atual.
- Trial.
- Status.
- Histórico.

### Commit

```text
feat: cria gerenciamento de assinaturas
```

---

# FASE 12 — Notificações em tempo real

## Etapa F18 — Infraestrutura realtime

Implementar comunicação em tempo real, preferencialmente utilizando uma tecnologia adequada ao backend .NET, como SignalR.

Eventos:

```text
Novo pedido
Pedido confirmado
Pedido em preparo
Pedido pronto
Pedido saiu para entrega
Pedido entregue
Pedido cancelado
```

### Commit

```text
feat: adiciona comunicacao em tempo real
```

---

## Etapa F19 — Central de notificações

Criar:

- Notification center.
- Badge de não lidas.
- Histórico.
- Marcar como lida.
- Toast para eventos importantes.

### Commit

```text
feat: implementa central de notificacoes
```

---

# FASE 13 — Push Notifications

## Etapa F20 — Push

Preparar Web Push/PWA para:

- Novo pedido.
- Alteração de status.
- Ofertas.
- Eventos importantes.

A arquitetura deve permitir posteriormente:

```text
Web Push
WhatsApp
E-mail
```

sem alterar o domínio.

### Commit

```text
feat: adiciona push notifications
```

---

# FASE 14 — PWA

## Etapa F21 — PWA

Configurar:

- Service Worker.
- Manifest.
- Ícones.
- Cache.
- Estratégias adequadas de atualização.
- Instalação como aplicativo.

### Commit

```text
feat: transforma frontend em pwa
```

---

# FASE 15 — UX e responsividade

## Etapa F22 — Mobile First

Revisar todas as telas:

- Cliente.
- Tenant.
- Delivery.
- Super Admin.
- Landing page.

Garantir funcionamento em:

- Smartphone.
- Tablet.
- Desktop.

### Commit

```text
refactor: aprimora responsividade e ux
```

---

# FASE 16 — Qualidade

## Etapa F23 — Testes frontend

Implementar testes para:

- Services.
- Guards.
- Interceptors.
- Components críticos.
- Fluxo de checkout.
- Carrinho.
- Autenticação.

### Commit

```text
test: adiciona testes dos principais fluxos do frontend
```

---

## Etapa F24 — Build e Docker

Dockerizar Angular para produção.

Preparar:

```text
Angular build
 ↓
Nginx
 ↓
SPA
```

Configurar integração com o backend.

### Commit

```text
build: adiciona dockerizacao do frontend
```

---

# FASE 17 — Finalização

## Etapa F25 — Revisão completa

Verificar:

- UX.
- Responsividade.
- Acessibilidade.
- Performance.
- PWA.
- SEO da área pública.
- Lazy loading.
- Guards.
- Error handling.
- Loading states.
- Empty states.
- Notificações.
- Segurança no frontend.

### Commit

```text
refactor: finaliza revisao do frontend
```

---

# CRITÉRIO FINAL DO PROJETO

O projeto será considerado V1 concluído quando existir:

```text
                    ┌──────────────────┐
                    │    SaaS Público  │
                    └────────┬─────────┘
                             │
                      Cadastro / Trial
                             │
                    ┌────────▼─────────┐
                    │      Tenant      │
                    └────────┬─────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
        ▼                    ▼                    ▼
    Produtos             Pedidos             Delivery
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
                         Cliente
                             │
                         Carrinho
                             │
                         Checkout
                             │
                         Pagamento
                             │
                         Entrega
                             │
                       Notificação
```

## Fluxo principal do cliente

```text
Entrar no catálogo
        ↓
Visualizar categorias
        ↓
Buscar produto
        ↓
Visualizar produto
        ↓
Adicionar ao carrinho
        ↓
Finalizar pedido
        ↓
Login/Cadastro
        ↓
Selecionar endereço
        ↓
Escolher entrega
        ↓
Escolher pagamento
        ↓
Confirmar pedido
        ↓
Acompanhar status
        ↓
Receber notificações
        ↓
Pedido entregue
```

## Fluxo principal do Tenant

```text
Login
 ↓
Dashboard
 ↓
Gerenciar categorias
 ↓
Gerenciar produtos
 ↓
Receber pedido
 ↓
Confirmar
 ↓
Preparar
 ↓
Marcar como pronto
 ↓
Atribuir/encaminhar para delivery
 ↓
Acompanhar entrega
```

## Fluxo principal do Delivery

```text
Login
 ↓
Visualizar entregas
 ↓
Selecionar/receber entrega
 ↓
Sair para entrega
 ↓
Realizar entrega
 ↓
Confirmar entrega
```

## Fluxo principal do Super Admin

```text
Login
 ↓
Dashboard SaaS
 ↓
Gerenciar tenants
 ↓
Gerenciar planos
 ↓
Gerenciar assinaturas
 ↓
Acompanhar utilização
 ↓
Bloquear/desbloquear tenant
```

---

# PRINCÍPIOS ARQUITETURAIS OBRIGATÓRIOS

## Backend

O backend deve seguir:

- Clean Architecture.
- DDD.
- CQRS **puro** (sem uso de bibliotecas de Mediator, como MediatR).
- SOLID.
- Separation of Concerns.
- Dependency Inversion.
- Domain-Driven Design.
- Domain Events.
- Aggregates.
- Entities.
- Value Objects.
- Repositories quando necessários.
- Application Services/Handlers.
- Commands.
- Queries.

### Regra — CQRS puro (sem MediatR)

O uso de bibliotecas de Mediator (como MediatR) **não é permitido** neste projeto.

CQRS deve ser implementado de forma explícita e própria:

- Commands e Queries devem ser classes/records simples, sem depender de uma biblioteca de Mediator.
- Cada Command/Query deve ter seu Handler correspondente, com interface própria do projeto (ex.: `ICommandHandler<TCommand, TResult>`, `IQueryHandler<TQuery, TResult>`).
- O despacho (dispatch) de Commands/Queries deve ser feito por um mecanismo próprio (ex.: um pequeno dispatcher resolvido via injeção de dependência), implementado dentro da própria solução, sem depender de pacotes de terceiros que implementem o padrão Mediator.
- Controllers devem depender apenas das abstrações de Command/Query Handler (ou do dispatcher próprio), nunca da lógica de negócio diretamente.
- Domain Events podem ser despachados por um mecanismo próprio equivalente, também sem depender de bibliotecas de Mediator.

### Regra

Controllers devem ser finos.

A lógica de negócio não deve ficar em:

```text
Controller
DbContext
Repository
```

A regra deve estar no domínio ou nos casos de uso apropriados.

---

# MULTI-TENANCY

Todas as funcionalidades que pertencem a um tenant devem respeitar o contexto do tenant.

Nunca confiar cegamente em:

```text
TenantId
```

enviado pelo frontend.

O backend deve determinar o tenant através da identidade/autorização sempre que aplicável.

O isolamento deve ser validado também nos casos de:

- Produtos.
- Categorias.
- Carrinho.
- Pedidos.
- Delivery.
- Clientes.
- Configurações.
- Assinaturas.

---

# EVOLUÇÃO FUTURA

A arquitetura deve deixar espaço para:

## Pagamentos

V1:

```text
Dinheiro
Cartão na entrega
```

Futuro:

```text
PIX
Cartão online
Mercado Pago
Stripe
Outros gateways
```

## Notificações

V1:

```text
Notificações internas
Realtime
Push
```

Futuro:

```text
E-mail
WhatsApp
SMS
```

## Delivery

V1 (implementado nas Etapas 25.1 e F15.1):

```text
Rastreamento
Geolocalização
Rotas
Distância
Cálculo automático de taxa
```

Futuro:

- Integrações externas (provedores de mapas de terceiros, otimização de rotas com múltiplas entregas, etc.).

---

# REGRA FINAL PARA O AGENTE

Ao concluir cada etapa, responder:

```text
Etapa concluída: <nome>

Implementado:
- ...
- ...
- ...

Testes:
- Build: OK
- Testes: OK

Commit sugerido:

<commit>

A etapa foi concluída.

Deseja realizar o commit? (Y) ou (N)
Deseja verificar o que foi implementado antes de continuar? (Y) ou (N)
Deseja apontar algum ajuste, ponto específico ou bug no código atual antes de prosseguir? (Y) ou (N)

AGUARDANDO RESPOSTA DO USUÁRIO.
```

**Nunca continuar automaticamente para a próxima etapa após sugerir o commit.**

O usuário deve responder à pergunta acima (confirmando o commit, pedindo verificação, ou apontando ajustes/bugs) antes da próxima etapa.

---

# ORDEM DE EXECUÇÃO

```text
PLANO 1
│
├── Infraestrutura
├── Arquitetura
├── Domínio
├── Multi-tenancy
├── Autenticação
├── Catálogo
├── Carrinho
├── Checkout
├── Pedidos
├── Pagamentos
├── Delivery
├── SaaS
├── Assinaturas
├── Notificações
├── Testes
└── Backend concluído
        │
        ▼
PLANO 2
│
├── Angular
├── Design System
├── Auth
├── Landing Page
├── Catálogo
├── Carrinho
├── Checkout
├── Cliente
├── Tenant
├── Pedidos
├── Delivery
├── Super Admin
├── Realtime
├── Push
├── PWA
├── Testes
└── Frontend concluído
```

**FIM DO PLANO DE IMPLEMENTAÇÃO**
