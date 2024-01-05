using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.Enemies.Model.Properties;

namespace Runtime.DataFramework.Properties.TestOnly {
	public class TestHealth: AbstractLoadFromConfigProperty<HealthInfo>, IHealthProperty {
		
		public int GetMaxHealth() {
			return RealValue.Value.MaxHealth;
		}

		public int GetCurrentHealth() {
			return RealValue.Value.CurrentHealth;
		}

		

		protected override IPropertyDependencyModifier<HealthInfo> GetDefautModifier() {
			return new TestHealthDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.health;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}


		public bool IsBuffed { get; set; }
		public HashSet<BuffTag> BuffTags { get; }
	}
	
	public class TestHealthDefaultModifier : PropertyDependencyModifier<HealthInfo> {
		public override HealthInfo OnModify(HealthInfo propertyValue) {
			
			int rarityMultiplier = GetDependency(new PropertyNameInfo(PropertyName.rarity)).GetInitialValue() *
			                       GetModifierParameterFromConfig<int>("health_rarity_multiplier", 5);
			
			propertyValue = new HealthInfo(propertyValue.MaxHealth * rarityMultiplier, propertyValue.CurrentHealth * rarityMultiplier);
			return propertyValue;
		}
	}
}