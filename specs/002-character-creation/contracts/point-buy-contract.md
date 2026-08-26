# Contrato: Point Buy (alocação de atributos)

Contrato do módulo que valida e aplica a alocação de pontos de atributo durante a criação de
personagem (FR-002, FR-003). Ver [data-model.md](../data-model.md#attributeallocationstate).

## Entrada

| Campo | Descrição |
|---|---|
| `currentScores` | Valor atual dos 4 atributos (`AttributeAllocationState.scores`) |
| `attributeToChange` | Qual dos 4 atributos está sendo alterado |
| `requestedScore` | Novo valor solicitado para esse atributo |

## Saída

| Campo | Descrição |
|---|---|
| `accepted` | `true` se a mudança é válida e foi aplicada; `false` caso contrário |
| `updatedScores` | Novo conjunto de valores (igual ao anterior se `accepted == false`) |
| `pointsRemaining` | Pontos restantes do orçamento após a mudança |

## Regras do contrato

1. `requestedScore` DEVE estar entre 8 e 15, inclusive. Fora desse intervalo, a mudança é
   rejeitada (`accepted = false`) sem alterar `currentScores` (FR-002, edge case de tentar
   descer abaixo de 8 ou subir acima de 15).
2. O custo da mudança é `PointBuyCostTable[requestedScore] - PointBuyCostTable[currentScore]`
   para o atributo alterado (pode ser negativo, se o jogador está reduzindo o valor para
   recuperar pontos antes de finalizar — a spec permite revisar escolhas via FR-009).
3. A mudança só é aceita se `pointsSpent` resultante (soma de todos os 4 atributos via
   `PointBuyCostTable`) não exceder `totalBudget` (18). Se exceder, `accepted = false` e nada
   muda.
4. `Finalize()` (ver
   [character-creation-finalization-contract.md](./character-creation-finalization-contract.md))
   só é permitido quando `pointsRemaining == 0` — não quando `pointsRemaining >= 0` — ou seja,
   o jogador é obrigado a gastar exatamente o orçamento, não apenas respeitar o teto (FR-003:
   "impedir a finalização... enquanto restarem pontos... não gastos").
5. Este módulo DEVE ser testável (EditMode) sem depender de UI ou de uma cena Unity carregada —
   opera inteiramente sobre `AttributeAllocationState` em memória.

## Consumidores deste contrato

- `CharacterCreationUI` (chama a cada interação do jogador com os controles de atributo).
- `CharacterCreationProfile.Finalize()` (verifica `pointsRemaining == 0` antes de prosseguir).
