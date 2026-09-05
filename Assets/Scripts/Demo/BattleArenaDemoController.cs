using System;
using GameRpg.Characters;
using GameRpg.Combat;
using GameRpg.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// Playable, visual harness for manually testing User Story 1 (real-time
    /// combat) in the Editor: draws a 2D side-view arena, spawns the player
    /// and an enemy as sprites, and lets the player move freely, attack, cast
    /// a skill, and flee — all continuously, with no turn structure. Replaces
    /// Demo.CombatDemoController. Built entirely at runtime, no hand-authored
    /// scene content. Attach to an empty GameObject in the combat scene.
    /// </summary>
    public class BattleArenaDemoController : MonoBehaviour
    {
        [SerializeField] private float arenaWidth = 12f;
        [SerializeField] private float moveSpeedPerSecond = 3.5f;
        [SerializeField] private int playerMaxHitPoints = 20;
        [SerializeField] private int enemyMaxHitPoints = 15;
        [SerializeField] private float edgeFleeThreshold = 1.5f;

        private BattleArena _arena;
        private Character _player;
        private NonPlayerCombatant _enemy;
        private CombatArenaEncounter _encounter;
        private RealTimeActionExecutor _executor;
        private EnemyCombatAI _enemyAI;
        private RealTimeFleeAction _fleeAction;
        private CombatOutcomeHandler _outcomeHandler;
        private RealTimeActionDefinition _meleeAttack;
        private RealTimeActionDefinition _rangedAttack;
        private RealTimeActionDefinition _skillAttack;
        private bool _hasRangedCapability;

        private GameObject _playerVisual;
        private GameObject _enemyVisual;
        private HealthBarWidget _playerHealthBar;
        private HealthBarWidget _enemyHealthBar;
        private Text _statusText;
        private Button _rangedButton;
        private Button _skillButton;
        private bool _combatOver;
        private string _lastMessage = string.Empty;

        private void Start()
        {
            EnsureCamera();
            BuildArenaAndCombatants();
            BuildEncounter();
            BuildUi();
            Refresh("Combate iniciado. Mova-se e ataque livremente — não há turnos.");
        }

        private void EnsureCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraGameObject = new GameObject("DemoCamera");
                camera = cameraGameObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.transform.position = new Vector3(arenaWidth / 2f, 1f, -10f);
                cameraGameObject.tag = "MainCamera";
            }

            camera.transform.rotation = Quaternion.identity;

            var boundedCamera = camera.GetComponent<BoundedFollowCamera>();
            if (boundedCamera == null)
            {
                boundedCamera = camera.gameObject.AddComponent<BoundedFollowCamera>();
            }

            boundedCamera.SetWorldBounds(0f, arenaWidth, 0f, 2f);
        }

        private void BuildArenaAndCombatants()
        {
            _arena = new BattleArena(0f, arenaWidth);

            _player = new Character("player", playerMaxHitPoints, maxTechPoints: 10, new CharacterAttributes(5, 5, 3, 3))
            {
                PositionX = 1f,
            };
            _enemy = new NonPlayerCombatant("enemy", enemyMaxHitPoints, maxTechPoints: 5)
            {
                PositionX = arenaWidth - 1f,
            };

            _playerVisual = CreateCombatantVisual("Player", Color.blue);
            _enemyVisual = CreateCombatantVisual("Enemy", Color.red);

            _playerHealthBar = HealthBarWidget.Create(_playerVisual.transform, new Vector3(0f, 0.7f, 0f));
            _enemyHealthBar = HealthBarWidget.Create(_enemyVisual.transform, new Vector3(0f, 0.7f, 0f));
            _playerHealthBar.SetFraction(1f);
            _enemyHealthBar.SetFraction(1f);

            var boundedCamera = Camera.main.GetComponent<BoundedFollowCamera>();
            boundedCamera.SetTarget(_playerVisual.transform);
        }

        private static GameObject CreateCombatantVisual(string name, Color color)
        {
            var visual = new GameObject(name);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSolidSprite();
            renderer.color = color;
            visual.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            return visual;
        }

        private static Sprite _solidSprite;

        /// <summary>A minimal 1x1 white sprite tinted per combatant — no art
        /// asset dependency for this MVP demo placeholder (real character art
        /// is used in Exploration, feature 003).</summary>
        private static Sprite CreateSolidSprite()
        {
            if (_solidSprite != null)
            {
                return _solidSprite;
            }

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _solidSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _solidSprite;
        }

        private void BuildEncounter()
        {
            _encounter = new CombatArenaEncounter(new IRealTimeCombatant[] { _player }, new IRealTimeCombatant[] { _enemy });
            _encounter.Start();

            _executor = new RealTimeActionExecutor(_encounter);
            _meleeAttack = RealTimeActionDefinition.CreateForTesting("melee_basic", RealTimeActionKind.Melee, range: 1.5f, executionTime: 0f, cooldown: 0.6f, baseDamage: 5);
            _rangedAttack = RealTimeActionDefinition.CreateForTesting("ranged_basic", RealTimeActionKind.Ranged, range: 5f, executionTime: 0.2f, cooldown: 1f, baseDamage: 4, requiredCapabilityId: "capability.ranged_attack");
            _skillAttack = RealTimeActionDefinition.CreateForTesting("skill_power_strike", RealTimeActionKind.Skill, range: 1.5f, executionTime: 1.2f, cooldown: 2f, resourceCost: 4f, baseDamage: 12);

            _hasRangedCapability = false; // FR-004: only true once the player has acquired the matching skill node.

            _enemyAI = new EnemyCombatAI(_encounter, _executor, _arena, _meleeAttack);
            _fleeAction = new RealTimeFleeAction(_encounter);
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
            var canvasGameObject = new GameObject("BattleArenaDemoCanvas");
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

            _statusText = DemoUiKit.CreateText(canvasGameObject.transform, new Vector2(0.02f, 0.9f), new Vector2(0.7f, 0.99f));
            _statusText.alignment = TextAnchor.UpperLeft;
            _statusText.fontSize = 16;

            DemoUiKit.CreateButton(canvasGameObject.transform, "Ataque Corpo a Corpo", new Vector2(0.02f, 0.02f), new Vector2(0.2f, 0.08f), OnMeleeClicked);
            _rangedButton = DemoUiKit.CreateButton(canvasGameObject.transform, "Ataque à Distância", new Vector2(0.24f, 0.02f), new Vector2(0.2f, 0.08f), OnRangedClicked);
            _skillButton = DemoUiKit.CreateButton(canvasGameObject.transform, "Golpe Poderoso", new Vector2(0.46f, 0.02f), new Vector2(0.2f, 0.08f), OnSkillClicked);

            CreateControlsHelpCard(canvasGameObject.transform);
        }

        private void CreateControlsHelpCard(Transform parent)
        {
            var panelImage = DemoUiKit.CreatePanel(parent, new Vector2(0.78f, 0.55f), new Vector2(0.99f, 0.99f));
            panelImage.gameObject.name = "ControlsHelpCard";

            var titleText = DemoUiKit.CreateText(panelImage.transform, new Vector2(0.05f, 0.85f), new Vector2(0.95f, 0.98f));
            titleText.text = "Comandos";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.black;

            var bodyText = DemoUiKit.CreateText(panelImage.transform, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.83f));
            bodyText.fontSize = 14;
            bodyText.alignment = TextAnchor.UpperLeft;
            bodyText.color = Color.black;
            bodyText.text =
                "A / D ou setas\n  -> mover livremente\n\n" +
                "Botões de ataque\n  -> corpo a corpo / à distância /\n     habilidade (tempo de conjuração)\n\n" +
                "Segurar F perto de uma borda\n  -> tentar fugir\n\n" +
                "Inimigos agem por conta própria,\n" +
                "continuamente — não há turnos.";
        }

        private void OnMeleeClicked()
        {
            if (_combatOver) return;
            if (!_executor.TryStartAction(_player, _meleeAttack, _enemy))
            {
                Refresh("Ataque corpo a corpo indisponível (recarga ou já conjurando).");
            }
        }

        private void OnRangedClicked()
        {
            if (_combatOver) return;
            if (!_executor.TryStartAction(_player, _rangedAttack, _enemy, _hasRangedCapability))
            {
                Refresh(_hasRangedCapability
                    ? "Ataque à distância indisponível (recarga ou já conjurando)."
                    : "Você ainda não adquiriu a capacidade de ataque à distância.");
            }
        }

        private void OnSkillClicked()
        {
            if (_combatOver) return;
            if (!_executor.TryStartAction(_player, _skillAttack, _enemy))
            {
                Refresh("Habilidade indisponível (recarga, Pontos de Técnica insuficientes, ou já conjurando).");
            }
        }

        private void Update()
        {
            if (_combatOver)
            {
                return;
            }

            var deltaTime = TimeSpan.FromSeconds(Time.deltaTime);

            HandleMovement();
            HandleFlee(deltaTime);

            _encounter.AdvanceTime(deltaTime);
            _executor.ResolvePendingActions(_encounter.Participants);
            _enemyAI.Tick(_enemy, deltaTime);
            _outcomeHandler.HandleStateIfTerminal(experienceRewardOnVictory: 25);

            Refresh(string.Empty);
        }

        private void HandleMovement()
        {
            if (_player.ActionState.HasPendingAction)
            {
                return; // mid-cast: FR-002 still allows movement in most action games, but keeping the
                        // caster still while casting makes the interruption risk (FR-009) meaningful.
            }

            var horizontal = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;

            if (horizontal == 0f)
            {
                return;
            }

            var proposed = _player.PositionX + horizontal * moveSpeedPerSecond * Time.deltaTime;
            _player.PositionX = _arena.Clamp(proposed);
        }

        private void HandleFlee(TimeSpan deltaTime)
        {
            var nearEdge = _player.PositionX <= _arena.MinX + edgeFleeThreshold || _player.PositionX >= _arena.MaxX - edgeFleeThreshold;
            var isAttemptingToFlee = nearEdge && Input.GetKey(KeyCode.F);

            var attemptResolved = _fleeAction.AdvanceChannel(_player, new IRealTimeCombatant[] { _enemy }, _player.Attributes.Dexterity, deltaTime, isAttemptingToFlee);
            if (attemptResolved)
            {
                _combatOver = _encounter.State == CombatEncounterState.PlayerFled;
                Refresh(_combatOver ? "Fuga bem-sucedida!" : "Tentativa de fuga falhou.");
            }
        }

        private void Refresh(string message)
        {
            _playerVisual.transform.position = new Vector3(_player.PositionX, 0.5f, 0f);
            _enemyVisual.transform.position = new Vector3(_enemy.PositionX, 0.5f, 0f);
            _enemyVisual.SetActive(!_enemy.IsDefeated);
            _playerVisual.SetActive(!_player.IsDefeated);

            _playerHealthBar.SetFraction((float)_player.CurrentHitPoints / _player.MaxHitPoints);
            _enemyHealthBar.SetFraction((float)_enemy.CurrentHitPoints / _enemy.MaxHitPoints);

            _rangedButton.interactable = !_combatOver;
            _skillButton.interactable = !_combatOver;

            if (!string.IsNullOrEmpty(message))
            {
                _lastMessage = message;
            }

            if (_statusText != null)
            {
                var turnLabel = _combatOver ? "Combate encerrado" : "Combate em andamento (tempo real)";
                _statusText.text =
                    $"{turnLabel}\n" +
                    $"Jogador HP: {_player.CurrentHitPoints}/{_player.MaxHitPoints}  |  " +
                    $"Inimigo HP: {_enemy.CurrentHitPoints}/{_enemy.MaxHitPoints}\n" +
                    $"Pontos de Técnica: {_player.ActionState.CurrentTechPoints:0.0}/{_player.ActionState.MaxTechPoints:0.0}\n\n" +
                    _lastMessage;
            }
        }
    }
}
