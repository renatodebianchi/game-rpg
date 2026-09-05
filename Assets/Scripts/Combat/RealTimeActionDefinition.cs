using UnityEngine;

namespace GameRpg.Combat
{
    public enum RealTimeActionKind
    {
        Melee,
        Ranged,
        Skill
    }

    /// <summary>
    /// Content-authored definition of a real-time combat action (basic attack,
    /// ranged attack, or a skill/spell) — data-driven per Principle II, same
    /// spirit as Skills.SkillNodeDefinition. See
    /// specs/004-2d-real-time-combat/contracts/realtime-action-contract.md for
    /// the full execution contract.
    /// </summary>
    [CreateAssetMenu(fileName = "RealTimeAction", menuName = "GameRpg/Combat/Real-Time Action")]
    public class RealTimeActionDefinition : ScriptableObject
    {
        [SerializeField] private string actionId;
        [SerializeField] private RealTimeActionKind kind;
        [SerializeField] private float range = 1f;
        [SerializeField] private float executionTime;
        [SerializeField] private float cooldown;
        [SerializeField] private float resourceCost;
        [SerializeField] private int baseDamage;
        [SerializeField] private string requiredCapabilityId;

        public string ActionId => actionId;
        public RealTimeActionKind Kind => kind;
        public float Range => range;
        public float ExecutionTime => executionTime;
        public float Cooldown => cooldown;
        public float ResourceCost => resourceCost;

        /// <summary>Base damage this action deals before IDamageModifier adjustments (e.g., hunger/sanity penalties).</summary>
        public int BaseDamage => baseDamage;

        public string RequiredCapabilityId => requiredCapabilityId;

        /// <summary>
        /// Creates an in-memory instance without going through the asset database.
        /// Intended for tests and tooling; normal content authoring should create
        /// these as ScriptableObject assets via the Editor (Assets/Data/Combat).
        /// </summary>
        public static RealTimeActionDefinition CreateForTesting(
            string actionId,
            RealTimeActionKind kind,
            float range = 1f,
            float executionTime = 0f,
            float cooldown = 0f,
            float resourceCost = 0f,
            int baseDamage = 0,
            string requiredCapabilityId = null)
        {
            var instance = CreateInstance<RealTimeActionDefinition>();
            instance.actionId = actionId;
            instance.kind = kind;
            instance.range = range;
            instance.executionTime = executionTime;
            instance.cooldown = cooldown;
            instance.resourceCost = resourceCost;
            instance.baseDamage = baseDamage;
            instance.requiredCapabilityId = requiredCapabilityId;
            return instance;
        }
    }
}
