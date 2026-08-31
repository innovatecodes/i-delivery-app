# i-delivery

SaaS Multi-Tenant para Sistema de Delivery

## 🗺️ Visão Geral e Plano Orientado por Estado

Este repositório utiliza um **Plano Orientado por Estado**. Os agentes de IA e desenvolvedores devem fazer *Discovery* antes de qualquer implementação e reconciliar o plano com o código já existente.

Fluxo de trabalho obrigatório:
`Discovery → Estado real → Gaps → Menor alteração → Testes → Status → Parada → Autorização`

Arquivos de diretrizes e planejamento:
- `AGENTS.md` — Regras consolidadas para os agentes de IA.
- `docs/Implementation.md` — Plano completo e regras de execução.
- `docs/implementation-status.md` — Mapa oficial do estado atual do projeto (criado/atualizado pelo agente).

---

## 🏗️ Arquitetura do Repositório

```
Project/
├── apps/
│   ├── client/     # Angular 20 Frontend
│   └── server/     # .NET 9 Backend
├── docs/
├── docker/
│   ├── client/
│   └── server/
├── docker-compose.yml
├── AGENTS.md
└── README.md
```

---

## 🚀 Tecnologias

### Backend
- C# / .NET 9
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

---

## ⚙️ Como executar

```bash
# Backend + Database
docker compose up -d

# Frontend (desenvolvimento)
cd apps/client
npm install
npm start
```

---

## 📂 Estrutura do Backend

```
apps/
└── server/
    ├── src/
    │   ├── Api/
    │   ├── Application/
    │   ├── Domain/
    │   ├── Infrastructure/
    │   └── SharedKernel/
    │
    └── tests/
        ├── UnitTests/
        └── IntegrationTests/
```

---

## 📚 Documentação

Veja `docs/Implementation.md` para o plano completo de implementação e as regras detalhadas de execução orientada por estado.