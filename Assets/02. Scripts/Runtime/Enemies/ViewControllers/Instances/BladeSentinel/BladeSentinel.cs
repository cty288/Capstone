using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;

public class BladeSentinelEntity : BossEntity<BladeSentinelEntity>
{
    [field: ES3Serializable]
    public override string EntityName { get; set; } = "BladeSentinel";
    private List<GameObject> bladeSpawnPositions;
    private List<GameObject> originalShieldList; // tracks original objects
    private List<GameObject> originalBladeList;
    
    private Stack<GameObject> activeBladeStack; // tracks active usable objects for attacks
    private Stack<GameObject> activeShieldStack;

    protected override void OnEntityStart(bool isLoadedFromSave) {
            
    }

    public override void OnRecycle() {

    }
    protected override void OnInitModifiers(int rarity, int level) {
            
    }
        
    protected override void OnEnemyRegisterAdditionalProperties() {
            
    }

    public void SetShieldBlades(List<GameObject> positionList, List<GameObject> shieldList, List<GameObject> bladeList) {
        bladeSpawnPositions = positionList;
        this.originalShieldList = shieldList;
        this.originalBladeList = bladeList;
        this.activeShieldStack = new Stack<GameObject>(shieldList);
        this.activeBladeStack = new Stack<GameObject>(bladeList);
    }

    public List<GameObject> GetPositionList() {
        return bladeSpawnPositions;
    }
    
    public List<GameObject> GetSwordList() {
        return originalBladeList;
    }
    
    public List<GameObject> GetShieldList() {
        return originalShieldList;
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
            new AutoConfigCustomProperty("ranges"),
            new AutoConfigCustomProperty("melee"),
            new AutoConfigCustomProperty("shield")
        };
    }
}

public class BladeSentinel : AbstractBossViewController<BladeSentinelEntity>
{
    private bool deathAnimationEnd = false;

    [BindCustomData("ranges", "closeRange")]
    public float CloseRange { get;}
        
    [BindCustomData("ranges", "midRange")]
    public float MidRange { get; }

    private Animator animator;
    private Rigidbody rb;
    private NavMeshAgent agent;

    private MeleeBladeViewController meleeBlade;
    
    [SerializeField] private GameObject model;

    public List<GameObject> bladeSpawnPositions;
    public List<GameObject> bladeList;
    public List<GameObject> shieldList;
    protected override void Awake() {
        base.Awake();
        animator = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        meleeBlade = GetComponentInChildren<MeleeBladeViewController>(true);
    }

    protected override void OnEntityStart() {
        rb.isKinematic = true;
        BoundEntity.SetShieldBlades(bladeSpawnPositions, shieldList, bladeList);
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
            case "MeleeBladeStartCheck":
                meleeBlade.Init(BoundEntity.CurrentFaction.Value,
                    BoundEntity.GetCustomDataValue<int>("melee", "meleeDamage"),
                    gameObject, BoundEntity);
                
                meleeBlade.StartCheckHit();
                break;
            case "MeleeBladeStopCheck":
                meleeBlade.StopCheckHit();
                break;
            case "ActivateShield":
                BoundEntity.IsInvincible.Value = true;
                break;
            case "DeactivateShield":
                BoundEntity.IsInvincible.Value = false;
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

        return UntilAction.Allocate(() => !model.gameObject.activeInHierarchy ||
                                          (animator.GetCurrentAnimatorStateInfo(0).IsName("Die") &&
                                           animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f));
    }

    protected override void OnEntityDie(ICanDealDamage damagedealer) {
        base.OnEntityDie(damagedealer);

        model.gameObject.SetActive(true);
        behaviorTree.enabled = false;
        animator.SetBool("Die", true);
        animator.CrossFadeInFixedTime("Die", 0.1f);
        rb.isKinematic = false;
        rb.useGravity = true;
        agent.enabled = false;
    }

    public override void OnRecycled() {
        base.OnRecycled();
        transform.localScale = Vector3.one;
        animator.SetBool("Die", false);
        deathAnimationEnd = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        agent.enabled = true;
        behaviorTree.enabled = true;
        model.gameObject.SetActive(true);
    }
}
