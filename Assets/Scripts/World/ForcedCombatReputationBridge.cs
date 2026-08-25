using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Combat;
using GameRpg.Core;
using GameRpg.NPCs;

namespace GameRpg.World
{
    /// <summary>
    /// Bridges Combat (US1) and World/Reputation (US4): when a combatant linked
    /// to a world NPC (NonPlayerCombatant.LinkedNpcId) is harmed during a forced
    /// encounter, this records the same ImpactfulChoice a deliberate
    /// "abandon/harm NPC" choice would (FR-022).
    /// </summary>
    public class ForcedCombatReputationBridge
    {
        private readonly ImpactfulChoiceLog _log;
        private readonly WorldClock _worldClock;
        private readonly IReadOnlyDictionary<string, string> _communityIdByNpcId;

        public ForcedCombatReputationBridge(
            CombatOutcomeHandler outcomeHandler,
            ImpactfulChoiceLog log,
            WorldClock worldClock,
            IEnumerable<NpcDefinition> npcDefinitions)
        {
            if (outcomeHandler == null)
            {
                throw new ArgumentNullException(nameof(outcomeHandler));
            }

            _log = log ?? throw new ArgumentNullException(nameof(log));
            _worldClock = worldClock ?? throw new ArgumentNullException(nameof(worldClock));
            _communityIdByNpcId = (npcDefinitions ?? throw new ArgumentNullException(nameof(npcDefinitions)))
                .ToDictionary(n => n.NpcId, n => n.CommunityId);

            outcomeHandler.CombatantHarmed += OnCombatantHarmed;
        }

        private void OnCombatantHarmed(CombatantHarmedEvent harmedEvent)
        {
            if (string.IsNullOrEmpty(harmedEvent.LinkedNpcId))
            {
                return;
            }

            if (!_communityIdByNpcId.TryGetValue(harmedEvent.LinkedNpcId, out var communityId))
            {
                return;
            }

            _log.Record(
                ImpactfulChoiceType.AbandonOrHarmNpc,
                communityId,
                _worldClock.ElapsedSimulatedTime,
                relatedNpcId: harmedEvent.LinkedNpcId);
        }
    }
}
