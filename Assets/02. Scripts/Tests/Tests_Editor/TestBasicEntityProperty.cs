using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity;
using _02._Scripts.Runtime.Base.Property;
using _02._Scripts.Runtime.Common.Properties;
using MikroFramework.Pool;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace _02._Scripts.Tests.Tests_Editor {
	public class TestBasicEntityProperty {
		internal class BasicEntity : Entity {
			public override string EntityName { get; protected set; } = "TestEntity";
			protected override void OnRegisterProperties() {
				RegisterProperty<IRarityProperty>(new Rarity());
				RegisterProperty<IDangerProperty>(new Danger());
			}
			

			public override void OnDoRecycle() {
				SafeObjectPool<BasicEntity>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
					
			}
		}

		public class TestEnemy : Entity {
			public override string EntityName { get; protected set; } = "TestEnemy";
			protected override void OnRegisterProperties() {
				RegisterProperty(new Rarity());
				RegisterProperty(new TestResourceList() {
					BaseValue = new List<TestResourceProperty>() {
						new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 1)},
						new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 2)},
						new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 3)},
					}
				});
			}

			public override void OnDoRecycle() {
				SafeObjectPool<TestEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		public class TestResourceTableEnemy : Entity {
			public override string EntityName { get; protected set; } = "TestEnemy";
			protected override void OnRegisterProperties() {
				RegisterProperty(new Rarity());
				RegisterProperty(new TestResourceTableProperty() {
					BaseValue = new List<TestResourceList>() {
						new TestResourceList(new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 1)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 2)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 3)}),
						new TestResourceList(new TestResourceProperty(new GoldPropertyModifier()){BaseValue = new TestResourceInfo("Gold", 10)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Silver", 20)}, new TestResourceProperty(){BaseValue = new TestResourceInfo("Bronze", 30)}),
					}
				});
			}
			

			public override void OnDoRecycle() {
				SafeObjectPool<TestResourceTableEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		public class TestResourceDictEnemy : Entity {
			public override string EntityName { get; protected set; } = "TestDictEnemy";
			protected override void OnRegisterProperties() {
				RegisterProperty(new Rarity());
				RegisterProperty(new TestResourceDictProperty() {
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
			
			

			public override void OnDoRecycle() {
				SafeObjectPool<TestResourceDictEnemy>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
				
			}
		}
		
		

		internal class MyNewDangerModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return GetDependency<Rarity>().InitialValue * 100;
			}
		}
		
		internal class GoldPropertyModifier : PropertyDependencyModifier<TestResourceInfo> {
			public override TestResourceInfo OnModify(TestResourceInfo propertyValue) {
				propertyValue.Rarity += GetDependency<Rarity>().InitialValue * 100;
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

			public override PropertyName[] GetDependentProperties() {
				return new[] {PropertyName.rarity};
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

		internal class TestResourceDictProperty : PropertyDictionary<TestResourceProperty> {
			protected override IPropertyDependencyModifier<Dictionary<PropertyName, TestResourceProperty>> GetDefautModifier() {
				return null;
			}

			protected override PropertyName GetPropertyName() {
				return PropertyName.test_resource_dict;
			}
		}
		//============================Start of Tests================================

		[Test]
		public void TestBasicEntity() {
			EntityPropertyDependencyCache.ClearCache();
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate(2).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(10, entity.GetProperty<Danger>().RealValue);
		}
		
		[Test]
		public void TestBasicPropertyModifier() {
			EntityPropertyDependencyCache.ClearCache();
			
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate(2).
				SetModifier(PropertyName.danger, new MyNewDangerModifier()).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(200, entity.GetProperty<IDangerProperty>().RealValue);
		}

		[Test]
		public void TestEntityModelCreate() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();
			
			string id = model.GetBuilder<BasicEntityBuilder<BasicEntity>, BasicEntity>(2)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build()
				.UUID;

			BasicEntity entity = model.GetEntity<BasicEntity>(id);
			
			Assert.IsNotNull(entity);
			Assert.AreEqual(200, entity.GetProperty<Danger>().RealValue);
		}

		[Test]
		public void TestEntityPool() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();


			IEntity ent1 = model.GetBuilder<BasicEntity>(2).SetProperty(PropertyName.rarity, 2)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build();
			string id1 = ent1.UUID;

			model.RemoveEntity(id1);
			
			IEntity ent2 = model.GetBuilder<BasicEntity>(3).SetProperty(PropertyName.rarity, 3)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build();
			string id2 = ent2.UUID;
			
			Assert.AreEqual(ent1, ent2);
			Assert.AreNotEqual(id1, id2);
			Assert.AreEqual(300, model.GetEntity<BasicEntity>(id2).GetProperty<Danger>().RealValue);
		}

		[Test]
		public void TestResourceListProperty() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestEnemy>(2)
				.Build();

			Assert.AreEqual(201, ent1.GetProperty<TestResourceList>().RealValues[0].RealValue.Value.Rarity);

			bool addTriggered = false;
			ent1.GetProperty<TestResourceList>().RealValues.RegisterOnAdd(OnAdd);
			ent1.GetProperty<TestResourceList>().AddToRealValue(new TestResourceProperty(new GoldPropertyModifier()) {
				BaseValue = new TestResourceInfo("Diamond", 100)
			}, ent1);

			Assert.AreEqual(300, ent1.GetProperty<TestResourceList>().RealValues[3].RealValue.Value.Rarity);
			
			void OnAdd(TestResourceProperty obj) {
				addTriggered = true;
				ent1.GetProperty<TestResourceList>().RealValues.UnRegisterOnAdd(OnAdd);
			}
			

			Assert.IsTrue(addTriggered);
		}

		[Test]
		public void TestComplexListProperty() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestResourceTableEnemy>(2)
				.Build();

			var table = ent1.GetProperty<TestResourceTableProperty>().RealValues;
			Assert.AreEqual(201, table[0].RealValues[0].RealValue.Value.Rarity);
			Assert.AreEqual(210, table[1].RealValues[0].RealValue.Value.Rarity);
		}

		[Test]
		public void TestDictProperty() {
			IEntityModel model = MainGame_Test.Interface.GetModel<IEntityModel>();
			
			IEntity ent1 = model.
				GetBuilder<TestResourceDictEnemy>(2)
				.Build();

			var table = ent1.GetProperty<TestResourceDictProperty>().RealValues;
			Assert.AreEqual(201, table[PropertyName.test_gold_resource].RealValue.Value.Rarity);
		}

	}
}