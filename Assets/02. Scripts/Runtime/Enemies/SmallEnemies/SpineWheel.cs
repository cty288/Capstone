using System.Collections;
using Runtime.DataFramework.ViewControllers;

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
using MikroFramework.AudioKit;
using MikroFramework.ActionKit;
using Runtime.DataFramework.Properties.CustomProperties;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;

namespace Runtime.Enemies.SmallEnemies
{
    public class SpineWheelEntity : NormalEnemyEntity<SpineWheelEntity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "SpineWheel";


        protected override void OnEntityStart(bool isLoadedFromSave)
        {

        }

        public override void OnRecycle()
        {

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


    public class SpineWheel : AbstractNormalEnemyViewController<SpineWheelEntity>
    {
        [SerializeField] private List<GameObject> waypoints;
        [SerializeField] private GameObject hurtBox;
        private float invincibleTime = 2f;
        private bool spawned;

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {



        }

        protected override void Awake()
        {
            base.Awake();
            // behaviorTree = GetComponent<BehaviorTree>();
            // behaviorTree.enabled = false;
        }


        protected override void OnEntityStart()
        {
            

        }

        private IEnumerator DelayedStart()
        {
            yield return null;
           





            // behaviorTree.enabled = true;
            //behaviorTree.Start();
        }

      
        protected override void OnReadyToRecycle()
        {
            base.OnReadyToRecycle();
            foreach (GameObject waypoint in waypoints)
            {
                waypoint.transform.SetParent(transform);
            }
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
          
        }

        protected override void OnAnimationEvent(string eventName)
        {

        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<SpineWheelEntity> builder)
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
           
            base.OnRecycled();
            invincibleTime = 2f;
            spawned = false;
            //behaviorTree.DisableBehavior();
            //behaviorTree.enabled = false;
        }
    }
}
