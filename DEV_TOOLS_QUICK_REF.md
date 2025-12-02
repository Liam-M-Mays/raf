# RAF Dev Tools - Quick Reference

## Setup (1 minute)
1. Create empty GameObject "DevManager"
2. Add these components:
   - `DevOverlay`
   - `DevGodMode`
   - `DevSpawner` → assign an enemy SO
   - All other Dev tools (optional)

## Keybinds

```
F1        Toggle overlay UI
G         Toggle god mode (invincible)
Insert    Spawn enemy (uses DevSpawner setting)
Tab       Select & inspect enemy
+/-       Scale difficulty 0.5x-3.0x
W         Skip to wave
V         Toggle visual gizmos
P         Toggle projectile visualization
R         Record/stop recording
Shift+R   Replay last recording
```

## Common Testing Workflows

### 1. Test Enemy Behavior
```
Press V (enable gizmos)
Press Insert (spawn 1 enemy)
Watch green/yellow/red circles
Press Tab to select and read data
```

### 2. Test Wave Balance
```
Press + (increase difficulty)
Press Insert multiple times
Disable god mode
See if difficulty feels right
```

### 3. Analyze What Happened
```
Press R (start recording)
Play wave
Press R (stop recording)
Press Shift+R (replay slow motion)
Press Tab (inspect specific enemies)
```

### 4. Spawn Custom Wave
```
Press Insert (spawn Shark)
Press Insert (spawn Shark)
Press Insert (spawn Ranged)
Watch them fight
Adjust difficulty with +/- as needed
```

## What Each Tool Does

| Tool | Keybind | Purpose |
|------|---------|---------|
| **DevOverlay** | F1 | Shows UI buttons (god mode, spawn, time control) |
| **DevGodMode** | G | Raft takes no damage |
| **DevSpawner** | Insert | Spawn the assigned enemy |
| **DevBehaviorInspector** | Tab | Shows enemy data on hover |
| **DevDifficultyScaler** | +/- | 0.5x to 3.0x enemy stats |
| **DevWaveSkipper** | W | Kill all enemies, move to next wave |
| **DevGizmoToggle** | V | Show attack/respawn ranges visually |
| **DevProjectileVisualizer** | P | Show projectile paths |
| **DevHealthBar** | (auto) | Floating HP bars on all entities |
| **DevTimelineRecorder** | R | Record/replay last 5 seconds |

## Inspector Setup Examples

### For Testing Sharks
DevSpawner:
- Enemy To Spawn: SharkEnemy_SO
- Spawn Count: 2

### For Testing Boss
DevSpawner:
- Enemy To Spawn: Boss_SO
- Spawn Count: 1

### For Testing Ranged Fleet
DevSpawner:
- Enemy To Spawn: RangeOrbit_SO
- Spawn Count: 4

## Console Output
When you press keys, the console prints status:
```
Difficulty: 1.5x
Recording started...
Recording stopped. Captured 120 frames (2.0s)
Replaying...
Replay finished
Gizmos ON
Projectile visualization ON
```

## Tips

- **No camera setup needed**: All dev tools work in any scene
- **Works with/without UI**: DevOverlay creates its own Canvas if needed
- **Non-invasive**: All optional, doesn't affect build
- **Combine tools**: Use gizmos + inspector + difficulty for deep analysis

## Troubleshooting

**"Enemy doesn't spawn"**
- Check DevSpawner has an EnemySO assigned
- Check SpawnManager exists in scene
- Look at console for warnings

**"Gizmos not showing"**
- Press V to toggle (might already be off)
- Make sure BehaviorDebugger exists on enemies
- Check Scene view shows Gizmos enabled

**"Record/replay not working"**
- Only records enemies in scene when you press R
- Replay is slow motion (0.5x speed)
- Limited to 300 frames (~5 seconds)

**"Inspector not selecting enemies"**
- Press Tab first to enable
- Hover mouse over enemy in scene
- Press Tab again to hide if not needed
