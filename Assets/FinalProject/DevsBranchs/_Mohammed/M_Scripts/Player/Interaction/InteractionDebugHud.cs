using UnityEngine;
using Aegis.Core;

namespace Aegis.Player
{
    /// <summary>
    /// Tiny on-screen debug readout for the interaction prompt. Shows the prompt text
    /// (e.g. "Hold F to shut down") near the bottom of the Game view whenever the
    /// InteractionController is looking at an IInteractable, and draws a fill bar that
    /// tracks the hold-to-interact progress.
    ///
    /// Uses Unity's IMGUI (OnGUI) so there's no Canvas/setup — just drop it on the Player.
    /// NOTE: testing only. The real prompt will come from UIManager + SCI-FI UI Pack Pro.
    /// </summary>
    public class InteractionDebugHud : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find an InteractionController on this or a parent object.")]
        [SerializeField] private InteractionController _interaction;

        [Header("Style")]
        [SerializeField] private int _fontSize = 22;
        [SerializeField] private Color _color = Color.white;
        [Tooltip("Distance from the bottom of the screen, in pixels.")]
        [SerializeField] private float _bottomMargin = 80f;
        [SerializeField] private Vector2 _barSize = new Vector2(220f, 8f);
        [SerializeField] private Color _barFillColor = new Color(1f, 0.9f, 0.2f, 0.9f);
        [SerializeField] private Color _barBackColor = new Color(0f, 0f, 0f, 0.6f);

        private GUIStyle _labelStyle;
        private Texture2D _whitePixel;

        private void Awake()
        {
            if (_interaction == null) _interaction = GetComponentInParent<InteractionController>();
        }

        private void OnDestroy()
        {
            // Clean up the runtime-created pixel texture, if any.
            if (_whitePixel != null) Destroy(_whitePixel);
        }

        private void OnGUI()
        {
            if (_interaction == null || _interaction.CurrentTarget == null) return;

            IInteractable target = _interaction.CurrentTarget;
            string text = target.Prompt ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            // Build the style lazily — GUI.skin is only valid inside OnGUI.
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            _labelStyle.normal.textColor = _color;

            // Centered, near the bottom.
            float w = 600f;
            float h = 40f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - _bottomMargin - h;

            // Cheap drop-shadow.
            GUIStyle shadow = new GUIStyle(_labelStyle);
            shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(x + 2, y + 2, w, h), text, shadow);
            GUI.Label(new Rect(x, y, w, h), text, _labelStyle);

            // Progress bar for hold-style interactions.
            if (target.HoldDuration > 0f)
            {
                float barX = (Screen.width - _barSize.x) * 0.5f;
                float barY = y + h + 6f;
                DrawRect(new Rect(barX, barY, _barSize.x, _barSize.y), _barBackColor);
                float fill = Mathf.Clamp01(_interaction.HoldProgress01) * _barSize.x;
                DrawRect(new Rect(barX, barY, fill, _barSize.y), _barFillColor);
            }
        }

        // Tiny helper: GUI.DrawTexture with a single white pixel tinted via GUI.color.
        private void DrawRect(Rect rect, Color color)
        {
            if (_whitePixel == null)
            {
                _whitePixel = new Texture2D(1, 1);
                _whitePixel.SetPixel(0, 0, Color.white);
                _whitePixel.Apply();
            }

            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _whitePixel);
            GUI.color = old;
        }
    }
}
