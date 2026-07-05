using UnityEngine;
using UnityEngine.InputSystem;

public class XPunchyHands : MonoBehaviour
{
    [Header("References")]
    public PlayerInput playerInput;
    public Animator animator;

    [Header("Input Action Names")]
    public string leftPunchActionName = "LeftPunch";
    public string rightPunchActionName = "RightPunch";
    public string heavyPunchActionName = "HeavyPunch";
    public string kickActionName = "Kick";

    [Header("Animator Trigger Names")]
    public string leftPunchTrigger = "LeftPunch";
    public string rightPunchTrigger = "RightPunch";
    public string heavyPunchTrigger = "HeavyPunch";
    public string kickTrigger = "Kick";

    [Header("Settings")]
    public float punchCooldown = 0.35f;
    public float heavyCooldown = 0.8f;
    public float kickCooldown = 0.7f;

    private InputAction leftPunchAction;
    private InputAction rightPunchAction;
    private InputAction heavyPunchAction;
    private InputAction kickAction;

    private float nextAttackTime;

    void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerInput == null) return;

        leftPunchAction = playerInput.actions.FindAction(leftPunchActionName, false);
        rightPunchAction = playerInput.actions.FindAction(rightPunchActionName, false);
        heavyPunchAction = playerInput.actions.FindAction(heavyPunchActionName, false);
        kickAction = playerInput.actions.FindAction(kickActionName, false);
    }

    void OnEnable()
    {
        if (leftPunchAction != null)
            leftPunchAction.performed += OnLeftPunch;

        if (rightPunchAction != null)
            rightPunchAction.performed += OnRightPunch;

        if (heavyPunchAction != null)
            heavyPunchAction.performed += OnHeavyPunch;

        if (kickAction != null)
            kickAction.performed += OnKick;
    }

    void OnDisable()
    {
        if (leftPunchAction != null)
            leftPunchAction.performed -= OnLeftPunch;

        if (rightPunchAction != null)
            rightPunchAction.performed -= OnRightPunch;

        if (heavyPunchAction != null)
            heavyPunchAction.performed -= OnHeavyPunch;

        if (kickAction != null)
            kickAction.performed -= OnKick;
    }

    void OnLeftPunch(InputAction.CallbackContext context)
    {
        TryAttack(leftPunchTrigger, punchCooldown);
    }

    void OnRightPunch(InputAction.CallbackContext context)
    {
        TryAttack(rightPunchTrigger, punchCooldown);
    }

    void OnHeavyPunch(InputAction.CallbackContext context)
    {
        TryAttack(heavyPunchTrigger, heavyCooldown);
    }

    void OnKick(InputAction.CallbackContext context)
    {
        TryAttack(kickTrigger, kickCooldown);
    }

    void TryAttack(string triggerName, float cooldown)
    {
        if (animator == null) return;
        if (Time.time < nextAttackTime) return;

        animator.ResetTrigger(leftPunchTrigger);
        animator.ResetTrigger(rightPunchTrigger);
        animator.ResetTrigger(heavyPunchTrigger);
        animator.ResetTrigger(kickTrigger);

        animator.SetTrigger(triggerName);

        nextAttackTime = Time.time + cooldown;
    }
}