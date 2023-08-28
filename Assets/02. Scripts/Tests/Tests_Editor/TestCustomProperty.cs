using System;
using Framework;
using NUnit.Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Tests.Tests_Editor {
	public class TestCustomProperty {
		internal class TestBasicEnemyWithCustomProperties1 : EnemyEntity<TestBasicEnemyWithCustomProperties1> {
			public override string EntityName { get; set; } = "TTT";

			public override void OnRecycle() {
            
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}
			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}

	

			protected override void OnEnemyRegisterAdditionalProperties() {
				RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
				RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			}

			protected override ICustomProperty[] OnRegisterCustomProperties() {
				return new [] {
					new AutoConfigCustomProperty("attack1"),
					new AutoConfigCustomProperty("attack2"),
					new AutoConfigCustomProperty("attack3")
				};
			}


		}
		

		/*internal class Attack2Property : CustomProperty {
			public override string GetCustomPropertyName() {
				return "attack2";
			}

			public override string OnGetDescription() {
				return "";
			}

			public override ICustomDataProperty[] GetCustomDataProperties() {
				return new ICustomDataProperty[] {
					new DamageDataProperty("damage", new Attack2Modifier(), new PropertyNameInfo("custom_properties.attack1.speed"), new PropertyNameInfo(PropertyName.rarity)),
				};
			}
		}*/

		internal class Attack2Modifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return propertyValue + GetDependency(new PropertyNameInfo(PropertyName.rarity)).GetRealValue().Value *
					GetDependency(new PropertyNameInfo("custom_properties.attack1.speed")).GetRealValue().Value;
			}
		}
		public class TestEntity : EnemyEntity<TestEntity> {
			[field: SerializeField]
			public override string EntityName { get; set; } = "TTT2";
			public override void OnRecycle() {
        
			}
			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}
			
			protected override void OnEnemyRegisterAdditionalProperties() {
				RegisterInitialProperty(new TestCustomProperty2());
				RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
				RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}
			protected override ICustomProperty[] OnRegisterCustomProperties() {
				return new ICustomProperty[] {
					new AutoConfigCustomProperty("attack1"),
					
					new CustomProperty("attack2", null,
						new CustomDataProperty<int>("damage", new Attack2Modifier(),
							new[] {
								new PropertyNameInfo("custom_properties.attack1.speed"),
								new PropertyNameInfo(PropertyName.rarity)
							})),
					
					new CustomProperty("attack3", null, new CustomDataProperty<int>("damage"))
				};
			}
		}

		public class TestCustomProperty2 : IndependentProperty<int> {

			protected override PropertyName GetPropertyName() {
				return PropertyName.test;
			}
		}


		public class TTT2DangerModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return GetDependency(new PropertyNameInfo("custom_properties.attack1.damage")).GetRealValue().Value +
				       GetDependency(new PropertyNameInfo(PropertyName.rarity)).GetRealValue().Value;
			}
		}
		
		public class TTT2Attack1SpeedModifier : PropertyDependencyModifier<dynamic> {
			public override dynamic OnModify(dynamic propertyValue) {
				return propertyValue + GetDependency(new PropertyNameInfo("custom_properties.attack1.damage")).GetRealValue().Value +
				       GetDependency(new PropertyNameInfo(PropertyName.rarity)).GetRealValue().Value;
			}
		}

		//================================================================
		
		
		[Test]
		public void TestBasicCustomProperties() {
			IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();


			//get the time interval in milliseconds
			
			DateTime startTime = DateTime.Now;
			TestBasicEnemyWithCustomProperties1 ent1 = model.GetEnemyBuilder<TestBasicEnemyWithCustomProperties1>(2)
				.FromConfig()
				.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
				.SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
				.SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
				.SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
				.Build();
			DateTime endTime = DateTime.Now;
			Debug.Log($"Time For Build Entity (With config file initialization): {(endTime - startTime).TotalMilliseconds}ms");
			
			
			TestBasicEnemyWithCustomProperties1[] ents = new TestBasicEnemyWithCustomProperties1[100];
			startTime = DateTime.Now;
			for (int i = 0; i < 100; i++) {
				ents[i] = model.GetEnemyBuilder<TestBasicEnemyWithCustomProperties1>(2)
					.FromConfig()
					.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
					.SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
					.SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
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
			//Assert.AreEqual(2.5, (double) ent1.GetCustomProperty("attack1").GetCustomDataValue("info").test);
			//Assert.AreEqual(100,  ent1.GetCustomDataProperty("attack1", "speed").GetRealValue().Value);
			//Assert.AreEqual(100,
			//	ent1.GetCustomDataProperty<object>("attack1", "speed").GetRealValue().Value);
			Assert.AreEqual(50, (int) ent1.GetCustomDataValue("attack3", "damage").Value);
			Assert.AreEqual(2.5, (double) ent1.GetCustomDataValue<dynamic>("attack1", "info").Value.test);

			Assert.IsFalse(ent1.HasCustomProperty("ttt"));

		}

		[Test]
		public void TestEnemyWithCustomPropertiesAndSaveLoad() {
			EntityPropertyDependencyCache.ClearCache();
			IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();

			
			
			TestEntity ent1 = model.GetEnemyBuilder<TestEntity>(10)
				.FromConfig()
				.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
				.SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
				.SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
				.SetDangerModifier(new TestBasicEntityProperty.MyNewDangerModifier())
				.Build();
			Assert.AreEqual(3, ent1.GetCustomProperties().Values.Count);
			//Assert.AreEqual(1004, (int) ent1.GetCustomDataProperty("attack2","damage").GetRealValue().Value);
			ent1.RegisterOnCustomDataChanged("attack1", "damage", OnAttack1DamageChanged);
			//ent1.custom

			ent1.GetCustomDataValue<dynamic>("attack1", "damage").Value += 100;
			void OnAttack1DamageChanged(ICustomDataProperty property, dynamic oldValue, dynamic newValue) {
				Debug.Log($"OnAttack1DamageChanged: {oldValue} -> {newValue}");
				Assert.AreEqual(1000, newValue);
				ent1.UnRegisterOnCustomDataChanged("attack1", "damage", OnAttack1DamageChanged);
			}

			ES3.Save("test_save_ent1", ent1, "test_save");
			model.RemoveEntity(ent1.UUID);

			Assert.AreEqual(0,
				ent1.GetProperty<CustomProperties>(new PropertyNameInfo(PropertyName.custom_properties))
					.BaseValue["attack1"].BaseValue.Count);


			ent1 = ES3.Load<TestEntity>("test_save_ent1", "test_save");
			ent1.OnLoadFromSave();
			
			Assert.IsNotNull(ent1);
			
			Assert.AreEqual(1000, ent1.GetCustomDataValue("attack1", "damage").Value);

			Assert.AreEqual(ent1.GetProperty<ICustomProperties>().BaseValue["attack1"].GetHashCode(),
				ent1.GetProperty<ICustomProperties>().RealValues.Value["attack1"].GetHashCode());
			
			Debug.Log("Hash code of attack1 Base: " + ent1.GetProperty<ICustomProperties>().BaseValue["attack1"].GetHashCode());
			Debug.Log("Hash code of attack1 Real: " +
			          ent1.GetProperty<ICustomProperties>().RealValues.Value["attack1"].GetHashCode());
			
			Assert.AreEqual(ent1.GetProperty<ICustomProperties>().BaseValue["attack1"].BaseValue["info"].GetRealValue().GetHashCode(),
				ent1.GetCustomDataValue("attack1", "info").GetHashCode());
			ES3.DeleteKey("test_save_ent1", "test_save");
		}
		
		
		[Test]
		public void TestOverrideDependencies() {
			EntityPropertyDependencyCache.ClearCache();
			IEnemyEntityModel model = MainGame_Test.Interface.GetModel<IEnemyEntityModel>();

			
			
			TestEntity ent1 = model.GetEnemyBuilder<TestEntity>(10)
				.FromConfig()
				.SetAllBasics(0, new HealthInfo(100, 100), TasteType.Type1, TasteType.Type2)
				.SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
				.SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
				.SetDependencies(new PropertyNameInfo(PropertyName.danger), new[] {
					new PropertyNameInfo(PropertyName.rarity),
					new PropertyNameInfo("custom_properties.attack1.damage")
				})
				.SetDangerModifier(new TTT2DangerModifier())
				.SetDependencies(new PropertyNameInfo("custom_properties.attack1.speed"),
					new[] {
						new PropertyNameInfo("custom_properties.attack1.damage"),
						new PropertyNameInfo(PropertyName.rarity)
					})
				.SetModifier(new PropertyNameInfo("custom_properties.attack1.speed"), new TTT2Attack1SpeedModifier())
				.Build();

			Assert.AreEqual(910,
				ent1.GetProperty<IDangerProperty>(new PropertyNameInfo(PropertyName.danger)).RealValue.Value);

			Assert.AreEqual(1010, (int) ent1.GetCustomDataValue("attack1", "speed").Value);

			ES3.Save("test_save_ent1", ent1, "test_save");
			model.RemoveEntity(ent1.UUID);
			

			ent1 = ES3.Load<TestEntity>("test_save_ent1", "test_save");
			ent1.OnLoadFromSave();
			
			Assert.AreEqual(910,
				ent1.GetProperty<IDangerProperty>(new PropertyNameInfo(PropertyName.danger)).RealValue.Value);
			ES3.DeleteKey("test_save_ent1", "test_save");
			
			TestEntity ent2 = model.GetEnemyBuilder<TestEntity>(10)
				.FromConfig()
				.SetAllBasics(0, new HealthInfo(100, 100),  TasteType.Type1, TasteType.Type2)
				.SetProperty(new PropertyNameInfo(PropertyName.vigiliance), 100f, null)
				.SetProperty(new PropertyNameInfo(PropertyName.attack_range), 200f, null)
				.SetDangerModifier(new EmptyModifier<int>())
				.Build();

			Assert.AreEqual(0, ent2.GetDanger().Value);
		}
	}
}