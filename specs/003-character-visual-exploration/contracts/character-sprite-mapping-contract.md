# Contrato: Mapeamento de Características Visuais para o Sprite

Contrato do módulo que traduz `Characters.VisualCharacteristics` (feature 002) em uma
aparência de sprite concreta na Exploração (FR-005). Ver
[data-model.md](../data-model.md#charactersspritemapping-dados-estáticos).

## Entrada

| Campo | Descrição |
|---|---|
| `visuals` | `Characters.VisualCharacteristics` do personagem a ser exibido |

## Saída

| Campo | Descrição |
|---|---|
| `spriteFrame` | Referência ao sprite/frame do spritesheet do Kenney a ser usado |
| `tintColor` | Cor (`Color`) a aplicar via `SpriteRenderer.color` sobre `spriteFrame` |

## Regras do contrato

1. Para cada valor de `SkinTone` (Light/Medium/Dark) DEVE existir um `tintColor`
   correspondente, fixo e determinístico — a mesma entrada de `SkinTone` sempre produz o mesmo
   `tintColor`.
2. Se existir um frame do spritesheet distinto o suficiente para representar `BodyType`, ele
   DEVE ser usado; caso contrário, o frame padrão é usado sem que isso seja tratado como erro
   (fallback documentado, não uma falha de conteúdo).
3. Este módulo NUNCA lança exceção por uma combinação de `VisualCharacteristics` não ter uma
   representação visual "perfeita" — sempre retorna uma combinação válida de
   `spriteFrame`/`tintColor` (princípio de "melhor esforço" da spec).
4. Este módulo DEVE ser testável (EditMode) sem depender de uma cena Unity carregada — opera
   sobre `VisualCharacteristics` e retorna dados, sem tocar em `GameObject`/`SpriteRenderer`
   diretamente (quem aplica ao `SpriteRenderer` é o `ExplorationCharacterController`, não este
   módulo).

## Consumidores deste contrato

- `ExplorationCharacterController` (aplica o resultado ao `SpriteRenderer` do personagem na
  Exploração).
