using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public bool CanInteract = true;
    [SerializeField] private float interactionRadius = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactionCooldown = 1f;
    [SerializeField] private float fov = 120f; // Field of view in degrees
    [SerializeField] private float checkForInteractablesInterval = 0.5f; // How often to check for interactables

    private float nextCheckTime = 0f;
    public List<Interactable> Interactables = new List<Interactable>();

    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager instance is not set. Please ensure it is initialized before using Interactor.");
            return;
        }

        // Subscribe to the interact action
        InputManager.Instance.InputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.InputActions.Player.Interact.performed -= OnInteract;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        print($"{name} is trying to interact with an object.");

        if (!CanInteract)
        {
            Debug.Log($"{name} cannot interact at the moment.");
            return;
        }

        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentGameState != GameState.Overworld)
        {
            Debug.Log($"{name} cannot interact because the game state is not Overworld.");
            return;
        }

        if (InputManager.Instance.CurrentActionMap != InputManager.ActionMap.PLAYER)
        {
            Debug.Log($"{name} cannot interact because the current action map is not Player.");
            return;
        }

        if (ctx.phase == InputActionPhase.Performed)
        {
            print($"{name} started interaction.");

            // Check if there are any interactables in range
            if (Interactables.Count > 0)
            {
                Interactable closestInteractable = GetClosestInteractable();
                if (closestInteractable != null)
                {
                    // Perform interaction with the closest interactable
                    closestInteractable.Interact(this);
                }
            }
            else
            {
                Debug.Log("No interactables in range.");
            }
        }
    }

    private void Update()
    {
        if (!CanInteract) return;

        // Check for interactables at regular intervals
        if (Time.time >= nextCheckTime)
        {
            Interactables = GetInteractables();
            nextCheckTime = Time.time + checkForInteractablesInterval;
        }
    }

    public List<Interactable> GetInteractables()
    {
        if (!CanInteract) return new List<Interactable>();

        List<Interactable> interactables = new List<Interactable>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayer);

        foreach (Collider collider in colliders)
        {
            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable != null && interactable.CanInteract)
            {
                if (CanInteract)
                {
                    Vector3 directionToInteractable = (interactable.transform.position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToInteractable);

                    // Check if the interactable is within the field of view
                    if (angle <= fov / 2f)
                    {
                        // Check if the interactable is not obstructed by any other objects
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, directionToInteractable, out hit, interactionRadius, interactableLayer))
                        {
                            if (hit.collider == collider)
                            {
                                interactables.Add(interactable);
                            }
                        }
                    }
                }
            }
        }

        return interactables;
    }

    public Interactable GetClosestInteractable()
    {
        Interactable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in Interactables)
        {
            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }
}
