# Contrato: Simulação de Economia/População de Comunidade (Vila)

Contrato do módulo de simulação que conecta `Resource`, `Community` e `NPC` (ver
[data-model.md](../data-model.md#community--faction-comunidadefacção--inclui-vilas)), definindo
a interface entre o sistema de mundo/tempo e o sistema de reputação/consequências (FR-013,
FR-014, FR-015).

## Entrada (input) por "tick" de simulação

| Campo | Descrição |
|---|---|
| `communityId` | Comunidade avaliada neste tick |
| `elapsedSimulatedTime` | Quanto tempo in-game se passou desde o último tick |
| `essentialResourceStock` | Estoque atual de recursos essenciais da comunidade |
| `populationNpcIds` | População atual da comunidade |

## Saída (output) por "tick" de simulação

| Campo | Descrição |
|---|---|
| `updatedEssentialResourceStock` | Estoque após consumo proporcional à população |
| `npcsTransitionedToDead[]` | NPCs que morreram de fome neste tick (estoque insuficiente) |
| `updatedEconomyState` | Novo estado econômico derivado (ex.: faixa de preços/disponibilidade) |
| `isPermanentlyInactive` | `true` se a população da comunidade chegou a zero neste tick ou em um tick anterior (ver regra 6) |

## Regras do contrato

1. O consumo de `essentialResourceStock` é proporcional à população viva da comunidade no início
   do tick — nunca negativo, nunca superior ao estoque disponível (consumo é limitado ao que
   existe; déficit é o que aciona a transição de NPCs para `Dead`).
2. Uma comunidade só pode perder população por fome se `essentialResourceStock` de recursos
   marcados `isEssential` estiver abaixo do limiar de sustentação por um período contínuo
   definido (o valor exato do limiar/período é um parâmetro de balanceamento, não uma decisão de
   arquitetura — ver `/speckit-tasks` para o valor inicial).
3. Repor `essentialResourceStock` (via `ImpactfulChoice` do tipo `TransportResource`) DEVE
   interromper a transição de NPCs para `Dead` a partir do próximo tick — o dano populacional já
   ocorrido não é revertido automaticamente pela reposição (é permanente, conforme Edge Case da
   spec).
4. Este módulo NÃO decide diretamente a reputação do jogador — ele apenas atualiza o estado da
   comunidade. A atualização de `reputationWithPlayer` é responsabilidade do consumidor do log de
   `ImpactfulChoice` (separação de responsabilidades exigida pelo Princípio II).
5. O módulo DEVE ser executável e testável (EditMode) sem depender de uma cena Unity carregada
   ou de renderização — apenas dos dados de `Community`/`Resource`/`NPC` em memória.
6. Quando `populationNpcIds` chega a vazio (0% de população) neste tick, o módulo DEVE marcar
   `isPermanentlyInactive = true` e a comunidade NÃO DEVE mais ser processada por ticks
   subsequentes desta simulação — mesmo que `essentialResourceStock` seja reposto depois. Este é
   um estado terminal e irreversível (FR-019).

## Consumidores deste contrato

- Loop de simulação de mundo (avança o tempo in-game e aciona ticks por comunidade).
- Sistema de reputação/consequências (reage a `npcsTransitionedToDead` e `updatedEconomyState`
  para refletir mudanças em diálogo/comércio de NPCs, conforme FR-016).
