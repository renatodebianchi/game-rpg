using System;
using System.Collections.Generic;
using GameRpg.Skills;

namespace GameRpg.Core
{
    /// <summary>Thrown when a saved/runtime reference cannot be resolved against loaded content.</summary>
    public class ContentValidationException : Exception
    {
        public ContentValidationException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Resolves content references (skill nodes, resources, NPCs, communities) by id,
    /// failing loudly instead of silently defaulting (FR-017 / save-data-contract.md, rule 2).
    /// </summary>
    public static class ContentValidation
    {
        public static T ResolveOrThrow<T>(IReadOnlyDictionary<string, T> registry, string id, string contentTypeName)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ContentValidationException($"A {contentTypeName} id was null or empty.");
            }

            if (!registry.TryGetValue(id, out var value))
            {
                throw new ContentValidationException(
                    $"{contentTypeName} '{id}' could not be resolved against loaded content.");
            }

            return value;
        }

        /// <summary>
        /// Validates that a set of skill nodes contains no prerequisite cycles
        /// (contracts/skill-node-data-contract.md, invariant 1).
        /// </summary>
        public static void ValidateNoSkillGraphCycles(IEnumerable<SkillNodeDefinition> allNodes)
        {
            var visiting = new HashSet<SkillNodeDefinition>();
            var visited = new HashSet<SkillNodeDefinition>();

            foreach (var node in allNodes)
            {
                VisitForCycleCheck(node, visiting, visited);
            }
        }

        private static void VisitForCycleCheck(
            SkillNodeDefinition node,
            HashSet<SkillNodeDefinition> visiting,
            HashSet<SkillNodeDefinition> visited)
        {
            if (node == null || visited.Contains(node))
            {
                return;
            }

            if (!visiting.Add(node))
            {
                throw new ContentValidationException(
                    $"Skill node prerequisite cycle detected involving '{node.NodeId}'.");
            }

            foreach (var prerequisite in node.Prerequisites)
            {
                VisitForCycleCheck(prerequisite, visiting, visited);
            }

            visiting.Remove(node);
            visited.Add(node);
        }
    }
}
