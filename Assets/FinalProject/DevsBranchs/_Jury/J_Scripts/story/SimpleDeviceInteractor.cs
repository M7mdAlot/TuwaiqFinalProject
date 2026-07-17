using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Aegis.Systems;

// Dead-simple, AIM-FREE "hold F near the device to shut it down". Put it on the PLAYER.
// No raycast, no aiming: it finds the NEAREST InteractableDevice within range, shows the prompt,
// fills the LOAD bar, defuses on hold, and fires the good ending directly.
//
// If you use this, DISABLE the InteractionUIBridge component so they don't fight over the prompt.
public class SimpleDeviceInteractor : MonoBehaviour
{
    [Header("Just walk within this distance and hold the key")]
    public float range = 4f;
    public Key useKey = Key.F;

    [Header("UI (auto-found by name)")]
    public GameObject promptRoot;
    public TMP_Text promptText;
    public Image loadFill;

    private float _hold;
    private InteractableDevice _defused; // the device we personally shut down — never re-open it

    void Start()
    {
        int devices = FindObjectsByType<InteractableDevice>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        Debug.Log("SimpleDeviceInteractor: RUNNING on '" + name + "'. InteractableDevices in scene = " +
                  devices + ". Walk within " + range + "m of one and HOLD " + useKey + ".", this);
    }

    void Update()
    {
        InteractableDevice device = NearestDevice(out float dist);

        // Force it usable so the crisis on/off gating can never block you — but NEVER re-open the
        // device we just defused. Forcing it back on every frame raced CrisisManager's own check
        // for CanInteract==false, so if CrisisManager didn't happen to poll in that exact frame,
        // it never saw the device as defused and the countdown never stopped.
        if (device != null && !device.CanInteract && device != _defused) device.SetEnabled(true);

        bool near = device != null;

        // Hook up the UI (auto-find).
        if (promptRoot == null) promptRoot = FindByNameContains("press");
        if (promptText == null && promptRoot != null) promptText = promptRoot.GetComponentInChildren<TMP_Text>(true);
        if (loadFill == null) { GameObject g = FindByName("LOAD FILL"); if (g != null) loadFill = g.GetComponent<Image>(); }

        if (promptRoot != null && promptRoot.activeSelf != near) promptRoot.SetActive(near);
        if (near && promptText != null) promptText.text = device.Prompt;

        if (near && Keyboard.current != null && Keyboard.current[useKey].isPressed)
        {
            _hold += Time.deltaTime;
            float p = device.HoldDuration > 0f ? Mathf.Clamp01(_hold / device.HoldDuration) : 1f;
            if (loadFill != null) { loadFill.type = Image.Type.Filled; loadFill.fillAmount = p; }

            if (_hold >= device.HoldDuration)
            {
                Debug.Log("SimpleDeviceInteractor: DONE -> shut down '" + device.name + "'.", this);
                device.Interact(gameObject);
                _defused = device;

                // Fire the good ending directly, so it works even if no crisis events are wired.
                CrisisEndingLink link = FindFirstObjectByType<CrisisEndingLink>();
                if (link != null) link.ForceGoodEnding();

                _hold = 0f;
                if (loadFill != null) loadFill.fillAmount = 0f;
            }
        }
        else
        {
            _hold = 0f;
            if (!near && loadFill != null) loadFill.fillAmount = 0f;
        }
    }

    InteractableDevice NearestDevice(out float bestDist)
    {
        InteractableDevice best = null;
        bestDist = range;
        foreach (InteractableDevice d in FindObjectsByType<InteractableDevice>(FindObjectsSortMode.None))
        {
            if (d == null) continue;
            float dist = Vector3.Distance(transform.position, d.transform.position);
            if (dist <= bestDist) { bestDist = dist; best = d; }
        }
        return best;
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
