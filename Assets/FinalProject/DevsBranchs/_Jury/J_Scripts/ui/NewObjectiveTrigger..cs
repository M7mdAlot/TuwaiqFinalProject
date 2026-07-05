using UnityEngine;

public class NewObjectiveTrigger : MonoBehaviour
{
    public ObjectiveUIManager objectiveUI;

    [TextArea]
    public string objectiveText = "Reach the control room";

    public string title = "NEW OBJECTIVE";

    public bool onlyOnce = true;
    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && used) return;
        if (!other.CompareTag("Player")) return;

        used = true;

        if (objectiveUI != null)
            objectiveUI.ShowObjective(title, objectiveText);
    }
}