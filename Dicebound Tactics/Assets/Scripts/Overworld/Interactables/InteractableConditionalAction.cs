using UnityEngine;

[System.Serializable]
public abstract class InteractableConditionalAction : InteractionAction
{
    public InteractableConditionalAction condition;
    public InteractionAction onSuccess;
    public InteractionAction onFailure;

    public override void Execute(GameObject interactor, GameObject target)
    {
        if (condition != null && condition.CheckCondition(interactor, target))
        {
            onSuccess?.Execute(interactor, target);
        }
        else
        {
            onFailure?.Execute(interactor, target);
        }
    }

    public abstract bool CheckCondition(GameObject interactor, GameObject target);
}
