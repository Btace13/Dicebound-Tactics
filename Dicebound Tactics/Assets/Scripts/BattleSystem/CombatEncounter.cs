using UnityEngine;
using System.Collections.Generic;
using TacticsToolkit;
using Sirenix.OdinInspector;

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

    [Header("Encounter References")]
    [SerializeField] private EncounterSide[] encounterSides = new EncounterSide[2];
    public List<EnemyManager> Enemies = new List<EnemyManager>();

    [Header("Events")]
    public GameEventCombatEncounter OnEncounterStarted;
    public GameEventCombatEncounter OnEncounterEnded;

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
    public void StartEncounter()
    {
        if (OnEncounterStarted != null)
        {
            OnEncounterStarted.Raise(this);
        }

        // Initialize encounter logic here, such as spawning enemies, setting up UI, etc.
        Debug.Log("Combat Encounter Started");
    }

    [Button("End Encounter")]
    public void EndEncounter()
    {
        if (OnEncounterEnded != null)
        {
            OnEncounterEnded.Raise(this);
        }

        // Cleanup encounter logic here, such as removing enemies, resetting UI, etc.
        Debug.Log("Combat Encounter Ended");
    }
}
