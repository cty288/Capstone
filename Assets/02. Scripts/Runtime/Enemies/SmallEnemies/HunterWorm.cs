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
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;

namespace Runtime.Enemies.SmallEnemies
{
    public class HunterWormEntity : NormalEnemyEntity<HunterWormEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "HunterWorm";


        protected override void OnEntityStart(bool isLoadedFromSave)
        {

        }

        public override void OnRecycle() {
            base.OnRecycle();
        }



       

        protected override void OnEnemyRegisterAdditionalProperties()
        {

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


    public class HunterWorm : AbstractNormalEnemyViewController<HunterWormEntity>
    {
        [SerializeField] private GameObject deathEffect;
        [SerializeField] private SafeGameObjectPool pool;
        [SerializeField] private GameObject hurtBox;
      
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

        protected override void FixedUpdate() {
            base.FixedUpdate();
            if (true) { //do nothing to update physics
              //from unity documentation: If you move Colliders from scripting or by animation, you need to allow at least one FixedUpdate to be executed so that the physics library can update before a Raycast will hit the Collider at its new position.
            }
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
            if (currenthealth <= 0)
            {
               
                GameObject a = pool.Allocate();
                a.transform.position = this.transform.GetChild(2).position;
               
            }
        }

        protected override void OnAnimationEvent(string eventName)
        {
            
        }
        

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<HunterWormEntity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }

        public override void OnRecycled()
        {
            //AudioSystem.Singleton.Play3DSound("SurveillanceDrone_Dead", this.gameObject.transform.position , 0.4f);
            base.OnRecycled();
        }
        protected override MikroAction WaitingForDeathCondition()
        {
            
            return null;
        }

    }
}
