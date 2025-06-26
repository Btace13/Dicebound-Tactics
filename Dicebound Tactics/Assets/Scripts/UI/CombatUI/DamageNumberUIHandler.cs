using UnityEngine;
using DamageNumbersPro;

public enum DamageNumberType
{
    Normal,
    Critical,
    Resistant,
    Immune,
    Heal
}

public class DamageNumberUIHandler : MonoBehaviour
{
    [SerializeField] DamageNumber damageNumberPrefab;
    [SerializeField] DamageNumber resistantDamageNumberPrefab;
    [SerializeField] DamageNumber criticalDamageNumberPrefab;
    [SerializeField] DamageNumber healDamageNumberPrefab;
    [SerializeField] DamageNumber immuneDamageNumberPrefab;

    public void ShowDamageNumber(float damageAmount, Vector3 position, DamageNumberType damageType = DamageNumberType.Normal)
    {
        print($"Showing damage number: {damageAmount} at position: {position} with type: {damageType}");

        // Set the type of damage number based on the provided type
        switch (damageType)
        {
            case DamageNumberType.Critical:
                criticalDamageNumberPrefab.Spawn(position, damageAmount);
                break;
            case DamageNumberType.Resistant:
                resistantDamageNumberPrefab.Spawn(position);
                break;
            case DamageNumberType.Immune:
                immuneDamageNumberPrefab.Spawn(position);
                break;
            case DamageNumberType.Heal:
                healDamageNumberPrefab.Spawn(position, damageAmount);
                break;
            default:
                damageNumberPrefab.Spawn(position, damageAmount);
                break;
        }
    }
}