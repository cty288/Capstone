using System.Collections.Generic;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Levels.Models.Properties {
	public interface IMaxSpawnPerEnemyProperty : IProperty<Dictionary<string, int>> {
		
	}
	public class MaxSpawnPerEnemyProperty : Property<Dictionary<string, int>>, IMaxSpawnPerEnemyProperty {
		protected override IPropertyDependencyModifier<Dictionary<string, int>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.max_spawn_per_enemy;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
	}
}