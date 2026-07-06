using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aegis.Input
{
    /// <summary>
    /// ScriptableObject wrapper over the New Input System (GDD §8). Reads the
    /// "Gameplay" action map from an assigned <see cref="InputActionAsset"/> and
    /// exposes designer-friendly values + events, so gameplay scripts never touch
    /// raw input devices. Supports both Keyboard&amp;Mouse and Gamepad control schemes.
    ///
    /// Create the asset via Assets ▸ Create ▸ AEGIS ▸ Input ▸ Input Reader, then
    /// drag the AegisControls.inputactions asset onto the Actions field.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Input/Input Reader", fileName = "InputReader")]
    public class InputReader : ScriptableObject
    {
        [Tooltip("Assign the AegisControls .inputactions asset.")]
        [SerializeField] private InputActionAsset _actions;

        // ---- Continuously-polled values (read every frame by movement/camera) ----
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsFiring { get; private set; }
        public bool IsInteractHeld { get; private set; }

        // ---- One-shot / edge events ----
        public event Action JumpEvent;
        public event Action SlideEvent;
        public event Action FireStartedEvent;
        public event Action FireCanceledEvent;
        public event Action InteractEvent;          // fired on a single press (taps)
        public event Action InteractStartedEvent;   // fired when the button goes down
        public event Action InteractCanceledEvent;  // fired when the button is released

        /// <summary>Weapon cycle direction: positive = next, negative = previous.</summary>
        public event Action<float> SwitchWeaponEvent;

        /// <summary>Direct slot selection (1..4) via the number keys.</summary>
        public event Action<int> EquipSlotEvent;

        public event Action ReloadEvent;
        public event Action MeleeEvent;

        private const string GameplayMapName = "Gameplay";

        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _slide;
        private InputAction _jump;
        private InputAction _fire;
        private InputAction _interact;
        private InputAction _switchWeapon;
        private InputAction _reload;
        private InputAction _equipSlot;
        private InputAction _melee;

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError($"{nameof(InputReader)}: no InputActionAsset assigned.", this);
                return;
            }

            InputActionMap map = _actions.FindActionMap(GameplayMapName, throwIfNotFound: true);
            _move = map.FindAction("Move", throwIfNotFound: true);
            _look = map.FindAction("Look", throwIfNotFound: true);
            _sprint = map.FindAction("Sprint", throwIfNotFound: true);
            _slide = map.FindAction("Slide", throwIfNotFound: true);
            _jump = map.FindAction("Jump", throwIfNotFound: true);
            _fire = map.FindAction("Fire", throwIfNotFound: true);
            _interact = map.FindAction("Interact", throwIfNotFound: true);
            _switchWeapon = map.FindAction("SwitchWeapon", throwIfNotFound: true);
            _reload = map.FindAction("Reload", throwIfNotFound: true);
            _equipSlot = map.FindAction("EquipSlot", throwIfNotFound: true);
            _melee = map.FindAction("Melee", throwIfNotFound: true);

            _move.performed += OnMove;
            _move.canceled += OnMove;
            _look.performed += OnLook;
            _look.canceled += OnLook;
            _sprint.performed += OnSprint;
            _sprint.canceled += OnSprint;
            _slide.performed += OnSlide;
            _jump.performed += OnJump;
            _fire.started += OnFireStarted;
            _fire.canceled += OnFireCanceled;
            _interact.performed += OnInteract;
            _interact.started += OnInteractStarted;
            _interact.canceled += OnInteractCanceled;
            _switchWeapon.performed += OnSwitchWeapon;
            _reload.performed += OnReload;
            _equipSlot.performed += OnEquipSlot;
            _melee.performed += OnMelee;

            EnableGameplay();
        }

        private void OnDisable()
        {
            if (_actions == null) return;

            _move.performed -= OnMove;
            _move.canceled -= OnMove;
            _look.performed -= OnLook;
            _look.canceled -= OnLook;
            _sprint.performed -= OnSprint;
            _sprint.canceled -= OnSprint;
            _slide.performed -= OnSlide;
            _jump.performed -= OnJump;
            _fire.started -= OnFireStarted;
            _fire.canceled -= OnFireCanceled;
            _interact.performed -= OnInteract;
            _interact.started -= OnInteractStarted;
            _interact.canceled -= OnInteractCanceled;
            _switchWeapon.performed -= OnSwitchWeapon;
            _reload.performed -= OnReload;
            _equipSlot.performed -= OnEquipSlot;
            _melee.performed -= OnMelee;

            DisableGameplay();
        }

        /// <summary>Enable gameplay input (call when entering the campaign / play state).</summary>
        public void EnableGameplay() => _actions?.FindActionMap(GameplayMapName)?.Enable();

        /// <summary>Disable gameplay input (call for menus, cutscenes, pause) and zero current values.</summary>
        public void DisableGameplay()
        {
            _actions?.FindActionMap(GameplayMapName)?.Disable();
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            IsSprinting = false;
            IsFiring = false;
        }

        private void OnMove(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        private void OnLook(InputAction.CallbackContext ctx) => LookInput = ctx.ReadValue<Vector2>();
        private void OnSprint(InputAction.CallbackContext ctx) => IsSprinting = ctx.ReadValueAsButton();
        private void OnSlide(InputAction.CallbackContext ctx) => SlideEvent?.Invoke();
        private void OnJump(InputAction.CallbackContext ctx) => JumpEvent?.Invoke();

        private void OnFireStarted(InputAction.CallbackContext ctx)
        {
            IsFiring = true;
            FireStartedEvent?.Invoke();
        }

        private void OnFireCanceled(InputAction.CallbackContext ctx)
        {
            IsFiring = false;
            FireCanceledEvent?.Invoke();
        }

        private void OnInteract(InputAction.CallbackContext ctx) => InteractEvent?.Invoke();

        private void OnInteractStarted(InputAction.CallbackContext ctx)
        {
            IsInteractHeld = true;
            InteractStartedEvent?.Invoke();
        }

        private void OnInteractCanceled(InputAction.CallbackContext ctx)
        {
            IsInteractHeld = false;
            InteractCanceledEvent?.Invoke();
        }

        private void OnSwitchWeapon(InputAction.CallbackContext ctx) => SwitchWeaponEvent?.Invoke(ctx.ReadValue<float>());
        private void OnReload(InputAction.CallbackContext ctx) => ReloadEvent?.Invoke();
        private void OnMelee(InputAction.CallbackContext ctx) => MeleeEvent?.Invoke();

        private void OnEquipSlot(InputAction.CallbackContext ctx)
        {
            // The scale processor makes key 1 → 1, key 2 → 2, etc. 0 = "released", ignore.
            int slot = Mathf.RoundToInt(ctx.ReadValue<float>());
            if (slot > 0) EquipSlotEvent?.Invoke(slot);
        }
    }
}
