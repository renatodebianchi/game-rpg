using System;

namespace GameRpg.Combat
{
    /// <summary>
    /// A non-player combat participant: a generic enemy, or an ally/forced
    /// combatant that represents a named world NPC via LinkedNpcId
    /// (data-model.md, "Enemy / Ally Combatant"; used by FR-022 / T057).
    /// </summary>
    public class NonPlayerCombatant : IRealTimeCombatant
    {
        public string CombatantId { get; }

        /// <summary>Optional reference to a world NPC id, when this combatant represents one.</summary>
        public string LinkedNpcId { get; }

        public int MaxHitPoints { get; }
        public int CurrentHitPoints { get; private set; }
        public CombatantActionState ActionState { get; }
        public float PositionX { get; set; }
        public bool IsDefeated => CurrentHitPoints <= 0;

        public NonPlayerCombatant(string combatantId, int maxHitPoints, int maxTechPoints, string linkedNpcId = null)
        {
            CombatantId = combatantId ?? throw new ArgumentNullException(nameof(combatantId));
            MaxHitPoints = maxHitPoints;
            CurrentHitPoints = maxHitPoints;
            ActionState = new CombatantActionState(maxTechPoints);
            LinkedNpcId = linkedNpcId;
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
    }
}
