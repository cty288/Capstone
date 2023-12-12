using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;


public class BladeSentinelEntity : BossEntity<BladeSentinelEntity>
{
    [field: ES3Serializable]
    public override string EntityName { get; set; } = "BladeSentinel";
        
    

    protected override void OnEntityStart(bool isLoadedFromSave) {
            
    }

    public override void OnRecycle() {

    }
    protected override void OnInitModifiers(int rarity, int level) {
            
    }
        

        
    protected override void OnEnemyRegisterAdditionalProperties() {
            
    }

    protected override string OnGetDescription(string defaultLocalizationKey) {
        return Localization.Get(defaultLocalizationKey);
    }

    protected override ICustomProperty[] OnRegisterCustomProperties() {

        return new[] {
            new AutoConfigCustomProperty("dash"),
            new AutoConfigCustomProperty("walk"),
            new AutoConfigCustomProperty("uppercutBlades"),
            new AutoConfigCustomProperty("honingBlades"),
            new AutoConfigCustomProperty("shockWave"),
            new AutoConfigCustomProperty("danmaku"),
            new AutoConfigCustomProperty("ranges")
        };
    }
}

public class BladeSentinel : AbstractBossViewController<BladeSentinelEntity>
{
    private bool deathAnimationEnd = false;

   // public bool currentAnimationEnd;
    
    [BindCustomData("ranges", "closeRange")]
    public float CloseRange { get;}
        
    [BindCustomData("ranges", "midRange")]
    public float MidRange { get; }

    private Animator animator;
    private Rigidbody rb;
    private NavMeshAgent agent;

    protected override void Awake() {
        base.Awake();
        animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    protected override void OnEntityStart() {

    }

    protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer) {
        
    }

    protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
        
    }

    protected override void OnAnimationEvent(string eventName) {
        switch (eventName)
        {
            case "CurrentAnimationEnd":
               // currentAnimationEnd = true;
                break;
            default:
                break;
        }
    }
    protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<BladeSentinelEntity> builder) {
        return builder.
            FromConfig()
            .Build();
    }

    protected override MikroAction WaitingForDeathCondition() {
        
        return UntilAction.Allocate(() =>
            animator.GetCurrentAnimatorStateInfo(0).IsName("Die") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }

    protected override void OnEntityDie(ICanDealDamage damagedealer) {
        base.OnEntityDie(damagedealer);

        behaviorTree.enabled = false;
        animator.CrossFadeInFixedTime("Die", 0.1f);
        rb.isKinematic = false;
        rb.useGravity = true;
        agent.enabled = false;
    }

    public override void OnRecycled() {
        base.OnRecycled();
        transform.localScale = Vector3.one;
        deathAnimationEnd = false;
        rb.isKinematic = false;
        rb.useGravity = false;
        agent.enabled = true;
        behaviorTree.enabled = true;
    }
}
