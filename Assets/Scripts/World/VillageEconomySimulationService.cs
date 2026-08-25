using System;
using System.Collections.Generic;
using GameRpg.Core;

namespace GameRpg.World
{
    public enum CommunityEconomyState
    {
        Stable,
        Strained,
        Collapsed
    }

    public readonly struct VillageTickResult
    {
        public readonly float UpdatedEssentialResourceStock;
        public readonly IReadOnlyList<string> NpcsTransitionedToDead;
        public readonly CommunityEconomyState UpdatedEconomyState;
        public readonly bool IsPermanentlyInactive;

        public VillageTickResult(
            float updatedEssentialResourceStock,
            IReadOnlyList<string> npcsTransitionedToDead,
            CommunityEconomyState updatedEconomyState,
            bool isPermanentlyInactive)
        {
            UpdatedEssentialResourceStock = updatedEssentialResourceStock;
            NpcsTransitionedToDead = npcsTransitionedToDead;
            UpdatedEconomyState = updatedEconomyState;
            IsPermanentlyInactive = isPermanentlyInactive;
        }
    }

    /// <summary>
    /// Implements contracts/village-economy-simulation-contract.md: consumes a
    /// community's essential resource stock proportionally to its population,
    /// and — once the stock stays below BalancingConfig's sustain threshold for
    /// longer than the tolerance period — reduces population, eventually
    /// reaching the permanent-inactivation terminal state (FR-014, FR-019).
    /// </summary>
    public class VillageEconomySimulationService
    {
        private const float ConsumptionPerNpcPerHour = 1f;

        private readonly BalancingConfig _config;
        private readonly Dictionary<string, float> _hoursBelowThresholdByCommunity = new Dictionary<string, float>();

        public VillageEconomySimulationService(BalancingConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public VillageTickResult Tick(Community community, string essentialResourceId, TimeSpan elapsed)
        {
            if (community == null)
            {
                throw new ArgumentNullException(nameof(community));
            }

            if (community.IsPermanentlyInactive)
            {
                // Rule 6: a permanently inactive community is never processed again,
                // even if its resource stock is later replenished.
                return new VillageTickResult(
                    community.GetResourceStock(essentialResourceId),
                    Array.Empty<string>(),
                    CommunityEconomyState.Collapsed,
                    isPermanentlyInactive: true);
            }

            var hours = (float)elapsed.TotalHours;
            var demand = community.PopulationNpcIds.Count * ConsumptionPerNpcPerHour * hours;
            community.ConsumeResourceStock(essentialResourceId, demand);

            var currentStock = community.GetResourceStock(essentialResourceId);
            var belowThreshold = currentStock < _config.VillageSustainThresholdStock;
            var npcsTransitionedToDead = new List<string>();

            if (belowThreshold)
            {
                var hoursBelow = _hoursBelowThresholdByCommunity.TryGetValue(community.CommunityId, out var existing)
                    ? existing + hours
                    : hours;

                if (hoursBelow >= _config.VillageSustainTolerancePeriodHours && community.PopulationNpcIds.Count > 0)
                {
                    var npcId = community.PopulationNpcIds[0];
                    community.RemoveNpcFromPopulation(npcId);
                    npcsTransitionedToDead.Add(npcId);
                    hoursBelow = 0f; // tolerance window resets after a loss
                }

                _hoursBelowThresholdByCommunity[community.CommunityId] = hoursBelow;
            }
            else
            {
                _hoursBelowThresholdByCommunity[community.CommunityId] = 0f;
            }

            var economyState = community.IsPermanentlyInactive
                ? CommunityEconomyState.Collapsed
                : belowThreshold ? CommunityEconomyState.Strained : CommunityEconomyState.Stable;

            return new VillageTickResult(currentStock, npcsTransitionedToDead, economyState, community.IsPermanentlyInactive);
        }
    }
}
