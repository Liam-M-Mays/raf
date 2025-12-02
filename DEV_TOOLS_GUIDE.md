# RAF Dev Tools Guide

## Existing Dev Tools

### 1. **DevOverlay** (Runtime UI Panel)
**Location**: `Assets/Scripts/Dev/DevOverlay.cs`
**How to use**:
- Attach to any GameObject (e.g., a Manager object)
- Optionally assign `DevGodMode` and `DevSpawner` in the inspector
- Press `F1` to toggle the overlay visibility
- Buttons appear in top-left corner:
  - **Toggle God** - Toggle invincibility
  - **Spawn** - Spawn the assigned enemy
  - **Pause/Unpause** - Freeze time
  - **+Time** - Speed up time (0.25x increments)
  - **-Time** - Slow down time (0.25x decrements)

**Features**:
- Creates its own Canvas if none exists (safe for all scenes)
- Uses built-in Arial font (no TextMesh Pro dependency)
- Simple black background for visibility

---

### 2. **DevGodMode** (Invincibility Toggle)
**Location**: `Assets/Scripts/Dev/DevGodMode.cs`
**How to use**:
- Attach to any GameObject
- Press `G` to toggle god mode
- Console prints "GOD MODE ENABLED" / "God mode disabled"

**Features**:
- Heals the raft automatically when taking damage
- Works by subscribing to `Health.OnDamaged` event
- Safe if Health component doesn't exist (logs warning)

**Keybind**: `G`

---

### 3. **DevSpawner** (Manual Enemy Spawning)
**Location**: `Assets/Scripts/Dev/DevSpawner.cs`
**How to use**:
- Attach to any GameObject
- Assign an `EnemySO` (enemy ScriptableObject) in the inspector
- Optionally set `spawnCount` (default: 1)
- Press `Insert` to spawn

**Features**:
- Spawns multiples at once if `spawnCount > 1`
- Works with SpawnManager (finds it automatically)
- Console logs if no EnemySO assigned

**Keybind**: `Insert`
**Inspector Fields**:
- `enemyToSpawn`: EnemySO to spawn
- `spawnCount`: How many to spawn per press

---

### 4. **DevBehaviorInspector** (On-Screen Enemy Info)
**Location**: `Assets/Scripts/Dev/DevBehaviorInspector.cs`
**How to use**:
- Attach to any GameObject
- Press `Tab` to toggle inspector visibility
- Hover mouse over an enemy to select it
- Info box shows:
  - Enemy name
  - Current behavior type
  - Distance to raft
  - Current HP

**Features**:
- Uses OnGUI (no Canvas required)
- Displays info in top-left as you hover
- Press Tab again to hide

**Keybind**: `Tab`

---

## New Dev Tools (Expanded)

### 5. **DevDifficultyScaler** (Wave Difficulty Control)
**Location**: `Assets/Scripts/Dev/DevDifficultyScaler.cs`
**How to use**:
- Attach to any GameObject
- Press `+` to increase difficulty
- Press `-` to decrease difficulty
- Console shows current difficulty multiplier

**Features**:
- Scales enemy health, damage, and speed simultaneously
- Difficulty range: 0.5x to 3.0x
- Useful for playtesting balance
- Persists across spawns

**Keybinds**: `=` (plus) and `-` (minus)

---

### 6. **DevWaveSkipper** (Skip to Any Wave)
**Location**: `Assets/Scripts/Dev/DevWaveSkipper.cs`
**How to use**:
- Attach to any GameObject (usually WaveManager)
- Press `W` to show wave selection menu
- Type wave number and press Enter
- Current wave skips to selected

**Features**:
- Kills all current enemies immediately
- Resets timers
- Safe if WaveManager doesn't exist
- Console feedback

**Keybind**: `W`

---

### 7. **DevGizmoToggle** (Visual Debug Display)
**Location**: `Assets/Scripts/Dev/DevGizmoToggle.cs`
**How to use**:
- Attach to any GameObject
- Press `V` to toggle visual gizmos
- All enemies show:
  - Green circle: attack range
  - Yellow circle: out-of-range respawn threshold
  - Red line: direction to target
  - Purple circle: perception/neighbor radius

**Features**:
- Zero performance cost when disabled
- Enables all BehaviorDebugger gizmos
- Helpful for understanding enemy spacing

**Keybind**: `V`

---

### 8. **DevProjectileVisualizer** (Projectile Tracker)
**Location**: `Assets/Scripts/Dev/DevProjectileVisualizer.cs`
**How to use**:
- Attach to any GameObject
- Press `P` to toggle projectile tracking
- When on, all projectiles show:
  - Trajectory line (extrapolated path)
  - Damage amount as label
  - Target type (Raft, other)

**Features**:
- Real-time trajectory prediction
- Helps tune projectile speed/direction
- Auto-disables when no projectiles active

**Keybind**: `P`

---

### 9. **DevHealthBar** (Floating HP Indicators)
**Location**: `Assets/Scripts/Dev/DevHealthBar.cs`
**How to use**:
- Attach to any GameObject with Health component
- Health bar appears above the entity
- Color changes: Green -> Yellow -> Red as HP decreases
- Numbers show current/max HP

**Features**:
- WorldSpace canvas (follows entities)
- Only shows if HP < max
- Useful for seeing damage numbers during waves

**Auto-enabled**: No keybind (always on if attached)

---

### 10. **DevTimelineRecorder** (Replay Functionality)
**Location**: `Assets/Scripts/Dev/DevTimelineRecorder.cs`
**How to use**:
- Attach to any GameObject
- Press `R` to start recording
- Play the game
- Press `R` again to stop
- Press `Shift+R` to replay last recording

**Features**:
- Records enemy positions, states, raft position every frame
- Max 300 frames (5 seconds at 60fps)
- Useful for understanding what happened in a wave
- Console shows frame count

**Keybinds**: 
- `R` to record/stop
- `Shift+R` to replay

---

## Quick Setup Guide

### Minimal Setup
1. Create an empty GameObject called "DevManager"
2. Add **DevOverlay** (provides UI for other tools)
3. Add **DevGodMode**
4. Add **DevSpawner** (assign an enemy SO)
5. **Done** - You now have F1 menu + god mode + spawning

### Full Debug Setup
Add all of these to the same GameObject:
- DevOverlay
- DevGodMode
- DevSpawner
- DevBehaviorInspector
- DevDifficultyScaler
- DevWaveSkipper
- DevGizmoToggle
- DevProjectileVisualizer
- DevTimelineRecorder

### Keybind Reference
| Key | Tool | Action |
|-----|------|--------|
| `F1` | DevOverlay | Toggle UI |
| `G` | DevGodMode | Toggle invincibility |
| `Insert` | DevSpawner | Spawn enemy |
| `Tab` | DevBehaviorInspector | Select/inspect enemy |
| `=` (plus) | DevDifficultyScaler | Increase difficulty |
| `-` | DevDifficultyScaler | Decrease difficulty |
| `W` | DevWaveSkipper | Skip to wave |
| `V` | DevGizmoToggle | Toggle gizmos |
| `P` | DevProjectileVisualizer | Toggle projectiles |
| `R` | DevTimelineRecorder | Record/replay |

---

## Tips & Tricks

### Testing Enemy Behavior
1. Enable DevGizmoToggle (press `V`)
2. Use DevSpawner to spawn 1 enemy (press `Insert`)
3. Watch the green/yellow/red circles to understand ranges
4. Use DevBehaviorInspector (Tab) to see internal state

### Testing Wave Balance
1. Increase difficulty with DevDifficultyScaler (press `+`)
2. Spawn a wave mix manually
3. Disable god mode to see actual damage
4. Adjust numbers in EnemySO based on playtesting

### Quick Replay Analysis
1. Press `R` to start recording
2. Play a challenging wave
3. Press `R` to stop
4. Press `Shift+R` to replay and analyze
5. Use DevGizmoToggle to see decision-making

### Underwater Testing
1. Use DevProjectileVisualizer (press `P`)
2. Spawn Acid gun + Bouncer enemies
3. Watch how trajectories differ vs surface

---

## Console Output Examples

**DevGodMode**:
```
DevGodMode: GOD MODE ENABLED
DevGodMode: God mode disabled
```

**DevSpawner**:
```
DevSpawner: No EnemySO assigned.
DevSpawner spawned 3 enemies
```

**DevDifficultyScaler**:
```
Difficulty: 1.0x
Difficulty: 1.25x
Difficulty: 0.75x
```

**DevWaveSkipper**:
```
Skipping to wave 5. Killing all current enemies.
Wave 5 started.
```

---

## Known Limitations

- DevTimelineRecorder only records 300 frames (~5 seconds at 60fps)
- DevGizmoToggle requires BehaviorDebugger on enemies (manual or automatic)
- DevDifficultyScaler affects newly spawned enemies only (not mid-wave changes)
- DevWaveSkipper may fail if WaveManager doesn't follow expected structure

---

## Extending Dev Tools

Want to add your own? Template:

```csharp
public class DevMyTool : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F;
    private bool enabled = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            enabled = !enabled;
            Debug.Log("DevMyTool: " + (enabled ? "ON" : "OFF"));
        }
    }

    void OnGUI()
    {
        if (enabled)
        {
            GUILayout.Label("My custom debug tool");
        }
    }
}
```

Attach to DevManager and it works immediately!
