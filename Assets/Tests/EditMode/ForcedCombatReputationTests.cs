using System;
using GameRpg.Combat;
using GameRpg.Core;
using GameRpg.NPCs;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class ForcedCombatReputationTests
    {
        [Test]
        public void HarmingLinkedNpcInForcedCombat_RecordsSameChoiceAsDeliberateHarm()
        {
            var player = new NonPlayerCombatant("player", 20, 4) { PositionX = 0f };
            var forcedAlly = new NonPlayerCombatant("forced-ally", 10, 4, linkedNpcId: "npc_friendly")
            {
                PositionX = 1f,
            };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { forcedAlly });
            encounter.Start();

            var executor = new RealTimeActionExecutor(encounter);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var log = new ImpactfulChoiceLog();
            var worldClock = new WorldClock();
            var npcDefinitions = new[] { NpcDefinition.CreateForTesting("npc_friendly", "Friendly NPC", "village_a") };
            var bridge = new ForcedCombatReputationBridge(outcomeHandler, log, worldClock, npcDefinitions);
            var villageA = new Community("village_a", new[] { "npc_friendly" });
            var reputationService = new ReputationService(new[] { villageA }, log);

            var attack = RealTimeActionDefinition.CreateForTesting("test_attack", RealTimeActionKind.Melee, range: 5f, baseDamage: 5);
            executor.TryStartAction(player, attack, forcedAlly);
            executor.ResolvePendingActions(encounter.Participants);

            Assert.AreEqual(1, log.Entries.Count);
            Assert.AreEqual(ImpactfulChoiceType.AbandonOrHarmNpc, log.Entries[0].Type);
            Assert.AreEqual("npc_friendly", log.Entries[0].RelatedNpcId);
            Assert.Less(villageA.ReputationWithPlayer, 0);
        }

        [Test]
        public void HarmingCombatantWithoutLinkedNpc_DoesNotRecordAnyChoice()
        {
            var player = new NonPlayerCombatant("player", 20, 4) { PositionX = 0f };
            var genericEnemy = new NonPlayerCombatant("enemy", 10, 4) { PositionX = 1f };
            var encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { player }, new IRealTimeCombatant[] { genericEnemy });
            encounter.Start();

            var executor = new RealTimeActionExecutor(encounter);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var log = new ImpactfulChoiceLog();
            var worldClock = new WorldClock();
            var bridge = new ForcedCombatReputationBridge(outcomeHandler, log, worldClock, Array.Empty<NpcDefinition>());

            var attack = RealTimeActionDefinition.CreateForTesting("test_attack", RealTimeActionKind.Melee, range: 5f, baseDamage: 5);
            executor.TryStartAction(player, attack, genericEnemy);
            executor.ResolvePendingActions(encounter.Participants);

            Assert.AreEqual(0, log.Entries.Count);
        }
    }
}
