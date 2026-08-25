using System;
using System.Collections.Generic;

namespace GameRpg.Combat
{
    /// <summary>
    /// Extension point for anything that adjusts outgoing damage before it is
    /// applied — e.g., acquired skill capabilities (US2) or hunger/sanity
    /// penalties (US3). Registering a modifier is how those later stories
    /// extend combat without ActionResolver needing to know about them.
    /// </summary>
    public interface IDamageModifier
    {
        int ModifyOutgoingDamage(ICombatant attacker, int baseDamage);
    }

    /// <summary>
    /// Resolves an attack (or, later, a skill use) against a target (FR-001/FR-002).
    /// Consumes the attacker's action resource and applies the resulting damage
    /// through the owning CombatEncounter.
    /// </summary>
    public class ActionResolver
    {
        private readonly CombatEncounter _encounter;
        private readonly TurnResourceManager _turnResourceManager;
        private readonly List<IDamageModifier> _damageModifiers = new List<IDamageModifier>();

        public ActionResolver(CombatEncounter encounter, TurnResourceManager turnResourceManager)
        {
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _turnResourceManager = turnResourceManager ?? throw new ArgumentNullException(nameof(turnResourceManager));
        }

        public void RegisterDamageModifier(IDamageModifier modifier)
        {
            _damageModifiers.Add(modifier ?? throw new ArgumentNullException(nameof(modifier)));
        }

        /// <summary>Spends the attacker's action and applies damage to the target, after modifiers.</summary>
        public void ResolveBasicAttack(ICombatant attacker, ICombatant target, int baseDamage)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            _turnResourceManager.ConsumeAction(attacker);

            var damage = baseDamage;
            foreach (var modifier in _damageModifiers)
            {
                damage = modifier.ModifyOutgoingDamage(attacker, damage);
            }

            damage = Math.Max(0, damage);
            _encounter.ApplyDamage(target, damage);
        }
    }
}
