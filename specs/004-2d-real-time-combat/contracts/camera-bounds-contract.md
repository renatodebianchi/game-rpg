# Contrato: Câmera com Clamp nas Bordas

Contrato de `BoundedFollowCamera`, compartilhado pela Exploração e pela arena de combate
(FR-015, SC-006). Ver [data-model.md](../data-model.md).

## Entrada

| Campo | Descrição |
|---|---|
| `target` | `Transform` a seguir (o personagem do jogador) |
| `worldBounds` | Retângulo de limites de mundo (min/max X e, quando aplicável, Y) do mapa/arena atual |

## Regras do contrato

1. A cada frame, a posição desejada da câmera é a posição do alvo (X, e Y quando o eixo
   vertical se aplica — Exploração), mantendo a profundidade (Z) fixa da câmera.
2. Essa posição desejada é restringida (`Mathf.Clamp`) para que a metade visível da câmera
   (`orthographicSize` × aspect para X, `orthographicSize` para Y) nunca ultrapasse
   `worldBounds` — ou seja, a borda do mapa/arena é sempre a última coisa visível, nunca havendo
   espaço vazio além dela (SC-006).
3. Quando `worldBounds` é menor que o campo de visão da câmera em algum eixo (mapa/arena menor
   que a tela), a câmera fica centralizada nesse eixo em vez de tentar seguir o alvo (evita
   tremor/oscilação nos limites).
4. O comportamento é idêntico entre a Exploração e a arena de combate — a única diferença é qual
   `worldBounds` é fornecido a cada cena.

## Consumidores deste contrato

- `BoundedFollowCamera` (implementação).
- `ExplorationCharacterController`/cena de Exploração (fornece os limites do mapa).
- `BattleArenaDemoController`/cena de combate (fornece os limites de `BattleArena`).

## Nota

Este contrato é presentation-only (câmera) — isento da exigência de teste automatizado do
Princípio III, validado manualmente via [quickstart.md](../quickstart.md).
