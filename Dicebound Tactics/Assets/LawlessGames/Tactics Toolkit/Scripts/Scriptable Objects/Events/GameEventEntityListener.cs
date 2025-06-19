using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventEntityListener : GameEventListener<Entity>
    {
        [SerializeField] private GameEventEntity eventGameObject = null;
        [SerializeField] private UnityEvent<Entity> response = null;

        public override GameEvent<Entity> Event => eventGameObject;
        public override UnityEvent<Entity> Response => response;
    }
}
