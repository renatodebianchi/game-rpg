using UnityEngine;

namespace GameRpg.World
{
    /// <summary>
    /// Content-authored definition of a community/faction (e.g., a village).
    /// Runtime state (current resource stock, population, reputation,
    /// isPermanentlyInactive) is tracked separately by World.Community
    /// (see data-model.md, "Community/Faction").
    /// </summary>
    [CreateAssetMenu(fileName = "Community", menuName = "GameRpg/World/Community")]
    public class CommunityDefinition : ScriptableObject
    {
        [SerializeField] private string communityId;
        [SerializeField] private string displayName;
        [SerializeField] private int startingPopulation = 10;
        [SerializeField] private int startingEssentialResourceStock = 100;

        public static CommunityDefinition CreateForTesting(
            string communityId, string displayName, int startingPopulation, int startingEssentialResourceStock)
        {
            var instance = CreateInstance<CommunityDefinition>();
            instance.communityId = communityId;
            instance.displayName = displayName;
            instance.startingPopulation = startingPopulation;
            instance.startingEssentialResourceStock = startingEssentialResourceStock;
            return instance;
        }

        public string CommunityId => communityId;
        public string DisplayName => displayName;
        public int StartingPopulation => startingPopulation;
        public int StartingEssentialResourceStock => startingEssentialResourceStock;
    }
}
