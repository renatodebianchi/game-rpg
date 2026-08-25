using GameRpg.Combat;

namespace GameRpg.Skills
{
    /// <summary>
    /// A usable capability granted by a SkillNodeDefinition once acquired (FR-007).
    /// Combat-affecting capabilities plug into Combat.ActionResolver via
    /// DamageModifier; non-combat capabilities (world interactions) can be
    /// extended here later without touching CapabilityResolver's resolution logic.
    /// </summary>
    public class SkillCapability
    {
        public string CapabilityId { get; }
        public string DisplayName { get; }

        /// <summary>Optional: how this capability changes outgoing combat damage, if it does.</summary>
        public IDamageModifier DamageModifier { get; }

        public SkillCapability(string capabilityId, string displayName, IDamageModifier damageModifier = null)
        {
            CapabilityId = capabilityId;
            DisplayName = displayName;
            DamageModifier = damageModifier;
        }
    }
}
