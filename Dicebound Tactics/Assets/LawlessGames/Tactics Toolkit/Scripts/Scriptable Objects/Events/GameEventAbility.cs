using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventAbility", menuName = "GameEvents/GameEventAbility", order = 2)]
    public class GameEventAbility : GameEvent<AbilitySO>
    {
        public AbilitySO Ability;
    }
}
