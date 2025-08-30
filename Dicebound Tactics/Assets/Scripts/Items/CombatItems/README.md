# Combat Items System Documentation

## Overview
The combat items system allows players to use healing and revival items during combat encounters. This system includes basic heal potions and revive items with customizable properties.

## Item Types

### 1. Heal Items (CombatItemType.Heal)
- **Purpose**: Restore health to living allies or self
- **Targeting**: Can target self and/or allies (configurable)
- **Effects**: 
  - Fixed heal amount (`healAmount`)
  - Percentage-based healing (`healPercentage`)
  - Optional debuff removal (`removeDebuffs`)
  - Optional heal over time effect (`healOverTime`)

### 2. Revive Items (CombatItemType.Revive)
- **Purpose**: Bring fallen allies back to life
- **Targeting**: Can target dead allies only
- **Effects**:
  - Revive with specified health amount/percentage
  - Optional mana restoration (`restoreMana`)
  - Optional revival protection buff (`applyRevivalBuff`)

## How to Create Items

### Creating a Basic Heal Potion
1. Right-click in Project window
2. Navigate to `Create > Items > Heal Potion`
3. Configure the following properties:
   - `ItemName`: Display name (e.g., "Health Potion")
   - `Description`: Item description
   - `healAmount`: Fixed HP to restore (e.g., 50)
   - `healPercentage`: Percentage of max HP to restore (e.g., 25%)
   - `apCost`: Action points required to use (typically 1)
   - `range`: Maximum distance to target (typically 4)

### Creating a Revive Item
1. Right-click in Project window
2. Navigate to `Create > Items > Revive Item`
3. Configure the following properties:
   - `ItemName`: Display name (e.g., "Revive Scroll")
   - `Description`: Item description
   - `healPercentage`: Percentage of max HP to revive with (e.g., 25%)
   - `apCost`: Action points required (typically 2)
   - `canTargetDeadAllies`: Must be true for revive items
   - `restoreMana`: Whether to also restore mana
   - `applyRevivalBuff`: Whether to apply temporary protection

## Properties Reference

### Core Properties
- `itemType`: Enum defining item behavior (Heal, Revive, Buff, Other)
- `apCost`: Action points consumed when using the item
- `range`: Maximum distance from user to target

### Healing Properties
- `healAmount`: Fixed HP amount to restore
- `healPercentage`: Percentage of target's max HP to restore
- Both can be used together for combined healing

### Targeting Options
- `canTargetSelf`: Allow using on self
- `canTargetAllies`: Allow using on allies
- `canTargetEnemies`: Allow using on enemies
- `canTargetDeadAllies`: Allow using on dead allies (required for revive items)

### Special Properties (HealPotion)
- `removeDebuffs`: Remove negative status effects
- `healOverTime`: Apply healing over multiple turns
- `healOverTimeDuration`: Number of rounds for HoT effect
- `healOverTimeAmount`: HP restored per round

### Special Properties (ReviveItem)
- `restoreMana`: Also restore mana when reviving
- `manaAmount`: Fixed mana amount to restore
- `manaPercentage`: Percentage of max mana to restore
- `applyRevivalBuff`: Apply temporary damage reduction
- `buffDuration`: Duration of revival protection
- `damageReduction`: Percentage damage reduction

## Usage in Game

### For Players
1. Select the item from the item panel during combat
2. Click on a valid target (highlighted based on item targeting rules)
3. The item effect is applied and the item is consumed

### Item Validation
The system automatically validates:
- User has enough Action Points
- Target is within range
- Target meets the item's targeting criteria
- Special conditions (e.g., target must be dead for revive items)

## Example Items

### Health Potion
- Restores 50 HP
- Can target self and allies
- Costs 1 AP
- Range: 4 units

### Super Health Potion
- Restores 50% of max HP
- Removes all debuffs
- Can target self and allies
- Costs 1 AP
- Range: 4 units

### Revive Scroll
- Revives with 25% health and 25% mana
- Applies 50% damage reduction for 3 rounds
- Can only target dead allies
- Costs 2 AP
- Range: 4 units

## Technical Notes

### Integration with Combat System
- Items are selected through the `ItemPanel` UI
- `CombatManager.ItemSelected()` handles targeting setup
- `CombatManager.ExecuteAction()` executes item effects
- Items are automatically removed from inventory after successful use

### Events and Effects
- Healing shows green damage numbers
- Revival logs are displayed in console
- Character UI updates automatically after item use
- Temporary modifiers are applied through the existing system

### Extensibility
The system is designed to be easily extended:
- Add new `CombatItemType` enum values
- Override `UseItem()` method for custom behavior
- Create new derived classes for specialized items
- Add custom validation in `CanUseOn()` method
