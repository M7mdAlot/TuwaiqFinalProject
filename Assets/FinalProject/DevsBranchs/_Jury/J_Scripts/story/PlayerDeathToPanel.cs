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
        if (playerHealth == null) playerHealth = FindFirstObjectByType<HealthSystem>();
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
        if (xEnding != null) xEnding.ShowDeathPanel();
        else if (aegisEnding != null) aegisEnding.ShowDeathPanel();
        else Debug.LogWarning("PlayerDeathToPanel: no ending manager assigned.", this);
    }
}
