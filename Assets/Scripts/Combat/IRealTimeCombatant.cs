namespace GameRpg.Combat
{
    /// <summary>
    /// Anything that can participate in a CombatArenaEncounter in real time:
    /// the player Character, or an enemy. Replaces the turn-based ICombatant
    /// (feature 001) — position is a continuous horizontal coordinate instead
    /// of a grid cell, and there is no TurnResources/action-per-turn gate;
    /// real-time action bookkeeping lives in CombatantActionState instead.
    /// </summary>
    public interface IRealTimeCombatant
    {
        string CombatantId { get; }
        int CurrentHitPoints { get; }
        int MaxHitPoints { get; }

        /// <summary>Continuous position along the BattleArena's horizontal axis (FR-002).</summary>
        float PositionX { get; set; }

        CombatantActionState ActionState { get; }
        bool IsDefeated { get; }

        void ApplyDamage(int amount);
        void Heal(int amount);
    }
}
