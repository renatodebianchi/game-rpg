using System;
using GameRpg.Combat;
using GameRpg.Core;
using UnityEngine;

namespace GameRpg.Characters
{
    /// <summary>
    /// Tracks a Character's hunger over simulated time and exposes it as a
    /// combat damage modifier once it crosses the Alert/Critical thresholds
    /// from BalancingConfig (FR-008, FR-009, FR-021).
    /// </summary>
    public class HungerSystem : IDamageModifier
    {
        private readonly Character _character;
        private readonly BalancingConfig _config;
        private TimeSpan _lastProcessedElapsed;

        public SurvivalThresholdLevel CurrentLevel { get; private set; } = SurvivalThresholdLevel.Normal;

        public event Action<SurvivalThresholdLevel> LevelChanged;

        public HungerSystem(Character character, BalancingConfig config, WorldClock worldClock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            if (worldClock != null)
            {
                _lastProcessedElapsed = worldClock.ElapsedSimulatedTime;
                worldClock.TimeAdvanced += OnWorldTimeAdvanced;
            }

            RecomputeLevel();
        }

        private void OnWorldTimeAdvanced(TimeSpan cumulativeElapsed)
        {
            var delta = cumulativeElapsed - _lastProcessedElapsed;
            _lastProcessedElapsed = cumulativeElapsed;
            AdvanceByElapsedTime(delta);
        }

        /// <summary>Increases hunger for the given amount of simulated time elapsed.</summary>
        public void AdvanceByElapsedTime(TimeSpan delta)
        {
            var hours = (float)delta.TotalHours;
            if (hours <= 0f)
            {
                return;
            }

            _character.Hunger = Mathf.Clamp(_character.Hunger + _config.HungerIncreasePerSimulatedHour * hours, 0f, 100f);
            RecomputeLevel();
        }

        /// <summary>Restores hunger by the given amount (e.g., after consuming food) (FR-008).</summary>
        public void Feed(float restoreAmount)
        {
            if (restoreAmount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(restoreAmount));
            }

            _character.Hunger = Mathf.Clamp(_character.Hunger - restoreAmount, 0f, 100f);
            RecomputeLevel();
        }

        private void RecomputeLevel()
        {
            var newLevel = ComputeLevel(_character.Hunger);
            if (newLevel == CurrentLevel)
            {
                return;
            }

            CurrentLevel = newLevel;
            LevelChanged?.Invoke(newLevel);
        }

        private SurvivalThresholdLevel ComputeLevel(float hunger)
        {
            if (hunger >= _config.HungerCriticalThreshold)
            {
                return SurvivalThresholdLevel.Critical;
            }

            if (hunger >= _config.HungerAlertThreshold)
            {
                return SurvivalThresholdLevel.Alert;
            }

            return SurvivalThresholdLevel.Normal;
        }

        /// <summary>Applies a hunger penalty to this character's own outgoing damage (FR-009, FR-021).</summary>
        public int ModifyOutgoingDamage(IRealTimeCombatant attacker, int baseDamage)
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
