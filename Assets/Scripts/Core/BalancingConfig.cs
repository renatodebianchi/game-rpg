using UnityEngine;

namespace GameRpg.Core
{
    /// <summary>
    /// Centralizes numeric balancing parameters that were left undefined by the
    /// spec (see /speckit-analyze finding U1): hunger/sanity alert/critical
    /// thresholds, village sustain threshold/tolerance period, and the
    /// "defined game-time period" used by SC-003/SC-004/SC-005. A single
    /// instance is authored as a ScriptableObject asset so designers can
    /// balance the game without touching code (Principle II).
    /// </summary>
    [CreateAssetMenu(fileName = "BalancingConfig", menuName = "GameRpg/Core/Balancing Config")]
    public class BalancingConfig : ScriptableObject
    {
        [Header("Hunger (0 = sated, 100 = starving)")]
        [SerializeField] private float hungerAlertThreshold = 60f;
        [SerializeField] private float hungerCriticalThreshold = 85f;
        [SerializeField] private float hungerIncreasePerSimulatedHour = 2f;

        [Header("Sanity (0 = broken, 100 = stable)")]
        [SerializeField] private float sanityAlertThreshold = 40f;
        [SerializeField] private float sanityCriticalThreshold = 15f;

        [Header("Village economy simulation")]
        [SerializeField] private float villageSustainThresholdStock = 10f;
        [SerializeField] private float villageSustainTolerancePeriodHours = 24f;

        [Header("Success-criteria observation window (SC-003/004/005)")]
        [SerializeField] private float definedGamePeriodHours = 72f;

        /// <summary>
        /// Creates an in-memory instance without going through the asset database.
        /// Intended for tests; normal usage authors this as a single project asset.
        /// </summary>
        public static BalancingConfig CreateForTesting(
            float hungerAlertThreshold = 60f,
            float hungerCriticalThreshold = 85f,
            float hungerIncreasePerSimulatedHour = 2f,
            float sanityAlertThreshold = 40f,
            float sanityCriticalThreshold = 15f,
            float villageSustainThresholdStock = 10f,
            float villageSustainTolerancePeriodHours = 24f,
            float definedGamePeriodHours = 72f)
        {
            var instance = CreateInstance<BalancingConfig>();
            instance.hungerAlertThreshold = hungerAlertThreshold;
            instance.hungerCriticalThreshold = hungerCriticalThreshold;
            instance.hungerIncreasePerSimulatedHour = hungerIncreasePerSimulatedHour;
            instance.sanityAlertThreshold = sanityAlertThreshold;
            instance.sanityCriticalThreshold = sanityCriticalThreshold;
            instance.villageSustainThresholdStock = villageSustainThresholdStock;
            instance.villageSustainTolerancePeriodHours = villageSustainTolerancePeriodHours;
            instance.definedGamePeriodHours = definedGamePeriodHours;
            return instance;
        }

        public float HungerAlertThreshold => hungerAlertThreshold;
        public float HungerCriticalThreshold => hungerCriticalThreshold;
        public float HungerIncreasePerSimulatedHour => hungerIncreasePerSimulatedHour;

        public float SanityAlertThreshold => sanityAlertThreshold;
        public float SanityCriticalThreshold => sanityCriticalThreshold;

        /// <summary>Minimum essential resource stock a community needs to be considered sustained.</summary>
        public float VillageSustainThresholdStock => villageSustainThresholdStock;

        /// <summary>How long a community may remain below the sustain threshold before losing population.</summary>
        public float VillageSustainTolerancePeriodHours => villageSustainTolerancePeriodHours;

        /// <summary>Reference window (in simulated hours) used to evaluate SC-003/004/005.</summary>
        public float DefinedGamePeriodHours => definedGamePeriodHours;
    }
}
