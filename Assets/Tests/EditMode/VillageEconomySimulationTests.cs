using System;
using GameRpg.Core;
using GameRpg.World;
using NUnit.Framework;

namespace GameRpg.Tests.EditMode
{
    public class VillageEconomySimulationTests
    {
        private const string FoodResourceId = "food";

        private static Community CreateCommunity(int population, float startingStock)
        {
            var community = new Community("village", new[] { "npc1", "npc2", "npc3" }[..population]);
            community.AddResourceStock(FoodResourceId, startingStock);
            return community;
        }

        [Test]
        public void Tick_ConsumesStockProportionalToPopulation()
        {
            var community = CreateCommunity(population: 2, startingStock: 100f);
            var config = BalancingConfig.CreateForTesting(villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 24f);
            var service = new VillageEconomySimulationService(config);

            var result = service.Tick(community, FoodResourceId, TimeSpan.FromHours(5));

            // 2 NPCs * 1 unit/hour * 5 hours = 10 consumed.
            Assert.AreEqual(90f, result.UpdatedEssentialResourceStock, 0.001f);
        }

        [Test]
        public void Tick_ConsumptionNeverMakesStockNegative()
        {
            var community = CreateCommunity(population: 3, startingStock: 2f);
            var config = BalancingConfig.CreateForTesting();
            var service = new VillageEconomySimulationService(config);

            var result = service.Tick(community, FoodResourceId, TimeSpan.FromHours(10));

            Assert.AreEqual(0f, result.UpdatedEssentialResourceStock, 0.001f);
        }

        [Test]
        public void Tick_BelowThresholdBeyondTolerancePeriod_ReducesPopulation()
        {
            var community = CreateCommunity(population: 2, startingStock: 1f);
            var config = BalancingConfig.CreateForTesting(villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 5f);
            var service = new VillageEconomySimulationService(config);

            service.Tick(community, FoodResourceId, TimeSpan.FromHours(6)); // stays below threshold for 6h > 5h tolerance

            Assert.AreEqual(1, community.PopulationNpcIds.Count);
        }

        [Test]
        public void Tick_BelowThresholdWithinTolerancePeriod_DoesNotReducePopulationYet()
        {
            var community = CreateCommunity(population: 2, startingStock: 1f);
            var config = BalancingConfig.CreateForTesting(villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 24f);
            var service = new VillageEconomySimulationService(config);

            service.Tick(community, FoodResourceId, TimeSpan.FromHours(2));

            Assert.AreEqual(2, community.PopulationNpcIds.Count);
        }
    }
}
