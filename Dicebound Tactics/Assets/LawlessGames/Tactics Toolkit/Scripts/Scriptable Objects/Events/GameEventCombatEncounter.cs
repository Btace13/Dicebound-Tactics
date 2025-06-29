using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventCombatEncounter", menuName = "GameEvents/GameEventCombatEncounter")]
    public class GameEventCombatEncounter : GameEvent<CombatEncounter>
    {
        public CombatEncounter value;
    }
}
