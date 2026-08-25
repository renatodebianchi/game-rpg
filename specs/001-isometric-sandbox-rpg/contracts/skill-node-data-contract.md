# Contrato: SkillNode (ScriptableObject de conteúdo)

Contrato entre o conteúdo de design (assets `ScriptableObject` de nós de habilidade, criados por
designers no Editor Unity) e o código que consome esses dados (árvore de habilidades, combate).
Ver [data-model.md](../data-model.md#skillnode-nó-de-habilidade) para a entidade completa.

## Campos obrigatórios

| Campo | Tipo conceitual | Obrigatório | Observação |
|---|---|---|---|
| `id` | string única | Sim | Chave estável, não deve mudar após publicado (referenciada por saves) |
| `displayName` | string | Sim | Nome exibido na UI da árvore |
| `track` | enum: `Combatant` \| `Arcanist` \| `Hybrid` | Sim | Determina agrupamento visual na árvore |
| `prerequisites` | lista de `id` de outros `SkillNode` | Não (pode ser vazia) | Nós `Hybrid` DEVEM ter ao menos um pré-requisito de cada trilha (`Combatant` e `Arcanist`) |
| `cost` | inteiro > 0 | Sim | Pontos de habilidade consumidos ao adquirir |
| `grantedCapabilityId` | referência a uma capacidade (ação de combate, passiva, interação de mundo) | Sim | Deve resolver para uma capacidade implementada; falha de resolução é erro de conteúdo, não deve travar o jogo silenciosamente |

## Invariantes (validadas em EditMode test / validação de conteúdo)

1. Não deve haver ciclos no grafo de `prerequisites` entre `SkillNode`s.
2. Todo `id` referenciado em `prerequisites` DEVE existir como um `SkillNode` publicado.
3. Um nó `Hybrid` sem pré-requisitos de ambas as trilhas é conteúdo inválido (falha de validação
   de conteúdo, não erro silencioso em runtime).
4. `grantedCapabilityId` duplicado entre dois `SkillNode`s distintos é permitido apenas se
   intencional (ex.: dois caminhos alternativos para a mesma capacidade); não deve ser tratado
   como erro automático, mas deve ser sinalizado em revisão de conteúdo.

## Consumidores deste contrato

- Sistema de árvore de habilidades (exibição, validação de disponibilidade, investimento).
- Sistema de combate (resolução de `grantedCapabilityId` em ações utilizáveis).
- Save/Load (persiste apenas `acquiredSkillNodeIds`, nunca o conteúdo do `SkillNode` em si).
