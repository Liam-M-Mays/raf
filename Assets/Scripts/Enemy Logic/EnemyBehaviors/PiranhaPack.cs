using UnityEngine;
using System;
using System.Collections.Generic;


#region Pack Coordinator - Manages a single pack

public class PiranhaPackCoordinator
{
    public int PackId { get; private set; }
    public List<IBehavior> Members { get; private set; }
    public PackState State { get; private set; }
    public Vector2 PackCenter { get; private set; }
    public Transform Target { get; private set; }
    
    private float stateTimer;
    private bool hasInitiatedSwarm;
    
    public enum PackState
    {
        Circling,      // Circle around target, waiting for opening
        Closing,       // One member got close, begin closing in
        Swarming,      // All members attack at once
        Regrouping     // Reform circle after being scattered
    }
    
    public PiranhaPackCoordinator(Transform target)
    {
        Members = new List<IBehavior>();
        State = PackState.Circling;
        Target = target;
        hasInitiatedSwarm = false;
        
        // Register with manager
        if (PiranhaPackManager.Instance != null)
        {
            PackId = PiranhaPackManager.Instance.RegisterPack(this);
        }
    }
    
    public void AddMember(IBehavior member)
    {
        if (!Members.Contains(member))
        {
            Members.Add(member);
        }
    }
    
    public void RemoveMember(IBehavior member)
    {
        Members.Remove(member);
        
        // If pack is too small, disband
        if (Members.Count == 0)
        {
            if (PiranhaPackManager.Instance != null)
            {
                PiranhaPackManager.Instance.UnregisterPack(PackId);
            }
        }
    }
    
    public void UpdatePackState(float deltaTime)
    {
        if (Members.Count == 0) return;
        
        // Calculate pack center
        CalculatePackCenter();
        
        stateTimer -= deltaTime;
        
        switch (State)
        {
            case PackState.Circling:
                // Check if any member is close enough to trigger swarm
                foreach (var member in Members)
                {
                    var piranhaData = GetPiranhaData(member);
                    if (piranhaData != null && piranhaData.distanceToTarget < 2.5f && !hasInitiatedSwarm)
                    {
                        TransitionToState(PackState.Closing);
                        return;
                    }
                }
                
                // Occasionally try to close in even without trigger
                if (stateTimer <= 0f && !hasInitiatedSwarm)
                {
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        TransitionToState(PackState.Closing);
                    }
                    else
                    {
                        stateTimer = UnityEngine.Random.Range(3f, 6f);
                    }
                }
                break;
                
            case PackState.Closing:
                // Check if enough members are close
                int closeMembers = 0;
                foreach (var member in Members)
                {
                    var piranhaData = GetPiranhaData(member);
                    if (piranhaData != null && piranhaData.distanceToTarget < 4f)
                    {
                        closeMembers++;
                    }
                }
                
                // Transition to swarm when enough are close or timeout
                if (closeMembers >= Mathf.Max(2, Members.Count / 2) || stateTimer <= 0f)
                {
                    TransitionToState(PackState.Swarming);
                }
                break;
                
            case PackState.Swarming:
                // Swarm for a duration then regroup
                if (stateTimer <= 0f)
                {
                    hasInitiatedSwarm = true;
                    TransitionToState(PackState.Regrouping);
                }
                break;
                
            case PackState.Regrouping:
                // Check if pack has reformed
                int reformedMembers = 0;
                foreach (var member in Members)
                {
                    var piranhaData = GetPiranhaData(member);
                    if (piranhaData != null && piranhaData.isInFormation)
                    {
                        reformedMembers++;
                    }
                }
                
                if (reformedMembers >= Members.Count - 1 || stateTimer <= 0f)
                {
                    TransitionToState(PackState.Circling);
                    stateTimer = UnityEngine.Random.Range(4f, 8f);
                }
                break;
        }
    }
    
    private void TransitionToState(PackState newState)
    {
        State = newState;
        
        switch (newState)
        {
            case PackState.Circling:
                stateTimer = UnityEngine.Random.Range(5f, 10f);
                break;
            case PackState.Closing:
                stateTimer = 4f;
                break;
            case PackState.Swarming:
                stateTimer = UnityEngine.Random.Range(6f, 10f);
                break;
            case PackState.Regrouping:
                stateTimer = 5f;
                break;
        }
    }
    
    private void CalculatePackCenter()
    {
        if (Members.Count == 0)
        {
            PackCenter = Target.position;
            return;
        }
        
        Vector2 sum = Vector2.zero;
        int validMembers = 0;
        
        foreach (var member in Members)
        {
            var piranhaData = GetPiranhaData(member);
            if (piranhaData != null)
            {
                sum += piranhaData.position;
                validMembers++;
            }
        }
        
        PackCenter = validMembers > 0 ? sum / validMembers : (Vector2)Target.position;
    }
    
    public int GetMemberIndex(IBehavior member)
    {
        return Members.IndexOf(member);
    }
    
    public float GetFormationAngle(int index)
    {
        if (Members.Count == 0) return 0f;
        return (index * Mathf.PI * 2f / Members.Count) + (Time.time * 0.3f);
    }
    
    private PiranhaPackData GetPiranhaData(IBehavior member)
    {
        if (member is PiranhaPack piranhapack)
        {
            return piranhapack.GetPackData();
        }
        return null;
    }
}

#endregion

#region Inline Config

[Serializable]
public class PiranhaPackCfg : BehaviorCfg
{
    [Header("Movement")]
    [Min(0f)] public float maxSpeed = 3f;
    [Min(0f)] public float speed = 1.5f;
    
    [Header("Ranges")]
    [Min(0f)] public float attackRange = 0.8f;
    [Min(0f)] public float attackRangeMax = 1.2f;
    [Min(0f)] public float outOfRange = 15f;
    [Min(0f)] public float respawnRange = 12f;
    
    [Header("Pack Behavior")]
    [Min(0f)] public float divergeRange = 1.5f;
    public float divergeWeight = 0.8f;
    [Min(2)] public int packSize = 6;
    
    [Header("Formation")]
    [Min(2f)] public float circleRadius = 6f;
    [Min(0.5f)] public float circleRadiusMin = 4f;
    [Min(0.1f)] public float formationTightness = 0.7f; // How tight the formation is (0-1)
    
    [Header("Attack Behavior")]
    [Min(0.1f)] public float swarmSpeed = 4f;
    [Min(0.1f)] public float closeInSpeed = 2.5f;
    public bool randomizeAttackTiming = true;
    
    public override IBehavior CreateRuntimeBehavior() => new PiranhaPack(this);
}

#endregion

#region Pack Behavior Data

public class PiranhaPackData
{
    public PiranhaPackCoordinator coordinator;
    public int memberIndex;
    public float formationAngle;
    public Vector2 formationPosition;
    public Vector2 position;
    public float distanceToTarget;
    public bool isInFormation;
    public float attackDelay;
    public bool hasAttacked;
}

#endregion

#region Main Behavior

public class PiranhaPack : IBehavior
{
    private BehaviorContext ctx;
    private PiranhaPackCfg config;
    private PiranhaPackData packData;
    
    public PiranhaPack(PiranhaPackCfg cfg)
    {
        config = cfg;
    }
    
    public void OnEnter(Transform _self, Animator _anim)
    {
        // Initialize context
        ctx = new BehaviorContext(_self, GameObject.FindGameObjectWithTag("Raft").transform, _anim);
        
        // Copy config to context
        ctx.maxSpeed = config.maxSpeed;
        ctx.speed = config.speed;
        ctx.attackRange = config.attackRange;
        ctx.attackRangeMax = config.attackRangeMax;
        ctx.outOfRange = config.outOfRange;
        ctx.respawnRange = config.respawnRange;
        ctx.divergeRange = config.divergeRange;
        ctx.divergeWeight = config.divergeWeight;
        
        // Initialize pack data
        packData = new PiranhaPackData();
        packData.position = _self.position;
        packData.distanceToTarget = Vector2.Distance(_self.position, ctx.target.position);
        packData.isInFormation = false;
        packData.hasAttacked = false;
        packData.attackDelay = config.randomizeAttackTiming ? UnityEngine.Random.Range(0f, 0.5f) : 0f;
        
        // Find or create coordinator
        FindOrCreateCoordinator();
        
        ctx.customData = packData;
    }
    
    public void OnExit()
    {
        if (packData?.coordinator != null)
        {
            packData.coordinator.RemoveMember(this);
        }
    }
    
    public void OnUpdate()
    {
        ctx.UpdateFrame();
        packData.position = ctx.self.position;
        packData.distanceToTarget = ctx.distanceToTarget;
        
        // Out of range check
        if (UtilityNodes.IsOutOfRange(ctx))
        {
            ActionNodes.Respawn(ctx, ctx.respawnRange);
            packData.isInFormation = false;
            return;
        }
        
        // Update pack coordinator
        if (packData.coordinator != null)
        {
            packData.coordinator.UpdatePackState(ctx.deltaTime);
            packData.memberIndex = packData.coordinator.GetMemberIndex(this);
            packData.formationAngle = packData.coordinator.GetFormationAngle(packData.memberIndex);
        }
        
        // Execute behavior based on pack state
        if (packData.coordinator != null)
        {
            switch (packData.coordinator.State)
            {
                case PiranhaPackCoordinator.PackState.Circling:
                    ExecuteCircling();
                    break;
                    
                case PiranhaPackCoordinator.PackState.Closing:
                    ExecuteClosing();
                    break;
                    
                case PiranhaPackCoordinator.PackState.Swarming:
                    ExecuteSwarming();
                    break;
                    
                case PiranhaPackCoordinator.PackState.Regrouping:
                    ExecuteRegrouping();
                    break;
            }
        }
        else
        {
            // Fallback if no coordinator
            DirectChaseMovement.Execute(ctx);
        }
    }
    
    public void OnLateUpdate() { }
    
    private void FindOrCreateCoordinator()
    {
        // Try to find existing pack nearby
        var nearbyPiranhas = Physics2D.OverlapCircleAll(ctx.self.position, 20f, ctx.sharkLayer);
        
        foreach (var col in nearbyPiranhas)
        {
            if (col.transform == ctx.self) continue;
            
            var brain = col.GetComponent<LiamEnemyBrain>();
            if (brain != null)
            {
                var behavior = brain.manager.Current;
                if (behavior is PiranhaPack otherPiranha)
                {
                    var otherData = otherPiranha.GetPackData();
                    if (otherData?.coordinator != null && otherData.coordinator.Members.Count < config.packSize)
                    {
                        // Join existing pack
                        packData.coordinator = otherData.coordinator;
                        packData.coordinator.AddMember(this);
                        return;
                    }
                }
            }
        }
        
        // Create new pack
        packData.coordinator = new PiranhaPackCoordinator(ctx.target);
        packData.coordinator.AddMember(this);
    }
    
    private void ExecuteCircling()
    {
        ctx.anim.SetBool("Moving", true);
        ctx.anim.SetBool("Attacking", false);
        
        // Calculate formation position on circle
        float radius = Mathf.Lerp(config.circleRadiusMin, config.circleRadius, 
            Mathf.Clamp01(ctx.distanceToTarget / config.circleRadius));
        
        packData.formationPosition = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(packData.formationAngle) * radius,
            Mathf.Sin(packData.formationAngle) * radius
        );
        
        // Move to formation position
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, packData.formationPosition);
        float distToFormation = Vector2.Distance(ctx.self.position, packData.formationPosition);
        
        packData.isInFormation = distToFormation < 2f;
        
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, config.formationTightness);
    }
    
    private void ExecuteClosing()
    {
        ctx.anim.SetBool("Moving", true);
        ctx.anim.SetBool("Attacking", false);
        
        // Move toward target but maintain some formation structure
        float radius = Mathf.Lerp(config.circleRadiusMin * 0.5f, config.circleRadiusMin, 
            Mathf.Clamp01(ctx.distanceToTarget / config.circleRadiusMin));
        
        Vector2 closingPosition = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(packData.formationAngle) * radius,
            Mathf.Sin(packData.formationAngle) * radius
        );
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, closingPosition);
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, 1.2f);
    }
    
    private void ExecuteSwarming()
    {
        // Delay attack slightly for wave effect
        if (packData.attackDelay > 0f)
        {
            packData.attackDelay -= ctx.deltaTime;
            ExecuteClosing();
            return;
        }
        
        ctx.anim.SetBool("Moving", true);
        
        // Attack if in range
        if (UtilityNodes.IsInAttackRange(ctx))
        {
            ctx.anim.SetBool("Attacking", true);
            packData.hasAttacked = true;
            
            // Stick to target when attacking
            Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, ctx.target.position);
            UtilityNodes.UpdateFacing(ctx, targetPos);
            UtilityNodes.MoveTowards(ctx, targetPos, 0.5f);
        }
        else
        {
            ctx.anim.SetBool("Attacking", false);
            
            // Aggressive chase with zigzag
            ZigzagMovement.Execute(ctx, 1.5f, 8f);
            
            // Override speed for faster swarm
            ctx.maxSpeed = config.swarmSpeed;
        }
    }
    
    private void ExecuteRegrouping()
    {
        ctx.anim.SetBool("Moving", true);
        ctx.anim.SetBool("Attacking", false);
        packData.hasAttacked = false;
        packData.attackDelay = config.randomizeAttackTiming ? UnityEngine.Random.Range(0f, 0.5f) : 0f;
        
        // Move back to formation position
        float radius = config.circleRadius;
        packData.formationPosition = (Vector2)ctx.target.position + new Vector2(
            Mathf.Cos(packData.formationAngle) * radius,
            Mathf.Sin(packData.formationAngle) * radius
        );
        
        Vector2 targetPos = UtilityNodes.ApplyDivergence(ctx, packData.formationPosition);
        float distToFormation = Vector2.Distance(ctx.self.position, packData.formationPosition);
        
        packData.isInFormation = distToFormation < 2f;
        
        UtilityNodes.UpdateFacing(ctx, targetPos);
        UtilityNodes.MoveTowards(ctx, targetPos, 0.9f);
    }
    
    public PiranhaPackData GetPackData()
    {
        return packData;
    }
}

#endregion