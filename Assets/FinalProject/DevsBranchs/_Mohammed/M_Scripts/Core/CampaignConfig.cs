using System.Collections.Generic;
using UnityEngine;
using Aegis.Weapons;
using Aegis.Systems;

namespace Aegis.Core
{
    /// <summary>
    /// The single switch that makes the game "good" or "evil" (GDD §4). One asset for
    /// AEGIS.2, one for X. Systems read this to know which faction to fight, which weapons
    /// are allowed, which crisis to run, and which dialogue to play. Make assets via
    /// Assets ▸ Create ▸ AEGIS ▸ Data ▸ Campaign Config.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Data/Campaign Config", fileName = "CampaignConfig")]
    public class CampaignConfig : ScriptableObject
    {
        public string displayName;
        public Side side = Side.Good;
        public Faction hostileFaction = Faction.Corrupted;
        public ClimaxType climaxType = ClimaxType.Reactor;

        [Tooltip("Weapons this side is allowed to find / use.")]
        public List<WeaponData> allowedWeapons = new List<WeaponData>();

        public DialogueData dialogueSet;
    }
}
