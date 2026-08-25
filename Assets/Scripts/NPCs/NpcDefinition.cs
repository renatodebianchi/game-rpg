using UnityEngine;

namespace GameRpg.NPCs
{
    /// <summary>
    /// Content-authored definition of an NPC archetype (see data-model.md, "NPC").
    /// Runtime, per-save state (life state, community membership at runtime) is
    /// tracked separately, not on this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "Npc", menuName = "GameRpg/NPCs/Npc")]
    public class NpcDefinition : ScriptableObject
    {
        [SerializeField] private string npcId;
        [SerializeField] private string displayName;
        [SerializeField] private string communityId;

        public static NpcDefinition CreateForTesting(string npcId, string displayName, string communityId)
        {
            var instance = CreateInstance<NpcDefinition>();
            instance.npcId = npcId;
            instance.displayName = displayName;
            instance.communityId = communityId;
            return instance;
        }

        public string NpcId => npcId;
        public string DisplayName => displayName;

        /// <summary>Home community/faction id (see World.CommunityDefinition).</summary>
        public string CommunityId => communityId;
    }
}
