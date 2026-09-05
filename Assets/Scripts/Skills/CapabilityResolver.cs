using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Characters;
using GameRpg.Combat;
using GameRpg.Core;

namespace GameRpg.Skills
{
    /// <summary>
    /// Resolves a SkillNodeDefinition's grantedCapabilityId into a usable
    /// SkillCapability (FR-007), and exposes the set of capabilities a
    /// Character's currently-acquired nodes grant.
    /// </summary>
    public class CapabilityResolver
    {
        private readonly Dictionary<string, SkillNodeDefinition> _nodesById;
        private readonly Dictionary<string, SkillCapability> _capabilitiesById;

        public CapabilityResolver(IEnumerable<SkillNodeDefinition> allNodes, IEnumerable<SkillCapability> capabilities)
        {
            _nodesById = (allNodes ?? throw new ArgumentNullException(nameof(allNodes))).ToDictionary(n => n.NodeId);
            _capabilitiesById = (capabilities ?? throw new ArgumentNullException(nameof(capabilities)))
                .ToDictionary(c => c.CapabilityId);
        }

        public SkillCapability Resolve(string capabilityId) =>
            ContentValidation.ResolveOrThrow(_capabilitiesById, capabilityId, "SkillCapability");

        /// <summary>All capabilities granted by the character's currently-acquired skill nodes.</summary>
        public IEnumerable<SkillCapability> ResolveAcquiredCapabilities(Character character)
        {
            foreach (var nodeId in character.AcquiredSkillNodeIds)
            {
                var node = ContentValidation.ResolveOrThrow(_nodesById, nodeId, "SkillNodeDefinition");
                if (!string.IsNullOrEmpty(node.GrantedCapabilityId))
                {
                    yield return Resolve(node.GrantedCapabilityId);
                }
            }
        }

        /// <summary>
        /// Registers every damage-affecting capability the character has acquired
        /// into <paramref name="damageModifierRegistry"/> (extends the combat
        /// action executor with skill-tree build choices, per tasks.md T035 of
        /// feature 001, adapted for feature 004's real-time executor).
        /// </summary>
        public void ApplyAcquiredCapabilities(Character character, IDamageModifierRegistry damageModifierRegistry)
        {
            foreach (var capability in ResolveAcquiredCapabilities(character))
            {
                if (capability.DamageModifier != null)
                {
                    damageModifierRegistry.RegisterDamageModifier(capability.DamageModifier);
                }
            }
        }
    }
}
