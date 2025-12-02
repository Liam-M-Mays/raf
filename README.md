# RAF - Raft Defense Game

A strategic tower-defense-style game where enemies attack your raft with diverse behaviors, and you defend with multiple weapon types.

---

##  Documentation

Start here based on what you need:

### **Quick Start** 
 Read **[FEATURES_SUMMARY.md](FEATURES_SUMMARY.md)** (5-10 min read)
- Overview of all systems
- How to test new features
- Configuration guide
- Known issues & solutions

### **Dev Tools Reference**
 Read **[DEV_TOOLS_QUICK_REF.md](DEV_TOOLS_QUICK_REF.md)** (2-3 min read)
- Hotkeys and quick commands
- Dev tool overview
- How to use each tool

### **Detailed Dev Tool Guide**
 Read **[DEV_TOOLS_GUIDE.md](DEV_TOOLS_GUIDE.md)** (15+ min read)
- Complete documentation of 10 dev tools
- Usage examples
- Troubleshooting

### **Technical Architecture**
 Read **[SYSTEM_ARCHITECTURE.md](SYSTEM_ARCHITECTURE.md)** (advanced)
- Code structure and design patterns
- Behavior system internals
- Physics integration
- Extension points for new features

---

##  Quick Actions

### I want to... 
**Test a new weapon**  Go to 'WeaponManager' on player, set 'Starting Weapon' field, play

**Test a new behavior**  Use 'DevSpawner' (press F1) to spawn enemies with custom behavior

**Debug why something isn't working**  Press F1, toggle 'DevBehaviorInspector', watch state changes

**Speed up testing**  Press F1, use '+Time' or 'Pause' buttons

**See performance metrics**  Add 'DevHealthBar' or 'DevProjectileVisualizer' components

---

## Latest Features

### New in This Update
-  Physics Impact: Tank charges push raft, weapons have kickback
-  Dev HUD Fix: Fixed UI rendering (now uses TextMeshPro)
-  Weapon Knockback: Projectiles can knock enemies back
-  Starting Weapon Selector: Easily test different weapons
-  Documentation Cleanup: Consolidated 18 files  5 focused docs

### Previously Completed
-  8 enemy behavior types
-  Perception system
-  Formation manager
-  10 dev tools
-  Advanced weapons
-  Robust attack queue

---

**Last Updated**: December 1, 2025  
**Status**: All core features implemented and tested
