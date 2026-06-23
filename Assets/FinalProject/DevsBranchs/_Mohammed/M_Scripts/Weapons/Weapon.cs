using System;

namespace Aegis.Weapons
{
    /// <summary>
    /// Runtime instance of an equipped weapon. Pairs the immutable <see cref="WeaponData"/>
    /// (damage, fire rate, prefab — defined once in the asset) with per-instance state that
    /// changes during play (current ammo, cooldown, charge progress).
    ///
    /// NOT a MonoBehaviour — these live inside <see cref="WeaponHandler"/>, which ticks them.
    /// Tier 2 — depends on Tier 0 data + the FireMode enum.
    /// </summary>
    [Serializable]
    public class Weapon
    {
        public WeaponData Data { get; }
        public int CurrentAmmo { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float ChargeProgress01 { get; private set; } // 0..1, only meaningful for Charge fire mode
        public bool IsCharging { get; private set; }

        /// <summary>Negative ammo on the data means "infinite" (e.g. a beam or melee).</summary>
        public bool HasAmmo => Data.ammo < 0 || CurrentAmmo > 0;
        public bool IsReady => CooldownRemaining <= 0f;

        /// <summary>Seconds between shots, derived from fire rate (shots/sec).</summary>
        public float TimeBetweenShots => Data.fireRate > 0f ? 1f / Data.fireRate : 0f;

        public Weapon(WeaponData data)
        {
            Data = data;
            CurrentAmmo = data.ammo;
        }

        /// <summary>Advance internal timers. The WeaponHandler calls this every frame.</summary>
        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f) CooldownRemaining -= deltaTime;
            if (IsCharging && Data.chargeTime > 0f)
                ChargeProgress01 = Math.Min(1f, ChargeProgress01 + deltaTime / Data.chargeTime);
        }

        public void StartCharging() { IsCharging = true; ChargeProgress01 = 0f; }
        public void StopCharging() { IsCharging = false; ChargeProgress01 = 0f; }

        /// <summary>Called by the handler after a successful shot — spends one round + starts the cooldown.</summary>
        public void OnFired()
        {
            if (Data.ammo >= 0) CurrentAmmo = Math.Max(0, CurrentAmmo - 1);
            CooldownRemaining = TimeBetweenShots;
            ChargeProgress01 = 0f;
            IsCharging = false;
        }

        public void Refill() => CurrentAmmo = Data.ammo;
    }
}
