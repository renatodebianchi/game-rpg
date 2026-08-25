using System;

namespace GameRpg.Combat
{
    /// <summary>
    /// Per-turn action economy for a single combatant: movement, one action,
    /// one bonus action (FR-002). Shared by Characters.Character and any
    /// non-player combatant; owned/reset by Combat.TurnResourceManager.
    /// </summary>
    [Serializable]
    public class TurnResources
    {
        public int MaxMovementPoints { get; private set; }
        public int MovementPointsRemaining { get; private set; }
        public bool ActionAvailable { get; private set; }
        public bool BonusActionAvailable { get; private set; }

        public TurnResources(int maxMovementPoints)
        {
            if (maxMovementPoints < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxMovementPoints));
            }

            MaxMovementPoints = maxMovementPoints;
            ResetForNewTurn();
        }

        /// <summary>Restores movement/action/bonus action at the start of this combatant's turn.</summary>
        public void ResetForNewTurn()
        {
            MovementPointsRemaining = MaxMovementPoints;
            ActionAvailable = true;
            BonusActionAvailable = true;
        }

        public void ConsumeMovement(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount > MovementPointsRemaining)
            {
                throw new InvalidOperationException("Not enough movement points remaining this turn.");
            }

            MovementPointsRemaining -= amount;
        }

        public void ConsumeAction()
        {
            if (!ActionAvailable)
            {
                throw new InvalidOperationException("Action already used this turn.");
            }

            ActionAvailable = false;
        }

        public void ConsumeBonusAction()
        {
            if (!BonusActionAvailable)
            {
                throw new InvalidOperationException("Bonus action already used this turn.");
            }

            BonusActionAvailable = false;
        }
    }
}
