using System;
using System.Collections.Generic;
using System.Linq;
using GameRpg.Core;
using GameRpg.NPCs;
using GameRpg.UI;
using GameRpg.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// Playable, visual harness for manually testing User Story 4 (reputation
    /// and world reactivity: village economy, population, permanent collapse)
    /// in the Editor. Two villages, side by side, with buttons to advance time,
    /// save/abandon an NPC, and transport food between them — enough to watch
    /// FR-014/FR-015/FR-019/FR-020 play out live. Built entirely at runtime,
    /// same spirit as the other demo controllers.
    /// Attach to an empty GameObject in Assets/Scenes/ReputationEconomyDemo.unity,
    /// and assign the seeded CommunityDefinition/NpcDefinition/ResourceDefinition
    /// assets to the serialized fields.
    /// </summary>
    public class ReputationEconomyDemoController : MonoBehaviour
    {
        private const string FoodResourceId = "food";
        private const float TransportAmount = 20f;
        private const float ConsumeAmount = 20f;
        private const int HoursPerTick = 2;

        [SerializeField] private List<CommunityDefinition> communityDefinitions = new List<CommunityDefinition>();
        [SerializeField] private List<NpcDefinition> npcDefinitions = new List<NpcDefinition>();

        private WorldClock _worldClock;
        private BalancingConfig _config;
        private VillageEconomySimulationService _simulationService;
        private ImpactfulChoiceLog _choiceLog;
        private ReputationService _reputationService;
        private PlayerChoiceActions _playerChoiceActions;

        private readonly Dictionary<string, Community> _communitiesById = new Dictionary<string, Community>();
        private readonly Dictionary<string, string> _displayNamesById = new Dictionary<string, string>();
        private readonly Dictionary<string, Text> _panelTextByCommunity = new Dictionary<string, Text>();
        private Text _logText;

        private void Start()
        {
            EnsureCamera();
            BuildWorldState();
            BuildUi();
            Log("Demo de reputação/economia iniciada.");
            Refresh();
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null)
            {
                return;
            }

            var cameraGameObject = new GameObject("DemoCamera");
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
            cameraGameObject.tag = "MainCamera";
        }

        private void BuildWorldState()
        {
            _worldClock = new WorldClock();

            // Tuned for a snappy demo: a village left below the sustain
            // threshold for just a few simulated hours starts losing
            // population, instead of needing a full day (BalancingConfig's
            // shipped default) per contracts/village-economy-simulation-contract.md.
            _config = BalancingConfig.CreateForTesting(
                villageSustainThresholdStock: 10f, villageSustainTolerancePeriodHours: 3f);

            _simulationService = new VillageEconomySimulationService(_config);
            _choiceLog = new ImpactfulChoiceLog();
            _playerChoiceActions = new PlayerChoiceActions(_choiceLog, _worldClock);

            var npcsByCommunity = npcDefinitions
                .Where(npc => npc != null)
                .GroupBy(npc => npc.CommunityId)
                .ToDictionary(group => group.Key, group => group.Select(npc => npc.NpcId).ToList());

            foreach (var definition in communityDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                var npcIds = npcsByCommunity.TryGetValue(definition.CommunityId, out var ids) ? ids : new List<string>();
                var community = new Community(definition.CommunityId, npcIds);
                community.AddResourceStock(FoodResourceId, definition.StartingEssentialResourceStock);

                _communitiesById[definition.CommunityId] = community;
                _displayNamesById[definition.CommunityId] = definition.DisplayName;
            }

            _reputationService = new ReputationService(_communitiesById.Values, _choiceLog);
        }

        private void BuildUi()
        {
            var canvasGameObject = new GameObject("ReputationEconomyDemoCanvas");
            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystemGameObject = new GameObject("EventSystem");
                eventSystemGameObject.AddComponent<EventSystem>();
                eventSystemGameObject.AddComponent<StandaloneInputModule>();
            }

            var communityIds = _communitiesById.Keys.ToList();
            var panelWidth = 0.44f;

            for (var i = 0; i < communityIds.Count && i < 2; i++)
            {
                var communityId = communityIds[i];
                var x = 0.03f + i * (panelWidth + 0.04f);
                BuildVillagePanel(canvasGameObject.transform, communityId, x, panelWidth);
            }

            CreateActionButton(canvasGameObject.transform, $"Avançar {HoursPerTick}h", new Vector2(0.03f, 0.28f), () =>
            {
                _worldClock.Advance(TimeSpan.FromHours(HoursPerTick));
                var deaths = new List<string>();
                foreach (var community in _communitiesById.Values)
                {
                    var result = _simulationService.Tick(community, FoodResourceId, TimeSpan.FromHours(HoursPerTick));
                    deaths.AddRange(result.NpcsTransitionedToDead);
                }

                Log(deaths.Count > 0
                    ? $"Tempo avançou {HoursPerTick}h. NPCs mortos de fome: {string.Join(", ", deaths)}."
                    : $"Tempo avançou {HoursPerTick}h. Nenhuma morte por fome.");
                Refresh();
            });

            if (communityIds.Count >= 2)
            {
                var idA = communityIds[0];
                var idB = communityIds[1];

                CreateActionButton(canvasGameObject.transform, $"Transportar comida:\n{_displayNamesById[idA]} -> {_displayNamesById[idB]}",
                    new Vector2(0.26f, 0.28f), () => TransportFood(idA, idB));

                CreateActionButton(canvasGameObject.transform, $"Transportar comida:\n{_displayNamesById[idB]} -> {_displayNamesById[idA]}",
                    new Vector2(0.52f, 0.28f), () => TransportFood(idB, idA));
            }

            _logText = CreateText(canvasGameObject.transform, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.2f));
            _logText.alignment = TextAnchor.UpperLeft;
            _logText.fontSize = 14;
        }

        private void TransportFood(string sourceCommunityId, string destinationCommunityId)
        {
            var source = _communitiesById[sourceCommunityId];
            var destination = _communitiesById[destinationCommunityId];

            var consumed = source.ConsumeResourceStock(FoodResourceId, TransportAmount);
            if (consumed <= 0f)
            {
                Log($"{_displayNamesById[sourceCommunityId]} não tem comida para transportar.");
                return;
            }

            _playerChoiceActions.TransportResource(destination, FoodResourceId, Mathf.RoundToInt(consumed));
            Log($"Transportadas {consumed:0} unidades de comida de {_displayNamesById[sourceCommunityId]} para {_displayNamesById[destinationCommunityId]}.");
            Refresh();
        }

        private void BuildVillagePanel(Transform parent, string communityId, float x, float width)
        {
            var panelImage = DemoUiKit.CreatePanel(parent, new Vector2(x, 0.35f), new Vector2(x + width, 0.97f));
            var panelGameObject = panelImage.gameObject;
            panelGameObject.name = $"Panel_{communityId}";

            var title = CreateText(panelGameObject.transform, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.97f));
            title.text = _displayNamesById[communityId];
            title.fontStyle = FontStyle.Bold;
            title.fontSize = 18;
            title.color = Color.black;

            var infoText = CreateText(panelGameObject.transform, new Vector2(0.05f, 0.35f), new Vector2(0.95f, 0.87f));
            infoText.alignment = TextAnchor.UpperLeft;
            infoText.fontSize = 14;
            infoText.color = Color.black;
            _panelTextByCommunity[communityId] = infoText;

            CreateActionButton(panelGameObject.transform, "Salvar NPC", new Vector2(0.05f, 0.18f), () => SaveFirstNpc(communityId));
            CreateActionButton(panelGameObject.transform, "Abandonar NPC", new Vector2(0.52f, 0.18f), () => AbandonFirstNpc(communityId));
        }

        private void SaveFirstNpc(string communityId)
        {
            var community = _communitiesById[communityId];
            if (community.PopulationNpcIds.Count == 0)
            {
                Log($"{_displayNamesById[communityId]} não tem mais NPCs.");
                return;
            }

            var npcId = community.PopulationNpcIds[0];
            _playerChoiceActions.SaveNpc(npcId, communityId);
            Log($"Você salvou {npcId} em {_displayNamesById[communityId]}. Reputação aumentou.");
            Refresh();
        }

        private void AbandonFirstNpc(string communityId)
        {
            var community = _communitiesById[communityId];
            if (community.PopulationNpcIds.Count == 0)
            {
                Log($"{_displayNamesById[communityId]} não tem mais NPCs.");
                return;
            }

            var npcId = community.PopulationNpcIds[0];
            _playerChoiceActions.AbandonOrHarmNpc(npcId, communityId);
            Log($"Você não salvou {npcId} em {_displayNamesById[communityId]}. Reputação diminuiu.");
            Refresh();
        }

        // Rendering delegates to the shared DemoUiKit (FR-007) instead of keeping a
        // local copy of these methods — see research.md, "Extrair os componentes de
        // UI duplicados".
        private void CreateActionButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.22f, 0.08f), onClick, fontSize: 12);

        private Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax) =>
            DemoUiKit.CreateText(parent, anchorMin, anchorMax);

        private void Log(string message)
        {
            if (_logText != null)
            {
                _logText.text = $"[{_worldClock.ElapsedSimulatedTime.TotalHours:0}h] {message}";
            }
        }

        private void Refresh()
        {
            foreach (var (communityId, community) in _communitiesById.Select(kv => (kv.Key, kv.Value)))
            {
                var economyState = community.GetResourceStock(FoodResourceId) < _config.VillageSustainThresholdStock
                    ? "Em dificuldade"
                    : "Estável";

                var text = _panelTextByCommunity[communityId];
                text.text =
                    $"Reputação: {community.ReputationWithPlayer}\n" +
                    $"População: {community.PopulationNpcIds.Count}\n" +
                    $"Estoque de comida: {community.GetResourceStock(FoodResourceId):0}\n" +
                    $"Economia: {(community.IsPermanentlyInactive ? "COLAPSADA (permanente)" : economyState)}";

                text.color = community.IsPermanentlyInactive ? new Color(0.7f, 0.1f, 0.1f) : Color.black;
            }
        }
    }
}
