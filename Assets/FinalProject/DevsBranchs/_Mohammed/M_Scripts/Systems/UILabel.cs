using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Aegis.Systems
{
    /// <summary>
    /// A text slot that works with EITHER a legacy uGUI <see cref="Text"/> (what the
    /// SCI-FI UI Pack Pro uses) OR a <see cref="TMP_Text"/> (what the teammate's UI uses).
    /// In the Inspector you get two slots — assign whichever your widget has (or both).
    /// UIManager uses this so it doesn't matter which text type your HUD is built from.
    /// </summary>
    [Serializable]
    public class UILabel
    {
        [Tooltip("Legacy uGUI Text (e.g. from the SCI-FI UI Pack Pro).")]
        [SerializeField] private Text _legacy;
        [Tooltip("TextMeshPro text (e.g. from the teammate's UI).")]
        [SerializeField] private TMP_Text _tmp;

        /// <summary>Set the displayed string on whichever text component is assigned.</summary>
        public void Set(string value)
        {
            if (_legacy != null) _legacy.text = value;
            if (_tmp != null) _tmp.text = value;
        }

        public bool HasTarget => _legacy != null || _tmp != null;
    }
}
