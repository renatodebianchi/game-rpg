using System;
using GameRpg.Combat;
using GameRpg.Combat.Grid;
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
            var player = new NonPlayerCombatant("player", 20, 4) { Position = new GridCoordinate(0, 0) };
            var forcedAlly = new NonPlayerCombatant("forced-ally", 10, 4, linkedNpcId: "npc_friendly")
            {
                Position = new GridCoordinate(1, 0),
            };
            var encounter = new CombatEncounter(new ICombatant[] { player }, new ICombatant[] { forcedAlly });
            encounter.Start(new ICombatant[] { player, forcedAlly });

            var turnResourceManager = new TurnResourceManager(encounter);
            var actionResolver = new ActionResolver(encounter, turnResourceManager);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var log = new ImpactfulChoiceLog();
            var worldClock = new WorldClock();
            var npcDefinitions = new[] { NpcDefinition.CreateForTesting("npc_friendly", "Friendly NPC", "village_a") };
            var bridge = new ForcedCombatReputationBridge(outcomeHandler, log, worldClock, npcDefinitions);
            var villageA = new Community("village_a", new[] { "npc_friendly" });
            var reputationService = new ReputationService(new[] { villageA }, log);

            actionResolver.ResolveBasicAttack(player, forcedAlly, baseDamage: 5);

            Assert.AreEqual(1, log.Entries.Count);
            Assert.AreEqual(ImpactfulChoiceType.AbandonOrHarmNpc, log.Entries[0].Type);
            Assert.AreEqual("npc_friendly", log.Entries[0].RelatedNpcId);
            Assert.Less(villageA.ReputationWithPlayer, 0);
        }

        [Test]
        public void HarmingCombatantWithoutLinkedNpc_DoesNotRecordAnyChoice()
        {
            var player = new NonPlayerCombatant("player", 20, 4) { Position = new GridCoordinate(0, 0) };
            var genericEnemy = new NonPlayerCombatant("enemy", 10, 4) { Position = new GridCoordinate(1, 0) };
            var encounter = new CombatEncounter(new ICombatant[] { player }, new ICombatant[] { genericEnemy });
            encounter.Start(new ICombatant[] { player, genericEnemy });

            var turnResourceManager = new TurnResourceManager(encounter);
            var actionResolver = new ActionResolver(encounter, turnResourceManager);
            var outcomeHandler = new CombatOutcomeHandler(encounter);

            var log = new ImpactfulChoiceLog();
            var worldClock = new WorldClock();
            var bridge = new ForcedCombatReputationBridge(outcomeHandler, log, worldClock, Array.Empty<NpcDefinition>());

            actionResolver.ResolveBasicAttack(player, genericEnemy, baseDamage: 5);

            Assert.AreEqual(0, log.Entries.Count);
        }
    }
}
