using System;
using System.Collections.Generic;
using System.Linq;

namespace GameRpg.World
{
    /// <summary>
    /// Applies reputation deltas from recorded ImpactfulChoices to their target
    /// Community only — reputation is never propagated to any other community,
    /// including declared rivals (FR-012, FR-020).
    /// </summary>
    public class ReputationService
    {
        private const int SaveNpcReputationDelta = 10;
        private const int AbandonOrHarmNpcReputationDelta = -10;
        private const int TransportResourceReputationDelta = 5;

        private readonly Dictionary<string, Community> _communitiesById;

        public ReputationService(IEnumerable<Community> communities, ImpactfulChoiceLog log)
        {
            if (communities == null)
            {
                throw new ArgumentNullException(nameof(communities));
            }

            _communitiesById = communities.ToDictionary(c => c.CommunityId);

            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            log.ChoiceRecorded += OnChoiceRecorded;
        }

        private void OnChoiceRecorded(ImpactfulChoice choice)
        {
            if (!_communitiesById.TryGetValue(choice.TargetCommunityId, out var community))
            {
                return;
            }

            var delta = choice.Type switch
            {
                ImpactfulChoiceType.SaveNpc => SaveNpcReputationDelta,
                ImpactfulChoiceType.AbandonOrHarmNpc => AbandonOrHarmNpcReputationDelta,
                ImpactfulChoiceType.TransportResource => TransportResourceReputationDelta,
                _ => 0,
            };

            community.ApplyReputationDelta(delta);
        }
    }
}
