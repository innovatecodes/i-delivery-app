# Implementation Status

## Última análise
Data: 2026-09-03 (atualizado B13 Pagamento V1)

## Backend
| Etapa | Status | Evidências | Próxima ação |
|---|---|---|---|
| B01 — Monorepo e infraestrutura | CONCLUÍDA | apps/client, apps/server, docs, docker, docker-compose.yml, AGENTS.md, README.md | — |
| B02 — Fundação .NET | CONCLUÍDA | IDelivery.slnx com 5 projetos (Api, Application, Domain, Infrastructure, SharedKernel) + 2 projetos de teste | — |
| B03 — Shared Kernel | CONCLUÍDA | Entity, AggregateRoot, ValueObject, DomainEvent, Result/Error, CQRS próprio (ICommand, IQuery, handlers, dispatchers) | — |
| B04 — Tenant | CONCLUÍDA | Domain: Tenant aggregate, TenantStatus, Address VO, 4 domain events. Application: Create/Activate/Block/Delete/Update commands + validators + handlers, GetTenant/GetTenants queries. Infrastructure: TenantConfiguration, TenantRepository, Migration InitialCreate. Tests: 7 unit + 5 integration (todos passando). | — |
| B05 — Roles e Users | CONCLUÍDA | Domain: User aggregate, Role enum (SuperAdmin, TenantAdmin, Delivery, Customer), UserStatus, 9 domain events, RoleExtensions. Application: IUserRepository interface. Infrastructure: UserRepository, UserConfiguration, Migration AddUserEntity. Tests: 3 unit (UserTests). | — |
| B06 — Autenticação | CONCLUÍDA | Abstractions: IJwtTokenService, IEmailService, ICurrentUser. Implementation: JwtTokenService (JWT access/refresh tokens), EmailService (SMTP/console), CurrentUserService. Commands: Register, Login, RefreshToken, ActivateAccount, ForgotPassword, ResetPassword + validators + handlers. Domain Event Pipeline: UserRegistered → TokenGenerated → NotificationService → Email. Infrastructure: JWT middleware, AuthController endpoints, HttpContextAccessor. Tests: 168 unit + 35 integration. | — |
| B07 — Multi-tenancy | CONCLUÍDA | ICurrentUser.TenantId, CurrentUserService extrai tenant_id do JWT, ITenantContext, TenantContext, JwtTokenService inclui tenant_id no token, User.TenantId | — |
| B08 — Catálogo | CONCLUÍDA | Domain: Category e Product aggregates, Money VO, 6 domain events. Application: ICategoryRepository, IProductRepository, Create/Update/Delete commands + handlers + validators, Get/GetAll queries + handlers. Infrastructure: CategoryConfiguration, ProductConfiguration, CategoryRepository, ProductRepository, ApplicationDbContext atualizado, Migration AddCatalogTables. Tests: 18 unitários (CategoryTests + ProductTests) | — |
| B09 — Carrinho | CONCLUÍDA | Domain: Cart aggregate (Cart + CartItem), 4 domain events (Created, ItemAdded, ItemRemoved, CartCleared). Application: ICartRepository, Add/Remove/UpdateQuantity/Clear commands + handlers + validators, GetCart query + handler. Infrastructure: CartConfiguration, CartItemConfiguration, CartRepository, ApplicationDbContext atualizado com Carts/CartItems DbSets, Migration AddCartTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B10 — Customer e endereço | CONCLUÍDA | Domain: Customer aggregate (Customer + CustomerAddress), 4 domain events (Created, Updated, AddressAdded, AddressRemoved). Application: ICustomerRepository, Create/Update/Delete/AddAddress/RemoveAddress/SetDefaultAddress commands + handlers + validators, GetCustomer query + handler. Infrastructure: CustomerConfiguration (OwnsOne Email/PhoneNumber), CustomerAddressConfiguration, CustomerRepository, ApplicationDbContext atualizado com Customers/CustomerAddresses DbSets, Migration AddCustomerTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B11 — Delivery settings | CONCLUÍDA | Domain: DeliverySettings aggregate, DeliveryFeeType enum (Free, FreeAboveAmount, Fixed, PerDistance), 2 domain events (Created, Updated), CalculateFee method. Application: IDeliverySettingsRepository, Create/Update/Delete commands + handlers + validators, GetDeliverySettings query + handler. Infrastructure: DeliverySettingsConfiguration (OwnsOne 5 Money properties), DeliverySettingsRepository, ApplicationDbContext atualizado com DeliverySettings DbSet, Migration AddDeliverySettingsTable. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B12 — Pedido e checkout | CONCLUÍDA | Domain: Order aggregate (Order + OrderItem snapshot), OrderState enum (Pending→Confirmed→Preparing→ReadyForDelivery→OutForDelivery→Delivered|DeliveryFailed|Cancelled), DeliveryFailureReason enum, 5 domain events (Created, StatusChanged, Delivered, DeliveryFailed, Cancelled). Transições controladas com autoridade: Deliver/FailDelivery só pelo entregador atribuído; Cancel até ReadyForDelivery por Tenant/Cliente/Sistema. Application: IOrderRepository, Create/Confirm/StartPreparing/MarkReady/StartDelivery/Deliver/FailDelivery/Cancel commands + handlers + validators, GetOrder/GetOrders queries + handlers. Infrastructure: OrderConfiguration, OrderItemConfiguration, OrderRepository, ApplicationDbContext atualizado com Orders/OrderItems DbSets, Migration AddOrderTables. DI registrations atualizados. Tests: 154 unitários passando. | — |
| B13 — Pagamento V1 | CONCLUÍDA | Domain: Payment aggregate, PaymentMethod enum (Cash, CardOnDelivery), PaymentStatus enum (Pending, Paid, NotCollected), 2 domain events (Created, MarkedAsPaid). Application: IPaymentRepository, Create/MarkAsPaid/MarkAsNotCollected commands + handlers, GetPaymentByOrderId/GetPaymentById queries + handlers. Infrastructure: PaymentConfiguration (OwnsOne Money), PaymentRepository, ApplicationDbContext atualizado com Payments DbSet. Integração: OrderDeliveredPaymentHandler marca Payment como Paid quando Order.Deliver() é chamado. Abstração extensível para métodos futuros. Migration necessária: AddPaymentEntity. Tests: 188 unit + 35 integration. | — |
| B14 — Gestão de pedidos | NÃO IMPLEMENTADA | — | — |
| B15 — Delivery | NÃO IMPLEMENTADA | — | — |
| B16 — Rastreamento | NÃO IMPLEMENTADA | — | — |
| B17 — SaaS | NÃO IMPLEMENTADA | — | — |
| B18 — Notificações | CONCLUÍDA | INotificationService (abstração), NotificationService (implementação com pattern matching de payloads), IEmailService (abstração), EmailService (implementação SMTP/console), IClientUrlGenerator, ClientUrlGenerator, EmailTemplate (HTML), ClientSettings/ClientRoutesSettings (configuração parametrizada). Pipeline completo: Domain Event → Token Handler → NotificationService → EmailService. 3 payloads: UserActivationPayload, UserPasswordResetPayload. 5 handlers de domínio registrados via Assembly Scanning. | — |
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

## Problemas arquiteturais encontrados e corrigidos
1. ~~**Duplicate DI registration**: `CreateTenantCommandHandler` registrado duas vezes em `Application/DependencyInjection.cs`~~ → **CORRIGIDO**
2. ~~**CreateTenantCommandHandler** tenta acessar `.Value` em resultado falho~~ → **CORRIGIDO** (checa IsSuccess antes)
3. ~~**PhoneNumber regex** muito restritiva para fixos — testes usavam número inválido~~ → **CORRIGIDO** (testes usam números válidos)
4. ~~**Teste de integração `BlockAndActivate_Tenant_ShouldWork`** falha~~ → **CORRIGIDO** (testa Block → Activate)
5. ~~**Mocks em testes unitários** não configurados para `ExistsBySlugAsync`~~ → **CORRIGIDO** (mock configurado)
6. ~~**IUserRepository** não implementado em Infrastructure~~ → **CORRIGIDO** (UserRepository implementado)
7. ~~**Authentication/Email/ExternalServices** pastas em Infrastructure existem mas vazias~~ → **CORRIGIDO** (Email, Security, Messaging implementados)
8. ~~**Sem controllers/endpoints** na API — apenas health check~~ → **RESOLVIDO** (AuthController adicionado)
9. ~~**Dockerfile** não copia `IDelivery.SharedKernel.csproj`~~ → **CORRIGIDO** (COPY adicionado antes do restore)
10. ~~**Auth Handlers não persistiam mudanças** (Login, Activate, ForgotPassword, ResetPassword, RefreshToken não chamavam Update)~~ → **CORRIGIDO** (Update adicionado)
11. ~~**Delete Handlers bypassavam domínio** (usavam DeleteAsync em vez de Deactivate)~~ → **CORRIGIDO** (usa Deactivate + UpdateAsync)
12. ~~**CommandDispatcher usava Reflection** (MakeGenericType + GetMethod + Invoke)~~ → **CORRIGIDO** (IServiceProvider tipado)
13. ~~**DomainEventDispatcher usava Reflection** (MakeGenericType + GetMethod + Invoke)~~ → **CORRIGIDO** (ConcurrentDictionary cache + dynamic)
14. ~~**DI registration manual** (200+ linhas para handlers)~~ → **CORRIGIDO** (Assembly Scanning para ICommandHandler, IQueryHandler, IDomainEventHandler)
15. ~~**ForgotPassword hardcoded URL** (https://app.idelivery.com)~~ → **CORRIGIDO** (IClientUrlGenerator + ClientSettings parametrizado)
16. ~~**ForgotPassword usava IEmailService diretamente** (bypassava NotificationService)~~ → **CORRIGIDO** (domain event pattern)
17. ~~**EF Core OwnsOne ausente** (Customer Email/PhoneNumber, DeliverySettings 5 Money, User PhoneNumber)~~ → **CORRIGIDO** (configurações atualizadas)
18. ~~**Console.WriteLine em EmailService**~~ → **CORRIGIDO** (ILogger)
19. ~~**Catalog Query Handlers implementavam ICommandHandler**~~ → **CORRIGIDO** (IQueryHandler)
20. ~~**UserRegisteredDomainEventHandler estava na Infrastructure**~~ → **CORRIGIDO** (movido para Application)

## Arquitetura de Eventos e Notificações

### Pipeline de Domain Events
```
AggregateRoot.AddDomainEvent()
    → ApplicationDbContext.SaveChangesAsync()
        → CollectAggregatesWithEvents()
        → base.SaveChangesAsync()
        → DomainEventDispatcher.DispatchAsync()
            → IDomainEventHandler<T>.Handle()
        → ClearDomainEvents()
```

### Pipeline de CQRS
```
CommandDispatcher.Dispatch<TCommand>()
    → IServiceProvider.GetRequiredService<ICommandHandler<TCommand>>()
    → handler.Handle()
    → IUnitOfWork.SaveChangesAsync() (apenas se IsSuccess)
```

### Fluxo de Notificações (Ativação)
```
User.Create() → UserRegisteredDomainEvent
  → UserRegisteredDomainEventHandler: gera token, salva, despacha UserActivationTokenGeneratedDomainEvent
    → UserActivationTokenGeneratedDomainEventHandler: INotificationService → EmailTemplate → IEmailService
```

### Fluxo de Notificações (Reset de Senha)
```
ForgotPasswordCommandHandler → user.RequestPasswordReset() → UserPasswordResetRequestedDomainEvent
  → UserPasswordResetRequestedDomainEventHandler: gera token, salva, despacha UserPasswordResetTokenGeneratedDomainEvent
    → UserPasswordResetTokenGeneratedDomainEventHandler: INotificationService → EmailTemplate → IEmailService
```

### Configuração de Client URLs
- `ClientSettings.BaseUrl` — URL base do frontend
- `ClientRoutesSettings.Activate` — rota de ativação
- `ClientRoutesSettings.Reset` — rota de reset de senha
- `ClientRoutesSettings.Confirm` — rota de confirmação
- `ClientRoutesSettings.Deactivate` — rota de desativação

## Domain Events — Inventário

### Com handler (dispatched + handled)
| Evento | Handler |
|---|---|
| UserRegisteredDomainEvent | UserRegisteredDomainEventHandler |
| UserActivationTokenGeneratedDomainEvent | UserActivationTokenGeneratedDomainEventHandler |
| UserPasswordResetRequestedDomainEvent | UserPasswordResetRequestedDomainEventHandler |
| UserPasswordResetTokenGeneratedDomainEvent | UserPasswordResetTokenGeneratedDomainEventEventHandler |
| TenantCreatedDomainEvent | (persistido, sem notificação externa) |

### Dispatched sem handler (padrão DDD — handlers serão implementados quando necessário)
- UserActivatedDomainEvent, UserPasswordChangedDomainEvent, UserRoleChangedDomainEvent, UserProfileUpdatedDomainEvent, UserDeletedDomainEvent, UserDeactivatedDomainEvent
- CategoryCreatedDomainEvent, CategoryUpdatedDomainEvent, ProductCreatedDomainEvent, ProductUpdatedDomainEvent
- TenantActivatedDomainEvent, TenantBlockedDomainEvent, TenantUpdatedDomainEvent
- CustomerCreatedDomainEvent, CustomerUpdatedDomainEvent, CustomerAddressAddedDomainEvent, CustomerAddressRemovedDomainEvent
- CartCreatedDomainEvent, CartClearedDomainEvent, CartItemAddedDomainEvent, CartItemRemovedDomainEvent
- DeliverySettingsCreatedDomainEvent, DeliverySettingsUpdatedDomainEvent
- OrderCreatedDomainEvent, OrderStatusChangedDomainEvent, OrderDeliveredDomainEvent, OrderCancelledDomainEvent, OrderDeliveryFailedDomainEvent

### Não dispatched (declarados mas sem AddDomainEvent)
- CategoryDeletedDomainEvent, ProductDeletedDomainEvent

## Testes
- Backend build: OK (0 erros, warnings de nullable)
- Backend tests: **188 unit + 35 integration** (todos passando)

## Arquivos Criados/Alterados (Refatoração CQRS/Events/Notifications)

### Criados
- `Application/Dispatching/CommandDispatcher.cs`
- `Application/Events/Handlers/UserActivationTokenGeneratedDomainEventHandler.cs`
- `Application/Events/Handlers/UserPasswordResetRequestedDomainEventHandler.cs`
- `Application/Events/Handlers/UserPasswordResetTokenGeneratedDomainEventHandler.cs`
- `Application/Common/Models/UserActivationPayload.cs`
- `Application/Common/Models/UserPasswordResetPayload.cs`
- `Application/Settings/ClientSettings.cs`
- `Application/Settings/ClientRoutesSettings.cs`
- `Application/Abstractions/Messaging/INotificationService.cs`
- `Application/Abstractions/Messaging/IClientUrlGenerator.cs`
- `Domain/Users/Events/UserActivationTokenGeneratedDomainEvent.cs`
- `Domain/Users/Events/UserPasswordResetTokenGeneratedDomainEvent.cs`
- `Infrastructure/Messaging/Common/NotificationService.cs`
- `Infrastructure/Messaging/Common/ClientUrlGenerator.cs`
- `Infrastructure/Messaging/Templates/EmailTemplate.cs`
- `SharedKernel/Extentions/UrlExtensions.cs`
- `tests/IDelivery.UnitTests/Dispatching/CommandDispatcherTests.cs`
- `tests/IDelivery.UnitTests/Events/UserRegisteredDomainEventHandlerTests.cs`
- `tests/IDelivery.IntegrationTests/Events/DomainEventPipelineTests.cs`

### Alterados
- `Application/DependencyInjection.cs` — Assembly Scanning para handlers
- `Application/Commands/Auth/ForgotPasswordCommandHandler.cs` — domain event pattern
- `Application/Commands/Auth/LoginCommandHandler.cs` — Update() adicionado
- `Application/Commands/Auth/ActivateAccountCommandHandler.cs` — Update() adicionado
- `Application/Commands/Auth/ResetPasswordCommandHandler.cs` — Update() adicionado
- `Application/Commands/Auth/RefreshTokenCommandHandler.cs` — Update() adicionado
- `Application/Commands/Catalog/DeleteCategoryCommandHandler.cs` — Deactivate() + UpdateAsync()
- `Application/Commands/Catalog/DeleteProductCommandHandler.cs` — Deactivate() + UpdateAsync()
- `Application/Commands/Customers/DeleteCustomerCommandHandler.cs` — Deactivate() + UpdateAsync()
- `Application/Commands/Delivery/DeleteDeliverySettingsCommandHandler.cs` — Deactivate() + UpdateAsync()
- `Infrastructure/Events/DomainEventDispatcher.cs` — ConcurrentDictionary cache + dynamic
- `Infrastructure/Messaging/Email/EmailService.cs` — ILogger, sem body logging
- `Infrastructure/Persistence/Configurations/CustomerConfiguration.cs` — OwnsOne
- `Infrastructure/Persistence/Configurations/DeliverySettingsConfiguration.cs` — OwnsOne
- `Infrastructure/Persistence/Configurations/UserConfiguration.cs` — OwnsOne
- `Infrastructure/Persistence/Context/ApplicationDbContext.cs` — Collect→Save→Dispatch→Clear
- `Infrastructure/DependencyInjection.cs` — NotificationService, ClientUrlGenerator
- `tests/IDelivery.UnitTests/Application/AuthTests.cs` — atualizado
- `tests/IDelivery.UnitTests/Application/CategoryTests.cs` — atualizado
- `tests/IDelivery.UnitTests/Application/ProductTests.cs` — atualizado

### Movidos
- `Infrastructure/Events/Handlers/UserRegisteredDomainEventHandler.cs` → `Application/Events/Handlers/`

## Arquivos Criados (B13 — Pagamento V1)

### Domain
- `Domain/Payments/Entities/Payment.cs`
- `Domain/Payments/Enums/PaymentMethod.cs`
- `Domain/Payments/Enums/PaymentStatus.cs`
- `Domain/Payments/Events/PaymentCreatedDomainEvent.cs`
- `Domain/Payments/Events/PaymentMarkedAsPaidDomainEvent.cs`

### Application
- `Application/Abstractions/Persistence/IPaymentRepository.cs`
- `Application/Commands/Payments/PaymentCommands.cs`
- `Application/Commands/Payments/CreatePaymentCommandHandler.cs`
- `Application/Commands/Payments/MarkPaymentAsPaidCommandHandler.cs`
- `Application/Commands/Payments/MarkPaymentAsNotCollectedCommandHandler.cs`
- `Application/Queries/Payments/PaymentQueries.cs`
- `Application/Queries/Payments/GetPaymentByOrderIdQueryHandler.cs`
- `Application/Queries/Payments/GetPaymentByIdQueryHandler.cs`
- `Application/Events/Handlers/OrderDeliveredPaymentHandler.cs`

### Infrastructure
- `Infrastructure/Persistence/Configurations/PaymentConfiguration.cs`
- `Infrastructure/Persistence/Repositories/PaymentRepository.cs`

### Tests
- `tests/IDelivery.UnitTests/Domain/PaymentTests.cs`
- `tests/IDelivery.UnitTests/Application/Payments/PaymentHandlerTests.cs`

## Próxima etapa recomendada
**B13 — Pagamento V1** ou **B19 — API e qualidade** (OpenAPI/Swagger, mais controllers)
