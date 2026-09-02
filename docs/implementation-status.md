# Implementation Status

## Última análise
Data: 2026-09-01 (atualizado B11, B12)

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
| B08 — Catálogo | CONCLUÍDA | Domain: Category e Product aggregates, Money VO, domain events. Application: ICategoryRepository, IProductRepository, Create/Update/Delete commands + handlers + validators, Get/GetAll queries + handlers. Infrastructure: CategoryConfiguration, ProductConfiguration, CategoryRepository, ProductRepository, ApplicationDbContext atualizado, Migration AddCatalogTables. Tests: 18 unitários (CategoryTests + ProductTests) | — |
| B09 — Carrinho | CONCLUÍDA | Domain: Cart aggregate (Cart + CartItem), 4 domain events (Created, ItemAdded, ItemRemoved, CartCleared). Application: ICartRepository, Add/Remove/UpdateQuantity/Clear commands + handlers + validators, GetCart query + handler. Infrastructure: CartConfiguration, CartItemConfiguration, CartRepository, ApplicationDbContext atualizado com Carts/CartItems DbSets, Migration AddCartTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B10 — Customer e endereço | CONCLUÍDA | Domain: Customer aggregate (Customer + CustomerAddress), 4 domain events (Created, Updated, AddressAdded, AddressRemoved). Application: ICustomerRepository, Create/Update/Delete/AddAddress/RemoveAddress/SetDefaultAddress commands + handlers + validators, GetCustomer query + handler. Infrastructure: CustomerConfiguration, CustomerAddressConfiguration, CustomerRepository, ApplicationDbContext atualizado com Customers/CustomerAddresses DbSets, Migration AddCustomerTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B11 — Delivery settings | CONCLUÍDA | Domain: DeliverySettings aggregate, DeliveryFeeType enum (Free, FreeAboveAmount, Fixed, PerDistance), 2 domain events (Created, Updated), CalculateFee method. Application: IDeliverySettingsRepository, Create/Update/Delete commands + handlers + validators, GetDeliverySettings query + handler. Infrastructure: DeliverySettingsConfiguration, DeliverySettingsRepository, ApplicationDbContext atualizado com DeliverySettings DbSet, Migration AddDeliverySettingsTable. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B12 — Pedido e checkout | CONCLUÍDA | Domain: Order aggregate (Order + OrderItem snapshot), OrderState enum (Pending→Confirmed→Preparing→ReadyForDelivery→OutForDelivery→Delivered|DeliveryFailed|Cancelled), DeliveryFailureReason enum, 5 domain events (Created, StatusChanged, Delivered, DeliveryFailed, Cancelled). Transições controladas com autoridade: Deliver/FailDelivery só pelo entregador atribuído; Cancel até ReadyForDelivery por Tenant/Cliente/Sistema. Application: IOrderRepository, Create/Confirm/StartPreparing/MarkReady/StartDelivery/Deliver/FailDelivery/Cancel commands + handlers + validators, GetOrder/GetOrders queries + handlers. Infrastructure: OrderConfiguration, OrderItemConfiguration, OrderRepository, ApplicationDbContext atualizado com Orders/OrderItems DbSets, Migration AddOrderTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
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
- Backend tests: **154 unit + 29 integration** (todos passando)

## Análise de Tratamento de Erros (Refatoração)

### Arquitetura atual (Mapping)
| Componente | Localização | Status |
|---|---|---|
| Result Pattern | `SharedKernel/Common/Result/Result.cs`, `Error.cs` | Implementado, funcional |
| BaseException | `SharedKernel/Common/Exceptions/BaseException.cs` | Preservado |
| DomainException | `Domain/Common/Exceptions/DomainExceptions.cs` | Preservado, used by VOs |
| ApplicationException | `Application/Common/Exceptions/ApplicationException.cs` | Preservado, abstract |
| BadRequestException | `Application/Common/Exceptions/BadRequestException.cs` | Preservado |
| ValidationException | `Application/Common/Exceptions/ValidationException.cs` | Preservado |
| NotFoundException | `Application/Common/Exceptions/NotFoundException.cs` | Preservado |
| UnauthorizedException | `Application/Common/Exceptions/UnauthorizedException.cs` | Preservado |
| ForbiddenException | `Application/Common/Exceptions/ForbiddenException.cs` | Preservado |
| ConflictException | `Application/Common/Exceptions/ConflictException.cs` | Preservado |
| GlobalExceptionHandler | `Api/Http/GlobalExceptionHandler.cs` | Implementado (IExceptionHandler) |
| Error codes | Inline `new Error(...)` em 287+ locations | ⚠️ Não centralizados |

### Gaps encontrados

1. **Console.WriteLine em EmailService** — 3 ocorrências em `Infrastructure/Messaging/Email/EmailService.cs` devem usar ILogger
2. **Value Objects usam DomainException** — Email, Money, PhoneNumber, ZipCode, Cnpj, Coordinates lançam DomainException. Isso é aceitável (validação técnica, não fluxo de negócio). Handlers convertem via try/catch → Result.Failure.
3. **Handlers usam try/catch(DomainException)** — Todos os handlers que criam VOs capturam DomainException e convertem para Result.Failure. Isso é aceitável porque VOs não retornam Result.
4. **Catalog Query Handlers implementam ICommandHandler** — 5 handlers de query implementam ICommandHandler em vez de IQueryHandler (bug existente)
5. **Extension methods ausentes** — Não há `GetCode()`, `Match()`, etc. para Result. O código já usa `.IsSuccess`/`.IsFailure`/`.Error.Code` diretamente.

### O que NÃO precisa mudar (preservado)
- Hierarquia de exceções completa (BaseException → DomainException + ApplicationException → 6 concretas)
- GlobalExceptionHandler (IExceptionHandler pattern)
- Result Pattern (Result + Result<T> + Error)
- Domain entities que retornam Result<T> corretamente
- Value Objects que lançam DomainException (validação de invariantes)

### O que PRECISA de correção
1. **Console.WriteLine → ILogger** (EmailService)
2. **Catalog Query Handlers** — corrigir ICommandHandler → IQueryHandler
3. **Extension methods** — opcionalmente adicionar GetCode() para Result

## Próxima etapa recomendada
**Refatoração de Tratamento de Erros** — Corrigir os 3 gaps acima + commit unificado.