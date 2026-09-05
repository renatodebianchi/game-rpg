---
description: "Task list template for feature implementation"
---

# Tasks: Combate em Tempo Real 2D (estilo Tales of Phantasia)

**Input**: Design documents from `/specs/004-2d-real-time-combat/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Incluídos e OBRIGATÓRIOS para a lógica central (arena, execução/interrupção de ação,
IA de inimigo, canal de fuga, fluxo completo de encontro), por exigência do Princípio III
(NON-NEGOTIABLE) da constituição do projeto — mesmo padrão já adotado pelas features
001/002/003. A câmera (`BoundedFollowCamera`) é presentation-only e fica isenta, como já vale
para `DemoCameraController`.

**Organization**: Tarefas agrupadas por história de usuário (spec.md). Esta feature **substitui**
— não estende — o combate por turnos/grid da feature `001-isometric-sandbox-rpg` e a câmera
isométrica 3D usada desde então (features 001/003); por isso a fase Foundational inclui remoções
de arquivos, não apenas adições.

**⚠️ Nota sobre compilação entre fases**: como esta feature substitui o tipo central
`Combat.ICombatant` por `Combat.IRealTimeCombatant`, o projeto só volta a compilar por completo
ao final da **User Story 1** (que reescreve `CombatEncounter`/`CombatOutcomeHandler`) — não ao
final da fase Foundational. Isso é esperado e não é um erro: só rode a verificação completa de
compilação/testes do Unity CLI após concluir a User Story 1, não no meio dela.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: A qual história de usuário a tarefa pertence (US1, US2)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Mesmo projeto Unity único das features 001/002/003 (ver [plan.md](./plan.md#project-structure)):
`Assets/Scripts/`, `Assets/Scenes/`, `Assets/Settings/`, `Assets/Editor/`,
`Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Remover o código de combate por turnos/grid que esta feature substitui, antes de
introduzir os novos tipos que ocupam seu lugar — evita manter dois modelos de combate
coexistindo (Assumptions da spec, Princípio V).

- [X] T001 [P] Remover `Assets/Scripts/Combat/Grid/GridMap.cs` (substituído por `BattleArena`)
- [X] T002 [P] Remover `Assets/Scripts/Combat/GridPathfinding.cs` (sem pathfinding em arena sem obstáculos, research.md)
- [X] T003 [P] Remover `Assets/Scripts/Combat/TurnResources.cs`
- [X] T004 [P] Remover `Assets/Scripts/Combat/TurnResourceManager.cs`
- [X] T005 [P] Remover `Assets/Scripts/Combat/ActionResolver.cs`
- [X] T006 [P] Remover `Assets/Scripts/Combat/EnemyAI.cs`
- [X] T007 [P] Remover `Assets/Scripts/Combat/FleeAction.cs`
- [X] T008 [P] Remover `Assets/Scripts/Combat/InitiativeService.cs`
- [X] T009 [P] Remover `Assets/Scripts/Combat/CombatCameraController.cs`
- [X] T010 [P] Remover `Assets/Scripts/Combat/ICombatant.cs` (substituído por `IRealTimeCombatant`)
- [X] T011 [P] Remover `Assets/Scripts/Demo/CombatDemoController.cs` (substituído por `BattleArenaDemoController`)
- [X] T012 [P] Remover `Assets/Scripts/Demo/GridTileClickHandler.cs` (sem grid clicável)
- [X] T013 [P] Remover os testes obsoletos do combate por turnos: `Assets/Tests/EditMode/CombatTurnResourcesTests.cs`, `Assets/Tests/EditMode/FleeActionTests.cs`, `Assets/Tests/EditMode/InitiativeOrderTests.cs`, `Assets/Tests/PlayMode/CombatEncounterFlowTests.cs`

**Checkpoint**: Código de combate por turnos/grid removido; o projeto não compila ainda
(esperado — `CombatEncounter.cs`/`CombatOutcomeHandler.cs`/`NonPlayerCombatant.cs` ainda
referenciam os tipos removidos, corrigido na fase seguinte).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Novos tipos de dados/infraestrutura que a User Story 1 depende

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes desta fase estar completa

- [X] T014 Trocar o Universal Renderer 3D pelo URP **2D Renderer** — criar `Renderer2DData` em `Assets/Settings/GameRpgUrpRenderer2D.asset` e apontar o `UniversalRenderPipelineAsset` para ele — em `Assets/Editor/ProjectBootstrap.cs` (research.md, "Decision: URP 2D Renderer")
- [X] T015 [P] Implementar `IRealTimeCombatant` (substitui `ICombatant`: `PositionX` float, `ActionState`, sem `TurnResources`/`GridCoordinate`) em `Assets/Scripts/Combat/IRealTimeCombatant.cs`
- [X] T016 [P] Implementar `BattleArena` (limites `MinX`/`MaxX`, `Clamp(float)`, substitui `GridMap`) em `Assets/Scripts/Combat/BattleArena.cs`
- [X] T017 [P] Implementar `IDamageModifierRegistry` (`RegisterDamageModifier(IDamageModifier)`, extraído do antigo `ActionResolver`) em `Assets/Scripts/Combat/IDamageModifierRegistry.cs`
- [X] T018 [P] Implementar `CombatantActionState` (Pontos de Técnica, cooldowns por ação, ação pendente, canal de fuga — data-model.md) em `Assets/Scripts/Combat/CombatantActionState.cs`
- [X] T019 [P] Implementar `RealTimeActionDefinition` (`ScriptableObject`: `ActionId`, `Kind`, `Range`, `ExecutionTime`, `Cooldown`, `ResourceCost`, `RequiredCapabilityId`) em `Assets/Scripts/Combat/RealTimeActionDefinition.cs`
- [X] T020 Adaptar `Character` para implementar `IRealTimeCombatant` (`PositionX` float em vez de `GridCoordinate Position`, expõe `CombatantActionState`, remove `TurnResources`) em `Assets/Scripts/Characters/Character.cs` (depende de T015, T018)
- [X] T021 Adaptar `NonPlayerCombatant` para implementar `IRealTimeCombatant` (mesmas mudanças de T020) em `Assets/Scripts/Combat/NonPlayerCombatant.cs` (depende de T015, T018)
- [X] T022 Adaptar `CapabilityResolver.ApplyAcquiredCapabilities` para aceitar `IDamageModifierRegistry` em vez do antigo `ActionResolver` em `Assets/Scripts/Skills/CapabilityResolver.cs` (depende de T017)

**Checkpoint**: Novos tipos fundamentais prontos; `Character`/`NonPlayerCombatant` já
implementam `IRealTimeCombatant`. `CombatEncounter.cs`/`CombatOutcomeHandler.cs` ainda não
compilam — corrigido no início da User Story 1 (ver nota no topo deste arquivo).

---

## Phase 3: User Story 1 - Combate em tempo real controlando o personagem diretamente (Priority: P1) 🎯 MVP

**Goal**: O jogador controla diretamente seu personagem numa arena 2D side-view em tempo real —
movimento livre, ataque corpo a corpo/à distância, habilidade com tempo de conjuração
interruptível, inimigos autônomos, fuga por canal contínuo, câmera que segue com clamp nas
bordas.

**Independent Test**: Iniciar um encontro de combate, mover o personagem livremente pela arena,
conectar um ataque corpo a corpo, conjurar uma habilidade com tempo de conjuração observável, e
observar o(s) inimigo(s) agindo por conta própria — tudo sem qualquer prompt de "sua vez"/"fim
de turno" (spec.md, Acceptance Scenarios 1-7).

### Tests for User Story 1 (obrigatórios — Princípio III)

- [X] T023 [P] [US1] Teste EditMode: `BattleArena.Clamp` restringe posições aos limites (`MinX`/`MaxX`) em `Assets/Tests/EditMode/BattleArenaTests.cs`
- [X] T024 [P] [US1] Teste EditMode: `RealTimeActionExecutor` inicia, resolve e interrompe ações corretamente (contrato [realtime-action-contract.md](./contracts/realtime-action-contract.md), incluindo a interrupção de FR-009 e o gasto/recarga do recurso de FR-008) em `Assets/Tests/EditMode/RealTimeActionExecutorTests.cs`
- [X] T025 [P] [US1] Teste EditMode: `EnemyCombatAI` decide mover em direção ao alvo mais próximo ou atacar quando ao alcance, continuamente em `Assets/Tests/EditMode/EnemyCombatAITests.cs`
- [X] T026 [P] [US1] Teste EditMode: canal de `RealTimeFleeAction` só tenta a fuga após a duração mínima e reseta se interrompido (contrato [flee-channel-contract.md](./contracts/flee-channel-contract.md)) em `Assets/Tests/EditMode/RealTimeFleeActionTests.cs`
- [X] T027 [P] [US1] Teste PlayMode: fluxo completo de `CombatArenaEncounter` (vitória, derrota, fuga bem-sucedida) em `Assets/Tests/PlayMode/CombatArenaEncounterFlowTests.cs`

### Implementation for User Story 1

- [X] T028 [US1] Reescrever `Combat/CombatEncounter.cs` como `Combat/CombatArenaEncounter.cs` (sem iniciativa/turnos; `AdvanceTime(TimeSpan)`; participantes via `IRealTimeCombatant`; mesmos estados terminais) em `Assets/Scripts/Combat/CombatArenaEncounter.cs` (depende de T015, T020, T021)
- [X] T029 [US1] Adaptar `CombatOutcomeHandler` ao novo `CombatArenaEncounter`/`IRealTimeCombatant` em `Assets/Scripts/Combat/CombatOutcomeHandler.cs` (depende de T028)
- [X] T030 [US1] Implementar `RealTimeActionExecutor` (implementa `IDamageModifierRegistry`; contrato [realtime-action-contract.md](./contracts/realtime-action-contract.md)) em `Assets/Scripts/Combat/RealTimeActionExecutor.cs` (depende de T017, T018, T019, T028)
- [X] T031 [US1] Implementar `EnemyCombatAI` (decisão contínua via `Tick(TimeSpan)`, sem pathfinding) em `Assets/Scripts/Combat/EnemyCombatAI.cs` (depende de T030)
- [X] T032 [US1] Implementar `RealTimeFleeAction` (canal contínuo; contrato [flee-channel-contract.md](./contracts/flee-channel-contract.md)) em `Assets/Scripts/Combat/RealTimeFleeAction.cs` (depende de T028)
- [X] T033 [US1] Implementar `BoundedFollowCamera` (segue o alvo centralizado, clamp nos limites do mundo; contrato [camera-bounds-contract.md](./contracts/camera-bounds-contract.md)) em `Assets/Scripts/Demo/BoundedFollowCamera.cs`
- [X] T034 [US1] Implementar `BattleArenaDemoController` (substitui `CombatDemoController`: spawna jogador/inimigo como sprites 2D na arena, movimento horizontal via teclado, botões de ataque corpo a corpo/à distância/habilidade/fuga, barra de vida contínua, integra `CombatArenaEncounter`/`RealTimeActionExecutor`/`EnemyCombatAI`/`RealTimeFleeAction`/`BoundedFollowCamera`) em `Assets/Scripts/Demo/BattleArenaDemoController.cs` (depende de T028-T033)
- [X] T035 [US1] Conectar `BattleArenaDemoController` à cena de combate (câmera 2D ortográfica sem inclinação isométrica) via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T014, T034)
- [X] T036 [US1] Registrar conteúdo de exemplo de `RealTimeActionDefinition` (um ataque corpo a corpo, um ataque à distância exigindo capacidade adquirida, uma habilidade com tempo de conjuração e custo de recurso) via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T019, T035)

**Checkpoint**: Combate em tempo real 2D jogável de ponta a ponta, de forma independente — o
projeto volta a compilar por completo a partir daqui.

---

## Phase 4: User Story 2 - Mundo de exploração convertido para 2D side-view (Priority: P2)

**Goal**: A Exploração usa a mesma perspectiva 2D side-view do combate (câmera ortográfica sem
inclinação isométrica), com a mesma câmera de clamp nas bordas (`BoundedFollowCamera`).

**Independent Test**: Abrir a demo de Exploração e confirmar câmera/cenário 2D side-view,
movimento livre do personagem, e clamp da câmera perto das extremidades do mapa; entrar em
combate e confirmar a transição para a arena 2D side-view da User Story 1.

### Implementation for User Story 2

- [X] T037 [US2] Remover o billboard 3D (rotação da câmera aplicada ao sprite) e adaptar `ExplorationCharacterController` para a câmera 2D side-view pura em `Assets/Scripts/Demo/ExplorationCharacterController.cs` (depende de T014)
- [X] T038 [US2] Adicionar `BoundedFollowCamera` à câmera da cena de Exploração, com os limites do mapa configurados via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T033, T037)
- [X] T039 [US2] [P] Verificar/ajustar a transição de cena da Criação de Personagem para a Exploração (feature 003) com a nova câmera 2D, sem alterar os dados do personagem transferidos em `Assets/Scripts/UI/CharacterCreationUI.cs`

**Checkpoint**: Exploração e combate compartilham a mesma perspectiva 2D side-view e o mesmo
comportamento de câmera — testável de forma independente.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Confirmar ausência de regressão e validação final de ponta a ponta

- [X] T040 [P] Rodar a suíte completa de testes EditMode/PlayMode das features 001-003 (atributos, habilidades, sobrevivência, reputação/economia, criação de personagem) e confirmar que 100% continuam passando sem alteração de comportamento (SC-004)
- [ ] T041 Executar a validação completa de [quickstart.md](./quickstart.md) (testes automatizados + os 2 blocos de validação manual por história)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — remoções puras, podem começar imediatamente
- **Foundational (Phase 2)**: Depende da conclusão do Setup (os novos tipos substituem os
  removidos) — BLOQUEIA a User Story 1
- **User Story 1 (Phase 3)**: Depende da conclusão da Foundational; é quem restaura a
  compilação do projeto (ver nota no topo deste arquivo)
- **User Story 2 (Phase 4)**: Depende de T014 (renderer 2D) e T033 (`BoundedFollowCamera`, User
  Story 1) — não pode começar antes da User Story 1 terminar, apesar de ser uma história
  separada, porque reaproveita o componente de câmera construído nela
- **Polish (Phase 5)**: Depende de todas as histórias de usuário desejadas estarem completas

### User Story Dependencies

- **US1 (P1 — Combate em tempo real)**: Núcleo da feature; sem dependência de US2.
- **US2 (P2 — Exploração 2D side-view)**: Depende tecnicamente de artefatos da US1
  (`BoundedFollowCamera`, renderer 2D) — não é "independente" no sentido de poder ser
  implementada antes da US1, mas é testável de forma independente uma vez que a US1 esteja
  pronta (a Exploração não depende de nenhuma lógica de combate em si).

### Within Each User Story

- Testes (quando aplicável) DEVEM ser escritos e falhar antes da implementação
- Tipos de dados/estado antes de serviços (`RealTimeActionExecutor`, `EnemyCombatAI`,
  `RealTimeFleeAction` dependem de `CombatArenaEncounter`/`CombatantActionState` já existirem)
- Serviços antes de integração (`BattleArenaDemoController` por último, integra tudo)
- Implementação central antes da integração com `ProjectBootstrap`

### Parallel Opportunities

- Todas as remoções do Setup (T001-T013) podem rodar em paralelo entre si
- T015-T019 (novos tipos fundamentais, arquivos distintos) podem rodar em paralelo
- T023-T027 (testes da User Story 1, arquivos distintos) podem rodar em paralelo
- T040 (regressão) pode rodar em paralelo a outras tarefas de Polish, se houver mais de uma

---

## Parallel Example: Setup

```bash
# Rodar as remoções independentes do Setup juntas:
Task: "Remover Assets/Scripts/Combat/Grid/GridMap.cs"
Task: "Remover Assets/Scripts/Combat/GridPathfinding.cs"
Task: "Remover Assets/Scripts/Combat/TurnResources.cs"
Task: "Remover Assets/Scripts/Combat/ICombatant.cs"
# ...demais remoções T001-T013
```

## Parallel Example: Foundational

```bash
Task: "Implementar IRealTimeCombatant em Assets/Scripts/Combat/IRealTimeCombatant.cs"
Task: "Implementar BattleArena em Assets/Scripts/Combat/BattleArena.cs"
Task: "Implementar IDamageModifierRegistry em Assets/Scripts/Combat/IDamageModifierRegistry.cs"
Task: "Implementar CombatantActionState em Assets/Scripts/Combat/CombatantActionState.cs"
Task: "Implementar RealTimeActionDefinition em Assets/Scripts/Combat/RealTimeActionDefinition.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar a Fase 1: Setup (remoção do código por turnos/grid)
2. Completar a Fase 2: Foundational (CRÍTICO — bloqueia a User Story 1)
3. Completar a Fase 3: User Story 1 (combate em tempo real) — só agora o projeto volta a
   compilar por completo
4. **PARAR e VALIDAR**: testar a User Story 1 de forma independente via
   [quickstart.md](./quickstart.md#validação-manual--combate-em-tempo-real-user-story-1--p1)
5. Demonstrar/avaliar se pronto

### Incremental Delivery

1. Completar Setup + Foundational → tipos fundamentais prontos (projeto momentaneamente sem
   compilar, por design — ver nota no topo)
2. Adicionar US1 (Combate em tempo real) → projeto volta a compilar → testar independentemente
   → demo (MVP!)
3. Adicionar US2 (Exploração 2D side-view) → testar independentemente → demo
4. Completar a Fase 5 (Polish) → validação final via quickstart.md

### Parallel Team Strategy

Dado que US2 depende tecnicamente de artefatos da US1 (câmera, renderer 2D), esta feature não se
presta bem a paralelismo de equipe entre histórias — a sequência Setup → Foundational → US1 →
US2 é essencialmente linear. Dentro de cada fase, as tarefas marcadas [P] podem ser divididas
entre desenvolvedores normalmente.

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências pendentes
- O rótulo [Story] mapeia a tarefa para a história de usuário correspondente (rastreabilidade)
- Verificar que os testes falham antes de implementar (TDD para os sistemas centrais, por
  exigência do Princípio III da constituição)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a história independentemente
- Não espere o projeto compilar entre o final do Setup e o final da User Story 1 (nota no topo
  deste arquivo) — é o único ponto desta feature onde isso é esperado
