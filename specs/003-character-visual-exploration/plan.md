# Implementation Plan: Assets Visuais do Personagem e da Interface + Exploração com Personagem Criado

**Branch**: `003-character-visual-exploration` | **Date**: 2026-08-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-character-visual-exploration/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Dar um visual real ao personagem e à interface do jogo usando assets abertos (Kenney.nl, CC0):
um sprite 2D humanoide (pacote "Roguelike Characters") representa o personagem na cena de
Exploração, com tingimento/variação de frame refletindo as características visuais escolhidas
na criação (feature 002); os componentes de UI hoje duplicados em cada demo são extraídos para
um kit compartilhado (`DemoUiKit`) que aplica o "UI Pack" do Kenney a botões/painéis/fonte em
todas as telas de uma vez; e a tela de Criação de Personagem passa a levar o jogador direto para
a Exploração com o personagem recém-criado. Abordagem técnica detalhada em
[research.md](./research.md).

## Technical Context

**Language/Version**: C# (Unity 6000.5.9f1 — mesma versão das features 001/002)

**Primary Dependencies**: Unity UGUI (sem pacotes novos do Package Manager); assets binários de terceiros (sprites PNG, spritesheet, fontes TTF) do Kenney.nl importados como assets Unity

**Storage**: Inalterado (JSON local); mais os arquivos de asset importados em `Assets/Art/`

**Testing**: Unity Test Framework — EditMode (mapeamento de características visuais → sprite/tingimento) e PlayMode (transição Criação → Exploração)

**Target Platform**: PC desktop (mesmo alvo das features 001/002)

**Project Type**: Extensão do mesmo projeto Unity single-player (não é um novo projeto)

**Performance Goals**: Mesmos das features 001/002 (60 fps, <100ms de input); sprites 2D leves, sem risco de regressão

**Constraints**: Assets de terceiros DEVEM ser CC0/licença aberta equivalente (FR-006/FR-009); nenhum download de arquivo de terceiro sem confirmação explícita do usuário (nome, fonte, tamanho)

**Scale/Scope**: 1 pacote de sprite de personagem, 1 pacote de UI, 1 nova cena/transição jogável (Exploração), refatoração dos componentes de UI de 5 controllers existentes para um kit compartilhado

Todas as decisões acima foram resolvidas em [research.md](./research.md); nenhuma marcação
`NEEDS CLARIFICATION` permanece.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` (v1.0.1):

| Princípio | Avaliação | Status |
|---|---|---|
| I. Gameplay-First Design | Ver o personagem criado (feature 002) de forma visualmente reconhecível e poder movê-lo é uma melhoria direta da experiência do jogador, não uma tarefa técnica isolada. | PASS |
| II. Modular & Data-Driven Architecture | `CharacterSpriteMapping` como dados estáticos; `DemoUiKit` centraliza os componentes de UI (corrige uma duplicação real encontrada em research.md); conteúdo de terceiros importado como assets Unity, não hard-coded. | PASS |
| III. Test Coverage for Core Systems (NON-NEGOTIABLE) | O mapeamento de características visuais e a transição de cena (lógica, não apresentação pura) recebem testes EditMode/PlayMode obrigatórios; a aparência visual em si (qual PNG é bonito) fica fora dessa exigência, como já ocorre com as demais demos. | PASS |
| IV. Performance & Responsiveness Budgets | Reutiliza o orçamento já definido (60 fps, <100ms); sprites 2D leves não introduzem risco de regressão de performance. | PASS |
| V. Simplicity & Iterative Scope | Escolha explícita de sprite 2D em vez de modelo 3D rigado; mapeamento "melhor esforço" em vez de cobertura exata de todas as combinações; objeto de transferência entre cenas simples (valor estático) em vez de um sistema de gerenciamento de estado mais robusto — todas justificadas em research.md. | PASS |

**Resultado**: Nenhuma violação. Nenhuma entrada necessária em "Complexity Tracking".

**Nota de processo**: esta feature envolve baixar arquivos de terceiros (assets Kenney.nl). Cada
download será confirmado explicitamente com o usuário durante a implementação (nome do arquivo,
fonte, tamanho), conforme já registrado na spec e em research.md.

## Project Structure

### Documentation (this feature)

```text
specs/003-character-visual-exploration/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   ├── character-sprite-mapping-contract.md
│   └── scene-transition-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

Mesmo projeto Unity único das features 001/002 — esta feature adiciona/refatora arquivos
dentro da estrutura já existente.

```text
Assets/
├── Art/
│   ├── Characters/                              # novo: sprites/spritesheet do Kenney Roguelike Characters
│   ├── UI/                                       # novo: sprites/fontes do Kenney UI Pack
│   └── CREDITS.md                                # novo: registro de créditos de assets (FR-008)
├── Scripts/
│   ├── Characters/
│   │   └── CharacterSpriteMapping.cs             # novo (dados estáticos)
│   ├── Core/
│   │   └── PendingPlayerCharacter.cs             # novo (transferência entre cenas)
│   ├── Demo/
│   │   ├── ExplorationCharacterController.cs     # novo
│   │   ├── CombatDemoController.cs               # (feature 001) refatorado para usar DemoUiKit
│   │   ├── SkillTreeDemoController.cs            # (feature 001) refatorado para usar DemoUiKit
│   │   ├── SurvivalDemoController.cs             # (feature 001) refatorado para usar DemoUiKit
│   │   └── ReputationEconomyDemoController.cs    # (feature 001) refatorado para usar DemoUiKit
│   └── UI/
│       ├── DemoUiKit.cs                          # novo: componentes de UI compartilhados (botão/painel/texto)
│       └── CharacterCreationUI.cs                # (feature 002) refatorado para usar DemoUiKit + transição de cena
└── Scenes/
    └── Exploration.unity                          # (feature 001) recebe o ExplorationCharacterController

Assets/Tests/
├── EditMode/
│   └── CharacterSpriteMappingTests.cs             # novo
└── PlayMode/
    └── CharacterCreationToExplorationFlowTests.cs # novo
```

**Structure Decision**: Continuação do projeto Unity único já estabelecido (Princípio V). Os
novos arquivos seguem a convenção de pastas por responsabilidade já em uso; a refatoração dos
controllers de demo existentes para `DemoUiKit` é escopo desta feature porque é o mecanismo que
torna FR-007 verdadeiro (ver research.md).

## Complexity Tracking

*Nenhuma violação da Constitution Check acima — seção não aplicável.*
