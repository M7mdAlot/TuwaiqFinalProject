using UnityEngine;
using Aegis.Systems;

// TEMP TEST HELPER. The reactor/EMP device is disabled at start by CrisisManager (it only
// turns on when the crisis fires after the boss dies). Drop this on the device to force it
// interactable immediately, so you can test the hold-F interaction on its own.
//
// It runs in Start (after CrisisManager's Awake), so it wins and the device becomes usable.
// When the real boss->crisis chain works, DISABLE or REMOVE this component.
public class ReactorTestEnabler : MonoBehaviour
{
    [Tooltip("Left empty = uses the InteractableDevice on this same object.")]
    public InteractableDevice device;

    [Tooltip("Turn off once you've confirmed interaction works and the real crisis chain is wired.")]
    public bool enableOnStart = true;

    void Start()
    {
        if (device == null) device = GetComponent<InteractableDevice>();

        if (device == null)
        {
            Debug.LogWarning("ReactorTestEnabler: no InteractableDevice found on '" + name + "'.", this);
            return;
        }

        if (enableOnStart)
        {
            device.SetEnabled(true);
            Debug.Log("ReactorTestEnabler: forced '" + name + "' interactable for testing.", this);
        }
    }
}
