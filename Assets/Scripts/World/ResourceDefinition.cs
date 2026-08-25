using UnityEngine;

namespace GameRpg.World
{
    /// <summary>
    /// Content-authored definition of an essential or tradeable resource
    /// (see data-model.md, "Resource").
    /// </summary>
    [CreateAssetMenu(fileName = "Resource", menuName = "GameRpg/World/Resource")]
    public class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private string resourceId;
        [SerializeField] private string displayName;
        [SerializeField] private bool isEssential;

        public static ResourceDefinition CreateForTesting(string resourceId, string displayName, bool isEssential)
        {
            var instance = CreateInstance<ResourceDefinition>();
            instance.resourceId = resourceId;
            instance.displayName = displayName;
            instance.isEssential = isEssential;
            return instance;
        }

        public string ResourceId => resourceId;
        public string DisplayName => displayName;

        /// <summary>
        /// Whether this resource affects a Community's survival simulation
        /// (see contracts/village-economy-simulation-contract.md).
        /// </summary>
        public bool IsEssential => isEssential;
    }
}
