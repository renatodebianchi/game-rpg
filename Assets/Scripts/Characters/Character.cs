using System;
using System.Collections.Generic;
using GameRpg.Combat;
using GameRpg.Combat.Grid;

namespace GameRpg.Characters
{
    /// <summary>
    /// Runtime state of the player-controlled character (see data-model.md, "Character").
    /// Skill node content itself lives in ScriptableObject assets (Skills.SkillNodeDefinition);
    /// this class only tracks which node ids have been acquired, per the save-data contract.
    /// </summary>
    public class Character : ICombatant
    {
        private readonly HashSet<string> _acquiredSkillNodeIds = new HashSet<string>();

        public string CombatantId { get; }
        public CharacterAttributes Attributes { get; set; }

        public int MaxHitPoints { get; private set; }
        public int CurrentHitPoints { get; private set; }

        public TurnResources TurnResources { get; }
        public GridCoordinate Position { get; set; }

        public bool IsDefeated => CurrentHitPoints <= 0;

        /// <summary>0 (sated) to 100 (starving). See BalancingConfig for thresholds.</summary>
        public float Hunger { get; set; }

        /// <summary>0 (broken) to 100 (stable). See BalancingConfig for thresholds.</summary>
        public float Sanity { get; set; } = 100f;

        public int AvailableSkillPoints { get; private set; }
        public IReadOnlyCollection<string> AcquiredSkillNodeIds => _acquiredSkillNodeIds;
        public Inventory Inventory { get; } = new Inventory();

        public Character(string combatantId, int maxHitPoints, int maxMovementPoints, CharacterAttributes attributes)
        {
            CombatantId = combatantId ?? throw new ArgumentNullException(nameof(combatantId));
            MaxHitPoints = maxHitPoints;
            CurrentHitPoints = maxHitPoints;
            TurnResources = new TurnResources(maxMovementPoints);
            Attributes = attributes;
        }

        public void ApplyDamage(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            CurrentHitPoints = Math.Max(0, CurrentHitPoints - amount);
        }

        public void Heal(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            CurrentHitPoints = Math.Min(MaxHitPoints, CurrentHitPoints + amount);
        }

        public void GrantSkillPoints(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            AvailableSkillPoints += amount;
        }

        /// <summary>Called by SkillTreeService after prerequisite/duplicate validation (FR-005, FR-006).</summary>
        public void AcquireSkillNode(string nodeId, int cost)
        {
            if (_acquiredSkillNodeIds.Contains(nodeId))
            {
                throw new InvalidOperationException($"Skill node '{nodeId}' is already acquired.");
            }

            if (cost > AvailableSkillPoints)
            {
                throw new InvalidOperationException("Not enough skill points available.");
            }

            AvailableSkillPoints -= cost;
            _acquiredSkillNodeIds.Add(nodeId);
        }

        /// <summary>Called by SkillTreeService during a respec (FR-018); refunds the node's cost.</summary>
        public void RemoveSkillNode(string nodeId, int refundedCost)
        {
            if (!_acquiredSkillNodeIds.Remove(nodeId))
            {
                throw new InvalidOperationException($"Skill node '{nodeId}' was not acquired.");
            }

            AvailableSkillPoints += refundedCost;
        }

        /// <summary>
        /// Overwrites this instance's state from a loaded save (contracts/save-data-contract.md),
        /// bypassing the normal mutation rules (e.g., skill-point cost checks), since
        /// a save represents an already-validated snapshot.
        /// </summary>
        public void RestoreFromSave(
            int currentHitPoints,
            int availableSkillPoints,
            IEnumerable<string> acquiredSkillNodeIds,
            float hunger,
            float sanity,
            IEnumerable<KeyValuePair<string, int>> inventoryEntries)
        {
            CurrentHitPoints = Math.Clamp(currentHitPoints, 0, MaxHitPoints);
            AvailableSkillPoints = availableSkillPoints;

            _acquiredSkillNodeIds.Clear();
            foreach (var nodeId in acquiredSkillNodeIds)
            {
                _acquiredSkillNodeIds.Add(nodeId);
            }

            Hunger = hunger;
            Sanity = sanity;

            foreach (var entry in inventoryEntries)
            {
                Inventory.Add(entry.Key, entry.Value);
            }
        }
    }
}
