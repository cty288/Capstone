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
    public class BeeEntity : NormalEnemyEntity<BeeEntity> 
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "SurveillanceDrone";


        protected override void OnEntityStart(bool isLoadedFromSave) {
            
        }

        public override void OnRecycle() {
         
        }
        
        

        protected override void OnEnemyRegisterAdditionalProperties() {
            
        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return null;
        }
        

        protected override void OnInitModifiers(int rarity, int level) {
            
        }

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return new[] {
                new AutoConfigCustomProperty("attack", null)
            };
        }
    }


    public class Bee : AbstractNormalEnemyViewController<BeeEntity> {
        [SerializeField] private List<GameObject> waypoints;
        //private BehaviorTree behaviorTree;
        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
            
           
            
        }

        protected override void Awake() {
            base.Awake();
           // behaviorTree = GetComponent<BehaviorTree>();
           // behaviorTree.enabled = false;
        }


        protected override void OnEntityStart() {
            foreach (GameObject waypoint in waypoints) {
                waypoint.transform.SetParent(null);
            }
            AudioSystem.Singleton.Play3DSound("Surveillance Drone_Spawn", this.gameObject.transform.position, 0.5f);
            //StartCoroutine(DelayedStart());


        }
        
        private IEnumerator DelayedStart() {
            yield return null;
            yield return null;
            Vector3 referencePoint = this.gameObject.transform.position;
            Vector2 randomOffset1 = Random.insideUnitCircle * 15;
            Vector2 randomOffset2 = Random.insideUnitCircle * 15;
            waypoints[0].transform.position = new Vector3(referencePoint.x + randomOffset1.x, referencePoint.y, referencePoint.z + randomOffset1.y);
            waypoints[1].transform.position = new Vector3(referencePoint.x + randomOffset2.x, referencePoint.y, referencePoint.z + randomOffset2.y);



            
            
           // behaviorTree.enabled = true;
            //behaviorTree.Start();
        }
        

        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            foreach (GameObject waypoint in waypoints) {
                waypoint.transform.SetParent(transform);
            }
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer)
        {
            Debug.Log($"bee 1 Take damage: {damage}. bee 1 current health: {currenthealth}");
        }

        protected override void OnAnimationEvent(string eventName) {
            
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<BeeEntity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }
        

        protected override MikroAction WaitingForDeathCondition() {
            return null;
        }

        public override void OnRecycled() {
            //AudioSystem.Singleton.Play3DSound("SurveillanceDrone_Dead", this.gameObject.transform.position , 0.4f);
            base.OnRecycled();
            //behaviorTree.DisableBehavior();
            //behaviorTree.enabled = false;
        }
    }
}
