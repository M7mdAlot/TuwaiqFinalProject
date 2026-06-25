using UnityEngine;
using Aegis.Weapons;

namespace Aegis.Player
{
    /// <summary>
    /// Tiny on-screen debug readout for the equipped weapon and its ammo. Uses Unity's
    /// IMGUI (OnGUI) so there's no Canvas/setup — just drop it on the Player.
    ///
    /// NOTE: This is for testing only. The real HUD will come from the SCI-FI UI Pack Pro
    /// via UIManager; this script can be removed once that exists.
    /// </summary>
    public class WeaponDebugHud : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find a WeaponHandler on this or a parent object.")]
        [SerializeField] private WeaponHandler _handler;

        [Header("Style")]
        [SerializeField] private int _fontSize = 22;
        [SerializeField] private Color _color = Color.white;
        [Tooltip("Top-left corner offset, in pixels.")]
        [SerializeField] private Vector2 _screenPadding = new Vector2(16f, 16f);

        private GUIStyle _style;

        private void Awake()
        {
            if (_handler == null) _handler = GetComponentInParent<WeaponHandler>();
        }

        private void OnGUI()
        {
            if (_handler == null) return;

            // Lazily build the GUIStyle once OnGUI is actually running (skin is only valid here).
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label) { fontSize = _fontSize };
            }
            _style.normal.textColor = _color;

            Weapon w = _handler.CurrentWeapon;
            string text = w == null
                ? "No weapon"
                : $"{w.Data.displayName}\nAmmo: {(w.Data.ammo < 0 ? "∞" : w.CurrentAmmo.ToString())}";

            Rect rect = new Rect(_screenPadding.x, _screenPadding.y, 400f, 80f);

            // Cheap drop-shadow so the text reads on any background.
            GUIStyle shadow = new GUIStyle(_style);
            shadow.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), text, shadow);

            GUI.Label(rect, text, _style);
        }
    }
}
