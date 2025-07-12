using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    public bool CanInteract = true;
    [SerializeField] private bool isSingleUse = true;
    public bool IsSingleUse => isSingleUse;

    [SerializeReference]
    public List<InteractionAction> actions = new();

    public virtual void Interact(Interactor interactor)
    {
        if (!CanInteract)
        {
            Debug.Log($"{interactor.name} cannot interact with {gameObject.name} at the moment.");
            return;
        }

        Debug.Log($"{interactor.name} is interacting with {gameObject.name}");

        foreach (var action in actions)
        {
            action.Execute(interactor.gameObject, gameObject);
        }
    }
}
