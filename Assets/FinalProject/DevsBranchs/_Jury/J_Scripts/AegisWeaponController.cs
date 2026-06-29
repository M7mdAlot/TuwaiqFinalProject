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

    [Header("Animator State Names")]
    public string normalIdleStateName = "Idle normal";
    public string weaponIdleStateName = "idle rifle";

    [Header("Shooting")]
    public float singleShotCooldown = 0.25f;
    public float autoFireRate = 0.12f;
    public float holdToAutoTime = 0.22f;
    public float range = 100f;
    public int damage = 10;

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
        HandleAim();
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
            animator.CrossFade(weaponIdleStateName, 0.08f);
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
            animator.ResetTrigger(closeWeaponParam);

            animator.SetTrigger(closeWeaponParam);
            animator.CrossFade(normalIdleStateName, 0.08f);
        }

        if (movementController != null)
            movementController.SetMovementLocked(false);

        if (weaponObject != null)
            weaponObject.SetActive(false);
    }

    void HandleAim()
    {
        if (animator == null) return;

        bool aiming = hasWeapon && aimAction != null && aimAction.IsPressed();
        animator.SetBool(isAimingParam, aiming);
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

            if (heldTime >= holdToAutoTime)
            {
                isAutoFiring = true;
                animator.SetBool(fireAutoParam, true);

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
        {
            Debug.Log("Aegis shot hit: " + hit.collider.name);
        }
    }

    public void OnSwitchWeapon(InputValue value) { }
    public void OnFire(InputValue value) { }
    public void OnReload(InputValue value) { }
    public void OnAim(InputValue value) { }
}