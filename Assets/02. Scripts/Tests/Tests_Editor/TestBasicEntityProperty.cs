using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity;
using _02._Scripts.Runtime.Common.Properties;
using MikroFramework.Pool;
using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace _02._Scripts.Tests.Tests_Editor {
	public class TestBasicEntityProperty {
		internal class BasicEntity : Entity {
			public override string EntityName { get; protected set; } = "TestEntity";
			protected override IPropertyBase[] OnGetOriginalProperties() {
				return new IPropertyBase[] {new Rarity(), new Danger()};
			}


			public override void OnDoRecycle() {
				SafeObjectPool<BasicEntity>.Singleton.Recycle(this);
			}

			public override void OnRecycle() {
					
			}
		}

		internal class MyNewDangerModifier : PropertyDependencyModifier<int> {
			public override int OnModify(int propertyValue) {
				return GetDependency<Rarity>().InitialValue * 100;
			}
		}
		
		[Test]
		public void TestBasicEntity() {
			EntityPropertyDependencyCache.ClearCache();
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate().
				SetProperty(PropertyName.rarity, 2).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(10, entity.GetProperty<Danger>().RealValue);
		}
		
		[Test]
		public void TestBasicPropertyModifier() {
			EntityPropertyDependencyCache.ClearCache();
			
			BasicEntity entity = BasicEntityBuilder<BasicEntity>.
				Allocate().
				SetProperty(PropertyName.rarity, 2).
				SetModifier(PropertyName.danger, new MyNewDangerModifier()).
				Build();

			Debug.Log($"UUID: {entity.UUID}");
			Assert.AreEqual(200, entity.GetProperty<Danger>().RealValue);
		}

		[Test]
		public void TestEntityModelCreate() {
			IEntityModel model = MainGame.Interface.GetModel<IEntityModel>();
			
			string id = model.GetBuilder<BasicEntity>().SetProperty(PropertyName.rarity, 2)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build()
				.UUID;

			BasicEntity entity = model.GetEntity<BasicEntity>(id);
			
			Assert.IsNotNull(entity);
			Assert.AreEqual(200, entity.GetProperty<Danger>().RealValue);
		}

		[Test]
		public void TestEntityPool() {
			IEntityModel model = MainGame.Interface.GetModel<IEntityModel>();


			IEntity ent1 = model.GetBuilder<BasicEntity>().SetProperty(PropertyName.rarity, 2)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build();
			string id1 = ent1.UUID;

			model.RemoveEntity(id1);
			
			IEntity ent2 = model.GetBuilder<BasicEntity>().SetProperty(PropertyName.rarity, 3)
				.SetModifier(PropertyName.danger, new MyNewDangerModifier()).Build();
			string id2 = ent2.UUID;
			
			Assert.AreEqual(ent1, ent2);
			Assert.AreNotEqual(id1, id2);
			Assert.AreEqual(300, model.GetEntity<BasicEntity>(id2).GetProperty<Danger>().RealValue);
		}
	}
}