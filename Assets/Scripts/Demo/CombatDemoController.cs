using System.Collections.Generic;
using GameRpg.Characters;
using GameRpg.Combat;
using GameRpg.Combat.Grid;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// Playable, visual harness for manually testing User Story 1 (combat) in
    /// the Editor: draws the grid, spawns a player and an enemy as simple
    /// primitives, and exposes Attack/End Turn/Flee buttons plus click-to-move.
    /// Not part of the shipped game architecture — this is a manual-test tool,
    /// built entirely at runtime so it needs no hand-authored scene content.
    /// Attach to an empty GameObject in Assets/Scenes/CombatEncounterTest.unity.
    /// </summary>
    public class CombatDemoController : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private int playerMaxHitPoints = 20;
        [SerializeField] private int enemyMaxHitPoints = 15;
        [SerializeField] private int baseAttackDamage = 5;
        [SerializeField] private int enemyAttackDamage = 4;

        private GridMap _gridMap;
        private GridPathfinding _pathfinding;
        private Character _player;
        private NonPlayerCombatant _enemy;
        private CombatEncounter _encounter;
        private TurnResourceManager _turnResourceManager;
        private ActionResolver _actionResolver;
        private FleeAction _fleeAction;
        private EnemyAI _enemyAI;
        private CombatOutcomeHandler _outcomeHandler;

        private readonly Dictionary<GridCoordinate, GameObject> _tileVisuals = new Dictionary<GridCoordinate, GameObject>();
        private GameObject _playerVisual;
        private GameObject _enemyVisual;
        private HealthBarWidget _playerHealthBar;
        private HealthBarWidget _enemyHealthBar;
        private Text _statusText;
        private Button _attackButton;
        private Button _endTurnButton;
        private Button _fleeButton;
        private bool _combatOver;

        private void Start()
        {
            EnsureCamera();
            BuildGrid();
            FrameCameraOnGrid();
            SpawnCombatants();
            BuildEncounter();
            BuildUi();
            Refresh("Combate iniciado. Clique em um tile para mover, ou use os botões.");
        }

        /// <summary>
        /// Repositions Camera.main so the whole grid is actually in frame,
        /// regardless of whether it's a freshly created camera or one already
        /// placed in the scene (e.g., by ProjectBootstrap) at unrelated coordinates.
        /// </summary>
        private void FrameCameraOnGrid()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var center = new Vector3((gridWidth - 1) / 2f, 0f, (gridHeight - 1) / 2f);
            var distance = Mathf.Max(gridWidth, gridHeight) * 2f;
            camera.transform.position = center - camera.transform.rotation * Vector3.forward * distance;
            camera.orthographicSize = Mathf.Max(gridWidth, gridHeight) * 0.75f;
        }

        private static void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGameObject = new GameObject("DemoCamera");
                camera = cameraGameObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                cameraGameObject.tag = "MainCamera";
                cameraGameObject.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
                cameraGameObject.transform.position = new Vector3(6f, 8f, -6f);
            }

            if (camera.GetComponent<DemoCameraController>() == null)
            {
                camera.gameObject.AddComponent<DemoCameraController>();
            }
        }

        private void BuildGrid()
        {
            _gridMap = new GridMap(gridWidth, gridHeight);
            _pathfinding = new GridPathfinding(_gridMap);

            var tileMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            for (var x = 0; x < gridWidth; x++)
            {
                for (var y = 0; y < gridHeight; y++)
                {
                    var coordinate = new GridCoordinate(x, y);
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    tile.name = $"Tile_{x}_{y}";
                    tile.transform.SetParent(transform);
                    tile.transform.position = new Vector3(x, 0f, y);
                    tile.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    tile.transform.localScale = Vector3.one * 0.92f;
                    tile.GetComponent<Renderer>().material = new Material(tileMaterial) { color = new Color(0.3f, 0.5f, 0.3f) };
                    _tileVisuals[coordinate] = tile;

                    var clickHandler = tile.AddComponent<GridTileClickHandler>();
                    clickHandler.Initialize(this, coordinate);
                }
            }
        }

        private void SpawnCombatants()
        {
            _player = new Character("player", playerMaxHitPoints, maxMovementPoints: 3, new CharacterAttributes(5, 5, 3, 3))
            {
                Position = new GridCoordinate(0, 0),
            };
            _enemy = new NonPlayerCombatant("enemy", enemyMaxHitPoints, maxMovementPoints: 3)
            {
                Position = new GridCoordinate(gridWidth - 1, gridHeight - 1),
            };
            _gridMap.PlaceOccupant(_player.Position, _player.CombatantId);
            _gridMap.PlaceOccupant(_enemy.Position, _enemy.CombatantId);

            _playerVisual = CreateCombatantVisual("Player", Color.blue, PrimitiveType.Capsule);
            _enemyVisual = CreateCombatantVisual("Enemy", Color.red, PrimitiveType.Capsule);

            _playerHealthBar = HealthBarWidget.Create(_playerVisual.transform, new Vector3(0f, 1.1f, 0f));
            _enemyHealthBar = HealthBarWidget.Create(_enemyVisual.transform, new Vector3(0f, 1.1f, 0f));
            _playerHealthBar.SetFraction(1f);
            _enemyHealthBar.SetFraction(1f);
        }

        private GameObject CreateCombatantVisual(string name, Color color, PrimitiveType primitiveType)
        {
            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = name;
            visual.transform.SetParent(transform);
            visual.transform.localScale = Vector3.one * 0.6f;
            visual.GetComponent<Renderer>().material.color = color;
            return visual;
        }

        private void BuildEncounter()
        {
            _encounter = new CombatEncounter(new ICombatant[] { _player }, new ICombatant[] { _enemy });
            var initiativeService = new InitiativeService();
            _encounter.Start(initiativeService.CalculateOrder(new ICombatant[] { _player, _enemy }));

            _turnResourceManager = new TurnResourceManager(_encounter);
            _actionResolver = new ActionResolver(_encounter, _turnResourceManager);
            _fleeAction = new FleeAction(_encounter, _turnResourceManager);
            _enemyAI = new EnemyAI(_encounter, _turnResourceManager, _actionResolver, _pathfinding);
            _outcomeHandler = new CombatOutcomeHandler(_encounter);

            _outcomeHandler.VictoryRewardsGranted += reward =>
            {
                _combatOver = true;
                Refresh($"Vitória! Recompensa: {reward} XP.");
            };
            _outcomeHandler.DefeatCheckpointRestored += () =>
            {
                _combatOver = true;
                Refresh("Derrota. Retornando ao último checkpoint.");
            };
            _outcomeHandler.CombatantHarmed += harmedEvent =>
            {
                var bar = harmedEvent.Combatant == _player ? _playerHealthBar : _enemyHealthBar;
                bar.FlashDamage();
            };
        }

        private void BuildUi()
        {
            var canvasGameObject = new GameObject("DemoCanvas");
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

            _statusText = CreateText(canvasGameObject.transform, new Vector2(0.02f, 0.9f), new Vector2(0.7f, 0.99f));
            _attackButton = CreateButton(canvasGameObject.transform, "Atacar", new Vector2(0.02f, 0.02f), OnAttackClicked);
            _endTurnButton = CreateButton(canvasGameObject.transform, "Terminar Turno", new Vector2(0.18f, 0.02f), OnEndTurnClicked);
            _fleeButton = CreateButton(canvasGameObject.transform, "Fugir", new Vector2(0.38f, 0.02f), OnFleeClicked);

            CreateControlsHelpCard(canvasGameObject.transform);
        }

        /// <summary>
        /// Fixed help card on the right edge of the screen listing every input
        /// this demo responds to — there is no camera control bound to any key
        /// (the camera is static by design here), so this makes that explicit
        /// instead of leaving the player guessing.
        /// </summary>
        private void CreateControlsHelpCard(Transform parent)
        {
            var panelGameObject = new GameObject("ControlsHelpCard");
            panelGameObject.transform.SetParent(parent, worldPositionStays: false);
            var panelImage = panelGameObject.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.6f);

            var rect = panelImage.rectTransform;
            rect.anchorMin = new Vector2(0.78f, 0.55f);
            rect.anchorMax = new Vector2(0.99f, 0.99f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var titleText = CreateText(panelGameObject.transform, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.98f));
            titleText.text = "Comandos";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;

            var bodyText = CreateText(panelGameObject.transform, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.83f));
            bodyText.fontSize = 14;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.text =
                "Clique em um tile\n  -> mover o personagem\n\n" +
                "Botão Atacar\n  -> atacar o inimigo adjacente\n\n" +
                "Botão Terminar Turno\n  -> passa o turno (a IA do\n     inimigo joga em seguida)\n\n" +
                "Botão Fugir\n  -> tenta fugir do combate\n\n" +
                "Câmera:\n" +
                "WASD / setas\n  -> mover a câmera\n" +
                "Scroll do mouse ou Q / E\n  -> zoom\n" +
                "Botão do meio + arrastar\n  -> arrastar a câmera (pan)\n" +
                "Botão direito + arrastar\n  -> girar a câmera (orbit)";
        }

        private Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textGameObject = new GameObject("StatusText");
            textGameObject.transform.SetParent(parent, worldPositionStays: false);
            var text = textGameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            var rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick)
        {
            var buttonGameObject = new GameObject($"Button_{label}");
            buttonGameObject.transform.SetParent(parent, worldPositionStays: false);
            var image = buttonGameObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            var button = buttonGameObject.AddComponent<Button>();
            button.onClick.AddListener(onClick);

            var rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMin + new Vector2(0.15f, 0.06f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var labelText = CreateText(buttonGameObject.transform, Vector2.zero, Vector2.one);
            labelText.text = label;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.fontSize = 16;

            return button;
        }

        private void OnAttackClicked()
        {
            if (_combatOver || _encounter.CurrentActor != _player)
            {
                return;
            }

            if (!_player.TurnResources.ActionAvailable)
            {
                Refresh("Você já usou sua ação neste turno. Clique em 'Terminar Turno'.");
                return;
            }

            if (GridCoordinate.ManhattanDistance(_player.Position, _enemy.Position) > 1)
            {
                Refresh("Inimigo fora de alcance. Aproxime-se primeiro.");
                return;
            }

            _actionResolver.ResolveBasicAttack(_player, _enemy, baseAttackDamage);
            _outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 25);
            Refresh($"Você atacou o inimigo por {baseAttackDamage}.");
        }

        private void OnFleeClicked()
        {
            if (_combatOver || _encounter.CurrentActor != _player)
            {
                return;
            }

            if (!_player.TurnResources.ActionAvailable)
            {
                Refresh("Você já usou sua ação neste turno. Clique em 'Terminar Turno'.");
                return;
            }

            var chance = _fleeAction.CalculateSuccessChance(_player, new ICombatant[] { _enemy }, _player.Attributes.Dexterity);
            var succeeded = _fleeAction.TryFlee(_player, chance);
            Refresh(succeeded ? "Fuga bem-sucedida!" : "Tentativa de fuga falhou.");
            if (succeeded)
            {
                _combatOver = true;
            }
        }

        private void OnEndTurnClicked()
        {
            if (_combatOver || _encounter.CurrentActor != _player)
            {
                return;
            }

            _encounter.AdvanceTurn();
            if (_encounter.CurrentActor == _enemy && !_enemy.IsDefeated)
            {
                _turnResourceManager.ResetForActor(_enemy);
                _enemyAI.TakeTurn(_enemy, enemyAttackDamage);
                _outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 25);

                if (!_combatOver)
                {
                    _encounter.AdvanceTurn();
                    _turnResourceManager.ResetForActor(_player);
                }
            }

            Refresh("Novo turno.");
        }

        internal void TryMovePlayerTo(GridCoordinate destination)
        {
            if (_combatOver || _encounter.CurrentActor != _player)
            {
                return;
            }

            var path = _pathfinding.FindPath(_player.Position, destination);
            if (path == null)
            {
                Refresh("Sem caminho até esse tile.");
                return;
            }

            var cost = _pathfinding.CalculatePathCost(path);
            if (cost > _player.TurnResources.MovementPointsRemaining || cost == 0)
            {
                Refresh("Movimento insuficiente para chegar até lá.");
                return;
            }

            _gridMap.RemoveOccupant(_player.Position);
            _turnResourceManager.ConsumeMovement(_player, cost);
            _player.Position = destination;
            _gridMap.PlaceOccupant(_player.Position, _player.CombatantId);
            Refresh("Personagem moveu-se.");
        }

        private void Refresh(string message)
        {
            _playerVisual.transform.position = new Vector3(_player.Position.X, 0.5f, _player.Position.Y);
            _enemyVisual.transform.position = new Vector3(_enemy.Position.X, 0.5f, _enemy.Position.Y);
            _enemyVisual.SetActive(!_enemy.IsDefeated);
            _playerVisual.SetActive(!_player.IsDefeated);

            _playerHealthBar.SetFraction((float)_player.CurrentHitPoints / _player.MaxHitPoints);
            _enemyHealthBar.SetFraction((float)_enemy.CurrentHitPoints / _enemy.MaxHitPoints);

            var isPlayerTurnWithActionLeft = !_combatOver && _encounter.CurrentActor == _player && _player.TurnResources.ActionAvailable;
            var isPlayerTurn = !_combatOver && _encounter.CurrentActor == _player;
            _attackButton.interactable = isPlayerTurnWithActionLeft;
            _fleeButton.interactable = isPlayerTurnWithActionLeft;
            _endTurnButton.interactable = isPlayerTurn;

            if (_statusText != null)
            {
                var turnLabel = _combatOver ? "Combate encerrado" : (_encounter.CurrentActor == _player ? "Seu turno" : "Turno do inimigo");
                _statusText.text =
                    $"{turnLabel}\n" +
                    $"Jogador HP: {_player.CurrentHitPoints}/{_player.MaxHitPoints}  |  " +
                    $"Inimigo HP: {_enemy.CurrentHitPoints}/{_enemy.MaxHitPoints}\n" +
                    $"Movimento restante: {_player.TurnResources.MovementPointsRemaining}\n" +
                    message;
            }
        }
    }
}
