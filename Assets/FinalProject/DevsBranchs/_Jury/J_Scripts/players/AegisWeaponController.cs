using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class AegisWeaponController : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public Camera playerCamera;
    public GameObject weaponObject;
    public TinyToasterController movementController;
    public Animator animator;

    [Header("Input Action Names")]
    public string switchWeaponActionName = "SwitchWeapon";
    public string aimActionName = "Aim";
    public string fireActionName = "Fire";
    public string reloadActionName = "Reload";

    [Header("Animator Parameters")]
    public string hasWeaponParam = "HasWeapon";
    public string isAimingParam = "IsAiming";
    public string fireSingleParam = "FireSingle";
    public string fireAutoParam = "FireAuto";
    public string reloadParam = "Refill";

    [Header("Weapon Settings")]
    public bool requireWeaponToFire = true;
    public bool lockMovementWhileWeaponEquipped = true;

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float aimFOV = 35f;
    public float zoomSpeed = 12f;

    [Header("Shooting")]
    public float firstShotCooldown = 0.12f;
    public float autoFireRate = 0.09f;
    public float autoStartDelay = 0.18f;
    public float range = 100f;
    public int damage = 10;

    [Header("Interaction Blocking")]
    public bool blockInteractionWhileFiring = true;
    public bool blockInteractionWhileWeaponEquipped = false;

    [Header("Ammo")]
    public int magazineSize = 30;
    public string reloadStateName = "refill";
    public float reloadDuration = 5f; // safety fallback only, see IsReloadAnimationDone
    public UnityEvent onAmmoChanged;

    [Header("Debug")]
    public bool debugLogs = true;

    public bool HasWeapon => hasWeapon;
    public bool IsFiring => hasWeapon && fireAction != null && fireAction.IsPressed();
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;

    public bool BlocksInteraction
    {
        get
        {
            if (blockInteractionWhileWeaponEquipped && hasWeapon)
                return true;

            if (blockInteractionWhileFiring && IsFiring)
                return true;

            return false;
        }
    }

    private InputAction switchWeaponAction;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction reloadAction;

    private bool hasWeapon;
    private bool wasFirePressed;
    private bool autoFireActive;

    private float firePressedTime;
    private float nextShotTime;

    private int currentAmmo;
    private bool isReloading;
    private bool hasEnteredReloadState;
    private float reloadEndTime;

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (movementController == null)
            movementController = GetComponent<TinyToasterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerInput == null)
        {
            Debug.LogError("AegisWeaponController: Missing PlayerInput.");
            enabled = false;
            return;
        }

        switchWeaponAction = playerInput.actions.FindAction(switchWeaponActionName, false);
        aimAction = playerInput.actions.FindAction(aimActionName, false);
        fireAction = playerInput.actions.FindAction(fireActionName, false);
        reloadAction = playerInput.actions.FindAction(reloadActionName, false);

        CheckAction(switchWeaponAction, switchWeaponActionName);
        CheckAction(aimAction, aimActionName);
        CheckAction(fireAction, fireActionName);
        CheckAction(reloadAction, reloadActionName);
    }

    void Start()
    {
        ForceCloseWeapon();

        currentAmmo = magazineSize;
        onAmmoChanged?.Invoke();

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleSwitchWeapon();
        HandleZoomAndAim();
        HandleFire();
        HandleReload();
        UpdateReloadState();
    }

    void HandleSwitchWeapon()
    {
        if (switchWeaponAction == null) return;

        if (switchWeaponAction.WasPressedThisFrame())
            SetWeapon(!hasWeapon);
    }

    void SetWeapon(bool equipped)
    {
        hasWeapon = equipped;
        wasFirePressed = false;
        autoFireActive = false;
        firePressedTime = 0f;
        nextShotTime = 0f;

        if (weaponObject != null)
            weaponObject.SetActive(hasWeapon);

        if (movementController != null)
            movementController.SetMovementLocked(lockMovementWhileWeaponEquipped && hasWeapon);

        if (animator != null)
        {
            animator.SetBool(hasWeaponParam, hasWeapon);
            animator.SetBool(isAimingParam, false);
            animator.SetBool(fireAutoParam, false);
            animator.ResetTrigger(fireSingleParam);
            animator.ResetTrigger(reloadParam);
        }

        if (debugLogs)
            Debug.Log(hasWeapon ? "WEAPON EQUIPPED" : "WEAPON CLOSED");
    }

    void ForceCloseWeapon()
    {
        hasWeapon = false;
        wasFirePressed = false;
        autoFireActive = false;

        if (weaponObject != null)
            weaponObject.SetActive(false);

        if (movementController != null)
            movementController.SetMovementLocked(false);

        if (animator != null)
        {
            animator.SetBool(hasWeaponParam, false);
            animator.SetBool(isAimingParam, false);
            animator.SetBool(fireAutoParam, false);
            animator.ResetTrigger(fireSingleParam);
            animator.ResetTrigger(reloadParam);
        }
    }

    void HandleZoomAndAim()
    {
        bool aiming = hasWeapon && aimAction != null && aimAction.IsPressed();

        if (animator != null)
            animator.SetBool(isAimingParam, aiming);

        if (playerCamera == null) return;

        float targetFOV = aiming ? aimFOV : normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    void HandleFire()
    {
        if (fireAction == null) return;

        if (requireWeaponToFire && !hasWeapon)
        {
            StopFiringAnimation();
            wasFirePressed = false;
            return;
        }

        if (isReloading || currentAmmo <= 0)
        {
            StopFiringAnimation();
            wasFirePressed = false;
            return;
        }

        bool firePressed = fireAction.IsPressed();

        // First click / first frame
        if (firePressed && !wasFirePressed)
        {
            firePressedTime = Time.time;
            autoFireActive = false;
            nextShotTime = Time.time + firstShotCooldown;

            if (animator != null)
            {
                animator.SetBool(fireAutoParam, false);
                animator.ResetTrigger(fireSingleParam);
                animator.SetTrigger(fireSingleParam);
            }

            ShootRaycast();
        }

        // Hold fire = auto fire after delay
        if (firePressed)
        {
            float heldTime = Time.time - firePressedTime;

            if (heldTime >= autoStartDelay)
            {
                if (!autoFireActive)
                {
                    autoFireActive = true;

                    if (animator != null)
                        animator.SetBool(fireAutoParam, true);

                    nextShotTime = Time.time;
                }

                if (Time.time >= nextShotTime && currentAmmo > 0)
                {
                    ShootRaycast();
                    nextShotTime = Time.time + autoFireRate;

                    if (currentAmmo <= 0)
                        StopFiringAnimation();
                }
            }
        }

        // Release
        if (!firePressed && wasFirePressed)
            StopFiringAnimation();

        wasFirePressed = firePressed;
    }

    void StopFiringAnimation()
    {
        autoFireActive = false;

        if (animator != null)
            animator.SetBool(fireAutoParam, false);
    }

    void HandleReload()
    {
        if (reloadAction == null) return;
        if (requireWeaponToFire && !hasWeapon) return;
        if (isReloading) return;
        if (currentAmmo >= magazineSize) return;

        if (reloadAction.WasPressedThisFrame())
        {
            StopFiringAnimation();

            isReloading = true;
            hasEnteredReloadState = false;
            reloadEndTime = Time.time + reloadDuration;

            if (animator != null)
                animator.SetTrigger(reloadParam);

            Debug.Log("RELOAD PRESSED");
        }
    }

    void UpdateReloadState()
    {
        if (!isReloading) return;

        if (IsReloadAnimationDone() || Time.time >= reloadEndTime)
        {
            isReloading = false;
            currentAmmo = magazineSize;
            onAmmoChanged?.Invoke();
        }
    }

    bool IsReloadAnimationDone()
    {
        if (animator == null || string.IsNullOrEmpty(reloadStateName))
            return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!hasEnteredReloadState)
        {
            // Wait for the trigger to actually take effect before watching for it to end,
            // otherwise we'd read the previous (still-active) state and think it's "done" instantly.
            if (stateInfo.IsName(reloadStateName))
                hasEnteredReloadState = true;

            return false;
        }

        return !stateInfo.IsName(reloadStateName);
    }

    void ShootRaycast()
    {
        currentAmmo = Mathf.Max(0, currentAmmo - 1);
        onAmmoChanged?.Invoke();

        if (playerCamera == null)
        {
            Debug.LogWarning("AegisWeaponController: No camera assigned.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
            Debug.Log("SHOT HIT: " + hit.collider.name);
        else
            Debug.Log("SHOT FIRED");
    }

    void CheckAction(InputAction action, string actionName)
    {
        if (action == null)
            Debug.LogError("AegisWeaponController: Missing Input Action: " + actionName);
    }
}