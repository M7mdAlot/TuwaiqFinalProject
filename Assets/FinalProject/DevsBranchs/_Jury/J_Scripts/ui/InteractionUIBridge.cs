using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Player;
using Aegis.Core;

// Drives YOUR interaction UI (the "press e to interact" prompt + LOAD bar) from Mohammed's
// InteractionController — so your own HUD shows instead of his on-screen debug overlay.
// Auto-finds everything by name; just drop it on a UI object. (Disable his InteractionDebugHud.)
//
// Also self-heals: Mohammed's InteractionController raycasts with QueryTriggerInteraction.Ignore
// and its _rayOrigin often isn't wired to the camera, so it can silently miss devices (e.g. the
// EMP bomb) no matter how you aim. The first time we find the player, we attach a
// SimpleDeviceInteractor to it (the aim-free "walk within range and hold F" interactor already
// proven on the Aegis reactor) and hand this bridge's UI job off to it, so devices always work.
public class InteractionUIBridge : MonoBehaviour
{
    [Header("Optional — all auto-found by name")]
    public InteractionController interaction;
    public GameObject promptRoot;   // auto-finds "press e to interact"
    public TMP_Text promptText;
    public Image loadFill;          // auto-finds "LOAD FILL" (Image Type = Filled)

    private bool _autoUpgraded;

    void Start()
    {
        GameObject found = FindPromptRoot();
        Debug.Log("InteractionUIBridge: InteractionController found=" + (FindFirstObjectByType<InteractionController>() != null)
            + ", prompt object found=" + (found != null ? "'" + found.name + "'" : "NONE")
            + ", 'LOAD FILL' found=" + (FindByName("LOAD FILL") != null), this);

        if (promptRoot != null) promptRoot.SetActive(false);
    }

    void Update()
    {
        if (interaction == null) interaction = FindFirstObjectByType<InteractionController>();

        if (!_autoUpgraded && interaction != null)
        {
            _autoUpgraded = true;

            if (interaction.GetComponent<SimpleDeviceInteractor>() == null)
            {
                interaction.gameObject.AddComponent<SimpleDeviceInteractor>();
                Debug.Log("InteractionUIBridge: added SimpleDeviceInteractor to '" + interaction.name +
                          "' automatically (aim-free hold-to-interact), so devices work even when " +
                          "Mohammed's raycast misses them.", this);
            }

            // Hand the same prompt/LOAD FILL UI off entirely so the two don't fight over it.
            enabled = false;
            return;
        }

        if (promptRoot == null) promptRoot = FindPromptRoot();
        if (promptText == null && promptRoot != null)
            promptText = promptRoot.GetComponentInChildren<TMP_Text>(true);
        if (loadFill == null)
        {
            GameObject go = FindByName("LOAD FILL");
            if (go != null) loadFill = go.GetComponent<Image>();
        }

        IInteractable target = interaction != null ? interaction.CurrentTarget : null;
        bool has = target != null;

        if (promptRoot != null && promptRoot.activeSelf != has)
            promptRoot.SetActive(has);

        if (has)
        {
            if (promptText != null) promptText.text = target.Prompt;

            if (loadFill != null)
            {
                // Show + drive the fill bar while looking at a hold-style device.
                if (!loadFill.gameObject.activeSelf) loadFill.gameObject.SetActive(true);
                loadFill.type = Image.Type.Filled;          // ensure fillAmount actually shows
                loadFill.fillAmount = interaction.HoldProgress01;
            }
        }
        else if (loadFill != null)
        {
            loadFill.fillAmount = 0f;
        }
    }

    GameObject FindByName(string n)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all) if (t.name == n) return t.gameObject;
        return null;
    }

    // Flexible: exact name first, then anything named like a "press …" prompt, then anything
    // containing "interact" (so it works even if the object isn't named exactly right).
    GameObject FindPromptRoot()
    {
        GameObject g = FindByName("press e to interact");
        if (g != null) return g;

        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all)
        {
            if (t.gameObject == gameObject) continue;
            if (t.name.ToLowerInvariant().Contains("press")) return t.gameObject;
        }
        foreach (Transform t in all)
        {
            if (t.gameObject == gameObject) continue;
            if (t.GetComponent<InteractionController>() != null) continue;
            if (t.name.ToLowerInvariant().Contains("interact")) return t.gameObject;
        }
        return null;
    }
}
