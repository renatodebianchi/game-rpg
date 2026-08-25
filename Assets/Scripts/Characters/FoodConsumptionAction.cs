using System;

namespace GameRpg.Characters
{
    /// <summary>
    /// Consuming food from inventory restores hunger proportionally to the
    /// amount consumed (FR-008's acceptance scenario 2).
    /// </summary>
    public class FoodConsumptionAction
    {
        private readonly Character _character;
        private readonly HungerSystem _hungerSystem;

        public FoodConsumptionAction(Character character, HungerSystem hungerSystem)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _hungerSystem = hungerSystem ?? throw new ArgumentNullException(nameof(hungerSystem));
        }

        /// <param name="resourceId">Food resource consumed from the character's inventory.</param>
        /// <param name="quantity">How many units are consumed.</param>
        /// <param name="hungerRestoredPerUnit">Hunger restored per unit consumed (content-defined).</param>
        public void Consume(string resourceId, int quantity, float hungerRestoredPerUnit)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            _character.Inventory.Remove(resourceId, quantity);
            _hungerSystem.Feed(hungerRestoredPerUnit * quantity);
        }
    }
}
