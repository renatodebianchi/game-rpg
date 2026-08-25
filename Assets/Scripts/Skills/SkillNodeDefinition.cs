using System.Collections.Generic;
using UnityEngine;

namespace GameRpg.Skills
{
    /// <summary>
    /// Content-authored definition of a single skill-tree node.
    /// See specs/001-isometric-sandbox-rpg/contracts/skill-node-data-contract.md
    /// for the full data contract and its invariants.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNode", menuName = "GameRpg/Skills/Skill Node")]
    public class SkillNodeDefinition : ScriptableObject
    {
        [SerializeField] private string nodeId;
        [SerializeField] private string displayName;
        [SerializeField] private SkillTrack track;
        [SerializeField] private List<SkillNodeDefinition> prerequisites = new List<SkillNodeDefinition>();
        [SerializeField] private int cost = 1;
        [SerializeField] private string grantedCapabilityId;

        public string NodeId => nodeId;
        public string DisplayName => displayName;
        public SkillTrack Track => track;
        public IReadOnlyList<SkillNodeDefinition> Prerequisites => prerequisites;
        public int Cost => cost;
        public string GrantedCapabilityId => grantedCapabilityId;

        /// <summary>
        /// Per the data contract, a Hybrid node must have at least one prerequisite
        /// from the Combatant track and one from the Arcanist track.
        /// </summary>
        /// <summary>
        /// Creates an in-memory instance without going through the asset database.
        /// Intended for tests and tooling; normal content authoring should create
        /// these as ScriptableObject assets via the Editor (Assets/Data/Skills).
        /// </summary>
        public static SkillNodeDefinition CreateForTesting(
            string nodeId,
            SkillTrack track,
            IEnumerable<SkillNodeDefinition> prerequisites = null,
            int cost = 1,
            string grantedCapabilityId = null)
        {
            var instance = CreateInstance<SkillNodeDefinition>();
            instance.nodeId = nodeId;
            instance.displayName = nodeId;
            instance.track = track;
            instance.prerequisites = prerequisites != null ? new List<SkillNodeDefinition>(prerequisites) : new List<SkillNodeDefinition>();
            instance.cost = cost;
            instance.grantedCapabilityId = grantedCapabilityId;
            return instance;
        }

        public bool HasValidHybridPrerequisites()
        {
            if (track != SkillTrack.Hybrid)
            {
                return true;
            }

            var hasCombatant = false;
            var hasArcanist = false;

            foreach (var prerequisite in prerequisites)
            {
                if (prerequisite == null)
                {
                    continue;
                }

                if (prerequisite.Track == SkillTrack.Combatant)
                {
                    hasCombatant = true;
                }
                else if (prerequisite.Track == SkillTrack.Arcanist)
                {
                    hasArcanist = true;
                }
            }

            return hasCombatant && hasArcanist;
        }
    }
}
