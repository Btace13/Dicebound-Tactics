using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventEnemyManager", menuName = "GameEvents/GameEventEnemyManager")]
    public class GameEventEnemyManager : GameEvent<EnemyManager>
    {
        public EnemyManager value;
    }
}
