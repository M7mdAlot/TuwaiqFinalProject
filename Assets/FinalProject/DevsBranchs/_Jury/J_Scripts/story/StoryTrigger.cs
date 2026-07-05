using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    public enum TriggerAction
    {
        StartFirstFight,
        StartMainEvent,
        StartBossFight,
        StartFinalCopyFight,
        ShowObjectiveOnly
    }

    public AegisStoryManager storyManager;
    public TriggerAction action;

    [TextArea]
    public string objectiveText;

    public bool onlyOnce = true;
    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && used)
            return;

        TinyToasterController player = other.GetComponentInParent<TinyToasterController>();

        if (player == null && !other.CompareTag("Player"))
            return;

        used = true;

        if (storyManager == null)
        {
            Debug.LogWarning("StoryTrigger has no AegisStoryManager.");
            return;
        }

        switch (action)
        {
            case TriggerAction.StartFirstFight:
                storyManager.StartFirstFight();
                break;

            case TriggerAction.StartMainEvent:
                storyManager.StartMainEvent();
                break;

            case TriggerAction.StartBossFight:
                storyManager.StartBossFight();
                break;

            case TriggerAction.StartFinalCopyFight:
                storyManager.StartFinalCopyFight();
                break;

            case TriggerAction.ShowObjectiveOnly:
                storyManager.ShowObjective(objectiveText);
                break;
        }
    }
}