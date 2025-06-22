using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventEnemyManagerListener : GameEventListener<EnemyManager>
    {
        [SerializeField] private GameEventEnemyManager eventGameObject = null;
        [SerializeField] private UnityEvent<EnemyManager> response = null;

        public override GameEvent<EnemyManager> Event => eventGameObject;
        public override UnityEvent<EnemyManager> Response => response;
    }
}
