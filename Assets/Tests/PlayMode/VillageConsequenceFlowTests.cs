using System;
using GameRpg.Core;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.PlayMode
{
    public class VillageConsequenceFlowTests
    {
        private const string FoodResourceId = "food";

        [Test]
        public void RemovingResource_ThenRestoringIt_StopsFurtherPopulationDecline()
        {
            var community = new Community("village", new[] { "npc1", "npc2", "npc3" });
            var config = BalancingConfig.CreateForTesting(
                villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 5f);
            var service = new VillageEconomySimulationService(config);
            var worldClock = new WorldClock();

            // Village starts with no reserve beyond what's needed for a couple of hours.
            community.AddResourceStock(FoodResourceId, 2f);

            // Advance time in 1-hour ticks while resources are depleted (player removed the supply).
            for (var hour = 0; hour < 6; hour++)
            {
                worldClock.Advance(TimeSpan.FromHours(1));
                service.Tick(community, FoodResourceId, TimeSpan.FromHours(1));
            }

            var populationAfterDecline = community.PopulationNpcIds.Count;
            Assert.Less(populationAfterDecline, 3, "Population should have declined from starvation.");

            // Player restores the resource well above the sustain threshold.
            community.AddResourceStock(FoodResourceId, 1000f);

            for (var hour = 0; hour < 5; hour++)
            {
                worldClock.Advance(TimeSpan.FromHours(1));
                service.Tick(community, FoodResourceId, TimeSpan.FromHours(1));
            }

            Assert.AreEqual(
                populationAfterDecline, community.PopulationNpcIds.Count,
                "Population loss should stop once resources are restored above the sustain threshold.");
            Assert.IsFalse(community.IsPermanentlyInactive);
        }
    }
}
