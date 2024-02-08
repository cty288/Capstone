using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Entities.Creatures {

	
	public abstract class AbstractCreature : DamageableEntity, ICreature {
		protected IItemDropCollectionsProperty itemDropCollectionsProperty;
		public override void OnAwake() {
			base.OnAwake();
			itemDropCollectionsProperty = GetProperty<IItemDropCollectionsProperty>();
		}

		public IItemDropCollectionsProperty GetItemDropCollectionsProperty() {
			return itemDropCollectionsProperty;
		}
		
		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<IItemDropCollectionsProperty>(new ItemDropCollections());
		}

		public ReferenceCounter StunnedCounter { get; } = new ReferenceCounter();

		public override void OnRecycle() {
			StunnedCounter.Clear();
		}
	}
}