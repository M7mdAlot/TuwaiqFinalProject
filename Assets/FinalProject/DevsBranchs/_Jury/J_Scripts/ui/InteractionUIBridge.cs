using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Player;
using Aegis.Core;

// Drives YOUR interaction UI (the "press e to interact" prompt + LOAD bar) from Mohammed's
// InteractionController — so your own HUD shows instead of his on-screen debug overlay.
// Auto-finds everything by name; just drop it on a UI object. (Disable his InteractionDebugHud.)
public class InteractionUIBridge : MonoBehaviour
{
    [Header("Optional — all auto-found by name")]
    public InteractionController interaction;
    public GameObject promptRoot;   // auto-finds "press e to interact"
    public TMP_Text promptText;
    public Image loadFill;          // auto-finds "LOAD FILL" (Image Type = Filled)

    void Start()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    void Update()
    {
        if (interaction == null) interaction = FindFirstObjectByType<InteractionController>();

        if (promptRoot == null) promptRoot = FindByName("press e to interact");
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
            if (loadFill != null) loadFill.fillAmount = interaction.HoldProgress01;
        }
    }

    GameObject FindByName(string n)
    {
        Transform[] all = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform t in all) if (t.name == n) return t.gameObject;
        return null;
    }
}
