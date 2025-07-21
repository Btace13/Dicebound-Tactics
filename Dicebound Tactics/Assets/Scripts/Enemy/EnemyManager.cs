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
            // Debug.LogWarning($"No OverworldEnemyController found on {name}. Please ensure it is added for proper overworld functionality.");
        }

        // Event Listeners
        EventManager.OnEnemyTurnStarted += HandleEnemyTurnStarted;
    }

    protected override void Start()
    {
        base.Start();

        turnManager = FindFirstObjectByType<TurnManager>();

        if (diceProfile != null)
        {
            equippedDice = CreateDiceFromProfile(diceProfile);
        }
        else
        {
            // Debug.LogWarning($"No dice profile assigned for {name}. Using default dice.");
            equippedDice = new Dice(new List<DiceSide> { new(1, null) });
        }

    }

    private void HandleEnemyTurnStarted(EnemyManager enemy)
    {
        if (enemy == this)
        {
            BeginAITurn();
        }
    }

    public void BeginAITurn()
    {
        StartCoroutine(ExecuteAITurn());
    }

    private IEnumerator ExecuteAITurn()
    {
        // Debug.Log($"[AI] {name} begins turn with {CurrentAP} AP");

        while (true)
        {
            var usableAbilities = abilityLoadout
                .Where(a => a.apCost <= CurrentAP)
                .ToList();

            if (usableAbilities.Count == 0)
            {
                // Debug.Log($"[AI] {name} has no abilities it can afford with {CurrentAP} AP.");
                break;
            }

            var ability = usableAbilities[Random.Range(0, usableAbilities.Count)];

            var targets = turnManager.playerUnits
                .Where(p => p != null && p.isAlive)
                .ToList();

            if (targets.Count == 0)
            {
                // Debug.Log("[AI] No valid targets remain.");
                break;
            }

            var target = targets[Random.Range(0, targets.Count)];

            // Debug.Log($"[AI] {name} using {ability.abilityName} (cost {ability.apCost}) on {target.name}");

            // Camera setup
            CameraManager.Instance.SetActiveCombatCharacter(target.transform);
            CameraManager.Instance.SetCombatTarget(transform);
            CameraManager.Instance.TrySetActiveCamera("EnemyAttackCamera");

            // Wait for ability execution to finish
            yield return ability.Execute(this, target);

            // Optional shake or visual cue
            CameraManager.Instance.ShakeActiveCamera();

            yield return new WaitForSeconds(0.5f); // slight pacing delay between abilities
        }

        // Debug.Log($"[AI] {name} ends turn with {CurrentAP} AP remaining");
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
        EventManager.TriggerEnemyTurnEnded(this);
    }

    public override void Die()
    {
        base.Die();

        if (overworldController != null && overworldController.Encounter != null)
        {
            // Debug.Log($"[EnemyManager] {name} has been defeated. Checking if should end encounter.");
            // Check if the encounter should end
            if (overworldController.Encounter.ShouldEndEncounter())
            {
                overworldController.Encounter.EndEncounter();
            }
        }
        else
        {
            // Debug.LogWarning($"No OverworldEnemyController found on {name}. Cannot handle enemy defeat logic.");
        }
    }
}
