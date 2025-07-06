using System.Collections;
using UnityEngine;
using System.Linq;
using TacticsToolkit;
using System.Collections.Generic;

public class EnemyManager : Entity
{
    [Header("Enemy Specifics")]
    public EnemyDiceProfile diceProfile;
    public OverworldEnemyController overworldController;
    private TurnManager turnManager;

    private void Awake()
    {
        overworldController = GetComponent<OverworldEnemyController>();
        if (overworldController == null)
        {
            Debug.LogWarning($"No OverworldEnemyController found on {name}. Please ensure it is added for proper overworld functionality.");
        }
    }

    private void Start()
    {
        turnManager = FindFirstObjectByType<TurnManager>();

        if (diceProfile != null)
        {
            equippedDice = CreateDiceFromProfile(diceProfile);
        }
        else
        {
            Debug.LogWarning($"No dice profile assigned for {name}. Using default dice.");
            equippedDice = new Dice(new List<DiceSide> { new(1, null) });
        }

    }

    public void BeginAITurn()
    {
        if (!isAlive || CurrentAP <= 0)
        {
            EndAITurn();
            return;
        }

        StartCoroutine(ExecuteAITurn());
    }

    private IEnumerator ExecuteAITurn()
    {
        Debug.Log($"[AI] {name} begins turn with {CurrentAP} AP");

        while (CurrentAP > 0)
        {
            var usableAbilities = abilityLoadout
                .Where(a => a.apCost <= CurrentAP)
                .ToList();

            if (usableAbilities.Count == 0)
                break;

            AbilitySO ability = usableAbilities[Random.Range(0, usableAbilities.Count)];

            var targets = turnManager.playerUnits
                .Where(p => p != null && p.isAlive)
                .ToList();

            if (targets.Count == 0)
                break;

            var target = targets[Random.Range(0, targets.Count)];

            Debug.Log($"[AI] {name} uses {ability.abilityName} on {target.name}");

            //need to add logic for timing and animations here
            CameraManager.Instance.SetActiveCombatCharacter(target.transform);
            CameraManager.Instance.SetCombatTarget(transform);

            CameraManager.Instance.TrySetActiveCamera("EnemyAttackCamera");
            yield return new WaitForSeconds(1f);

            ability.Execute(this, target);
            CameraManager.Instance.ShakeActiveCamera();
            statsContainer.ActionPoints.statValue -= ability.apCost;

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[AI] {name} ends turn with {CurrentAP} AP");
        EndAITurn();
    }

    public static Dice CreateDiceFromProfile(EnemyDiceProfile profile)
    {
        int sides = Random.Range(profile.minSides, profile.maxSides + 1);
        List<DiceSide> generatedSides = new();

        for (int i = 0; i < sides; i++)
        {
            DiceModifier randomMod = profile.possibleModifiers[Random.Range(0, profile.possibleModifiers.Count)];
            generatedSides.Add(new DiceSide(i + 1, randomMod));
        }

        return new Dice(generatedSides);
    }

    private void EndAITurn()
    {
        turnManager.EndTurn();
    }
}
