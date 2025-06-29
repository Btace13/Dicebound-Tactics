using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventCombatEncounterListener : GameEventListener<CombatEncounter>
    {
        [SerializeField] private GameEventCombatEncounter eventGameObject = null;
        [SerializeField] private UnityEvent<CombatEncounter> response = null;

        public override GameEvent<CombatEncounter> Event => eventGameObject;
        public override UnityEvent<CombatEncounter> Response => response;
    }
}
