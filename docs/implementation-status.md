# Implementation Status

## Última análise
Data: 2026-09-01 (atualizado)

## Backend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|
| B01 — Monorepo e infraestrutura | CONCLUÍDA | apps/client, apps/server, docs, docker, docker-compose.yml, AGENTS.md, README.md | — |
| B02 — Fundação .NET | CONCLUÍDA | IDelivery.slnx com 5 projetos (Api, Application, Domain, Infrastructure, SharedKernel) + 2 projetos de teste | — |
| B03 — Shared Kernel | CONCLUÍDA | Entity, AggregateRoot, ValueObject, DomainEvent, Result/Error, CQRS próprio (ICommand, IQuery, handlers, dispatchers) | — |
| B04 — Tenant | CONCLUÍDA | Domain: Tenant aggregate, TenantStatus, Address VO, 4 domain events. Application: Create/Activate/Block/Delete/Update commands + validators + handlers, GetTenant/GetTenants queries. Infrastructure: TenantConfiguration, TenantRepository, Migration InitialCreate. Tests: 7 unit + 5 integration (todos passando). | — |
| B05 — Roles e Users | CONCLUÍDA | Domain: User aggregate, Role enum (SuperAdmin, TenantAdmin, Delivery, Customer), UserStatus, 8 domain events, RoleExtensions. Application: IUserRepository interface. Infrastructure: UserRepository, UserConfiguration, Migration AddUserEntity. Tests: 3 unit (UserTests). | Adicionar comandos/queries de User (CreateUser, GetUser, etc.) quando necessário |
| B06 — Autenticação | CONCLUÍDA | Abstractions: IJwtTokenService, IEmailService, ICurrentUser. Implementation: JwtTokenService (JWT access/refresh tokens), EmailService (SMTP/console), CurrentUserService. Commands: Register, Login, RefreshToken, ActivateAccount, ForgotPassword, ResetPassword + validators + handlers. Domain Event Handler: UserRegisteredDomainEventHandler → send activation email. Infrastructure: JWT middleware, AuthController endpoints, HttpContextAccessor. Tests: existing tests pass. | — |
| B07 — Multi-tenancy | CONCLUÍDA | ICurrentUser.TenantId, CurrentUserService extrai tenant_id do JWT, ITenantContext, TenantContext, JwtTokenService inclui tenant_id no token, User.TenantId | — |
| B08 — Catálogo | NÃO IMPLEMENTADA | — | — |
| B09 — Carrinho | NÃO IMPLEMENTADA | — | — |
| B10 — Customer e endereço | NÃO IMPLEMENTADA | Address VO existe em Tenant | — |
| B11 — Delivery settings | NÃO IMPLEMENTADA | — | — |
| B12 — Pedido e checkout | NÃO IMPLEMENTADA | — | — |
| B13 — Pagamento V1 | NÃO IMPLEMENTADA | — | — |
| B14 — Gestão de pedidos | NÃO IMPLEMENTADA | — | — |
| B15 — Delivery | NÃO IMPLEMENTADA | — | — |
| B16 — Rastreamento | NÃO IMPLEMENTADA | — | — |
| B17 — SaaS | NÃO IMPLEMENTADA | — | — |
| B18 — Notificações | CONCLUÍDA | IEmailService implementation, UserRegisteredDomainEventHandler sends activation email | — |
| B19 — API e qualidade | PARCIAL | GlobalExceptionHandler, ProblemDetails, HealthChecks, AuthController, JWT middleware | OpenAPI/Swagger, mais controllers, paginação, testes de API |

## Frontend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|
| F01 — Angular bootstrap | NÃO IMPLEMENTADA | apps/client vazio | — |
| F02-F13 | NÃO IMPLEMENTADA | — | — |

## Bloqueios
- B06 Autenticação → **RESOLVIDO**
- B06 bloqueia B07 Multi-tenancy (precisa de usuário autenticado para obter tenant context) → **RESOLVIDO**
- B05 Users (IUserRepository) → **RESOLVIDO**: UserRepository implementado
- B04 Tenant → **RESOLVIDO**: Todos os testes passando

## Problemas arquiteturais encontrados
1. ~~**Duplicate DI registration**: `CreateTenantCommandHandler` registrado duas vezes em `Application/DependencyInjection.cs`~~ → **CORRIGIDO**
2. ~~**CreateTenantCommandHandler** tenta acessar `.Value` em resultado falho~~ → **CORRIGIDO** (checa IsSuccess antes)
3. ~~**PhoneNumber regex** muito restritiva para fixos — testes usavam número inválido~~ → **CORRIGIDO** (testes usam números válidos)
4. ~~**Teste de integração `BlockAndActivate_Tenant_ShouldWork`** falha~~ → **CORRIGIDO** (testa Block → Activate)
5. ~~**Mocks em testes unitários** não configurados para `ExistsBySlugAsync`~~ → **CORRIGIDO** (mock configurado)
6. ~~**IUserRepository** não implementado em Infrastructure~~ → **CORRIGIDO** (UserRepository implementado)
7. **Authentication/Email/ExternalServices** pastas em Infrastructure existem mas vazias → **PARCIALMENTE RESOLVIDO** (Email e Security implementados)
8. **Sem controllers/endpoints** na API — apenas health check → **RESOLVIDO** (AuthController adicionado)
9. ~~**Dockerfile** não copia `IDelivery.SharedKernel.csproj`~~ → **CORRIGIDO** (COPY adicionado antes do restore)

## Testes
- Backend build: OK (com warnings de versão EF Core Relational 9.0.1 vs 9.0.19 no IntegrationTests)
- Backend tests: **19 passed, 0 failed** (14 unit + 5 integration)

## Próxima etapa recomendada
**B08 — Catálogo** - Implementar:
1. Product e Category aggregates
2. Commands e Queries para CRUD
3. Persistência com EF Core
4. Testes unitários e de integração

Dependências resolvidas: B04 Tenant, B05 Users, B06 Auth, B07 Multi-tenancy.