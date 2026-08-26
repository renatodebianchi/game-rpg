using System;
using GameRpg.Characters;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class CharacterSpriteMappingTests
    {
        [Test]
        public void Resolve_SameSkinTone_AlwaysProducesTheSameTintColor()
        {
            // Contract rule 1: fixed, deterministic tint per SkinTone.
            var visualsA = new VisualCharacteristics { SkinTone = SkinTone.Dark, BodyType = BodyType.Slim };
            var visualsB = new VisualCharacteristics { SkinTone = SkinTone.Dark, BodyType = BodyType.Sturdy };

            var appearanceA = CharacterSpriteMapping.Resolve(visualsA);
            var appearanceB = CharacterSpriteMapping.Resolve(visualsB);

            Assert.AreEqual(appearanceA.TintColor, appearanceB.TintColor);
        }

        [Test]
        public void Resolve_DifferentSkinTones_ProduceDistinctTintColors()
        {
            var light = CharacterSpriteMapping.Resolve(new VisualCharacteristics { SkinTone = SkinTone.Light });
            var medium = CharacterSpriteMapping.Resolve(new VisualCharacteristics { SkinTone = SkinTone.Medium });
            var dark = CharacterSpriteMapping.Resolve(new VisualCharacteristics { SkinTone = SkinTone.Dark });

            Assert.AreNotEqual(light.TintColor, medium.TintColor);
            Assert.AreNotEqual(medium.TintColor, dark.TintColor);
            Assert.AreNotEqual(light.TintColor, dark.TintColor);
        }

        [Test]
        public void Resolve_DifferentBodyTypes_ProduceDistinctSpriteFrames()
        {
            // Contract rule 2: a distinct frame is used when one is available.
            var slim = CharacterSpriteMapping.Resolve(new VisualCharacteristics { BodyType = BodyType.Slim });
            var sturdy = CharacterSpriteMapping.Resolve(new VisualCharacteristics { BodyType = BodyType.Sturdy });

            Assert.AreNotEqual(slim.SpriteResourceName, sturdy.SpriteResourceName);
        }

        [Test]
        public void Resolve_NeverThrowsForAnyCombinationOfVisualCharacteristics()
        {
            // Contract rule 3: best-effort, never an error.
            foreach (BodyType bodyType in Enum.GetValues(typeof(BodyType)))
            {
                foreach (SkinTone skinTone in Enum.GetValues(typeof(SkinTone)))
                {
                    foreach (HairStyle hairStyle in Enum.GetValues(typeof(HairStyle)))
                    {
                        var visuals = new VisualCharacteristics
                        {
                            BodyType = bodyType,
                            SkinTone = skinTone,
                            HairStyle = hairStyle,
                        };

                        Assert.DoesNotThrow(() => CharacterSpriteMapping.Resolve(visuals));

                        var appearance = CharacterSpriteMapping.Resolve(visuals);
                        Assert.IsFalse(string.IsNullOrEmpty(appearance.SpriteResourceName));
                    }
                }
            }
        }
    }
}
