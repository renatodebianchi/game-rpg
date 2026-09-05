---
description: "Task list template for feature implementation"
---

# Tasks: Cena de Teste de Mobilidade

**Input**: Design documents from `/specs/005-mobility-test-scene/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Incluídos e OBRIGATÓRIOS para a lógica central (`PlatformerMovementState`: pulo,
pulo duplo, parede — incluindo a velocidade de deslizar, não só a condição —, e o acúmulo/
liberação de energia, incluindo sua guarda interna de chão+agachado), por exigência do
Princípio III (NON-NEGOTIABLE) da constituição do projeto. A física do `Rigidbody2D`, os
raycasts, e a troca de sprite são apresentação pura e ficam isentas, mesmo padrão já adotado
pelas demos das features 001-004 — mas **nenhuma regra de jogo** pode viver só nelas (ver nota
abaixo, resultado do `/speckit-analyze`).

**Organization**: Tarefas agrupadas por história de usuário (spec.md). Esta feature adiciona uma
cena isolada nova (FR-014) — não modifica `Combat`/`Demo` de Exploração/Combate existentes.

**⚠️ Nota de licenciamento/download**: T001 envolve baixar um arquivo de terceiro (Anokolisa,
itch.io). O download exige confirmação explícita do usuário (nome do arquivo, fonte, tamanho)
antes de ser realizado — não apenas a aprovação geral já dada na spec. Diferente do CDN estático
do Kenney.nl usado nas features anteriores, o itch.io pode exigir navegar até a página do pacote
em vez de um link direto por linha de comando (research.md). O pacote não é CC0 formal — é
gratuito para uso comercial sob uma licença customizada do autor (research.md).

**⚠️ Nota de remediação (`/speckit-analyze`)**: a versão anterior deste arquivo colocava a
guarda de "abaixar só funciona no chão" (FR-008) e o valor da velocidade reduzida ao deslizar na
parede (FR-006) inteiramente no `MonoBehaviour`, sem cobertura de teste — um risco de violação do
Princípio III (apresentação não pode conter lógica de jogo). `PlatformerMovementState` agora
expõe `IsCrouching`, `GetFallSpeedMultiplier()`, e `AdvanceCharge` com guarda interna; as tarefas
abaixo já refletem esse desenho corrigido.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: A qual história de usuário a tarefa pertence (US1, US2, US3)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Mesmo projeto Unity único das features 001-004 (ver [plan.md](./plan.md#project-structure)):
`Assets/Scripts/Mobility/`, `Assets/Art/Platformer/`, `Assets/Scenes/`, `Assets/Editor/`,
`Assets/Tests/EditMode/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Obter e importar o pacote de assets de terceiros que a feature depende.

- [X] T001 Confirmar com o usuário e baixar o pacote Anokolisa "Legacy Fantasy – High Forest" (itch.io, gratuito, https://anokolisa.itch.io/sidescroller-pixelart-sprites-asset-pack-forest-16x16); importar as poses do personagem (Run/Idle/Attack/Start Jump/Air Jump/End Jump), o tileset do cenário (Forest/Ruins/Lake/Cave Tiles), e o fundo de duas camadas em `Assets/Art/Platformer/`
- [X] T002 [P] Criar `Assets/Art/Platformer/CREDITS.md` documentando fonte (anokolisa.itch.io), pacote e licença (gratuito para uso comercial, licença customizada do autor) do asset importado em T001 (FR-013)

**Checkpoint**: Assets de terceiros importados e documentados; prontos para uso pelo código.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: A máquina de decisão de movimentação e os componentes de apresentação que as três
histórias de usuário dependem

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes desta fase estar completa

- [X] T003 [P] Implementar `PlatformerMovementState` — pulo do chão/aéreo, contato de parede, `IsWallSliding`/`GetFallSpeedMultiplier()`, `IsCrouching`/`AdvanceCharge` com guarda interna de chão+agachado, e `ReleaseCharge` ([contracts/movement-state-contract.md](./contracts/movement-state-contract.md) e [contracts/charge-jump-contract.md](./contracts/charge-jump-contract.md)) em `Assets/Scripts/Mobility/PlatformerMovementState.cs`
- [X] T004 [P] Implementar `SpriteFlipbookAnimator` (troca de `SpriteRenderer.sprite` por estado, sem `Animator Controller` — research.md) em `Assets/Scripts/Mobility/SpriteFlipbookAnimator.cs`
- [X] T005 Implementar o esqueleto de `PlatformerMovementController` (`Rigidbody2D`, `BoxCollider2D`, raycast de chão, aplica velocidade horizontal básica) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T003, T004)

**Checkpoint**: Fundação pronta — as histórias de usuário podem começar.

---

## Phase 3: User Story 1 - Movimentação básica no cenário de teste (Priority: P1) 🎯 MVP

**Goal**: Cenário de teste visível (chão, plataforma, fundo texturizado) onde o jogador anda,
corre e pula, com o personagem animado em cada estado.

**Independent Test**: Abrir a cena de teste de mobilidade, andar de um lado a outro, correr
segurando o comando de corrida, e pular do chão — observando o personagem animado em cada
estado, sem depender de nenhuma habilidade das outras histórias.

### Tests for User Story 1 (obrigatórios — Princípio III)

- [X] T006 [US1] Teste EditMode: `TryGroundJump` só produz efeito quando `IsGrounded == true`, e não incrementa `JumpsUsed` em `Assets/Tests/EditMode/PlatformerMovementStateTests.cs`

### Implementation for User Story 1

- [X] T007 [US1] Implementar leitura de input horizontal (andar) e aplicação de velocidade ao `Rigidbody2D` em `Assets/Scripts/Mobility/PlatformerMovementController.cs`
- [X] T008 [US1] Implementar corrida (velocidade horizontal aumentada enquanto o comando de corrida é mantido, só no chão — FR-003, Edge Cases) em `Assets/Scripts/Mobility/PlatformerMovementController.cs`
- [X] T009 [US1] Integrar `TryGroundJump` ao controller (aciona o pulo do chão a partir do input) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T006)
- [X] T010 [US1] Configurar as poses de Idle/Walking/Running/Jumping/Falling no `SpriteFlipbookAnimator` a partir dos sprites importados em T001 em `Assets/Scripts/Mobility/SpriteFlipbookAnimator.cs`
- [X] T011 [US1] Criar a cena `MobilityTest.unity` (chão plano, uma plataforma elevada, fundo texturizado, câmera 2D) via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs`
- [X] T012 [US1] Conectar `PlatformerMovementController` e `Demo.BoundedFollowCamera` (feature 004) à cena via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T005, T011)

**Checkpoint**: O jogador anda, corre e pula no cenário de teste, com animação correspondente —
testável de forma independente.

---

## Phase 4: User Story 2 - Mobilidade aérea avançada (pulo duplo e parede) (Priority: P2)

**Goal**: Pulo duplo, deslizar em paredes verticais, e pular a partir delas (estilo Super
Metroid), no mesmo cenário de teste.

**Independent Test**: Pular do chão e acionar o pulo novamente no ar (pulo duplo); separadamente,
pular contra uma parede vertical, deslizar por ela, e pular a partir dela — sem depender do
salto de energia acumulada (User Story 3).

### Tests for User Story 2 (obrigatórios — Princípio III)

- [X] T013 [P] [US2] Teste EditMode: `TryAerialJump` funciona uma vez no ar mas não uma segunda (sem terceiro pulo) em `Assets/Tests/EditMode/PlatformerMovementStateTests.cs`
- [X] T014 [P] [US2] Teste EditMode: `TryWallJump` só funciona com contato de parede no ar, e reseta `JumpsUsed` ao suceder (permite encadear) em `Assets/Tests/EditMode/PlatformerMovementStateTests.cs`
- [X] T015 [P] [US2] Teste EditMode: `GetFallSpeedMultiplier()` retorna `WallSlideFallSpeedMultiplier` (< 1) quando `IsWallSliding`, e `1f` no chão ou sem contato de parede (FR-006) em `Assets/Tests/EditMode/PlatformerMovementStateTests.cs`

### Implementation for User Story 2

- [X] T016 [US2] Integrar `TryAerialJump` ao controller (pulo duplo a partir do input, no ar) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T013)
- [X] T017 [US2] Implementar detecção de contato de parede via raycast, alimentar `WallContactDirection`, e multiplicar a velocidade de queda pelo valor de `GetFallSpeedMultiplier()` (não decidir o valor no controller) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T015)
- [X] T018 [US2] Integrar `TryWallJump` ao controller (impulsiona para longe da parede e para cima a partir do input) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T014, T017)
- [X] T019 [US2] Configurar as poses de DoubleJumping/WallSliding no `SpriteFlipbookAnimator` em `Assets/Scripts/Mobility/SpriteFlipbookAnimator.cs` (depende de T010)
- [X] T020 [US2] Adicionar ao menos uma parede vertical ao cenário de `MobilityTest.unity`, alta o suficiente para testar deslizar/pular na parede, via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T011)

**Checkpoint**: Pulo duplo e mobilidade de parede funcionando — testável de forma independente.

---

## Phase 5: User Story 3 - Salto de energia acumulada (Priority: P3)

**Goal**: Abaixar para carregar energia e saltar verticalmente até o teto (estilo Silksong), no
mesmo cenário de teste.

**Independent Test**: Abaixar o personagem sob uma plataforma elevada, segurar o comando de
abaixar por diferentes durações, soltar, e observar o personagem saltar com altura proporcional
à energia acumulada — sem depender de nenhuma habilidade das outras histórias.

### Tests for User Story 3 (obrigatórios — Princípio III)

- [X] T021 [US3] Teste EditMode: `AdvanceCharge` só acumula quando `IsGrounded && IsCrouching` (não tem efeito no ar nem fora de agachado, FR-008), nunca excede `MaxChargeSeconds`; `ReleaseCharge` abaixo de `MinChargeSecondsToLeap` retorna 0 (nenhum salto) e sempre zera a energia acumulada em `Assets/Tests/EditMode/PlatformerMovementStateTests.cs`

### Implementation for User Story 3

- [X] T022 [US3] Implementar abaixar (crouch): alimentar `IsCrouching` no `PlatformerMovementState` a partir do input, só reconhecido com o personagem no chão (o próprio estado já ignora chamadas fora dessa condição — T021) em `Assets/Scripts/Mobility/PlatformerMovementController.cs`
- [X] T023 [US3] Chamar `AdvanceCharge` a cada frame (a guarda interna decide se acumula) e exibir feedback visual do progresso em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T021, T022)
- [X] T024 [US3] Integrar `ReleaseCharge` ao controller (aplica a fração retornada como velocidade vertical ao soltar o comando de abaixar) em `Assets/Scripts/Mobility/PlatformerMovementController.cs` (depende de T023)
- [X] T025 [US3] Configurar a pose de Crouching no `SpriteFlipbookAnimator` em `Assets/Scripts/Mobility/SpriteFlipbookAnimator.cs` (depende de T010)
- [X] T026 [US3] Posicionar uma plataforma/teto no cenário de `MobilityTest.unity` para testar a colisão do salto de energia acumulada, via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs` (depende de T011)

**Checkpoint**: Todas as três histórias de usuário funcionam de forma independente e integrada.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Confirmar ausência de regressão e validação final de ponta a ponta

- [X] T027 [P] Rodar a suíte completa de testes EditMode/PlayMode das features 001-004 (atributos, habilidades, sobrevivência, reputação/economia, criação de personagem, combate em tempo real) e confirmar que 100% continuam passando sem alteração de comportamento (FR-014)
- [ ] T028 Executar a validação completa de [quickstart.md](./quickstart.md) (testes automatizados + os 3 blocos de validação manual por história)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente (mas requer confirmação de download do usuário)
- **Foundational (Phase 2)**: Pode começar em paralelo ao Setup (não depende dos assets em si, apenas define a lógica de decisão) — mas BLOQUEIA todas as histórias de usuário
- **User Stories (Phase 3-5)**: Todas dependem da conclusão da fase Foundational; a integração de cada uma no `PlatformerMovementController` depende do esqueleto criado em T005
- **Polish (Phase 6)**: Depende de todas as histórias de usuário desejadas estarem completas

### User Story Dependencies

- **US1 (P1 — Movimentação básica)**: Pode começar após Foundational. Sem dependência de US2/US3.
- **US2 (P2 — Pulo duplo e parede)**: Pode começar após Foundational; tecnicamente independente de US1 na lógica (`PlatformerMovementState` já cobre ambos desde a Foundational), mas sua tarefa de cena (T020) depende de `MobilityTest.unity` já existir (T011, de US1).
- **US3 (P3 — Salto de energia acumulada)**: Mesma relação de US2 — independente na lógica, mas sua tarefa de cena (T026) depende de T011 (US1).

### Within Each User Story

- Testes (quando aplicável) DEVEM ser escritos e falhar antes da implementação
- Integração no controller depende do método correspondente já existir em `PlatformerMovementState` (Foundational)
- Poses de animação dependem do `SpriteFlipbookAnimator` já existir (Foundational) e dos assets já importados (Setup)

### Parallel Opportunities

- T013, T014 e T015 (testes de US2, mesmo arquivo mas write-only e não conflitantes em intenção) podem ser escritos em sequência rápida; tarefas [P] marcadas indicam apenas ausência de dependência de código, não de arquivo
- T003 e T004 (Foundational, arquivos distintos) podem rodar em paralelo
- T002 (créditos) pode rodar em paralelo a T001 seguir para importação, uma vez que o download tenha concluído

---

## Parallel Example: Foundational

```bash
Task: "Implementar PlatformerMovementState em Assets/Scripts/Mobility/PlatformerMovementState.cs"
Task: "Implementar SpriteFlipbookAnimator em Assets/Scripts/Mobility/SpriteFlipbookAnimator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar a Fase 1: Setup
2. Completar a Fase 2: Foundational (CRÍTICO — bloqueia todas as histórias)
3. Completar a Fase 3: User Story 1 (movimentação básica)
4. **PARAR e VALIDAR**: testar a User Story 1 de forma independente via
   [quickstart.md](./quickstart.md#validação-manual--movimentação-básica-user-story-1--p1)
5. Demonstrar/avaliar se pronto

### Incremental Delivery

1. Completar Setup + Foundational → fundação pronta
2. Adicionar US1 (movimentação básica) → testar independentemente → demo (MVP!)
3. Adicionar US2 (pulo duplo e parede) → testar independentemente → demo
4. Adicionar US3 (salto de energia acumulada) → testar independentemente → demo
5. Completar a Fase 6 (Polish) → validação final via quickstart.md

### Parallel Team Strategy

Com múltiplos desenvolvedores, após a Fase Foundational: Dev A pode seguir com US1 enquanto
Dev B escreve os testes de US2/US3 contra a `PlatformerMovementState` já pronta — a integração
de cada história no `PlatformerMovementController` (arquivo compartilhado) é o único ponto que
exige coordenação entre elas.

---

## Notes

- [P] tasks = arquivos diferentes (ou escrita não-conflitante no mesmo arquivo de teste), sem dependências pendentes
- O rótulo [Story] mapeia a tarefa para a história de usuário correspondente (rastreabilidade)
- Verificar que os testes falham antes de implementar (TDD para os sistemas centrais, por exigência do Princípio III da constituição)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a história independentemente
- Antes de T001, confirmar explicitamente com o usuário o download do arquivo de terceiro (nome, fonte, tamanho) — não presumir aprovação apenas pela escolha de fonte já feita na spec
- Nenhuma regra de jogo (guardas, valores de velocidade, limites) deve ser adicionada apenas ao `PlatformerMovementController` sem um método/propriedade correspondente e testado em `PlatformerMovementState` — é o critério que motivou a remediação registrada no topo deste arquivo
