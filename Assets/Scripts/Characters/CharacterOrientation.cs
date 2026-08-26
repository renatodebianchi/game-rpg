namespace GameRpg.Characters
{
    /// <summary>
    /// Predominant orientation chosen during character creation, used only to
    /// determine the starting equipment kit (FR-004). Unrelated to
    /// Skills.SkillTrack: it never restricts which skill-tree nodes the player
    /// may invest in afterwards, and is not persisted past creation.
    /// </summary>
    public enum CharacterOrientation
    {
        Combatant,
        Arcanist
    }
}
