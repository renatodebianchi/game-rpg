# Implementation Plan: Cena de Teste de Mobilidade

**Branch**: `005-mobility-test-scene` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-mobility-test-scene/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Adiciona uma cena isolada de teste de mobilidade (chão plano, plataformas e paredes verticais)
com um controlador de movimentação estilo Metroidvania: andar, correr, pular, pulo duplo,
deslizar/pular na parede (estilo Super Metroid), e um salto vertical de energia acumulada
(agachar para carregar, soltar para saltar até o teto — estilo Silksong). Usa assets gratuitos
de Anokolisa no itch.io ("Legacy Fantasy – High Forest") para o sprite do personagem (com poses
animadas por estado), tileset do cenário, e fundo texturizado. Reaproveita
`Demo.BoundedFollowCamera` (feature 004)
para a câmera. A lógica de decisão de movimentação vive numa classe C# pura testável
(`PlatformerMovementState`), com um `MonoBehaviour` aplicando o resultado ao `Rigidbody2D` e ao
sprite. Abordagem técnica detalhada em [research.md](./research.md).

## Technical Context

**Language/Version**: C# (Unity 6000.5.9f1 — mesma versão das features 001-004)

**Primary Dependencies**: `UnityEngine.Physics2D` (módulo embutido, `Rigidbody2D`/`BoxCollider2D`/
raycasts); nenhum pacote novo do Package Manager; assets binários de terceiros (sprites/tileset/
fundo de Anokolisa, "Legacy Fantasy – High Forest", itch.io)

**Storage**: N/A — cena de teste sem persistência de estado

**Testing**: Unity Test Framework — EditMode (`PlatformerMovementState`: limite de pulo duplo,
condições de deslizar/pular na parede, acúmulo e liberação de energia do salto carregado)

**Target Platform**: PC desktop (mesmo alvo das features 001-004)

**Project Type**: Extensão do mesmo projeto Unity single-player já existente — nova cena de demo
isolada

**Performance Goals**: 60 fps; resposta de input de movimentação dentro de um frame (mesmo
orçamento já usado no combate em tempo real, feature 004)

**Constraints**: Assets de terceiros DEVEM ser de fontes abertas/gratuitas com créditos
registrados (FR-013 — não necessariamente CC0 formal, ver research.md sobre a licença
customizada da Anokolisa); nenhum download de arquivo de terceiro sem confirmação explícita do
usuário; a cena de teste não pode alterar a movimentação já existente em Exploração ou Combate
(FR-014); regras de jogo (deslizar/pular na parede, acúmulo de energia) DEVEM viver em
`PlatformerMovementState` (classe pura testável), nunca apenas no `MonoBehaviour` de
apresentação (Princípio III)

**Scale/Scope**: 1 cena nova (`MobilityTest.unity`), 1 pacote de assets de terceiros, 1
controlador de movimentação com ~9 estados, 1 componente de animação por troca de sprite

Todas as decisões acima foram resolvidas em [research.md](./research.md); nenhuma marcação
`NEEDS CLARIFICATION` permanece.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` (v1.0.1):

| Princípio | Avaliação | Status |
|---|---|---|
| I. Gameplay-First Design | O pedido é inteiramente sobre a sensação de movimentação do jogador ("deixar o jogo mais apresentável", testar mobilidade) — não uma tarefa técnica isolada. | PASS |
| II. Modular & Data-Driven Architecture | Parâmetros de movimentação (velocidades, altura de pulo, limites de energia) ficam em campos serializados configuráveis no `MonoBehaviour`, não hard-coded em fórmulas espalhadas; a lógica central (`PlatformerMovementState`) é isolada da apresentação (Rigidbody2D/sprite), como já ocorre com `CombatantActionState`. | PASS |
| III. Test Coverage for Core Systems (NON-NEGOTIABLE) | `PlatformerMovementState` decide TODAS as regras de jogo (pulo duplo, condição/velocidade de deslizar na parede via `GetFallSpeedMultiplier()`, guarda e acúmulo de energia via `AdvanceCharge`) como classe C# pura com testes EditMode obrigatórios; o `MonoBehaviour` só alimenta dados de física e aplica os valores já decididos, nunca decide nada por conta própria — a apresentação (troca de sprite, física do Rigidbody2D em si) é isenta, como já vale para os demais controllers de demo. | PASS |
| IV. Performance & Responsiveness Budgets | Reutiliza o orçamento já estabelecido (60 fps, resposta de input dentro de um frame); `Rigidbody2D` com raycasts é uma técnica leve e padrão, sem risco de regressão. | PASS |
| V. Simplicity & Iterative Scope | Reaproveita `BoundedFollowCamera` em vez de uma câmera nova; usa animação por troca de sprite via código em vez de um `AnimatorController` autorado na GUI; usa `Rigidbody2D`/raycasts (técnica padrão) em vez de reimplementar colisão manualmente — todas escolhas deliberadamente mais simples, justificadas em research.md. | PASS |

**Resultado**: Nenhuma violação. Nenhuma entrada necessária em "Complexity Tracking".

**Nota de processo**: esta feature envolve baixar um arquivo de terceiro (asset de Anokolisa no
itch.io). O download será confirmado explicitamente com o usuário durante a implementação (nome
do arquivo, fonte, tamanho), conforme já registrado na spec e em research.md.

## Project Structure

### Documentation (this feature)

```text
specs/005-mobility-test-scene/
├── plan.md               # This file (/speckit-plan command output)
├── research.md           # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   ├── movement-state-contract.md
│   └── charge-jump-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

Continuação do projeto Unity único já estabelecido (features 001-004); esta feature adiciona
uma nova cena de demo isolada e seus próprios scripts, sem tocar no combate ou na exploração
existentes (FR-014).

```text
Assets/
├── Art/
│   └── Platformer/
│       ├── Resources/                            # sprites/poses do personagem + tiles do cenário + fundo (Anokolisa, Legacy Fantasy – High Forest)
│       └── CREDITS.md                             # créditos do pacote (FR-013)
├── Scripts/
│   └── Mobility/
│       ├── PlatformerMovementState.cs             # novo (classe C# pura, testável)
│       ├── PlatformerMovementController.cs        # novo (MonoBehaviour: Rigidbody2D + raycasts + input)
│       └── SpriteFlipbookAnimator.cs              # novo (troca de sprite por estado, sem Animator)
├── Editor/
│   └── ProjectBootstrap.cs                        # adaptado: cria a cena MobilityTest.unity (chão, plataformas, paredes) e importa/wire os assets
└── Scenes/
    └── MobilityTest.unity                         # nova cena isolada (FR-001, FR-014)

Assets/Tests/EditMode/
├── PlatformerMovementStateTests.cs                # novo (pulo duplo, parede, energia acumulada)
```

**Structure Decision**: Continuação do projeto Unity único (Princípio V). Uma nova pasta
`Assets/Scripts/Mobility/` isola os scripts desta feature dos de `Combat`/`Demo` existentes,
refletindo que esta é uma cena de teste técnico independente (FR-014), não uma extensão dos
sistemas de Exploração/Combate.

## Complexity Tracking

*Nenhuma violação da Constitution Check acima — seção não aplicável.*
