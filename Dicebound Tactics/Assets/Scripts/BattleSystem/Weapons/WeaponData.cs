using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Dicebound Tactics/Weapons/Weapon Data", order = 1)]
public class WeaponData : ScriptableObject
{
    public GameObject ItemPrefab;
    public string WeaponName;
    public float Damage;
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    public float EquipTime = 0.5f; // Time taken to equip the weapon

    [Header("UI Settings")]
    public Sprite Icon;
    public Sprite WeaponTypeIcon;
    public Color RarityColor;
    [Range(0, 5)]
    public int rarityAmount = 1; // Number of times the rarity color is applied
}
