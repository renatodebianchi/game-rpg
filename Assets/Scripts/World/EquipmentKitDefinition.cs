using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Characters;
using UnityEngine;

namespace GameRpg.World
{
    [Serializable]
    public class EquipmentKitItemEntry
    {
        public ResourceDefinition resource;
        public int quantity = 1;
    }

    /// <summary>
    /// Content-authored fixed starting equipment kit for one CharacterOrientation
    /// (FR-005; data-model.md, "EquipmentKitDefinition"). Its items are added
    /// directly to the character's existing Inventory on finalization — there
    /// is no separate equipment-slot system (research.md).
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentKit", menuName = "GameRpg/World/Equipment Kit")]
    public class EquipmentKitDefinition : ScriptableObject
    {
        [SerializeField] private CharacterOrientation orientation;
        [SerializeField] private List<EquipmentKitItemEntry> items = new List<EquipmentKitItemEntry>();

        public CharacterOrientation Orientation => orientation;
        public IReadOnlyList<EquipmentKitItemEntry> Items => items;

        public static EquipmentKitDefinition CreateForTesting(
            CharacterOrientation orientation, IEnumerable<(ResourceDefinition resource, int quantity)> items)
        {
            var instance = CreateInstance<EquipmentKitDefinition>();
            instance.orientation = orientation;
            instance.items = items
                .Select(entry => new EquipmentKitItemEntry { resource = entry.resource, quantity = entry.quantity })
                .ToList();
            return instance;
        }
    }
}
