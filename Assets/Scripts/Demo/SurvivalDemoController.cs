using System;
using GameRpg.Characters;
using GameRpg.Core;
using GameRpg.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// Playable, visual harness for manually testing User Story 3 (hunger and
    /// sanity, including their cumulative combat penalty per FR-021) in the
    /// Editor. Built entirely at runtime, same spirit as CombatDemoController.
    /// Attach to an empty GameObject in Assets/Scenes/SurvivalDemo.unity.
    /// </summary>
    public class SurvivalDemoController : MonoBehaviour
    {
        [SerializeField] private int previewBaseDamage = 10;
        [SerializeField] private float foodHungerRestorePerUnit = 25f;

        private Character _player;
        private BalancingConfig _config;
        private WorldClock _worldClock;
        private HungerSystem _hungerSystem;
        private SanitySystem _sanitySystem;
        private FoodConsumptionAction _foodConsumptionAction;
        private SanityRecoveryAction _sanityRecoveryAction;

        private Slider _hungerBar;
        private Slider _sanityBar;
        private Text _statusText;

        private void Start()
        {
            EnsureCamera();

            _player = new Character("player", maxHitPoints: 20, maxTechPoints: 3, new CharacterAttributes(4, 4, 4, 4));
            _player.Inventory.Add("food", 5);

            _config = BalancingConfig.CreateForTesting();
            _worldClock = new WorldClock();
            _hungerSystem = new HungerSystem(_player, _config, _worldClock);
            _sanitySystem = new SanitySystem(_player, _config);
            _foodConsumptionAction = new FoodConsumptionAction(_player, _hungerSystem);
            _sanityRecoveryAction = new SanityRecoveryAction(_sanitySystem);

            BuildUi();
            Refresh("Demo de sobrevivência iniciada.");
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
            var canvasGameObject = new GameObject("SurvivalDemoCanvas");
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

            CreateLabel(canvasGameObject.transform, "Fome", new Vector2(0.05f, 0.85f));
            _hungerBar = CreateBar(canvasGameObject.transform, new Vector2(0.05f, 0.78f), new Color(0.8f, 0.55f, 0.2f));

            CreateLabel(canvasGameObject.transform, "Sanidade", new Vector2(0.05f, 0.68f));
            _sanityBar = CreateBar(canvasGameObject.transform, new Vector2(0.05f, 0.61f), new Color(0.4f, 0.5f, 0.9f));

            _statusText = CreateText(canvasGameObject.transform, new Vector2(0.05f, 0.2f), new Vector2(0.6f, 0.58f));
            _statusText.alignment = TextAnchor.UpperLeft;
            _statusText.fontSize = 16;

            CreateActionButton(canvasGameObject.transform, "Avançar 1h", new Vector2(0.05f, 0.05f), () =>
            {
                _worldClock.Advance(TimeSpan.FromHours(1));
                Refresh("O tempo avançou 1 hora.");
            });

            CreateActionButton(canvasGameObject.transform, "Comer (1 alimento)", new Vector2(0.24f, 0.05f), () =>
            {
                if (_player.Inventory.GetQuantity("food") <= 0)
                {
                    Refresh("Sem alimento no inventário.");
                    return;
                }

                _foodConsumptionAction.Consume("food", 1, foodHungerRestorePerUnit);
                Refresh("Você comeu 1 unidade de alimento.");
            });

            CreateActionButton(canvasGameObject.transform, "Evento Perturbador", new Vector2(0.46f, 0.05f), () =>
            {
                _sanitySystem.ApplyDisturbingEvent(25f);
                Refresh("Um evento perturbador reduziu sua sanidade.");
            });

            CreateActionButton(canvasGameObject.transform, "Descansar", new Vector2(0.68f, 0.05f), () =>
            {
                _sanityRecoveryAction.Recover(SanityRecoveryMethod.Rest, 25f);
                Refresh("Você descansou e recuperou sanidade.");
            });
        }

        private void CreateActionButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.18f, 0.06f), onClick, fontSize: 13);

        private void CreateLabel(Transform parent, string label, Vector2 anchorMin)
        {
            var text = CreateText(parent, anchorMin, anchorMin + new Vector2(0.3f, 0.05f));
            text.text = label;
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
        }

        private Slider CreateBar(Transform parent, Vector2 anchorMin, Color fillColor)
        {
            var sliderGameObject = new GameObject("Bar");
            sliderGameObject.transform.SetParent(parent, worldPositionStays: false);
            var slider = sliderGameObject.AddComponent<Slider>();
            var rect = sliderGameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMin + new Vector2(0.4f, 0.05f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var background = new GameObject("Background");
            background.transform.SetParent(sliderGameObject.transform, false);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.15f, 0.15f, 0.15f);
            var backgroundRect = backgroundImage.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGameObject.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            var fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.interactable = false;

            return slider;
        }

        private Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax) =>
            DemoUiKit.CreateText(parent, anchorMin, anchorMax);

        private void Refresh(string message)
        {
            _hungerBar.value = _player.Hunger;
            _sanityBar.value = _player.Sanity;

            var afterHunger = _hungerSystem.ModifyOutgoingDamage(_player, previewBaseDamage);
            var afterBoth = _sanitySystem.ModifyOutgoingDamage(_player, afterHunger);

            _statusText.text =
                $"Fome: {_player.Hunger:0.0}/100 ({_hungerSystem.CurrentLevel})\n" +
                $"Sanidade: {_player.Sanity:0.0}/100 ({_sanitySystem.CurrentLevel})\n" +
                $"Alimento no inventário: {_player.Inventory.GetQuantity("food")}\n" +
                $"Tempo simulado: {_worldClock.ElapsedSimulatedTime.TotalHours:0.0}h\n\n" +
                $"Prévia de dano de ataque:\n" +
                $"  Base {previewBaseDamage} -> após fome: {afterHunger} -> após fome+sanidade: {afterBoth}\n" +
                $"  (FR-021: penalidades de fome e sanidade se acumulam)\n\n" +
                message;
        }
    }
}
