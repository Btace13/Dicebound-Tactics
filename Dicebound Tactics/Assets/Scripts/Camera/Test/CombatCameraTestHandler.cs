using System.Collections.Generic;
using Sirenix.OdinInspector;
using TacticsToolkit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CombatCameraTestHandler : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private TurnManager turnManager;

    [Header("Event Handlers")]
    public UnityEvent<Transform> OnTargetChanged;

    private int currentTargetIndex = 0;
    private int currentActiveCharacterIndex = 0;

    public CharacterManager CurrentActiveCharacter => turnManager.playerUnits[currentActiveCharacterIndex];
    public EnemyManager CurrentTarget => turnManager.enemyUnits[currentTargetIndex];

    public void SetTarget(int index)
    {
        if (index < 0 || index >= turnManager.enemyUnits.Count)
        {
            Debug.LogError("Index out of range for targets list.");
            return;
        }

        currentTargetIndex = index;

        OnTargetChanged?.Invoke(turnManager.enemyUnits[currentTargetIndex].transform);
    }

    public void SetActiveCharacter(int index)
    {
        if (index < 0 || index >= turnManager.playerUnits.Count)
        {
            Debug.LogError("Index out of range for active characters list.");
            return;
        }
        currentActiveCharacterIndex = index;

        EventManager.TriggerNewActiveEntity(turnManager.playerUnits[currentActiveCharacterIndex]);
    }

    [Button("Previous Target", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void PreviousTarget()
    {
        int previousIndex = (currentTargetIndex - 1 + turnManager.enemyUnits.Count) % turnManager.enemyUnits.Count;
        SetTarget(previousIndex);
    }

    [Button("Next Target", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void NextTarget()
    {
        int nextIndex = (currentTargetIndex + 1) % turnManager.enemyUnits.Count;
        SetTarget(nextIndex);
    }


    [Button("Previous Active Character", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void PreviousActiveCharacter()
    {
        int previousIndex = (currentActiveCharacterIndex - 1 + turnManager.playerUnits.Count) % turnManager.playerUnits.Count;
        SetActiveCharacter(previousIndex);
    }

    [Button("Next Active Character", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void NextActiveCharacter()
    {
        int nextIndex = (currentActiveCharacterIndex + 1) % turnManager.playerUnits.Count;
        SetActiveCharacter(nextIndex);
    }
}
