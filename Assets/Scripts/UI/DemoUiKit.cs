using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameRpg.UI
{
    /// <summary>
    /// Shared runtime UI component factory (FR-007). Consolidates the CreateText/
    /// CreateButton methods previously duplicated in each demo controller (see
    /// research.md, "Extrair os componentes de UI duplicados"), and applies the
    /// Kenney UI Pack assets (Assets/Art/UI/Resources/) to every button built
    /// through it, so the new visual propagates automatically to every screen that
    /// uses this kit instead of its own local copy.
    /// </summary>
    public static class DemoUiKit
    {
        private static Font _sharedFont;
        private static Sprite _buttonSprite;
        private static Sprite _buttonPressedSprite;
        private static Sprite _panelSprite;
        private static bool _resourcesLoaded;

        private static void EnsureResourcesLoaded()
        {
            if (_resourcesLoaded)
            {
                return;
            }

            _sharedFont = Resources.Load<Font>("KenneyFuture");
            _buttonSprite = Resources.Load<Sprite>("button_flat");
            _buttonPressedSprite = Resources.Load<Sprite>("button_flat_pressed");
            _panelSprite = Resources.Load<Sprite>("panel_background");
            _resourcesLoaded = true;
        }

        private static Font SharedFont
        {
            get
            {
                EnsureResourcesLoaded();
                return _sharedFont != null ? _sharedFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
        }

        private static Sprite ButtonSprite
        {
            get
            {
                EnsureResourcesLoaded();
                return _buttonSprite;
            }
        }

        private static Sprite PanelSprite
        {
            get
            {
                EnsureResourcesLoaded();
                return _panelSprite;
            }
        }

        public static Text CreateText(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textGameObject = new GameObject("Text");
            textGameObject.transform.SetParent(parent, worldPositionStays: false);
            var text = textGameObject.AddComponent<Text>();
            text.font = SharedFont;
            text.color = Color.white;
            var rect = text.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return text;
        }

        /// <summary>anchorMin/size define the button's rect the same way the demo
        /// controllers already position elements (anchorMax = anchorMin + size).</summary>
        public static Button CreateButton(Transform parent, string label, Vector2 anchorMin, Vector2 size, UnityAction onClick, int fontSize = 14)
        {
            var buttonGameObject = new GameObject($"Button_{label}");
            buttonGameObject.transform.SetParent(parent, worldPositionStays: false);
            var image = buttonGameObject.AddComponent<Image>();
            var button = buttonGameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ApplyButtonSprite(image, button);

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMin + size;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = CreateText(buttonGameObject.transform, Vector2.zero, Vector2.one);
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = fontSize;
            text.color = Color.white;

            return button;
        }

        public static Image CreatePanel(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var panelGameObject = new GameObject("Panel");
            panelGameObject.transform.SetParent(parent, worldPositionStays: false);
            var image = panelGameObject.AddComponent<Image>();
            ApplyPanelSprite(image);

            var rect = image.rectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private static void ApplyButtonSprite(Image image, Button button)
        {
            var sprite = ButtonSprite;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;

                var pressedSprite = _buttonPressedSprite;
                if (pressedSprite != null)
                {
                    button.transition = Selectable.Transition.SpriteSwap;
                    button.spriteState = new SpriteState { pressedSprite = pressedSprite, disabledSprite = pressedSprite };
                }
            }
            else
            {
                // FR-007 best-effort fallback: assets not imported yet (e.g. tests running
                // without the Kenney pack present) — keep the previous flat-color look.
                image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }
        }

        private static void ApplyPanelSprite(Image image)
        {
            var sprite = PanelSprite;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            else
            {
                image.color = new Color(0.1f, 0.1f, 0.12f, 0.85f);
            }
        }
    }
}
