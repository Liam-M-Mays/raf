# RAF - Comprehensive Development Proposal
**Date**: December 1, 2025  
**Version**: 1.0

---

## Executive Summary

Your RAF game has a solid foundation with 8 distinct enemy behaviors, advanced weapon systems, and comprehensive dev tools. However, several critical issues prevent it from feeling polished and intelligent:

1. **AI Feels Unintelligent** - Movement patterns are erratic, unpredictable, and don't visually read as "hunting"
2. **Hammershark Miss Mechanic Missing** - No daze state when hammer misses
3. **Health Healing Broken** - Overnight/between-wave healing doesn't work properly
4. **Projectile Pass-Through** - Bullets ignore non-hittable enemies instead of passing through
5. **Enemy Speed Not Random** - All enemies of same type have identical speed (unrealistic)
6. **Underwater System Incomplete** - Hittable toggle mentioned but not fully implemented
7. **Behavior Logic Issues** - Several behaviors have inconsistent state management and movement

---

## IDENTIFIED ISSUES & FIXES

### CRITICAL - Must Fix (Affects Gameplay)

#### 1. **Hammer Shark Miss Daze** ⚠️ HIGH PRIORITY
**Current Issue**: HammerDefault charges but has no consequence when missing the player
**Impact**: No tension when Hammer charges - feels ineffectual
**Location**: `Assets/Scripts/Enemy Logic/EnemyBehaviors/HammerDefault.cs`

**Root Cause**:
- Charge completes when close to raft, but if player dodges the charge, nothing happens
- No "dazed" recovery state when charge fails
- Should enter daze state to create tactical window for player

**Solution**:
```csharp
// Add to HammerDefault.cs
private float missDistance = 1.5f;  // Distance to raft when charge "ends"

// In Charging state:
case State.Charging:
    ctx.self.position += (Vector3)(moveDir * cfg.chargeSpeed * Time.deltaTime);
    
    // CHECK FOR MISS - if charge has gone too far, it's a miss
    if (Vector3.Distance(ctx.self.position, ctx.target.position) > missDistance * 2)
    {
        // MISS! Enter dazed state
        state = State.Recover;
        cooldownTimer = cfg.chargeCooldown;
        ctx.anim.SetBool("Dazed", true);
        // Do NOT deal damage
        break;
    }
    
    // Original end-of-charge logic
    if (dist <= 1f)
    {
        // Hit! Same as before...
    }
    break;
```

**Implementation Steps**:
1. Track charge distance traveled
2. If charge travels more than expected range without hitting, mark as miss
3. Set "Dazed" animation state
4. Increase cooldown duration for missed charges (e.g., 6 sec vs 4 sec)
5. Make hammer vulnerable during daze (set `ctx.hittable = true`)

---

#### 2. **Health Healing Between Waves** 🔴 CRITICAL
**Current Issue**: Player health doesn't properly heal overnight/between waves
**Impact**: Health slowly drains across waves until raft dies
**Location**: `Assets/Scripts/PlayerMovement.cs` + `Assets/Scripts/Combat/Health.cs`

**Root Cause**:
- Sheets upgrade has `maxHealthBonus` but healing is called once per frame in `UpdateUpgrades()`
- `matressHealth.Heal(sheetsHealthEffect)` is called every frame (line 185)
- This should be called once per shop/wave start, not every frame
- No "overnight healing" mechanic between waves exists

**Current Code Problem**:
```csharp
// PlayerMovement.cs line 185 - WRONG LOCATION
void UpdateUpgrades()
{
    if (Sheets != null)
    {
        // This runs EVERY FRAME!
        matressHealth.Heal(sheetsHealthEffect);  // ❌ Wrong
    }
}
```

**Solution**:
```csharp
// Add to PlayerMovement.cs
public void HealBetweenWaves(float healPercent = 0.25f)
{
    // Heal a percentage of max health when entering shop
    float maxHealth = matressHealth.GetMaxHealth();
    float healAmount = maxHealth * healPercent;
    matressHealth.Heal(healAmount);
    
    Debug.Log($"Healed {healAmount} HP between waves");
}

// Move Sheets bonus OUT of UpdateUpgrades
void UpdateUpgrades()
{
    if (Sheets != null)
    {
        // Just update the MAX health cap, don't heal every frame
        baseSprite.sprite = Sheets.upgradeSprite;
        sheetsHealthEffect = Sheets.maxHealthBonus;
        baseSprite.enabled = true;
    }
}
```

**Call Sites** (New):
```csharp
// In MasonManager.cs (when entering shop)
public void StartShop()
{
    // Heal player 25% health when entering shop
    PlayerMovement player = GameServices.GetPlayerMovement();
    if (player != null) player.HealBetweenWaves(0.25f);
    
    // ... rest of shop logic
}

// Or in WaveManager.cs
public void EndWave()
{
    PlayerMovement player = GameServices.GetPlayerMovement();
    if (player != null) player.HealBetweenWaves(0.25f);
    
    waveActive = false;
    waveNumber++;
    masonManager.StartShop();
}
```

**Implementation Steps**:
1. Remove frame-by-frame healing in `UpdateUpgrades()`
2. Add `HealBetweenWaves()` method to PlayerMovement
3. Call it once when entering shop (modify MasonManager or WaveManager)
4. Add UI feedback (heal text, sound effect)
5. Make heal percentage configurable in WaveManager

---

#### 3. **Projectiles Pass Through Non-Hittable Enemies** 🔴 CRITICAL
**Current Issue**: Bullets destroy themselves on contact with any enemy, even underwater/non-hittable ones
**Impact**: Bullets waste ammo on enemies player can't damage
**Location**: `Assets/Scripts/Combat/PlayerProjectile.cs` line 44-60

**Root Cause**:
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Enemy"))
    {
        if (collision.GetComponent<LiamEnemyBrain>().manager.Current.CTX().hittable)
        {
            // Deal damage...
        }
        
        // ❌ ALWAYS destroys, even if not hittable!
        Destroy(gameObject);  
    }
}
```

**Solution**:
```csharp
void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Enemy"))
    {
        LiamEnemyBrain brain = collision.GetComponent<LiamEnemyBrain>();
        if (brain != null && brain.manager.Current.CTX().hittable)
        {
            // ONLY damage if hittable
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsDead())
            {
                enemyHealth.TakeDamage(damage, transform.position);
            }
            
            // Apply knockback
            if (knockbackForce > 0f)
            {
                PlayerMovement.ApplyEnemyKnockback(collision.transform, direction, knockbackForce);
            }
            
            SpawnHitEffect();
            Destroy(gameObject);
        }
        // ✅ If NOT hittable, just pass through (don't destroy)
        
        return;
    }
    
    // ... rest of collision logic
}
```

**Implementation Steps**:
1. Modify collision detection to only destroy on hittable enemies
2. Pass through non-hittable enemies (no collision)
3. Optional: Add visual feedback for pass-through (particle effect)
4. Test with underwater enemies when they surface/dive

---

### MAJOR - Affects Game Feel (High Priority)

#### 4. **Enemy Speeds Not Randomized** 🟠 HIGH
**Current Issue**: All enemies of same type move at identical speeds
**Impact**: Predictable patterns, feels unnatural and mechanical
**Location**: All behavior configs initialize with fixed `speed` and `maxSpeed`

**Root Cause**:
- Configs have `speed = 1f` hardcoded for all instances
- No variance between individual enemies of same type

**Solution**:
```csharp
// Create new utility in BehaviorContext.cs
public class RandomizedSpeedSystem
{
    public static void ApplySpeedVariance(ref float speed, ref float maxSpeed, float variancePercent = 0.15f)
    {
        // Random variance between -15% and +15%
        float variance = Random.Range(1f - variancePercent, 1f + variancePercent);
        speed *= variance;
        maxSpeed *= variance;
    }
}

// In EACH behavior's OnEnter, after setting speeds:
public void OnEnter(Transform _self, Animator _anim)
{
    // ... setup code ...
    
    ctx.maxSpeed = config.maxSpeed;
    ctx.speed = config.speed;
    
    // ✅ Add randomization
    RandomizedSpeedSystem.ApplySpeedVariance(ref ctx.speed, ref ctx.maxSpeed, 0.15f);
}
```

**Apply to All Behaviors**:
- Direct
- SharkDefault
- HammerDefault
- RangeOrbit
- Tank
- PackFormation
- Pufferfish
- Boss

**Implementation Steps**:
1. Create `RandomizedSpeedSystem` utility class
2. Add to each behavior's `OnEnter()` method
3. Parameterize variance percentage (15% default)
4. Test that enemies feel more natural and varied
5. Optional: Add config field to control variance per-enemy-type

---

#### 5. **AI Movement Feels Unintelligent** 🟠 MAJOR ISSUE
**Current Issue**: Enemy behaviors are inconsistent, jittery, and don't read as "intelligent hunting"
**Impact**: Game feels unsatisfying - enemies appear random/broken, not tactical
**Root Causes**:

a) **SharkDefault Complex State Management**
   - Multiple overlapping conditions make behavior unpredictable
   - "Lurk" animation state but no clear lurking behavior
   - Zigzag + circle + attack states conflict

b) **HammerDefault Confusing State Transitions**
   - "Lurk" state is used but not clearly defined
   - Windup positioning unclear
   - "Dazed" animation set but state never reaches it

c) **Tank Approach State Unclear**
   - Slowly approaches but no evasion
   - Charge decision logic convoluted
   - Recovery after charge too predictable

d) **PackFormation Boids Overhead**
   - Flocking calculations every frame expensive
   - Doesn't actually look coordinated on screen
   - Too many competing forces

**Solutions**:

**A) Simplify SharkDefault Behavior**
```csharp
// Rewrite state machine to be clearer
private enum State
{
    Approach,      // Swim toward raft in zigzag
    Orbiting,      // Circle at distance
    Attacking      // Rush in for bite
}

// Clearer state transitions
public void OnUpdate()
{
    switch(state)
    {
        case State.Approach:
            // If distance > orbit distance, zigzag closer
            if (ctx.distanceToTarget > orbit)
            {
                ZigzagMovement.Execute(ctx, zigzag_amplitude, zigzag_frequency);
            }
            else
            {
                state = State.Orbiting;
                attackTimer = config.attackTimer;
            }
            break;
            
        case State.Orbiting:
            // Circle at safe distance
            CircleMovement.Execute(ctx, orbit, orbit + orbitMax, circleDirection);
            
            // After timer, attempt attack
            attackTimer -= ctx.deltaTime;
            if (attackTimer <= 0f && RaftTracker.addSelf(this))
            {
                state = State.Attacking;
                ActionNodes.Attack(ctx);
                attackStart = (Vector2)ctx.target.position;
            }
            break;
            
        case State.Attacking:
            // Chase if player moves away from attack start
            if (Vector2.Distance(attackStart, (Vector2)ctx.target.position) > attackRangeMax)
            {
                // Gave up chase
                state = State.Orbiting;
                ActionNodes.StopAttack(ctx);
                RaftTracker.removeSelf(this);
                attackTimer = config.attackTimer;
            }
            else if (ctx.distanceToTarget < attackRange)
            {
                // In close for bite
                ctx.hittable = true;
                DirectChaseMovement.SlowExecute(ctx);
            }
            else
            {
                // Still chasing
                DirectChaseMovement.Execute(ctx);
            }
            break;
    }
}
```

**B) Improve Tank Behavior Predictability**
```csharp
// Tank should have clearer intent
// Current: confusing cooldown logic
// Improved: Clear approach → charge → recovery cycle

case State.Approach:
    // Move slowly toward raft
    ctx.self.position += moveDir * cfg.speed * Time.deltaTime;
    ctx.anim.SetBool("Moving", true);
    
    // Attack when in melee range
    if (dist <= cfg.attackRange && attackCooldownTimer <= 0f)
    {
        // Simple melee attack
        DealDamageToRaft();
        attackCooldownTimer = cfg.attackCooldown;
    }
    
    // Charge when close and enough time has passed
    float chargeReadyTime = 3f;  // Wait at least 3 sec between charges
    if (cfg.canCharge && chargeReadyTime <= 0f && dist < cfg.attackRange * 3)
    {
        state = State.Windup;
        windupTimer = cfg.chargeWindup;
        chargeReadyTime = cfg.chargeCooldown;
    }
    chargeReadyTime -= ctx.deltaTime;
    break;
```

**C) PackFormation Looks More Coordinated**
```csharp
// Current: Too many forces fight each other
// Improved: Simpler priority system

// Priority: Orbit > Flocking > Attack
float dist = ctx.distanceToTarget;

if (dist > cfg.attackRange * 1.5f)
{
    // Far away: Orbit with flocking
    Vector3 orbitPos = CalculateOrbitPosition();
    Vector3 separation = CalculateSeparation();
    
    Vector3 movement = (orbitPos - ctx.self.position).normalized * cfg.speed * 0.7f
                     + separation * 0.3f;
    ctx.self.position += movement * Time.deltaTime;
}
else if (dist > cfg.attackRange)
{
    // Medium distance: Tighter orbit, ready to attack
    Vector3 orbitPos = CalculateOrbitPosition();
    ctx.self.position = Vector3.MoveTowards(ctx.self.position, orbitPos, cfg.speed * 1.2f * Time.deltaTime);
}
else
{
    // In attack range: Attack if on cooldown
    if (attackCooldownTimer <= 0f && RaftTracker.addSelf(this))
    {
        DealDamageToRaft();
        attackCooldownTimer = cfg.attackCooldown;
    }
}
```

**D) Visual Feedback for Clarity**
- Add gizmos showing intended movement target
- Add animation transitions that match behavior state
- Add sound cues when state changes (e.g., charge windup sound)

**Implementation Steps**:
1. Rewrite SharkDefault state machine for clarity
2. Simplify Tank approach/charge/recovery logic
3. Improve PackFormation priority system
4. Add visual gizmos for behavior debugging
5. Playtest each behavior individually with dev tools
6. Record video of improved vs old behavior for comparison

---

#### 6. **Underwater/Hittable State System Incomplete** 🟠 MAJOR
**Current Issue**: Documentation mentions enemies surface/dive, but no implementation
**Impact**: No underwater stealth mechanic for enemies
**Location**: Multiple files missing implementation

**Current State**:
- `ctx.hittable` exists but only toggled based on distance/state
- No "underwater" animation state
- No actual dive/surface mechanics
- SharkDefault sets `ctx.hittable = false` but no clear underwater indicator

**Solution - Phase 1 (Basic Implementation)**:

```csharp
// Create new behavior component
public class UnderwaterState
{
    public bool isUnderwater = false;
    public float surfaceHeight = -2f;  // Y position where surface is
    public float diveTimer = 0f;
    public float diveDuration = 3f;
    public float surfaceInterval = 5f;  // Time between dives
}

// In BehaviorContext.cs
public UnderwaterState underwaterState;

public BehaviorContext(Transform self, Transform target, Animator anim)
{
    // ...
    underwaterState = new UnderwaterState();
}

// Utility function to handle underwater state
public static class UnderwaterManager
{
    public static void UpdateUnderwaterState(BehaviorContext ctx)
    {
        if (ctx.underwaterState == null) return;
        
        ctx.underwaterState.diveTimer -= ctx.deltaTime;
        
        if (ctx.underwaterState.diveTimer <= 0f)
        {
            // Toggle dive/surface
            ctx.underwaterState.isUnderwater = !ctx.underwaterState.isUnderwater;
            
            if (ctx.underwaterState.isUnderwater)
            {
                ctx.underwaterState.diveTimer = ctx.underwaterState.diveDuration;
                ctx.hittable = false;
                ctx.anim.SetBool("Underwater", true);
            }
            else
            {
                ctx.underwaterState.diveTimer = ctx.underwaterState.surfaceInterval;
                ctx.hittable = true;
                ctx.anim.SetBool("Underwater", false);
            }
        }
    }
}

// Usage in behaviors (SharkDefault example)
public void OnUpdate()
{
    ctx.UpdateFrame();
    UnderwaterManager.UpdateUnderwaterState(ctx);
    
    // Rest of behavior logic...
}
```

**Visual Changes Needed**:
- Add "Underwater" animation parameter to all enemies
- Underwater animation: enemies fade/sink below surface
- Surface animation: enemies rise/appear above water
- Projectiles can't damage underwater enemies (pass through)

**Implementation Steps**:
1. Add UnderwaterState to BehaviorContext
2. Create UnderwaterManager utility
3. Add to SharkDefault first (test behavior)
4. Add "Underwater" animation state to all enemies
5. Configure dive/surface timings in behavior configs
6. Test projectile pass-through with underwater enemies

---

### MEDIUM - Polish & Optimization

#### 7. **Circle Movement Jitter** 🟡 MEDIUM
**Current Issue**: Enemies orbiting stutter/jitter when changing direction
**Location**: `Assets/Scripts/Enemy Logic/BehaviorNodes/CircleMovement.cs`

**Root Cause**:
```csharp
// Current calculation recalculates angle every frame
data.currentAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
```

**Solution**:
```csharp
// Smooth angle update
float targetAngle = Mathf.Atan2(currentOffset.y, currentOffset.x);
data.currentAngle = Mathf.Lerp(data.currentAngle, targetAngle, 0.1f);
// Or use angular velocity instead of recalculating
```

---

#### 8. **Boss Animation State Mismatch** 🟡 MEDIUM
**Current Issue**: Boss uses "Charging" state but should have "Windup" → "Attack" sequence
**Impact**: Animation doesn't feel impactful
**Location**: `Assets/Scripts/Enemy Logic/EnemyBehaviors/Boss.cs`

**Solution**:
```csharp
// Separate windup from attack
ctx.anim.SetBool("WindingUp", true);  // During windup
// ...
ctx.anim.SetBool("WindingUp", false);
ctx.anim.SetTrigger("Attack");  // Trigger attack animation
```

---

#### 9. **AttackQueue Threading Issue** 🟡 MEDIUM
**Current Issue**: AttackQueue mentioned as "fixed" but may have race conditions
**Location**: `Assets/Scripts/Enemy Logic/AttackQueue.cs`

**Solution**: Verify thread safety with locks:
```csharp
private static readonly object lockObj = new object();

public static bool addSelf(IBehavior behavior)
{
    lock(lockObj)
    {
        if (current_attacking >= max_attacking) return false;
        current_attacking++;
        return true;
    }
}
```

---

#### 10. **Ranged Enemy Range Checks** 🟡 MEDIUM
**Current Issue**: RangeOrbit doesn't have clear attack logic
**Location**: `Assets/Scripts/Enemy Logic/EnemyBehaviors/RangeOrbit.cs`

**Solution**: Add line-of-sight and clear attack ranges

---

## NEW FEATURES TO ADD

### PHASE 1 - Core Gameplay (2-3 days)

#### A. **Wave Difficulty Scaling** 🟢 NEW
**Feature**: Waves get progressively harder with balanced pacing
**Implementation**:
```csharp
// In StandardWaveLogic
public class WaveDifficultyScaler
{
    public static void ScaleWave(Wave wave, int waveNumber)
    {
        float healthMultiplier = 1f + (waveNumber * 0.1f);  // +10% each wave
        float damageMultiplier = 1f + (waveNumber * 0.05f);  // +5% each wave
        
        foreach(var enemy in wave.enemies)
        {
            enemy.health *= healthMultiplier;
            enemy.damage *= damageMultiplier;
        }
    }
}
```

#### B. **Enemy Variation Within Waves** 🟢 NEW
**Feature**: Each spawned enemy slightly different (speed, damage) for variety
**Implementation**: Use the randomized speed system + damage variance

#### C. **Combat Feedback Polish** 🟢 NEW
- Enemy hit flash (white flash on damage)
- Enemy knockback animation on hit
- Improved damage number display
- Screen shake on major impacts (tank charge, boss attack)

---

### PHASE 2 - Advanced AI (3-4 days)

#### A. **Hive Mind Communication** 🟣 ADVANCED
**Feature**: Enemies coordinate attacks better
```csharp
// When one shark spots player, others get alerted
if (PlayerInRange)
{
    EnemyPerception.BroadcastPlayerSpotted(ctx.target.position);
}

// Nearby enemies increase aggression
Vector3 hiveMindInfluence = EnemyPerception.GetHiveMindInfluence(allies, myHeading);
```

#### B. **Tactical Behavior Switching** 🟣 ADVANCED
**Feature**: Enemies evaluate situation and switch tactics
```csharp
// If player health is low, more enemies rush
// If player is far, ranged enemies fire from safe distance
// If player just fired weapon, melee enemies charge
```

---

### PHASE 3 - Content Expansion (4-5 days)

#### A. **New Enemy Types** 🟣 NEW
- **Jellyfish**: Drifts with pulsing attacks, floats on surface
- **Squid**: Intelligent, uses ink cloud to escape
- **Eel**: Fast zigzag movement, electric attacks
- **Kraken Tentacle**: Emerges periodically, high damage

#### B. **Boss Variants** 🟣 NEW
- **Ice Shark**: Freezes raft, slows player movement
- **Electricity Eel**: Arcs electricity between enemies
- **Mega Octopus**: Multiple tentacles attack simultaneously

#### C. **Environmental Hazards** 🟣 NEW
- Whirlpools (slow player)
- Waves (push raft around)
- Underwater vents (spawn enemies)

---

## PRIORITY MATRIX

| Priority | Issue | Effort | Impact | Total |
|----------|-------|--------|--------|-------|
| 🔴 CRITICAL | Hammershark daze | 2 hrs | HIGH | 1 |
| 🔴 CRITICAL | Health healing | 1 hr | HIGH | 1 |
| 🔴 CRITICAL | Projectile pass-through | 30 min | HIGH | 1 |
| 🟠 HIGH | Random speeds | 1 hr | MEDIUM | 2 |
| 🟠 HIGH | AI unintelligent | 4 hrs | HIGH | 2 |
| 🟠 HIGH | Underwater system | 2 hrs | MEDIUM | 2 |
| 🟡 MEDIUM | Circle jitter | 30 min | LOW | 3 |
| 🟡 MEDIUM | Boss animation | 1 hr | MEDIUM | 3 |
| 🟡 MEDIUM | AttackQueue safety | 30 min | MEDIUM | 3 |
| 🟢 LOW | Range checks | 1 hr | LOW | 3 |

---

## RECOMMENDED IMPLEMENTATION ORDER

### Week 1 - Critical Fixes
1. **Day 1-2: Fix Critical Bugs**
   - Hammershark daze (2 hrs)
   - Health healing (1 hr)
   - Projectile pass-through (30 min)
   - **Total: 3.5 hrs** ✅ One morning

2. **Day 2-3: Improve AI Feel**
   - Random speeds (1 hr)
   - Simplify SharkDefault (2 hrs)
   - Improve Tank behavior (1.5 hrs)
   - **Total: 4.5 hrs** ✅ One afternoon

3. **Day 3-4: Underwater Mechanic**
   - Implement dive/surface (2 hrs)
   - Add animations (1 hr)
   - Test with projectiles (30 min)
   - **Total: 3.5 hrs** ✅ One morning

### Week 2 - Polish & Features
4. **Day 5: Polish & Optimization**
   - Circle jitter fix (30 min)
   - Boss animations (1 hr)
   - AttackQueue safety (30 min)
   - **Total: 2 hrs**

5. **Day 6-7: New Features**
   - Wave difficulty scaling (2 hrs)
   - Combat feedback (2 hrs)
   - Enemy variations (1 hr)
   - **Total: 5 hrs**

---

## TESTING CHECKLIST

### Critical Fixes Validation
- [ ] Hammershark enters daze after missing charge
- [ ] Hammershark vulnerable during daze
- [ ] Player health increases 25% when entering shop
- [ ] Bullets pass through underwater enemies
- [ ] No wasted ammo on non-hittable enemies

### AI Improvements Validation
- [ ] Each enemy type has different speed (+/- 15%)
- [ ] SharkDefault: Approach → Orbit → Attack flow clear
- [ ] Tank: Simple approach + charge + recovery
- [ ] PackFormation: Coordinated orbit visible
- [ ] All behaviors use smooth movement (no jitter)

### Underwater System Validation
- [ ] Enemies dive/surface on timer
- [ ] Enemies have "Underwater" animation
- [ ] Projectiles pass through underwater enemies
- [ ] Can see health bars for all enemies
- [ ] Surface enemies are clearly hittable

---

## CODE REFACTORING OPPORTUNITIES

### Low-Hanging Fruit
1. Extract common behavior state patterns into base class
2. Consolidate animation parameter names (use constants)
3. Move magic numbers to BehaviorCfg fields
4. Add XML documentation to all behavior classes

### Medium Effort
1. Create BehaviorTemplate to reduce code duplication
2. Implement behavior factory pattern
3. Add serialization for behavior state (for save/load)

---

## NEXT IMMEDIATE STEPS

1. **Today**: Implement critical fixes (3.5 hrs)
   - Hammershark daze
   - Health healing fix
   - Projectile pass-through

2. **Tomorrow**: AI improvements (4.5 hrs)
   - Random speeds
   - Simplify SharkDefault & Tank

3. **This Week**: Underwater + Polish
   - Dive/surface mechanic
   - Animation fixes
   - Combat feedback

---

## QUESTIONS FOR YOU

Before I implement changes, please clarify:

1. **Healing Philosophy**: Should healing happen:
   - Only at shop? (25% max health)
   - Passive over time between waves? (5 HP/sec?)
   - Or both?

2. **Underwater Mechanic**: Should enemies:
   - Randomly dive/surface?
   - Dive when damaged?
   - Surface to attack?

3. **Random Speeds**: Should variance:
   - Be ±15%?
   - Be per-wave (all enemies in wave same speed)?
   - Be tied to difficulty?

4. **Priority**: Which feels most broken to you:
   - AI behavior being unpredictable?
   - Health system?
   - Projectile wasting?

5. **New Enemies**: Interested in adding:
   - Jellyfish (easy, floaty)?
   - More boss variants?
   - Environmental hazards?

---

**Next Step**: Pick one critical fix to start with, and I'll implement it with full testing. Recommend starting with **Hammershark Daze** as it's isolated and high-impact.
