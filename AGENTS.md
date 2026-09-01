# AGENTS — Como executar o plano

Leia `docs/Implementation.md` integralmente antes de agir.

## Regra crítica

Você está trabalhando em um repositório possivelmente parcialmente implementado.

**NUNCA assuma que o projeto está vazio. NUNCA comece pela primeira etapa numérica sem fazer Discovery.**

Primeiro descubra o estado real.

## Primeira execução

Não altere código.

Faça:

1. inspeção da árvore;
2. leitura da documentação existente;
3. identificação de backend/frontend;
4. análise de projetos e dependências;
5. localização de entidades, Commands, Queries, Handlers, Services, Controllers, DbContext, repositories, migrations e testes;
6. análise de autenticação, multi-tenancy e infraestrutura;
7. build/testes atuais;
8. comparação com as capacidades do plano;
9. criação/atualização de `docs/implementation-status.md`; 
10. diagnóstico final.

Depois pare.

## Em qualquer etapa posterior

Antes de editar:

- consulte `implementation-status.md`;
- pesquise implementação equivalente;
- confirme o gap;
- verifique se a mudança planejada impacta o schema (entidade/DbContext) e, se sim, já preveja a necessidade de migration no plano;
- planeje o menor conjunto de mudanças.

Se já estiver concluído, valide e não recrie.

Se estiver parcial, complete apenas os gaps.

Se houver problema arquitetural, corrija somente o necessário.

### Regra de Arquitetura e Docker
- **Backend (`docker/server/Dockerfile`):** Sempre que um novo projeto `.csproj` for adicionado, removido ou renomeado na solução .NET, você **deve** atualizar automaticamente o `Dockerfile` do server, inserindo o `COPY` correspondente antes do comando `dotnet restore`.
- **Frontend (`docker/client/Dockerfile` - Futuro):** Quando a camada de frontend for implementada, crie e mantenha o `Dockerfile` correspondente (seguindo o padrão de copiar os arquivos de dependência — ex: `package.json` — antes de rodar o comando de instalação/build para otimizar o cache).

### Regra de Migrations (EF Core)
- Sempre que uma alteração for feita em **entidades, DbContext, Fluent API (`OnModelCreating`), Value Objects mapeados, ou qualquer configuração que afete o schema**, você **deve**, antes de considerar a etapa concluída:
  1. Verificar se existe uma migration pendente para essa alteração (`dotnet ef migrations has-pending-model-changes` ou comparação manual do modelo com a última migration aplicada).
  2. Caso exista alteração de schema sem migration correspondente, **gerar e sugerir explicitamente o comando** (ex: `dotnet ef migrations add NomeDaMigration --project ... --startup-project ...`), nunca aplicar (`update`) sem confirmação do usuário.
  3. Reportar isso de forma destacada no resumo da etapa — nunca deixar implícito ou omitir só porque a build passou (build passa mesmo com schema desatualizado).
- Nunca finalize uma etapa que envolveu mudança de entidade/DbContext sem responder explicitamente: **"Migration necessária: sim/não"** e, se sim, o nome sugerido e o comando.

## Proibições

- Não duplicar código existente.
- Não criar abstrações desnecessárias.
- Não antecipar funcionalidades futuras.
- Não usar MediatR.
- Não colocar regra de negócio em Controllers.
- Não confiar em TenantId enviado pelo cliente.
- Não agrupar tipos públicos/internal em um `.cs`.
- Não fazer commit automaticamente.
- Não avançar automaticamente.

## Padrão de Mensagens de Commit

Sempre que for sugerir ou gerar uma mensagem de commit (quando solicitado), siga estritamente estas regras:
* **Adicionar arquivos:** Sempre execute `git add .` (ou adicione explicitamente todos os arquivos modificados e criados) antes de sugerir ou realizar qualquer commit, garantindo que nenhum arquivo novo ou alterado fique de fora.
* **Idioma:** Português do Brasil (PT-BR).
* **Padrão:** Conventional Commits (`<tipo>(<escopo>): <descrição>`).
* **Estilo:** Descrição curta e imperativa (ex: *adiciona*, *corrige*, *atualiza*).
* **Tipos permitidos:** `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`.

## Final de etapa

Antes de perguntar, declare obrigatoriamente:

**Migration pendente:** Sim / Não
(se Sim: comando sugerido de `dotnet ef migrations add ...`, sem aplicar)

Depois, sempre parar e perguntar exatamente:

O que deseja fazer agora?

0. Deseja seguir para a próxima etapa?
1. Deseja realizar o commit sugerido?
2. Deseja verificar/revisar o que foi implementado antes de continuar?
3. Deseja apontar algum ajuste, ponto específico ou bug no código atual antes de prosseguir?

Informe a opção para confirmar sua decisão no formato:

0 para (Y), 1 para (Y), 2 para (Y), 3 para (Y)

AGUARDANDO RESPOSTA DO USUÁRIO.