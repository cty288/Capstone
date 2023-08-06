using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Property;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace _02._Scripts.Runtime.Common.Properties {
	public struct HealthInfo {
		public int MaxHealth;
		public int CurrentHealth;
		
		public HealthInfo(int maxHealth, int currentHealth) {
			MaxHealth = maxHealth;
			CurrentHealth = currentHealth;
		}
		
		
		public static HealthInfo operator +(HealthInfo healthInfo, int value) {
			healthInfo.CurrentHealth += value;
			return healthInfo;
		}
		
		public static HealthInfo operator -(HealthInfo healthInfo, int value) {
			healthInfo.CurrentHealth -= value;
			return healthInfo;
		}

	}

	public interface IHealthProperty : IProperty<HealthInfo>, ILoadFromConfigProperty {
		
	}
	public class Health: AbstractLoadFromConfigProperty<HealthInfo>, IHealthProperty {
		
		public int GetMaxHealth() {
			return RealValue.Value.MaxHealth;
		}

		
		public override HealthInfo OnSetBaseValueFromConfig(dynamic value) {
			return new HealthInfo((int)value.maxHealth, (int)value.currentHealth);
		}

		protected override IPropertyDependencyModifier<HealthInfo> GetDefautModifier() {
			return new HealthDefaultModifier();
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.health;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
		
		
	}
	
	public class HealthDefaultModifier : PropertyDependencyModifier<HealthInfo> {
		public override HealthInfo OnModify(HealthInfo propertyValue) {
			int rarityMultiplier = GetDependency<Rarity>(new PropertyNameInfo(PropertyName.rarity)).InitialValue * 5;
			propertyValue = new HealthInfo(propertyValue.MaxHealth * rarityMultiplier, propertyValue.CurrentHealth * rarityMultiplier);
			return propertyValue;
		}
	}
}