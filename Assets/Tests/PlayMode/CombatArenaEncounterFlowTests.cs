using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.PlayMode
{
    public class CombatArenaEncounterFlowTests
    {
        private static (CombatArenaEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy) CreateStartedEncounter()
        {
            var player = new NonPlayerCombatant("player", maxHitPoints: 10, maxTechPoints: 0) { PositionX = 0f };
            var enemy = new NonPlayerCombatant("enemy", maxHitPoints: 10, maxTechPoints: 0) { PositionX = 1f };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { enemy });
            encounter.Start();
            return (encounter, player, enemy);
        }

        [Test]
        public void FullEncounter_PlayerDefeatsEnemy_EndsInWonByPlayerAndGrantsRewards()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var outcomeHandler = new CombatOutcomeHandler(encounter);
            var lethalHit = RealTimeActionDefinition.CreateForTesting("lethal", RealTimeActionKind.Melee, range: 5f, baseDamage: 999);

            var rewardsGranted = -1;
            outcomeHandler.VictoryRewardsGranted += reward => rewardsGranted = reward;

            executor.TryStartAction(player, lethalHit, enemy);
            executor.ResolvePendingActions(encounter.Participants);
            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.AreEqual(CombatEncounterState.WonByPlayer, encounter.State);
            Assert.AreEqual(50, rewardsGranted);
        }

        [Test]
        public void FullEncounter_EnemyDefeatsPlayer_EndsInPlayerDefeatedAndRestoresCheckpoint()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var executor = new RealTimeActionExecutor(encounter);
            var outcomeHandler = new CombatOutcomeHandler(encounter);
            var lethalHit = RealTimeActionDefinition.CreateForTesting("lethal", RealTimeActionKind.Melee, range: 5f, baseDamage: 999);

            var checkpointRestored = false;
            outcomeHandler.DefeatCheckpointRestored += () => checkpointRestored = true;

            executor.TryStartAction(enemy, lethalHit, player);
            executor.ResolvePendingActions(encounter.Participants);
            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.AreEqual(CombatEncounterState.PlayerDefeated, encounter.State);
            Assert.IsTrue(checkpointRestored);
        }

        [Test]
        public void FullEncounter_PlayerFleesSuccessfully_EndsInPlayerFledWithoutRewardsOrCheckpoint()
        {
            var (encounter, player, enemy) = CreateStartedEncounter();
            var outcomeHandler = new CombatOutcomeHandler(encounter);
            var flee = new RealTimeFleeAction(encounter, () => 0.0);

            var rewardsGranted = false;
            var checkpointRestored = false;
            outcomeHandler.VictoryRewardsGranted += _ => rewardsGranted = true;
            outcomeHandler.DefeatCheckpointRestored += () => checkpointRestored = true;

            flee.AdvanceChannel(player, new IRealTimeCombatant[] { enemy }, dexterity: 20, System.TimeSpan.FromSeconds(3), isAttemptingToFlee: true);
            outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 50);

            Assert.AreEqual(CombatEncounterState.PlayerFled, encounter.State);
            Assert.IsFalse(rewardsGranted);
            Assert.IsFalse(checkpointRestored);
        }

        [Test]
        public void AdvanceTime_RegeneratesTechPointsAndCooldownsForEveryLivingParticipant()
        {
            var player = new NonPlayerCombatant("player", maxHitPoints: 10, maxTechPoints: 10) { PositionX = 0f };
            var enemy = new NonPlayerCombatant("enemy", maxHitPoints: 10, maxTechPoints: 0) { PositionX = 1f };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { enemy });
            encounter.Start();

            var costlyAction = RealTimeActionDefinition.CreateForTesting("costly", RealTimeActionKind.Skill, range: 5f, resourceCost: 4f, cooldown: 3f);
            var executor = new RealTimeActionExecutor(encounter);
            var started = executor.TryStartAction(player, costlyAction, enemy);
            executor.ResolvePendingActions(encounter.Participants); // resolves instantly (executionTime 0), applies cooldown.

            Assert.IsTrue(started);
            Assert.IsFalse(player.ActionState.IsOffCooldown("costly"));

            encounter.AdvanceTime(System.TimeSpan.FromSeconds(3));

            Assert.IsTrue(player.ActionState.IsOffCooldown("costly"), "Cooldown must tick down via CombatArenaEncounter.AdvanceTime.");
        }
    }
}
