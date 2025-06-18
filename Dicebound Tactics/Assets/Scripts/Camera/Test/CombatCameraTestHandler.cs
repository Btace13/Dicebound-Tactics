using System.Collections.Generic;
using Sirenix.OdinInspector;
using TacticsToolkit;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CombatCameraTestHandler : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] List<EnemyManager> targets = new List<EnemyManager>();
    [SerializeField] List<CharacterManager> activeCharacters = new List<CharacterManager>();

    [Header("Event Handlers")]
    public UnityEvent<Transform> OnTargetChanged;
    public UnityEvent<Transform> OnActiveCharacterChanged;

    private int currentTargetIndex = 0;
    private int currentActiveCharacterIndex = 0;

    public CharacterManager CurrentActiveCharacter => activeCharacters[currentActiveCharacterIndex];
    public EnemyManager CurrentTarget => targets[currentTargetIndex];


    [Button("Start Combat Camera Test", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void StartCombatCameraTest()
    {
        if (activeCharacters.Count > 0)
        {
            SetActiveCharacter(0);
        }
        else
        {
            Debug.LogWarning("No active characters set in CombatCameraTestHandler.");
        }

        if (targets.Count > 0)
        {
            SetTarget(0);
        }
        else
        {
            Debug.LogWarning("No targets set in CombatCameraTestHandler.");
        }

        CameraManager.Instance.TrySetActiveCamera("CombatMenuCamera1");
    }

    public void SetTarget(int index)
    {
        if (index < 0 || index >= targets.Count)
        {
            Debug.LogError("Index out of range for targets list.");
            return;
        }

        currentTargetIndex = index;

        OnTargetChanged?.Invoke(targets[currentTargetIndex].transform);
    }

    public void SetActiveCharacter(int index)
    {
        if (index < 0 || index >= activeCharacters.Count)
        {
            Debug.LogError("Index out of range for active characters list.");
            return;
        }
        currentActiveCharacterIndex = index;

        OnActiveCharacterChanged?.Invoke(activeCharacters[currentActiveCharacterIndex].transform);
    }

    [Button("Previous Target", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void PreviousTarget()
    {
        int previousIndex = (currentTargetIndex - 1 + targets.Count) % targets.Count;
        SetTarget(previousIndex);
    }

    [Button("Next Target", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void NextTarget()
    {
        int nextIndex = (currentTargetIndex + 1) % targets.Count;
        SetTarget(nextIndex);
    }


    [Button("Previous Active Character", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void PreviousActiveCharacter()
    {
        int previousIndex = (currentActiveCharacterIndex - 1 + activeCharacters.Count) % activeCharacters.Count;
        SetActiveCharacter(previousIndex);
    }

    [Button("Next Active Character", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void NextActiveCharacter()
    {
        int nextIndex = (currentActiveCharacterIndex + 1) % activeCharacters.Count;
        SetActiveCharacter(nextIndex);
    }
}
