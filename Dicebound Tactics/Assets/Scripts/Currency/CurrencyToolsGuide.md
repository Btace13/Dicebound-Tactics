# Currency System Tools Usage Guide

## When to Use CurrencySystemWindow vs CurrencyDebugger

### 🔧 **CurrencySystemWindow** (Editor Only)
**Use for:** System management, setup, and comprehensive testing

**Features:**
- Complete system overview and health check
- Create/manage currency managers, shops, forges
- Batch currency operations (max all, reset all)
- Advanced pickup creation (scatter spawns, victory drops)
- Prefab configuration status
- Scene component inventory

**Best for:**
- Setting up the currency system
- Testing different currency amounts
- Creating test scenarios
- Managing scene components
- Batch operations

---

### 🐛 **CurrencyDebugger** (Runtime Component)
**Use for:** Real-time debugging and event monitoring

**Features:**
- Live event logging during gameplay
- UI panel visibility debugging
- Event flow analysis (pickup → manager → UI)
- Runtime testing in builds
- Quick currency operations for testing

**Best for:**
- Debugging pickup issues
- UI panels not showing/updating
- Event flow problems
- Testing in builds (where editor window isn't available)
- Real-time event monitoring

---

## Quick Usage Tips

### Setting Up Currency System:
1. Use **CurrencySystemWindow** → "System Tools" → "Create Currency Manager"
2. Configure currencies in CurrencyConfiguration asset
3. Use **CurrencySystemWindow** → "Overview" to verify setup

### Debugging Pickup Issues:
1. Add **CurrencyDebugger** to any GameObject in scene
2. Enable "Log All Currency Events" and "Log UI Events"
3. Test pickup and watch console for event flow
4. Use "Find Currency Panels" to check UI setup

### Testing Currency Operations:
- **Editor**: Use **CurrencySystemWindow** → "Test Currency" tab
- **Runtime/Build**: Use **CurrencyDebugger** quick test buttons

### Creating Test Content:
- **Pickups**: Use **CurrencySystemWindow** → "Create Pickups" tab
- **Shops/Forges**: Use **CurrencySystemWindow** → "System Tools" tab

---

## Troubleshooting Workflow

1. **System Setup Issues** → Use CurrencySystemWindow Overview tab
2. **Currency Not Adding** → Use CurrencyDebugger event logging
3. **UI Not Updating** → Use CurrencyDebugger panel debugging
4. **Testing Scenarios** → Use CurrencySystemWindow test tabs
5. **Runtime Issues** → Use CurrencyDebugger (works in builds)

Both tools complement each other and serve different purposes!