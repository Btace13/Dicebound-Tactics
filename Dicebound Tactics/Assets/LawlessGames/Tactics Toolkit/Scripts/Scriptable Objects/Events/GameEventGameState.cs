using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventGameState", menuName = "GameEvents/GameEventGameState")]
    public class GameEventGameState : GameEvent<GameState>
    {
        public GameState value;
    }
}
