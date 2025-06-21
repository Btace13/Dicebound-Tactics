using UnityEngine;
using UnityEngine.Events;

namespace TacticsToolkit
{
    public class GameEventCharacterManagerListener : GameEventListener<CharacterManager>
    {
        [SerializeField] private GameEventCharacterManager eventGameObject = null;
        [SerializeField] private UnityEvent<CharacterManager> response = null;

        public override GameEvent<CharacterManager> Event => eventGameObject;
        public override UnityEvent<CharacterManager> Response => response;
    }
}
