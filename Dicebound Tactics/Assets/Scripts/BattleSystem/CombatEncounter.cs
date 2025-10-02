using UnityEngine;
using System.Collections.Generic;
using TacticsToolkit;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Linq;
using Unity.Cinemachine;
using andywiecko.BurstTriangulator;

public class CombatEncounter : MonoBehaviour
{
    [Header("Loot Table")]
    [SerializeField] private LootTable lootTable;
    [System.Serializable]
    public class EncounterSlot
    {
        public bool isOccupied = false;
        public Entity entity = null;
        public Transform slotTransform;
    }

    [System.Serializable]
    public class EncounterSide
    {
        [Header("Side Configuration")]
        public bool isPlayerSide = false;
        public CombatCameraController combatCamera;
        public List<EncounterSlot> combatSlots = new List<EncounterSlot>();
        public Vector3 CenterPosition
        {
            get
            {
                if (combatSlots.Count == 0)
                {
                    return Vector3.zero;
                }

                Vector3 sum = Vector3.zero;
                foreach (EncounterSlot slot in combatSlots)
                {
                    sum += slot.slotTransform.position;
                }
                return sum / combatSlots.Count;
            }
        }
    }

    public bool IsActive { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    private Dictionary<EnemyManager, float> _timeSinceLastAction = new Dictionary<EnemyManager, float>();

    [Header("Encounter Settings")]
    public float encounterRadius = 8f;
    public float timeBetweenEnemyActions = 2f;
    public bool IsAntagonistic = true;
    public AudioClip encounterMusic;

    [Header("Movement Settings")]
    [Tooltip("Enable this to make characters leap to encounter slots instead of running")]
    public bool useLeapMovement = false;
    [ShowIf("useLeapMovement")] public float leapDuration = 1.0f;
    [ShowIf("useLeapMovement")] public float leapHeight = 3.0f;
    [ShowIf("useLeapMovement")] public AnimationCurve leapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Encounter References")]
    [SerializeField] private EncounterSide[] encounterSides = new EncounterSide[2];
    public List<EnemyManager> EnemyPrefabs = new List<EnemyManager>();
    public List<EnemyManager> Enemies { get; private set; } = new List<EnemyManager>();

    public void Awake()
    {
        SpawnEnemies(EnemyPrefabs);
        IsActive = false;
        IsCompleted = false;

        // Event Listeners
        EventManager.OnCombatEncounterEnded += HandleCombatEncounterEnded;
    }

    void OnDisable()
    {
        EventManager.OnCombatEncounterEnded -= HandleCombatEncounterEnded;
    }

    /// <summary>
    /// Call this to spawn loot at the end of the encounter (e.g. on victory)
    /// </summary>
    public void SpawnEncounterLoot()
    {
        if (lootTable == null) return;
        var loot = lootTable.RollLoot();
        // Use the center of the encounter as the spawn position
        Vector3 spawnPosition = Enemies.Count > 0 ? Enemies[0].transform.position : transform.position;
        float scatterRadius = 2f;
        int pickupCount = 5;
        foreach (var (currencyType, amount) in loot)
        {
            CurrencyUtils.SpawnCurrencyScatter(spawnPosition, currencyType, amount, pickupCount, scatterRadius);
        }
    }

    public void SpawnEnemies(List<EnemyManager> enemies)
    {
        Enemies.Clear();

        foreach (EnemyManager enemy in enemies)
        {
            if (enemy == null) continue;

            EnemyManager spawnedEnemy = Instantiate(enemy, GetRandomPointInEncounterArea(), Quaternion.identity, transform);
            spawnedEnemy.transform.parent = null;

            Enemies.Add(spawnedEnemy);
        }
    }

    public void Update()
    {
        if (IsCompleted) return;

        // Only update enemy behavior if the encounter is not active
        if (IsActive) return;

        Transform target = PartyManager.Instance.PartyLeader.transform;

        foreach (EnemyManager enemy in Enemies)
        {
            if (target != null && enemy.overworldController.CanSeeTarget(target))
            {
                enemy.overworldController.UpdateLastKnownTargetPosition(target.position);
            }

            // Ensure the enemy's overworld controller is linked to this encounter
            if (enemy.overworldController.Encounter == null)
            {
                enemy.overworldController.Encounter = this;
            }

            if (!_timeSinceLastAction.ContainsKey(enemy))
            {
                _timeSinceLastAction[enemy] = 0f;
            }

            if (IsAntagonistic && enemy.overworldController.HasSpottedTarget)
            {
                HandleEnemyChaseBehavior(enemy);
            }

            if (!IsAntagonistic || !enemy.overworldController.IsChasingTarget)
            {
                HandleEnemyWanderBehavior(enemy);
            }
        }
    }

    public EncounterSide GetClosestEncounterSide(Vector3 position)
    {
        EncounterSide closestSide = null;
        float closestDistance = float.MaxValue;

        foreach (EncounterSide side in encounterSides)
        {
            if (side.combatSlots.Count == 0) continue;

            Vector3 centerPosition = side.CenterPosition;
            float distance = Vector3.Distance(position, centerPosition);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSide = side;
            }
        }

        return closestSide;
    }

    public EncounterSlot GetClosestSlot(Vector3 position, EncounterSide side)
    {
        EncounterSlot closestSlot = null;
        float closestDistance = float.MaxValue;

        foreach (EncounterSlot slot in side.combatSlots)
        {
            if (slot.isOccupied) continue;

            float distance = Vector3.Distance(position, slot.slotTransform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        return closestSlot;
    }

    [Button("Start Encounter")]
    public async void StartEncounter()
    {
        TurnManager.Instance.SetEnemies(Enemies);

        IsActive = true;

        EncounterSide playerSide = GetPlayerEncounterSide();
        EncounterSide enemySide = GetEnemyEncounterSide();

        if (playerSide == null)
        {
            Debug.LogError("No player side found! Please mark one EncounterSide as isPlayerSide = true");
            return;
        }

        if (enemySide == null)
        {
            Debug.LogError("No enemy side found! Please ensure at least one EncounterSide has isPlayerSide = false");
            return;
        }

        int remainingMovingCharacters = PartyManager.Instance.ActivePartyMembers.Count;

        foreach (CharacterManager c in PartyManager.Instance.ActivePartyMembers)
        {
            if (c.TryGetComponent(out OverworldCharacterController controller))
            {
                // Ensure the character is linked to this encounter
                if (controller.Encounter == null)
                {
                    controller.Encounter = this;
                }

                controller.CanFollowLeader = false; // Disable player control during combat
                controller.CancelPath(); // Cancel any existing pathfinding
                controller.SetShouldSprint(true); // Enable sprinting for combat movement

                EncounterSlot closestSlot = GetClosestSlot(c.transform.position, playerSide);

                if (closestSlot == null)
                {
                    Debug.LogWarning($"No available slots for {c.name} in the closest encounter side.");
                    continue;
                }

                controller.AssignedEncounterSlot = closestSlot; // Assign the slot to the controller

                // Choose movement type based on settings
                if (useLeapMovement)
                {
                    // Use leap movement
                    LeapMovementController leapController = c.GetComponent<LeapMovementController>();
                    if (leapController == null)
                    {
                        leapController = c.gameObject.AddComponent<LeapMovementController>();
                    }

                    // Configure leap parameters
                    leapController.SetLeapParameters(leapDuration, leapHeight, leapCurve);

                    // Cancel any pathfinding to prevent conflicts
                    controller.CancelPath();

                    // Perform the leap
                    leapController.LeapToTarget(closestSlot.slotTransform, () =>
                    {
                        // Clear pathfinding destination after leap to prevent running back
                        controller.CancelPath();

                        // Set the AI destination to current position to stop any movement
                        if (controller.TryGetComponent(out CustomRichAI richAI))
                        {
                            richAI.destination = c.transform.position;
                            richAI.canMove = true; // Ensure movement is re-enabled
                        }

                        remainingMovingCharacters--;
                        if (remainingMovingCharacters <= 0)
                        {
                            // All characters have reached their combat slots
                        }
                    });
                }
                else
                {
                    // Use normal pathfinding movement
                    controller.MoveToTarget(closestSlot.slotTransform, true, () =>
                    {
                        remainingMovingCharacters--;
                        if (remainingMovingCharacters <= 0)
                        {
                            // All characters have reached their combat slots
                        }
                    });
                }
                closestSlot.isOccupied = true;
                closestSlot.entity = c; // Assign the character to the slot
            }
        }

        int remainingMovingEnemies = Enemies.Count;

        foreach (EnemyManager enemy in Enemies)
        {
            if (enemy.TryGetComponent(out OverworldEnemyController controller))
            {
                controller.CancelPath(); // Cancel any existing pathfinding
                controller.SetShouldSprint(true); // Enable sprinting for combat movement

                EncounterSlot closestSlot = GetClosestSlot(enemy.transform.position, enemySide);

                if (closestSlot == null)
                {
                    Debug.LogWarning($"No available slots for {enemy.name} in the opposite encounter side.");
                    continue;
                }

                controller.AssignedEncounterSlot = closestSlot; // Assign the slot to the controller

                // Choose movement type based on settings
                if (useLeapMovement)
                {
                    // Use leap movement for enemies too
                    LeapMovementController leapController = enemy.GetComponent<LeapMovementController>();
                    if (leapController == null)
                    {
                        leapController = enemy.gameObject.AddComponent<LeapMovementController>();
                    }

                    // Configure leap parameters
                    leapController.SetLeapParameters(leapDuration, leapHeight, leapCurve);

                    // Cancel any pathfinding to prevent conflicts
                    controller.CancelPath();

                    // Perform the leap
                    leapController.LeapToTarget(closestSlot.slotTransform, () =>
                    {
                        // Clear pathfinding destination after leap to prevent running back
                        controller.CancelPath();

                        // Set the AI destination to current position to stop any movement
                        if (controller.TryGetComponent(out CustomRichAI richAI))
                        {
                            richAI.destination = enemy.transform.position;
                            richAI.canMove = true; // Ensure movement is re-enabled
                        }

                        remainingMovingEnemies--;
                        if (remainingMovingEnemies <= 0)
                        {
                            // All enemies have reached their combat slots
                        }
                    });
                }
                else
                {
                    // Use normal pathfinding movement
                    controller.MoveToTarget(closestSlot.slotTransform, true, () =>
                    {
                        remainingMovingEnemies--;
                        if (remainingMovingEnemies <= 0)
                        {
                            // All enemies have reached their combat slots
                        }
                    });
                }

                closestSlot.isOccupied = true;
                closestSlot.entity = enemy; // Assign the enemy to the slot
            }
        }


        float waitStartTime = Time.realtimeSinceStartup;
        float maxWaitTime = 10f; // seconds
        int lastCharacters = remainingMovingCharacters;
        int lastEnemies = remainingMovingEnemies;
        while (remainingMovingCharacters > 0 || remainingMovingEnemies > 0)
        {
            if (lastCharacters != remainingMovingCharacters || lastEnemies != remainingMovingEnemies)
            {
                Debug.Log($"[CombatEncounter] Waiting: Characters={remainingMovingCharacters}, Enemies={remainingMovingEnemies}");
                lastCharacters = remainingMovingCharacters;
                lastEnemies = remainingMovingEnemies;
            }
            if (Time.realtimeSinceStartup - waitStartTime > maxWaitTime)
            {
                Debug.LogError($"[CombatEncounter] Timeout waiting for units to finish leaping! Characters={remainingMovingCharacters}, Enemies={remainingMovingEnemies}");
                break;
            }
            await Task.Yield();
        }

        foreach (CharacterManager c in PartyManager.Instance.ActivePartyMembers)
        {
            c.EquipWeapon(c.CurrentWeapon); // Ensure the character has their weapon equipped for combat
        }

        foreach (EnemyManager enemy in Enemies)
        {
            enemy.EquipWeapon(enemy.CurrentWeapon); // Ensure the enemy has their weapon equipped for combat
        }

        EventManager.TriggerCombatEncounterStarted(this);

        // Initialize encounter logic here, such as spawning enemies, setting up UI, etc.
    }

    [Button("End Encounter")]
    public void EndEncounter()
    {
        // Cleanup encounter logic here, such as removing enemies, resetting UI, etc.

        // Reset encounter state
        IsActive = false;

        foreach (CharacterManager c in PartyManager.Instance.ActivePartyMembers)
        {
            c.OverworldCharacterController.Encounter = null;
            c.OverworldCharacterController.AssignedEncounterSlot = null;

            c.OverworldCharacterController.CanFollowLeader = true; // Re-enable player control after combat
            c.OverworldCharacterController.CancelPath(); // Cancel any existing pathfinding
            c.OverworldCharacterController.SetShouldSprint(false); // Disable sprinting for combat movement
            c.EquipWeapon(null); // Put away the weapon after combat
        }

        foreach (EnemyManager enemy in Enemies)
        {
            enemy.overworldController.Encounter = null;
            enemy.overworldController.AssignedEncounterSlot = null;
            enemy.overworldController.CancelPath(); // Cancel any existing pathfinding
            enemy.overworldController.SetShouldSprint(false); // Disable sprinting for combat movement
        }

        EventManager.TriggerCombatEncounterEnded(this);
    }

    public bool ShouldEndEncounter()
    {
        bool allEnemiesDefeated = Enemies.All(e => e == null || e.CurrentHealth <= 0);
        return allEnemiesDefeated;
    }

    public EncounterSide GetPlayerEncounterSide()
    {
        return encounterSides.FirstOrDefault(side => side.isPlayerSide);
    }

    public EncounterSide GetEnemyEncounterSide()
    {
        // Return the first side that is NOT marked as player side
        return encounterSides.FirstOrDefault(side => !side.isPlayerSide);
    }

    #region OVERWORLD ENEMY MANAGEMENT

    public Vector3 GetRandomPointInEncounterArea()
    {
        Vector2 randomCircle = Random.insideUnitCircle * encounterRadius;
        Vector3 point = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        if (Physics.Raycast(point + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, LayerMask.GetMask("Ground")))
        {
            point = hit.point;
        }
        else
        {
            point = transform.position; // Fallback to the center if no ground found
        }

        return point;
    }

    public void HandleEnemyWanderBehavior(EnemyManager enemy)
    {
        if (enemy == null || enemy.overworldController == null)
        {
            Debug.LogWarning($"Enemy or OverworldController is null for {enemy?.name}");
            return;
        }

        if (enemy.overworldController.IsChasingTarget && IsAntagonistic)
        {
            // If the enemy is chasing, do not wander
            return;
        }

        // Only handle wandering if the enemy is not currently moving
        if (!enemy.overworldController.HasPath || enemy.overworldController.HasReachedDestination)
        {
            if (_timeSinceLastAction[enemy] > timeBetweenEnemyActions)
            {

                Vector3 randomPoint = GetRandomPointInEncounterArea();
                enemy.overworldController.MoveToPosition(randomPoint, true);

                // Handle enemy wandering behavior here
                if (Vector3.Distance(enemy.transform.position, randomPoint) > 1f)
                {
                    _timeSinceLastAction[enemy] = 0f;
                }
            }
            else
            {
                _timeSinceLastAction[enemy] += Time.deltaTime;
            }
        }
    }

    public CombatCameraController GetCameraControllerForSide(Entity entity)
    {
        // find side that has the entity
        EncounterSide side = encounterSides.FirstOrDefault(s => s.combatSlots.Any(slot => slot.entity == entity));
        if (side == null)
        {
            Debug.LogWarning($"No encounter side found for entity {entity.name}");
            return null;
        }

        return side.combatCamera;
    }

    public List<CombatCameraController> GetAllCameraControllers()
    {
        return encounterSides.Select(side => side.combatCamera).ToList();
    }

    public void HandleEnemyChaseBehavior(EnemyManager enemy)
    {
        if (enemy == null || enemy.overworldController == null)
        {
            Debug.LogWarning($"Enemy, OverworldController, or target is null for {enemy?.name}");
            return;
        }

        if (enemy.overworldController.LastKnownTargetPosition == Vector3.positiveInfinity)
        {
            return;
        }

        enemy.overworldController.MoveToLastKnownTargetPosition(() =>
        {
            Transform target = PartyManager.Instance.PartyLeader.transform;

            if (enemy.overworldController.CanSeeTarget(target))
            {
                enemy.overworldController.UpdateLastKnownTargetPosition(target.position);
            }
            else
            {
                enemy.overworldController.ClearLastKnownTargetPosition();
                enemy.overworldController.CancelPath();
            }
        });
    }

    public int CalculateEXP(Entity character)
    {
        int totalExp = 1;
        foreach (var enemy in Enemies)
        {
            float levelDiff = enemy.level - character.level;
            float multiplier = 1f + (levelDiff * 0.1f);
            multiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
            int baseExp = enemy.level * 10;
            totalExp += Mathf.RoundToInt(baseExp * multiplier);
        }
        return totalExp;
    }

    private void HandleCombatEncounterEnded(CombatEncounter encounter, bool playerWon)
    {

        if (encounter != this) return;

        IsCompleted = true;
        IsActive = false;
        if (playerWon)
        {
            SpawnEncounterLoot();

            TurnManager.Instance.playerUnits.ForEach(unit =>
            {
                int expAdded = playerWon ? 100 / TurnManager.Instance.playerUnits.Count : 0;
                Debug.Log($"Adding {expAdded} EXP to {unit.name}");
                unit.IncreaseExp(expAdded);
            });
        }
    }

    #region Testing Methods

    [Button("Test Leap Movement")]
    private void TestLeapMovement()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Leap movement test can only be run in play mode.");
            return;
        }

        if (PartyManager.Instance == null || PartyManager.Instance.ActivePartyMembers.Count == 0)
        {
            Debug.LogWarning("No party members found for leap movement test.");
            return;
        }

        CharacterManager testCharacter = PartyManager.Instance.ActivePartyMembers[0];
        LeapMovementController leapController = testCharacter.GetComponent<LeapMovementController>();

        if (leapController == null)
        {
            leapController = testCharacter.gameObject.AddComponent<LeapMovementController>();
        }

        // Configure leap parameters from encounter settings
        leapController.SetLeapParameters(leapDuration, leapHeight, leapCurve);

        // Find a target slot to leap to
        EncounterSide closestSide = GetClosestEncounterSide(testCharacter.transform.position);
        EncounterSlot targetSlot = GetClosestSlot(testCharacter.transform.position, closestSide);

        if (targetSlot != null)
        {
            leapController.LeapToTarget(targetSlot.slotTransform, () =>
            {
                // Leap movement test completed
            });
        }
        else
        {
            Debug.LogWarning("No available encounter slot found for leap movement test.");
        }
    }

    #endregion

    #endregion
}
