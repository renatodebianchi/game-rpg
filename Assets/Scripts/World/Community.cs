using System;
using System.Collections.Generic;

namespace GameRpg.World
{
    /// <summary>
    /// Runtime state of a community/faction (data-model.md, "Community/Faction").
    /// Content (display name, starting values) lives in CommunityDefinition;
    /// this class is the mutable per-save state derived from it.
    /// </summary>
    public class Community
    {
        private readonly Dictionary<string, float> _essentialResourceStock = new Dictionary<string, float>();
        private readonly List<string> _populationNpcIds;

        public string CommunityId { get; }
        public int ReputationWithPlayer { get; private set; }
        public bool IsPermanentlyInactive { get; private set; }

        public IReadOnlyList<string> PopulationNpcIds => _populationNpcIds;

        public Community(string communityId, IEnumerable<string> initialPopulationNpcIds)
        {
            CommunityId = communityId ?? throw new ArgumentNullException(nameof(communityId));
            _populationNpcIds = new List<string>(initialPopulationNpcIds ?? Array.Empty<string>());
        }

        public float GetResourceStock(string resourceId) =>
            _essentialResourceStock.TryGetValue(resourceId, out var quantity) ? quantity : 0f;

        /// <summary>All tracked resource stocks, for save serialization.</summary>
        public IEnumerable<KeyValuePair<string, float>> EnumerateResourceStock() => _essentialResourceStock;

        public void AddResourceStock(string resourceId, float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            _essentialResourceStock[resourceId] = GetResourceStock(resourceId) + amount;
        }

        /// <summary>Consumes up to <paramref name="amount"/>; never goes negative (contract rule 1).</summary>
        public float ConsumeResourceStock(string resourceId, float amount)
        {
            var available = GetResourceStock(resourceId);
            var consumed = Math.Min(available, amount);
            _essentialResourceStock[resourceId] = available - consumed;
            return consumed;
        }

        public void ApplyReputationDelta(int delta)
        {
            if (IsPermanentlyInactive)
            {
                return; // An inactive community no longer has anyone to have reputation with.
            }

            ReputationWithPlayer += delta;
        }

        /// <summary>Removes an NPC from the population (e.g., death by starvation or in forced combat).</summary>
        public void RemoveNpcFromPopulation(string npcId)
        {
            _populationNpcIds.Remove(npcId);

            if (_populationNpcIds.Count == 0)
            {
                MarkPermanentlyInactive();
            }
        }

        /// <summary>Terminal, irreversible state once population reaches zero (FR-019).</summary>
        public void MarkPermanentlyInactive()
        {
            IsPermanentlyInactive = true;
        }

        /// <summary>
        /// Overwrites this instance's state from a loaded save (contracts/save-data-contract.md),
        /// bypassing the normal mutation rules (e.g., reputation deltas), since a save
        /// represents an already-validated snapshot.
        /// </summary>
        public void RestoreFromSave(
            IEnumerable<KeyValuePair<string, float>> resourceStock,
            int reputationWithPlayer,
            bool isPermanentlyInactive,
            IEnumerable<string> populationNpcIds)
        {
            _essentialResourceStock.Clear();
            foreach (var entry in resourceStock)
            {
                _essentialResourceStock[entry.Key] = entry.Value;
            }

            ReputationWithPlayer = reputationWithPlayer;
            IsPermanentlyInactive = isPermanentlyInactive;

            _populationNpcIds.Clear();
            _populationNpcIds.AddRange(populationNpcIds);
        }
    }
}
