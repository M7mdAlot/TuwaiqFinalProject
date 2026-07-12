using UnityEngine;
using Aegis.Systems;

// Auto-drives the endings from the crisis outcome — NO Inspector event wiring needed.
// Drop this on ONE object (e.g. the manager). When the reactor is DEFUSED it plays the good
// ending; when the timer RUNS OUT it plays the bad ending. It auto-finds the CrisisManager and
// the ending manager for whichever scene you're in.
public class CrisisEndingLink : MonoBehaviour
{
    [Header("All optional — auto-found")]
    public CrisisManager crisis;
    public AegisEndingManager aegisEnding;
    public XEndingManager xEnding;

    private bool _done;

    void Update()
    {
        if (_done) return;

        if (crisis == null) crisis = FindFirstObjectByType<CrisisManager>();
        if (crisis == null || !crisis.HasTriggered) return;

        // The CrisisManager marks itself resolved (success = defused, failure = timed out).
        if (crisis.IsResolved)
        {
            _done = true;
            if (crisis.ResolvedWithSuccess)
            {
                Debug.Log("CrisisEndingLink: reactor DEFUSED -> GOOD ending.", this);
                PlayGood();
            }
            else
            {
                Debug.Log("CrisisEndingLink: timer RAN OUT -> BAD ending.", this);
                PlayBad();
            }
            return;
        }

        // Safety net: if the timer reaches 0 but somehow didn't resolve, force the bad ending.
        if (crisis.TimeRemaining <= 0f)
        {
            _done = true;
            Debug.Log("CrisisEndingLink: timer hit 0 -> BAD ending (fallback).", this);
            PlayBad();
        }
    }

    /// <summary>Call this from SimpleDeviceInteractor / the device's OnDisabled to force the good ending immediately.</summary>
    public void ForceGoodEnding()
    {
        if (_done) return;
        _done = true;
        Debug.Log("CrisisEndingLink: forced GOOD ending (device shut down).", this);
        PlayGood();
    }

    void PlayGood()
    {
        Resolve();
        if (IsXScene() && xEnding != null) { xEnding.PlayGoodEnding(); return; }
        if (aegisEnding != null) { aegisEnding.PlayGoodEnding(); return; }
        if (xEnding != null) { xEnding.PlayGoodEnding(); return; }
        Debug.LogError("CrisisEndingLink: no ending manager found for the GOOD ending.", this);
    }

    void PlayBad()
    {
        Resolve();
        if (IsXScene() && xEnding != null) { xEnding.PlayBadEnding(); return; }
        if (aegisEnding != null) { aegisEnding.PlayBadEnding(); return; }
        if (xEnding != null) { xEnding.PlayBadEnding(); return; }
        Debug.LogError("CrisisEndingLink: no ending manager found for the BAD ending.", this);
    }

    void Resolve()
    {
        if (aegisEnding == null) aegisEnding = FindFirstObjectByType<AegisEndingManager>();
        if (xEnding == null) xEnding = FindFirstObjectByType<XEndingManager>();
    }

    bool IsXScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower().StartsWith("x");
    }
}
