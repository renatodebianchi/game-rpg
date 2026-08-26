using GameRpg.Characters;

namespace GameRpg.Core
{
    /// <summary>
    /// In-memory transfer of the finalized Character from Character Creation to the
    /// Exploration scene (contracts/scene-transition-contract.md). Not persisted to
    /// disk — save/load already covers cross-session persistence; this only bridges a
    /// single scene load within the same session.
    /// </summary>
    public static class PendingPlayerCharacter
    {
        public static Character Character { get; private set; }

        public static void Set(Character character)
        {
            Character = character;
        }

        /// <summary>Reads and clears the pending character, so a later direct scene
        /// visit never reuses a character from an earlier session (contract rule 2).</summary>
        public static Character Consume()
        {
            var character = Character;
            Character = null;
            return character;
        }
    }
}
