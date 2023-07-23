using System;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.Properties.CustomsBase;
using NUnit.Framework;
using UnityEngine;

namespace _02._Scripts.Tests.Tests_Editor {
	public class TestCustomProperty {
		internal class TestBasicEnemyWithCustomProperties1 : EnemyEntity<TestBasicEnemyWithCustomProperties1> {
			public override string EntityName { get; protected set; } = "TTT";

			public override void OnRecycle() {
            
			}

			protected override void OnEnemyRegisterProperties() {
            
			}

			protected override ICustomProperty[] OnRegisterCustomProperties() {
				return new [] {
					new DataOnlyCustomProperty("attack1"),
					new DataOnlyCustomProperty("attack2"),
					new DataOnlyCustomProperty("attack3")
				};
			}
		}
		
		
		[Test]
		public void TestBasicCustomProperties() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();


			//get the time interval in milliseconds
			
			DateTime startTime = DateTime.Now;
			TestBasicEnemyWithCustomProperties1 ent1 = model.GetEnemyBuilder<TestBasicEnemyWithCustomProperties1>(2)
				.FromConfig()
				.SetAllBasics(0, new HealthInfo(100, 100), 100, 200, TasteType.Type1, TasteType.Type2)
				.SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
				.Build();
			DateTime endTime = DateTime.Now;
			Debug.Log($"Time For Build Entity (With config file initialization): {(endTime - startTime).TotalMilliseconds}ms");
			
			
			TestBasicEnemyWithCustomProperties1[] ents = new TestBasicEnemyWithCustomProperties1[100];
			startTime = DateTime.Now;
			for (int i = 0; i < 100; i++) {
				ents[i] = model.GetEnemyBuilder<TestBasicEnemyWithCustomProperties1>(2)
					.FromConfig()
					.SetAllBasics(0, new HealthInfo(100, 100), 100, 200, TasteType.Type1, TasteType.Type2)
					.SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
					.Build();
			}
			endTime = DateTime.Now;
			Debug.Log($"Time For Build 100 Entities: {(endTime - startTime).TotalMilliseconds}ms");

			Assert.AreEqual(200, ent1.GetProperty<IDangerProperty>().RealValue.Value);
			Assert.GreaterOrEqual(1000f, ent1.GetProperty<IHealthProperty>().RealValue.Value.CurrentHealth);
			Assert.AreEqual(TasteType.Type1, ent1.GetProperty<ITasteProperty>().RealValues[0]);
			Assert.AreEqual(1000.0f, ent1.GetProperty<IVigilianceProperty>().RealValue.Value);
			Assert.AreEqual(2000.0f, ent1.GetProperty<IAttackRangeProperty>().RealValue.Value);

			Assert.AreEqual(3, ent1.GetCustomProperties().Values.Count);
			Assert.AreEqual(2.5, (double) ent1.GetCustomProperty("attack1").GetCustomDataValue("info").test);
			Assert.AreEqual(100,  ent1.GetCustomDataProperty("attack1", "speed").GetRealValue().ObjectValue);
			Assert.AreEqual(100,
				ent1.GetCustomDataProperty<object>("attack1", "speed").GetRealValue().ObjectValue);
			Assert.AreEqual(50, (int) ent1.GetCustomDataValue("attack3", "damage"));
			Assert.AreEqual(2.5, (double) ent1.GetCustomDataValue<dynamic>("attack1", "info").test);

			Assert.IsFalse(ent1.HasCustomProperty("ttt"));

		}
	}
}