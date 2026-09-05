using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class RealTimeActionExecutorTests
    {
        private static (CombatArenaEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy) CreateStartedEncounter()
        {
            var player = new NonPlayerCombatant("player", maxHitPoints: 20, maxTechPoints: 10) { PositionX = 0f };
            var enemy = new NonPlayerCombatant("enemy", maxHitPoints: 20, maxTechPoints: 0) { PositionX = 1f };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { enemy });
            encounter.Start();
            return (encounter, player, enemy);
        }

        [Test]
        public void TryStartAction_InstantAction_ResolvesImmediatelyOnResolvePendingActions()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var melee = RealTimeActionDefinition.CreateForTesting("melee", RealTimeActionKind.Melee, range: 2f, executionTime: 0f, baseDamage: 5);

            var started = executor.TryStartAction(player, melee, enemy);
            executor.ResolvePendingActions(encounter.Participants);

            Assert.IsTrue(started);
            Assert.AreEqual(15, enemy.CurrentHitPoints);
            Assert.IsFalse(player.ActionState.HasPendingAction);
        }

        [Test]
        public void ResolvePendingActions_BeforeExecutionTimeElapsed_DoesNotResolveYet()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var castTime = RealTimeActionDefinition.CreateForTesting("cast", RealTimeActionKind.Skill, range: 2f, executionTime: 1f, baseDamage: 10);

            executor.TryStartAction(player, castTime, enemy);
            executor.ResolvePendingActions(encounter.Participants); // elapsed 0 < executionTime 1

            Assert.AreEqual(20, enemy.CurrentHitPoints, "Damage must not apply before the execution time elapses.");
            Assert.IsTrue(player.ActionState.HasPendingAction);
        }

        [Test]
        public void ResolvePendingActions_AfterExecutionTimeElapsed_AppliesDamageOnce()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var castTime = RealTimeActionDefinition.CreateForTesting("cast", RealTimeActionKind.Skill, range: 2f, executionTime: 1f, baseDamage: 10);

            executor.TryStartAction(player, castTime, enemy);
            encounter.AdvanceTime(System.TimeSpan.FromSeconds(1.5));
            executor.ResolvePendingActions(encounter.Participants);

            Assert.AreEqual(10, enemy.CurrentHitPoints);
            Assert.IsFalse(player.ActionState.HasPendingAction);
        }

        [Test]
        public void PendingAction_InterruptedByDamage_NeverAppliesItsEffect()
        {
            // FR-009 / SC-002.
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var castTime = RealTimeActionDefinition.CreateForTesting("cast", RealTimeActionKind.Skill, range: 2f, executionTime: 5f, resourceCost: 3f, baseDamage: 999);

            executor.TryStartAction(player, castTime, enemy);
            Assert.AreEqual(7f, player.ActionState.CurrentTechPoints, "Resource cost is spent immediately on start.");

            encounter.ApplyDamage(player, 1); // interrupts the pending cast.
            Assert.AreEqual(7f, player.ActionState.CurrentTechPoints, "The resource already spent is not refunded by interruption.");
            Assert.IsFalse(player.ActionState.HasPendingAction);

            encounter.AdvanceTime(System.TimeSpan.FromSeconds(10));
            executor.ResolvePendingActions(encounter.Participants);

            Assert.AreEqual(20, enemy.CurrentHitPoints, "An interrupted action must never apply its effect.");
        }

        [Test]
        public void TryStartAction_NotEnoughTechPoints_Fails()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var expensive = RealTimeActionDefinition.CreateForTesting("expensive", RealTimeActionKind.Skill, resourceCost: 999f);

            var started = executor.TryStartAction(player, expensive, enemy);

            Assert.IsFalse(started);
            Assert.IsFalse(player.ActionState.HasPendingAction);
        }

        [Test]
        public void TryStartAction_WhileOnCooldown_Fails()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var melee = RealTimeActionDefinition.CreateForTesting("melee", RealTimeActionKind.Melee, range: 2f, cooldown: 5f);

            executor.TryStartAction(player, melee, enemy);
            executor.ResolvePendingActions(encounter.Participants);
            var startedAgainImmediately = executor.TryStartAction(player, melee, enemy);

            Assert.IsFalse(startedAgainImmediately);
        }

        [Test]
        public void TryStartAction_RangedWithoutRequiredCapability_Fails()
        {
            // FR-004.
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var ranged = RealTimeActionDefinition.CreateForTesting(
                "ranged", RealTimeActionKind.Ranged, range: 5f, requiredCapabilityId: "capability.ranged_attack");

            var started = executor.TryStartAction(player, ranged, enemy, hasRequiredCapability: false);

            Assert.IsFalse(started);
        }

        [Test]
        public void ResolvePendingActions_TargetMovedOutOfRange_DoesNotApplyDamage()
        {
            // Contract rule 5: range is re-checked at resolution time.
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var castTime = RealTimeActionDefinition.CreateForTesting("cast", RealTimeActionKind.Skill, range: 2f, executionTime: 1f, baseDamage: 10);

            executor.TryStartAction(player, castTime, enemy);
            enemy.PositionX = 100f; // moves out of range before the cast finishes.
            encounter.AdvanceTime(System.TimeSpan.FromSeconds(1.5));
            executor.ResolvePendingActions(encounter.Participants);

            Assert.AreEqual(20, enemy.CurrentHitPoints);
        }
    }
}
