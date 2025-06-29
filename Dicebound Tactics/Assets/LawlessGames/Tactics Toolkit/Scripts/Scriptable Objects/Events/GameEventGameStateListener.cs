using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventGameStateListener : GameEventListener<GameState>
    {
        [SerializeField] private GameEventGameState eventGameObject = null;
        [SerializeField] private UnityEvent<GameState> response = null;

        public override GameEvent<GameState> Event => eventGameObject;
        public override UnityEvent<GameState> Response => response;
    }
}
