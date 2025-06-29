using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class OverworldCharacterController : MonoBehaviour
{
    [BoxGroup("Control Settings"), SerializeField] protected float moveSpeed = 5f;
    [BoxGroup("Control Settings"), SerializeField] protected float rotationSpeed = 720f;
    [BoxGroup("Control Settings"), SerializeField] protected bool isControlled = false;

    private CharacterController characterController;

    public bool IsControlled { get { return isControlled; } private set { isControlled = value; } }

    private void Awake()
    {
        characterController = gameObject.GetOrAddComponent<CharacterController>();
    }

    public void Update()
    {
        if (InputManager.Instance == null || InputManager.Instance.InputActions == null)
        {
            Debug.LogWarning("InputManager or InputActions is not initialized.");
            return;
        }

        HandleMovement(InputManager.Instance.InputActions.Player.Move.ReadValue<Vector2>());
    }

    private void HandleMovement(Vector2 input)
    {
        if (!IsControlled || characterController == null)
        {
            return;
        }

        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Overworld)
        {
            IsControlled = true;
        }
        else
        {
            IsControlled = false;
        }
    }
}
