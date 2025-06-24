using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyDiceProfile", menuName = "Enemy/Enemy Dice Profile")]
public class EnemyDiceProfile : ScriptableObject
{
    [Header("Die Side Count")]
    public int minSides = 4;
    public int maxSides = 6;

    [Header("Possible Modifiers")]
    public List<DiceModifier> possibleModifiers = new();
}
