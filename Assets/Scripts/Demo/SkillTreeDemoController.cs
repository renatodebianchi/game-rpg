using System.Collections.Generic;
using System.Linq;
using GameRpg.Characters;
using GameRpg.Skills;
using GameRpg.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// Playable, visual harness for manually testing User Story 2 (skill tree,
    /// including respec) in the Editor. Lays nodes out in three columns by
    /// track (Combatant / Hybrid / Arcanist), stacked by prerequisite depth,
    /// and lets you click to invest or respec. Built entirely at runtime, same
    /// spirit as CombatDemoController — no hand-authored scene content.
    /// Attach to an empty GameObject in Assets/Scenes/SkillTreeDemo.unity, and
    /// assign the seeded skill node assets to <see cref="allNodes"/>.
    /// </summary>
    public class SkillTreeDemoController : MonoBehaviour
    {
        [SerializeField] private List<SkillNodeDefinition> allNodes = new List<SkillNodeDefinition>();
        [SerializeField] private int startingSkillPoints = 6;

        private Character _player;
        private SkillTreeService _skillTreeService;
        private readonly Dictionary<SkillNodeDefinition, Button> _buttonsByNode = new Dictionary<SkillNodeDefinition, Button>();
        private readonly Dictionary<SkillNodeDefinition, Text> _labelsByNode = new Dictionary<SkillNodeDefinition, Text>();
        private Text _statusText;

        private void Start()
        {
            EnsureCamera();
            _player = new Character("player", maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes(4, 4, 4, 4));
            _player.GrantSkillPoints(startingSkillPoints);
            _skillTreeService = new SkillTreeService(allNodes);

            BuildUi();
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

        private void BuildUi()
        {
            var canvasGameObject = new GameObject("SkillTreeDemoCanvas");
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

            _statusText = CreateText(canvasGameObject.transform, new Vector2(0.02f, 0.92f), new Vector2(0.98f, 0.99f));
            _statusText.alignment = TextAnchor.UpperLeft;

            CreateColumnHeader(canvasGameObject.transform, "Combatente", 0.05f, new Color(0.85f, 0.4f, 0.3f));
            CreateColumnHeader(canvasGameObject.transform, "Híbrido", 0.4f, new Color(0.8f, 0.75f, 0.2f));
            CreateColumnHeader(canvasGameObject.transform, "Arcanista", 0.7f, new Color(0.4f, 0.5f, 0.9f));

            var depths = ComputeDepths(allNodes);
            var columnX = new Dictionary<SkillTrack, float>
            {
                [SkillTrack.Combatant] = 0.05f,
                [SkillTrack.Hybrid] = 0.4f,
                [SkillTrack.Arcanist] = 0.7f,
            };

            foreach (var node in allNodes)
            {
                var x = columnX[node.Track];
                var y = 0.82f - depths[node] * 0.14f;
                CreateNodeButton(canvasGameObject.transform, node, x, y);
            }

            var resetButton = CreateSimpleButton(canvasGameObject.transform, "Resetar Tudo (Respec)", new Vector2(0.4f, 0.03f));
            resetButton.onClick.AddListener(OnResetAllClicked);
        }

        private void CreateColumnHeader(Transform parent, string label, float x, Color color)
        {
            var text = CreateText(parent, new Vector2(x, 0.86f), new Vector2(x + 0.25f, 0.91f));
            text.text = label;
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
        }

        private static Dictionary<SkillNodeDefinition, int> ComputeDepths(IReadOnlyList<SkillNodeDefinition> nodes)
        {
            var depths = new Dictionary<SkillNodeDefinition, int>();

            int GetDepth(SkillNodeDefinition node)
            {
                if (depths.TryGetValue(node, out var cached))
                {
                    return cached;
                }

                var depth = node.Prerequisites.Count == 0
                    ? 0
                    : node.Prerequisites.Max(GetDepth) + 1;

                depths[node] = depth;
                return depth;
            }

            foreach (var node in nodes)
            {
                GetDepth(node);
            }

            return depths;
        }

        // Rendering delegates to the shared DemoUiKit (FR-007) instead of keeping a
        // local copy of these methods — see research.md, "Extrair os componentes de
        // UI duplicados".
        private void CreateNodeButton(Transform parent, SkillNodeDefinition node, float x, float y)
        {
            var button = DemoUiKit.CreateButton(
                parent, string.Empty, new Vector2(x, y), new Vector2(0.25f, 0.11f),
                () => OnNodeClicked(node), fontSize: 13);
            button.gameObject.name = $"Node_{node.NodeId}";

            var label = button.GetComponentInChildren<Text>();
            _buttonsByNode[node] = button;
            _labelsByNode[node] = label;
        }

        private Button CreateSimpleButton(Transform parent, string label, Vector2 anchorMin) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.2f, 0.06f), null, fontSize: 14);

        private Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax) =>
            DemoUiKit.CreateText(parent, anchorMin, anchorMax);

        private void OnNodeClicked(SkillNodeDefinition node)
        {
            var acquired = _player.AcquiredSkillNodeIds.Contains(node.NodeId);

            if (acquired)
            {
                _skillTreeService.Respec(_player, node);
            }
            else if (_skillTreeService.IsAvailableForInvestment(_player, node))
            {
                _skillTreeService.AcquireNode(_player, node);
            }

            Refresh();
        }

        private void OnResetAllClicked()
        {
            foreach (var nodeId in _player.AcquiredSkillNodeIds.ToList())
            {
                var node = allNodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node != null && _player.AcquiredSkillNodeIds.Contains(nodeId))
                {
                    _skillTreeService.Respec(_player, node);
                }
            }

            Refresh();
        }

        private void Refresh()
        {
            foreach (var node in allNodes)
            {
                var acquired = _player.AcquiredSkillNodeIds.Contains(node.NodeId);
                var available = _skillTreeService.IsAvailableForInvestment(_player, node);
                var button = _buttonsByNode[node];
                var label = _labelsByNode[node];

                button.image.color = acquired
                    ? new Color(0.2f, 0.7f, 0.2f)
                    : available
                        ? new Color(0.3f, 0.3f, 0.35f)
                        : new Color(0.15f, 0.15f, 0.15f);
                button.interactable = acquired || available;

                label.text = acquired
                    ? $"{node.DisplayName}\n(adquirido, custo {node.Cost})"
                    : available
                        ? $"{node.DisplayName}\n(custo {node.Cost})"
                        : $"{node.DisplayName}\n(bloqueado)";
            }

            if (_statusText != null)
            {
                var acquiredNames = _player.AcquiredSkillNodeIds.Count > 0
                    ? string.Join(", ", _player.AcquiredSkillNodeIds)
                    : "nenhum";
                _statusText.text =
                    $"Pontos de habilidade disponíveis: {_player.AvailableSkillPoints}\n" +
                    $"Adquiridos: {acquiredNames}";
            }
        }
    }
}
