# Implementation Plan: RPG Sandbox com Árvore de Habilidades, Combate Tático e Mundo Reativo

**Branch**: `001-isometric-sandbox-rpg` | **Date**: 2026-08-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-isometric-sandbox-rpg/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Construir a base de um RPG sandbox de fantasia em Unity: combate tático por turnos em grade
sobre visão isométrica (estilo Baldur's Gate 3), uma árvore de habilidades rica sem classes
fixas (trilhas Combatente, Arcanista e nós híbridos, com respec livre e ilimitado — FR-018),
mecânicas de sobrevivência (fome e sanidade, com penalidades cumulativas quando ambas estão
críticas — FR-021) e um sistema de reputação totalmente independente por comunidade (FR-020) que
conecta escolhas do jogador (salvar/prejudicar NPCs — inclusive em combate forçado, FR-022 —, e
transportar recursos) a consequências simuladas na economia e população de vilas, incluindo o
colapso permanente de uma vila que perde 100% de sua população (FR-019). Abordagem técnica:
Unity (URP + Cinemachine) com conteúdo orientado a dados via `ScriptableObject`, lógica de
combate/simulação desacoplada da apresentação e testável via Unity Test Framework, e
persistência local em JSON — conforme decisões detalhadas em [research.md](./research.md).

## Technical Context

**Language/Version**: C# (Unity, LTS mais recente disponível no início do projeto — ex. Unity 6 LTS)

**Primary Dependencies**: Unity URP, Cinemachine, Unity Input System, ScriptableObjects (dados de conteúdo), Unity Test Framework

**Storage**: Arquivos locais em JSON via `Application.persistentDataPath` (sem banco de dados; sem sincronização em nuvem no MVP)

**Testing**: Unity Test Framework — EditMode (lógica pura: combate, árvore de habilidades, fome/sanidade, simulação de economia) e PlayMode (fluxos integrados: encontro de combate completo, ciclo save/load)

**Target Platform**: PC desktop (Windows como plataforma primária; build standalone Unity)

**Project Type**: Jogo desktop single-player (projeto único Unity)

**Performance Goals**: 60 fps em hardware de médio porte durante exploração e combate; latência de input percebida < 100ms (movimento, ataque, navegação de menu/árvore de habilidades)

**Constraints**: Offline-capable (sem dependência de serviços de rede no MVP); escopo de mundo limitado a uma região com poucas vilas interligadas (ver Scale/Scope)

**Scale/Scope**: MVP com 1 região, ~2–4 vilas interligadas, população de NPCs simulados na casa de dezenas por vila, árvore de habilidades com dezenas de nós entre as trilhas Combatente/Arcanista/Híbrida

Todas as decisões acima foram resolvidas em [research.md](./research.md); nenhuma marcação
`NEEDS CLARIFICATION` permanece.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` (v1.0.0):

| Princípio | Avaliação | Status |
|---|---|---|
| I. Gameplay-First Design | Todas as 4 histórias de usuário do spec partem da experiência do jogador (combate, build, sobrevivência, consequências); nenhum sistema técnico é proposto sem uma história correspondente. | PASS |
| II. Modular & Data-Driven Architecture | Conteúdo (habilidades, itens, NPCs, vilas) definido via `ScriptableObject`; lógica de combate/simulação de vila desacoplada da apresentação (ver contratos de dados). | PASS |
| III. Test Coverage for Core Systems (NON-NEGOTIABLE) | Combate, fórmulas, save/load e transições de estado de reputação/economia cobertos por EditMode/PlayMode tests (ver `research.md` e `quickstart.md`); apresentação (renderização/animação) explicitamente fora dessa exigência. | PASS |
| IV. Performance & Responsiveness Budgets | Orçamento explícito definido no Technical Context (60 fps, <100ms de input) antes do início da construção dos sistemas. | PASS |
| V. Simplicity & Iterative Scope | Escopo do MVP limitado (1 região, poucas vilas); alternativas mais simples avaliadas e preferidas em cada decisão técnica (ver "Alternatives considered" em `research.md`); nenhuma engine/abstração introduzida sem necessidade concreta. | PASS |

**Resultado**: Nenhuma violação. Nenhuma entrada necessária em "Complexity Tracking".

**Re-check pós-clarificação (2026-08-25)**: as decisões registradas em `/speckit-clarify`
(respec livre de habilidades, colapso permanente de vila, reputação independente por comunidade,
penalidades cumulativas de fome/sanidade, combate forçado contando como escolha de impacto) são
regras de negócio dentro dos sistemas já modulares definidos — nenhuma delas introduz uma nova
dependência, engine ou camada de abstração. Constitution Check permanece PASS em todos os 5
princípios.

**Nota de governança**: a stack técnica escolhida (Unity/C#) resolveu o `TODO(TECH_STACK)` que
estava em aberto na seção "Technical Constraints" da constituição. A seção foi atualizada via
`/speckit-constitution` (constituição v1.0.1) para registrar formalmente Unity/C#/PC desktop.

## Project Structure

### Documentation (this feature)

```text
specs/001-isometric-sandbox-rpg/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   ├── skill-node-data-contract.md
│   ├── save-data-contract.md
│   └── village-economy-simulation-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

Projeto Unity único (single project), sem separação frontend/backend (não se aplica a um jogo
desktop standalone).

```text
Assets/
├── Scripts/
│   ├── Core/            # loop de jogo, gerenciador de turnos, tempo simulado, save/load
│   ├── Combat/          # grid lógico de combate, iniciativa, resolução de ações, IA de inimigos
│   ├── Characters/      # personagem do jogador, atributos, fome/sanidade
│   ├── Skills/          # árvore de habilidades (validação de pré-requisitos, investimento)
│   ├── World/           # comunidades/vilas, recursos, simulação de economia/população
│   ├── NPCs/            # estado e comportamento de NPCs, ganchos de diálogo
│   └── UI/              # HUD, UI da árvore de habilidades, UI de diálogo/reputação
├── Data/                 # assets ScriptableObject (nós de habilidade, itens/recursos, NPCs, vilas)
├── Scenes/                # cenas de exploração e de combate
├── Prefabs/
└── Art/                   # tilesets/modelos isométricos, personagens, VFX, iluminação

└── Tests/
    ├── EditMode/          # testes unitários: combate, árvore de habilidades, fome/sanidade, economia de vila
    └── PlayMode/          # testes de integração: encontro de combate completo, ciclo save/load
```

**Structure Decision**: Projeto único Unity (Option 1 adaptada para um jogo, não uma
lib/serviço). Não há necessidade de separação frontend/backend nem de múltiplos projetos —
manter um único projeto Unity atende ao Princípio V (Simplicidade) e é suficiente para o escopo
do MVP definido em `research.md`.

## Complexity Tracking

*Nenhuma violação da Constitution Check acima — seção não aplicável.*
