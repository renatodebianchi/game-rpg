# Contrato: Execução de Ação em Tempo Real

Contrato de como uma `RealTimeActionDefinition` (ataque corpo a corpo/à distância, habilidade)
é iniciada, executada e resolvida por `RealTimeActionExecutor` (FR-003, FR-004, FR-005, FR-006,
FR-008, FR-009). Ver [data-model.md](../data-model.md).

## Pré-condições para iniciar uma ação

1. O combatente não pode ter outra ação pendente em andamento
   (`CombatantActionState.PendingAction == null`).
2. O cooldown da ação para este combatente deve estar zerado
   (`CooldownRemainingByActionId[actionId] <= 0`).
3. Se `ResourceCost > 0`, `CurrentTechPoints >= ResourceCost` (FR-008); caso contrário a ação
   não inicia e o recurso não é gasto.
4. Se `Kind == Ranged` e `RequiredCapabilityId` estiver definido, o combatente deve ter essa
   capacidade resolvida via `CapabilityResolver.ResolveAcquiredCapabilities` (FR-004); caso
   contrário a ação não inicia.
5. O alvo deve estar a uma distância (`Math.Abs(PositionX diferença)`) menor ou igual a `Range`
   no momento em que a ação **se resolve** (não necessariamente no momento em que inicia — ver
   regra 3 abaixo).

## Efeito ao iniciar

1. `CurrentTechPoints -= ResourceCost` (gasto imediato, não devolvido mesmo se a ação for
   interrompida depois — Edge Cases da spec).
2. `PendingAction = actionDefinition`; `PendingActionElapsed = 0`.
3. `CooldownRemainingByActionId[actionId] = Cooldown` (a recarga começa a contar já a partir do
   início da ação, não da sua conclusão).

## Efeito a cada `AdvanceTime(delta)` com uma ação pendente

1. `PendingActionElapsed += delta`.
2. Se `PendingActionElapsed >= ExecutionTime`: a ação **se resolve** — reavalia a regra 5 de
   pré-condição (alvo ainda ao alcance); se sim, aplica dano/efeito através do pipeline de
   `IDamageModifierRegistry` (mesma cadeia de `IDamageModifier` já usada pelas features
   001/002); em seguida `PendingAction = null`.
3. Se o combatente for atingido por um ataque enquanto `PendingAction != null` (antes da regra
   2 disparar): a ação é **interrompida** — `PendingAction = null` imediatamente, sem aplicar
   dano/efeito, e sem devolver o `ResourceCost` já gasto (FR-009).

## Pós-condições

- Uma ação nunca aplica dano/efeito duas vezes.
- Uma ação interrompida nunca aplica dano/efeito.
- O cooldown de uma ação sempre é aplicado ao ser iniciada, independentemente de ela ser
  concluída ou interrompida depois.

## Consumidores deste contrato

- `RealTimeActionExecutor` (implementação).
- `EnemyCombatAI` (inicia ações em nome de inimigos).
- Testes automatizados (`RealTimeActionExecutorTests`) que validam interrupção (FR-009,
  SC-002) e o gasto/recarga do recurso (FR-008).
