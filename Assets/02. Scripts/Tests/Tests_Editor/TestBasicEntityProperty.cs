using System;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Pool;
using NUnit.Framework;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Tests.Tests_Editor {
	public class TestBasicEntityProperty {
		internal class BasicEntity : Entity {
			
			public override string EntityName { get; set; } = "TestEntity";
			protected override void OnRegisterProperties() {
				RegisterInitialProperty<IRarityProperty>(new Rarity());
				RegisterInitialProperty<IDangerProperty>(new TestDanger());
			}

			protected override void OnEntityStart() {
				
			}

			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}
			public override void OnDoRecycle() {
				SafeObjectPool<BasicEntity>.Singleton.Recycle(this);
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return Localization.GetFormat(defaultLocalizationKey, GetProperty<IDangerProperty>().RealValue.Value);
			}
			public override void OnRecycle() {
				
			}
		}

		public class TestEnemy : Entity {
			public override string EntityName { get; set; } = "TestEnemy";
			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}

			protected override void OnRegisterProperties() {
				RegisterInitialProperty(new Rarity());
				RegisterInitialProperty(new TestResourceList() {
					BaseValue = new List<TestResourceProperty>() {
						new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 1)},
						new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 2)},
						new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 3)},
					}
				});
			}

			protected override void OnEntityStart() {
				
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}
			public override void OnDoRecycle() {
				SafeObjectPool<TestEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		public class TestResourceTableEnemy : Entity {
			public override string EntityName { get; set; } = "TestEnemy";
			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}

			protected override void OnRegisterProperties() {
				RegisterInitialProperty(new Rarity());
				RegisterInitialProperty(new TestResourceTableProperty() {
					BaseValue = new List<TestResourceList>() {
						new TestResourceList(new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 1)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 2)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 3)}),
						new TestResourceList(new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 10)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 20)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 30)}),
					}
				});
			}

			protected override void OnEntityStart() {
				
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}

			public override void OnDoRecycle() {
				SafeObjectPool<TestResourceTableEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		public class TestResourceDictEnemy : Entity {
			public override string EntityName { get; set; } = "TestDictEnemy";
			
			protected override ConfigTable GetConfigTable() {
				return ConfigDatas.Singleton.EnemyEntityConfigTable_Test;
			}
			protected override void OnRegisterProperties() {
				RegisterInitialProperty(new Rarity());
				RegisterInitialProperty(new TestResourceDictProperty() {
					BaseValue = new Dictionary<PropertyName, TestResourceProperty>() {
						{
							PropertyName.test_gold_resource,
							new TestResourceProperty(new GoldPropertyModifier())
								{BaseValue = new TestResourceInfo("Gold", 1)}
						}, {
							PropertyName.test_silver_resource,
							new TestResourceProperty() {BaseValue = new TestResourceInfo("Silver", 2)}
						},
					}
				});
			}

			protected override void OnEntityStart() {
				
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}
			

			public override void OnDoRecycle() {
				SafeObjectPool<TestResourceDictEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		

		internal class MyNewDangerModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return GetDependency<Rarity>().RealValue * 100;
			}
		}
		
		internal class GoldPropertyModifier : PropertyDependencyModifier<TestResourceInfo> {
			public override TestResourceInfo OnModify(TestResourceInfo propertyValue) {
				propertyValue.Rarity += GetDependency<Rarity>().RealValue * 100;
				return propertyValue;
			}
		}

		internal class TestResourceInfo: ICloneable {
			public string Name;
			public int Rarity;
			
			public TestResourceInfo(string name, int rarity) {
				Name = name;
				Rarity = rarity;
			}

			public object Clone() {
				return new TestResourceInfo(Name, Rarity);
			}
		}

		internal class TestResourceProperty : Property<TestResourceInfo> {
			
			public TestResourceProperty(): base(){}
			public TestResourceProperty(IPropertyDependencyModifier<TestResourceInfo> modifier) : base() {
				this.modifier = modifier;
			}
			

			protected override IPropertyDependencyModifier<TestResourceInfo> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.resource;
			}
			
			public override PropertyNameInfo[] GetDefaultDependentProperties() {
				return new[] {new PropertyNameInfo(PropertyName.rarity)};
			}
		}
		
		

		internal class TestResourceList : PropertyList<TestResourceProperty> {
			
			public TestResourceList() : base() {
			
			}
		
			public TestResourceList(params TestResourceProperty[] baseValues) : base(baseValues) {
				
			}
			

			protected override IPropertyDependencyModifier<List<TestResourceProperty>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.resource_list;
			}
		}

		internal class TestResourceTableProperty : PropertyList<TestResourceList> {
			protected override IPropertyDependencyModifier<List<TestResourceList>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.resource_list;
			}
		}

		internal class TestResourceDictProperty : PropertyDictionary<Runtime.DataFramework.Properties.PropertyName,TestResourceProperty> {

			protected override IPropertyDependencyModifier<Dictionary<PropertyName, TestResourceProperty>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.test_resource_dict;
			}

			public override PropertyName GetKey(TestResourceProperty value) {
				return value.PropertyName;
			}
		}


		public abstract class TestInterestProperty : Property<int> {

			protected override PropertyName GetPropertyName() {
				return PropertyName.test_interest;
			} 
		}
		
		public class TestPlayCommputerPropertyModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return propertyValue + GetDependency<Rarity>().RealValue +
				       GetDependency<TestStudyInterestProperty>(new PropertyNameInfo("test_interest_dict.study[1]"))
					       .RealValue;
			}
		}
		
		public class TestPlayChessPropertyModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return propertyValue + GetDependency<Rarity>().RealValue +
				       GetDependency<TestPlayComputerInterestProperty>(new PropertyNameInfo("test_interest_dict.play[0]"))
					       .RealValue;
			}
		}

		public class TestPlayComputerInterestProperty : TestInterestProperty {
			protected override IPropertyDependencyModifier<int> GetDefautModifier() {
				return new TestPlayCommputerPropertyModifier();
			}

			public override PropertyNameInfo[] GetDefaultDependentProperties() {
				return new[]
					{new PropertyNameInfo(PropertyName.rarity), new PropertyNameInfo("test_interest_dict.study[1]")};
			}
		}
		
		public class TestPlayChineseInterestProperty : TestInterestProperty {
			protected override IPropertyDependencyModifier<int> GetDefautModifier() {
				return new TestPlayChessPropertyModifier();
			}

			public override PropertyNameInfo[] GetDefaultDependentProperties() {
				return new[]
					{new PropertyNameInfo(PropertyName.rarity), new PropertyNameInfo("test_interest_dict.play[0]")};
			}
		}
		
		public class TestStudyInterestProperty : TestInterestProperty {
			
			public TestStudyInterestProperty(int baseValue) : base(){
				BaseValue = baseValue;
			}
			
			protected override IPropertyDependencyModifier<int> GetDefautModifier() {
				return null;
			}

			public override PropertyNameInfo[] GetDefaultDependentProperties() {
				return null;
			}
		}

		internal class TestInterestListProperty : PropertyList<TestInterestProperty> {
			
			public string InterestType;
			
			public TestInterestListProperty(string interestType) : base() {
				this.InterestType = interestType;
			}
			
			
			
			protected override IPropertyDependencyModifier<List<TestInterestProperty>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.test_interest_list;
			}
		}
		internal class TestInterestDictProperty : PropertyDictionary<string, TestInterestListProperty> {

			public TestInterestDictProperty(params TestInterestListProperty[] baseValues) : base(baseValues) {
				
			}
			protected override IPropertyDependencyModifier<Dictionary<string, TestInterestListProperty>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.test_interest_dict;
			}

			public override string GetKey(TestInterestListProperty value) {
				return value.InterestType;
			}
		}


		internal class TestInterestEntity : Entity {
			
			protected override ConfigTable GetConfigTable() {
				return null;
			}
			public override string EntityName { get; set; } = "TestInterestEntity";
			protected override void OnRegisterProperties() {
				RegisterInitialProperty(new Rarity());
				RegisterInitialProperty(new TestInterestDictProperty(
					new TestInterestListProperty("study") {
						BaseValue = new List<TestInterestProperty>()
							{new TestStudyInterestProperty(10), 
								new TestStudyInterestProperty(20)}
					},
					new TestInterestListProperty("play") {
						BaseValue = new List<TestInterestProperty>() {
							new TestPlayComputerInterestProperty() {BaseValue = 5},
							new TestPlayChineseInterestProperty() {BaseValue = 10}
						}
					}
				));
			}

			protected override void OnEntityStart() {
				
			}

			protected override string OnGetDescription(string defaultLocalizationKey) {
				return null;
			}
			public override void OnDoRecycle() {
				SafeObjectPool<TestInterestEntity>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		//============================Start of Tests================================

		[Test]
		public void TestBasicEntity() {
			EntityPropertyDependencyCache.ClearCache();
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate(2).
				SetProperty(new PropertyNameInfo(PropertyName.danger), 1).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(10, entity.GetProperty<TestDanger>().RealValue);
		}
		
		[Test]
		public void TestEntityDesc() {
			EntityPropertyDependencyCache.ClearCache();
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate(2).
				SetProperty(new PropertyNameInfo(PropertyName.danger), 1).
				Build();

			string desc = entity.GetDescription();
			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual("My danger is 10", desc);
		}
		
		[Test]
		public void TestBasicPropertyModifier() {
			EntityPropertyDependencyCache.ClearCache();
			
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate(2).
				SetModifier(new PropertyNameInfo(PropertyName.danger), new MyNewDangerModifier()).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(200, entity.GetProperty<IDangerProperty>().RealValue);
		}

		[Test]
		public void TestEntityModelCreate() {
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			string id = model.GetBuilder<BasicEntityBuilder<BasicEntity>, BasicEntity>(2)
				.SetModifier(new PropertyNameInfo(PropertyName.danger), new MyNewDangerModifier()).Build()
				.UUID;

			BasicEntity entity = model.GetEntity<BasicEntity>(id);
			
			Assert.IsNotNull(entity);
			Assert.AreEqual(200, entity.GetProperty<TestDanger>().RealValue);
		}

		[Test]
		public void TestEntityPool() {
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();


			IEntity ent1 = model.GetBuilder<BasicEntity>(2).SetProperty(new PropertyNameInfo(PropertyName.rarity), 2)
				.SetModifier(new PropertyNameInfo(PropertyName.danger), new MyNewDangerModifier()).Build();
			string id1 = ent1.UUID;

			model.RemoveEntity(id1);
			
			IEntity ent2 = model.GetBuilder<BasicEntity>(3).SetProperty(new PropertyNameInfo(PropertyName.rarity), 3)
				.SetModifier(new PropertyNameInfo(PropertyName.danger), new MyNewDangerModifier()).Build();
			string id2 = ent2.UUID;
			
			Assert.AreEqual(ent1, ent2);
			Assert.AreNotEqual(id1, id2);
			Assert.AreEqual(300, model.GetEntity<BasicEntity>(id2).GetProperty<TestDanger>().RealValue);
		}

		[Test]
		public void TestResourceListProperty() {
			EntityPropertyDependencyCache.ClearCache();
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestEnemy>(2)
				.Build();

			Assert.AreEqual(201, ent1.GetProperty<TestResourceList>().RealValues[0].RealValue.Value.Rarity);

			bool addTriggered = false;
			ent1.GetProperty<TestResourceList>().RealValues.RegisterOnAdd(OnAdd);
			ent1.GetProperty<TestResourceList>().AddToRealValue(new TestResourceProperty(new GoldPropertyModifier()) {
				BaseValue = new TestResourceInfo("Diamond", 100)
			});

			Assert.AreEqual(300, ent1.GetProperty<TestResourceList>().RealValues[3].RealValue.Value.Rarity);
			
			void OnAdd(TestResourceProperty obj) {
				addTriggered = true;
				ent1.GetProperty<TestResourceList>().RealValues.UnRegisterOnAdd(OnAdd);
			}
			
			Assert.AreEqual(300, (int) ent1.GetProperty(new PropertyNameInfo("resource_list[3]")).GetRealValue().Value.Rarity);
			

			Assert.IsTrue(addTriggered);
		}

		[Test]
		public void TestComplexListProperty() {
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestResourceTableEnemy>(2)
				.Build();

			var table = ent1.GetProperty<TestResourceTableProperty>().RealValues;
			Assert.AreEqual(201, table[0].RealValues[0].RealValue.Value.Rarity);
			Assert.AreEqual(210, table[1].RealValues[0].RealValue.Value.Rarity);
		}

		[Test]
		public void TestDictProperty() {
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestResourceDictEnemy>(2)
				.Build();
			  
			var table = ent1.GetProperty<TestResourceDictProperty>().RealValues;
			Assert.AreEqual(201, table[PropertyName.test_gold_resource].RealValue.Value.Rarity);
		}
		
		[Test]
		public void TestGetNestedProperties() {
			EntityPropertyDependencyCache.ClearCache();
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestResourceTableEnemy>(2)
				.Build();

			//var table = ent1.GetProperty<TestResourceTableProperty>().RealValues;
			Assert.AreEqual(201, (int) ent1.GetProperty(new PropertyNameInfo("resource_list[0][0]")).GetRealValue().Value.Rarity);
			Assert.AreEqual(210,
				(int) ent1.GetProperty<TestResourceProperty>(new PropertyNameInfo("resource_list[1][0]")).RealValue
					.Value.Rarity);
		}

		[Test]
		public void TestEntityRecycle() {
			EntityPropertyDependencyCache.ClearCache();
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			IEntity ent1 = null;

			for (int i = 0; i < 100; i++) {
				ent1 = model.
					GetBuilder<TestEnemy>(2)
					.Build();

				bool addTriggered = false;
				ent1.GetProperty<TestResourceList>().RealValues.RegisterOnAdd(OnAdd);

				for (int j = 0; j < 50; j++) {
					ent1.GetProperty<TestResourceList>().AddToRealValue(new TestResourceProperty(new GoldPropertyModifier()) {
						BaseValue = new TestResourceInfo("Diamond", 100)
					});
					Assert.AreEqual(300, (int) ent1.GetProperty(new PropertyNameInfo($"resource_list[{j+3}]")).
						GetRealValue().Value.Rarity);
				}

				Assert.AreEqual(300, (int) ent1.GetProperty(new PropertyNameInfo("resource_list[3]")).GetRealValue().Value.Rarity);
				Assert.AreEqual(53, ent1.GetProperty<TestResourceList>().RealValues.Value.Count());
			
			
				void OnAdd(TestResourceProperty obj) {
					addTriggered = true;
					ent1.GetProperty<TestResourceList>().RealValues.UnRegisterOnAdd(OnAdd);
				}

				model.RemoveEntity(ent1.UUID);

				Assert.IsNull(ent1.GetProperty<TestResourceList>().RealValues.Value);
			}

		}
		
		[Test]
		public void TestNestedDependencies() {
			EntityPropertyDependencyCache.ClearCache();
			ICommonEntityModel model = MainGame_Test.Interface.GetModel<ICommonEntityModel>();
			
			TestInterestEntity ent1 = model.
				GetBuilder<TestInterestEntity>(5)
				.Build();

			//var table = ent1.GetProperty<TestResourceTableProperty>().RealValues;
			Assert.AreEqual(30, (int) ent1.GetProperty(new PropertyNameInfo("test_interest_dict.play[0]")).GetRealValue().Value);
			Assert.AreEqual(45,
				(int) ent1.GetProperty(new PropertyNameInfo("test_interest_dict.play[1]")).GetRealValue().Value);
		}
		
		
		[Test]
		
		public void TestSerializationFactory() {
			EntityPropertyDependencyCache.ClearCache();
			Assert.AreEqual(typeof(Dictionary<string, HealthInfo>), SerializationFactory.Singleton.ParseType("Dictionary<string, HealthInfo>"));
			Assert.AreEqual(typeof(Dictionary<string, HealthInfo>), SerializationFactory.Singleton.ParseType("Dictionary<string, HealthInfo>"));
			Assert.AreEqual(typeof(Dictionary<string, Dictionary<string, HealthInfo[]>>), SerializationFactory.Singleton.ParseType("Dictionary<string, Dictionary<string, HealthInfo[]>>"));
			Assert.AreEqual(typeof(List<Dictionary<string, HashSet<HealthInfo[]>>>), SerializationFactory.Singleton.ParseType("List<Dictionary<string, HashSet<HealthInfo[]>>>"));
			Assert.AreEqual(typeof(Dictionary<string, object>), SerializationFactory.Singleton.ParseType("Dictionary<string, dynamic>")); 
			Assert.AreEqual(typeof(Dictionary<HealthInfo, List<HashSet<string[]>>>), SerializationFactory.Singleton.ParseType("Dictionary<HealthInfo, List<HashSet<string[]>>>"));
			Assert.AreEqual(typeof(Dictionary<object, List<Dictionary<string, HealthInfo[]>>>), SerializationFactory.Singleton.ParseType("Dictionary<dynamic, List<Dictionary<string, HealthInfo[]>>>")); 
			Assert.AreEqual(typeof(HashSet<Dictionary<HealthInfo, HashSet<object[]>>>), SerializationFactory.Singleton.ParseType("HashSet<Dictionary<HealthInfo, HashSet<dynamic[]>>>"));
			Assert.AreEqual(typeof(List<Dictionary<Dictionary<object, HealthInfo>, List<HashSet<string[]>>>>), SerializationFactory.Singleton.ParseType("List<Dictionary<Dictionary<dynamic, HealthInfo>, List<HashSet<string[]>>>>"));
			Assert.AreEqual(typeof(Dictionary<string, List<HashSet<Dictionary<HealthInfo, object[]>>>>), SerializationFactory.Singleton.ParseType("Dictionary<string, List<HashSet<Dictionary<HealthInfo, dynamic[]>>>>")); 
		}

	} 
}