using System;
using UnityEngine;

namespace GameRpg.Characters
{
    public enum BodyType
    {
        Slim,
        Sturdy
    }

    public enum SkinTone
    {
        Light,
        Medium,
        Dark
    }

    public enum HairStyle
    {
        Short,
        Long,
        Bald
    }

    /// <summary>
    /// Purely cosmetic character-creation choices (FR-006, FR-007) — no effect
    /// on attributes, combat, or skills. Default values are applied to any
    /// characteristic the player leaves unselected (FR-007), so finalization
    /// is never blocked by an incomplete appearance choice.
    /// </summary>
    [Serializable]
    public struct VisualCharacteristics
    {
        public BodyType BodyType;
        public SkinTone SkinTone;
        public HairStyle HairStyle;
        public Color HairColor;

        public static VisualCharacteristics Default => new VisualCharacteristics
        {
            BodyType = BodyType.Slim,
            SkinTone = SkinTone.Medium,
            HairStyle = HairStyle.Short,
            HairColor = new Color(0.25f, 0.15f, 0.1f), // dark brown
        };
    }
}
