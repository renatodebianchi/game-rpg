# Contrato: Canal de Fuga em Tempo Real

Contrato de como fugir de um `CombatArenaEncounter` sem depender de uma ação de menu de turno
(FR-013, Edge Cases). Ver [data-model.md](../data-model.md).

## Constantes

- `MinChannelDuration`: duração contínua mínima (segundos) segurando o movimento em direção a
  uma borda da arena antes de a fuga ser tentada.
- Mesma faixa de chance de sucesso (`MinSuccessChance`/`MaxSuccessChance`) já usada por
  `FleeAction` na feature 001.

## Efeito a cada `AdvanceTime(delta)`

1. Se o jogador está segurando o comando de movimento em direção a uma borda da arena **e**
   já está a uma distância pequena o suficiente dela (definida pela implementação):
   `FleeChannelElapsed += delta`; `IsChannelingFlee = true`.
2. Se o jogador parar de se mover em direção à borda (soltar o comando, mudar de direção, ou ser
   atingido): `FleeChannelElapsed = 0`; `IsChannelingFlee = false` — o canal reseta, não pausa.
3. Quando `FleeChannelElapsed >= MinChannelDuration`: a fuga é **tentada** uma única vez —
   calcula a chance de sucesso (distância ao hostil vivo mais próximo + Destreza, mesma fórmula
   de `FleeAction.CalculateSuccessChance` adaptada a `float`) e sorteia o resultado.
   - Sucesso: `CombatArenaEncounter.State = PlayerFled`.
   - Falha: `IsChannelingFlee = false`; `FleeChannelElapsed = 0` (o jogador pode tentar de novo).

## Pós-condições

- Uma tentativa de fuga nunca é avaliada antes de `MinChannelDuration` ser atingida.
- Uma fuga bem-sucedida sempre encerra o encontro em `PlayerFled`.
- Interromper o canal antes de completar nunca conta como uma tentativa (não gasta nada, não
  tem custo além do tempo perdido).

## Consumidores deste contrato

- `RealTimeFleeAction` (implementação).
- Testes automatizados (`RealTimeFleeActionTests`).
