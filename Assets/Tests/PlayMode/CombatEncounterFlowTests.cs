using GameRpg.Combat;
using GameRpg.Combat.Grid;
using NUnit.Framework;

namespace GameRpg.Tests.PlayMode
{
    public class CombatEncounterFlowTests
    {
        private static (CombatEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy) CreateStartedEncounter()
        {
            var player = new NonPlayerCombatant("player", maxHitPoints: 10, maxMovementPoints: 4)
            {
                Position = new GridCoordinate(0, 0),
            };
            var enemy = new NonPlayerCombatant("enemy", maxHitPoints: 10, maxMovementPoints: 4)
            {
                Position = new GridCoordinate(1, 0),
            };
            var encounter = new CombatEncounter(new ICombatant[] { player }, new ICombatant[] { enemy });
            encounter.Start(new ICombatant[] { player, enemy });
            return (encounter, player, enemy);
        }

        [Test]
        public void FullEncounter_PlayerDefeatsEnemy_EndsInWonByPlayerAndGrantsRewards()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var turnResourceManager = new TurnResourceManager(encounter);
            var actionResolver = new ActionResolver(encounter, turnResourceManager);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var rewardsGranted = -1;
            outcomeHandler.VictoryRewardsGranted += reward => rewardsGranted = reward;

            actionResolver.ResolveBasicAttack(player, enemy, baseDamage: 10);
            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.AreEqual(CombatEncounterState.WonByPlayer, encounter.State);
            Assert.AreEqual(50, rewardsGranted);
        }

        [Test]
        public void FullEncounter_EnemyDefeatsPlayer_EndsInPlayerDefeatedAndRestoresCheckpoint()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var turnResourceManager = new TurnResourceManager(encounter);
            var actionResolver = new ActionResolver(encounter, turnResourceManager);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var checkpointRestored = false;
            outcomeHandler.DefeatCheckpointRestored += () => checkpointRestored = true;

            // Player's turn: attack for 0 damage so the encounter stays in progress.
            actionResolver.ResolveBasicAttack(player, enemy, baseDamage: 0);
            encounter.AdvanceTurn();

            // Enemy's turn: defeat the player.
            actionResolver.ResolveBasicAttack(enemy, player, baseDamage: 10);

            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.AreEqual(CombatEncounterState.PlayerDefeated, encounter.State);
            Assert.IsTrue(checkpointRestored);
        }

        [Test]
        public void FullEncounter_PlayerFleesSuccessfully_EndsInPlayerFledWithoutRewardsOrCheckpoint()
        {
            var (encounter, player, _) = CreateStartedEncounter();
            var turnResourceManager = new TurnResourceManager(encounter);
            var flee = new FleeAction(encounter, turnResourceManager, randomRollProvider: () => 0.0);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var rewardsGranted = false;
            var checkpointRestored = false;
            outcomeHandler.VictoryRewardsGranted += _ => rewardsGranted = true;
            outcomeHandler.DefeatCheckpointRestored += () => checkpointRestored = true;

            var succeeded = flee.TryFlee(player, successChance: 1.0);
            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.IsTrue(succeeded);
            Assert.AreEqual(CombatEncounterState.PlayerFled, encounter.State);
            Assert.IsFalse(rewardsGranted);
            Assert.IsFalse(checkpointRestored);
        }
    }
}
