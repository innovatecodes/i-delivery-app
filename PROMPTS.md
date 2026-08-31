# PROMPTS.md — Guia de Comandos para o OpenCode

Guia rápido de comandos e frases de comando para controlar o comportamento da IA no VS Code, garantindo que ela siga estritamente o protocolo do `AGENTS.md`.

---

### 1) PRIMEIRA MENSAGEM AO ABRIR O PROJETO (Verificação do protocolo)
*Use esta antes de deixar o agente mexer em qualquer código, especialmente na primeira vez ou com modelos mais leves:*
> Antes de fazer qualquer coisa, me explique resumidamente qual é o protocolo de execução que você vai seguir neste projeto, baseado no `AGENTS.md`.

### 2) INICIAR / RETOMAR A ETAPA ATUAL
> Leia o `docs/implementation-status.md` (se ele existir em docs/) e o plano em `docs/Implementation.md`. Comece a analisar ou implementar a próxima etapa recomendada seguindo rigorosamente o protocolo do `AGENTS.md`.

### 2.1) CONTINUAR DE ONDE PAROU (Se o agente pausar ou travar)
> Continue de onde parou, seguindo rigorosamente o protocolo do `AGENTS.md`.

### 3) APROVAR A ETAPA E SEGUIR PARA A PRÓXIMA
*(Use depois de revisar o código e atualizar o status)*
> Aprovado. Pode seguir para a próxima etapa.

### 4) PEDIR AJUSTE ANTES DE AVANÇAR
> Antes de seguir, ajuste isso: `<descreva o problema ou ponto específico>`.

### 5) PEDIR REVISÃO SEM AVANÇAR
> Antes de decidir, me mostre um resumo do que foi implementado nesta etapa e quais arquivos foram criados ou alterados.

### 6) CONFIRMAR O COMMIT SUGERIDO
> Pode fazer o commit sugerido utilizando o Conventional Commit apropriado.

### 7) SE O AGENTE TENTAR PULAR ETAPA OU ADIANTAR ALGO (Correção)
> Pare. Isso não pertence à etapa atual. Volte e siga apenas o escopo descrito no plano de capacidades, conforme o `AGENTS.md`.

### 8) SE O AGENTE PARECER NÃO TER LIDO O AGENTS.MD (Reforço manual)
> Siga o protocolo de execução deste projeto, que está no `AGENTS.md` na raiz: faça o Discovery, leia o `docs/implementation-status.md` antes de agir, implemente somente o escopo necessário, nunca pule etapas, e ao final pare e pergunte o que eu quero fazer antes de commitar ou avançar.

### 9) VERIFICAR EM QUE ETAPA O PROJETO ESTÁ
> Qual é o status atual do projeto segundo o `docs/implementation-status.md` e qual é a próxima ação recomendada?

### 10) PULAR DIRETO PARA UMA ETAPA ESPECÍFICA (Uso excepcional)
> Quero verificar o conteúdo da capacidade `<nome/número da etapa>` antes de chegarmos nela, só para consulta — não implemente nada ainda.
