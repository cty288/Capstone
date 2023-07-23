using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.CustomsBase;
using _02._Scripts.Tests.Tests_Editor;
using NUnit.Framework;
using UnityEngine;

public class TestEnemy {

    internal class TestBasicEnemy : EnemyEntity<TestBasicEnemy> {
        public override string EntityName { get; protected set; } = "TestEnemy2";

        public override void OnRecycle() {
            
        }

        protected override void OnEnemyRegisterProperties() {
            
        }

        protected override ICustomProperty[] OnRegisterCustomProperties() {
            return null;
        }
    }
    
    //===============================Start writing your tests here===============================
    [Test]
    public void TestBasicEnemyProperties() {
        IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();


        TestBasicEnemy ent1 = model.GetBuilder<TestBasicEnemy>(2)
            .SetModifier(PropertyName.danger, new TestBasicEntityProperty.MyNewDangerModifier()).
            SetProperty(PropertyName.health, new HealthInfo(100,100)).
            SetProperty(PropertyName.taste, new List<TasteType>() {TasteType.Type1, TasteType.Type2}).
            SetProperty(PropertyName.vigiliance, 100.0f). 
            SetProperty(PropertyName.attack_range, 200.0f).
        Build();
        
        Assert.AreEqual(200, ent1.GetProperty<IDangerProperty>().RealValue.Value);
        Assert.GreaterOrEqual(1000f, ent1.GetProperty<IHealthProperty>().RealValue.Value.CurrentHealth);
        Assert.AreEqual(TasteType.Type1, ent1.GetProperty<ITasteProperty>().RealValues[0]);
        Assert.AreEqual(1000.0f, ent1.GetProperty<IVigilianceProperty>().RealValue.Value);
        Assert.AreEqual(2000.0f, ent1.GetProperty<IAttackRangeProperty>().RealValue.Value);
        
        //another convenient way
        Assert.AreEqual(200, ent1.GetDanger().Value);
        Assert.GreaterOrEqual(1000f, ent1.GetHealth().Value.CurrentHealth);
        Assert.AreEqual(TasteType.Type1, ent1.GetTaste()[0]);
        Assert.AreEqual(1000.0f, ent1.GetVigiliance().Value);
        Assert.AreEqual(2000.0f, ent1.GetAttackRange().Value);
        
        Debug.Log($"Danger: {ent1.GetProperty<IDangerProperty>().RealValue}");
    }
    
    [Test]
    public void TestBasicEnemyBuilder() {
        IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();


        TestBasicEnemy ent1 = model.GetEnemyBuilder<TestBasicEnemy>(2)
            .SetAllBasics(0, new HealthInfo(100, 100), 100, 200, TasteType.Type1, TasteType.Type2)
            .SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
            .Build();

        Assert.AreEqual(200, ent1.GetProperty<IDangerProperty>().RealValue.Value);
        Assert.GreaterOrEqual(1000f, ent1.GetProperty<IHealthProperty>().RealValue.Value.CurrentHealth);
        Assert.AreEqual(TasteType.Type1, ent1.GetProperty<ITasteProperty>().RealValues[0]);
        Assert.AreEqual(1000.0f, ent1.GetProperty<IVigilianceProperty>().RealValue.Value);
        Assert.AreEqual(2000.0f, ent1.GetProperty<IAttackRangeProperty>().RealValue.Value);
        
    }
}
