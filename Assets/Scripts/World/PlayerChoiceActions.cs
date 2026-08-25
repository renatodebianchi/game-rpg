using System;
using GameRpg.Core;

namespace GameRpg.World
{
    /// <summary>
    /// Player-facing actions that constitute ImpactfulChoices (FR-013).
    /// TransportResource both updates the receiving community's stock and logs
    /// the choice — logging alone is not enough (/speckit-analyze finding U2).
    /// </summary>
    public class PlayerChoiceActions
    {
        private readonly ImpactfulChoiceLog _log;
        private readonly WorldClock _worldClock;

        public PlayerChoiceActions(ImpactfulChoiceLog log, WorldClock worldClock)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _worldClock = worldClock ?? throw new ArgumentNullException(nameof(worldClock));
        }

        public ImpactfulChoice SaveNpc(string npcId, string communityId)
        {
            return _log.Record(
                ImpactfulChoiceType.SaveNpc, communityId, _worldClock.ElapsedSimulatedTime, relatedNpcId: npcId);
        }

        public ImpactfulChoice AbandonOrHarmNpc(string npcId, string communityId)
        {
            return _log.Record(
                ImpactfulChoiceType.AbandonOrHarmNpc, communityId, _worldClock.ElapsedSimulatedTime, relatedNpcId: npcId);
        }

        /// <summary>Delivers a resource to a community: updates its stock (FR-015) and logs the choice (FR-013).</summary>
        public ImpactfulChoice TransportResource(Community targetCommunity, string resourceId, int quantity)
        {
            if (targetCommunity == null)
            {
                throw new ArgumentNullException(nameof(targetCommunity));
            }

            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            targetCommunity.AddResourceStock(resourceId, quantity);

            return _log.Record(
                ImpactfulChoiceType.TransportResource,
                targetCommunity.CommunityId,
                _worldClock.ElapsedSimulatedTime,
                relatedResourceId: resourceId,
                quantity: quantity);
        }
    }
}
