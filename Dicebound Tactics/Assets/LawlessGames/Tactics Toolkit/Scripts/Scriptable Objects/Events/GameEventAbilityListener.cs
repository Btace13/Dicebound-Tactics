using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventAbilityListener : GameEventListener<AbilitySO>
    {
        [SerializeField] private GameEvent<AbilitySO> ability = null;
        [SerializeField] private UnityEvent<AbilitySO> response = null;

        public override GameEvent<AbilitySO> Event => ability;
        public override UnityEvent<AbilitySO> Response => response;
    }
}
