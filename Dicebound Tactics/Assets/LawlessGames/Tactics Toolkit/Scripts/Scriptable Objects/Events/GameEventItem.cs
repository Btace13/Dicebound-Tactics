using System;
using UnityEngine;

namespace TacticsToolkit
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameEventItem", menuName = "GameEvents/GameEventItem", order = 2)]
    public class GameEventItem : GameEvent<GameObject>
    {
        public CombatItem Item;
    }
}
