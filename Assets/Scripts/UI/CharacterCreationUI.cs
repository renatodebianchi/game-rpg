using System.Collections.Generic;
using GameRpg.Characters;
using GameRpg.Core;
using GameRpg.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameRpg.UI
{
    /// <summary>
    /// Three-step character creation screen (attributes, orientation,
    /// appearance) driving a CharacterCreationProfile to Finalize() a live
    /// Character (FR-001, FR-008, FR-009). Built entirely at runtime, the same
    /// pattern already validated by the feature 001 demo controllers.
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        private enum Step
        {
            Attributes,
            Orientation,
            Appearance,
            Summary
        }

        [SerializeField] private List<EquipmentKitDefinition> equipmentKits = new List<EquipmentKitDefinition>();
        [SerializeField] private string explorationSceneName = "Exploration";

        private Character _character;
        private CharacterCreationProfile _profile;
        private Step _currentStep = Step.Attributes;

        private Transform _stepContainer;
        private Text _statusText;
        private Button _backButton;
        private Button _nextButton;

        private void Start()
        {
            EnsureCamera();

            _character = new Character("player", maxHitPoints: 20, maxMovementPoints: 3, new CharacterAttributes());
            _profile = new CharacterCreationProfile();

            BuildUi();
            RefreshStep();
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
            var canvasGameObject = new GameObject("CharacterCreationCanvas");
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

            var titleText = CreateText(canvasGameObject.transform, new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.99f));
            titleText.text = "Criação de Personagem";
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;

            var stepContainerGameObject = new GameObject("StepContainer");
            stepContainerGameObject.transform.SetParent(canvasGameObject.transform, worldPositionStays: false);
            var stepContainerRect = stepContainerGameObject.AddComponent<RectTransform>();
            stepContainerRect.anchorMin = new Vector2(0.05f, 0.18f);
            stepContainerRect.anchorMax = new Vector2(0.95f, 0.9f);
            stepContainerRect.offsetMin = Vector2.zero;
            stepContainerRect.offsetMax = Vector2.zero;
            _stepContainer = stepContainerGameObject.transform;

            _statusText = CreateText(canvasGameObject.transform, new Vector2(0.05f, 0.09f), new Vector2(0.95f, 0.17f));
            _statusText.alignment = TextAnchor.UpperLeft;
            _statusText.fontSize = 14;

            _backButton = CreateButton(canvasGameObject.transform, "Voltar", new Vector2(0.05f, 0.02f), OnBackClicked);
            _nextButton = CreateButton(canvasGameObject.transform, "Avançar", new Vector2(0.79f, 0.02f), OnNextOrFinalizeClicked);
        }

        private void OnBackClicked()
        {
            if (_currentStep == Step.Attributes)
            {
                return;
            }

            _currentStep--;
            RefreshStep();
        }

        private void OnNextOrFinalizeClicked()
        {
            switch (_currentStep)
            {
                case Step.Attributes:
                    if (_profile.AttributeAllocation.PointsRemaining != 0)
                    {
                        return; // FR-003: cannot advance while points remain unspent.
                    }

                    _currentStep = Step.Orientation;
                    break;

                case Step.Orientation:
                    if (_profile.Orientation == null)
                    {
                        return; // Finalization contract, precondition 2.
                    }

                    _currentStep = Step.Appearance;
                    break;

                case Step.Appearance:
                    _currentStep = Step.Summary;
                    break;

                case Step.Summary:
                    _profile.Finalize(_character, equipmentKits);
                    PendingPlayerCharacter.Set(_character);
                    SceneManager.LoadScene(explorationSceneName);
                    return; // Scene transition (contracts/scene-transition-contract.md) — nothing left to refresh here.
            }

            RefreshStep();
        }

        private void RefreshStep()
        {
            for (var i = _stepContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_stepContainer.GetChild(i).gameObject);
            }

            switch (_currentStep)
            {
                case Step.Attributes:
                    BuildAttributesStep();
                    break;
                case Step.Orientation:
                    BuildOrientationStep();
                    break;
                case Step.Appearance:
                    BuildAppearanceStep();
                    break;
                case Step.Summary:
                    BuildSummaryStep();
                    break;
            }

            _backButton.interactable = _currentStep != Step.Attributes;
            _nextButton.gameObject.GetComponentInChildren<Text>().text =
                _currentStep == Step.Summary ? "Finalizar" : "Avançar";

            UpdateStatusText();
        }

        private void BuildAttributesStep()
        {
            var attributes = new[]
            {
                AttributeKind.Strength, AttributeKind.Dexterity, AttributeKind.Intellect, AttributeKind.Willpower,
            };

            for (var i = 0; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                var y = 0.8f - i * 0.22f;

                CreateText(_stepContainer, new Vector2(0.0f, y), new Vector2(0.3f, y + 0.2f)).text = attribute.ToString();

                var scoreText = CreateText(_stepContainer, new Vector2(0.3f, y), new Vector2(0.45f, y + 0.2f));
                scoreText.alignment = TextAnchor.MiddleCenter;
                scoreText.text = _profile.AttributeAllocation.GetScore(attribute).ToString();

                CreateSmallButton(_stepContainer, "-", new Vector2(0.48f, y + 0.02f), () =>
                {
                    _profile.AttributeAllocation.TryChangeAttribute(attribute, _profile.AttributeAllocation.GetScore(attribute) - 1);
                    RefreshStep();
                });

                CreateSmallButton(_stepContainer, "+", new Vector2(0.56f, y + 0.02f), () =>
                {
                    _profile.AttributeAllocation.TryChangeAttribute(attribute, _profile.AttributeAllocation.GetScore(attribute) + 1);
                    RefreshStep();
                });
            }
        }

        private void BuildOrientationStep()
        {
            CreateActionButton(_stepContainer, "Combatente", new Vector2(0.1f, 0.4f), () =>
            {
                _profile.Orientation = CharacterOrientation.Combatant;
                RefreshStep();
            });

            CreateActionButton(_stepContainer, "Arcanista", new Vector2(0.55f, 0.4f), () =>
            {
                _profile.Orientation = CharacterOrientation.Arcanist;
                RefreshStep();
            });
        }

        private void BuildAppearanceStep()
        {
            CreateText(_stepContainer, new Vector2(0f, 0.75f), new Vector2(0.4f, 0.85f)).text =
                $"Tipo de corpo: {_profile.VisualCharacteristics.BodyType}";
            CreateSmallButton(_stepContainer, "Trocar", new Vector2(0.42f, 0.75f), () =>
            {
                var current = _profile.VisualCharacteristics;
                current.BodyType = current.BodyType == BodyType.Slim ? BodyType.Sturdy : BodyType.Slim;
                _profile.VisualCharacteristics = current;
                RefreshStep();
            });

            CreateText(_stepContainer, new Vector2(0f, 0.55f), new Vector2(0.4f, 0.65f)).text =
                $"Tom de pele: {_profile.VisualCharacteristics.SkinTone}";
            CreateSmallButton(_stepContainer, "Trocar", new Vector2(0.42f, 0.55f), () =>
            {
                var current = _profile.VisualCharacteristics;
                current.SkinTone = (SkinTone)(((int)current.SkinTone + 1) % 3);
                _profile.VisualCharacteristics = current;
                RefreshStep();
            });

            CreateText(_stepContainer, new Vector2(0f, 0.35f), new Vector2(0.4f, 0.45f)).text =
                $"Cabelo: {_profile.VisualCharacteristics.HairStyle}";
            CreateSmallButton(_stepContainer, "Trocar", new Vector2(0.42f, 0.35f), () =>
            {
                var current = _profile.VisualCharacteristics;
                current.HairStyle = (HairStyle)(((int)current.HairStyle + 1) % 3);
                _profile.VisualCharacteristics = current;
                RefreshStep();
            });
        }

        private void BuildSummaryStep()
        {
            var text = CreateText(_stepContainer, new Vector2(0f, 0f), new Vector2(1f, 1f));
            text.alignment = TextAnchor.UpperLeft;
            text.text =
                $"Força: {_profile.AttributeAllocation.GetScore(AttributeKind.Strength)}\n" +
                $"Destreza: {_profile.AttributeAllocation.GetScore(AttributeKind.Dexterity)}\n" +
                $"Intelecto: {_profile.AttributeAllocation.GetScore(AttributeKind.Intellect)}\n" +
                $"Vontade: {_profile.AttributeAllocation.GetScore(AttributeKind.Willpower)}\n\n" +
                $"Orientação: {_profile.Orientation}\n\n" +
                $"Aparência: {_profile.VisualCharacteristics.BodyType}, {_profile.VisualCharacteristics.SkinTone}, " +
                $"{_profile.VisualCharacteristics.HairStyle}";
        }

        private void UpdateStatusText()
        {
            _statusText.text = _currentStep switch
            {
                Step.Attributes => $"Pontos restantes: {_profile.AttributeAllocation.PointsRemaining}",
                Step.Orientation => "Escolha a orientação predominante do seu personagem.",
                Step.Appearance => "Personalize a aparência do seu personagem.",
                Step.Summary => "Revise suas escolhas e finalize.",
                _ => string.Empty,
            };
        }

        // Rendering delegates to the shared DemoUiKit (FR-007) instead of keeping a
        // local copy of these methods — see research.md, "Extrair os componentes de
        // UI duplicados".
        private Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax) =>
            DemoUiKit.CreateText(parent, anchorMin, anchorMax);

        private Button CreateButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.18f, 0.06f), onClick, fontSize: 14);

        private void CreateActionButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.3f, 0.15f), onClick, fontSize: 16);

        private void CreateSmallButton(Transform parent, string label, Vector2 anchorMin, UnityEngine.Events.UnityAction onClick) =>
            DemoUiKit.CreateButton(parent, label, anchorMin, new Vector2(0.06f, 0.06f), onClick, fontSize: 12);
    }
}
