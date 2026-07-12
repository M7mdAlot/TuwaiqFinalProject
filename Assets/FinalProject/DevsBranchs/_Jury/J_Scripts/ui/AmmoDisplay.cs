using UnityEngine;
using TMPro;
using Aegis.Core;
using Aegis.Player;
using Aegis.Weapons;

// Self-contained ammo readout. Drop this straight onto the AMMO TEXT object.
// It finds the PLAYER's WeaponHandler by itself and updates every frame. Shows "-- / --"
// until a weapon is equipped. Crash-proof and prefers the player (enemies have
// WeaponHandlers too now, so a blind search could grab the wrong one).
[RequireComponent(typeof(TMP_Text))]
public class AmmoDisplay : MonoBehaviour
{
    [Tooltip("Optional. Left empty, it auto-finds the PLAYER's WeaponHandler in the scene.")]
    public WeaponHandler weapons;

    private TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void Start()
    {
        // If you DON'T see this log, the AMMO TEXT object is INACTIVE (turn its checkbox on)
        // or it's missing this component — that's why the ammo doesn't appear.
        Debug.Log("AmmoDisplay: RUNNING on '" + name + "'.", this);
    }

    void Update()
    {
        if (label == null) return;

        // Keep the text itself visible in case something dimmed its alpha.
        if (label.color.a < 1f) { Color col = label.color; col.a = 1f; label.color = col; }

        // Re-acquire the player's weapon handler if we lost it (or grabbed an enemy's).
        if (weapons == null || !IsPlayer(weapons)) weapons = FindPlayerWeapons();

        if (weapons == null) { label.text = "-- / --"; return; }

        Weapon w = weapons.CurrentWeapon;
        if (w == null || w.Data == null) { label.text = "-- / --"; return; }

        string max = w.Data.ammo < 0 ? "∞" : w.Data.ammo.ToString(); // ∞ for infinite
        label.text = w.CurrentAmmo + " / " + max;
    }

    static bool IsPlayer(Component c)
    {
        return c != null &&
               (c.GetComponentInParent<PlayerCharacterIdentity>() != null || c.CompareTag("Player"));
    }

    WeaponHandler FindPlayerWeapons()
    {
        // 1) A WeaponHandler that lives on the actual player.
        PlayerCharacterIdentity id = FindFirstObjectByType<PlayerCharacterIdentity>();
        if (id != null)
        {
            WeaponHandler wh = id.GetComponentInChildren<WeaponHandler>(true);
            if (wh != null) return wh;
        }

        // 2) Tagged "Player".
        GameObject tagged = GameObject.FindWithTag("Player");
        if (tagged != null)
        {
            WeaponHandler wh = tagged.GetComponentInChildren<WeaponHandler>(true);
            if (wh != null) return wh;
        }

        // 3) Last resort: any (may be an enemy's, but better than nothing).
        return FindFirstObjectByType<WeaponHandler>();
    }
}
