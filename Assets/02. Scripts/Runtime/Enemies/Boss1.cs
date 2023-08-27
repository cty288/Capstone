using Framework;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Enemies.ViewControllers.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Enemies;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Enemies
{
    public class Boss1Entity : EnemyEntity<Boss1Entity>
    {
        [field: ES3Serializable]
        public override string EntityName { get; protected set; } = "Boss1";

        [field: ES3Serializable]
        public bool ShellStatus { get; set; } = true;
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
    public class Boss1 : AbstractEnemyViewController<Boss1Entity>
    {
        [Bind("ShellStatus", null,nameof(OnShellStatusChanged))]
        public bool ShellStatus { get; }
        protected override void OnEntityDie(IBelongToFaction damagedealer)
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

        protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<Boss1Entity> builder)
        {
            return builder.
                FromConfig()
                //.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
                .Build();
        }

        protected void OnShellStatusChanged(bool oldValue,bool newValue)
        {
            Debug.Log("Shell status changed to:" + newValue);
        }
    }
}

