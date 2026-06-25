using System.Collections.Generic;
using UnityEngine;

namespace Aegis.Systems
{
    /// <summary>
    /// An ordered set of dialogue lines for one campaign/side (GDD §7). The
    /// DialogueManager (Tier 1) plays these on the right story beat. Make one asset per
    /// side via Assets ▸ Create ▸ AEGIS ▸ Data ▸ Dialogue.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Data/Dialogue", fileName = "DialogueData")]
    public class DialogueData : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
    }
}
