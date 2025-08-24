# Defensive Timing System - Setup Guide

## Overview
The defensive timing system allows players to reduce incoming damage by successfully completing button sequences within a time window before enemy attacks land.

## How to Configure

### 1. On DamageAbilitySO Assets
In the inspector for any DamageAbilitySO, you'll find these new settings:

- **Defensive Window Duration**: How long before the hit can the player start the defensive sequence (default: 1.5 seconds)
- **Button Sequence Time Limit**: How long the player has to complete the sequence (default: 1.0 seconds) 
- **Required Button Sequence**: Array of button names the player must press in order

### 2. Supported Button Names
- "LeftGamepad" - X/Square button
- "RightGamepad" - B/Circle button  
- "TopGamepad" - Y/Triangle button
- "BottomGamepad" - A/Cross button

### 3. Example Configurations

**Easy Defense** (for weaker enemies):
- Window Duration: 2.0 seconds
- Time Limit: 1.5 seconds
- Sequence: ["LeftGamepad"]

**Hard Defense** (for boss enemies):
- Window Duration: 1.0 seconds
- Time Limit: 0.8 seconds
- Sequence: ["LeftGamepad", "RightGamepad", "TopGamepad"]

## Damage Reduction
- **Successful Defense**: 50% damage reduction
- **Failed Defense**: Full damage taken

To modify the damage reduction, edit the `ApplyDamage` method in `DamageAbilitySO.cs`:
```csharp
finalDamage = Mathf.RoundToInt(amount * 0.5f); // 50% reduction
```

## Testing
1. Create a DamageAbilitySO asset
2. Set defensive timing values
3. Have an enemy use it against a player character
4. Watch console logs for defensive window notifications
5. Press the button sequence when prompted

## Future Enhancements
- Visual UI indicators for button prompts
- Audio cues for timing windows
- Different defensive actions (parry, dodge, block)
- Varying damage reduction based on timing accuracy
