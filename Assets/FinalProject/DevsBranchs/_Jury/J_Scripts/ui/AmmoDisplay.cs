using UnityEngine;
using TMPro;
using Aegis.Player;
using Aegis.Weapons;

// Self-contained ammo readout. Drop this straight onto the AMMO TEXT object.
// It finds the player's WeaponHandler by itself and updates every frame — no
// PlayerHudBridge or UIManager wiring required. Shows "-- / --" until a weapon
// is equipped (WeaponHandler needs at least one weapon in its Starting Weapons).
[RequireComponent(typeof(TMP_Text))]
public class AmmoDisplay : MonoBehaviour
{
    [Tooltip("Optional. Left empty, it auto-finds the player's WeaponHandler in the scene.")]
    public WeaponHandler weapons;

    private TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (label == null) return;

        if (weapons == null)
            weapons = FindFirstObjectByType<WeaponHandler>();

        if (weapons == null)
        {
            label.text = "-- / --";
            return;
        }

        Weapon w = weapons.CurrentWeapon;
        if (w == null)
        {
            label.text = "-- / --";
            return;
        }

        string max = w.Data.ammo < 0 ? "∞" : w.Data.ammo.ToString(); // ∞ for infinite
        label.text = w.CurrentAmmo + " / " + max;
    }
}
