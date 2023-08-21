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
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

public class Boss1_TestEntity : EnemyEntity<Boss1_TestEntity>
{
    [field: ES3Serializable]
    public override string EntityName { get; protected set; } = "Boss1";

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

    protected override Faction GetDefaultFaction()
    {
        return Faction.Hostile;
    }
}
