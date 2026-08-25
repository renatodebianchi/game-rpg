# Phase 1 Data Model: RPG Sandbox com Árvore de Habilidades, Combate Tático e Mundo Reativo

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

Modelo conceitual (independente de implementação) das entidades identificadas na spec. Campos
descrevem o *quê*, não o tipo concreto em C#/Unity — isso é decidido durante `/speckit-tasks` /
implementação, respeitando o Princípio II (dados desacoplados de lógica de apresentação).

## Character (Personagem do Jogador)

Representa o avatar controlado pelo jogador.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador único do personagem | Obrigatório, único por save |
| `attributes` | Atributos base (físicos e místicos/intelectuais) | Usados por fórmulas de combate e por pré-requisitos de nós de habilidade |
| `combatResources` | Vida atual/máxima, recursos de turno (movimento, ação, ação bônus) | Resetados no início de cada turno próprio (FR-002) |
| `hunger` | Indicador de fome (0–100, 0 = saciado) | Aumenta com o tempo/atividade; limiares críticos aplicam penalidades (FR-008, FR-009) |
| `sanity` | Indicador de sanidade (0–100, 100 = estável) | Reduzido por eventos perturbadores; limiares críticos aplicam efeitos negativos (FR-010, FR-011) |
| `skillPoints` | Pontos de habilidade disponíveis e investidos | Investimento decrementa disponíveis e marca nó como adquirido (FR-005, FR-006). Respec (FR-018) devolve pontos ao desfazer um nó adquirido, sem custo e sem limite de frequência. |
| `acquiredSkillNodeIds` | Conjunto de nós de habilidade adquiridos | Não pode conter duplicatas (FR-006). Um nó pode ser removido deste conjunto via respec (FR-018), devolvendo seu `cost` a `skillPoints` disponíveis |
| `inventory` | Recursos/itens portados pelo personagem | Usado para transporte de recursos entre locais (FR-015) |
| `reputationByFaction` | Referência ao estado de reputação por facção/comunidade | Ver entidade `FactionReputation` |

**Regras de estado**: `hunger` e `sanity` transitam por faixas (normal → alerta → crítico); cada
transição de faixa é o gatilho para aplicar/remover penalidades (não a variação contínua do
valor). Quando ambas as faixas estão em nível crítico simultaneamente, as penalidades de `hunger`
e `sanity` são aplicadas cumulativamente, sem teto combinado (FR-021).

**Regra de respec (FR-018)**: remover um `SkillNode` de `acquiredSkillNodeIds` que seja
pré-requisito de outro nó ainda presente no conjunto exige remover também esse nó dependente em
cascata (não é permitido deixar `acquiredSkillNodeIds` em um estado que viole os `prerequisites`
declarados de algum nó nele contido).

## SkillNode (Nó de Habilidade)

Unidade individual da árvore de habilidades.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador único do nó | Obrigatório, único |
| `track` | Trilha à qual pertence: `Combatant`, `Arcanist`, ou `Hybrid` | FR-004 |
| `prerequisites` | Lista de `SkillNode.id` exigidos antes deste nó ficar disponível | Nós `Hybrid` exigem pré-requisitos de ambas as trilhas (FR-005) |
| `cost` | Custo em pontos de habilidade | > 0 |
| `grantedCapability` | Capacidade concedida (nova ação de combate, bônus passivo, interação especial no mundo) | FR-007 |

**Relações**: `SkillNode` → `SkillNode` (auto-relacionamento via `prerequisites`, formando um
grafo acíclico dirigido/árvore).

## CombatEncounter (Encontro de Combate)

Instância de combate por turnos.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador da instância de encontro | Único por sessão |
| `gridMap` | Referência ao grid lógico do encontro (dimensões, obstáculos, terreno) | Desacoplado da representação visual (ver research.md) |
| `participants` | Lista ordenada de combatentes (jogador, aliados, inimigos) com posição no grid | Não pode haver duas entidades na mesma célula |
| `initiativeOrder` | Ordem de turnos calculada no início do encontro | FR-001 |
| `state` | `NotStarted` \| `InProgress` \| `WonByPlayer` \| `PlayerFled` \| `PlayerDefeated` | Transições definem o desfecho e a consequência aplicada (FR-003) |
| `turnResourcesByParticipant` | Recursos de turno restantes por participante no turno atual | Resetado a cada novo turno do participante (FR-002) |

## Enemy / Ally Combatant (Combatente não-jogador em combate)

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador do combatente na instância de encontro | Único dentro do encontro |
| `combatResources` | Vida, recursos de turno, capacidades disponíveis em combate | Mesma mecânica de turno do `Character` |
| `linkedNpcId` | (opcional) referência a um `NPC` do mundo, quando o combatente representa um NPC nomeado | Usado para casos de borda (NPC amigo forçado a lutar). Se este combatente sofrer dano ou morrer durante o encontro, DEVE gerar um `ImpactfulChoice` do tipo `AbandonOrHarmNpc` contra `linkedNpcId`, com o mesmo efeito de reputação de uma escolha deliberada (FR-022) |

## NPC

Personagem não-jogável pertencente a uma comunidade.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador único do NPC | Obrigatório, único |
| `communityId` | Comunidade/facção à qual pertence | Ver entidade `Community` |
| `lifeState` | `Alive` \| `Rescued` \| `Dead` \| `AtRisk` | Alterado por `ImpactfulChoice` (FR-013) e por fome extrema da comunidade (FR-014) |
| `availableInteractions` | Diálogo/missões/comércio disponíveis, condicionados à reputação do jogador com `communityId` | FR-016 |

## Community / Faction (Comunidade/Facção — inclui Vilas)

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador único da comunidade | Obrigatório, único |
| `name` | Nome exibido (ex.: nome da vila) | — |
| `reputationWithPlayer` | Nível de reputação do jogador com esta comunidade | Aumenta/diminui via `ImpactfulChoice` (FR-012); totalmente independente da reputação de qualquer outra `Community`, mesmo rivais (FR-020) |
| `essentialResourceStock` | Estoque atual de recursos essenciais (ex.: alimento) | Consumido ao longo do tempo simulado; abaixo do limiar de sustentação, dispara perda de população (FR-014) |
| `populationNpcIds` | Lista de `NPC.id` pertencentes a esta comunidade | Diminui conforme NPCs transitam para `Dead` por fome |
| `economyState` | Estado econômico derivado (ex.: preços/disponibilidade de bens) | Deriva de `essentialResourceStock` e `populationNpcIds` (FR-014, FR-015) |
| `isPermanentlyInactive` | Indica se a comunidade atingiu o estado terminal de colapso | Passa a `true` de forma irreversível quando `populationNpcIds` chega a vazio (0% de população); uma vez `true`, a comunidade não oferece mais comércio/missões/NPCs pelo resto da campanha, mesmo que `essentialResourceStock` seja reposto (FR-019) |

**Regra de simulação**: a cada intervalo de tempo simulado definido (a detalhar em
`/speckit-tasks`), cada `Community` com `isPermanentlyInactive == false` consome recursos de
`essentialResourceStock` proporcionalmente à sua população; estoque insuficiente reduz população
(NPCs → `Dead`) e degrada `economyState`; reposição de recursos (via transporte do jogador)
interrompe degradações futuras, mas não reverte perdas populacionais já ocorridas (FR-015). Ao
atingir população zero, `isPermanentlyInactive` é definido como `true` e a comunidade deixa de
ser processada por este tick de simulação (FR-019).

## Resource

Bem essencial ou comerciável.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador do tipo de recurso (ex.: "food") | Obrigatório, único |
| `isEssential` | Indica se afeta diretamente a sobrevivência de uma comunidade | Usado pela regra de simulação de `Community` |
| `quantity` | Quantidade transportável/armazenável (em inventário ou estoque de comunidade) | ≥ 0 |

## ImpactfulChoice (Escolha de Impacto)

Registro de uma decisão relevante do jogador, usado para auditar/derivar consequências.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `id` | Identificador único do registro | Obrigatório, único |
| `type` | `SaveNpc` \| `AbandonOrHarmNpc` \| `TransportResource` | FR-013 |
| `targetCommunityId` | Comunidade afetada pela escolha | Usado para aplicar delta de reputação (FR-012) |
| `relatedNpcId` | (opcional) NPC diretamente envolvido | Aplicável a `SaveNpc`/`AbandonOrHarmNpc` |
| `relatedResourceId` / `quantity` | (opcional) recurso e quantidade movida | Aplicável a `TransportResource` |
| `timestamp` (in-game) | Momento simulado em que a escolha ocorreu | Usado para calcular quando as consequências (FR-014/FR-015) devem se manifestar |

## FactionReputation (projeção auxiliar)

Vista derivada, por personagem, da reputação com cada `Community` — pode ser modelada como
parte do `Character` (`reputationByFaction`) referenciando `Community.reputationWithPlayer`, ou
como entidade própria caso múltiplos personagens/saves precisem coexistir. Para o MVP
(single-player, um personagem por save), é tratada como parte do estado do `Character`.

## Diagrama de relações (conceitual)

```
Character 1---* SkillNode (via acquiredSkillNodeIds)
Character 1---1 CombatEncounter (participante)
CombatEncounter 1---* Enemy/Ally Combatant
Enemy/Ally Combatant 0..1---1 NPC (linkedNpcId)
NPC *---1 Community
Community 1---* Resource (essentialResourceStock)
Character 1---* ImpactfulChoice
ImpactfulChoice *---1 Community (targetCommunityId)
ImpactfulChoice 0..1---1 NPC (relatedNpcId)
```
