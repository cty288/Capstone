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

namespace Runtime.Enemies.SmallEnemies
{
    public class BeeEntity : EnemyEntity<BeeEntity>
    {
        public override string EntityName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override void OnRecycle()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEnemyRegisterAdditionalProperties()
        {
            throw new System.NotImplementedException();
        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            throw new System.NotImplementedException();
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            throw new System.NotImplementedException();
        }
    }


    public class Bee : AbstractNormalEnemyViewController<BeeEntity>
    {
        protected override int GetCurrentHitDamage()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnDestroyHealthBar(HealthBar healthBar)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<BeeEntity> builder)
        {
            throw new System.NotImplementedException();
        }

        protected override HealthBar OnSpawnHealthBar()
        {
            throw new System.NotImplementedException();
        }

        protected override MikroAction WaitingForDeathCondition()
        {
            throw new System.NotImplementedException();
        }
        public override void HurtResponse(HitData data)
        {
            base.HurtResponse(data);

        }
        public override void HitResponse(HitData data)
        {
            base.HitResponse(data);
        }


    }
}
