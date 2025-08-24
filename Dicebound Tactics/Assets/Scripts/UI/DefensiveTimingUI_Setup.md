# Defensive Timing UI Setup Guide

## UI Hierarchy Structure

Create the following UI hierarchy in your scene:

```
Canvas
└── DefensiveTimingPanel (GameObject)
    ├── DefensiveTimingUI (Script Component)
    ├── Background (Image - Semi-transparent black)
    ├── InstructionText (TextMeshPro - "Press the button sequence!")
    ├── TimerBar (Slider or Image with Image Type: Filled)
    └── ButtonContainer (Horizontal Layout Group)
        └── ButtonPromptPrefab (Prefab - will be instantiated at runtime)
```

## Component Setup

### DefensiveTimingPanel
- Add `DefensiveTimingUI` script
- Configure references in inspector:
  - Defensive Prompt Panel: Self reference
  - Instruction Text: Reference to InstructionText
  - Timer Fill Bar: Reference to TimerBar (Image component)
  - Button Prompt Container: Reference to ButtonContainer
  - Button Prompt Prefab: Reference to your button prefab

### ButtonPromptPrefab Structure
```
ButtonPromptPrefab (GameObject)
├── ButtonPrompt (Script Component)
├── Background (Image - Button background)
└── Icon (Image - Button icon)
```

## Button Icons
You'll need sprites for:
- Left Gamepad (X/Square)
- Right Gamepad (B/Circle)  
- Top Gamepad (Y/Triangle)
- Bottom Gamepad (A/Cross)

Assign these in the DefensiveTimingUI inspector.

## Prefab Setup Steps

1. **Create the UI Canvas** (if you don't have one)
2. **Create DefensiveTimingPanel** as child of Canvas
3. **Add DefensiveTimingUI script** to DefensiveTimingPanel
4. **Create child UI elements** as shown in hierarchy
5. **Create ButtonPromptPrefab** with ButtonPrompt script
6. **Save ButtonPromptPrefab** in your prefabs folder
7. **Assign all references** in DefensiveTimingUI inspector
8. **Test in play mode**

## Styling Tips

- Use DOTween for smooth animations
- Set Panel to scale from 0 to 1 for entrance effect
- Use color transitions for timer urgency (green → yellow → red)
- Add punch scale animations for button presses
- Consider adding sound effects for button presses and completion

## Integration

The system automatically detects if DefensiveTimingUI.Instance exists and uses it. If not, it falls back to console logs for testing.
