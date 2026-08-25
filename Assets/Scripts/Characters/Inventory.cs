using System;
using System.Collections.Generic;

namespace GameRpg.Characters
{
    /// <summary>Resource/item quantities carried by a Character, keyed by resourceId.</summary>
    [Serializable]
    public class Inventory
    {
        private readonly Dictionary<string, int> _quantitiesByResourceId = new Dictionary<string, int>();

        public int GetQuantity(string resourceId) =>
            _quantitiesByResourceId.TryGetValue(resourceId, out var quantity) ? quantity : 0;

        public void Add(string resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            _quantitiesByResourceId[resourceId] = GetQuantity(resourceId) + amount;
        }

        public void Remove(string resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var current = GetQuantity(resourceId);
            if (amount > current)
            {
                throw new InvalidOperationException(
                    $"Cannot remove {amount} of '{resourceId}'; only {current} available.");
            }

            _quantitiesByResourceId[resourceId] = current - amount;
        }

        public IReadOnlyDictionary<string, int> AsReadOnly() => _quantitiesByResourceId;
    }
}
