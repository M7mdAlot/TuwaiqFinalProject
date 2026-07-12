using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Aegis.Player;

// When the player dies, this BUILDS a guaranteed full-screen death screen in code — its own
// Screen-Space-Overlay canvas at the maximum sort order, so it CANNOT be hidden by the scene's
// canvases (which were rendering the designed DEATH panel invisibly). It has a title + Restart
// and Main Menu buttons. Put it on the player. Press K to test.
public class PlayerDeathToPanel : MonoBehaviour
{
    [Tooltip("Leave empty to auto-find the player's HealthSystem on this object / in the scene.")]
    public HealthSystem playerHealth;

    [Header("Death screen")]
    public string deathMessage = "YOU DIED";
    public string mainMenuScene = "MAIN MENU";

    [Header("TEST: press this key to force the death screen (turn OFF for the build)")]
    public bool enableTestKey = true;
    public Key testKey = Key.K;

    private bool shown;

    void Awake()
    {
        if (playerHealth == null) playerHealth = GetComponent<HealthSystem>();
        if (playerHealth == null) playerHealth = GetComponentInParent<HealthSystem>();
        if (playerHealth == null)
        {
            GameObject tagged = GameObject.FindWithTag("Player");
            if (tagged != null) playerHealth = tagged.GetComponentInChildren<HealthSystem>();
        }
        if (playerHealth == null) playerHealth = FindFirstObjectByType<HealthSystem>();

        Debug.Log("PlayerDeathToPanel: watching health on '" +
                  (playerHealth != null ? playerHealth.gameObject.name : "NULL") + "'.", this);
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
    }

    void Update()
    {
        if (enableTestKey && Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            Debug.Log("PlayerDeathToPanel: TEST KEY -> building death screen.", this);
            OnPlayerDied();
        }
    }

    void OnPlayerDied()
    {
        if (shown) return;
        shown = true;

        Debug.Log("PlayerDeathToPanel: PLAYER DIED -> building guaranteed death screen.", this);

        BuildDeathScreen();
        DisablePlayerControl();

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Hard-stop the player so you can't keep moving/looking/shooting after you're dead.
    void DisablePlayerControl()
    {
        if (playerHealth == null) return;
        Transform root = playerHealth.transform.root;

        foreach (MovementController mc in root.GetComponentsInChildren<MovementController>(true))
            mc.enabled = false;
        foreach (WeaponHandler wh in root.GetComponentsInChildren<WeaponHandler>(true))
            wh.enabled = false;
        // Disabling PlayerInput cuts ALL input (move, look, fire) in one shot.
        foreach (PlayerInput pi in root.GetComponentsInChildren<PlayerInput>(true))
            pi.enabled = false;
    }

    void BuildDeathScreen()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Own overlay canvas — renders on top of EVERYTHING, needs no camera.
        GameObject canvasGO = new GameObject("RUNTIME DEATH SCREEN");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        // Full-screen dark background.
        Image bg = NewImage("BG", canvasGO.transform, new Color(0.02f, 0f, 0f, 0.92f));
        Stretch(bg.rectTransform);

        // Title.
        Text title = NewText("TITLE", canvasGO.transform, deathMessage, font, 96,
                             new Color(0.85f, 0.1f, 0.12f));
        Place(title.rectTransform, new Vector2(0f, 150f), new Vector2(1400f, 240f));

        // Buttons.
        MakeButton("RESTART", canvasGO.transform, font, new Vector2(0f, -30f),
            () => { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); });

        MakeButton("MAIN MENU", canvasGO.transform, font, new Vector2(0f, -150f),
            () => { Time.timeScale = 1f; if (!string.IsNullOrEmpty(mainMenuScene)) SceneManager.LoadScene(mainMenuScene); });
    }

    // ---- tiny UI builders ----

    Image NewImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Text NewText(string name, Transform parent, string content, Font font, int size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text t = go.AddComponent<Text>();
        t.text = content;
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    void MakeButton(string label, Transform parent, Font font, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        Image img = NewImage("BTN_" + label, parent, new Color(0.12f, 0.12f, 0.14f, 0.95f));
        Place(img.rectTransform, pos, new Vector2(420f, 90f));

        Button btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        Text t = NewText("Label", img.transform, label, font, 40, Color.white);
        Stretch(t.rectTransform);
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void Place(RectTransform rt, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }
}
