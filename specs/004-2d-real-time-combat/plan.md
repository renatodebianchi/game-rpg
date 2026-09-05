# Implementation Plan: Combate em Tempo Real 2D (estilo Tales of Phantasia)

**Branch**: `004-2d-real-time-combat` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-2d-real-time-combat/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Substitui o combate baseado em grid/turnos (feature 001) por um combate em tempo real numa
arena 2D side-view (Linear Motion Battle System, estilo Tales of Phantasia): o jogador se move
livremente no eixo horizontal, ataca corpo a corpo/à distância e conjura habilidades com tempo
de execução interrompível, enquanto inimigos agem de forma autônoma e contínua. A câmera (tanto
na arena quanto na Exploração) segue o personagem centralizado, mas para nas bordas do
mapa/arena (FR-015). O jogo passa a usar o URP 2D Renderer em vez da câmera isométrica 3D. Os
sistemas de atributos, habilidades, sobrevivência e reputação/economia são preservados sem
alteração de regras — apenas a camada de combate e a câmera mudam. Abordagem técnica detalhada
em [research.md](./research.md).

## Technical Context

**Language/Version**: C# (Unity 6000.5.9f1 — mesma versão das features 001/002/003)

**Primary Dependencies**: URP com **2D Renderer** (`Renderer2DData`) em vez do 3D Universal
Renderer atual; UGUI (inalterado); nenhum pacote novo do Package Manager — Cinemachine e Input
System já instalados seguem disponíveis mas não são estritamente necessários para esta feature
(ver research.md, "Decision: Câmera com clamp nas bordas")

**Storage**: Inalterado (JSON local via `Application.persistentDataPath`)

**Testing**: Unity Test Framework — EditMode (limites da `BattleArena`, temporização/interrupção
de `RealTimeAction`, decisão de `EnemyCombatAI`, canal de fuga em tempo real) e PlayMode
(fluxo completo de um encontro, análogo ao `CombatEncounterFlowTests` já existente)

**Target Platform**: PC desktop (mesmo alvo das features 001/002/003)

**Project Type**: Substituição/extensão do mesmo projeto Unity single-player já existente

**Performance Goals**: 60 fps; latência de input-para-ação (movimento, ataque, conjuração) é
ainda mais crítica que no modelo por turnos anterior, por ser a essência do combate em tempo
real (Princípio IV) — meta de <100ms do input ao efeito visível

**Constraints**: As regras dos sistemas de atributos, árvore de habilidades, sobrevivência e
reputação/economia NÃO podem mudar de comportamento (FR-012/SC-004); o código de grid/turnos
tornado obsoleto DEVE ser removido, não deixado como código morto (Assumptions, Princípio V)

**Scale/Scope**: 1 novo modelo de combate substituindo ~10 arquivos do combate por turnos da
feature 001; troca do renderer URP de 3D para 2D; 1 novo componente de câmera compartilhado
(Exploração + arena de combate)

Todas as decisões acima foram resolvidas em [research.md](./research.md); nenhuma marcação
`NEEDS CLARIFICATION` permanece.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` (v1.0.1):

| Princípio | Avaliação | Status |
|---|---|---|
| I. Gameplay-First Design | O pivô é inteiramente motivado pela experiência de jogo pedida (combate mais dinâmico, estilo Tales of Phantasia) — não é uma mudança técnica isolada. | PASS |
| II. Modular & Data-Driven Architecture | `RealTimeActionDefinition` como `ScriptableObject` (tempo de execução/conjuração, alcance, custo, mesmo padrão de `SkillNodeDefinition`); a lógica de combate continua desacoplada da UI/apresentação, como já era com `CombatEncounter`. | PASS |
| III. Test Coverage for Core Systems (NON-NEGOTIABLE) | `BattleArena`, `RealTimeActionExecutor`, `EnemyCombatAI` e o canal de fuga são classes C# puras avançadas por `AdvanceTime(TimeSpan)`/`Tick`, testáveis sem cena carregada (mesmo padrão de `WorldClock`/`HungerSystem`); a câmera com clamp é apresentação pura e fica isenta, conforme o próprio princípio já prevê. | PASS |
| IV. Performance & Responsiveness Budgets | O combate em tempo real torna a responsividade de input um requisito ainda mais central (ver Technical Context); mantém o orçamento de 60 fps/<100ms já estabelecido. | PASS |
| V. Simplicity & Iterative Scope | Remove deliberadamente o código de grid/pathfinding/turnos que deixa de ser necessário (sem coexistência com o novo modelo); a IA de inimigo e o clamp de câmera usam a abordagem mais simples que resolve o problema atual (sem pathfinding em arena sem obstáculos; sem `CinemachineConfiner2D`, só um componente de câmera direto), evitando complexidade não demandada pelo escopo atual. | PASS |

**Resultado**: Nenhuma violação. Nenhuma entrada necessária em "Complexity Tracking".

**Nota de processo**: esta feature substitui — não estende — o sistema de combate da feature
001; os arquivos removidos estão listados em Project Structure abaixo, conforme já registrado
nas Assumptions da spec.

## Project Structure

### Documentation (this feature)

```text
specs/004-2d-real-time-combat/
├── plan.md               # This file (/speckit-plan command output)
├── research.md           # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   ├── realtime-action-contract.md
│   ├── flee-channel-contract.md
│   └── camera-bounds-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

Continuação do projeto Unity único já estabelecido (features 001-003); esta feature substitui
o combate por turnos e a câmera isométrica 3D pelos equivalentes em tempo real/2D.

```text
Assets/
├── Scripts/
│   ├── Combat/
│   │   ├── BattleArena.cs                        # novo — substitui Grid/GridMap.cs
│   │   ├── IRealTimeCombatant.cs                  # novo — substitui ICombatant.cs (posição float, sem TurnResources)
│   │   ├── RealTimeActionDefinition.cs            # novo (ScriptableObject: alcance, tempo de execução, custo)
│   │   ├── CombatantActionState.cs                # novo — cooldowns e recurso (TechPoints) por combatente
│   │   ├── RealTimeActionExecutor.cs              # novo — substitui ActionResolver.cs (reaproveita IDamageModifier)
│   │   ├── IDamageModifierRegistry.cs             # novo — interface compartilhada para RegisterDamageModifier
│   │   ├── CombatArenaEncounter.cs                # novo — substitui CombatEncounter.cs (sem iniciativa/turnos)
│   │   ├── EnemyCombatAI.cs                       # novo — substitui EnemyAI.cs (decisão contínua, não por turno)
│   │   ├── RealTimeFleeAction.cs                  # novo — substitui FleeAction.cs (canal contínuo, não gasta turno)
│   │   ├── CombatOutcomeHandler.cs                # mantido (adaptado ao novo CombatArenaEncounter)
│   │   ├── NonPlayerCombatant.cs                  # adaptado (PositionX float em vez de GridCoordinate)
│   │   ├── ICombatant.cs                          # REMOVIDO (substituído por IRealTimeCombatant.cs)
│   │   ├── TurnResources.cs                       # REMOVIDO
│   │   ├── TurnResourceManager.cs                 # REMOVIDO
│   │   ├── ActionResolver.cs                      # REMOVIDO
│   │   ├── EnemyAI.cs                             # REMOVIDO
│   │   ├── FleeAction.cs                          # REMOVIDO
│   │   ├── InitiativeService.cs                   # REMOVIDO (sem ordem de turno)
│   │   ├── GridPathfinding.cs                     # REMOVIDO (sem pathfinding em arena sem obstáculos)
│   │   ├── CombatCameraController.cs              # REMOVIDO (substituído por Demo/BoundedFollowCamera.cs)
│   │   └── Grid/GridMap.cs                        # REMOVIDO (substituído por BattleArena.cs)
│   ├── Characters/
│   │   └── Character.cs                          # adaptado: implementa IRealTimeCombatant em vez de ICombatant
│   ├── Skills/
│   │   └── CapabilityResolver.cs                  # adaptado: ApplyAcquiredCapabilities aceita IDamageModifierRegistry
│   └── Demo/
│       ├── BoundedFollowCamera.cs                 # novo — câmera compartilhada com clamp nas bordas (FR-015)
│       ├── BattleArenaDemoController.cs           # novo — substitui CombatDemoController.cs
│       ├── ExplorationCharacterController.cs      # adaptado: sem billboard 3D, usa BoundedFollowCamera
│       ├── GridTileClickHandler.cs                # REMOVIDO (sem grid clicável)
│       └── CombatDemoController.cs                # REMOVIDO (substituído por BattleArenaDemoController.cs)
├── Settings/
│   └── GameRpgUrpRenderer2D.asset                 # novo — Renderer2DData (substitui o Universal Renderer 3D)
└── Editor/
    └── ProjectBootstrap.cs                        # adaptado: cria o URP 2D Renderer, a arena e a cena de Exploração em 2D side-view

Assets/Tests/
├── EditMode/
│   ├── BattleArenaTests.cs                        # novo
│   ├── RealTimeActionExecutorTests.cs             # novo (inclui interrupção de conjuração, FR-009)
│   ├── EnemyCombatAITests.cs                      # novo
│   └── RealTimeFleeActionTests.cs                 # novo
└── PlayMode/
    └── CombatArenaEncounterFlowTests.cs           # novo — substitui CombatEncounterFlowTests.cs
```

**Structure Decision**: Continuação do projeto Unity único (Princípio V). O combate por turnos
da feature 001 é removido (não mantido em paralelo) — ver Assumptions da spec e a linha "Nota de
processo" acima. `IDamageModifier`/`HungerSystem`/`SanitySystem`/`SkillTreeService`/
`CapabilityResolver` são reaproveitados sem alteração de regra (FR-012), só a costura de
integração com o novo executor de ações muda.

## Complexity Tracking

*Nenhuma violação da Constitution Check acima — seção não aplicável.*
