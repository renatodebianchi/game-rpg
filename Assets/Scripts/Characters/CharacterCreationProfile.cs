using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Core;
using GameRpg.World;

namespace GameRpg.Characters
{
    /// <summary>
    /// Aggregates the in-progress state of all three character-creation user
    /// stories (attribute allocation, orientation, visual characteristics)
    /// until Finalize() applies them to a live Character
    /// (contracts/character-creation-finalization-contract.md).
    /// </summary>
    public class CharacterCreationProfile
    {
        public AttributeAllocationState AttributeAllocation { get; } = new AttributeAllocationState();

        /// <summary>Null until the player chooses one; finalization requires a choice (contract, precondition 2).</summary>
        public CharacterOrientation? Orientation { get; set; }

        public VisualCharacteristics VisualCharacteristics { get; set; } = VisualCharacteristics.Default;

        /// <summary>
        /// Applies the current allocation/orientation/appearance to
        /// <paramref name="character"/>: sets its attributes, adds the matching
        /// EquipmentKitDefinition's items to its inventory, and applies its
        /// visual characteristics. Once this succeeds, base attributes are
        /// treated as fixed for the rest of the campaign (FR-012) — no API in
        /// this feature allows calling Finalize() again for the same character.
        /// </summary>
        public void Finalize(Character character, IEnumerable<EquipmentKitDefinition> availableEquipmentKits)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            if (AttributeAllocation.PointsRemaining != 0)
            {
                throw new InvalidOperationException(
                    $"Cannot finalize character creation while {AttributeAllocation.PointsRemaining} " +
                    "Point Buy points remain unspent (or over-spent).");
            }

            if (Orientation == null)
            {
                throw new InvalidOperationException(
                    "Cannot finalize character creation without choosing an orientation " +
                    "(needed to determine the starting equipment kit).");
            }

            character.Attributes = AttributeAllocation.ToCharacterAttributes();

            var kit = ResolveEquipmentKit(Orientation.Value, availableEquipmentKits);
            foreach (var entry in kit.Items)
            {
                character.Inventory.Add(entry.resource.ResourceId, entry.quantity);
            }

            character.Visuals = VisualCharacteristics;
        }

        private static EquipmentKitDefinition ResolveEquipmentKit(
            CharacterOrientation orientation, IEnumerable<EquipmentKitDefinition> availableEquipmentKits)
        {
            var kit = availableEquipmentKits.FirstOrDefault(k => k.Orientation == orientation);
            if (kit == null)
            {
                throw new ContentValidationException(
                    $"No EquipmentKitDefinition found for orientation '{orientation}'.");
            }

            return kit;
        }
    }
}
