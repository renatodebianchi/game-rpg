using System;
using System.Collections.Generic;

namespace GameRpg.World
{
    /// <summary>Records ImpactfulChoices as they happen (FR-013).</summary>
    public class ImpactfulChoiceLog
    {
        private readonly List<ImpactfulChoice> _entries = new List<ImpactfulChoice>();
        private int _nextChoiceNumber = 1;

        public IReadOnlyList<ImpactfulChoice> Entries => _entries;

        public event Action<ImpactfulChoice> ChoiceRecorded;

        public ImpactfulChoice Record(
            ImpactfulChoiceType type,
            string targetCommunityId,
            TimeSpan simulatedTimestamp,
            string relatedNpcId = null,
            string relatedResourceId = null,
            int quantity = 0)
        {
            var choice = new ImpactfulChoice(
                $"choice-{_nextChoiceNumber++}",
                type,
                targetCommunityId,
                simulatedTimestamp,
                relatedNpcId,
                relatedResourceId,
                quantity);

            _entries.Add(choice);
            ChoiceRecorded?.Invoke(choice);
            return choice;
        }
    }
}
