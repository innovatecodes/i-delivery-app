# i-delivery

SaaS Multi-Tenant para Sistema de Delivery

## Arquitetura

```
Project/
├── apps/
│   ├── client/     # Angular 20 Frontend
│   └── server/     # .NET 8 Backend
├── docs/
├── docker/
│   ├── client/
│   └── server/
├── docker-compose.yml
└── README.md
```

## Tecnologias

### Backend
- C# / .NET 8
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- CQRS (puro, sem MediatR)
- Clean Architecture + DDD
- FluentValidation

### Frontend
- Angular 20 (Standalone Components)
- TypeScript
- Tailwind CSS
- Font Awesome
- PWA
- Mobile First

## Como executar

```bash
# Backend + Database
docker compose up -d

# Frontend (desenvolvimento)
cd apps/client
npm install
npm start
```

## Estrutura do Backend

```
apps/server/
├── src/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
└── tests/
    ├── UnitTests/
    └── IntegrationTests/
```

## Documentação

Veja [docs/Implementation – Multi-Tenant SaaS for Delivery.md](docs/Implementation%20%E2%80%93%20Multi-Tenant%20SaaS%20for%20Delivery.md) para o plano completo de implementação.