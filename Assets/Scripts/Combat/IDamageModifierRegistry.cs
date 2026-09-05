namespace GameRpg.Combat
{
    /// <summary>
    /// Extension point for anything that adjusts outgoing damage before it is
    /// applied — e.g., acquired skill capabilities (feature 001 US2) or
    /// hunger/sanity penalties (feature 001 US3). Preserved unchanged in spirit
    /// from the turn-based ActionResolver (FR-012) — only the attacker type
    /// changes, from ICombatant to IRealTimeCombatant.
    /// </summary>
    public interface IDamageModifier
    {
        int ModifyOutgoingDamage(IRealTimeCombatant attacker, int baseDamage);
    }

    /// <summary>
    /// Minimal registration surface shared by whatever resolves combat actions
    /// (RealTimeActionExecutor), extracted from the old ActionResolver so that
    /// Skills.CapabilityResolver.ApplyAcquiredCapabilities can register damage
    /// modifiers without depending on the concrete executor type (data-model.md).
    /// </summary>
    public interface IDamageModifierRegistry
    {
        void RegisterDamageModifier(IDamageModifier modifier);
    }
}
