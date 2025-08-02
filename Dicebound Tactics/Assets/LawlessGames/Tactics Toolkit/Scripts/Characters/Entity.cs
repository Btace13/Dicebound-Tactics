using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace TacticsToolkit
{
    //Parent Class for Characters and Enemys
    public class Entity : MonoBehaviour
    {
        [Header("Character Specific")]
        public Dice equippedDice;
        public List<AbilityContainer> abilitiesForUse;
        public Sprite portrait;
        public int CurrentHealth => statsContainer.CurrentHealth.statValue;
        public int CurrentAP => statsContainer.ActionPoints.statValue;
        public int LastRollValue => equippedDice.LastRollValue;
        public bool HasFreeAbility => nextAbilityFree;
        public bool IsTaunting => isTaunting;
        public bool IsOverloaded => isOverloaded;

        [Header("Weapon")]
        [SerializeField] private WeaponData StartingWeapon;
        public WeaponData CurrentWeapon { get; set; }
        private WeaponData equippedWeapon;
        public WeaponData EquippedWeapon { get { return equippedWeapon; } }

        [Header("Abilities")]
        public List<AbilitySO> abilityLoadout = new();

        [Header("Inventory")]
        public Inventory inventory;
        public List<CombatItem> combatItems => inventory.combatItems.Keys;

        [Header("Level")]
        public int level;
        public int experience = 0;
        public int requiredExperience = 0;

        [Header("General")]
        public bool isStunned = false;
        public int teamID = 0;
        [HideInInspector]
        public OverlayTile activeTile;
        public CharacterClass characterClass;
        [HideInInspector]
        public CharacterStats statsContainer;
        [HideInInspector]
        public int initiativeValue;

        [HideInInspector]
        public bool isAlive = true;
        [HideInInspector]
        public bool isActive;
        public HealthBarUI healthBar;
        [HideInInspector]
        public int previousTurnCost = -1;

        private bool isTargetted = false;

        public GameConfig gameConfig;

        private int initiativeBase = 1000;
        private float i;
        private bool nextAbilityFree = false;
        private Dictionary<string, float> tempModifiers = new();
        private float healOnNextHit = 0f;
        private bool isTaunting = false;
        private int tauntTurnsRemaining = 0;
        private bool isOverloaded = false;

        public event Action OnLevelChanged;
        public event Action<Entity> OnCharacterStatChanged;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button("Add EXP", ButtonSizes.Medium)]
        public void AddTestExp([Sirenix.OdinInspector.MinValue(1)] int expAmount = 50)
        {
            IncreaseExp(expAmount);
            // Debug.Log($"{name} gained {expAmount} EXP.");
        }
#endif

        protected virtual void Start()
        {
            SetupWeapon();
            SpawnCharacter();
        }

        public void SpawnCharacter()
        {
            SetAbilityList();
            SetDefaultAbilityList();
            SetStats();
            requiredExperience = gameConfig.GetRequiredExp(level);

            if (statsContainer != null)
            {
                initiativeValue = Mathf.RoundToInt(initiativeBase / GetStat(Stats.Speed).statValue);
            }
        }

        //Setup the statsContainer and scale up the stats based on level. 
        public void SetStats()
        {

            if (statsContainer == null)
            {
                statsContainer = ScriptableObject.CreateInstance<CharacterStats>();

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
            }
            for (int i = 0; i < level; i++)
            {
                LevelUpStats();
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (statsContainer == null)
                SetStats();
            if (isTargetted)
            {
                //Just a Color Lerp for when a character is targetted for an attack. 
                i += Time.deltaTime * 0.5f;
            }
        }

        public void SetDefaultAbilityList()
        {
            if (abilityLoadout == null || abilityLoadout.Count == 0)
            {
                var usableAbilities = characterClass.GetUsableAbilities(level);
                if (usableAbilities != null && usableAbilities.Count > 0)
                {
                    abilityLoadout = usableAbilities.Take(3).ToList();
                }
            }
        }


        //Get's all the available abilities from the characters class. 
        public void SetAbilityList()
        {
            abilitiesForUse = new List<AbilityContainer>();
            foreach (var ability in characterClass.abilitiesLegacy)
            {
                if (level >= ability.requiredLevel)
                    abilitiesForUse.Add(new AbilityContainer(ability));
            }
        }

        //Scale up attributes based on a weighted random.
        public void LevelUpStats()
        {
            float v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Health.ChangeStatValue(statsContainer.Health.statValue + Mathf.RoundToInt(characterClass.Health.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Mana.ChangeStatValue(statsContainer.Mana.statValue + Mathf.RoundToInt(characterClass.Mana.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Strength.ChangeStatValue(statsContainer.Strength.statValue + Mathf.RoundToInt(characterClass.Strength.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Endurance.ChangeStatValue(statsContainer.Endurance.statValue + Mathf.RoundToInt(characterClass.Endurance.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Speed.ChangeStatValue(statsContainer.Speed.statValue + Mathf.RoundToInt(characterClass.Speed.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Intelligence.ChangeStatValue(statsContainer.Intelligence.statValue + Mathf.RoundToInt(characterClass.Intelligence.baseStatModifier.Evaluate(v) * 10));

            statsContainer.CurrentHealth.ChangeStatValue(statsContainer.Health.statValue);
            statsContainer.CurrentMana.ChangeStatValue(statsContainer.Mana.statValue);
        }

        //Scale down attributes based on a weighted random. 
        public void LevelDownStats()
        {
            float v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Health.ChangeStatValue(statsContainer.Health.statValue - Mathf.RoundToInt(characterClass.Health.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Mana.ChangeStatValue(statsContainer.Mana.statValue - Mathf.RoundToInt(characterClass.Mana.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Strength.ChangeStatValue(statsContainer.Strength.statValue - Mathf.RoundToInt(characterClass.Strength.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Endurance.ChangeStatValue(statsContainer.Endurance.statValue - Mathf.RoundToInt(characterClass.Endurance.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Speed.ChangeStatValue(statsContainer.Speed.statValue - Mathf.RoundToInt(characterClass.Speed.baseStatModifier.Evaluate(v) * 10));
            v = UnityEngine.Random.Range(0f, 1f);
            statsContainer.Intelligence.ChangeStatValue(statsContainer.Intelligence.statValue - Mathf.RoundToInt(characterClass.Intelligence.baseStatModifier.Evaluate(v) * 10));

            statsContainer.CurrentHealth.ChangeStatValue(statsContainer.Health.statValue);
            statsContainer.CurrentMana.ChangeStatValue(statsContainer.Mana.statValue);
        }

        //Level up stats and get the new required experience for the next level. 
        public void LevelUpCharacter()
        {
            level++;
            LevelUpStats();
            requiredExperience = gameConfig.GetRequiredExp(level);
            OnLevelChanged?.Invoke();
        }

        public void IncreaseExp(int value)
        {
            experience += value;

            while (experience >= requiredExperience)
            {
                experience -= requiredExperience;
                LevelUpCharacter();
            }
        }

        //Level down stats and get the new required experience for the next level. 
        public void LevelDownCharacter()
        {
            level--;
            LevelDownStats();
            requiredExperience = gameConfig.GetRequiredExp(level);
            OnLevelChanged?.Invoke();
        }

        //Update the characters initiative after the perform an action. This is used for Dynamic Turn Order. 
        public void UpdateInitiative(int turnValue)
        {
            initiativeValue += Mathf.RoundToInt(turnValue / GetStat(Stats.Speed).statValue);
            previousTurnCost = turnValue;
        }

        public void InvokeCharacterStatChanged()
        {
            OnCharacterStatChanged?.Invoke(this);
        }

        //Entity is being targets for an attack. 
        public void SetTargeted(bool focused = false)
        {
            isTargetted = focused;
        }

        //Take damage from an attack or ability. 
        public void TakeDamage(int damage, bool ignoreDefence = false)
        {
            int damageToTake = CalculateDamageTakenWithModifiers(damage);

            if (damageToTake > 0)
            {
                statsContainer.CurrentHealth.statValue -= damageToTake;
                CombatManager.Instance.CombatUIHandler.damageNumberUIHandler.ShowDamageNumber(damageToTake, transform.position, DamageNumberType.Normal);
                CameraManager.Instance?.ShakeActiveCamera();
                //CameraShake.Shake(0.125f, 0.1f);

                UpdateCharacterUI();

                if (GetStat(Stats.CurrentHealth).statValue <= 0)
                {
                    Die();
                }
            }
        }

        public virtual void Die()
        {
            isAlive = false;
            StartCoroutine(DieCoroutine());
            UnlinkCharacterToTile();
        }

        public void HealEntity(int value)
        {
            statsContainer.CurrentHealth.statValue += value;
            // Debug.Log($"{name} healed for {value} HP.");
            UpdateCharacterUI();
        }

        //basic example if using a defensive stat
        private int CalculateDamage(int damage)
        {
            var endurance = (float)GetStat(Stats.Endurance).statValue;
            float percentage = ((endurance / (float)damage) * 100) / 2;

            percentage = percentage > 75 ? 75 : percentage;

            int damageToTake = damage - Mathf.CeilToInt((float)(percentage / 100f) * (float)damage);
            return damageToTake;
        }

        //Get a perticular stat object. 
        public Stat GetStat(Stats statName)
        {
            if (statsContainer == null)
                return null;

            switch (statName)
            {
                case Stats.Health:
                    return statsContainer.Health;
                case Stats.Mana:
                    return statsContainer.Mana;
                case Stats.Strength:
                    return statsContainer.Strength;
                case Stats.Endurance:
                    return statsContainer.Endurance;
                case Stats.Speed:
                    return statsContainer.Speed;
                case Stats.Intelligence:
                    return statsContainer.Intelligence;
                case Stats.MoveRange:
                    return statsContainer.MoveRange;
                case Stats.CurrentHealth:
                    return statsContainer.CurrentHealth;
                case Stats.CurrentMana:
                    return statsContainer.CurrentMana;
                case Stats.AttackRange:
                    return statsContainer.AttackRange;
                case Stats.ActionPoints:
                    return statsContainer.ActionPoints;
                case Stats.CarriedOverActionPoints:
                    return statsContainer.CarriedOverActionPoints;
                default:
                    return statsContainer.Health;
            }
        }

        //What happens when a character dies. 
        public IEnumerator DieCoroutine()
        {
            float DegreesPerSecond = 360f;
            Vector3 currentRot, targetRot = new Vector3();
            currentRot = transform.eulerAngles;
            targetRot.z = currentRot.z + 90; // calculate the new angle

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            while (currentRot.z < targetRot.z)
            {
                currentRot.z = Mathf.MoveTowardsAngle(currentRot.z, targetRot.z, DegreesPerSecond * Time.deltaTime);
                transform.eulerAngles = currentRot;
                yield return null;
            }
        }

        private void SetupWeapon()
        {
            if (StartingWeapon != null)
            {
                CurrentWeapon = StartingWeapon;

                if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentGameState == GameState.Combat)
                {
                    EquipWeapon(CurrentWeapon);
                }
            }
        }

        public void EquipWeapon(WeaponData weapon)
        {
            if (weapon != null)
            {
                equippedWeapon = weapon;

                UnitAnimationHandler animationHandler = GetComponentInChildren<UnitAnimationHandler>(true);
                animationHandler.ToggleEquipWeapon(weapon);
            }
            else
            {
                // Debug.Log("Attempting to unequip weapon.");
                equippedWeapon = null;

                UnitAnimationHandler animationHandler = GetComponentInChildren<UnitAnimationHandler>(true);
                animationHandler.ToggleEquipWeapon(null);
            }
        }

        //Updates the characters healthbar. 
        private void UpdateCharacterUI()
        {
            if (healthBar)
                healthBar.SetHealth((float)statsContainer.CurrentHealth.statValue, statsContainer.Health.statValue);

            InvokeCharacterStatChanged();
        }

        //Change characters mana
        public void UpdateMana(int value) => statsContainer.CurrentMana.statValue -= value;

        //Attach an effect to the Entity from a tile or ability. 
        public void AttachEffect(ScriptableEffect scriptableEffect)
        {
            if (scriptableEffect)
            {
                var statToEffect = GetStat(scriptableEffect.statKey);

                if (statToEffect.statMods.FindIndex(x => x.statModName == scriptableEffect.name) != -1)
                {
                    int modIndex = statToEffect.statMods.FindIndex(x => x.statModName == scriptableEffect.name);
                    statToEffect.statMods[modIndex] = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name);
                }
                else
                    statToEffect.statMods.Add(new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name));
            }
        }

        //Effects that don't have a duration can just be applied straight away. 
        public void ApplySingleEffects(ScriptableEffect scriptableEffect)
        {
            var statMod = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name);
            Stat value = statsContainer.getStat(scriptableEffect.GetStatKey());
            value.ApplySingleStatMod(statMod);
            UpdateCharacterUI();
        }

        //Effects that don't have a duration should be manually removed. 
        public void UndoEffect(ScriptableEffect scriptableEffect)
        {
            var statMod = new StatModifier(scriptableEffect.statKey, scriptableEffect.Value, scriptableEffect.Duration, scriptableEffect.Operator, scriptableEffect.name);
            Stat value = statsContainer.getStat(scriptableEffect.GetStatKey());
            value.UndoStatMod(statMod);
            UpdateCharacterUI();
        }


        //Apply all the currently attached effects. Happens when a new turn begins. 
        public void ApplyEffects()
        {
            var fields = typeof(CharacterStats).GetFields();

            foreach (var item in fields)
            {
                var type = item.FieldType;
                Stat value = (Stat)item.GetValue(statsContainer);

                value.ApplyStatMods();
            }

            UpdateCharacterUI();
        }

        //Gets Entities ability. 
        public AbilityContainer GetAbilityByName(string abilityName)
        {
            return abilitiesForUse.Find(x => x.ability.Name == abilityName);
        }

        public virtual void StartTurn()
        {
            var fields = typeof(CharacterStats).GetFields();

            foreach (var item in fields)
            {
                if (statsContainer == null)
                {
                    // Debug.LogError("StatsContainer is not assigned for " + name);
                    continue;
                }

                var type = item.FieldType;
                Stat value = (Stat)item.GetValue(statsContainer);

                value.TickStatMods();
            }
        }

        public virtual void CharacterMoved()
        {
        }

        //When an Entity moves, link it to the tiles it's standing on. 
        public void LinkCharacterToTile(OverlayTile tile)
        {
            UnlinkCharacterToTile();
            tile.activeCharacter = this;
            tile.isBlocked = true;
            activeTile = tile;
        }

        //Unlink an entity from a previous tile it was standing on. 
        public void UnlinkCharacterToTile()
        {
            if (activeTile)
            {
                activeTile.activeCharacter = null;
                activeTile.isBlocked = false;
                activeTile = null;
            }
        }

        public int CalculateDamageWithModifiers(int baseDamage)
        {
            float modifiedDamage = baseDamage;

            foreach (var modifier in tempModifiers)
            {
                switch (modifier.Key)
                {
                    case "BonusDamage":
                        modifiedDamage += ((modifier.Value / 100f) * baseDamage);
                        tempModifiers.Remove(modifier.Key);
                        break;
                    case "PowerStack":
                        modifiedDamage += (baseDamage * (modifier.Value / 100f));
                        break;
                    default:
                        // Debug.LogWarning($"Unknown damage modifier key: {modifier.Key}");
                        break;
                }
            }

            return Mathf.RoundToInt(modifiedDamage);
        }

        public int CalculateDamageTakenWithModifiers(int baseDamage)
        {
            float modifiedDamage = baseDamage;

            foreach (var modifier in tempModifiers)
            {
                switch (modifier.Key)
                {
                    case "DamageReduction":
                        modifiedDamage -= ((modifier.Value / 100f) * baseDamage);
                        // Debug.Log($"Damage reduced by {modifier.Value}%");
                        break;
                    default:
                        // Debug.LogWarning($"Unknown damage taken modifier key: {modifier.Key}");
                        break;
                }
            }

            return modifiedDamage > 0 ? Mathf.RoundToInt(modifiedDamage) : 0;
        }

        public void HealOnHit(int damageDealt)
        {
            if (healOnNextHit > 0f)
            {
                int healAmount = Mathf.RoundToInt((healOnNextHit / 100) * damageDealt);
                HealEntity(healAmount);
                healOnNextHit = 0f;
            }
        }

        public void Reset()
        {
            isAlive = true;
            isStunned = false;
            isTargetted = false;
            statsContainer.CurrentHealth.statValue = statsContainer.Health.statValue;
            statsContainer.CurrentMana.statValue = statsContainer.Mana.statValue;
            statsContainer.ActionPoints.statValue = 0;
            statsContainer.CarriedOverActionPoints.statValue = 0;
            OnCharacterStatChanged?.Invoke(this);
            UpdateCharacterUI();
        }

        public bool SpendAP(int apCost)
        {
            int cost = apCost;

            if (nextAbilityFree)
            {
                nextAbilityFree = false;
                return true;
            }

            if (tempModifiers.TryGetValue("APCostReduction", out float reduction))
            {
                cost -= Mathf.RoundToInt(reduction);
                cost = Mathf.Max(0, cost);
                tempModifiers.Remove("APCostReduction");
            }

            if (cost <= statsContainer.ActionPoints.statValue)
            {
                statsContainer.ActionPoints.statValue -= cost;
                return true;
            }
            else
            {
                // Debug.LogWarning("Not enough Action Points to perform this action.");
                return false;
            }
        }

        public void ResetActionPoints()
        {
            statsContainer.ActionPoints.statValue = 0;
            statsContainer.CarriedOverActionPoints.statValue = 0;
            equippedDice.LastRollModifier = null;
            OnCharacterStatChanged?.Invoke(this);
        }

        public void RollDice()
        {
            if (equippedDice == null)
            {
                // Debug.LogError("Character Dice is not assigned for " + name);
                return;
            }

            if (statsContainer == null)
            {
                // Debug.LogError("StatsContainer is not assigned for " + name);
                return;
            }

            DiceSide diceRoll = equippedDice.Roll();
            EventManager.TriggerModifierApplied(diceRoll.modifier); 
            int totalAP = diceRoll.value + statsContainer.CarriedOverActionPoints.statValue;

            statsContainer.ActionPoints.statValue = statsContainer.ActionPoints.statValue += totalAP;
            statsContainer.CarriedOverActionPoints.statValue = 0;
            diceRoll.modifier.Apply(this);
            OnCharacterStatChanged?.Invoke(this);
        }

        public void ApplyDiceRoll(int value)
        { 
            if (statsContainer == null)
            {
                Debug.LogError("StatsContainer is not assigned for " + name);
                return;
            }

            DiceSide diceRoll = equippedDice.ApplyRoll(value - 1);
            diceRoll.modifier.Apply(this);
            int totalAP = diceRoll.value + statsContainer.CarriedOverActionPoints.statValue;

            statsContainer.ActionPoints.statValue = statsContainer.ActionPoints.statValue += totalAP;
            statsContainer.CarriedOverActionPoints.statValue = 0;
            EventManager.TriggerModifierApplied(diceRoll.modifier);
            OnCharacterStatChanged?.Invoke(this);
        }

        public void ResetTempModifiers()
        {
            tempModifiers.Clear();
            nextAbilityFree = false;
            isOverloaded = false;
            healOnNextHit = 0f;

            if (isTaunting)
            {
                tauntTurnsRemaining--;
                if (tauntTurnsRemaining <= 0)
                    isTaunting = false;
            }
        }

        public void AddActionPoints(int amount)
        {
            statsContainer.ActionPoints.statValue += amount;
        }

        public void SetNextAbilityFree()
        {
            nextAbilityFree = true;
        }

        public void AddTempModifier(string key, float value)
        {
            if (!tempModifiers.ContainsKey(key))
                tempModifiers[key] = 0;

            tempModifiers[key] += value;
        }

        public void ApplyFocusBoost()
        {
            AddTempModifier("BonusDamage", 10f);
            AddTempModifier("APCostReduction", 1f);
        }

        public void AddPowerStack(int percentage)
        {
            AddTempModifier("PowerStack", percentage);
        }

        public void ApplyTemporaryDefenseBuff(float reduction)
        {
            AddTempModifier("DamageReduction", reduction);
        }

        public void SetHealOnNextHit(int amount)
        {
            healOnNextHit = amount;
        }

        public void ApplyTaunt()
        {
            isTaunting = true;
            tauntTurnsRemaining = 1;
        }

        public void ApplyOverload()
        {
            isOverloaded = true;
        }

        public void ApplyOverloadHit(int damageDealt, Entity originalTarget)
        {
            if (isOverloaded)
            {
                var enemies = FindObjectsByType<Entity>(FindObjectsSortMode.None).Where(e => e.teamID != teamID && e.isAlive).ToList();
                if (enemies.Count == 0)
                    return;

                Entity randomEnemy;
                if (enemies.Count == 1)
                {
                    randomEnemy = enemies[0];
                }
                else
                {
                    do
                    {
                        randomEnemy = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                    } while (randomEnemy == originalTarget);
                }
                randomEnemy.TakeDamage(damageDealt);
                // Debug.Log($"{name} overloads and hits {randomEnemy.name} for {damageDealt} damage.");
                isOverloaded = false;
            }
        }

        public void HealTeam(int teamID, int amount)
        {
            var team = FindObjectsByType<Entity>(FindObjectsSortMode.None).Where(e => e.teamID == teamID && e.isAlive);
            foreach (var entity in team)
            {
                entity.HealEntity(amount);
            }
        }

        public void HealTeamByPercentage(int teamID, float percent)
        {
            var team = FindObjectsByType<Entity>(FindObjectsSortMode.None).Where(e => e.teamID == teamID && e.isAlive);
            foreach (var entity in team)
            {
                int healAmount = Mathf.RoundToInt((percent / 100f) * entity.statsContainer.Health.statValue);
                entity.HealEntity(healAmount);
            }
        }

        public void HealEntityByPercentage(float percent)
        {
            int healAmount = Mathf.RoundToInt((percent / 100f) * statsContainer.Health.statValue);
            HealEntity(healAmount);
        }
    }
}
