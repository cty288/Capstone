using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Framework;
using NUnit.Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Tests.Tests_Editor;
using UnityEngine;
using PropertyName = UnityEngine.PropertyName;

public class TestBuffSystem
{
    private IBuffSystem buffSystem;
    private IEnemyEntityModel enemyModel; 
    public class TestEntity : BossEntity<TestCustomProperty.TestEntity> {
	    [field: SerializeField]
	    public override string EntityName { get; set; } = "TTT4";
	    public override void OnRecycle() {
        
	    }
	    protected override void OnInitModifiers(int rarity, int level) {
            
	    }
	    protected override ConfigTable GetConfigTable() {
		    return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
	    }


	    protected override void OnEntityStart(bool isLoadedFromSave) {
				
	    }

	    protected override void OnEnemyRegisterAdditionalProperties() {
		    RegisterInitialProperty(new TestCustomProperty.TestCustomProperty2());
		    //RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
		    RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
	    }

	    protected override string OnGetDescription(string defaultLocalizationKey) {
		    return null;
	    }
	    protected override ICustomProperty[] OnRegisterCustomProperties() {
		    return new ICustomProperty[] {
			    new AutoConfigCustomProperty("attack1"),
			    new AutoConfigCustomProperty("attack2"),
			    new AutoConfigCustomProperty("attack3")
		    };
	    }
    }

    public class BasicBuff : Buff<BasicBuff> {
	    public override bool IsGoodBuff => false;
	    public override float MaxDuration { get; protected set; } = 5;
	    public override float TickInterval { get; protected set; } = 0.5f;
	    public override int Priority { get; } = 10;


	    public override string OnGetDescription(string defaultLocalizationKey) {
		    return null;
	    }

	    public override bool IsDisplayed() {
		    return true;
	    }

	    public override bool Validate() {
		    return true;
	    }

	    public override void OnInitialize() {
		    
	    }

	    public override void OnStacked(BasicBuff buff) {
		    
	    }

	    public override void OnStart() {
		   
	    }

	    public override BuffStatus OnTick() {
		    return BuffStatus.Running;
	    }

	    public override void OnEnds() {
		    
	    }
    }

    public class PropertyBuffBasic1 : PropertyBuff<PropertyBuffBasic1> {
	    public override bool IsGoodBuff => false;
	    public override float MaxDuration { get; protected set; } = 5;
	    public override float TickInterval { get; protected set; } = 0.5f;
	    public override int Priority { get; } = 5;

	    private BuffedProperties _speedProperty;
	    private BuffedProperties _damageBuff;
	    public override string OnGetDescription(string defaultLocalizationKey) {
		    return null;
	    }

	    public override bool IsDisplayed() {
		    return true;
	    }

	    public override void OnInitialize() {
		    _speedProperty = new BuffedProperties<int>(buffOwner, true, BuffTag.TestBuff1);
		    _damageBuff = new BuffedProperties<int>(buffOwner, true, BuffTag.TestBuff2, BuffTag.TestBuff3);
	    }

	    

	    public override void OnStacked(PropertyBuffBasic1 buff) {
		    
	    }

	    public override void OnStart() {
		   
	    }

	    public override BuffStatus OnTick() {
		    return BuffStatus.Running;
	    }


	    public override void OnBuffEnd() {
		    
	    }

	    protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
		    return new BuffedProperties[] {
			    _speedProperty,
			    _damageBuff
		    };
	    }
    }
    
    public class PropertyBuffBasic2 : PropertyBuff<PropertyBuffBasic2> {
	    public override bool IsGoodBuff => false;
	    public override float MaxDuration { get; protected set; } = 5;
	    public override float TickInterval { get; protected set; } = 0.5f;
	    public override int Priority { get; } = 5;

	    private BuffedProperties _speedProperty;

	    public override bool IsDisplayed() {
		    return true;
	    }
	    public override string OnGetDescription(string defaultLocalizationKey) {
		    return null;
	    }
	    public override void OnInitialize() {
		    _speedProperty = new BuffedProperties<int>(buffOwner, true, BuffTag.TestBuff1, BuffTag.TestBuff2, BuffTag.TestBuff3);
		  
	    }

	    public override void OnStacked(PropertyBuffBasic2 buff) {
		    
	    }

	    public override void OnStart() {
		   
	    }

	    public override BuffStatus OnTick() {
		    return BuffStatus.Running;
	    }


	    public override void OnBuffEnd() {
		    
	    }

	    protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
		    return new BuffedProperties[] {
			    _speedProperty
		
		    };
	    }
    }

    [SetUp]
    public void SetUp() {
	    ((MainGame_Test) MainGame_Test.Interface).ClearSave();
	    
	    Reset();
       
    }
    
    private void Reset() {
		((MainGame_Test) MainGame_Test.Interface).Reset();
		buffSystem = MainGame_Test.Interface.GetSystem<IBuffSystem>();
		enemyModel= MainGame_Test.Interface.GetModel<IEnemyEntityModel>();
	}



    [Test]
    public void TestBasicBuffSystem() {
        EntityPropertyDependencyCache.ClearCache();
        
        TestEntity ent1 = enemyModel.GetEnemyBuilder<TestEntity>(10)
            .FromConfig()
            .Build();


        buffSystem.AddBuff(ent1, null, BasicBuff.Allocate(null, ent1));
        Assert.IsTrue(buffSystem.ContainsBuff<BasicBuff>(ent1, out _));
        
        PropertyBuffBasic1 buff2 = PropertyBuffBasic1.Allocate(null, ent1);
        Assert.IsTrue(buffSystem.CanAddBuff(ent1, buff2));
        buffSystem.AddBuff(ent1, null, buff2);
        Assert.IsTrue(buffSystem.ContainsBuff<PropertyBuffBasic1>(ent1, out _));
        Assert.IsTrue(ent1.GetCustomProperties()["attack2"].GetCustomDataProperty<int>("damage").GetIsBuffed());
        
        PropertyBuffBasic2 buff3 = PropertyBuffBasic2.Allocate(null, ent1);
        Assert.IsFalse(buffSystem.CanAddBuff(ent1, buff3));
 
        string uuid = ent1.UUID;
        /*ES3.Save("test_save_ent1_buffed_1", ent1, "test_save");
        model.RemoveEntity(ent1.UUID);

        ent1 = ES3.Load<TestEntity>("test_save_ent1_buffed_1", "test_save");
        ent1.OnLoadFromSave();*/

        ((SavableArchitecture<MainGame_Test>) MainGame_Test.Interface).SaveGame();
        Reset();
        ent1 = enemyModel.GetEntity(uuid) as TestEntity;
        
			
        
        Assert.IsTrue(buffSystem.ContainsBuff<BasicBuff>(ent1, out _));
        Assert.IsTrue(buffSystem.ContainsBuff<PropertyBuffBasic1>(ent1, out _));
        Assert.IsTrue(ent1.GetCustomProperties()["attack2"].GetCustomDataProperty<int>("damage").GetIsBuffed());
        
        
        buffSystem.RemoveBuff<BasicBuff>(ent1);
        Assert.IsFalse(buffSystem.ContainsBuff<BasicBuff>(ent1, out _));
        
        buffSystem.RemoveBuff<PropertyBuffBasic1>(ent1);
        Assert.IsFalse(ent1.GetCustomProperties()["attack2"].GetCustomDataProperty<int>("damage").GetIsBuffed());
       // ES3.DeleteKey("test_save_ent1_buffed", "test_save");
    }
}
