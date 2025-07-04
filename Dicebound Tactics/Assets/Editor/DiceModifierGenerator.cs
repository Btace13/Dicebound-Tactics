using UnityEngine;
using UnityEditor;
using System.IO;

public class DiceModifierGenerator : EditorWindow
{
    [MenuItem("Tools/Dice/Generate All Dice Modifiers")]
    public static void GenerateDiceModifiers()
    {
        string folderPath = "Assets/DiceModifiers";

        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/", "DiceModifiers");

        CreateModifier("+1 AP", "Gain 1 additional Action Point.", new GainAPModifier { modifierValue = 1 });
        CreateModifier("+2 AP", "Gain 2 additional Action Points.", new GainAPModifier { modifierValue = 2 });
        CreateModifier("+3 AP", "Gain 3 additional Action Points.", new GainAPModifier { modifierValue = 3 });

        CreateModifier("AP Steal", "Steal 1 AP from a random enemy.", new APStealModifier { amount = 1 });

        CreateModifier("Bonus Damage", "Next ability deals +10% damage.", new BonusDamageModifier { bonusPercent = 10 });
        CreateModifier("Self Heal", "Restore 10% of max HP.", new SelfHealModifier { percent = 10 });
        CreateModifier("Team Heal", "All allies restore 15 HP.", new TeamHealModifier { percent = 15 });

        CreateModifier("Grant Ally AP", "Grant 1 AP to a random ally.", new GrantAllyAPModifier { amount = 1 });
        CreateModifier("AP Refund", "Next ability costs 0 AP.", new APRefundModifier());

        CreateModifier("Focus Boost", "Next ability +10% damage, -1 AP cost.", new FocusBoostModifier());

        CreateModifier("Reinforce", "Take 25% less damage this turn.", new ReinforceModifier { defenseBoost = 25 });

        CreateModifier("Power Stack", "+5% damage bonus to each ability this turn.", new PowerStackModifier { percent = 5 });

        CreateModifier("Heal on Hit", "Heal 15% of damage dealt on next ability.", new HealOnHitModifier { percent = 15 });

        CreateModifier("Taunt", "Enemies prioritize you until next turn.", new TauntModifier());

        CreateModifier("Overload", "Your next ability targets an extra enemy.", new OverloadModifier());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ All DiceModifiers generated!");
    }

    private static void CreateModifier(string name, string description, DiceModifier instance)
    {
        instance.Name = name;
        instance.Description = description;

        string path = $"Assets/DiceModifiers/{name}.asset";
        AssetDatabase.CreateAsset(instance, path);
    }
}
