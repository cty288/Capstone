using System.Collections.Generic;
using System.Linq;
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
	}
}