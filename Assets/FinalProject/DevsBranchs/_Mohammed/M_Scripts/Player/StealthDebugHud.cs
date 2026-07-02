using UnityEngine;
using Aegis.Systems;

namespace Aegis.Player
{
    /// <summary>
    /// Tiny on-screen debug readout for the stealth alert level. Shows a colored bar at the
    /// top center of the Game view that fills with the highest detection across all guards,
    /// plus a status label (Hidden / Suspicious / Spotted).
    ///
    /// Uses IMGUI (OnGUI) so there's no Canvas/setup — drop it anywhere in the scene.
    /// NOTE: testing only. The real readout will come from UIManager + SCI-FI UI Pack Pro.
    /// </summary>
    public class StealthDebugHud : MonoBehaviour
    {
        [Header("Style")]
        [SerializeField] private int _fontSize = 18;
        [SerializeField] private Vector2 _topOffset = new Vector2(0f, 16f);
        [SerializeField] private Vector2 _barSize = new Vector2(280f, 14f);

        [Header("Colors")]
        [SerializeField] private Color _backColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color _hiddenColor = new Color(0.4f, 0.9f, 0.4f, 0.9f);
        [SerializeField] private Color _suspiciousColor = new Color(1f, 0.85f, 0.2f, 0.9f);
        [SerializeField] private Color _spottedColor = new Color(1f, 0.25f, 0.25f, 0.95f);

        private Texture2D _pixel;
        private GUIStyle _labelStyle;

        private void OnDestroy()
        {
            if (_pixel != null) Destroy(_pixel);
        }

        private void OnGUI()
        {
            if (StealthSystem.Instance == null) return;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            float detection = StealthSystem.Instance.HighestDetection;
            StealthSystem.AlertLevel level = StealthSystem.Instance.CurrentLevel;

            Color fill =
                level == StealthSystem.AlertLevel.Spotted    ? _spottedColor :
                level == StealthSystem.AlertLevel.Suspicious ? _suspiciousColor :
                                                               _hiddenColor;

            float barX = (Screen.width - _barSize.x) * 0.5f + _topOffset.x;
            float barY = _topOffset.y;

            DrawRect(new Rect(barX, barY, _barSize.x, _barSize.y), _backColor);
            DrawRect(new Rect(barX, barY, _barSize.x * Mathf.Clamp01(detection), _barSize.y), fill);

            string text = level.ToString().ToUpperInvariant() + $"   ({Mathf.RoundToInt(detection * 100f)}%)";
            _labelStyle.normal.textColor = level == StealthSystem.AlertLevel.Spotted ? Color.white : Color.white;

            Rect textRect = new Rect(barX, barY + _barSize.y + 2f, _barSize.x, _fontSize + 6f);
            GUIStyle shadow = new GUIStyle(_labelStyle); shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(textRect.x + 2, textRect.y + 2, textRect.width, textRect.height), text, shadow);
            GUI.Label(textRect, text, _labelStyle);
        }

        private void DrawRect(Rect rect, Color color)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(1, 1);
                _pixel.SetPixel(0, 0, Color.white);
                _pixel.Apply();
            }
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, _pixel);
            GUI.color = old;
        }
    }
}
