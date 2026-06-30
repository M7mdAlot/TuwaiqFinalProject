using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TinyToasterController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public PlayerInput playerInput;
    public Animator animator;
    public Transform cameraRoot;

    [Header("Input Action Names")]
    public string moveActionName = "Move";
    public string lookActionName = "Look";
    public string sprintActionName = "Sprint";
    public string jumpActionName = "Jump";
    public string slideActionName = "Slide";

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Slide")]
    public bool slideRequiresSprint = false;
    public bool slideRequiresGrounded = false;
    public float slideSpeed = 10f;
    public float slideDuration = 0.8f;
    public float slideCooldown = 1f;

    [Header("Look")]
    public float mouseSensitivity = 0.12f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Animator Parameters")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string speedParam = "Speed";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string isJumpingParam = "IsJumping";
    public string isSlidingParam = "IsSliding";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    [Header("Animation Timing")]
    public float animationSmoothTime = 0.1f;
    public float jumpAnimationTime = 0.6f;

    [Header("State")]
    public bool movementLocked;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction slideAction;

    private float verticalVelocity;
    private float cameraPitch;

    private bool isSliding;
    private bool isJumpAnimating;
    private float slideTimer;
    private float slideCooldownTimer;
    private Vector3 slideDirection;

    private bool isDead;

    void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerInput == null)
        {
            Debug.LogError("TinyToasterController: PlayerInput is missing.");
            return;
        }

        moveAction = playerInput.actions.FindAction(moveActionName, false);
        lookAction = playerInput.actions.FindAction(lookActionName, false);
        sprintAction = playerInput.actions.FindAction(sprintActionName, false);
        jumpAction = playerInput.actions.FindAction(jumpActionName, false);
        slideAction = playerInput.actions.FindAction(slideActionName, false);
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (animator != null)
        {
            animator.SetBool(isJumpingParam, false);
            animator.SetBool(isSlidingParam, false);
            animator.SetBool(isDeadParam, false);
        }
    }

    void Update()
    {
        if (isDead) return;
        if (characterController == null) return;
        if (!characterController.enabled) return;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        if (movementLocked)
            moveInput = Vector2.zero;

        HandleLook(lookInput);
        HandleMovement(moveInput);
        HandleActions();
        UpdateAnimator(moveInput);
    }

    void HandleLook(Vector2 lookInput)
    {
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        if (cameraRoot != null)
        {
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, minLookAngle, maxLookAngle);
            cameraRoot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    void HandleMovement(Vector2 moveInput)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (slideCooldownTimer > 0f)
            slideCooldownTimer -= Time.deltaTime;

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
                StopSlide();
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isRunning = sprintAction != null && sprintAction.IsPressed() && isMoving && !isSliding;

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 horizontalMove = isSliding
            ? slideDirection * slideSpeed
            : moveDirection * currentSpeed;

        characterController.Move(horizontalMove * Time.deltaTime);

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleActions()
    {
        if (movementLocked) return;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
            TryJump();

        if (slideAction != null && slideAction.WasPressedThisFrame())
            TrySlide();
    }

    void TryJump()
    {
        if (isSliding) return;
        if (isJumpAnimating) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        isJumpAnimating = true;

        if (animator != null)
            animator.SetBool(isJumpingParam, true);

        CancelInvoke(nameof(StopJumpAnimation));
        Invoke(nameof(StopJumpAnimation), jumpAnimationTime);
    }

    void StopJumpAnimation()
    {
        isJumpAnimating = false;

        if (animator != null)
            animator.SetBool(isJumpingParam, false);
    }

    void TrySlide()
    {
        if (isSliding) return;
        if (slideCooldownTimer > 0f) return;

        if (slideRequiresGrounded && !characterController.isGrounded)
            return;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isSprinting = sprintAction != null && sprintAction.IsPressed();

        if (!isMoving) return;

        if (slideRequiresSprint && !isSprinting)
            return;

        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward;

        slideDirection = direction.normalized;

        isSliding = true;
        slideTimer = slideDuration;
        slideCooldownTimer = slideCooldown;

        if (animator != null)
            animator.SetBool(isSlidingParam, true);
    }

    void StopSlide()
    {
        isSliding = false;

        if (animator != null)
            animator.SetBool(isSlidingParam, false);
    }

    void UpdateAnimator(Vector2 moveInput)
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isRunning = sprintAction != null && sprintAction.IsPressed() && isMoving && !isSliding;

        animator.SetFloat(moveXParam, moveInput.x, animationSmoothTime, Time.deltaTime);
        animator.SetFloat(moveYParam, moveInput.y, animationSmoothTime, Time.deltaTime);
        animator.SetFloat(speedParam, moveInput.magnitude, animationSmoothTime, Time.deltaTime);

        animator.SetBool(isMovingParam, isMoving);
        animator.SetBool(isRunningParam, isRunning);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
        {
            isSliding = false;
            isJumpAnimating = false;

            if (animator != null)
            {
                animator.SetFloat(moveXParam, 0f);
                animator.SetFloat(moveYParam, 0f);
                animator.SetFloat(speedParam, 0f);

                animator.SetBool(isMovingParam, false);
                animator.SetBool(isRunningParam, false);
                animator.SetBool(isJumpingParam, false);
                animator.SetBool(isSlidingParam, false);
            }
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;

        if (animator != null)
            animator.SetTrigger(damageParam);
    }

    public void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetBool(isDeadParam, true);
    }
}