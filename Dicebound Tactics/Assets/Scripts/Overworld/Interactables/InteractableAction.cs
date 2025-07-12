using UnityEngine;

[System.Serializable]
public abstract class InteractionAction
{
    public abstract void Execute(GameObject interactor, GameObject target);
}
