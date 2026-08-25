using System;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class ReputationServiceTests
    {
        [Test]
        public void SavingNpc_IncreasesReputationWithTargetCommunity()
        {
            var villageA = new Community("village_a", Array.Empty<string>());
            var log = new ImpactfulChoiceLog();
            var service = new ReputationService(new[] { villageA }, log);

            log.Record(ImpactfulChoiceType.SaveNpc, "village_a", TimeSpan.Zero, relatedNpcId: "npc1");

            Assert.Greater(villageA.ReputationWithPlayer, 0);
        }

        [Test]
        public void AbandoningNpc_DecreasesReputationWithTargetCommunity()
        {
            var villageA = new Community("village_a", Array.Empty<string>());
            var log = new ImpactfulChoiceLog();
            var service = new ReputationService(new[] { villageA }, log);

            log.Record(ImpactfulChoiceType.AbandonOrHarmNpc, "village_a", TimeSpan.Zero, relatedNpcId: "npc1");

            Assert.Less(villageA.ReputationWithPlayer, 0);
        }

        [Test]
        public void ChoiceAffectingOneCommunity_DoesNotAffectAnyOtherCommunity_EvenRivals()
        {
            var villageA = new Community("village_a", Array.Empty<string>());
            var villageB = new Community("village_b", Array.Empty<string>());
            var log = new ImpactfulChoiceLog();
            var service = new ReputationService(new[] { villageA, villageB }, log);

            log.Record(ImpactfulChoiceType.SaveNpc, "village_a", TimeSpan.Zero, relatedNpcId: "npc1");

            Assert.Greater(villageA.ReputationWithPlayer, 0);
            Assert.AreEqual(0, villageB.ReputationWithPlayer);
        }

        [Test]
        public void TransportingResource_IncreasesReputationWithReceivingCommunity()
        {
            var villageA = new Community("village_a", Array.Empty<string>());
            var log = new ImpactfulChoiceLog();
            var service = new ReputationService(new[] { villageA }, log);

            log.Record(ImpactfulChoiceType.TransportResource, "village_a", TimeSpan.Zero, relatedResourceId: "food", quantity: 10);

            Assert.Greater(villageA.ReputationWithPlayer, 0);
        }
    }
}
