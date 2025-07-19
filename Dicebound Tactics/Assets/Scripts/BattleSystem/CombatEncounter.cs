using UnityEngine;
using System.Collections.Generic;
using TacticsToolkit;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Linq;

public class CombatEncounter : MonoBehaviour
{
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

    [Header("Encounter References")]
    [SerializeField] private EncounterSide[] encounterSides = new EncounterSide[2];
    public List<EnemyManager> Enemies = new List<EnemyManager>();
    public void Update()
    {
        if (IsCompleted) return;

        // Only update enemy behavior if the encounter is not active
        if (IsActive) return;

        foreach (EnemyManager enemy in Enemies)
        {
            // Ensure the enemy's overworld controller is linked to this encounter
            if (enemy.overworldController.Encounter == null)
            {
                enemy.overworldController.Encounter = this;
            }

            if (!_timeSinceLastAction.ContainsKey(enemy))
            {
                _timeSinceLastAction[enemy] = 0f;
            }

            if (IsAntagonistic)
            {
                HandleEnemyChaseBehavior(enemy);
            }

            if (!enemy.overworldController.IsChasingTarget)
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
        IsActive = true;

        EncounterSide closestSide = GetClosestEncounterSide(PartyManager.Instance.PartyLeader.transform.position);

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

                EncounterSlot closestSlot = GetClosestSlot(c.transform.position, closestSide);

                if (closestSlot == null)
                {
                    Debug.LogWarning($"No available slots for {c.name} in the closest encounter side.");
                    continue;
                }
                else
                {
                    Debug.Log($"{c.name} assigned to slot at {closestSlot.slotTransform.position}");
                }

                controller.AssignedEncounterSlot = closestSlot; // Assign the slot to the controller

                controller.MoveToTarget(closestSlot.slotTransform, true, () =>
                {
                    remainingMovingCharacters--;
                    if (remainingMovingCharacters <= 0)
                    {
                        Debug.Log("All characters have reached their combat slots.");
                    }
                });
                closestSlot.isOccupied = true;
            }
        }

        int remainingMovingEnemies = Enemies.Count;

        foreach (EnemyManager enemy in Enemies)
        {
            if (enemy.TryGetComponent(out OverworldEnemyController controller))
            {
                controller.CancelPath(); // Cancel any existing pathfinding
                controller.SetShouldSprint(true); // Enable sprinting for combat movement

                EncounterSide enemySide = encounterSides.Where(s => s != closestSide).FirstOrDefault();
                EncounterSlot closestSlot = GetClosestSlot(enemy.transform.position, enemySide);

                if (closestSlot == null)
                {
                    Debug.LogWarning($"No available slots for {enemy.name} in the opposite encounter side.");
                    continue;
                }
                else
                {
                    Debug.Log($"{enemy.name} assigned to slot at {closestSlot.slotTransform.position}");
                }

                controller.AssignedEncounterSlot = closestSlot; // Assign the slot to the controller

                controller.MoveToTarget(closestSlot.slotTransform, true, () =>
                {
                    remainingMovingEnemies--;
                    if (remainingMovingEnemies <= 0)
                    {
                        Debug.Log("All enemies have reached their combat slots.");
                    }
                });

                closestSlot.isOccupied = true;
            }
        }

        while (remainingMovingCharacters > 0 || remainingMovingEnemies > 0)
        {
            await Task.Yield(); // Wait until all units have moved to their slots
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
        Debug.Log("Combat Encounter Started");
    }

    [Button("End Encounter")]
    public void EndEncounter()
    {
        // Cleanup encounter logic here, such as removing enemies, resetting UI, etc.
        Debug.Log("Combat Encounter Ended");

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

        if (enemy.overworldController.HasSpottedTarget)
        {
            // If the enemy is chasing or has spotted a target, do not wander
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
                    Debug.Log($"{enemy.name} is wandering to {randomPoint}");
                    _timeSinceLastAction[enemy] = 0f;
                }
            }
            else
            {
                _timeSinceLastAction[enemy] += Time.deltaTime;
            }
        }
    }

    public void HandleEnemyChaseBehavior(EnemyManager enemy)
    {
        if (enemy == null || enemy.overworldController == null)
        {
            Debug.LogWarning($"Enemy, OverworldController, or target is null for {enemy?.name}");
            return;
        }
        Transform target = PartyManager.Instance.PartyLeader.transform;

        if (target == null)
        {
            Debug.LogWarning("Target Transform is null.");
            return;
        }

        if (enemy.overworldController.CanSeeTarget(target))
        {
            enemy.overworldController.UpdateLastKnownTargetPosition(target.position);
        }

        if (enemy.overworldController.LastKnownTargetPosition == Vector3.positiveInfinity)
        {
            return;
        }

        enemy.overworldController.MoveToLastKnownTargetPosition(() =>
        {
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

    #endregion
}
