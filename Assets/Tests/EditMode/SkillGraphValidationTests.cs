using GameRpg.Core;
using GameRpg.Skills;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class SkillGraphValidationTests
    {
        [Test]
        public void AcyclicGraph_DoesNotThrow()
        {
            var root = SkillNodeDefinition.CreateForTesting("root", SkillTrack.Combatant);
            var child = SkillNodeDefinition.CreateForTesting("child", SkillTrack.Combatant, new[] { root });

            Assert.DoesNotThrow(() => ContentValidation.ValidateNoSkillGraphCycles(new[] { root, child }));
        }

        [Test]
        public void DirectCycle_Throws()
        {
            // Two nodes whose Prerequisites lists reference each other.
            var a = SkillNodeDefinition.CreateForTesting("a", SkillTrack.Combatant);
            var b = SkillNodeDefinition.CreateForTesting("b", SkillTrack.Combatant, new[] { a });
            typeof(SkillNodeDefinition)
                .GetField("prerequisites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(a, new System.Collections.Generic.List<SkillNodeDefinition> { b });

            Assert.Throws<ContentValidationException>(() => ContentValidation.ValidateNoSkillGraphCycles(new[] { a, b }));
        }

        [Test]
        public void HybridNode_WithOnlyOneTrackPrerequisite_FailsInvariantCheck()
        {
            var combatantRoot = SkillNodeDefinition.CreateForTesting("combatant-root", SkillTrack.Combatant);
            var invalidHybrid = SkillNodeDefinition.CreateForTesting(
                "invalid-hybrid", SkillTrack.Hybrid, new[] { combatantRoot });

            Assert.IsFalse(invalidHybrid.HasValidHybridPrerequisites());
        }
    }
}
