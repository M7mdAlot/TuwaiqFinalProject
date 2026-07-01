using UnityEngine;
using UnityEngine.InputSystem;

public class AegisWeaponController : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public Camera playerCamera;
    public GameObject weaponObject;
    public TinyToasterController movementController;

    [Header("Input Action Names")]
    public string switchWeaponActionName = "SwitchWeapon";
    public string aimActionName = "Aim";
    public string fireActionName = "Fire";
    public string reloadActionName = "Reload";

    [Header("Weapon")]
    public bool requireWeaponToFire = true;
    public bool lockMovementWhileWeaponEquipped = true;

    [Header("Zoom")]
    public float normalFOV = 60f;
    public float aimFOV = 35f;
    public float zoomSpeed = 12f;

    [Header("Shooting")]
    public float autoStartDelay = 0.18f;
    public float autoFireRate = 0.09f;
    public float range = 100f;
    public int damage = 10;

    [Header("Interaction Blocking")]
    public bool blockInteractionWhileWeaponEquipped = false;
    public bool blockInteractionWhileFiring = true;

    public bool HasWeapon => hasWeapon;
    public bool IsFiring => isFireButtonHeld && (!requireWeaponToFire || hasWeapon);

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
    private bool isFireButtonHeld;
    private bool wasFireButtonHeld;

    private float fireHeldTimer;
    private float nextAutoShotTime;

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (movementController == null)
            movementController = GetComponent<TinyToasterController>();

        if (playerInput == null)
        {
            Debug.LogError("AegisWeaponController: PlayerInput is missing.");
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
        SetWeapon(false);

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleSwitchWeapon();
        HandleZoom();
        HandleFire();
        HandleReload();
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

        isFireButtonHeld = false;
        wasFireButtonHeld = false;
        fireHeldTimer = 0f;

        if (weaponObject != null)
            weaponObject.SetActive(hasWeapon);

        if (movementController != null)
            movementController.SetMovementLocked(lockMovementWhileWeaponEquipped && hasWeapon);

        Debug.Log(hasWeapon ? "Weapon equipped." : "Weapon closed.");
    }

    void HandleZoom()
    {
        bool aiming = hasWeapon && aimAction != null && aimAction.IsPressed();

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
            ResetFire();
            return;
        }

        isFireButtonHeld = fireAction.IsPressed();

        // أول ضغطة = طلقة وحدة فورًا
        if (isFireButtonHeld && !wasFireButtonHeld)
        {
            fireHeldTimer = 0f;
            nextAutoShotTime = Time.time + autoStartDelay;

            ShootRaycast();
        }

        // تعليق الزر = طلق متكرر
        if (isFireButtonHeld)
        {
            fireHeldTimer += Time.deltaTime;

            if (fireHeldTimer >= autoStartDelay && Time.time >= nextAutoShotTime)
            {
                ShootRaycast();
                nextAutoShotTime = Time.time + autoFireRate;
            }
        }

        if (!isFireButtonHeld && wasFireButtonHeld)
            ResetFire();

        wasFireButtonHeld = isFireButtonHeld;
    }

    void ResetFire()
    {
        isFireButtonHeld = false;
        wasFireButtonHeld = false;
        fireHeldTimer = 0f;
    }

    void HandleReload()
    {
        if (reloadAction == null) return;
        if (requireWeaponToFire && !hasWeapon) return;

        if (reloadAction.WasPressedThisFrame())
            Debug.Log("Reload / Refill pressed.");
    }

    void ShootRaycast()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("No camera assigned for shooting.");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
            Debug.Log("Aegis shot hit: " + hit.collider.name);
        else
            Debug.Log("Aegis shot fired.");
    }

    void CheckAction(InputAction action, string actionName)
    {
        if (action == null)
            Debug.LogWarning("AegisWeaponController: Missing Input Action: " + actionName);
    }
}