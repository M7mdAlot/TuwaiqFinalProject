using UnityEngine;

namespace Aegis.Systems
{
    /// <summary>
    /// Tiny on-screen readout for the crisis (objective banner + countdown + fill bar).
    /// Uses IMGUI (OnGUI) so there's no Canvas/setup — just drop it anywhere in the scene.
    /// Shows the CrisisManager's state live: idle → active → success/failure.
    ///
    /// NOTE: testing only. The real HUD will come from UIManager + SCI-FI UI Pack Pro.
    /// </summary>
    public class CrisisDebugHud : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find a CrisisManager in the scene.")]
        [SerializeField] private CrisisManager _manager;

        [Header("Style")]
        [SerializeField] private int _bannerFontSize = 26;
        [SerializeField] private int _timerFontSize = 42;
        [SerializeField] private int _resultFontSize = 48;
        [SerializeField] private Vector2 _topOffset = new Vector2(0f, 20f);
        [SerializeField] private Vector2 _barSize = new Vector2(360f, 16f);
        [Tooltip("Seconds after resolve to keep the SUCCESS/FAILED text visible.")]
        [SerializeField] private float _resultDisplayTime = 3f;

        [Header("Colors")]
        [SerializeField] private Color _bannerColor = new Color(1f, 0.95f, 0.7f, 1f);
        [SerializeField] private Color _timerColor = new Color(0.85f, 0.95f, 1f, 1f);
        [SerializeField] private Color _warningColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color _dangerColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _successColor = new Color(0.4f, 1f, 0.5f, 1f);
        [SerializeField] private Color _barBackColor = new Color(0f, 0f, 0f, 0.6f);

        [Header("Thresholds")]
        [Tooltip("Timer turns yellow below this many seconds.")]
        [SerializeField] private float _warningAt = 10f;
        [Tooltip("Timer turns red / flashes below this many seconds.")]
        [SerializeField] private float _dangerAt = 5f;

        private GUIStyle _bannerStyle;
        private GUIStyle _timerStyle;
        private GUIStyle _resultStyle;
        private Texture2D _pixel;
        private float _resolvedAt = -1f;

        private void Awake()
        {
#if UNITY_2023_1_OR_NEWER
            if (_manager == null) _manager = FindFirstObjectByType<CrisisManager>();
#else
            if (_manager == null) _manager = FindObjectOfType<CrisisManager>();
#endif
        }

        private void OnDestroy()
        {
            if (_pixel != null) Destroy(_pixel);
        }

        private void OnGUI()
        {
            if (_manager == null || !_manager.HasTriggered) return;

            EnsureStyles();

            // Track when it resolved so we can show the result briefly, then fade out.
            if (_manager.IsResolved && _resolvedAt < 0f) _resolvedAt = Time.unscaledTime;

            if (_manager.IsResolved)
            {
                DrawResult();
                if (Time.unscaledTime - _resolvedAt > _resultDisplayTime) return;
            }
            else
            {
                DrawObjectiveAndTimer();
            }
        }

        private void DrawObjectiveAndTimer()
        {
            float centerX = Screen.width * 0.5f;
            float y = _topOffset.y;

            // Objective banner.
            string banner = string.IsNullOrEmpty(_manager.ObjectiveText) ? "OBJECTIVE" : _manager.ObjectiveText;
            _bannerStyle.normal.textColor = _bannerColor;
            Rect bannerRect = new Rect(centerX - 400f, y, 800f, _bannerFontSize + 10f);
            DrawShadowedLabel(bannerRect, banner, _bannerStyle);
            y += bannerRect.height + 4f;

            // Countdown text — color changes as time drops.
            float remaining = _manager.TimeRemaining;
            Color timerColor =
                remaining <= _dangerAt ? _dangerColor :
                remaining <= _warningAt ? _warningColor :
                _timerColor;

            // Danger flash: fade in/out below _dangerAt.
            if (remaining <= _dangerAt)
            {
                float pulse = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
                timerColor = new Color(timerColor.r, timerColor.g, timerColor.b, pulse);
            }

            _timerStyle.normal.textColor = timerColor;
            string timerText = FormatTime(remaining);
            Rect timerRect = new Rect(centerX - 200f, y, 400f, _timerFontSize + 12f);
            DrawShadowedLabel(timerRect, timerText, _timerStyle);
            y += timerRect.height + 4f;

            // Progress bar (drains as time runs out).
            float barX = centerX - _barSize.x * 0.5f;
            DrawRect(new Rect(barX, y, _barSize.x, _barSize.y), _barBackColor);
            float fill = _manager.Duration > 0f ? (remaining / _manager.Duration) : 0f;
            DrawRect(new Rect(barX, y, _barSize.x * Mathf.Clamp01(fill), _barSize.y), timerColor);
        }

        private void DrawResult()
        {
            string text = _manager.ResolvedWithSuccess ? "OBJECTIVE COMPLETE" : "OBJECTIVE FAILED";
            _resultStyle.normal.textColor = _manager.ResolvedWithSuccess ? _successColor : _dangerColor;

            Rect rect = new Rect(0f, Screen.height * 0.35f, Screen.width, _resultFontSize + 20f);
            DrawShadowedLabel(rect, text, _resultStyle);
        }

        private void EnsureStyles()
        {
            if (_bannerStyle == null)
                _bannerStyle = new GUIStyle(GUI.skin.label) { fontSize = _bannerFontSize, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            if (_timerStyle == null)
                _timerStyle = new GUIStyle(GUI.skin.label) { fontSize = _timerFontSize, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            if (_resultStyle == null)
                _resultStyle = new GUIStyle(GUI.skin.label) { fontSize = _resultFontSize, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        }

        private static string FormatTime(float seconds)
        {
            int total = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }

        private void DrawShadowedLabel(Rect rect, string text, GUIStyle style)
        {
            GUIStyle shadow = new GUIStyle(style);
            shadow.normal.textColor = new Color(0f, 0f, 0f, style.normal.textColor.a);
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), text, shadow);
            GUI.Label(rect, text, style);
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
