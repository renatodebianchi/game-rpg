using GameRpg.Combat.Grid;

namespace GameRpg.Combat
{
    /// <summary>
    /// Anything that can participate in a CombatEncounter turn order:
    /// the player Character, allies, or enemies.
    /// </summary>
    public interface ICombatant
    {
        string CombatantId { get; }
        int CurrentHitPoints { get; }
        int MaxHitPoints { get; }
        TurnResources TurnResources { get; }
        GridCoordinate Position { get; set; }
        bool IsDefeated { get; }

        void ApplyDamage(int amount);
        void Heal(int amount);
    }
}
