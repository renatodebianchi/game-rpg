using System;
using GameRpg.Combat;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class EnemyCombatAITests
    {
        private static (CombatArenaEncounter encounter, NonPlayerCombatant player, NonPlayerCombatant enemy, BattleArena arena) CreateScenario(float enemyStartX)
        {
            var player = new NonPlayerCombatant("player", 20, 0) { PositionX = 0f };
            var enemy = new NonPlayerCombatant("enemy", 20, 0) { PositionX = enemyStartX };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { enemy });
            encounter.Start();
            var arena = new BattleArena(0f, 20f);
            return (encounter, player, enemy, arena);
        }

        [Test]
        public void Tick_TargetOutOfRange_MovesTowardTarget()
        {
            var (encounter, player, enemy, arena) = CreateScenario(enemyStartX: 10f);
            var basicAttack = RealTimeActionDefinition.CreateForTesting("basic", RealTimeActionKind.Melee, range: 1f, baseDamage: 5);
            var executor = new RealTimeActionExecutor(encounter);
            var ai = new EnemyCombatAI(encounter, executor, arena, basicAttack);

            ai.Tick(enemy, TimeSpan.FromSeconds(1));

            Assert.Less(enemy.PositionX, 10f, "The enemy should have moved toward the player (at PositionX 0).");
        }

        [Test]
        public void Tick_TargetWithinRange_StartsAnAttackInsteadOfMoving()
        {
            var (encounter, player, enemy, arena) = CreateScenario(enemyStartX: 0.5f);
            var basicAttack = RealTimeActionDefinition.CreateForTesting("basic", RealTimeActionKind.Melee, range: 1f, baseDamage: 5);
            var executor = new RealTimeActionExecutor(encounter);
            var ai = new EnemyCombatAI(encounter, executor, arena, basicAttack);

            ai.Tick(enemy, TimeSpan.FromSeconds(1));

            Assert.AreEqual(0.5f, enemy.PositionX, "The enemy should not move once within attack range.");
            Assert.IsTrue(enemy.ActionState.HasPendingAction);
        }

        [Test]
        public void Tick_DefeatedEnemy_DoesNothing()
        {
            var (encounter, player, enemy, arena) = CreateScenario(enemyStartX: 10f);
            enemy.ApplyDamage(9999);
            var basicAttack = RealTimeActionDefinition.CreateForTesting("basic", RealTimeActionKind.Melee, range: 1f, baseDamage: 5);
            var executor = new RealTimeActionExecutor(encounter);
            var ai = new EnemyCombatAI(encounter, executor, arena, basicAttack);

            ai.Tick(enemy, TimeSpan.FromSeconds(1));

            Assert.AreEqual(10f, enemy.PositionX);
        }

        [Test]
        public void Tick_AlreadyActing_DoesNotMoveOrStartAnotherAction()
        {
            var (encounter, player, enemy, arena) = CreateScenario(enemyStartX: 10f);
            var castAction = RealTimeActionDefinition.CreateForTesting("cast", RealTimeActionKind.Skill, range: 1f, executionTime: 5f, baseDamage: 5);
            var executor = new RealTimeActionExecutor(encounter);
            executor.TryStartAction(enemy, castAction, player);
            var ai = new EnemyCombatAI(encounter, executor, arena, castAction);

            ai.Tick(enemy, TimeSpan.FromSeconds(1));

            Assert.AreEqual(10f, enemy.PositionX, "An enemy mid-action should hold its position.");
        }
    }
}
