using UnityEngine;
using System.IO;
using UnityEditor;
using Microsoft.Unity.VisualStudio.Editor;

namespace TacticsToolkit
{
    [RequireComponent(typeof(OverworldCharacterController))]
    //Script for a playable character.
    public class CharacterManager : Entity, IOverworldControllable
    {
        public string characterId;
        public Sprite characterFullBodyImage;
        public bool IsControllable { get; set; } = true;
        public bool IsControlled { get; set; } = false;
        public OverworldCharacterController OverworldCharacterController { get; set; }
        private void Awake()
        {
            OverworldCharacterController = GetComponent<OverworldCharacterController>();
            
            LoadOrCreateStats();
            LoadOrCreateDie();
        }

        private void LoadOrCreateStats()
        {
            string path = $"CharacterStats/{characterId}"; // Resources path (no extension)
            statsContainer = Resources.Load<CharacterStats>(path);

            if (statsContainer == null)
            {
                statsContainer = ScriptableObject.CreateInstance<CharacterStats>();
                SetupStats();

#if UNITY_EDITOR
                string fullPath = "Assets/Resources/CharacterStats";
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                string assetPath = $"{fullPath}/{characterId}.asset";
                AssetDatabase.CreateAsset(statsContainer, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#else
                // Debug.LogWarning("Cannot create ScriptableObject at runtime outside of the editor.");
#endif
            }

            statsContainer.ResetAP();
            InvokeCharacterStatChanged();
        }

        public void LoadOrCreateDie()
        {
            string path = $"CharacterDice/{characterId}Dice"; // Resources path (no extension)
            equippedDice = Resources.Load<Dice>(path);

            if (equippedDice == null)
            {
                equippedDice = ScriptableObject.CreateInstance<Dice>();

#if UNITY_EDITOR
                string fullPath = "Assets/Resources/CharacterDice";
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                string assetPath = $"{fullPath}/{characterId}Dice.asset";
                AssetDatabase.CreateAsset(equippedDice, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#else
                // Debug.LogWarning("Cannot create ScriptableObject at runtime outside of the editor.");
#endif
            }
        }

        private void SetupStats()
        {
            if (statsContainer != null)
            {
                statsContainer.Health = new Stat(Stats.Health, characterClass.Health.baseStatValue, this);
                statsContainer.Mana = new Stat(Stats.Mana, characterClass.Mana.baseStatValue, this);
                statsContainer.Strength = new Stat(Stats.Strength, characterClass.Strength.baseStatValue, this);
                statsContainer.Endurance = new Stat(Stats.Endurance, characterClass.Endurance.baseStatValue, this);
                statsContainer.Speed = new Stat(Stats.Speed, characterClass.Speed.baseStatValue, this);
                statsContainer.Intelligence = new Stat(Stats.Intelligence, characterClass.Intelligence.baseStatValue, this);
                statsContainer.MoveRange = new Stat(Stats.MoveRange, characterClass.MoveRange, this);
                statsContainer.AttackRange = new Stat(Stats.AttackRange, characterClass.AttackRange, this);
                statsContainer.CurrentHealth = new Stat(Stats.CurrentHealth, characterClass.Health.baseStatValue, this);
                statsContainer.CurrentMana = new Stat(Stats.CurrentMana, characterClass.Mana.baseStatValue, this);
                statsContainer.ActionPoints = new Stat(Stats.ActionPoints, 0, this);
                statsContainer.CarriedOverActionPoints = new Stat(Stats.CarriedOverActionPoints, 0, this);

                for (int i = 0; i < level; i++)
                {
                    LevelUpStats();
                }
            }
        }

        public bool HasEnoughApToUseAbility(AbilitySO ability)
        {
            if (ability != null)
            {
                return CurrentAP >= ability.apCost;
            }
            return false;
        }
    }
}