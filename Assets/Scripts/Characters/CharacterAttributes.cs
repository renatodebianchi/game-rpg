using System;

namespace GameRpg.Characters
{
    /// <summary>
    /// Base attributes used by combat formulas and by skill-node prerequisites.
    /// Kept intentionally small for the MVP; extend as content requires.
    /// </summary>
    [Serializable]
    public struct CharacterAttributes
    {
        public int Strength;
        public int Dexterity;
        public int Intellect;
        public int Willpower;

        public CharacterAttributes(int strength, int dexterity, int intellect, int willpower)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intellect = intellect;
            Willpower = willpower;
        }
    }
}
