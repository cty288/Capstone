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
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.ViewControllers.Base;
using UnityEngine;



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
            new AutoConfigCustomProperty("dash")
        };
    }
}

public class BladeSentinel : AbstractBossViewController<BladeSentinelEntity>
{
    private bool deathAnimationEnd = false;

    public bool currentAnimationEnd;
    
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
                currentAnimationEnd = true;
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
        transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
            deathAnimationEnd = true;
        });
            
        return UntilAction.Allocate(() => deathAnimationEnd);
    }
    
    public override void OnRecycled() {
        base.OnRecycled();
        transform.localScale = Vector3.one;
        deathAnimationEnd = false;
    }
}
