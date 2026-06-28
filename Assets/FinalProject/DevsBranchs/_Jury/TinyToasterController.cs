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

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Slide")]
    public float slideSpeed = 10f;
    public float slideDuration = 0.65f;
    public float slideCooldown = 1f;

    [Header("Look")]
    public float mouseSensitivity = 0.12f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("Input Action Names")]
    public string moveActionName = "Move";
    public string lookActionName = "Look";
    public string sprintActionName = "Sprint";
    public string jumpActionName = "Jump";
    public string slideActionName = "Slide";

    [Header("Animator Parameters")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string speedParam = "Speed";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string jumpParam = "Jump";
    public string slideParam = "Slide";

    [Header("Animation")]
    public float animationSmoothTime = 0.1f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction slideAction;

    private float verticalVelocity;
    private float cameraPitch;

    private bool isSliding;
    private float slideTimer;
    private float slideCooldownTimer;
    private Vector3 slideDirection;

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
            Debug.LogError("PlayerInput is missing. Add PlayerInput to the Player object.");
            return;
        }

        moveAction = playerInput.actions.FindAction(moveActionName, false);
        lookAction = playerInput.actions.FindAction(lookActionName, false);
        sprintAction = playerInput.actions.FindAction(sprintActionName, false);
        jumpAction = playerInput.actions.FindAction(jumpActionName, false);
        slideAction = playerInput.actions.FindAction(slideActionName, false);
    }

    void OnEnable()
    {
        if (jumpAction != null)
            jumpAction.performed += OnJump;

        if (slideAction != null)
            slideAction.performed += OnSlide;
    }

    void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.performed -= OnJump;

        if (slideAction != null)
            slideAction.performed -= OnSlide;
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (characterController == null) return;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        HandleLook(lookInput);
        HandleMovement(moveInput);
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
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (slideCooldownTimer > 0f)
            slideCooldownTimer -= Time.deltaTime;

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
                isSliding = false;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isRunning = sprintAction != null && sprintAction.IsPressed() && isMoving && !isSliding;

        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 horizontalMove;

        if (isSliding)
            horizontalMove = slideDirection * slideSpeed;
        else
            horizontalMove = moveDirection * currentSpeed;

        characterController.Move(horizontalMove * Time.deltaTime);

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        if (characterController == null) return;
        if (!characterController.isGrounded) return;
        if (isSliding) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (animator != null)
            animator.SetTrigger(jumpParam);
    }

    void OnSlide(InputAction.CallbackContext context)
    {
        if (characterController == null) return;
        if (!characterController.isGrounded) return;
        if (isSliding) return;
        if (slideCooldownTimer > 0f) return;

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (direction.sqrMagnitude < 0.01f)
            direction = transform.forward;

        slideDirection = direction.normalized;

        isSliding = true;
        slideTimer = slideDuration;
        slideCooldownTimer = slideCooldown;

        if (animator != null)
            animator.SetTrigger(slideParam);
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
}