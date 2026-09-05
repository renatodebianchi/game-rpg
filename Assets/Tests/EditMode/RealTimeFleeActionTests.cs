using System;
using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class RealTimeFleeActionTests
    {
        private static (CombatArenaEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy) CreateScenario()
        {
            var player = new NonPlayerCombatant("player", 20, 0) { PositionX = 0f };
            var enemy = new NonPlayerCombatant("enemy", 20, 0) { PositionX = 1f };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { enemy });
            encounter.Start();
            return (encounter, player, enemy);
        }

        [Test]
        public void AdvanceChannel_NotAttemptingToFlee_NeverResolvesAnAttempt()
        {
            var (encounter, player, enemy) = CreateScenario();
            var flee = new RealTimeFleeAction(encounter, () => 0.0); // would always succeed if rolled.

            var resolved = flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, dexterity: 20, TimeSpan.FromSeconds(10), isAttemptingToFlee: false);

            Assert.IsFalse(resolved);
            Assert.AreEqual(CombatEncounterState.InProgress, encounter.State);
        }

        [Test]
        public void AdvanceChannel_BelowMinimumDuration_DoesNotResolveYet()
        {
            var (encounter, player, enemy) = CreateScenario();
            var flee = new RealTimeFleeAction(encounter, () => 0.0);

            var resolved = flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, dexterity: 20, TimeSpan.FromSeconds(0.5), isAttemptingToFlee: true);

            Assert.IsFalse(resolved);
            Assert.IsTrue(player.ActionState.IsChannelingFlee);
        }

        [Test]
        public void AdvanceChannel_InterruptedBeforeMinimumDuration_ResetsProgress()
        {
            var (encounter, player, enemy) = CreateScenario();
            var flee = new RealTimeFleeAction(encounter, () => 0.0);

            flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, 20, TimeSpan.FromSeconds(1.5), isAttemptingToFlee: true);
            flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, 20, TimeSpan.FromSeconds(0.1), isAttemptingToFlee: false);

            Assert.IsFalse(player.ActionState.IsChannelingFlee);
            Assert.AreEqual(0f, player.ActionState.FleeChannelElapsed);
        }

        [Test]
        public void AdvanceChannel_ReachesMinimumDuration_ResolvesASuccessfulAttempt()
        {
            var (encounter, player, enemy) = CreateScenario();
            var flee = new RealTimeFleeAction(encounter, () => 0.0); // roll 0.0 always beats any positive chance.

            var resolved = flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, dexterity: 20, TimeSpan.FromSeconds(3), isAttemptingToFlee: true);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CombatEncounterState.PlayerFled, encounter.State);
            Assert.IsFalse(player.ActionState.IsChannelingFlee, "The channel resets once an attempt resolves.");
        }

        [Test]
        public void AdvanceChannel_ReachesMinimumDuration_FailedAttemptDoesNotEndEncounter()
        {
            var (encounter, player, enemy) = CreateScenario();
            var flee = new RealTimeFleeAction(encounter, () => 1.0); // roll 1.0 never beats any chance <= MaxSuccessChance.

            var resolved = flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, dexterity: 0, TimeSpan.FromSeconds(3), isAttemptingToFlee: true);

            Assert.IsTrue(resolved);
            Assert.AreEqual(CombatEncounterState.InProgress, encounter.State);
        }

        [Test]
        public void CalculateSuccessChance_NoLivingHostiles_ReturnsMaxChance()
        {
            var (encounter, player, enemy) = CreateScenario();
            enemy.ApplyDamage(999);
            var flee = new RealTimeFleeAction(encounter);

            var chance = flee.CalculateSuccessChance(player, new IRealTimeCombatant[] { enemy }, dexterity: 0);

            Assert.AreEqual(0.9, chance);
        }
    }
}
