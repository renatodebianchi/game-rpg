using System;

namespace GameRpg.World
{
    /// <summary>
    /// A recorded player decision used to derive reputation/world consequences
    /// (FR-013; data-model.md, "Escolha de Impacto").
    /// </summary>
    public class ImpactfulChoice
    {
        public string ChoiceId { get; }
        public ImpactfulChoiceType Type { get; }
        public string TargetCommunityId { get; }
        public string RelatedNpcId { get; }
        public string RelatedResourceId { get; }
        public int Quantity { get; }
        public TimeSpan SimulatedTimestamp { get; }

        public ImpactfulChoice(
            string choiceId,
            ImpactfulChoiceType type,
            string targetCommunityId,
            TimeSpan simulatedTimestamp,
            string relatedNpcId = null,
            string relatedResourceId = null,
            int quantity = 0)
        {
            ChoiceId = choiceId ?? throw new ArgumentNullException(nameof(choiceId));
            Type = type;
            TargetCommunityId = targetCommunityId ?? throw new ArgumentNullException(nameof(targetCommunityId));
            SimulatedTimestamp = simulatedTimestamp;
            RelatedNpcId = relatedNpcId;
            RelatedResourceId = relatedResourceId;
            Quantity = quantity;
        }
    }
}
