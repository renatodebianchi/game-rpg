using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Characters;

namespace GameRpg.Skills
{
    /// <summary>
    /// Investment/respec rules for the skill tree (FR-004, FR-005, FR-006, FR-018).
    /// Holds the loaded set of SkillNodeDefinitions so it can resolve reverse
    /// dependencies (needed for cascading respec).
    /// </summary>
    public class SkillTreeService
    {
        private readonly Dictionary<string, SkillNodeDefinition> _nodesById;

        public SkillTreeService(IEnumerable<SkillNodeDefinition> allNodes)
        {
            if (allNodes == null)
            {
                throw new ArgumentNullException(nameof(allNodes));
            }

            _nodesById = allNodes.ToDictionary(n => n.NodeId);
        }

        public bool ArePrerequisitesSatisfied(Character character, SkillNodeDefinition node)
        {
            return node.Prerequisites.All(prerequisite => character.AcquiredSkillNodeIds.Contains(prerequisite.NodeId));
        }

        /// <summary>
        /// True when the node can be invested in right now: not already acquired,
        /// prerequisites met, and (for Hybrid nodes) the dual-track invariant holds.
        /// </summary>
        public bool IsAvailableForInvestment(Character character, SkillNodeDefinition node)
        {
            if (character.AcquiredSkillNodeIds.Contains(node.NodeId))
            {
                return false;
            }

            if (node.Track == SkillTrack.Hybrid && !node.HasValidHybridPrerequisites())
            {
                return false;
            }

            return ArePrerequisitesSatisfied(character, node);
        }

        /// <summary>Invests the character's points into <paramref name="node"/> (FR-005, FR-006).</summary>
        public void AcquireNode(Character character, SkillNodeDefinition node)
        {
            if (!IsAvailableForInvestment(character, node))
            {
                throw new InvalidOperationException(
                    $"Skill node '{node.NodeId}' is not available for investment right now.");
            }

            character.AcquireSkillNode(node.NodeId, node.Cost);
        }

        /// <summary>
        /// Undoes the investment in <paramref name="node"/>, refunding its cost. Any
        /// currently-acquired node that lists <paramref name="node"/> as a
        /// prerequisite is removed first, recursively (FR-018 / data-model.md
        /// respec rule).
        /// </summary>
        public void Respec(Character character, SkillNodeDefinition node)
        {
            if (!character.AcquiredSkillNodeIds.Contains(node.NodeId))
            {
                throw new InvalidOperationException($"Skill node '{node.NodeId}' was not acquired.");
            }

            RemoveWithDependents(character, node);
        }

        private void RemoveWithDependents(Character character, SkillNodeDefinition node)
        {
            var directDependents = _nodesById.Values
                .Where(candidate =>
                    character.AcquiredSkillNodeIds.Contains(candidate.NodeId) &&
                    candidate.Prerequisites.Contains(node))
                .ToList();

            foreach (var dependent in directDependents)
            {
                RemoveWithDependents(character, dependent);
            }

            character.RemoveSkillNode(node.NodeId, node.Cost);
        }
    }
}
