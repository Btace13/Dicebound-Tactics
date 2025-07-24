using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TriggerDialogueAction : InteractionAction
{
    public DialogueSystemTrigger dialogueTrigger;

    public override void Execute(GameObject interactor, GameObject target)
    {
        if (dialogueTrigger == null)
        {
            Debug.LogWarning("DialogueTrigger is not assigned.");
            return;
        }

        dialogueTrigger.OnUse(interactor.transform);
    }
}
