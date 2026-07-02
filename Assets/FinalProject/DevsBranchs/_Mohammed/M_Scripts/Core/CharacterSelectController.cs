using UnityEngine;
using UnityEngine.Events;

namespace Aegis.Core
{
    /// <summary>
    /// The character-select screen brain. Two public methods (<see cref="SelectAegis"/>,
    /// <see cref="SelectX"/>) are meant to be wired to the AEGIS.2 and X buttons in your
    /// UI — clicking a button stamps the chosen <see cref="CampaignConfig"/> onto
    /// <see cref="CampaignManager.ActiveConfig"/>, tells the GameManager we're entering
    /// the campaign, and loads the lab scene.
    /// Tier 4 — depends on Tier 0 (GameManager, SceneLoader) + Tier 4 (CampaignManager).
    /// </summary>
    public class CharacterSelectController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private CampaignConfig _aegisConfig;
        [SerializeField] private CampaignConfig _xConfig;

        [Header("Scene")]
        [Tooltip("Name of the campaign scene to load. Must be added to Build Settings.")]
        [SerializeField] private string _campaignSceneName;

        [Header("Optional hooks")]
        public UnityEvent<Side> OnSideSelected;

        /// <summary>Wire this to the AEGIS.2 button's OnClick.</summary>
        public void SelectAegis() => Select(_aegisConfig);

        /// <summary>Wire this to the X button's OnClick.</summary>
        public void SelectX() => Select(_xConfig);

        private void Select(CampaignConfig config)
        {
            if (config == null)
            {
                Debug.LogError("CharacterSelectController: config not assigned for this side.", this);
                return;
            }

            CampaignManager.ActiveConfig = config;
            OnSideSelected?.Invoke(config.side);

            if (GameManager.Instance != null) GameManager.Instance.StartCampaign();

            if (!string.IsNullOrEmpty(_campaignSceneName) && SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene(_campaignSceneName);
        }
    }
}
