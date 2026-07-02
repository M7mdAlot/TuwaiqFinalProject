using UnityEngine;
using Aegis.Player;
using Aegis.Systems;

namespace Aegis.Core
{
    /// <summary>
    /// The one script that turns a scene into either AEGIS.2's or X's campaign. Reads the
    /// active <see cref="CampaignConfig"/> (chosen at character select, or via the fallback
    /// field for testing) and applies it in one place:
    ///   • Sets the player's <see cref="WeaponHandler.PlayerSide"/> so faction filters work.
    ///   • Enables the "correct-side" set of spawners / pickups / props, disables the other.
    ///   • Hands the correct <see cref="DialogueData"/> to <see cref="DialogueManager"/>.
    ///
    /// Design: <see cref="ActiveConfig"/> is a static so it survives scene loads
    /// (CharacterSelectController sets it before loading the Lab scene). If it's null on
    /// scene start, <see cref="_fallbackConfig"/> is used — perfect for quick testing.
    /// Tier 4 — depends on Tier 2 (WeaponHandler) + Tier 1 (DialogueManager) + Tier 0.
    /// </summary>
    public class CampaignManager : MonoBehaviour
    {
        /// <summary>Set by CharacterSelectController before loading the campaign scene.</summary>
        public static CampaignConfig ActiveConfig { get; set; }

        [Header("Config")]
        [Tooltip("Used only if ActiveConfig hasn't been set (e.g. you press Play directly in the Lab scene).")]
        [SerializeField] private CampaignConfig _fallbackConfig;

        [Header("Player")]
        [Tooltip("The WeaponHandler on the player — needed to set PlayerSide for weapon filtering.")]
        [SerializeField] private WeaponHandler _playerWeaponHandler;

        [Header("Per-side content (enable one set, disable the other)")]
        [Tooltip("Objects active only for AEGIS.2 (corrupted robot spawners, Good weapon pickups, etc.).")]
        [SerializeField] private GameObject[] _goodSideObjects;
        [Tooltip("Objects active only for X (human spawners, Evil weapon pickups, failsafe room, etc.).")]
        [SerializeField] private GameObject[] _evilSideObjects;

        public CampaignConfig Config => ActiveConfig != null ? ActiveConfig : _fallbackConfig;

        private void Start()
        {
            CampaignConfig cfg = Config;
            if (cfg == null)
            {
                Debug.LogWarning("CampaignManager: no CampaignConfig set (ActiveConfig or fallback). Nothing to apply.", this);
                return;
            }

            ApplyConfig(cfg);
        }

        private void ApplyConfig(CampaignConfig cfg)
        {
            // Player weapon side.
            if (_playerWeaponHandler != null)
                _playerWeaponHandler.PlayerSide = cfg.side;

            // Per-side scene content.
            bool isGood = cfg.side == Side.Good;
            SetGroupActive(_goodSideObjects, isGood);
            SetGroupActive(_evilSideObjects, !isGood);

            // Dialogue.
            if (DialogueManager.Instance != null && cfg.dialogueSet != null)
                DialogueManager.Instance.SetDialogue(cfg.dialogueSet);
        }

        private static void SetGroupActive(GameObject[] group, bool active)
        {
            if (group == null) return;
            foreach (GameObject go in group)
                if (go != null && go.activeSelf != active) go.SetActive(active);
        }
    }
}
