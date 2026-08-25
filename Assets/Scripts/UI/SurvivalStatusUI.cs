using GameRpg.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace GameRpg.UI
{
    /// <summary>
    /// Displays hunger/sanity indicators and active penalty icons, including the
    /// combined state when both are critical simultaneously (FR-021).
    /// Presentation-only; exempt from automated test coverage per tasks.md's
    /// testing scope note.
    /// </summary>
    public class SurvivalStatusUI : MonoBehaviour
    {
        [SerializeField] private Slider hungerBar;
        [SerializeField] private Slider sanityBar;
        [SerializeField] private GameObject hungerPenaltyIcon;
        [SerializeField] private GameObject sanityPenaltyIcon;

        private HungerSystem _hungerSystem;
        private SanitySystem _sanitySystem;

        public void Initialize(HungerSystem hungerSystem, SanitySystem sanitySystem)
        {
            _hungerSystem = hungerSystem;
            _sanitySystem = sanitySystem;

            _hungerSystem.LevelChanged += _ => Refresh();
            _sanitySystem.LevelChanged += _ => Refresh();
            Refresh();
        }

        private void Refresh()
        {
            var hungerPenalized = _hungerSystem.CurrentLevel != SurvivalThresholdLevel.Normal;
            var sanityPenalized = _sanitySystem.CurrentLevel != SurvivalThresholdLevel.Normal;

            if (hungerPenaltyIcon != null)
            {
                hungerPenaltyIcon.SetActive(hungerPenalized);
            }

            if (sanityPenaltyIcon != null)
            {
                sanityPenaltyIcon.SetActive(sanityPenalized);
            }

            // Both bars/icons remain independently visible when both are critical
            // at once — penalties are cumulative (FR-021), not mutually exclusive.
        }
    }
}
