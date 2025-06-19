using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventEntity", menuName = "GameEvents/GameEventEntity")]
    public class GameEventEntity : GameEvent<Entity>
    {
        public Entity value;
    }
}
