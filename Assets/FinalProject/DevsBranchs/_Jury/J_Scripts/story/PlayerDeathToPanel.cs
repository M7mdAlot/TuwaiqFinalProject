using UnityEngine;
using Aegis.Player;

// Shows the death panel when the player dies. Listens to Mohammed's HealthSystem "Died"
// event and calls the ending manager. Put it on the player (or anywhere) and assign the
// ending manager for this scene (Aegis OR X).
public class PlayerDeathToPanel : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the player's HealthSystem on this object / in the scene.")]
    public HealthSystem playerHealth;

    [Header("Assign the ending manager for THIS scene (use one)")]
    public AegisEndingManager aegisEnding;
    public XEndingManager xEnding;

    void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<HealthSystem>();
        if (playerHealth == null) playerHealth = GetComponentInParent<HealthSystem>();

        // Prefer the actual player — enemies now carry HealthSystem too, so a blind
        // FindFirstObjectByType could grab a soldier's health by mistake.
        if (playerHealth == null)
        {
            GameObject tagged = GameObject.FindWithTag("Player");
            if (tagged != null) playerHealth = tagged.GetComponentInChildren<HealthSystem>();
        }
        if (playerHealth == null) playerHealth = FindFirstObjectByType<HealthSystem>();

        Debug.Log("PlayerDeathToPanel: watching health on '" +
                  (playerHealth != null ? playerHealth.gameObject.name : "NULL") +
                  "'. aegisEnding=" + (aegisEnding != null) + " xEnding=" + (xEnding != null), this);
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
    }

    void OnPlayerDied()
    {
        Debug.Log("PlayerDeathToPanel: PLAYER DIED -> routing to ending panel.", this);

        bool hasX = xEnding != null;
        bool hasAegis = aegisEnding != null;

        // Only one assigned (the normal case) -> use it, no guessing needed.
        if (hasX && !hasAegis) { xEnding.ShowDeathPanel(); return; }
        if (hasAegis && !hasX) { aegisEnding.ShowDeathPanel(); return; }

        // Both assigned -> pick by the active scene. "X scene" -> X, anything else -> Aegis.
        if (hasX && hasAegis)
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
            if (scene.StartsWith("x")) xEnding.ShowDeathPanel();
            else aegisEnding.ShowDeathPanel();
            return;
        }

        Debug.LogWarning("PlayerDeathToPanel: no ending manager assigned.", this);
    }
}
