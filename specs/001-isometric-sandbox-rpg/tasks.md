---
description: "Task list template for feature implementation"
---

# Tasks: RPG Sandbox com Árvore de Habilidades, Combate Tático e Mundo Reativo

**Input**: Design documents from `/specs/001-isometric-sandbox-rpg/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Incluídos e OBRIGATÓRIOS para os sistemas centrais (combate, fórmulas, árvore de
habilidades, save/load, simulação de economia/reputação), por exigência do Princípio III
(NON-NEGOTIABLE) da constituição do projeto. Renderização/UI pura não exige testes automatizados.

**Organization**: Tarefas agrupadas por história de usuário (spec.md) para permitir
implementação e teste independentes de cada uma.

> **Nota de regeneração (2026-08-25, pós `/speckit-analyze`)**: esta versão corrige os achados
> do relatório de análise: **C2** — critério de performance por história adicionado a cada
> checkpoint (Princípio IV); **G1** — mecânica de fuga em combate agora tem tarefas próprias
> (T018 teste, T024 implementação); **U1** — parâmetros de balanceamento centralizados em uma
> nova tarefa Foundational (T014); **U2** — T056 agora deixa explícito que transportar um
> recurso atualiza o estoque da comunidade receptora, não apenas registra a escolha. (O achado
> **C1** — constituição desatualizada — foi corrigido diretamente em
> `.specify/memory/constitution.md`, agora v1.0.1.) Toda a numeração de tarefas foi recalculada
> em relação à geração anterior.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: A qual história de usuário a tarefa pertence (US1, US2, US3, US4)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Projeto único Unity (ver [plan.md](./plan.md#project-structure)): `Assets/Scripts/`,
`Assets/Data/`, `Assets/Scenes/`, `Assets/Tests/EditMode/`, `Assets/Tests/PlayMode/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Inicialização do projeto Unity e estrutura básica

- [X] T001 Criar a estrutura de pastas do projeto conforme `plan.md`: `Assets/Scripts/{Core,Combat,Characters,Skills,World,NPCs,UI}`, `Assets/Data`, `Assets/Scenes`, `Assets/Prefabs`, `Assets/Art`, `Assets/Tests/EditMode`, `Assets/Tests/PlayMode`
- [X] T002 Configurar `Packages/manifest.json` com as dependências do projeto: Universal Render Pipeline (URP), Cinemachine, Input System, Unity Test Framework
- [X] T003 [P] Configurar o URP Asset e uma câmera ortográfica isométrica base em `Assets/Scenes/Exploration.unity` (câmera/pipeline apenas — sem lógica de jogo)
- [X] T004 [P] Criar assembly definitions (`.asmdef`) separando `Assets/Scripts` (runtime) de `Assets/Tests/EditMode` e `Assets/Tests/PlayMode`, garantindo que os testes referenciem o assembly de runtime

**Checkpoint**: Projeto Unity abre, builda vazio, e a suíte de testes (vazia) roda sem erros.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura central que TODAS as histórias de usuário dependem

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes desta fase estar completa

- [X] T005 Implementar as estruturas lógicas de grid (`GridCell`, `GridMap`) desacopladas de renderização em `Assets/Scripts/Combat/Grid/GridMap.cs` (ver [research.md](./research.md#decision-movimento-e-combate-em-grade))
- [X] T006 [P] Implementar `SkillNodeDefinition` (ScriptableObject) em `Assets/Scripts/Skills/SkillNodeDefinition.cs` conforme [contracts/skill-node-data-contract.md](./contracts/skill-node-data-contract.md)
- [X] T007 [P] Implementar `ResourceDefinition` (ScriptableObject) em `Assets/Scripts/World/ResourceDefinition.cs` conforme [data-model.md](./data-model.md#resource)
- [X] T008 [P] Implementar `NpcDefinition` (ScriptableObject) em `Assets/Scripts/NPCs/NpcDefinition.cs` conforme [data-model.md](./data-model.md#npc)
- [X] T009 [P] Implementar `CommunityDefinition` (ScriptableObject) em `Assets/Scripts/World/CommunityDefinition.cs` conforme [data-model.md](./data-model.md#community--faction-comunidadefacção--inclui-vilas)
- [X] T010 Implementar o modelo runtime `Character` (atributos, vida, recursos de turno, fome, sanidade, pontos/nós de habilidade adquiridos, inventário) em `Assets/Scripts/Characters/Character.cs` (depende de T006)
- [X] T011 Implementar o serviço `WorldClock` de tempo simulado in-game em `Assets/Scripts/Core/WorldClock.cs`, usado por fome/sanidade (US3) e simulação de economia (US4)
- [X] T012 Implementar as classes de esquema `SaveData` e um serviço de serialização/desserialização JSON esqueleto em `Assets/Scripts/Core/SaveSystem.cs` conforme [contracts/save-data-contract.md](./contracts/save-data-contract.md)
- [X] T013 Implementar validação de resolução de referências de conteúdo (skill nodes, recursos, NPCs, comunidades) com erro explícito em falha, em `Assets/Scripts/Core/ContentValidation.cs`
- [X] T014 [P] Implementar `BalancingConfig` (ScriptableObject/constantes centralizadas) definindo os parâmetros numéricos de balanceamento hoje indefinidos na spec: limiares de alerta/crítico de fome e de sanidade, limiar e período de sustentação de recursos essenciais de uma comunidade (usado para disparar perda de população), e a duração do "período de jogo definido" usado em SC-003/SC-004/SC-005, em `Assets/Scripts/Core/BalancingConfig.cs` — consumido por T040 (Hunger), T041 (Sanity) e T052 (VillageEconomySimulationService)

**Checkpoint**: Fundação pronta — as histórias de usuário podem começar (em paralelo, se houver equipe).

---

## Phase 3: User Story 1 - Combate tático por turnos em visão isométrica (Priority: P1) 🎯 MVP

**Goal**: Permitir que o jogador resolva um encontro hostil através de combate por turnos em
grade sobre câmera isométrica, com recursos de ação/movimento/bônus corretamente geridos.

**Independent Test**: Colocar o jogador em um encontro contra 1+ inimigos em um mapa de grade
isométrico e verificar que o combate pode ser vencido, perdido ou evitado (fuga) inteiramente
através de decisões em turnos.

### Tests for User Story 1 (obrigatórios — Princípio III)

- [X] T015 [P] [US1] Teste EditMode para consumo/reset de recursos de turno (movimento/ação/ação bônus) em `Assets/Tests/EditMode/CombatTurnResourcesTests.cs`
- [X] T016 [P] [US1] Teste EditMode para cálculo da ordem de iniciativa em `Assets/Tests/EditMode/InitiativeOrderTests.cs`
- [X] T017 [P] [US1] Teste PlayMode para o fluxo completo de um encontro de combate (início → vitória / fuga / derrota) em `Assets/Tests/PlayMode/CombatEncounterFlowTests.cs`
- [X] T018 [P] [US1] Teste EditMode para a ação de fuga: sucesso/falha conforme condição de posição/atributos, e transição correta para o estado `PlayerFled` apenas em caso de sucesso (FR-003) em `Assets/Tests/EditMode/FleeActionTests.cs`

### Implementation for User Story 1

- [X] T019 [P] [US1] Implementar a máquina de estados `CombatEncounter` (`NotStarted`/`InProgress`/`WonByPlayer`/`PlayerFled`/`PlayerDefeated`) em `Assets/Scripts/Combat/CombatEncounter.cs`
- [X] T020 [P] [US1] Implementar `InitiativeService` (cálculo da ordem de turnos) em `Assets/Scripts/Combat/InitiativeService.cs`
- [X] T021 [US1] Implementar `TurnResourceManager` (movimento/ação/ação bônus por participante) em `Assets/Scripts/Combat/TurnResourceManager.cs` (depende de T010, T019)
- [X] T022 [US1] Implementar movimentação/pathfinding em grade para combate em `Assets/Scripts/Combat/GridPathfinding.cs` (depende de T005)
- [X] T023 [US1] Implementar `ActionResolver` (resolução de ataque/uso de habilidade contra alvo) em `Assets/Scripts/Combat/ActionResolver.cs` (depende de T021)
- [X] T024 [US1] Implementar a ação de fuga (`FleeAction`: calcula sucesso/falha a partir de posição no grid/atributos, consome o recurso de turno correspondente, e aciona a transição de `CombatEncounter` para `PlayerFled` apenas em caso de sucesso — FR-003) em `Assets/Scripts/Combat/FleeAction.cs` (depende de T019, T021)
- [X] T025 [US1] Implementar comportamento básico de IA de inimigos no turno em `Assets/Scripts/Combat/EnemyAI.cs` (depende de T021, T023)
- [X] T026 [US1] Implementar `CombatOutcomeHandler` (recompensas na vitória, checkpoint na derrota, saída na fuga; expõe o evento de dano/morte de cada combatente, incluindo os com `linkedNpcId`, para consumo pela US4 — ver T057) em `Assets/Scripts/Combat/CombatOutcomeHandler.cs` (depende de T019)
- [X] T027 [US1] Configurar a câmera de combate isométrica (Cinemachine virtual camera) em `Assets/Scripts/Combat/CombatCameraController.cs`
- [X] T028 [US1] Montar a cena de teste de encontro de combate em `Assets/Scenes/CombatEncounterTest.unity`

**Checkpoint**: Um encontro de combate completo (incluindo fuga) é jogável e testável de forma
independente. Validar que o orçamento de performance de [plan.md](./plan.md#technical-context)
(60 fps, <100ms de input) é respeitado no loop de combate antes de considerar esta história
concluída (Princípio IV).

---

## Phase 4: User Story 2 - Árvore de habilidades sem classes fixas (Priority: P2)

**Goal**: Permitir que o jogador invista pontos de habilidade nas trilhas Combatente, Arcanista
e nós híbridos, refletindo essas escolhas como capacidades utilizáveis, e redistribua (respec)
esses pontos livremente a qualquer momento (FR-018).

**Independent Test**: Criar um personagem, investir pontos em nós de diferentes trilhas e
verificar que as habilidades escolhidas ficam disponíveis para uso, refletindo a combinação
específica escolhida; em seguida, desfazer parte do investimento via respec e confirmar que os
pontos voltam a ficar disponíveis para reinvestimento.

### Tests for User Story 2 (obrigatórios — Princípio III)

- [X] T029 [P] [US2] Teste EditMode para validação de pré-requisitos de nós de habilidade (incluindo exigência de ambas as trilhas para nós híbridos) em `Assets/Tests/EditMode/SkillNodePrerequisiteTests.cs`
- [X] T030 [P] [US2] Teste EditMode que impede aquisição duplicada de uma habilidade em `Assets/Tests/EditMode/SkillAcquisitionTests.cs`
- [X] T031 [P] [US2] Teste EditMode de validação de conteúdo: ausência de ciclos no grafo de pré-requisitos da árvore de habilidades em `Assets/Tests/EditMode/SkillGraphValidationTests.cs`
- [X] T032 [P] [US2] Teste EditMode para respec: desalocar um nó adquirido devolve seus pontos como disponíveis, permite reinvestimento livre, e remove em cascata qualquer nó dependente que ficaria com pré-requisito violado (FR-018) em `Assets/Tests/EditMode/SkillRespecTests.cs`

### Implementation for User Story 2

- [X] T033 [P] [US2] Implementar `SkillTreeService` (disponibilidade de nós, investimento, checagem de pré-requisitos, e respec com remoção em cascata de dependentes) em `Assets/Scripts/Skills/SkillTreeService.cs` (depende de T006, T010)
- [X] T034 [US2] Implementar `CapabilityResolver` (resolve `grantedCapabilityId` em ação/passiva utilizável) em `Assets/Scripts/Skills/CapabilityResolver.cs` (depende de T033)
- [X] T035 [US2] Integrar capacidades resolvidas como opções de ação em `Assets/Scripts/Combat/ActionResolver.cs` (estende US1; depende de T023, T034)
- [X] T036 [US2] Implementar a UI da árvore de habilidades (trilhas, nós bloqueados/disponíveis, investimento e controle de respec) em `Assets/Scripts/UI/SkillTreeUI.cs`
- [X] T037 [P] [US2] Criar o conjunto inicial de conteúdo de nós de habilidade (Combatente, Arcanista, Híbridos) como assets ScriptableObject em `Assets/Data/Skills/`

**Checkpoint**: A árvore de habilidades, incluindo respec, é jogável e testável de forma
independente; builds diferentes produzem capacidades de combate observavelmente distintas.
Validar que a UI da árvore de habilidades (abertura, investimento, respec) responde dentro do
orçamento de input de [plan.md](./plan.md#technical-context) (<100ms) antes de concluir esta
história (Princípio IV).

---

## Phase 5: User Story 3 - Sobrevivência: fome e sanidade (Priority: P3)

**Goal**: Aplicar penalidades progressivas quando fome/sanidade não são geridas — cumulativamente
quando ambas estão críticas ao mesmo tempo (FR-021) —, usando os limiares centralizados em
`BalancingConfig` (T014), e permitir sua restauração através de ações do jogador.

**Independent Test**: Deixar o personagem sem se alimentar ou exposto a eventos perturbadores
por um período definido e verificar que os indicadores degradam e aplicam penalidades
mensuráveis, revertidas ao satisfazer a necessidade; e que, se ambos os indicadores estiverem
críticos simultaneamente, as penalidades de ambos aparecem somadas.

### Tests for User Story 3 (obrigatórios — Princípio III)

- [X] T038 [P] [US3] Teste EditMode para aplicação/remoção de penalidades por limiar de fome (usando os limiares de `BalancingConfig`) em `Assets/Tests/EditMode/HungerSystemTests.cs`
- [X] T039 [P] [US3] Teste EditMode para aplicação/remoção de efeitos por limiar de sanidade, incluindo o caso de fome e sanidade críticas simultâneas (penalidades cumulativas, sem teto — FR-021) em `Assets/Tests/EditMode/SanitySystemTests.cs`

### Implementation for User Story 3

- [X] T040 [P] [US3] Implementar `HungerSystem` (degradação ao longo do tempo simulado, penalidades por limiar lidas de `BalancingConfig`) em `Assets/Scripts/Characters/HungerSystem.cs` (depende de T010, T011, T014)
- [X] T041 [P] [US3] Implementar `SanitySystem` (redução por eventos, efeitos por limiar lidos de `BalancingConfig`, recuperação) em `Assets/Scripts/Characters/SanitySystem.cs` (depende de T010, T014)
- [X] T042 [US3] Implementar a ação de consumir alimento (restaura fome) em `Assets/Scripts/Characters/FoodConsumptionAction.cs` (depende de T040)
- [X] T043 [US3] Implementar ações de recuperação de sanidade (descanso/item/ambiente seguro) em `Assets/Scripts/Characters/SanityRecoveryAction.cs` (depende de T041)
- [X] T044 [US3] Aplicar modificadores de penalidade de fome/sanidade de forma cumulativa ao cálculo de atributos em combate em `Assets/Scripts/Combat/ActionResolver.cs` (estende US1; depende de T023, T040, T041)
- [X] T045 [US3] Implementar a UI de status de sobrevivência (indicadores de fome/sanidade, ícones de penalidade ativa e cumulativa) em `Assets/Scripts/UI/SurvivalStatusUI.cs`

**Checkpoint**: As mecânicas de sobrevivência, incluindo penalidades cumulativas, são jogáveis e
testáveis de forma independente. Validar que a degradação/aplicação de penalidades de
fome/sanidade não introduz quedas de frame perceptíveis (orçamento de 60 fps de
[plan.md](./plan.md#technical-context)) antes de concluir esta história (Princípio IV).

---

## Phase 6: User Story 4 - Reputação e mundo reativo (economia e NPCs) (Priority: P4)

**Goal**: Propagar escolhas de impacto do jogador (salvar/não salvar NPCs — inclusive em combate
forçado, FR-022 —, transportar recursos) para uma reputação totalmente independente por
comunidade (FR-020) e para o estado populacional/econômico das vilas, incluindo seu colapso
permanente ao atingir 0% de população (FR-019).

**Independent Test**: Realizar uma escolha de impacto e verificar, após um período de tempo de
jogo definido, que a reputação do jogador (isolada por comunidade) e o estado da vila (população,
recursos) refletem a consequência; verificar também que uma vila zerada permanece inativa mesmo
após reposição de recursos, e que ferir um NPC aliado em combate forçado reduz a reputação como
uma escolha deliberada.

### Tests for User Story 4 (obrigatórios — Princípio III)

- [X] T046 [P] [US4] Teste EditMode para o tick de simulação de economia de vila (consumo, perda de população, piso de recursos, usando os limiares de `BalancingConfig`) em `Assets/Tests/EditMode/VillageEconomySimulationTests.cs` conforme [contracts/village-economy-simulation-contract.md](./contracts/village-economy-simulation-contract.md)
- [X] T047 [P] [US4] Teste EditMode para o colapso permanente de vila: população chega a zero → `isPermanentlyInactive = true` → repor `essentialResourceStock` depois NÃO reverte o estado nem retoma a simulação daquela comunidade (FR-019) em `Assets/Tests/EditMode/VillagePermanentInactivationTests.cs`
- [X] T048 [P] [US4] Teste EditMode para aplicação de delta de reputação a partir de um `ImpactfulChoice`, incluindo a garantia de que a reputação de uma comunidade não afeta a de nenhuma outra (FR-020) em `Assets/Tests/EditMode/ReputationServiceTests.cs`
- [X] T049 [P] [US4] Teste PlayMode ponta a ponta: "remover recurso → declínio populacional → repor recurso → declínio interrompido" em `Assets/Tests/PlayMode/VillageConsequenceFlowTests.cs`
- [X] T050 [P] [US4] Teste EditMode: ferir/matar um combatente com `linkedNpcId` em um encontro forçado gera um `ImpactfulChoice` do tipo `AbandonOrHarmNpc` com o mesmo efeito de reputação de uma escolha deliberada (FR-022) em `Assets/Tests/EditMode/ForcedCombatReputationTests.cs`

### Implementation for User Story 4

- [X] T051 [P] [US4] Implementar o estado runtime `Community` (estoque de recursos, população, flag `isPermanentlyInactive`) em `Assets/Scripts/World/Community.cs` (depende de T007, T009)
- [X] T052 [US4] Implementar `VillageEconomySimulationService` conforme o contrato, usando os limiares de `BalancingConfig` e incluindo a transição irreversível para `isPermanentlyInactive` ao atingir população zero (FR-019) em `Assets/Scripts/World/VillageEconomySimulationService.cs` (depende de T011, T014, T051)
- [X] T053 [P] [US4] Implementar `ImpactfulChoiceLog` (registro de escolhas de impacto) em `Assets/Scripts/World/ImpactfulChoiceLog.cs`
- [X] T054 [US4] Implementar `ReputationService` (aplica deltas de `ImpactfulChoice` exclusivamente à reputação da `Community` alvo, sem propagação para outras comunidades — FR-020) em `Assets/Scripts/World/ReputationService.cs` (depende de T051, T053)
- [X] T055 [US4] Implementar `NpcInteractionGate` (disponibilidade de diálogo/missão/comércio por reputação) em `Assets/Scripts/NPCs/NpcInteractionGate.cs` (depende de T008, T054)
- [X] T056 [US4] Implementar `PlayerChoiceActions`, conectando as ações do jogador "salvar/abandonar NPC" e "transportar recurso" ao `ImpactfulChoiceLog`; a ação de transportar recurso DEVE, além de registrar a escolha, incrementar efetivamente `Community.essentialResourceStock` da comunidade receptora na quantidade transportada (FR-015) em `Assets/Scripts/World/PlayerChoiceActions.cs` (depende de T051, T053)
- [X] T057 [US4] Implementar `ForcedCombatReputationBridge`, escutando o evento de dano/morte exposto por `CombatOutcomeHandler` (T026) para combatentes com `linkedNpcId` e registrando automaticamente um `ImpactfulChoice` do tipo `AbandonOrHarmNpc` (FR-022) em `Assets/Scripts/World/ForcedCombatReputationBridge.cs` (depende de T026, T053)
- [X] T058 [US4] Implementar a UI de retorno de reputação/economia (status da vila — incluindo indicação visual de colapso permanente — e indicador de reputação) em `Assets/Scripts/UI/ReputationEconomyUI.cs`
- [X] T059 [P] [US4] Criar o conjunto inicial de conteúdo de vilas/NPCs/recursos para a região do MVP (2–4 vilas) em `Assets/Data/World/`

**Checkpoint**: Todas as quatro histórias de usuário funcionam de forma independente e
integrada. Validar que o tick de simulação de economia/reputação (T052, T054) não introduz
quedas de frame perceptíveis ao avançar o tempo simulado, conforme o orçamento de
[plan.md](./plan.md#technical-context) (Princípio IV).

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Melhorias que afetam múltiplas histórias de usuário

- [X] T060 Completar a integração de save/load cobrindo Character (incl. respec), Skills, Survival e World/Reputation (incl. `isPermanentlyInactive`) conforme [contracts/save-data-contract.md](./contracts/save-data-contract.md) em `Assets/Scripts/Core/SaveSystem.cs` (estende T012)
- [X] T061 [P] Teste PlayMode para o ciclo completo salvar → carregar e sua idempotência em `Assets/Tests/PlayMode/SaveLoadRoundTripTests.cs`
- [ ] T062 [P] Validar o orçamento de performance (60 fps, <100ms de input) de ponta a ponta, com todas as histórias integradas simultaneamente (exploração, combate, árvore de habilidades, sobrevivência e simulação de mundo rodando juntas), conforme [plan.md](./plan.md#technical-context) — consolida e reconfirma as validações incrementais já feitas em cada checkpoint de história (Princípio IV)
- [X] T063 Executar a validação completa de [quickstart.md](./quickstart.md) (testes automatizados + os 4 blocos de validação manual por história)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: Depende da conclusão do Setup — BLOQUEIA todas as histórias de usuário
- **User Stories (Phase 3-6)**: Todas dependem da conclusão da fase Foundational
  - Podem prosseguir em paralelo (se houver equipe) ou sequencialmente em ordem de prioridade (P1 → P2 → P3 → P4)
- **Polish (Phase 7)**: Depende de todas as histórias de usuário desejadas estarem completas

### User Story Dependencies

- **US1 (P1 – Combate)**: Pode começar após a Fase Foundational. Sem dependência de outras histórias. Expõe em T026 o evento de dano/morte por combatente, consumido posteriormente por US4 (T057).
- **US2 (P2 – Árvore de Habilidades)**: Pode começar após a Fase Foundational; integra-se com US1 (T035 estende `ActionResolver`), mas é testável de forma independente, incluindo o fluxo de respec.
- **US3 (P3 – Sobrevivência)**: Pode começar após a Fase Foundational (depende também de T014 `BalancingConfig`); integra-se com US1 (T044 estende `ActionResolver`), mas os sistemas de fome/sanidade em si são testáveis isoladamente.
- **US4 (P4 – Reputação/Economia)**: Pode começar após a Fase Foundational (depende também de T014 `BalancingConfig`); seus serviços centrais (`VillageEconomySimulationService`, `ReputationService`) são testáveis de forma independente, mas T057 (combate forçado → reputação) depende de T026 da US1 já existir.

### Within Each User Story

- Testes (quando aplicável) DEVEM ser escritos e falhar antes da implementação
- Modelos/definições antes de serviços
- Serviços antes de UI/integração
- Implementação central antes de integração cruzada com outras histórias

### Parallel Opportunities

- Todas as tarefas de Setup marcadas [P] podem rodar em paralelo
- Todas as tarefas Foundational marcadas [P] podem rodar em paralelo (T006-T009, T014)
- Após a Fase Foundational, US1-US4 podem começar em paralelo (se houver capacidade de equipe), exceto T057 (US4) que depende de T026 (US1)
- Todos os testes de uma história marcados [P] podem rodar em paralelo
- Definições/modelos dentro de uma história marcados [P] podem rodar em paralelo

---

## Parallel Example: User Story 1

```bash
# Rodar todos os testes da User Story 1 juntos:
Task: "Teste EditMode para consumo/reset de recursos de turno em Assets/Tests/EditMode/CombatTurnResourcesTests.cs"
Task: "Teste EditMode para cálculo da ordem de iniciativa em Assets/Tests/EditMode/InitiativeOrderTests.cs"
Task: "Teste PlayMode para o fluxo completo de um encontro de combate em Assets/Tests/PlayMode/CombatEncounterFlowTests.cs"
Task: "Teste EditMode para a ação de fuga em Assets/Tests/EditMode/FleeActionTests.cs"

# Rodar as implementações independentes da User Story 1 juntas:
Task: "Implementar a máquina de estados CombatEncounter em Assets/Scripts/Combat/CombatEncounter.cs"
Task: "Implementar InitiativeService em Assets/Scripts/Combat/InitiativeService.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar a Fase 1: Setup
2. Completar a Fase 2: Foundational (CRÍTICO — bloqueia todas as histórias)
3. Completar a Fase 3: User Story 1 (Combate)
4. **PARAR e VALIDAR**: testar a User Story 1 de forma independente via [quickstart.md](./quickstart.md#validação-manual--combate-user-story-1--p1)
5. Demonstrar/avaliar se pronto

### Incremental Delivery

1. Completar Setup + Foundational → fundação pronta
2. Adicionar US1 (Combate, incl. fuga) → testar independentemente → demo (MVP!)
3. Adicionar US2 (Árvore de Habilidades, incl. respec) → testar independentemente → demo
4. Adicionar US3 (Sobrevivência, incl. penalidades cumulativas) → testar independentemente → demo
5. Adicionar US4 (Reputação/Economia, incl. colapso permanente e combate forçado) → testar independentemente → demo
6. Completar a Fase 7 (Polish) → validação final via quickstart.md

### Parallel Team Strategy

Com múltiplos desenvolvedores:

1. Equipe completa Setup + Foundational em conjunto
2. Após a Fase Foundational:
   - Dev A: User Story 1 (Combate)
   - Dev B: User Story 2 (Árvore de Habilidades)
   - Dev C: User Story 3 (Sobrevivência)
   - Dev D: User Story 4 (Reputação/Economia) — inicia T057 apenas quando T026 (Dev A) estiver pronta
3. Histórias se integram nos pontos definidos (T035, T044, T057) ao final de cada incremento

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências pendentes
- O rótulo [Story] mapeia a tarefa para a história de usuário correspondente (rastreabilidade)
- Cada história de usuário deve ser completável e testável de forma independente
- Verificar que os testes falham antes de implementar (TDD para os sistemas centrais, por exigência do Princípio III da constituição)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a história independentemente (incluindo o critério de performance do Princípio IV, anotado em cada checkpoint acima)
- Evitar: tarefas vagas, conflitos no mesmo arquivo, dependências entre histórias que quebrem a independência
