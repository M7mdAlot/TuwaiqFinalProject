using UnityEngine;
using UnityEngine.InputSystem;

public class AegisWeaponController : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public Animator animator;
    public Camera playerCamera;
    public GameObject weaponObject;
    public TinyToasterController movementController;

    [Header("Input Action Names")]
    public string switchWeaponActionName = "SwitchWeapon";
    public string aimActionName = "Aim";
    public string fireActionName = "Fire";
    public string reloadActionName = "Reload";

    [Header("Animator Parameters")]
    public string hasWeaponParam = "HasWeapon";
    public string isAimingParam = "IsAiming";
    public string summonWeaponParam = "SummonWeapon";
    public string closeWeaponParam = "CloseWeapon";
    public string fireSingleParam = "FireSingle";
    public string fireAutoParam = "FireAuto";
    public string reloadParam = "Refill";

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float aimFOV = 35f;
    public float zoomSpeed = 12f;

    [Header("Shooting")]
    public float singleShotCooldown = 0.2f;
    public float autoFireRate = 0.09f;
    public float autoStartDelay = 0.18f;
    public float range = 100f;
    public int damage = 10;

    [Header("Interaction Blocking")]
    public bool blockInteractionWhileWeaponEquipped = true;

    public bool HasWeapon => hasWeapon;
    public bool IsAiming => hasWeapon && aimAction != null && aimAction.IsPressed();
    public bool IsFiring => hasWeapon && fireAction != null && fireAction.IsPressed();
    public bool BlocksInteraction => blockInteractionWhileWeaponEquipped && hasWeapon;

    private InputAction switchWeaponAction;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction reloadAction;

    private bool hasWeapon;
    private bool isAutoFiring;

    private float firePressedTime;
    private float nextSingleShotTime;
    private float nextAutoShotTime;

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (movementController == null)
            movementController = GetComponent<TinyToasterController>();

        if (playerInput == null)
        {
            Debug.LogError("AegisWeaponController: PlayerInput is missing.");
            return;
        }

        switchWeaponAction = playerInput.actions.FindAction(switchWeaponActionName, false);
        aimAction = playerInput.actions.FindAction(aimActionName, false);
        fireAction = playerInput.actions.FindAction(fireActionName, false);
        reloadAction = playerInput.actions.FindAction(reloadActionName, false);
    }

    void Start()
    {
        hasWeapon = false;
        isAutoFiring = false;

        if (weaponObject != null)
            weaponObject.SetActive(false);

        if (movementController != null)
            movementController.SetMovementLocked(false);

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;

        if (animator != null)
        {
            animator.SetBool(hasWeaponParam, false);
            animator.SetBool(isAimingParam, false);
            animator.SetBool(fireAutoParam, false);
        }
    }

    void Update()
    {
        HandleSwitchWeapon();
        HandleAimAndZoom();
        HandleFire();
        HandleReload();
    }

    void HandleSwitchWeapon()
    {
        if (switchWeaponAction == null) return;

        if (switchWeaponAction.WasPressedThisFrame())
        {
            if (hasWeapon)
                CloseWeapon();
            else
                SummonWeapon();
        }
    }

    void SummonWeapon()
    {
        hasWeapon = true;
        isAutoFiring = false;

        if (weaponObject != null)
            weaponObject.SetActive(true);

        if (movementController != null)
            movementController.SetMovementLocked(true);

        if (animator != null)
        {
            animator.SetBool(hasWeaponParam, true);
            animator.SetBool(isAimingParam, false);
            animator.SetBool(fireAutoParam, false);

            animator.ResetTrigger(closeWeaponParam);
            animator.ResetTrigger(fireSingleParam);
            animator.ResetTrigger(reloadParam);

            animator.SetTrigger(summonWeaponParam);
        }
    }

    void CloseWeapon()
    {
        hasWeapon = false;
        isAutoFiring = false;

        if (animator != null)
        {
            animator.SetBool(hasWeaponParam, false);
            animator.SetBool(isAimingParam, false);
            animator.SetBool(fireAutoParam, false);

            animator.ResetTrigger(fireSingleParam);
            animator.ResetTrigger(reloadParam);
            animator.ResetTrigger(summonWeaponParam);
            animator.SetTrigger(closeWeaponParam);
        }

        if (movementController != null)
            movementController.SetMovementLocked(false);

        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    void HandleAimAndZoom()
    {
        bool aiming = hasWeapon && aimAction != null && aimAction.IsPressed();

        if (animator != null)
            animator.SetBool(isAimingParam, aiming);

        if (playerCamera != null)
        {
            float targetFOV = aiming ? aimFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * zoomSpeed
            );
        }
    }

    void HandleFire()
    {
        if (!hasWeapon) return;
        if (fireAction == null) return;
        if (animator == null) return;

        if (fireAction.WasPressedThisFrame())
        {
            firePressedTime = Time.time;
            isAutoFiring = false;
            nextAutoShotTime = Time.time + autoStartDelay;

            animator.SetBool(fireAutoParam, false);

            if (Time.time >= nextSingleShotTime)
            {
                animator.ResetTrigger(fireSingleParam);
                animator.SetTrigger(fireSingleParam);

                ShootRaycast();

                nextSingleShotTime = Time.time + singleShotCooldown;
            }
        }

        if (fireAction.IsPressed())
        {
            float heldTime = Time.time - firePressedTime;

            if (heldTime >= autoStartDelay)
            {
                if (!isAutoFiring)
                {
                    isAutoFiring = true;
                    animator.SetBool(fireAutoParam, true);
                    nextAutoShotTime = Time.time;
                }

                if (Time.time >= nextAutoShotTime)
                {
                    ShootRaycast();
                    nextAutoShotTime = Time.time + autoFireRate;
                }
            }
        }

        if (fireAction.WasReleasedThisFrame())
        {
            isAutoFiring = false;
            animator.SetBool(fireAutoParam, false);
        }
    }

    void HandleReload()
    {
        if (!hasWeapon) return;
        if (reloadAction == null) return;
        if (animator == null) return;

        if (reloadAction.WasPressedThisFrame())
        {
            isAutoFiring = false;
            animator.SetBool(fireAutoParam, false);
            animator.SetTrigger(reloadParam);
        }
    }

    void ShootRaycast()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
            Debug.Log("Aegis shot hit: " + hit.collider.name);
        else
            Debug.Log("Aegis shot fired.");
    }
}