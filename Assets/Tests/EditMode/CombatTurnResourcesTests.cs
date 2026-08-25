using System;
using GameRpg.Combat;
using GameRpg.Combat.Grid;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class CombatTurnResourcesTests
    {
        private static (CombatEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy, TurnResourceManager manager)
            CreateSingleActorEncounter()
        {
            var player = new NonPlayerCombatant("player", maxHitPoints: 10, maxMovementPoints: 4);
            var enemy = new NonPlayerCombatant("enemy", maxHitPoints: 10, maxMovementPoints: 4);
            var encounter = new CombatEncounter(new[] { player }, new[] { enemy });
            encounter.Start(new ICombatant[] { player, enemy });
            var manager = new TurnResourceManager(encounter);
            return (encounter, player, enemy, manager);
        }

        [Test]
        public void ConsumingMovement_ReducesRemainingMovementPoints()
        {
            var (_, player, _, manager) = CreateSingleActorEncounter();

            manager.ConsumeMovement(player, 3);

            Assert.AreEqual(1, player.TurnResources.MovementPointsRemaining);
        }

        [Test]
        public void ConsumingAction_TwiceInSameTurn_Throws()
        {
            var (_, player, _, manager) = CreateSingleActorEncounter();

            manager.ConsumeAction(player);

            Assert.Throws<InvalidOperationException>(() => manager.ConsumeAction(player));
        }

        [Test]
        public void ConsumingBonusAction_TwiceInSameTurn_Throws()
        {
            var (_, player, _, manager) = CreateSingleActorEncounter();

            manager.ConsumeBonusAction(player);

            Assert.Throws<InvalidOperationException>(() => manager.ConsumeBonusAction(player));
        }

        [Test]
        public void ResourcesAreRestored_AtStartOfNextTurn()
        {
            var (encounter, player, enemy, manager) = CreateSingleActorEncounter();

            manager.ConsumeMovement(player, 4);
            manager.ConsumeAction(player);
            manager.ConsumeBonusAction(player);

            encounter.AdvanceTurn(); // enemy's turn
            encounter.AdvanceTurn(); // back to player's turn
            manager.ResetForActor(player);

            Assert.AreEqual(4, player.TurnResources.MovementPointsRemaining);
            Assert.IsTrue(player.TurnResources.ActionAvailable);
            Assert.IsTrue(player.TurnResources.BonusActionAvailable);
        }

        [Test]
        public void SpendingResources_ForNonCurrentActor_Throws()
        {
            var (_, player, enemy, manager) = CreateSingleActorEncounter();

            Assert.Throws<InvalidOperationException>(() => manager.ConsumeAction(enemy));
        }

        [Test]
        public void ConsumingMoreMovementThanRemaining_Throws()
        {
            var (_, player, _, manager) = CreateSingleActorEncounter();

            Assert.Throws<InvalidOperationException>(() => manager.ConsumeMovement(player, 5));
        }
    }
}
