# RAF - Complete Features Summary

**Last Updated**: December 1, 2025  
**Status**: Fully Implemented & Ready to Test

---

## 🎮 What We Built

A complete **strategic enemy behavior system** with diverse enemy types, advanced physics, dynamic weapons, and comprehensive dev tools for rapid iteration.

---

## 🐋 Enemy System

### New Behaviors (8 Total)
| Behavior | Description | Special Abilities |
|----------|-------------|-------------------|
| **Direct** | Swims straight to raft, attacks | Basic threat |
| **SharkDefault** | Circles raft with hysteresis flipping | Predictable pattern |
| **HammerDefault** | Orbits at distance, ranged attacks | Hit-and-run |
| **RangeOrbit** | Maintains distance, keeps firing | Tactical positioning |
| **Tank** | Charges with impact force, recovery phase | Physics-based threat |
| **PackFormation** | Boids flocking, cooperative attacks | Hive-mind coordination |
| **Pufferfish** | Explosive mines that trigger on timer | Area denial |
| **Boss** | 3-phase (chomp/bite, ranged, recovery) | Ultimate threat |

### Key Systems
- **BehaviorManager**: State machine for all behaviors
- **EnemyPerception**: Static utility for neighbor detection & hive-mind influence
- **FormationManager**: Deterministic slot assignment for pack behaviors
- **AttackQueue**: Manages which enemies can attack per frame (threadsafe with fallback initialization)
- **StatusEffectManager**: Non-invasive debuff system for future status effects

### Enemy AI Improvements - How They're Smarter

We implemented several systems that make enemies behave strategically and coordinate:

#### 1. **EnemyPerception System** (Hive-Mind Awareness)
- Enemies can detect neighbors within a radius
- Soft sync of speeds and headings among nearby allies
- Enemies share knowledge: if one sees the player, others get influenced
- Implemented in: `EnemyPerception.cs` (static utility)

**Code Example**:
```csharp
// Enemies find nearby allies
List<AllyInfo> allies = EnemyPerception.FindNearbyAllies(myPosition, detectionRadius);

// Enemies coordinate movement
Vector2 hiveMindInfluence = EnemyPerception.GetHiveMindInfluence(allies, myHeading);
```

#### 2. **FormationManager** (Deterministic Pack Behavior)
- Pack members are assigned deterministic "slots" around the raft
- Prevents clumping - each pack member knows its orbit position
- New pack members automatically join the formation
- Implemented in: `FormationManager.cs` (singleton)

**How it Works**:
```
Raft at center
  ↓
FormationManager assigns slots:
  - Slot 0: 0° (front-right)
  - Slot 1: 90° (back-right)
  - Slot 2: 180° (back-left)
  - Slot 3: 270° (front-left)
  
Each pack member orbits their slot
```

#### 3. **Boids-Like Flocking** (Coordinated Movement)
- **Separation**: Avoid crowding other pack members
- **Alignment**: Match heading of nearby allies
- **Cohesion**: Move toward center of nearby allies
- **Orbit**: Maintain formation around raft

All weights are configurable per behavior (`PackFormationCfg`)

#### 4. **Balanced Attack System** (Prevents Spam)
- **Tank**: Melee attacks on cooldown (1.5 sec default), charges every 4 sec
- **PackFormation**: Attacks coordinated per pack member, 2 sec cooldown default
- **Attacks deal actual damage** - fixed issue where attacks didn't damage

**Tank Behavior Improvements**:
```csharp
// Tank NOW:
- Approaches and melee attacks when in range
- Has cooldown between attacks
- Charges at least 4 seconds apart
- Charge deals bonus damage (1.5x)
- Each attack actually damages the raft
```

**PackFormation Improvements**:
```csharp
// Pack NOW:
- Members orbit around raft in formation
- Attack as individuals (not spam)
- Each member has 2 second attack cooldown
- Attacks deal moderate damage (5 per hit)
- Coordinate via formation slots
```

#### 5. **Perception-Based Behavior Switching**
- Enemies switch tactics based on what they perceive
- Tank: Charges when close, melee attacks when closer
- Ranged: Maintains distance, fires from safe range
- Pack: Forms up, attacks together when range closes

---

## 🔫 Weapon System

### Weapon Customization - Projectile Types

You can now customize projectiles with **8 different types**, giving you massive variety without creating new weapon classes:

| Type | Description | Use Case |
|------|-------------|----------|
| **Standard** | Basic straight projectile | Default, fast, simple |
| **Homing** | Seeks nearest enemy, curves toward target | Tactical, high skill ceiling |
| **Bouncing** | Bounces off walls/obstacles, reduced damage per bounce | Ricochet plays, room control |
| **Explosive** | Detonates on impact, damages nearby enemies in radius | Area denial, crowd control |
| **Piercing** | Goes through enemies, damages all in path | Line attacks, group damage |
| **Spray** | Spreads like shotgun pellets | Spread damage, close range |
| **Slow** | Slows enemies on impact (integrates with StatusEffectManager) | Control, tactical |
| **Splitting** | Splits into smaller projectiles on impact | Adaptive, scaling threat |

### How to Create a Custom Weapon with Projectile Type

1. **Create Projectile Prefab**:
   - Make a projectile GameObject
   - Add `PlayerProjectile` component
   - Add `ProjectileTypeConfig` component
   - Set `Projectile Type` to desired enum (Homing, Explosive, etc.)
   - Configure type-specific settings (radius, count, speed reduction, etc.)

2. **Create Weapon SO**:
   - Right-click in `Assets/Assets/Weapons/` → Create → Items → Weapon
   - Name it (e.g., "HomingRifle", "ExplosiveGrenade")
   - Assign your projectile prefab
   - Set physics impact forces
   - Set fire rate and damage

3. **Test It**:
   - Set as `Starting Weapon` in WeaponManager
   - Fire and watch projectile behavior

### Weapon Customization Examples

**Homing Rifle**: 
- ProjectileType: Homing
- homingTurnSpeed: 5
- homingRange: 10
- Damage: 12, FireRate: 0.4

**Explosive Shotgun**:
- ProjectileType: Explosive  
- explosionRadius: 4
- explosionDamage: 20
- bulletsPerShot: 3, spreadAngle: 30

**Piercing Beam**:
- ProjectileType: Piercing
- projectileDamage: 25
- fireRate: 0.6
- bulletPerShot: 1 (no spread)

### Available Weapons
You can now create and configure weapons with preset templates:

1. **Rifle** (Balanced ranged)
   - Moderate damage, good fire rate
   - Configure: `fireRate=0.3`, `autoFire=false`, `projectileDamage=15`

2. **Flamethrower** (Area/DoT)
   - Multiple projectiles with spread
   - Configure: `fireRate=0.15`, `autoFire=true`, `bulletsPerShot=3`, `spreadAngle=20`, `weaponKickbackForce=2`

3. **Laser** (Precision)
   - High damage, slow fire rate, beam-like
   - Configure: `fireRate=0.8`, `projectileDamage=40`, `projectileSpeed=20`, `enemyKnockbackForce=3`

4. **Acid/Glue** (Control)
   - Slows enemies on hit, tactical weapon
   - Configure: `fireRate=0.4`, `bulletsPerShot=2`, `projectileRange=12`, `enemyKnockbackForce=1`

5. **Bouncer** (Ricochet)
   - Bounces off obstacles, unpredictable
   - Configure: `fireRate=0.25`, `bulletsPerShot=1`, `projectileSpeed=15`

### How to Configure Weapons

1. In Unity, go to `Assets/Assets/Weapons/`
2. Create new WeaponSO: Right-click → Create → Items → Weapon
3. Name it (e.g., "Flamethrower", "LaserRifle")
4. Fill in editor fields:
   - **Shop Info**: name, cost, sprite, description
   - **Prefabs**: weapon visual, projectile prefab
   - **Ranged Properties**: fireRate, autoFire, damage, speed, range
   - **Multi-Shot**: bulletsPerShot, spreadAngle
   - **Physics Impact**: weaponKickbackForce (recoil), enemyKnockbackForce (enemy knockback)

### Physics Impact
- **Weapon Kickback**: Pushes raft backward when firing (e.g., Laser has force 3)
- **Enemy Knockback**: Projectiles push enemies on hit (e.g., Laser has force 3)
- **Tank Charge**: Impacts raft with configurable force (default 5)

### Starting Weapon
- In `WeaponManager` component on player, set the `Starting Weapon` field
- Drag any WeaponSO to test different weapons immediately

---

## ⚙️ Physics Improvements

### Raft Physics
- **Paddle Force**: Configurable acceleration (upgradable with items)
- **Water Drag**: Realistic deceleration
- **Impact Force**: Enemies can push the raft (Tank charges, weapon kickback)
- **Mass Scaling**: Frame/Paddle upgrades affect weight and acceleration

### Physics Methods (in PlayerMovement.cs)
```csharp
// Apply impact to raft from external source
public void ApplyRaftImpact(Vector3 direction, float force)

// Apply knockback to enemies
public static void ApplyEnemyKnockback(Transform enemy, Vector3 direction, float force)
```

---

## 🛠️ Dev Tools (10 Total)

### Quick Start
Press **F1** to toggle the dev overlay. Buttons appear in top-left:

| Tool | Function | Hotkey |
|------|----------|--------|
| **God** | Invincible mode | F1 overlay |
| **Spawn** | Spawn random enemy | F1 overlay |
| **Pause** | Pause/unpause time | F1 overlay |
| **+Time/-Time** | Speed up/slow time | F1 overlay |

### Advanced Dev Tools
Access by adding these components to any GameObject:

- **DevBehaviorInspector**: Displays enemy behavior state in real-time
- **DevSpawner**: Configurable enemy spawning with behavior selection
- **DevGodMode**: Invincibility + infinite ammo
- **DevDifficultyScaler**: Adjust enemy health/damage multipliers
- **DevGizmoToggle**: Visualize enemy formation slots and perception ranges
- **DevWaveSkipper**: Skip to next wave instantly
- **DevProjectileVisualizer**: Show projectile hitboxes and paths
- **DevHealthBar**: Display all entity health bars
- **DevTimelineRecorder**: Record gameplay for analysis

### Notes
- All tools are optional - game works without them
- God mode works standalone
- Dev overlay creates UI on demand (no scene setup required)
- Use `DevBehaviorInspector` + `DevSpawner` for rapid behavior testing

---

## 🎯 How to Test New Features

### Test Tank Charge Impact
1. Spawn Tank enemies near raft
2. Watch them charge and knock raft backward
3. Adjust `TankCfg.chargeImpactForce` to tune impact (default 5)

### Test Weapon Physics
1. Go to `WeaponManager` on player
2. Set Starting Weapon to a high-kickback weapon (Laser recommended)
3. Fire toward enemies - notice raft pushes back
4. Fire toward enemies - they get knocked away

### Test PackFormation
1. Spawn 4+ PackFormation enemies
2. Watch them form slots around raft
3. Observe hive-mind coordination (they attack together when in range)
4. Use `DevGizmoToggle` to see formation slots

### Test Boss Behavior
1. Spawn Boss enemy
2. Phase 1 (Approach): Chomps when in range
3. Phase 2 (Ranged): Retreats and fires projectiles
4. Phase 3 (Recovery): Takes heavy damage to kill, low threat

---

## 📋 Configuration Files

### Main Configuration (GameConstants.cs)
- MAX_ATTACKING_ENEMIES: How many enemies can attack per frame
- Weapon stats: Speed ranges, damage caps
- Physics: Gravity, drag constants

### Behavior Configurations
Each behavior has a `Cfg` class with public fields:
- **TankCfg**: chargeSpeed, chargeImpactForce, chargeCooldown
- **PackFormationCfg**: formationRadius, attackRange, attackCooldown
- **PufferfishCfg**: explosionRadius, explosionForce, explosionDamage
- **BossCfg**: phaseHealthThresholds, attackCooldowns

### How to Edit
1. In Unity Inspector, select any enemy GameObject
2. Find `LiamEnemyBrain` component
3. Expand `Current Behavior` → behavior config
4. Adjust sliders in real-time (changes apply immediately)

---

## 🐛 Known Issues & Solutions

| Issue | Solution |
|-------|----------|
| AttackQueue not found | Now auto-creates with fallback - should be fixed |
| PackFormation not attacking | Attack logic added - now checks range and fires |
| Dev HUD not showing | Updated to use TextMeshPro - should be visible now |
| Config dropdowns empty | EnemySO now initializes all configs - should work |

---

## 🚀 Next Steps

### Quick Wins
- [ ] Test Tank charge impact with different force values
- [ ] Create Flamethrower weapon and test spread physics
- [ ] Spawn PackFormation and watch formation behavior
- [ ] Use DevBehaviorInspector to inspect enemy state machine

### Medium Features
- [ ] Projectile pooling for performance
- [ ] Boss minion spawning in phase 3
- [ ] Player weapon upgrade system
- [ ] Custom wave configurations

### Polish
- [ ] Tune all behavior timings
- [ ] Balance weapon damage/speed tradeoffs
- [ ] Add sound effects for impacts
- [ ] Particle effects for explosions

---

## 📚 Additional Resources

- **DEV_TOOLS_GUIDE.md**: Detailed dev tool documentation
- **SYSTEM_ARCHITECTURE.md**: Technical architecture overview
- **Code Comments**: Extensive inline documentation

---

## 🎓 Key Concepts

### Behavior Pattern
All behaviors inherit from `IBehavior` and implement:
```csharp
void OnEnter(Transform self, Animator anim)  // Initialize
void OnUpdate()                                // Per-frame logic
void OnExit()                                  // Cleanup
```

### Configuration Pattern
Each behavior has a `BehaviorCfg` class with public fields for tuning:
```csharp
[Serializable]
public class TankCfg : BehaviorCfg {
    public float chargeSpeed = 3f;
    public float chargeImpactForce = 5f;
}
```

### Physics Pattern
- Apply forces via `Rigidbody2D.AddForce()`
- Use `ForceMode2D.Impulse` for instant hits
- Use `ForceMode2D.Force` for continuous acceleration

---

**Questions?** Check code comments - they're comprehensive. Use dev tools for rapid iteration!
