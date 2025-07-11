using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool CanInteract = true;

    public virtual void Interact(Interactor interactor)
    {
        if (CanInteract)
        {
            // Interact with the object
            Debug.Log($"{interactor.name} interacted with {gameObject.name}");
        }
    }
}
