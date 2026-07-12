using UnityEngine;

// Attaches a gun (or any) model to an AI's hand bone at runtime. Drop this on any enemy,
// assign the weapon prefab and the hand bone, and tweak the offset until it sits in the hand.
//
// How to find the hand bone: expand the enemy in the Hierarchy into its skeleton
// (Armature/Hips/.../Hand_R or similar) and drag that bone into "Hand Bone".
public class AttachWeaponModel : MonoBehaviour
{
    [Tooltip("The gun model/prefab to put in the AI's hand.")]
    public GameObject weaponPrefab;

    [Tooltip("The bone to parent the gun to (usually the right hand). Empty = this object.")]
    public Transform handBone;

    [Header("Fit it into the hand (tweak in Play mode, then copy the values back)")]
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localEuler = Vector3.zero;
    public Vector3 localScale = Vector3.one;

    private GameObject _spawned;

    void Start()
    {
        if (weaponPrefab == null) return;

        Transform parent = handBone != null ? handBone : transform;
        _spawned = Instantiate(weaponPrefab, parent);
        _spawned.transform.localPosition = localPosition;
        _spawned.transform.localEulerAngles = localEuler;
        _spawned.transform.localScale = localScale;
    }

    // Lets you nudge the offset live in the Inspector while playing and see it move.
    void OnValidate()
    {
        if (_spawned == null) return;
        _spawned.transform.localPosition = localPosition;
        _spawned.transform.localEulerAngles = localEuler;
        _spawned.transform.localScale = localScale;
    }
}
