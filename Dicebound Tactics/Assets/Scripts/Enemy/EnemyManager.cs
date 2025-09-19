using System.Collections;
using UnityEngine;
using System.Linq;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEditor;

public class EnemyManager : Entity
{
    [Header("Enemy Specifics")]
    public EnemyDiceProfile diceProfile;
    public OverworldEnemyController overworldController;
    private TurnManager turnManager;
    private CombatEncounter _currentEncounter;


    private void Awake()
    {
        overworldController = GetComponent<OverworldEnemyController>();
        if (overworldController == null)
        {
            // Debug.LogWarning($"No OverworldEnemyController found on {name}. Please ensure it is added for proper overworld functionality.");
        }

        // Event Listeners
        EventManager.OnEnemyTurnStarted += HandleEnemyTurnStarted;
        EventManager.OnCombatEncounterStarted += encounter => _currentEncounter = encounter;
        EventManager.OnCombatEncounterEnded += (encounter, playerWon) => _currentEncounter = null;
    }

    void OnDisable()
    {
        EventManager.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
        EventManager.OnCombatEncounterStarted -= encounter => _currentEncounter = encounter;
        EventManager.OnCombatEncounterEnded -= (encounter, playerWon) => _currentEncounter = null;
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

        // Ensure turnManager is available
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
            if (turnManager == null)
            {
                Debug.LogError($"[EnemyManager] TurnManager not found for {name}. Cannot execute AI turn.");
                EndAITurn();
                yield break;
            }
        }

        while (true)
        {
            // Check if abilityLoadout is available
            if (abilityLoadout == null || abilityLoadout.Count == 0)
            {
                Debug.LogWarning($"[EnemyManager] {name} has no abilities in loadout. Trying to set default abilities.");
                SetDefaultAbilityList();
                
                // If still no abilities after setting defaults, end turn
                if (abilityLoadout == null || abilityLoadout.Count == 0)
                {
                    Debug.LogWarning($"[EnemyManager] {name} could not get any abilities. Ending turn.");
                    break;
                }
            }

            var usableAbilities = abilityLoadout
                .Where(a => a != null && a.apCost <= CurrentAP)
                .ToList();

            if (usableAbilities.Count == 0)
            {
                // Debug.Log($"[AI] {name} has no abilities it can afford with {CurrentAP} AP.");
                break;
            }

            var ability = usableAbilities[Random.Range(0, usableAbilities.Count)];

            // Additional null check for playerUnits
            if (turnManager.playerUnits == null)
            {
                Debug.LogError($"[EnemyManager] TurnManager.playerUnits is null for {name}. Cannot find targets.");
                break;
            }

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

            // Camera setup with null checks
            if (CameraManager.Instance != null && _currentEncounter != null)
            {
                CameraManager.Instance.SetActiveCombatCharacter(target.transform);
                CameraManager.Instance.SetCombatTarget(transform);
                
                var cameraController = _currentEncounter.GetCameraControllerForSide(target);
                if (cameraController != null)
                {
                    CameraManager.Instance.TrySetActiveCamera(cameraController.name);
                }
            }
            else
            {
                if (CameraManager.Instance == null)
                    Debug.LogWarning($"[EnemyManager] CameraManager.Instance is null during {name}'s turn.");
                if (_currentEncounter == null)
                    Debug.LogWarning($"[EnemyManager] _currentEncounter is null during {name}'s turn.");
            }

            // Wait for ability execution to finish
            yield return ability.Execute(this, target);

            // Optional shake or visual cue
            // CameraManager.Instance.ShakeActiveCamera();

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
            DiceModifier randomMod = null;
            // 50% chance to apply a modifier
            if (Random.value < 0.5f && profile.possibleModifiers.Count > 0)
            {
                randomMod = profile.possibleModifiers[Random.Range(0, profile.possibleModifiers.Count)];
                // Debug.Log($"Generated side {i + 1} with modifier: {randomMod.name}");
            }
            else
            {
                // No modifier for this side
                // Debug.Log($"Generated side {i + 1} without modifier");
                randomMod = null;
            }
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
