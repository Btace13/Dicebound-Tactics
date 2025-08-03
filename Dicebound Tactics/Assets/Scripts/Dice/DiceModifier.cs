using UnityEngine;
using TacticsToolkit;

public abstract class DiceModifier : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    [TextArea] public string Description;
    public GameObject modifierTriggerEffectPrefab;

    // Called when the modifier is triggered
    public virtual void Apply(Entity user)
    {
        if (modifierTriggerEffectPrefab != null && user is CharacterManager characterManager)
        {
            GameObject effect = Instantiate(modifierTriggerEffectPrefab, characterManager.transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up the effect after 2 seconds
        }
    }
}
