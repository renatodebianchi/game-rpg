using System;
using GameRpg.Core;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class VillagePermanentInactivationTests
    {
        private const string FoodResourceId = "food";

        [Test]
        public void PopulationReachingZero_MarksCommunityPermanentlyInactive()
        {
            var community = new Community("village", new[] { "npc1" });
            var config = BalancingConfig.CreateForTesting(villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 1f);
            var service = new VillageEconomySimulationService(config);

            var result = service.Tick(community, FoodResourceId, TimeSpan.FromHours(2)); // no stock at all, stays below threshold

            Assert.IsTrue(community.IsPermanentlyInactive);
            Assert.IsTrue(result.IsPermanentlyInactive);
            Assert.AreEqual(CommunityEconomyState.Collapsed, result.UpdatedEconomyState);
        }

        [Test]
        public void RestockingAfterPermanentInactivation_DoesNotResumeSimulation()
        {
            var community = new Community("village", new[] { "npc1" });
            var config = BalancingConfig.CreateForTesting(villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 1f);
            var service = new VillageEconomySimulationService(config);

            service.Tick(community, FoodResourceId, TimeSpan.FromHours(2));
            Assert.IsTrue(community.IsPermanentlyInactive);

            community.AddResourceStock(FoodResourceId, 1000f); // restock after the fact
            var resultAfterRestock = service.Tick(community, FoodResourceId, TimeSpan.FromHours(1));

            // Rule 6: still inactive, and the tick is a no-op (stock untouched by consumption).
            Assert.IsTrue(community.IsPermanentlyInactive);
            Assert.AreEqual(1000f, resultAfterRestock.UpdatedEssentialResourceStock, 0.001f);
            Assert.AreEqual(CommunityEconomyState.Collapsed, resultAfterRestock.UpdatedEconomyState);
        }

        [Test]
        public void ReputationDeltas_AreIgnoredOnceCommunityIsPermanentlyInactive()
        {
            var community = new Community("village", Array.Empty<string>());
            community.MarkPermanentlyInactive();

            community.ApplyReputationDelta(10);

            Assert.AreEqual(0, community.ReputationWithPlayer);
        }
    }
}
