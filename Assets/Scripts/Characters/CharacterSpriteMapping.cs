using UnityEngine;

namespace GameRpg.Characters
{
    /// <summary>Result of resolving VisualCharacteristics to a concrete sprite appearance (FR-005).</summary>
    public readonly struct CharacterSpriteAppearance
    {
        public readonly string SpriteResourceName;
        public readonly Color TintColor;

        public CharacterSpriteAppearance(string spriteResourceName, Color tintColor)
        {
            SpriteResourceName = spriteResourceName;
            TintColor = tintColor;
        }
    }

    /// <summary>
    /// Static, best-effort translation of VisualCharacteristics (feature 002) into a
    /// sprite/tint pair (feature 003, FR-005). See
    /// contracts/character-sprite-mapping-contract.md — this module never throws and
    /// always returns a valid result, even for combinations without a distinct frame.
    /// Sprites are loaded at runtime via Resources.Load using SpriteResourceName
    /// (see ExplorationCharacterController), keeping this class free of Unity asset
    /// references so it stays testable without a loaded scene (contract rule 4).
    /// </summary>
    public static class CharacterSpriteMapping
    {
        private const string DefaultSpriteResourceName = "character_default";

        public static CharacterSpriteAppearance Resolve(VisualCharacteristics visuals)
        {
            var spriteResourceName = visuals.BodyType switch
            {
                BodyType.Slim => "character_slim",
                BodyType.Sturdy => "character_sturdy",
                _ => DefaultSpriteResourceName,
            };

            var tintColor = visuals.SkinTone switch
            {
                SkinTone.Light => new Color(1.15f, 1.05f, 0.95f),
                SkinTone.Medium => Color.white,
                SkinTone.Dark => new Color(0.55f, 0.4f, 0.3f),
                _ => Color.white,
            };

            return new CharacterSpriteAppearance(spriteResourceName, tintColor);
        }
    }
}
