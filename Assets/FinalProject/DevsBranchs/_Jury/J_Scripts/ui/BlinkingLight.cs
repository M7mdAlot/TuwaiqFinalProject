using System.Collections;
using UnityEngine;

// Blinks a Light on and off like an alarm. Put it on the light object (or drag a light
// into Target Light). Default = on 1s, off 1s. Optional smooth pulse instead of hard blink.
public class BlinkingLight : MonoBehaviour
{
    [Header("Light")]
    [Tooltip("Leave empty to use the Light on this same object.")]
    public Light targetLight;

    [Header("Timing (seconds)")]
    public float onDuration = 1f;
    public float offDuration = 1f;

    [Header("Style")]
    [Tooltip("Off = hard on/off blink. On = smooth fade in/out (pulse).")]
    public bool smoothPulse = false;

    private float maxIntensity;

    void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (targetLight != null)
            maxIntensity = targetLight.intensity;
    }

    void OnEnable()
    {
        if (targetLight != null)
            StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            if (smoothPulse)
            {
                yield return Fade(0f, maxIntensity, onDuration);   // fade in
                yield return Fade(maxIntensity, 0f, offDuration);  // fade out
            }
            else
            {
                targetLight.enabled = true;
                yield return new WaitForSeconds(onDuration);
                targetLight.enabled = false;
                yield return new WaitForSeconds(offDuration);
            }
        }
    }

    IEnumerator Fade(float from, float to, float time)
    {
        targetLight.enabled = true;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            targetLight.intensity = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        targetLight.intensity = to;
    }
}
