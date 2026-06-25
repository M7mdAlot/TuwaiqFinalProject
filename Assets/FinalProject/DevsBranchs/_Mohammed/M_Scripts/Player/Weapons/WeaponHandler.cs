using System;
using System.Collections.Generic;
using UnityEngine;
using Aegis.Core;
using Aegis.Input;
using Aegis.Weapons;

namespace Aegis.Player
{
    /// <summary>
    /// Manages the player's loadout: which weapons are owned, which one is equipped, and
    /// listening to the InputReader to actually fire. When firing, it spawns the
    /// <see cref="WeaponData.projectilePrefab"/> (your Bullet prefab) at the muzzle and calls
    /// <see cref="Bullet.Launch"/> with the data's speed and damage. Supports Single, Auto,
    /// and Charge fire modes (Beam is stubbed for later).
    /// Tier 2 — depends on Tier 0 (InputReader, WeaponData, AudioManager) + Tier 1 (Bullet).
    /// </summary>
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader _inputReader;

        [Header("Refs")]
        [Tooltip("Base muzzle ANCHOR (usually the camera). Each WeaponData adds its own muzzleOffset on top of this, so different guns spawn bullets at different barrel positions.")]
        [SerializeField] private Transform _muzzle;

        [Header("Loadout")]
        [SerializeField] private List<WeaponData> _startingWeapons = new List<WeaponData>();

        [Header("Faction filter")]
        [Tooltip("Which side this player is — used to decide which weapons can be picked up.")]
        [SerializeField] private Side _playerSide = Side.Good;

        private readonly List<Weapon> _weapons = new List<Weapon>();
        private int _currentIndex = -1;

        public Weapon CurrentWeapon =>
            _currentIndex >= 0 && _currentIndex < _weapons.Count ? _weapons[_currentIndex] : null;
        public Side PlayerSide { get => _playerSide; set => _playerSide = value; }
        public int WeaponCount => _weapons.Count;

        public event Action<Weapon> WeaponEquipped;
        public event Action<Weapon> WeaponFired;
        public event Action<Weapon> AmmoChanged;

        private void Awake()
        {
            foreach (WeaponData data in _startingWeapons)
                if (data != null) AddWeapon(data);

            if (_weapons.Count > 0) EquipIndex(0);
        }

        private void OnEnable()
        {
            if (_inputReader == null) return;
            _inputReader.FireStartedEvent += OnFireStarted;
            _inputReader.FireCanceledEvent += OnFireCanceled;
            _inputReader.SwitchWeaponEvent += OnSwitchWeapon;
            _inputReader.ReloadEvent += Reload;
        }

        private void OnDisable()
        {
            if (_inputReader == null) return;
            _inputReader.FireStartedEvent -= OnFireStarted;
            _inputReader.FireCanceledEvent -= OnFireCanceled;
            _inputReader.SwitchWeaponEvent -= OnSwitchWeapon;
            _inputReader.ReloadEvent -= Reload;
        }

        private void Update()
        {
            CurrentWeapon?.Tick(Time.deltaTime);

            // Auto-fire: keep shooting while the trigger is held.
            if (_inputReader != null && _inputReader.IsFiring && CurrentWeapon != null
                && CurrentWeapon.Data.fireMode == FireMode.Auto
                && CurrentWeapon.IsReady && CurrentWeapon.HasAmmo)
            {
                FireOneShot();
            }
        }

        /// <summary>True if this side is allowed to use this weapon.</summary>
        public bool CanUse(WeaponData data)
        {
            if (data == null) return false;
            if (data.allowed == WeaponAvailability.Both) return true;
            return (data.allowed == WeaponAvailability.Good && _playerSide == Side.Good)
                || (data.allowed == WeaponAvailability.Evil && _playerSide == Side.Evil);
        }

        /// <summary>Add a weapon (e.g. from a pickup). Returns false if not allowed or already owned.</summary>
        public bool AddWeapon(WeaponData data)
        {
            if (!CanUse(data) || HasWeapon(data)) return false;
            _weapons.Add(new Weapon(data));
            if (_currentIndex < 0) EquipIndex(0);
            return true;
        }

        public bool HasWeapon(WeaponData data)
        {
            foreach (Weapon w in _weapons) if (w.Data == data) return true;
            return false;
        }

        public void EquipIndex(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;
            _currentIndex = index;
            WeaponEquipped?.Invoke(CurrentWeapon);
        }

        public void EquipNext() => Cycle(+1);
        public void EquipPrevious() => Cycle(-1);

        /// <summary>Instant reload — refills the current weapon's magazine to its max ammo.</summary>
        public void Reload()
        {
            Weapon w = CurrentWeapon;
            if (w == null || w.Data.ammo < 0) return;       // -1 ammo means "infinite"
            if (w.CurrentAmmo >= w.Data.ammo) return;       // already full

            w.Refill();
            AmmoChanged?.Invoke(w);
        }

        private void Cycle(int direction)
        {
            if (_weapons.Count == 0) return;
            int next = (_currentIndex + direction + _weapons.Count) % _weapons.Count;
            EquipIndex(next);
        }

        // ---- Input handlers ----
        private void OnFireStarted()
        {
            Weapon w = CurrentWeapon;
            if (w == null) return;

            switch (w.Data.fireMode)
            {
                case FireMode.Single:
                    if (w.IsReady && w.HasAmmo) FireOneShot();
                    break;
                case FireMode.Charge:
                    w.StartCharging();
                    break;
                // Auto: handled in Update while button is held.
                // Beam: not yet implemented; would start a beam VFX/damage tick here.
            }
        }

        private void OnFireCanceled()
        {
            Weapon w = CurrentWeapon;
            if (w == null) return;

            if (w.Data.fireMode == FireMode.Charge && w.IsCharging)
            {
                bool fullyCharged = w.ChargeProgress01 >= 1f;
                if (fullyCharged && w.IsReady && w.HasAmmo) FireOneShot();
                else w.StopCharging();
            }
        }

        private void OnSwitchWeapon(float direction)
        {
            if (direction > 0.1f) EquipNext();
            else if (direction < -0.1f) EquipPrevious();
        }

        // ---- Firing ----
        private void FireOneShot()
        {
            Weapon w = CurrentWeapon;
            if (w == null || _muzzle == null) return;
            if (w.Data.projectilePrefab == null)
            {
                Debug.LogWarning($"WeaponHandler: '{w.Data.displayName}' has no projectile prefab assigned.", this);
                return;
            }

            // Resolve this weapon's barrel position: anchor + per-weapon offset (in the anchor's local space).
            Vector3 spawnPos = _muzzle.position + _muzzle.TransformVector(w.Data.muzzleOffset);
            Quaternion spawnRot = _muzzle.rotation;

            // Spawn the Bullet prefab at the resolved barrel, aimed forward.
            GameObject go = Instantiate(w.Data.projectilePrefab, spawnPos, spawnRot);
            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.IgnoreShooter(gameObject); // don't hit our own player capsule when aiming down
                bullet.Launch(w.Data.projectileSpeed, w.Data.damage, null);
            }

            // Optional muzzle flash + sound (also at the resolved barrel).
            if (w.Data.muzzleVFX != null)
                Instantiate(w.Data.muzzleVFX, spawnPos, spawnRot);
            if (w.Data.fireSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(w.Data.fireSFX);

            w.OnFired();
            WeaponFired?.Invoke(w);
            AmmoChanged?.Invoke(w);
        }
    }
}
