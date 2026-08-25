using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameRpg.Demo
{
    /// <summary>
    /// A small world-space health bar floating above a combatant, plus a brief
    /// flash on damage — the visual feedback requested for manual combat
    /// testing (a bar alone was hard to notice; the flash makes a hit read
    /// instantly). Billboards to face the main camera every frame.
    /// </summary>
    public class HealthBarWidget : MonoBehaviour
    {
        private RectTransform _fillRect;
        private Image _fillImage;
        private Color _normalFillColor;
        private Coroutine _flashCoroutine;

        public static HealthBarWidget Create(Transform target, Vector3 localOffset)
        {
            var canvasGameObject = new GameObject("HealthBar");
            canvasGameObject.transform.SetParent(target, worldPositionStays: false);
            canvasGameObject.transform.localPosition = localOffset;
            canvasGameObject.transform.localScale = Vector3.one * 0.01f;

            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var canvasRect = canvasGameObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100f, 14f);

            var background = new GameObject("Background");
            background.transform.SetParent(canvasGameObject.transform, worldPositionStays: false);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            var backgroundRect = backgroundImage.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(canvasGameObject.transform, worldPositionStays: false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.green;
            var fillRect = fillImage.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            fillRect.pivot = new Vector2(0f, 0.5f);

            var widget = canvasGameObject.AddComponent<HealthBarWidget>();
            widget._fillRect = fillRect;
            widget._fillImage = fillImage;
            widget._normalFillColor = Color.green;
            return widget;
        }

        private void LateUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }

        /// <summary>Updates the bar's fill (0-1) and color (green -> yellow -> red as it depletes).</summary>
        public void SetFraction(float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            _fillRect.anchorMax = new Vector2(fraction, 1f);
            _normalFillColor = Color.Lerp(Color.red, Color.green, fraction);
            if (_flashCoroutine == null)
            {
                _fillImage.color = _normalFillColor;
            }
        }

        /// <summary>Briefly flashes the bar white to draw the eye to a hit just landed.</summary>
        public void FlashDamage()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _fillImage.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            _fillImage.color = _normalFillColor;
            _flashCoroutine = null;
        }
    }
}
