using System;
using GameRpg.Combat;
using GameRpg.Core;
using UnityEngine;

namespace GameRpg.Characters
{
    /// <summary>
    /// Tracks a Character's sanity, reduced by disturbing events and restored by
    /// recovery actions, exposing Alert/Critical effects as a combat damage
    /// modifier (FR-010, FR-011, FR-021).
    /// </summary>
    public class SanitySystem : IDamageModifier
    {
        private readonly Character _character;
        private readonly BalancingConfig _config;

        public SurvivalThresholdLevel CurrentLevel { get; private set; } = SurvivalThresholdLevel.Normal;

        public event Action<SurvivalThresholdLevel> LevelChanged;

        public SanitySystem(Character character, BalancingConfig config)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            RecomputeLevel();
        }

        /// <summary>Applies a disturbing event (extreme combat, supernatural surroundings, isolation) (FR-010).</summary>
        public void ApplyDisturbingEvent(float sanityReduction)
        {
            if (sanityReduction < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sanityReduction));
            }

            _character.Sanity = Mathf.Clamp(_character.Sanity - sanityReduction, 0f, 100f);
            RecomputeLevel();
        }

        /// <summary>Restores sanity (rest, items, safe environments) (FR-010).</summary>
        public void Recover(float sanityRestored)
        {
            if (sanityRestored < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(sanityRestored));
            }

            _character.Sanity = Mathf.Clamp(_character.Sanity + sanityRestored, 0f, 100f);
            RecomputeLevel();
        }

        private void RecomputeLevel()
        {
            var newLevel = ComputeLevel(_character.Sanity);
            if (newLevel == CurrentLevel)
            {
                return;
            }

            CurrentLevel = newLevel;
            LevelChanged?.Invoke(newLevel);
        }

        private SurvivalThresholdLevel ComputeLevel(float sanity)
        {
            if (sanity <= _config.SanityCriticalThreshold)
            {
                return SurvivalThresholdLevel.Critical;
            }

            if (sanity <= _config.SanityAlertThreshold)
            {
                return SurvivalThresholdLevel.Alert;
            }

            return SurvivalThresholdLevel.Normal;
        }

        /// <summary>Applies a sanity penalty (hallucinations/mental-test penalty) to this character's own outgoing damage (FR-011, FR-021).</summary>
        public int ModifyOutgoingDamage(ICombatant attacker, int baseDamage)
        {
            if (!(attacker is Character character) || character != _character)
            {
                return baseDamage;
            }

            return CurrentLevel switch
            {
                SurvivalThresholdLevel.Critical => Mathf.RoundToInt(baseDamage * 0.5f),
                SurvivalThresholdLevel.Alert => Mathf.RoundToInt(baseDamage * 0.8f),
                _ => baseDamage,
            };
        }
    }
}
