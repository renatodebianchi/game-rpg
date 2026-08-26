---
description: "Task list template for feature implementation"
---

# Tasks: Criação de Personagem (Atributos, Aparência e Equipamento Inicial)

**Input**: Design documents from `/specs/002-character-creation/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Incluídos e OBRIGATÓRIOS para a lógica central (Point Buy, resolução de kit de
equipamento, regras de finalização), por exigência do Princípio III (NON-NEGOTIABLE) da
constituição do projeto. A UI de criação em si (apresentação) não exige testes automatizados,
mesmo padrão já adotado pelos controladores de demo da feature 001.

**Organization**: Tarefas agrupadas por história de usuário (spec.md) para permitir
implementação e teste independentes de cada uma. Esta feature estende o projeto Unity único já
existente (feature `001-isometric-sandbox-rpg`) — não há fase de Setup de infraestrutura nova.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: A qual história de usuário a tarefa pertence (US1, US2, US3)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Mesmo projeto Unity único da feature 001 (ver [plan.md](./plan.md#project-structure)):
`Assets/Scripts/`, `Assets/Data/`, `Assets/Scenes/`, `Assets/Tests/EditMode/`,
`Assets/Tests/PlayMode/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar a pasta de conteúdo nova desta feature; nenhuma dependência de pacote nova
é necessária (reutiliza a stack já configurada na feature 001).

- [X] T001 Criar a pasta `Assets/Data/Equipment` (para os assets `EquipmentKitDefinition` da US2)

**Checkpoint**: Estrutura de pastas pronta; nenhuma configuração de projeto adicional necessária.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Tipos de dados e o ponto de integração (`CharacterCreationProfile`) que as três
histórias de usuário compartilham

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes desta fase estar completa

- [X] T002 [P] Implementar o enum `CharacterOrientation` (`Combatant`/`Arcanist`) em `Assets/Scripts/Characters/CharacterOrientation.cs` (ver [data-model.md](./data-model.md#characterorientation-enum))
- [X] T003 [P] Implementar `PointBuyCostTable` (tabela estática de custo cumulativo 8→0 ... 15→9) em `Assets/Scripts/Characters/PointBuyCostTable.cs` (ver [data-model.md](./data-model.md#pointbuycosttable-dados-estáticos))
- [X] T004 [P] Implementar `AttributeAllocationState` (scores dos 4 atributos, orçamento de 18 pontos, validação de mudança de atributo conforme [contracts/point-buy-contract.md](./contracts/point-buy-contract.md)) em `Assets/Scripts/Characters/AttributeAllocationState.cs` (depende de T003)
- [X] T005 [P] Implementar a struct `VisualCharacteristics` (tipo de corpo, tom de pele, estilo/cor de cabelo, com valores padrão para FR-007) em `Assets/Scripts/Characters/VisualCharacteristics.cs`
- [X] T006 [P] Implementar `EquipmentKitDefinition` (ScriptableObject: orientação + lista de itens/quantidade) em `Assets/Scripts/World/EquipmentKitDefinition.cs` (depende de T002)
- [X] T007 Estender `Character.cs` com um campo `Visuals` (`VisualCharacteristics`), atribuível apenas pelo fluxo de criação de personagem, em `Assets/Scripts/Characters/Character.cs` (depende de T005)
- [X] T008 Estender `Core/SaveData.cs` (`CharacterSaveData`: campos de aparência) e `Core/SaveSystem.cs` (`CaptureGameState`/`ApplyGameState` passam a incluir `Visuals`) em `Assets/Scripts/Core/SaveData.cs`, `Assets/Scripts/Core/SaveSystem.cs` (depende de T007)
- [X] T009 Implementar `CharacterCreationProfile` com `Finalize()` (aplica atributos, resolve e adiciona o kit de equipamento ao inventário, aplica aparência) conforme [contracts/character-creation-finalization-contract.md](./contracts/character-creation-finalization-contract.md) em `Assets/Scripts/Characters/CharacterCreationProfile.cs` (depende de T004, T006, T007)

**Checkpoint**: Fundação pronta — as histórias de usuário podem começar (em paralelo, se houver equipe).

---

## Phase 3: User Story 1 - Definir atributos base por Point Buy (Priority: P1) 🎯 MVP

**Goal**: Permitir que o jogador aloque os 18 pontos do Point Buy entre os 4 atributos base,
respeitando a curva de custo do D&D 5e, antes de poder avançar na criação do personagem.

**Independent Test**: Distribuir o orçamento de pontos entre os quatro atributos e verificar
que o personagem resultante (após finalizar) reflete exatamente os valores escolhidos, sem
depender das histórias de equipamento ou aparência.

### Tests for User Story 1 (obrigatórios — Princípio III)

- [X] T010 [P] [US1] Teste EditMode para os valores de `PointBuyCostTable` (8→0, 9→1, 10→2, 11→3, 12→4, 13→5, 14→7, 15→9) em `Assets/Tests/EditMode/PointBuyCostTableTests.cs`
- [X] T011 [P] [US1] Teste EditMode para `AttributeAllocationState`: rejeita valores fora de 8-15, rejeita mudanças que excedam o orçamento de 18 pontos, e permite reduzir um atributo para recuperar pontos antes de finalizar em `Assets/Tests/EditMode/AttributeAllocationStateTests.cs`
- [X] T012 [P] [US1] Teste EditMode: `CharacterCreationProfile.Finalize()` é bloqueado enquanto `pointsRemaining != 0`, e aplica corretamente os valores finais a `Character.Attributes` quando o orçamento é gasto por completo em `Assets/Tests/EditMode/CharacterCreationFinalizationTests.cs`

### Implementation for User Story 1

- [X] T013 [US1] Implementar a etapa de atributos da UI de criação (`CharacterCreationUI`): controles para os 4 atributos, exibição de pontos restantes, e bloqueio de avanço enquanto `pointsRemaining != 0` (FR-003) em `Assets/Scripts/UI/CharacterCreationUI.cs` (depende de T004)

**Checkpoint**: A alocação de atributos por Point Buy é jogável e testável de forma independente.

---

## Phase 4: User Story 2 - Receber equipamento inicial pela orientação escolhida (Priority: P2)

**Goal**: Permitir que o jogador escolha uma orientação (combatente/arcanista) e receba
automaticamente o kit de equipamento fixo correspondente no inventário, sem que isso restrinja
as trilhas de habilidade disponíveis mais tarde.

**Independent Test**: Escolher cada orientação isoladamente e verificar que o personagem
resultante recebe o kit de equipamento fixo correspondente, sem depender das outras histórias.

### Tests for User Story 2 (obrigatórios — Princípio III)

- [X] T014 [P] [US2] Testes EditMode: `Finalize()` resolve o `EquipmentKitDefinition` correto pela orientação escolhida e adiciona seus itens a `Character.Inventory`; e falha explicitamente (`ContentValidationException`) se nenhum kit corresponder à orientação (contrato, regra 2) em `Assets/Tests/EditMode/EquipmentKitResolutionTests.cs`

### Implementation for User Story 2

- [X] T015 [US2] Implementar a etapa de orientação da UI de criação (escolha entre Combatente/Arcanista, bloqueando o avanço até uma escolha ser feita — contrato de finalização, regra 2) em `Assets/Scripts/UI/CharacterCreationUI.cs` (depende de T006, T009)
- [X] T016 [P] [US2] Criar os dois assets de conteúdo `EquipmentKitDefinition` iniciais (Combatente, Arcanista) em `Assets/Data/Equipment/`

**Checkpoint**: A escolha de orientação e o recebimento do kit de equipamento inicial são
jogáveis e testáveis de forma independente; investir em qualquer trilha da árvore de
habilidades depois continua livre, independentemente da orientação escolhida aqui.

---

## Phase 5: User Story 3 - Personalizar características visuais básicas (Priority: P3)

**Goal**: Permitir que o jogador escolha tipo de corpo, tom de pele e estilo/cor de cabelo
entre opções predefinidas, com valores padrão aplicados a qualquer característica não
escolhida.

**Independent Test**: Selecionar cada característica visual disponível isoladamente e verificar
que o personagem resultante reflete as escolhas feitas, sem depender das histórias de atributos
ou equipamento.

### Tests for User Story 3 (obrigatórios — Princípio III)

- [X] T017 [P] [US3] Teste EditMode: `Finalize()` aplica as `VisualCharacteristics` escolhidas a `Character.Visuals`, e aplica os valores padrão documentados para qualquer característica não selecionada (FR-007) em `Assets/Tests/EditMode/VisualCharacteristicsTests.cs`

### Implementation for User Story 3

- [X] T018 [US3] Implementar a etapa de aparência da UI de criação (seleção de tipo de corpo, tom de pele, estilo/cor de cabelo, e a tela de resumo de todas as escolhas — FR-008) em `Assets/Scripts/UI/CharacterCreationUI.cs` (depende de T005, T009)

**Checkpoint**: Todas as três histórias de usuário funcionam de forma independente e integrada.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validação de ponta a ponta e integração com a demo existente

- [X] T019 Teste PlayMode para o fluxo completo de criação (alocar atributos → escolher orientação → escolher aparência → finalizar) verificando o `Character` resultante (atributos, inventário, `Visuals`) em `Assets/Tests/PlayMode/CharacterCreationFlowTests.cs`
- [X] T020 [P] Estender o teste de round-trip de save/load da feature 001 (`Assets/Tests/PlayMode/SaveLoadRoundTripTests.cs`) para cobrir também o novo campo `Visuals`
- [X] T021 [P] Conectar `CharacterCreationUI` a uma nova cena de demo `Assets/Scenes/CharacterCreationDemo.unity` via `ProjectBootstrap`, seguindo o mesmo padrão de UI construída em runtime das demos da feature 001
- [X] T022 Executar a validação completa de [quickstart.md](./quickstart.md) (testes automatizados + os 3 blocos de validação manual por história)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente
- **Foundational (Phase 2)**: Depende da conclusão do Setup — BLOQUEIA todas as histórias de usuário
- **User Stories (Phase 3-5)**: Todas dependem da conclusão da fase Foundational
  - Podem prosseguir em paralelo (se houver equipe) ou sequencialmente em ordem de prioridade (P1 → P2 → P3)
- **Polish (Phase 6)**: Depende de todas as histórias de usuário desejadas estarem completas

### User Story Dependencies

- **US1 (P1 – Atributos)**: Pode começar após a Fase Foundational. Sem dependência de outras histórias.
- **US2 (P2 – Orientação/Equipamento)**: Pode começar após a Fase Foundational; é independente de US1, mas ambas escrevem na mesma UI compartilhada (`CharacterCreationUI.cs`), então T013/T015/T018 devem ser integradas sequencialmente nesse arquivo, mesmo que os testes e o restante da lógica sejam paralelizáveis.
- **US3 (P3 – Aparência)**: Pode começar após a Fase Foundational; mesma observação de integração em `CharacterCreationUI.cs` que US2.

### Within Each User Story

- Testes (quando aplicável) DEVEM ser escritos e falhar antes da implementação
- Tipos de dados/definições antes de serviços
- Serviços antes de UI/integração
- Implementação central antes de integração cruzada com outras histórias

### Parallel Opportunities

- Todas as tarefas Foundational marcadas [P] podem rodar em paralelo (T002-T006)
- Após a Fase Foundational, US1-US3 podem começar em paralelo (se houver capacidade de equipe), mas as tarefas de UI (T013, T015, T018) tocam o mesmo arquivo e não são paralelizáveis entre si
- Todos os testes de uma história marcados [P] podem rodar em paralelo

---

## Parallel Example: Foundational

```bash
# Rodar as tarefas independentes da Fase Foundational juntas:
Task: "Implementar o enum CharacterOrientation em Assets/Scripts/Characters/CharacterOrientation.cs"
Task: "Implementar PointBuyCostTable em Assets/Scripts/Characters/PointBuyCostTable.cs"
Task: "Implementar a struct VisualCharacteristics em Assets/Scripts/Characters/VisualCharacteristics.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar a Fase 1: Setup
2. Completar a Fase 2: Foundational (CRÍTICO — bloqueia todas as histórias)
3. Completar a Fase 3: User Story 1 (Atributos por Point Buy)
4. **PARAR e VALIDAR**: testar a User Story 1 de forma independente via [quickstart.md](./quickstart.md#validação-manual--atributos-por-point-buy-user-story-1--p1)
5. Demonstrar/avaliar se pronto

### Incremental Delivery

1. Completar Setup + Foundational → fundação pronta
2. Adicionar US1 (Atributos) → testar independentemente → demo
3. Adicionar US2 (Orientação/Equipamento) → testar independentemente → demo
4. Adicionar US3 (Aparência) → testar independentemente → demo
5. Completar a Fase 6 (Polish) → validação final via quickstart.md

### Parallel Team Strategy

Com múltiplos desenvolvedores:

1. Equipe completa Setup + Foundational em conjunto
2. Após a Fase Foundational:
   - Dev A: User Story 1 (Atributos)
   - Dev B: User Story 2 (Orientação/Equipamento)
   - Dev C: User Story 3 (Aparência)
3. Como as três histórias compartilham `CharacterCreationUI.cs`, as integrações de UI (T013,
   T015, T018) precisam ser combinadas/revisadas em conjunto ao final de cada incremento, mesmo
   que a lógica de cada história tenha sido desenvolvida em paralelo

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências pendentes
- O rótulo [Story] mapeia a tarefa para a história de usuário correspondente (rastreabilidade)
- Cada história de usuário deve ser completável e testável de forma independente
- Verificar que os testes falham antes de implementar (TDD para os sistemas centrais, por exigência do Princípio III da constituição)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a história independentemente
- Evitar: tarefas vagas, conflitos no mesmo arquivo, dependências entre histórias que quebrem a independência (a exceção documentada aqui é `CharacterCreationUI.cs`, compartilhada por design entre US1/US2/US3 por ser uma única tela de criação com três etapas)
