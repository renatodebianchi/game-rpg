using System;

namespace GameRpg.Core
{
    /// <summary>
    /// Tracks simulated in-game time, independent of real-world/render frame time.
    /// Consumed by hunger/sanity decay (Characters, US3) and by the village economy
    /// simulation tick (World.VillageEconomySimulationService, US4).
    /// </summary>
    public class WorldClock
    {
        /// <summary>Total simulated time elapsed since a new game/save was started.</summary>
        public TimeSpan ElapsedSimulatedTime { get; private set; }

        public event Action<TimeSpan> TimeAdvanced;

        /// <summary>
        /// Advances simulated time by <paramref name="delta"/>. Called by the game loop,
        /// scaled by whatever real-time-to-simulated-time ratio the game uses (not decided
        /// here; this class only tracks the resulting simulated timeline).
        /// </summary>
        public void Advance(TimeSpan delta)
        {
            if (delta < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(delta), "Simulated time cannot move backwards.");
            }

            ElapsedSimulatedTime += delta;
            TimeAdvanced?.Invoke(ElapsedSimulatedTime);
        }

        /// <summary>Restores elapsed time from a loaded save (see contracts/save-data-contract.md).</summary>
        public void RestoreFromSave(TimeSpan elapsedSimulatedTime)
        {
            if (elapsedSimulatedTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsedSimulatedTime));
            }

            ElapsedSimulatedTime = elapsedSimulatedTime;
        }
    }
}
