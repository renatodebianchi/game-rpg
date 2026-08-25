namespace GameRpg.NPCs
{
    /// <summary>
    /// Life state of an NPC (see data-model.md, "NPC"). Transitions to Dead are
    /// permanent, whether caused by village starvation (FR-014/FR-019) or by
    /// harm during a forced-combat encounter (FR-022).
    /// </summary>
    public enum NpcLifeState
    {
        Alive,
        Rescued,
        AtRisk,
        Dead
    }
}
