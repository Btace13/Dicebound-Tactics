using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventCharacterManager", menuName = "GameEvents/GameEventCharacterManager")]
    public class GameEventCharacterManager : GameEvent<CharacterManager>
    {
        public CharacterManager value;
    }
}
