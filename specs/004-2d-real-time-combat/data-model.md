# Phase 1 Data Model: Combate em Tempo Real 2D (estilo Tales of Phantasia)

**Feature**: [spec.md](./spec.md) | **Research**: [research.md](./research.md)

Modelo conceitual das entidades introduzidas ou adaptadas por esta feature. Reutiliza
`Characters.Character`, `Characters.CharacterAttributes`, `Skills.SkillTreeService`,
`Skills.CapabilityResolver`, `Characters.HungerSystem`/`SanitySystem` (todos já existentes,
feature 001) sem alterar suas regras (FR-012).

## IRealTimeCombatant (substitui `Combat.ICombatant`)

Contrato mínimo de quem participa de um `CombatArenaEncounter`.

| Campo/Membro | Descrição | Regras/Validação |
|---|---|---|
| `CombatantId` | Identificador único | Igual ao `ICombatant` anterior |
| `CurrentHitPoints` / `MaxHitPoints` | Vida atual/máxima | Igual ao `ICombatant` anterior |
| `PositionX` | Posição contínua no eixo horizontal da arena (`float`) | Substitui `GridCoordinate Position`; sem eixo vertical em combate (FR-002) |
| `IsDefeated` | `CurrentHitPoints <= 0` | Igual ao `ICombatant` anterior |
| `ActionState` | Referência ao `CombatantActionState` deste combatente | Novo |
| `ApplyDamage(amount)` / `Heal(amount)` | Igual ao `ICombatant` anterior | — |

`Characters.Character` e `Combat.NonPlayerCombatant` passam a implementar esta interface em vez
de `ICombatant`.

## BattleArena (substitui `Combat.Grid.GridMap`)

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `MinX` / `MaxX` | Limites horizontais da arena | `MaxX > MinX` |
| `Clamp(float x)` | Restringe uma posição aos limites da arena | Usado por movimento do jogador e da IA inimiga |

Sem células, ocupação ou terreno — a arena é um intervalo contínuo (research.md, "Decision:
Arena de combate como espaço contínuo").

## RealTimeActionDefinition (dado, `ScriptableObject`)

Análogo em espírito a `Skills.SkillNodeDefinition`: conteúdo de ação de combate autorado como
dado, não hard-coded.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `ActionId` | Identificador único | Não vazio |
| `Kind` | `Melee` \| `Ranged` \| `Skill` | Determina se exige a capacidade de ataque à distância (FR-004) |
| `Range` | Alcance máximo (distância em `PositionX`) para a ação se aplicar | `>= 0` |
| `ExecutionTime` | Tempo (segundos) entre acionar a ação e ela se resolver | `>= 0`; `0` = instantânea |
| `Cooldown` | Tempo mínimo entre usos consecutivos desta ação pelo mesmo combatente | `>= 0` |
| `ResourceCost` | Custo em Pontos de Técnica (só > 0 para `Skill`, FR-008) | `>= 0` |
| `BaseDamage` | Dano base aplicado quando a ação se resolve, antes dos ajustes de `IDamageModifier` (fome/sanidade, etc.) | `>= 0` |
| `RequiredCapabilityId` | Id de capacidade da árvore de habilidades exigido (opcional, ex. ataque à distância) | Resolvido via `CapabilityResolver` quando presente |

## CombatantActionState (substitui `Combat.TurnResources`)

Estado de combate em tempo real de um `IRealTimeCombatant`.

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `MaxTechPoints` / `CurrentTechPoints` | Recurso limitado gasto por habilidades/magias (FR-008) | Regenera com o tempo (research.md); nunca excede o máximo |
| `CooldownRemainingByActionId` | Tempo de recarga restante por `RealTimeActionDefinition` | Decrementado por `AdvanceTime` |
| `PendingAction` | A `RealTimeActionDefinition` em execução/conjuração, se houver | `null` quando nenhuma ação está em andamento |
| `PendingActionElapsed` | Tempo decorrido da ação pendente | Comparado a `ExecutionTime` para saber se já concluiu |
| `IsChannelingFlee` / `FleeChannelElapsed` | Estado do canal de fuga (FR-013) | Reseta se o combatente para de se mover em direção à borda |

## RealTimeActionExecutor (substitui `Combat.ActionResolver`)

Não é uma entidade de dados, mas o serviço que: inicia uma ação (valida alcance/recurso/
cooldown), avança seu `ExecutionTime` via `AdvanceTime(TimeSpan)`, e — se não interrompida por
dano recebido antes da conclusão (FR-009) — aplica o dano/efeito através do mesmo pipeline de
`IDamageModifierRegistry` (ver abaixo) já usado pelas features 001/002 (fome/sanidade,
capacidades de habilidade).

## IDamageModifierRegistry (novo, extraído de `Combat.ActionResolver`)

Interface mínima (`RegisterDamageModifier(IDamageModifier)`) implementada tanto pelo antigo
`ActionResolver` (removido) quanto pelo novo `RealTimeActionExecutor`, para que
`Skills.CapabilityResolver.ApplyAcquiredCapabilities` e os testes de `HungerSystem`/
`SanitySystem` continuem funcionando sem conhecer o executor concreto (FR-012).

## CombatArenaEncounter (substitui `Combat.CombatEncounter`)

| Campo | Descrição | Regras/Validação |
|---|---|---|
| `State` | `NotStarted` \| `InProgress` \| `WonByPlayer` \| `PlayerFled` \| `PlayerDefeated` | Mesmos valores terminais da feature 001; sem `CurrentTurnIndex`/`InitiativeOrder` |
| `Participants` | Jogador + inimigos | Sem "lado do jogador" com múltiplos membros (Assumptions: sem grupo nesta feature) |
| `AdvanceTime(TimeSpan delta)` | Avança cooldowns, ações pendentes e IA de todos os participantes | Chamado a cada frame por um MonoBehaviour (mesmo padrão de `WorldClock.Advance`) |
| `ApplyDamage(target, amount)` | Aplica dano e reavalia o estado terminal | Igual em espírito ao `CombatEncounter` anterior |

Eventos `ParticipantDamaged`/`ParticipantDefeated` preservados para `CombatOutcomeHandler` (que
é mantido, apenas adaptado ao novo tipo de encontro).

## EnemyCombatAI (substitui `Combat.EnemyAI`)

Não é uma entidade de dados, mas o comportamento: a cada `Tick(TimeSpan delta)`, decide mover em
linha reta em direção ao alvo vivo mais próximo do lado do jogador, ou executar uma ação (via
`RealTimeActionExecutor`) quando dentro do alcance dela. Sem pathfinding (research.md).

## RealTimeFleeAction (substitui `Combat.FleeAction`)

Não é uma entidade de dados, mas o serviço: acompanha o canal de fuga
(`CombatantActionState.IsChannelingFlee/FleeChannelElapsed`) e, ao atingir a duração mínima
exigida, calcula a chance de sucesso com a mesma fórmula de `FleeAction` (distância ao hostil
mais próximo + Destreza), adaptada para `float` em vez de `GridCoordinate`.

## BoundedFollowCamera (comportamento, não dado)

Componente MonoBehaviour compartilhado pela Exploração e pela arena de combate: centraliza um
alvo na câmera, restringindo a posição da câmera a um retângulo de limites de mundo derivado dos
limites do mapa/arena e do tamanho ortográfico da câmera (FR-015). Presentation-only — isento do
requisito de teste automatizado do Princípio III, como já vale para `DemoCameraController`.

## Diagrama de relações (conceitual)

```
BattleArena --limita--> IRealTimeCombatant.PositionX
CombatArenaEncounter --contém--> IRealTimeCombatant (Character, NonPlayerCombatant)
IRealTimeCombatant --possui--> CombatantActionState
RealTimeActionExecutor --lê/gasta--> CombatantActionState --executa--> RealTimeActionDefinition
RealTimeActionExecutor --implementa--> IDamageModifierRegistry <--registra-- CapabilityResolver / HungerSystem / SanitySystem
EnemyCombatAI --usa--> RealTimeActionExecutor + BattleArena
RealTimeFleeAction --lê--> CombatantActionState (canal de fuga)
BoundedFollowCamera --segue--> IRealTimeCombatant do jogador, limitado por BattleArena (combate) ou pelos limites do mapa (Exploração)
```
