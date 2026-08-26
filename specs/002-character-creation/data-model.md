# Phase 1 Data Model: Criação de Personagem

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

Modelo conceitual das entidades introduzidas por esta feature, e como elas se conectam às
entidades já existentes na feature `001-isometric-sandbox-rpg`
(`specs/001-isometric-sandbox-rpg/data-model.md`) — em particular `Character` e `Resource`.

## CharacterOrientation (enum)

Orientação predominante escolhida na criação, usada **apenas** para determinar o kit de
equipamento inicial (FR-004). Não tem relação com `Skills.SkillTrack` e não é persistida após a
criação — é consumida uma vez e descartada.

| Valor | Descrição |
|---|---|
| `Combatant` | Kit de equipamento inicial voltado a combate físico |
| `Arcanist` | Kit de equipamento inicial voltado a magia/intelecto |

## PointBuyCostTable (dados estáticos)

Tabela de custo cumulativo, a partir do valor base 8, para elevar um atributo até cada valor
entre 9 e 15 (research.md, "Decision: Tabela de custo do Point Buy").

| Valor do atributo | Custo cumulativo (a partir de 8) |
|---|---|
| 8 | 0 |
| 9 | 1 |
| 10 | 2 |
| 11 | 3 |
| 12 | 4 |
| 13 | 5 |
| 14 | 7 |
| 15 | 9 |

**Regras**: nenhum atributo pode ficar abaixo de 8 nem acima de 15 durante a criação (FR-002).
Orçamento total: 18 pontos (research.md / spec.md Assumptions).

## AttributeAllocationState

Estado mutável, mantido apenas durante o fluxo de criação (antes da finalização), representando
quanto do orçamento de Point Buy foi investido em cada um dos quatro atributos já existentes em
`Characters.CharacterAttributes` (feature 001): `Strength`, `Dexterity`, `Intellect`,
`Willpower`.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `scores` | Valor atual de cada um dos 4 atributos | Cada valor entre 8 e 15 (FR-002) |
| `totalBudget` | Orçamento total de pontos (18) | Constante para esta versão |
| `pointsSpent` | Soma dos custos (via `PointBuyCostTable`) de todos os 4 atributos no estado atual | Derivado; não pode exceder `totalBudget` |
| `pointsRemaining` | `totalBudget - pointsSpent` | Deve ser exatamente 0 para permitir finalizar (FR-003) |

## EquipmentKitDefinition (conteúdo, ScriptableObject)

Conteúdo autoral (não runtime) definindo o kit fixo de itens iniciais de uma orientação
(FR-005; research.md).

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `orientation` | `CharacterOrientation` a que este kit pertence | Único kit ativo por orientação nesta versão |
| `items` | Lista de pares (`Resource` referenciado por id, quantidade) | Cada quantidade > 0 |

**Regra de resolução**: ao finalizar a criação, o sistema resolve o `EquipmentKitDefinition` cuja
`orientation` casa com a escolha do jogador e adiciona cada entrada ao `Character.Inventory` via
`Inventory.Add(resourceId, quantity)` (feature 001).

## VisualCharacteristics

Escolhas cosméticas puramente descritivas, sem efeito em atributos/combate/habilidades
(FR-006, FR-007).

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `bodyType` | Tipo de corpo (enum, 2 valores) | Se não escolhido, usa o valor padrão (FR-007) |
| `skinTone` | Tom de pele (enum, 3 valores) | Se não escolhido, usa o valor padrão (FR-007) |
| `hairStyle` | Estilo de cabelo (enum, 3 valores) | Se não escolhido, usa o valor padrão (FR-007) |
| `hairColor` | Cor do cabelo (valor de cor livre) | Se não escolhido, usa o valor padrão (FR-007) |

## CharacterCreationProfile

Agrega o estado das três histórias de usuário durante o fluxo de criação, até a finalização
(FR-008, FR-009). É um objeto transitório — não é persistido por si só; seu resultado (após
`Finalize()`) é aplicado a um `Characters.Character` e a esse ponto o `CharacterCreationProfile`
deixa de ser necessário.

| Campo | Descrição |
|---|---|
| `attributeAllocation` | `AttributeAllocationState` atual |
| `orientation` | `CharacterOrientation` escolhida (nula/indefinida até o jogador escolher) |
| `visualCharacteristics` | `VisualCharacteristics` atual (usa padrões até serem alteradas) |

**Regra de finalização** (`Finalize()`, FR-010, FR-012):

1. Falha (não finaliza) se `attributeAllocation.pointsRemaining != 0` (FR-003).
2. Aplica `attributeAllocation.scores` a `Character.Attributes` (campo já existente,
   `CharacterAttributes`, com setter público — feature 001).
3. Resolve o `EquipmentKitDefinition` correspondente a `orientation` e adiciona seus itens ao
   `Character.Inventory`.
4. Aplica `visualCharacteristics` ao novo campo `Character.Visuals` (extensão desta feature —
   ver abaixo).
5. A partir daqui, `Character.Attributes` é tratado como fixo pelo restante da campanha
   (FR-012) — nenhuma API de "redistribuir atributos" é exposta após a finalização.

## Extensão de `Characters.Character` (feature 001)

- **Novo campo**: `Visuals` (tipo `VisualCharacteristics`), com setter controlado apenas pelo
  fluxo de criação de personagem (`CharacterCreationProfile.Finalize()`).
- Nenhum campo existente de `Character` é removido ou renomeado.

## Extensão de `Core.SaveData` (feature 001, contracts/save-data-contract.md)

- **Novo campo em `CharacterSaveData`**: `visuals` (serializando `bodyType`, `skinTone`,
  `hairStyle`, `hairColorHex`), persistido/restaurado junto com o restante do estado do
  personagem (FR-011). O kit de equipamento inicial não precisa de um campo próprio no save —
  seus itens já ficam representados dentro de `CharacterSaveData.inventory` (existente), pois
  passam a ser apenas itens normais do inventário assim que atribuídos.

## Diagrama de relações (conceitual)

```
CharacterCreationProfile 1---1 AttributeAllocationState
CharacterCreationProfile 1---1 VisualCharacteristics
CharacterCreationProfile 1---0..1 CharacterOrientation
CharacterCreationProfile --Finalize()--> Characters.Character (feature 001)
CharacterOrientation 1---1 EquipmentKitDefinition (conteúdo)
EquipmentKitDefinition 1---* Resource (feature 001, por id)
Characters.Character 1---1 VisualCharacteristics (campo Visuals, novo)
```
