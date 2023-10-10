﻿using Runtime.DataFramework.Properties;

namespace Runtime.Enemies.Model.Properties {
	public interface ISpawnWeightProperty : IProperty<int>, ILoadFromConfigProperty {
		
	}
	public class SpawnWeight : AbstractLoadFromConfigProperty<int>, ISpawnCostProperty {
		protected override IPropertyDependencyModifier<int> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.spawn_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}