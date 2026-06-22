using System;
using UnityEngine;

namespace Aegis.Systems
{
    /// <summary>
    /// One spoken/displayed line, tagged with the story beat it belongs to (GDD §7).
    /// Plain data used inside <see cref="DialogueData"/> (not a separate asset on its own).
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Which story moment this line belongs to, e.g. \"intro\", \"crisis\", \"ending\".")]
        public string beat;
        public string speaker;
        [TextArea] public string text;
        public AudioClip voiceClip; // optional machine-voice VO
    }
}
