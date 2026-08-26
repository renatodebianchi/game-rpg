---
description: "Task list template for feature implementation"
---

# Tasks: Assets Visuais do Personagem e da Interface + Exploração com Personagem Criado

**Input**: Design documents from `/specs/003-character-visual-exploration/`

**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

**Tests**: Incluídos e OBRIGATÓRIOS para a lógica central (mapeamento de características
visuais, transição de cena entre Criação de Personagem e Exploração), por exigência do
Princípio III (NON-NEGOTIABLE) da constituição do projeto. A aparência visual em si (quais PNGs
foram escolhidos, o reskin de botões/painéis) não exige testes automatizados, mesmo padrão já
adotado pelas demos das features 001/002.

**Organization**: Tarefas agrupadas por história de usuário (spec.md) para permitir
implementação e teste independentes de cada uma. Esta feature estende o projeto Unity único já
existente (features `001-isometric-sandbox-rpg`, `002-character-creation`).

**⚠️ Nota de licenciamento/download**: T001 e T002 envolvem baixar arquivos de terceiros
(Kenney.nl). Cada download exige confirmação explícita do usuário (nome do arquivo, fonte,
tamanho) antes de ser realizado — não apenas a aprovação geral já dada para usar Kenney.nl como
fonte padrão na fase de clarificação da spec.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências pendentes)
- **[Story]**: A qual história de usuário a tarefa pertence (US1, US2, US3)
- Caminhos de arquivo exatos incluídos em cada descrição

## Path Conventions

Mesmo projeto Unity único das features 001/002 (ver [plan.md](./plan.md#project-structure)):
`Assets/Scripts/`, `Assets/Art/`, `Assets/Scenes/`, `Assets/Tests/EditMode/`,
`Assets/Tests/PlayMode/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Obter e importar os pacotes de assets de terceiros que todas as histórias
dependem.

- [X] T001 Confirmar com o usuário e baixar o pacote Kenney "Roguelike Characters" (`kenney_roguelike-characters.zip`, CC0, https://kenney.nl/assets/roguelike-characters); importar o(s) sprite(s)/spritesheet necessário(s) em `Assets/Art/Characters/` com configurações de importação para pixel art (filtro Point, sem compressão com perdas)
- [X] T002 Confirmar com o usuário e baixar o pacote Kenney "UI Pack" (`kenney_ui-pack.zip`, CC0, https://kenney.nl/assets/ui-pack); importar os sprites de botão/painel e as fontes TTF necessários em `Assets/Art/UI/`
- [X] T003 [P] Criar `Assets/Art/CREDITS.md` documentando fonte (kenney.nl), pacote e licença (CC0) de cada asset importado em T001/T002 (FR-008)

**Checkpoint**: Assets de terceiros importados e documentados; prontos para uso pelo código.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Estruturas de dados e componentes compartilhados que as três histórias de usuário
dependem

**⚠️ CRITICAL**: Nenhuma história de usuário pode começar antes desta fase estar completa

- [X] T004 [P] Implementar `CharacterSpriteMapping` (tabela estática: tingimento por `SkinTone`, frame por `BodyType`, conforme [contracts/character-sprite-mapping-contract.md](./contracts/character-sprite-mapping-contract.md)) em `Assets/Scripts/Characters/CharacterSpriteMapping.cs`
- [X] T005 [P] Implementar `PendingPlayerCharacter` (portador estático simples entre cenas, conforme [contracts/scene-transition-contract.md](./contracts/scene-transition-contract.md)) em `Assets/Scripts/Core/PendingPlayerCharacter.cs`
- [X] T006 [P] Implementar o esqueleto de `DemoUiKit` (`CreateText`/`CreateButton`/`CreatePanel`, funcional com o visual padrão da UGUI, pronto para receber os assets do UI Pack em US3) em `Assets/Scripts/UI/DemoUiKit.cs`

**Checkpoint**: Fundação pronta — as histórias de usuário podem começar (em paralelo, se houver equipe).

---

## Phase 3: User Story 1 - Ver e mover o personagem criado na demo de Exploração (Priority: P1) 🎯 MVP

**Goal**: Ao finalizar a criação de personagem, o jogador é levado para a Exploração e vê/move
seu personagem como um sprite humanoide.

**Independent Test**: Finalizar uma criação de personagem, verificar a transição para a
Exploração, e mover o personagem nas quatro direções, sem depender das histórias de aparência
fiel ou de reskin da interface.

### Tests for User Story 1 (obrigatórios — Princípio III)

- [X] T007 [P] [US1] Teste EditMode: `PendingPlayerCharacter` guarda um `Character` até ser lido, e é limpo após a leitura (não reutiliza um personagem de sessão anterior) em `Assets/Tests/EditMode/PendingPlayerCharacterTests.cs`
- [X] T008 [P] [US1] Teste PlayMode: finalizar a criação de personagem e carregar a Exploração resulta em um personagem com os mesmos atributos/inventário/aparência do que foi finalizado (contrato de transição de cena) em `Assets/Tests/PlayMode/CharacterCreationToExplorationFlowTests.cs`

### Implementation for User Story 1

- [X] T009 [US1] Implementar `ExplorationCharacterController` (instancia o sprite do personagem, lê `PendingPlayerCharacter` ou cria um personagem padrão — FR-004 —, e move o personagem via teclado WASD/setas) em `Assets/Scripts/Demo/ExplorationCharacterController.cs` (depende de T001, T005)
- [X] T010 [US1] Conectar `ExplorationCharacterController` à cena `Assets/Scenes/Exploration.unity` via `ProjectBootstrap` em `Assets/Editor/ProjectBootstrap.cs`
- [X] T011 [US1] Estender o botão "Finalizar" de `CharacterCreationUI` para definir `PendingPlayerCharacter` e carregar a cena de Exploração (`SceneManager.LoadScene`) em `Assets/Scripts/UI/CharacterCreationUI.cs` (depende de T005)

**Checkpoint**: O jogador consegue finalizar a criação, ver seu personagem na Exploração como
sprite humanoide, e movê-lo — testável de forma independente.

---

## Phase 4: User Story 2 - Aparência do sprite reflete as características visuais escolhidas (Priority: P2)

**Goal**: O sprite exibido na Exploração varia (tingimento/frame) conforme as características
visuais escolhidas na criação de personagem.

**Independent Test**: Criar dois personagens com tons de pele diferentes e comparar os sprites
exibidos na Exploração lado a lado, sem depender da história de reskin da interface.

### Tests for User Story 2 (obrigatórios — Princípio III)

- [X] T012 [P] [US2] Testes EditMode: cada valor de `SkinTone` mapeia para um `tintColor` fixo e distinto; o mapeamento nunca lança exceção para nenhuma combinação de `VisualCharacteristics` (contrato, regras 1 e 3) em `Assets/Tests/EditMode/CharacterSpriteMappingTests.cs`

### Implementation for User Story 2

- [X] T013 [US2] Aplicar o resultado de `CharacterSpriteMapping` (tingimento + frame) ao `SpriteRenderer` do personagem em `ExplorationCharacterController` em `Assets/Scripts/Demo/ExplorationCharacterController.cs` (depende de T004, T009)

**Checkpoint**: Personagens com aparências diferentes são visualmente diferenciáveis na
Exploração — testável de forma independente.

---

## Phase 5: User Story 3 - Interface visual estilizada com assets abertos (Priority: P3)

**Goal**: Todos os botões/painéis/texto das telas construídas em runtime usam os assets do
Kenney UI Pack, aplicados via `DemoUiKit`, sem precisar redesenhar cada tela.

**Independent Test**: Abrir qualquer tela construída em runtime e verificar que botões/painéis
usam os novos assets visuais, sem depender das histórias de personagem visual.

### Implementation for User Story 3

- [X] T014 [US3] Aplicar os sprites de botão/painel e a fonte do Kenney UI Pack a `DemoUiKit.CreateButton`/`CreateText`/`CreatePanel` em `Assets/Scripts/UI/DemoUiKit.cs` (depende de T002, T006)
- [X] T015 [P] [US3] Migrar `CombatDemoController` para usar `DemoUiKit` em vez de seus métodos de UI duplicados em `Assets/Scripts/Demo/CombatDemoController.cs` (depende de T014)
- [X] T016 [P] [US3] Migrar `SkillTreeDemoController` para usar `DemoUiKit` em `Assets/Scripts/Demo/SkillTreeDemoController.cs` (depende de T014)
- [X] T017 [P] [US3] Migrar `SurvivalDemoController` para usar `DemoUiKit` em `Assets/Scripts/Demo/SurvivalDemoController.cs` (depende de T014)
- [X] T018 [P] [US3] Migrar `ReputationEconomyDemoController` para usar `DemoUiKit` em `Assets/Scripts/Demo/ReputationEconomyDemoController.cs` (depende de T014)
- [X] T019 [US3] Migrar `CharacterCreationUI` para usar `DemoUiKit` em `Assets/Scripts/UI/CharacterCreationUI.cs` (depende de T014; deve ser feita depois de T011, já que ambas tocam o mesmo arquivo)

**Checkpoint**: Todas as três histórias de usuário funcionam de forma independente e integrada;
nenhuma tela construída em runtime usa mais retângulos de cor sólida.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Validação final de ponta a ponta

- [ ] T020 Executar a validação completa de [quickstart.md](./quickstart.md) (testes automatizados + os 3 blocos de validação manual por história)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode começar imediatamente (mas requer confirmação de download do usuário)
- **Foundational (Phase 2)**: Pode começar em paralelo ao Setup (não depende dos assets de terceiros em si, apenas define estruturas de dados) — mas BLOQUEIA todas as histórias de usuário
- **User Stories (Phase 3-5)**: Todas dependem da conclusão da fase Foundational; US1 também depende de T001 (sprite do personagem); US3 também depende de T002 (assets de UI)
- **Polish (Phase 6)**: Depende de todas as histórias de usuário desejadas estarem completas

### User Story Dependencies

- **US1 (P1 – Ver/mover na Exploração)**: Pode começar após Foundational + T001. Sem dependência de US2/US3.
- **US2 (P2 – Aparência fiel)**: Pode começar após Foundational; sua implementação (T013) depende de `ExplorationCharacterController` já existir (T009, de US1) para ter onde aplicar o `SpriteRenderer` — portanto, embora testável de forma independente (T012), sua tarefa de implementação tem uma dependência técnica direta em US1.
- **US3 (P3 – Reskin de UI)**: Pode começar após Foundational + T002; suas migrações (T015-T019) tocam arquivos das features 001/002, então DEVEM ser feitas depois que os arquivos dessas features já existem (já existem, pois são features anteriores) — T019 especificamente depende de T011 (US1) ter sido concluída primeiro, por tocarem o mesmo arquivo `CharacterCreationUI.cs`.

### Within Each User Story

- Testes (quando aplicável) DEVEM ser escritos e falhar antes da implementação
- Tipos de dados/definições antes de serviços
- Serviços antes de UI/integração
- Implementação central antes de integração cruzada com outras histórias

### Parallel Opportunities

- Todas as tarefas Foundational marcadas [P] podem rodar em paralelo (T004-T006)
- T015-T018 (migração de 4 controllers distintos para `DemoUiKit`) podem rodar em paralelo entre si, mas todas dependem de T014
- T001 e T002 (downloads) podem ser confirmados/executados em paralelo entre si

---

## Parallel Example: Foundational

```bash
# Rodar as tarefas independentes da Fase Foundational juntas:
Task: "Implementar CharacterSpriteMapping em Assets/Scripts/Characters/CharacterSpriteMapping.cs"
Task: "Implementar PendingPlayerCharacter em Assets/Scripts/Core/PendingPlayerCharacter.cs"
Task: "Implementar o esqueleto de DemoUiKit em Assets/Scripts/UI/DemoUiKit.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 apenas)

1. Completar a Fase 1: Setup (pelo menos T001, o sprite do personagem)
2. Completar a Fase 2: Foundational (CRÍTICO — bloqueia todas as histórias)
3. Completar a Fase 3: User Story 1 (Ver/mover na Exploração)
4. **PARAR e VALIDAR**: testar a User Story 1 de forma independente via [quickstart.md](./quickstart.md#validação-manual--ver-e-mover-o-personagem-na-exploração-user-story-1--p1)
5. Demonstrar/avaliar se pronto

### Incremental Delivery

1. Completar Setup + Foundational → fundação pronta
2. Adicionar US1 (Ver/mover na Exploração) → testar independentemente → demo (MVP!)
3. Adicionar US2 (Aparência fiel) → testar independentemente → demo
4. Adicionar US3 (Reskin de UI) → testar independentemente → demo
5. Completar a Fase 6 (Polish) → validação final via quickstart.md

### Parallel Team Strategy

Com múltiplos desenvolvedores:

1. Equipe completa Setup + Foundational em conjunto
2. Após a Fase Foundational:
   - Dev A: User Story 1 (Exploração)
   - Dev B: User Story 3 (Reskin de UI) — pode adiantar T014-T018, mas aguarda T011 (Dev A) antes de T019
   - Dev C: prepara os testes de User Story 2, que dependem de T009 (Dev A) para a integração final
3. Histórias se integram nos pontos definidos (T013, T019) ao final de cada incremento

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências pendentes
- O rótulo [Story] mapeia a tarefa para a história de usuário correspondente (rastreabilidade)
- Cada história de usuário deve ser completável e testável de forma independente
- Verificar que os testes falham antes de implementar (TDD para os sistemas centrais, por exigência do Princípio III da constituição)
- Fazer commit após cada tarefa ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar a história independentemente
- Antes de T001/T002, confirmar explicitamente com o usuário cada download de arquivo de terceiro (nome, fonte, tamanho) — não presumir aprovação apenas pela escolha de fonte já feita na spec
