using GameRpg.Combat;
using GameRpg.Combat.Grid;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class FleeActionTests
    {
        private static (CombatEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy, TurnResourceManager manager)
            CreateEncounter()
        {
            var player = new NonPlayerCombatant("player", 10, 4) { Position = new GridCoordinate(0, 0) };
            var enemy = new NonPlayerCombatant("enemy", 10, 4) { Position = new GridCoordinate(1, 0) };
            var encounter = new CombatEncounter(new ICombatant[] { player }, new ICombatant[] { enemy });
            encounter.Start(new ICombatant[] { player, enemy });
            var manager = new TurnResourceManager(encounter);
            return (encounter, player, enemy, manager);
        }

        [Test]
        public void TryFlee_RollBelowChance_SucceedsAndSetsPlayerFledState()
        {
            var (encounter, player, _, manager) = CreateEncounter();
            var flee = new FleeAction(encounter, manager, randomRollProvider: () => 0.1);

            var succeeded = flee.TryFlee(player, successChance: 0.5);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(CombatEncounterState.PlayerFled, encounter.State);
        }

        [Test]
        public void TryFlee_RollAboveChance_FailsAndKeepsEncounterInProgress()
        {
            var (encounter, player, _, manager) = CreateEncounter();
            var flee = new FleeAction(encounter, manager, randomRollProvider: () => 0.9);

            var succeeded = flee.TryFlee(player, successChance: 0.5);

            Assert.IsFalse(succeeded);
            Assert.AreEqual(CombatEncounterState.InProgress, encounter.State);
        }

        [Test]
        public void TryFlee_AlwaysConsumesTheActingCombatantsAction()
        {
            var (encounter, player, _, manager) = CreateEncounter();
            var flee = new FleeAction(encounter, manager, randomRollProvider: () => 0.9);

            flee.TryFlee(player, successChance: 0.5);

            Assert.IsFalse(player.TurnResources.ActionAvailable);
        }

        [Test]
        public void CalculateSuccessChance_FartherFromHostiles_YieldsHigherChance()
        {
            var (encounter, player, enemy, manager) = CreateEncounter();
            var flee = new FleeAction(encounter, manager);

            var nearChance = flee.CalculateSuccessChance(player, new ICombatant[] { enemy }, dexterity: 10);

            enemy.Position = new GridCoordinate(10, 0);
            var farChance = flee.CalculateSuccessChance(player, new ICombatant[] { enemy }, dexterity: 10);

            Assert.Greater(farChance, nearChance);
        }

        [Test]
        public void CalculateSuccessChance_NoLivingHostiles_ReturnsMaximumChance()
        {
            var (encounter, player, enemy, manager) = CreateEncounter();
            enemy.ApplyDamage(10);
            var flee = new FleeAction(encounter, manager);

            var chance = flee.CalculateSuccessChance(player, new ICombatant[] { enemy }, dexterity: 5);

            Assert.AreEqual(0.9, chance);
        }
    }
}
