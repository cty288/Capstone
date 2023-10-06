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
using MikroFramework.ActionKit;
using Runtime.DataFramework.Properties.CustomProperties;
using System.Collections.Generic;

namespace Runtime.Enemies.SmallEnemies
{
    public class BeeEntity : NormalEnemyEntity<BeeEntity> 
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "SurveillanceDrone";


        protected override void OnEntityStart(bool isLoadedFromSave) {
            
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

        protected override void OnInitModifiers(int rarity) {
            
        }

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return new[] {
                new AutoConfigCustomProperty("attack", null)
            };
        }
    }


    public class Bee : AbstractNormalEnemyViewController<BeeEntity> {
        [SerializeField] private List<GameObject> waypoints;

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
            
           
            
        }
        

        protected override void OnEntityStart() {
            foreach (GameObject waypoint in waypoints) {
                waypoint.transform.SetParent(null);
            }
        }

        public override void OnRecycled() {
            base.OnRecycled();
            foreach (GameObject waypoint in waypoints) {
                waypoint.transform.SetParent(transform);
            }
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer)
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
        
    }
}
