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
	}
}