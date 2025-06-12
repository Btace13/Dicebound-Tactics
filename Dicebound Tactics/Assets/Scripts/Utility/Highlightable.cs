using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Highlightable : MonoBehaviour
{
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private UnityEvent onClick;
    private Color originalColor;
    private Material objectMaterial;

    // Static reference to the currently highlighted object
    private static Highlightable currentlyHighlighted;
    private Camera mainCamera;

    private void Start()
    {
        // Get the object's material and save the original color
        objectMaterial = GetComponent<Renderer>().material;
        originalColor = objectMaterial.color;

        // Get the main camera
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Get the current mouse position from the new Input System
        if (Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            // Perform a raycast from the camera to the mouse position
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object has a Highlightable component
                Highlightable hitHighlightable = hit.collider.GetComponent<Highlightable>();

                if (hitHighlightable != null)
                {
                    // If we hit a new object, update the highlight
                    if (hitHighlightable != currentlyHighlighted)
                    {
                        currentlyHighlighted?.Unhighlight(); // Unhighlight the previously highlighted object
                        hitHighlightable.Highlight();        // Highlight the new object
                        currentlyHighlighted = hitHighlightable; // Update the currently highlighted object
                    }

                    // Check if the left mouse button is clicked
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        hitHighlightable.OnClick(); // Trigger the onClick event
                    }
                }
                else
                {
                    // If we didn't hit a highlightable object, unhighlight the last one
                    currentlyHighlighted?.Unhighlight();
                    currentlyHighlighted = null;
                }
            }
            else
            {
                // If nothing is hit, unhighlight the last highlighted object
                currentlyHighlighted?.Unhighlight();
                currentlyHighlighted = null;
            }
        }
    }

    public void Highlight()
    {
        // Change the material color to the highlight color
        objectMaterial.color = highlightColor;
        //EventManager.HighlightableHover(gameObject);
    }

    public void Unhighlight()
    {
        // Revert to the original color
        objectMaterial.color = originalColor;
        //EventManager.HighlightableUnhover();
    }

    public void OnClick()
    {
        // Invoke the onClick UnityEvent, triggering any assigned actions
        onClick.Invoke();
    }
}
