using UnityEngine;

namespace Aegis.Weapons
{
    /// <summary>
    /// Pure data describing a weapon (GDD §9). No logic — the Weapon / WeaponHandler
    /// scripts (Tier 2) read these values to actually shoot. Make one asset per weapon via
    /// Assets ▸ Create ▸ AEGIS ▸ Data ▸ Weapon.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Data/Weapon", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Rules")]
        public WeaponAvailability allowed = WeaponAvailability.Both;
        public FireMode fireMode = FireMode.Single;
        public WeaponEffect effect = WeaponEffect.None;

        [Header("Stats [TUNABLE]")]
        public float damage = 10f;
        public float splashRadius = 0f;
        public float chargeTime = 0f;       // used by Charge fire mode
        public float fireRate = 5f;         // shots per second (Auto)
        public float projectileSpeed = 50f;
        public int ammo = 30;

        [Header("Prefabs & FX")]
        public GameObject projectilePrefab;
        public GameObject muzzleVFX;
        public GameObject hitVFX;
        public AudioClip fireSFX;

        [Header("Muzzle (per weapon, in the muzzle anchor's local space)")]
        [Tooltip("Where this weapon's barrel sticks out, relative to the player's muzzle anchor (e.g. the camera). +X = right, +Y = up, +Z = forward. Different shapes per gun.")]
        public Vector3 muzzleOffset;
    }
}
