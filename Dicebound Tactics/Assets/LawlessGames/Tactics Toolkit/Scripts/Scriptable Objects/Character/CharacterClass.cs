using System.Collections.Generic;
using UnityEngine;


namespace TacticsToolkit
{
    [CreateAssetMenu(fileName = "CharacterClass", menuName = "ScriptableObjects/CharacterClass", order = 1)]
    public class CharacterClass : ScriptableObject
    {
        [Header("Class Info")]
        public string className;
        public Sprite classIcon;

        [Header("Base Stats")]
        public BaseStat Health;
        public BaseStat Mana;
        public BaseStat Strenght;
        public BaseStat Endurance;
        public BaseStat Speed;
        public BaseStat Intelligence;

        public int MoveRange;
        public int AttackRange;


        public List<Ability> abilities;

    }
}
