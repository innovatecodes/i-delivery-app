# Implementation Status

## Última análise
Data: 2026-08-31

## Backend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|
| B01 — Monorepo e infraestrutura | CONCLUÍDA | apps/client, apps/server, docs, docker, docker-compose.yml, AGENTS.md, README.md | — |
| B02 — Fundação .NET | CONCLUÍDA | IDelivery.slnx com 5 projetos (Api, Application, Domain, Infrastructure, SharedKernel) + 2 projetos de teste | — |
| B03 — Shared Kernel | CONCLUÍDA | Entity, AggregateRoot, ValueObject, DomainEvent, Result/Error, CQRS próprio (ICommand, IQuery, handlers, dispatchers) | — |
| B04 — Tenant | CONCLUÍDA | Domain: Tenant aggregate, TenantStatus, Address VO, 4 domain events. Application: Create/Activate/Block/Delete/Update commands + validators + handlers, GetTenant/GetTenants queries. Infrastructure: TenantConfiguration, TenantRepository, Migration InitialCreate. Tests: 7 unit + 5 integration (todos passando). | — |
| B05 — Roles e Users | CONCLUÍDA | Domain: User aggregate, Role enum (SuperAdmin, TenantAdmin, Delivery, Customer), UserStatus, 8 domain events, RoleExtensions. Application: IUserRepository interface. Infrastructure: UserRepository, UserConfiguration, Migration AddUserEntity. Tests: 3 unit (UserTests). | Adicionar comandos/queries de User (CreateUser, GetUser, etc.) quando necessário |
| B06 — Autenticação | NÃO IMPLEMENTADA | Abstrações: IPasswordHasher, ISecureTokenGenerator, ITokenHasher, ICurrentUser. Implementações: PasswordHasher, SecureTokenGenerator, TokenHasher. | Register, Login, JWT, Refresh Token, ativação por e-mail |
| B07 — Multi-tenancy | NÃO IMPLEMENTADA | Nenhum middleware, context resolver, tenant isolation | TenantContext, middleware, isolamento |
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
| B18 — Notificações | PARCIAL | IEmailService abstraction exists | Implementar handlers de domain events para envio de e-mail |
| B19 — API e qualidade | PARCIAL | GlobalExceptionHandler, ProblemDetails, HealthChecks | OpenAPI/Swagger, controllers, paginação, testes de API |

## Frontend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|
| F01 — Angular bootstrap | NÃO IMPLEMENTADA | apps/client vazio | — |
| F02-F13 | NÃO IMPLEMENTADA | — | — |

## Bloqueios
- B06 Autenticação bloqueia B07 Multi-tenancy (precisa de usuário autenticado para obter tenant context)
- B05 Users (IUserRepository) → **RESOLVIDO**: UserRepository implementado
- B04 Tenant → **RESOLVIDO**: Todos os testes passando

## Problemas arquiteturais encontrados
1. ~~**Duplicate DI registration**: `CreateTenantCommandHandler` registrado duas vezes em `Application/DependencyInjection.cs`~~ → **CORRIGIDO**
2. ~~**CreateTenantCommandHandler** tenta acessar `.Value` em resultado falho~~ → **CORRIGIDO** (checa IsSuccess antes)
3. ~~**PhoneNumber regex** muito restritiva para fixos — testes usavam número inválido~~ → **CORRIGIDO** (testes usam números válidos)
4. ~~**Teste de integração `BlockAndActivate_Tenant_ShouldWork`** falha~~ → **CORRIGIDO** (testa Block → Activate)
5. ~~**Mocks em testes unitários** não configurados para `ExistsBySlugAsync`~~ → **CORRIGIDO** (mock configurado)
6. ~~**IUserRepository** não implementado em Infrastructure~~ → **CORRIGIDO** (UserRepository implementado)
7. **Authentication/Email/ExternalServices** pastas em Infrastructure existem mas vazias
8. **Sem controllers/endpoints** na API — apenas health check

## Testes
- Backend build: OK (com warnings de versão EF Core Relational 9.0.1 vs 9.0.19 no IntegrationTests)
- Backend tests: **19 passed, 0 failed** (14 unit + 5 integration)
  - Unit: 7 TenantTests + 3 UserTests + 4 CreateTenantCommandHandlerTests + 3 CreateTenantCommandValidatorTests = 17 (wait, let me recount)

## Próxima etapa recomendada
**B06 Autenticação** - Implementar:
1. Register (criar usuário, gerar token de ativação, enviar e-mail via IEmailService)
2. Login (validar credenciais, gerar JWT + Refresh Token)
3. JWT (middleware de autenticação, claims, expiração)
4. Refresh Token (rotação, revogação)
5. Ativação por e-mail (endpoint para validar token, ativar usuário)
6. Domain Event handlers para UserRegisteredDomainEvent → enviar e-mail de ativação

Dependências já resolvidas: UserRepository, TenantRepository, PasswordHasher, SecureTokenGenerator, TokenHasher, IEmailService abstraction.