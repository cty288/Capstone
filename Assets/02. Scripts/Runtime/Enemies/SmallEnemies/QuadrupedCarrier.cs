using Runtime.DataFramework.ViewControllers;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Enemies;
using Runtime.UI.NameTags;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.AI;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using MikroFramework.ActionKit;
using Runtime.DataFramework.Properties.CustomProperties;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;

namespace Runtime.Enemies.SmallEnemies
{
    public class QuadrupedCarrierEntity : NormalEnemyEntity<QuadrupedCarrierEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "QuadrupedCarrier";


        protected override void OnEntityStart(bool isLoadedFromSave)
        {

        }

        public override void OnRecycle()
        {
            
        }
        

        
        protected override void OnEnemyRegisterAdditionalProperties() {
           
        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return null;
        }

        protected override void OnInitModifiers(int rarity, int level)
        {

        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return new[] {
                new AutoConfigCustomProperty("attack")
            };
        }
    }


    public class QuadrupedCarrier : AbstractNormalEnemyViewController<QuadrupedCarrierEntity>
    {

        [SerializeField] private GameObject deathEffect;
        [SerializeField] private SafeGameObjectPool pool;
        [SerializeField] private GameObject hurtBox;
        private float invincibleTime = 2f;
        private bool spawned;
        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {



        }
        

        protected override void OnEntityStart()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(deathEffect, 10, 15);
        }


        protected override void OnReadyToRecycle()
        {
            base.OnReadyToRecycle();
          
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
            if (currenthealth <= 0)
            {

                GameObject a = pool.Allocate();
                a.transform.position = this.transform.position + new Vector3(0, 2, 0);

            }
            Debug.Log($"carrier 1 Take damage: {damage}. carrier 1 current health: {currenthealth}");
        }

        protected override void OnAnimationEvent(string eventName)
        {

        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<QuadrupedCarrierEntity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }


        protected override MikroAction WaitingForDeathCondition()
        {
            return null;
        }
        public override void OnRecycled()
        {
            //AudioSystem.Singleton.Play3DSound("SurveillanceDrone_Dead", this.gameObject.transform.position , 0.4f);
            base.OnRecycled();
            invincibleTime = 2f;
            spawned = false;
            //behaviorTree.DisableBehavior();
            //behaviorTree.enabled = false;
        }

    }
}
