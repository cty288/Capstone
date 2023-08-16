﻿namespace Runtime.DataFramework.Properties.TestOnly {
	public class TestHealth: AbstractLoadFromConfigProperty<HealthInfo>, IHealthProperty {
		
		public int GetMaxHealth() {
			return RealValue.Value.MaxHealth;
		}

		public int GetCurrentHealth() {
			return RealValue.Value.CurrentHealth;
		}


		public override HealthInfo OnSetBaseValueFromConfig(dynamic value) {
			return new HealthInfo((int)value.maxHealth, (int)value.currentHealth);
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