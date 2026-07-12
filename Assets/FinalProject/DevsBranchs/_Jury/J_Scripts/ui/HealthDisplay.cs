using UnityEngine;
using UnityEngine.UI;
using Aegis.Player;

// Self-contained health bar. Drop this straight onto your HP bar Image (the coloured "fill"
// image). It finds the PLAYER's HealthSystem by itself and sets the bar's fillAmount every
// frame — no PlayerHudBridge / UIManager wiring needed.
//
// IMPORTANT: the Image this is on must have Image Type = Filled (Fill Method = Horizontal).
[RequireComponent(typeof(Image))]
public class HealthDisplay : MonoBehaviour
{
    [Tooltip("Optional. Left empty, it auto-finds the PLAYER's HealthSystem.")]
    public HealthSystem playerHealth;

    private Image bar;
    private float _lastPct = -1f;

    void Awake()
    {
        bar = GetComponent<Image>();
        if (bar != null) bar.type = Image.Type.Filled; // make sure fillAmount works
    }

    void Start()
    {
        Debug.Log("HealthDisplay: RUNNING on '" + name + "'.", this);
    }

    void Update()
    {
        if (bar == null) return;

        if (playerHealth == null || !IsPlayer(playerHealth)) playerHealth = FindPlayerHealth();
        if (playerHealth == null) return;

        float pct = playerHealth.MaxHealth > 0f ? playerHealth.CurrentHealth / playerHealth.MaxHealth : 0f;
        bar.fillAmount = Mathf.Clamp01(pct);

        // Log only when it actually changes, so we can see the real numbers without spam.
        if (Mathf.Abs(pct - _lastPct) > 0.001f)
        {
            _lastPct = pct;
            Debug.Log("HealthDisplay: reading '" + playerHealth.gameObject.name + "' HP=" +
                      playerHealth.CurrentHealth + "/" + playerHealth.MaxHealth +
                      " -> fill=" + pct.ToString("F2"), this);
        }
    }

    static bool IsPlayer(Component c)
    {
        return c != null &&
               (c.GetComponentInParent<PlayerCharacterIdentity>() != null || c.CompareTag("Player"));
    }

    HealthSystem FindPlayerHealth()
    {
        PlayerCharacterIdentity id = FindFirstObjectByType<PlayerCharacterIdentity>();
        if (id != null)
        {
            HealthSystem h = id.GetComponentInChildren<HealthSystem>(true);
            if (h != null) return h;
        }

        GameObject tagged = GameObject.FindWithTag("Player");
        if (tagged != null)
        {
            HealthSystem h = tagged.GetComponentInChildren<HealthSystem>(true);
            if (h != null) return h;
        }

        return FindFirstObjectByType<HealthSystem>();
    }
}
