using UnityEngine;
using Aegis.Core;
using Aegis.Weapons;

namespace Aegis.Enemies
{
    /// <summary>
    /// Pure data describing an enemy type (GDD §10): a corrupted AEGIS robot or an armed
    /// human. The EnemyController (Tier 2) reads these values. Make one asset per enemy via
    /// Assets ▸ Create ▸ AEGIS ▸ Data ▸ Enemy.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Data/Enemy", fileName = "EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;
        public Faction faction = Faction.Corrupted;

        [Header("Stats [TUNABLE]")]
        public float health = 60f;
        public float moveSpeed = 3.5f;
        public float detectionRadius = 15f;
        public float rangedAttackRange = 10f;
        public float meleeAttackRange = 2f;

        [Header("Loadout")]
        public WeaponData weapon;
        public GameObject prefab;
    }
}
