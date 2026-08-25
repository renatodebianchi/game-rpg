namespace GameRpg.NPCs
{
    public enum NpcInteractionTone
    {
        Friendly,
        Neutral,
        Hostile
    }

    /// <summary>
    /// Derives NPC interaction behavior (dialogue tone, price, mission
    /// availability) from the player's reputation with the NPC's community (FR-016).
    /// </summary>
    public static class NpcInteractionGate
    {
        private const int FriendlyThreshold = 10;
        private const int HostileThreshold = -10;

        public static NpcInteractionTone GetTone(int reputation)
        {
            if (reputation <= HostileThreshold)
            {
                return NpcInteractionTone.Hostile;
            }

            return reputation >= FriendlyThreshold ? NpcInteractionTone.Friendly : NpcInteractionTone.Neutral;
        }

        public static bool AreMissionsAvailable(int reputation) => GetTone(reputation) != NpcInteractionTone.Hostile;

        public static float GetPriceMultiplier(int reputation) => GetTone(reputation) switch
        {
            NpcInteractionTone.Hostile => 1.5f,
            NpcInteractionTone.Friendly => 0.9f,
            _ => 1.0f,
        };
    }
}
