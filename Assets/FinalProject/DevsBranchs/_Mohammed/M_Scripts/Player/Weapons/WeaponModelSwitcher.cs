using System;
using System.Collections.Generic;
using UnityEngine;
using Aegis.Weapons;

namespace Aegis.Player
{
    /// <summary>
    /// Shows the model of the currently-equipped weapon and hides all the others.
    /// Fill in one entry per weapon: pair its <see cref="WeaponData"/> with the model
    /// GameObject in the scene (usually a child of a WeaponSocket under the camera).
    /// Listens to <see cref="WeaponHandler.WeaponEquipped"/> so switching weapons swaps
    /// the visible model automatically.
    /// Tier 2 — depends on Tier 2 WeaponHandler + WeaponData.
    /// </summary>
    public class WeaponModelSwitcher : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find a WeaponHandler in the parent hierarchy.")]
        [SerializeField] private WeaponHandler _handler;
        [SerializeField] private List<Entry> _models = new List<Entry>();

        [Serializable]
        public class Entry
        {
            public WeaponData data;
            public GameObject model;
        }

        private void Awake()
        {
            if (_handler == null) _handler = GetComponentInParent<WeaponHandler>();
        }

        private void OnEnable()
        {
            if (_handler == null) return;
            _handler.WeaponEquipped += OnEquipped;
            OnEquipped(_handler.CurrentWeapon); // sync on start
        }

        private void OnDisable()
        {
            if (_handler == null) return;
            _handler.WeaponEquipped -= OnEquipped;
        }

        private void OnEquipped(Weapon current)
        {
            WeaponData currentData = current != null ? current.Data : null;
            foreach (Entry entry in _models)
            {
                if (entry.model == null) continue;
                bool shouldShow = currentData != null && entry.data == currentData;
                if (entry.model.activeSelf != shouldShow) entry.model.SetActive(shouldShow);
            }
        }
    }
}
