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
    public class BeeEntity : EnemyEntity<BeeEntity> 
    {
        [field: ES3Serializable]
        public override string EntityName { get; set; } = "Bee";

      

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

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }
    }


    public class Bee : AbstractNormalEnemyViewController<BeeEntity> 
    {
        

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
            
           
            
        }
      
        

        protected override void OnEntityStart()
        {
            Debug.Log("bee created");
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer)
        {
            Debug.Log($"bee 1 Take damage: {damage}. bee 1 current health: {currenthealth}");
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
