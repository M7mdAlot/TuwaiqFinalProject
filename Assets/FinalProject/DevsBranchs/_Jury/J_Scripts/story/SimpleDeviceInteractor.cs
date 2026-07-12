using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Aegis.Systems;

// Bulletproof "hold F to shut down the device" — completely bypasses Mohammed's
// InteractionController. Put it on the PLAYER. It raycasts from the camera, works on TRIGGER
// or SOLID colliders, shows the prompt, fills the LOAD bar, and calls InteractableDevice.Interact()
// when you hold F long enough.
//
// NOTE: if you use this, DISABLE the InteractionUIBridge component so the two don't fight over
// the prompt (this one drives the same UI by name).
public class SimpleDeviceInteractor : MonoBehaviour
{
    [Header("Aim")]
    [Tooltip("Left empty = uses the main camera automatically.")]
    public Camera cam;
    public float range = 5f;
    public Key useKey = Key.F;

    [Header("UI (auto-found by name)")]
    public GameObject promptRoot;   // the "press F to interact" object
    public TMP_Text promptText;
    public Image loadFill;          // "LOAD FILL" (forced to Filled type)

    private float _hold;

    void Update()
    {
        // Prefer THIS player's own camera, then the tagged main camera, then any camera.
        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        // Raycast from the camera. QueryTriggerInteraction.Collide = hits triggers too (forgiving).
        InteractableDevice device = null;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                             out RaycastHit hit, range, ~0, QueryTriggerInteraction.Collide))
            device = hit.collider.GetComponentInParent<InteractableDevice>();

        // Force it usable so the crisis on/off gating can never block you.
        if (device != null && !device.CanInteract) device.SetEnabled(true);

        bool canUse = device != null;

        // Hook up the UI (auto-find once).
        if (promptRoot == null) promptRoot = FindByNameContains("press");
        if (promptText == null && promptRoot != null) promptText = promptRoot.GetComponentInChildren<TMP_Text>(true);
        if (loadFill == null) { GameObject g = FindByName("LOAD FILL"); if (g != null) loadFill = g.GetComponent<Image>(); }

        if (promptRoot != null && promptRoot.activeSelf != canUse) promptRoot.SetActive(canUse);
        if (canUse && promptText != null) promptText.text = device.Prompt;

        if (canUse && Keyboard.current != null && Keyboard.current[useKey].isPressed)
        {
            _hold += Time.deltaTime;
            float p = device.HoldDuration > 0f ? Mathf.Clamp01(_hold / device.HoldDuration) : 1f;
            if (loadFill != null) { loadFill.type = Image.Type.Filled; loadFill.fillAmount = p; }

            if (_hold >= device.HoldDuration)
            {
                Debug.Log("SimpleDeviceInteractor: HELD F -> shutting down '" + device.name + "'.", this);
                device.Interact(gameObject);   // flips CanInteract=false -> CrisisManager sees success

                // Directly fire the good ending too, so it works even if no crisis events are wired.
                CrisisEndingLink link = FindFirstObjectByType<CrisisEndingLink>();
                if (link != null) link.ForceGoodEnding();

                _hold = 0f;
                if (loadFill != null) loadFill.fillAmount = 0f;
            }
        }
        else
        {
            _hold = 0f;
            if (!canUse && loadFill != null) loadFill.fillAmount = 0f;
        }
    }

    GameObject FindByName(string n)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == n) return t.gameObject;
        return null;
    }

    GameObject FindByNameContains(string s)
    {
        s = s.ToLowerInvariant();
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.gameObject != gameObject && t.name.ToLowerInvariant().Contains(s)) return t.gameObject;
        return null;
    }
}
