# Implementation Plan: Criação de Personagem (Atributos, Aparência e Equipamento Inicial)

**Branch**: `002-character-creation` | **Date**: 2026-08-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-character-creation/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Adicionar um fluxo de criação de personagem ao RPG sandbox existente: o jogador aloca 18 pontos
entre os 4 atributos já implementados (Força, Destreza, Intelecto, Vontade) usando a curva de
custo do Point Buy do D&D 5e, escolhe uma orientação (combatente ou arcanista) que determina um
kit fixo de equipamento inicial, e escolhe características visuais básicas (tipo de corpo, tom
de pele, cabelo). Abordagem técnica: reaproveitar integralmente a stack e a arquitetura já
estabelecidas na feature `001-isometric-sandbox-rpg` — mesma engine, mesmo padrão de UI
construída em runtime, mesmo `Character`/`Inventory`/`SaveData` estendidos em vez de
substituídos — conforme decisões detalhadas em [research.md](./research.md).

## Technical Context

**Language/Version**: C# (Unity 6000.5.9f1 — mesma versão da feature 001, já instalada no projeto)

**Primary Dependencies**: Unity UGUI, Unity Test Framework (nenhuma dependência nova além das já usadas na feature 001)

**Storage**: Arquivos locais em JSON via `Application.persistentDataPath` (estende `Core.SaveData`/`SaveSystem` da feature 001)

**Testing**: Unity Test Framework — EditMode (Point Buy, resolução de kit de equipamento, regras de finalização) e PlayMode (fluxo completo de criação + persistência)

**Target Platform**: PC desktop (mesmo alvo da feature 001)

**Project Type**: Extensão do mesmo projeto Unity single-player da feature 001 (não é um novo projeto)

**Performance Goals**: Mesmos da feature 001 (60 fps, <100ms de input); tela de menu sem carga adicional relevante

**Constraints**: Offline-capable; sem sistema de slots de equipamento (reutiliza `Character.Inventory` — ver research.md); sem pipeline de arte/customização visual 3D

**Scale/Scope**: 4 atributos, orçamento de 18 pontos, 2 orientações (2 kits fixos), ~18 combinações de aparência básica (2×3×3)

Todas as decisões acima foram resolvidas em [research.md](./research.md); nenhuma marcação
`NEEDS CLARIFICATION` permanece.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Avaliação contra `.specify/memory/constitution.md` (v1.0.1):

| Princípio | Avaliação | Status |
|---|---|---|
| I. Gameplay-First Design | O fluxo de criação existe para dar ao jogador controle mecânico (atributos), tático (equipamento inicial) e expressivo (aparência) sobre o personagem antes de jogar — todas as 3 histórias partem diretamente da experiência do jogador. | PASS |
| II. Modular & Data-Driven Architecture | Kits de equipamento como `ScriptableObject` (`EquipmentKitDefinition`); `AttributeAllocationState`/`CharacterCreationProfile` desacoplados de UI, testáveis isoladamente; estende `Character`/`Inventory`/`SaveData` existentes em vez de duplicá-los. | PASS |
| III. Test Coverage for Core Systems (NON-NEGOTIABLE) | Point Buy (fórmula/regra de negócio), resolução de kit de equipamento, e regras de finalização são lógica central e recebem testes EditMode/PlayMode obrigatórios (ver `quickstart.md`); a UI de criação em si (apresentação) fica fora dessa exigência, como já ocorre com os demais controladores de UI da feature 001. | PASS |
| IV. Performance & Responsiveness Budgets | Reutiliza o orçamento já definido na feature 001 (60 fps, <100ms de input); é uma tela de menu sem simulação pesada, risco de regressão desprezível. | PASS |
| V. Simplicity & Iterative Scope | Deliberadamente evita um sistema de slots de equipamento, um catálogo navegável de itens e um pipeline de customização visual 3D — nenhum dos três foi pedido pela spec, e todos foram avaliados e rejeitados em `research.md` por não terem necessidade concreta agora. | PASS |

**Resultado**: Nenhuma violação. Nenhuma entrada necessária em "Complexity Tracking".

## Project Structure

### Documentation (this feature)

```text
specs/002-character-creation/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md         # Phase 1 output (/speckit-plan command)
├── quickstart.md         # Phase 1 output (/speckit-plan command)
├── contracts/            # Phase 1 output (/speckit-plan command)
│   ├── point-buy-contract.md
│   └── character-creation-finalization-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

Mesmo projeto Unity único da feature 001 — esta feature adiciona arquivos dentro da estrutura
já existente, sem criar novos projetos/módulos.

```text
Assets/
├── Scripts/
│   ├── Characters/
│   │   ├── Character.cs                       # (feature 001) + novo campo Visuals
│   │   ├── CharacterOrientation.cs             # novo
│   │   ├── PointBuyCostTable.cs                # novo
│   │   ├── AttributeAllocationState.cs         # novo
│   │   ├── VisualCharacteristics.cs            # novo
│   │   └── CharacterCreationProfile.cs         # novo (aplica-se a um Character via Finalize())
│   ├── World/
│   │   └── EquipmentKitDefinition.cs           # novo (ScriptableObject, mesma pasta de conteúdo de mundo)
│   ├── UI/
│   │   └── CharacterCreationUI.cs              # novo (mesmo padrão runtime-UI da feature 001)
│   └── Core/
│       └── SaveData.cs                         # (feature 001) + campo visuals em CharacterSaveData
├── Data/
│   └── Equipment/                              # novo: assets EquipmentKitDefinition (combatente/arcanista)
└── Scenes/
    └── CharacterCreationDemo.unity             # novo, mesmo padrão de demo da feature 001

Assets/Tests/
├── EditMode/
│   ├── PointBuyTests.cs                        # novo
│   ├── EquipmentKitResolutionTests.cs          # novo
│   └── CharacterCreationFinalizationTests.cs   # novo
└── PlayMode/
    └── CharacterCreationFlowTests.cs           # novo
```

**Structure Decision**: Continuação do projeto Unity único já estabelecido na feature 001
(Princípio V) — os novos arquivos seguem a mesma convenção de pastas por responsabilidade
(`Characters/`, `World/`, `UI/`, `Core/`) já em uso, sem introduzir uma nova organização.

## Complexity Tracking

*Nenhuma violação da Constitution Check acima — seção não aplicável.*
