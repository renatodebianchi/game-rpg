using System;

namespace GameRpg.Characters
{
    public enum SanityRecoveryMethod
    {
        Rest,
        Item,
        SafeEnvironment
    }

    /// <summary>
    /// Recovery actions that restore sanity (FR-010's acceptance scenario 4):
    /// resting, using an item, or being in a safe environment.
    /// </summary>
    public class SanityRecoveryAction
    {
        private readonly SanitySystem _sanitySystem;

        public SanityRecoveryAction(SanitySystem sanitySystem)
        {
            _sanitySystem = sanitySystem ?? throw new ArgumentNullException(nameof(sanitySystem));
        }

        public void Recover(SanityRecoveryMethod method, float amount)
        {
            if (amount < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            // The recovery method is content/UX metadata (which action the player took);
            // the amount restored is what actually changes sanity, decided by the caller
            // based on content data (e.g., an item's potency) for each method.
            _sanitySystem.Recover(amount);
        }
    }
}
