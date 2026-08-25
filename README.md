# game-rpg

Projeto de um jogo RPG, desenvolvido seguindo o fluxo de **Spec-Driven Development**
do [GitHub Spec Kit](https://github.com/github/spec-kit) ("Speckit") com o Claude Code.

## O que é o Speckit

O Speckit organiza o desenvolvimento em torno de artefatos versionados (constituição,
especificações, planos e tarefas) que guiam o agente de IA antes de qualquer código ser
escrito. A ideia é reduzir ambiguidade: primeiro definimos *o quê* e *por quê*, depois
*como*, e só então implementamos.

Principais artefatos, em `.specify/`:

- `.specify/memory/constitution.md` — os princípios não-negociáveis do projeto (arquitetura,
  testes, performance, simplicidade). Toda spec/plano deve respeitar essas regras.
- `specs/` — especificações de features (criadas por feature, uma pasta cada).
- `.specify/templates/` e `.specify/scripts/` — infraestrutura interna do Speckit.

## Fluxo de trabalho

Os comandos abaixo são *skills* do Claude Code, instaladas em `.claude/skills/` durante o
`specify init`. Use-os nesta ordem para cada nova feature:

1. `/speckit-constitution` — cria ou atualiza os princípios do projeto (já ratificada em
   `v1.0.0` — veja `.specify/memory/constitution.md`).
2. `/speckit-specify` — descreve uma nova feature em linguagem natural e gera a
   especificação baseline.
3. `/speckit-clarify` *(opcional)* — faz perguntas estruturadas para eliminar ambiguidades
   antes do plano.
4. `/speckit-plan` — transforma a especificação em um plano técnico de implementação.
5. `/speckit-tasks` — quebra o plano em tarefas acionáveis.
6. `/speckit-analyze` *(opcional)* — checa consistência entre spec, plano e tarefas.
7. `/speckit-checklist` *(opcional)* — gera checklists de qualidade para validar os
   requisitos.
8. `/speckit-implement` — executa a implementação com base nas tarefas geradas.
9. `/speckit-converge` — avalia o estado atual do código e adiciona tarefas remanescentes,
   útil para reconciliar trabalho feito fora do fluxo padrão.

Cada etapa produz artefatos versionados em `specs/`, então o histórico de decisões do
projeto fica rastreável no próprio repositório, não apenas na conversa com o agente.

## Status atual

- Stack (engine/linguagem) ainda não definida — ver `TODO(TECH_STACK)` em
  `.specify/memory/constitution.md`.
- Nenhuma feature especificada ainda. Próximo passo recomendado: `/speckit-specify`.
